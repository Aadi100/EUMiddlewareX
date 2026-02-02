using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware
{
    /// <summary>
    /// Simple HTTP helper with a dedicated method for bulk attendance import.
    /// </summary>
    public sealed class HttpHandler : IDisposable
    {
        private readonly HttpClient _client;
        private readonly DatabaseHelper _dbHelper;

        //private const string BulkAttendanceUrl = "https://staging.smartgymstudio.com/api/attendance/bulk-import";
        private const string BulkAttendanceUrl = "https://api.fitnessmarvel.com/api/attendance/bulk-import";
        private const string ApiKeySettingKey = "APIKey";

        public HttpHandler(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _client = new HttpClient();
        }

        /// <summary>
        /// Sends bulk attendance payload to the cloud.
        /// Expects body:
        /// {
        ///   "attendances": [
        ///     { "attendee_id": 101, "check_in": "yyyy-MM-dd HH:mm:ss" },
        ///     { "attendee_id": 102, "check_in": "...", "check_out": "..." }
        ///   ]
        /// }
        /// </summary>
        public async Task<BulkAttendanceResponse> SendBulkAttendanceAsync(
            BulkAttendanceRequest request,
            CancellationToken token)
        {
            if (request == null || request.Attendances == null || request.Attendances.Count == 0)
                return null;

            string apiKey = _dbHelper.GetSetting(ApiKeySettingKey);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                LogHelper.Log("❌ API key missing in settings (key: APIKey). Cannot send bulk attendance.");
                return null;
            }

            _client.DefaultRequestHeaders.Remove("X-API-KEY");
            _client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            string json = JsonConvert.SerializeObject(request);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = await _client.PostAsync(BulkAttendanceUrl, content, token).ConfigureAwait(false))
            {
                var raw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    LogHelper.Log($"📤 Bulk attendance response: {raw}");
                    LogHelper.Log($"❌ Bulk attendance HTTP {response.StatusCode}: {raw}");
                    return null;
                }

                try
                {
                    return JsonConvert.DeserializeObject<BulkAttendanceResponse>(raw);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Failed to parse bulk attendance response: {ex.Message}. Raw: {raw}");
                    return null;
                }
            }
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