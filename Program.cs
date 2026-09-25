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

        [STAThread]
        static void Main()
        {
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
            Application.Run(new Login());
        }

        private static void DisableDebugMode()
        {
            try { new smart_attendance_middleware.DatabaseHelper().SaveSetting("DebugMode", "false"); } catch { }
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            DisableDebugMode();
            ServiceManager.StopLogSyncService();
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
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $@"-NoProfile -Command ""Get-NetFirewallRule -DisplayName '{ruleName}' -ErrorAction SilentlyContinue | Select-Object -First 1 | Measure-Object | Select-Object -ExpandProperty Count""",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    return output == "1";
                }
            }
            catch { return false; }
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
            }
            catch { }
        }
    }
}