using smart_attendance_middleware.Classes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace smart_attendance_middleware.Services
{
    public class EmailBackupService : IDisposable
    {
        //private static readonly TimeSpan BackupInterval = TimeSpan.FromMinutes(10); // Option 1: 10 minute
        private static readonly TimeSpan BackupInterval = TimeSpan.FromHours(3); // Option 2: 3 hours

        private readonly DatabaseHelper _db;
        private Timer _timer;
        private readonly object _lock = new object();
        private bool _sending;

        public EmailBackupService(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public void Start()
        {
            if (_timer != null)
            {
                LogHelper.Log("EmailBackupService already started; ignoring Start().");
                return;
            }

            _timer = new Timer(SendCallback, null, BackupInterval, BackupInterval);
            LogHelper.Log($"EmailBackupService started (every {FormatInterval(BackupInterval)}).");
        }

        public void Stop()
        {
            if (_timer == null)
            {
                LogHelper.Log("EmailBackupService already stopped; ignoring Stop().");
                return;
            }

            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
            _timer = null;

            LogHelper.Log("EmailBackupService stopped.");
        }

        private void SendCallback(object state)
        {
            if (_sending)
            {
                LogHelper.Log("EmailBackupService: send skipped (previous send still running).");
                return;
            }

            lock (_lock)
            {
                if (_sending)
                {
                    LogHelper.Log("EmailBackupService: send skipped (previous send still running).");
                    return;
                }

                _sending = true;
            }

            var startedAt = DateTime.Now;

            try
            {
                LogHelper.Log("EmailBackupService: starting backup bundle email job...");
                SendBackupEmailAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"EmailBackupService: backup job failed: {ex.Message}");
            }
            finally
            {
                var elapsed = DateTime.Now - startedAt;
                LogHelper.Log($"EmailBackupService: backup job finished in {elapsed.TotalSeconds:F1}s.");
                _sending = false;
            }
        }

        private static string FormatInterval(TimeSpan interval)
        {
            if (interval.TotalMinutes < 60)
                return string.Format("{0:F0} min", interval.TotalMinutes);

            return string.Format("{0:F0} h", interval.TotalHours);
        }

        private async Task SendBackupEmailAsync()
        {
            try
            {
                bool ok = await CommonUtils.CreateAndEmailDbLogsAndGLogsAsync(_db).ConfigureAwait(false);

                if (ok)
                    LogHelper.Log("EmailBackupService: backup bundle email sent successfully.");
                else
                    LogHelper.Log("EmailBackupService: backup bundle email not sent (bundle creation/SMTP config failed).");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"EmailBackupService: exception during CreateAndEmailDbLogsAndGLogsAsync: {ex.Message}");
                throw;
            }
        }

        public void Dispose() => Stop();
    }
}