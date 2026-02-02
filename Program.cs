using System.IO;
using System;
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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login());
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            ServiceManager.StopLogSyncService();
        }

        private static void Log(string tag, Exception ex, bool terminating = false)
        {
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
    }
}