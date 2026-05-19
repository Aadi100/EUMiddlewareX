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
        private bool _uiReady = false;
        private const int MaxLogLines = 1000000;
        public HttpHandler _httpHandler;

        private readonly object _loaderLock = new object();
        private int _loaderRefCount = 0;


        private Panel loadingOverlay;
        private Label loadingLabel;
        private bool servicesInitialized = false;
        private smart_attendance_middleware.UI.LoaderPopupForm _loaderPopup;
        private BackupService _backupService;
        private bool _attendanceInitialSyncHandled = false;
        private volatile bool _serverProgressKeepAlive = false;
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
            LogHelper.LogUpdated += AppendLogToTextBox; // Subscribe to log updates

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
                    dash._facePopup = new FacePopupForm(memberId, enrollmentId, action, time, 3, true);
                    dash._facePopup.Show();
                }
                else
                {
                    dash._facePopup.UpdateData(memberId, enrollmentId, action, time);
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
                _loaderPopup.Show(this); // owner
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




        private void AppendLogToTextBox(string logMessage)  // Method to append log messages to the TextBox
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AppendLogToTextBox(logMessage)));
            }
            else
            {
                txtLog.AppendText(logMessage + Environment.NewLine);
                TrimLogTextBox();
            }
        }

        private void TrimLogTextBox()
        {
            var lines = txtLog.Lines;
            if (lines.Length <= MaxLogLines)
            {
                return;
            }
            txtLog.Lines = lines.Skip(lines.Length - MaxLogLines).ToArray();
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }


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

        private void Dashboard_Load(object sender, EventArgs e)
        {
            UpdateAttendancePanels();
        }

        

        // Modify the Ready handler to finalize the bar and stop keep-alive
        private void AttendanceServerThreadManager_Ready()
        {
            // Unsubscribe to avoid repeated calls
            AttendanceServerThreadManager.Ready -= AttendanceServerThreadManager_Ready;
            LogHelper.Log("[Dashboard] AttendanceServer thread reported ready.");

            // Indicate server running on the progress bar
            if (prg_server != null)
            {
                prg_server.Style = ProgressBarStyle.Continuous;
                prg_server.Value = 100;
                prg_server.Visible = true;
            }

            // Stop keep-alive progress
            _serverProgressKeepAlive = false;

        }

        private async void OnAttendanceLogSyncCompleted()
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

                LogHelper.Log("🎯 Initial attendance log sync completed - entering post-batch delay window...");

                // Mark handled early to avoid re-entry
                _attendanceInitialSyncHandled = true;

                // Unsubscribe immediately (defensive)
                AttendanceLogSyncService.InitialSyncCompleted -= OnAttendanceLogSyncCompleted;

                // Determinate progress: 0->100 over 20s, then keep-alive up to 99% until server ready
                int delaySeconds = 20;

                if (prg_server != null)
                {
                    prg_server.Visible = true;
                    prg_server.Style = ProgressBarStyle.Continuous;
                    prg_server.Minimum = 0;
                    prg_server.Maximum = 100;
                    prg_server.Value = 0;
                }

                LogHelper.Log($"⏳ Finalizing initial sync... progressing server bar for {delaySeconds}s");

                int stepPerSecond = 100 / delaySeconds; // 5
                int current = 0;

                for (int remain = delaySeconds; remain > 0; remain--)
                {
                    if (prg_server != null)
                    {
                        current = Math.Min(100, current + stepPerSecond);
                        prg_server.Value = current;
                        prg_server.Refresh();
                    }

                    await Task.Delay(1000);
                }

                // After 20s, switch to keep-alive progress up to 99% until Ready sets it to 100%
                _serverProgressKeepAlive = true;
                LogHelper.Log("⏭️ Entering keep-alive progress mode until server is ready...");

                // Increment slowly to 99% (e.g., 1% every second) until Ready triggers
                while (_serverProgressKeepAlive && prg_server != null && prg_server.Value < 99)
                {
                    try
                    {
                        prg_server.Value = Math.Min(99, prg_server.Value + 1);
                        prg_server.Refresh();

                    }
                    catch
                    {
                        // Ignore UI update issues
                    }

                    await Task.Delay(1000);
                }

                // If server was already started manually, skip auto-start
                if (_attendanceServerStarted)
                {
                    LogHelper.Log("ℹ️ Attendance server already started manually. Skipping auto-start.");
                    return;
                }

                try
                {
                    AttendanceServerThreadManager.Ready -= AttendanceServerThreadManager_Ready;
                    AttendanceServerThreadManager.Ready += AttendanceServerThreadManager_Ready;

                    AttendanceServerThreadManager.Start(showUI: false, waitForReady: false);
                    _attendanceServerStarted = true;

                    LogHelper.Log("✅ AttendanceServerThreadManager start requested after progress completion");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Failed to start AttendanceServerThreadManager: {ex.Message}");
                    _serverProgressKeepAlive = false;
                    if (prg_server != null)
                    {
                        prg_server.Style = ProgressBarStyle.Continuous;
                        prg_server.Value = Math.Max(0, prg_server.Value - 10);
                        prg_server.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error in OnAttendanceLogSyncCompleted: {ex.Message}");
                _serverProgressKeepAlive = false;
                if (prg_server != null)
                {
                    prg_server.Style = ProgressBarStyle.Continuous;
                    prg_server.Value = 0;
                    prg_server.Refresh();
                }
            }
            finally
            {
                AttendanceLogSyncService.InitialSyncCompleted -= OnAttendanceLogSyncCompleted;
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

                // Update Missing Fingerprints button text (red button)
                if (btn_fingerprintno != null)
                {
                    btn_fingerprintno.Content = missingFingerprints.ToString();


                    // If there are no missing fingerprints, hide the button and label
                    if (missingFingerprints == 0)
                    {
                        btn_fingerprintno.Visible = false;
                        lbl_Fingerprints.Visible = false;
                    }
                    else
                    {
                        btn_fingerprintno.Visible = true;
                        btn_fingerprintno.Location = (new System.Drawing.Point(800, 204));
                        lbl_Fingerprints.Visible = true;
                        lbl_Fingerprints.Location = (new System.Drawing.Point(830, 207));
                        btn_fingerprintno.BringToFront();
                        lbl_Fingerprints.BringToFront();

                    }
                }

                // Enable/disable retry button based on pending jobs count and change color
                if (btn_retry != null)
                {
                    bool hasJobs = pendingJobs > 0;
                    btn_retry.Enabled = hasJobs;

                    if (hasJobs)
                    {
                        // Restore original colors when enabled
                        btn_retry.NormalBackground = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
                        btn_retry.HoverBackground = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(158)))), ((int)(((byte)(35)))));
                        btn_retry.NormalForeColor = System.Drawing.Color.White;
                        btn_retry.HoverForeColor = System.Drawing.Color.White;
                        LogHelper.Log($"🔘 Retry button enabled - pending jobs: {pendingJobs}");
                    }
                    else
                    {
                        // Set gray colors when disabled
                        btn_retry.NormalBackground = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                        btn_retry.HoverBackground = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                        btn_retry.NormalForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
                        btn_retry.HoverForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
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


        // Add a refresh method to manually update device status
        public async Task RefreshDeviceStatus()
        {

            // Also refresh the sync status information
            await RefreshDeviceSyncStatusAsync();
        }

        public async Task LoadDevicesAndCheckStatus()
        {
            await RefreshDeviceStatus();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (servicesInitialized) return;
            // Keep dashboard visible; just show loader popup
            ShowLoader("Preparing dashboard UI...");
            await LoadInitialUiAsync();
            _uiReady = true;
            HideLoader();

            // Delay before background services (requested 2 seconds)
            await Task.Delay(2000);

            ShowLoader("Starting services...");
            
            await StartServicesAsync();

            InitializeDevicePanel();

            dataPairPanel1.OpenDoorRequested += async (item) => {
                if (string.IsNullOrEmpty(item.IpAddress)) return;
                await CommonUtils.HandleDoorOpenRequestAsync(item.IpAddress, item.Port, item.MachineNo, item.Password, _dbHelper);
            };

            HideLoader();

            servicesInitialized = true;
        }

        // Add async initialization routine
        private async Task LoadInitialUiAsync()
        {
            try
            {
                UpdateLoaderMessage("Caching System data...");

                // Use CacheManager for initial load instead of direct database calls
                await _cacheManager.InitialLoadAsync();

                // Start automatic incremental updates every 30 seconds
                _cacheManager.StartAutoRefresh();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[LoadInitialUiAsync] Error caching data: {ex.Message}");
            }

            UpdateAttendancePanels();
            await Task.Yield(); // let UI pump once
        }

        private async Task StartServicesAsync()
        {
            try
            {
                UpdateLoaderMessage("Initializing UI timers...");
                attendancePanelTimer = new System.Windows.Forms.Timer { Interval = 5000 };
                attendancePanelTimer.Tick += (s, args) => UpdateAttendancePanels();
                attendancePanelTimer.Start();

                _processor.update_session();

                UpdateLoaderMessage("Starting General Log Sync Service...");
                await ServiceManager.StartLogSyncServiceAsync(_deviceFunctions);

                UpdateLoaderMessage("Starting Member Attendance Sync...");
                AttendanceLogSyncService.InitialSyncCompleted -= OnAttendanceLogSyncCompleted;
                AttendanceLogSyncService.InitialSyncCompleted += OnAttendanceLogSyncCompleted;
                await ServiceManager.StartAttendanceLogSyncServiceAsync();

                UpdateLoaderMessage("Starting Staff Attendance Sync...");
                await ServiceManager.StartStaffAttendanceLogSyncServiceAsync();

                UpdateLoaderMessage("Starting Periodic Maintenance Service...");
                await ServiceManager.StartPeriodicMaintenanceServiceAsync(this);

                UpdateLoaderMessage("Starting Backup Service...");
                if (_backupService == null)
                {
                    _backupService = new BackupService(_dbHelper);
                }
                _backupService.Start();

                UpdateLoaderMessage("Configuring connection monitors...");
                ConnectionStatusChecker.Instance.SetWebSocketManager(_wsManager);
                ConnectionStatusChecker.Instance.InternetStatusChanged -= OnInternetStatusChanged;
                ConnectionStatusChecker.Instance.InternetStatusChanged += OnInternetStatusChanged;

                ConnectionStatusChecker.Instance.SubscriptionStatusChanged -= OnSubscriptionStatusChanged;
                ConnectionStatusChecker.Instance.SubscriptionStatusChanged += OnSubscriptionStatusChanged;

                ConnectionStatusChecker.Instance.Start();

                UpdateLoaderMessage("Scanning hardware devices...");
                try
                {
                    await RefreshDeviceStatus();
                }
                catch (Exception ex)
                {
                    LogHelper.Log("[StartServicesAsync] Error refreshing device status: " + ex.Message);
                }

                UpdateLoaderMessage("Finalizing startup...");
            }
            catch (Exception ex)
            {
                LogHelper.Log("[StartServicesAsync][ERROR] " + ex.Message);
                UpdateLoaderMessage("Startup error! Check logs.");
                await Task.Delay(2000); // Allow user to see error message
            }
        }

        /// <summary>
        /// Handle cache update events - refresh UI when AppSession data changes
        /// </summary>
        private void OnCacheUpdated(object sender, CacheUpdateEventArgs e)
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
                    UpdateAttendancePanels();
                    LogHelper.Log("[Dashboard] Full cache refresh - UI updated");
                }
                else if (e.UpdateTypes != null && e.UpdateTypes.Count > 0)
                {
                    if (e.UpdateTypes.Contains(CacheUpdateType.AttendanceLogs) || 
                        e.UpdateTypes.Contains(CacheUpdateType.StaffAttendanceLogs))
                    {
                        UpdateAttendancePanels();
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

        private void ShowServerWhenReady()
        {
            AttendanceServerThreadManager.Ready -= ShowServerWhenReady;
            AttendanceServerThreadManager.ShowUI();
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
                // Stop cache manager auto-refresh
                _cacheManager?.StopAutoRefresh();

                // Unsubscribe AI chat logger events immediately to stop any further UI calls
                aiChatLogger.OnAddMessage -= AiChatLogger_OnAddMessage;
                aiChatLogger.OnClearMessages -= AiChatLogger_OnClearMessages;

                // Also detach log UI appender to avoid post-close UI updates
                LogHelper.LogUpdated -= AppendLogToTextBox;

                // Unsubscribe any pending attendance sync completion callback
                AttendanceLogSyncService.InitialSyncCompleted -= OnAttendanceLogSyncCompleted;

                if (!string.IsNullOrEmpty(AppSession.LoggedInUser))
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

        


        private bool _attendanceServerStarted = false;
        private void StartAttendanceServerManual()
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(StartAttendanceServerManual));
                    return;
                }

                if (_attendanceServerStarted)
                {
                    LogHelper.Log("ℹ️ Attendance server already started. Ignoring manual request.");
                    return;
                }

                LogHelper.Log("🟢 Manual request: starting attendance server...");

                // Subscribe to Ready BEFORE starting
                AttendanceServerThreadManager.Ready -= AttendanceServerThreadManager_Ready;
                AttendanceServerThreadManager.Ready += AttendanceServerThreadManager_Ready;

                AttendanceServerThreadManager.Start(showUI: false, waitForReady: false);
                _attendanceServerStarted = true;

                LogHelper.Log("✅ Attendance server start requested (manual).");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Manual attendance server start failed: {ex.Message}");
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Allow only role "admin" in any casing (admin / Admin / ADMIN)
            if (!"admin".Equals(AppSession.UserRole?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Access denied. Only admin can open Device Manager.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var devicemanager = new DeviceManager();
            devicemanager.ShowDialog();
        }

        private void btn_adddevice_Click(object sender, EventArgs e)
        {
            // Allow only role "admin" in any casing (admin / Admin / ADMIN)
            if (!"admin".Equals(AppSession.UserRole?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Access denied. Only admin can open Device Manager.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AddNewDevice addnewdevice = new AddNewDevice();
            addnewdevice.ShowDialog();

            //// Refresh device status after device manager is closed
            //Task.Run(async () =>
            //{
            //    await Task.Delay(500); // Small delay to ensure database changes are committed
            //    this.Invoke(new Action(async () => await LoadDevicesAndCheckStatus()));
            //});
        }


        //public async Task LoadDevicesAndCheckStatus()
        //{
        //    await RefreshDeviceStatus();
        //}

        private void RunOnUi(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
                return;
            }

            action();
        }




        // Add a field to track the current view state
        private bool isShowingFingerprints = false;

        private async void lbl_Fingerprints_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!isShowingFingerprints)
            {
                // Switch to fingerprints view
                lbl_Fingerprints.Text = "AI Assistant";
                aiChat.Visible = false;
                lst_missing_fingerprints.Visible = true;
                btn_fingerprintno.Visible = false;
                isShowingFingerprints = true;

                // Initialize and load the missing fingerprints data
                await InitializeMissingFingerprintsListView();
            }
            else
            {
                // Switch back to AI Assistant view
                lbl_Fingerprints.Text = "Fingerprint Tasks";
                lst_missing_fingerprints.Visible = false;
                btn_fingerprintno.Visible = true;
                aiChat.Visible = true;
                isShowingFingerprints = false;
            }
        }

        private async Task InitializeMissingFingerprintsListView()
        {
            try
            {
                // Initialize ListView structure
                lst_missing_fingerprints.View = View.Details;
                lst_missing_fingerprints.FullRowSelect = true;
                lst_missing_fingerprints.Columns.Clear();

                lst_missing_fingerprints.Columns.Add("Member ID", 80);
                lst_missing_fingerprints.Columns.Add("Name", 150);
                lst_missing_fingerprints.Columns.Add("Enrollment ID", 100);

                // Subscribe to double-click event if not already subscribed
                lst_missing_fingerprints.DoubleClick -= lst_missing_fingerprints_DoubleClick;
                lst_missing_fingerprints.DoubleClick += lst_missing_fingerprints_DoubleClick;

                // Load the missing fingerprint data
                await LoadMissingFingerprintMembersAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error initializing missing fingerprints ListView: {ex.Message}");
            }
        }

        private async Task LoadMissingFingerprintMembersAsync()
        {
            try
            {
                lst_missing_fingerprints.Items.Clear();

                List<Member> members = await _dbHelper.GetMembersWithMissingFingerprintsAsync();

                foreach (var member in members)
                {
                    ListViewItem item = new ListViewItem(member.MemberID.ToString());
                    item.SubItems.Add(member.Name);
                    item.SubItems.Add(member.EnrollmentID?.ToString() ?? "NULL");

                    lst_missing_fingerprints.Items.Add(item);
                }

                LogHelper.Log($"Listed {members.Count} members with missing fingerprints.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error loading missing fingerprint members: {ex.Message}");
                MessageBox.Show("Failed to load members. See logs for more info.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lst_missing_fingerprints_DoubleClick(object sender, EventArgs e)
        {
            if (lst_missing_fingerprints.SelectedItems.Count == 0)
                return;

            // Get the selected item
            ListViewItem selectedItem = lst_missing_fingerprints.SelectedItems[0];

            // Get enrollment ID from the third column (index 2)
            string enrollmentId = selectedItem.SubItems[2].Text;

            // Validate the enrollment ID
            if (string.IsNullOrWhiteSpace(enrollmentId) || enrollmentId == "NULL")
            {
                MessageBox.Show("Selected member doesn't have a valid enrollment ID.",
                              "Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Create and show the Fingerprint form
                Fingerprint fingerprintForm = new Fingerprint();

                // Pass the enrollment ID using the public property
                fingerprintForm.EnrollmentID = enrollmentId;

                // Optional: Set form position relative to parent
                fingerprintForm.StartPosition = FormStartPosition.CenterParent;

                // Show the form (use ShowDialog() for modal)
                fingerprintForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening fingerprint form: {ex.Message}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                LogHelper.Log($"Error in double-click handler: {ex.Message}");
            }
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

        private void UpdateAttendancePanels()
        {
            try
            {
                // Update last check-in panel
                if (AppSession.LastCheckIn != null)
                {
                    var checkIn = AppSession.LastCheckIn;

                    // Update labels
                    lbl_name_in.Text = checkIn.MemberName;
                    lbl_action_in.Text = $"Action: {checkIn.Action.ToUpper()}";
                    lbl_time_in.Text = $"Time: {checkIn.Time:HH:mm:ss}";
                    lbl_enroll_no_in.Text = $"Enroll ID: {checkIn.EnrollmentId}";   

                    // Load member image
                    LoadMemberImage(pictureBox2, checkIn.ImagePath, checkIn.MemberId);
                }
                else
                {
                    // Clear check-in panel
                    lbl_name_in.Text = "No recent check-ins";
                    lbl_action_in.Text = "Action: --";
                    lbl_time_in.Text = "Time: --";
                    lbl_enroll_no_in.Text = "Enroll ID: --";
                    pictureBox2.Image = null;
                }

                // Update last check-out panel  
                if (AppSession.LastCheckOut != null)
                {
                    var checkOut = AppSession.LastCheckOut;

                    // Update labels
                    lbl_name_out.Text = checkOut.MemberName;
                    lbl_action_out.Text = $"Action: {checkOut.Action.ToUpper()}";
                    lbl_time_out.Text = $"Time: {checkOut.Time:HH:mm:ss}";
                    lbl_enroll_no_out.Text = $"Enroll ID: {checkOut.EnrollmentId}";

                    // Load member image
                    LoadMemberImage(pictureBox3, checkOut.ImagePath, checkOut.MemberId);
                }
                else
                {
                    // Clear check-out panel
                    lbl_name_out.Text = "No recent check-outs";
                    lbl_action_out.Text = "Action: --";
                    lbl_time_out.Text = "Time: --";
                    lbl_enroll_no_out.Text = "Enroll ID: --";
                    pictureBox3.Image = null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[UpdateAttendancePanels][ERROR] {ex.Message}");
            }
        }

        private void LoadMemberImage(PictureBox pictureBox, string imagePath, int memberId)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    // Load image from file
                    using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        Image image = Image.FromStream(fs);

                        // Check if the image is too large
                        int maxWidth = pictureBox.Width;
                        int maxHeight = pictureBox.Height;

                        if (image.Width > maxWidth || image.Height > maxHeight)
                        {
                            // Calculate the new dimensions while maintaining aspect ratio
                            double widthRatio = (double)maxWidth / image.Width;
                            double heightRatio = (double)maxHeight / image.Height;
                            double ratio = Math.Min(widthRatio, heightRatio);

                            int newWidth = (int)(image.Width * ratio);
                            int newHeight = (int)(image.Height * ratio);

                            // Create a resized copy of the image
                            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
                            using (Graphics g = Graphics.FromImage(resizedImage))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.DrawImage(image, 0, 0, newWidth, newHeight);
                            }

                            // Dispose of the original image and use the resized image
                            image.Dispose();
                            pictureBox.Image = resizedImage;
                        }
                        else
                        {
                            pictureBox.Image = new Bitmap(image);
                        }
                    }
                }
                else
                {
                    // Create default placeholder image
                    CreateDefaultMemberImage(pictureBox, memberId);
                }
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
                    Task.Run(async () =>
                    {
                        // Initial delay before first check
                        await Task.Delay(2000);

                        while (!dataPairPanel1.IsDisposed)
                        {
                            try
                            {
                                // Run pings for all devices in parallel and collect results
                                var pingTasks = allDevices.Select(async (device, index) =>
                                {
                                    string finalStatus = await CommonUtils.PingDeviceWithStatusAsync(
                                        device.DeviceIP,
                                        status => {
                                            // Callback to show "RETRYING" (Orange) immediately on first failure
                                            if (!dataPairPanel1.IsDisposed)
                                                dataPairPanel1.BeginInvoke(new Action(() => dataPairPanel1.UpdateItemStatus(index, status)));
                                        }
                                    );

                                    if (!dataPairPanel1.IsDisposed)
                                        dataPairPanel1.BeginInvoke(new Action(() => dataPairPanel1.UpdateItemStatus(index, finalStatus)));

                                    return finalStatus;
                                }).ToList();

                                string[] results = await Task.WhenAll(pingTasks);

                                // Dynamically update the connected devices count based on pings
                                int onlineCount = results.Count(s => s == "ONLINE");

                                if (!dataPairPanel1.IsDisposed)
                                {
                                    dataPairPanel1.BeginInvoke(new Action(() =>
                                    {
                                        // Update the numeric label
                                        if (lbl_connecteddevice != null)
                                        {
                                            lbl_connecteddevice.Text = onlineCount.ToString();
                                        }

                                        // Update the status icon: connected if at least one is online
                                        if (pic_device_status != null)
                                        {
                                            pic_device_status.Image = (onlineCount > 0)
                                                ? Properties.Resources.device_connected
                                                : Properties.Resources.device_disconnected;
                                        }
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Log($"[Dashboard][Monitoring] Error in status loop: {ex.Message}");
                            }

                            // Wait for 1 minute before the next sweep
                            await Task.Delay(60000);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[ERROR] Failed to populate DataPairPanel: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                LogHelper.Log("🎲 Simulating direct attendance injection (8 hours ago)...");

                // 1. Pick a random active member
                var members = _dbHelper.GetMembers().Where(m => m.EnrollmentID.HasValue && m.Status == "active").ToList();
                if (members.Count == 0) return;

                var random = new Random();
                var member = members[random.Next(members.Count)];
                DateTime simulatedTime = DateTime.Now.AddHours(-8);
                string simulatedTimeStr = simulatedTime.ToString("yyyy-MM-dd HH:mm:ss");

                // 2. Insert directly into Database with Lock
                int logId = 0;
                string attendanceGuid = null;

                await DatabaseQueueManager.EnterDbWriteLockAsync("DirectManualAttendance", DbTaskPriority.High);
                try
                {
                    var dbResult = _dbHelper.InsertAttendanceLog(member.MemberID, simulatedTimeStr);
                    logId = dbResult.LogId;
                    attendanceGuid = dbResult.AttendanceId;
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("DirectManualAttendance");
                }

                if (logId > 0 && !string.IsNullOrEmpty(attendanceGuid))
                {
                    LogHelper.Log($"✅ Local Log Created: Member={member.Name}, LogID={logId}, GUID={attendanceGuid}");

                    // 3. Send directly to Server via HttpHandler
                    // We convert the simulated time to UTC-0 as required by the backend
                    var gmt = _dbHelper.GetSetting("GMT");
                    string utcTimeStr = CommonUtils.ConvertToUtc0(simulatedTime, gmt);

                    await _httpHandler.PostAttendanceRecordAsync(
                        member.EnrollmentID.Value,
                        checkinTime: utcTimeStr,
                        checkoutTime: null,
                        id: attendanceGuid
                    );

                    LogHelper.Log($"🚀 Attendance pushed to cloud for Member={member.MemberID}");

                    // Update UI Dashboard panels
                    UpdateAttendancePanels();
                }
                else
                {
                    LogHelper.Log("⚠️ Database insertion failed (likely timestamp validation).");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error in button1_Click: {ex.Message}");
            }
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                LogHelper.Log("⚡ Manually triggering Force Check-Out operation...");

                // Accessing the maintenance service through the ServiceManager
                var maintenanceService = ServiceManager.PeriodicMaintenanceService;

                if (maintenanceService != null)
                {
                    // Using the existing HttpHandler which is required for cloud syncing during force checkout
                    await CommonUtils.ProcessForceCheckOutAsync(_dbHelper, _httpHandler).ConfigureAwait(false);
                    LogHelper.Log("✅ Force Check-Out operation completed successfully.");
                }
                else
                {
                    LogHelper.Log("⚠️ PeriodicMaintenanceService is not initialized.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error in button2_Click: {ex.Message}");
            }
        }
        /////////////////////////////////////////////////////////////////////////////////
    }
}
