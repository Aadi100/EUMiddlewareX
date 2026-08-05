using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Controls;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;




namespace smart_attendance_middleware
{
    public partial class Dashboard : Form
    {

        //*******************************************************************************************
        // This is used to make the form draggable by clicking and dragging anywhere on the form.
        //*******************************************************************************************
        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0); // "this" refers to the form
            }
        }
        //*******************************************************************************************


        private bool internet = false;
        private bool biometric = false;
        private bool subscription = false;
        public bool Connected = false;
        public bool isExist = false;

        public DatabaseHelper _dbHelper;
        public WebSocketManager _wsManager;
        private Processor _processor;
        private DeviceFunctions _deviceFunctions = new DeviceFunctions();
        private CacheManager _cacheManager;
        private System.Windows.Forms.Timer countdownTimer;
        private int countdownSeconds;
        private string originalButtonText;
        private System.Windows.Forms.Timer attendancePanelTimer;
        private CancellationTokenSource _deviceMonitorCts;
        private DateTime _lastPanelRefresh = DateTime.MinValue;
        private readonly ToolTip _buttonTooltip = new ToolTip();
        private bool _uiReady = false;
        public HttpHandler _httpHandler;

        private readonly object _loaderLock = new object();
        private int _loaderRefCount = 0;


        private Panel loadingOverlay;
        private Label loadingLabel;
        private bool servicesInitialized = false;
        private smart_attendance_middleware.UI.LoaderPopupForm _loaderPopup;
        private BackupService _backupService;
        private bool _attendanceInitialSyncHandled = false;
        private FacePopupForm _facePopup;
        // DTO to carry computed sync status results
        public class SyncStatusResult
        {
            public int MembersSynced { get; set; }
            public int PendingBiometricJobs { get; set; }
            public int MissingFingerprints { get; set; }
        }


        public Dashboard()
        {
            InitializeComponent();

            

            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
            InitializeWebSocketManager();

            // Initialize CacheManager with 30-second refresh interval
            _cacheManager = new CacheManager(_dbHelper, refreshIntervalSeconds: 30);
            _cacheManager.CacheUpdated += OnCacheUpdated;

            // Create DeviceFunctions with the WebSocketManager
            _deviceFunctions = new DeviceFunctions(_wsManager);
            _processor = new Processor(_dbHelper, _wsManager, _deviceFunctions, _cacheManager);
            _httpHandler = new HttpHandler(_dbHelper, _processor);

            _wsManager.LiveUpdate += WsManager_LiveUpdate;

            // Check if debug mode is enabled from settings; default to false if missing or null
            string debugSetting = _dbHelper.GetSetting("DebugMode");
            bool isDebugEnabled = !string.IsNullOrEmpty(debugSetting) &&
                                (debugSetting == "1" || debugSetting.Equals("true", StringComparison.OrdinalIgnoreCase));
            AppSession.IsDebugMode = isDebugEnabled;

            if (isDebugEnabled)
            {
                var logForm = new smart_attendance_middleware.UI.LogForm();
                logForm.Show();
            }

            // Initialize the aiChatLogger
            aiChatLogger.Initialize();

            // Subscribe to the events
            aiChatLogger.OnAddMessage += AiChatLogger_OnAddMessage;
            aiChatLogger.OnClearMessages += AiChatLogger_OnClearMessages;

            // Initialize your WebView2 control
            InitializeWebView2();

            BiometricLoader = rectangularLoader1;

            this.FormClosing += Dashboard_FormClosing;

            HideAllToasts();
        }





        // ── Rectangular biometric loader (drag-dropped in Designer) ────────────

        /// <summary>
        /// Exposes the Designer-placed RectangularLoader so service classes
        /// can drive it without holding a Dashboard reference.
        /// </summary>
        public static RectangularLoader BiometricLoader { get; private set; }

        /// <summary>Starts the biometric loader from any thread.</summary>
        public static void GlobalStartBiometricLoader(string message = null)
        {
            var loader = BiometricLoader;
            if (loader == null || loader.IsDisposed) return;

            Action act = () => loader.Start(message);
            if (loader.InvokeRequired) loader.BeginInvoke(act);
            else act();
        }

        /// <summary>Updates the active label while the loader is running, from any thread.</summary>
        public static void GlobalUpdateBiometricLoader(string message)
        {
            var loader = BiometricLoader;
            if (loader == null || loader.IsDisposed) return;

            Action act = () => { loader.ActiveText = message ?? string.Empty; };
            if (loader.InvokeRequired) loader.BeginInvoke(act);
            else act();
        }

        /// <summary>Stops the loader and returns it to idle, from any thread.</summary>
        public static void GlobalStopBiometricLoader()
        {
            var loader = BiometricLoader;
            if (loader == null || loader.IsDisposed) return;

            Action act = () => loader.Stop();
            if (loader.InvokeRequired) loader.BeginInvoke(act);
            else act();
        }





        /// <summary>Updates the dashboard's persistent face popup and shows it.</summary>
        public static void GlobalShowFacePopup(int memberId, int enrollmentId, string action, DateTime time)
        {
            var dash = Application.OpenForms.OfType<Dashboard>().FirstOrDefault();
            if (dash == null || dash.IsDisposed) return;

            dash.BeginInvoke(new Action(() =>
            {
                if (dash._facePopup == null || dash._facePopup.IsDisposed)
                {
                    dash._facePopup = new FacePopupForm(memberId, enrollmentId, action, time, 1.3, true);
                    dash._facePopup.Show(dash);
                    dash._facePopup.TopMost = true;
                    dash._facePopup.BringToFront();
                }
                else
                {
                    dash._facePopup.UpdateData(memberId, enrollmentId, action, time, 1.3);
                    dash._facePopup.ShowPopup();
                }
            }));
        }

        private void ShowLoader(string message = null)
        {
            if (_loaderPopup == null || _loaderPopup.IsDisposed)
            {
                _loaderPopup = new smart_attendance_middleware.UI.LoaderPopupForm();
            }
            if (message != null)
                _loaderPopup.StatusText = message;
            if (!_loaderPopup.Visible)
            {
                _loaderPopup.Show(this); // owner
            }
            _loaderPopup.BringToFront();
            _loaderPopup.Refresh();
        }

        private void HideLoader()
        {
            _loaderPopup?.SafeClose();
            _loaderPopup = null;
        }

        // aiChatLogger Code Started from Here *************

        private void InitializeWebView2()
        {
            try
            {
                // Path to the HTML file in Resources folder (copied to output)
                string htmlPath = Path.Combine(Application.StartupPath, "Resources", "ailogs.html");

                if (!File.Exists(htmlPath))
                {
                    MessageBox.Show("File not found: " + htmlPath);
                    return;
                }

                // Correctly build file:// URI
                aiChat.Source = new Uri(htmlPath);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing WebView2: " + ex.Message);
            }
        }

        private async void AiChatLogger_OnAddMessage(string icon, string message)
        {
            // Handle cross-thread invocation
            if (aiChat.InvokeRequired)
            {
                aiChat.Invoke(new Action<string, string>(AiChatLogger_OnAddMessage), icon, message);
                return;
            }

            try
            {
                if (aiChat.CoreWebView2 != null)
                {
                    string script = $"addMessage('{icon}', '{message}')";
                    await aiChat.ExecuteScriptAsync(script);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing JavaScript: {ex.Message}");
            }
        }

        private async void AiChatLogger_OnClearMessages()
        {
            // Handle cross-thread invocation
            if (aiChat.InvokeRequired)
            {
                aiChat.Invoke(new Action(AiChatLogger_OnClearMessages));
                return;
            }

            try
            {
                if (aiChat.CoreWebView2 != null)
                {
                    await aiChat.ExecuteScriptAsync("clearMessages()");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing JavaScript: {ex.Message}");
            }
        }


        // aiChatLogger Code Ended *************************




        private void InitializeWebSocketManager()
        {
            _wsManager = new WebSocketManager(_dbHelper);
        }
        private void WsManager_LiveUpdate(object sender, LiveUpdateEventArgs e)
        {
            if (e.Data is JObject jdata)
            {
                _processor.HandleLiveUserUpdate(e.EventType, jdata);
            }
        }

        private async void Dashboard_Load(object sender, EventArgs e)
        {
            await UpdateAttendancePanelsAsync();
        }

        

        private void OnAttendanceLogSyncCompleted()
        {
            // Prevent double handling
            if (_attendanceInitialSyncHandled)
                return;

            try
            {
                // Ensure UI thread
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(OnAttendanceLogSyncCompleted));
                    return;
                }

                LogHelper.Log("🎯 Initial attendance log sync completed.");

                // Mark handled early to avoid re-entry
                _attendanceInitialSyncHandled = true;

                // Unsubscribe immediately (defensive)
                AttendanceLogSyncService.InitialSyncCompleted -= OnAttendanceLogSyncCompleted;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error in OnAttendanceLogSyncCompleted: {ex.Message}");
            }
            finally
            {
                AttendanceLogSyncService.InitialSyncCompleted -= OnAttendanceLogSyncCompleted;
            }
        }




        /// <summary>
        /// Updates prg_server based on how many WebSocket devices are connected vs. total registered in DB.
        /// Green = all connected, Orange = some connected, Red = none connected.
        /// </summary>
        private void OnDeviceConnectionChanged(int connected, int total)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnDeviceConnectionChanged(connected, total)));
                return;
            }

            if (prg_server == null) return;

            try
            {
                prg_server.Visible = true;
                prg_server.Style = ProgressBarStyle.Continuous;
                prg_server.Minimum = 0;
                prg_server.Maximum = total > 0 ? total : 1;
                prg_server.Value = Math.Min(connected, prg_server.Maximum);

                if (total == 0)
                {
                    prg_server.ForeColor = Color.Gray;
                }
                else if (connected == 0)
                {
                    prg_server.ForeColor = Color.Red;
                }
                else if (connected < total)
                {
                    prg_server.ForeColor = Color.Orange;
                }
                else
                {
                    prg_server.ForeColor = Color.LimeGreen;
                }

                prg_server.Refresh();
                LogHelper.Log($"[Dashboard] Device server: {connected}/{total} devices connected.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Dashboard] OnDeviceConnectionChanged error: {ex.Message}");
            }
        }

        /// <summary>
        /// Original public method refactored: fetches DB values then updates the UI.
        /// Keeps existing behavior (UI update + logging).
        /// </summary>
        public async Task RefreshDeviceSyncStatusAsync()
        {
            try
            {
                LogHelper.Log("🔄 Starting device sync status refresh...");

                // Get values from settings, otherwise from AppSession
                int membersSynced = 0;
                string membersSyncedSetting = _dbHelper.GetSetting("MembersSynced");
                if (!string.IsNullOrEmpty(membersSyncedSetting) && int.TryParse(membersSyncedSetting, out int membersSyncedVal))
                {
                    membersSynced = membersSyncedVal;
                }
                else if (AppSession.SyncStatus != null)
                {
                    membersSynced = AppSession.SyncStatus.MembersSynced;
                }

                int pendingBiometricJobs = 0;
                string pendingJobsSetting = _dbHelper.GetSetting("PendingBiometricJobs");
                if (!string.IsNullOrEmpty(pendingJobsSetting) && int.TryParse(pendingJobsSetting, out int pendingJobsVal))
                {
                    pendingBiometricJobs = pendingJobsVal;
                }
                else if (AppSession.SyncStatus != null)
                {
                    pendingBiometricJobs = AppSession.SyncStatus.PendingBiometricJobs;
                }

                int missingFingerprints = 0;
                string missingFingerprintsSetting = _dbHelper.GetSetting("MissingFingerprints");
                if (!string.IsNullOrEmpty(missingFingerprintsSetting) && int.TryParse(missingFingerprintsSetting, out int missingFingerprintsVal))
                {
                    missingFingerprints = missingFingerprintsVal;
                }
                else if (AppSession.SyncStatus != null)
                {
                    missingFingerprints = AppSession.SyncStatus.MissingFingerprints;
                }

                // Ensure UI update runs on UI thread by calling the existing UpdateSyncStatusLabels
                // which already marshals to the UI thread.
                UpdateSyncStatusLabels(membersSynced, pendingBiometricJobs, missingFingerprints);

                LogHelper.Log("✅ Device sync status refresh completed successfully");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error refreshing device sync status: {ex.Message}");
                UpdateSyncStatusLabels(0, 0, 0);
            }
        }



        /// <summary>
        /// Updates the UI labels with sync status information
        /// </summary>
        private void UpdateSyncStatusLabels(int membersSynced, int pendingJobs, int missingFingerprints)
        {
            try
            {
                // Ensure UI updates are performed on the UI thread
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateSyncStatusLabels(membersSynced, pendingJobs, missingFingerprints)));
                    return;
                }

                // Update Member Synced label
                if (lbl_memberssynced != null)
                {
                    lbl_memberssynced.Text = membersSynced.ToString();
                }

                // Update Pending Jobs label
                if (lbl_pendingjobs != null)
                {
                    lbl_pendingjobs.Text = pendingJobs.ToString();
                }

                // Enable/disable retry button based on pending jobs count and change color
                if (btn_retry != null)
                {
                    bool hasJobs = pendingJobs > 0;
                    btn_retry.Enabled = hasJobs;

                    if (hasJobs)
                    {
                        btn_retry.NormalBackground = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
                        btn_retry.HoverBackground = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(158)))), ((int)(((byte)(35)))));
                        btn_retry.NormalForeColor = System.Drawing.Color.White;
                        btn_retry.HoverForeColor = System.Drawing.Color.White;
                        _buttonTooltip.SetToolTip(btn_retry, $"Retry {pendingJobs} pending sync job(s)");
                        LogHelper.Log($"🔘 Retry button enabled - pending jobs: {pendingJobs}");
                    }
                    else
                    {
                        btn_retry.NormalBackground = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                        btn_retry.HoverBackground = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                        btn_retry.NormalForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
                        btn_retry.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
                        _buttonTooltip.SetToolTip(btn_retry, "No pending jobs to retry");
                        LogHelper.Log($"🔘 Retry button disabled and grayed out - no pending jobs");
                    }

                    // Force the button to refresh its appearance
                    btn_retry.Invalidate();
                }

                LogHelper.Log($"📊 Status Updated - Synced: {membersSynced}, Pending: {pendingJobs}, Missing FP: {missingFingerprints}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error updating sync status labels: {ex.Message}");
            }
        }


        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (servicesInitialized) return;

            try
            {
                // Initial Load - Ensure UI has a moment to settle
                ShowLoader("Initializing Middleware System...");
                await Task.Delay(500);

                // Phase 1: Data Caching and UI Prep
                await LoadInitialUiAsync();
                _uiReady = true;
                await Task.Delay(200);

                // Phase 2: WebSocket Device Server
                UpdateLoaderMessage("Starting WebSocket device server...");
                WebsocketServerThreadManager.DeviceConnectionChanged -= OnDeviceConnectionChanged;
                WebsocketServerThreadManager.DeviceConnectionChanged += OnDeviceConnectionChanged;
                string debugMode = _dbHelper.GetSetting("DebugMode");
                bool showWsUI = !string.IsNullOrEmpty(debugMode) &&
                                (debugMode == "1" || debugMode.Equals("true", StringComparison.OrdinalIgnoreCase));
                await ServiceManager.StartWebsocketServerAsync(showUI: showWsUI);
                await Task.Delay(200);

                // Phase 3: Background Sync Engines
                UpdateLoaderMessage("Starting background sync engines...");
                await StartServicesAsync();
                await Task.Delay(200);

                // Phase 4: Hardware Interface Prep
                UpdateLoaderMessage("Finalizing hardware interface...");
                InitializeDevicePanel();

                dataPairPanel1.OpenDoorRequested += async (item) => {
                    if (string.IsNullOrEmpty(item.IpAddress)) return;
                    await CommonUtils.HandleDoorOpenRequestAsync(item.IpAddress, item.Port, item.MachineNo, item.Password, _dbHelper);
                };

                // Success feedback
                UpdateLoaderMessage("All systems operational. Launching Dashboard...");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OnShown][ERROR] {ex.Message}");
            }
            finally
            {
                HideLoader();
                servicesInitialized = true;
            }
        }

        // Add async initialization routine
        private async Task LoadInitialUiAsync()
        {
            try
            {
                UpdateLoaderMessage("Caching membership and attendance data...");

                // Use CacheManager for initial load instead of direct database calls
                await _cacheManager.InitialLoadAsync();

                // Start automatic incremental updates every 30 seconds
                _cacheManager.StartAutoRefresh();

                LogHelper.Log("[LoadInitialUiAsync] Cache warmed successfully");
                UpdateLoaderMessage("Membership data cached successfully.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[LoadInitialUiAsync] Error caching data: {ex.Message}");
                UpdateLoaderMessage("Cache failure. System will run in direct-query mode.");
            }

            await UpdateAttendancePanelsAsync();
            await Task.Yield(); // let UI pump once
        }

        public async Task StartServicesAsync()
        {
            try
            {
                UpdateLoaderMessage("Initializing UI update timers...");
                attendancePanelTimer = new System.Windows.Forms.Timer { Interval = 5000 };
                attendancePanelTimer.Tick += async (s, args) =>
                {
                    // Skip if cache already refreshed the panels within the last 4 seconds
                    if ((DateTime.Now - _lastPanelRefresh).TotalSeconds < 4) return;
                    await UpdateAttendancePanelsAsync();
                };
                attendancePanelTimer.Start();

                _processor.update_session();

                // 1. Log Sync Services
                UpdateLoaderMessage("Sync: Initializing General log service...");
                await ServiceManager.StartLogSyncServiceAsync(_deviceFunctions);
                UpdateLoaderMessage("Sync: General Logs ready.");

                // 2. Member Sync
                UpdateLoaderMessage("Sync: Connecting Member Attendance stream...");
                AttendanceLogSyncService.InitialSyncCompleted -= OnAttendanceLogSyncCompleted;
                AttendanceLogSyncService.InitialSyncCompleted += OnAttendanceLogSyncCompleted;
                await ServiceManager.StartAttendanceLogSyncServiceAsync();
                UpdateLoaderMessage("Sync: Member stream active.");

                // 3. Staff Sync
                UpdateLoaderMessage("Sync: Connecting Staff Attendance stream...");
                await ServiceManager.StartStaffAttendanceLogSyncServiceAsync();
                UpdateLoaderMessage("Sync: Staff stream active.");

                // 4. Maintenance & Backup
                UpdateLoaderMessage("System: Scheduling periodic maintenance...");
                await ServiceManager.StartPeriodicMaintenanceServiceAsync(this);
                if (_backupService == null)
                {
                    _backupService = new BackupService(_dbHelper);
                }
                _backupService.Start();
                UpdateLoaderMessage("System: Maintenance & Backup scheduled.");

                // 5. Connection Monitors
                UpdateLoaderMessage("Network: Configuring cloud monitors...");
                ConnectionStatusChecker.Instance.SetWebSocketManager(_wsManager);
                ConnectionStatusChecker.Instance.InternetStatusChanged -= OnInternetStatusChanged;
                ConnectionStatusChecker.Instance.InternetStatusChanged += OnInternetStatusChanged;
                ConnectionStatusChecker.Instance.SubscriptionStatusChanged -= OnSubscriptionStatusChanged;
                ConnectionStatusChecker.Instance.SubscriptionStatusChanged += OnSubscriptionStatusChanged;
                ConnectionStatusChecker.Instance.Start();
                UpdateLoaderMessage("Network: Monitors active.");

                // 6. Device Pings
                UpdateLoaderMessage("Hardware: Pinging biometric devices...");
                try
                {
                    await RefreshDeviceSyncStatusAsync();
                    UpdateLoaderMessage("Hardware: Scanning complete.");
                }
                catch (Exception ex)
                {
                    LogHelper.Log("[StartServicesAsync] Device refresh failed: " + ex.Message);
                    UpdateLoaderMessage("Hardware: Scan incomplete (Check network)");
                }

                LogHelper.Log("[StartServicesAsync] Services started successfully");
                UpdateLoaderMessage("All services initialized successfully.");
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                LogHelper.Log("[StartServicesAsync][ERROR] " + ex.Message);
                UpdateLoaderMessage("Startup encountered errors! Please check logs.");
                await Task.Delay(3000); 
            }
        }

        /// <summary>
        /// Handle cache update events - refresh UI when AppSession data changes
        /// </summary>
        private async void OnCacheUpdated(object sender, CacheUpdateEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnCacheUpdated(sender, e)));
                return;
            }

            try
            {
                // Refresh relevant UI components based on what was updated
                if (e.UpdateType == CacheUpdateType.FullRefresh)
                {
                    await UpdateAttendancePanelsAsync();
                    LogHelper.Log("[Dashboard] Full cache refresh - UI updated");
                }
                else if (e.UpdateTypes != null && e.UpdateTypes.Count > 0)
                {
                    if (e.UpdateTypes.Contains(CacheUpdateType.AttendanceLogs) || 
                        e.UpdateTypes.Contains(CacheUpdateType.StaffAttendanceLogs))
                    {
                        await UpdateAttendancePanelsAsync();
                    }

                    if (e.UpdateTypes.Contains(CacheUpdateType.Members) || 
                        e.UpdateTypes.Contains(CacheUpdateType.Staff))
                    {
                        // Update any member/staff displays if needed
                        LogHelper.Log($"[Dashboard] Cache updated: {string.Join(", ", e.UpdateTypes)}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Dashboard] Error in OnCacheUpdated: {ex.Message}");
            }
        }

        private void OnInternetStatusChanged(bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(OnInternetStatusChanged), isConnected);
                return;
            }

            pic_net_status.Image = isConnected
                ? Properties.Resources.inertnet_connected
                : Properties.Resources.internet_disconnected;

            internet = isConnected;
            UpdateWorkStatus();

            if (isConnected)
            {
                aiChatLogger.AddNetworkMessage("Internet connection restored! I'm back online and ready to sync.");
            }
            else
            {
                aiChatLogger.AddWarningMessage("Lost internet connection. Don't worry, I'll keep working offline!");
            }
        }

        private void OnBiometricStatusChanged(bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(OnBiometricStatusChanged), isConnected);
                return;
            }

            pic_device_status.Image = isConnected
                ? Properties.Resources.device_connected
                : Properties.Resources.device_disconnected;

            biometric = isConnected;
            UpdateWorkStatus();

            if (isConnected)
            {
                aiChatLogger.AddDeviceMessage("Biometric devices are responding! I can now process fingerprints and attendance.");
            }
            else
            {
                aiChatLogger.AddWarningMessage("Oh!... Biometric devices seem disconnected. Please check your local area network.");
            }
        }

        private void OnSubscriptionStatusChanged(bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(OnSubscriptionStatusChanged), isConnected);
                return;
            }

            pic_cloud_status.Image = isConnected
                ? Properties.Resources.fitness_marvel_connected
                : Properties.Resources.fitness_marvel_disconnected;

            subscription = isConnected;
            UpdateWorkStatus();

            if (isConnected)
            {
                aiChatLogger.AddConnectionMessage("Cloud service connected! I can now sync your data seamlessly.");
            }
            else
            {
                aiChatLogger.AddErrorMessage("Lost connection to cloud service. I'll retry automatically when possible.");
            }
        }

        private void OnStaffDeviceStatusChanged(bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(OnStaffDeviceStatusChanged), isConnected);
                return;
            }

            if (isConnected)
            {
                aiChatLogger.AddConnectionMessage("Staff Device Connected");
            }
            else
            {
                aiChatLogger.AddErrorMessage("Staff Device Disconnected");
            }
        }

        public void UpdateWorkStatus()
        {
            Connected = internet && biometric && subscription;
        }

        

        

        public async Task TriggerWebSocketReconnection()
        {
            try
            {
                LogHelper.Log("🔄 WebSocket reconnection triggered by subscription checker...");

                //// Check if we have an API key and devices configured
                //var existingDevices = _dbHelper.LoadDevices();
                //if (existingDevices == null || existingDevices.Count == 0)
                //{
                //    LogHelper.Log("❌ No devices configured - skipping reconnection");
                //    return;
                //}

                string apiKey = _dbHelper.GetSetting("APIKey");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    LogHelper.Log("❌ No API key found - skipping reconnection");
                    return;
                }

                // Subscribe to channel
                bool isSubscribed = await _wsManager.ConnectAndSubscribeAsync(preferTls: true, handshakeTimeoutMs: 5000, fallbackToCleartext: true);

                // Update the subscription status
                OnSubscriptionStatusChanged(isSubscribed);

                if (isSubscribed)
                {
                    LogHelper.Log("✅ WebSocket reconnection successful");
                }
                else
                {
                    LogHelper.Log("❌ WebSocket reconnection failed - subscription unsuccessful");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ WebSocket reconnection failed: {ex.Message}");
                OnSubscriptionStatusChanged(false);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private static string ShowPasswordDialog(IWin32Window owner, string title, string prompt)
        {
            using (var form = new Form())
            using (var lbl = new Label())
            using (var txt = new TextBox())
            using (var ok = new Button())
            using (var cancel = new Button())
            {
                form.Text = title;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = false;
                form.ClientSize = new Size(360, 140);
                form.TopMost = true;

                lbl.Text = prompt;
                lbl.AutoSize = true;
                lbl.Location = new Point(12, 12);

                txt.UseSystemPasswordChar = true;
                txt.Location = new Point(15, 45);
                txt.Width = form.ClientSize.Width - 30;
                txt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                ok.Text = "OK";
                ok.DialogResult = DialogResult.OK;
                ok.Location = new Point(form.ClientSize.Width - 170, 95);
                ok.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                cancel.Text = "Cancel";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new Point(form.ClientSize.Width - 90, 95);
                cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
                form.AcceptButton = ok;
                form.CancelButton = cancel;

                return form.ShowDialog(owner) == DialogResult.OK ? txt.Text : null;
            }
        }

        private void Dashboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Unsubscribe AI chat logger events immediately to stop any further UI calls
                aiChatLogger.OnAddMessage -= AiChatLogger_OnAddMessage;
                aiChatLogger.OnClearMessages -= AiChatLogger_OnClearMessages;

                // Unsubscribe any pending attendance sync completion callback
                AttendanceLogSyncService.InitialSyncCompleted -= OnAttendanceLogSyncCompleted;
                WebsocketServerThreadManager.DeviceConnectionChanged -= OnDeviceConnectionChanged;

                if (!string.IsNullOrEmpty(AppSession.LoggedInUser) &&
                    !string.Equals(AppSession.UserRole, "developer", StringComparison.OrdinalIgnoreCase))
                {
                    var enteredPassword = ShowPasswordDialog(this, "Confirm Exit",
                        "Enter your password to close the application:");

                    if (string.IsNullOrEmpty(enteredPassword) ||
                        !_dbHelper.VerifyLoginUser(AppSession.LoggedInUser, enteredPassword))
                    {
                        LogHelper.Log($"Application closure denied: Invalid or missing password for user '{AppSession.LoggedInUser}'.");
                        e.Cancel = true;
                        MessageBox.Show("Invalid password. Application closure canceled.", "Authentication Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    LogHelper.Log($"Application closure authorized for user '{AppSession.LoggedInUser}'.");
                }

                // Password confirmed — stop background services
                _deviceMonitorCts?.Cancel();
                _deviceMonitorCts?.Dispose();
                _deviceMonitorCts = null;

                // Stop cache manager auto-refresh
                _cacheManager?.StopAutoRefresh();

                // Stop and dispose countdown timer
                if (countdownTimer != null)
                {
                    countdownTimer.Stop();
                    countdownTimer.Dispose();
                    countdownTimer = null;
                }

                LogHelper.Log("Application is closing. Performing cleanup...");

                // Unsubscribe from connection monitors
                ConnectionStatusChecker.Instance.InternetStatusChanged -= OnInternetStatusChanged;
                ConnectionStatusChecker.Instance.SubscriptionStatusChanged -= OnSubscriptionStatusChanged;
                ConnectionStatusChecker.Instance.SetWebSocketManager(null);

                // Stop the connection status checker service
                ConnectionStatusChecker.Instance.Stop();

                _backupService?.Stop();
                _backupService?.Dispose();

                // Stop and dispose the WebSocket manager if needed
                if (_wsManager != null)
                {
                    _wsManager.LiveUpdate -= WsManager_LiveUpdate;
                    LogHelper.Log("Closing WebSocket connection...");
                }

                try
                {
                    LogHelper.Log("Stopping all services...");
                    ServiceManager.StopAllServices();
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Error stopping services: {ex.Message}");
                }

                // Clean up database helper reference
                try
                {
                    _dbHelper = null;
                    LogHelper.Log("Database connections closed");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Error closing database: {ex.Message}");
                }

                LogHelper.Log("Application cleanup completed");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error during application shutdown: {ex.Message}");
            }
        }

        private void pic_cloud_status_Click(object sender, EventArgs e)
        {

        }

        private void cuiButton2_Click(object sender, EventArgs e)
        {

        }

        


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var devicemanager = new DeviceManager();
            devicemanager.ShowDialog();
        }

        private void lbl_fingerprintmanagement_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var fingerManagement = new FingerManagement();
            fingerManagement.ShowDialog();
        }

        private void btn_adddevice_Click(object sender, EventArgs e)
        {
            if (AppSession.UserRole == "user")
            {
                MessageBox.Show("Access denied. Only admin or developer can add devices.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AddNewDevice addnewdevice = new AddNewDevice();
            addnewdevice.ShowDialog();

            // Refresh device status after device manager is closed
            Task.Run(async () =>
            {
                await Task.Delay(500); // Small delay to ensure database changes are committed
                this.Invoke(new Action(async () => await RefreshDevicePanelAsync()));
            });
        }

        
        private async void cuiButton4_Click(object sender, EventArgs e)
        {
            // Store original button text and start countdown
            originalButtonText = btn_sync.Content?.ToString() ?? "Connect";
            StartButtonCountdown();

            try
            {
                // Step 0: Temporarily stop subscription checking to prevent interference
                LogHelper.Log("⏸️ Pausing Connection Status Checker");
                ConnectionStatusChecker.Instance.Stop();

                // Step 1: Check for existing devices
                var existingDevices = _dbHelper.LoadDevices();
                if (existingDevices == null || existingDevices.Count == 0)
                {
                    LogHelper.Log("❌ No devices found in the database. Please add devices first.");
                    MessageBox.Show("No devices found in the database. Please add devices before connecting.",
                                  "No Devices Found",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                // Step 2: Get API key
                string apiKey = _dbHelper.GetSetting("APIKey");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    LogHelper.Log("❌ No API key found. Please configure it first.");
                    MessageBox.Show("API Key is missing. Please configure it in the settings.",
                                  "Missing API Key",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                // Step 3: Check if already connected by sending ping
                bool isAlreadyConnected = false;
                try
                {
                    LogHelper.Log("🔍 Checking existing WebSocket connection...");
                    await _wsManager.ConnectionStatusAsync();

                    // Wait for pong response with a timeout (3 attempts, 1 second apart)
                    var pongReceived = await Task.Run(async () =>
                    {
                        for (int attempt = 1; attempt <= 3; attempt++)
                        {
                            LogHelper.Log($"⏳ Waiting for pong response (Attempt {attempt}/3)...");
                            await Task.Delay(1000);

                            if (_wsManager.LastPongReceived > DateTime.Now.AddSeconds(-2))
                            {
                                LogHelper.Log("✅ Received fresh pong response - connection is active");
                                return true;
                            }

                            // If no pong received, send another ping
                            if (attempt < 3)
                            {
                                LogHelper.Log("🔄 No pong received, sending another ping...");
                                await _wsManager.ConnectionStatusAsync();
                            }
                        }

                        LogHelper.Log("❌ No pong received after 3 attempts");
                        return false;
                    });

                    if (pongReceived)
                    {
                        isAlreadyConnected = true;
                        LogHelper.Log("✅ Already connected to WebSocket (verified with pong response)");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"⚠️ Connection check failed: {ex.Message}");
                    // Proceed with normal connection
                }

                if (!isAlreadyConnected)
                {
                    LogHelper.Log("🔌 Establishing new WebSocket connection...");

                    // Step 4: Connect and Subscribe
                    LogHelper.Log("🔄 Connecting to WebSocket...");
                    LogHelper.Log("📡 Subscribing to channel...");
                    bool isSubscribed = await _wsManager.ConnectAndSubscribeAsync(preferTls: true, handshakeTimeoutMs: 5000, fallbackToCleartext: true);

                    // Step 5: Update cloud icon and status
                    OnSubscriptionStatusChanged(isSubscribed);

                    if (!isSubscribed)
                    {
                        LogHelper.Log("❌ Subscription failed. Could not request members.");
                        return;
                    }

                    LogHelper.Log("✅ Successfully connected and subscribed");
                }

                LogHelper.Log("📡 Connection verified. Requesting all members from cloud via HTTP...");

                // Step 6: Request all members via HttpHandler
                await _httpHandler.FetchAllMembers(CancellationToken.None);

                LogHelper.Log("✅ Member synchronization request completed via HTTP");

                // Step 7: Execute complete user processing workflow
                await _deviceFunctions.ProcessUserWorkflowAsync();

                // Step 8: Refresh sync status after processing
                await RefreshDeviceSyncStatusAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ WebSocket connection/sync failed: {ex.Message}");
                MessageBox.Show("Failed to complete synchronization.\n" + ex.Message,
                              "Sync Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                OnSubscriptionStatusChanged(false);
            }
            finally
            {
                // Step 9: Restart subscription checking now that button operations are complete
                LogHelper.Log("🔄 Restarting Connection Status Checker");
                ConnectionStatusChecker.Instance.Start();

                LogHelper.Log("🔚 Connect operation completed");
            }
        }
        // Add these methods to handle the countdown timer
        private void StartButtonCountdown()
        {
            // Initialize countdown
            countdownSeconds = 60;

            // Disable the button
            btn_sync.Enabled = false;

            // Update button text with initial countdown
            btn_sync.Content = $"{countdownSeconds}";

            // Initialize timer if not already created
            if (countdownTimer == null)
            {
                countdownTimer = new System.Windows.Forms.Timer();
                countdownTimer.Interval = 1000; // 1 second
                countdownTimer.Tick += CountdownTimer_Tick;
            }

            // Start the timer
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            // Decrement countdown
            countdownSeconds--;

            // Update button text with remaining seconds
            btn_sync.Content = $"{countdownSeconds}";

            // Check if countdown is complete
            if (countdownSeconds <= 0)
            {
                // Stop the timer
                countdownTimer.Stop();

                // Re-enable the button
                btn_sync.Enabled = true;

                // Restore original button text
                btn_sync.Content = originalButtonText;
            }
        }

        private async void cuiButton5_Click(object sender, EventArgs e)
        {
            try
            {
                LogHelper.Log("🔄 Starting retry jobs...");

                await _deviceFunctions.ProcessUserWorkflowAsync();

                LogHelper.Log("✅ Retry jobs completed");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Retry jobs failed: {ex.Message}");
            }

        }

        private async Task UpdateAttendancePanelsAsync()
        {
            _lastPanelRefresh = DateTime.Now;
            try
            {
                if (AppSession.LastCheckIn != null)
                {
                    var checkIn = AppSession.LastCheckIn;
                    lbl_name_in.Text       = checkIn.MemberName;
                    lbl_action_in.Text     = $"Action: {checkIn.Action.ToUpper()}";
                    lbl_time_in.Text       = $"Time: {checkIn.Time:HH:mm:ss}";
                    lbl_enroll_no_in.Text  = $"Enroll ID: {checkIn.EnrollmentId}";
                    await LoadMemberImageAsync(pictureBox2, checkIn.ImagePath, checkIn.MemberId);
                }
                else
                {
                    lbl_name_in.Text      = "No recent check-ins";
                    lbl_action_in.Text    = "Action: --";
                    lbl_time_in.Text      = "Time: --";
                    lbl_enroll_no_in.Text = "Enroll ID: --";
                    pictureBox2.Image     = null;
                }

                if (AppSession.LastCheckOut != null)
                {
                    var checkOut = AppSession.LastCheckOut;
                    lbl_name_out.Text      = checkOut.MemberName;
                    lbl_action_out.Text    = $"Action: {checkOut.Action.ToUpper()}";
                    lbl_time_out.Text      = $"Time: {checkOut.Time:HH:mm:ss}";
                    lbl_enroll_no_out.Text = $"Enroll ID: {checkOut.EnrollmentId}";
                    await LoadMemberImageAsync(pictureBox3, checkOut.ImagePath, checkOut.MemberId);
                }
                else
                {
                    lbl_name_out.Text      = "No recent check-outs";
                    lbl_action_out.Text    = "Action: --";
                    lbl_time_out.Text      = "Time: --";
                    lbl_enroll_no_out.Text = "Enroll ID: --";
                    pictureBox3.Image      = null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[UpdateAttendancePanels][ERROR] {ex.Message}");
            }
        }

        private async Task LoadMemberImageAsync(PictureBox pictureBox, string imagePath, int memberId)
        {
            try
            {
                Image newImage = null;

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    int maxWidth  = pictureBox.Width;
                    int maxHeight = pictureBox.Height;

                    // Read file bytes and resize entirely off the UI thread.
                    // Retries a couple of times because the image file may still be
                    // mid-write by the photo-download code when we try to read it here —
                    // a partially written file makes new Bitmap() throw a misleading
                    // OutOfMemoryException even though it's really just corrupt/incomplete data.
                    newImage = await Task.Run(() =>
                    {
                        const int maxAttempts = 3;
                        for (int attempt = 1; attempt <= maxAttempts; attempt++)
                        {
                            try
                            {
                                byte[] bytes = File.ReadAllBytes(imagePath);
                                if (bytes.Length == 0)
                                    throw new InvalidDataException("Image file is empty (still being written?).");

                                using (var ms = new MemoryStream(bytes))
                                {
                                    // new Bitmap() copies pixel data so the stream can be closed safely
                                    Image img = new Bitmap(ms);

                                    if (img.Width <= maxWidth && img.Height <= maxHeight)
                                        return img;

                                    double ratio    = Math.Min((double)maxWidth / img.Width, (double)maxHeight / img.Height);
                                    int newWidth    = (int)(img.Width  * ratio);
                                    int newHeight   = (int)(img.Height * ratio);

                                    Bitmap resized = new Bitmap(newWidth, newHeight);
                                    using (Graphics g = Graphics.FromImage(resized))
                                    {
                                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                        g.DrawImage(img, 0, 0, newWidth, newHeight);
                                    }
                                    img.Dispose();
                                    return (Image)resized;
                                }
                            }
                            catch (Exception) when (attempt < maxAttempts)
                            {
                                Thread.Sleep(150 * attempt);
                            }
                        }
                        return null;
                    });
                }

                // Assign on UI thread — dispose old image to prevent GDI leaks
                var old = pictureBox.Image;
                if (newImage != null)
                    pictureBox.Image = newImage;
                else
                    CreateDefaultMemberImage(pictureBox, memberId);
                old?.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[LoadMemberImage][ERROR] Failed to load image for member {memberId}: {ex.Message}");
                CreateDefaultMemberImage(pictureBox, memberId);
            }
        }

        // Helper method to create default member image
        private void CreateDefaultMemberImage(PictureBox pictureBox, int memberId)
        {
            try
            {
                Bitmap defaultImage = new Bitmap(pictureBox.Width, pictureBox.Height);
                using (Graphics g = Graphics.FromImage(defaultImage))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.FromArgb(240, 240, 240));

                    // Draw circle background
                    using (var brush = new SolidBrush(Color.FromArgb(200, 200, 200)))
                    {
                        int diameter = Math.Min(defaultImage.Width, defaultImage.Height) - 10;
                        int x = (defaultImage.Width - diameter) / 2;
                        int y = (defaultImage.Height - diameter) / 2;
                        g.FillEllipse(brush, x, y, diameter, diameter);
                    }

                    // Draw member ID text
                    string idText = $"ID: {memberId}";
                    using (Font font = new Font("Segoe UI", 8, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                    {
                        SizeF textSize = g.MeasureString(idText, font);
                        g.DrawString(idText, font, textBrush,
                            (defaultImage.Width - textSize.Width) / 2f,
                            (defaultImage.Height - textSize.Height) / 2f);
                    }
                }
                pictureBox.Image = defaultImage;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[CreateDefaultMemberImage][ERROR] {ex.Message}");
            }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }


        private void dev3_ip_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {

        }
        


        /////////////////////////Global Loader////////////////////////////////////////////


        /// <summary>
        /// Starts the loader and optionally sets a status message.
        /// Multiple concurrent starts are reference-counted; the loader remains visible
        /// until the last Stop is called.
        /// Safe to call from any thread.
        /// </summary>
        public void StartLoader(string message = null)
        {
            try
            {
                lock (_loaderLock)
                {
                    _loaderRefCount++;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() =>
                    {
                        ShowLoader(message);
                    }));
                }
                else
                {
                    ShowLoader(message);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[StartLoader][ERROR] {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the loader. Decrements the reference-count; hides when it reaches zero.
        /// Safe to call from any thread.
        /// </summary>
        public void StopLoader()
        {
            try
            {
                bool shouldHide = false;
                lock (_loaderLock)
                {
                    if (_loaderRefCount > 0)
                    {
                        _loaderRefCount--;
                        shouldHide = _loaderRefCount == 0;
                    }
                    else
                    {
                        _loaderRefCount = 0;
                    }
                }

                if (shouldHide)
                {
                    if (InvokeRequired)
                        BeginInvoke(new Action(HideLoader));
                    else
                        HideLoader();
                }
                else
                {
                    // Refresh if still visible for other callers
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            if (_loaderPopup != null && !_loaderPopup.IsDisposed)
                                _loaderPopup.Refresh();
                        }));
                    }
                    else
                    {
                        if (_loaderPopup != null && !_loaderPopup.IsDisposed)
                            _loaderPopup.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[StopLoader][ERROR] {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the loader message while it is visible.
        /// Safe to call from any thread.
        /// </summary>
        public void UpdateLoaderMessage(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                    return;

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (_loaderPopup != null && !_loaderPopup.IsDisposed)
                        {
                            _loaderPopup.StatusText = message;
                            _loaderPopup.Refresh();
                        }
                    }));
                }
                else
                {
                    if (_loaderPopup != null && !_loaderPopup.IsDisposed)
                    {
                        _loaderPopup.StatusText = message;
                        _loaderPopup.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[UpdateLoaderMessage][ERROR] {ex.Message}");
            }
        }

        /// <summary>
        /// Start loader from anywhere without a Dashboard reference.
        /// </summary>
        public static void GlobalStartLoader(string message = null)
        {
            var dash = Application.OpenForms.Cast<Form>().OfType<Dashboard>().FirstOrDefault();
            if (dash == null)
            {
                LogHelper.Log("[GlobalStartLoader] Dashboard not available.");
                return;
            }
            dash.StartLoader(message);
        }

        /// <summary>
        /// Stop loader from anywhere without a Dashboard reference.
        /// </summary>
        public static void GlobalStopLoader()
        {
            var dash = Application.OpenForms.Cast<Form>().OfType<Dashboard>().FirstOrDefault();
            if (dash == null)
            {
                LogHelper.Log("[GlobalStopLoader] Dashboard not available.");
                return;
            }
            dash.StopLoader();
        }

        /// <summary>
        /// Update loader message from anywhere.
        /// </summary>
        public static void GlobalUpdateLoaderMessage(string message)
        {
            var dash = Application.OpenForms.Cast<Form>().OfType<Dashboard>().FirstOrDefault();
            if (dash == null)
            {
                LogHelper.Log("[GlobalUpdateLoaderMessage] Dashboard not available.");
                return;
            }
            dash.UpdateLoaderMessage(message);
        }

        public static void GlobalShowToast(string title, string message, Icon icon = null, int displayMs = 5000)
        {
            var dash = Application.OpenForms.Cast<Form>().OfType<Dashboard>().FirstOrDefault();
            if (dash == null || dash.IsDisposed) return;

            dash.BeginInvoke(new Action(() =>
            {
                // Usage of toastNotificationControl2 which is the primary dashboard notification area
                if (dash.toastNotificationControl2 != null && !dash.toastNotificationControl2.IsDisposed)
                {
                    dash.toastNotificationControl2.UpdateToast(title, message, icon, displayMs);
                }
            }));
        }

        public static void GlobalShowToastData(Newtonsoft.Json.Linq.JObject data, string title, string defaultMsg)
        {
            var dash = Application.OpenForms.Cast<Form>().OfType<Dashboard>().FirstOrDefault();
            if (dash == null || dash.IsDisposed) return;

            dash.BeginInvoke(new Action(() =>
            {
                if (dash.toastNotificationControl2 != null && !dash.toastNotificationControl2.IsDisposed)
                {
                    dash.toastNotificationControl2.UpdateFromData(data, title, defaultMsg);
                }
            }));
        }

        /// <summary>
        /// Hides all toast controls on the dashboard. Called on initialize.
        /// </summary>
        private void HideAllToasts()
        {
            if (toastNotificationControl2 != null) toastNotificationControl2.Visible = false;
        }

        private void InitializeDevicePanel()
        {
            try
            {
                var dataPairs = new List<Controls.DataPairItem>();

                // Load all devices from AppSession
                var devices = AppSession.DeviceList ?? new List<Device>();
                var staffDevices = AppSession.StaffDeviceList ?? new List<Device>();

                // Combine all devices
                var allDevices = new List<Device>();
                allDevices.AddRange(devices);
                allDevices.AddRange(staffDevices);

                // Update the connected devices label text (Initial count of active devices)
                if (lbl_connecteddevice != null)
                {
                    lbl_connecteddevice.Text = allDevices.Count(d => d.IsActive).ToString();
                }

                if (allDevices.Count == 0)
                {
                    dataPairs.Add(new Controls.DataPairItem
                    {
                        Label = "No Devices",
                        Elements = new List<Controls.DataElement>
                        {
                            new Controls.DataElement("No devices configured", Color.FromArgb(220, 53, 69))
                        }
                    });
                }
                else
                {
                    // Create a pair for each device
                    int deviceCount = 0;
                    foreach (var device in allDevices)
                    {
                        deviceCount++;

                        // Name (Label part)
                        string deviceName = device.Location ?? "Device";

                        // Determine if it's Staff or Member
                        bool isStaff = AppSession.StaffDeviceList?.Any(sd => sd.DeviceIP == device.DeviceIP) ?? false;
                        string deviceType = isStaff ? "Staff" : "Member";

                        var elements = new List<Controls.DataElement>
                        {
                            // 0: Machine No
                            new Controls.DataElement($"Machine No: {device.MachineNo}"),

                            // 1: Device IP
                            new Controls.DataElement($"IP: {device.DeviceIP ?? "N.A"}"),

                            // 2: Status (Badge)
                            new Controls.DataElement(device.IsActive ? "ONLINE" : "OFFLINE",
                                device.IsActive ? Color.FromArgb(34, 177, 76) : Color.FromArgb(220, 53, 69)),

                            // 3: Open Button
                            new Controls.DataElement("Open", isLink: true, tooltip: $"Open {deviceName}")
                        };

                        dataPairs.Add(new Controls.DataPairItem
                        {
                            Label = deviceName,
                            Type = deviceType, // Pass the type here
                            Elements = elements,
                            IpAddress = device.DeviceIP,
                            Port = device.Port,
                            MachineNo = device.MachineNo,
                            Password = "0"
                        });
                    }
                }

                dataPairPanel1.SetData(dataPairs);

                LogHelper.Log($"[DEBUG] DataPairPanel populated with {allDevices.Count} devices from AppSession.");

                // --- INTEGRATED CONTINUOUS DEVICE MONITORING ---
                if (allDevices.Count > 0)
                {
                    _deviceMonitorCts = new CancellationTokenSource();
                    var token = _deviceMonitorCts.Token;

                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(2000, token);

                            while (!token.IsCancellationRequested && !dataPairPanel1.IsDisposed)
                            {
                                try
                                {
                                    var pingTasks = allDevices.Select(async (device, index) =>
                                    {
                                        string finalStatus = await CommonUtils.PingDeviceWithStatusAsync(
                                            device.DeviceIP,
                                            status =>
                                            {
                                                if (!token.IsCancellationRequested && !dataPairPanel1.IsDisposed)
                                                    dataPairPanel1.BeginInvoke(new Action(() => dataPairPanel1.UpdateItemStatus(index, status)));
                                            }
                                        );

                                        if (!token.IsCancellationRequested && !dataPairPanel1.IsDisposed)
                                            dataPairPanel1.BeginInvoke(new Action(() => dataPairPanel1.UpdateItemStatus(index, finalStatus)));

                                        return finalStatus;
                                    }).ToList();

                                    string[] results = await Task.WhenAll(pingTasks);

                                    int onlineCount = results.Count(s => s == "ONLINE");

                                    if (!token.IsCancellationRequested && !dataPairPanel1.IsDisposed)
                                    {
                                        dataPairPanel1.BeginInvoke(new Action(() =>
                                        {
                                            if (lbl_connecteddevice != null)
                                                lbl_connecteddevice.Text = onlineCount.ToString();

                                            if (pic_device_status != null)
                                                pic_device_status.Image = (onlineCount > 0)
                                                    ? Properties.Resources.device_connected
                                                    : Properties.Resources.device_disconnected;
                                        }));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Log($"[Dashboard][Monitoring] Error in status loop: {ex.Message}");
                                }

                                await Task.Delay(60000, token);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Normal shutdown — form is closing
                        }
                    }, token);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[ERROR] Failed to populate DataPairPanel: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Refreshes the device list and their connection statuses manually 
        /// without re-initializing background monitoring threads.
        /// </summary>
        public async Task RefreshDevicePanelAsync()
        {
            try
            {
                var dataPairs = new List<Controls.DataPairItem>();

                // 1. Load latest devices from AppSession
                var devices = AppSession.DeviceList ?? new List<Device>();
                var staffDevices = AppSession.StaffDeviceList ?? new List<Device>();

                var allDevices = new List<Device>();
                allDevices.AddRange(devices);
                allDevices.AddRange(staffDevices);

                // 2. Build the list items
                if (allDevices.Count == 0)
                {
                    dataPairs.Add(new Controls.DataPairItem
                    {
                        Label = "No Devices",
                        Elements = new List<Controls.DataElement>
                {
                    new Controls.DataElement("No devices configured", Color.FromArgb(220, 53, 69))
                }
                    });
                }
                else
                {
                    foreach (var device in allDevices)
                    {
                        string deviceName = device.Location ?? "Device";
                        bool isStaff = AppSession.StaffDeviceList?.Any(sd => sd.DeviceIP == device.DeviceIP) ?? false;
                        string deviceType = isStaff ? "Staff" : "Member";

                        var elements = new List<Controls.DataElement>
                {
                    new Controls.DataElement($"Machine No: {device.MachineNo}"),
                    new Controls.DataElement($"IP: {device.DeviceIP ?? "N.A"}"),
                    new Controls.DataElement(device.IsActive ? "ONLINE" : "OFFLINE",
                        device.IsActive ? Color.FromArgb(34, 177, 76) : Color.FromArgb(220, 53, 69)),
                    new Controls.DataElement("Open", isLink: true, tooltip: $"Open {deviceName}")
                };

                        dataPairs.Add(new Controls.DataPairItem
                        {
                            Label = deviceName,
                            Type = deviceType,
                            Elements = elements,
                            IpAddress = device.DeviceIP,
                            Port = device.Port,
                            MachineNo = device.MachineNo,
                            Password = "0"
                        });
                    }
                }

                // 3. Update the UI control data
                if (!dataPairPanel1.IsDisposed)
                {
                    dataPairPanel1.SetData(dataPairs);
                }

                // 4. Perform immediate server-side status ping update
                if (allDevices.Count > 0)
                {
                    var pingTasks = allDevices.Select(async (device, index) =>
                    {
                        string finalStatus = await CommonUtils.PingDeviceWithStatusAsync(
                            device.DeviceIP,
                            status => {
                                if (!dataPairPanel1.IsDisposed)
                                    dataPairPanel1.BeginInvoke(new Action(() => dataPairPanel1.UpdateItemStatus(index, status)));
                            }
                        );

                        if (!dataPairPanel1.IsDisposed)
                            dataPairPanel1.BeginInvoke(new Action(() => dataPairPanel1.UpdateItemStatus(index, finalStatus)));

                        return finalStatus;
                    }).ToList();

                    string[] results = await Task.WhenAll(pingTasks);
                    int onlineCount = results.Count(s => s == "ONLINE");

                    // Update Counts and Icons
                    this.BeginInvoke(new Action(() =>
                    {
                        if (lbl_connecteddevice != null)
                        {
                            lbl_connecteddevice.Text = onlineCount.ToString();
                        }

                        if (pic_device_status != null)
                        {
                            pic_device_status.Image = (onlineCount > 0)
                                ? Properties.Resources.device_connected
                                : Properties.Resources.device_disconnected;
                        }
                    }));
                }

                LogHelper.Log($"[Dashboard] Device panel refreshed. Devices tracked: {allDevices.Count}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[ERROR] Failed to refresh device panel values: {ex.Message}");
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (_dbHelper == null)
            {
                MessageBox.Show("Services not ready.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Pick up to 10 active members with a valid EnrollmentID
            var members = _dbHelper.GetMembers()
                .Where(m => m.EnrollmentID.HasValue && m.EnrollmentID > 0
                         && string.Equals(m.Status, "active", StringComparison.OrdinalIgnoreCase))
                .OrderBy(_ => Guid.NewGuid())
                .Take(10)
                .ToList();

            if (members.Count == 0)
            {
                MessageBox.Show("No active members with EnrollmentID found.", "Test Bulk", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var rng = new Random();

            // Step 1: Insert check-in + check-out into AttendanceLogs with SyncStatus false
            foreach (var m in members)
            {
                DateTime checkIn  = DateTime.Now.AddHours(-rng.Next(1, 24)).AddMinutes(-rng.Next(0, 60));
                DateTime checkOut = checkIn.AddMinutes(rng.Next(30, 180));

                string checkInStr  = checkIn.ToString("yyyy-MM-dd HH:mm:ss");
                string checkOutStr = checkOut.ToString("yyyy-MM-dd HH:mm:ss");

                var (logId, attendanceId) = _dbHelper.InsertAttendanceLog(m.MemberID, checkInStr);
                if (logId > 0)
                {
                    _dbHelper.UpdateAttendanceLogForCheckOut(m.MemberID, checkOutStr);
                    LogHelper.Log($"[TestBulk] Inserted Member={m.MemberID} Enroll={m.EnrollmentID} LogID={logId} CheckIn={checkInStr} CheckOut={checkOutStr} (unsynced)");
                }
                else
                {
                    LogHelper.Log($"[TestBulk] Skipped Member={m.MemberID} — InsertAttendanceLog returned 0 (possible duplicate or validation failure)");
                }
            }

            LogHelper.Log($"[TestBulk] Done inserting {members.Count} records locally with SyncStatus=false.");
            MessageBox.Show($"Inserted {members.Count} records locally (unsynced).\nClick 'Send Bulk' to sync them to the server.",
                "Test Bulk — Step 1 Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var syncService = ServiceManager.AttendanceLogSyncService;
            if (syncService == null)
            {
                LogHelper.Log("[TestBulk] AttendanceLogSyncService not running — cannot trigger sync.");
                MessageBox.Show("Sync service not running.", "Test Bulk", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LogHelper.Log("[TestBulk] Checking unsynced attendance logs and sending bulk...");
            int synced = await syncService.SyncUnsyncedAttendanceLogsAsync(CancellationToken.None).ConfigureAwait(false);
            LogHelper.Log($"[TestBulk] Bulk sync complete. {synced} records processed.");

            this.BeginInvoke(new Action(() =>
                MessageBox.Show($"Bulk sync done. {synced} records sent to server.\nCheck logs for full payload details.",
                    "Test Bulk — Step 2 Done", MessageBoxButtons.OK, MessageBoxIcon.Information)));
        }




        /////////////////////////////////////////////////////////////////////////////////
    }
}
