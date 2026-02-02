using System.IO;
using System;

public static class LogHelper
{
    private static readonly string LogsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    private static readonly string LogFilePath;
    private static StreamWriter _logWriter;

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

        // Register for application exit to close the log file
        AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
    }

    // ✅ Event for external UIs to subscribe to
    public static event Action<string> LogUpdated;

    public static void Log(string message)
    {
        try
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            Console.WriteLine(logEntry);
            _logWriter.WriteLine(logEntry);

            // 🔔 Raise event so UI can show the log in real time
            LogUpdated?.Invoke(logEntry);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error writing to log file: " + ex.Message);
            Console.WriteLine(message);
        }
    }

    public static void Close()
    {
        if (_logWriter != null)
        {
            _logWriter.Flush();
            _logWriter.Close();
            _logWriter = null;
        }
    }
}