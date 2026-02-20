using smart_attendance_middleware;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            foreach (var log in ordered)
            {
                if (token.IsCancellationRequested)
                {
                    LogHelper.Log("⏹ Cancellation requested (staff processing).");
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

                // Resolve machineNo for device where possible
                int machineNo = 0;
                if (staffDeviceMap.TryGetValue(log.DeviceId, out var matchedDev))
                    machineNo = matchedDev.MachineNo;

                // Resolve action and staffId similar to StaffAttendenceServerFunctions.Process
                string resolvedAction = null;
                string resolvedStaffId = null;
                try
                {
                    if (machineNo != 0)
                    {
                        resolvedAction = _dbHelper.GetStaffDeviceAttendanceAction(machineNo);
                    }
                    // fallback to log's DeviceType normalization if DB lookup fails
                    if (string.IsNullOrWhiteSpace(resolvedAction))
                    {
                        // map DeviceType like "Check_In"/"Check_Out" to "check-in"/"check-out"
                        if (string.Equals(log.DeviceType, "Check_In", StringComparison.OrdinalIgnoreCase))
                            resolvedAction = "check-in";
                        else if (string.Equals(log.DeviceType, "Check_Out", StringComparison.OrdinalIgnoreCase))
                            resolvedAction = "check-out";
                    }

                    resolvedStaffId = _dbHelper.GetStaffIdByEnrollment(enrollmentId);

                    if (!string.IsNullOrEmpty(resolvedAction) && !string.IsNullOrEmpty(resolvedStaffId))
                    {
                        // Mixed-mode resolution if needed
                        if (resolvedAction.IndexOf("/", StringComparison.OrdinalIgnoreCase) >= 0
                            || resolvedAction.Equals("check-in/check-out", StringComparison.OrdinalIgnoreCase)
                            || resolvedAction.Equals("check_in/check_out", StringComparison.OrdinalIgnoreCase)
                            || resolvedAction.Equals("both", StringComparison.OrdinalIgnoreCase))
                        {
                            if (machineNo != 0)
                            {
                                var eff = _dbHelper.GetEffectiveStaffActionForDevice(machineNo, enrollmentId, log.DeviceTime);
                                if (!string.IsNullOrEmpty(eff))
                                {
                                    LogHelper.Log($"[StaffSync] Device mixed mode; resolved effective action='{eff}' for Enroll={enrollmentId}");
                                    resolvedAction = eff;
                                }
                                else
                                {
                                    LogHelper.Log("[StaffSync] Could not resolve effective action for mixed-mode device; letting DB helpers decide later.");
                                }
                            }
                        }

                        LogHelper.Log($"[StaffSync] Staff device action='{resolvedAction}', StaffID='{resolvedStaffId}', Enroll={enrollmentId}");
                    }
                    else
                    {
                        LogHelper.Log("[StaffSync] Missing action or staff id; debounce will be skipped for this log.");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[StaffSync][Resolve] Error resolving action/staff: {ex.Message}");
                }

                // --- Apply debounce exactly as StaffAttendenceServerFunctions.Process ---
                try
                {
                    var s = _dbHelper.GetSetting("AttendanceDebounceSeconds");
                    if (!int.TryParse(s, out var debounceSeconds) || debounceSeconds < 0)
                    {
                        MessageBox.Show(
                            "Debounce setting is missing or invalid. Please set it in Device Manager and try again.",
                            "Debounce Setting Required",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        LogHelper.Log("[StaffSync][Debounce] Missing/invalid debounce setting. Aborting processing of staff filtered logs.");
                        return; // abort entire processing loop, matching server behavior
                    }

                    if (string.IsNullOrEmpty(resolvedAction) || string.IsNullOrEmpty(resolvedStaffId))
                    {
                        LogHelper.Log("[StaffSync] Debounce check skipped (missing action or staffId)");
                    }
                    else
                    {
                        string action = resolvedAction;

                        if (action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                        {
                            DateTime? pairedCheckIn = null;
                            try
                            {
                                pairedCheckIn = _dbHelper.GetPairedCheckInTimeForStaff(resolvedStaffId, log.DeviceTime);
                            }
                            catch (Exception exPair)
                            {
                                LogHelper.Log($"[StaffSync][Debounce] Failed to get paired check-in for staff {resolvedStaffId}: {exPair.Message}");
                            }

                            if (pairedCheckIn.HasValue)
                            {
                                var diff = log.DeviceTime - pairedCheckIn.Value;
                                if (diff.TotalSeconds < debounceSeconds)
                                {
                                    skipped++;
                                    LogHelper.Log($"[StaffSync][Debounce] STAFF CHECK-OUT ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) Staff={resolvedStaffId} Enroll={enrollmentId} Time={log.DeviceTime:yyyy-MM-dd HH:mm:ss}");
                                    continue; // skip this log
                                }
                            }
                            else
                            {
                                LogHelper.Log("[StaffSync][Debounce] No paired/open session for staff checkout; passing to handler.");
                            }
                        }
                        else if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                        {
                            bool openSessionExists = false;
                            DateTime? lastCheckOut = null;

                            try
                            {
                                var logs = _dbHelper.GetAllStaffAttendanceLogs();
                                if (logs != null)
                                {
                                    foreach (var lr in logs)
                                    {
                                        try
                                        {
                                            string sid = null;
                                            try { sid = Convert.ToString(lr.StaffID); } catch { try { sid = Convert.ToString(lr.StaffId); } catch { sid = null; } }

                                            if (string.IsNullOrEmpty(sid) || !sid.Equals(resolvedStaffId, StringComparison.OrdinalIgnoreCase))
                                                continue;

                                            DateTime? co = null;
                                            try
                                            {
                                                co = lr.CheckOutTime as DateTime?;
                                            }
                                            catch
                                            {
                                                try
                                                {
                                                    var coStr = Convert.ToString(lr.CheckOutTime);
                                                    if (DateTime.TryParse(coStr, out DateTime tmpCo))
                                                        co = tmpCo;
                                                    else
                                                        co = null;
                                                }
                                                catch
                                                {
                                                    co = null;
                                                }
                                            }

                                            if (!co.HasValue)
                                                openSessionExists = true;
                                            else
                                            {
                                                if (!lastCheckOut.HasValue || co.Value > lastCheckOut.Value)
                                                    lastCheckOut = co.Value;
                                            }
                                        }
                                        catch { /* ignore malformed rows */ }
                                    }
                                }
                            }
                            catch (Exception exLogs)
                            {
                                LogHelper.Log($"[StaffSync][Debounce] Failed to scan staff logs for debounce: {exLogs.Message}");
                            }

                            if (!openSessionExists)
                            {
                                const int completedSessionCheckInDebounceSeconds = 10;
                                if (lastCheckOut.HasValue)
                                {
                                    var diff = log.DeviceTime - lastCheckOut.Value;
                                    if (diff.TotalSeconds < completedSessionCheckInDebounceSeconds)
                                    {
                                        skipped++;
                                        LogHelper.Log(
                                            $"[StaffSync][Debounce] STAFF CHECK-IN ignored after completed session " +
                                            $"(Δ={diff.TotalSeconds:F0}s < {completedSessionCheckInDebounceSeconds}s) " +
                                            $"Staff={resolvedStaffId} Enroll={enrollmentId} Time={log.DeviceTime:yyyy-MM-dd HH:mm:ss}");
                                        continue;
                                    }
                                }
                                else
                                {
                                    LogHelper.Log(
                                        $"[StaffSync][Debounce] STAFF CHECK-IN completed-session debounce check skipped (no last checkout). " +
                                        $"Staff={resolvedStaffId} Enroll={enrollmentId} Time={log.DeviceTime:yyyy-MM-dd HH:mm:ss}");
                                }
                            }
                            else
                            {
                                if (lastCheckOut.HasValue)
                                {
                                    var diff = log.DeviceTime - lastCheckOut.Value;
                                    if (diff.TotalSeconds < debounceSeconds)
                                    {
                                        skipped++;
                                        LogHelper.Log(
                                            $"[StaffSync][Debounce] STAFF CHECK-IN ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) " +
                                            $"Staff={resolvedStaffId} Enroll={enrollmentId} Time={log.DeviceTime:yyyy-MM-dd HH:mm:ss}");
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exDebounce)
                {
                    LogHelper.Log($"[StaffSync][Debounce][ERROR] {exDebounce.Message}");
                    // Fail-open: proceed to normal handling
                }

                // --- Handling (insert/update) using staff DB helpers ---
                try
                {
                    // If we don't have resolvedStaffId, try to resolve now
                    string staffId = resolvedStaffId ?? _dbHelper.GetStaffIdByEnrollment(enrollmentId);
                    if (string.IsNullOrWhiteSpace(staffId))
                    {
                        skipped++;
                        LogHelper.Log($"⚠️ No staff mapping found for enrollment {enrollmentId} (skipping)");
                        continue;
                    }

                    // Use resolvedAction if available, otherwise fallback to log.DeviceType mapping
                    string actionToApply = resolvedAction;
                    if (string.IsNullOrWhiteSpace(actionToApply))
                    {
                        if (string.Equals(log.DeviceType, "Check_In", StringComparison.OrdinalIgnoreCase))
                            actionToApply = "check-in";
                        else if (string.Equals(log.DeviceType, "Check_Out", StringComparison.OrdinalIgnoreCase))
                            actionToApply = "check-out";
                    }

                    if (string.Equals(actionToApply, "check-in", StringComparison.OrdinalIgnoreCase))
                    {
                        int newId = _dbHelper.InsertStaffCheckInByStaffId(staffId, ts, null);
                        if (newId > 0)
                        {
                            inserted++;
                            LogHelper.Log($"✅ Staff Check-In applied: StaffId={staffId}, Enroll={log.EnrollmentNumber}, Time={ts}, RowId={newId}");
                        }
                        else
                        {
                            skipped++;
                            LogHelper.Log($"[StaffSync] Ignored duplicate/invalid staff check-in StaffId={staffId}, Enroll={log.EnrollmentNumber}, Time={ts}");
                        }
                    }
                    else if (string.Equals(actionToApply, "check-out", StringComparison.OrdinalIgnoreCase))
                    {
                        bool ok = _dbHelper.InsertStaffCheckOutByStaffId(staffId, ts);
                        if (ok)
                        {
                            updated++;
                            LogHelper.Log($"🟩 Staff Check-Out applied: StaffId={staffId}, Enroll={log.EnrollmentNumber}, Time={ts}");
                        }
                        else
                        {
                            skipped++;
                            LogHelper.Log($"⚠️ Staff checkout update failed (StaffId={staffId}, Enroll={log.EnrollmentNumber}, Time={ts})");
                        }
                    }
                    else
                    {
                        skipped++;
                        LogHelper.Log($"ℹ️ Ignoring non-attendance type record (LogId={log.LogId}, Type={log.DeviceType})");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error applying staff attendance for Enroll={enrollmentId}: {ex.Message}");
                    skipped++;
                }
            }

            LogHelper.Log($"🏁 Staff attendance processing complete. Check-Ins inserted={inserted}, Check-Outs updated={updated}, Skipped={skipped}.");
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