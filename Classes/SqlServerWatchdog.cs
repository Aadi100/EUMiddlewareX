using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace smart_attendance_middleware.Classes
{
    /// <summary>
    /// Checks the SQL Server Windows service on startup and monitors it at runtime,
    /// restarting automatically if it stops. Requires the app to run as Administrator.
    /// </summary>
    public static class SqlServerWatchdog
    {
        // Common SQL Server service names — first one found on the machine wins
        private static readonly string[] KnownServiceNames =
        {
            "MSSQLSERVER",       // default instance
            "MSSQL$SQLEXPRESS",  // SQL Server Express
        };

        private static string _serviceName;
        private static CancellationTokenSource _cts;
        private static Task _monitorTask;

        private const int CheckIntervalMs    = 30_000; // 30 s between checks
        private const int RestartTimeoutMs   = 60_000; // max 60 s to reach Running
        private const int MaxRestartAttempts = 3;

        /// <summary>Fired with a short status string whenever the service state changes.</summary>
        public static event Action<string> StatusChanged;

        /// <summary>Fired when all restart attempts have been exhausted.</summary>
        public static event Action<string> RestartFailed;

        // ── Startup ────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the SQL Server service, ensures it is Running, and starts it if not.
        /// Returns true when the service is Running on exit.
        /// Call this before InitializeDatabase().
        /// </summary>
        public static bool EnsureRunningOnStartup()
        {
            _serviceName = ResolveServiceName();
            if (_serviceName == null)
            {
                LogHelper.Log("[SqlServerWatchdog] No SQL Server service found on this machine.");
                return false;
            }

            LogHelper.Log($"[SqlServerWatchdog] Detected service: {_serviceName}");

            try
            {
                using (var sc = new ServiceController(_serviceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        LogHelper.Log("[SqlServerWatchdog] SQL Server is already running.");
                        StatusChanged?.Invoke("running");
                        return true;
                    }

                    LogHelper.Log($"[SqlServerWatchdog] SQL Server status is '{sc.Status}'. Starting…");
                    StatusChanged?.Invoke($"starting (was {sc.Status})");

                    // Wait out any pending transition first
                    if (sc.Status == ServiceControllerStatus.StopPending ||
                        sc.Status == ServiceControllerStatus.StartPending)
                    {
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
                    }

                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromMilliseconds(RestartTimeoutMs));

                    bool ok = sc.Status == ServiceControllerStatus.Running;
                    LogHelper.Log(ok
                        ? "[SqlServerWatchdog] SQL Server started successfully."
                        : "[SqlServerWatchdog] SQL Server did NOT reach Running state.");
                    StatusChanged?.Invoke(ok ? "running" : "start-failed");
                    return ok;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[SqlServerWatchdog] Startup check error: {ex.Message}");
                StatusChanged?.Invoke("error");
                return false;
            }
        }

        // ── Runtime monitor ────────────────────────────────────────────────

        /// <summary>Starts the background watchdog loop. Safe to call multiple times.</summary>
        public static void StartMonitor()
        {
            if (_monitorTask != null && !_monitorTask.IsCompleted)
                return;

            _cts = new CancellationTokenSource();
            _monitorTask = Task.Run(() => MonitorLoop(_cts.Token));
            LogHelper.Log("[SqlServerWatchdog] Background monitor started.");
        }

        /// <summary>Stops the background watchdog loop.</summary>
        public static void StopMonitor()
        {
            _cts?.Cancel();
            LogHelper.Log("[SqlServerWatchdog] Background monitor stopped.");
        }

        // ── Internals ──────────────────────────────────────────────────────

        private static async Task MonitorLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try { await Task.Delay(CheckIntervalMs, ct).ConfigureAwait(false); }
                catch (TaskCanceledException) { break; }

                if (_serviceName == null)
                    _serviceName = ResolveServiceName();

                if (_serviceName == null)
                    continue;

                try
                {
                    using (var sc = new ServiceController(_serviceName))
                    {
                        sc.Refresh();
                        if (sc.Status == ServiceControllerStatus.Running)
                            continue;

                        LogHelper.Log($"[SqlServerWatchdog] Service not running (status={sc.Status}). Restarting…");
                        StatusChanged?.Invoke($"down ({sc.Status})");

                        if (!TryRestart(sc))
                        {
                            string msg = $"SQL Server '{_serviceName}' could not be restarted after {MaxRestartAttempts} attempts.";
                            LogHelper.Log($"[SqlServerWatchdog] {msg}");
                            RestartFailed?.Invoke(msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[SqlServerWatchdog] Monitor check error: {ex.Message}");
                }
            }
        }

        private static bool TryRestart(ServiceController sc)
        {
            for (int attempt = 1; attempt <= MaxRestartAttempts; attempt++)
            {
                try
                {
                    LogHelper.Log($"[SqlServerWatchdog] Restart attempt {attempt}/{MaxRestartAttempts}…");

                    if (sc.Status == ServiceControllerStatus.StopPending)
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));

                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));
                    }

                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running,
                                     TimeSpan.FromMilliseconds(RestartTimeoutMs));

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        LogHelper.Log($"[SqlServerWatchdog] Restarted on attempt {attempt}.");
                        StatusChanged?.Invoke("running (restarted)");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[SqlServerWatchdog] Attempt {attempt} error: {ex.Message}");
                }

                Thread.Sleep(5000);
            }

            return false;
        }

        private static string ResolveServiceName()
        {
            foreach (var name in KnownServiceNames)
            {
                try
                {
                    using (var sc = new ServiceController(name))
                    {
                        var _ = sc.Status; // throws InvalidOperationException if service doesn't exist
                        return name;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
