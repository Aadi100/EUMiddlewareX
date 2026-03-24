using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static smart_attendance_middleware.Dashboard;

namespace smart_attendance_middleware.Classes
{
    /// <summary>
    /// A service responsible for periodic maintenance tasks including:
    /// - Calling UpdateSyncStatusLabels 10 seconds after each FetchDeviceSyncStatus run
    /// - Checking unsynced log entries and conditionally calling cuiButton5_Click
    /// - Clearing general logs using EmptyGeneralLogData based on timing conditions
    /// - Tracking clear time in settings to ensure it only clears once per approximately 48 hours
    /// - Processing pending jobs every 60 minutes
    /// - Syncing unsynced attendance logs every 10 minutes
    /// - Disabling inactive users every 8 hours (first run after initial 10-minute delay)
    /// - Fetching device sync status every 6 hours (first run immediately, then every 6 hours)
    /// - One-time cleanup of completed and synced attendance logs (Older than 2 Days) upon startup
    /// 
    /// Note: UpdateSyncStatusLabels is now scheduled to run once, 10 seconds after each successful
    /// execution of ExecuteFetchDeviceSyncStatusAsync(). A one-shot timer is used and disposed after firing.
    /// </summary>
    public class PeriodicMaintenanceService : IDisposable
    {
        private readonly int _initialDelay = 10 * 60 * 1000;   // 10 minutes in milliseconds
        private readonly int _clearLogCheckInterval = 9 * 60 * 60 * 1000; // Check every 9 hours for log clearing
        private readonly int _pendingJobsInterval = 60 * 60 * 1000; // 60 minutes in milliseconds
        private readonly int _unsyncedInterval = 10 * 60 * 1000; // 10 minutes in milliseconds
        private readonly int _disableInactiveUsersInterval = 8 * 60 * 60 * 1000; // 8 hours in milliseconds
        private readonly int _fetchDeviceSyncStatusInterval = 6 * 60 * 60 * 1000; // 6 hours in milliseconds
        private readonly int _delayAfterFetchForUpdate = 10 * 1000; // 10 sec

        private bool _syncInProgress = false;
        private Timer _pendingJobsTimer;

        private Timer _updateTimer; // now used as a one-shot timer scheduled after fetch
        private Timer _logClearTimer;
        private Timer _unsyncedLogsTimer;
        private Timer _disableInactiveUsersTimer;
        private Timer _fetchDeviceSyncStatusTimer;

        private readonly Dashboard _dashboard;
        private readonly DatabaseHelper _dbHelper;
        private WebSocketManager _wsManager;

        private bool _disposed = false;
        private readonly object _lockObject = new object();

        // Attendance sync helper and cancellation for periodic unsynced sync
        private AttendanceLogSyncService _attendanceSyncService;
        private CancellationTokenSource _unsyncedSyncCts;
        private StaffAttendanceLogSyncService _staffAttendanceSyncService;

        // Setting keys for tracking clear times
        private const string LAST_CLEAR_TIME_KEY = "LastLogClearTime";
        private const string CLEAR_INTERVAL_HOURS = "47.9997222222"; // 47 hours, 59 minutes, 59 seconds in hours


