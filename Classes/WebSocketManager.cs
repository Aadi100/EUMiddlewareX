using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace smart_attendance_middleware
{
    public class LiveUpdateEventArgs : EventArgs
    {
        public string EventType { get; set; }
        public object Data { get; set; }
    }



    public class Attendance
    {
        public string id { get; set; }
        public string check_in { get; set; }
        public string check_out { get; set; }
        public string source { get; set; }
        public string source_out { get; set; }
        public bool is_staff { get; set; }
        public bool is_manual { get; set; }
        public string reason { get; set; }
        public Member member { get; set; }
    }

    public class UserCreated
    {
        public string action { get; set; }
        public Member user { get; set; }  // Reusing Member class as it has the same structure
        public string timestamp { get; set; }
        public int gym_id { get; set; }
        public int branch_id { get; set; }
        public string channel { get; set; }
    }

    public class UserUpdated
    {
        public string action { get; set; }
        public Member user { get; set; }  // Reusing Member class as it has the same structure
        public string timestamp { get; set; }
        public int gym_id { get; set; }
        public int branch_id { get; set; }
        public string channel { get; set; }
    }

    public class WebSocketManager
    {
        // Pusher/Soketi defaults
        private const string DefaultHost = "ws-stg.fitnessmarvel.com";
        private const int DefaultTlsPort = 443;
        private const int DefaultWsPort = 6001;
        private const string DefaultAppKey = "ab28e935c0c63b8fabb4";
        private const string PusherProtocolVersion = "7";
        private const string PusherClientName = "csharp";
        private const string PusherClientVersion = "1.0.0";

        private ClientWebSocket _websocket;
        private CancellationTokenSource _cts;

        // Pusher/Soketi connection-related
        private string _host = DefaultHost;
        private int _port = DefaultWsPort;
        private string _appKey = DefaultAppKey;
        private string _jwtToken;
        private string _socketId;
        private int _activityTimeoutSeconds = 120;
        private string _activeChannel;
        private string _defaultChannel;
        private bool _autoSubscribeOnConnect = true;
        private bool _useTls = true;

        private readonly DatabaseHelper _dbHelper;

        public event EventHandler<string> StatusChanged;
        public event EventHandler<string> ErrorOccurred;
        public event EventHandler<LiveUpdateEventArgs> LiveUpdate;

        private TaskCompletionSource<bool> _subscriptionTcs; // subscription confirmation
        public DateTime LastPongReceived { get; private set; } = DateTime.MinValue;

        // Public accessors
        public string SocketId => _socketId;
        public string ActiveChannel => _activeChannel;
        public bool IsOpen => _websocket?.State == WebSocketState.Open; // NEW

        public WebSocketManager(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));

            try
            {
                var gymId = _dbHelper.GetSetting("GymId");
                var jwt = _dbHelper.GetSetting("GWTToken");

                _jwtToken = string.IsNullOrWhiteSpace(jwt) ? null : jwt.Trim();
                _defaultChannel = string.IsNullOrWhiteSpace(gymId) ? null : $"private-gym.{gymId.Trim()}";

                OnStatusChanged($"Settings loaded. Channel='{_defaultChannel ?? "(none)"}', JWT={(string.IsNullOrEmpty(_jwtToken) ? "(none)" : "present")}.");
            }
            catch (Exception ex)
            {
                OnErrorOccurred("Error loading WebSocket settings: " + ex.Message);
            }
        }

        // New configuration for Pusher/Soketi
        public void ConfigurePusher()
        {
            // Always use built-in defaults
            _host = DefaultHost;
            _port = _useTls ? DefaultTlsPort : DefaultWsPort;
            _appKey = DefaultAppKey;

            OnStatusChanged($"Pusher configured: host={_host}, port={_port}, appKey={_appKey}");
        }

        // NEW: control TLS (wss) vs cleartext (ws)
        public void SetUseTls(bool useTls)
        {
            _useTls = useTls;
            OnStatusChanged($"Transport set to {(_useTls ? "wss" : "ws")}");
        }

        public void ConfigureDefaultChannel(string channelName, bool autoSubscribeOnConnect = true)
        {
            if (!string.IsNullOrWhiteSpace(channelName)) _defaultChannel = channelName.Trim();
            _autoSubscribeOnConnect = autoSubscribeOnConnect;
            OnStatusChanged($"Default channel set to '{_defaultChannel}', autoSubscribe={_autoSubscribeOnConnect}");
        }

        public void SetSupabaseJwt(string jwtToken)
        {
            _jwtToken = jwtToken;
            OnStatusChanged("JWT token set for Pusher auth.");
        }

        private Uri BuildPusherUri()
        {
            var scheme = _useTls ? "wss" : "ws";
            var portPart = _useTls && _port == 443 ? "" : $":{_port}";
            var qs = $"protocol={PusherProtocolVersion}&client={PusherClientName}&version={PusherClientVersion}";
            var url = $"{scheme}://{_host}{portPart}/app/{_appKey}?{qs}";
            return new Uri(url);
        }

        public async Task ConnectAsync()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

                _websocket = new ClientWebSocket();
                _websocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(Math.Max(15, _activityTimeoutSeconds / 2)); // keepalive
                _cts = new CancellationTokenSource();

                var uri = BuildPusherUri();
                OnStatusChanged("Connecting to " + uri);
                await _websocket.ConnectAsync(uri, _cts.Token);
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

        // Subscribe to a channel (supports private- channels via HTTP auth)
        public async Task<bool> SubscribeAsync(string channelName, string jwtToken = null)
        {
            if (_websocket?.State != WebSocketState.Open)
            {
                OnErrorOccurred("WebSocket is not connected.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(channelName))
            {
                OnErrorOccurred("Channel name is required.");
                return false;
            }

            _subscriptionTcs = new TaskCompletionSource<bool>();

            string auth = null;
            if (channelName.StartsWith("private-", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(_socketId))
                {
                    OnErrorOccurred("Socket ID not established yet. Wait for 'pusher:connection_established'.");
                    return false;
                }

                var token = jwtToken ?? _jwtToken;
                if (string.IsNullOrWhiteSpace(token))
                {
                    OnErrorOccurred("JWT token is required for private channel authentication.");
                    return false;
                }

                auth = await GetPusherAuthAsync(channelName, token);
                if (string.IsNullOrWhiteSpace(auth))
                {
                    OnErrorOccurred("Private channel authentication failed.");
                    return false;
                }
            }

            var subscribeFrame = new
            {
                @event = "pusher:subscribe",
                data = new
                {
                    channel = channelName,
                    auth = auth
                }
            };

            await SendMessageAsync(JsonConvert.SerializeObject(subscribeFrame));
            OnStatusChanged("📡 Subscription request sent. Awaiting confirmation...");

            var completedTask = await Task.WhenAny(_subscriptionTcs.Task, Task.Delay(10000));
            if (completedTask == _subscriptionTcs.Task && _subscriptionTcs.Task.Result)
            {
                _activeChannel = channelName;
                OnStatusChanged($"Subscription confirmed by server for '{channelName}'.");
                LogHelper.Log($"Subscription confirmed by server for '{channelName}'.");
                return true;
            }

            OnErrorOccurred("Subscription failed or timed out.");
            return false;
        }

        public async Task<bool> ConnectAndSubscribeAsync(bool preferTls = true, int handshakeTimeoutMs = 5000, bool fallbackToCleartext = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_defaultChannel))
                {
                    OnErrorOccurred("Channel is required.");
                    return false;
                }

                // Prevent auto-subscribe to avoid race with manual flow
                ConfigureDefaultChannel(_defaultChannel, autoSubscribeOnConnect: false);

                // Connect if not already open
                if (!IsOpen)
                {
                    ConfigurePusher();

                    if (preferTls)
                    {
                        // Try secure reverse-proxy first (wss:443)
                        SetUseTls(true);
                        OnStatusChanged("Connecting securely (wss:443)...");
                        var tlsOk = await TryConnectAsyncSilently();
                        if (!tlsOk && fallbackToCleartext)
                        {
                            OnStatusChanged("Secure connect failed; falling back to cleartext (ws:6001)...");
                            SetUseTls(false);
                            var wsOk = await TryConnectAsyncSilently();
                            if (!wsOk)
                            {
                                OnErrorOccurred("WebSocket not open after connection attempts.");
                                return false;
                            }
                        }
                        else if (!tlsOk)
                        {
                            OnErrorOccurred("Secure connect failed and fallback disabled.");
                            return false;
                        }
                    }
                    else
                    {
                        // Prefer cleartext directly
                        SetUseTls(false);
                        OnStatusChanged("Connecting (ws:6001)...");
                        var wsOk = await TryConnectAsyncSilently();
                        if (!wsOk)
                        {
                            OnErrorOccurred("WebSocket not open after connection attempt.");
                            return false;
                        }
                    }
                }
                else
                {
                    OnStatusChanged("WebSocket already open.");
                }

                // Wait for pusher:connection_established (socket_id required for private auth)
                var deadline = DateTime.UtcNow.AddMilliseconds(Math.Max(1000, handshakeTimeoutMs));
                while (string.IsNullOrEmpty(SocketId) && DateTime.UtcNow < deadline)
                {
                    await Task.Delay(50);
                }

                if (string.IsNullOrEmpty(SocketId))
                {
                    OnErrorOccurred("Handshake did not provide socket_id within timeout.");
                    return false;
                }

                // Subscribe
                OnStatusChanged($"Subscribing to '{_defaultChannel}'...");
                var isSubscribed = await SubscribeAsync(_defaultChannel);
                if (!isSubscribed)
                {
                    OnErrorOccurred("Subscription failed.");
                    return false;
                }

                OnStatusChanged($"Connected and subscribed to {_defaultChannel}.");
                return true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred("ConnectAndSubscribeAsync error: " + ex.Message);
                return false;
            }
        }

        // Helper: attempt connect and return success/failure without dumping exceptions to logs
        private async Task<bool> TryConnectAsyncSilently()
        {
            try
            {
                await ConnectAsync();
                return IsOpen;
            }
            catch
            {
                // Swallow here; detailed logging happens inside ConnectAsync already.
                return false;
            }
        }

        // Send application command via client event on active channel
        public async Task SendCommandAsync(string command, object payload)
        {
            if (_websocket?.State != WebSocketState.Open || string.IsNullOrWhiteSpace(_activeChannel))
            {
                OnErrorOccurred(string.IsNullOrWhiteSpace(_activeChannel) ? "No active channel. Subscribe to a channel first." : "WebSocket is not connected.");
                return;
            }

            string payloadJson = JsonConvert.SerializeObject(payload);
            var normalizedPayload = ParsePayload(payloadJson);

            var eventData = new
            {
                data = new
                {
                    command = command,
                    payload = normalizedPayload
                }
            };

            await SendClientEventAsync("client-command", eventData, _activeChannel, wrapDataObject: true);
            OnStatusChanged($"Command sent: {command}");
        }
        
        // Generic client event sender (supports raw object envelope or stringified payload)
        public async Task SendClientEventAsync(string eventName, object data, string channelName = null, bool wrapDataObject = false)
        {
            if (_websocket?.State != WebSocketState.Open)
            {
                OnErrorOccurred("WebSocket is not connected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(eventName) || !eventName.StartsWith("client-", StringComparison.OrdinalIgnoreCase))
            {
                OnErrorOccurred("Client events must start with 'client-'.");
                return;
            }

            var channel = channelName ?? _activeChannel;
            if (string.IsNullOrWhiteSpace(channel))
            {
                OnErrorOccurred("No channel provided and no active channel set.");
                return;
            }

            // For server that expects nested { data: { ... } } object, do NOT stringify.
            var frame = new
            {
                @event = eventName,
                channel = channel,
                data = wrapDataObject ? data : JsonConvert.SerializeObject(data)
            };

            await SendMessageAsync(JsonConvert.SerializeObject(frame));
            OnStatusChanged($"Client event sent: {eventName}");
        }

        private async Task<string> GetPusherAuthAsync(string channelName, string jwtToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var form = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "socket_id", _socketId },
                        { "channel_name", channelName }
                    });

                    // The auth endpoint is HTTPS without the WS port
                    var authUrl = $"https://{_host}/pusher-auth";
                    var resp = await client.PostAsync(authUrl, form);
                    var respBody = await resp.Content.ReadAsStringAsync();

                    if (!resp.IsSuccessStatusCode)
                    {
                        OnErrorOccurred($"Auth failed ({resp.StatusCode}): {respBody}");
                        return null;
                    }

                    var json = JsonConvert.DeserializeObject<JObject>(respBody);
                    return json?["auth"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred("Auth error: " + ex.Message);
                return null;
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
                    using (var ms = new System.IO.MemoryStream())
                    {
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await _websocket.ReceiveAsync(buffer, _cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                                OnStatusChanged("WebSocket disconnected.");
                                return;
                            }

                            ms.Write(buffer.Array, 0, result.Count);
                        } while (!result.EndOfMessage);

                        string message = Encoding.UTF8.GetString(ms.ToArray());
                        await ParseAndRaiseLiveUpdate(message);
                    }
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred("Receive loop error: " + ex);
            }
        }

        private async Task ParseAndRaiseLiveUpdate(string message)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<dynamic>(message);
                string eventType = msg.@event;
                string dataStr = msg.data != null ? (msg.data.Type == JTokenType.String ? (string)msg.data : JsonConvert.SerializeObject(msg.data)) : null;
                object parsedData = null;

                switch (eventType)
                {
                    // Pusher/Soketi handshake
                    case "pusher:connection_established":
                        if (!string.IsNullOrEmpty(dataStr))
                        {
                            var inner = JsonConvert.DeserializeObject<JObject>(dataStr);
                            _socketId = inner?["socket_id"]?.ToString();
                            _activityTimeoutSeconds = inner?["activity_timeout"] != null ? (int)inner["activity_timeout"] : _activityTimeoutSeconds;
                            LastPongReceived = DateTime.Now;
                            OnStatusChanged($"Connected. socket_id={_socketId}, activity_timeout={_activityTimeoutSeconds}s");
                        }

                        // Auto-subscribe
                        if (_autoSubscribeOnConnect && !string.IsNullOrWhiteSpace(_defaultChannel))
                        {
                            _ = SubscribeAsync(_defaultChannel);
                        }
                        return;

                    case "pusher:ping":
                        var pong = JsonConvert.SerializeObject(new { @event = "pusher:pong" });
                        _ = SendMessageAsync(pong);
                        OnStatusChanged("Responded to ping with pong.");
                        return;

                    case "pusher:pong":
                        LastPongReceived = DateTime.Now;
                        parsedData = "Pong";
                        LogHelper.Log("Pong");
                        break;

                    case "pusher_internal:subscription_succeeded":
                        _subscriptionTcs?.TrySetResult(true);
                        OnStatusChanged("Subscription confirmed by server.");
                        return;

                    case "attendance_created":
                        parsedData = JsonConvert.DeserializeObject<Attendance>(dataStr);
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs { EventType = "attendance_created", Data = JObject.Parse(dataStr) });
                        break;

                    case "attendance_updated":
                        parsedData = JsonConvert.DeserializeObject<Attendance>(dataStr);
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs { EventType = "attendance_updated", Data = JObject.Parse(dataStr) });
                        break;

                    case "user_created":
                        parsedData = JsonConvert.DeserializeObject<UserCreated>(dataStr);
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs { EventType = "user_created", Data = JObject.Parse(dataStr) });
                        break;

                    case "user_updated":
                        parsedData = JsonConvert.DeserializeObject<UserUpdated>(dataStr);
                        LiveUpdate?.Invoke(this, new LiveUpdateEventArgs { EventType = "user_updated", Data = JObject.Parse(dataStr) });
                        break;

                    default:
                        parsedData = message;
                        break;
                }

                LiveUpdate?.Invoke(this, new LiveUpdateEventArgs { EventType = eventType, Data = parsedData });
            }
            catch (Exception ex)
            {
                OnErrorOccurred("Error parsing message: " + ex);
            }
        }

        protected virtual void OnStatusChanged(string status) => StatusChanged?.Invoke(this, status);
        protected virtual void OnErrorOccurred(string error) => ErrorOccurred?.Invoke(this, error);

        // Heartbeat: manual ping trigger 
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


        // Robust JSON payload parser using System.Text.Json,
        // returns a Dictionary<string, object?> with nested dictionaries/lists normalized.
        public Dictionary<string, object> ParsePayload(string json)
        {
            if (string.IsNullOrEmpty(json?.Trim()))
            {
                return new Dictionary<string, object>();
            }

            using (var doc = JsonDocument.Parse(json))
            {
                var obj = ConvertJsonElement(doc.RootElement);
                return obj as Dictionary<string, object> ?? new Dictionary<string, object>();
            }
        }

        public static object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = ConvertJsonElement(prop.Value);
                    }
                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(ConvertJsonElement(item));
                    }
                    return list;
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long l)) return l;
                    if (element.TryGetDecimal(out decimal d)) return d;
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                default:
                    return null;
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

                                                                                // Force Face sync status to false for re-enrollment
                                        await _dbHelper.ForceFaceSyncStatusFalseAsync(int.Parse(memberId));
                                        LogHelper.Log($"Set FaceSyncStatus to false for Member ID: {memberId}");
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