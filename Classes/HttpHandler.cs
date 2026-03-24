using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


namespace smart_attendance_middleware
{
    /// <summary>
    /// Simple HTTP helper with a dedicated method for bulk attendance import.
    /// </summary>
    public sealed class HttpHandler : IDisposable
    {
        private readonly HttpClient _client;
        private readonly DatabaseHelper _dbHelper;
        private readonly Processor _processor;

        private const string BaseUrl = "https://ws-stg.fitnessmarvel.com";
        private const string GetMembersEndpoint = "/api/sync/members-staff";
        private const string UploadAttendanceEndpoint = "/api/attendance/upload";

        private const string ApiKeySettingKey = "GWTToken";

        public HttpHandler(DatabaseHelper dbHelper, Processor processor)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _client = new HttpClient();
        }

        public HttpHandler(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _client = new HttpClient();
        }

        private void SetBearerAuthorization()
        {
            string apiKey = _dbHelper.GetSetting(ApiKeySettingKey);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("API key missing in settings (key: APIKey).");
            }

            // Clear any previous Authorization header and set Bearer
            _client.DefaultRequestHeaders.Authorization = null;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }



        /// <summary>
        /// Uploads a single attendance record.
        /// Required: enrollmentId, source (hardcoded "biometric").
        /// Optional: id, checkin_time, checkout_time (ISO 8601 strings, e.g. 2024-01-01T10:00:00Z).
        /// </summary>
        public async Task<JObject> UploadAttendanceRecordAsync(
            int enrollmentId,
            string checkinTime = null,
            string checkoutTime = null,
            string id = null,
            CancellationToken token = default(CancellationToken))
        {
            try
            {
                SetBearerAuthorization();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ {ex.Message} Cannot upload attendance.");
                return null;
            }

            // Build payload as specified
            var record = new JObject
            {
                ["enrollment_id"] = enrollmentId,
                ["source"] = "biometric"
            };

            if (id != null) record["id"] = id;
            if (!string.IsNullOrWhiteSpace(checkinTime)) record["checkin_time"] = checkinTime;
            if (!string.IsNullOrWhiteSpace(checkoutTime))
            {
                record["checkout_time"] = checkoutTime;
                // Only include source_out when there is a checkout value
                record["source_out"] = "biometric";
            }

            var payload = new JObject
            {
                ["records"] = new JArray(record)
            };

            var requestUrl = BaseUrl + UploadAttendanceEndpoint;
            LogHelper.Log($"➡️ POST {requestUrl}");
            LogHelper.Log($"🧾 Body: {payload.ToString(Formatting.None)}");

            using (var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(requestUrl, content, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.Log($"📤 Upload attendance response: {raw}");
                    LogHelper.Log($"❌ Upload attendance HTTP {response.StatusCode}: {raw}");
                    return null;
                }

                try
                {
                    return JObject.Parse(raw);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Failed to parse upload response: {ex.Message}. Raw: {raw}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Uploads multiple attendance records in one request to the same endpoint.
        /// Each record requires enrollment_id and has source hardcoded to "biometric".
        /// Optional per-record: id, checkin_time, checkout_time (ISO 8601 strings, e.g. 2024-01-01T10:00:00Z).
        /// </summary>
        public async Task<JObject> UploadAttendanceRecordsAsync(
            IEnumerable<(int enrollmentId, string checkinTime, string checkoutTime, long? id)> records,
            CancellationToken token = default(CancellationToken))
        {
            if (records == null)
            {
                LogHelper.Log("❌ UploadAttendanceRecordsAsync called with null records collection.");
                return null;
            }

            try
            {
                SetBearerAuthorization();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ {ex.Message} Cannot upload attendance.");
                return null;
            }

            var array = new JArray();
            foreach (var r in records)
            {
                // enrollment_id is required
                var rec = new JObject
                {
                    ["enrollment_id"] = r.enrollmentId,
                    ["source"] = "biometric"
                };

                if (r.id.HasValue) rec["id"] = r.id.Value;
                if (!string.IsNullOrWhiteSpace(r.checkinTime)) rec["checkin_time"] = r.checkinTime;
                if (!string.IsNullOrWhiteSpace(r.checkoutTime)) rec["checkout_time"] = r.checkoutTime;

                array.Add(rec);
            }

            var payload = new JObject
            {
                ["records"] = array
            };

            using (var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(BaseUrl + UploadAttendanceEndpoint, content, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.Log($"📤 Upload attendance (bulk) response: {raw}");
                    LogHelper.Log($"❌ Upload attendance (bulk) HTTP {response.StatusCode}: {raw}");
                    return null;
                }

                try
                {
                    return JObject.Parse(raw);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Failed to parse bulk upload response: {ex.Message}. Raw: {raw}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Retrieves all members from the cloud.
        /// </summary>
        public async Task<(List<JObject> members, string syncedAt)> GetAllMembersAsync(CancellationToken token)
        {
            try
            {
                SetBearerAuthorization();
            }
            catch (System.Exception ex)
            {
                LogHelper.Log($"❌ {ex.Message} Cannot get members.");
                return (null, null);
            }

            // Build request URL with optional last_sync
            var requestUrl = BaseUrl + GetMembersEndpoint;
            string lastSync = null;
            try
            {
                lastSync = _dbHelper.GetSetting("last_sync");
            }
            catch (Exception ex)
            {
                // Non-fatal: just log if settings retrieval fails
                LogHelper.Log($"⚠️ Failed to read 'lastsync' setting: {ex.Message}");
            }

            if (!string.IsNullOrWhiteSpace(lastSync))
            {
                // Ensure proper URL escaping
                requestUrl = $"{requestUrl}?last_sync={Uri.EscapeDataString(lastSync)}";
            }

            LogHelper.Log($"➡️ GET {requestUrl}");

            using (var response = await _client.GetAsync(requestUrl, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.Log($"📤 Get members response: {raw}");
                    LogHelper.Log($"❌ Get members HTTP {response.StatusCode}: {raw}");
                    return (null, null);
                }

                try
                {
                    var root = JObject.Parse(raw);
                    var dataArray = root["data"] as JArray;
                    var syncedAt = root["synced_at"]?.ToString();

                    if (dataArray == null)
                    {
                        LogHelper.Log("❌ 'data' array not found in response.");
                        return (new List<JObject>(), syncedAt);
                    }
                    return (dataArray.OfType<JObject>().ToList(), syncedAt);
                }
                catch (System.Exception ex)
                {
                    LogHelper.Log($"❌ Failed to parse members response: {ex.Message}. Raw: {raw}");
                    return (null, null);
                }
            }
        }








        /// /////////////////////////////////////////Wrapper of Requests//////////////////////////////////////////


        /// <summary>
        /// Fetches all members, logs the result, and passes them to the Processor to be handled.
        /// </summary>
        public async Task FetchAllMembers(CancellationToken token)
        {
            if (_processor == null)
            {
                LogHelper.Log("❌ Processor not initialized in HttpHandler. Cannot process members.");
                return;
            }

            var (members, syncedAt) = await GetAllMembersAsync(token).ConfigureAwait(false);
            if (members == null)
            {
                LogHelper.Log("❌ Failed to fetch members for processing.");
                return;
            }

            LogHelper.Log($"✅ Fetched {members.Count} members for processing.");

            // The HandleHttpEvent expects a single JObject, so we wrap the list in a root object.
            var dataToProcess = new JObject
            {
                ["members"] = new JArray(members),
                ["synced_at"] = syncedAt
            };
            await _processor.HandleHttpEvent("member.all", dataToProcess).ConfigureAwait(false);
        }


        /// <summary>
        /// Uploads a single attendance record and passes the response to the Processor.
        /// </summary>
        public async Task PostAttendanceRecordAsync(
            int enrollmentId,
            string checkinTime = null,
            string checkoutTime = null,
            string id = null,
            CancellationToken token = default(CancellationToken))
        {
            if (_processor == null)
            {
                LogHelper.Log("❌ Processor not initialized in HttpHandler. Cannot process attendance response.");
                return;
            }

            var response = await UploadAttendanceRecordAsync(enrollmentId, checkinTime, checkoutTime, id, token).ConfigureAwait(false);
            if (response == null)
            {
                LogHelper.Log("❌ Failed to upload attendance record, no response to process.");
                return;
            }

            await _processor.HandleHttpEvent("attendance.response", response).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads multiple attendance records and passes the response to the Processor.
        /// </summary>
        public async Task PostAttendanceRecordsBulkAsync(
            IEnumerable<(int enrollmentId, string checkinTime, string checkoutTime, long? id)> records,
            CancellationToken token = default(CancellationToken))
        {
            if (_processor == null)
            {
                LogHelper.Log("❌ Processor not initialized in HttpHandler. Cannot process bulk attendance response.");
                return;
            }

            var response = await UploadAttendanceRecordsAsync(records, token).ConfigureAwait(false);
            if (response == null)
            {
                LogHelper.Log("❌ Failed to upload bulk attendance records, no response to process.");
                return;
            }

            // Route to the bulk response handler in Processor
            await _processor.HandleHttpEvent("attendance.response.bulk", response).ConfigureAwait(false);
        }




        public void Dispose()
        {
            _client.Dispose();
        }
    }

    #region Bulk DTOs

    public sealed class BulkAttendanceRequest
    {
        [JsonProperty("attendances")]
        public System.Collections.Generic.List<BulkAttendanceItem> Attendances { get; set; }
    }

    public sealed class BulkAttendanceItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("attendee_id")]
        public int AttendeeId { get; set; }

        [JsonProperty("check_in")]
        public string CheckIn { get; set; }

        [JsonProperty("check_out")]
        public string CheckOut { get; set; }
    }

    public sealed class BulkAttendanceResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public BulkAttendanceResponseData Data { get; set; }
    }

    public sealed class BulkAttendanceResponseData
    {
        [JsonProperty("processed")]
        public int Processed { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("attendances")]
        public System.Collections.Generic.List<BulkAttendanceResponseItem> Attendances { get; set; }

        [JsonProperty("errors")]
        public System.Collections.Generic.List<object> Errors { get; set; }
    }

    public sealed class BulkAttendanceResponseItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("member_id")]
        public int MemberId { get; set; }

        [JsonProperty("check_in")]
        public string CheckIn { get; set; }

        [JsonProperty("check_out")]
        public string CheckOut { get; set; }
    }

    #endregion
}