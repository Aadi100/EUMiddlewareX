using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace smart_attendance_middleware
{
    public partial class WebsocketServer : Form
    {
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

        private async void WebsocketServer_Load(object sender, EventArgs e)
        {
            UpdateStatus("Status: Initializing...");
            LogToUI("--- AUTOMATIC STARTUP INITIATED ---");

            // 1. Perform Network Scan
            await ScanForDevicesAsync();

            // 2. Start WebSocket Server
            StartServer();
        }

        private void StartServer()
        {
            if (_isListening) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://*:7788/");
                _listener.Start();
                _isListening = true;

                LogToUI("WebSocket Server Started on port 7788...");
                UpdateStatus("Status: Server Running (Port 7788)");

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
            try
            {
                wsContext = await context.AcceptWebSocketAsync(subProtocol: null);
                LogToUI($"New Incoming Connection Request from {context.Request.RemoteEndPoint}");
            }
            catch (Exception ex)
            {
                LogToUI($"WebSocket Acceptance Error: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Close();
                return;
            }

            WebSocket socket = wsContext.WebSocket;
            string currentSn = null;
            byte[] buffer = new byte[1024 * 64];
            CancellationTokenSource connectionCts = CancellationTokenSource.CreateLinkedTokenSource(_formCts.Token);

            try
            {
                while (socket.State == WebSocketState.Open && _isListening)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), connectionCts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", connectionCts.Token);
                        break;
                    }

                    if (result.MessageType != WebSocketMessageType.Text) continue;

                    string jsonIncoming = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    LogToUI($"[INCOMING] Raw Data: {jsonIncoming}");

                    JObject doc = null;
                    try { doc = JObject.Parse(jsonIncoming); } catch { continue; }

                    string cmd = doc["cmd"]?.ToString();
                    if (string.Equals(cmd, "reg", StringComparison.OrdinalIgnoreCase))
                    {
                        string sn = doc["sn"]?.ToString();
                        if (string.IsNullOrWhiteSpace(sn)) continue;

                        // Check if device is already registered in the AppSession
                        if (AppSession.RegisteredDevices.ContainsKey(sn))
                        {
                            LogToUI($"[REG] Device {sn} already has an active session.");
                        }

                        var match = _dbDevices.FirstOrDefault(d => string.Equals(d.DeviceSerial, sn, StringComparison.OrdinalIgnoreCase));
                        if (match == null)
                        {
                            var failResponse = new { ret = "reg", result = false, reason = "Device not in Database" };
                            await SendToSocket(socket, failResponse, connectionCts.Token);
                            await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Device not in DB", connectionCts.Token);
                            break;
                        }

                        currentSn = sn;
                        
                        // Use AppSession instead of local _manager dictionary
                        AppSession.RegisteredDevices[sn] = socket;
                        
                        _deviceStates[sn] = doc;

                        LogToUI($"[REG] Device {sn} registered.");
                        UpdateStatus($"Status: Device {sn} Connected");

                        var response = new { ret = "reg", result = true, cloudtime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") };
                        await SendToSocket(socket, response, connectionCts.Token);

                        // Start heartbeat loop again - the hardware REQUIRES activity every 20s
                        _ = Task.Run(() => SendHeartbeatLoop(sn, socket, connectionCts.Token));
                    }
                    else if (string.Equals(cmd, "heartbeat", StringComparison.OrdinalIgnoreCase))
                    {
                        var pong = new { ret = "heartbeat", result = true };
                        await SendToSocket(socket, pong, connectionCts.Token);
                    }
                    else if (string.Equals(cmd, "sendlog", StringComparison.OrdinalIgnoreCase))
                    {
                        string snForLog = doc["sn"]?.ToString() ?? currentSn;
                        int logCount = doc["count"]?.Value<int>() ?? 0;
                        
                        LogToUI($"[LOG] Received {logCount} records from {snForLog}");
                        
                        // CRITICAL: Acknowledge the log receipt or the device will disconnect!
                        var logAck = new { 
                            ret = "sendlog", 
                            result = true, 
                            sn = snForLog,
                            count = logCount 
                        };
                        await SendToSocket(socket, logAck, connectionCts.Token);
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
                if (!string.IsNullOrEmpty(currentSn))
                {
                    AppSession.RegisteredDevices.TryRemove(currentSn, out _);
                }
                socket.Dispose();
                connectionCts.Dispose();
                LogToUI($"Connection with {(currentSn ?? "Unknown")} closed.");
            }
        }

        private async Task SendToSocket(WebSocket socket, object payload, CancellationToken token)
        {
            try
            {
                string json = JsonConvert.SerializeObject(payload);
                LogToUI($"[SENDING] Raw Data: {json}");
                byte[] data = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, token);
            }
            catch { }
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

        private void LogToUI(string message)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke((MethodInvoker)delegate { LogToUI(message); }); return; }
            if (txtLogs.TextLength > 50000) { txtLogs.Select(0, 10000); txtLogs.SelectedText = "[...]\r\n"; }
            txtLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        public async Task SendJsonToDevice(string sn, string json)
        {
            if (AppSession.RegisteredDevices.TryGetValue(sn, out var socket) && socket.State == WebSocketState.Open)
            {
                LogToUI($"[SENDING] Raw Data: {json}");
                byte[] data = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                LogToUI($"[COMMAND-ERROR] Device {sn} disconnected.");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopServer();
            _formCts.Cancel();
            base.OnFormClosing(e);
        }

        private async Task ConnectToDeviceAndRegister(string ip, string serialNumber)
        {
            var uri = new Uri($"ws://{ip}:7788/pub/chat");
            using (ClientWebSocket socket = new ClientWebSocket())
            {
                try
                {
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        await socket.ConnectAsync(uri, cts.Token);

                    var regMessage = new { cmd = "reg", sn = serialNumber };
                    await SendToSocket(socket, regMessage, CancellationToken.None);

                    byte[] buffer = new byte[1024 * 8];
                    while (socket.State == WebSocketState.Open && !_formCts.IsCancellationRequested)
                    {
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), _formCts.Token);
                        if (result.MessageType == WebSocketMessageType.Close) break;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Public static wrapper called from anywhere.
        /// Example: await WebsocketServer.ExecuteServiceCommand("SN123", "setusername", "{'enrollid': 111, 'name': 'Aadi'}");
        /// </summary>
        public static async Task ExecuteServiceCommand(string sn, string methodName, string jsonParams)
        {
            if (Instance == null) return;

            string jsonToDevice = null;
            JObject data = string.IsNullOrEmpty(jsonParams) ? new JObject() : JObject.Parse(jsonParams);

            switch (methodName.ToLower())
            {
                case "setusername":
                    jsonToDevice = Instance._services.setusername(
                        sn, 
                        data["enrollid"]?.Value<int>() ?? 0, 
                        data["name"]?.ToString()
                    );
                    break;
                case "setface":
                    jsonToDevice = await Instance._services.setface(
                        sn,
                        data["enrollid"]?.Value<int>() ?? 0,
                        data["name"]?.ToString(),
                        data["record"]
                    );
                    break;
                case "setrfid":
                    jsonToDevice = Instance._services.setrfid(
                        sn, 
                        data["enrollid"]?.Value<int>() ?? 0, 
                        data["name"]?.ToString(), 
                        data["record"]
                    );
                    break;
                case "setfingerprint":
                    jsonToDevice = Instance._services.setfingerprint(
                        sn, 
                        data["enrollid"]?.Value<int>() ?? 0, 
                        data["name"]?.ToString(), 
                        data["record"]
                    );
                    break;
                case "getusername":
                    jsonToDevice = Instance._services.getusername(sn, data["enrollid"]?.Value<int>() ?? 0);
                    break;
                case "enableuser":
                    jsonToDevice = Instance._services.enableuser(sn, data["enrollid"]?.Value<int>() ?? 0, data["enflag"]?.Value<int>() ?? 0);
                    break;
                case "cleanuser":
                    jsonToDevice = Instance._services.cleanuser(sn);
                    break;
                case "deleteuser":
                    jsonToDevice = Instance._services.deleteuser(sn, data["enrollid"]?.Value<int>() ?? 0, data["backupnum"]?.Value<int>() ?? 0);
                    break;
                default:
                    jsonToDevice = jsonParams; // Fallback to raw JSON
                    break;
            }

            if (!string.IsNullOrEmpty(jsonToDevice))
            {
                await Instance.SendJsonToDevice(sn, jsonToDevice);
            }
        }

    }
}