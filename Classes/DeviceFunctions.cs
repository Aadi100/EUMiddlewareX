using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace smart_attendance_middleware.Classes
{
    public class DeviceFunctions
    {
        public DatabaseHelper _dbHelper;
        private WebSocketManager _wsManager;
        private Processor _processor;


        private static readonly object _pendingJobLock = new object();
   

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


        //public async Task ProcessAllEntries()
        //{
        //    // Start the dashboard loader with initial text
        //    Dashboard.GlobalStartLoader("Processing unsynced biometric entries...");

        //    try
        //    {
        //        List<LogEntry> unsyncedEntries;
        //        List<Member> members;
        //        List<Device> devices;
        //        List<FingerprintData> allFingerprints;

        //        // Step 1: read snapshot under short lock
        //        unsyncedEntries = await _dbHelper.GetUnsyncedLogEntriesAsync();
        //        if (unsyncedEntries.Count == 0)
        //        {
        //            LogHelper.Log("No unsynced entries found");
        //            Dashboard.GlobalUpdateLoaderMessage("No unsynced entries found.");
        //            return;
        //        }

        //        members = _dbHelper.GetMembers();
        //        devices = _dbHelper.LoadDevices();
        //        allFingerprints = _dbHelper.GetAllFingerprints();

        //        // Step 2: process entries without holding DB lock
        //        int processed = 0;
        //        int total = unsyncedEntries.Count;

        //        foreach (var entry in unsyncedEntries)
        //        {
        //            try
        //            {
        //                var member = members.FirstOrDefault(m => m.MemberID == entry.MemberID);
        //                var device = devices.FirstOrDefault(d => d.DeviceId == entry.DeviceID);

        //                if (member == null || device == null) continue;

        //                LogHelper.Log($"Processing {member.Name} on {device.DeviceIP}");
        //                Dashboard.GlobalUpdateLoaderMessage($"Processing {member.Name} on {device.DeviceId} ({processed + 1}/{total})");

        //                var processResults = new Dictionary<string, bool>();

        //                foreach (var biometric in entry.BiometricData)
        //                {
        //                    if (entry.SyncStatus.ContainsKey(biometric.Key) && entry.SyncStatus[biometric.Key]) continue;

        //                    bool success = false;
        //                    string authType = biometric.Key.ToLower();

        //                    switch (biometric.Key)
        //                    {
        //                        case "Fingerprint":
        //                            success = await ProcessFingerprint(entry.EnrollmentID, member, device, allFingerprints);
        //                            await Task.Delay(1000);
        //                            break;
        //                        case "Face":
        //                            success = await ProcessFace(biometric.Value, member, device);
        //                            await Task.Delay(1000);
        //                            break;
        //                        case "RFID":
        //                            success = await ProcessRFID(biometric.Value, member, device);
        //                            await Task.Delay(1000);
        //                            break;
        //                    }

        //                    if (success)
        //                    {
        //                        // Step 3: brief write under lock
        //                        await DatabaseQueueManager.EnterDbWriteLockAsync("ProcessAllEntries_Write", DbTaskPriority.High);
        //                        try
        //                        {
        //                            await _dbHelper.UpdateBiometricSyncStatus(entry.Id, biometric.Key, true);
        //                            await _dbHelper.CheckAndUpdateMemberDeviceSyncStatus(entry.MemberID, entry.DeviceID);
        //                        }
        //                        finally
        //                        {
        //                            DatabaseQueueManager.ReleaseDbWriteLock("ProcessAllEntries_Write");
        //                        }
        //                    }

        //                    processResults[biometric.Key] = success;
        //                }

        //                processed++;
        //                Dashboard.GlobalUpdateLoaderMessage($"Processed {processed}/{total} entries...");
        //                await Task.Delay(2000);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogHelper.Log($"Error processing entry {entry.Id}: {ex.Message}");
        //                Dashboard.GlobalUpdateLoaderMessage($"Error on entry {entry.Id}: {ex.Message}");
        //            }
        //        }

        //        Dashboard.GlobalUpdateLoaderMessage("Biometric processing completed.");
        //    }
        //    finally
        //    {
        //        // Always stop loader
        //        Dashboard.GlobalStopLoader();
        //    }
        //}

        public async Task ProcessAllEntries()
        {
            // Start the dashboard loader with initial text
            Dashboard.GlobalStartLoader("Processing unsynced biometric entries...");

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

                        LogHelper.Log($"Processing {member.Name} on {device.DeviceIP}");
                        Dashboard.GlobalUpdateLoaderMessage($"Processing {member.Name} on {device.DeviceId} ({processed + 1}/{total})");

                        var processResults = new Dictionary<string, bool>();

                        foreach (var biometric in entry.BiometricData)
                        {
                            if (entry.SyncStatus.ContainsKey(biometric.Key) && entry.SyncStatus[biometric.Key]) continue;

                            bool success = false;

                            switch (biometric.Key)
                            {
                                case "Fingerprint":
                                    success = await ProcessFingerprint(entry.EnrollmentID, member, device, allFingerprints);
                                    await Task.Delay(1000);
                                    break;
                                case "Face":
                                    success = await ProcessFace(biometric.Value, member, device);
                                    await Task.Delay(1000);
                                    break;
                                case "RFID":
                                    success = await ProcessRFID(biometric.Value, member, device);
                                    await Task.Delay(1000);
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

                            processResults[biometric.Key] = success;
                        }

                        processed++;
                        Dashboard.GlobalUpdateLoaderMessage($"Processed {processed}/{total} entries...");
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Error processing entry {entry.Id}: {ex.Message}");
                        Dashboard.GlobalUpdateLoaderMessage($"Error on entry {entry.Id}: {ex.Message}");
                    }
                }

                // ---------- Staff processing ----------
                Dashboard.GlobalUpdateLoaderMessage("Processing unsynced staff biometric entries...");

                var unsyncedStaffEntries = await _dbHelper.GetUnsyncedStaffLogEntriesAsync();
                if (unsyncedStaffEntries.Count == 0)
                {
                    LogHelper.Log("[Staff] No unsynced staff entries found");
                    Dashboard.GlobalUpdateLoaderMessage("No unsynced staff entries found.");
                    return;
                }

                var staffs = _dbHelper.GetStaffs();                // Staff list (for names)
                var staffDevices = _dbHelper.LoadStaffDevices();   // Staff devices snapshot

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

                        LogHelper.Log($"[Staff] Processing {staffName} on {device.DeviceIP}");
                        Dashboard.GlobalUpdateLoaderMessage($"[Staff] Processing {staffName} on {device.StaffDeviceId} ({staffProcessed + 1}/{staffTotal})");

                        foreach (var biometric in sEntry.BiometricData)
                        {
                            if (sEntry.SyncStatus.ContainsKey(biometric.Key) && sEntry.SyncStatus[biometric.Key]) continue;

                            bool success = false;

                            switch (biometric.Key)
                            {
                                case "Fingerprint":
                                    {
                                        var fingerprint = allFingerprints.FirstOrDefault(f => f.EnrollNumber == (sEntry.EnrollmentID ?? 0));
                                        if (fingerprint == null)
                                        {
                                            LogHelper.Log($"[Staff] Fingerprint data not found for {staffName} (Enrollment: {sEntry.EnrollmentID ?? 0})");
                                            break;
                                        }

                                        success = await DeviceService.AddFingerprint(
                                            device.DeviceIP,
                                            device.Port,
                                            device.MachineNo,
                                            device.Password,
                                            fingerprint.EnrollNumber,
                                            fingerprint.FingerNumber,
                                            fingerprint.Privilege,
                                            fingerprint.TemplateData);

                                        if (success)
                                        {
                                            bool nameSet = await DeviceService.SetUserName(
                                                device.DeviceIP,
                                                device.Port,
                                                device.MachineNo,
                                                device.Password,
                                                fingerprint.EnrollNumber,
                                                staffName);

                                            if (!nameSet)
                                                LogHelper.Log($"[Staff] Fingerprint enrolled but name not set for {staffName}");
                                        }
                                        await Task.Delay(1000);
                                        break;
                                    }
                                case "Face":
                                    {
                                        // biometric.Value is path
                                        success = await DeviceService.AddFace(
                                            device.DeviceIP,
                                            device.Port,
                                            device.MachineNo,
                                            device.Password,
                                            sEntry.EnrollmentID ?? 0,
                                            biometric.Value);

                                        if (success)
                                        {
                                            bool nameSet = await DeviceService.SetUserName(
                                                device.DeviceIP,
                                                device.Port,
                                                device.MachineNo,
                                                device.Password,
                                                sEntry.EnrollmentID ?? 0,
                                                staffName);

                                            if (!nameSet)
                                                LogHelper.Log($"[Staff] Face enrolled but name not set for {staffName}");
                                        }
                                        await Task.Delay(1000);
                                        break;
                                    }
                                case "RFID":
                                    {
                                        if (!long.TryParse(biometric.Value, out long rfidNumber))
                                        {
                                            LogHelper.Log($"[Staff] Invalid RFID format for {staffName} (RFID: {biometric.Value})");
                                            break;
                                        }

                                        success = await DeviceService.AddRfidCard(
                                            device.DeviceIP,
                                            device.Port,
                                            device.MachineNo,
                                            device.Password,
                                            sEntry.EnrollmentID ?? 0,
                                            rfidNumber,
                                            1);

                                        if (success)
                                        {
                                            bool nameSet = await DeviceService.SetUserName(
                                                device.DeviceIP,
                                                device.Port,
                                                device.MachineNo,
                                                device.Password,
                                                sEntry.EnrollmentID ?? 0,
                                                staffName);

                                            if (!nameSet)
                                                LogHelper.Log($"[Staff] RFID enrolled but name not set for {staffName}");
                                        }
                                        await Task.Delay(1000);
                                        break;
                                    }
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
                        }

                        staffProcessed++;
                        Dashboard.GlobalUpdateLoaderMessage($"[Staff] Processed {staffProcessed}/{staffTotal} entries...");
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[Staff] Error processing staff entry {sEntry.Id}: {ex.Message}");
                        Dashboard.GlobalUpdateLoaderMessage($"[Staff] Error on entry {sEntry.Id}: {ex.Message}");
                    }
                }

                Dashboard.GlobalUpdateLoaderMessage("Biometric processing completed.");
            }
            finally
            {
                // Always stop loader
                Dashboard.GlobalStopLoader();
            }
        }

        private async Task<bool> ProcessFingerprint(int enrollNumber, Member member, Device device, List<FingerprintData> allFingerprints)
        {
            var fingerprint = allFingerprints.FirstOrDefault(f => f.EnrollNumber == enrollNumber);
            if (fingerprint == null)
            {
                LogHelper.Log($"Fingerprint data not found for {member.Name} (Enrollment: {enrollNumber})");
                return false;
            }

            bool success = await DeviceService.AddFingerprint(
                device.DeviceIP,
                device.Port,
                device.MachineNo,
                device.Password,
                fingerprint.EnrollNumber,
                fingerprint.FingerNumber,
                fingerprint.Privilege,
                fingerprint.TemplateData);

            if (success)
            {
                bool nameSet = await DeviceService.SetUserName(
                    device.DeviceIP,
                    device.Port,
                    device.MachineNo,
                    device.Password,
                    enrollNumber,
                    member.Name);

                if (!nameSet)
                {
                    LogHelper.Log($"Fingerprint enrolled but name not set for {member.Name}");
                }
            }
            else
            {
                LogHelper.Log($"Failed to enroll fingerprint for {member.Name}");
            }

            return success;
        }

        private async Task<bool> ProcessFace(string faceImagePath, Member member, Device device)
        {
            bool success = await DeviceService.AddFace(
                device.DeviceIP,
                device.Port,
                device.MachineNo,
                device.Password,
                member.EnrollmentID ?? 0, // Use member's enrollment ID or default
                faceImagePath);

            if (success)
            {
                bool nameSet = await DeviceService.SetUserName(
                    device.DeviceIP,
                    device.Port,
                    device.MachineNo,
                    device.Password,
                    member.EnrollmentID ?? 0,
                    member.Name);

                if (!nameSet)
                {
                    LogHelper.Log($"Face enrolled but name not set for {member.Name}");
                }
            }
            else
            {
                LogHelper.Log($"Failed to enroll face for {member.Name}");
            }

            return success;
        }

        private async Task<bool> ProcessRFID(string rfidCode, Member member, Device device)
        {
            if (!long.TryParse(rfidCode, out long rfidNumber))
            {
                LogHelper.Log($"Invalid RFID format for {member.Name} (RFID: {rfidCode})");
                return false;
            }

            bool success = await DeviceService.AddRfidCard(
                device.DeviceIP,
                device.Port,
                device.MachineNo,
                device.Password,
                member.EnrollmentID ?? 0,
                rfidNumber,
                1); // Privilege level

            if (success)
            {
                bool nameSet = await DeviceService.SetUserName(
                    device.DeviceIP,
                    device.Port,
                    device.MachineNo,
                    device.Password,
                    member.EnrollmentID ?? 0,
                    member.Name);

                if (!nameSet)
                {
                    LogHelper.Log($"RFID enrolled but name not set for {member.Name}");
                }
            }
            else
            {
                LogHelper.Log($"Failed to enroll RFID for {member.Name}");
            }

            return success;
        }


        private async Task VerifyAndUpdateBiometricLogsAsync(int memberId)
        {
            try
            {
                LogHelper.Log($"Starting biometric log verification and update process for MemberId={memberId}...");

                // Verify/update biometrics for ONLY this member
                var changes = await _dbHelper.VerifyAndUpdateLogBiometricsAsync(memberId);

                var deviceArray = new System.Collections.Generic.List<object>();

                foreach (var change in changes)
                {
                    var deviceDetails = _dbHelper.GetDeviceDetails(change.DeviceId);
                    if (deviceDetails.ip == null) continue; // skip if device not found

                    // Always use password "0"
                    string password = "0";

                    int backupNumber = 0;
                    switch (change.BiometricType)
                    {
                        case "RFID":
                            backupNumber = 11;
                            break;
                        case "Fingerprint":
                            backupNumber = 0;
                            break;
                        case "Face":
                            backupNumber = 50;
                            break;
                    }

                    deviceArray.Add(new
                    {
                        enrollmentNumber = change.EnrollmentId,
                        ip = deviceDetails.ip,
                        machineNo = deviceDetails.machineNo,
                        port = deviceDetails.port,
                        backupNumber = backupNumber,
                        biometricType = change.BiometricType
                    });

                    // Call DeleteEnrollmentData for this change
                    if (change.EnrollmentId.HasValue)
                    {
                        try
                        {
                            bool deleted = await DeviceService.DeleteEnrollmentData(
                                deviceDetails.ip,
                                deviceDetails.port,
                                deviceDetails.machineNo,
                                password, // always "0"
                                change.EnrollmentId.Value,
                                deviceDetails.machineNo, // enrollmentMachineId, adjust if needed
                                backupNumber
                            );

                            LogHelper.Log($"DeleteEnrollmentData: Member {memberId}, Device {change.DeviceId}, EnrollmentId {change.EnrollmentId}, Type {change.BiometricType}, Result: {deleted}");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"DeleteEnrollmentData failed for Member {memberId}, Device {change.DeviceId}, EnrollmentId {change.EnrollmentId}, Type {change.BiometricType}: {ex.Message}");
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

                var deviceArray = new System.Collections.Generic.List<object>();

                foreach (var change in changes)
                {
                    var deviceDetails = _dbHelper.GetStaffDeviceDetails(change.DeviceId);
                    if (deviceDetails.ip == null) continue;

                    string password = "0";

                    int backupNumber = 0;
                    switch (change.BiometricType)
                    {
                        case "RFID": backupNumber = 11; break;
                        case "Fingerprint": backupNumber = 0; break;
                        case "Face": backupNumber = 50; break;
                    }

                    deviceArray.Add(new
                    {
                        enrollmentNumber = change.EnrollmentId,
                        ip = deviceDetails.ip,
                        machineNo = deviceDetails.machineNo,
                        port = deviceDetails.port,
                        backupNumber = backupNumber,
                        biometricType = change.BiometricType
                    });

                    if (change.EnrollmentId.HasValue)
                    {
                        try
                        {
                            bool deleted = await DeviceService.DeleteEnrollmentData(
                                deviceDetails.ip,
                                deviceDetails.port,
                                deviceDetails.machineNo,
                                password,
                                change.EnrollmentId.Value,
                                deviceDetails.machineNo,
                                backupNumber
                            );

                            LogHelper.Log($"[Staff] DeleteEnrollmentData: Staff {staffId}, StaffDevice {change.DeviceId}, EnrollmentId {change.EnrollmentId}, Type {change.BiometricType}, Result: {deleted}");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"[Staff] DeleteEnrollmentData failed for Staff {staffId}, StaffDevice {change.DeviceId}, EnrollmentId {change.EnrollmentId}, Type {change.BiometricType}: {ex.Message}");
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
                if (string.IsNullOrEmpty(deviceDetails.ip) || deviceDetails.machineNo == 0 || deviceDetails.port == 0)
                {
                    MessageBox.Show("Device details not found or incomplete.", "Device Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string password = "0";

                bool result = await DeviceService.EmptyEnrollmentData(deviceDetails.ip, deviceDetails.port, deviceDetails.machineNo, password);

                if (result)
                {
                    await DatabaseQueueManager.EnterDbWriteLockAsync("EmptyEnrollmentData_DeleteLogs", DbTaskPriority.High);
                    try
                    {
                        _dbHelper.DeleteLogsByDeviceId(deviceId);
                    }
                    finally
                    {
                        DatabaseQueueManager.ReleaseDbWriteLock("EmptyEnrollmentData_DeleteLogs");
                    }

                    MessageBox.Show("All enrollment data and logs for this device have been cleared.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to clear enrollment data from the device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (string.IsNullOrEmpty(staffDeviceDetails.ip) || staffDeviceDetails.machineNo == 0 || staffDeviceDetails.port == 0)
                {
                    MessageBox.Show("Staff device details not found or incomplete.", "Device Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string password = "0";

                bool result = await DeviceService.EmptyEnrollmentData(staffDeviceDetails.ip, staffDeviceDetails.port, staffDeviceDetails.machineNo, password);

                if (result)
                {
                    await DatabaseQueueManager.EnterDbWriteLockAsync("EmptyStaffEnrollmentData_DeleteLogs", DbTaskPriority.High);
                    try
                    {
                        _dbHelper.DeleteStaffGLogsByStaffDeviceId(deviceId);
                    }
                    finally
                    {
                        DatabaseQueueManager.ReleaseDbWriteLock("EmptyStaffEnrollmentData_DeleteLogs");
                    }

                    LogHelper.Log($"[Staff] Cleared enrollment data and staff GLogs for staff device {deviceId} (machineNo={staffDeviceDetails.machineNo})");
                    MessageBox.Show("All enrollment data and staff logs for this device have been cleared.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to clear enrollment data from the staff device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId error: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //public async Task ProcessUserWorkflowAsync()
        //{
        //    try
        //    {
        //        aiChatLogger.AddInfoMessage("Starting user processing workflow...");

        //        // Step 0: Create logs ONLY for active members with biometrics that have no Log rows yet
        //        //aiChatLogger.AddSyncMessage("Scanning for active members with biometrics and no logs...");
        //        var memberIds = _dbHelper.GetActiveMemberIdsWithBiometricsAndNoLogs();

        //        if (memberIds != null && memberIds.Count > 0)
        //        {
        //            aiChatLogger.AddInfoMessage($"Found {memberIds.Count} members needing log creation.");

        //            foreach (var memberId in memberIds)
        //            {
        //                aiChatLogger.AddSyncMessage($"Processing enrollment logs for MemberID {memberId}...");
        //                await ProcessLogCreation(memberId);
        //            }

        //            aiChatLogger.AddSuccessMessage("Log processing completed");
        //        }
        //        else
        //        {
        //            //aiChatLogger.AddSuccessMessage("No members found requiring log creation.");
        //        }

        //        // Step 2: Process all entries
        //        aiChatLogger.AddSyncMessage("Enrolling biometric data to devices...");
        //        await ProcessAllEntries();
        //        aiChatLogger.AddSuccessMessage("Biometric enrollment completed");
        //        aiChatLogger.AddSuccessMessage("User processing workflow completed successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        aiChatLogger.AddErrorMessage($"User processing failed: {ex.Message}");
        //        LogHelper.Log($"❌ Error in user processing: {ex.Message}");
        //        MessageBox.Show($"Error processing users: {ex.Message}", "Error",
        //                      MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        throw;
        //    }
        //    finally
        //    {
        //        // After the workflow completes, trigger the device sync status update.
        //        if (ServiceManager.PeriodicMaintenanceService != null)
        //        {
        //            LogHelper.Log("Triggering post-workflow device sync status update...");
        //            await ServiceManager.PeriodicMaintenanceService.ExecuteFetchDeviceSyncStatusAsync();
        //        }
        //    }
        //}

        public async Task ProcessUserWorkflowAsync()
        {
            try
            {
                aiChatLogger.AddInfoMessage("Starting user processing workflow...");

                // Step 0a: Members - Create logs ONLY for active members with biometrics and no Log rows yet
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

                // Step 0b: Staff - Create logs ONLY for active staff with biometrics and no StaffLog rows yet
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

                // Step 2: Process all member entries (unsynced biometrics)
                aiChatLogger.AddSyncMessage("Enrolling biometric data to devices...");
                await ProcessAllEntries();
                aiChatLogger.AddSuccessMessage("Biometric enrollment completed");
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
                if (ServiceManager.PeriodicMaintenanceService != null)
                {
                    LogHelper.Log("Triggering post-workflow device sync status update...");
                    await ServiceManager.PeriodicMaintenanceService.ExecuteFetchDeviceSyncStatusAsync();
                }
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
                        LogHelper.Log($"Attempting to disable on device {device.machineNo} (IP: {device.ip})");

                        bool result = await DeviceService.DisableUser(
                            device.ip,
                            device.port,
                            device.machineNo,
                            device.password,
                            device.enrollmentNumber,
                            0, // enrollmentMachineId
                            device.backupNumber);

                        if (result)
                        {
                            LogHelper.Log($"Successfully disabled member {memberId} on device {device.machineNo}");
                            successCount++;
                            memberSuccessCount++;
                        }
                        else
                        {
                            LogHelper.Log($"Failed to disable member {memberId} on device {device.machineNo} - enqueuing for retry");
                            failureCount++;
                            EnqueuePendingJob(new PendingJob
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
                        EnqueuePendingJob(new PendingJob
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
                LogHelper.Log("🔄 Starting pending jobs processing (one-by-one)...");

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
                            job = jobs.First(); // pick the oldest (DB returns ordered by CreatedAt)
                        }
                    }

                    if (job == null)
                    {
                        LogHelper.Log("ℹ️ No pending jobs found in database.");
                        break;
                    }

                    string desc = $"{job.Action} mId={job.MemberId} enId={job.EnrollmentId} dev={job.MachineNo}@{job.Ip}";
                    bool success = false;

                    try
                    {
                        switch (job.Action)
                        {
                            case "enable":
                                success = await ExecuteWithRetryAsync(
                                    () => DeviceService.EnableUser(job.Ip, job.Port, job.MachineNo, job.Password, job.EnrollmentId, 0, 0),
                                    desc);
                                break;
                            case "disable":
                                success = await ExecuteWithRetryAsync(
                                    () => DeviceService.DisableUser(job.Ip, job.Port, job.MachineNo, job.Password, job.EnrollmentId, 0, job.BackupNumber),
                                    desc);
                                break;
                            case "delete":
                                success = await ExecuteWithRetryAsync(
                                    () => DeviceService.DeleteEnrollmentData(job.Ip, job.Port, job.MachineNo, job.Password, job.EnrollmentId, job.EnrollmentId, job.BackupNumber),
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
                            // Use DB queue only for the brief write (deleting the job)
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
                            // If deletion failed, stop to avoid tight retry; next run will attempt again.
                            break;
                        }

                        // Continue to next job (immediately)
                        continue;
                    }
                    else
                    {
                        LogHelper.Log($"⏳ Pending job failed and will remain in DB for retry later: {desc}");
                        // Move on to next job to avoid getting stuck on a single failing job.
                        await Task.Delay(500);
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

                // 1) Session devices only for logging
                var sessionDevices = AppSession.DeviceList;
                var deviceInfo = new StringBuilder();
                deviceInfo.AppendLine("Devices in current session:");
                deviceInfo.AppendLine();

                if (sessionDevices == null || sessionDevices.Count == 0)
                {
                    LogHelper.Log("ℹ️ No devices found in the current app session.");

                    // Build minimal members info for logs only
                    var memberInfoNoDevices = new StringBuilder();
                    memberInfoNoDevices.AppendLine("Members from response:");
                    memberInfoNoDevices.AppendLine();

                    if (data == null)
                    {
                        LogHelper.Log("❌ Data payload is null.");
                        return;
                    }

                    var membersArrayNoDevices = data["members"] as JArray;
                    if (membersArrayNoDevices == null)
                    {
                        LogHelper.Log("❌ 'members' array not found in response.");
                        memberInfoNoDevices.AppendLine("No member data found in response.");
                    }
                    else
                    {
                        LogHelper.Log($"📦 Received {membersArrayNoDevices.Count} member record(s) in response.");
                        foreach (var memberToken in membersArrayNoDevices)
                        {
                            try
                            {
                                int memberId = memberToken["id"] != null ? (int)memberToken["id"] : 0;
                                string cloudStatus = memberToken["status"] != null ? (string)memberToken["status"] : null;

                                if (memberId == 0)
                                {
                                    LogHelper.Log("⚠️ Member entry missing valid 'id'. Skipping.");
                                    continue;
                                }

                                // READ-ONLY: no global DB lock
                                Member existingMember = _dbHelper.GetMember(memberId);

                                string localStatus = existingMember?.Status ?? "N/A";
                                string localEnrollmentStr = existingMember?.EnrollmentID.HasValue == true
                                    ? existingMember.EnrollmentID.Value.ToString()
                                    : "N/A";

                                memberInfoNoDevices.AppendLine($"MemberID: {memberId}, Cloud: {cloudStatus ?? "N/A"}, Local: {localStatus}, EnrollmentID(Local): {localEnrollmentStr}");
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Log($"⚠️ Error processing member entry (no devices case): {ex.Message}");
                            }
                        }
                    }

                    LogHelper.Log(memberInfoNoDevices.ToString());
                    return;
                }

                // Build devices info (for logs only)
                foreach (var sd in sessionDevices)
                {
                    string ip = sd?.DeviceIP ?? "N/A";
                    int machineNo = sd?.MachineNo ?? 0;
                    string location = sd?.Location ?? "N/A";
                    deviceInfo.AppendLine($"IP: {ip}, Machine No: {machineNo}, Location: {location}");
                }

                // 2) Process members and perform actions
                var memberInfo = new StringBuilder();
                memberInfo.AppendLine("Members from response:");
                memberInfo.AppendLine();

                if (data == null)
                {
                    LogHelper.Log("❌ Data payload is null.");
                    return;
                }

                var membersArray = data["members"] as JArray;
                if (membersArray == null)
                {
                    LogHelper.Log("❌ 'members' array not found in response.");
                    memberInfo.AppendLine("No member data found in response.");
                }
                else
                {
                    LogHelper.Log($"📦 Received {membersArray.Count} member record(s) in response.");

                    foreach (var memberToken in membersArray)
                    {
                        try
                        {
                            int memberId = memberToken["id"] != null ? (int)memberToken["id"] : 0;
                            string cloudStatus = memberToken["status"] != null ? (string)memberToken["status"] : null;

                            if (memberId == 0)
                            {
                                LogHelper.Log("⚠️ Member entry missing valid 'id'. Skipping.");
                                continue;
                            }

                            // READ-ONLY: no global DB lock
                            Member existingMember = _dbHelper.GetMember(memberId);

                            if (existingMember == null)
                            {
                                LogHelper.Log($"⚠️ Local member not found for ID {memberId}.");
                                memberInfo.AppendLine($"MemberID: {memberId}, Cloud: {cloudStatus ?? "N/A"}, Local: N/A (not found), EnrollmentID(Local): N/A");
                                continue;
                            }

                            string localStatus = existingMember.Status ?? "N/A";
                            int? enrollmentId = existingMember.EnrollmentID;
                            string localEnrollmentStr = enrollmentId.HasValue ? enrollmentId.Value.ToString() : "N/A";

                            LogHelper.Log($"Member ID: {memberId}, Cloud Status: {cloudStatus ?? "N/A"}, EnrollmentID(Local): {localEnrollmentStr}");
                            LogHelper.Log($"Local Status for Member ID {memberId}: {localStatus}");
                            memberInfo.AppendLine($"MemberID: {memberId}, Cloud: {cloudStatus ?? "N/A"}, Local: {localStatus}, EnrollmentID(Local): {localEnrollmentStr}");

                            // Only proceed with device actions if local enrollmentId exists
                            if (!enrollmentId.HasValue)
                            {
                                LogHelper.Log($"⚠️ Member {memberId} has no local enrollment ID. Skipping device actions.");
                                continue;
                            }

                            // READ-ONLY: load actionable device details without lock
                            var dbDevices = _dbHelper.GetAvailableDeviceDetails(memberId);
                            if (dbDevices == null || dbDevices.Count == 0)
                            {
                                LogHelper.Log($"ℹ️ No available device details from DB for member {memberId}. Skipping device actions.");
                                continue;
                            }

                            // Normalize cloud status
                            var isLocalActive = localStatus.Equals("Active", StringComparison.OrdinalIgnoreCase);
                            var isCloudActive = cloudStatus != null && cloudStatus.Equals("Active", StringComparison.OrdinalIgnoreCase);
                            var isCloudOverdue = cloudStatus != null && cloudStatus.Equals("Overdue", StringComparison.OrdinalIgnoreCase);
                            var isCloudExpired = cloudStatus != null && cloudStatus.Equals("Expired", StringComparison.OrdinalIgnoreCase);
                            var isCloudInactive = cloudStatus != null && cloudStatus.Equals("Inactive", StringComparison.OrdinalIgnoreCase);
                            var isCloudDeleted = cloudStatus != null && cloudStatus.Equals("Deleted", StringComparison.OrdinalIgnoreCase);

                            if (isCloudActive && !isLocalActive)
                            {
                                LogHelper.Log($"Member {memberId}: Cloud=Active, Local={localStatus}. Re-enabling user on all DB devices.");
                                foreach (var d in dbDevices)
                                {
                                    var desc = $"enable member {memberId} (en={enrollmentId.Value}) on device {d.machineNo} ({d.ip})";
                                    bool ok = await ExecuteWithRetryAsync(
                                        () => DeviceService.EnableUser(d.ip, d.port, d.machineNo, d.password, enrollmentId.Value, 0, 0),
                                        desc);

                                    if (ok)
                                    {
                                        LogHelper.Log($"✅ Enabled member {memberId} on device {d.machineNo} (IP: {d.ip}).");
                                    }
                                    else
                                    {
                                        await EnqueuePendingJob(new PendingJob
                                        {
                                            Action = "enable",
                                            MemberId = memberId,
                                            EnrollmentId = enrollmentId.Value,
                                            DeviceId = _dbHelper.GetDeviceIdByMachineNo(d.machineNo),
                                            Ip = d.ip,
                                            Port = d.port,
                                            MachineNo = d.machineNo,
                                            Password = d.password,
                                            BackupNumber = 0,
                                            CreatedAt = DateTime.Now
                                        });
                                    }
                                }
                            }
                            else if ((isCloudOverdue || isCloudInactive) && isLocalActive)
                            {
                                LogHelper.Log($"Member {memberId}: Cloud={cloudStatus}, Local=Active. Disabling user on all DB devices.");
                                foreach (var d in dbDevices)
                                {
                                    var desc = $"disable member {memberId} (en={enrollmentId.Value}) on device {d.machineNo} ({d.ip})";
                                    bool ok = await ExecuteWithRetryAsync(
                                        () => DeviceService.DisableUser(d.ip, d.port, d.machineNo, d.password, enrollmentId.Value, 0, d.backupNumber),
                                        desc);

                                    if (ok)
                                    {
                                        LogHelper.Log($"✅ Disabled member {memberId} on device {d.machineNo} (IP: {d.ip}).");
                                    }
                                    else
                                    {
                                        await EnqueuePendingJob(new PendingJob
                                        {
                                            Action = "disable",
                                            MemberId = memberId,
                                            EnrollmentId = enrollmentId.Value,
                                            DeviceId = _dbHelper.GetDeviceIdByMachineNo(d.machineNo),
                                            Ip = d.ip,
                                            Port = d.port,
                                            MachineNo = d.machineNo,
                                            Password = d.password,
                                            BackupNumber = d.backupNumber,
                                            CreatedAt = DateTime.Now
                                        });
                                    }
                                }
                            }
                            else if (isCloudExpired && isLocalActive)
                            {
                                LogHelper.Log($"Member {memberId}: Cloud={cloudStatus}, Local=Active. Deleting enrollment from all DB devices and removing local logs.");
                                foreach (var d in dbDevices)
                                {
                                    var desc = $"delete member {memberId} (en={enrollmentId.Value}) on device {d.machineNo} ({d.ip})";
                                    bool ok = await ExecuteWithRetryAsync(
                                        () => DeviceService.DeleteEnrollmentData(d.ip, d.port, d.machineNo, d.password, enrollmentId.Value, enrollmentId.Value, 12),
                                        desc);

                                    if (ok)
                                    {
                                        LogHelper.Log($"🗑️ Deleted member {memberId} from device {d.machineNo} (IP: {d.ip}).");
                                    }
                                    else
                                    {
                                        await EnqueuePendingJob(new PendingJob
                                        {
                                            Action = "delete",
                                            MemberId = memberId,
                                            EnrollmentId = enrollmentId.Value,
                                            DeviceId = _dbHelper.GetDeviceIdByMachineNo(d.machineNo),
                                            Ip = d.ip,
                                            Port = d.port,
                                            MachineNo = d.machineNo,
                                            Password = d.password,
                                            BackupNumber = 12,
                                            CreatedAt = DateTime.Now
                                        });
                                    }
                                }

                                // WRITE: call DB directly, no global write queue
                                try
                                {
                                    _dbHelper.DeleteMemberLogs(memberId);
                                    LogHelper.Log($"🧹 Deleted local logs for member {memberId}.");
                                    _dbHelper.DeleteMemberById(memberId);
                                    LogHelper.Log($"🗑️ Deleted local member record for member {memberId}.");
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Log($"❌ Error deleting local logs/member {memberId}: {ex.Message}");
                                }
                            }
                            else if (isCloudDeleted)
                            {
                                LogHelper.Log($"Member {memberId}: Cloud=Deleted. Deleting enrollment from all DB devices and removing local logs.");
                                foreach (var d in dbDevices)
                                {
                                    var desc = $"delete member {memberId} (en={enrollmentId.Value}) on device {d.machineNo} ({d.ip})";
                                    bool ok = await ExecuteWithRetryAsync(
                                        () => DeviceService.DeleteEnrollmentData(d.ip, d.port, d.machineNo, d.password, enrollmentId.Value, enrollmentId.Value, 12),
                                        desc);

                                    if (ok)
                                    {
                                        LogHelper.Log($"🗑️ Deleted member {memberId} from device {d.machineNo} (IP: {d.ip}).");
                                    }
                                    else
                                    {
                                        await EnqueuePendingJob(new PendingJob
                                        {
                                            Action = "delete",
                                            MemberId = memberId,
                                            EnrollmentId = enrollmentId.Value,
                                            DeviceId = _dbHelper.GetDeviceIdByMachineNo(d.machineNo),
                                            Ip = d.ip,
                                            Port = d.port,
                                            MachineNo = d.machineNo,
                                            Password = d.password,
                                            BackupNumber = 12,
                                            CreatedAt = DateTime.Now
                                        });
                                    }
                                }

                                // WRITE: call DB directly, no global write queue
                                try
                                {
                                    _dbHelper.DeleteMemberLogs(memberId);
                                    LogHelper.Log($"🧹 Deleted local logs for member {memberId}.");
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Log($"❌ Error deleting local logs for member {memberId}: {ex.Message}");
                                }
                            }
                            else
                            {
                                LogHelper.Log($"Member {memberId}: No action required for Cloud={cloudStatus ?? "N/A"} vs Local={localStatus}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"⚠️ Error processing member entry: {ex.Message}");
                        }
                    }
                }

                // 3) Log combined info (no MessageBox)
                LogHelper.Log("📋 Session summary:");
                LogHelper.Log(memberInfo.ToString());
                LogHelper.Log(deviceInfo.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Fatal error in StatusSync: {ex.Message}");
                throw;
            }
        }
    }
}
