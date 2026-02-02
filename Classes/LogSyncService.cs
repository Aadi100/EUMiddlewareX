using System;
using System.Threading;
using System.Threading.Tasks;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware.Services
{
    public class LogSyncService
    {
        private readonly DeviceFunctions _deviceFunctions;
        private bool _isRunning = false;
        private readonly object _lock = new object();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public LogSyncService(DeviceFunctions deviceFunctions)
        {
            _deviceFunctions = deviceFunctions;
        }

        public void Start()
        {
            // Run the task once in the background.
            Task.Run(async () => await RunAsync(), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            // Cancel the task if it's running and dispose of the source.
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        private async Task RunAsync()
        {
            lock (_lock)
            {
                if (_isRunning) return;
                _isRunning = true;
            }

            try
            {
                LogHelper.Log("LogSyncService: Starting one-time check for unsynced logs...");

                // This is a DB read; no global write lock required.
                bool hasUnsyncedLogs = await _deviceFunctions._dbHelper.CheckForUnsyncedLogs();

                if (hasUnsyncedLogs)
                {
                    LogHelper.Log("LogSyncService: Unsynced logs found. Running workflow...");
                    // ProcessUserWorkflowAsync manages its own DB locks internally.
                    await _deviceFunctions.ProcessUserWorkflowAsync();
                }
                else
                {
                    LogHelper.Log("LogSyncService: No unsynced logs found.");
                }
            }
            catch (TaskCanceledException)
            {
                LogHelper.Log("LogSyncService: Task was canceled.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"LogSyncService: Error - {ex.Message}");
            }
            finally
            {
                _isRunning = false;
                LogHelper.Log("LogSyncService: One-time check finished.");
            }
        }

    }
}