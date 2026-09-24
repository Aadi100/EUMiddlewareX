using Google.Apis.Auth.OAuth2; 
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text; 
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware.Classes
{
    public static class CommonUtils
    {
        public static string GetDeviceIpBySn(string sn)
        {
            if (string.IsNullOrEmpty(sn)) return null;
            var d = AppSession.DeviceList?.FirstOrDefault(x =>
                string.Equals(x.DeviceSerial, sn, StringComparison.OrdinalIgnoreCase));
            if (d != null) return d.DeviceIP;
            var s = AppSession.StaffDeviceList?.FirstOrDefault(x =>
                string.Equals(x.DeviceSerial, sn, StringComparison.OrdinalIgnoreCase));
            return s?.DeviceIP;
        }

        public static string GetDeviceSnByIp(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return null;

            var memberDevice = AppSession.DeviceList?.FirstOrDefault(d =>
                string.Equals(d.DeviceIP, ip, StringComparison.OrdinalIgnoreCase));
            if (memberDevice != null && !string.IsNullOrEmpty(memberDevice.DeviceSerial))
                return memberDevice.DeviceSerial;

            var staffDevice = AppSession.StaffDeviceList?.FirstOrDefault(d =>
                string.Equals(d.DeviceIP, ip, StringComparison.OrdinalIgnoreCase));
            if (staffDevice != null && !string.IsNullOrEmpty(staffDevice.DeviceSerial))
                return staffDevice.DeviceSerial;

            return null;
        }


        /// <summary>
        /// Pings a device and handles retries, returning a status string ("ONLINE", "OFFLINE", "RETRYING")
        /// suitable for updating the DataPairPanel UI.
        /// </summary>
        public static async Task<string> PingDeviceWithStatusAsync(string ipAddress, Action<string> onStatusUpdate = null)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) return "OFFLINE";

            const int timeout = 2000;
            const int maxRetries = 3;
            const int retryDelayMs = 2000;

            using (var ping = new System.Net.NetworkInformation.Ping())
            {
                // First attempt
                try
                {
                    var reply = await ping.SendPingAsync(ipAddress, timeout).ConfigureAwait(false);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        return "ONLINE";
                }
                catch { }

                // If first attempt fails, signal RETRYING and start retry loop
                onStatusUpdate?.Invoke("RETRYING");

                for (int i = 0; i < maxRetries; i++)
                {
                    await Task.Delay(retryDelayMs).ConfigureAwait(false);
                    try
                    {
                        var reply = await ping.SendPingAsync(ipAddress, timeout).ConfigureAwait(false);
                        if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                            return "ONLINE";
                    }
                    catch { }
                }
            }

            return "OFFLINE";
        }

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
        /// Creates a ZIP containing conditionally:
        /// - the snapshot .db (if includeDb is true)
        /// - all files inside /Logs (if includeLogs is true)
        /// Returns the zip path; otherwise null.
        /// </summary>
        public static string CreateDatabaseSnapshotAndZipLogs(DatabaseHelper dbHelper, bool includeDb = true, bool includeLogs = true)
        {
            if (dbHelper == null) throw new ArgumentNullException(nameof(dbHelper));
            if (!includeDb && !includeLogs) return null;

            string tempRoot = null;
            string snapshotPath = null;

            try
            {
                tempRoot = CreateTempWorkDir("SmartAttendanceBackupBundle");
                var workDir = Path.Combine(tempRoot, "bundle");
                Directory.CreateDirectory(workDir);

                if (includeDb)
                {
                    snapshotPath = dbHelper.CreateDatabaseSnapshot();
                    if (!string.IsNullOrWhiteSpace(snapshotPath) && File.Exists(snapshotPath))
                    {
                        var snapshotCopy = Path.Combine(workDir, Path.GetFileName(snapshotPath));
                        File.Copy(snapshotPath, snapshotCopy, overwrite: true);
                    }
                    else
                    {
                        LogHelper.Log("DB snapshot requested but creation failed.");
                    }
                }

                if (includeLogs)
                {
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
            }
        }

        ///// <summary>
        ///// Downloads GLogs from all available (active) devices into separate txt files (one per device),
        ///// then zips them into a single ZIP and returns that ZIP path.
        ///// </summary>
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

                        string sn = CommonUtils.GetDeviceSnByIp(device.DeviceIP);
                        if (string.IsNullOrEmpty(sn))
                        {
                            LogHelper.Log($"No SN found for {device.DeviceIP}, skipping GLog download.");
                            continue;
                        }

                        // Delete stale file so we can detect when the new one arrives
                        var appBase = AppDomain.CurrentDomain.BaseDirectory;
                        string expectedFile = Path.Combine(appBase, $"GLog.{device.MachineNo}.txt");
                        if (File.Exists(expectedFile)) File.Delete(expectedFile);

                        string glogResponse = await WebsocketServer.ExecuteServiceGetCommand(sn, "getalllogs", null).ConfigureAwait(false);

                        int readCount = 0;
                        if (!string.IsNullOrEmpty(glogResponse) && File.Exists(expectedFile))
                            readCount = File.ReadAllLines(expectedFile).Length - 1; // minus header

                        LogHelper.Log($"GLog read result count={readCount} for MachineNo={device.MachineNo}");

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

            // Load backup content settings
            bool incLogs = dbHelper.GetSetting("BackupIncludeLogs")?.ToLower() == "true";
            bool incGLogs = dbHelper.GetSetting("BackupIncludeGLogs")?.ToLower() == "true";
            bool incDb = dbHelper.GetSetting("BackupIncludeDatabase")?.ToLower() == "true";

            if (!incLogs && !incGLogs && !incDb)
            {
                LogHelper.Log("Backup email skipped: All backup content types are disabled in settings.");
                return false;
            }

            string logsDbZip = null;
            string glogsZip = null;

            try
            {
                if (incLogs || incDb)
                {
                    LogHelper.Log($"Starting bundle creation (DB={incDb}, Logs={incLogs}).");
                    logsDbZip = CreateDatabaseSnapshotAndZipLogs(dbHelper, incDb, incLogs);
                }

                if (incGLogs)
                {
                    LogHelper.Log("Starting GLogs download+zip.");
                    glogsZip = await DownloadGLogsAndZipAsync(dbHelper).ConfigureAwait(false);
                }

                if (string.IsNullOrWhiteSpace(logsDbZip) && string.IsNullOrWhiteSpace(glogsZip))
                {
                    LogHelper.Log("Nothing to email (ZIPs were empty or failed).");
                    return false;
                }

                // Fetch credentials from decrypted database settings instead of App.config
                var to = dbHelper.GetDecryptedSetting("BackupEmailTo");
                var user = dbHelper.GetDecryptedSetting("SmtpUser");
                var pass = dbHelper.GetDecryptedSetting("SmtpPassword");
                var host = dbHelper.GetDecryptedSetting("SmtpHost");
                var portStr = dbHelper.GetDecryptedSetting("SmtpPort");

                if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(host))
                {
                    LogHelper.Log("Missing SMTP settings in database (BackupEmailTo, SmtpUser, SmtpPassword, SmtpHost).");
                    return false;
                }

                if (!int.TryParse(portStr, out var port)) port = 587;

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(user);
                    msg.To.Add(to);
                    msg.Subject = $"SmartAttendance Backup Bundle {DateTime.Now:yyyy-MM-dd HH:mm}";
                    msg.Body = "Backup bundle attached.\n\n" + "Included components:\n" +
                               (incDb ? "- DB snapshot\n" : "") +
                               (incLogs ? "- System /Logs\n" : "") +
                               (incGLogs ? "- Device GLogs\n" : "");

                    if (!string.IsNullOrEmpty(logsDbZip) && File.Exists(logsDbZip))
                        msg.Attachments.Add(new Attachment(logsDbZip));
                    if (!string.IsNullOrEmpty(glogsZip) && File.Exists(glogsZip))
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
                if (!string.IsNullOrWhiteSpace(logsDbZip) && File.Exists(logsDbZip))
                    try { File.Delete(logsDbZip); } catch { }
                if (!string.IsNullOrWhiteSpace(glogsZip) && File.Exists(glogsZip))
                    try { File.Delete(glogsZip); } catch { }
            }
        }

        /// <summary>
        /// Parses a GMT offset string into a TimeSpan.
        /// Accepts "+HH", "-HH", "+HH:mm", "-HH:mm".
        /// Returns TimeSpan.Zero on invalid input.
        /// </summary>
        public static TimeSpan ParseGmtOffset(string gmtSetting)
        {
            if (string.IsNullOrWhiteSpace(gmtSetting))
                return TimeSpan.Zero;

            // Normalize and validate leading sign
            var s = gmtSetting.Trim();
            if (s.Length < 2 || (s[0] != '+' && s[0] != '-'))
                return TimeSpan.Zero;

            // Try "+HH:mm" or "-HH:mm" by stripping the '+'
            TimeSpan ts;
            var candidate = s[0] == '+' ? s.Substring(1) : s.Substring(1); // remove sign for TryParse
            if (TimeSpan.TryParse(candidate, out ts))
            {
                return s[0] == '-' ? TimeSpan.FromTicks(-ts.Ticks) : ts;
            }

            // Fallback for "+HH" or "-HH"
            if (s.Length == 3)
            {
                int hours;
                if (int.TryParse(s.Substring(1, 2), out hours))
                {
                    return s[0] == '-' ? TimeSpan.FromHours(-hours) : TimeSpan.FromHours(hours);
                }
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Converts a local wall-clock time under a given GMT offset to UTC 0 
        /// and formats it as "yyyy-MM-dd HH:mm:ss.fff+00".
        /// </summary>
        public static string ConvertToUtc0(DateTime localTime, string gmtSetting)
        {
            // Get the offset using custom logic
            var offset = ParseGmtOffset(gmtSetting);

            // Create a DateTimeOffset and convert to UTC
            DateTimeOffset localWithOffset = new DateTimeOffset(localTime, offset);
            return localWithOffset.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "+00";
        }

        /// <summary>
        /// Parses an ISO 8601 datetime string (with or without offset) and converts it to local time.
        /// Returns true when parsing succeeded. Outputs the local DateTime and formatted string "yyyy-MM-dd HH:mm:ss".
        /// </summary>
        public static bool TryParseIsoToLocal(string iso, out DateTime localTime, out string formatted)
        {
            localTime = default(DateTime);
            formatted = null;

            if (string.IsNullOrWhiteSpace(iso))
                return false;

            // Prefer DateTimeOffset to respect the provided offset
            DateTimeOffset dto;
            if (DateTimeOffset.TryParse(iso, out dto))
            {
                localTime = dto.ToLocalTime().DateTime;
                formatted = localTime.ToString("yyyy-MM-dd HH:mm:ss");
                return true;
            }

            // Fallback to DateTime
            DateTime dt;
            if (DateTime.TryParse(iso, out dt))
            {
                if (dt.Kind == DateTimeKind.Utc)
                    dt = dt.ToLocalTime();
                else if (dt.Kind == DateTimeKind.Unspecified)
                    dt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dt, DateTimeKind.Utc), TimeZoneInfo.Local);

                localTime = dt;
                formatted = localTime.ToString("yyyy-MM-dd HH:mm:ss");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts an ISO 8601 datetime string (e.g., "2026-02-06T04:45:44.298Z") to "yyyy-MM-dd HH:mm:ss-00".
        /// Ensures UTC representation and removes milliseconds. Returns null if parsing fails.
        /// </summary>
        public static string FormatSyncedAtUtcCompact(string iso)
        {
            if (string.IsNullOrWhiteSpace(iso))
                return null;

            // Respect provided offset (Z or +/-HH:mm)
            if (DateTimeOffset.TryParse(iso, out var dto))
            {
                var utc = dto.ToUniversalTime();
                return utc.ToString("yyyy-MM-dd HH:mm:ss") + "-00";
            }

            // Fallback: try DateTime, normalize to UTC
            if (DateTime.TryParse(iso, out var dt))
            {
                var utc = dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                utc = utc.ToUniversalTime();
                return utc.ToString("yyyy-MM-dd HH:mm:ss") + "-00";
            }

            return null;
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

        public static async Task<bool> IsDeviceReachableAsync(string ipAddress, int maxAttempts, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || maxAttempts <= 0)
                return false;

            using (var ping = new Ping())
            {
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    if (token.IsCancellationRequested) return false;

                    try
                    {
                        try { LogHelper.Log($"🔄 Pinging device {ipAddress}, attempt {attempt}"); } catch { }
                        var reply = await ping.SendPingAsync(ipAddress, 2000).ConfigureAwait(false); // 2s timeout

                        if (reply.Status == IPStatus.Success)
                        {
                            try { LogHelper.Log($"✅ Device {ipAddress} is reachable"); } catch { }
                            return true;
                        }
                        else
                        {
                            try { LogHelper.Log($"⚠️ Ping attempt {attempt} failed for device {ipAddress}: {reply.Status}"); } catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        try { LogHelper.Log($"❌ Error pinging device {ipAddress}: {ex.Message}"); } catch { }
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

    public static async Task<bool> UploadToGoogleDriveAsync(string filePath, string folderId, DatabaseHelper db)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;

            try
            {
                // 1. Get the OAuth Client Secret from DB
                string clientSecretJson = db.GetDecryptedSetting("GoogleDriveCredentialsJson");
                if (string.IsNullOrEmpty(clientSecretJson))
                {
                    LogHelper.Log("[Drive] Error: OAuth Client Secret (Desktop) not found in DB.");
                    return false;
                }

                UserCredential credential;
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(clientSecretJson)))
                {
                    // This will look for a 'token.json' folder in your bin directory to store the login
                    string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.json");

                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { DriveService.Scope.DriveFile },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }

                // 2. Create the Drive Service
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "SmartAttendanceMiddleware",
                });

                // 3. Prepare Metadata
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = Path.GetFileName(filePath),
                    Parents = new List<string> { folderId }
                };

                // 4. Upload
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    var request = service.Files.Create(fileMetadata, stream, "application/zip");
                    request.Fields = "id";

                    LogHelper.Log($"[Drive] Starting upload using User Account: {Path.GetFileName(filePath)}");
                    var progress = await request.UploadAsync();

                    if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
                    {
                        LogHelper.Log($"[Drive] Upload failed: {progress.Exception?.Message}");
                        return false;
                    }
                }

                LogHelper.Log($"[Drive] Backup successful! Saved to your personal Google Drive storage.");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Drive] CRITICAL ERROR: {ex.Message}");
                return false;
            }
        }


    public static async Task<bool> CreateAndUploadBackupToDriveAsync(DatabaseHelper dbHelper)
        {
            if (dbHelper == null) throw new ArgumentNullException(nameof(dbHelper));

            // Load backup content settings
            bool incLogs = dbHelper.GetSetting("BackupIncludeLogs")?.ToLower() == "true";
            bool incGLogs = dbHelper.GetSetting("BackupIncludeGLogs")?.ToLower() == "true";
            bool incDb = dbHelper.GetSetting("BackupIncludeDatabase")?.ToLower() == "true";

            if (!incLogs && !incGLogs && !incDb)
            {
                LogHelper.Log("Google Drive backup skipped: All backup content types are disabled in settings.");
                return false;
            }

            string logsDbZip = null;
            string glogsZip = null;
            string folderId = dbHelper.GetSetting("GoogleDriveFolderId");

            if (string.IsNullOrEmpty(folderId))
            {
                LogHelper.Log("Google Drive backup skipped: GoogleDriveFolderId not found in database settings.");
                return false;
            }

            try
            {
                LogHelper.Log($"BackupService: generating ZIP bundles for Google Drive (DB={incDb}, Logs={incLogs}, GLogs={incGLogs})...");
                
                if (incLogs || incDb)
                    logsDbZip = CreateDatabaseSnapshotAndZipLogs(dbHelper, incDb, incLogs);

                if (incGLogs)
                    glogsZip = await DownloadGLogsAndZipAsync(dbHelper).ConfigureAwait(false);

                bool success = true;

                if (!string.IsNullOrEmpty(logsDbZip) && File.Exists(logsDbZip))
                {
                    success &= await UploadToGoogleDriveAsync(logsDbZip, folderId, dbHelper).ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(glogsZip) && File.Exists(glogsZip))
                {
                    success &= await UploadToGoogleDriveAsync(glogsZip, folderId, dbHelper).ConfigureAwait(false);
                }

                return success;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"CreateAndUploadBackupToDriveAsync failed: {ex.Message}");
                return false;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(logsDbZip) && File.Exists(logsDbZip))
                    try { File.Delete(logsDbZip); } catch { }
                if (!string.IsNullOrWhiteSpace(glogsZip) && File.Exists(glogsZip))
                    try { File.Delete(glogsZip); } catch { }
            }
        }

        public static async Task<int> CleanupOldDriveFilesAsync(string folderId, int days, DatabaseHelper db)
        {
            int deletedCount = 0;
            try
            {
                // 1. Setup Auth (Same logic as Upload)
                string credentialsJson = db.GetDecryptedSetting("GoogleDriveCredentialsJson");
                if (string.IsNullOrEmpty(credentialsJson)) return 0;

                UserCredential credential;
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialsJson)))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { DriveService.Scope.DriveFile },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.json"), true));
                }

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "SmartAttendanceMiddleware",
                });

                // 2. Calculate threshold date
                DateTime threshold = DateTime.Now.AddDays(-days);
                LogHelper.Log($"[Drive] Cleaning up files older than {threshold:yyyy-MM-dd}...");

                // 3. List files in the specific folder
                var listRequest = service.Files.List();
                listRequest.Q = $"'{folderId}' in parents and trashed = false";
                listRequest.Fields = "files(id, name, createdTime)";

                var response = await listRequest.ExecuteAsync();

                if (response.Files != null)
                {
                    foreach (var file in response.Files)
                    {
                        if (file.CreatedTime.HasValue && file.CreatedTime.Value < threshold)
                        {
                            LogHelper.Log($"[Drive] Deleting old backup: {file.Name} (Created: {file.CreatedTime})");
                            await service.Files.Delete(file.Id).ExecuteAsync();
                            deletedCount++;
                        }
                    }
                }

                return deletedCount;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Drive] Cleanup Failed: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Retrieves 'BackupRetentionDays' from settings and deletes synced attendance logs 
        /// (both member and staff) that are older than that many days.
        /// </summary>
        public static async Task<int> CleanupOldSyncedAttendanceLogsAsync(DatabaseHelper dbHelper)
        {
            if (dbHelper == null) throw new ArgumentNullException(nameof(dbHelper));

            try
            {
                // 1. Get retention days from settings
                string retentionStr = dbHelper.GetSetting("BackupRetentionDays");
                if (!int.TryParse(retentionStr, out int days) || days <= 0)
                {
                    LogHelper.Log("Cleanup skipped: 'BackupRetentionDays' not set or invalid.");
                    return 0;
                }

                LogHelper.Log($"Starting cleanup of synced attendance logs older than {days} days...");

                // 2. Execute deletion logic via DatabaseHelper
                // Note: DeleteOldSyncedAttendanceLogs is synchronous in DatabaseHelper, 
                // but we wrap it in Task.Run to keep the UI responsive if called from UI thread.
                int totalDeleted = await Task.Run(() => dbHelper.DeleteOldSyncedAttendanceLogs(days)).ConfigureAwait(false);

                if (totalDeleted > 0)
                {
                    LogHelper.Log($"Cleanup complete. Total records deleted: {totalDeleted}");
                }
                else
                {
                    LogHelper.Log("Cleanup complete. No records matched the criteria.");
                }

                return totalDeleted;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"CleanupOldSyncedAttendanceLogsAsync failed: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Identifies members expired by more than 2 months and performs physical (Cloud WebSocket) 
        /// and database cleanup.
        /// </summary>
        public static async Task ProcessLongExpiredMembersCleanupAsync(DatabaseHelper dbHelper)
        {
            if (dbHelper == null) throw new ArgumentNullException(nameof(dbHelper));

            try
            {
                LogHelper.Log("Starting cleanup for members expired more than 2 months ago...");

                // Get the list of members before any deletion occurs
                List<Member> expiredMembers = dbHelper.GetExpiredMembersForCleanup(monthsThreshold: 2);

                if (expiredMembers.Count == 0)
                {
                    LogHelper.Log("No members found with expiry older than 2 months.");
                    return;
                }

                var activeDevices = dbHelper.LoadDevices()?.Where(d => d.IsActive).ToList() ?? new List<Device>();

                foreach (var member in expiredMembers)
                {
                    LogHelper.Log($"[Processing Cleanup] Member: {member.Name} (ID: {member.MemberID}, Enrollment: {member.EnrollmentID})");

                    // --- STEP 1: DEVICE LEVEL CLEANUP (WebSocket/Cloud) ---
                    if (member.EnrollmentID.HasValue && activeDevices.Count > 0)
                    {
                        foreach (var device in activeDevices)
                        {
                            try
                            {
                                // 1. Resolve Serial Number (SN) for WebSocket routing
                                string sn = device.DeviceSerial;
                                if (string.IsNullOrEmpty(sn))
                                {
                                    sn = CommonUtils.GetDeviceSnByIp(device.DeviceIP);
                                }

                                if (string.IsNullOrEmpty(sn))
                                {
                                    LogHelper.Log($"[DeviceCleanup] Skipping device {device.DeviceIP} - No SN found.");
                                    continue;
                                }

                                // 2. Prepare JSON for deleteuser command
                                // BackupNumber 12 in cloud protocol = all data (fp, card, pwd)
                                var deleteParams = new
                                {
                                    enrollid = member.EnrollmentID.Value,
                                    backupnum = 12
                                };

                                // 3. Dispatch via WebSocket
                                bool dispatched = await WebsocketServer.ExecuteServiceCommand(
                                    sn,
                                    "deleteuser",
                                    JsonConvert.SerializeObject(deleteParams)
                                ).ConfigureAwait(false);

                                if (dispatched)
                                    LogHelper.Log($"[DeviceCleanup] Dispatched delete command for EnrollID {member.EnrollmentID} to {sn}");
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Log($"[DeviceCleanup] Error for EnrollID {member.EnrollmentID} on {device.DeviceIP}: {ex.Message}");
                            }
                        }
                    }

                    // --- STEP 2: DATABASE LEVEL CLEANUP ---
                    bool dbSuccess = dbHelper.DeleteExpiredMemberFromDb(member.MemberID);
                    if (dbSuccess)
                    {
                        LogHelper.Log($"[DbCleanup] Successfully deleted records for {member.Name} (ID: {member.MemberID})");
                    }
                }

                LogHelper.Log($"Cleanup complete. Total expired members processed: {expiredMembers.Count}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"ProcessLongExpiredMembersCleanupAsync fatal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks attendance logs for sessions with no check-out. 
        /// If the current time exceeds check-in time by 6 hours, 
        /// it forces a check-out exactly 6 hours after check-in, 
        /// updates the database, and sends the update to the server.
        /// </summary>
        public static async Task ProcessForceCheckOutAsync(DatabaseHelper dbHelper, HttpHandler httpHandler)
        {
            if (dbHelper == null || httpHandler == null) return;

            try
            {
                var logs = dbHelper.GetAllAttendanceLogs();
                var now = DateTime.Now;

                foreach (var log in logs)
                {
                    if (string.IsNullOrEmpty(log.CheckOutTime) && !string.IsNullOrEmpty(log.CheckInTime))
                    {
                        if (DateTime.TryParse(log.CheckInTime, out DateTime checkInTime))
                        {
                            if ((now - checkInTime).TotalHours >= 6)
                            {
                                DateTime forcedCheckOutTime = checkInTime.AddHours(6);
                                string forcedCheckOutStr = forcedCheckOutTime.ToString("yyyy-MM-dd HH:mm:ss");

                                LogHelper.Log($"[ForceCheckOut] MemberID: {log.MemberID}, LogID: {log.LogID}. No checkout for >6 hours. Forcing checkout at {forcedCheckOutStr}");

                                // 1. Update local Database (This works, as seen in your screenshot)
                                dbHelper.UpdateAttendanceLogForCheckOut(log.MemberID, forcedCheckOutStr);

                                // 2. Send to Server (This is where the error triggered)
                                var member = dbHelper.GetMember(log.MemberID);
                                // Changed .HasValue to > 0 to prevent the 'int' definition error
                                if (member != null && member.EnrollmentID > 0)
                                {
                                    var gmtSetting = dbHelper.GetSetting("GMT");
                                    string formattedCheckIn = ConvertToUtc0(checkInTime, gmtSetting);
                                    string formattedCheckOut = ConvertToUtc0(forcedCheckOutTime, gmtSetting);

                                    LogHelper.Log($"[ForceCheckOut] Syncing forced checkout to server for EnrollmentID: {member.EnrollmentID}");
                                    await httpHandler.PostAttendanceRecordAsync(
                                        (int)member.EnrollmentID, 
                                        formattedCheckIn,
                                        formattedCheckOut,
                                        log.AttendanceId
                                    ).ConfigureAwait(false);
                                }
                                else
                                {
                                    LogHelper.Log($"[ForceCheckOut] Skip server sync: EnrollmentID not found for MemberID {log.MemberID}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[ForceCheckOut][ERROR] {ex.Message}");
            }
        }

        /// <summary>
        /// A generic helper to handle door opening logic using WebSockets. Shows the authorization form 
        /// and communicates with the physical device using the resolved Serial Number.
        /// </summary>
        public static async Task<bool> HandleDoorOpenRequestAsync(string ip, int port, int machineNo, string password, DatabaseHelper dbHelper)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                LogHelper.Log("[HandleDoorOpenRequestAsync] Error: IP address is null or empty.");
                return false;
            }

            try
            {
                LogHelper.Log($"[HandleDoorOpenRequestAsync] Attempting to open door for IP: {ip}");

                // 1. Resolve Serial Number (SN) for WebSocket routing
                string sn = CommonUtils.GetDeviceSnByIp(ip);
                if (string.IsNullOrEmpty(sn))
                {
                    LogHelper.Log($"[HandleDoorOpenRequestAsync] Error: Serial Number (SN) not found for IP {ip}.");
                    System.Windows.Forms.MessageBox.Show($"Device {ip} is not currently registered or online via WebSocket.", "Cloud Protocol Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return false;
                }

                // 2. Show the GateAccessForm for authorization/logging — unless the
                // "RequireGateEntryForm" setting has been turned off (Setup Management),
                // in which case skip straight to opening the door with no prompt/logging.
                string requireFormSetting = dbHelper.GetSetting("RequireGateEntryForm");
                bool requireForm = string.IsNullOrWhiteSpace(requireFormSetting) ||
                                    requireFormSetting.Equals("true", StringComparison.OrdinalIgnoreCase);

                if (requireForm)
                {
                    string loggedInUser = AppSession.LoggedInUser;
                    using (var gateAccessForm = new GateAccessForm(ip, dbHelper, loggedInUser))
                    {
                        if (gateAccessForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        {
                            LogHelper.Log($"[HandleDoorOpenRequestAsync] Info: Gate access cancelled/denied for {ip} (SN: {sn}).");
                            return false;
                        }
                    }
                }
                else
                {
                    // No entry form, but the "RequireGatePassword" setting still applies here
                    // exactly like it does inside GateAccessForm — same gate, same rule either way.
                    if (IsGatePasswordRequired(dbHelper) && !PromptForPassword(AppSession.LoggedInUser, dbHelper))
                    {
                        LogHelper.Log($"[HandleDoorOpenRequestAsync] Info: Password verification failed/cancelled for direct open on {ip} (SN: {sn}).");
                        return false;
                    }

                    LogHelper.Log($"[HandleDoorOpenRequestAsync] RequireGateEntryForm is off — opening {ip} (SN: {sn}) directly, no entry form.");
                }

                // 3. Execute physical door open command via WebSocket
                // Passing null for jsonParams triggers 'opendoor' with null doornum (Open All Doors)
                bool doorOpened = await WebsocketServer.ExecuteServiceCommand(sn, "opendoor", null).ConfigureAwait(true);

                if (doorOpened)
                {
                    LogHelper.Log($"[HandleDoorOpenRequestAsync] Success: Door open command dispatched to {sn} ({ip}).");
                    return true;
                }
                else
                {
                    LogHelper.Log($"[HandleDoorOpenRequestAsync] Error: Failed to dispatch open command to {sn} ({ip}).");
                    System.Windows.Forms.MessageBox.Show($"Device {sn} is disconnected. Please check connection.", "WebSocket Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[HandleDoorOpenRequestAsync] Exception: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"An error occurred: {ex.Message}", "System Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Reads the "RequireGatePassword" setting (default true — same rule for both the
        /// with-form and without-form gate-open paths).
        /// </summary>
        public static bool IsGatePasswordRequired(DatabaseHelper dbHelper)
        {
            string setting = dbHelper.GetSetting("RequireGatePassword");
            return string.IsNullOrWhiteSpace(setting) || setting.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Shows the inline password prompt and verifies it against the logged-in user.
        /// Shared by GateAccessForm and the no-form direct-open path so both enforce the
        /// same "RequireGatePassword" rule identically. Returns false on cancel or a wrong
        /// password (an error is already shown to the user in the wrong-password case).
        /// </summary>
        public static bool PromptForPassword(string loggedInUser, DatabaseHelper dbHelper)
        {
            using (var prompt = new Form
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Password Required - {loggedInUser}",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
                var label = new Label { Left = 10, Top = 20, Width = 320, Text = "Enter your password:" };
                var inputBox = new TextBox { Left = 10, Top = 50, Width = 320, UseSystemPasswordChar = true };
                var okButton = new Button { Text = "OK", Left = 180, Width = 70, Top = 85, DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Cancel", Left = 260, Width = 70, Top = 85, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(label);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(okButton);
                prompt.Controls.Add(cancelButton);
                prompt.AcceptButton = okButton;
                prompt.CancelButton = cancelButton;

                if (prompt.ShowDialog() != DialogResult.OK)
                {
                    LogHelper.Log("[PromptForPassword] Password prompt cancelled.");
                    return false;
                }

                if (!dbHelper.VerifyLoginUser(loggedInUser, inputBox.Text))
                {
                    MessageBox.Show("Incorrect password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogHelper.Log("[PromptForPassword] Incorrect password entered.");
                    return false;
                }

                LogHelper.Log("[PromptForPassword] Password verified successfully.");
                return true;
            }
        }

        /// <summary>
        /// Converts an image from a URL or local file path into a Base64 encoded string.
        /// </summary>
        /// <param name="imageUrl">The web URL or local file path of the image.</param>
        /// <returns>Base64 string of the image, or null if conversion fails.</returns>
        /// <summary>
        /// Decodes a Base64 string into a System.Drawing.Image.
        /// Returns null if the string is empty, not valid Base64, or not a valid image.
        /// Strips a leading data-URI prefix (e.g. "data:image/jpeg;base64,") if present.
        /// </summary>
        public static System.Drawing.Image Base64ToImage(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return null;
            try
            {
                int commaIndex = base64.IndexOf(',');
                if (commaIndex >= 0) base64 = base64.Substring(commaIndex + 1);

                byte[] bytes = Convert.FromBase64String(base64);
                using (var ms = new MemoryStream(bytes))
                    return System.Drawing.Image.FromStream(new MemoryStream(bytes)); // keep stream alive via copy
            }
            catch { return null; }
        }

        public static async Task<string> ConvertImageToBase64Async(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return null;

            try
            {
                byte[] imageBytes;

                // Check if the input is a web URL
                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    using (var webClient = new WebClient())
                    {
                        imageBytes = await webClient.DownloadDataTaskAsync(uriResult).ConfigureAwait(false);
                    }
                }
                // Otherwise check if it's a local file
                else if (File.Exists(imageUrl))
                {
                    imageBytes = File.ReadAllBytes(imageUrl);
                }
                else
                {
                    return null;
                }

                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                // Assuming LogHelper exists based on previous context in the file
                try { LogHelper.Log($"Failed to convert image to Base64. URL: {imageUrl}. Error: {ex.Message}"); } catch { }
                return null;
            }
        }
    }
}