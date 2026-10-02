using System.IO;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    static class Program
    {
        private static readonly object _logLock = new object();
        private static Mutex _singleInstanceMutex;

        [STAThread]
        static void Main()
        {
            // Prevent multiple instances
            _singleInstanceMutex = new Mutex(true, "smart_attendance_middleware_single_instance");
            if (!_singleInstanceMutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Application is already running. Please close the existing instance first.", "Application Already Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Log("ThreadException", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Log("AppDomainUnhandled", e.ExceptionObject as Exception, e.IsTerminating);
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Log("UnobservedTaskException", e.Exception);
                e.SetObserved();
            };

            // Ensure cleanup runs
            Application.ApplicationExit += OnApplicationExit;

            // Setup firewall rule for port 7788 on first run
            if (!FirewallRuleExists("FitnessMarvel"))
            {
                CreateFirewallRule();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if admin exists, if not show UserSetup, else show Login
            DatabaseHelper dbHelper = new DatabaseHelper();
            dbHelper.InitializeDatabase();

            if (dbHelper.AdminUserExists())
            {
                Application.Run(new Login());
            }
            else
            {
                Application.Run(new UserSetup());
            }
        }

        private static void DisableDebugMode()
        {
            try { new smart_attendance_middleware.DatabaseHelper().SaveSetting("DebugMode", "false"); } catch { }
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            DisableDebugMode();
            ServiceManager.StopLogSyncService();
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
        }

        private static void Log(string tag, Exception ex, bool terminating = false)
        {
            DisableDebugMode();
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("==== Crash Entry ====");
                sb.AppendLine("UTC: " + DateTime.UtcNow.ToString("o"));
                sb.AppendLine("Tag: " + tag);
                sb.AppendLine("Terminating: " + terminating);
                sb.AppendLine("Thread: " + Thread.CurrentThread.ManagedThreadId);
                sb.AppendLine("Message: " + ex?.Message);
                sb.AppendLine("Type: " + ex?.GetType().FullName);
                sb.AppendLine("Stack: " + ex?.StackTrace);
                sb.AppendLine("Inner: " + ex?.InnerException?.Message);
                sb.AppendLine();

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
                lock (_logLock)
                {
                    File.AppendAllText(path, sb.ToString());
                }
            }
            catch
            {
                // Last-resort minimal fallback
                try
                {
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_fallback.log"),
                        DateTime.UtcNow.ToString("o") + " logging failed for tag=" + tag + Environment.NewLine);
                }
                catch { }
            }
        }

        private static bool FirewallRuleExists(string ruleName)
        {
            string markerFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".firewall_rule_created");
            if (File.Exists(markerFile)) return true;
            return false;
        }

        private static void CreateFirewallRule()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = @"-NoProfile -Command ""New-NetFirewallRule -DisplayName 'FitnessMarvel' -Direction Inbound -LocalPort 7788 -Protocol TCP -Action Allow -RemoteAddress Any -ErrorAction SilentlyContinue""",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                }

                // Mark that rule was created successfully
                try
                {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".firewall_rule_created"), "");
                }
                catch { }
            }
            catch { }
        }
    }
}