        public PeriodicMaintenanceService(Dashboard dashboard)
        {
            try
            {
                _dashboard = dashboard ?? throw new ArgumentNullException(nameof(dashboard));
                _dbHelper = new DatabaseHelper();

                LogHelper.Log("🔧 PeriodicMaintenanceService initialized");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error initializing PeriodicMaintenanceService: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Starts the periodic maintenance service with initial 15-minute delay
        /// </summary>
        private readonly TaskCompletionSource<bool> _startedTcs = new TaskCompletionSource<bool>();
        public Task Started => _startedTcs.Task;

        public void Start()
        {
            if (_disposed)
            {
                LogHelper.Log("❌ Cannot start PeriodicMaintenanceService - already disposed");
                return;
            }

            lock (_lockObject)
            {
                if (_updateTimer != null || _logClearTimer != null || _pendingJobsTimer != null || _unsyncedLogsTimer != null || _disableInactiveUsersTimer != null || _fetchDeviceSyncStatusTimer != null)
                {
                    LogHelper.Log("⚠️ PeriodicMaintenanceService already started");
                    return;
                }

                LogHelper.Log("🚀 Starting PeriodicMaintenanceService with 15-minute initial delay");

                // Timers created => service is considered started
                if (!_startedTcs.Task.IsCompleted)
                {
                    _startedTcs.TrySetResult(true);
                }

                // Trigger a one-time cleanup of old synced logs
                Task.Run(async () => await ExecuteOneTimeSyncedLogCleanup());

                // Start log clear timer with initial 15-minute delay, then check every hour
                _logClearTimer = new Timer(
                    callback: async _ => await ExecuteLogClearOperation(),
                    state: null,
                    dueTime: _initialDelay,
                    period: _clearLogCheckInterval
                );

                // Start pending jobs timer with initial 15-minute delay, then every 60 minutes
                _pendingJobsTimer = new Timer(
                    callback: async _ => await ExecutePendingJobsOperation(),
                    state: null,
                    dueTime: _initialDelay,
                    period: _pendingJobsInterval
                );

                // Start unsynced attendance logs sync timer (every configured interval) with initial delay
                _unsyncedLogsTimer = new Timer(
                    callback: async _ => await ExecuteSyncUnsyncedAttendanceLogsAsync(),
                    state: null,
                    dueTime: _initialDelay,
                    period: _unsyncedInterval
                );

                // Start disable inactive users timer: first run after initial delay, then every 12 hours
                _disableInactiveUsersTimer = new Timer(
                    callback: async _ => await ExecuteDisableInactiveUsersOperation(),
                    state: null,
                    dueTime: _initialDelay,
                    period: _disableInactiveUsersInterval
                );

                _fetchDeviceSyncStatusTimer = new Timer(
                    callback: async _ => await ExecuteFetchDeviceSyncStatusAsync(),
                    state: null,
                    dueTime: 0,
                    period: _fetchDeviceSyncStatusInterval
                );

                LogHelper.Log($"⏰ Timers started - LogClear check: every {_clearLogCheckInterval / 60000} minutes, PendingJobs: every {_pendingJobsInterval / 60000} minutes, UnsyncedAttendanceSync: every {_unsyncedInterval / 60000} minutes, DisableInactiveUsers: every {_disableInactiveUsersInterval / 3600000} hours, FetchDeviceSyncStatus: every {_fetchDeviceSyncStatusInterval / 60000} minutes. UpdateSyncStatusLabels will be scheduled 10 minutes after each fetch.");
            }
        }

        /// <summary>
        /// Stops the periodic maintenance service
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (_updateTimer != null)
                {
                    _updateTimer.Change(Timeout.Infinite, 0);
                    _updateTimer.Dispose();
                    _updateTimer = null;
                }

                if (_logClearTimer != null)
                {
                    _logClearTimer.Change(Timeout.Infinite, 0);
                    _logClearTimer.Dispose();
                    _logClearTimer = null;
                }

                if (_pendingJobsTimer != null)
                {
                    _pendingJobsTimer.Change(Timeout.Infinite, 0);
                    _pendingJobsTimer.Dispose();
                    _pendingJobsTimer = null;
                }

                if (_unsyncedLogsTimer != null)
                {
                    _unsyncedLogsTimer.Change(Timeout.Infinite, 0);
                    _unsyncedLogsTimer.Dispose();
                    _unsyncedLogsTimer = null;
                }

                if (_disableInactiveUsersTimer != null)
                {
                    _disableInactiveUsersTimer.Change(Timeout.Infinite, 0);
                    _disableInactiveUsersTimer.Dispose();
                    _disableInactiveUsersTimer = null;
                }

                if (_fetchDeviceSyncStatusTimer != null)
                {
                    _fetchDeviceSyncStatusTimer.Change(Timeout.Infinite, 0);
                    _fetchDeviceSyncStatusTimer.Dispose();
                    _fetchDeviceSyncStatusTimer = null;
                }

                // Cancel and dispose of the unsynced sync cancellation token and dispose sync service
                try
                {
                    if (_unsyncedSyncCts != null)
                    {
                        _unsyncedSyncCts.Cancel();
                        _unsyncedSyncCts.Dispose();
                        _unsyncedSyncCts = null;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error cancelling unsynced sync CTS: {ex.Message}");
                }

                try
                {
                    if (_attendanceSyncService != null)
                    {
                        _attendanceSyncService.Dispose();
                        _attendanceSyncService = null;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error disposing AttendanceLogSyncService: {ex.Message}");
                }

                LogHelper.Log("🛑 PeriodicMaintenanceService stopped");
            }
        }
       
        private async Task ExecuteUpdateSyncStatusLabels()
        {
            try
            {
                LogHelper.Log("🔄 Executing periodic UpdateSyncStatusLabels...");

                if (_dashboard.IsDisposed)
                {
                    LogHelper.Log("⚠️ Dashboard is disposed, skipping UpdateSyncStatusLabels");
                    return;
                }

                // Execute on UI thread since this updates UI elements
                if (_dashboard.InvokeRequired)
                {
                    await Task.Run(() =>
                    {
                        _dashboard.Invoke(new Action(async () =>
                        {
                            try
                            {
                                await _dashboard.RefreshDeviceSyncStatusAsync();
                                LogHelper.Log("✅ UpdateSyncStatusLabels completed successfully");
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Log($"❌ Error in UpdateSyncStatusLabels: {ex.Message}");
                            }
                        }));
                    });
                }
                else
                {
                    await _dashboard.RefreshDeviceSyncStatusAsync();
                    LogHelper.Log("✅ UpdateSyncStatusLabels completed successfully");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error executing UpdateSyncStatusLabels: {ex.Message}");
            }
        }

        /// <summary>
        /// Schedules a one-shot UpdateSyncStatusLabels execution to run after the configured delay following a fetch.
        /// The created timer is disposed after firing.
        /// </summary>
        private void ScheduleUpdateAfterFetch()
        {
            try
            {
                // If an update timer is already scheduled, dispose it and replace with the new schedule
                if (_updateTimer != null)
                {
                    try
                    {
                        _updateTimer.Change(Timeout.Infinite, 0);
                        _updateTimer.Dispose();
                    }
                    catch { }
                    _updateTimer = null;
                }

                // Create a one-shot timer that will invoke ExecuteUpdateSyncStatusLabels after the delay
                Timer t = null;
                t = new Timer(async _ =>
                {
                    try
                    {
                        await ExecuteUpdateSyncStatusLabels();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Error executing scheduled UpdateSyncStatusLabels: {ex.Message}");
                    }
                    finally
                    {
                        try
                        {
                            t?.Change(Timeout.Infinite, 0);
                            t?.Dispose();
                        }
                        catch { }
                        // Clear the reference if it's the same timer instance
                        if (_updateTimer == t) _updateTimer = null;
                    }
                }, null, _delayAfterFetchForUpdate, Timeout.Infinite);

                _updateTimer = t;
                LogHelper.Log($"⏱️ Scheduled UpdateSyncStatusLabels to run in {_delayAfterFetchForUpdate / 1000} seconds after fetch.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error scheduling UpdateAfterFetch: {ex.Message}");
            }
        }

        /// <summary>
        /// Periodically calls AttendanceLogSyncService.SyncUnsyncedAttendanceLogsAsync every configured interval
        /// </summary>
        private async Task ExecuteSyncUnsyncedAttendanceLogsAsync()
        {
            if (_syncInProgress)
            {
                LogHelper.Log("⚠️ Sync already in progress, skipping this cycle.");
                return;
            }

            _syncInProgress = true;
            try
            {
                LogHelper.Log("🔁 Executing periodic SyncUnsyncedAttendanceLogs (Members & Staff)...");

                if (_dashboard.IsDisposed)
                {
                    LogHelper.Log("⚠️ Dashboard is disposed, skipping SyncUnsyncedAttendanceLogs");
                    return;
                }

                // Initialize cancellation token if needed
                if (_unsyncedSyncCts == null || _unsyncedSyncCts.IsCancellationRequested)
                {
                    _unsyncedSyncCts = new CancellationTokenSource();
                }
                var token = _unsyncedSyncCts.Token;

                // 1) MEMBER SYNC
                if (_attendanceSyncService == null)
                {
                    try
                    {
                        // Constructor now takes no arguments based on previous refactoring
                        _attendanceSyncService = new AttendanceLogSyncService();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Error creating AttendanceLogSyncService: {ex.Message}");
                    }
                }

                if (_attendanceSyncService != null)
                {
                    int syncedCount = await _attendanceSyncService.SyncUnsyncedAttendanceLogsAsync(token).ConfigureAwait(false);
                    LogHelper.Log($"✅ SyncUnsyncedAttendanceLogs (Members) completed - {syncedCount} records synced");
                }

                if (token.IsCancellationRequested) return;

                // 2) STAFF SYNC
                if (_staffAttendanceSyncService == null)
                {
                    try
                    {
                        // Constructor now takes no arguments based on previous refactoring
                        _staffAttendanceSyncService = new StaffAttendanceLogSyncService();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Error creating StaffAttendanceLogSyncService: {ex.Message}");
                    }
                }

                if (_staffAttendanceSyncService != null)
                {
                    int staffSyncedCount = await _staffAttendanceSyncService.SyncUnsyncedStaffAttendanceLogsAsync(token).ConfigureAwait(false);
                    LogHelper.Log($"✅ SyncUnsyncedStaffAttendanceLogs (Staff) completed - {staffSyncedCount} records synced");
                }
            }
            catch (TaskCanceledException)
            {
                LogHelper.Log("⏹ SyncUnsyncedAttendanceLogs cancelled");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error executing SyncUnsyncedAttendanceLogs: {ex.Message}");
            }
            finally
            {
                _syncInProgress = false;
            }
        }


        /// <summary>
        /// Executes the log clear operation if conditions are met
        /// </summary>
        private async Task ExecuteLogClearOperation()
        {
            try
            {
                LogHelper.Log("🧹 Checking if log clearing is needed...");

                if (!ShouldClearLogs())
                {
                    LogHelper.Log("⏭️ Log clearing not needed at this time");
                    return;
                }

                LogHelper.Log("🗑️ Starting general log clear operation for all devices...");

                // Regular Devices (network I/O - no DB lock)
                var devices = _dbHelper.LoadDevices();
                if (devices == null || devices.Count == 0)
                {
                    LogHelper.Log("⚠️ No devices found for log clearing");
                }
                else
                {
                    int successCount = 0;
                    int failureCount = 0;

                    foreach (var device in devices)
                    {
                        try
                        {
                            LogHelper.Log($"🧹 Clearing logs for device {device.DeviceId} ({device.DeviceIP})...");

                            bool success = await DeviceService.EmptyGeneralLogData(
                                device.DeviceIP,
                                device.Port,
                                device.MachineNo,
                                device.Password
                            );

                            if (success)
                            {
                                successCount++;
                                LogHelper.Log($"✅ Successfully cleared logs for device {device.DeviceId}");
                            }
                            else
                            {
                                failureCount++;
                                LogHelper.Log($"❌ Failed to clear logs for device {device.DeviceId}");
                            }

                            await Task.Delay(2000);
                        }
                        catch (Exception deviceEx)
                        {
                            failureCount++;
                            LogHelper.Log($"❌ Exception clearing logs for device {device.DeviceId}: {deviceEx.Message}");
                        }
                    }
                    LogHelper.Log($"🎯 Log clear operation completed for regular devices - Success: {successCount}, Failed: {failureCount}");
                }

                // Staff Devices (network I/O - no DB lock)
                var staffDevices = _dbHelper.LoadStaffDevices();
                if (staffDevices == null || staffDevices.Count == 0)
                {
                    LogHelper.Log("⚠️ No staff devices found for log clearing");
                }
                else
                {
                    int staffSuccessCount = 0;
                    int staffFailureCount = 0;

                    foreach (var device in staffDevices)
                    {
                        try
                        {
                            LogHelper.Log($"🧹 Clearing logs for staff device {device.StaffDeviceId}...");

                            bool success = await DeviceService.EmptyGeneralLogData(
                                device.DeviceIP,
                                device.Port,
                                device.MachineNo,
                                device.Password
                            );

                            if (success)
                            {
                                staffSuccessCount++;
                                LogHelper.Log($"✅ Successfully cleared logs for staff device {device.StaffDeviceId}");
                            }
                            else
                            {
                                staffFailureCount++;
                                LogHelper.Log($"❌ Failed to clear logs for staff device {device.StaffDeviceId}");
                            }

                            await Task.Delay(2000);
                        }
                        catch (Exception deviceEx)
                        {
                            staffFailureCount++;
                            LogHelper.Log($"❌ Exception clearing logs for staff device {device.StaffDeviceId}: {deviceEx.Message}");
                        }
                    }
                    LogHelper.Log($"🎯 Log clear operation completed for staff devices - Success: {staffSuccessCount}, Failed: {staffFailureCount}");
                }

                // Database deletes under short lock
                try
                {
                    await DatabaseQueueManager.EnterDbWriteLockAsync("LogClearOperation_DBDelete", DbTaskPriority.Low);
                    try
                    {
                        _dbHelper.DeleteAllGLogs();
                        _dbHelper.DeleteAllStaffGLogs(); // if implemented
                    }
                    finally
                    {
                        DatabaseQueueManager.ReleaseDbWriteLock("LogClearOperation_DBDelete");
                    }
                    LogHelper.Log("✅ Successfully cleared GLog and StaffGLog data from the database");
                }
                catch (Exception dbEx)
                {
                    LogHelper.Log($"❌ Error clearing GLog and StaffGLog data from the database: {dbEx.Message}");
                }

                // Update the last clear time only if there were devices to clear
                if ((devices != null && devices.Count > 0) || (staffDevices != null && staffDevices.Count > 0))
                {
                    try
                    {
                        await DatabaseQueueManager.EnterDbWriteLockAsync("LogClearOperation_UpdateLastClearTime", DbTaskPriority.Low);
                        try
                        {
                            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            _dbHelper.SaveSetting(LAST_CLEAR_TIME_KEY, currentTime);
                            LogHelper.Log($"💾 Updated last clear time: {currentTime}");
                        }
                        finally
                        {
                            DatabaseQueueManager.ReleaseDbWriteLock("LogClearOperation_UpdateLastClearTime");
                        }
                        LogHelper.Log("🎯 Log clear operation completed");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Error updating last clear time: {ex.Message}");
                    }
                }
                else
                {
                    LogHelper.Log($"⚠️ Log clear operation failed for all devices");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error executing log clear operation: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines if logs should be cleared based on timing conditions
        /// </summary>
        private bool ShouldClearLogs()
        {
            try
            {
                string lastClearTimeStr = _dbHelper.GetSetting(LAST_CLEAR_TIME_KEY);

                if (string.IsNullOrEmpty(lastClearTimeStr))
                {
                    LogHelper.Log("📅 No previous clear time found - logs will be cleared");
                    return true;
                }

                if (DateTime.TryParse(lastClearTimeStr, out DateTime lastClearTime))
                {
                    TimeSpan timeSinceLastClear = DateTime.Now - lastClearTime;
                    double hoursSinceLastClear = timeSinceLastClear.TotalHours;
                    double requiredHours = double.Parse(CLEAR_INTERVAL_HOURS);

                    LogHelper.Log($"📊 Time since last clear: {hoursSinceLastClear:F2} hours (required: {requiredHours} hours)");

                    if (hoursSinceLastClear >= requiredHours)
                    {
                        LogHelper.Log("✅ Clear interval exceeded - logs will be cleared");
                        return true;
                    }
                    else
                    {
                        double hoursRemaining = requiredHours - hoursSinceLastClear;
                        LogHelper.Log($"⏰ Clear interval not reached - {hoursRemaining:F02} hours remaining");
                        return false;
                    }
                }
                else
                {
                    LogHelper.Log("⚠️ Invalid last clear time format - logs will be cleared");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error checking clear conditions: {ex.Message}");
                return false; // Conservative approach - don't clear if there's an error
            }
        }

        /// <summary>
        /// Updates the last clear time in settings
        /// </summary>
        private void UpdateLastClearTime()
        {
            try
            {
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _dbHelper.SaveSetting(LAST_CLEAR_TIME_KEY, currentTime);
                LogHelper.Log($"💾 Updated last clear time: {currentTime}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error updating last clear time: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the last clear time from settings
        /// </summary>
        public DateTime? GetLastClearTime()
        {
            try
            {
                string lastClearTimeStr = _dbHelper.GetSetting(LAST_CLEAR_TIME_KEY);

                if (string.IsNullOrEmpty(lastClearTimeStr))
                    return null;

                if (DateTime.TryParse(lastClearTimeStr, out DateTime lastClearTime))
                    return lastClearTime;

                return null;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error getting last clear time: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Executes the pending jobs operation in the database.
        /// </summary>
        private async Task ExecutePendingJobsOperation()
        {
            try
            {
                LogHelper.Log("🔁 Executing periodic pending jobs processing...");

                if (_dashboard.IsDisposed)
                {
                    LogHelper.Log("⚠️ Dashboard is disposed, skipping pending jobs processing");
                    return;
                }

                var deviceFunctions = new DeviceFunctions();
                await deviceFunctions.ProcessPendingJobsAsync();

                LogHelper.Log("✅ Periodic pending jobs processing completed");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error executing pending jobs: {ex.Message}");
            }
           
        }

        /// <summary>
        /// Executes DisableInactiveUsers every 12 hours (first run after initial delay)
        /// </summary>
        private async Task ExecuteDisableInactiveUsersOperation()
        {
            try
            {
                LogHelper.Log("🛠️ Executing periodic DisableInactiveUsers...");

                if (_dashboard.IsDisposed)
                {
                    LogHelper.Log("⚠️ Dashboard is disposed, skipping DisableInactiveUsers");
                    return;
                }

                var deviceFunctions = new DeviceFunctions();
                try
                {
                    await deviceFunctions.DisableInactiveUsers();
                    LogHelper.Log("✅ DisableInactiveUsers completed successfully");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Error executing DisableInactiveUsers: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Fatal error in ExecuteDisableInactiveUsersOperation: {ex.Message}");
            }

        }

        /// <summary>
        /// Performs only database work and returns computed sync status values.
        /// No UI updates are performed here.
        /// </summary>
        public async Task<SyncStatusResult> FetchDeviceSyncStatusAsync()
        {
            try
            {
                LogHelper.Log("🔄 Fetching device sync status (DB-only)...");

                var devices = _dbHelper.LoadDevices();
                if (devices == null || devices.Count == 0)
                {
                    LogHelper.Log("❌ No devices found for sync status check (DB-only)");
                    return new SyncStatusResult { MembersSynced = 0, PendingBiometricJobs = 0, MissingFingerprints = 0 };
                }

                int completelySyncedMembers = _dbHelper.GetFullySyncedMembersCountAgainstAllDevices(onlyActiveDevices: true);
                LogHelper.Log($"✅ {completelySyncedMembers} members are completely synced across all devices (DB-only)");

                var unsyncedLogEntries = await _dbHelper.GetUnsyncedLogEntriesAsync().ConfigureAwait(false);
                int pendingBiometricJobs = 0;
                foreach (var entry in unsyncedLogEntries)
                {
                    foreach (var biometric in entry.BiometricData)
                    {
                        if (!entry.SyncStatus.ContainsKey(biometric.Key) || !entry.SyncStatus[biometric.Key])
                            pendingBiometricJobs++;
                    }
                }
                LogHelper.Log($"⏳ {pendingBiometricJobs} pending biometric sync jobs found (DB-only)");

                var missingFingerprintMembers = await _dbHelper.GetMembersWithMissingFingerprintsAsync().ConfigureAwait(false);
                int missingFingerprints = missingFingerprintMembers?.Count ?? 0;
                LogHelper.Log($"❌ {missingFingerprints} members with missing fingerprints (DB-only)");

                var result = new SyncStatusResult
                {
                    MembersSynced = completelySyncedMembers,
                    PendingBiometricJobs = pendingBiometricJobs,
                    MissingFingerprints = missingFingerprints
                };

                // Write settings under short lock
                await DatabaseQueueManager.EnterDbWriteLockAsync("FetchDeviceSyncStatus_Save", DbTaskPriority.Low);
                try
                {
                    _dbHelper.SaveSetting("MembersSynced", completelySyncedMembers.ToString());
                    _dbHelper.SaveSetting("PendingBiometricJobs", pendingBiometricJobs.ToString());
                    _dbHelper.SaveSetting("MissingFingerprints", missingFingerprints.ToString());
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("FetchDeviceSyncStatus_Save");
                }

                AppSession.SyncStatus = result;
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error fetching device sync status (DB-only): {ex.Message}");
                return new SyncStatusResult { MembersSynced = 0, PendingBiometricJobs = 0, MissingFingerprints = 0 };
            }
        }

        /// <summary>
        /// Executes the fetch device sync status operation periodically
        /// </summary>

        public async Task ExecuteFetchDeviceSyncStatusAsync()
        {
            try
            {
                LogHelper.Log("🔄 Executing periodic FetchDeviceSyncStatus...");

                await FetchDeviceSyncStatusAsync();

                LogHelper.Log("✅ Periodic FetchDeviceSyncStatus completed");

                // Schedule UpdateSyncStatusLabels to run 10 minutes after this fetch completes
                if (!_disposed && (_dashboard == null || !_dashboard.IsDisposed))
                {
                    ScheduleUpdateAfterFetch();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error executing FetchDeviceSyncStatus: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _disposed = true;
                LogHelper.Log("🗑️ PeriodicMaintenanceService disposed");
            }
        }

        /// <summary>
        /// Executes a one-time cleanup of completed and synced attendance logs older than 2 days.
        /// </summary>
        private async Task ExecuteOneTimeSyncedLogCleanup()
        {
            try
            {
                LogHelper.Log("🧹 Executing one-time cleanup of synced attendance logs (Older than 2 days)...");

                await DatabaseQueueManager.EnterDbWriteLockAsync("OneTimeSyncedLogCleanup", DbTaskPriority.Low);
                try
                {
                    // Delete logs older than 2 days
                    int deletedCount = _dbHelper.DeleteOldSyncedAttendanceLogs(2);

                    if (deletedCount > 0)
                        LogHelper.Log($"✅ One-time log cleanup completed - Removed {deletedCount} completed and synced records");
                    else
                        LogHelper.Log("Keep syntax as of others in this class - No old synced attendance logs found to clean");
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("OneTimeSyncedLogCleanup");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error during one-time synced log cleanup: {ex.Message}");
            }
        }
    }
}
