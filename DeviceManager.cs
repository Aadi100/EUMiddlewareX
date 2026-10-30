using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using CLV = smart_attendance_middleware.Controls.CardListView;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace smart_attendance_middleware
{
    public partial class DeviceManager : Form
    {
        private DatabaseHelper _dbHelper;
        private WebSocketManager _wsManager;
        private Processor _processor;
        private DeviceFunctions _deviceFunctions = new DeviceFunctions();
        private BackupService _backupService;
        private CacheManager _cacheManager;

        private readonly List<(int Id, string Name, string ImagePath)> _enrolledStaffBuffer= new List<(int Id, string Name, string ImagePath)>();

        private bool _selectedIsStaff = false;
        private int _selectedDeviceId = 0;      // Devices.DeviceId
        private int _selectedStaffDeviceId = 0; // StaffDevices.StaffDeviceId
        private Device _selectedDevice = null;  // Currently selected device card

        // Cached full member item list — rebuilt only when source data changes
        private List<ListViewItem> _cachedMemberItems;

        // Guard: prevents TextChanged handlers from firing during form initialization
        private bool _isInitialized = false;

        // Filter controls declared in Designer.cs
        private Controls.RoundedComboBox cmbMemberStatus = new Controls.RoundedComboBox();

        private bool _loadingUserData;

        // rcmb_* and setup fields now declared in Designer.cs

        // Font hierarchy — Title / Body / Small
        private static readonly Font FontTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
        private static readonly Font FontBody   = new Font("Segoe UI",  9.5F, FontStyle.Bold);
        private static readonly Font FontSmall  = new Font("Segoe UI",  8.5F, FontStyle.Regular);

        // Edit-mode colors
        private static readonly Color EditableColor  = Color.White;
        private static readonly Color ReadOnlyColor  = Color.FromArgb(240, 240, 240);




        public DeviceManager()
        {
            InitializeComponent();

            // Restore emoji icons lost due to Designer file encoding
            tile_device.Icon = "🖥";
            tile_user.Icon   = "👤";
            tile_staff.Icon  = "👥";
            tile_member.Icon = "🪪";
            tile_setup.Icon  = "⚙";

            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
            _backupService = new BackupService(_dbHelper);

            // Initialize CacheManager with 30-second refresh interval
            _cacheManager = new CacheManager(_dbHelper, refreshIntervalSeconds: 30);
            _cacheManager.CacheUpdated += OnCacheUpdated;

            InitializeWebSocketManager();
            SetupListViewColumns();

            _processor = new Processor(_dbHelper, _wsManager, _deviceFunctions, _cacheManager);
            _wsManager.LiveUpdate += WsManager_LiveUpdate;
            this.Shown += DeviceManager_Shown;

            LoadSetupPanelData();
        }

        private void cmbPendingDevice_SelectedIndexChanged(object sender, EventArgs e) => RefreshMemberPendingJobsList();
        private void cmbPendingFor_SelectedIndexChanged(object sender, EventArgs e) => RefreshMemberPendingJobsList();
        private void cmbStaffPendingDevice_SelectedIndexChanged(object sender, EventArgs e) => RefreshStaffPendingJobsList();
        private void cmbStaffPendingFor_SelectedIndexChanged(object sender, EventArgs e) => RefreshStaffPendingJobsList();
        private void cmbDoorVisitor_SelectedIndexChanged(object sender, EventArgs e) => LoadFilteredManualDoorLogs();
        private void cmbStaffStatus_SelectedIndexChanged(object sender, EventArgs e) => LoadStaffsIntoListView(rtb_staff?.Text?.Trim());
        private void cmbStaffSync_SelectedIndexChanged(object sender, EventArgs e) => LoadStaffsIntoListView(rtb_staff?.Text?.Trim());
        private void chkStaffFailure_CheckedChanged(object sender, EventArgs e) => LoadStaffsIntoListView(rtb_staff?.Text?.Trim());

        private void tile_finger_Click(object sender, EventArgs e)
        {
            try { new FingerManagement().ShowDialog(this); } catch { }
        }

        // ── CardListView substitution helpers ────────────────────────────────────────────

        private void SetupListViewColumns()
        {
            clv_devices.SetColumns(
                new[] { "MACHINE NO", "ATT. TYPE", "DEVICE IP", "PORT", "CONN TYPE", "STATUS", "IS STAFF", "ACTIONS" },
                new[] {  0,            90,           180,         300,    360,          430,      502,         572 });

            clv_localUsers.SetColumns(
                new[] { "USER ID", "USERNAME", "ROLE", "CREATED",  "ACTIONS" },
                new[] {  0,         50,         200,    320,         500 });

            clv_members.SetColumns(
                new[] { "MEMBER", "ENROLLMENT", "STATUS", "BIOMETRIC", "SYNC", "EXPIRY", "FAILURE", "ACTIONS" },
                new[] {  0,        220,           300,      372,         478,    543,       638,       712 });

            clv_memberAttendance.SetColumns(
                new[] { "MEMBER", "LOG ID", "CHECK-IN", "CHECK-OUT", "TIME ON SITE" },
                new[] {  0,        295,       390,        520,          660 });

            clv_manualDoor.SetColumns(
                new[] { "GATE", "PERSON", "REASON", "OPENED AT" },
                new[] {  0,      220,      540,       710 });

            clv_pendingJobs.SetColumns(
                new[] { "MEMBER ID", "NAME", "ENROLLMENT", "DEVICE", "PENDING FOR", "SINCE" },
                new[] {  0,           90,     240,           330,      470,           655 });

            clv_staffPendingJobs.SetColumns(
                new[] { "STAFF ID", "NAME", "ENROLLMENT", "DEVICE", "PENDING FOR", "SINCE" },
                new[] {  0,          90,     240,           330,      470,           655 });

            clv_staffs.SetColumns(
                new[] { "STAFF", "STATUS", "BIOMETRIC", "ENROLLMENT", "SYNC", "FAILURE", "ACTIONS" },
                new[] {  0,       275,      360,          490,          575,    650,        725 });

            clv_staffAttendance.SetColumns(
                new[] { "STAFF", "LOG ID", "CHECK-IN", "CHECK-OUT", "TIME ON SITE" },
                new[] {  0,       295,       390,        520,          660 });
        }


        // ── Setup panel helpers ──────────────────────────────────────────────────

        private const string RequireGateEntryFormSettingKey = "RequireGateEntryForm";
        private const string RequireGatePasswordSettingKey  = "RequireGatePassword";

        private void ApplyCardRoundedRegions()
        {
            foreach (var card in new System.Windows.Forms.Panel[] { card_api, card_sync, card_backup, card_maint, card_gate })
            {
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                int r = 10, d = r * 2;
                var rc = new Rectangle(0, 0, card.Width, card.Height);
                path.AddArc(rc.X, rc.Y, d, d, 180, 90);
                path.AddArc(rc.Right - d, rc.Y, d, d, 270, 90);
                path.AddArc(rc.Right - d, rc.Bottom - d, d, d, 0, 90);
                path.AddArc(rc.X, rc.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                card.Region = new Region(path);
            }
        }

        private void LoadSetupPanelData()
        {
            pnl_setup.BackColor  = Color.FromArgb(243, 244, 246);
            ApplyCardRoundedRegions();
            try { rtb_APIkey.Text = _dbHelper.GetSetting("APIKey") ?? ""; } catch { }
            try { rtb_debounce.Text = _dbHelper.GetSetting("DebounceSetting") ?? ""; } catch { }

            rcmb_retention.Items.Clear();
            rcmb_retention.Items.AddRange(new object[] { "0", "7", "14", "30", "60", "90", "180", "365" });
            try
            {
                string saved = _dbHelper.GetSetting("BackupRetentionDays");
                int idx = saved != null ? rcmb_retention.Items.IndexOf(saved) : -1;
                rcmb_retention.SelectedIndex = idx >= 0 ? idx : 3;
            }
            catch { rcmb_retention.SelectedIndex = 3; }

            try
            {
                chk_logs.Checked     = (_dbHelper.GetSetting("BackupIncludeLogs")     ?? "true").Equals("true",  StringComparison.OrdinalIgnoreCase);
                chk_glogs.Checked    = (_dbHelper.GetSetting("BackupIncludeGLogs")    ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                chk_database.Checked = (_dbHelper.GetSetting("BackupIncludeDatabase") ?? "true").Equals("true",  StringComparison.OrdinalIgnoreCase);
            }
            catch { chk_logs.Checked = true; chk_database.Checked = true; }

            try { rtb_drivefolderid.Text = _dbHelper.GetSetting("GoogleDriveFolderId") ?? ""; } catch { }

            bool isConnected = !string.IsNullOrWhiteSpace(rtb_drivefolderid.Text);
            lbl_backupConnectionStatus.Text      = isConnected ? "● Connected" : "● Not connected";
            lbl_backupConnectionStatus.ForeColor = isConnected ? Color.FromArgb(5, 150, 105) : Color.FromArgb(217, 119, 6);
            lbl_backupConnectionStatus.BackColor = isConnected ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 243, 199);

            try
            {
                string formSetting = _dbHelper.GetSetting(RequireGateEntryFormSettingKey);
                chk_requireGateEntryForm.Checked = string.IsNullOrWhiteSpace(formSetting) || formSetting.Equals("true", StringComparison.OrdinalIgnoreCase);
                string passwordSetting = _dbHelper.GetSetting(RequireGatePasswordSettingKey);
                chk_requireGatePassword.Checked = string.IsNullOrWhiteSpace(passwordSetting) || passwordSetting.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch { chk_requireGateEntryForm.Checked = true; chk_requireGatePassword.Checked = true; }
        }

        // rtb_* fields now declared in Designer.cs
        private List<Member> _allMembersCache = new List<Member>();

        // clv_* fields now declared in Designer.cs

        private void InitializeWebSocketManager()
        {
            _wsManager = new WebSocketManager(_dbHelper);
            // _wsManager.LiveUpdate += WsManager_LiveUpdate; // Delegate to Processor instead
        }
        private void WsManager_LiveUpdate(object sender, LiveUpdateEventArgs e)
        {
            if (e.Data is JObject jdata)
            {
                _processor.HandleLiveUserUpdate(e.EventType, jdata);
                //AppendLog($"☁️ Live event handled: {e.EventType}");
            }
        }


        //*************************************Setup CardListView************************************//
        //*************************************Setup CardListView************************************//
        private void SetupDeviceList()
        {
            var devices      = AppSession.DeviceList      ?? new List<Device>();
            var staffDevices = AppSession.StaffDeviceList ?? new List<Device>();

            // Build combined row list
            var rows = new List<(string id, string machineNo, string attType, string ip, string port, string connType, string status, string isStaff, object tag)>();
            foreach (var d in devices)
                rows.Add((d.DeviceId.ToString(), d.MachineNo.ToString(), d.AttendanceType ? "Check-in" : "Check-out",
                    d.DeviceIP ?? "-", d.Port.ToString(), d.ConnectionType ?? "-", d.IsActive ? "Active" : "Inactive", "No", d));
            foreach (var sd in staffDevices)
                rows.Add((sd.StaffDeviceId.ToString(), sd.MachineNo.ToString(),
                    string.IsNullOrWhiteSpace(sd.StaffAttendanceType) ? "-" : sd.StaffAttendanceType,
                    sd.DeviceIP ?? "-", sd.Port.ToString(), sd.ConnectionType ?? "-", sd.IsActive ? "Active" : "Inactive", "Yes", sd));

            if (chkDeviceIsStaff?.Checked == true)
                rows = rows.Where(r => r.isStaff == "Yes").ToList();

            var accent = Color.FromArgb(59, 130, 246);
            var navy   = Color.FromArgb(30, 38, 90);
            var muted  = Color.FromArgb(130, 138, 165);
            var green  = Color.FromArgb(16, 185, 129);
            var red    = Color.FromArgb(239, 68, 68);

            clv_devices.SetRowFactory(rows.Count, idx =>
            {
                var r = rows[idx];
                bool isActive = string.Equals(r.status, "Active", StringComparison.OrdinalIgnoreCase);
                Color sBg = isActive ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 226, 226);
                Color sFg = isActive ? green : red;
                bool isStaff = r.isStaff == "Yes";
                object capturedTag = r.tag;
                var editBg   = Color.FromArgb(30, 38, 90);
                var removeBg = Color.FromArgb(239, 68, 68);
                var formatBg = Color.FromArgb(107, 114, 128);
                return new CLV.CardBuilder()
                    .Text(r.machineNo, 0,   15, 88,  22, CLV.F9Bold, navy)
                    .Text(r.attType,   90,  15, 88,  22, CLV.F9,     muted)
                    .Text(r.ip,        180, 15, 118, 22, CLV.F9,     navy)
                    .Text(r.port,      300, 15, 56,  22, CLV.F9,     muted)
                    .Text(r.connType,  360, 15, 68,  22, CLV.F9,     muted)
                    .Pill(r.status, sBg, sFg, 430, 14, 68, 22)
                    .Pill(isStaff ? "Staff" : "Member",
                          isStaff ? Color.FromArgb(237, 233, 254) : Color.FromArgb(219, 234, 254),
                          isStaff ? Color.FromArgb(109, 40, 217) : accent,
                          502, 14, 62, 22)
                    .Button("Edit",   editBg,   Color.White, 572, 13, 46, 26, () => {
                        var dev = capturedTag as Device;
                        if (dev == null) return;
                        _selectedDevice        = dev;
                        _selectedIsStaff       = dev.StaffDeviceId > 0;
                        _selectedDeviceId      = dev.DeviceId;
                        _selectedStaffDeviceId = dev.StaffDeviceId;
                        btnEdit_Click(null, EventArgs.Empty);
                    })
                    .Button("Remove", removeBg, Color.White, 622, 13, 56, 26, () => {
                        var dev = capturedTag as Device;
                        if (dev == null) return;
                        _selectedDevice        = dev;
                        _selectedIsStaff       = dev.StaffDeviceId > 0;
                        _selectedDeviceId      = dev.DeviceId;
                        _selectedStaffDeviceId = dev.StaffDeviceId;
                        btn_remove_device_Click(null, EventArgs.Empty);
                    })
                    .Button("Format", formatBg, Color.White, 682, 13, 54, 26, () => {
                        var dev = capturedTag as Device;
                        if (dev == null) return;
                        _selectedDevice        = dev;
                        _selectedIsStaff       = dev.StaffDeviceId > 0;
                        _selectedDeviceId      = dev.DeviceId;
                        _selectedStaffDeviceId = dev.StaffDeviceId;
                        btn_format_Click(null, EventArgs.Empty);
                    })
                    .CardClick(() => {
                        var dev = capturedTag as Device;
                        if (dev == null) return;
                        _selectedDevice        = dev;
                        _selectedIsStaff       = dev.StaffDeviceId > 0;
                        _selectedDeviceId      = dev.DeviceId;
                        _selectedStaffDeviceId = dev.StaffDeviceId;
                    })
                    .Build(750, 52);
            });

        }

        //*************************************Event Listview Setup************************************//



        //*************************************Local user's data ************************************//

        //Abdul Hadi Code
        public void LoadLocalUserData()
        {
            if (InvokeRequired) { Invoke((Action)LoadLocalUserData); return; }
            if (_loadingUserData) return;
            _loadingUserData = true;
            try { LoadLocalUserDataCore(); } finally { _loadingUserData = false; }
        }

        private void LoadLocalUserDataCore()
        {
            List<LocalUser> users = AppSession.AllUsers ?? new List<LocalUser>();
            var visibleUsers = users.Where(u => !string.Equals(u.Role, "developer", StringComparison.OrdinalIgnoreCase)).ToList();

            // Refresh role filter items
            if (cmbUserRole != null)
            {
                string prevRole = cmbUserRole.SelectedItem?.ToString() ?? "All Roles";
                cmbUserRole.Items.Clear();
                cmbUserRole.Items.Add("All Roles");
                foreach (var role in visibleUsers.Select(u => u.Role).Distinct().OrderBy(r => r))
                    if (!string.IsNullOrWhiteSpace(role)) cmbUserRole.Items.Add(role);
                cmbUserRole.SelectedIndex = cmbUserRole.Items.Contains(prevRole) ? cmbUserRole.Items.IndexOf(prevRole) : 0;
            }

            string roleSel = cmbUserRole?.SelectedItem?.ToString() ?? "All Roles";
            if (roleSel != "All Roles")
                visibleUsers = visibleUsers.Where(u => string.Equals(u.Role, roleSel, StringComparison.OrdinalIgnoreCase)).ToList();

            var navy   = Color.FromArgb(30, 38, 90);
            var muted  = Color.FromArgb(130, 138, 165);
            var accent = Color.FromArgb(59, 130, 246);
            var red    = Color.FromArgb(239, 68, 68);
            var green  = Color.FromArgb(16, 185, 129);


            clv_localUsers.SetRowFactory(visibleUsers.Count, idx =>
            {
                var user = visibleUsers[idx];
                string initial = string.IsNullOrWhiteSpace(user.UserName) ? "?" : user.UserName.Substring(0, 1).ToUpperInvariant();
                var card = CLV.MakeCard(900, 52);
                card.Controls.Add(CLV.MakeAvatarLabel(initial, accent, 8, 8, 36));
                card.Controls.Add(new Label { Text = user.LocalUserID.ToString(), Location = new Point(50,  17), Width = 145, Height = 18, Font = CLV.F9,     ForeColor = navy,  BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = user.UserName ?? "-",        Location = new Point(200, 17), Width = 115, Height = 18, Font = CLV.F9Bold, ForeColor = navy,  BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = user.Role ?? "-",            Location = new Point(320, 17), Width = 155, Height = 18, Font = CLV.F9,     ForeColor = muted, BackColor = Color.Transparent });

                int capturedId   = user.LocalUserID;
                string capturedName = user.UserName ?? "";
                string capturedRole = user.Role ?? "";

                var btnEdit = CLV.MakeActionButton("Edit", accent, Color.White, 500, 13, 60, 26);
                btnEdit.Click += (s, ev) =>
                {
                    selectedUserId   = capturedId;
                    selectedUsername = capturedName;
                    selectedRole     = capturedRole;
                    btn_editUser_Click_1(s, ev);
                };
                card.Controls.Add(btnEdit);

                var btnPwd = CLV.MakeActionButton("Password", Color.FromArgb(245, 158, 11), Color.White, 566, 13, 72, 26);
                btnPwd.Click += (s, ev) =>
                {
                    selectedUserId   = capturedId;
                    selectedUsername = capturedName;
                    selectedRole     = capturedRole;
                    btn_change_password_Click(s, ev);
                };
                card.Controls.Add(btnPwd);

                var btnDel = CLV.MakeActionButton("Delete", Color.White, red, 644, 13, 60, 26);
                btnDel.Click += (s, ev) =>
                {
                    selectedUserId   = capturedId;
                    selectedUsername = capturedName;
                    selectedRole     = capturedRole;
                    btn_deleteUser_Click(s, ev);
                };
                card.Controls.Add(btnDel);

                return card;
            });
        }

        private async void DeviceManager_Load(object sender, EventArgs e)
        {
            try
            {
                // Start the global loader
                Dashboard.GlobalStartLoader("Loading Device Manager...");

                SetupDeviceList(); // creates columns and sample rows
                LoadSetupPanelData();

                // Show welcome screen — no panel selected on open
                HideAllPanels();
                SetActiveButton(null);
                SetHeader("Smart Access Manager", "Manage your biometric devices, users, staff, members, and system settings — all in one place.");
                pnl_welcome_header.BringToFront();
                pnl_welcome.Visible = true;

                pnl_welcome.Visible = true;
                pnl_welcome.BringToFront();


                ClearDeviceFields(); // Clear device fields on load
                await UpdateSessionAsync(); // Update session with device data
                InvalidateMemberCache();  // force rebuild on next member list load
                InitializeMemberList();
                InitializeMemberListForStats();       // dynamic name suggestions
                InitializeMembersListView();  // members list view setup

                InitializePendingJobsListView();

                // Wire events
                rb_flt_members.Click += flt_members_Click;
                rtb_memb_name.TextChanged += rtb_memb_name_TextChanged;

                InitializeStaffSuggestions();
                InitializeStaffsListView();
                rb_stafffilter.Click += btn_stafffilter_Click;
                rtb_staff.TextChanged += rtb_staff_TextChanged;

                InitializeStaffAttendanceSuggestions();
                InitializeStaffAttendanceListView();
                rb_filterattendencestaff.Click += btn_filterattendencestaff_Click;
                rtb_staffname.TextChanged += rtb_staffname_TextChanged;
                date_staffattendence.ValueChanged += date_staffattendence_ValueChanged;

                InitializeStaffPendingJobsListView();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DeviceManager] Error during load: {ex.Message}");
            }
            finally
            {
                _isInitialized = true;

                // Stop the global loader
                Dashboard.GlobalStopLoader();
            }
        }

        private void tab_attendence_SelectedIndexChanged(object sender, EventArgs e) { }
        private void tab_staff_SelectedIndexChanged(object sender, EventArgs e) { }

        // ── Filter event handlers ───────────────────────────────────────────
        private void chkDeviceIsStaff_CheckedChanged(object sender, EventArgs e) { SetupDeviceList(); }
        private void cmbUserRole_SelectedIndexChanged(object sender, EventArgs e) { LoadLocalUserData(); }
        private void cmbMemberSync_SelectedIndexChanged(object sender, EventArgs e) { LoadMembersIntoListView(rtb_memb_name?.Text?.Trim()); }
        private void chkMemberFailure_CheckedChanged(object sender, EventArgs e) { LoadMembersIntoListView(rtb_memb_name?.Text?.Trim()); }

        // ── Setup save handlers ─────────────────────────────────────────────
        private void btn_sync_save_Click(object sender, EventArgs e)
        {
            btn_savedebounce_Click(sender, e);
            if (int.TryParse(rcmb_retention.SelectedItem?.ToString(), out int days))
                num_retention.Value = Math.Max(num_retention.Minimum, Math.Min(num_retention.Maximum, days));
            btn_save_retentiondays_Click(sender, e);
        }
        private void btn_savebackup_Click(object sender, EventArgs e)
        {
            btn_save_drivefolderid_Click(sender, e);
            if (!string.IsNullOrWhiteSpace(rtb_drivecredentials.Text))
                btn_save_drivecredentials_Click(sender, e);
            btn_save_backuptype_Click(sender, e);
            bool nowConn = !string.IsNullOrWhiteSpace(_dbHelper.GetSetting("GoogleDriveFolderId"));
            lbl_backupConnectionStatus.Text      = nowConn ? "● Connected" : "● Not connected";
            lbl_backupConnectionStatus.ForeColor = nowConn ? Color.FromArgb(5, 150, 105) : Color.FromArgb(217, 119, 6);
            lbl_backupConnectionStatus.BackColor = nowConn ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 243, 199);
        }
        private void btn_gate_save_Click(object sender, EventArgs e)
        {
            try
            {
                _dbHelper.SaveSetting(RequireGateEntryFormSettingKey, chk_requireGateEntryForm.Checked ? "true" : "false");
                _dbHelper.SaveSetting(RequireGatePasswordSettingKey,  chk_requireGatePassword.Checked  ? "true" : "false");
                LogHelper.Log($"[Setup][Gate] Saved: EntryForm={chk_requireGateEntryForm.Checked}, Password={chk_requireGatePassword.Checked}");
                ModernMessageBox.Show(this, "Success", "Gate access settings saved.", MsgType.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Setup][Gate][ERROR] {ex.Message}");
                ModernMessageBox.Show(this, "Error", "Failed to save gate access settings.", MsgType.Error);
            }
        }

        private void DeviceManager_closing(object sender, FormClosingEventArgs e)
        {
            // Stop cache manager auto-refresh
            _cacheManager?.StopAutoRefresh();

            // CardListViews are disposed automatically as form controls
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
                    // Refresh all lists
                    RefreshAllLists();
                    LogHelper.Log("[DeviceManager] Full cache refresh - UI updated");
                }
                else if (e.UpdateTypes != null && e.UpdateTypes.Count > 0)
                {
                    if (e.UpdateTypes.Contains(CacheUpdateType.Members))
                    {
                        RefreshMembersList();
                    }

                    if (e.UpdateTypes.Contains(CacheUpdateType.Staff))
                    {
                        RefreshStaffList();
                    }

                    if (e.UpdateTypes.Contains(CacheUpdateType.Users))
                    {
                        RefreshUsersList();
                    }

                    if (e.UpdateTypes.Contains(CacheUpdateType.PendingBiometricSync))
                    {
                        RefreshMemberPendingJobsList();
                    }

                    if (e.UpdateTypes.Contains(CacheUpdateType.StaffPendingBiometricSync))
                    {
                        RefreshStaffPendingJobsList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DeviceManager] Error in OnCacheUpdated: {ex.Message}");
            }
        }

        private void RefreshAllLists()
        {
            RefreshMembersList();
            RefreshStaffList();
            RefreshUsersList();
            RefreshMemberPendingJobsList();
            RefreshStaffPendingJobsList();
        }

        private void RefreshMembersList()
        {
            // Refresh members list if currently visible
            if (pnl_staffsManagment.Visible && clv_members.Visible)
            {
                LoadMembersIntoListView(rtb_memb_name.Text?.Trim());
            }
        }

        private void RefreshStaffList()
        {
            // Refresh staff list if currently visible
            if (pnl_staffsManagment.Visible && clv_staffs.Visible)
            {
                LoadStaffsIntoListView(rtb_staff.Text?.Trim());
            }
        }

        private void RefreshUsersList()
        {
            // Refresh users list if currently visible
            if (pnl_UserManagement.Visible)
            {
                LoadLocalUserData();
            }
        }

        public async Task UpdateSessionAsync()
        {
            try
            {
                var devices      = await Task.Run(() => _dbHelper.LoadDevices()      ?? new List<Device>());
                var staffDevices = await Task.Run(() => _dbHelper.LoadStaffDevices() ?? new List<Device>());
                var users        = await Task.Run(() => _dbHelper.LoadUsers()        ?? new List<LocalUser>());

                AppSession.DeviceList      = devices;
                AppSession.StaffDeviceList = staffDevices;
                AppSession.AllUsers        = users;
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Error", "Error loading devices: " + ex.Message, MsgType.Error);
            }
        }


        //////////////////////////////////NAVIGATION HELPERS///////////////////////////////
        private static readonly Color SidebarBg   = Color.FromArgb(18, 23, 49);
        private static readonly Color SidebarHover = Color.FromArgb(30, 60, 100);

        private void HideAllPanels()
        {
            pnl_deviceManager.Visible   = false;
            pnl_UserManagement.Visible  = false;
            pnl_staffsManagment.Visible = false;
            pnl_attendence.Visible      = false;
            pnl_setup.Visible           = false;
            pnl_welcome.Visible         = false;
            pnl_topnav.Visible          = false;
        }

        private void SetActiveButton(Label active)
        {
            foreach (var btn in new[] { btn_device_management, btn_userManagement,
                                        btn_staffmanagment, btn_attendencemanagment, btn_setupmanage })
            {
                if (btn == null) continue;
                btn.BackColor = SidebarBg;
                btn.ForeColor = Color.White;
            }
            if (active != null)
            {
                active.BackColor = Color.WhiteSmoke;
                active.ForeColor = Color.Black;
            }
        }

        private void SidebarButton_MouseHover(object sender, EventArgs e)
        {
            if (sender is Label btn && btn.BackColor != Color.WhiteSmoke)
            {
                btn.BackColor = SidebarHover;
                btn.ForeColor = Color.White;
                btn.Cursor    = Cursors.Hand;
            }
        }

        private void SidebarButton_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Label btn && btn.BackColor != Color.WhiteSmoke)
            {
                btn.BackColor = SidebarBg;
                btn.ForeColor = Color.White;
                btn.Cursor    = Cursors.Default;
            }
        }

        //////////////////////////////////WELCOME PANEL///////////////////////////////
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pnl_welcome.Visible) return;
            HideAllPanels();
            pnl_topnav.Visible = false;
            SetActiveButton(null);
            SetActiveNavButton(null);
            SetHeader("Smart Access Manager", "Manage your biometric devices, users, staff, members, and system settings — all in one place.");
            pnl_welcome.Visible = true;
            pnl_welcome.BringToFront();
            pnl_welcome_header.BringToFront();
        }

        private void SetHeader(string title, string subtitle)
        {
            lbl_welcome_title.Text = title;
            lbl_welcome_sub.Text   = subtitle;
        }

        //////////////////////////////////SIDE BUTTONS///////////////////////////////
        private void btn_device_management_Click(object sender, EventArgs e)
        {
            if (AppSession.UserRole == "user")
            {
                ModernMessageBox.Show(this, "Access Denied", "Access denied. Only admin or developer can open Device Management.", MsgType.Warning);
                return;
            }

            if (pnl_deviceManager.Visible && pnl_deviceManager.Location == Point.Empty) return;
            HideAllPanels();
            SetHeader("Device Management", "Add, edit and monitor biometric devices.");
            pnl_topnav.Visible = true;
            pnl_topnav.BringToFront();
            pnl_welcome_header.BringToFront();
            SetActiveButton(btn_device_management);
            SetActiveNavButton(nav_device);
            pnl_deviceManager.Location = new Point(0, 0);
            pnl_deviceManager.Visible  = true;
            ClearDeviceFields();
            _selectedDevice = null;
        }

        private void btn_userManagement_Click(object sender, EventArgs e)
        {
            if (pnl_UserManagement.Visible && pnl_UserManagement.Location == Point.Empty) return;
            HideAllPanels();
            SetHeader("User Management", "Manage system users and their permissions.");
            pnl_topnav.Visible = true;
            pnl_topnav.BringToFront();
            pnl_welcome_header.BringToFront();
            SetActiveButton(btn_userManagement);
            SetActiveNavButton(nav_user);
            pnl_UserManagement.Location = new Point(0, 0);
            pnl_UserManagement.Visible  = true;
            LoadLocalUserData();
        }

        private async void btn_attendance_Click(object sender, EventArgs e)
        {
            if (pnl_staffsManagment.Visible && pnl_staffsManagment.Location == Point.Empty) return;
            HideAllPanels();
            SetHeader("Staff Management", "View and manage staff attendance.");
            pnl_topnav.Visible = true;
            pnl_topnav.BringToFront();
            pnl_welcome_header.BringToFront();
            SetActiveButton(btn_staffmanagment);
            SetActiveNavButton(nav_staff);
            pnl_staffsManagment.Location = new Point(0, 0);
            pnl_staffsManagment.Visible  = true;
            Dashboard.GlobalStartLoader("Loading Staff Management...");
            await Task.Run(() => { }); // yield so loader renders before data load
            Dashboard.GlobalStopLoader();
        }

        private async void btn_attendencemanagment_Click(object sender, EventArgs e)
        {
            if (pnl_attendence.Visible && pnl_attendence.Location == Point.Empty) return;
            HideAllPanels();
            SetHeader("Member Management", "Track member records and attendance logs.");
            pnl_topnav.Visible = true;
            pnl_topnav.BringToFront();
            pnl_welcome_header.BringToFront();
            SetActiveButton(btn_attendencemanagment);
            SetActiveNavButton(nav_member);
            pnl_attendence.Location = new Point(0, 0);
            pnl_attendence.Visible  = true;
            Dashboard.GlobalStartLoader("Loading Member Attendance...");
            await Task.Run(() => { }); // yield so loader renders before data load
            LoadMemberAttendanceLogs();
            LoadManualDoorLogs();
            Dashboard.GlobalStopLoader();
        }

        private void btn_setupmanage_Click(object sender, EventArgs e)
        {
            if (AppSession.UserRole == "user")
            {
                ModernMessageBox.Show(this, "Access Denied", "Access denied. Only admin or developer can open Setup Management.", MsgType.Warning);
                return;
            }

            if (pnl_setup.Visible && pnl_setup.Location == Point.Empty) return;
            HideAllPanels();
            SetHeader("Setup Management", "Configure API keys, backups, and system settings.");
            pnl_topnav.Visible = true;
            pnl_topnav.BringToFront();
            pnl_welcome_header.BringToFront();
            SetActiveButton(btn_setupmanage);
            SetActiveNavButton(nav_setup);
            pnl_setup.Location = new Point(0, 0);
            pnl_setup.Visible  = true;
        }

        /////////////////////////////////////////////////////////////////////////////////

        private void btn_userManagement_MouseHover(object sender, EventArgs e)  => SidebarButton_MouseHover(sender, e);
        private void btn_userManagement_MouseLeave(object sender, EventArgs e)  => SidebarButton_MouseLeave(sender, e);
        private void btn_attendance_MouseHover(object sender, EventArgs e)      => SidebarButton_MouseHover(sender, e);
        private void btn_attendance_MouseLeave(object sender, EventArgs e)      => SidebarButton_MouseLeave(sender, e);
        private void btn_device_management_MouseHover(object sender, EventArgs e) => SidebarButton_MouseHover(sender, e);
        private void btn_device_management_MouseLeave(object sender, EventArgs e) => SidebarButton_MouseLeave(sender, e);

        private void SetActiveNavButton(Label active)
        {
            foreach (var btn in new[] { nav_device, nav_user, nav_staff, nav_member, nav_setup })
            {
                if (btn == null) continue;
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.White;
            }
            if (active != null)
            {
                active.BackColor = Color.WhiteSmoke;
                active.ForeColor = Color.FromArgb(18, 23, 49);
            }
        }

        private void NavBtn_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Label btn && btn.BackColor != Color.WhiteSmoke)
                btn.BackColor = SidebarHover;
        }

        private void NavBtn_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Label btn && btn.BackColor != Color.WhiteSmoke)
                btn.BackColor = Color.Transparent;
        }


        // Device selection is handled via per-row click handlers in SetupDeviceList()
        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e) { }


        private void button2_Click(object sender, EventArgs e) { }

        //********************************* Toggle Device Details Fields EditMode ***************************************

        private void ToggleDeviceFieldsEditMode(bool enable) { }


        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedDevice == null)
            {
                ModernMessageBox.Show(this, "No Selection", "Please select a device to edit.", MsgType.Warning);
                return;
            }
            var editForm = new EditDevice(_selectedDevice, _selectedIsStaff);
            editForm.FormClosed += (s, args) => { SetupDeviceList(); _selectedDevice = null; };
            editForm.ShowDialog(this);
        }



        private async Task<bool> IsValidIpAsync(string ipAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ipAddress, 2000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private void LoadSelectedStaffDeviceDetails(int staffDeviceId)
        {
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                ModernMessageBox.Show(this, "Error", "No user is currently logged in. Cannot proceed.", MsgType.Error);
                return;
            }

            string enteredPassword = ModernMessageBox.ShowPasswordPrompt(this,
                $"Confirm Update",
                $"Signed in as {currentUser}",
                "Enter your password or backup code to update the device:");

            if (enteredPassword != null)
            {

                if (_dbHelper.VerifyLoginUser(currentUser, enteredPassword))
                {
                    string deviceIp = (_selectedDevice?.DeviceIP ?? "").Trim();

                    if (!await IsValidIpAsync(deviceIp))
                    {
                        ModernMessageBox.Show(this, "Invalid IP", "Invalid IP address. Please check the IP and try again.", MsgType.Warning);
                        return;
                    }

                    int machineNoParsed = _selectedDevice?.MachineNo ?? 0;

                    // For normal devices, guard against IP/Machine conflicts (port uniqueness removed per helper doc)
                    if (!_selectedIsStaff)
                    {
                        string deviceExistsMessage = _dbHelper.IDCheckDeviceExists(deviceIp, machineNoParsed, _selectedDeviceId);
                        if (!string.IsNullOrEmpty(deviceExistsMessage))
                        {
                            ModernMessageBox.Show(this, "Device Conflict", deviceExistsMessage, MsgType.Warning);
                            return;
                        }
                    }
                    // Staff devices may have different constraints; skipping duplicate guard unless implemented

                    ToggleDeviceFieldsEditMode(false);

                    // Build common model from fields
                    var deviceModel = new Device
                    {
                        Department     = _selectedDevice?.Department     ?? "",
                        DeviceSerial   = _selectedDevice?.DeviceSerial   ?? "",
                        DeviceIP       = deviceIp,
                        Port           = _selectedDevice?.Port           ?? 0,
                        TimeZone       = _selectedDevice?.TimeZone       ?? "",
                        Location       = _selectedDevice?.Location       ?? "",
                        Password       = _selectedDevice?.Password       ?? "",
                        ConnectionType = _selectedDevice?.ConnectionType ?? "",
                        MachineNo      = machineNoParsed,
                        IsActive     = _selectedDevice?.IsActive ?? false,
                        LastSyncTime = _selectedDevice?.LastSyncTime ?? "",
                        FaceCapacity    = _selectedDevice?.FaceCapacity    ?? 0,
                        RFIDCapacity    = _selectedDevice?.RFIDCapacity    ?? 0,
                        FingerCapacity  = _selectedDevice?.FingerCapacity  ?? 0
                    };

                    // Attendance type mapping
                    var attendanceText = _selectedIsStaff
                        ? (_selectedDevice?.StaffAttendanceType ?? string.Empty)
                        : (_selectedDevice?.AttendanceType == true ? "Check-in" : "Check-out");

                    try
                    {
                        if (_selectedIsStaff)
                        {
                            // Staff update
                            deviceModel.StaffDeviceId = _selectedStaffDeviceId;
                            deviceModel.StaffAttendanceType = attendanceText; // supports "Check-in/Check-out"

                            _dbHelper.UpdateStaffDevice(deviceModel);
                        }
                        else
                        {
                            // Normal device update (boolean only)
                            deviceModel.DeviceId = _selectedDeviceId;
                            deviceModel.AttendanceType = string.Equals(attendanceText, "Check-in", StringComparison.OrdinalIgnoreCase);

                            _dbHelper.UpdateDevice(deviceModel);
                        }

                        // Notify AI Chat Logger about the device update
                        aiChatLogger.AddDeviceMessage($"Device '{deviceModel.Location}' configuration updated successfully!");
                        aiChatLogger.AddSuccessMessage("All changes saved. Your device is ready to go!");

                        SetupDeviceList();
                        ClearDeviceFields();
                        _selectedDevice = null;
                        await UpdateSessionAsync(); // Update session with device data
                InvalidateMemberCache();

                        // Notify dashboard to refresh
                        NotifyDashboardRefresh();

                        ModernMessageBox.Show(this, "Success", "Device updated successfully.", MsgType.Success);
                    }
                    catch (InvalidOperationException ex)
                    {
                        aiChatLogger.AddErrorMessage("Device update failed due to duplicate entry. Please check your settings.");
                        ModernMessageBox.Show(this, "Update Failed - Duplicate Entry", ex.Message, MsgType.Error);
                    }
                    catch (Exception ex)
                    {
                        ModernMessageBox.Show(this, "Error", $"An unexpected error occurred:\n{ex.Message}", MsgType.Error);
                    }
                }
                else
                {
                    ModernMessageBox.Show(this, "Error", "Authentication failed. Device update was cancelled.", MsgType.Error);
                }
            }
            else
            {
                ModernMessageBox.Show(this, "Info", "Device update cancelled.", MsgType.Info);
                _selectedDevice = null;
                ClearDeviceFields();
            }
        }




        private void btnCancle_Click(object sender, EventArgs e)
        {
            _selectedDevice = null;
            ClearDeviceFields(); // Clear the fields
            // Revert UI to non-editable mode
            ToggleDeviceFieldsEditMode(false);




            // Optionally reset field values (if you saved originals before editing)
            // ResetDeviceFormValues(originalDeviceData);
        }
        //**************************************************************



        private void LoadSelectedDeviceDetails(int deviceId)
        {
            // Fetch device details from the database based on the selected deviceId
            var device = _dbHelper.LoadDevices().FirstOrDefault(d => d.DeviceId == deviceId);

        }

        public void ClearDeviceFields()
        {
        }


        private void btn_add_device_Click(object sender, EventArgs e)
        {
            ClearDeviceFields();
            _selectedDevice = null;
            AddNewDevice adddevice = new AddNewDevice();
            adddevice.ShowDialog();
            SetupDeviceList(); // Refresh the device list after adding a new device
        }

        private async void btn_remove_device_Click(object sender, EventArgs e)
        {
            if (_selectedDevice == null)
            {
                ModernMessageBox.Show(this, "No Selection", "Please select a device to remove.", MsgType.Warning);
                return;
            }

            int deviceId = _selectedIsStaff ? _selectedStaffDeviceId : _selectedDeviceId;
            if (deviceId <= 0)
            {
                ModernMessageBox.Show(this, "Error", "Invalid Device ID. Please enter a valid number.", MsgType.Error);
                return;
            }

            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                ModernMessageBox.Show(this, "Error", "No user is currently logged in. Cannot proceed.", MsgType.Error);
                return;
            }

            string enteredPassword = ModernMessageBox.ShowPasswordPrompt(this,
                "Confirm Deletion",
                $"Signed in as {currentUser}",
                $"Enter your password or backup code to delete Device ID {deviceId}:",
                "Delete");

            if (enteredPassword != null)
            {

                if (!await IsValidIpAsync((_selectedDevice?.DeviceIP ?? "").Trim()))
                {
                    ModernMessageBox.Show(this, "Ping Failed", "The device is not reachable. Please check the IP address and try again.", MsgType.Warning);
                    return;
                }

                if (_dbHelper.VerifyLoginUser(currentUser, enteredPassword))
                {
                    if (_selectedIsStaff)
                    {
                        aiChatLogger.AddDeviceMessage($"Preparing to remove staff device from {_selectedDevice?.Location ?? ""}...");
                        aiChatLogger.AddInfoMessage("Clearing all enrollment data from the staff device first...");

                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId(_selectedStaffDeviceId);
                        _dbHelper.DeleteStaffDevice(_selectedStaffDeviceId);
                    }
                    else
                    {
                        aiChatLogger.AddDeviceMessage($"Preparing to remove device from {_selectedDevice?.Location ?? ""}...");
                        aiChatLogger.AddInfoMessage("Clearing all enrollment data from the device first...");

                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByDeviceId(_selectedDeviceId);
                        _dbHelper.DeleteDevice(_selectedDeviceId);
                    }

                    SetupDeviceList(); // Refresh the device list after deletion
                    ClearDeviceFields(); // Clear the fields after deletion
                    _selectedDevice = null;
                    await UpdateSessionAsync(); // Update session with device data
                InvalidateMemberCache();

                    // Notify dashboard to refresh
                    NotifyDashboardRefresh();

                    // Add AI messages after successful removal
                    aiChatLogger.AddSuccessMessage($"Device from {_selectedDevice?.Location ?? ""} has been successfully removed!");
                    aiChatLogger.AddInfoMessage("Don't worry, I'm still here to help with your remaining devices.");
                    aiChatLogger.AddDeviceMessage("Device network updated. All enrollment data has been cleared.");

                    ModernMessageBox.Show(this, "Success", "Device deleted successfully.", MsgType.Success);
                }
                else
                {
                    ModernMessageBox.Show(this, "Error", "Authentication failed. Device was not deleted.", MsgType.Error);
                }
            }
            else
            {
                ClearDeviceFields(); // Clear the fields if the user cancels
                _selectedDevice = null;
                    ModernMessageBox.Show(this, "Info", "Device deletion cancelled.", MsgType.Info);
                    }
                }


                private async void btnSaveAPI_Click_1(object sender, EventArgs e)
        {
            string apiKey = rtb_APIkey.Text.Trim();

            // Step 1: Basic validation
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ModernMessageBox.Show(this, "Validation Error", "Please enter a valid API Key.", MsgType.Warning);
                return;
            }

            // ✅ Validate format: Must contain colon (GYMID:SECRETKEY)
            if (!apiKey.Contains(":") || apiKey.Split(':').Length != 2)
            {
                ModernMessageBox.Show(this, "Format Error", "API Key format is invalid. Expected format: gymId:secretKey", MsgType.Warning);
                return;
            }

            try
            {
                // Split on the first colon only
                int sepIndex = apiKey.IndexOf(':');
                if (sepIndex <= 0 || sepIndex == apiKey.Length - 1)
                {
                    ModernMessageBox.Show(this, "Invalid Format", "Invalid API key format. Expected '<gymId>:<jwt>'.", MsgType.Warning);
                    return;
                }

                string gymId = apiKey.Substring(0, sepIndex).Trim();
                string jwt = apiKey.Substring(sepIndex + 1).Trim();

                if (string.IsNullOrWhiteSpace(gymId) || string.IsNullOrWhiteSpace(jwt))
                {
                    ModernMessageBox.Show(this, "Validation Error", "Gym ID or token is empty after split.", MsgType.Warning);
                    return;
                }

                // Persist all parts
                _dbHelper.SaveSetting("APIKey", apiKey);
                _dbHelper.SaveSetting("GymId", gymId);
                _dbHelper.SaveSetting("GWTToken", jwt);

                LogHelper.Log($"[API Key Saved] GymId='{gymId}', JWT length={jwt.Length}.");
                ModernMessageBox.Show(this, "Success", "API key saved.\nGym ID and GWT token stored successfully.", MsgType.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[btnSaveAPI_Click_1][ERROR] {ex.Message}");
                ModernMessageBox.Show(this, "Error", "Failed to save API key. See logs for details.", MsgType.Error);
            }
        }


        private void btn_addUser_Click(object sender, EventArgs e)
        {
            if (AppSession.UserRole == "user")
            {
                ModernMessageBox.Show(this, "Access Denied", "You do not have permission to add users.", MsgType.Warning);
                return;
            }
            Thread newThread = new Thread(Newuser);
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }

        //Abdul Hadi Code
        private void Newuser()
        {
            AddNewUser newuser = new AddNewUser();
            Application.Run(newuser);
        }

        private int? selectedUserId = null;
        private string selectedUsername = null;
        private string selectedRole = null;



        //Abdul Hadi Code
        //Event to select an item in the list and pass its id
        // User selection is handled via per-row click handlers in LoadUsersIntoListView()
        private void list_LocalUser_SelectedIndexChanged(object sender, EventArgs e) { }


        //Abdul Hadi Code
        //Event to delete the selected user
        private void btn_deleteUser_Click(object sender, EventArgs e)
        {
            if (!selectedUserId.HasValue)
            {
                ModernMessageBox.Show(this, "No User Selected", "Please select a user to delete.", MsgType.Info);
                return;
            }

            // Nobody can delete themselves
            if (selectedUserId.Value == AppSession.LoggedInUserID)
            {
                ModernMessageBox.Show(this, "Access Denied", "You cannot delete your own account.", MsgType.Warning);
                return;
            }

            string currentRole = AppSession.UserRole;

            // user cannot delete anyone
            if (currentRole == "user")
            {
                ModernMessageBox.Show(this, "Access Denied", "You do not have permission to delete users.", MsgType.Warning);
                return;
            }

            // admin cannot delete developer or another admin
            if (currentRole == "admin" && (selectedRole == "developer" || selectedRole == "admin"))
            {
                ModernMessageBox.Show(this, "Access Denied", "You do not have permission to delete this user.", MsgType.Warning);
                return;
            }

            if (ModernMessageBox.ShowConfirm(this, "Confirm Delete", "Are you sure you want to delete this user?", "Yes", "No", MsgType.Warning))
            {
                aiChatLogger.AddUserMessage($"Removing user {selectedUsername} from the system...");
                _dbHelper.DeleteUserById(selectedUserId.Value);
                LoadLocalUserData();
                selectedUserId = null;
                aiChatLogger.AddSuccessMessage($"User {selectedUsername} has been successfully removed from the system.");
                aiChatLogger.AddInfoMessage("User management completed. The team roster has been updated.");
            }
        }

        //Abdul Hadi Code
        //Event to change the password of the selected user
        private void btn_change_password_Click(object sender, EventArgs e)
        {
            if (!selectedUserId.HasValue)
            {
                ModernMessageBox.Show(this, "No Selection", "Please select a user to change password.", MsgType.Info);
                return;
            }

            string currentRole = AppSession.UserRole;

            // user can only change their own password
            if (currentRole == "user" && selectedUserId.Value != AppSession.LoggedInUserID)
            {
                ModernMessageBox.Show(this, "Access Denied", "You can only change your own password.", MsgType.Warning);
                return;
            }

            // admin cannot change password of developer or another admin (unless own)
            if (currentRole == "admin" && selectedUserId.Value != AppSession.LoggedInUserID
                && (selectedRole == "developer" || selectedRole == "admin"))
            {
                ModernMessageBox.Show(this, "Access Denied", "You do not have permission to change this user's password.", MsgType.Warning);
                return;
            }

            if (selectedUserId.HasValue)
            {
                var pwResult = ModernMessageBox.ShowChangePasswordPrompt(this);
                if (pwResult.HasValue)
                {
                    string newPassword     = pwResult.Value.newPw;
                    string confirmPassword = pwResult.Value.confirmPw;

                    if (newPassword.Length < 8)
                    {
                        ModernMessageBox.Show(this, "Validation", "Password must be at least 8 characters long.", MsgType.Warning);
                        return;
                    }

                    if (newPassword != confirmPassword)
                    {
                        ModernMessageBox.Show(this, "Validation", "Passwords do not match. Please try again.", MsgType.Warning);
                        return;
                    }

                    if (ModernMessageBox.ShowConfirm(this, "Confirm", "Are you sure you want to change the password?", "Yes", "No", MsgType.Warning))
                    {
                        _dbHelper.UpdateUserPassword(selectedUserId.Value, newPassword);
                        ModernMessageBox.Show(this, "Success", "Password changed successfully.", MsgType.Success);
                    }
                }
            }
            else
            {
                ModernMessageBox.Show(this, "No Selection", "Please select a user to change password.", MsgType.Info);
            }
        }



        //Abdul Hadi Code
        //Event to edit the selected user
        private void btn_editUser_Click_1(object sender, EventArgs e)
        {
            if (!selectedUserId.HasValue || string.IsNullOrEmpty(selectedUsername) || string.IsNullOrEmpty(selectedRole))
            {
                ModernMessageBox.Show(this, "No Selection", "Please select a user to edit.", MsgType.Info);
                return;
            }

            string currentRole = AppSession.UserRole;

            // user can only edit themselves
            if (currentRole == "user" && selectedUserId.Value != AppSession.LoggedInUserID)
            {
                ModernMessageBox.Show(this, "Access Denied", "You can only edit your own account.", MsgType.Warning);
                return;
            }

            // admin cannot edit developer or another admin (unless own)
            if (currentRole == "admin" && selectedUserId.Value != AppSession.LoggedInUserID
                && (selectedRole == "developer" || selectedRole == "admin"))
            {
                ModernMessageBox.Show(this, "Access Denied", "You do not have permission to edit this user.", MsgType.Warning);
                return;
            }

            using (EditUser editForm = new EditUser(selectedUserId.Value, selectedUsername, selectedRole))
            {
                editForm.ShowDialog();
            }
            LoadLocalUserData();
        }

        private async void CheckAllDeviceConnections()
        {
            var allDevices = (AppSession.DeviceList ?? new List<Device>())
                .Concat(AppSession.StaffDeviceList ?? new List<Device>())
                .ToList();

            await Task.Run(async () =>
            {
                foreach (var d in allDevices)
                {
                    try
                    {
                        string serial = await DeviceService.GetSerial(d.DeviceIP, d.Port, d.MachineNo, "0");
                    }
                    catch { /* Handle error, optionally update UI */ }
                }
            });

            // Refresh after all checks
            this.Invoke(new Action(() => SetupDeviceList()));
        }

        private void NotifyDashboardRefresh()
        {
            try
            {
                // Find the Dashboard form and refresh its device status
                foreach (Form form in Application.OpenForms)
                {
                    if (form is Dashboard dashboard)
                    {
                        dashboard.Invoke(new Action(async () => await dashboard.RefreshDevicePanelAsync()));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't interrupt the current operation
                LogHelper.Log($"Error notifying dashboard refresh: {ex.Message}");
            }
        }


        private async void DeviceManager_Shown(object sender, EventArgs e)
        {
            CheckAllDeviceConnections();

        }


        private async void btn_format_Click(object sender, EventArgs e)
        {
            // Require a selection
            if (_selectedDevice == null)
            {
                ModernMessageBox.Show(this, "No Device Selected", "Please select a device to format.", MsgType.Warning);
                return;
            }

            var selectedDevice = _selectedDevice;

            // Auth
            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                ModernMessageBox.Show(this, "Error", "No user is currently logged in. Cannot proceed.", MsgType.Error);
                return;
            }

            if (!ModernMessageBox.ShowConfirm(this, "Confirm Format",
                "WARNING: This will delete ALL user data (Fingerprints, Face, RFID, Passwords) from the terminal via Cloud WebSocket.\n\nThis action cannot be undone. Are you sure you want to continue?",
                "Yes, Format", "Cancel", MsgType.Warning)) return;

            // Authentication Prompt
            string formatPassword = ModernMessageBox.ShowPasswordPrompt(this,
                "Confirm Format",
                $"Signed in as {currentUser}",
                "Enter your password to format the device:",
                "Format Device");

            if (formatPassword == null) return;

            if (!_dbHelper.VerifyLoginUser(currentUser, formatPassword))
            {
                ModernMessageBox.Show(this, "Error", "Authentication failed. Device formatting was cancelled.", MsgType.Error);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                // 1. Resolve Serial Number (SN) for WebSocket Routing
                string sn = selectedDevice.DeviceSerial;
                if (string.IsNullOrEmpty(sn))
                {
                    sn = CommonUtils.GetDeviceSnByIp(selectedDevice.DeviceIP);
                }

                if (string.IsNullOrEmpty(sn))
                {
                    ModernMessageBox.Show(this, "Device Unreachable", "Device Serial Number not found. Ensure the device is registered.", MsgType.Warning);
                    return;
                }

                LogHelper.Log($"[Format] Initiating cloud format for device: {sn}");

                // 2. Execute Format Command via Cloud Protocol (Protocol 9: cleanuser)
                // Returns true if dispatched successfully to terminal
                bool success = await WebsocketServer.ExecuteServiceCommand(sn, "cleanuser", null);

                if (success)
                {
                    // 3. Clear local DB logs and enrollment metadata
                    bool isStaff = selectedDevice.StaffDeviceId > 0;

                    if (isStaff)
                    {
                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId(selectedDevice.StaffDeviceId);
                    }
                    else
                    {
                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByDeviceId(selectedDevice.DeviceId);
                    }

                    ModernMessageBox.Show(this, "Format Complete", "All user data has been cleared from terminal and database Successfully.", MsgType.Success);
                }
                else
                {
                    ModernMessageBox.Show(this, "Format Failed", "Failed to reach device via WebSocket. Please ensure the terminal is online.", MsgType.Error);
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Error", $"An error occurred while formatting the device: {ex.Message}", MsgType.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                SetupDeviceList();
                ClearDeviceFields();
                _selectedDevice = null;
                await UpdateSessionAsync();
                NotifyDashboardRefresh();
            }
        }

        /////////////////////////MEMBER MANAGMENT///////////////////////////////

        // Call this in DeviceManager_Load after InitializeMemberList()
        private void InitializeMembersListView()
        {
            LoadMembersIntoListView(null);
        }

        // Reuse your function as-is for rtb_memb_name dynamic suggestions:
        // Map rtb_memb_name to rtb_membername internally (if different control names exist)
        private void InitializeMemberListForStats()
        {
            // Load all members from AppSession
            List<Member> members = AppSession.AllMembers ?? new List<Member>();
            LogHelper.Log($"Initialized statistics suggestions with {members.Count} members from session.");

            // Configure the AutoCompleteSource and AutoCompleteMode
            rtb_memb_name.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            rtb_memb_name.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Create a StringCollection to store member names
            AutoCompleteStringCollection memberCollection = new AutoCompleteStringCollection();

            // Add each member's name to the collection
            foreach (var member in members)
            {
                if (!string.IsNullOrWhiteSpace(member.Name))
                {
                    memberCollection.Add(member.Name);
                }
            }

            // Assign the collection to the AutoCompleteCustomSource
            rtb_memb_name.AutoCompleteCustomSource = memberCollection;
        }

        // Button click handler for __flt_members__ to filter by name
        private void flt_members_Click(object sender, EventArgs e)
        {
            string nameFilter = rtb_memb_name.Text?.Trim();
            LoadMembersIntoListView(string.IsNullOrEmpty(nameFilter) ? null : nameFilter);
        }

        // Invalidate cached member items — call whenever AppSession.AllMembers changes
        public void InvalidateMemberCache() => _cachedMemberItems = null;

        // Builds ListViewItems for every member — called once and cached
        private List<ListViewItem> BuildAllMemberItems()
        {
            var members    = AppSession.AllMembers ?? new List<Member>();
            var allDevices = AppSession.DeviceList  ?? new List<Device>();
            int totalDevices = allDevices.Count;
            var result = new List<ListViewItem>(members.Count);

            foreach (var m in members)
            {
                string enrollmentText = m.EnrollmentID.HasValue && m.EnrollmentID.Value > 0
                    ? m.EnrollmentID.Value.ToString() : "-";

                string syncStatusText = "-";
                if (totalDevices > 0)
                {
                    int syncedCount = 0;
                    foreach (var d in allDevices)
                    {
                        if (m.SyncStatus != null && m.SyncStatus.TryGetValue(d.DeviceId, out bool isSynced) && isSynced)
                            syncedCount++;
                    }
                    syncStatusText = $"{syncedCount}/{totalDevices} devices";
                }

                var bioList = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(m.RFID)) bioList.Add("RFID");
                if (!string.IsNullOrWhiteSpace(m.Picture)) bioList.Add("Face");
                if (m.Fingerprint) bioList.Add("Fingerprint");
                string bioText = bioList.Count > 0 ? string.Join(", ", bioList) : "-";

                var item = new ListViewItem(m.MemberID.ToString());
                item.SubItems.Add(string.IsNullOrWhiteSpace(m.Name)        ? "-" : m.Name);
                item.SubItems.Add(enrollmentText);
                item.SubItems.Add(string.IsNullOrWhiteSpace(m.Status)      ? "-" : m.Status);
                item.SubItems.Add(bioText);
                item.SubItems.Add(syncStatusText);
                item.SubItems.Add(string.IsNullOrWhiteSpace(m.PackageExpiry) ? "-" : m.PackageExpiry);

                // Get failure reason from Log table
                string failureReason = "-";
                try
                {
                    if (m.EnrollmentID.HasValue && m.EnrollmentID.Value > 0)
                    {
                        var logEntry = _dbHelper.GetLatestLogForMember(m.EnrollmentID.Value);
                        if (logEntry != null)
                        {
                            failureReason = string.IsNullOrWhiteSpace(logEntry.FailureReason) ? "-" : logEntry.FailureReason;
                        }
                    }
                }
                catch { }

                item.SubItems.Add(failureReason != "-" ? "[View]" : "-");
                item.SubItems.Add("[View]");
                item.Tag = m;
                result.Add(item);
            }

            return result;
        }

        // Core loader: filters in-memory from cache — no item rebuild on every keystroke
        private void LoadMembersIntoListView(string nameFilter)
        {
            try
            {
                if (_cachedMemberItems == null)
                    _cachedMemberItems = BuildAllMemberItems();

                List<ListViewItem> displayItems;
                if (string.IsNullOrWhiteSpace(nameFilter))
                    displayItems = _cachedMemberItems.ToList();
                else
                    displayItems = _cachedMemberItems
                        .Where(item => item.Tag is Member m &&
                                       !string.IsNullOrWhiteSpace(m.Name) &&
                                       m.Name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();

                if (cmbMemberStatus != null && cmbMemberStatus.SelectedIndex > 0)
                {
                    string statusFilter = cmbMemberStatus.SelectedItem.ToString();
                    displayItems = displayItems.Where(item => item.Tag is Member m &&
                        (m.Status ?? "").Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (cmbMemberSync != null && cmbMemberSync.SelectedIndex > 0)
                {
                    string syncFilter = cmbMemberSync.SelectedItem.ToString();
                    displayItems = displayItems.Where(item =>
                    {
                        string syncText = item.SubItems.Count > 5 ? item.SubItems[5].Text : "";
                        if (syncFilter == "Not Synced") return syncText.StartsWith("0/");
                        if (syncFilter == "Fully Synced")
                        {
                            if (!syncText.Contains("/")) return false;
                            var parts = syncText.Split('/');
                            return parts.Length == 2 && parts[0].Trim() == parts[1].Split(' ')[0].Trim();
                        }
                        if (syncFilter == "Partially Synced")
                        {
                            if (!syncText.Contains("/")) return false;
                            var parts = syncText.Split('/');
                            return parts.Length == 2 && parts[0].Trim() != "0" && parts[0].Trim() != parts[1].Split(' ')[0].Trim();
                        }
                        return true;
                    }).ToList();
                }

                if (chkMemberFailure != null && chkMemberFailure.Checked)
                    displayItems = displayItems.Where(item =>
                        item.SubItems.Count > 7 && item.SubItems[7].Text == "[View]").ToList();

                var navy   = Color.FromArgb(30, 38, 90);
                var muted  = Color.FromArgb(130, 138, 165);
                var accent = Color.FromArgb(59, 130, 246);
                var amber  = Color.FromArgb(245, 158, 11);
                var red    = Color.FromArgb(239, 68, 68);
                var green  = Color.FromArgb(16, 185, 129);

                clv_members.SetRowFactory(displayItems.Count, idx =>
                {
                    var item = displayItems[idx];
                    var m = item.Tag as Member;
                    bool hasFailure = item.SubItems.Count > 7 && item.SubItems[7].Text == "[View]";
                    Member capturedM = m;

                    // Status pill color
                    string statusTxt = item.SubItems.Count > 3 ? item.SubItems[3].Text : "-";
                    bool isActive = string.Equals(statusTxt, "active", StringComparison.OrdinalIgnoreCase);
                    Color sBg = isActive ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 226, 226);
                    Color sFg = isActive ? green : red;

                    string initial = (m != null && !string.IsNullOrWhiteSpace(m.Name)) ? m.Name.Substring(0, 1).ToUpperInvariant() : "?";

                    var builder = new CLV.CardBuilder()
                        .Avatar(initial, accent, 8, 8, 36)
                        .Text(item.SubItems[1].Text, 50, 6,  160, 18, CLV.F9Bold, navy)
                        .Text(item.SubItems[0].Text, 50, 26, 160, 16, CLV.F8,     muted)
                        .Text(item.SubItems[2].Text, 220, 6,  75,  18, CLV.F9,     muted)
                        .Pill(statusTxt,            sBg, sFg, 300, 13, 68, 22)
                        .Pill(item.SubItems.Count > 4 && !string.IsNullOrWhiteSpace(item.SubItems[4].Text) ? item.SubItems[4].Text : "-", Color.FromArgb(219, 234, 254), Color.FromArgb(37, 99, 235), 372, 13, 100, 22)
                        .Text(item.SubItems.Count > 5 ? item.SubItems[5].Text : "-", 478, 6,  60,  18, CLV.F8, muted)
                        .Text(item.SubItems.Count > 6 ? item.SubItems[6].Text : "-", 543, 6,  90,  18, CLV.F8, muted);

                    if (hasFailure)
                        builder.Button("Failure", Color.FromArgb(254, 226, 226), red, 638, 13, 68, 26, () =>
                        {
                            if (capturedM == null) return;
                            string fullReason = "-";
                            try
                            {
                                if (capturedM.EnrollmentID.HasValue)
                                {
                                    var log = _dbHelper.GetLatestLogForMember(capturedM.EnrollmentID.Value);
                                    if (log != null && !string.IsNullOrWhiteSpace((string)log.FailureReason))
                                        fullReason = (string)log.FailureReason;
                                }
                            }
                            catch { }
                            if (fullReason != "-")
                                using (var dlg = new FailureReasonDetailForm(fullReason, capturedM, _dbHelper))
                                    dlg.ShowDialog(this);
                        });

                    builder.Button("View", accent, Color.White, 712, 13, 60, 26, () =>
                    {
                        if (capturedM != null) ShowMemberDeviceStatus(capturedM);
                    });

                    return builder.Build(900, 60);
                });
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Error", $"Failed to load members: {ex.Message}", MsgType.Error);
            }
        }

        private void rtb_memb_name_TextChanged(object sender, EventArgs e)
        {
            if (!_isInitialized) return;
            var filter = rtb_memb_name.Text?.Trim();
            LoadMembersIntoListView(string.IsNullOrEmpty(filter) ? null : filter);
        }

        /////////////////////////PENDING JOBS///////////////////////////////

        // Same "Pending: N" figure shown on the Dashboard — driven by the Log/StaffLog tables,
        // where a member/staff+device row has at least one biometric whose SyncStatus is still false.
        // Data comes straight from AppSession (kept current by CacheManager); no manual refresh needed —
        // OnCacheUpdated repaints these lists whenever CacheManager's periodic refresh picks up a change.
        private void InitializePendingJobsListView()
        {
            if (cmbPendingFor.Items.Count == 0)
            {
                cmbPendingFor.Items.AddRange(new object[] { "All Types", "Fingerprint", "Face", "RFID" });
                cmbPendingFor.SelectedIndex = 0;
            }
            if (cmbPendingDevice.Items.Count == 0)
            {
                cmbPendingDevice.Items.Add("All Devices");
                foreach (var d in AppSession.DeviceList ?? new List<Device>())
                    cmbPendingDevice.Items.Add(!string.IsNullOrWhiteSpace(d.Location) ? d.Location : d.DeviceSerial ?? $"Device {d.DeviceId}");
                cmbPendingDevice.SelectedIndex = 0;
            }

            RefreshMemberPendingJobsList();
        }

        private void RefreshMemberPendingJobsList()
        {
            try
            {
                var unsyncedEntries = AppSession.PendingBiometricSyncLogs ?? new List<LogEntry>();

                var membersById = (AppSession.AllMembers ?? new List<Member>())
                    .GroupBy(m => m.MemberID)
                    .ToDictionary(g => g.Key, g => g.First());

                var devicesById = (AppSession.DeviceList ?? new List<Device>())
                    .GroupBy(d => d.DeviceId)
                    .ToDictionary(g => g.Key, g => g.First());

                var rows = new List<(string MemberId, string Name, string Enrollment, string Device, string PendingFor, string Since)>();

                foreach (var entry in unsyncedEntries)
                {
                    string name = membersById.TryGetValue(entry.MemberID, out var m) && !string.IsNullOrWhiteSpace(m.Name)
                        ? m.Name : "-";

                    string deviceName = devicesById.TryGetValue(entry.DeviceID, out var d)
                        ? (!string.IsNullOrWhiteSpace(d.Location) ? d.Location
                           : !string.IsNullOrWhiteSpace(d.DeviceSerial) ? d.DeviceSerial
                           : $"Device {entry.DeviceID}")
                        : $"Device {entry.DeviceID}";

                    string pendingFor = entry.SyncStatus == null
                        ? "-"
                        : string.Join(", ", entry.SyncStatus.Where(kv => !kv.Value).Select(kv => kv.Key));
                    if (string.IsNullOrEmpty(pendingFor)) continue;

                    rows.Add((entry.MemberID.ToString(), name, entry.EnrollmentID > 0 ? entry.EnrollmentID.ToString() : "-", deviceName, pendingFor, string.IsNullOrWhiteSpace(entry.UpdatedAt) ? "-" : entry.UpdatedAt));
                }

                // Apply filters
                string devSel = cmbPendingDevice?.SelectedItem?.ToString() ?? "All Devices";
                if (devSel != "All Devices")
                    rows = rows.Where(r => string.Equals(r.Device, devSel, StringComparison.OrdinalIgnoreCase)).ToList();

                string typeSel = cmbPendingFor?.SelectedItem?.ToString() ?? "All Types";
                if (typeSel != "All Types")
                    rows = rows.Where(r => r.PendingFor.IndexOf(typeSel, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                var navy  = Color.FromArgb(30, 38, 90);
                var muted = Color.FromArgb(130, 138, 165);
                var amber = Color.FromArgb(245, 158, 11);

                clv_pendingJobs.SetRowFactory(rows.Count, idx =>
                {
                    var r = rows[idx];
                    return new CLV.CardBuilder()
                        .Text(r.MemberId,   0,   6, 90,  18, CLV.F9,     navy)
                        .Text(r.Name,       90,  6, 145, 18, CLV.F9Bold, navy)
                        .Text(r.Enrollment, 240, 6, 85,  18, CLV.F9,     muted)
                        .Text(r.Device,     330, 6, 135, 18, CLV.F8,     muted)
                        .Pill(r.PendingFor, Color.FromArgb(254, 243, 199), amber, 470, 13, 178, 22)
                        .Text(r.Since,      655, 6, 160, 18, CLV.F8,     muted)
                        .Build(820, 52);
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RefreshMemberPendingJobsList] Error: {ex.Message}");
            }
        }

        private void InitializeStaffPendingJobsListView()
        {
            if (cmbStaffPendingFor.Items.Count == 0)
            {
                cmbStaffPendingFor.Items.AddRange(new object[] { "All Types", "Fingerprint", "Face", "RFID" });
                cmbStaffPendingFor.SelectedIndex = 0;
            }
            if (cmbStaffPendingDevice.Items.Count == 0)
            {
                cmbStaffPendingDevice.Items.Add("All Devices");
                foreach (var d in AppSession.StaffDeviceList ?? new List<Device>())
                    cmbStaffPendingDevice.Items.Add(!string.IsNullOrWhiteSpace(d.Location) ? d.Location : d.DeviceSerial ?? $"Device {d.DeviceId}");
                cmbStaffPendingDevice.SelectedIndex = 0;
            }

            RefreshStaffPendingJobsList();
        }

        private void RefreshStaffPendingJobsList()
        {
            try
            {
                var unsyncedEntries = AppSession.StaffPendingBiometricSyncLogs ?? new List<StaffLogEntry>();

                var staffById = (AppSession.AllStaff ?? new List<Staff>())
                    .GroupBy(s => s.StaffID)
                    .ToDictionary(g => g.Key, g => g.First());

                var devicesById = (AppSession.StaffDeviceList ?? new List<Device>())
                    .GroupBy(d => d.StaffDeviceId)
                    .ToDictionary(g => g.Key, g => g.First());

                var rows = new List<(string StaffId, string Name, string Enrollment, string Device, string PendingFor, string Since)>();

                foreach (var entry in unsyncedEntries)
                {
                    string name = staffById.TryGetValue(entry.StaffID, out var s) && !string.IsNullOrWhiteSpace(s.Name)
                        ? s.Name : "-";

                    string deviceName = devicesById.TryGetValue(entry.StaffDeviceId, out var d)
                        ? (!string.IsNullOrWhiteSpace(d.Location) ? d.Location
                           : !string.IsNullOrWhiteSpace(d.DeviceSerial) ? d.DeviceSerial
                           : $"Device {entry.StaffDeviceId}")
                        : $"Device {entry.StaffDeviceId}";

                    string pendingFor = entry.SyncStatus == null
                        ? "-"
                        : string.Join(", ", entry.SyncStatus.Where(kv => !kv.Value).Select(kv => kv.Key));
                    if (string.IsNullOrEmpty(pendingFor)) continue;

                    rows.Add((entry.StaffID ?? "-", name, entry.EnrollmentID.HasValue && entry.EnrollmentID.Value > 0 ? entry.EnrollmentID.ToString() : "-", deviceName, pendingFor, string.IsNullOrWhiteSpace(entry.UpdatedAt) ? "-" : entry.UpdatedAt));
                }

                // Apply filters
                string sDevSel = cmbStaffPendingDevice?.SelectedItem?.ToString() ?? "All Devices";
                if (sDevSel != "All Devices")
                    rows = rows.Where(r => string.Equals(r.Device, sDevSel, StringComparison.OrdinalIgnoreCase)).ToList();

                string sTypeSel = cmbStaffPendingFor?.SelectedItem?.ToString() ?? "All Types";
                if (sTypeSel != "All Types")
                    rows = rows.Where(r => r.PendingFor.IndexOf(sTypeSel, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                var sNavy  = Color.FromArgb(30, 38, 90);
                var sMuted = Color.FromArgb(130, 138, 165);
                var sAmber = Color.FromArgb(245, 158, 11);

                clv_staffPendingJobs.SetRowFactory(rows.Count, idx =>
                {
                    var r = rows[idx];
                    return new CLV.CardBuilder()
                        .Text(r.StaffId,    0,   6, 90,  18, CLV.F9,     sNavy)
                        .Text(r.Name,       90,  6, 145, 18, CLV.F9Bold, sNavy)
                        .Text(r.Enrollment, 240, 6, 85,  18, CLV.F9,     sMuted)
                        .Text(r.Device,     330, 6, 135, 18, CLV.F8,     sMuted)
                        .Pill(r.PendingFor, Color.FromArgb(254, 243, 199), sAmber, 470, 13, 178, 22)
                        .Text(r.Since,      655, 6, 160, 18, CLV.F8,     sMuted)
                        .Build(820, 52);
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RefreshStaffPendingJobsList] Error: {ex.Message}");
            }
        }

        private void InitializeMemberList()
        {
            // Load all members from the AppSession cache
            List<Member> members = AppSession.AllMembers ?? new List<Member>();
            LogHelper.Log($"Initialized Member AutoComplete with {members.Count} members from session.");

            // Configure the AutoCompleteSource and AutoCompleteMode
            rtb_membername.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            rtb_membername.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Create a StringCollection to store member names
            AutoCompleteStringCollection memberCollection = new AutoCompleteStringCollection();

            // Add each member's name to the collection
            foreach (var member in members)
            {
                if (!string.IsNullOrWhiteSpace(member.Name))
                {
                    memberCollection.Add(member.Name);
                }
            }

            // Assign the collection to the AutoCompleteCustomSource
            rtb_membername.AutoCompleteCustomSource = memberCollection;
        }


        private void txt_membername_TextChanged(object sender, EventArgs e)
        {

        }

        private void date_member_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btn_filter_member_Click(object sender, EventArgs e)
        {
            LoadFilteredMemberAttendanceLogs();
        }

        private void LoadFilteredMemberAttendanceLogs()
        {
            string memberNameFilter = rtb_membername.Text.Trim();
            DateTime? selectedDate = date_member.Checked ? date_member.Value.Date : (DateTime?)null;

            var attendanceLogs = AppSession.AllAttendanceLogs ?? new List<dynamic>();
            var members        = AppSession.AllMembers ?? new List<Member>();

            var filteredLogs = attendanceLogs.Where(log =>
            {
                var mem = members.FirstOrDefault(m => m.MemberID == log.MemberID);
                string memberName = mem?.Name ?? "-";

                if (!string.IsNullOrEmpty(memberNameFilter) && (string.IsNullOrEmpty(memberName) || !memberName.Contains(memberNameFilter)))
                    return false;

                if (selectedDate.HasValue)
                {
                    DateTime? inTime  = ParseDateTime(log.CheckInTime);
                    DateTime? outTime = ParseDateTime(log.CheckOutTime);
                    if ((!inTime.HasValue || inTime.Value.Date != selectedDate.Value) &&
                        (!outTime.HasValue || outTime.Value.Date != selectedDate.Value))
                        return false;
                }
                return true;
            }).ToList();

            RenderMemberAttendanceLogs(filteredLogs, members);
        }

        private DateTime? ParseDateTime(string dateTimeString)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                return null;
            }

            if (DateTime.TryParse(dateTimeString, out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }

            return null;
        }


        private void lst_member_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMemberAttendanceLogs();
        }

        private void LoadMemberAttendanceLogs()
        {
            var attendanceLogs = AppSession.AllAttendanceLogs ?? new List<dynamic>();
            var members        = AppSession.AllMembers ?? new List<Member>();
            RenderMemberAttendanceLogs(attendanceLogs, members);
        }

        private void RenderMemberAttendanceLogs(IList<dynamic> logs, List<Member> members)
        {
            var navy   = Color.FromArgb(30, 38, 90);
            var muted  = Color.FromArgb(130, 138, 165);
            var green  = Color.FromArgb(16, 185, 129);
            var amber  = Color.FromArgb(180, 120, 0);
            var accent = Color.FromArgb(59, 130, 246);

            clv_memberAttendance.SetRowFactory(logs.Count, idx =>
            {
                var log = logs[idx];
                var mem = members.FirstOrDefault(m => m.MemberID == log.MemberID);
                string name    = mem?.Name ?? "-";
                string initial = !string.IsNullOrWhiteSpace(name) && name != "-" ? name.Substring(0, 1).ToUpperInvariant() : "?";
                string ciRaw   = log.CheckInTime  as string ?? "";
                string coRaw   = log.CheckOutTime as string ?? "";
                string ciFmt   = FormatAttendanceTime(ciRaw);
                string coFmt   = FormatAttendanceTime(coRaw);
                bool stillInside = string.IsNullOrWhiteSpace(coRaw);
                string duration  = ComputeTimeOnSite(ciRaw, coRaw);

                var builder = new CLV.CardBuilder()
                    .Avatar(initial, accent, 8, 12, 36)
                    .Text(name,                    50,  8,  240, 18, CLV.F9Bold, navy)
                    .Text($"ID {log.MemberID}",   50,  28, 240, 16, CLV.F8,     muted)
                    .Text(log.LogID.ToString(),   295, 20,  85, 18, CLV.F9,     muted)
                    .Pill(ciFmt ?? "-", Color.FromArgb(209, 250, 229), green, 390, 17, 112, 24);

                if (stillInside)
                    builder.Pill("● Still inside", Color.FromArgb(254, 243, 199), amber, 520, 17, 120, 24);
                else
                    builder.Pill(coFmt ?? "-", Color.FromArgb(209, 250, 229), green, 520, 17, 112, 24);

                builder.Text(duration ?? "—", 660, 20, 140, 18, CLV.F9, navy);

                return builder.Build(820, 60);
            });
        }

        private static string FormatAttendanceTime(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateTime.TryParse(raw, out DateTime dt)) return dt.ToString("hh:mm tt");
            return raw;
        }

        private static string ComputeTimeOnSite(string checkIn, string checkOut)
        {
            if (!DateTime.TryParse(checkIn, out DateTime ci) || !DateTime.TryParse(checkOut, out DateTime co)) return null;
            var diff = co - ci;
            if (diff.TotalSeconds < 0) return null;
            if (diff.TotalMinutes < 1) return "<1m";
            if (diff.TotalHours < 1) return $"{(int)diff.TotalMinutes}m";
            return $"{(int)diff.TotalHours}h {diff.Minutes}m";
        }

        private void date_manualdoor_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btn_filter_manualdoor_Click(object sender, EventArgs e)
        {
            LoadFilteredManualDoorLogs();
        }

        private void LoadFilteredManualDoorLogs()
        {
            if (cmbDoorVisitor.Items.Count == 0)
            {
                cmbDoorVisitor.Items.AddRange(new object[] { "All", "Members Only", "Visitors Only" });
                cmbDoorVisitor.SelectedIndex = 0;
            }

            DateTime selectedDate  = date_manualdoor.Value.Date;
            var gateAccessLogs     = AppSession.AllGateLogs ?? new List<GateAccessed>();
            var members            = AppSession.AllMembers ?? new List<Member>();

            var filteredLogs = gateAccessLogs.Where(log => log.AccessedTime.Date == selectedDate).ToList();

            string visitorSel = cmbDoorVisitor?.SelectedItem?.ToString() ?? "All";
            if (visitorSel == "Members Only")
                filteredLogs = filteredLogs.Where(log => !log.IsVisitor).ToList();
            else if (visitorSel == "Visitors Only")
                filteredLogs = filteredLogs.Where(log => log.IsVisitor).ToList();

            RenderManualDoorLogs(filteredLogs, members);
        }

        private void lst_manualdoor_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadManualDoorLogs();
        }

        private void LoadManualDoorLogs()
        {
            var gateAccessLogs = AppSession.AllGateLogs ?? new List<GateAccessed>();
            var members        = AppSession.AllMembers ?? new List<Member>();
            RenderManualDoorLogs(gateAccessLogs, members);
        }

        private void RenderManualDoorLogs(List<GateAccessed> logs, List<Member> members)
        {
            var navy   = Color.FromArgb(30, 38, 90);
            var muted  = Color.FromArgb(130, 138, 165);
            var violet = Color.FromArgb(109, 40, 217);

            var allDevices = (AppSession.DeviceList ?? new List<Device>())
                .Concat(AppSession.StaffDeviceList ?? new List<Device>())
                .ToList();

            clv_manualDoor.SetRowFactory(logs.Count, idx =>
            {
                var log = logs[idx];
                var mem = members.FirstOrDefault(m => m.MemberID == log.MemberId);

                // Gate display: look up device name by IP
                var dev = allDevices.FirstOrDefault(d => d.DeviceIP == log.DeviceIP);
                string gateName = !string.IsNullOrWhiteSpace(dev?.Location) ? dev.Location
                    : (!string.IsNullOrWhiteSpace(dev?.Department) ? dev.Department : $"Gate {log.GateId}");
                string gateIp   = log.DeviceIP ?? "-";

                // Person display
                string personName, personSub;
                bool isVisitor = log.IsVisitor;
                if (isVisitor)
                {
                    personName = !string.IsNullOrWhiteSpace(log.VisitorName) ? log.VisitorName : "Visitor";
                    personSub  = $"Visitor · {log.VisitorPhone ?? "-"}";
                }
                else
                {
                    personName = mem?.Name ?? $"Member {log.MemberId}";
                    personSub  = $"Member · ID {log.MemberId}";
                }

                string timeFmt = FormatAttendanceTime(log.AccessedTime.ToString()) ?? log.AccessedTime.ToString("hh:mm tt");
                string reason  = !string.IsNullOrWhiteSpace(log.Reason) ? log.Reason : "-";

                var builder = new CLV.CardBuilder()
                    .Text(gateName, 0,   8,  215, 18, CLV.F9Bold, navy)
                    .Text(gateIp,   0,   28, 215, 16, CLV.F8,     muted)
                    .Text(personName, 220, 8, isVisitor ? 145 : 310, 18, CLV.F9Bold, navy);

                if (isVisitor)
                    builder.Pill("Visitor", Color.FromArgb(237, 233, 254), violet, 370, 11, 60, 22);

                builder
                    .Text(personSub,  220, 28, 310, 16, CLV.F8, muted)
                    .Text(reason,     540, 20, 165, 18, CLV.F9, muted)
                    .Text(timeFmt,    710, 20, 100, 18, CLV.F9, navy);

                return builder.Build(820, 60);
            });
        }


        /////////////////////////////////SETUP/////////////////////////////////////////
        private void btn_savedebounce_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rtb_debounce.Text) || !int.TryParse(rtb_debounce.Text, out int seconds))
                {
                    ModernMessageBox.Show(this, "Validation Error", "Please enter a valid numeric value for debounce seconds.", MsgType.Warning);
                    return;
                }

                if (seconds < 0)
                {
                    ModernMessageBox.Show(this, "Validation", "Debounce seconds cannot be negative.", MsgType.Warning);
                    return;
                }

                // Persist as integer (string storage), key: AttendanceDebounceSeconds
                _dbHelper.SaveSetting("AttendanceDebounceSeconds", seconds.ToString());

                LogHelper.Log($"[Debounce] Saved AttendanceDebounceSeconds = {seconds}");
                ModernMessageBox.Show(this, "Success", "Debounce setting saved.", MsgType.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Debounce][ERROR] {ex.Message}");
                ModernMessageBox.Show(this, "Error", "Failed to save debounce setting.", MsgType.Error);
            }
        }


        private async void btn_getdb_Click(object sender, EventArgs e)
        {
            rb_getdb.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                LogHelper.Log("Starting backup bundle (DB snapshot + Logs + GLogs) email.");

                bool ok = await CommonUtils.CreateAndEmailDbLogsAndGLogsAsync(_dbHelper);

                if (ok)
                {
                    ModernMessageBox.Show(this, "Success", "Backup bundle has been sent via email (DB snapshot + Logs ZIP + GLogs ZIP).", MsgType.Success);
                }
                else
                {
                    ModernMessageBox.Show(this, "Error", "Failed to send backup bundle email. Check Logs for details.", MsgType.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in btn_getdb_Click: {ex.Message}");
                ModernMessageBox.Show(this, "Error", $"An error occurred: {ex.Message}", MsgType.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                rb_getdb.Enabled = true;
            }
        }


        private void btn_settime_Click(object sender, EventArgs e)
        {
            try
            {
                // Get current local time and offset
                var nowLocal = DateTime.Now;
                var nowOffset = DateTimeOffset.Now.Offset; // includes DST if applicable

                // Build GMT string: "+HH:mm" or "-HH:mm"
                string sign = nowOffset < TimeSpan.Zero ? "-" : "+";
                var abs = nowOffset.Duration();
                string gmt = $"{sign}{abs.Hours:D2}:{abs.Minutes:D2}";

                // Persist settings
                _dbHelper.SaveSetting("LocalTime", nowLocal.ToString("yyyy-MM-dd HH:mm:ss"));
                _dbHelper.SaveSetting("GMT", gmt);

                LogHelper.Log($"[Setup] Saved LocalTime='{nowLocal:yyyy-MM-dd HH:mm:ss}', GMT='{gmt}'");
                ModernMessageBox.Show(this, "Time Saved", $"Local time and GMT saved.\nLocalTime: {nowLocal:yyyy-MM-dd HH:mm:ss}\nGMT: {gmt}", MsgType.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[btn_settime_Click][ERROR] {ex.Message}");
                ModernMessageBox.Show(this, "Error", "Failed to save local time and GMT. See logs for details.", MsgType.Error);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////



        ////////////////////////////////////STAFF MANAGEMENT HELPERS////////////////////////////////////

        // initialize AutoComplete suggestions for rtb_staff using DB: _dbHelper.GetStaffs()
        private void InitializeStaffSuggestions()
        {
            try
            {
                var staffList = AppSession.AllStaff ?? new List<Staff>();

                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in staffList)
                {
                    if (!string.IsNullOrWhiteSpace(s.Name))
                        names.Add(s.Name.Trim());
                }

                rtb_staff.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                rtb_staff.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var staffCollection = new AutoCompleteStringCollection();
                staffCollection.AddRange(names.ToArray());

                rtb_staff.AutoCompleteCustomSource = staffCollection;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[InitializeStaffSuggestions][ERROR] {ex.Message}");
            }
        }

        // core loader with optional name filter, backed by DB: _dbHelper.GetStaffs()
        private void LoadStaffsIntoListView(string nameFilter)
        {
            try
            {
                var staffList = _dbHelper.GetStaffs() ?? new List<Staff>();

                if (!string.IsNullOrWhiteSpace(nameFilter))
                {
                    staffList = staffList
                        .Where(s =>
                        {
                            var name = s?.GetType().GetProperty("Name")?.GetValue(s)?.ToString();
                            if (string.IsNullOrWhiteSpace(name)) name = SafeToString(s, "Name");
                            return !string.IsNullOrWhiteSpace(name) &&
                                   name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                        })
                        .ToList();
                }

                // Get all staff devices for sync status calculation
                var allDevices = AppSession.StaffDeviceList ?? new List<Device>();
                int totalDevices = allDevices.Count;

                var allItems = new List<ListViewItem>();

                foreach (var s in staffList)
                {
                    // Resolve common fields
                    string staffIdText =
                        s?.GetType().GetProperty("StaffID")?.GetValue(s)?.ToString() ??
                        s?.GetType().GetProperty("Id")?.GetValue(s)?.ToString() ??
                        "-";

                    string nameText =
                        s?.GetType().GetProperty("Name")?.GetValue(s)?.ToString() ??
                        "-";

                    string statusText =
                        s?.GetType().GetProperty("Status")?.GetValue(s)?.ToString() ??
                        "-";

                    string rfidText =
                        s?.GetType().GetProperty("RFID")?.GetValue(s)?.ToString() ??
                        "-";

                    string pictureRaw =
                        s?.GetType().GetProperty("Picture")?.GetValue(s)?.ToString() ??
                        string.Empty;
                    string pictureText = string.IsNullOrWhiteSpace(pictureRaw) ? "-" : Path.GetFileName(pictureRaw);

                    // Fingerprint flag resolution
                    string fingerprintText = "-";
                    try
                    {
                        var hasFpProp = s.GetType().GetProperty("HasFingerprint");
                        var fpFlagProp = s.GetType().GetProperty("FingerprintFlag");
                        var fpCountProp = s.GetType().GetProperty("FingerprintCount");

                        if (hasFpProp != null)
                        {
                            bool hasFp = (bool)(hasFpProp.GetValue(s) ?? false);
                            fingerprintText = hasFp ? "Yes" : "No";
                        }
                        else if (fpFlagProp != null)
                        {
                            var val = fpFlagProp.GetValue(s);
                            fingerprintText = (val != null && val.ToString() == "1") ? "Yes" : "No";
                        }
                        else if (fpCountProp != null)
                        {
                            int cnt = Convert.ToInt32(fpCountProp.GetValue(s) ?? 0);
                            fingerprintText = cnt > 0 ? $"Yes ({cnt})" : "No";
                        }
                    }
                    catch { }

                    string enrollmentIdText =
                        s?.GetType().GetProperty("EnrollmentID")?.GetValue(s)?.ToString() ??
                        s?.GetType().GetProperty("EnrollmentId")?.GetValue(s)?.ToString() ??
                        "-";

                    // Sync status across devices (like Members)
                    string syncStatusText = "-";
                    if (totalDevices > 0)
                    {
                        int syncedCount = 0;
                        // For staff, iterate staff devices and check sync per device
                        // Using similar pattern as members: for each device, check if staff is synced
                        foreach (var d in allDevices)
                        {
                            try
                            {
                                // Assuming DB helper has similar method for staff device sync status
                                // If not available, you can calculate based on staff enrollment presence
                                var hasSyncProp = s.GetType().GetProperty("SyncStatus");
                                if (hasSyncProp != null && hasSyncProp.GetValue(s) is Dictionary<int, bool> syncDict)
                                {
                                    int deviceKey = d.StaffDeviceId > 0 ? d.StaffDeviceId : d.DeviceId;
                                    if (syncDict.TryGetValue(deviceKey, out bool isSynced) && isSynced)
                                        syncedCount++;
                                }
                            }
                            catch { }
                        }
                        syncStatusText = $"{syncedCount}/{totalDevices} devices";
                    }

                    var staffBioList = new System.Collections.Generic.List<string>();
                    if (!string.IsNullOrWhiteSpace(rfidText) && rfidText != "-") staffBioList.Add("RFID");
                    if (!string.IsNullOrWhiteSpace(pictureText) && pictureText != "-" && pictureText != "No") staffBioList.Add("Face");
                    if (!string.IsNullOrWhiteSpace(fingerprintText) && fingerprintText != "No") staffBioList.Add("Fingerprint");
                    string staffBioText = staffBioList.Count > 0 ? string.Join(", ", staffBioList) : "-";

                    // Create list item with all fields
                    var item = new ListViewItem(string.IsNullOrWhiteSpace(staffIdText) ? "-" : staffIdText);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(nameText) ? "-" : nameText);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(statusText) ? "-" : statusText);
                    item.SubItems.Add(staffBioText);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(enrollmentIdText) ? "-" : enrollmentIdText);
                    item.SubItems.Add(syncStatusText);

                    // Get failure reason from StaffLog table
                    string failureReason = "-";
                    try
                    {
                        int enrollId = 0;
                        if (int.TryParse(enrollmentIdText, out enrollId) && enrollId > 0)
                        {
                            var logEntry = _dbHelper.GetLatestStaffLogForEnrollment(enrollId);
                            if (logEntry != null)
                            {
                                failureReason = string.IsNullOrWhiteSpace(logEntry.FailureReason) ? "-" : logEntry.FailureReason;
                            }
                        }
                    }
                    catch { }

                    item.SubItems.Add(failureReason != "-" ? "[View]" : "-");

                    // Actions column: View button to open details form
                    item.SubItems.Add("[View]");

                    // Store staff object for action handlers
                    item.Tag = new { StaffID = staffIdText, Name = nameText, EnrollmentID = enrollmentIdText, StaffObject = s };

                    allItems.Add(item);
                }

                // Apply staff filters
                string staffStatusSel = cmbStaffStatus?.SelectedItem?.ToString() ?? "All Status";
                if (staffStatusSel != "All Status")
                    allItems = allItems.Where(i => string.Equals(i.SubItems[2].Text, staffStatusSel, StringComparison.OrdinalIgnoreCase)).ToList();

                string staffSyncSel = cmbStaffSync?.SelectedItem?.ToString() ?? "All Sync";
                if (staffSyncSel == "Fully Synced")
                    allItems = allItems.Where(i => { var p = i.SubItems[5].Text.Split('/'); return p.Length == 2 && p[0].Trim() == p[1].Split(' ')[0].Trim(); }).ToList();
                else if (staffSyncSel == "Not Synced")
                    allItems = allItems.Where(i => i.SubItems[5].Text.StartsWith("0/")).ToList();

                if (chkStaffFailure?.Checked == true)
                    allItems = allItems.Where(i => i.SubItems[6].Text == "[View]").ToList();

                var sNavy   = Color.FromArgb(30, 38, 90);
                var sMuted  = Color.FromArgb(130, 138, 165);
                var sAccent = Color.FromArgb(59, 130, 246);
                var sGreen  = Color.FromArgb(16, 185, 129);
                var sRed    = Color.FromArgb(239, 68, 68);

                clv_staffs.SetRowFactory(allItems.Count, idx =>
                {
                    var item = allItems[idx];
                    string staffId   = item.SubItems[0].Text;
                    string name      = item.SubItems[1].Text;
                    string status    = item.SubItems[2].Text;
                    string bio       = item.SubItems[3].Text;
                    string enrollId  = item.SubItems[4].Text;
                    string sync      = item.SubItems[5].Text;
                    bool hasFailure  = item.SubItems.Count > 6 && item.SubItems[6].Text == "[View]";

                    bool isActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
                    Color stBg = isActive ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 226, 226);
                    Color stFg = isActive ? sGreen : sRed;

                    string initial = !string.IsNullOrWhiteSpace(name) ? name.Substring(0, 1).ToUpperInvariant() : "?";
                    dynamic capturedTag = item.Tag;

                    var builder = new CLV.CardBuilder()
                        .Avatar(initial, sAccent, 8, 8, 36)
                        .Text(name,    50, 6,  215, 18, CLV.F9Bold, sNavy)
                        .Text(staffId, 50, 26, 215, 16, CLV.F8,     sMuted)
                        .Pill(status,  stBg, stFg, 275, 13, 80, 22)
                        .Pill(string.IsNullOrWhiteSpace(bio) ? "-" : bio, Color.FromArgb(219, 234, 254), Color.FromArgb(37, 99, 235), 360, 13, 125, 22)
                        .Text(enrollId, 490, 6, 80,  18, CLV.F8,     sMuted)
                        .Text(sync,     575, 6, 70,  18, CLV.F8,     sMuted);

                    if (hasFailure)
                        builder.Button("Failure", Color.FromArgb(254, 226, 226), sRed, 650, 13, 68, 26, () =>
                        {
                            string staffName = capturedTag?.Name?.ToString() ?? "-";
                            int.TryParse(capturedTag?.EnrollmentID?.ToString() ?? "", out int enrollIdInt);
                            string fullReason = "-";
                            try
                            {
                                var log = _dbHelper.GetLatestLogForMember(enrollIdInt);
                                if (log != null && !string.IsNullOrWhiteSpace((string)log.FailureReason))
                                    fullReason = (string)log.FailureReason;
                            }
                            catch { }
                            if (fullReason != "-")
                                using (var dlg = new FailureReasonDetailForm(fullReason, null, _dbHelper))
                                    dlg.ShowDialog(this);
                        });

                    builder.Button("View", sAccent, Color.White, 723, 13, 60, 26, () =>
                    {
                        ShowStaffDetailsForm(capturedTag?.StaffID?.ToString(), capturedTag?.Name?.ToString(), capturedTag?.EnrollmentID?.ToString());
                    });

                    return builder.Build(800, 60);
                });
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Error", $"Failed to load staff list: {ex.Message}", MsgType.Error);
            }
        }

        // setup clv_staffs and initial load
        private void InitializeStaffsListView()
        {
            if (cmbStaffStatus.Items.Count == 0)
            {
                cmbStaffStatus.Items.AddRange(new object[] { "All Status", "active", "inactive" });
                cmbStaffStatus.SelectedIndex = 0;
            }
            if (cmbStaffSync.Items.Count == 0)
            {
                cmbStaffSync.Items.AddRange(new object[] { "All Sync", "Fully Synced", "Not Synced" });
                cmbStaffSync.SelectedIndex = 0;
            }

            // Initial full load
            LoadStaffsIntoListView(null);
        }


        // button click handler to filter by rtb_staff text (unchanged)
        private void btn_stafffilter_Click(object sender, EventArgs e)
        {
            string nameFilter = rtb_staff.Text?.Trim();
            LoadStaffsIntoListView(string.IsNullOrEmpty(nameFilter) ? null : nameFilter);
        }

        // optional live filtering as user types (unchanged)
        private void rtb_staff_TextChanged(object sender, EventArgs e)
        {
            if (!_isInitialized) return;
            var filter = rtb_staff.Text?.Trim();
            LoadStaffsIntoListView(string.IsNullOrEmpty(filter) ? null : filter);
        }

        private void lst_members_ItemActivate(object sender, EventArgs e)
        {
            try
            {
                var listView = sender as ListView;
                if (listView?.SelectedItems.Count != 1) return;

                var item = listView.SelectedItems[0];
                var clickedPoint = listView.PointToClient(Control.MousePosition);
                var hitInfo = listView.HitTest(clickedPoint);

                if (hitInfo.Item != null && hitInfo.SubItem != null)
                {
                    int columnIndex = item.SubItems.IndexOf(hitInfo.SubItem);

                    if (columnIndex == 9 - 1) // Actions column [View] (members has 9 columns)
                    {
                        if (item.Tag is Member member)
                        {
                            ShowMemberDeviceStatus(member);
                        }
                    }
                    else if (columnIndex == 9 - 2) // Failure Reason [View] column
                    {
                        if (item.Tag is Member m && item.SubItems[columnIndex].Text == "[View]")
                        {
                            // Re-query full reason fresh from DB
                            string fullReason = "-";
                            try
                            {
                                if (m.EnrollmentID.HasValue)
                                {
                                    var log = _dbHelper.GetLatestLogForMember(m.EnrollmentID.Value);
                                    if (log != null && !string.IsNullOrWhiteSpace((string)log.FailureReason))
                                        fullReason = (string)log.FailureReason;
                                }
                            }
                            catch { }

                            if (fullReason != "-")
                            {
                                using (var dlg = new FailureReasonDetailForm(fullReason, m, _dbHelper))
                                    dlg.ShowDialog(this);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[lst_members_ItemActivate] Error: {ex.Message}");
            }
        }

        private async void ShowMemberDeviceStatus(Member member)
        {
            if (member == null) return;

            if (!member.EnrollmentID.HasValue || member.EnrollmentID.Value <= 0)
            {
                ModernMessageBox.Show(this, "Not Found", "This member does not have an Enrollment ID assigned.", MsgType.Info);
                return;
            }

            int enrollmentId = member.EnrollmentID.Value;
            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

            if (allDevices.Count == 0)
            {
                ModernMessageBox.Show(this, "No Devices", "No devices available to check.", MsgType.Info);
                return;
            }

            try
            {
                Dashboard.GlobalStartLoader($"Fetching device details for {member.Name}...");
                var deviceData = new List<(Device Device, Newtonsoft.Json.Linq.JObject Details, string Error)>();

                foreach (var device in allDevices)
                {
                    Dashboard.GlobalUpdateLoaderMessage($"Checking {device.Location ?? device.DeviceIP}...");
                    string sn = CommonUtils.GetDeviceSnByIp(device.DeviceIP);
                    if (string.IsNullOrEmpty(sn))
                    {
                        deviceData.Add((device, null, "Device not connected via WebSocket"));
                        continue;
                    }
                    try
                    {
                        string detailsJson = await WebsocketServer.ExecuteServiceGetCommand(sn, "getuserdetails",
                            $"{{\"enrollid\":{enrollmentId}}}");
                        if (string.IsNullOrEmpty(detailsJson))
                        {
                            deviceData.Add((device, null, "No response from device (timeout or not enrolled)"));
                        }
                        else
                        {
                            var jObj = Newtonsoft.Json.Linq.JObject.Parse(detailsJson);
                            deviceData.Add((device, jObj, null));
                        }
                    }
                    catch (Exception ex)
                    {
                        deviceData.Add((device, null, ex.Message));
                    }
                }

                Dashboard.GlobalStopLoader();
                using (var dlg = new DeviceStatusDialog(member, deviceData))
                {
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Dashboard.GlobalStopLoader();
                ModernMessageBox.Show(this, "Error", $"Error retrieving member status: {ex.Message}", MsgType.Error);
            }
        }


        // AutoComplete for rtb_staffname using known staff names from session
        private void InitializeStaffAttendanceSuggestions()
        {
            try
            {
                rtb_staffname.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                rtb_staffname.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var staffList = AppSession.AllStaff ?? new List<Staff>();
                var names = new AutoCompleteStringCollection();

                foreach (var s in staffList)
                {
                    if (!string.IsNullOrWhiteSpace(s.Name))
                        names.Add(s.Name);
                }

                rtb_staffname.AutoCompleteCustomSource = names;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[InitializeStaffAttendanceSuggestions][ERROR] {ex.Message}");
            }
        }

        //  setup clv_staffAttendance and initial load
        private void InitializeStaffAttendanceListView()
        {
            LoadFilteredStaffAttendanceLogs();
        }


        // unified filter loader (by optional name and/or date)
        private void LoadFilteredStaffAttendanceLogs()
        {
            try
            {
                string nameFilter = rtb_staffname.Text?.Trim();
                bool filterByDate = date_staffattendence.Checked;
                DateTime selectedDate = date_staffattendence.Value.Date;

                var logs = AppSession.AllStaffAttendanceLogs ?? new List<dynamic>();

                var filtered = logs.Where(log =>
                {
                    if (!string.IsNullOrEmpty(nameFilter))
                    {
                        string logName = SafeToString(log, "Name");
                        if (string.IsNullOrEmpty(logName) || logName.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                    }

                    if (filterByDate)
                    {
                        DateTime? inTime  = ParseDateTime(SafeToString(log, "CheckInTime"));
                        DateTime? outTime = ParseDateTime(SafeToString(log, "CheckOutTime"));
                        if ((!inTime.HasValue || inTime.Value.Date != selectedDate) &&
                            (!outTime.HasValue || outTime.Value.Date != selectedDate))
                            return false;
                    }
                    return true;
                }).ToList();

                var navy  = Color.FromArgb(30, 38, 90);
                var muted = Color.FromArgb(130, 138, 165);

                var amber  = Color.FromArgb(180, 120, 0);
                var accent = Color.FromArgb(59, 130, 246);
                var green  = Color.FromArgb(16, 185, 129);

                clv_staffAttendance.SetRowFactory(filtered.Count, idx =>
                {
                    var log         = filtered[idx];
                    string logId    = SafeToString(log, "LogID");
                    string enrollId = SafeToString(log, "EnrollmentID");
                    if (string.IsNullOrEmpty(enrollId)) enrollId = SafeToString(log, "EnrollmentId");
                    string attendeeId = SafeToString(log, "AttendeeID");
                    if (string.IsNullOrEmpty(attendeeId)) attendeeId = SafeToString(log, "StaffID");
                    string name     = SafeToString(log, "Name");
                    string checkIn  = SafeToString(log, "CheckInTime");
                    string checkOut = SafeToString(log, "CheckOutTime");

                    string initial   = !string.IsNullOrWhiteSpace(name) ? name.Substring(0, 1).ToUpperInvariant() : "?";
                    string ciFmt     = FormatAttendanceTime(checkIn);
                    string coFmt     = FormatAttendanceTime(checkOut);
                    bool stillInside = string.IsNullOrWhiteSpace(checkOut);
                    string duration  = ComputeTimeOnSite(checkIn, checkOut);
                    string subText   = !string.IsNullOrWhiteSpace(enrollId) ? $"Enroll {enrollId}" : (!string.IsNullOrWhiteSpace(attendeeId) ? $"ID {attendeeId}" : "-");

                    var builder = new CLV.CardBuilder()
                        .Avatar(initial, accent, 8, 12, 36)
                        .Text(string.IsNullOrWhiteSpace(name) ? "-" : name, 50,  8,  240, 18, CLV.F9Bold, navy)
                        .Text(subText,                                       50,  28, 240, 16, CLV.F8,     muted)
                        .Text(string.IsNullOrWhiteSpace(logId) ? "-" : logId, 295, 20, 85, 18, CLV.F9,   muted)
                        .Pill(ciFmt ?? "-", Color.FromArgb(209, 250, 229), green, 390, 17, 112, 24);

                    if (stillInside)
                        builder.Pill("● Still inside", Color.FromArgb(254, 243, 199), amber, 520, 17, 120, 24);
                    else
                        builder.Pill(coFmt ?? "-", Color.FromArgb(209, 250, 229), green, 520, 17, 112, 24);

                    builder.Text(duration ?? "—", 660, 20, 140, 18, CLV.F9, navy);

                    return builder.Build(820, 60);
                });
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Error", $"Failed to load staff attendance logs: {ex.Message}", MsgType.Error);
            }
        }

        
        // button click handler
        private void btn_filterattendencestaff_Click(object sender, EventArgs e)
        {
            LoadFilteredStaffAttendanceLogs();
        }

        // live filter on typing
        private void rtb_staffname_TextChanged(object sender, EventArgs e)
        {
            if (!_isInitialized) return;
            LoadFilteredStaffAttendanceLogs();
        }

        // re-filter when date changes
        private void date_staffattendence_ValueChanged(object sender, EventArgs e)
        {
            LoadFilteredStaffAttendanceLogs();
        }

        // Utility: safely get dynamic property by common key names
        private string SafeToString(dynamic obj, string key)
        {
            try
            {
                var dict = obj as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey(key))
                    return dict[key]?.ToString();

                // Try reflection
                var prop = obj.GetType().GetProperty(key);
                if (prop != null)
                {
                    var val = prop.GetValue(obj, null);
                    return val?.ToString();
                }
            }
            catch { /* ignore */ }

            return string.Empty;
        }

        // Handle click on Actions column 
        private void lst_staffs_ItemActivate(object sender, EventArgs e)
        {
            try
            {
                var listView = sender as ListView;
                if (listView?.SelectedItems.Count != 1) return;

                var item = listView.SelectedItems[0];
                var clickedPoint = listView.PointToClient(Control.MousePosition);
                var hitInfo = listView.HitTest(clickedPoint);

                // If Actions column (index 10) was clicked
                if (hitInfo.Item != null && hitInfo.SubItem != null)
                {
                    int columnIndex = item.SubItems.IndexOf(hitInfo.SubItem);

                    if (columnIndex == 8 - 1) // Actions column [View] (staffs has 8 columns)
                    {
                        dynamic staffTag = item.Tag;
                        string staffIdStr = staffTag.StaffID;
                        string enrollmentStr = staffTag.EnrollmentID;
                        string staffName = staffTag.Name;

                        // Open staff details form with Enroll/Delete buttons
                        ShowStaffDetailsForm(staffIdStr, staffName, enrollmentStr);
                    }
                    else if (columnIndex == 8 - 2 && item.SubItems[columnIndex].Text == "[View]") // Failure Reason [View]
                    {
                        dynamic staffTag = item.Tag;
                        string staffName = staffTag.Name?.ToString() ?? "-";
                        int.TryParse(staffTag.EnrollmentID?.ToString() ?? "", out int staffEnrollId);

                        string fullReason = "-";
                        string pictureUrl = null;
                        try
                        {
                            var log = _dbHelper.GetLatestLogForMember(staffEnrollId);
                            if (log != null && !string.IsNullOrWhiteSpace((string)log.FailureReason))
                                fullReason = (string)log.FailureReason;
                            var memberId = _dbHelper.GetMemberIdByEnrollment(staffEnrollId);
                            if (memberId.HasValue)
                                pictureUrl = _dbHelper.GetMember(memberId.Value)?.Picture;
                        }
                        catch { }

                        if (fullReason == "-") return;

                        using (var dlg = new FailureReasonDetailForm(
                            fullReason,
                            currentMemberId: null,
                            currentEnrollmentId: staffEnrollId > 0 ? staffEnrollId : (int?)null,
                            currentName: staffName,
                            currentPictureUrl: pictureUrl,
                            dbHelper: _dbHelper))
                        {
                            dlg.ShowDialog(this);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[lst_staffs_ItemActivate] Error: {ex.Message}");
                ModernMessageBox.Show(this, "Error", $"Error: {ex.Message}", MsgType.Error);
            }
        }

        private async void ShowStaffDetailsForm(string staffId, string staffName, string enrollmentId)
        {
            if (string.IsNullOrEmpty(enrollmentId) || !int.TryParse(enrollmentId, out int enrollmentIdInt) || enrollmentIdInt <= 0)
            {
                ModernMessageBox.Show(this, "Not Found", "This staff member does not have a valid Enrollment ID.", MsgType.Info);
                return;
            }

            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

            if (allDevices.Count == 0)
            {
                ModernMessageBox.Show(this, "No Devices", "No devices available to check.", MsgType.Info);
                return;
            }

            try
            {
                Dashboard.GlobalStartLoader($"Fetching device details for {staffName}...");
                var deviceData = new List<(Device Device, Newtonsoft.Json.Linq.JObject Details, string Error)>();

                foreach (var device in allDevices)
                {
                    Dashboard.GlobalUpdateLoaderMessage($"Checking {device.Location ?? device.DeviceIP}...");
                    string sn = CommonUtils.GetDeviceSnByIp(device.DeviceIP);
                    if (string.IsNullOrEmpty(sn))
                    {
                        deviceData.Add((device, null, "Device not connected via WebSocket"));
                        continue;
                    }
                    try
                    {
                        string detailsJson = await WebsocketServer.ExecuteServiceGetCommand(sn, "getuserdetails",
                            $"{{\"enrollid\":{enrollmentIdInt}}}");
                        if (string.IsNullOrEmpty(detailsJson))
                        {
                            deviceData.Add((device, null, "No response from device (timeout or not enrolled)"));
                        }
                        else
                        {
                            var jObj = Newtonsoft.Json.Linq.JObject.Parse(detailsJson);
                            deviceData.Add((device, jObj, null));
                        }
                    }
                    catch (Exception ex)
                    {
                        deviceData.Add((device, null, ex.Message));
                    }
                }

                Dashboard.GlobalStopLoader();

                // Use DeviceStatusDialog with staff data (same UI as members) and Enroll/Delete callbacks
                using (var dlg = new DeviceStatusDialog(
                    new { StaffID = staffId, Name = staffName, EnrollmentID = enrollmentId },
                    deviceData,
                    onEnroll: () => EnrollStaffFromListView(staffId, staffName, enrollmentId),
                    onDelete: () => DeleteStaffFromListView(staffId, staffName, enrollmentId)))
                {
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[ShowStaffDetailsForm] Error: {ex.Message}");
                ModernMessageBox.Show(this, "Error", $"Error: {ex.Message}", MsgType.Error);
            }
        }

        private async void DeleteStaffFromListView(string staffIdentifier, string staffName, string enrollmentIdStr = null)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(staffName))
            {
                ModernMessageBox.Show(this, "Validation Error", "Staff name is required.", MsgType.Warning);
                return;
            }

            // Resolve the staff record from DB
            int? enrollmentId = null;
            object dbStaff = null;
            try
            {
                var staffList = _dbHelper.GetStaffs() ?? new List<Staff>();
                dbStaff = staffList.FirstOrDefault(s =>
                {
                    var sid = s?.GetType().GetProperty("StaffID")?.GetValue(s)?.ToString();
                    var id = s?.GetType().GetProperty("Id")?.GetValue(s)?.ToString();
                    return string.Equals(sid, staffIdentifier, StringComparison.OrdinalIgnoreCase)
                           || string.Equals(id, staffIdentifier, StringComparison.OrdinalIgnoreCase);
                });

                if (dbStaff != null)
                {
                    // Try extract enrollment id from DB staff record
                    var enrolProp = dbStaff.GetType().GetProperty("EnrollmentID") ?? dbStaff.GetType().GetProperty("EnrollmentId");
                    if (enrolProp != null)
                    {
                        var val = enrolProp.GetValue(dbStaff)?.ToString();
                        if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out int parsedEnrol) && parsedEnrol > 0)
                            enrollmentId = parsedEnrol;
                    }
                }
            }
            catch (Exception dbEx)
            {
                LogHelper.Log($"[DeleteStaffFromListView] Warning - DB extraction failed: {dbEx.Message}");
            }

            // If enrollmentId not found in DB, try parse from argument
            if (!enrollmentId.HasValue && !string.IsNullOrWhiteSpace(enrollmentIdStr))
            {
                if (int.TryParse(enrollmentIdStr, out int parsed))
                    enrollmentId = parsed;
            }

            // If still no enrollmentId, we cannot proceed
            if (!enrollmentId.HasValue || enrollmentId.Value <= 0)
            {
                ModernMessageBox.Show(this, "Missing Enrollment ID", $"Cannot delete '{staffName}'. No numeric Enrollment ID found for this staff.\n\nPlease ensure the staff record has a valid EnrollmentID (numeric) in the database.", MsgType.Warning);
                LogHelper.Log($"[DeleteStaffFromListView] Aborted - EnrollmentID missing for staff '{staffName}' (Identifier: {staffIdentifier})");
                return;
            }

            int enrollId = enrollmentId.Value;

            // Confirm deletion with user
            if (!ModernMessageBox.ShowConfirm(this, "Confirm Delete", $"Are you sure you want to DELETE all biometric data for '{staffName}' (Enrollment ID: {enrollId}) from all devices?\n\nThis action cannot be undone.", "Yes, Delete", "Cancel", MsgType.Warning))
            {
                LogHelper.Log($"[DeleteStaffFromListView] Deletion cancelled by user for staff '{staffName}' (EnrollmentID: {enrollId})");
                return;
            }

            Cursor = Cursors.WaitCursor;

            try
            {
                LogHelper.Log($"[DeleteStaffFromListView] Starting deletion for staff '{staffName}' (EnrollmentID: {enrollId})");

                // Delete from all devices
                bool deleteSuccess = await DeleteStaffAcrossAllDevicesAsync(enrollId, staffName);

                // Only show success message if the operation was not cancelled
                if (deleteSuccess)
                {
                    ModernMessageBox.Show(this, "Deletion Complete", $"✓ Staff '{staffName}' (Enrollment ID: {enrollId}) deleted successfully from all devices!", MsgType.Success);

                    LogHelper.Log($"[DeleteStaffFromListView] ✓ Successfully deleted staff '{staffName}' (Enrollment ID: {enrollId})");

                    // Refresh the staff list view
                    LoadStaffsIntoListView(rtb_staff.Text?.Trim());
                }
                else
                {
                    LogHelper.Log($"[DeleteStaffFromListView] Deletion cancelled or failed for staff '{staffName}' (Enrollment ID: {enrollId})");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DeleteStaffFromListView] ❌ Error: {ex.Message}\n{ex.StackTrace}");
                ModernMessageBox.Show(this, "Error", $"❌ Deletion failed: {ex.Message}", MsgType.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private async void EnrollStaffFromListView(string staffIdentifier, string staffName, string enrollmentIdStr = null)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(staffName))
            {
                ModernMessageBox.Show(this, "Validation Error", "Staff name is required.", MsgType.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;

            try
            {
                LogHelper.Log($"[EnrollStaffFromListView] Starting enrollment for staff '{staffName}' (Identifier: {staffIdentifier})");

                // Resolve the staff record from DB using common keys (StaffID or Id)
                int? enrollmentId = null;
                object dbStaff = null;
                try
                {
                    var staffList = _dbHelper.GetStaffs() ?? new List<Staff>();
                    dbStaff = staffList.FirstOrDefault(s =>
                    {
                        var sid = s?.GetType().GetProperty("StaffID")?.GetValue(s)?.ToString();
                        var id = s?.GetType().GetProperty("Id")?.GetValue(s)?.ToString();
                        return string.Equals(sid, staffIdentifier, StringComparison.OrdinalIgnoreCase)
                               || string.Equals(id, staffIdentifier, StringComparison.OrdinalIgnoreCase);
                    });

                    if (dbStaff != null)
                    {
                        // Try extract enrollment id from DB staff record (common property names)
                        var enrolProp = dbStaff.GetType().GetProperty("EnrollmentID") ?? dbStaff.GetType().GetProperty("EnrollmentId");
                        if (enrolProp != null)
                        {
                            var val = enrolProp.GetValue(dbStaff)?.ToString();
                            if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out int parsedEnrol) && parsedEnrol > 0)
                                enrollmentId = parsedEnrol;
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    LogHelper.Log($"[EnrollStaffFromListView] Warning - DB extraction failed: {dbEx.Message}");
                }

                // If enrollmentId not provided in tag / db, try parse enrollmentIdStr argument
                if (!enrollmentId.HasValue && !string.IsNullOrWhiteSpace(enrollmentIdStr))
                {
                    if (int.TryParse(enrollmentIdStr, out int parsed))
                        enrollmentId = parsed;
                }

                // If still no enrollmentId, we cannot safely call the device APIs that require numeric enrollment IDs
                if (!enrollmentId.HasValue || enrollmentId.Value <= 0)
                {
                    ModernMessageBox.Show(this, "Missing Enrollment ID", $"Cannot enroll '{staffName}'. No numeric Enrollment ID found for this staff.\n\nPlease ensure the staff record has a valid EnrollmentID (numeric) in the database.", MsgType.Warning);
                    LogHelper.Log($"[EnrollStaffFromListView] Aborted - EnrollmentID missing for staff '{staffName}' (Identifier: {staffIdentifier})");
                    return;
                }

                int enrollId = enrollmentId.Value;

                // Check for duplicate enrollment in the in-memory buffer by enrollment id
                var already = _enrolledStaffBuffer.FirstOrDefault(x => x.Id == enrollId);
                if (already != default)
                {
                    ModernMessageBox.Show(this, "Duplicate Entry", $"Enrollment ID {enrollId} is already enrolled in the buffer. Cannot enroll twice.", MsgType.Warning);
                    return;
                }

                // Extract card ID and face image from DB staff object if available
                long staffCardId = 0;
                string storedImagePath = null;

                if (dbStaff != null)
                {
                    try
                    {
                        // Get RFID/Card ID
                        var rfidProp = dbStaff.GetType().GetProperty("RFID");
                        if (rfidProp != null && long.TryParse(rfidProp.GetValue(dbStaff)?.ToString() ?? "0", out long rfid))
                        {
                            staffCardId = rfid;
                            LogHelper.Log($"[EnrollStaffFromListView] Found Card ID: {staffCardId} for enrollment {enrollId}");
                        }

                        // Get StaffID to resolve local face image from FACE directory
                        var staffIdProp = dbStaff.GetType().GetProperty("StaffID");
                        string staffIdForImage = staffIdProp?.GetValue(dbStaff)?.ToString() ?? staffIdentifier;

                        // Resolve face image from FACE directory using StaffID (not the Picture URL)
                        string faceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FACE");

                        // Try different extensions with StaffID as filename
                        string[] candidates =
                        {
                    Path.Combine(faceDir, staffIdForImage + ".jpg"),
                    Path.Combine(faceDir, staffIdForImage + ".png"),
                    Path.Combine(faceDir, staffIdForImage + ".jpeg")
                };

                        foreach (var candidate in candidates)
                        {
                            if (File.Exists(candidate))
                            {
                                storedImagePath = candidate;
                                LogHelper.Log($"[EnrollStaffFromListView] Found face image at: {storedImagePath}");
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(storedImagePath))
                        {
                            LogHelper.Log($"[EnrollStaffFromListView] No local face image found for StaffID: {staffIdForImage} in FACE directory");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[EnrollStaffFromListView] Warning extracting RFID/Image: {ex.Message}");
                    }
                }

                // Validate: at least one biometric required
                bool hasImage = storedImagePath != null && File.Exists(storedImagePath);
                bool hasCard = staffCardId > 0;

                if (!hasImage && !hasCard)
                {
                    ModernMessageBox.Show(this, "Insufficient Data", $"❌ No Card ID or Face Image found for '{staffName}'.\n\nCannot enroll without biometric data.", MsgType.Warning);
                    LogHelper.Log($"[EnrollStaffFromListView] Enrollment cancelled - no biometric data for staff '{staffName}' (EnrollmentID: {enrollId})");
                    return;
                }

                // Add to in-memory buffer using the numeric enrollment ID
                _enrolledStaffBuffer.Add((enrollId, staffName, storedImagePath ?? string.Empty));

                // Enroll across all devices using the numeric enrollment ID
                bool enrollSuccess = await EnrollStaffAcrossAllDevicesAsync(enrollId, staffName, staffCardId, storedImagePath ?? string.Empty);

                // Only show success message if the operation was not cancelled
                if (enrollSuccess)
                {
                    ModernMessageBox.Show(this, "Enrollment Complete", $"✓ Staff '{staffName}' (Enrollment ID: {enrollId}) enrolled successfully!\n\nCard ID: {(hasCard ? staffCardId.ToString() : "Not used")}\nFace Image: {(hasImage ? "Yes" : "No")}", MsgType.Success);

                    LogHelper.Log($"[EnrollStaffFromListView] ✓ Successfully enrolled staff '{staffName}' (Enrollment ID: {enrollId})");
                }
                else
                {
                    LogHelper.Log($"[EnrollStaffFromListView] Enrollment cancelled or failed for staff '{staffName}' (Enrollment ID: {enrollId})");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[EnrollStaffFromListView] ❌ Error: {ex.Message}\n{ex.StackTrace}");
                ModernMessageBox.Show(this, "Error", $"❌ Enrollment failed: {ex.Message}", MsgType.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private List<Device> ShowDeviceSelectionDialog(List<Device> devices, string title = "Select Devices")
        {
            return ModernMessageBox.ShowDeviceSelector(this, devices, title);
        }

        private async Task<bool> EnrollStaffAcrossAllDevicesAsync(int enrollId, string userName, long cardId, string imagePath)
        {
            var normalDevices = AppSession.DeviceList ?? new List<Device>();

            if (normalDevices.Count == 0)
            {
                ModernMessageBox.Show(this, "No Devices", "No normal devices are available to enroll.", MsgType.Info);
                return false;
            }

            List<Device> targetDevices = ShowDeviceSelectionDialog(normalDevices, "Select Devices to Enroll");
            if (targetDevices == null || targetDevices.Count == 0) return false;

            try
            {
                Dashboard.GlobalStartLoader($"Enrolling \"{userName}\" via Cloud WebSocket...");

                int processed = 0;
                foreach (var device in targetDevices)
                {
                    processed++;
                    try
                    {
                        // WebSocket uses Serial Number (SN) as the key
                        string sn = device.DeviceSerial;
                        string deviceLabel = !string.IsNullOrWhiteSpace(device.Location) ? device.Location : sn;
                        Dashboard.GlobalUpdateLoaderMessage($"[{processed}/{targetDevices.Count}] Enrolling on {deviceLabel}...");

                        // 1) Set username (Cloud Protocol 6)
                        var nameParams = new { enrollid = enrollId, name = userName };
                        bool nameOk = await WebsocketServer.ExecuteServiceCommand(sn, "setusername", JsonConvert.SerializeObject(nameParams));

                        if (!nameOk)
                        {
                            LogHelper.Log($"Failed WebSocket Dispatch: setusername for {userName} on {sn}");
                            continue;
                        }

                        // 2) Add RFID (Cloud Protocol 3 - backupnum 11)
                        if (cardId > 0)
                        {
                            var rfidParams = new { enrollid = enrollId, name = userName, record = cardId.ToString() };
                            await WebsocketServer.ExecuteServiceCommand(sn, "setrfid", JsonConvert.SerializeObject(rfidParams));
                        }

                        // 3) Add Face (Cloud Protocol 3 - backupnum 50)
                        if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                        {
                            var faceParams = new { enrollid = enrollId, name = userName, record = imagePath };
                            await WebsocketServer.ExecuteServiceCommand(sn, "setface", JsonConvert.SerializeObject(faceParams));
                        }

                        Dashboard.GlobalUpdateLoaderMessage($"[{processed}/{targetDevices.Count}] Dispatched to {deviceLabel}");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Error enrolling {userName} on device {device?.DeviceSerial}: {ex.Message}");
                    }
                }
                Dashboard.GlobalUpdateLoaderMessage($"Enrollment commands dispatched for \"{userName}\".");
            }
            finally
            {
                Dashboard.GlobalStopLoader();
            }

            return true;
        }

        private async Task<bool> DeleteStaffAcrossAllDevicesAsync(int enrollId, string userName)
        {
            var normalDevices = AppSession.DeviceList ?? new List<Device>();

            if (normalDevices.Count == 0)
            {
                ModernMessageBox.Show(this, "No Devices", "No normal devices are available.", MsgType.Info);
                return false;
            }

            List<Device> targetDevices = ShowDeviceSelectionDialog(normalDevices, "Select Devices to Delete From");
            if (targetDevices == null || targetDevices.Count == 0) return false;

            try
            {
                Dashboard.GlobalStartLoader($"Deleting \"{userName}\" from {targetDevices.Count} device(s)...");

                int processed = 0;
                int successCount = 0;
                int failCount = 0;

                foreach (var device in targetDevices)
                {
                    processed++;
                    try
                    {
                        string sn = device.DeviceSerial;
                        string deviceLabel = !string.IsNullOrWhiteSpace(device.Location) ? device.Location : sn;
                        Dashboard.GlobalUpdateLoaderMessage($"[{processed}/{targetDevices.Count}] Deleting from {deviceLabel}...");

                        // Cloud Protocol 4: deleteuser
                        // backupnum 12 = all templates (fp, card, pwd, face)
                        var deleteParams = new { enrollid = enrollId, backupnum = 12 };

                        bool isDispatched = await WebsocketServer.ExecuteServiceCommand(sn, "deleteuser", JsonConvert.SerializeObject(deleteParams));

                        if (isDispatched)
                        {
                            successCount++;
                            LogHelper.Log($"Delete command dispatched for {userName} ({enrollId}) on {sn}");
                        }
                        else
                        {
                            failCount++;
                            LogHelper.Log($"Failed to dispatch delete to {sn}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        LogHelper.Log($"Error deleting on device {device?.DeviceSerial}: {ex.Message}");
                    }
                }

                string summary = $"Deletion process complete for \"{userName}\".\n\nDispatched: {successCount}\nFailed: {failCount}";
                Dashboard.GlobalUpdateLoaderMessage(summary);
                await Task.Delay(1500);
            }
            finally
            {
                Dashboard.GlobalStopLoader();
            }

            return true;
        }
        private void btn_save_drivefolderid_Click(object sender, EventArgs e)
        {
            try
            {
                string folderId = rtb_drivefolderid.Text.Trim();
                if (string.IsNullOrWhiteSpace(folderId))
                {
                    ModernMessageBox.Show(this, "Validation Error", "Please enter a valid Google Drive Folder ID.", MsgType.Warning);
                    return;
                }

                _dbHelper.SaveSetting("GoogleDriveFolderId", folderId);
                LogHelper.Log($"[Backup] Saved GoogleDriveFolderId: {folderId}");
                ModernMessageBox.Show(this, "Success", "Google Drive Folder ID saved successfully.", MsgType.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Backup][ERROR] btn_save_drivefolderid: {ex.Message}");
            }
        }

        private void btn_save_drivecredentials_Click(object sender, EventArgs e)
        {
            try
            {
                string json = rtb_drivecredentials.Text.Trim();
                if (string.IsNullOrWhiteSpace(json))
                {
                    ModernMessageBox.Show(this, "Validation Error", "Please paste the credentials JSON content.", MsgType.Warning);
                    return;
                }

                // Verify if it's valid JSON before saving
                try { Newtonsoft.Json.Linq.JObject.Parse(json); }
                catch { ModernMessageBox.Show(this, "Invalid Format", "The content is not a valid JSON string.", MsgType.Error); return; }

                _dbHelper.SaveEncryptedSetting("GoogleDriveCredentialsJson", json);
                LogHelper.Log("[Backup] Saved and Encrypted Google Drive Credentials.");
                ModernMessageBox.Show(this, "Success", "Google Drive credentials saved and encrypted successfully.", MsgType.Success);
                rtb_drivecredentials.Clear(); // Clear for security

            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Backup][ERROR] btn_save_drivecredentials: {ex.Message}");
            }
        }

        private void btn_save_retentiondays_Click(object sender, EventArgs e)
        {
            try
            {
                // Reading numeric value from num_retention
                int days = (int)num_retention.Value;

                _dbHelper.SaveSetting("BackupRetentionDays", days.ToString());

                LogHelper.Log($"[Backup] Saved BackupRetentionDays: {days} days");
                ModernMessageBox.Show(this, "Success", "Retention policy saved successfully.", MsgType.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Backup][ERROR] btn_save_retentiondays: {ex.Message}");
                ModernMessageBox.Show(this, "Error", "Failed to save retention policy.", MsgType.Error);
            }
        }

        private void btn_save_backuptype_Click(object sender, EventArgs e)
        {
            try
            {
                // Reading boolean values from the content checkboxes
                bool includeLogs = chk_logs.Checked;
                bool includeGLogs = chk_glogs.Checked;
                bool includeDB = chk_database.Checked;

                // Save preferences to database
                _dbHelper.SaveSetting("BackupIncludeLogs", includeLogs.ToString().ToLower());
                _dbHelper.SaveSetting("BackupIncludeGLogs", includeGLogs.ToString().ToLower());
                _dbHelper.SaveSetting("BackupIncludeDatabase", includeDB.ToString().ToLower());

                LogHelper.Log($"[Backup] Content settings updated: Logs={includeLogs}, GLogs={includeGLogs}, DB={includeDB}");
                ModernMessageBox.Show(this, "Settings Saved", $"Backup preferences updated:\nLogs: {includeLogs}\nGLogs: {includeGLogs}\nDatabase: {includeDB}", MsgType.Success);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Backup][ERROR] btn_save_backuptype: {ex.Message}");
                ModernMessageBox.Show(this, "Error", "Failed to save backup preferences.", MsgType.Error);
            }
        }

        private async void btn_backupcleanup_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Get required settings from Database
                string folderId = _dbHelper.GetSetting("GoogleDriveFolderId");
                string retentionStr = _dbHelper.GetSetting("BackupRetentionDays");

                // 2. Validations
                if (string.IsNullOrEmpty(folderId))
                {
                    ModernMessageBox.Show(this, "Configuration Error", "Google Drive Folder ID is not configured. Please save it first.", MsgType.Warning);
                    return;
                }

                if (!int.TryParse(retentionStr, out int days) || days <= 0)
                {
                    ModernMessageBox.Show(this, "Configuration Error", "Retention policy is invalid or not set. Please save a value above 0 in Retention Days.", MsgType.Warning);
                    return;
                }

                // 3. User Confirmation
                var confirm = MessageBox.Show($"Are you sure you want to scan Google Drive and PERMANENTLY DELETE backup files older than {days} days?",
                                            "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                // 4. Execution
                rb_backupcleanup.Enabled = false;
                Cursor = Cursors.WaitCursor;

                int deleted = await CommonUtils.CleanupOldDriveFilesAsync(folderId, days, _dbHelper);

                // 5. Result Feedback
                if (deleted >= 0)
                {
                    ModernMessageBox.Show(this, "Success", $"Drive cleanup successful!\nTotal files removed: {deleted}", MsgType.Success);
                }
                else
                {
                    ModernMessageBox.Show(this, "Error", "Drive cleanup failed. Please check the system logs for details (likely an authentication error).", MsgType.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[UI] Error in btn_backupcleanup_Click: {ex.Message}");
                ModernMessageBox.Show(this, "Exception", $"An unexpected error occurred: {ex.Message}", MsgType.Error);
            }
            finally
            {
                rb_backupcleanup.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void btn_test_connection_Click(object sender, EventArgs e)
        {
            // Trigger initial backup after 5 seconds
            _backupService?.TriggerBackupAfterDelay(TimeSpan.FromSeconds(5));
        }

        private void card_Paint(object sender, PaintEventArgs e)
        {
            var card = sender as System.Windows.Forms.Panel;
            if (card == null) return;
            if (this.DesignMode) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var rc = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int r = 10, d = r * 2;
            path.AddArc(rc.X, rc.Y, d, d, 180, 90);
            path.AddArc(rc.Right - d, rc.Y, d, d, 270, 90);
            path.AddArc(rc.Right - d, rc.Bottom - d, d, d, 0, 90);
            path.AddArc(rc.X, rc.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            using (var br = new SolidBrush(Color.White)) g.FillPath(br, path);
            using (var pen = new Pen(Color.FromArgb(229, 231, 235), 1f)) g.DrawPath(pen, path);
            path.Dispose();
        }

        //////////////////////////////////////////////////////////////////////////////////////
    }
}

