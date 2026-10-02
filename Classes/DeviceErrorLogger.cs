using System;
using Newtonsoft.Json.Linq;

namespace smart_attendance_middleware.Classes
{
    public static class DeviceErrorLogger
    {
        /// <summary>
        /// Parse device error response and return formatted error details
        /// </summary>
        public static string ParseDeviceError(JObject response)
        {
            if (response == null) return "Unknown error";

            try
            {
                int? reasonCode = response["reason"]?.Value<int>();
                string message = response["msg"]?.Value<string>() ?? "";

                // Extract conflicting ID if present (e.g., "face is double,id=23783")
                string conflictingId = "";
                if (!string.IsNullOrEmpty(message) && message.Contains("id="))
                {
                    int idIndex = message.LastIndexOf("id=") + 3;
                    conflictingId = message.Substring(idIndex);
                }

                // Format: "Reason:12 | Message:face is double | ConflictID:23783"
                string errorDetails = $"Reason:{reasonCode} | Message:{message}";
                if (!string.IsNullOrEmpty(conflictingId))
                    errorDetails += $" | ConflictID:{conflictingId}";

                return errorDetails;
            }
            catch
            {
                return response.ToString();
            }
        }

        /// <summary>
        /// Log device operation failure to database
        /// </summary>
        public static void LogDeviceOperationFailure(
            DatabaseHelper dbHelper,
            string logTableName,
            int enrollmentId,
            string deviceSN,
            string operationType,
            string errorReason,
            int attemptCount = 1)
        {
            try
            {
                string escapedReason = errorReason.Replace("'", "''");

                // Update the log entry with attempt count and failure reason
                string query = $@"
                    UPDATE {logTableName}
                    SET AttemptCount = AttemptCount + 1,
                        FailureReason = '{escapedReason}'
                    WHERE EnrollmentID = {enrollmentId}
                    AND DeviceSN = '{deviceSN}'
                    AND OperationType = '{operationType}';

                    IF @@ROWCOUNT = 0
                    BEGIN
                        INSERT INTO {logTableName} (EnrollmentID, DeviceSN, OperationType, AttemptCount, FailureReason, LogTime)
                        VALUES ({enrollmentId}, '{deviceSN}', '{operationType}', 1, '{escapedReason}', GETDATE());
                    END";

                dbHelper.ExecuteNonQuery(query);
                LogHelper.Log($"[DeviceErrorLog] Logged error for EnrollmentID:{enrollmentId} Device:{deviceSN}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DeviceErrorLogger] Failed to log error: {ex.Message}");
            }
        }
    }
}
