using smart_attendance_middleware;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class StaffAttendance
{
    private readonly DatabaseHelper _dbHelper;
    private readonly WebSocketManager _wsManager;

    public StaffAttendance(DatabaseHelper dbHelper, WebSocketManager wsManager)
    {
        _dbHelper = dbHelper;
        _wsManager = wsManager;
    }

    /// <summary>
    /// Main function to handle the entire staff attendance sync process.
    /// </summary>
    /// <param name="token">Cancellation token to handle task cancellation.</param>
    public async Task HandleStaffAttendanceSync(CancellationToken token)
    {
        try
        {
            LogHelper.Log("🔄 Starting staff attendance sync process");

            // Step 1: Download logs from staff devices and save them to .txt files
            await DownloadAndSaveStaffLogsAsync(token);

            // Step 2: Insert logs into staffglogs, process them, and clear GLogs from the device
            await ProcessStaffLogsAsync(token);

            LogHelper.Log("✅ Staff attendance sync process completed");
        }
        catch (Exception ex)
        {
            LogHelper.Log($"❌ Error in staff attendance sync process: {ex.Message}");
        }
        
    }

    /// <summary>
    /// Downloads logs from staff devices and saves them to .txt files.
    /// </summary>
    /// <param name="token">Cancellation token to handle task cancellation.</param>
    private async Task DownloadAndSaveStaffLogsAsync(CancellationToken token)
    {
        if (AppSession.StaffDeviceList?.Count > 0)
        {
            var prioritizedDevices = AppSession.StaffDeviceList
                .OrderBy(device => GetDevicePriority(device.StaffAttendanceType))
                .ToList();

            foreach (var device in prioritizedDevices)
            {
                if (token.IsCancellationRequested) break;

                bool isDeviceReachable = await IsDeviceReachableAsync(device.DeviceIP, 3, token);
                if (!isDeviceReachable)
                {
                    LogHelper.Log($"❌ Device {device.StaffDeviceId} ({device.DeviceIP}) is not reachable after multiple attempts. Skipping...");
                    continue;
                }

                bool success = false;
                int retryCount = 0;

                while (!success && retryCount < 3)
                {
                    try
                    {
                        LogHelper.Log($"📥 Attempting to read logs from staff device {device.StaffDeviceId} ({device.DeviceIP}), attempt {retryCount + 1}");

                        await Task.Delay(TimeSpan.FromSeconds(2), token);

                        int recordCount = await DeviceService.ReadAndSaveStaffGLogData(
                            device.DeviceIP,
                            device.Port,
                            device.MachineNo,
                            device.Password
                        );

                        if (recordCount >= 0)
                        {
                            LogHelper.Log($"✅ Successfully read {recordCount} records from staff device {device.StaffDeviceId}");
                            success = true;
                        }
                        else
                        {
                            LogHelper.Log($"❌ Failed to read logs from staff device {device.StaffDeviceId}");
                            retryCount++;
                            if (retryCount < 3)
                            {
                                LogHelper.Log($"Retrying in 5 seconds...");
                                await Task.Delay(TimeSpan.FromSeconds(5), token);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Error reading logs from staff device {device.StaffDeviceId}: {ex.Message}");
                        retryCount++;
                        if (retryCount < 3)
                        {
                            LogHelper.Log($"Retrying in 5 seconds...");
                            await Task.Delay(TimeSpan.FromSeconds(5), token);
                        }
                    }
                }

                if (!success)
                {
                    var result = System.Windows.Forms.MessageBox.Show(
                        $"Failed to download logs from device {device.StaffDeviceId} ({device.DeviceIP}) after 3 attempts. Do you want to retry downloading from the start?",
                        "Download Failed",
                        System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Warning
                    );

                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        retryCount = 0;
                        continue; // Restart the loop
                    }
                    else
                    {
                        LogHelper.Log($"Skipping device {device.StaffDeviceId} ({device.DeviceIP}) due to repeated failures.");
                    }
                }
            }
        }
        else
        {
            LogHelper.Log("No staff devices available for attendance log sync");
        }
    }

    /// <summary>
    /// Processes the saved logs by inserting them into staffglogs, filtering, inserting into staffattendencelog,
    /// clearing GLogs from the device, and deleting the .txt files.
    /// </summary>
    /// <param name="token">Cancellation token to handle task cancellation.</param>
    private async Task ProcessStaffLogsAsync(CancellationToken token)
    {
        string folderPath = AppDomain.CurrentDomain.BaseDirectory;
        string[] staffGlogFiles = Directory.GetFiles(folderPath, "StaffGLogs.*.txt");

        if (staffGlogFiles.Length == 0)
        {
            LogHelper.Log("No staff GLog files found for processing");
            return;
        }

        bool allDevicesProcessedSuccessfully = true;

        foreach (string filePath in staffGlogFiles)
        {
            if (token.IsCancellationRequested) break;

            string fileName = Path.GetFileName(filePath);
            string machineNoStr = fileName.Replace("StaffGLogs.", "").Replace(".txt", "");

            if (!int.TryParse(machineNoStr, out int machineNo))
            {
                LogHelper.Log($"❌ Invalid machine number in filename: {fileName}. Skipping file.");
                allDevicesProcessedSuccessfully = false;
                continue;
            }

            // Retrieve the device object using the machine number
            var device = AppSession.StaffDeviceList.FirstOrDefault(d => d.MachineNo == machineNo);
            if (device == null)
            {
                LogHelper.Log($"❌ Device with MachineNo: {machineNo} not found in StaffDeviceList. Skipping file.");
                allDevicesProcessedSuccessfully = false;
                continue;
            }

            try
            {
                // Explicitly declare the tuple variables
                (int processed, int inserted) = await InsertLogsIntoStaffGLogsAsync(filePath, device, token);
                LogHelper.Log($"📊 Processed {processed} records, inserted {inserted} into staffglogs");

                if (inserted > 0)
                {
                    await InsertFilteredLogsIntoStaffAttendanceLogAsync(machineNo, token);
                }
                else
                {
                    LogHelper.Log($"⚠️ Skipping clearing GLogs and deleting file {fileName} due to failed insertion.");
                    allDevicesProcessedSuccessfully = false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error processing staff GLog file {fileName}: {ex.Message}");
                allDevicesProcessedSuccessfully = false;
            }
        }

        // Only delete files and clear GLogs if all devices were processed successfully
        if (allDevicesProcessedSuccessfully)
        {
            foreach (string filePath in staffGlogFiles)
            {
                string fileName = Path.GetFileName(filePath);
                string machineNoStr = fileName.Replace("StaffGLogs.", "").Replace(".txt", "");

                if (int.TryParse(machineNoStr, out int machineNo))
                {
                    try
                    {
                        File.Delete(filePath);
                        LogHelper.Log($"🗑️ Deleted file: {fileName} after processing");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Error clearing GLogs or deleting file {fileName}: {ex.Message}");
                    }
                }
            }
        }
        else
        {
            LogHelper.Log("⚠️ Not all devices were processed successfully. Retaining .txt files and GLogs.");
        }
    }

    /// <summary>
    /// Inserts logs from a .txt file into the staffglogs table.
    /// </summary>
    /// <param name="filePath">Path to the GLog file.</param>
    /// <param name="machineNo">Machine number of the device.</param>
    /// <param name="token">Cancellation token to handle task cancellation.</param>
    /// <returns>Tuple containing counts of processed and inserted records.</returns>
    private async Task<(int processed, int inserted)> InsertLogsIntoStaffGLogsAsync(string filePath, Device device, CancellationToken token)
    {
        int processed = 0;
        int inserted = 0;

        string fileName = Path.GetFileName(filePath);
        LogHelper.Log($"Processing staff GLog file: {fileName} for device: {device.StaffDeviceId}");

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1)
        {
            LogHelper.Log($"No data records found in {fileName} (only header)");
            return (0, 0);
        }

        var records = new List<(int enrollNumber, DateTime deviceTime)>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (token.IsCancellationRequested) break;

            string line = lines[i];
            string[] parts = line.Split(',');

            if (parts.Length < 3)
            {
                LogHelper.Log($"Invalid line format in {fileName}: {line}, skipping");
                continue;
            }

            if (!int.TryParse(parts[0], out int enrollNumber))
            {
                LogHelper.Log($"Invalid enrollment number in {fileName}: {parts[0]}, skipping");
                continue;
            }

            if (!DateTime.TryParse(parts[2], out DateTime deviceTime))
            {
                LogHelper.Log($"Invalid date/time in {fileName}: {parts[2]}, skipping");
                continue;
            }

            records.Add((enrollNumber, deviceTime));
        }

        processed = records.Count;

        foreach (var rec in records)
        {
            if (token.IsCancellationRequested) break;

            try
            {
                _dbHelper.InsertStaffGLog(
                    device.StaffDeviceId,
                    rec.enrollNumber.ToString(),
                    rec.deviceTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    $"{rec.enrollNumber},,{rec.deviceTime:yyyy-MM-dd HH:mm:ss}",
                    sync: false,
                    deviceType: device.StaffAttendanceType,
                    filter: false
                );

                inserted++;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error inserting staff GLog record for enrollment {rec.enrollNumber}: {ex.Message}. Line: {rec}");
            }
        }

        return (processed, inserted);
    }

    /// <summary>
    /// Retrieves filtered data from staffglogs and inserts it into staffattendencelog.
    /// </summary>
    /// <param name="machineNo">Machine number of the device.</param>
    /// <param name="token">Cancellation token to handle task cancellation.</param>
    private async Task InsertFilteredLogsIntoStaffAttendanceLogAsync(int machineNo, CancellationToken token)
    {
        try
        {
            LogHelper.Log("👣 Starting staff attendance application from filtered staff GLogs...");
            _dbHelper.DebugStaffGLogFiltering();

            var filteredLogs = _dbHelper.FilterAndUpdateStaffGLogRecordsByDate();
            if (filteredLogs == null || filteredLogs.Count == 0)
            {
                LogHelper.Log("ℹ️ No filtered staff logs to apply.");
                return;
            }

            bool isOneWay = string.Equals(_dbHelper.DecideAttendanceCase(), "1way", StringComparison.OrdinalIgnoreCase);
            LogHelper.Log($"⚙️ Attendance mode detected: {(isOneWay ? "1-WAY" : "2-WAY")}");

            // Order deterministically
            var ordered = filteredLogs
                .OrderBy(l => l.EnrollmentNumber)
                .ThenBy(l => l.DeviceTime)
                .ToList();

            int inserted = 0;
            int updated = 0;
            int skipped = 0;

            var staffDevicesFromDb = _dbHelper.LoadStaffDevices() ?? new List<Device>();
            var matchedDbDevice = staffDevicesFromDb.FirstOrDefault(d => d.MachineNo == machineNo);
            int? staffDeviceId = matchedDbDevice?.StaffDeviceId ?? _dbHelper.GetStaffDeviceIdByMachineNo(machineNo);

            // Resolve device contact details to call DeviceService.GetUserName
            string deviceIp = null;
            int devicePort = 0;
            int deviceMachineNo = machineNo; // default to the machineNo being processed
            string devicePassword = string.Empty;

            if (matchedDbDevice != null)
            {
                deviceIp = matchedDbDevice.DeviceIP;
                devicePort = matchedDbDevice.Port;
                deviceMachineNo = matchedDbDevice.MachineNo;
                devicePassword = matchedDbDevice.Password ?? string.Empty;
            }
            else if (staffDeviceId.HasValue)
            {
                try
                {
                    var details = _dbHelper.GetStaffDeviceDetails(staffDeviceId.Value);
                    deviceIp = details.ip;
                    devicePort = details.port;
                    deviceMachineNo = details.machineNo;
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"⚠️ Unable to load staff device details for StaffDeviceId={staffDeviceId}: {ex.Message}");
                }
            }
            else
            {
                LogHelper.Log($"⚠️ Could not resolve staff device details for MachineNo={machineNo}; GetUserName calls will be skipped.");
            }

            if (isOneWay)
            {
                // 1-WAY: each punch alternates (handled internally by DB helper)
                foreach (var log in ordered)
                {
                    if (token.IsCancellationRequested)
                    {
                        LogHelper.Log("⏹ Cancellation requested (1-way processing).");
                        break;
                    }

                    if (log == null ||
                        string.IsNullOrWhiteSpace(log.EnrollmentNumber) ||
                        log.DeviceTime == DateTime.MinValue)
                    {
                        skipped++;
                        continue;
                    }

                    if (!int.TryParse(log.EnrollmentNumber, out int enrollmentId))
                    {
                        skipped++;
                        continue;
                    }

                    string ts = log.DeviceTime.ToString("yyyy-MM-dd HH:mm:ss");

                    // Try to obtain username from device (best-effort). If it fails, continue with empty string.
                    string userName = string.Empty;
                    if (!string.IsNullOrWhiteSpace(deviceIp))
                    {
                        try
                        {
                            userName = await DeviceService.GetUserName(deviceIp, devicePort, deviceMachineNo, devicePassword, enrollmentId, machineNo);
                            LogHelper.Log($"🔎 Resolved username for Enroll={enrollmentId} -> '{userName}'");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"⚠️ GetUserName failed for Enroll={enrollmentId} on device {deviceIp}:{devicePort} - {ex.Message}");
                        }
                    }

                    int logId = _dbHelper.InsertOrUpdateStaffAttendance1Way(enrollmentId, ts, userName);

                    if (logId > 0)
                    {
                        // We cannot easily distinguish insert vs update without extra query—treat all as "processed".
                        inserted++;
                        LogHelper.Log($"[1WAY] Punch applied Enroll={enrollmentId}, Time={ts}, AttendanceLogId={logId}");
                    }
                    else
                    {
                        skipped++;
                        LogHelper.Log($"[1WAY] Ignored duplicate/invalid punch Enroll={enrollmentId}, Time={ts}");
                    }
                }

                LogHelper.Log($"🏁 1-WAY staff attendance processing complete. Punches processed={inserted}, Skipped={skipped}.");
            }
            else
            {
                // 2-WAY: original logic
                var openCheckInSeen = new HashSet<string>();

                foreach (var log in ordered)
                {
                    if (token.IsCancellationRequested)
                    {
                        LogHelper.Log("⏹ Cancellation requested (2-way processing).");
                        break;
                    }

                    if (log == null ||
                        string.IsNullOrWhiteSpace(log.EnrollmentNumber) ||
                        log.DeviceTime == DateTime.MinValue)
                    {
                        skipped++;
                        LogHelper.Log($"⚠️ Skipping invalid log (LogId={log?.LogId})");
                        continue;
                    }

                    if (!int.TryParse(log.EnrollmentNumber, out int enrollmentId))
                    {
                        skipped++;
                        LogHelper.Log($"⚠️ Skipping log with non-numeric enrollment '{log.EnrollmentNumber}' (LogId={log.LogId})");
                        continue;
                    }

                    string ts = log.DeviceTime.ToString("yyyy-MM-dd HH:mm:ss");

                    // Try to obtain username from device (best-effort). If it fails, continue with empty string.
                    string userName = string.Empty;
                    if (!string.IsNullOrWhiteSpace(deviceIp))
                    {
                        try
                        {
                            userName = await DeviceService.GetUserName(deviceIp, devicePort, deviceMachineNo, devicePassword, enrollmentId, machineNo);
                            LogHelper.Log($"🔎 Resolved username for Enroll={enrollmentId} -> '{userName}'");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"⚠️ GetUserName failed for Enroll={enrollmentId} on device {deviceIp}:{devicePort} - {ex.Message}");
                        }
                    }

                    if (string.Equals(log.DeviceType, "Check_In", StringComparison.OrdinalIgnoreCase))
                    {
                        var id = _dbHelper.InsertStaffAttendanceLog(enrollmentId, ts, userName);
                        openCheckInSeen.Add(log.EnrollmentNumber);
                        inserted++;
                        LogHelper.Log($"✅ Check-In applied: Enroll={log.EnrollmentNumber}, Time={ts}, RowId={id}");
                    }
                    else if (string.Equals(log.DeviceType, "Check_Out", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!openCheckInSeen.Contains(log.EnrollmentNumber))
                        {
                            skipped++;
                            LogHelper.Log($"⚠️ Unmatched Check-Out (Enroll={log.EnrollmentNumber}, Time={ts}) – skipped.");
                            continue;
                        }

                        bool ok = _dbHelper.UpdateStaffAttendanceLogForCheckOut(enrollmentId, ts);
                        if (ok)
                        {
                            openCheckInSeen.Remove(log.EnrollmentNumber);
                            updated++;
                            LogHelper.Log($"🟩 Check-Out applied: Enroll={log.EnrollmentNumber}, Time={ts}");
                        }
                        else
                        {
                            skipped++;
                            LogHelper.Log($"⚠️ Checkout update failed (Enroll={log.EnrollmentNumber}, Time={ts})");
                        }
                    }
                    else
                    {
                        skipped++;
                        LogHelper.Log($"ℹ️ Ignoring non-attendance type record (LogId={log.LogId}, Type={log.DeviceType})");
                    }
                }

                LogHelper.Log($"🏁 2-WAY staff attendance complete. Check-Ins inserted={inserted}, Check-Outs updated={updated}, Skipped={skipped}.");
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"❌ Error in InsertFilteredLogsIntoStaffAttendanceLogAsync: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private async Task<bool> IsDeviceReachableAsync(string ipAddress, int maxAttempts, CancellationToken token)
    {
        using (var ping = new System.Net.NetworkInformation.Ping())
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (token.IsCancellationRequested) return false;

                try
                {
                    LogHelper.Log($"🔄 Pinging device {ipAddress}, attempt {attempt}");
                    var reply = await ping.SendPingAsync(ipAddress, 2000); // 2-second timeout

                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        LogHelper.Log($"✅ Device {ipAddress} is reachable");
                        return true;
                    }
                    else
                    {
                        LogHelper.Log($"⚠️ Ping attempt {attempt} failed for device {ipAddress}: {reply.Status}");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error pinging device {ipAddress}: {ex.Message}");
                }

                // Wait 1 second before the next attempt
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
        }

        return false;
    }

    private int GetDevicePriority(string staffAttendanceType)
    {
        switch (staffAttendanceType)
        {
            case "Check_In":
                return 1;
            case "Check_Out":
                return 2;
            case "Check_In/Check_Out":
                return 3;
            default:
                return 4; // Default priority for unknown types
        }
    }
}