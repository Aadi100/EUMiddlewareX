using System;
using System.Collections.Generic;
using System.IO;

namespace smart_attendance_middleware
{
    // Handles privileged attendance TXT logging, name resolution and session pairing.
    public static class PrivilegedAttendanceLogger
    {
        private static readonly object _txtLock = new object();
        private static DatabaseHelper _dbHelper; // use this field for DB lookups

        // Map deviceId -> action. Update as needed.
        private static readonly Dictionary<int, string> _deviceActionMap = new Dictionary<int, string>
        {
            { 1, "check-in" },
            { 2, "check-out" },
        };

        // Cache for privilegemember.txt
        private static Dictionary<int, string> _privilegedNames;
        private static DateTime _privilegedNamesLastWriteUtc;
        private static string _privilegedNamesPath;

        public static void ConfigureDeviceAction(int devId, string action)
        {
            if (string.IsNullOrWhiteSpace(action)) return;
            action = action.Trim().ToLowerInvariant();
            if (action != "check-in" && action != "check-out") return;

            lock (_txtLock)
                _deviceActionMap[devId] = action;
        }

        public static void Mark(int devId, string ip, int enroll, DateTime time)
        {
            try
            {
                // Use application base directory (e.g., SmartAttendanceMiddleware\bin\x86\Debug)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                Directory.CreateDirectory(baseDir);

                string filePath = Path.Combine(baseDir, "privillegememberattendence.txt"); // single file
                string action = GetActionForDevice(devId); // "check-in" or "check-out"
                string name = GetPrivilegedMemberName(enroll);

                // CSV schema: Enroll,Name,DevID,Action,CheckIn,CheckOut,IP
                string header = "Enroll,Name,DevID,Action,CheckIn,CheckOut,IP";
                string enrollStr = enroll.ToString();
                string timeStr = time.ToString("yyyy-MM-dd HH:mm:ss");

                lock (_txtLock)
                {
                    bool fileExists = File.Exists(filePath);
                    var lines = fileExists ? File.ReadAllLines(filePath) : new string[0];

                    if (!fileExists || lines.Length == 0)
                    {
                        File.WriteAllText(filePath, header + Environment.NewLine);
                        lines = new[] { header };
                    }

                    Func<string, string[]> parse = l => (l ?? "").Split(new[] { ',' }, StringSplitOptions.None);

                    // Find last open session (CheckOut empty) for the enroll
                    int openIndex = -1;
                    for (int i = lines.Length - 1; i >= 0; i--)
                    {
                        var l = lines[i];
                        if (string.IsNullOrWhiteSpace(l)) continue;
                        if (l.StartsWith("Enroll,")) continue;

                        var cols = parse(l);
                        if (cols.Length < 7) continue;

                        if (cols[0].Trim() == enrollStr && string.IsNullOrWhiteSpace(cols[5]))
                        {
                            openIndex = i;
                            break;
                        }
                    }

                    bool wrote = false;

                    if (string.Equals(action, "check-in", StringComparison.OrdinalIgnoreCase))
                    {
                        if (openIndex >= 0)
                        {
                            LogHelper.Log($"[AttendanceServer] TXT: duplicate check-in ignored (Enroll={enroll})");
                        }
                        else
                        {
                            string line = string.Join(",",
                                enrollStr,
                                name,
                                devId.ToString(),
                                "check-in",
                                timeStr,
                                "",
                                ip
                            );
                            File.AppendAllText(filePath, line + Environment.NewLine);
                            wrote = true;
                        }
                    }
                    else // check-out
                    {
                        if (openIndex >= 0)
                        {
                            var cols = parse(lines[openIndex]);
                            if (cols.Length < 7)
                                Array.Resize(ref cols, 7);

                            cols[3] = "check-in"; // keep original action normalized
                            cols[5] = timeStr;    // set CheckOut

                            lines[openIndex] = string.Join(",", cols);
                            File.WriteAllLines(filePath, lines);
                            wrote = true;
                        }
                        else
                        {
                            // No open check-in found -> write a standalone checkout row
                            string line = string.Join(",",
                                enrollStr,
                                name,
                                devId.ToString(),
                                "check-out",
                                "",
                                timeStr,
                                ip
                            );
                            File.AppendAllText(filePath, line + Environment.NewLine);
                            wrote = true;
                        }
                    }

                    if (wrote)
                        LogHelper.Log($"[AttendanceServer] Attendance TXT updated (Enroll={enroll}, Name=\"{name}\", Action={action}) -> {filePath}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServer] Failed to mark attendance to TXT: " + ex.Message);
            }
        }

        private static string GetActionForDevice(int devId)
        {
            try
            {
                if (_dbHelper == null) _dbHelper = new DatabaseHelper();
                // DatabaseHelper expects MachineNo; devId here should be MachineNo
                var actionFromDb = _dbHelper.GetDeviceAttendanceAction(devId);
                if (!string.IsNullOrWhiteSpace(actionFromDb))
                    return actionFromDb.Trim().ToLowerInvariant();
            }
            catch (Exception ex)
            {
                LogHelper.Log("[PrivilegedAttendanceLogger] GetActionForDevice DB lookup failed: " + ex.Message);
            }

            // Fallback to in-memory map
            string mapped;
            if (_deviceActionMap != null && _deviceActionMap.TryGetValue(devId, out mapped) && !string.IsNullOrWhiteSpace(mapped))
                return mapped.Trim().ToLowerInvariant();

            // Default to check-in when device not mapped
            return "check-in";
        }

        private static void EnsurePrivilegedNamesLoaded()
        {
            try
            {
                // Use application base directory
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                Directory.CreateDirectory(baseDir);

                if (string.IsNullOrEmpty(_privilegedNamesPath))
                    _privilegedNamesPath = Path.Combine(baseDir, "privilegemember.txt");

                var fi = new FileInfo(_privilegedNamesPath);
                if (!fi.Exists)
                {
                    if (_privilegedNames == null)
                        _privilegedNames = new Dictionary<int, string>();
                    return;
                }

                if (_privilegedNames != null && fi.LastWriteTimeUtc == _privilegedNamesLastWriteUtc)
                    return;

                var dict = new Dictionary<int, string>();
                var lines = File.ReadAllLines(_privilegedNamesPath);
                foreach (var raw in lines)
                {
                    var line = (raw ?? string.Empty).Trim();
                    if (line.Length == 0) continue;
                    if (line.StartsWith("#") || line.StartsWith("//") || line.StartsWith(";")) continue;

                    var parts = line.Split(new[] { ',', '=', '|', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;

                    int enroll;
                    if (!int.TryParse(parts[0].Trim(), out enroll)) continue;

                    var name = parts[1].Trim();
                    if (!dict.ContainsKey(enroll))
                        dict.Add(enroll, name);
                }

                _privilegedNames = dict;
                _privilegedNamesLastWriteUtc = fi.LastWriteTimeUtc;
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServer] Failed loading privilegemember.txt: " + ex.Message);
                if (_privilegedNames == null)
                    _privilegedNames = new Dictionary<int, string>();
            }
        }

        private static string GetPrivilegedMemberName(int enroll)
        {
            try
            {
                EnsurePrivilegedNamesLoaded();
                string name;
                if (_privilegedNames != null && _privilegedNames.TryGetValue(enroll, out name))
                    return name;
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServer] GetPrivilegedMemberName error: " + ex.Message);
            }
            return string.Empty;
        }
    }
}