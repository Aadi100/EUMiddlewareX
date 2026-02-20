using AxFPCLOCK_SVRLib;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class AttendanceServerForm : Form
    {
        // Singleton infrastructure 
        private static readonly object _sync = new object();
        private static AttendanceServerForm _instance;
        private static Exception _lastInitException;

        private readonly AttendenceServerFunctions _attendanceFunctions = new AttendenceServerFunctions();
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();
        private readonly StaffAttendenceServerFunctions _staffAttendanceFunctions = new StaffAttendenceServerFunctions();


        // Add this method to your AttendanceServerForm class
        public void SetWebSocketManager(WebSocketManager wsManager)
        {
            if (wsManager == null)
            {
                LogHelper.Log("[AttendanceServer] Cannot set null WebSocketManager");
                return;
            }

            // Pass it to the AttendenceServerFunctions instance
            _attendanceFunctions.SetWebSocketManager(wsManager);
            LogHelper.Log("[AttendanceServer] WebSocketManager has been set");
        }

        /// <summary>
        /// Public entry-point for test/simulation to inject synthetic attendance events through the same pipeline
        /// used by real devices. Runs on the server UI thread.
        /// </summary>
        public static void SimulateAttendance(int devId, string ipWithPort, int enroll, DateTime time)
        {
            try
            {
                ExecuteOnServerThread(() =>
                {
                    try
                    {
                        var inst = Instance;
                        if (inst == null || inst.IsDisposed)
                        {
                            LogHelper.Log("[AttendanceServer] SimulateAttendance ignored: instance not ready.");
                            return;
                        }
                        inst.ForwardAttendanceData(devId, ipWithPort, enroll, time);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log("[AttendanceServer] SimulateAttendance inner error: " + ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServer] SimulateAttendance error: " + ex.Message);
            }
        }

        /// <summary>
        /// Access current instance (may be null if not created or disposed).
        /// </summary>
        public static AttendanceServerForm Instance
        {
            get { lock (_sync) return _instance; }
        }

        /// <summary>
        /// True when the singleton form exists and is not disposed.
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                lock (_sync)
                {
                    return _instance != null && !_instance.IsDisposed;
                }
            }
        }

        /// <summary>
        /// Last initialization exception (if any).
        /// </summary>
        public static Exception LastInitializationException
        {
            get { lock (_sync) return _lastInitException; }
        }

        /// <summary>
        /// Initialize (optionally auto-start). Safe to call multiple times.
        /// </summary>
        public static bool InitializeGlobal(bool startImmediately = true, bool showUI = false)
        {
            if (!EnsureInstance())
                return false;

            if (startImmediately)
                StartServer(showUI);

            return true;
        }

        public static void StartServer(bool showUI = false)
        {
            if (!EnsureInstance())
            {
                LogHelper.Log("[AttendanceServer] StartServer aborted (instance init failed).");
                return;
            }

            var inst = Instance;
            if (inst == null || inst.IsDisposed)
            {
                LogHelper.Log("[AttendanceServer] StartServer aborted (instance unavailable).");
                return;
            }

            try
            {
                if (inst.InvokeRequired)
                    inst.BeginInvoke(new Action(() => inst.StartCore(showUI)));
                else
                    inst.StartCore(showUI);
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServer] StartServer marshal failed: " + ex.Message);
            }
        }

        public static bool ExecuteOnServerThread(Action action, bool ensureStartIfNeeded = true)
        {
            if (action == null) return false;

            if (ensureStartIfNeeded && !EnsureInstance())
            {
                LogHelper.Log("[AttendanceServer] ExecuteOnServerThread aborted (init failed).");
                return false;
            }

            var inst = Instance;
            if (inst == null || inst.IsDisposed)
            {
                LogHelper.Log("[AttendanceServer] ExecuteOnServerThread aborted (instance missing).");
                return false;
            }

            try
            {
                if (inst.InvokeRequired)
                    inst.BeginInvoke(action);
                else
                    action();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServer] ExecuteOnServerThread marshal error: " + ex.Message);
                return false;
            }
        }

        public static void StopServer()
        {
            AttendanceServerForm local;
            lock (_sync)
            {
                if (_instance == null)
                {
                    LogHelper.Log("[AttendanceServer] StopServer requested but instance not created.");
                    return;
                }
                local = _instance;
                _instance = null;
            }

            try
            {
                void DoStop()
                {
                    try
                    {
                        LogHelper.Log("[AttendanceServer] Stop requested.");
                        local.InternalStop();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log("[AttendanceServer] InternalStop error: " + ex.Message);
                    }
                }

                if (local.IsDisposed) return;

                if (local.InvokeRequired)
                    local.BeginInvoke((Action)DoStop);
                else
                    DoStop();
            }
            finally
            {
                LogHelper.Log("[AttendanceServer] Stopped and cleaned up (no dedicated thread).");
            }
        }

        private static bool EnsureInstance()
        {
            lock (_sync)
            {
                if (_instance != null && !_instance.IsDisposed)
                    return true;

                try
                {
                    _lastInitException = null;
                    _instance = new AttendanceServerForm();

                    // Keep hidden unless explicitly shown
                    _instance.Load += (s, e) =>
                    {
                        if (!_instance._autoMode && !_instance.Visible)
                        {
                            // Only show if requested later via StartServer(showUI:true)
                            _instance.Hide();
                        }
                    };

                    _instance.FormClosed += (s, e) =>
                    {
                        lock (_sync)
                        {
                            if (ReferenceEquals(_instance, s))
                                _instance = null;
                        }
                        LogHelper.Log("[AttendanceServer] Instance FormClosed.");
                    };

                    return true;
                }
                catch (Exception ex)
                {
                    _lastInitException = ex;
                    LogHelper.Log("[AttendanceServer] Initialization failed: " + ex);
                    MessageBox.Show("Failed to initialize Attendance Server: " + ex.Message, "Initialization Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        // Instance state
        private int nIndex = 0;
        private int Recordnumber = 0;
        private int _openedPort;
        private bool _connected;
        private bool _autoMode;

        private AttendanceServerForm()
        {
            InitializeComponent();

            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;

            textPort.Text = "7005";
            textPort.ReadOnly = true;

            if (listView1.Columns.Count == 0)
            {
                listView1.Columns.Add(" ", 40, HorizontalAlignment.Left);
                listView1.Columns.Add("EnrollNo", 100, HorizontalAlignment.Left);
                listView1.Columns.Add("VerifyMode", 100, HorizontalAlignment.Left);
                listView1.Columns.Add("InOut", 60, HorizontalAlignment.Left);
                listView1.Columns.Add("DateTime", 140, HorizontalAlignment.Left);
                listView1.Columns.Add("IP", 130, HorizontalAlignment.Left);
                listView1.Columns.Add("Port", 60, HorizontalAlignment.Left);
                listView1.Columns.Add("DevID", 60, HorizontalAlignment.Left);
                listView1.Columns.Add("SerialNo", 60, HorizontalAlignment.Left);
            }

            LogHelper.Log("[AttendanceServer] Form constructed (main UI thread).");
        }

        private void StartCore(bool showUI)
        {
            _autoMode = !showUI;
            LogHelper.Log($"[AttendanceServer] StartCore showUI={showUI} connected={_connected}");

            if (!_connected)
                TryOpen();
            else
                LogHelper.Log("[AttendanceServer] Already connected.");

            if (showUI)
            {
                if (!Visible)
                {
                    Show();
                    BringToFront();
                }
                else
                {
                    BringToFront();
                }
            }
            else
            {
                if (Visible) Hide();
            }
        }

        private void InternalStop()
        {
            LogHelper.Log("[AttendanceServer] InternalStop executing.");
            TryClose();
            if (!IsDisposed)
            {
                try { Close(); } catch (Exception ex) { LogHelper.Log("[AttendanceServer] Close error: " + ex.Message); }
            }
        }

        private void TryOpen()
        {
            try
            {
                LogHelper.Log("[AttendanceServer] Opening network listener...");

                if (!int.TryParse(textPort.Text.Trim(), out _openedPort) || _openedPort <= 0)
                    throw new Exception("Invalid port.");

                axFPCLOCK_Svr1.OpenNetwork(_openedPort);
                _connected = true;
                LogHelper.Log($"[AttendanceServer] Listener opened on port {_openedPort}.");
            }
            catch (Exception ex)
            {
                _connected = false;
                LogHelper.Log("[AttendanceServer] Failed to open port: " + ex.Message);
                if (!_autoMode)
                {
                    try
                    {
                        MessageBox.Show("Failed to open server port: " + ex.Message, "Server Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch { }
                }
            }
        }

        private void TryClose()
        {
            if (!_connected)
            {
                LogHelper.Log("[AttendanceServer] TryClose ignored (not connected).");
                return;
            }
            try
            {
                axFPCLOCK_Svr1.CloseNetwork(_openedPort);
                LogHelper.Log($"[AttendanceServer] Listener closed on port {_openedPort}.");
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServer] Error closing network: " + ex.Message);
            }
            _connected = false;
        }

        private void ForwardAttendanceData(int devId, string ip, int enroll, DateTime time)
        {
            // Normalize IP (strip port if present)
            string ipOnly = ip ?? string.Empty;
            int colon = ipOnly.IndexOf(':');
            if (colon >= 0) ipOnly = ipOnly.Substring(0, colon);

            try
            {
                // Use DatabaseHelper to determine whether this IP belongs to a staff or member device
                var category = _dbHelper.GetDeviceCategoryByIp(ipOnly);

                if (category.exists && string.Equals(category.type, "staff", StringComparison.OrdinalIgnoreCase))
                {
                    // Route to staff pipeline
                    var srec = new StaffAttendenceServerFunctions.StaffAttendanceRecord
                    {
                        DevID = devId,
                        IP = ip,
                        Enroll = enroll,
                        Time = time
                    };
                    _staffAttendanceFunctions.Process(srec);
                    return;
                }

                // Default / member pipeline
                var mrec = new AttendenceServerFunctions.AttendanceRecord
                {
                    DevID = devId,
                    IP = ip,
                    Enroll = enroll,
                    Time = time
                };
                _attendanceFunctions.Process(mrec);
            }
            catch (Exception ex)
            {
                // On any error, fallback to member pipeline to avoid losing events
                LogHelper.Log($"[ForwardAttendanceData] Routing error for IP={ip}: {ex.Message}. Falling back to member pipeline.");
                var fallback = new AttendenceServerFunctions.AttendanceRecord
                {
                    DevID = devId,
                    IP = ip,
                    Enroll = enroll,
                    Time = time
                };
                _attendanceFunctions.Process(fallback);
            }
        }


        // Device event
        private void axFPCLOCK_Svr1_OnReceiveGLogData(object sender, _DFPCLOCK_SvrEvents_OnReceiveGLogDataEvent e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, _DFPCLOCK_SvrEvents_OnReceiveGLogDataEvent>(axFPCLOCK_Svr1_OnReceiveGLogData), sender, e);
                return;
            }

            if (e.anSEnrollNumber == 0)
            {
                try { axFPCLOCK_Svr1.SendResultandTime(e.linkindex, e.vnDeviceID, e.anSEnrollNumber, 1); } catch { }
                return;
            }
            string rawSummary =
                $"link={e.linkindex}, DevID={e.vnDeviceID}, SN={e.anSN}, IP={e.astrDeviceIP}:{e.anDevicePort}, " +
                $"Enroll={e.anSEnrollNumber}, VerifyMode={e.anVerifyMode}, InOut={e.anInOutMode}, Time={e.anLogDate:yyyy-MM-dd HH:mm:ss}";
            LogHelper.Log("[AttendanceServer] ▼ Received GLogData: " + rawSummary);

            try
            {
                axFPCLOCK_Svr1.SendResultandTime(e.linkindex, e.vnDeviceID, e.anSEnrollNumber, 1);
                LogHelper.Log($"[AttendanceServer] Ack sent (DevID={e.vnDeviceID}, Enroll={e.anSEnrollNumber}).");
            }
            catch (Exception ackEx)
            {
                LogHelper.Log("[AttendanceServer] Ack failed: " + ackEx.Message);
            }
            try
            {
                string strKey = (nIndex + 1).ToString();
                string enrollStr = e.anSEnrollNumber.ToString("D8");
                long dwCardNum1;
                double aTemperature;

                byte[] imageBuffer = null;
                string imagePath = null;

                if (e.anSEnrollNumber > 0)
                {
                    IntPtr ptr = IntPtr.Zero;
                    try
                    {
                        int imagelen = 0;
                        ptr = Marshal.AllocHGlobal(200 * 1024);
                        bool ok = axFPCLOCK_Svr1.GetLogImageCS(e.linkindex, ref imagelen, ptr);
                        if (ok && imagelen > 0)
                        {
                            imageBuffer = new byte[imagelen];
                            Marshal.Copy(ptr, imageBuffer, 0, imagelen);
                            imagePath = $@"C:\\PHOTO\\{e.anSEnrollNumber}_{e.anLogDate:yy_MM_dd_HH_mm_ss}.jpg";
                            LogHelper.Log($"[AttendanceServer] Image captured (bytes={imagelen}) -> {imagePath}");
                        }
                        else
                        {
                            LogHelper.Log($"[AttendanceServer] No image captured (ok={ok}, imagelen={imagelen}).");
                        }
                    }
                    catch (Exception imgEx)
                    {
                        LogHelper.Log("[AttendanceServer] Image capture error: " + imgEx.Message);
                    }
                    finally
                    {
                        if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
                    }
                }

                listView1.BeginUpdate();
                try
                {
                    var lvi = new ListViewItem { Text = strKey };

                    if (e.anSEnrollNumber < 0)
                    {
                        dwCardNum1 = e.anSEnrollNumber + 4294967296;
                        enrollStr = dwCardNum1.ToString();
                    }
                    lvi.SubItems.Add(enrollStr);

                    string verifyStr;
                    if (e.anVerifyMode > 40)
                    {
                        aTemperature = (250 + e.anVerifyMode) / 10.0;
                        verifyStr = aTemperature.ToString("#0.0");
                    }
                    else
                    {
                        if (e.anSEnrollNumber < 0)
                        {
                            dwCardNum1 = e.anSEnrollNumber + 4294967296;
                            verifyStr = FormStringlong(e.anVerifyMode, dwCardNum1);
                        }
                        else
                        {
                            verifyStr = FormString(e.anVerifyMode, e.anSEnrollNumber);
                        }
                    }
                    lvi.SubItems.Add(verifyStr);

                    string inout = e.anInOutMode == 1 ? "OUT" : (e.anInOutMode == 0 ? "IN" : "--");
                    lvi.SubItems.Add(inout);

                    string ts = e.anLogDate.ToString("yyyy/MM/dd HH:mm:ss");
                    lvi.SubItems.Add(ts);
                    lvi.SubItems.Add(e.astrDeviceIP);
                    lvi.SubItems.Add(e.anDevicePort.ToString());
                    lvi.SubItems.Add(e.vnDeviceID.ToString());
                    lvi.SubItems.Add(e.anSN.ToString());

                    listView1.Items.Add(lvi);
                    listView1.EnsureVisible(nIndex);

                    LogHelper.Log($"[AttendanceServer] ▲ Listed record #{strKey}: Enroll={enrollStr}, Verify={verifyStr}, InOut={inout}, Time={ts}, DevID={e.vnDeviceID}, SN={e.anSN}");
                }
                finally
                {
                    listView1.EndUpdate();
                }

                //forward only required minimal data (DevID, IP:Port, Enroll, Time)
                try
                {
                    ForwardAttendanceData(
                        e.vnDeviceID,
                        e.astrDeviceIP + ":" + e.anDevicePort,
                        e.anSEnrollNumber,
                        e.anLogDate);
                }
                catch (Exception fwdEx)
                {
                    LogHelper.Log("[AttendanceServer] ForwardAttendanceData error: " + fwdEx.Message);
                }

               

                nIndex++;
                if (nIndex > 1000)
                {
                    LogHelper.Log("[AttendanceServer] Clearing list (max reached).");
                    nIndex = 0;
                    listView1.Items.Clear();
                }

                if (imageBuffer != null && imagePath != null)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            Directory.CreateDirectory(@"C:\\PHOTO");
                            File.WriteAllBytes(imagePath, imageBuffer);
                            LogHelper.Log("[AttendanceServer] Image written: " + imagePath);
                        }
                        catch (Exception ioEx)
                        {
                            LogHelper.Log("[AttendanceServer] Image write error: " + ioEx.Message);
                        }
                    });
                }
            }
            catch (Exception exOuter)
            {
                LogHelper.Log("[AttendanceServer] Handler exception: " + exOuter.Message);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) { }

        public string FormString(int nVerify, int nEnrollNum)
        {
            int nAction = nVerify % 8;
            if (nEnrollNum == 0)
            {
                switch (nAction)
                {
                    case 0: return "Closed";
                    case 1: return "Opened";
                    case 2: return "HandOpen";
                    case 3: return "ProcOpen";
                    case 4: return "ProcClose";
                    case 5: return "IllegalOpen";
                    case 6: return "IlleagalRemove";
                    case 7: return "Alarm";
                    case 8: return "--";
                }
            }
            return nVerify.ToString();
        }

        public string FormStringlong(int nVerify, long nEnrollNum)
        {
            int nAction = nVerify % 8;
            if (nEnrollNum == 0)
            {
                switch (nAction)
                {
                    case 0: return "Closed";
                    case 1: return "Opened";
                    case 2: return "HandOpen";
                    case 3: return "ProcOpen";
                    case 4: return "ProcClose";
                    case 5: return "IllegalOpen";
                    case 6: return "IlleagalRemove";
                    case 7: return "Alarm";
                    case 8: return "--";
                }
            }
            return nVerify.ToString();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            LogHelper.Log("[AttendanceServer] Form closing.");
            TryClose();
            base.OnFormClosing(e);
        }

        private void AttendanceServerForm_Load(object sender, EventArgs e)
        {

        }
    }
}