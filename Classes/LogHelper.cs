using System.IO;
using System;
using System.Collections.Concurrent;
using System.Threading;
using smart_attendance_middleware;

public static class LogHelper
{
    private static readonly string LogsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    private static readonly string LogFilePath;
    private static StreamWriter _logWriter;

    // Producer/consumer queue: Log() just enqueues and returns immediately;
    // a single dedicated background thread is the only thing that ever touches
    // _logWriter, so there's no cross-thread race on the file anymore.
    private static readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>();
    private static readonly Thread _writerThread;
    private static volatile bool _closed = false;

    static LogHelper()
    {
        // Ensure the Logs directory exists
        if (!Directory.Exists(LogsDirectory))
        {
            Directory.CreateDirectory(LogsDirectory);
        }

        // Create a unique log file for each app run
        string fileName = $"MiddlewareLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        LogFilePath = Path.Combine(LogsDirectory, fileName);

        // Open the log file for writing
        _logWriter = new StreamWriter(LogFilePath, append: false) { AutoFlush = true };

        _writerThread = new Thread(ProcessQueue)
        {
            IsBackground = true,
            Name = "LogHelper.Writer"
        };
        _writerThread.Start();

        // Register for application exit to close the log file
        AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
    }

    // ✅ Event for external UIs to subscribe to
    public static event Action<string> LogUpdated;

    public static void Log(string message)
    {
        try
        {
            string user = !string.IsNullOrEmpty(AppSession.LoggedInUser) ? $"[{AppSession.LoggedInUser}] " : "";
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {user}{message}";
            Console.WriteLine(logEntry);

            if (!_closed && !_logQueue.IsAddingCompleted)
            {
                try { _logQueue.Add(logEntry); } catch (InvalidOperationException) { /* queue completed mid-add, safe to ignore */ }
            }

            // 🔔 Raise event so UI can show the log in real time
            LogUpdated?.Invoke(logEntry);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error writing to log file: " + ex.Message);
            Console.WriteLine(message);
        }
    }

    // Like Log() but only writes when a developer is logged in.
    // Use this for messages that contain raw API payloads, PII, or biometric data.
    public static void LogDev(string message)
    {
        if (!string.Equals(AppSession.UserRole, "developer", StringComparison.OrdinalIgnoreCase))
            return;
        Log(message);
    }

    // Runs on the single dedicated writer thread only — no locking needed
    // since nothing else ever touches _logWriter.
    private static void ProcessQueue()
    {
        foreach (var logEntry in _logQueue.GetConsumingEnumerable())
        {
            try
            {
                _logWriter?.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to log file: " + ex.Message);
            }
        }
    }

    public static void Close()
    {
        if (_closed) return;
        _closed = true;

        // Stop accepting new entries and let the writer thread drain what's left
        _logQueue.CompleteAdding();
        _writerThread.Join(TimeSpan.FromSeconds(5));

        if (_logWriter != null)
        {
            _logWriter.Flush();
            _logWriter.Close();
            _logWriter = null;
        }
    }
}