using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware.Classes
{
    public class DeviceFunctions
    {
        public DatabaseHelper _dbHelper;
        private WebSocketManager _wsManager;
        private Processor _processor;

        // Result class to capture both success and error details
        public class BiometricProcessResult
        {
            public bool Success { get; set; }
            public string ErrorReason { get; set; }
            public int? ConflictingId { get; set; }
        }

        private static readonly object _pendingJobLock = new object();
        private volatile bool _workflowRunning = false;
        private volatile bool _workflowRerunRequested = false;

        public class PendingJob
        {
            public string Action { get; set; } // "enable" | "disable" | "delete"
            public int MemberId { get; set; }
            public int EnrollmentId { get; set; }
            public string Ip { get; set; }
            public int Port { get; set; }
            public int MachineNo { get; set; }
            public string Password { get; set; }
            public int BackupNumber { get; set; } // e.g., 0 for enable/disable, 12 for delete
            public DateTime CreatedAt { get; set; }
            public int? DeviceId { get; set; }
        }

        public DeviceFunctions()
        {
            _dbHelper = new DatabaseHelper();
        }

        // Constructor that accepts a WebSocketManager
        public DeviceFunctions(WebSocketManager wsManager)
        {
            _dbHelper = new DatabaseHelper();
            _wsManager = wsManager;

            // Only create processor if WebSocketManager was provided
            if (_wsManager != null)
            {
                _processor = new Processor(_dbHelper, _wsManager, this);
            }
        }

        // Method to set the WebSocketManager if needed after construction
        public void SetWebSocketManager(WebSocketManager wsManager)
        {
            _wsManager = wsManager;

            // Create processor only if not already created
            if (_processor == null && _wsManager != null)
            {
                _processor = new Processor(_dbHelper, _wsManager, this);
            }
        }

        public async Task ProcessLogCreation(int memberid)
        {
            try
            {
                await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessLogCreation", DbTaskPriority.High);
                try
                {
                    LogHelper.Log("Inserting logs for member ID: " + memberid);
                    await _dbHelper.InsertLogsForAllMembersAsync(memberid);
                    LogHelper.Log("Finished inserting logs for member ID: " + memberid);

                    LogHelper.Log("Verifying and updating biometric logs for member ID: " + memberid);
                    await VerifyAndUpdateBiometricLogsAsync(memberid);
                    LogHelper.Log("Finished verifying and updating biometric logs for member ID: " + memberid);
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("ProcessLogCreation");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error during log creation process: {ex.Message}");
                throw;
            }
        }

        public async Task ProcessStaffLogCreation(string staffId)
        {
            try
            {
                await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessStaffLogCreation", DbTaskPriority.High);
                try
                {
                    LogHelper.Log("Inserting logs for staff ID: " + staffId);
                    await _dbHelper.InsertLogsForAllStaffAsync(staffId);
                    LogHelper.Log("Finished inserting logs for staff ID: " + staffId);

                    LogHelper.Log("Verifying and updating biometric logs for staff ID: " + staffId);
                    await VerifyAndUpdateStaffBiometricLogsAsync(staffId);
                    LogHelper.Log("Finished verifying and updating biometric logs for staff ID: " + staffId);
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("ProcessStaffLogCreation");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error during staff log creation process: {ex.Message}");
                throw;
            }
        }

        // Returns true when the reason string is just a generic timeout/no-ack fallback
        // with no device-supplied detail (e.g. "Face sync failed" with nothing after it).
        private static bool IsGenericTimeoutReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return true;
            string lower = reason.Trim().ToLowerInvariant();
            // Generic fallbacks produced when no LastCommandErrors entry exists (timeout/no-ack)
            return lower == "face sync failed"
                || lower == "fingerprint sync failed"
                || lower == "rfid sync failed"
                || lower == "sync failed";
        }

        private async Task<string> BuildDetailedErrorReason(string biometricType, string deviceSerial, int enrollId)
        {
            try
            {
                // Try to get error details from WebsocketServer
                string errorKey = $"{deviceSerial}_{enrollId}";
                if (WebsocketServer.LastCommandErrors.TryGetValue(errorKey, out var errorResponse))
                {
                    int? reasonCode = errorResponse["reason"]?.Value<int>();
                    string errorMsg = errorResponse["msg"]?.Value<string>() ?? "";

                    // Try to extract conflicting enrollment ID from message
                    // Format: "face is double,id=23783"
                    int conflictingEnrollId = 0;
                    if (errorMsg.Contains("id="))
                    {
                        string idPart = errorMsg.Substring(errorMsg.LastIndexOf("id=") + 3);
                        if (int.TryParse(idPart, out int id))
                            conflictingEnrollId = id;
                    }

                    // Build error message
                    string reason = $"{biometricType} sync failed";

                    if (!string.IsNullOrEmpty(errorMsg))
                        reason += $": {errorMsg}";

                    if (reasonCode.HasValue)
                        reason += $" (Code: {reasonCode.Value})";

                    // Try to get conflicting entity from any of the 3 tables (Member, Staff, Offline)
                    if (conflictingEnrollId > 0)
                    {
                        try
                        {
                            // Try Member table first
                            int? memberId = _dbHelper.GetMemberIdByEnrollment(conflictingEnrollId);
                            if (memberId.HasValue)
                            {
                                var conflictingMember = _dbHelper.GetMember(memberId.Value);
                                if (conflictingMember != null)
                                {
                                    reason += $" | Conflicts with: {conflictingMember.Name} (Member Enrollment: {conflictingEnrollId})";
                                }
                                else
                                {
                                    reason += $" | Conflicts with: Unknown Member (Enrollment: {conflictingEnrollId})";
                                }
                            }
                            else
                            {
                                // Try OfflineMember table
                                var offlineMembers = _dbHelper.GetOfflineMembers();
                                var conflictingOffline = offlineMembers?.FirstOrDefault(o => o.EnrollmentID == conflictingEnrollId);
                                if (conflictingOffline != null)
                                {
                                    reason += $" | Conflicts with: {conflictingOffline.Name} (Offline Member Enrollment: {conflictingEnrollId})";
                                }
                                else
                                {
                                    // Enrollment not found in any table
                                    reason += $" | Conflicts with: Unknown (Enrollment: {conflictingEnrollId})";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"[ErrorDetail] Error looking up conflicting enrollment {conflictingEnrollId}: {ex.Message}");
                            reason += $" | Conflicts with: Unknown (Enrollment: {conflictingEnrollId})";
                        }
                    }

                    // Clean up
                    WebsocketServer.LastCommandErrors.TryRemove(errorKey, out _);

                    return reason;
                }

                return $"{biometricType} sync failed";
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[ErrorDetail] Failed to build detailed error reason: {ex.Message}");
                return $"{biometricType} sync failed";
            }
        }

        public async Task<List<Member>> CheckMissingFingerprints()
        {
            try
            {
                LogHelper.Log("Checking for members with missing fingerprints...");
                List<Member> missingFingerprintMembers = await _dbHelper.GetMembersWithMissingFingerprintsAsync();

                if (missingFingerprintMembers.Count == 0)
                {
                    LogHelper.Log("No members found with missing fingerprints.");
                }
                else
                {
                    LogHelper.Log($"Found {missingFingerprintMembers.Count} member(s) with missing fingerprints:");
                    foreach (var member in missingFingerprintMembers)
                    {
                        LogHelper.Log($"MemberID: {member.MemberID}, Name: {member.Name}, EnrollmentID: {member.EnrollmentID}, RFID: {member.RFID}");
                    }
                }

                return missingFingerprintMembers;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error during missing fingerprints check: {ex.Message}");
                throw; // Re-throw to stop the workflow
            }

        }


        public async Task ProcessAllEntries()
        {
            Dashboard.GlobalStartBiometricLoader("Processing unsynced biometric entries...");

            try
            {
                List<LogEntry> unsyncedEntries;
                List<Member> members;
                List<Device> devices;
                List<FingerprintData> allFingerprints;

                // Step 1: read snapshot under short lock - Members
                unsyncedEntries = await _dbHelper.GetUnsyncedLogEntriesAsync();
                if (unsyncedEntries.Count == 0)
                {
                    LogHelper.Log("No unsynced member entries found");
                }

                members = _dbHelper.GetMembers();
                devices = _dbHelper.LoadDevices();
                allFingerprints = _dbHelper.GetAllFingerprints();

                // Step 2: process member entries without holding DB lock
                int processed = 0;
                int total = unsyncedEntries.Count;

                foreach (var entry in unsyncedEntries)
                {
                    try
                    {
                        var member = members.FirstOrDefault(m => m.MemberID == entry.MemberID);
                        var device = devices.FirstOrDefault(d => d.DeviceId == entry.DeviceID);

                        if (member == null || device == null) continue;

                        // Skip if attempt count is >= 5
                        if (entry.EnrollmentID <= 0) continue;
                        var logEntry = _dbHelper.GetLatestLogForMember(entry.EnrollmentID);
                        if (logEntry != null && (int?)logEntry.AttemptCount >= 5)
                        {
                            LogHelper.Log($"Skipping {member.Name} - attempt count is {logEntry.AttemptCount} (>= 5). Will retry when count drops below 5.");
                            processed++;
                            continue;
                        }

                        LogHelper.Log($"Processing {member.Name} on {device.DeviceIP}");
                        Dashboard.GlobalUpdateBiometricLoader($"Processing {member.Name} on {device.DeviceId} ({processed + 1}/{total})");

                        await SetUsername(member.EnrollmentID ?? 0, member.Name, device.DeviceSerial);
                        await Task.Delay(200);

                        var processResults = new Dictionary<string, bool>();

                        foreach (var biometric in entry.BiometricData)
                        {
                            if (entry.SyncStatus.ContainsKey(biometric.Key) && entry.SyncStatus[biometric.Key]) continue;

                            bool success = false;

                            switch (biometric.Key)
                            {
                                case "Fingerprint":
                                    success = await ProcessFingerprint(entry.EnrollmentID, member.Name, device.DeviceSerial, allFingerprints);
                                    await Task.Delay(200);
                                    break;
                                case "Face":
                                    success = await ProcessFace(biometric.Value, member.EnrollmentID ?? 0, member.Name, device.DeviceSerial);
                                    await Task.Delay(200);
                                    break;
                                case "RFID":
                                    success = await ProcessRFID(biometric.Value, member.EnrollmentID ?? 0, member.Name, device.DeviceSerial);
                                    await Task.Delay(200);
                                    break;
                            }

                            if (success)
                            {
                                // Step 3: brief write under lock
                                await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessAllEntries_Write", DbTaskPriority.High);
                                try
                                {
                                    await _dbHelper.UpdateBiometricSyncStatus(entry.Id, biometric.Key, true);
                                    await _dbHelper.CheckAndUpdateMemberDeviceSyncStatus(entry.MemberID, entry.DeviceID);
                                }
                                finally
                                {
                                    DatabaseQueueManager.ReleaseDbWriteLock("ProcessAllEntries_Write");
                                }
                            }
                            else
                            {
                                // Log failure with detailed reason
                                string errorReason = await BuildDetailedErrorReason(biometric.Key, device.DeviceSerial, entry.EnrollmentID);
                                // Preserve any existing meaningful reason if new one is just a generic timeout fallback
                                if (IsGenericTimeoutReason(errorReason))
                                {
                                    string existing = _dbHelper.GetLogFailureReasonById(entry.Id, "Log");
                                    if (!string.IsNullOrWhiteSpace(existing) && !IsGenericTimeoutReason(existing))
                                        errorReason = existing;
                                }
                                await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessAllEntries_Write", DbTaskPriority.High);
                                try
                                {
                                    await _dbHelper.UpdateBiometricSyncStatus(entry.Id, biometric.Key, false, errorReason, attemptCount: 1);
                                }
                                finally
                                {
                                    DatabaseQueueManager.ReleaseDbWriteLock("ProcessAllEntries_Write");
                                }
                            }

                            processResults[biometric.Key] = success;
                        }

                        processed++;
                        Dashboard.GlobalUpdateBiometricLoader($"Processed {processed}/{total} entries...");
                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Error processing entry {entry.Id}: {ex.Message}");
                        Dashboard.GlobalUpdateBiometricLoader($"Error on entry {entry.Id}: {ex.Message}");
                    }
                }

                // ---------- Staff processing ----------
                Dashboard.GlobalUpdateBiometricLoader("Processing unsynced staff biometric entries...");

                var unsyncedStaffEntries = await _dbHelper.GetUnsyncedStaffLogEntriesAsync();
                if (unsyncedStaffEntries.Count == 0)
                {
                    LogHelper.Log("[Staff] No unsynced staff entries found");
                    Dashboard.GlobalUpdateBiometricLoader("No unsynced staff entries found.");
                    return;
                }

                var staffs = _dbHelper.GetStaffs();
                var staffDevices = _dbHelper.LoadStaffDevices();

                int staffProcessed = 0;
                int staffTotal = unsyncedStaffEntries.Count;

                foreach (var sEntry in unsyncedStaffEntries)
                {
                    try
                    {
                        var staff = staffs.FirstOrDefault(s => s.StaffID == sEntry.StaffID);
                        var device = staffDevices.FirstOrDefault(d => d.StaffDeviceId == sEntry.StaffDeviceId);

                        if (staff == null || device == null) continue;

                        string staffName = staff.Name ?? sEntry.StaffID;

                        // Skip if attempt count is >= 5
                        var staffLogEntry = _dbHelper.GetLatestStaffLogForEnrollment(sEntry.EnrollmentID ?? 0);
                        if (staffLogEntry != null && (int?)staffLogEntry.AttemptCount >= 5)
                        {
                            LogHelper.Log($"[Staff] Skipping {staffName} - attempt count is {staffLogEntry.AttemptCount} (>= 5). Will retry when count drops below 5.");
                            staffProcessed++;
                            continue;
                        }

                        LogHelper.Log($"[Staff] Processing {staffName} on {device.DeviceIP}");
                        Dashboard.GlobalUpdateBiometricLoader($"[Staff] Processing {staffName} on {device.StaffDeviceId} ({staffProcessed + 1}/{staffTotal})");

                        await SetUsername(sEntry.EnrollmentID ?? 0, staffName, device.DeviceSerial);
                        await Task.Delay(200);

                        foreach (var biometric in sEntry.BiometricData)
                        {
                            if (sEntry.SyncStatus.ContainsKey(biometric.Key) && sEntry.SyncStatus[biometric.Key]) continue;

                            bool success = false;
                            int staffEnrollId = sEntry.EnrollmentID ?? 0;

                            switch (biometric.Key)
                            {
                                case "Fingerprint":
                                    success = await ProcessFingerprint(staffEnrollId, staffName, device.DeviceSerial, allFingerprints);
                                    await Task.Delay(200);
                                    break;
                                case "Face":
                                    success = await ProcessFace(biometric.Value, staffEnrollId, staffName, device.DeviceSerial);
                                    await Task.Delay(200);
                                    break;
                                case "RFID":
                                    success = await ProcessRFID(biometric.Value, staffEnrollId, staffName, device.DeviceSerial);
                                    await Task.Delay(200);
                                    break;
                            }

                            if (success)
                            {
                                // Step 3: brief write under lock
                                await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessAllEntries_Write", DbTaskPriority.High);
                                try
                                {
                                    await _dbHelper.UpdateStaffBiometricSyncStatusAsync(sEntry.Id, biometric.Key, true);
                                    await _dbHelper.CheckAndUpdateStaffDeviceSyncStatus(sEntry.StaffID, sEntry.StaffDeviceId);
                                }
                                finally
                                {
                                    DatabaseQueueManager.ReleaseDbWriteLock("ProcessAllEntries_Write");
                                }
                            }
                            else
                            {
                                // Log failure with detailed reason
                                string errorReason = await BuildDetailedErrorReason(biometric.Key, device.DeviceSerial, staffEnrollId);
                                // Preserve any existing meaningful reason if new one is just a generic timeout fallback
                                if (IsGenericTimeoutReason(errorReason))
                                {
                                    string existing = _dbHelper.GetLogFailureReasonById(sEntry.Id, "StaffLog");
                                    if (!string.IsNullOrWhiteSpace(existing) && !IsGenericTimeoutReason(existing))
                                        errorReason = existing;
                                }
                                await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessAllEntries_Write", DbTaskPriority.High);
                                try
                                {
                                    await _dbHelper.UpdateStaffBiometricSyncStatusAsync(sEntry.Id, biometric.Key, false, errorReason, attemptCount: 1);
                                }
                                finally
                                {
                                    DatabaseQueueManager.ReleaseDbWriteLock("ProcessAllEntries_Write");
                                }
                            }
                        }

                        staffProcessed++;
                        Dashboard.GlobalUpdateBiometricLoader($"[Staff] Processed {staffProcessed}/{staffTotal} entries...");
                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[Staff] Error processing staff entry {sEntry.Id}: {ex.Message}");
                        Dashboard.GlobalUpdateBiometricLoader($"[Staff] Error on entry {sEntry.Id}: {ex.Message}");
                    }
                }

                Dashboard.GlobalUpdateBiometricLoader("Biometric processing completed.");
            }
            finally
            {
                Dashboard.GlobalStopBiometricLoader();
            }
        }

        // Same as ProcessAllEntries but for offline (locally-enrolled) members only —
        // pushes their biometrics (RFID/Face/Fingerprint) to every member device.
        public async Task ProcessAllOfflineEntries()
        {
            LogHelper.Log("[Offline] ProcessAllOfflineEntries started");
            Dashboard.GlobalStartBiometricLoader("Processing unsynced offline member biometric entries...");

            try
            {
                List<OfflineLogEntry> unsyncedEntries;
                List<OfflineMember> offlineMembers;
                List<Device> devices;
                List<FingerprintData> allFingerprints;

                // Step 1: read snapshot under short lock - Offline Members
                unsyncedEntries = await _dbHelper.GetUnsyncedOfflineLogEntriesAsync();
                LogHelper.Log($"[Offline] Found {unsyncedEntries.Count} unsynced OfflineMemberLog entries");
                if (unsyncedEntries.Count == 0)
                {
                    LogHelper.Log("[Offline] No unsynced offline member entries found");
                    Dashboard.GlobalUpdateBiometricLoader("No unsynced offline member entries found.");
                    return;
                }

                offlineMembers = _dbHelper.GetOfflineMembers();
                devices = _dbHelper.LoadDevices();
                allFingerprints = _dbHelper.GetAllFingerprints();
                LogHelper.Log($"[Offline] Loaded {offlineMembers.Count} offline members, {devices.Count} devices, {allFingerprints.Count} fingerprints");

                // Step 2: process offline member entries without holding DB lock
                int processed = 0;
                int total = unsyncedEntries.Count;

                foreach (var entry in unsyncedEntries)
                {
                    try
                    {
                        var offlineMember = offlineMembers.FirstOrDefault(m => m.ClientId == entry.ClientId);
                        var device = devices.FirstOrDefault(d => d.DeviceId == entry.DeviceID);

                        if (offlineMember == null)
                        {
                            LogHelper.Log($"[Offline] Skipping log entry {entry.Id}: no OfflineMember found for ClientId={entry.ClientId}");
                            continue;
                        }
                        if (device == null)
                        {
                            LogHelper.Log($"[Offline] Skipping log entry {entry.Id}: no Device found for DeviceID={entry.DeviceID}");
                            continue;
                        }

                        // Skip if attempt count is >= 5
                        var offlineLogEntry = _dbHelper.GetLatestOfflineLogForMember(entry.ClientId);
                        if (offlineLogEntry != null && (int?)offlineLogEntry.AttemptCount >= 5)
                        {
                            LogHelper.Log($"[Offline] Skipping {offlineMember.Name} - attempt count is {offlineLogEntry.AttemptCount} (>= 5). Will retry when count drops below 5.");
                            processed++;
                            continue;
                        }

                        LogHelper.Log($"[Offline] Processing {offlineMember.Name} (Enroll={offlineMember.EnrollmentID}) on {device.DeviceIP} (DeviceId={device.DeviceId})");
                        Dashboard.GlobalUpdateBiometricLoader($"[Offline] Processing {offlineMember.Name} on {device.DeviceId} ({processed + 1}/{total})");

                        await SetUsername(offlineMember.EnrollmentID, offlineMember.Name, device.DeviceSerial);
                        await Task.Delay(200);

                        var processResults = new Dictionary<string, bool>();

                        foreach (var biometric in entry.BiometricData)
                        {
                            if (entry.SyncStatus.ContainsKey(biometric.Key) && entry.SyncStatus[biometric.Key])
                            {
                                LogHelper.Log($"[Offline] {biometric.Key} already synced for {offlineMember.Name} on DeviceId={device.DeviceId}; skipping");
                                continue;
                            }

                            LogHelper.Log($"[Offline] Pushing {biometric.Key} for {offlineMember.Name} (Enroll={offlineMember.EnrollmentID}) to DeviceId={device.DeviceId}");

                            bool success = false;

                            switch (biometric.Key)
                            {
                                case "Fingerprint":
                                    success = await ProcessFingerprint(offlineMember.EnrollmentID, offlineMember.Name, device.DeviceSerial, allFingerprints);
                                    await Task.Delay(200);
                                    break;
                                case "Face":
                                    success = await ProcessFace(biometric.Value, offlineMember.EnrollmentID, offlineMember.Name, device.DeviceSerial);
                                    await Task.Delay(200);
                                    break;
                                case "RFID":
                                    success = await ProcessRFID(biometric.Value, offlineMember.EnrollmentID, offlineMember.Name, device.DeviceSerial);
                                    await Task.Delay(200);
                                    break;
                                default:
                                    LogHelper.Log($"[Offline] Unknown biometric type '{biometric.Key}' for {offlineMember.Name}; skipping");
                                    break;
                            }

                            LogHelper.Log($"[Offline] {biometric.Key} for {offlineMember.Name} on DeviceId={device.DeviceId}: {(success ? "SUCCESS" : "FAILED")}");

                            if (success)
                            {
                                // Step 3: brief write under lock
                                await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessAllOfflineEntries_Write", DbTaskPriority.High);
                                try
                                {
                                    await _dbHelper.UpdateOfflineBiometricSyncStatus(entry.Id, biometric.Key, true);
                                    LogHelper.Log($"[Offline] Marked {biometric.Key} synced for log entry {entry.Id}");

                                    bool memberDeviceFullySynced = await _dbHelper.CheckAndUpdateOfflineMemberDeviceSyncStatus(entry.ClientId, entry.DeviceID);
                                    if (memberDeviceFullySynced)
                                        LogHelper.Log($"[Offline] {offlineMember.Name} is now fully synced for DeviceId={device.DeviceId}");
                                }
                                finally
                                {
                                    DatabaseQueueManager.ReleaseDbWriteLock("ProcessAllOfflineEntries_Write");
                                }
                            }
                            else
                            {
                                // Log failure with detailed reason
                                string errorReason = await BuildDetailedErrorReason(biometric.Key, device.DeviceSerial, offlineMember.EnrollmentID);
                                // Preserve any existing meaningful reason if new one is just a generic timeout fallback
                                if (IsGenericTimeoutReason(errorReason))
                                {
                                    string existing = _dbHelper.GetLogFailureReasonById(entry.Id, "OfflineMemberLog");
                                    if (!string.IsNullOrWhiteSpace(existing) && !IsGenericTimeoutReason(existing))
                                        errorReason = existing;
                                }
                                await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessAllOfflineEntries_Write", DbTaskPriority.High);
                                try
                                {
                                    await _dbHelper.UpdateOfflineBiometricSyncStatus(entry.Id, biometric.Key, false, errorReason, attemptCount: 1);
                                }
                                finally
                                {
                                    DatabaseQueueManager.ReleaseDbWriteLock("ProcessAllOfflineEntries_Write");
                                }
                            }

                            processResults[biometric.Key] = success;
                        }

                        processed++;
                        LogHelper.Log($"[Offline] Finished entry {entry.Id} for {offlineMember.Name} on DeviceId={device.DeviceId} ({processed}/{total})");
                        Dashboard.GlobalUpdateBiometricLoader($"[Offline] Processed {processed}/{total} entries...");
                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[Offline] Error processing entry {entry.Id}: {ex.Message}");
                        Dashboard.GlobalUpdateBiometricLoader($"[Offline] Error on entry {entry.Id}: {ex.Message}");
                    }
                }

                LogHelper.Log($"[Offline] ProcessAllOfflineEntries completed. Processed {processed}/{total} entries.");
                Dashboard.GlobalUpdateBiometricLoader("Offline member biometric processing completed.");
            }
            finally
            {
                Dashboard.GlobalStopBiometricLoader();
                LogHelper.Log("[Offline] ProcessAllOfflineEntries finished");
            }
        }

        private async Task SetUsername(int enrollId, string name, string deviceSerial)
        {
            var nameParams = new { enrollid = enrollId, name };
            bool nameSet = await WebsocketServer.ExecuteServiceCommand(deviceSerial, "setusername", JsonConvert.SerializeObject(nameParams));
            if (!nameSet)
                LogHelper.Log($"setusername failed via WebSocket for {name} (Enrollment: {enrollId}) on {deviceSerial}");
        }

        private async Task<bool> ProcessFingerprint(int enrollId, string name, string deviceSerial, List<FingerprintData> allFingerprints)
        {
            var fingerprint = allFingerprints.FirstOrDefault(f => f.EnrollNumber == enrollId);
            if (fingerprint == null)
            {
                LogHelper.Log($"Fingerprint data not found for {name} (Enrollment: {enrollId})");
                return false;
            }

            var fpParams = new
            {
                enrollid = enrollId,
                name,
                record = fingerprint.TemplateData,
                backupnum = fingerprint.FingerNumber
            };

            bool success = await WebsocketServer.ExecuteServiceCommand(deviceSerial, "setfingerprint", JsonConvert.SerializeObject(fpParams));

            if (!success)
                LogHelper.Log($"Failed to send fingerprint command via WebSocket for {name} on {deviceSerial}");

            return success;
        }

        private async Task<bool> ProcessFace(string faceImagePath, int enrollId, string name, string deviceSerial)
        {
            var faceParams = new
            {
                enrollid = enrollId,
                name,
                record = faceImagePath
            };

            bool success = await WebsocketServer.ExecuteServiceCommand(deviceSerial, "setface", JsonConvert.SerializeObject(faceParams));

            if (!success)
                LogHelper.Log($"Failed to send face command via WebSocket for {name} on {deviceSerial}");

            return success;
        }

        private async Task<bool> ProcessRFID(string rfidCode, int enrollId, string name, string deviceSerial)
        {
            if (string.IsNullOrWhiteSpace(rfidCode))
            {
                LogHelper.Log($"Empty RFID code for {name}");
                return false;
            }

            var rfidParams = new
            {
                enrollid = enrollId,
                name,
                record = rfidCode
            };

            bool success = await WebsocketServer.ExecuteServiceCommand(deviceSerial, "setrfid", JsonConvert.SerializeObject(rfidParams));

            if (!success)
                LogHelper.Log($"Failed to send RFID command via WebSocket for {name} on {deviceSerial}");

            return success;
        }

        public async Task VerifyAndUpdateBiometricLogsAsync(int memberId)
        {
            try
            {
                LogHelper.Log($"Starting biometric log verification and update process for MemberId={memberId}...");

                // Verify/update biometrics for ONLY this member
                var changes = await _dbHelper.VerifyAndUpdateLogBiometricsAsync(memberId);

                foreach (var change in changes)
                {
                    var deviceDetails = _dbHelper.GetDeviceDetails(change.DeviceId);
                    if (deviceDetails.ip == null) continue; // skip if device not found

                    // 1. WebSocket communication uses the Device Serial (SN)
                    string sn = CommonUtils.GetDeviceSnByIp(deviceDetails.ip);
                    if (string.IsNullOrEmpty(sn))
                    {
                        LogHelper.Log($"[WS-ERR] Could not find SN for IP {deviceDetails.ip}. Skipping delete.");
                        continue;
                    }

                    int backupNumber = 0;
                    switch (change.BiometricType)
                    {
                        case "RFID":
                            backupNumber = 11;
                            break;
                        case "Fingerprint":
                            backupNumber = 0; // 0-9 for fp
                            break;
                        case "Face":
                            backupNumber = 50; // photo/face
                            break;
                        default:
                            backupNumber = 12; // all data fallback
                            break;
                    }

                    // 2. Call Delete via WebSocket for this change
                    if (change.EnrollmentId.HasValue)
                    {
                        try
                        {
                            // Prepare JSON for deleteuser command
                            var deleteParams = new
                            {
                                enrollid = change.EnrollmentId.Value,
                                backupnum = backupNumber
                            };

                            bool success = await WebsocketServer.ExecuteServiceCommand(
                                sn,
                                "deleteuser",
                                JsonConvert.SerializeObject(deleteParams)
                            );

                            LogHelper.Log($"WebSocket Delete: Member {memberId}, Device {sn}, EnrollmentId {change.EnrollmentId}, Type {change.BiometricType}, Dispatched: {success}");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"WebSocket Delete failed for Member {memberId}, Device {sn}: {ex.Message}");
                        }
                    }
                }

                LogHelper.Log($"Biometric log verification and update process completed for MemberId={memberId}.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task VerifyAndUpdateStaffBiometricLogsAsync(string staffId)
        {
            try
            {
                LogHelper.Log($"Starting biometric log verification and update process for StaffId={staffId}...");

                var changes = await _dbHelper.VerifyAndUpdateStaffLogBiometricsAsync(staffId);

                foreach (var change in changes)
                {
                    var deviceDetails = _dbHelper.GetStaffDeviceDetails(change.DeviceId);
                    if (deviceDetails.ip == null) continue;

                    // WebSocket needs the Serial Number (SN)
                    string sn = CommonUtils.GetDeviceSnByIp(deviceDetails.ip);
                    if (string.IsNullOrEmpty(sn))
                    {
                        LogHelper.Log($"[Staff-WS-ERR] No SN found for {deviceDetails.ip}");
                        continue;
                    }

                    int backupNumber = 0;
                    switch (change.BiometricType)
                    {
                        case "RFID": backupNumber = 11; break;
                        case "Fingerprint": backupNumber = 0; break;
                        case "Face": backupNumber = 50; break;
                        default: backupNumber = 12; break;
                    }

                    if (change.EnrollmentId.HasValue)
                    {
                        try
                        {
                            var deleteParams = new { enrollid = change.EnrollmentId.Value, backupnum = backupNumber };
                            bool success = await WebsocketServer.ExecuteServiceCommand(sn, "deleteuser", JsonConvert.SerializeObject(deleteParams));
                            LogHelper.Log($"[Staff-WS] Delete: Staff {staffId}, SN {sn}, Type {change.BiometricType}, Dispatched: {success}");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"[Staff-WS] Delete failed for Staff {staffId}, SN {sn}: {ex.Message}");
                        }
                    }
                }
                LogHelper.Log($"Biometric log verification and update process completed for StaffId={staffId}.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task EmptyEnrollmentDataAndDeleteLogsByDeviceId(int deviceId)
        {
            try
            {
                var deviceDetails = _dbHelper.GetDeviceDetails(deviceId);
                string sn = CommonUtils.GetDeviceSnByIp(deviceDetails.ip);

                if (string.IsNullOrEmpty(sn))
                {
                    MessageBox.Show("Device Serial Number not found in database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Protocol command 9: Clean all users
                bool result = await WebsocketServer.ExecuteServiceCommand(sn, "cleanuser", null);

                if (result)
                {
                    await DatabaseQueueManager.EnterDbWriteLockAsync("EmptyEnrollmentData_DeleteLogs", DbTaskPriority.High);
                    try { _dbHelper.DeleteLogsByDeviceId(deviceId); }
                    finally { DatabaseQueueManager.ReleaseDbWriteLock("EmptyEnrollmentData_DeleteLogs"); }

                    MessageBox.Show("All enrollment data and logs for this device have been cleared via WebSocket.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to dispatch clean command to the device via WebSocket.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId(int deviceId)
        {
            try
            {
                var staffDeviceDetails = _dbHelper.GetStaffDeviceDetails(deviceId);
                string sn = CommonUtils.GetDeviceSnByIp(staffDeviceDetails.ip);

                if (string.IsNullOrEmpty(sn))
                {
                    MessageBox.Show("Staff Device Serial Number not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Protocol command 9: Clean all users
                bool result = await WebsocketServer.ExecuteServiceCommand(sn, "cleanuser", null);

                if (result)
                {
                    await DatabaseQueueManager.EnterDbWriteLockAsync("EmptyStaffEnrollmentData_DeleteLogs", DbTaskPriority.High);
                    try { _dbHelper.DeleteStaffGLogsByStaffDeviceId(deviceId); }
                    finally { DatabaseQueueManager.ReleaseDbWriteLock("EmptyStaffEnrollmentData_DeleteLogs"); }

                    LogHelper.Log($"[Staff-WS] Dispatched cleanuser command to staff device {sn}");
                    MessageBox.Show("All enrollment data and staff logs for this device have been cleared via WebSocket.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to dispatch clean command to the staff device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId error: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public async Task ProcessUserWorkflowAsync()
        {
            if (_workflowRunning)
            {
                // Don't drop this request on the floor — a concurrent trigger may carry
                // work (e.g. a brand-new fingerprint) that the in-flight run already
                // passed over. Flag a rerun so it executes right after the current one finishes.
                LogHelper.Log("⚠️ ProcessUserWorkflowAsync already running. Queuing a rerun after it completes.");
                aiChatLogger.AddInfoMessage("Workflow already in progress. Queued for rerun.");
                _workflowRerunRequested = true;
                return;
            }

            _workflowRunning = true;
            try
            {
                aiChatLogger.AddInfoMessage("Starting user processing workflow...");

                bool hasMemberDevices = AppSession.DeviceList != null && AppSession.DeviceList.Count > 0;
                bool hasStaffDevices = AppSession.StaffDeviceList != null && AppSession.StaffDeviceList.Count > 0;

                // Step 0a: Members - Create logs ONLY for active members with biometrics and no Log rows yet
                if (hasMemberDevices)
                {
                    var memberIds = _dbHelper.GetActiveMemberIdsWithBiometricsAndNoLogs();
                    if (memberIds != null && memberIds.Count > 0)
                    {
                        aiChatLogger.AddInfoMessage($"Found {memberIds.Count} members needing log creation.");
                        foreach (var memberId in memberIds)
                        {
                            aiChatLogger.AddSyncMessage($"Processing enrollment logs for MemberID {memberId}...");
                            await ProcessLogCreation(memberId);
                        }
                        aiChatLogger.AddSuccessMessage("Member log processing completed");
                    }
                }
                else
                {
                    LogHelper.Log("⚠️ No member devices in session. Skipping member log creation.");
                }

                // Step 0b: Staff - Create logs ONLY for active staff with biometrics and no StaffLog rows yet
                if (hasStaffDevices)
                {
                    var staffIds = _dbHelper.GetActiveStaffIdsWithBiometricsAndNoLogs();
                    if (staffIds != null && staffIds.Count > 0)
                    {
                        aiChatLogger.AddInfoMessage($"Found {staffIds.Count} staff needing log creation.");
                        foreach (var staffId in staffIds)
                        {
                            aiChatLogger.AddSyncMessage($"Processing enrollment logs for StaffID {staffId}...");
                            await ProcessStaffLogCreation(staffId);
                        }
                        aiChatLogger.AddSuccessMessage("Staff log processing completed");
                    }
                }
                else
                {
                    LogHelper.Log("⚠️ No staff devices in session. Skipping staff log creation.");
                }

                // Step 2: Process all member entries (unsynced biometrics) — requires at least one device type
                if (hasMemberDevices || hasStaffDevices)
                {
                    aiChatLogger.AddSyncMessage("Enrolling biometric data to devices...");
                    await ProcessAllEntries();
                    aiChatLogger.AddSuccessMessage("Biometric enrollment completed");

                    // Refresh cache so UI reflects updated SyncStatus values written during enrollment
                    AppSession.AllMembers = _dbHelper.GetMembers() ?? new List<Member>();
                    AppSession.AllStaff = _dbHelper.GetStaffs() ?? new List<Staff>();
                }
                else
                {
                    LogHelper.Log("⚠️ No devices available in session. Skipping biometric enrollment.");
                }

                aiChatLogger.AddSuccessMessage("User processing workflow completed successfully");
            }
            catch (Exception ex)
            {
                aiChatLogger.AddErrorMessage($"User processing failed: {ex.Message}");
                LogHelper.Log($"❌ Error in user processing: {ex.Message}");
                MessageBox.Show($"Error processing users: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            finally
            {
                _workflowRunning = false;  // ← always reset, even on exception
                if (ServiceManager.PeriodicMaintenanceService != null)
                {
                    LogHelper.Log("Triggering post-workflow device sync status update...");
                    await ServiceManager.PeriodicMaintenanceService.ExecuteFetchDeviceSyncStatusAsync();
                }
            }

            // A trigger arrived while we were running and was queued instead of skipped.
            // Run it now so any work it carried (e.g. a newly added fingerprint) isn't lost.
            if (_workflowRerunRequested)
            {
                _workflowRerunRequested = false;
                LogHelper.Log("🔁 Running queued ProcessUserWorkflowAsync rerun...");
                await ProcessUserWorkflowAsync();
            }
        }

        public async Task DisableInactiveUsers()
        {
            Dictionary<int, (bool shouldDisable, List<(int machineNo, string ip, int port, string password, int enrollmentNumber, int backupNumber)> deviceInfo)> membersToProcess;

            // Step 1: read snapshot under short lock
            try
            {
                LogHelper.Log("=== Starting Inactive User Disable Process (Reading Stage) ===");
                membersToProcess = _dbHelper.GetMembersAccessStatus();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error reading members access status: {ex.Message}");
                return;
            }

            if (membersToProcess == null || membersToProcess.Count == 0)
            {
                LogHelper.Log("No members found to process for disabling.");
                return;
            }

            LogHelper.Log("=== Processing Inactive Users (Network Stage) ===");
            int totalProcessed = 0;
            int successCount = 0;
            int failureCount = 0;

            // Step 2: process members and perform network calls WITHOUT holding the DB lock
            foreach (var memberEntry in membersToProcess)
            {
                int memberId = memberEntry.Key;
                var (shouldDisable, deviceInfo) = memberEntry.Value;

                if (!shouldDisable)
                    continue;

                LogHelper.Log($"Processing member {memberId} (should disable)");
                if (deviceInfo == null || deviceInfo.Count == 0)
                {
                    LogHelper.Log($"No device info available for member {memberId}; cannot perform disables.");
                    continue;
                }

                LogHelper.Log($"Found {deviceInfo.Count} devices for this member");
                int memberSuccessCount = 0;

                foreach (var device in deviceInfo)
                {
                    try
                    {
                        // WebSocket communication uses the Device Serial (SN) instead of IP
                        string sn = CommonUtils.GetDeviceSnByIp(device.ip);
                        LogHelper.Log($"Attempting to disable on device {sn} via WebSocket");

                        // Prepare JSON for Disable (enflag: 0)
                        var disableParams = new
                        {
                            enrollid = device.enrollmentNumber,
                            enflag = 0
                        };

                        bool result = await WebsocketServer.ExecuteServiceCommand(
                            sn,
                            "enableuser",
                            JsonConvert.SerializeObject(disableParams)
                        );

                        if (result)
                        {
                            LogHelper.Log($"Successfully dispatched disable command for member {memberId} to device {sn}");
                            successCount++;
                            memberSuccessCount++;
                        }
                        else
                        {
                            LogHelper.Log($"Failed to reach device {sn} via WebSocket - enqueuing for retry");
                            failureCount++;
                            await EnqueuePendingJob(new PendingJob
                            {
                                Action = "disable",
                                MemberId = memberId,
                                EnrollmentId = device.enrollmentNumber,
                                DeviceId = _dbHelper.GetDeviceIdByMachineNo(device.machineNo),
                                Ip = device.ip,
                                Port = device.port,
                                MachineNo = device.machineNo,
                                Password = device.password,
                                BackupNumber = device.backupNumber,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Error disabling member {memberId} on device {device.machineNo}: {ex.Message} - enqueuing for retry");
                        failureCount++;
                        await EnqueuePendingJob(new PendingJob
                        {
                            Action = "disable",
                            MemberId = memberId,
                            EnrollmentId = device.enrollmentNumber,
                            DeviceId = _dbHelper.GetDeviceIdByMachineNo(device.machineNo),
                            Ip = device.ip,
                            Port = device.port,
                            MachineNo = device.machineNo,
                            Password = device.password,
                            BackupNumber = device.backupNumber,
                            CreatedAt = DateTime.Now
                        });
                    }
                    finally
                    {
                        totalProcessed++;
                    }
                }

                // Step 3: brief DB write under lock
                try
                {
                    await DatabaseQueueManager.EnterDbWriteLockAsync("DisableInactiveUsers_Write", DbTaskPriority.High);
                    try
                    {
                        bool statusUpdated = _dbHelper.UpdateMemberStatus(memberId, "Disabled");
                        if (statusUpdated)
                            LogHelper.Log($"Updated member {memberId} status to 'Disabled' in DB.");
                        else
                            LogHelper.Log($"Failed to update member {memberId} status to 'Disabled' in DB.");
                    }
                    finally
                    {
                        DatabaseQueueManager.ReleaseDbWriteLock("DisableInactiveUsers_Write");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error updating DB status for member {memberId}: {ex.Message}");
                }
            }

            LogHelper.Log($"=== Disable Process Complete ===");
            LogHelper.Log($"Total attempts: {totalProcessed}");
            LogHelper.Log($"Successful disables: {successCount}");
            LogHelper.Log($"Failed disables (enqueued for retry): {failureCount}");
        }

        private static JObject EnsureMembersArray(JObject data)
        {
            if (data == null) return null;

            var members = data["members"];

            // Case 1: already an array
            if (members is JArray) return data;

            // Case 2: "members" is a single object -> wrap as array
            if (members is JObject membersObj)
            {
                data["members"] = new JArray(membersObj);
                return data;
            }

            // Case 3: no "members" property -> assume root is the member
            // Clone first to avoid self-reference and remove any existing "members" just in case
            var singleMember = (JObject)data.DeepClone();
            singleMember.Remove("members");

            return new JObject
            {
                ["members"] = new JArray(singleMember)
            };
        }


        private async Task<bool> ExecuteWithRetryAsync(Func<Task<bool>> operation, string description, int retries = 3, int delayMs = 5000)
        {
            for (int attempt = 1; attempt <= retries; attempt++)
            {
                try
                {
                    var ok = await operation();
                    if (ok) return true;
                    LogHelper.Log($"⚠️ Attempt {attempt}/{retries} failed for {description}. Retrying in {delayMs / 1000}s...");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Attempt {attempt}/{retries} threw for {description}: {ex.Message}. Retrying in {delayMs / 1000}s...");
                }
                if (attempt < retries) await Task.Delay(delayMs);
            }
            return false;
        }

        private async Task EnqueuePendingJob(PendingJob job)
        {
            await DatabaseQueueManager.EnterDbWriteLockAsync("EnqueuePendingJob", DbTaskPriority.High);
            try
            {
                _dbHelper.InsertPendingJob(job); // Insert into database
            }
            finally
            {
                DatabaseQueueManager.ReleaseDbWriteLock("EnqueuePendingJob");
            }
            LogHelper.Log($"📄 Enqueued pending job: {job.Action} mId={job.MemberId} enId={job.EnrollmentId} dev={job.MachineNo}@{job.Ip}");
        }


        public async Task ProcessPendingJobsAsync()
        {
            try
            {
                LogHelper.Log("🔄 Starting pending jobs processing over WebSocket...");

                while (true)
                {
                    PendingJob job = null;

                    // Retrieve a single pending job under global lock for DB read.
                    lock (_pendingJobLock)
                    {
                        var jobs = _dbHelper.GetAllPendingJobs();
                        if (jobs == null || jobs.Count == 0)
                        {
                            job = null;
                        }
                        else
                        {
                            job = jobs.First(); // pick the oldest
                        }
                    }

                    if (job == null)
                    {
                        LogHelper.Log("ℹ️ No pending jobs found in database.");
                        break;
                    }

                    // 1. WebSocket communication requires the Device Serial (SN)
                    string sn = CommonUtils.GetDeviceSnByIp(job.Ip);
                    string desc = $"{job.Action} mId={job.MemberId} enId={job.EnrollmentId} dev={sn}";

                    if (string.IsNullOrEmpty(sn))
                    {
                        LogHelper.Log($"⚠️ Skipping job: No Serial Number found for DeviceId {job.DeviceId}");
                        // Optional: Delete invalid job here to prevent infinite skips
                        break;
                    }

                    bool success = false;

                    try
                    {
                        switch (job.Action.ToLower())
                        {
                            case "enable":
                                success = await ExecuteWithRetryAsync(
                                    () => WebsocketServer.ExecuteServiceCommand(sn, "enableuser",
                                        JsonConvert.SerializeObject(new { enrollid = job.EnrollmentId, enflag = 1 })),
                                    desc);
                                break;
                            case "disable":
                                success = await ExecuteWithRetryAsync(
                                    () => WebsocketServer.ExecuteServiceCommand(sn, "enableuser",
                                        JsonConvert.SerializeObject(new { enrollid = job.EnrollmentId, enflag = 0 })),
                                    desc);
                                break;
                            case "delete":
                                success = await ExecuteWithRetryAsync(
                                    () => WebsocketServer.ExecuteServiceCommand(sn, "deleteuser",
                                        JsonConvert.SerializeObject(new { enrollid = job.EnrollmentId, backupnum = job.BackupNumber })),
                                    desc);
                                break;
                            default:
                                LogHelper.Log($"⚠️ Unknown pending job action '{job.Action}'. Skipping.");
                                success = true;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Pending job execution error for {desc}: {ex.Message}");
                        success = false;
                    }

                    if (success)
                    {
                        try
                        {
                            await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessPendingJobs_Delete", DbTaskPriority.High);
                            try
                            {
                                lock (_pendingJobLock)
                                {
                                    _dbHelper.DeletePendingJob(job);
                                }
                                LogHelper.Log($"✅ Pending job succeeded and removed: {desc}");
                            }
                            finally
                            {
                                DatabaseQueueManager.ReleaseDbWriteLock("ProcessPendingJobs_Delete");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"❌ Failed to delete pending job after success ({desc}): {ex.Message}");
                            break;
                        }
                        continue;
                    }
                    else
                    {
                        LogHelper.Log($"⏳ Pending job failed to dispatch to {sn} and will remain in DB: {desc}");
                        await Task.Delay(1000); // Wait longer on failure to allow connection recovery
                        continue;
                    }
                }

                LogHelper.Log("🧹 Pending jobs processing completed.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Fatal error in ProcessPendingJobsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task StatusSync(JObject data)
        {
            try
            {
                await Task.Yield();
                data = EnsureMembersArray(data);

                var sessionDevices = AppSession.DeviceList;
                if (sessionDevices == null || sessionDevices.Count == 0)
                {
                    LogHelper.Log("ℹ️ No devices found in session - processing logs only.");
                    // (Minimal logging for no-device case remains same as your original)
                    return;
                }

                var membersArray = data["members"] as JArray;
                if (membersArray == null)
                {
                    LogHelper.Log("❌ 'members' array not found in response.");
                    return;
                }

                LogHelper.Log($"📦 Received {membersArray.Count} member record(s) in response.");

                foreach (var memberToken in membersArray)
                {
                    try
                    {
                        int memberId = memberToken["id"] != null ? (int)memberToken["id"] : 0;
                        string cloudStatus = memberToken["status"]?.ToString();
                        if (memberId == 0) continue;

                        Member existingMember = _dbHelper.GetMember(memberId);
                        if (existingMember == null) continue;

                        string localStatus = existingMember.Status ?? "N/A";
                        int? enrollmentId = existingMember.EnrollmentID;
                        if (!enrollmentId.HasValue) continue;

                        var dbDevices = _dbHelper.GetAvailableDeviceDetails(memberId);
                        if (dbDevices == null || dbDevices.Count == 0) continue;

                        var isLocalActive = localStatus.Equals("Active", StringComparison.OrdinalIgnoreCase);
                        var isCloudActive = cloudStatus != null && cloudStatus.Equals("Active", StringComparison.OrdinalIgnoreCase);
                        var isCloudOverdue = cloudStatus != null && (cloudStatus.Equals("Overdue", StringComparison.OrdinalIgnoreCase) || cloudStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase));
                        var isCloudExpired = cloudStatus != null && (cloudStatus.Equals("Expired", StringComparison.OrdinalIgnoreCase) || cloudStatus.Equals("Deleted", StringComparison.OrdinalIgnoreCase));

                        foreach (var d in dbDevices)
                        {
                            // Resolve Serial Number for WebSocket routing
                            string sn = CommonUtils.GetDeviceSnByIp(d.ip);
                            if (string.IsNullOrEmpty(sn))
                            {
                                LogHelper.Log($"[WS-ERR] No SN found for IP {d.ip}. Skipping action for {memberId}.");
                                continue;
                            }

                            bool ok = false;
                            string desc = "";

                            if (isCloudActive && !isLocalActive)
                            {
                                desc = $"Enable {memberId} on {sn}";
                                ok = await ExecuteWithRetryAsync(() =>
                                    WebsocketServer.ExecuteServiceCommand(sn, "enableuser",
                                    JsonConvert.SerializeObject(new { enrollid = enrollmentId.Value, enflag = 1 })), desc);

                                if (!ok) await EnqueuePendingJob(new PendingJob { Action = "enable", MemberId = memberId, EnrollmentId = enrollmentId.Value, DeviceId = _dbHelper.GetDeviceIdByMachineNo(d.machineNo), Ip = d.ip, Port = d.port, MachineNo = d.machineNo, Password = d.password, BackupNumber = 0, CreatedAt = DateTime.Now });
                            }
                            else if (isCloudOverdue && isLocalActive)
                            {
                                desc = $"Disable {memberId} on {sn}";
                                ok = await ExecuteWithRetryAsync(() =>
                                    WebsocketServer.ExecuteServiceCommand(sn, "enableuser",
                                    JsonConvert.SerializeObject(new { enrollid = enrollmentId.Value, enflag = 0 })), desc);

                                if (!ok) await EnqueuePendingJob(new PendingJob { Action = "disable", MemberId = memberId, EnrollmentId = enrollmentId.Value, DeviceId = _dbHelper.GetDeviceIdByMachineNo(d.machineNo), Ip = d.ip, Port = d.port, MachineNo = d.machineNo, Password = d.password, BackupNumber = d.backupNumber, CreatedAt = DateTime.Now });
                            }
                            else if (isCloudExpired && isLocalActive)
                            {
                                desc = $"Delete {memberId} on {sn}";
                                ok = await ExecuteWithRetryAsync(() =>
                                    WebsocketServer.ExecuteServiceCommand(sn, "deleteuser",
                                    JsonConvert.SerializeObject(new { enrollid = enrollmentId.Value, backupnum = 12 })), desc);

                                if (!ok) await EnqueuePendingJob(new PendingJob { Action = "delete", MemberId = memberId, EnrollmentId = enrollmentId.Value, DeviceId = _dbHelper.GetDeviceIdByMachineNo(d.machineNo), Ip = d.ip, Port = d.port, MachineNo = d.machineNo, Password = d.password, BackupNumber = 12, CreatedAt = DateTime.Now });
                            }
                        }

                        // Clean up local DB records for terminated users
                        if (isCloudExpired && isLocalActive)
                        {
                            try
                            {
                                _dbHelper.DeleteMemberLogs(memberId);
                                if (cloudStatus.Equals("Expired", StringComparison.OrdinalIgnoreCase)) _dbHelper.DeleteMemberById(memberId);
                                LogHelper.Log($"🧹 Cleaned local DB records for member {memberId}.");
                            }
                            catch (Exception ex) { LogHelper.Log($"❌ Local DB cleanup error: {ex.Message}"); }
                        }
                    }
                    catch (Exception ex) { LogHelper.Log($"⚠️ Error processing member entry: {ex.Message}"); }
                }
                LogHelper.Log("🧹 StatusSync process completed.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Fatal error in StatusSync: {ex.Message}");
            }
        }
    }
}
