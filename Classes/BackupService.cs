using System;
using System.Threading;
using System.Threading.Tasks;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware.Services
{
    public class BackupService : IDisposable
    {
        private static readonly TimeSpan BackupInterval = TimeSpan.FromHours(5);
        //private static readonly TimeSpan BackupInterval = TimeSpan.FromMinutes(5); // For testing purposes, set to 5 minutes. Change to 3 hours for production.


        private readonly DatabaseHelper _db;
        private Timer _timer;
        private readonly object _lock = new object();
        private bool _backingUp;

        // Flags to control backup destination
        private bool _enableEmailBackup;
        private bool _enableDriveBackup;

        public BackupService(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            RefreshSettings();
        }

        public void Start()
        {
            if (_timer != null)
            {
                LogHelper.Log("BackupService already started; ignoring Start().");
                return;
            }

            RefreshSettings();
            _timer = new Timer(BackupCallback, null, BackupInterval, BackupInterval);
            LogHelper.Log($"BackupService started (Interval: {BackupInterval.TotalHours}h). Config: Email={_enableEmailBackup}, Drive={_enableDriveBackup}");
        }

        private void RefreshSettings()
        {
            try
            {
                string emailSetting = _db.GetSetting("EnableEmailBackup");
                string driveSetting = _db.GetSetting("EnableGoogleDriveBackup");

                // Default to false if settings are null, empty, or not "true"
                _enableEmailBackup = !string.IsNullOrEmpty(emailSetting) && emailSetting.Trim().ToLower() == "true";
                _enableDriveBackup = !string.IsNullOrEmpty(driveSetting) && driveSetting.Trim().ToLower() == "true";
            }
            catch (Exception ex)
            {
                LogHelper.Log($"BackupService: Error refreshing settings: {ex.Message}");
                _enableEmailBackup = false;
                _enableDriveBackup = false;
            }
        }

        public void Stop()
        {
            if (_timer == null) return;

            _timer.Change(Timeout.Infinite, 0);
            _timer.Dispose();
            _timer = null;

            LogHelper.Log("BackupService stopped.");
        }

        private void BackupCallback(object state)
        {
            if (_backingUp)
            {
                LogHelper.Log("BackupService: backup skipped (previous cycle still running).");
                return;
            }

            lock (_lock)
            {
                if (_backingUp) return;
                _backingUp = true;
            }

            try
            {
                // Refresh settings per cycle to pick up any database changes
                RefreshSettings();

                LogHelper.Log("BackupService: starting backup cycle...");
                PerformBackupAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"BackupService: background job failed: {ex.Message}");
            }
            finally
            {
                lock (_lock) { _backingUp = false; }
            }
        }

        private async Task PerformBackupAsync()
        {
            if (!_enableEmailBackup && !_enableDriveBackup)
            {
                LogHelper.Log("BackupService: No backup methods enabled (Email and Drive are both disabled). Skipping cycle.");
                return;
            }

            try
            {
                if (_enableEmailBackup)
                {
                    await CommonUtils.CreateAndEmailDbLogsAndGLogsAsync(_db).ConfigureAwait(false);
                }

                if (_enableDriveBackup)
                {
                    // The folder ID can also be retrieved via _db.GetSetting
                    string folderId = _db.GetSetting("GoogleDriveFolderId");

                    // Pass the database instance to the upload method
                    await CommonUtils.CreateAndUploadBackupToDriveAsync(_db).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"BackupService error: {ex.Message}");
            }
        }

        /// <summary>
        /// Triggers a one-time backup after the specified delay.
        /// </summary>
        /// <param name="delay">Delay before starting the backup.</param>
        public void TriggerBackupAfterDelay(TimeSpan delay)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay).ConfigureAwait(false);
                LogHelper.Log($"BackupService: Triggering initial backup after {delay.TotalSeconds}s delay...");
                await PerformBackupAsync().ConfigureAwait(false);
            });
        }

        public void Dispose() => Stop();
    }
}