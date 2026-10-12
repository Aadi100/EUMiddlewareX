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

        //private const string BaseUrl = "https://eu.ws.fitnessmarvel.com";

        private const string BaseUrl = "https://ws-stg.fitnessmarvel.com";
        private const string GetMembersEndpoint = "/api/sync/members-staff";
        private const string UploadAttendanceEndpoint = "/api/attendance/upload";
        private const string GetCatalogEndpoint = "/api/sync/catalog";
        private const string CatalogSyncSettingKey = "CatalogSync";
        private const string PostEnrollmentsEndpoint = "/api/sync/enrollments";
        private const string PostMemberPictureEndpoint = "/api/sync/member-picture";

        private const string ApiKeySettingKey = "GWTToken";

        public HttpHandler(DatabaseHelper dbHelper, Processor processor)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public HttpHandler(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
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
            string gateSerial = null,
            string gateAlias = null,
            string gateSerialOut = null,
            string gateAliasOut = null,
            CancellationToken token = default(CancellationToken))
        {
            try
            {
                SetBearerAuthorization();
            }
            catch (Exception ex)
            {
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
            if (!string.IsNullOrWhiteSpace(gateSerial)) record["gate_serial"] = gateSerial;
            if (!string.IsNullOrWhiteSpace(gateAlias)) record["gate_alias"] = gateAlias;
            if (!string.IsNullOrWhiteSpace(checkoutTime))
            {
                record["checkout_time"] = checkoutTime;
                // Only include source_out when there is a checkout value
                record["source_out"] = "biometric";
                if (!string.IsNullOrWhiteSpace(gateSerialOut)) record["gate_serial_out"] = gateSerialOut;
                if (!string.IsNullOrWhiteSpace(gateAliasOut)) record["gate_alias_out"] = gateAliasOut;
            }

            var payload = new JObject
            {
                ["records"] = new JArray(record)
            };

            var requestUrl = BaseUrl + UploadAttendanceEndpoint;
            LogHelper.Log($"➡️ POST {requestUrl}");
            LogHelper.LogDev($"🧾 Body: {payload.ToString(Formatting.None)}");

            using (var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(requestUrl, content, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.LogDev($"📤 Upload attendance response: {raw}");
                    LogHelper.LogDev($"❌ Upload attendance HTTP {response.StatusCode}: {raw}");
                    return null;
                }

                try
                {
                    return JObject.Parse(raw);
                }
                catch (Exception ex)
                {
                    LogHelper.LogDev($"❌ Failed to parse upload response: {ex.Message}. Raw: {raw}");
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
            IEnumerable<(int enrollmentId, string checkinTime, string checkoutTime, string id, string gateSerial, string gateAlias, string gateSerialOut, string gateAliasOut)> records,
            CancellationToken token = default(CancellationToken))
        {
            if (records == null)
            {
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
                // Skip records with invalid enrollment ID — should already be filtered upstream
                if (r.enrollmentId <= 0)
                {
                    LogHelper.Log($"⚠️ Skipping record with invalid enrollment_id={r.enrollmentId}");
                    continue;
                }

                var rec = new JObject
                {
                    ["enrollment_id"] = r.enrollmentId,
                    ["source"] = "biometric"
                };

                if (!string.IsNullOrWhiteSpace(r.id)) rec["id"] = r.id;
                if (!string.IsNullOrWhiteSpace(r.checkinTime)) rec["checkin_time"] = r.checkinTime;
                if (!string.IsNullOrWhiteSpace(r.gateSerial)) rec["gate_serial"] = r.gateSerial;
                if (!string.IsNullOrWhiteSpace(r.gateAlias)) rec["gate_alias"] = r.gateAlias;
                if (!string.IsNullOrWhiteSpace(r.checkoutTime))
                {
                    rec["checkout_time"] = r.checkoutTime;
                    rec["source_out"] = "biometric";
                    if (!string.IsNullOrWhiteSpace(r.gateSerialOut)) rec["gate_serial_out"] = r.gateSerialOut;
                    if (!string.IsNullOrWhiteSpace(r.gateAliasOut)) rec["gate_alias_out"] = r.gateAliasOut;
                }

                array.Add(rec);
            }

            var payload = new JObject
            {
                ["records"] = array
            };

            LogHelper.Log($"📤 [BulkUpload] Sending {array.Count} records.");
            LogHelper.LogDev($"🧾 [BulkUpload] Payload: {payload.ToString(Formatting.None)}");

            using (var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(BaseUrl + UploadAttendanceEndpoint, content, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.LogDev($"📤 Upload attendance (bulk) response: {raw}");
                    LogHelper.LogDev($"❌ Upload attendance (bulk) HTTP {response.StatusCode}: {raw}");
                    return null;
                }

                try
                {
                    return JObject.Parse(raw);
                }
                catch (Exception ex)
                {
                    LogHelper.LogDev($"❌ Failed to parse bulk upload response: {ex.Message}. Raw: {raw}");
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
                    LogHelper.LogDev($"📤 Get members response: {raw}");
                    LogHelper.Log($"❌ Get members HTTP {response.StatusCode} ({raw.Length} bytes)");
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
                    LogHelper.LogDev($"❌ Failed to parse members response: {ex.Message}. Raw: {raw}");
                    return (null, null);
                }
            }
        }

        /// <summary>
        /// Retrieves the packages/trainers catalog. Sends no query param for a full pull
        /// when there is no stored CatalogSync watermark, otherwise sends ?last_sync=... .
        /// </summary>
        public async Task<(List<JObject> packages, List<JObject> trainers, string syncedAt)> GetCatalogAsync(CancellationToken token)
        {
            try
            {
                SetBearerAuthorization();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ {ex.Message} Cannot get catalog.");
                return (null, null, null);
            }

            var requestUrl = BaseUrl + GetCatalogEndpoint;
            string lastSync = null;
            try
            {
                lastSync = _dbHelper.GetSetting(CatalogSyncSettingKey);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"⚠️ Failed to read '{CatalogSyncSettingKey}' setting: {ex.Message}");
            }

            if (!string.IsNullOrWhiteSpace(lastSync))
            {
                requestUrl = $"{requestUrl}?last_sync={Uri.EscapeDataString(lastSync)}";
            }

            LogHelper.Log($"➡️ GET {requestUrl}");

            using (var response = await _client.GetAsync(requestUrl, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.LogDev($"📤 Get catalog response: {raw}");
                    LogHelper.Log($"❌ Get catalog HTTP {response.StatusCode} ({raw.Length} bytes)");
                    return (null, null, null);
                }

                try
                {
                    var root = JObject.Parse(raw);
                    var packages = (root["packages"] as JArray)?.OfType<JObject>().ToList() ?? new List<JObject>();
                    var trainers = (root["trainers"] as JArray)?.OfType<JObject>().ToList() ?? new List<JObject>();
                    var syncedAt = root["synced_at"]?.ToString();

                    return (packages, trainers, syncedAt);
                }
                catch (Exception ex)
                {
                    LogHelper.LogDev($"❌ Failed to parse catalog response: {ex.Message}. Raw: {raw}");
                    return (null, null, null);
                }
            }
        }

        /// <summary>
        /// Uploads offline-enrolled members (member + subscription) so the server can create the
        /// real member/enrollment/subscription/invoice records. Returns the parsed response
        /// (uploaded/duplicates_resolved/errors/records[]), or null on failure.
        /// </summary>
        public async Task<JObject> PostEnrollmentsAsync(JArray records, CancellationToken token = default(CancellationToken))
        {
            if (records == null || records.Count == 0)
            {
                return null;
            }

            try
            {
                SetBearerAuthorization();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ {ex.Message} Cannot post enrollments.");
                return null;
            }

            var payload = new JObject { ["records"] = records };
            var requestUrl = BaseUrl + PostEnrollmentsEndpoint;
            LogHelper.Log($"➡️ POST {requestUrl}");
            LogHelper.LogDev($"🧾 Body: {payload.ToString(Formatting.None)}");

            using (var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(requestUrl, content, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.LogDev($"📤 Post enrollments response: {raw}");
                    LogHelper.Log($"❌ Post enrollments HTTP {response.StatusCode} ({raw.Length} bytes)");
                    return null;
                }

                try
                {
                    return JObject.Parse(raw);
                }
                catch (Exception ex)
                {
                    LogHelper.LogDev($"❌ Failed to parse enrollments response: {ex.Message}. Raw: {raw}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Uploads a member's photo (base64 data URL) so the server can store it on the CDN and
        /// hand back a public URL to embed in the enrollment body. Returns null on any failure.
        /// </summary>
        public async Task<string> PostMemberPictureAsync(string base64DataUrl, string firstName, string lastName, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(base64DataUrl))
            {
                return null;
            }

            try
            {
                SetBearerAuthorization();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ {ex.Message} Cannot upload member picture.");
                return null;
            }

            var payload = new JObject { ["picture"] = base64DataUrl };
            if (!string.IsNullOrWhiteSpace(firstName)) payload["first_name"] = firstName;
            if (!string.IsNullOrWhiteSpace(lastName)) payload["last_name"] = lastName;

            var requestUrl = BaseUrl + PostMemberPictureEndpoint;
            LogHelper.Log($"➡️ POST {requestUrl}");

            using (var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(requestUrl, content, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                LogHelper.LogDev($"📤 Post member-picture response: {raw}");

                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.Log($"❌ Post member-picture HTTP {response.StatusCode} ({raw.Length} bytes)");
                    return null;
                }

                try
                {
                    var root = JObject.Parse(raw);
                    var url = root["picture"]?.ToString();
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        LogHelper.LogDev($"❌ member-picture response had no 'picture' url. Raw: {raw}");
                        return null;
                    }
                    return url;
                }
                catch (Exception ex)
                {
                    LogHelper.LogDev($"❌ Failed to parse member-picture response: {ex.Message}. Raw: {raw}");
                    return null;
                }
            }
        }






        /// /////////////////////////////////////////Wrapper of Requests//////////////////////////////////////////


        /// <summary>
        /// Fetches all members, logs the result, and passes them to the Processor to be handled.
        /// </summary>
        public async Task FetchAllMembers(CancellationToken token)
        {
            if (_processor == null) return;

            var (members, syncedAt) = await GetAllMembersAsync(token).ConfigureAwait(false);
            if (members == null) return;

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
            string gateSerial = null,
            string gateAlias = null,
            string gateSerialOut = null,
            string gateAliasOut = null,
            CancellationToken token = default(CancellationToken))
        {
            if (_processor == null) return;

            var response = await UploadAttendanceRecordAsync(enrollmentId, checkinTime, checkoutTime, id, gateSerial, gateAlias, gateSerialOut, gateAliasOut, token).ConfigureAwait(false);
            if (response == null) return;

            await _processor.HandleHttpEvent("attendance.response", response).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads multiple attendance records and passes the response to the Processor.
        /// </summary>
        public async Task PostAttendanceRecordsBulkAsync(
            IEnumerable<(int enrollmentId, string checkinTime, string checkoutTime, string id, string gateSerial, string gateAlias, string gateSerialOut, string gateAliasOut)> records,
            CancellationToken token = default(CancellationToken))
        {
            if (_processor == null) return;

            var response = await UploadAttendanceRecordsAsync(records, token).ConfigureAwait(false);
            if (response == null) return;

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