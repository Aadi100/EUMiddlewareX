using smart_attendance_middleware;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class StaffAttendanceLogSyncService : IDisposable
{
    private readonly int _syncInterval = 6 * 60 * 60 * 1000;       // 6 hours
    private readonly int _retryDelay = 30000;                      // 30 seconds on unexpected error
    private readonly int _earlyRetryInterval = 15 * 60 * 1000;     // 15 minutes on total failure

    private CancellationTokenSource _cts;
    private Task _syncTask;

    private readonly DatabaseHelper _dbHelper;
    private readonly WebSocketManager _wsManager;

    public static event Action InitialSyncCompleted;

    // State fields (aligned with AttendanceLogSyncService)
    private bool _initialSyncRaised = false;
    private volatile bool _loopRunning = false;
    private volatile bool _earlyRetryRequested = false;

    // Optional: enable continuous looping like the original service
    private bool _continuousSyncEnabled = false;

    public StaffAttendanceLogSyncService(WebSocketManager wsManager)
    {
        _dbHelper = new DatabaseHelper();
        _wsManager = wsManager;
    }

    public void Start()
    {
        if (_syncTask != null && !_syncTask.IsCompleted)
            return;

        if (_loopRunning)
            return;

        _cts = new CancellationTokenSource();
        LogHelper.Log("Staff attendance log sync service started");

        _syncTask = Task.Run(async () =>
        {
            try
            {
                _loopRunning = true;
                await RunSyncLoopAsync(_cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Unhandled exception in staff sync task: {ex.Message}");
            }
            finally
            {
                _loopRunning = false;
            }
        }, _cts.Token);
    }

    public void Stop()
    {
        if (_cts != null)
        {
            LogHelper.Log("Staff attendance log sync service stopping");
            _cts.Cancel();
        }
    }

    private async Task RunSyncLoopAsync(CancellationToken token)
    {
        try
        {
            // Initial cycle (immediate)
            await ExecuteFullSyncCycleAsync(token, isInitial: true).ConfigureAwait(false);

            // Fire initial sync completed once
            if (!_initialSyncRaised)
            {
                _initialSyncRaised = true;
                LogHelper.Log("✅ Initial staff attendance log sync completed - firing completion event");
                try
                {
                    InitialSyncCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error firing InitialSyncCompleted event: {ex.Message}");
                }
            }

            // Exit if continuous is disabled (same pattern as original)
            if (!_continuousSyncEnabled)
            {
                LogHelper.Log("⏹ Continuous staff attendance log sync disabled - no further cycles will run.");
                return;
            }

            // Main loop with dynamic delay
            while (!token.IsCancellationRequested)
            {
                int delayMs = _earlyRetryRequested ? _earlyRetryInterval : _syncInterval;
                _earlyRetryRequested = false; // reset

                try
                {
                    LogHelper.Log($"⏳ Waiting {(delayMs / 60000)} minutes until next staff attendance sync cycle...");
                    await Task.Delay(delayMs, token).ConfigureAwait(false);

                    await ExecuteFullSyncCycleAsync(token, isInitial: false).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Staff attendance sync error: {ex.Message}");
                    // Short backoff on unexpected exception
                    try
                    {
                        await Task.Delay(_retryDelay, token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) { break; }
                }
            }
        }
        catch (TaskCanceledException)
        {
            // normal cancellation
        }
        finally
        {
            LogHelper.Log("Staff attendance log sync service stopped");
        }
    }

    private async Task ExecuteFullSyncCycleAsync(CancellationToken token, bool isInitial)
    {
        if (token.IsCancellationRequested) return;

        try
        {
            // 1) Download prioritized staff device logs (with retries)
            await SyncAllStaffDeviceLogsAsync(token).ConfigureAwait(false);
            if (token.IsCancellationRequested) return;

            // 2) Verify and process StaffGLogs.*.txt into StaffGLog table
            await VerifyAndProcessStaffGlogFilesAsync(token).ConfigureAwait(false);
            if (token.IsCancellationRequested) return;

            // 3) Process filtered GLog records into StaffAttendanceLog (1-way/2-way)
            await ProcessFilteredStaffGLogRecordsAsync(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"❌ Error during staff full sync cycle: {ex.Message}");
            _earlyRetryRequested = true;
        }
    }

    // ============================ Phase 1: Device downloads (prioritized + retry) ============================

    private async Task SyncAllStaffDeviceLogsAsync(CancellationToken token)
    {
        if (AppSession.StaffDeviceList?.Count > 0)
        {
            LogHelper.Log($"🔄 Starting prioritized staff log sync cycle for {AppSession.StaffDeviceList.Count} devices");

            var checkInDevices = AppSession.StaffDeviceList
                .Where(d => string.Equals(d.StaffAttendanceType, "check_in", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var checkOutDevices = AppSession.StaffDeviceList
                .Where(d => string.Equals(d.StaffAttendanceType, "check_out", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var otherDevices = AppSession.StaffDeviceList
                .Except(checkInDevices)
                .Except(checkOutDevices)
                .ToList();

            LogHelper.Log($"📊 Staff device distribution - Check-in: {checkInDevices.Count}, Check-out: {checkOutDevices.Count}, Other: {otherDevices.Count}");

            bool checkInSuccess = await DownloadDeviceLogsWithRetry(checkInDevices, "CHECK-IN", token).ConfigureAwait(false);
            if (!checkInSuccess)
            {
                LogHelper.Log("❌ Staff CHECK-IN device sync failed after all retries. Will retry earlier in next loop.");
                _earlyRetryRequested = true;
                return;
            }

            bool checkOutSuccess = await DownloadDeviceLogsWithRetry(checkOutDevices, "CHECK-OUT", token).ConfigureAwait(false);

            if (otherDevices.Count > 0)
            {
                await DownloadDeviceLogsWithRetry(otherDevices, "OTHER", token).ConfigureAwait(false);
            }

            if (checkInSuccess && checkOutSuccess)
                LogHelper.Log("✅ All prioritized staff log sync completed successfully");
            else
            {
                LogHelper.Log("⚠️ Some staff device sync operations failed - will retry earlier next cycle");
                _earlyRetryRequested = true;
            }
        }
        else
        {
            LogHelper.Log("No staff devices available for attendance log sync");
        }
    }

    // Same name as AttendanceLogSyncService for consistency
    private async Task<bool> DownloadDeviceLogsWithRetry(List<Device> devices, string deviceType, CancellationToken token)
    {
        if (devices == null || devices.Count == 0) return true;

        const int maxRetries = 3;
        const int retryDelaySeconds = 2;

        var failedDevices = new List<Device>(devices);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            if (token.IsCancellationRequested) break;

            LogHelper.Log($"🔄 {deviceType} staff devices - Attempt {attempt}/{maxRetries} for {failedDevices.Count} devices");

            var currentAttemptFailures = new List<Device>();
            int successCount = 0;

            foreach (var device in failedDevices)
            {
                if (token.IsCancellationRequested) break;

                try
                {
                    // Reachability check (shared)
                    bool reachable = await CommonUtils.IsDeviceReachableAsync(device.DeviceIP, 3, token).ConfigureAwait(false);
                    if (!reachable)
                    {
                        LogHelper.Log($"❌ Device {device.StaffDeviceId} ({device.DeviceIP}) is not reachable after multiple attempts. Will retry later.");
                        currentAttemptFailures.Add(device);
                        continue;
                    }

                    LogHelper.Log($"📥 Reading {deviceType} staff logs from device {device.StaffDeviceId} ({device.DeviceIP}) - Attempt {attempt}");

                    int recordCount = await DeviceService.ReadAndSaveStaffGLogData(
                        device.DeviceIP,
                        device.Port,
                        device.MachineNo,
                        device.Password
                    ).ConfigureAwait(false);

                    if (recordCount >= 0)
                    {
                        LogHelper.Log($"✅ Read {recordCount} {deviceType} staff records from device {device.StaffDeviceId} on attempt {attempt}");
                        successCount++;
                    }
                    else
                    {
                        LogHelper.Log($"❌ Failed reading {deviceType} staff logs from device {device.StaffDeviceId} on attempt {attempt}");
                        currentAttemptFailures.Add(device);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error reading {deviceType} staff logs from device {device.StaffDeviceId} on attempt {attempt}: {ex.Message}");
                    currentAttemptFailures.Add(device);
                }
            }

            LogHelper.Log($"📊 {deviceType} staff attempt {attempt} results - Success: {successCount}, Failed: {currentAttemptFailures.Count}");

            failedDevices = currentAttemptFailures;

            if (failedDevices.Count == 0)
            {
                LogHelper.Log($"✅ All {deviceType} staff devices synced successfully (attempt {attempt})");
                return true;
            }

            if (attempt < maxRetries && failedDevices.Count > 0)
            {
                LogHelper.Log($"⏳ Waiting {retryDelaySeconds}s before next {deviceType} staff retry...");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { break; }
            }
        }

        if (failedDevices.Count > 0)
        {
            var failedDeviceIds = string.Join(", ", failedDevices.Select(d => d.StaffDeviceId));
            LogHelper.Log($"❌ {deviceType} staff sync failed for devices: {failedDeviceIds} after all retries");
            _earlyRetryRequested = true;
            return false;
        }

        return true;
    }

    // ============================ Phase 2: Verify and process StaffGLogs.*.txt ============================

    private async Task VerifyAndProcessStaffGlogFilesAsync(CancellationToken token)
    {
        try
        {
            string folderPath = AppDomain.CurrentDomain.BaseDirectory;
            string[] staffGlogFiles = Directory.GetFiles(folderPath, "StaffGLogs.*.txt");

            if (staffGlogFiles.Length == 0)
            {
                LogHelper.Log("No staff GLog files found for verification");
                return;
            }

            LogHelper.Log($"Found {staffGlogFiles.Length} staff GLog files to verify");
            int totalProcessed = 0;
            int totalInserted = 0;

            foreach (string filePath in staffGlogFiles)
            {
                if (token.IsCancellationRequested) break;

                string fileName = Path.GetFileName(filePath);
                string machineNoStr = fileName.Replace("StaffGLogs.", "").Replace(".txt", "");

                if (!int.TryParse(machineNoStr, out int machineNo))
                {
                    LogHelper.Log($"Invalid machine number in staff filename: {fileName}, skipping");
                    continue;
                }

                try
                {
                    var (processed, inserted) = await ProcessStaffGlogFileAsync(filePath, machineNo, token).ConfigureAwait(false);
                    totalProcessed += processed;
                    totalInserted += inserted;

                    File.Delete(filePath);
                    LogHelper.Log($"Deleted staff GLog file: {fileName}");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Error processing staff GLog file {fileName}: {ex.Message}");
                }
            }

            LogHelper.Log($"Staff glog verification complete. Processed {totalProcessed} records, inserted {totalInserted} missing records.");
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error during staff GLog verification: {ex.Message}");
        }
    }

    private async Task<(int processed, int inserted)> ProcessStaffGlogFileAsync(string filePath, int machineNo, CancellationToken token)
    {
        int processed = 0;
        int inserted = 0;

        string fileName = Path.GetFileName(filePath);
        LogHelper.Log($"Processing staff GLog file: {fileName} for machine: {machineNo}");

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

            if (parts.Length < 3) continue;
            if (!int.TryParse(parts[0], out int enrollNumber)) continue;
            if (!DateTime.TryParse(parts[2], out DateTime deviceTime)) continue;

            records.Add((enrollNumber, deviceTime));
        }

        processed = records.Count;

        int? staffDeviceId = _dbHelper.GetStaffDeviceIdByMachineNo(machineNo);
        if (!staffDeviceId.HasValue)
        {
            LogHelper.Log($"No staff device found for machine number {machineNo}, skipping");
            return (processed, inserted);
        }

        // Load device type/password for proper StaffGLog insert
        var staffDevices = _dbHelper.LoadStaffDevices() ?? new List<Device>();
        var thisDevice = staffDevices.FirstOrDefault(d => d.StaffDeviceId == staffDeviceId.Value);
        string deviceType = thisDevice?.StaffAttendanceType ?? "Check_In/Check_Out";

        foreach (var rec in records)
        {
            if (token.IsCancellationRequested) break;

            try
            {
                string originalLine = lines.FirstOrDefault(l => l.StartsWith($"{rec.enrollNumber},")) ??
                                      $"{rec.enrollNumber},,{rec.deviceTime:yyyy-MM-dd HH:mm:ss}";

                _dbHelper.InsertStaffGLog(
                    staffDeviceId.Value,
                    rec.enrollNumber.ToString(),
                    rec.deviceTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    originalLine,
                    sync: false,
                    deviceType: deviceType,
                    filter: false
                );

                inserted++;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error inserting StaffGLog record enrollment {rec.enrollNumber}: {ex.Message}");
            }
        }

        LogHelper.Log($"Finished {fileName}. Processed {processed}, inserted {inserted}");
        await Task.CompletedTask;
        return (processed, inserted);
    }

    // ============================ Phase 3: Process filtered records into StaffAttendanceLog ============================

    private async Task ProcessFilteredStaffGLogRecordsAsync(CancellationToken token)
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

            var ordered = filteredLogs
                .OrderBy(l => l.EnrollmentNumber)
                .ThenBy(l => l.DeviceTime)
                .ToList();

            int inserted = 0;
            int updated = 0;
            int skipped = 0;

            var staffDevicesFromDb = _dbHelper.LoadStaffDevices() ?? new List<Device>();
            var staffDeviceMap = staffDevicesFromDb.ToDictionary(d => d.StaffDeviceId, d => d);

            if (isOneWay)
            {
                foreach (var log in ordered)
                {
                    if (token.IsCancellationRequested)
                    {
                        LogHelper.Log("⏹ Cancellation requested (1-way processing).");
                        break;
                    }

                    if (log == null || string.IsNullOrWhiteSpace(log.EnrollmentNumber) || log.DeviceTime == DateTime.MinValue)
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

                    // Resolve device details via StaffDeviceId stored in GLogRecord.DeviceId
                    string userName = string.Empty;
                    int staffDeviceId = log.DeviceId;
                    string deviceIp = null;
                    int devicePort = 0;
                    int deviceMachineNo = 0;
                    string devicePassword = string.Empty;

                    if (staffDeviceMap.TryGetValue(staffDeviceId, out var matched))
                    {
                        deviceIp = matched.DeviceIP;
                        devicePort = matched.Port;
                        deviceMachineNo = matched.MachineNo;
                        devicePassword = matched.Password ?? string.Empty;
                    }
                    else
                    {
                        try
                        {
                            var details = _dbHelper.GetStaffDeviceDetails(staffDeviceId);
                            deviceIp = details.ip;
                            devicePort = details.port;
                            deviceMachineNo = details.machineNo;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"⚠️ Unable to load staff device details for StaffDeviceId={staffDeviceId}: {ex.Message}");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(deviceIp))
                    {
                        try
                        {
                            userName = await DeviceService.GetUserName(deviceIp, devicePort, deviceMachineNo, devicePassword, enrollmentId, deviceMachineNo);
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
                var openCheckInSeen = new HashSet<string>();

                foreach (var log in ordered)
                {
                    if (token.IsCancellationRequested)
                    {
                        LogHelper.Log("⏹ Cancellation requested (2-way processing).");
                        break;
                    }

                    if (log == null || string.IsNullOrWhiteSpace(log.EnrollmentNumber) || log.DeviceTime == DateTime.MinValue)
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

                    // Resolve device details via StaffDeviceId stored in GLogRecord.DeviceId
                    string userName = string.Empty;
                    int staffDeviceId = log.DeviceId;
                    string deviceIp = null;
                    int devicePort = 0;
                    int deviceMachineNo = 0;
                    string devicePassword = string.Empty;

                    if (staffDeviceMap.TryGetValue(staffDeviceId, out var matched))
                    {
                        deviceIp = matched.DeviceIP;
                        devicePort = matched.Port;
                        deviceMachineNo = matched.MachineNo;
                        devicePassword = matched.Password ?? string.Empty;
                    }
                    else
                    {
                        try
                        {
                            var details = _dbHelper.GetStaffDeviceDetails(staffDeviceId);
                            deviceIp = details.ip;
                            devicePort = details.port;
                            deviceMachineNo = details.machineNo;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"⚠️ Unable to load staff device details for StaffDeviceId={staffDeviceId}: {ex.Message}");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(deviceIp))
                    {
                        try
                        {
                            userName = await DeviceService.GetUserName(deviceIp, devicePort, deviceMachineNo, devicePassword, enrollmentId, deviceMachineNo);
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
            LogHelper.Log($"❌ Error in ProcessFilteredStaffGLogRecordsAsync: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        LogHelper.Log("Staff attendance log sync service disposed");
    }
}