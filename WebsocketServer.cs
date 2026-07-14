using Eagle._Components.Public;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class WebsocketServer : Form
    {
        // 1. Definition of LogType to fix "could not be found" errors
        public enum LogType { General, Incoming, Outgoing, Attendance }

        // Static instance for global access from other classes/forms
        public static WebsocketServer Instance { get; private set; }

        private CancellationTokenSource _formCts = new CancellationTokenSource();
        private HttpListener _listener;
        private bool _isListening = false;

        private ConcurrentDictionary<string, WebSocket> _manager = new ConcurrentDictionary<string, WebSocket>();
        private ConcurrentDictionary<string, JObject> _deviceStates = new ConcurrentDictionary<string, JObject>();
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();
        private List<Device> _dbDevices = new List<Device>();
        public readonly WebServerServices _services = new WebServerServices();
        private readonly AttendenceServerFunctions _attendanceFunctions = new AttendenceServerFunctions();
        private readonly StaffAttendenceServerFunctions _staffAttendanceFunctions = new StaffAttendenceServerFunctions();

        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingResponses =
            new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        private int _connectedDeviceCount = 0;
        private readonly ConcurrentDictionary<string, (string Ip, DateTime ConnectedAt)> _connectedDeviceInfo =
            new ConcurrentDictionary<string, (string Ip, DateTime ConnectedAt)>();

        // ── Session-scoped file loggers ───────────────────────────────────────
        private static readonly string ServerLogsDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerLogs");
        private StreamWriter _allLogWriter;
        private StreamWriter _attendanceLogWriter;

        private void OpenLogFiles()
        {
            try
            {
                if (!Directory.Exists(ServerLogsDir))
                    Directory.CreateDirectory(ServerLogsDir);

                string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _allLogWriter        = new StreamWriter(Path.Combine(ServerLogsDir, $"AllLogs_{stamp}.txt"),        append: false) { AutoFlush = true };
                _attendanceLogWriter = new StreamWriter(Path.Combine(ServerLogsDir, $"Attendance_{stamp}.txt"), append: false) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                LogHelper.Log("[WebsocketServer] Failed to open server log files: " + ex.Message);
            }
        }

        private void CloseLogFiles()
        {
            try { _allLogWriter?.Flush();        _allLogWriter?.Close();        _allLogWriter = null; }        catch { }
            try { _attendanceLogWriter?.Flush(); _attendanceLogWriter?.Close(); _attendanceLogWriter = null; } catch { }
        }

        private void WriteToFile(string message, bool isAttendance)
        {
            string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            try { _allLogWriter?.WriteLine(entry); } catch { }
            if (isAttendance)
                try { _attendanceLogWriter?.WriteLine(entry); } catch { }
        }
        // ─────────────────────────────────────────────────────────────────────

        public WebsocketServer()
        {
            InitializeComponent();
            Instance = this;

            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            LoadDatabaseDevices();
        }

        private void LoadDatabaseDevices()
        {
            try
            {
                var devices = _dbHelper.LoadDevices() ?? new List<Device>();
                var staffDevices = _dbHelper.LoadStaffDevices() ?? new List<Device>();

                _dbDevices = devices.Concat(staffDevices)
                    .Where(d => d.IsActive)
                    .ToList();

                LogToUI($"[DB] Loaded {_dbDevices.Count} active devices from database.");
            }
            catch (Exception ex)
            {
                LogToUI($"[DB] Error loading devices: {ex.Message}");
            }
        }

        private void UpdateStatus(string status)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { UpdateStatus(status); });
                return;
            }
            lbl_status.Text = status;
        }

        private void LogToUI(string message, LogType type = LogType.General)
        {
            // Write to files from any thread before marshalling to UI.
            // Attendance file only gets the formatted [ATTENDANCE] lines, not the raw [INCOMING] dump.
            bool isAttendance = type == LogType.Attendance && message.StartsWith("[ATTENDANCE]");
            WriteToFile(message, isAttendance);

            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { LogToUI(message, type); });
                return;
            }

            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}\r\n";

            // 1. All logs (every message)
            txtLogsAll.AppendText(logEntry);

            // 2. Filtered tabs — Attendance tab shows only [ATTENDANCE] lines, not the raw [INCOMING] sendlog
            switch (type)
            {
                case LogType.Incoming:
                    txtLogsIncoming.AppendText(logEntry);
                    break;
                case LogType.Outgoing:
                    txtLogsOutgoing.AppendText(logEntry);
                    break;
                case LogType.Attendance:
                    // Skip the raw JSON dump; only show the formatted [ATTENDANCE] line
                    if (message.StartsWith("[ATTENDANCE]"))
                        txtLogsAttendance.AppendText(logEntry);
                    break;
            }
        }

        private async void WebsocketServer_Load(object sender, EventArgs e)
        {
            OpenLogFiles();
            UpdateStatus("Status: Scanning...");
            LogToUI("--- AUTOMATIC STARTUP INITIATED ---");

            // 1. Perform Network Scan
            await ScanForDevicesAsync();

            // 1b. Directly ping every known device and connect if reachable
            await PingAndConnectKnownDevicesAsync();

            // 2. Start WebSocket Server
            StartServer();

            // 3. Signal that the server is up and scan is done
            WebsocketServerThreadManager.NotifyReady();
            NotifyConnectionChanged();

            PopulateDeviceSnComboBox();

            // 4. Start background retry loop for devices that missed the initial scan
            _ = Task.Run(() => RetryUnconnectedDevicesAsync(_formCts.Token));
        }

        private void PopulateDeviceSnComboBox()
        {
            cmb_deviceSn.Items.Clear();
            var sns = new System.Collections.Generic.HashSet<string>();
            if (AppSession.DeviceList != null)
                foreach (var d in AppSession.DeviceList)
                    if (!string.IsNullOrEmpty(d.DeviceSerial)) sns.Add(d.DeviceSerial);
            if (AppSession.StaffDeviceList != null)
                foreach (var d in AppSession.StaffDeviceList)
                    if (!string.IsNullOrEmpty(d.DeviceSerial)) sns.Add(d.DeviceSerial);
            foreach (var sn in sns) cmb_deviceSn.Items.Add(sn);
            if (cmb_deviceSn.Items.Count > 0) cmb_deviceSn.SelectedIndex = 0;
        }

        private async void btn_sendCommand_Click(object sender, EventArgs e)
        {
            var sn = cmb_deviceSn.SelectedItem?.ToString();
            var cmd = txt_cmd.Text.Trim();
            if (string.IsNullOrEmpty(sn) || string.IsNullOrEmpty(cmd))
            {
                MessageBox.Show("Select a device and enter a command.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            btn_sendCommand.Enabled = false;
            try
            {
                var paramsText = txt_params.Text.Trim();
                if (!string.IsNullOrEmpty(paramsText))
                {
                    try { JObject.Parse(paramsText); }
                    catch
                    {
                        MessageBox.Show("JSON Params is not valid JSON.\nExample: {\"enrollid\": 107}", "Invalid JSON", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                bool result = await ExecuteServiceCommand(sn, cmd, paramsText);
                LogToUI($"[CMD] {cmd} → {sn}: {(result ? "OK" : "FAILED")}");
            }
            catch (Exception ex)
            {
                LogToUI($"[CMD-ERR] {cmd}: {ex.Message}");
                MessageBox.Show($"Command failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_sendCommand.Enabled = true;
            }
        }

        private void StartServer()
        {
            if (_isListening) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://*:7788/");
                _listener.Start();
                _listener.TimeoutManager.IdleConnection = TimeSpan.FromMinutes(1090);
                _isListening = true;

                LogToUI("WebSocket Server Started on port 7788...");
                UpdateStatus("Status: Running (7788)");

                _ = Task.Run(() => AcceptConnectionsLoop());
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 5)
            {
                LogToUI("ERROR: Access Denied (Port 7788). Run as ADMINISTRATOR.");
                UpdateStatus("Status: ERROR (Access Denied)");
                _isListening = false;
            }
            catch (Exception ex)
            {
                LogToUI($"Failed to start server: {ex.Message}");
                UpdateStatus($"Status: ERROR ({ex.Message})");
                _isListening = false;
            }
        }

        private async Task ScanForDevicesAsync()
        {
            UpdateStatus("Status: Scanning Network...");
            LogToUI("--- STARTING DEVICE SCAN (PORT 7788) ---");

            string localSubnet = GetLocalSubnet();
            if (string.IsNullOrEmpty(localSubnet))
            {
                LogToUI("Error: Subnet could not be determined.");
                UpdateStatus("Status: Scan Error (No Subnet)");
                return;
            }

            LogToUI($"Scanning {localSubnet}.1 - 255...");
            var onlineIps = await ScanSubnetAsync(localSubnet);

            var portStatusTasks = onlineIps.Select(async ip =>
            {
                bool isOpen = await Task.Run(() => IsPortOpen(ip, 7788, 1500));
                return new { IP = ip, IsOpen = isOpen };
            });

            var results = await Task.WhenAll(portStatusTasks);

            foreach (var result in results)
            {
                if (result.IsOpen)
                {
                    LogToUI($">>> DEVICE FOUND: {result.IP}");

                    var match = _dbDevices.FirstOrDefault(d => d.DeviceIP == result.IP);
                    if (match != null && !string.IsNullOrEmpty(match.DeviceSerial))
                    {
                        LogToUI($"[AUTO] Match Found: {match.DeviceSerial}. Registering...");
                        UpdateStatus($"Status: Registering {match.DeviceSerial}...");
                        _ = Task.Run(() => ConnectToDeviceAndRegister(result.IP, match.DeviceSerial));
                    }
                }
            }
            LogToUI("--- SCAN COMPLETE ---");
            UpdateStatus("Status: Scan Complete");
        }

        private async Task AcceptConnectionsLoop()
        {
            while (_isListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = Task.Run(() => ProcessWebSocketRequest(context));
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch { break; }
            }
        }

        private async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            WebSocketContext wsContext = null;
            WebSocket socket = null;
            CancellationTokenSource connectionCts = CancellationTokenSource.CreateLinkedTokenSource(_formCts.Token);

            try
            {
                wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
                socket = wsContext.WebSocket;
                LogToUI($"New Incoming Connection Request from {context.Request.RemoteEndPoint}");
            }
            catch (Exception ex)
            {
                LogToUI($"WebSocket Acceptance Error: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Close();
                connectionCts.Dispose();
                return;
            }

            string remoteIp = context.Request.RemoteEndPoint?.Address?.ToString();
            await HandleDeviceMessagesAsync(socket, null, remoteIp, connectionCts);
        }

        private void RegisterDevice(string sn, WebSocket socket, string ip)
        {
            AppSession.RegisteredDevices[sn] = socket;
            _connectedDeviceInfo[sn] = (ip, DateTime.Now);
            Interlocked.Increment(ref _connectedDeviceCount);
            LogToUI($"[REG] Device {sn} registered.");
            UpdateStatus($"Status: Device {sn} Connected");
            NotifyConnectionChanged();
            if (!cmb_deviceSn.Items.Contains(sn))
                cmb_deviceSn.Invoke((System.Windows.Forms.MethodInvoker)(() =>
                {
                    if (!cmb_deviceSn.Items.Contains(sn)) cmb_deviceSn.Items.Add(sn);
                    if (cmb_deviceSn.SelectedIndex < 0) cmb_deviceSn.SelectedIndex = 0;
                }));
        }

        /// <summary>
        /// Shared receive loop for both inbound (device-initiated) and outbound (self-initiated)
        /// WebSocket connections. When knownSn is null, registration happens once the device sends
        /// cmd:"reg". When knownSn is supplied (outbound connection where we already sent cmd:"reg"),
        /// registration happens once the device acknowledges with ret:"reg".
        /// </summary>
        private async Task HandleDeviceMessagesAsync(WebSocket socket, string knownSn, string remoteIp, CancellationTokenSource connectionCts)
        {
            string currentSn = knownSn;
            bool awaitingInitialAck = !string.IsNullOrEmpty(knownSn);
            byte[] buffer = new byte[1024 * 64];

            try
            {
                while (socket.State == WebSocketState.Open && _isListening)
                {
                    string jsonIncoming = string.Empty;
                    WebSocketReceiveResult result = null;

                    // 1. Use a MemoryStream to accumulate large messages (up to 5MB+)
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            using (var idleCts = CancellationTokenSource.CreateLinkedTokenSource(connectionCts.Token))
                            {
                                idleCts.CancelAfter(awaitingInitialAck ? TimeSpan.FromSeconds(10) : TimeSpan.FromMinutes(15));
                                try
                                {
                                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), idleCts.Token);
                                }
                                catch (OperationCanceledException) when (!connectionCts.Token.IsCancellationRequested)
                                {
                                    if (awaitingInitialAck)
                                        LogToUI($"[AUTO-CONNECT] {currentSn ?? "Unknown"} did not acknowledge registration within 10s, giving up.");
                                    else
                                        LogToUI($"[Idle] {currentSn ?? "Unknown"} idle >5 min, closing to force reconnect.");
                                    if (socket.State == WebSocketState.Open)
                                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Idle timeout", connectionCts.Token);
                                    return;
                                }
                            }

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", connectionCts.Token);
                                break;
                            }

                            if (result.MessageType == WebSocketMessageType.Text || result.MessageType == WebSocketMessageType.Binary)
                            {
                                ms.Write(buffer, 0, result.Count);
                            }

                            // Safety break for extremely large messages if desired
                            if (ms.Length > 1024 * 1024 * 10) // 10MB hard limit
                                throw new Exception("Message size exceeds 10MB limit.");

                        } while (!result.EndOfMessage);

                        if (result.MessageType == WebSocketMessageType.Close) break;

                        ms.Seek(0, SeekOrigin.Begin);
                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            jsonIncoming = await reader.ReadToEndAsync();
                        }
                    }

                    if (string.IsNullOrEmpty(jsonIncoming)) continue;

                    JObject doc = null;
                    try
                    {
                        doc = JObject.Parse(jsonIncoming);
                    }
                    catch (Exception ex)
                    {
                        LogToUI($"[ERROR] JSON Parse Failed (Size: {jsonIncoming.Length} bytes): {ex.Message}");
                        continue;
                    }

                    // Determine the command (requests use 'cmd', responses use 'ret')
                    string cmd = doc["cmd"]?.ToString()?.ToLower();
                    string ret = doc["ret"]?.ToString()?.ToLower();
                    string effectiveCmd = cmd ?? ret;

                    // 2. Logging
                    LogType messageType = (effectiveCmd == "sendlog") ? LogType.Attendance : LogType.Incoming;
                    LogToUI($"[INCOMING] Raw Data: {jsonIncoming}", messageType);

                    // 3. Routing
                    if (string.Equals(cmd, "reg", StringComparison.OrdinalIgnoreCase))
                    {
                        string sn = doc["sn"]?.ToString();
                        if (string.IsNullOrWhiteSpace(sn)) continue;

                        if (AppSession.RegisteredDevices.ContainsKey(sn))
                            LogToUI($"[REG] Device {sn} already has an active session.");

                        var match = _dbDevices.FirstOrDefault(d => string.Equals(d.DeviceSerial, sn, StringComparison.OrdinalIgnoreCase));
                        if (match == null)
                        {
                            var failResponse = new { ret = "reg", result = false, reason = "Device not in Database" };
                            await SendToSocket(socket, failResponse, connectionCts.Token);
                            await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Device not in DB", connectionCts.Token);
                            break;
                        }

                        currentSn = sn;
                        awaitingInitialAck = false;
                        _deviceStates[sn] = doc;
                        RegisterDevice(sn, socket, remoteIp);

                        var response = new { ret = "reg", result = true, cloudtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                        await SendToSocket(socket, response, connectionCts.Token);
                    }
                    else if (awaitingInitialAck && !string.IsNullOrEmpty(currentSn) &&
                             string.Equals(ret, "reg", StringComparison.OrdinalIgnoreCase))
                    {
                        // Ack for a connection we initiated ourselves (outbound auto-connect)
                        bool ok = doc["result"]?.Value<bool>() ?? true;
                        awaitingInitialAck = false;
                        if (ok)
                        {
                            RegisterDevice(currentSn, socket, remoteIp);
                        }
                        else
                        {
                            LogToUI($"[REG-FAIL] Device {currentSn} rejected registration: {doc["reason"]}");
                            break;
                        }
                    }
                    else if (string.Equals(cmd, "sendlog", StringComparison.OrdinalIgnoreCase))
                    {
                        string sn = doc["sn"]?.ToString() ?? currentSn;
                        int logCount = doc["count"]?.Value<int>() ?? 0;
                        var device = _dbDevices.FirstOrDefault(d => string.Equals(d.DeviceSerial, sn, StringComparison.OrdinalIgnoreCase));

                        int devId = device?.DeviceId ?? 0;
                        string devIp = device?.DeviceIP ?? remoteIp ?? "Unknown IP";

                        if (doc["record"] is JArray records)
                        {
                            foreach (var rec in records)
                            {
                                int enroll = rec["enrollid"]?.Value<int>() ?? 0;
                                string name = rec["name"]?.ToString();
                                string timeStr = rec["time"]?.ToString();
                                string image = rec["image"]?.ToString();
                                DateTime.TryParse(timeStr, out DateTime eventTime);

                                string imageLabel = string.IsNullOrEmpty(image) ? "none" : "present";
                                LogToUI($"[ATTENDANCE] DevID: {devId} | IP: {devIp} | EnrollID: {enroll} | Name: {name} | Time: {timeStr} | Image: {imageLabel}", LogType.Attendance);
                                ForwardAttendanceData(devId, devIp, enroll, eventTime, name, image);
                            }
                        }

                        var logAck = new { ret = "sendlog", result = true, sn = sn, count = logCount };
                        await SendToSocket(socket, logAck, connectionCts.Token);
                    }
                    else
                    {
                        // 4. Handle Responses to Getters/Commands
                        string snRes = doc["sn"]?.ToString() ?? currentSn;
                        if (!string.IsNullOrEmpty(ret) && !string.IsNullOrEmpty(snRes))
                        {
                            if (ret == "getalllog")
                            {
                                _ = Task.Run(() => SaveGetAllLogResponse(snRes, doc));

                                string glogKey = $"{snRes}|getalllog";
                                if (_pendingResponses.TryRemove(glogKey, out var tcsGlog))
                                    tcsGlog.TrySetResult(jsonIncoming);
                            }
                            else
                            {
                                string baseKey = $"{snRes}|{ret}";
                                int? enrollid = doc["enrollid"]?.Value<int?>();
                                string specificKey = enrollid.HasValue ? $"{baseKey}|{enrollid}" : null;

                                if (specificKey != null && _pendingResponses.TryRemove(specificKey, out var tcsSpecific))
                                    tcsSpecific.TrySetResult(jsonIncoming);
                                else if (_pendingResponses.TryRemove(baseKey, out var tcsBase))
                                    tcsBase.TrySetResult(jsonIncoming);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogToUI($"Socket Error for {(currentSn ?? "Unknown")}: {ex.Message}");
            }
            finally
            {
                connectionCts.Cancel();
                if (!string.IsNullOrEmpty(currentSn) && AppSession.RegisteredDevices.TryRemove(currentSn, out _))
                {
                    _connectedDeviceInfo.TryRemove(currentSn, out _);
                    Interlocked.Decrement(ref _connectedDeviceCount);
                    NotifyConnectionChanged();

                    var pendingKeys = _pendingResponses.Keys
                        .Where(k => k.StartsWith(currentSn + "|"))
                        .ToList();
                    foreach (var key in pendingKeys)
                        if (_pendingResponses.TryRemove(key, out var tcs))
                            tcs.TrySetCanceled();
                }
                socket?.Dispose();
                connectionCts.Dispose();
                LogToUI($"Connection with {(currentSn ?? "Unknown")} closed.");
            }
        }
        private void SaveGetAllLogResponse(string sn, JObject doc)
        {
            try
            {
                bool success = doc["result"]?.Value<bool>() ?? false;
                if (!success)
                {
                    LogToUI($"[GLOG] getalllog result=false for {sn}, nothing to save.");
                    return;
                }

                if (!(doc["record"] is JArray records) || records.Count == 0)
                {
                    LogToUI($"[GLOG] No records in getalllog response for {sn}.");
                    return;
                }

                // Determine staff vs member via IP → DeviceCategoryMap
                bool isStaff = false;
                string ip = _dbHelper.GetDeviceIpBySn(sn);
                if (!string.IsNullOrEmpty(ip) &&
                    AppSession.DeviceCategoryMap.TryGetValue(ip, out var cat))
                {
                    isStaff = cat.type?.Equals("staff", StringComparison.OrdinalIgnoreCase) == true;
                }
                else
                {
                    isStaff = AppSession.StaffDeviceList
                        .Any(d => string.Equals(d.DeviceSerial, sn, StringComparison.OrdinalIgnoreCase));
                }

                // Get MachineNo for the EMachineNo column (fallback to 0)
                int machineNo = 0;
                var deviceEntry = (isStaff ? AppSession.StaffDeviceList : AppSession.DeviceList)
                    .FirstOrDefault(d => string.Equals(d.DeviceSerial, sn, StringComparison.OrdinalIgnoreCase));
                if (deviceEntry != null) machineNo = deviceEntry.MachineNo;

                var lines = new List<string> { "EnrollNo,EMachineNo,DateTime" };
                int recordCount = 0;

                foreach (var rec in records)
                {
                    int enrollId = rec["enrollid"]?.Value<int>() ?? 0;
                    if (enrollId == 0) continue;

                    string timeStr = rec["time"]?.ToString() ?? "";
                    DateTime.TryParse(timeStr, out DateTime dt);

                    lines.Add($"{enrollId:D8},{machineNo},{dt:yyyy/MM/dd HH:mm:ss}");
                    recordCount++;
                }

                string fileName = isStaff ? $"StaffGLogs.{machineNo}.txt" : $"GLog.{machineNo}.txt";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                File.WriteAllLines(filePath, lines);

                LogToUI($"[GLOG] Saved {recordCount} records to {fileName}");
            }
            catch (Exception ex)
            {
                LogToUI($"[GLOG-ERROR] SaveGetAllLogResponse for {sn}: {ex.Message}");
            }
        }

        private async Task SendToSocket(WebSocket socket, object payload, CancellationToken token)
        {
            try
            {
                string json = JsonConvert.SerializeObject(payload);
                LogToUI($"[SENDING] Raw Data: {json}", LogType.Outgoing);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, token);
            }
            catch { }
        }

        private void NotifyConnectionChanged()
        {
            WebsocketServerThreadManager.NotifyDeviceConnectionChanged(_connectedDeviceCount, _dbDevices.Count);
            RefreshConnectedDevicesList();
        }

        private void RefreshConnectedDevicesList()
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { RefreshConnectedDevicesList(); });
                return;
            }

            lvConnectedDevices.BeginUpdate();
            lvConnectedDevices.Items.Clear();

            foreach (var device in _dbDevices)
            {
                if (string.IsNullOrEmpty(device.DeviceSerial)) continue;

                bool online = AppSession.RegisteredDevices.ContainsKey(device.DeviceSerial);
                string ip = device.DeviceIP ?? "-";
                string connectedSince = "-";

                if (online && _connectedDeviceInfo.TryGetValue(device.DeviceSerial, out var info))
                {
                    if (!string.IsNullOrEmpty(info.Ip)) ip = info.Ip;
                    connectedSince = info.ConnectedAt.ToString("yyyy-MM-dd HH:mm:ss");
                }

                var item = new ListViewItem(new[] { device.DeviceSerial, ip, online ? "Online" : "Offline", connectedSince })
                {
                    ForeColor = online ? System.Drawing.Color.DarkGreen : System.Drawing.Color.Gray
                };
                lvConnectedDevices.Items.Add(item);
            }

            lvConnectedDevices.EndUpdate();
        }

        private void StopServer()
        {
            _isListening = false;
            if (_listener != null && _listener.IsListening)
            {
                _listener.Stop();
                _listener.Close();
            }
        }

        private bool IsPortOpen(string host, int port, int timeoutMs)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    if (!result.AsyncWaitHandle.WaitOne(timeoutMs)) return false;
                    client.EndConnect(result);
                    return true;
                }
            }
            catch { return false; }
        }

        private string GetLocalSubnet()
        {
            try
            {
                var ni = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up && n.GetIPProperties().GatewayAddresses.Count > 0)
                    .FirstOrDefault();

                if (ni != null)
                {
                    var ip = ni.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                    if (ip != null)
                    {
                        string[] parts = ip.Address.ToString().Split('.');
                        return $"{parts[0]}.{parts[1]}.{parts[2]}";
                      }
                }
            }
            catch { }
            return null;
        }

        private async Task<List<string>> ScanSubnetAsync(string subnet)
        {
            var onlineIps = new List<string>();
            var tasks = Enumerable.Range(1, 255).Select(i => SendPingAsync($"{subnet}.{i}")).ToList();
            var results = await Task.WhenAll(tasks);
            foreach (var reply in results)
                if (reply != null && reply.Status == IPStatus.Success)
                    onlineIps.Add(reply.Address.ToString());
            return onlineIps;
        }

        private async Task<PingReply> SendPingAsync(string ip)
        {
            try { using (var p = new Ping()) return await p.SendPingAsync(ip, 1500); }
            catch { return null; }
        }

        /// <summary>
        /// Pings every device known to the DB directly (independent of local subnet detection),
        /// and for any that respond and have port 7788 open, initiates an outbound connection
        /// and registers it — without relying on the device connecting to us first.
        /// </summary>
        private async Task PingAndConnectKnownDevicesAsync()
        {
            var candidates = _dbDevices
                .Where(d => !string.IsNullOrWhiteSpace(d.DeviceIP) && !string.IsNullOrWhiteSpace(d.DeviceSerial))
                .ToList();

            if (candidates.Count == 0) return;

            LogToUI("--- PINGING KNOWN DEVICES ---");

            var tasks = candidates.Select(async device =>
            {
                if (AppSession.RegisteredDevices.ContainsKey(device.DeviceSerial)) return;

                var reply = await SendPingAsync(device.DeviceIP);
                if (reply == null || reply.Status != IPStatus.Success)
                {
                    LogToUI($"[PING] {device.DeviceSerial} ({device.DeviceIP}) unreachable.");
                    return;
                }

                bool isOpen = await Task.Run(() => IsPortOpen(device.DeviceIP, 7788, 1500));
                if (!isOpen)
                {
                    LogToUI($"[PING OK] {device.DeviceSerial} ({device.DeviceIP}) reachable but port 7788 closed.");
                    return;
                }

                LogToUI($"[PING OK] {device.DeviceSerial} ({device.DeviceIP}) reachable, connecting...");
                await ConnectToDeviceAndRegister(device.DeviceIP, device.DeviceSerial);
            });

            await Task.WhenAll(tasks);
            LogToUI("--- PING KNOWN DEVICES COMPLETE ---");
        }

        private async Task ConnectToDeviceAndRegister(string ip, string serialNumber)
        {
            if (AppSession.RegisteredDevices.ContainsKey(serialNumber)) return;

            var uri = new Uri($"ws://{ip}:7788/pub/chat");
            var socket = new ClientWebSocket();
            var connectionCts = CancellationTokenSource.CreateLinkedTokenSource(_formCts.Token);

            try
            {
                using (var connectCts = CancellationTokenSource.CreateLinkedTokenSource(_formCts.Token))
                {
                    connectCts.CancelAfter(TimeSpan.FromSeconds(10));
                    await socket.ConnectAsync(uri, connectCts.Token);
                }

                LogToUI($"[AUTO-CONNECT] Connected to {serialNumber} ({ip}), sending registration...");
                var regMessage = new { cmd = "reg", sn = serialNumber };
                await SendToSocket(socket, regMessage, connectionCts.Token);

                await HandleDeviceMessagesAsync(socket, serialNumber, ip, connectionCts);
            }
            catch (Exception ex)
            {
                LogToUI($"[AUTO-CONNECT-ERR] {serialNumber} ({ip}): {ex.Message}");
                socket.Dispose();
                connectionCts.Dispose();
            }
        }

        private async Task RetryUnconnectedDevicesAsync(CancellationToken token)
        {
            // Wait a bit after startup before first retry so initial connections can settle
            await Task.Delay(TimeSpan.FromSeconds(30), token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await PingAndConnectKnownDevicesAsync();
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    LogToUI($"[Retry] Error in retry loop: {ex.Message}");
                }

                // Check every 60 seconds
                await Task.Delay(TimeSpan.FromSeconds(60), token).ConfigureAwait(false);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopServer();
            _formCts.Cancel();
            CloseLogFiles();
            base.OnFormClosing(e);
        }


        public async Task<bool> SendJsonToDevice(string sn, string json)
        {
            if (AppSession.RegisteredDevices.TryGetValue(sn, out var socket) && socket.State == WebSocketState.Open)
            {
                try
                {
                    LogToUI($"[SENDING] Raw Data: {json}", LogType.Outgoing);
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
                    return true;
                }
                catch (Exception ex)
                {
                    LogToUI($"[SEND-ERR] {sn}: {ex.Message}");
                    return false;
                }
            }
            LogToUI($"[OFFLINE] Device {sn} is not connected.");
            return false;
        }


        /// <summary>
        /// Specifically handles GET requests or commands where the actual device response body is required.
        /// </summary>
        public static async Task<string> ExecuteServiceGetCommand(string sn, string methodName, string jsonParams)
        {
            if (Instance == null) return null;

            string jsonToDevice = null;
            JObject data;
            try { data = string.IsNullOrEmpty(jsonParams) ? new JObject() : JObject.Parse(jsonParams.Trim()); }
            catch { data = new JObject(); Instance?.LogToUI($"[CMD-ERR] Invalid JSON params: {jsonParams}"); }

            // Extract enrollid if it exists to build a specific response key
            int? enrollId = data["enrollid"]?.Value<int?>();

            try
            {
                // retCmd is the 'ret' value the device will send back (may differ from method name)
                string retCmd = methodName.ToLower();

                switch (retCmd)
                {
                    case "getusername":
                        jsonToDevice = Instance._services.getusername(sn, data["enrollid"]?.Value<int>() ?? 0);
                        break;
                    case "getuserinfo":
                        jsonToDevice = Instance._services.getuserinfo(sn,
                            data["enrollid"]?.Value<int>() ?? 0,
                            data["backupnum"]?.Value<int>() ?? 0);
                        break;
                    case "getuserdetails":
                    {
                        int eid = data["enrollid"]?.Value<int>() ?? 0;
                        string infoRaw = await ExecuteServiceGetCommand(sn, "getuserinfo", $"{{\"enrollid\":{eid},\"backupnum\":0}}");
                        string faceRaw = await ExecuteServiceGetCommand(sn, "getuserinfo", $"{{\"enrollid\":{eid},\"backupnum\":50}}");
                        JObject info = string.IsNullOrEmpty(infoRaw) ? new JObject() : JObject.Parse(infoRaw);
                        JObject face = string.IsNullOrEmpty(faceRaw) ? new JObject() : JObject.Parse(faceRaw);
                        info["facerecord"] = face["record"];
                        info["backupnum"] = 0;
                        return info.ToString(Formatting.None);
                    }
                    case "getalllogs":
                        jsonToDevice = Instance._services.getalllogs(sn);
                        retCmd = "getalllog"; // device responds with ret=getalllog
                        break;
                    default:
                        return null;
                }

                if (string.IsNullOrEmpty(jsonToDevice)) return null;

                // Setup the waiter
                var tcs = new TaskCompletionSource<string>();
                string baseKey = $"{sn}|{retCmd}";
                string specificKey = enrollId.HasValue ? $"{baseKey}|{enrollId.Value}" : null;

                Instance._pendingResponses[baseKey] = tcs;
                if (specificKey != null) Instance._pendingResponses[specificKey] = tcs;

                try
                {
                    bool sent = await Instance.SendJsonToDevice(sn, jsonToDevice);
                    if (!sent) return null;

                    // Wait for the response (15s timeout)
                    var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(15000));
                    if (completedTask == tcs.Task)
                    {
                        return await tcs.Task;
                    }
                    else
                    {
                        Instance.LogToUI($"[GET-TIMEOUT] {methodName} for {sn}");
                        return "{\"result\":false,\"reason\":\"timeout\"}";
                    }
                }
                catch (OperationCanceledException)
                {
                    Instance.LogToUI($"[CMD-CANCELLED] {methodName} for {sn} — device disconnected.");
                    return "{\"result\":false,\"reason\":\"disconnected\"}";
                }
                finally
                {
                    Instance._pendingResponses.TryRemove(baseKey, out _);
                    if (specificKey != null) Instance._pendingResponses.TryRemove(specificKey, out _);
                }
            }
            catch (Exception ex)
            {
                Instance.LogToUI($"[GET-ERROR] {methodName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Executes a command from anywhere in the system. Returns TRUE if sent successfully.
        /// This method acts as a router, mapping method names to WebServerServices payload generation.
        /// </summary>
        public static async Task<bool> ExecuteServiceCommand(string sn, string methodName, string jsonParams)
        {
            if (Instance == null) return false;

            // Optional: If you want getters called via this method to also await,
            // you can handle it here, but per your request, I've separated them.

            string jsonToDevice = null;
            JObject data;
            try { data = string.IsNullOrEmpty(jsonParams) ? new JObject() : JObject.Parse(jsonParams.Trim()); }
            catch { data = new JObject(); Instance?.LogToUI($"[CMD-ERR] Invalid JSON params: {jsonParams}"); }

            try
            {
                switch (methodName.ToLower())
                {
                    // --- Information Retrieval (Getters) ---
                    // Route these to the specialized getter handler if needed, 
                    // or just send and return true based on delivery.
                    case "getusername":
                    case "getuserinfo":
                        string response = await ExecuteServiceGetCommand(sn, methodName, jsonParams);
                        return !string.IsNullOrEmpty(response) && !response.Contains("\"reason\":\"timeout\"");

                    // --- User Data Management (Setters) ---
                    case "setusername":
                        if (data["records"] is JArray arr)
                        {
                            var users = arr.ToObject<List<UserRecord>>();
                            jsonToDevice = Instance._services.setusername(sn, users);
                        }
                        else
                        {
                            jsonToDevice = Instance._services.setusername(sn,
                                data["enrollid"]?.Value<int>() ?? 0,
                                data["name"]?.ToString());
                        }
                        break;
                    case "setface":
                        jsonToDevice = await Instance._services.setface(sn,
                            data["enrollid"]?.Value<int>() ?? 0,
                            data["name"]?.ToString(),
                            data["record"]
                        );
                        break;
                    case "setrfid":
                        jsonToDevice = Instance._services.setrfid(sn,
                            data["enrollid"]?.Value<int>() ?? 0,
                            data["name"]?.ToString(),
                            data["record"]);
                        break;
                    case "setfingerprint":
                        jsonToDevice = Instance._services.setfingerprint(sn,
                            data["enrollid"]?.Value<int>() ?? 0,
                            data["name"]?.ToString(),
                            data["record"],
                            data["backupnum"]?.Value<int>() ?? 0);
                        break;
                    case "setuserinfo":
                        jsonToDevice = Instance._services.setuserinfo(sn,
                            data["enrollid"]?.Value<int>(),
                            data["name"]?.ToString(),
                            data["backupnum"]?.Value<int>(),
                            data["admin"]?.Value<int>(),
                            data["record"]);
                        break;

                    // --- User status & Deletion ---
                    case "enableuser":
                        jsonToDevice = Instance._services.enableuser(sn,
                            data["enrollid"]?.Value<int>() ?? 0,
                            data["enflag"]?.Value<int>() ?? 0);
                        break;
                    case "set_enable":
                        jsonToDevice = Instance._services.set_enable(sn, data["enrollid"]?.Value<int>() ?? 0);
                        break;
                    case "set_disable":
                        jsonToDevice = Instance._services.set_disable(sn, data["enrollid"]?.Value<int>() ?? 0);
                        break;
                    case "deleteuser":
                        jsonToDevice = Instance._services.deleteuser(sn,
                            data["enrollid"]?.Value<int>() ?? 0,
                            data["backupnum"]?.Value<int>() ?? 0);
                        break;

                    // --- Maintenance & Control ---
                    case "opendoor":
                        jsonToDevice = Instance._services.opendoor(sn, data["doornum"]?.Value<int>());
                        break;
                    case "cleanuser":
                        jsonToDevice = Instance._services.cleanuser(sn);
                        break;
                    case "cleanlog":
                        jsonToDevice = Instance._services.cleanlog(sn);
                        break;

                    default:
                        jsonToDevice = jsonParams;
                        break;
                }

                if (!string.IsNullOrEmpty(jsonToDevice))
                {
                    // Correlate against the device's actual acknowledgment rather than just the socket write.
                    // The ack's "ret" comes from the "cmd" embedded in the outgoing payload — not necessarily
                    // methodName (e.g. setrfid/setfingerprint/setface all send cmd="setuserinfo",
                    // and set_enable/set_disable send cmd="enableuser").
                    string ackCmd = null;
                    int? ackEnrollId = null;
                    try
                    {
                        var toDeviceObj = JObject.Parse(jsonToDevice);
                        ackCmd = toDeviceObj["cmd"]?.ToString()?.ToLower();
                        ackEnrollId = toDeviceObj["enrollid"]?.Value<int?>();
                    }
                    catch { /* fall through to send-only below */ }

                    if (!string.IsNullOrEmpty(ackCmd))
                    {
                        var ack = await SendCommandAndAwaitResultAsync(sn, ackCmd, ackEnrollId, jsonToDevice);
                        if (ack == null)
                        {
                            // Send failed, device offline, or no acknowledgment within the timeout.
                            Instance.LogToUI($"[CMD-NOACK] {methodName} for {sn}: no acknowledgment received.");
                            return false;
                        }

                        bool ackResult = ack["result"]?.Value<bool>() ?? false;
                        if (!ackResult)
                        {
                            Instance.LogToUI($"[CMD-REJECTED] {methodName} for {sn}: reason={ack["reason"]} msg={ack["msg"]}");
                        }
                        return ackResult;
                    }

                    // No "cmd" could be determined (e.g. raw passthrough) — best-effort send-only.
                    return await Instance.SendJsonToDevice(sn, jsonToDevice);
                }
            }
            catch (Exception ex)
            {
                Instance.LogToUI($"[EXEC-ERROR] {methodName} for {sn}: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Sends a command to the device and awaits its correlated acknowledgment (by "ret" + enrollid),
        /// using the same _pendingResponses mechanism ExecuteServiceGetCommand uses for getters.
        /// Returns the parsed ack JObject, or null if the send failed or no ack arrived within the timeout.
        /// </summary>
        private static async Task<JObject> SendCommandAndAwaitResultAsync(string sn, string ackCmd, int? enrollId, string jsonToDevice, int timeoutMs = 15000)
        {
            if (Instance == null || string.IsNullOrEmpty(jsonToDevice)) return null;

            string baseKey = $"{sn}|{ackCmd}";
            string specificKey = enrollId.HasValue ? $"{baseKey}|{enrollId.Value}" : null;

            var tcs = new TaskCompletionSource<string>();
            Instance._pendingResponses[baseKey] = tcs;
            if (specificKey != null) Instance._pendingResponses[specificKey] = tcs;

            try
            {
                bool sent = await Instance.SendJsonToDevice(sn, jsonToDevice);
                if (!sent) return null;

                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
                if (completedTask != tcs.Task)
                {
                    Instance.LogToUI($"[CMD-TIMEOUT] {ackCmd} for {sn}");
                    return null;
                }

                string raw = await tcs.Task;
                try { return JObject.Parse(raw); }
                catch { return null; }
            }
            finally
            {
                Instance._pendingResponses.TryRemove(baseKey, out _);
                if (specificKey != null) Instance._pendingResponses.TryRemove(specificKey, out _);
            }
        }

        private static readonly HttpClient _imageProcessClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        private const string ImageProcessUrl = "http://10.20.69.81:5000/process";

        // Independent path: any enroll number with an image gets forwarded to the image-processing API.
        // Fire-and-forget so it never blocks or affects the staff/member attendance flow below.
        private void ForwardImageForProcessing(int devId, int enroll, string image)
        {
            if (string.IsNullOrEmpty(image)) return;

            _ = Task.Run(async () =>
            {
                try
                {
                    var payload = new JObject
                    {
                        ["images"] = new JArray(image)
                    };

                    var content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
                    var response = await _imageProcessClient.PostAsync(ImageProcessUrl, content);
                    string body = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                        LogToUI($"[IMG-PROCESS] DevID: {devId} | EnrollID: {enroll} | Result: {body}");
                    else
                        LogToUI($"[IMG-PROCESS] DevID: {devId} | EnrollID: {enroll} | Failed ({(int)response.StatusCode}): {body}");
                }
                catch (Exception ex)
                {
                    LogToUI($"[IMG-PROCESS] DevID: {devId} | EnrollID: {enroll} | Error: {ex.Message}");
                }
            });
        }

        private void ForwardAttendanceData(int devId, string ip, int enroll, DateTime time, string name = null, string image = null)
        {
            // Normalize IP (strip port if present)
            string ipOnly = ip ?? string.Empty;
            int colon = ipOnly.IndexOf(':');
            if (colon >= 0) ipOnly = ipOnly.Substring(0, colon);

            //ForwardImageForProcessing(devId, enroll, image);

            try
            {
                // Look up device category from in-memory session
                bool exists = AppSession.DeviceCategoryMap.TryGetValue(ipOnly, out var category);
                bool isStaffDevice = exists && string.Equals(category.type, "staff", StringComparison.OrdinalIgnoreCase);

                //// Unknown visitor (99999999) or staff enrollment ID — only from member devices
                //if (!isStaffDevice && (enroll == 99999999 || AppSession.StaffEnrollmentIds.Contains(enroll.ToString())))
                //{
                //    _attendanceFunctions.HandleUnknownOrStaffScan(enroll, name, image, ip, time);
                //}

                if (isStaffDevice)
                {
                    var srec = new StaffAttendenceServerFunctions.StaffAttendanceRecord
                    {
                        DevID = devId, IP = ip, Enroll = enroll, Time = time
                    };
                    _staffAttendanceFunctions.Process(srec);
                    return;
                }

                if (AppSession.StaffEnrollmentIds.Contains(enroll.ToString()))
                {
                    return;
                }

                var mrec = new AttendenceServerFunctions.AttendanceRecord
                {
                    DevID = devId, IP = ip, Enroll = enroll, Time = time
                };
                _attendanceFunctions.Process(mrec);
            }
            catch (Exception ex)
            {
                LogToUI($"[Forward Error] {ex.Message}");
            }
        }
    }
}