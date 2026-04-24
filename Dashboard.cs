using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
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
        private System.Windows.Forms.Timer countdownTimer;
        private int countdownSeconds;
        private string originalButtonText;
        private System.Windows.Forms.Timer attendancePanelTimer;
        private bool _uiReady = false;
        private const int MaxLogLines = 10;
        private HttpHandler _httpHandler;

        private readonly object _loaderLock = new object();
        private int _loaderRefCount = 0;


        private Panel loadingOverlay;
        private Label loadingLabel;
        private bool servicesInitialized = false;
        private smart_attendance_middleware.UI.LoaderPopupForm _loaderPopup;
        private BackupService _backupService;
        private bool _attendanceInitialSyncHandled = false;
        private volatile bool _serverProgressKeepAlive = false;

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

            // Initialize lbl_NoDevices to be visible initially
            lbl_NoDevices.Visible = true;

            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
            InitializeWebSocketManager();


            // Create DeviceFunctions with the WebSocketManager
            _deviceFunctions = new DeviceFunctions(_wsManager);
            _processor = new Processor(_dbHelper, _wsManager, _deviceFunctions);
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

            this.FormClosing += Dashboard_FormClosing;


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
            OnDeviceExistCheck();
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

        // Add these public methods to the Dashboard class after the existing methods

        /// <summary>
        /// Updates the border color of a specific device panel based on its retry state
        /// Called by ConnectionStatusChecker when device status changes
        /// </summary>
        /// <param name="deviceId">The device ID to update</param>
        /// <param name="isRetrying">True to set orange border (retrying), false to reset to normal</param>
        public async Task UpdateDevicePanelBorderColor(int deviceId, bool isRetrying)
        {
            try
            {
                // Ensure this runs on UI thread
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => _ = UpdateDevicePanelBorderColor(deviceId, isRetrying)));
                    return;
                }

                // Load devices to find the panel number for this deviceId
                var devices = _dbHelper.LoadDevices();

                // Find the device index (panel number)
                var deviceIndex = devices.FindIndex(d => d.DeviceId == deviceId);

                if (deviceIndex >= 0 && deviceIndex < 4) // Only first 4 devices have panels
                {
                    int panelNumber = deviceIndex + 1; // Panel numbers are 1-based

                    // Get the appropriate panel control
                    CuoreUI.Controls.cuiPanel panel = null;

                    switch (panelNumber)
                    {
                        case 1:
                            panel = panelDevice1;
                            break;
                        case 2:
                            panel = panelDevice2;
                            break;
                        case 3:
                            panel = panelDevice3;
                            break;
                        case 4:
                            panel = panelDevice4;
                            break;
                    }

                    if (panel != null)
                    {
                        if (isRetrying)
                        {
                            // Set orange border for retry state
                            panel.PanelOutlineColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(165)))), ((int)(((byte)(0)))));
                            panel.OutlineThickness = 2F;
                            LogHelper.Log($"Set panelDevice{panelNumber} border to orange (device {deviceId} retrying)");
                        }
                        else
                        {
                            // Reset to original border color
                            panel.PanelOutlineColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
                            panel.OutlineThickness = 1F;

                            LogHelper.Log($"Reset panelDevice{panelNumber} border to normal (device {deviceId} status resolved)");
                        }

                        // Force panel refresh
                        panel.Invalidate();
                    }
                    else
                    {
                        LogHelper.Log($"Panel panelDevice{panelNumber} not found for device ID {deviceId}");
                    }
                }
                else
                {
                    LogHelper.Log($"Device ID {deviceId} not found in first 4 devices or device index out of range");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error updating panel border color for device {deviceId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Called by ConnectionStatusChecker when device status changes are detected after all retries
        /// This will refresh the entire device status display
        /// </summary>
        public async Task OnDeviceStatusChangedAfterRetries()
        {
            try
            {
                LogHelper.Log("🔄 Device status changed after retries - refreshing device display...");

                // Ensure this runs on UI thread
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => _ = OnDeviceStatusChangedAfterRetries()));
                    return;
                }

                // Add AI message about device status change
                aiChatLogger.AddDeviceMessage("Device connectivity has changed. Let me update the status for you...");

                // Refresh the device status display
                await RefreshDeviceStatus();

                LogHelper.Log("✅ Device status refresh completed after retry process");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error refreshing device status after retries: {ex.Message}");
                aiChatLogger.AddErrorMessage("Had trouble updating device status. I'll keep monitoring in the background.");
            }
        }

        // Add a refresh method to manually update device status
        public async Task RefreshDeviceStatus()
        {
            // Call the original device status refresh
            await LoadDevicesAndCheckStatus();

            // Also refresh the sync status information
            await RefreshDeviceSyncStatusAsync();
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
                HideLoader();

            servicesInitialized = true;
        }

        // Add async initialization routine
        private async Task LoadInitialUiAsync()
        {
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

                ConnectionStatusChecker.Instance.BiometricDeviceStatusChanged -= OnBiometricStatusChanged;
                ConnectionStatusChecker.Instance.BiometricDeviceStatusChanged += OnBiometricStatusChanged;

                ConnectionStatusChecker.Instance.SubscriptionStatusChanged -= OnSubscriptionStatusChanged;
                ConnectionStatusChecker.Instance.SubscriptionStatusChanged += OnSubscriptionStatusChanged;

                ConnectionStatusChecker.Instance.StaffDeviceStatusChanged -= OnStaffDeviceStatusChanged;
                ConnectionStatusChecker.Instance.StaffDeviceStatusChanged += OnStaffDeviceStatusChanged;

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

        public void OnDeviceExistCheck()
        {
            //isExist = _dbHelper.DoesDeviceExist();

            //btn_setup.Image = isExist
            //    ? Properties.Resources.btn_device_manager
            //    : Properties.Resources.btn_Device_Setup;

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
                ConnectionStatusChecker.Instance.BiometricDeviceStatusChanged -= OnBiometricStatusChanged;
                ConnectionStatusChecker.Instance.SubscriptionStatusChanged -= OnSubscriptionStatusChanged;
                ConnectionStatusChecker.Instance.StaffDeviceStatusChanged -= OnStaffDeviceStatusChanged;
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

        private void cuiButton7_Click(object sender, EventArgs e)
        {
            try
            {
                string url = "https://smart-gym-studio-staging.vercel.app/auth/login?callbackUrl=%2Fdashboard";
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening URL in browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Refresh device status after device manager is closed
            Task.Run(async () =>
            {
                await Task.Delay(500); // Small delay to ensure database changes are committed
                this.Invoke(new Action(async () => await LoadDevicesAndCheckStatus()));
            });
        }

        private async Task LoadDevicesAndCheckStatus()
        {
            try
            {
                RunOnUi(() => aiChatLogger.AddDeviceMessage("Scanning for your devices..."));

                // Use AppSession.DeviceList instead of _dbHelper.LoadDevices()
                var devices = AppSession.DeviceList ?? new List<Device>();

                var devicesToCheck = devices.Take(4).ToList();
                var pingResults = await Task.WhenAll(devicesToCheck.Select(device => PingDevice(device.DeviceIP))).ConfigureAwait(false);

                RunOnUi(() =>
                {
                    // Hide all device panels initially
                    panelDevice1.Visible = false;
                    panelDevice2.Visible = false;
                    panelDevice3.Visible = false;
                    panelDevice4.Visible = false;

                    var devicePanels = new[] { panelDevice1, panelDevice2, panelDevice3, panelDevice4 };

                    label3.Text = $"{devices.Count} total";
                    label4.Text = "0";

                    int connectedCount = 0;

                    // Show/Hide the "No Devices Available" label based on device count
                    if (devices.Count == 0)
                    {
                        lbl_NoDevices.Visible = true;
                        lbl_NoDevices.BringToFront();
                        lbl_NoDevices.Location = new Point(95, 300);
                        aiChatLogger.AddInfoMessage("No devices configured yet. Would you like me to help you add your first device?");
                        return;
                    }

                    lbl_NoDevices.Visible = false;
                    aiChatLogger.AddInfoMessage($"Found {devices.Count} configured device(s). Checking their status...");

                    // Define the initial Y position for the first panel
                    int initialY = 130; // Adjust this value based on your layout
                    int initialX = 40;

                    // Process up to 4 devices
                    for (int i = 0; i < Math.Min(devicesToCheck.Count, 4); i++)
                    {
                        var device = devicesToCheck[i];
                        var panel = devicePanels[i];

                        // Update device information
                        UpdateDevicePanel(panel, device, i + 1);

                        // Check device status via ping
                        bool isOnline = pingResults[i];

                        if (isOnline)
                        {
                            connectedCount++;
                            UpdateDeviceStatus(panel, true, i + 1);
                        }
                        else
                        {
                            UpdateDeviceStatus(panel, false, i + 1);
                        }

                        // Set the panel's location
                        panel.Location = new Point(initialX, initialY + (i * (panel.Height + 5))); // Keep the original X, adjust Y
                        panel.Visible = true;
                        // Bring the panel to the front
                        panel.BringToFront();
                    }

                    // Update connected devices count
                    label4.Text = connectedCount.ToString();

                    // AI personality responses based on device status
                    if (connectedCount == devices.Count)
                    {
                        aiChatLogger.AddSuccessMessage($"Excellent! All {devices.Count} devices are online and ready to work!");
                    }
                    else if (connectedCount == 0)
                    {
                        aiChatLogger.AddErrorMessage("Hmm, none of your devices are responding.");
                    }
                    else
                    {
                        aiChatLogger.AddWarningMessage($"{connectedCount} out of {devices.Count} devices are online. I'll keep monitoring the others.");
                    }

                    LogHelper.Log($"Device status check completed: {connectedCount}/{devices.Count} devices online");
                });
            }
            catch (Exception ex)
            {
                RunOnUi(() => aiChatLogger.AddErrorMessage("Encountered an issue while checking device status. Let me try again in a moment."));
                LogHelper.Log($"Error loading devices: {ex.Message}");
            }
        }

        private void RunOnUi(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
                return;
            }

            action();
        }


        private void UpdateDevicePanel(CuoreUI.Controls.cuiPanel panel, Device device, int deviceNumber)
        {
            try
            {
                // Get the labels from the panel using the correct naming pattern
                var locationLabel = panel.Controls[$"dev{deviceNumber}_location"] as Label;
                var machineLabel = panel.Controls[$"dev{deviceNumber}_machineno"] as Label;
                var ipLabel = panel.Controls[$"dev{deviceNumber}_ip"] as Label;

                if (locationLabel != null)
                {
                    // Use the actual device location from the database
                    string displayLocation = !string.IsNullOrWhiteSpace(device.Location) ? device.Location : "Default Location";
                    locationLabel.Text = displayLocation;
                }

                if (machineLabel != null)
                {
                    machineLabel.Text = $"Machine No: {device.MachineNo}";
                }

                if (ipLabel != null)
                {
                    ipLabel.Text = $"Device IP: {device.DeviceIP}";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error updating device panel {deviceNumber}: {ex.Message}");
            }
        }

        private void UpdateDeviceStatus(CuoreUI.Controls.cuiPanel panel, bool isOnline, int deviceNumber)
        {
            try
            {
                // Find the status picture box in the panel using the correct naming pattern
                var statusPictureBox = panel.Controls[$"dev{deviceNumber}_status"] as CuoreUI.Controls.cuiPictureBox;

                if (statusPictureBox != null)
                {
                    if (isOnline)
                    {
                        statusPictureBox.BackgroundImage = Properties.Resources.device_connected_status;
                    }
                    else
                    {
                        statusPictureBox.BackgroundImage = Properties.Resources.device_offline;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error updating device status for device {deviceNumber}: {ex.Message}");
            }
        }

        private async Task<bool> PingDevice(string ipAddress, int timeout = 3000)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ipAddress, timeout);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Ping failed for {ipAddress}: {ex.Message}");
                return false;
            }
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

        // In Dashboard.cs, modify lnk_dooropen1_LinkClicked
        private async void lnk_dooropen1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                // Locate the label containing the device IP in panelDevice1.
                if (panelDevice1 != null && panelDevice1.Controls["dev1_ip"] is Label ipLabel)
                {
                    // Expected format: "Device IP: x.x.x.x"
                    string ipText = ipLabel.Text;
                    string ip = ipText.Replace("Device IP:", "").Trim();

                    // Log the action
                    LogHelper.Log($"[lnk_dooropen1_LinkClicked] Attempting to open door for IP: {ip}");

                    // Get the logged-in user
                    string loggedInUser = AppSession.LoggedInUser;

                    // Show the GateAccessForm
                    GateAccessForm gateAccessForm = new GateAccessForm(ip, _dbHelper, loggedInUser);
                    DialogResult result = gateAccessForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // Load devices from AppSession and find the matching device by IP.
                        var matchingDevice = AppSession.DeviceList?.FirstOrDefault(d => d.DeviceIP == ip);
                        if (matchingDevice == null)
                        {
                            string errorMessage = "Device not found in App Session.";
                            LogHelper.Log($"[lnk_dooropen1_LinkClicked] Error: {errorMessage}");
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Retrieve parameters from the matching device.
                        int port = matchingDevice.Port;
                        int machineNo = matchingDevice.MachineNo;
                        string password = matchingDevice.Password;

                        // Call the OpenDoor function.
                        bool doorOpened = await DeviceService.OpenDoor(ip, port, machineNo, password);
                        if (doorOpened)
                        {
                            string successMessage = "Door opened successfully.";
                            LogHelper.Log($"[lnk_dooropen1_LinkClicked] Success: {successMessage}");
                        }
                        else
                        {
                            string errorMessage = "Failed to open door.";
                            LogHelper.Log($"[lnk_dooropen1_LinkClicked] Error: {errorMessage}");
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        string infoMessage = "Gate access cancelled.";
                        LogHelper.Log($"[lnk_dooropen1_LinkClicked] Info: {infoMessage}");
                        MessageBox.Show(infoMessage, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    string errorMessage = "Device IP label not found.";
                    LogHelper.Log($"[lnk_dooropen1_LinkClicked] Error: {errorMessage}");
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred: {ex.Message}";
                LogHelper.Log($"[lnk_dooropen1_LinkClicked] Exception: {errorMessage}");
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void lnk_dooropen2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                // Locate the label containing the device IP in panelDevice2.
                if (panelDevice2 != null && panelDevice2.Controls["dev2_ip"] is Label ipLabel)
                {
                    // Expected format: "Device IP: x.x.x.x"
                    string ipText = ipLabel.Text;
                    string ip = ipText.Replace("Device IP:", "").Trim();

                    // Log the action
                    LogHelper.Log($"[lnk_dooropen2_LinkClicked] Attempting to open door for IP: {ip}");

                    // Get the logged-in user
                    string loggedInUser = AppSession.LoggedInUser;

                    // Show the GateAccessForm
                    GateAccessForm gateAccessForm = new GateAccessForm(ip, _dbHelper, loggedInUser);
                    DialogResult result = gateAccessForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // Load devices from AppSession and find the matching device by IP.
                        var matchingDevice = AppSession.DeviceList?.FirstOrDefault(d => d.DeviceIP == ip);
                        if (matchingDevice == null)
                        {
                            string errorMessage = "Device not found in App Session.";
                            LogHelper.Log($"[lnk_dooropen2_LinkClicked] Error: {errorMessage}");
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Retrieve parameters from the matching device.
                        int port = matchingDevice.Port;
                        int machineNo = matchingDevice.MachineNo;
                        string password = matchingDevice.Password;

                        // Call the OpenDoor function.
                        bool doorOpened = await DeviceService.OpenDoor(ip, port, machineNo, password);
                        if (doorOpened)
                        {
                            string successMessage = "Door opened successfully.";
                            LogHelper.Log($"[lnk_dooropen2_LinkClicked] Success: {successMessage}");
                        }
                        else
                        {
                            string errorMessage = "Failed to open door.";
                            LogHelper.Log($"[lnk_dooropen2_LinkClicked] Error: {errorMessage}");
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        string infoMessage = "Gate access cancelled.";
                        LogHelper.Log($"[lnk_dooropen2_LinkClicked] Info: {infoMessage}");
                        MessageBox.Show(infoMessage, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    string errorMessage = "Device IP label not found.";
                    LogHelper.Log($"[lnk_dooropen2_LinkClicked] Error: {errorMessage}");
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred: {ex.Message}";
                LogHelper.Log($"[lnk_dooropen2_LinkClicked] Exception: {errorMessage}");
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void lnk_dooropen3_LinkClicked(object sender, EventArgs e)
        {
            try
            {
                // Locate the label containing the device IP in panelDevice3.
                if (panelDevice3 != null && panelDevice3.Controls["dev3_ip"] is Label ipLabel)
                {
                    // Expected format: "Device IP: x.x.x.x"
                    string ipText = ipLabel.Text;
                    string ip = ipText.Replace("Device IP:", "").Trim();

                    // Log the action
                    LogHelper.Log($"[lnk_dooropen3_LinkClicked] Attempting to open door for IP: {ip}");

                    // Get the logged-in user
                    string loggedInUser = AppSession.LoggedInUser;

                    // Show the GateAccessForm
                    GateAccessForm gateAccessForm = new GateAccessForm(ip, _dbHelper, loggedInUser);
                    DialogResult result = gateAccessForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // Load devices from AppSession and find the matching device by IP.
                        var matchingDevice = AppSession.DeviceList?.FirstOrDefault(d => d.DeviceIP == ip);
                        if (matchingDevice == null)
                        {
                            string errorMessage = "Device not found in App Session.";
                            LogHelper.Log($"[lnk_dooropen3_LinkClicked] Error: {errorMessage}");
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Retrieve parameters from the matching device.
                        int port = matchingDevice.Port;
                        int machineNo = matchingDevice.MachineNo;
                        string password = matchingDevice.Password;

                        // Call the OpenDoor function.
                        bool doorOpened = await DeviceService.OpenDoor(ip, port, machineNo, password);
                        if (doorOpened)
                        {
                            string successMessage = "Door opened successfully.";
                            LogHelper.Log($"[lnk_dooropen3_LinkClicked] Success: {successMessage}");
                        }
                        else
                        {
                            string errorMessage = "Failed to open door.";
                            LogHelper.Log($"[lnk_dooropen3_LinkClicked] Error: {errorMessage}");
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        string infoMessage = "Gate access cancelled.";
                        LogHelper.Log($"[lnk_dooropen3_LinkClicked] Info: {infoMessage}");
                        MessageBox.Show(infoMessage, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    string errorMessage = "Device IP label not found.";
                    LogHelper.Log($"[lnk_dooropen3_LinkClicked] Error: {errorMessage}");
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred: {ex.Message}";
                LogHelper.Log($"[lnk_dooropen3_LinkClicked] Exception: {errorMessage}");
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void lnk_dooropen4_LinkClicked(object sender, EventArgs e)
        {
            try
            {
                // Locate the label containing the device IP in panelDevice4.
                if (panelDevice4 != null && panelDevice4.Controls["dev4_ip"] is Label ipLabel)
                {
                    // Expected format: "Device IP: x.x.x.x"
                    string ipText = ipLabel.Text;
                    string ip = ipText.Replace("Device IP:", "").Trim();

                    // Log the action
                    LogHelper.Log($"[lnk_dooropen4_LinkClicked] Attempting to open door for IP: {ip}");

                    // Get the logged-in user
                    string loggedInUser = AppSession.LoggedInUser;

                    // Show the GateAccessForm
                    GateAccessForm gateAccessForm = new GateAccessForm(ip, _dbHelper, loggedInUser);
                    DialogResult result = gateAccessForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // Load devices from AppSession and find the matching device by IP.
                        var matchingDevice = AppSession.DeviceList?.FirstOrDefault(d => d.DeviceIP == ip);
                        if (matchingDevice == null)
                        {
                            string errorMessage = "Device not found in App Session.";
                            LogHelper.Log($"[lnk_dooropen4_LinkClicked] Error: {errorMessage}");
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Retrieve parameters from the matching device.
                        int port = matchingDevice.Port;
                        int machineNo = matchingDevice.MachineNo;
                        string password = matchingDevice.Password;

                        // Call the OpenDoor function.
                        bool doorOpened = await DeviceService.OpenDoor(ip, port, machineNo, password);
                        if (doorOpened)
                        {
                            string successMessage = "Door opened successfully.";
                            LogHelper.Log($"[lnk_dooropen4_LinkClicked] Success: {successMessage}");
                        }
                        else
                        {
                            string errorMessage = "Failed to open door.";
                            LogHelper.Log($"[lnk_dooropen4_LinkClicked] Error: {errorMessage}");
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        string infoMessage = "Gate access cancelled.";
                        LogHelper.Log($"[lnk_dooropen4_LinkClicked] Info: {infoMessage}");
                    }
                }
                else
                {
                    string errorMessage = "Device IP label not found.";
                    LogHelper.Log($"[lnk_dooropen4_LinkClicked] Error: {errorMessage}");
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error occurred: {ex.Message}";
                LogHelper.Log($"[lnk_dooropen4_LinkClicked] Exception: {errorMessage}");
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        

        private async void button1_Click(object sender, EventArgs e)
        {
            // Ensure dependencies
            if (_dbHelper == null)
            {
                _dbHelper = new DatabaseHelper();
                _dbHelper.InitializeDatabase();
            }

            if (_wsManager == null)
            {
                MessageBox.Show("WebSocket Manager not initialized. Start services first.", "Simulator",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Open the SimulatorForm (normal UI). Simulation is controlled from that form.
            try
            {
                var form = new SimulatorForm(_dbHelper, _wsManager);

                // Optional: show as non-modal and bring to front
                form.Show();
                form.BringToFront();

                AppendLogToTextBox("[SIM] Simulator UI opened. Use Start (1h) to run and view real-time data.");
            }
            catch (Exception ex)
            {
                LogHelper.Log("[SIM][UI][ERROR] " + ex.Message);
                MessageBox.Show("Failed to open Simulator UI: " + ex.Message, "Simulator",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private const string HardcodedUuid = "d3e7a3b5-4d1b-4e8a-8f3b-2e2b3b4c5d6e";
        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int enrollmentId = 118;
                string checkinTime = DateTime.UtcNow.ToString("o"); // ISO 8601 format

                await _httpHandler.PostAttendanceRecordAsync(enrollmentId, checkinTime: checkinTime, id: HardcodedUuid);
                LogHelper.Log($"✅ Sent Check-In for Enrollment ID: {enrollmentId} with UUID: {HardcodedUuid}");
                MessageBox.Show($"Check-In sent for Enrollment ID: {enrollmentId}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Check-In failed: {ex.Message}");
                MessageBox.Show($"Error during Check-In: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                int enrollmentId = 118;
                string checkoutTime = DateTime.UtcNow.ToString("o"); // ISO 8601 format

                await _httpHandler.PostAttendanceRecordAsync(enrollmentId, checkoutTime: checkoutTime, id: HardcodedUuid);
                LogHelper.Log($"✅ Sent Check-Out for Enrollment ID: {enrollmentId} with UUID: {HardcodedUuid}");
                MessageBox.Show($"Check-Out sent for Enrollment ID: {enrollmentId}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Check-Out failed: {ex.Message}");
                MessageBox.Show($"Error during Check-Out: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        

        /////////////////////////////////////////////////////////////////////////////////
    }
}
