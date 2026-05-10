using smart_attendance_middleware;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static smart_attendance_middleware.AttendenceServerFunctions;
using Newtonsoft.Json;

public class AttendanceLogSyncService : IDisposable
{
    private readonly int _syncInterval = 6 * 60 * 60 * 1000; // 6 hours
    private readonly int _retryDelay = 30000;                // 30 seconds on unexpected error
    private readonly int _earlyRetryInterval = 15 * 60 * 1000; // 15 minutes on total failure

    private CancellationTokenSource _cts;
    private Task _syncTask;

    private readonly DatabaseHelper _dbHelper;
    public readonly AttendenceServerFunctions _attendenceServerFunctions;
    public HttpHandler _httpHandler;
    private Processor _processor; 

    public static event Action InitialSyncCompleted;

    // New state fields
    private bool _initialSyncRaised = false;
    private volatile bool _loopRunning = false;
    private volatile bool _earlyRetryRequested = false;

    // TaskCompletionSource to signal "initial sync completed"
    private readonly TaskCompletionSource<bool> _startedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task Started => _startedTcs.Task;

    // Local DTO used to prepare bulk payload from DB model
    private sealed class UnsyncedAttendanceDto
    {
        public int LogId { get; set; }
        public int MemberId { get; set; }
        public int EnrollmentId { get; set; }
        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }
        public bool NeedsCheckInSync { get; set; }
        public bool NeedsCheckOutSync { get; set; }
    }

    // Constructor updated to remove wsManager
    public AttendanceLogSyncService()
    {
        _dbHelper = new DatabaseHelper();
        _attendenceServerFunctions = new AttendenceServerFunctions();
        _processor = new Processor(_dbHelper, null, _attendenceServerFunctions);
        _httpHandler = new HttpHandler(_dbHelper, _processor);
    }
    
    public void Start()
    {
        if (_syncTask != null && !_syncTask.IsCompleted)
            return;

        if (_loopRunning)
            return;

        _cts = new CancellationTokenSource();
        LogHelper.Log("Attendance log sync service started");

        _syncTask = Task.Run(async () =>
        {
            try
            {
                _loopRunning = true;
                await RunSyncLoopAsync(_cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Unhandled exception in sync task: {ex.Message}");
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
            LogHelper.Log("Attendance log sync service stopping");
            _cts.Cancel();
        }
    }


    // Add near other state fields
    private bool _continuousSyncEnabled = false; // set true if you ever want the old looping behavior

    private async Task RunSyncLoopAsync(CancellationToken token)
    {
        try
        {
            // Initial cycle (immediate)
            await ExecuteFullSyncCycleAsync(token, isInitial: true).ConfigureAwait(false);

            // Fire initial sync completed only once and signal Started TCS after completion
            if (!_initialSyncRaised)
            {
                _initialSyncRaised = true;
                LogHelper.Log("✅ Initial attendance log sync completed - firing completion event");
                try
                {
                    // Invoke the event handlers so subscribers run before we signal Started
                    InitialSyncCompleted?.Invoke();
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error firing InitialSyncCompleted event: {ex.Message}");
                }
                finally
                {
                    // Signal callers awaiting Started that the initial sync is complete.
                    if (!_startedTcs.Task.IsCompleted)
                    {
                        _startedTcs.TrySetResult(true);
                    }
                }
            }

            // If continuous looping is disabled, exit after initial cycle
            if (!_continuousSyncEnabled)
            {
                LogHelper.Log("⏹ Continuous attendance log sync disabled - no further cycles will run.");
                return;
            }

            // Main loop with dynamic delay (only runs if enabled)
            while (!token.IsCancellationRequested)
            {
                int delayMs = _earlyRetryRequested ? _earlyRetryInterval : _syncInterval;
                _earlyRetryRequested = false; // reset request

                try
                {
                    LogHelper.Log($"⏳ Waiting {(delayMs / 60000)} minutes until next attendance sync cycle...");
                    await Task.Delay(delayMs, token).ConfigureAwait(false);

                    await ExecuteFullSyncCycleAsync(token, isInitial: false).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Attendance sync error: {ex.Message}");
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
            LogHelper.Log("Attendance log sync service stopped");
        }
    }

    private async Task ExecuteFullSyncCycleAsync(CancellationToken token, bool isInitial)
    {
        if (token.IsCancellationRequested) return;

        try
        {
            await SyncAllDeviceLogsAsync(token).ConfigureAwait(false);
            if (token.IsCancellationRequested) return;

            await VerifyAndProcessGlogFilesAsync(token).ConfigureAwait(false);
            if (token.IsCancellationRequested) return;

            await ProcessFilteredGLogRecordsAsync(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"❌ Error during full sync cycle: {ex.Message}");
        }
    }

    private async Task SyncAllDeviceLogsAsync(CancellationToken token)
    {
        if (AppSession.DeviceList?.Count > 0)
        {
            LogHelper.Log($"🔄 Starting prioritized attendance log sync cycle for {AppSession.DeviceList.Count} devices");

            var checkInDevices = AppSession.DeviceList.Where(d =>
                _dbHelper.GetDeviceAttendanceAction(d.MachineNo)?.ToLower() == "check-in").ToList();
            var checkOutDevices = AppSession.DeviceList.Where(d =>
                _dbHelper.GetDeviceAttendanceAction(d.MachineNo)?.ToLower() == "check-out").ToList();
            var otherDevices = AppSession.DeviceList.Except(checkInDevices).Except(checkOutDevices).ToList();

            LogHelper.Log($"📊 Device distribution - Check-in: {checkInDevices.Count}, Check-out: {checkOutDevices.Count}, Other: {otherDevices.Count}");

            bool checkInSuccess = await DownloadDeviceLogsWithRetry(checkInDevices, "CHECK-IN", token).ConfigureAwait(false);
            if (!checkInSuccess)
            {
                LogHelper.Log("❌ Check-in device sync failed after all retries. Will retry earlier in next loop.");
                _earlyRetryRequested = true;
                return;
            }

            bool checkOutSuccess = await DownloadDeviceLogsWithRetry(checkOutDevices, "CHECK-OUT", token).ConfigureAwait(false);

            if (otherDevices.Count > 0)
            {
                await DownloadDeviceLogsWithRetry(otherDevices, "OTHER", token).ConfigureAwait(false);
            }

            if (checkInSuccess && checkOutSuccess)
                LogHelper.Log("✅ All prioritized attendance log sync completed successfully");
            else
            {
                LogHelper.Log("⚠️ Some device sync operations failed - will retry earlier next cycle");
                _earlyRetryRequested = true;
            }
        }
        else
        {
            LogHelper.Log("No devices available for attendance log sync");
        }
    }

    private async Task<bool> DownloadDeviceLogsWithRetry(List<Device> devices, string deviceType, CancellationToken token)
    {
        if (devices.Count == 0) return true;

        const int maxRetries = 3;
        const int retryDelaySeconds = 2;

        var failedDevices = new List<Device>(devices);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            if (token.IsCancellationRequested) break;

            LogHelper.Log($"🔄 {deviceType} devices - Attempt {attempt}/{maxRetries} for {failedDevices.Count} devices");

            var currentAttemptFailures = new List<Device>();
            int successCount = 0;

            foreach (var device in failedDevices)
            {
                if (token.IsCancellationRequested) break;

                try
                {
                    bool reachable = await CommonUtils.IsDeviceReachableAsync(device.DeviceIP, 3, token).ConfigureAwait(false);
                    if (!reachable)
                    {
                        LogHelper.Log($"❌ Device {device.DeviceId} ({device.DeviceIP}) is not reachable after multiple attempts. Skipping...");
                        continue;
                    }

                    LogHelper.Log($"📥 Reading {deviceType} logs from device {device.DeviceId} ({device.DeviceIP}) - Attempt {attempt}");

                    int recordCount = await DeviceService.ReadAndSaveGLogData(
                        device.DeviceIP,
                        device.Port,
                        device.MachineNo,
                        device.Password
                    ).ConfigureAwait(false);

                    if (recordCount >= 0)
                    {
                        LogHelper.Log($"✅ Read {recordCount} {deviceType} records from device {device.DeviceId} on attempt {attempt}");
                        successCount++;
                    }
                    else
                    {
                        LogHelper.Log($"❌ Failed reading {deviceType} logs from device {device.DeviceId} on attempt {attempt}");
                        currentAttemptFailures.Add(device);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error reading {deviceType} logs from device {device.DeviceId} on attempt {attempt}: {ex.Message}");
                    currentAttemptFailures.Add(device);
                }
            }

            LogHelper.Log($"📊 {deviceType} attempt {attempt} results - Success: {successCount}, Failed: {currentAttemptFailures.Count}");

            failedDevices = currentAttemptFailures;

            if (failedDevices.Count == 0)
            {
                LogHelper.Log($"✅ All {deviceType} devices synced successfully (attempt {attempt})");
                return true;
            }

            if (attempt < maxRetries && failedDevices.Count > 0)
            {
                LogHelper.Log($"⏳ Waiting {retryDelaySeconds}s before next {deviceType} retry...");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), token).ConfigureAwait(false);
                }
                catch (TaskCanceledException) { break; }
            }
        }

        if (failedDevices.Count > 0)
        {
            var failedDeviceIds = string.Join(", ", failedDevices.Select(d => d.DeviceId));
            LogHelper.Log($"❌ {deviceType} sync failed for devices: {failedDeviceIds} after all retries");
            _earlyRetryRequested = true;
            return false;
        }

        return true;
    }

    private async Task VerifyAndProcessGlogFilesAsync(CancellationToken token)
    {
        try
        {
            string folderPath = AppDomain.CurrentDomain.BaseDirectory;
            string[] glogFiles = Directory.GetFiles(folderPath, "GLog.*.txt");

            if (glogFiles.Length == 0)
            {
                LogHelper.Log("No Glog files found for verification");
                return;
            }

            LogHelper.Log($"Found {glogFiles.Length} Glog files to verify");
            int totalProcessed = 0;
            int totalInserted = 0;

            foreach (string glogFile in glogFiles)
            {
                if (token.IsCancellationRequested) break;

                string fileName = Path.GetFileName(glogFile);
                string machineNoStr = fileName.Replace("GLog.", "").Replace(".txt", "");

                if (!int.TryParse(machineNoStr, out int machineNo))
                {
                    LogHelper.Log($"Invalid machine number in filename: {fileName}, skipping");
                    continue;
                }

                try
                {
                    var (processed, inserted) = await ProcessGlogFileAsync(glogFile, machineNo, token).ConfigureAwait(false);
                    totalProcessed += processed;
                    totalInserted += inserted;

                    File.Delete(glogFile);
                    LogHelper.Log($"Deleted Glog file: {fileName}");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Error processing Glog file {fileName}: {ex.Message}");
                }
            }

            LogHelper.Log($"Verification complete. Processed {totalProcessed} records, inserted {totalInserted} missing records.");
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error during Glog verification: {ex.Message}");
        }
    }

    public async Task<(int processed, int inserted)> ProcessGlogFileAsync(string filePath, int machineNo, CancellationToken token)
    {
        int processed = 0;
        int inserted = 0;

        string fileName = Path.GetFileName(filePath);
        LogHelper.Log($"Processing Glog file: {fileName} for machine: {machineNo}");

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
        int? deviceId = _dbHelper.GetDeviceIdByMachineNo(machineNo);
        if (!deviceId.HasValue)
        {
            LogHelper.Log($"No device found for machine number {machineNo}, skipping");
            return (processed, inserted);
        }

        foreach (var rec in records)
        {
            if (token.IsCancellationRequested) break;

            if (rec.enrollNumber >= 10000 && rec.enrollNumber <= 10100)
                continue;

            try
            {
                string originalLine = lines.FirstOrDefault(l => l.StartsWith($"{rec.enrollNumber},")) ??
                                      $"{rec.enrollNumber},,{rec.deviceTime:yyyy-MM-dd HH:mm:ss}";

                await _dbHelper.InsertGLogRecordAsync(
                    deviceId.Value,
                    rec.enrollNumber.ToString(),
                    rec.deviceTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    originalLine
                ).ConfigureAwait(false);

                inserted++;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error inserting GLog record enrollment {rec.enrollNumber}: {ex.Message}");
            }
        }

        LogHelper.Log($"Finished {fileName}. Processed {processed}, inserted {inserted}");
        return (processed, inserted);
    }

    public async Task ProcessFilteredGLogRecordsAsync(CancellationToken token)
    {
        List<GLogRecord> filteredRecords = _dbHelper.FilterAndUpdateGLogRecordsByDateTwoWay();
        LogHelper.Log($"Processing {filteredRecords.Count} filtered attendance events");

        foreach (var record in filteredRecords)
        {
            if (token.IsCancellationRequested) break;

            try
            {
                int enrollmentNumber = int.Parse(record.EnrollmentNumber);

                // Resolve machineNo from DeviceId
                var details = _dbHelper.GetDeviceDetails(record.DeviceId);
                if (details == (null, 0, 0)) continue;
                int machineNo = details.machineNo;

                // Resolve action via device mapping
                string action = _dbHelper.GetDeviceAttendanceAction(machineNo);
                if (string.IsNullOrEmpty(action))
                {
                    LogHelper.Log($"[FilteredDebounce] Unknown action for machine {machineNo}; skipping record {record.LogId}");
                    continue;
                }

                // Find memberId by enrollment
                int? memberId = _dbHelper.GetMemberIdByEnrollment(enrollmentNumber);
                if (!memberId.HasValue)
                {
                    LogHelper.Log($"[FilteredDebounce] Enrollment {enrollmentNumber} not linked to any member; skipping record {record.LogId}");
                    continue;
                }

                // Read debounce seconds from settings; if invalid, prompt and skip
                var s = _dbHelper.GetSetting("AttendanceDebounceSeconds");
                if (!int.TryParse(s, out var debounceSeconds) || debounceSeconds < 0)
                {
                    MessageBox.Show(
                        "Debounce setting is missing or invalid. Please set it in Device Manager and try again.",
                        "Debounce Setting Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    LogHelper.Log("[FilteredDebounce] Missing/invalid debounce setting. Skipping current record.");
                    continue;
                }

                // Apply debounce rules
                bool blockedByDebounce = false;
                if (action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                {
                    int openLogId = _dbHelper.GetOpenAttendanceLogId(memberId.Value);
                    if (openLogId > 0)
                    {
                        DateTime? openCheckInTime = _dbHelper.GetOpenAttendanceLogCheckInTime(memberId.Value);
                        if (openCheckInTime.HasValue)
                        {
                            var diff = record.DeviceTime - openCheckInTime.Value;
                            if (diff.TotalSeconds < debounceSeconds)
                            {
                                LogHelper.Log($"[FilteredDebounce] CHECK-OUT ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) Member={memberId} Time={record.DeviceTime:yyyy-MM-dd HH:mm:ss}");
                                blockedByDebounce = true;
                            }
                        }
                        else
                        {
                            LogHelper.Log("[FilteredDebounce] Open session found but no check-in time; proceeding.");
                        }
                    }
                    else
                    {
                        LogHelper.Log("[FilteredDebounce] No open session for checkout; passing to handler.");
                    }
                }
                else if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                {
                    DateTime? lastCheckOut = _dbHelper.GetLastCheckOutTimeForMember(memberId.Value);
                    if (lastCheckOut.HasValue)
                    {
                        var diff = record.DeviceTime - lastCheckOut.Value;
                        if (diff.TotalSeconds < debounceSeconds)
                        {
                            LogHelper.Log($"[FilteredDebounce] CHECK-IN ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) Member={memberId} Time={record.DeviceTime:yyyy-MM-dd HH:mm:ss}");
                            blockedByDebounce = true;
                        }
                    }
                }

                if (blockedByDebounce)
                    continue;

                // Original duplicate check
                if (!await _dbHelper.RecordExistsInAttendanceLogAsync(enrollmentNumber, record.DeviceTime).ConfigureAwait(false))
                {
                    var result = _attendenceServerFunctions.ProcessAttendanceEvent(
                        machineNo,
                        enrollmentNumber,
                        record.DeviceTime);

                    if (result != null)
                    {
                        var successProperty = result.GetType().GetProperty("Success");
                        if (successProperty != null && (bool)successProperty.GetValue(result))
                        {
                            LogHelper.Log($"Processed filtered attendance event - Machine:{machineNo}, Enrollment:{enrollmentNumber}, Time:{record.DeviceTime}");

                            await LogAttendanceToServerAsync(result).ConfigureAwait(false);
                            await Task.Delay(1000, token).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error processing filtered record {record.LogId}: {ex.Message}");
            }
        }
    }

    //Sync unsynced attendance logs in bulk
    //Sync unsynced attendance logs in bulk
    public async Task<int> SyncUnsyncedAttendanceLogsAsync(CancellationToken token)
    {
        const int maxBatchSize = 100;

        try
        {
            LogHelper.Log("Checking for unsynced attendance logs...");
            var unsyncedLogs = await _dbHelper.CheckUnsyncedAttendanceLogsAsync().ConfigureAwait(false);

            if (unsyncedLogs.Count == 0)
            {
                LogHelper.Log("No unsynced attendance logs found.");
                return 0;
            }

            var gmtSetting = _dbHelper.GetSetting("GMT");

            // Prepare items for batching
            var items = unsyncedLogs
                .Where(log => log.EnrollmentId > 0)                          // ← skip zero/invalid enrollment IDs
                .Select(log => new
                {
                    log.EnrollmentId,
                    CheckIn = log.CheckInTime != DateTime.MinValue ? CommonUtils.FormatWithGmt(log.CheckInTime, gmtSetting) : null,
                    CheckOut = (log.CheckOutTime != DateTime.MinValue && log.NeedsCheckOutSync) ? CommonUtils.FormatWithGmt(log.CheckOutTime, gmtSetting) : null,
                    Id = log.AttendanceId
                })
                .Where(x => x.CheckIn != null || x.CheckOut != null)
                .ToList();

            if (items.Count == 0)
            {
                LogHelper.Log("ℹ️ No valid attendance logs to sync after filtering invalid enrollment IDs.");
                return 0;
            }

            LogHelper.Log($"Found {items.Count} unsynced attendance logs formatted with GMT {gmtSetting}. Sending via bulk HTTP...");

            int totalBatchesProcessed = 0;

            // Ensure HttpHandler is ready AND wired to Processor
            if (_httpHandler == null)
            {
                if (_processor == null)
                {
                    _processor = new Processor(_dbHelper, null, _attendenceServerFunctions);
                }
                _httpHandler = new HttpHandler(_dbHelper, _processor);
            }

            for (int i = 0; i < items.Count; i += maxBatchSize)
            {
                if (token.IsCancellationRequested) break;

                var batch = items.Skip(i).Take(maxBatchSize)
                    .Select(x => {
                        long? longId = null;
                        if (long.TryParse(x.Id, out long parsedId)) longId = parsedId;
                        return (x.EnrollmentId, x.CheckIn, x.CheckOut, longId);
                    })
                    .ToList();

                LogHelper.Log($"➡ Sending bulk attendance batch {totalBatchesProcessed + 1} with {batch.Count} items");

                try
                {
                    // Use the wrapper method which handles passing the response to the Processor
                    await _httpHandler.PostAttendanceRecordsBulkAsync(batch, token).ConfigureAwait(false);

                    totalBatchesProcessed++;
                    LogHelper.Log($"✅ Batch {totalBatchesProcessed} processing triggered. Handled via Processor.");
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error sending bulk attendance batch: {ex.Message}");
                }
            }

            LogHelper.Log($"Completed batching {totalBatchesProcessed} groups of attendance sync.");
            return items.Count;
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error during attendance bulk log sync: {ex.Message}");
            return 0;
        }
    }


    private async Task LogAttendanceToServerAsync(object resultObj)
    {
        if (resultObj == null) return;

        try
        {
            var type = resultObj.GetType();
            var successProp = type.GetProperty("Success");
            if (successProp == null || !(bool)successProp.GetValue(resultObj, null)) return;

            int memberId = (int)type.GetProperty("MemberId").GetValue(resultObj, null);
            int enrollmentId = 0;
            var enrollmentProp = type.GetProperty("EnrollmentId");
            if (enrollmentProp != null)
                enrollmentId = (int)enrollmentProp.GetValue(resultObj, null);

            string action = (string)type.GetProperty("Action").GetValue(resultObj, null);
            DateTime eventTime = (DateTime)type.GetProperty("EventTime").GetValue(resultObj, null);

            if (enrollmentId <= 0)
            {
                LogHelper.Log($"[LogAttendanceToServerAsync] Cannot log to server: Invalid enrollment ID for Member={memberId}");
                return;
            }

            // Ensure HttpHandler is ready AND wired to Processor (do not use ctor without processor)
            if (_httpHandler == null)
            {
                // If _processor is not set for any reason, create it now
                if (_processor == null)
                {
                    _processor = new Processor(_dbHelper, null, _attendenceServerFunctions);
                }
                _httpHandler = new HttpHandler(_dbHelper, _processor);
            }

            // Read GMT offset and parse (e.g., "+05", "-03", "+05:30")
            var gmtSetting = _dbHelper.GetSetting("GMT");

            // Resolve attendance ID by action-specific timestamp under DB lock
            string resolvedId = null;
            await DatabaseQueueManager.EnterDbWriteLockAsync("ResolveAttendanceIdForMember", DbTaskPriority.High).ConfigureAwait(false);
            try
            {
                if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedId = _dbHelper.GetAttendanceIdForMember(memberId, eventTime, null);
                }
                else if (action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedId = _dbHelper.GetAttendanceIdForMember(memberId, null, eventTime);
                }
                else
                {
                    LogHelper.Log($"[LogAttendanceToServerAsync] Unsupported action '{action}'");
                    return;
                }
            }
            finally
            {
                DatabaseQueueManager.ReleaseDbWriteLock("ResolveAttendanceIdForMember");
            }

            if (string.IsNullOrEmpty(resolvedId))
            {
                LogHelper.Log($"[LogAttendanceToServerAsync] Cannot log to server: No attendance ID for Member={memberId} at {eventTime:yyyy-MM-dd HH:mm:ss}");
                return;
            }

            // Build strings with configured GMT offset (e.g., "yyyy-MM-dd HH:mm:ss.fff+05" or "+05:30")
            string checkInStr = null;
            string checkOutStr = null;

            if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
            {
                checkInStr = CommonUtils.FormatWithGmt(eventTime, gmtSetting);

                await _httpHandler.PostAttendanceRecordAsync(
                    enrollmentId,
                    checkinTime: checkInStr,
                    checkoutTime: null,
                    id: resolvedId
                ).ConfigureAwait(false);

                LogHelper.Log($"[LogAttendanceToServerAsync] CHECK-IN sent for Member={memberId} at {eventTime:yyyy-MM-dd HH:mm:ss} (GMT {gmtSetting})");
            }
            else if (action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
            {
                // Get paired check-in time under DB lock
                DateTime? checkInTime = null;
                await DatabaseQueueManager.EnterDbWriteLockAsync("GetPairedCheckInTime", DbTaskPriority.High).ConfigureAwait(false);
                try
                {
                    checkInTime = _dbHelper.GetPairedCheckInTime(memberId, eventTime);
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("GetPairedCheckInTime");
                }

                if (checkInTime.HasValue)
                {
                    checkInStr = CommonUtils.FormatWithGmt(checkInTime.Value, gmtSetting);
                }
                checkOutStr = CommonUtils.FormatWithGmt(eventTime, gmtSetting);

                await _httpHandler.PostAttendanceRecordAsync(
                    enrollmentId,
                    checkinTime: checkInStr,
                    checkoutTime: checkOutStr,
                    id: resolvedId
                ).ConfigureAwait(false);

                LogHelper.Log($"[LogAttendanceToServerAsync] CHECK-OUT sent for Member={memberId} with Check-In at {(checkInTime.HasValue ? checkInTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A")} and Check-Out at {eventTime:yyyy-MM-dd HH:mm:ss} (GMT {gmtSetting})");
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"[LogAttendanceToServerAsync] Error: {ex.Message}");
        }
    }


    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        LogHelper.Log("Attendance log sync service disposed");
    }
}
