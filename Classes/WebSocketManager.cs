using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public class LiveUpdateEventArgs : EventArgs
    {
        public string EventType { get; set; }
        public object Data { get; set; }
    }

    public class MemberCreated { public int id; public string status; public string rfid; public DateTime created_at; }
    public class MemberUpdated { public int id; public string status; public string rfid; public DateTime updated_at; }
    public class MemberDeleted { public int id; public string status; public string rfid; public DateTime deleted_at; }
    public class NotificationStatus { public int id; public string status; public string rfid; }
    public class AttendanceNotification
    {
        public string id;
        [JsonProperty("in")]
        public string CheckIn;
        [JsonProperty("out")]
        public string CheckOut;
        public int member_id;
        public string status;
    }

    public class WebSocketManager
    {
        //private const string ApiUrl = "wss://staging.smartgymstudio.com/app/gvckpce5wdrcdjwaxnxs";
        private const string ApiUrl = "wss://api.fitnessmarvel.com/app/gvckpce5wdrcdjwaxnxs";

        private ClientWebSocket _websocket;
        private CancellationTokenSource _cts;
        private string _apiKey;
        private string _gymId;
        private readonly DatabaseHelper _dbHelper;

        public event EventHandler<string> StatusChanged;
        public event EventHandler<string> ErrorOccurred;
        public event EventHandler<LiveUpdateEventArgs> LiveUpdate;
        public event EventHandler AllMembersSynced;

        private int _currentMemberPage = 1;
        private int _lastMemberPage = 1;
        private int _membersPerPage = 20;
        private int _version = 1;
        private bool _isPaginatedSyncInProgress = false;
        private int _lastRequestedPage = 0; // Guard to track the last page requested
        private TaskCompletionSource<bool> _subscriptionTcs; // it is for subscription confirmation
        public DateTime LastPongReceived { get; private set; } = DateTime.MinValue;

        public WebSocketManager(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }


        public void Initialize(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be empty.");

            _apiKey = apiKey;
            var parts = apiKey.Split(':');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid API key format. Expected: gymId:secretKey");

            _gymId = parts[0].Trim();
            OnStatusChanged("API key initialized.");
        }

        public async Task ConnectAsync()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

                _websocket = new ClientWebSocket();
                _cts = new CancellationTokenSource();

                OnStatusChanged("Connecting to " + ApiUrl);
                await _websocket.ConnectAsync(new Uri(ApiUrl), _cts.Token);
                OnStatusChanged("WebSocket connected.");
                LogHelper.Log("WebSocket connected.");

                _ = Task.Run(ReceiveLoopAsync);
            }
            catch (Exception ex)
            {
                OnErrorOccurred("WebSocket connection error: " + ex);
                LogHelper.Log("WebSocket connection error: " + ex);
            }
        }

        public async Task<bool> SubscribeAsync()
        {
            if (_websocket?.State != WebSocketState.Open)
            {
                OnErrorOccurred("WebSocket is not connected.");
                return false;
            }

            if (string.IsNullOrEmpty(_gymId) || string.IsNullOrEmpty(_apiKey))
            {
                OnErrorOccurred("API key is not initialized.");
                return false;
            }

            _subscriptionTcs = new TaskCompletionSource<bool>();

            string subscribeMsg = $"{{\"event\":\"pusher:subscribe\",\"data\":{{\"channel\":\"gym.{_gymId}\",\"auth\":\"{_apiKey}\"}}}}";
            await SendMessageAsync(subscribeMsg);

            OnStatusChanged("📡 Subscription request sent. Awaiting confirmation...");

            // Wait for subscription confirmation or timeout after 5 seconds
            var completedTask = await Task.WhenAny(_subscriptionTcs.Task, Task.Delay(5000));

            if (completedTask == _subscriptionTcs.Task && _subscriptionTcs.Task.Result)
            {
                OnStatusChanged("subscription confirmed by server.");
                LogHelper.Log("Subscription confirmed by server.");
                return true;
            }
            else
            {
                OnErrorOccurred("Subscription failed or timed out. Possibly invalid API key.");
                return false;
            }
        }


        public async Task SendCommandAsync(string command, object payload)
        {
            if (_websocket?.State != WebSocketState.Open)
            {
                OnErrorOccurred("WebSocket is not connected.");
                return;
            }

            string message = JsonConvert.SerializeObject(new
            {
                @event = "client-command",
                channel = $"gym.{_gymId}",
                data = new { command, payload }
            });

            LogHelper.Log($"[WebSocket] Sending command: {command}, Payload: {JsonConvert.SerializeObject(payload)}");

            await SendMessageAsync(message);
            OnStatusChanged("Command sent: " + message);
        }

        public async Task GetAllMembersAsync(int perPage, int version)
        {
            _membersPerPage = perPage;
            _version = version;
            _currentMemberPage = 1;
            _lastRequestedPage = 0; // reset the guard
            _isPaginatedSyncInProgress = true;
            await RequestMemberPageAsync(1);
        }

        private async Task RequestMemberPageAsync(int page)
        {
            if (page <= _lastRequestedPage)
            {
                OnStatusChanged($"❗️ Page {page} already requested; skipping.");
                return;
            }

            _lastRequestedPage = page;

            var payload = new { per_page = _membersPerPage, version = _version, page = page };
            string requestBody = JsonConvert.SerializeObject(payload, Formatting.Indented);
            OnStatusChanged($"📤 Sending 'member.all' with page: {page}\n{requestBody}");

            await SendCommandAsync("member.all", payload);
        }


        public async Task GetMemberAsync(int id)
        {
            await SendCommandAsync("member.get", new { id });
        }


        public async Task LogAttendanceAsync(int attendeeId, DateTime? checkIn = null, DateTime? checkOut = null)
        {
            // Ensure at least one timestamp is provided.
            if (checkIn == null && checkOut == null)
            {
                throw new ArgumentException("At least one of checkIn or checkOut must be provided.");
            }

            try
            {
                // Resolve attendance id when not supplied by caller.
                string resolvedId = null;
                if (string.IsNullOrWhiteSpace(resolvedId))
                {
                    try
                    {
                        resolvedId = _dbHelper.GetAttendanceIdForMember(attendeeId, checkIn, checkOut);
                        // resolvedId may be null if not found — that's acceptable (will omit id in payload)
                        if (!string.IsNullOrWhiteSpace(resolvedId))
                            LogHelper.Log($"Resolved AttendanceId for member {attendeeId}: {resolvedId}");
                        else
                            LogHelper.Log($"No AttendanceId found for member {attendeeId}; sending payload without id to create new record.");
                    }
                    catch (Exception ex)
                    {
                        // Do not fail the whole operation if lookup fails; log and continue without id.
                        LogHelper.Log($"Error resolving AttendanceId for member {attendeeId}: {ex.Message}");
                    }
                }

                // Build the payload dynamically.
                var payload = new Dictionary<string, object>
        {
            { "attendee_id", attendeeId }
        };

                if (!string.IsNullOrWhiteSpace(resolvedId))
                {
                    payload["id"] = resolvedId;
                }

                if (checkIn.HasValue)
                {
                    payload["check_in"] = checkIn.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }

                if (checkOut.HasValue)
                {
                    payload["check_out"] = checkOut.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // Send the attendance command.
                await SendCommandAsync("member.attendance", payload);
            }
            catch (Exception ex)
            {
                OnErrorOccurred("Error logging attendance: " + ex.Message);
            }
        }

       
        private async Task SendMessageAsync(string message)
        {
            try
            {
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                await _websocket.SendAsync(buffer, WebSocketMessageType.Text, true, _cts.Token);
            }
            catch (Exception ex)
            {
                OnErrorOccurred("Error sending message: " + ex);
            }
        }


        private async Task ReceiveLoopAsync()
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            try
            {
                while (_websocket.State == WebSocketState.Open)
                {
                    var result = await _websocket.ReceiveAsync(buffer, _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        OnStatusChanged("WebSocket disconnected.");
                    }
                    else
                    {
                        string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        ParseAndRaiseLiveUpdate(message);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred("Receive loop error: " + ex);
            }
        }

        private void ParseAndRaiseLiveUpdate(string message)
        {
            LogHelper.Log($"[WebSocket] Received message: {message}");
            try
            {
                // Deserialize the received JSON message into a dynamic object.
                var msg = JsonConvert.DeserializeObject<dynamic>(message);
                // Extract the event type and the "data" field from the received message.
                string eventType = msg.@event;
                string dataStr = msg.data;
                object parsedData = null; // This will hold our final parsed data.

                // Switch based on the event type.
                switch (eventType)
                {

                    // Handle member creation notifications.
                    case "notification.member_created":
                        // Parse for logging
                        parsedData = JsonConvert.DeserializeObject<MemberCreated>(dataStr);
                        var createdMember = (MemberCreated)parsedData;

                        // Log creation
                        LogHelper.Log($"🆕 Member created - ID: {createdMember.id}, " +
                                    $"Status: {createdMember.status}, " +
                                    $"RFID: {createdMember.rfid}, " +
                                    $"Created At: {createdMember.created_at}");

                        // Forward complete data
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs
                        {
                            EventType = eventType,
                            Data = JObject.Parse(dataStr) // Full original data
                        });
                        break;

                    case "notification.member_updated":
                        // Parse for logging
                        parsedData = JsonConvert.DeserializeObject<MemberUpdated>(dataStr);
                        var updatedMember = (MemberUpdated)parsedData;

                        // Log update
                        LogHelper.Log($"🔄 Member updated - ID: {updatedMember.id}, " +
                                    $"Status: {updatedMember.status}, " +
                                    $"RFID: {updatedMember.rfid}, " +
                                    $"Updated At: {updatedMember.updated_at}");

                        // Forward complete data
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs
                        {
                            EventType = eventType,
                            Data = JObject.Parse(dataStr) // Full original data
                        });
                        break;

                    case "notification.member_deleted":
                        // Parse for logging
                        parsedData = JsonConvert.DeserializeObject<MemberDeleted>(dataStr);
                        var deletedMember = (MemberDeleted)parsedData;

                        // Log deletion
                        LogHelper.Log($"🗑️ Member deleted - ID: {deletedMember.id}, " +
                                    $"Status: {deletedMember.status}, " +
                                    $"RFID: {deletedMember.rfid}, " +
                                    $"Deleted At: {deletedMember.deleted_at}");
                        // Forward complete data
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs
                        {
                            EventType = eventType,
                            Data = JObject.Parse(dataStr) // Full original data
                        });
                        break;

                    // Handle status notifications.
                    case "notification.status":
                        // Parse and extract only needed fields
                        var statusData = JsonConvert.DeserializeObject<NotificationStatus>(dataStr);

                        // Log only ID and status
                        LogHelper.Log($"Member Status Update - ID: {statusData.id}, Status: {statusData.status}");

                        // Forward only ID and status
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs
                        {
                            EventType = eventType,
                            Data = new JObject(
                                new JProperty("id", statusData.id),
                                new JProperty("status", statusData.status)
                            )
                        });
                        break;

                    // Handle attendance notifications.
                    case "gym.attendance":
                        var obj = !string.IsNullOrWhiteSpace(dataStr) ? JObject.Parse(dataStr) : new JObject();
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs { EventType = "gym.attendance", Data = obj });
                        break;

                    // Handle responses to client commands.
                    case "client-command:response":
                        // Deserialize the inner response object.
                        var inner = JsonConvert.DeserializeObject<dynamic>(dataStr);
                        // Check if the payload's command is "member.attendance".
                        // The response might either have a flat structure:
                        //    inner.payload.command == "member.attendance"
                        // or a nested structure (with an extra payload layer).

                        // Handle enrollment assignment response
                        if (inner.payload != null && inner.payload.command == "member.assign")
                        {
                            var responseData = JsonConvert.DeserializeObject<dynamic>(inner.data.ToString());
                            if (responseData.status == 200)
                            {
                                int memberId = responseData.data.id;
                                int? enrollmentId = responseData.data.enrollment_id;

                                _dbHelper.UpdateMemberEnrollment(memberId, enrollmentId);
                                LogHelper.Log($"🔄 Updated member {memberId} with enrollment ID {enrollmentId}");

                                parsedData = new
                                {
                                    MemberId = memberId,
                                    EnrollmentId = enrollmentId,
                                    Status = "Success"
                                };
                                eventType = "member.assign.response";
                                break;
                            }
                        }
                        else if (inner.payload != null && inner.payload.command == "member.attendance")
                        {
                            // For a flat attendance response, extract the data.
                            parsedData = inner.payload.data;
                            eventType = "member.attendance.response";
                        }
                        else if (inner.payload != null && inner.payload.payload != null && inner.payload.payload.command == "member.attendance")
                        {
                            // For a nested attendance response, extract the inner data.
                            parsedData = inner.payload.payload.data;
                            eventType = "member.attendance.response";
                        }
                        // Check if the response is for a bulk member fetch.
                        else if (inner.payload?.command == "member.all")
                        {
                            var data = inner.payload.data;
                            int currentPage = data.current_page != null ? (int)data.current_page : _currentMemberPage;
                            int lastPage = data.last_page != null ? (int)data.last_page : currentPage;

                            OnStatusChanged($"Received page {currentPage} of {lastPage}");

                            _currentMemberPage = currentPage;
                            _lastMemberPage = lastPage;

                            LiveUpdate?.Invoke(this, new LiveUpdateEventArgs { EventType = "member.all", Data = data });

                            Task.Run(() => SaveMemberPicturesAsync(data));

                            if (_isPaginatedSyncInProgress && currentPage < lastPage)
                            {
                                int nextPage = currentPage + 1;
                                _ = RequestMemberPageAsync(nextPage);
                            }
                            else
                            {
                                _isPaginatedSyncInProgress = false;
                                AllMembersSynced?.Invoke(this, EventArgs.Empty);
                                OnStatusChanged("All members synced.");
                            }
                            // Return early because we've fully handled the bulk fetch.
                            return;
                        }
                        // Check if the response is for a single member fetch.
                        else if (inner.payload?.command == "member.get")
                        {
                            parsedData = inner.payload.data;
                            eventType = "member.get";
                        }
                        else if (inner.payload?.command == "member.status.set")
                        {
                            // Set the event type directly
                            eventType = "member.status.set";
                            parsedData = inner.payload.data;

                            LogHelper.Log($"📊 Member status set response received");

                        }
                        else
                        {
                            // For any other client-command response, use the entire inner object.
                            parsedData = inner;
                        }
                        break;

                    //Abdul hadi code
                    // Handle ping messages from the server.
                    case "pusher:pong":
                        LastPongReceived = DateTime.Now;
                        parsedData = "Pong";
                        //LogHelper.Log("Pong");
                        break;

                    case "pusher_internal:subscription_succeeded":
                        _subscriptionTcs?.TrySetResult(true);
                        OnStatusChanged("Subscription confirmed by server.");
                        return; // No need to raise LiveUpdate for internal confirmation

                    // For any unrecognized event type, pass the raw message.
                    default:
                        parsedData = message;
                        break;
                }

                // Raise the LiveUpdate event so that the Processor or other subscribers can handle the event.
                LiveUpdate?.Invoke(this, new LiveUpdateEventArgs { EventType = eventType, Data = parsedData });
            }
            catch (Exception ex)
            {
                // If an error occurs during parsing, raise an error event.
                OnErrorOccurred("Error parsing message: " + ex);
            }
        }


        protected virtual void OnStatusChanged(string status) => StatusChanged?.Invoke(this, status);
        protected virtual void OnErrorOccurred(string error) => ErrorOccurred?.Invoke(this, error);


        //Abdul Hadi Code

        public async Task ConnectionStatusAsync()
        {
            if (_websocket?.State != WebSocketState.Open)
            {
                OnErrorOccurred("WebSocket is not connected.");
                return;
            }

            string pingmessage = JsonConvert.SerializeObject(new
            {
                @event = "pusher:ping",
                data = new { }
            });

            await SendMessageAsync(pingmessage);
            OnStatusChanged("Ping message sent.");
        }

        //Abdul Hadi Code
        // This method sets the status of a member based on their enrollment ID and authentication type.
        public async Task SetMemberStatusAsync(int enrollmentId, string authType, string status, string message )
        {
            if (_websocket?.State != WebSocketState.Open)
            {
                OnErrorOccurred("WebSocket is not connected.");
                return;
            }

            try
            {
                var payload = new
                {
                    enrollment_id = enrollmentId,
                    auth_type = authType,
                    status = status,
                    message = message
                };

                await SendCommandAsync("member.status.set", payload);
                LogHelper.Log($"Member status update sent - Enrollment ID: {enrollmentId}, Auth Type: {authType}, Status: {status}");
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Error setting member status: {ex.Message}");
                LogHelper.Log($"Error setting member status: {ex}");
            }
        }

        private async Task SaveMemberPicturesAsync(dynamic membersData)
        {
            try
            {
                string saveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "FACE");
                Directory.CreateDirectory(saveDirectory);

                foreach (var member in membersData.members)
                {
                    try
                    {
                        if (member.picture != null && !string.IsNullOrEmpty(member.picture.ToString()))
                        {
                            string newImageUrl = member.picture.ToString();
                            string memberId = member.id.ToString();
                            string fileName = $"{memberId}.jpg";
                            string filePath = Path.Combine(saveDirectory, fileName);

                            LogHelper.Log($"Processing Member ID: {memberId}, New Image URL: {newImageUrl}");

                            if (!File.Exists(filePath))
                            {
                                // File doesn't exist, download new image
                                await DownloadAndSaveImageAsync(newImageUrl, filePath);
                                LogHelper.Log($"Saved: {fileName}");
                            }
                            else
                            {
                                // File exists, check if URL has changed
                                var existingMember = _dbHelper.GetMember(int.Parse(memberId));
                                string storedImageUrl = existingMember?.Picture;

                                if (storedImageUrl != newImageUrl)
                                {
                                    // URL has changed, delete old image and download new one
                                    LogHelper.Log($"Image URL changed for Member ID: {memberId}");
                                    LogHelper.Log($"Old URL: {storedImageUrl ?? "None"}");
                                    LogHelper.Log($"New URL: {newImageUrl}");

                                    try
                                    {
                                        // Delete the old image file
                                        File.Delete(filePath);
                                        LogHelper.Log($"Deleted old image: {fileName}");

                                        // Download and save the new image
                                        await DownloadAndSaveImageAsync(newImageUrl, filePath);
                                        LogHelper.Log($"Downloaded and saved new image: {fileName}");
                                    }
                                    catch (Exception deleteEx)
                                    {
                                        LogHelper.Log($"Error replacing image for Member ID {memberId}: {deleteEx.Message}");
                                    }
                                }
                                else
                                {
                                    LogHelper.Log($"Image URL unchanged for Member ID: {memberId}, skipping download");
                                }
                            }
                        }
                        else
                        {
                            LogHelper.Log($"Missing image for Member ID: {member.id}");

                            // If member has no picture URL but file exists, optionally delete the old file
                            string memberId = member.id.ToString();
                            string fileName = $"{memberId}.jpg";
                            string filePath = Path.Combine(saveDirectory, fileName);

                            if (File.Exists(filePath))
                            {
                                try
                                {
                                    File.Delete(filePath);
                                    LogHelper.Log($"Deleted orphaned image file: {fileName} (member has no picture URL)");
                                }
                                catch (Exception deleteEx)
                                {
                                    LogHelper.Log($"Error deleting orphaned image for Member ID {memberId}: {deleteEx.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Error processing image for Member ID {member.id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("General error in SaveMemberPicturesAsync: " + ex.Message);
            }
        }

        private async Task DownloadAndSaveImageAsync(string imageUrl, string savePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                    await Task.Run(() => File.WriteAllBytes(savePath, imageBytes));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error downloading image from {imageUrl}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deletes the face image for a specific member from the FACE directory if it exists.
        /// </summary>
        /// <param name="memberId">The member ID whose face image should be deleted</param>
        /// <returns>True if the image was found and deleted, false otherwise</returns>
        public async Task<bool> DeleteMemberFaceImageAsync(int memberId)
        {
            try
            {
                string faceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "FACE");

                if (!Directory.Exists(faceDirectory))
                {
                    LogHelper.Log($"⚠️ FACE directory does not exist: {faceDirectory}");
                    return false;
                }

                // Check for common image extensions
                string[] extensions = { ".jpg", ".jpeg", ".png" };
                bool imageDeleted = false;

                foreach (string extension in extensions)
                {
                    string imagePath = Path.Combine(faceDirectory, $"{memberId}{extension}");

                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            // Use Task.Run to make file deletion asynchronous
                            await Task.Run(() => File.Delete(imagePath));
                            LogHelper.Log($"✅ Successfully deleted face image: {imagePath}");
                            imageDeleted = true;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"❌ Failed to delete face image {imagePath}: {ex.Message}");
                            return false;
                        }
                    }
                }

                if (!imageDeleted)
                {
                    LogHelper.Log($"ℹ️ No face image found for member {memberId} in FACE directory");
                }

                return imageDeleted;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error deleting face image for member {memberId}: {ex.Message}");
                return false;
            }
        }
    }
}
