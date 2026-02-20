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

        // Signal that the one-time initial run has completed
        private readonly TaskCompletionSource<bool> _startedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task Started => _startedTcs.Task;

        public LogSyncService(DeviceFunctions deviceFunctions)
        {
            _deviceFunctions = deviceFunctions;
        }

        public void Start()
        {
            Task.Run(async () => await RunAsync(), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
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

                bool hasUnsyncedLogs = await _deviceFunctions._dbHelper.CheckForUnsyncedLogs();

                if (hasUnsyncedLogs)
                {
                    LogHelper.Log("LogSyncService: Unsynced logs found. Running workflow...");
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

                // Signal callers that the initial one-shot run has completed.
                // Use TrySetResult so multiple calls/start attempts don't throw.
                if (!_startedTcs.Task.IsCompleted)
                {
                    _startedTcs.TrySetResult(true);
                }
            }
        }
    }
}