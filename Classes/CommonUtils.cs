using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace smart_attendance_middleware.Classes
{
    public static class CommonUtils
    {
        /// <summary>
        /// Copies a file even if the source is currently in-use by another process (shared read/write).
        /// Returns the destination path on success; otherwise null.
        /// </summary>
        public static string CopyFileAllowOpen(string sourcePath, string destPath, int maxAttempts = 3, int delayMs = 200)
        {
            if (!File.Exists(sourcePath)) return null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var dir = Path.GetDirectoryName(destPath);
                    if (!string.IsNullOrWhiteSpace(dir))
                        Directory.CreateDirectory(dir);

                    // Open source with shared read/write so writer can keep the file open
                    using (var src = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var dst = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        src.Position = 0;
                        var buffer = new byte[81920];
                        int read;
                        while ((read = src.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            dst.Write(buffer, 0, read);
                        }
                        dst.Flush(true);
                    }

                    return destPath;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    try { LogHelper.Log($"Copy attempt {attempt} failed for {sourcePath}: {ex.Message}"); } catch { }
                    Thread.Sleep(delayMs);
                }
                catch (Exception ex)
                {
                    try { LogHelper.Log($"Failed copying file: {sourcePath}. Error: {ex.Message}"); } catch { }
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a DB snapshot via DatabaseHelper and creates a ZIP containing:
        /// - the snapshot .db
        /// - all files inside /Logs (copies *.txt safely when in-use)
        /// Returns the zip path; otherwise null.
        /// </summary>
        public static string CreateDatabaseSnapshotAndZipLogs(DatabaseHelper dbHelper)
        {
            if (dbHelper == null) throw new ArgumentNullException(nameof(dbHelper));

            string tempRoot = null;
            string snapshotPath = null;

            try
            {
                snapshotPath = dbHelper.CreateDatabaseSnapshot();
                if (string.IsNullOrWhiteSpace(snapshotPath) || !File.Exists(snapshotPath))
                {
                    LogHelper.Log("DB snapshot creation failed (snapshot path missing).");
                    return null;
                }

                tempRoot = CreateTempWorkDir("SmartAttendanceBackupBundle");
                var workDir = Path.Combine(tempRoot, "bundle");
                Directory.CreateDirectory(workDir);

                // Copy snapshot into workdir
                var snapshotCopy = Path.Combine(workDir, Path.GetFileName(snapshotPath));
                File.Copy(snapshotPath, snapshotCopy, overwrite: true);

                // Collect /Logs
                string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                var logsOutDir = Path.Combine(workDir, "Logs");
                Directory.CreateDirectory(logsOutDir);

                if (Directory.Exists(logsDir))
                {
                    foreach (var file in Directory.GetFiles(logsDir, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            var dest = Path.Combine(logsOutDir, Path.GetFileName(file));
                            if (file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                            {
                                var copied = CopyFileAllowOpen(file, dest, maxAttempts: 5, delayMs: 300);
                                if (copied == null)
                                    LogHelper.Log($"Skipped log (copy failed): {file}");
                            }
                            else
                            {
                                File.Copy(file, dest, overwrite: true);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"Failed copying log file '{file}': {ex.Message}");
                        }
                    }
                }
                else
                {
                    LogHelper.Log($"Logs directory not found: {logsDir}");
                }

                // Zip it
                string zipPath = Path.Combine(
                    tempRoot,
                    $"SmartAttendanceBackup_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(workDir, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);

                return zipPath;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"CreateDatabaseSnapshotAndZipLogs failed: {ex.Message}");
                return null;
            }
            finally
            {
                // Remove snapshot created by DatabaseHelper (it lives in %TEMP%\SmartAttendanceBackup)
                try
                {
                    if (!string.IsNullOrWhiteSpace(snapshotPath) && File.Exists(snapshotPath))
                        File.Delete(snapshotPath);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Failed deleting temp db snapshot: {ex.Message}");
                }

                // IMPORTANT:
                // Do NOT delete tempRoot here, because the ZIP we return is stored inside tempRoot.
                // Caller (CreateAndEmailDbLogsAndGLogsAsync) will delete the ZIP after emailing.
            }
        }

        /// <summary>
        /// Downloads GLogs from all available (active) devices into separate txt files (one per device),
        /// then zips them into a single ZIP and returns that ZIP path.
        /// </summary>
        public static async Task<string> DownloadGLogsAndZipAsync(DatabaseHelper dbHelper)
        {
            if (dbHelper == null) throw new ArgumentNullException(nameof(dbHelper));

            string tempRoot = null;
            try
            {
                var devices = dbHelper.LoadDevices()
                    ?.Where(d => d != null && d.IsActive)
                    .ToList() ?? new List<Device>();

                if (devices.Count == 0)
                {
                    LogHelper.Log("No active devices available to download GLogs.");
                    return null;
                }

                tempRoot = CreateTempWorkDir("SmartAttendanceGLogs");
                var glogsDir = Path.Combine(tempRoot, "GLogs");
                Directory.CreateDirectory(glogsDir);

                foreach (var device in devices)
                {
                    try
                    {
                        LogHelper.Log($"Downloading GLog from {device.DeviceIP}:{device.Port} (MachineNo={device.MachineNo})");

                        // This method writes file to AppDomain base folder: "GLog.{EMachineNo}.txt"
                        int readCount = await DeviceService.ReadAndSaveGLogData(
                            device.DeviceIP,
                            device.Port,
                            device.MachineNo,
                            device.Password
                        ).ConfigureAwait(false);

                        LogHelper.Log($"GLog read result count={readCount} for MachineNo={device.MachineNo}");

                        // Find the created file(s). Prefer exact machine file if present; otherwise newest GLog.*.txt
                        var appBase = AppDomain.CurrentDomain.BaseDirectory;

                        string created = Path.Combine(appBase, $"GLog.{device.MachineNo}.txt");
                        if (!File.Exists(created))
                        {
                            created = Directory.GetFiles(appBase, "GLog.*.txt")
                                .OrderByDescending(f => f)
                                .FirstOrDefault();
                        }

                        if (string.IsNullOrWhiteSpace(created) || !File.Exists(created))
                        {
                            LogHelper.Log($"No GLog file found after download for MachineNo={device.MachineNo}");
                            continue;
                        }

                        // Copy to our work folder (safe if in-use)
                        var destName = $"GLog.{device.MachineNo}.txt";
                        var destPath = Path.Combine(glogsDir, destName);
                        var copied = CopyFileAllowOpen(created, destPath, maxAttempts: 5, delayMs: 200);
                        if (copied == null)
                        {
                            LogHelper.Log($"Failed to copy downloaded GLog file for MachineNo={device.MachineNo}: {created}");
                            continue;
                        }

                        // Cleanup created file in app base (optional)
                        try { File.Delete(created); } catch { }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Download GLog failed for MachineNo={device.MachineNo} ({device.DeviceIP}): {ex.Message}");
                    }
                }

                // Zip them
                string zipPath = Path.Combine(
                    tempRoot,
                    $"SmartAttendanceGLogs_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

                if (File.Exists(zipPath)) File.Delete(zipPath);
                ZipFile.CreateFromDirectory(glogsDir, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);

                return zipPath;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"DownloadGLogsAndZipAsync failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates:
        ///  - ZIP containing DB snapshot + all files inside /Logs
        ///  - ZIP containing downloaded GLogs from all active devices
        /// Sends both ZIPs via SMTP (App.config keys: BackupEmailTo, SmtpUser, SmtpPassword, SmtpHost, SmtpPort).
        /// Returns true if email sent; otherwise false.
        /// </summary>
        public static async Task<bool> CreateAndEmailDbLogsAndGLogsAsync(DatabaseHelper dbHelper)
        {
            if (dbHelper == null) throw new ArgumentNullException(nameof(dbHelper));

            string logsDbZip = null;
            string glogsZip = null;

            try
            {
                LogHelper.Log("Starting bundle creation (DB snapshot + Logs).");
                logsDbZip = CreateDatabaseSnapshotAndZipLogs(dbHelper);
                if (string.IsNullOrWhiteSpace(logsDbZip) || !File.Exists(logsDbZip))
                {
                    LogHelper.Log("Failed to create DB+Logs ZIP.");
                    return false;
                }

                LogHelper.Log("Starting GLogs download+zip.");
                glogsZip = await DownloadGLogsAndZipAsync(dbHelper).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(glogsZip) || !File.Exists(glogsZip))
                {
                    LogHelper.Log("Failed to create GLogs ZIP.");
                    return false;
                }

                // SMTP settings (same keys you already use elsewhere)
                var to = ConfigurationManager.AppSettings["BackupEmailTo"];
                var user = ConfigurationManager.AppSettings["SmtpUser"];
                var pass = ConfigurationManager.AppSettings["SmtpPassword"];
                var host = ConfigurationManager.AppSettings["SmtpHost"];
                var portStr = ConfigurationManager.AppSettings["SmtpPort"];

                if (string.IsNullOrWhiteSpace(to) ||
                    string.IsNullOrWhiteSpace(user) ||
                    string.IsNullOrWhiteSpace(pass) ||
                    string.IsNullOrWhiteSpace(host))
                {
                    LogHelper.Log("Missing SMTP settings in App.config (BackupEmailTo, SmtpUser, SmtpPassword, SmtpHost).");
                    return false;
                }

                if (!int.TryParse(portStr, out var port)) port = 587;

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(user);
                    msg.To.Add(to);
                    msg.Subject = $"SmartAttendance Backup Bundle {DateTime.Now:yyyy-MM-dd HH:mm}";
                    msg.Body =
                        "Backup bundle attached.\n\n" +
                        "Attachments:\n" +
                        "- DB snapshot + all /Logs (ZIP)\n" +
                        "- Downloaded GLogs from active devices (ZIP)\n";

                    msg.Attachments.Add(new Attachment(logsDbZip));
                    msg.Attachments.Add(new Attachment(glogsZip));

                    using (var client = new SmtpClient(host, port))
                    {
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(user, pass);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        await client.SendMailAsync(msg).ConfigureAwait(false);
                    }
                }

                LogHelper.Log("Backup bundle email sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"CreateAndEmailDbLogsAndGLogsAsync failed: {ex.Message}");
                return false;
            }
            finally
            {
                // Cleanup created ZIPs (they live in temp dirs)
                try
                {
                    if (!string.IsNullOrWhiteSpace(logsDbZip) && File.Exists(logsDbZip))
                        File.Delete(logsDbZip);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Failed deleting logs/db zip '{logsDbZip}': {ex.Message}");
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(glogsZip) && File.Exists(glogsZip))
                        File.Delete(glogsZip);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Failed deleting glogs zip '{glogsZip}': {ex.Message}");
                }
            }
        }

        private static string CreateTempWorkDir(string prefix)
        {
            string dir = Path.Combine(Path.GetTempPath(), prefix, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void TryDeleteDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return;

            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
            catch (Exception ex)
            {
                try { LogHelper.Log($"Failed deleting temp directory '{dir}': {ex.Message}"); } catch { }
            }
        }
    }
}