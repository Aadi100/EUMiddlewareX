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
        // Composite the entire form through a single OS-level back-buffer — eliminates all
        // per-control repaint flicker without needing per-panel double-buffer hacks.
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

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
        private Panel _syncOverlay;
        private System.Windows.Forms.Timer attendancePanelTimer;
        private CancellationTokenSource _deviceMonitorCts;
        private DateTime _lastPanelRefresh = DateTime.MinValue;
        private DateTime _dashboardStartTime = DateTime.Now;
        private readonly ToolTip _buttonTooltip = new ToolTip();
        public HttpHandler _httpHandler;

        private readonly object _loaderLock = new object();
        private int _loaderRefCount = 0;
        private string _pendingJobsSnapshot = null;

        private bool servicesInitialized = false;
        private smart_attendance_middleware.UI.LoaderPopupForm _loaderPopup;
        private BackupService _backupService;
        private bool _attendanceInitialSyncHandled = false;
        private FacePopupForm _facePopup;

        // Header status cards (declared in Designer.cs)
        private DateTime? _lastSyncTime = null;
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
            try
            {
                pbLogo.BackColor = Color.Transparent;
                using (var ico = new Icon(this.Icon, 48, 48))
                    pbLogo.Image = ico.ToBitmap();
            }
            catch { try { pbLogo.Image = this.Icon.ToBitmap(); } catch { } }
            InitializeHeaderCards();

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

            pnlLastEvent.Paint += (s, e) =>
            {
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 222, 235)))
                    e.Graphics.DrawLine(pen, 0, 0, pnlLastEvent.Width, 0);
            };

            InitializePendingJobsMiniList();
            _dashboardStartTime = DateTime.Now;
            RefreshDashboardWidgets();

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
            this.Enabled = false;
        }

        private void HideLoader()
        {
            _loaderPopup?.SafeClose();
            _loaderPopup = null;
            this.Enabled = true;
        }

        // aiChatLogger Code Started from Here *************

        private void AiChatLogger_OnAddMessage(string icon, string message)
        {
            if (lblLastEventText.InvokeRequired)
            {
                lblLastEventText.Invoke(new Action<string, string>(AiChatLogger_OnAddMessage), icon, message);
                return;
            }
            string text = string.IsNullOrEmpty(icon) ? message : $"{icon} {message}";
            lblLastEventText.Text = text;
        }

        private void AiChatLogger_OnClearMessages()
        {
            if (lblLastEventText.InvokeRequired)
            {
                lblLastEventText.Invoke(new Action(AiChatLogger_OnClearMessages));
                return;
            }
            lblLastEventText.Text = "Idle — waiting for activity";
        }

        // aiChatLogger Code Ended *************************

        // ── Dashboard widgets: alert bar / activity chart / system health / member overview ──
        // All data here comes from AppSession's already-cached in-memory lists (populated by
        // CacheManager) — no new DB queries are added by this refresh, in line with this
        // session's work on reducing unnecessary DB load.
        private void RefreshDashboardWidgets()
        {
            try
            {
                panel2.SuspendLayout();
                try
                {
                    RefreshAlertBar();
                    RefreshSystemHealth();
                    RefreshMemberOverview();
                    RefreshPendingJobsMiniList();
                }
                finally
                {
                    panel2.ResumeLayout(false);
                    panel2.Invalidate(true);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DashboardWidgets] Refresh error: {ex.Message}");
            }
        }

        private void RefreshAlertBar()
        {
            // Matches the same figure shown on the "Pending Jobs" stat card
            // (AppSession.SyncStatus.PendingBiometricJobs) — not the unrelated
            // AppSession.PendingJobs device-operation queue.
            int pendingCount = AppSession.SyncStatus?.PendingBiometricJobs ?? 0;
            if (pendingCount == 0)
            {
                pnlAlertBar.Visible = false;
                return;
            }

            lblAlertBar.Text = $"⚠ {pendingCount} pending biometric sync job(s) — please go take action.";
            pnlAlertBar.BackColor = Color.FromArgb(220, 53, 69);
            pnlAlertBar.Visible = true;
        }

        private DateTime _lastSqlMemoryCheck = DateTime.MinValue;
        private bool _sqlMemoryCheckInFlight = false;

        private void RefreshSystemHealth()
        {
            double memoryMb = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;

            var uptime = DateTime.Now - _dashboardStartTime;

            lblStatusAppMem.Text  = $"App memory {(int)memoryMb} MB";
            lblStatusUptime.Text  = uptime.TotalHours >= 1
                ? $"Uptime {(int)uptime.TotalHours}h {uptime.Minutes}m"
                : $"Uptime {uptime.Minutes}m {uptime.Seconds}s";
            lblStatusUser.Text    = $"Signed in: {AppSession.LoggedInUser ?? "–"}";
            lblStatusRole.Text    = $"Role: {AppSession.UserRole ?? "–"}";

            ReflowStatusBar();

            // SQL Server memory is checked far less often than everything else here (every 30s,
            // not every 5s) and off the UI thread, since it's a live DB query rather than
            // already-cached AppSession data — no reason to hit SQL Server more than necessary.
            if (_sqlMemoryCheckInFlight || (DateTime.Now - _lastSqlMemoryCheck).TotalSeconds < 30)
                return;

            _sqlMemoryCheckInFlight = true;
            _lastSqlMemoryCheck = DateTime.Now;

            Task.Run(() =>
            {
                bool applied = false;
                try
                {
                    int? sqlMemoryMb = _dbHelper?.GetSqlServerMemoryUsageMb();

                    if (!IsDisposed && IsHandleCreated)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            lblStatusSqlMem.Text = $"SQL Server {(sqlMemoryMb.HasValue ? sqlMemoryMb.Value + " MB" : "N/A")}";
                        }));
                        applied = true;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[DashboardWidgets] SQL memory check failed: {ex.Message}");
                }
                finally
                {
                    // Window not ready yet (or the check failed): allow the very next refresh to retry.
                    if (!applied) _lastSqlMemoryCheck = DateTime.MinValue;
                    _sqlMemoryCheckInFlight = false;
                }
            });
        }

        private void ApplyAlertBarRegion()
        {
            int w = pnlAlertBar.Width, h = pnlAlertBar.Height;
            if (w <= 0 || h <= 0) return;
            const int r = 8;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0,       0,       r * 2, r * 2, 180, 90);
            path.AddArc(w-r*2,   0,       r * 2, r * 2, 270, 90);
            path.AddArc(w-r*2,   h-r*2,   r * 2, r * 2,   0, 90);
            path.AddArc(0,       h-r*2,   r * 2, r * 2,  90, 90);
            path.CloseFigure();
            pnlAlertBar.Region = new System.Drawing.Region(path);
        }

        private void ReflowStatusBar()
        {
            const int gap = 8;
            // Chain the right-side labels starting after lblSep1 which follows lblStatusLastSync.
            // lblSep0/lblStatusLastSync are already reflowed in OnDeviceConnectionChanged;
            // we pick up from lblSep1 onward which contains the metrics that change every tick.
            int x = lblSep1.Right + gap;

            lblStatusAppMem.Left = x;
            x = lblStatusAppMem.Right + gap;
            lblSep2.Left = x;
            x = lblSep2.Right + gap;

            lblStatusSqlMem.Left = x;
            x = lblStatusSqlMem.Right + gap;
            lblSep3.Left = x;
            x = lblSep3.Right + gap;

            lblStatusUptime.Left = x;
            x = lblStatusUptime.Right + gap;
            lblSep4.Left = x;
            x = lblSep4.Right + gap;

            lblStatusUser.Left = x;
            x = lblStatusUser.Right + gap;
            lblSep5.Left = x;
            x = lblSep5.Right + gap;

            lblStatusRole.Left = x;
        }

        private void RefreshMemberOverview()
        {
            var members = AppSession.AllMembers ?? new List<Member>();
            int activeCount = members.Count(m => string.Equals(m.Status, "active", StringComparison.OrdinalIgnoreCase));

            var now = DateTime.Now;
            int expiringSoon = members.Count(m =>
                !string.IsNullOrEmpty(m.PackageExpiry)
                && DateTime.TryParse(m.PackageExpiry, out var expiry)
                && expiry >= now && expiry <= now.AddDays(7));

            _statActive?.SetValue(activeCount);
            _statExpiring?.SetValue(expiringSoon);
            _statStaff?.SetValue(AppSession.AllStaff?.Count ?? 0);
        }

        private void InitializePendingJobsMiniList()
        {
            lstPendingJobsMini.Columns.Clear();
            lstPendingJobsMini.Columns.Add("Name", 180);
            lstPendingJobsMini.Columns.Add("Device", 140);
            lstPendingJobsMini.Columns.Add("Pending For", 150);
            lstPendingJobsMini.Columns.Add("Failure Reason", 100);

            lstPendingJobsMini.PillColumnIndex   = 2;
            lstPendingJobsMini.ButtonColumnIndex = 3;
            lstPendingJobsMini.ButtonColumnItemClicked += LstPendingJobsMini_ButtonClicked;
        }

        private void LstPendingJobsMini_ButtonClicked(object sender, int rowIndex)
        {
            try
            {
                if (rowIndex < 0 || rowIndex >= lstPendingJobsMini.Items.Count) return;
                var tag = lstPendingJobsMini.Items[rowIndex].Tag as object[];
                if (tag == null || tag.Length < 2) return;
                int enrollId  = (int)tag[0];
                string reason = (string)tag[1];
                if (string.IsNullOrWhiteSpace(reason) || reason == "-") return;

                string fullReason = reason;
                try
                {
                    if (enrollId > 0)
                    {
                        var log = _dbHelper.GetLatestLogForMember(enrollId);
                        if (log != null && !string.IsNullOrWhiteSpace((string)log.FailureReason))
                            fullReason = (string)log.FailureReason;
                    }
                }
                catch { }

                using (var dlg = new FailureReasonDetailForm(fullReason, (int?)null, enrollId > 0 ? enrollId : (int?)null, null, null, _dbHelper))
                    dlg.ShowDialog(this);
            }
            catch { }
        }

        // Same underlying list shown in Device Manager's pending jobs grid — sourced from
        // AppSession.PendingBiometricSyncLogs (kept current by CacheManager), just condensed
        // to the few most recent rows so it fits inside the Telemetry Cluster card.
        private void RefreshPendingJobsMiniList()
        {
            try
            {
                var unsyncedEntries = AppSession.PendingBiometricSyncLogs ?? new List<LogEntry>();

                // Skip redraw when the entry set hasn't changed
                string snapshot = string.Join("|", unsyncedEntries.Select(e =>
                    $"{e.EnrollmentID}:{e.MemberID}:{e.DeviceID}:{string.Join(",", (e.SyncStatus ?? new System.Collections.Generic.Dictionary<string, bool>()).Where(kv => !kv.Value).Select(kv => kv.Key))}"));
                if (snapshot == _pendingJobsSnapshot) return;
                _pendingJobsSnapshot = snapshot;

                var membersById = (AppSession.AllMembers ?? new List<Member>())
                    .GroupBy(m => m.MemberID)
                    .ToDictionary(g => g.Key, g => g.First());

                var devicesById = (AppSession.DeviceList ?? new List<Device>())
                    .GroupBy(d => d.DeviceId)
                    .ToDictionary(g => g.Key, g => g.First());

                var displayItems = new List<ListViewItem>();

                foreach (var entry in unsyncedEntries)
                {
                    string pendingFor = entry.SyncStatus == null
                        ? "-"
                        : string.Join(", ", entry.SyncStatus.Where(kv => !kv.Value).Select(kv => kv.Key));
                    if (string.IsNullOrEmpty(pendingFor)) continue;

                    string name = membersById.TryGetValue(entry.MemberID, out var m) && !string.IsNullOrWhiteSpace(m.Name)
                        ? m.Name : "-";

                    string deviceName = devicesById.TryGetValue(entry.DeviceID, out var d)
                        ? (!string.IsNullOrWhiteSpace(d.Location) ? d.Location
                           : !string.IsNullOrWhiteSpace(d.DeviceSerial) ? d.DeviceSerial
                           : $"Device {entry.DeviceID}")
                        : $"Device {entry.DeviceID}";

                    string failureReason = "-";
                    try
                    {
                        if (entry.EnrollmentID > 0)
                        {
                            var log = _dbHelper.GetLatestLogForMember(entry.EnrollmentID);
                            if (log != null && !string.IsNullOrWhiteSpace((string)log.FailureReason))
                                failureReason = (string)log.FailureReason;
                        }
                    }
                    catch { }

                    var item = new ListViewItem(name);
                    item.SubItems.Add(deviceName);
                    item.SubItems.Add(pendingFor);
                    item.SubItems.Add(failureReason != "-" ? "view" : "-");
                    item.Tag = new object[] { entry.EnrollmentID, failureReason };
                    displayItems.Add(item);
                }

                lstPendingJobsMini.BeginUpdate();
                lstPendingJobsMini.Items.Clear();
                lstPendingJobsMini.Items.AddRange(displayItems.Take(5).ToArray());
                lstPendingJobsMini.EndUpdate();

                bool isEmpty = lstPendingJobsMini.Items.Count == 0;
                lstPendingJobsMini.Visible = !isEmpty;
                pnlEmptyState.Visible = isEmpty;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DashboardWidgets] Pending jobs mini list refresh error: {ex.Message}");
            }
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

        private async void Dashboard_Load(object sender, EventArgs e)
        {
            PositionHeaderCards(); // panel4.Width is now correct

            ApplyAlertBarRegion();
            pnlAlertBar.Resize += (s, _) => ApplyAlertBarRegion();

            pictureBox2.Paint += (s, ev) => DrawAvatarIfNoImage((PictureBox)s, Color.FromArgb(59, 130, 246), ev.Graphics);
            pictureBox3.Paint += (s, ev) => DrawAvatarIfNoImage((PictureBox)s, Color.FromArgb(124, 58, 237), ev.Graphics);

            ReflowStatusBar();

            await UpdateAttendancePanelsAsync();

            this.KeyDown += Dashboard_KeyDown;

            // Sync button progress overlay
            _syncOverlay = new Panel
            {
                Bounds   = btn_sync.Bounds,
                Visible  = false,
                Cursor   = Cursors.Default,
            };
            _syncOverlay.Paint += SyncOverlay_Paint;
            cuiPanel5.Controls.Add(_syncOverlay);
            _syncOverlay.BringToFront();
        }

        private void Dashboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Shift) return;
            switch (e.KeyCode)
            {
                case Keys.D1: case Keys.NumPad1: cuiButton4_Click(null, EventArgs.Empty); break;
                case Keys.D2: case Keys.NumPad2: cuiButton5_Click(null, EventArgs.Empty); break;
                case Keys.D3: case Keys.NumPad3: btn_fingerprint_qa_Click(null, EventArgs.Empty); break;
                case Keys.D4: case Keys.NumPad4: btn_backup_qa_Click(null, EventArgs.Empty); break;
                case Keys.D5: case Keys.NumPad5: btn_device_mgr_qa_Click(null, EventArgs.Empty); break;
            }
            e.Handled = true;
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
                _lastSyncTime = DateTime.Now;
                UpdateLastSyncCard();

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




        private void OnDeviceConnectionChanged(int connected, int total)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnDeviceConnectionChanged(connected, total)));
                return;
            }

            try
            {
                _pillsPanel?.UpdateDevices(connected, total);
                int offline = total - connected;
                if (total == 0)
                {
                    lblDevStatus.Text      = "No devices registered";
                    lblDevStatus.ForeColor = Color.FromArgb(100, 108, 132);
                }
                else if (offline == 0)
                {
                    lblDevStatus.Text      = "All connected";
                    lblDevStatus.ForeColor = Color.FromArgb(34, 139, 34);
                }
                else if (connected == 0)
                {
                    lblDevStatus.Text      = "All offline";
                    lblDevStatus.ForeColor = Color.FromArgb(220, 53, 69);
                }
                else
                {
                    lblDevStatus.Text      = $"{offline} offline";
                    lblDevStatus.ForeColor = Color.FromArgb(200, 100, 0);
                }

                // Reflow labels that follow the pills so they don't overlap
                if (_pillsPanel != null)
                {
                    lblDevStatus.Left       = _pillsPanel.Right + 8;
                    lblSep0.Left            = lblDevStatus.Right + 6;
                    lblStatusLastSync.Left  = lblSep0.Right + 6;
                }

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
                await UpdateSyncStatusLabels(membersSynced, pendingBiometricJobs, missingFingerprints);

                LogHelper.Log("✅ Device sync status refresh completed successfully");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error refreshing device sync status: {ex.Message}");
                await UpdateSyncStatusLabels(0, 0, 0);
            }
        }



        /// <summary>
        /// Updates the UI labels with sync status information
        /// </summary>
        private async Task UpdateSyncStatusLabels(int membersSynced, int pendingJobs, int missingFingerprints)
        {
            try
            {
                // Ensure UI updates are performed on the UI thread
                if (InvokeRequired)
                {
                    Invoke(new Func<Task>(async () => await UpdateSyncStatusLabels(membersSynced, pendingJobs, missingFingerprints)));
                    return;
                }

                _statMembersSynced?.SetValue(membersSynced);
                _statPendingJobs?.SetValue(pendingJobs);
                _statMissingFp?.SetValue(missingFingerprints);

                // Enable/disable retry button based on pending jobs count and attempt count threshold
                if (btn_retry != null)
                {
                    // Check if any members/staff have attempt count >= 5
                    bool hasHighAttempts = await CheckForHighAttemptCounts();
                    bool hasJobs = pendingJobs > 0;
                    _retryPendingJobs = pendingJobs;

                    bool hasOfflineRetries = false;
                    try
                    {
                        var offlineService = ServiceManager.OfflineEnrollmentSubscriptionService;
                        if (offlineService != null)
                            hasOfflineRetries = await offlineService.HasPendingOfflineRetriesAsync();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Error checking pending offline retries: {ex.Message}");
                    }

                    if (hasHighAttempts && !hasOfflineRetries)
                    {
                        // Red color for blocked state
                        btn_retry.Enabled = false;
                        btn_retry.NormalColor = System.Drawing.Color.FromArgb(220, 53, 69);  // Red
                        btn_retry.HoverColor = System.Drawing.Color.FromArgb(200, 35, 51);
                        btn_retry.ForeColor = System.Drawing.Color.White;
                        _buttonTooltip.SetToolTip(btn_retry, "⚠️ All pending members/staff have reached 5+ failed attempts. Nothing left to retry right now.");
                        LogHelper.Log($"🔴 Retry button disabled - members/staff with high attempt counts detected");
                    }
                    else if (hasJobs || hasOfflineRetries)
                    {
                        // Orange color for normal retry
                        btn_retry.Enabled = true;
                        btn_retry.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
                        btn_retry.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(158)))), ((int)(((byte)(35)))));
                        btn_retry.ForeColor = System.Drawing.Color.White;
                        string tip = hasJobs ? $"Retry {pendingJobs} pending sync job(s)" : "Retry pending offline member/renewal sync";
                        if (hasJobs && hasOfflineRetries) tip += " + offline items";
                        if (hasHighAttempts) tip += " (members/staff at 5+ attempts stay paused)";
                        _buttonTooltip.SetToolTip(btn_retry, tip);
                        LogHelper.Log($"🟠 Retry button enabled - pending jobs: {pendingJobs}, offline retries: {hasOfflineRetries}");
                    }
                    else
                    {
                        // Gray color for no jobs
                        btn_retry.Enabled = false;
                        btn_retry.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                        btn_retry.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                        btn_retry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
                        _buttonTooltip.SetToolTip(btn_retry, "No pending jobs to retry");
                        LogHelper.Log($"⚪ Retry button disabled - no pending jobs");
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
                attendancePanelTimer.Tick += (s, args) => RefreshDashboardWidgets();
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

                // 3b. Catalog Sync (Packages/Trainers for offline enrollment & subscription)
                UpdateLoaderMessage("Sync: Fetching packages & trainers catalog...");
                await ServiceManager.StartOfflineEnrollmentSubscriptionServiceAsync();
                UpdateLoaderMessage("Sync: Catalog ready.");

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

            internet = isConnected;
            UpdateWorkStatus();
            UpdateInternetCard(isConnected);

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

            subscription = isConnected;
            UpdateWorkStatus();
            UpdateCloudCard(isConnected);

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


        private static string ShowPasswordDialog(IWin32Window owner, string title, string prompt)
        {
            return ModernMessageBox.ShowPasswordPrompt(owner, title, "", prompt);
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
                        ModernMessageBox.Show(this, "Authentication Failed", "Invalid password. Application closure canceled.", MsgType.Warning);
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





        private void btn_adddevice_Click(object sender, EventArgs e)
        {
            if (AppSession.UserRole == "user")
            {
                ModernMessageBox.Show(this, "Access Denied", "Access denied. Only admin or developer can add devices.", MsgType.Warning);
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
            originalButtonText = btn_sync.Text?.ToString() ?? "Connect";
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
                    ModernMessageBox.Show(this, "No Devices Found", "No devices found in the database. Please add devices before connecting.", MsgType.Warning);
                    return;
                }

                // Step 2: Get API key
                string apiKey = _dbHelper.GetSetting("APIKey");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    LogHelper.Log("❌ No API key found. Please configure it first.");
                    ModernMessageBox.Show(this, "Missing API Key", "API Key is missing. Please configure it in the settings.", MsgType.Warning);
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

                    if (pongReceived && _wsManager.IsSubscribed)
                    {
                        isAlreadyConnected = true;
                        LogHelper.Log("✅ Already connected to WebSocket (verified with pong response and channel subscription)");
                    }
                    else if (pongReceived)
                    {
                        LogHelper.Log("⚠️ Pong received but channel is not subscribed - re-subscribing");
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
                ModernMessageBox.Show(this, "Sync Error", "Failed to complete synchronization.\n" + ex.Message, MsgType.Error);
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
            countdownSeconds = 60;
            btn_sync.Enabled = false;
            btn_sync.Text = originalButtonText; // keep underlying text unchanged

            if (_syncOverlay != null)
            {
                _syncOverlay.Visible = true;
                _syncOverlay.BringToFront();
                _syncOverlay.Invalidate();
            }

            if (countdownTimer == null)
            {
                countdownTimer = new System.Windows.Forms.Timer();
                countdownTimer.Interval = 1000;
                countdownTimer.Tick += CountdownTimer_Tick;
            }
            countdownTimer.Start();
        }

        private void SyncOverlay_Paint(object sender, PaintEventArgs e)
        {
            var g   = e.Graphics;
            var rc  = (sender as Panel).ClientRectangle;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var green    = Color.FromArgb(34, 197, 94);
            var greenDk  = Color.FromArgb(22, 163, 74);
            int radius   = rc.Height / 2;

            // Pill background
            using (var path = RoundedRect(rc, radius))
            using (var br = new System.Drawing.SolidBrush(green))
                g.FillPath(br, path);

            // Progress bar (white, semi-transparent) at bottom 5px
            float progress = countdownSeconds > 0 ? (60f - countdownSeconds) / 60f : 1f;
            int   barH     = 5;
            int   barW     = (int)(rc.Width * progress);
            var   barRc    = new Rectangle(0, rc.Height - barH, barW, barH);
            using (var br = new System.Drawing.SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                g.FillRectangle(br, barRc);

            // Clip pill shape so bar corners are rounded too
            using (var path = RoundedRect(rc, radius))
                g.SetClip(path);

            using (var br2 = new System.Drawing.SolidBrush(Color.FromArgb(100, 255, 255, 255)))
                g.FillRectangle(br2, barRc);

            g.ResetClip();

            // Text
            string txt = $"Sync again in {countdownSeconds} s";
            using (var f = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var br = new System.Drawing.SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(txt, f, br, rc, sf);
            }
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int rad)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            path.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            path.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            countdownSeconds--;

            _syncOverlay?.Invalidate();

            if (countdownSeconds <= 0)
            {
                countdownTimer.Stop();
                btn_sync.Enabled = true;
                if (_syncOverlay != null) _syncOverlay.Visible = false;
                btn_sync.Text = originalButtonText;
            }
        }

        /// <summary>
        /// Returns true only when EVERY pending member/staff has attempt count >= 5
        /// (i.e. nobody left who can actually be retried). If even one is below 5, returns false.
        /// </summary>
        private async Task<bool> CheckForHighAttemptCounts()
        {
            try
            {
                if (_dbHelper == null) return false;

                bool anyRetryable = false;

                var members = _dbHelper.GetMembers();
                if (members != null)
                {
                    foreach (var member in members)
                    {
                        if (member == null || !member.EnrollmentID.HasValue || member.EnrollmentID.Value <= 0) continue;
                        var logEntry = _dbHelper.GetLatestLogForMember(member.EnrollmentID.Value);
                        if (logEntry == null) continue;
                        int attempts = (int?)logEntry.AttemptCount ?? 0;
                        if (attempts > 0 && attempts < 5)
                        {
                            // At least one member is retryable — button should be enabled
                            anyRetryable = true;
                            break;
                        }
                    }
                }

                if (anyRetryable) return false;

                var staffs = _dbHelper.GetStaffs();
                if (staffs != null)
                {
                    foreach (var staff in staffs)
                    {
                        if (staff == null || !staff.EnrollmentID.HasValue || staff.EnrollmentID.Value <= 0) continue;
                        var logEntry = _dbHelper.GetLatestStaffLogForEnrollment(staff.EnrollmentID.Value);
                        if (logEntry == null) continue;
                        int attempts = (int?)logEntry.AttemptCount ?? 0;
                        if (attempts > 0 && attempts < 5)
                        {
                            anyRetryable = true;
                            break;
                        }
                    }
                }

                if (anyRetryable) return false;

                // Only return true (block button) if we found nobody retryable but someone IS at 5+
                var membersHigh = members?.Any(m =>
                {
                    if (m == null || !m.EnrollmentID.HasValue || m.EnrollmentID.Value <= 0) return false;
                    var log = _dbHelper.GetLatestLogForMember(m.EnrollmentID.Value);
                    return log != null && (int?)log.AttemptCount >= 5;
                }) ?? false;

                var staffsHigh = staffs?.Any(s =>
                {
                    if (s == null || !s.EnrollmentID.HasValue || s.EnrollmentID.Value <= 0) return false;
                    var log = _dbHelper.GetLatestStaffLogForEnrollment(s.EnrollmentID.Value);
                    return log != null && (int?)log.AttemptCount >= 5;
                }) ?? false;

                return membersHigh || staffsHigh;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error checking attempt counts: {ex.Message}");
                return false;
            }
        }

        private int _retryPendingJobs = 0;

        private async void cuiButton5_Click(object sender, EventArgs e)
        {
            try
            {
                bool hasJobs = _retryPendingJobs > 0;
                bool hasOfflineRetries = false;

                var offlineService = ServiceManager.OfflineEnrollmentSubscriptionService;
                if (offlineService != null)
                    hasOfflineRetries = await offlineService.HasPendingOfflineRetriesAsync();

                // Run only what is actually pending: offline retries, pending jobs, or both.
                if (hasOfflineRetries)
                {
                    LogHelper.Log("🔄 Manual retry: offline member device push and renewal sync...");
                    await offlineService.PushOfflineEntriesToDevicesAsync();
                    await offlineService.SyncPendingRenewalsWithQuickRetryAsync();

                    if (!hasJobs)
                    {
                        LogHelper.Log("✅ Manual retry completed (offline retries only)");
                        return;
                    }
                }

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
                    lbl_time_in.Text       = $"{checkIn.Time:HH:mm:ss}";
                    lbl_enroll_no_in.Text  = $"Enroll ID {checkIn.EnrollmentId}";
                    await LoadMemberImageAsync(pictureBox2, checkIn.ImagePath, checkIn.MemberId);
                    pictureBox2.Invalidate();
                }
                else
                {
                    lbl_name_in.Text      = "No recent check-ins";
                    lbl_time_in.Text      = "--";
                    lbl_enroll_no_in.Text = "Enroll ID: --";
                    pictureBox2.Image     = null;
                    pictureBox2.Invalidate();
                }

                if (AppSession.LastCheckOut != null)
                {
                    var checkOut = AppSession.LastCheckOut;
                    lbl_name_out.Text      = checkOut.MemberName;
                    lbl_time_out.Text      = $"{checkOut.Time:HH:mm:ss}";
                    lbl_enroll_no_out.Text = $"Enroll ID {checkOut.EnrollmentId}";
                    await LoadMemberImageAsync(pictureBox3, checkOut.ImagePath, checkOut.MemberId);
                    pictureBox3.Invalidate();
                }
                else
                {
                    lbl_name_out.Text      = "No recent check-outs";
                    lbl_time_out.Text      = "--";
                    lbl_enroll_no_out.Text = "Enroll ID: --";
                    pictureBox3.Image      = null;
                    pictureBox3.Invalidate();
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
                    using (var textBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                    {
                        SizeF textSize = g.MeasureString(idText, new Font("Segoe UI", 8.5f, FontStyle.Bold));
                        g.DrawString(idText, new Font("Segoe UI", 8.5f, FontStyle.Bold), textBrush,
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
            // Reserved for future panel header painting
        }

        private void DrawAvatarIfNoImage(PictureBox pb, Color avatarColor, System.Drawing.Graphics g)
        {
            if (pb.Image != null) return;
            var rc = pb.ClientRectangle;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var bg = new System.Drawing.SolidBrush(avatarColor))
            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                const int r = 10;
                path.AddArc(rc.X,              rc.Y,               r * 2, r * 2, 180, 90);
                path.AddArc(rc.Right - r * 2,  rc.Y,               r * 2, r * 2, 270, 90);
                path.AddArc(rc.Right - r * 2,  rc.Bottom - r * 2,  r * 2, r * 2,   0, 90);
                path.AddArc(rc.X,              rc.Bottom - r * 2,  r * 2, r * 2,  90, 90);
                path.CloseFigure();
                g.FillPath(bg, path);
            }
            string nameText = "";
            var card = pb.Parent;
            if (card != null)
            {
                foreach (Control c in card.Controls)
                {
                    if (c.Name == "lbl_name_in" || c.Name == "lbl_name_out")
                    { nameText = c.Text ?? ""; break; }
                }
            }
            string initials = GetInitials(nameText);
            if (!string.IsNullOrEmpty(initials))
            {
                using (var f = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold))
                using (var b = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                {
                    var sf = new System.Drawing.StringFormat
                    {
                        Alignment = System.Drawing.StringAlignment.Center,
                        LineAlignment = System.Drawing.StringAlignment.Center
                    };
                    g.DrawString(initials, f, b, rc, sf);
                }
            }
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        private void btn_fingerprint_qa_Click(object sender, EventArgs e)
        {
            var fingerManagement = new FingerManagement();
            fingerManagement.ShowDialog();
        }

        private void btn_backup_qa_Click(object sender, EventArgs e)
        {
            var membershipManagement = new MembershipManagement();
            membershipManagement.ShowDialog();
        }

        private void btn_controls_test_Click(object sender, EventArgs e)
        {
            try
            {
                new ControlsTestForm().ShowDialog(this);
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Controls Test Error", ex.ToString(), MsgType.Error);
            }
        }

        private void btn_device_mgr_qa_Click(object sender, EventArgs e)
        {
            try
            {
                var devicemanager = new DeviceManager();
                devicemanager.ShowDialog();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Error", "Failed to open Device Manager:\n\n" + ex.Message + "\n\n" + (ex.InnerException?.Message ?? ""), MsgType.Error);
            }
        }

        // ── Header status cards ──────────────────────────────────────────────────

        private void InitializeHeaderCards()
        {
            // Cards are initialized in Designer.cs; just wire resize handler here
            panel4.Resize += (s, e2) => PositionHeaderCards();
        }

        private void PositionHeaderCards()
        {
            const int cardW = 130, cardH = 52, gap = 8;
            int top   = (panel4.Height - cardH) / 2;
            int right = panel4.Width - 20;

            if (_hdrLastSyncCard  != null) { _hdrLastSyncCard.SetBounds(right - cardW, top, cardW, cardH); right -= cardW + gap; }
            if (_hdrCloudCard     != null) { _hdrCloudCard.SetBounds(right - cardW,    top, cardW, cardH); right -= cardW + gap; }
            if (_hdrInternetCard  != null) { _hdrInternetCard.SetBounds(right - cardW, top, cardW, cardH); }
        }

        private void UpdateInternetCard(bool connected)
            => _hdrInternetCard?.SetStatus(connected, connected ? "Connected" : "Disconnected");

        private void UpdateCloudCard(bool connected)
            => _hdrCloudCard?.SetStatus(connected, connected ? "Connected" : "Disconnected");

        private void UpdateLastSyncCard()
        {
            string text;
            if (_lastSyncTime == null)
            {
                text = "Never";
            }
            else
            {
                var dt     = _lastSyncTime.Value;
                string day = dt.Date == DateTime.Today             ? "Today"
                           : dt.Date == DateTime.Today.AddDays(-1) ? "Yesterday"
                           : dt.ToString("MMM d");
                text = $"{day} {dt:h:mm tt}";
            }
            // Update status bar label
            if (lblStatusLastSync != null)
                lblStatusLastSync.Text = $"Last sync: {text}";
        }

        private void _hdrLastSyncCard_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://eu.fitnessmarvel.com/");
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
                if (dash.toastNotificationControl2 != null && !dash.toastNotificationControl2.IsDisposed)
                    dash.toastNotificationControl2.UpdateToast(title, message, icon, displayMs);
            }));
        }

        public static void GlobalShowToastTyped(string title, string message, UI.ToastType type, int displayMs = 5000)
        {
            var dash = Application.OpenForms.Cast<Form>().OfType<Dashboard>().FirstOrDefault();
            if (dash == null || dash.IsDisposed) return;

            dash.BeginInvoke(new Action(() =>
            {
                if (dash.toastNotificationControl2 != null && !dash.toastNotificationControl2.IsDisposed)
                    dash.toastNotificationControl2.UpdateToastTyped(title, message, type, displayMs);
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
                                            OnDeviceConnectionChanged(onlineCount, allDevices.Count);
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
                ModernMessageBox.Show(this, "Error", $"Error: {ex.Message}", MsgType.Error);
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

                    this.BeginInvoke(new Action(() =>
                    {
                        OnDeviceConnectionChanged(onlineCount, allDevices.Count);
                    }));
                }

                LogHelper.Log($"[Dashboard] Device panel refreshed. Devices tracked: {allDevices.Count}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[ERROR] Failed to refresh device panel values: {ex.Message}");
            }
        }


        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlStatusBar_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblStatusUser_Click(object sender, EventArgs e)
        {

        }

        private void lblSep4_Click(object sender, EventArgs e)
        {

        }

        private void lblStatusRole_Click(object sender, EventArgs e)
        {

        }

        private void lblSep5_Click(object sender, EventArgs e)
        {

        }

        private void lblStatusUptime_Click(object sender, EventArgs e)
        {

        }




        /////////////////////////////////////////////////////////////////////////////////
    }
}


