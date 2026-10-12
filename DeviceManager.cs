using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
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

        // Pagination helpers - for high-volume data lists
        private ListViewPagination _membersPagination;
        private ListViewPagination _staffsPagination;
        private ListViewPagination _memberAttendancePagination;
        private ListViewPagination _staffAttendancePagination;
        private ListViewPagination _manualDoorPagination;
        private ListViewPagination _pendingJobsPagination;
        private ListViewPagination _staffPendingJobsPagination;

        // Cached full member item list — rebuilt only when source data changes
        private List<ListViewItem> _cachedMemberItems;

        // Guard: prevents TextChanged handlers from firing during form initialization
        private bool _isInitialized = false;

        // Filter controls — added programmatically in Initialize methods
        private ComboBox cmbMemberStatus;
        private ComboBox cmbMemberSync;
        private CheckBox chkMemberFailure;
        private ComboBox cmbAllMemberStatus;
        private ComboBox cmbAllMemberBiometric;
        private ComboBox cmbAllMemberExpiry;
        private ComboBox cmbOfflineServerSync;
        private ComboBox cmbOfflinePackage;
        private CheckBox chkOfflineFailure;
        private ComboBox cmbStaffStatus;
        private ComboBox cmbStaffSync;
        private CheckBox chkStaffFailure;
        private ComboBox cmbPendingDevice;
        private ComboBox cmbPendingFor;
        private ComboBox cmbStaffPendingDevice;
        private ComboBox cmbStaffPendingFor;
        private ComboBox cmbDoorVisitor;
        private ComboBox cmbDeviceStatus;
        private CheckBox chkDeviceIsStaff;
        private ComboBox cmbUserRole;

        // Font hierarchy — Title / Body / Small
        private static readonly Font FontTitle = new Font("Microsoft Sans Serif", 16F, FontStyle.Regular);
        private static readonly Font FontBody   = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular);
        private static readonly Font FontSmall  = new Font("Microsoft Sans Serif",  8.25F, FontStyle.Regular);

        // Edit-mode colors
        private static readonly Color EditableColor  = Color.White;
        private static readonly Color ReadOnlyColor  = Color.FromArgb(240, 240, 240);



        public DeviceManager()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
            _backupService = new BackupService(_dbHelper);

            // Initialize CacheManager with 30-second refresh interval
            _cacheManager = new CacheManager(_dbHelper, refreshIntervalSeconds: 30);
            _cacheManager.CacheUpdated += OnCacheUpdated;

            listViewDevices.SelectedIndexChanged += listViewDevices_SelectedIndexChanged;
            InitializeWebSocketManager();


            _processor = new Processor(_dbHelper, _wsManager, _deviceFunctions, _cacheManager);
            _wsManager.LiveUpdate += WsManager_LiveUpdate;
            this.Shown += DeviceManager_Shown;

            BuildOfflineMemberManagementUi();
            AddGateEntryFormSetting();
        }

        //////////////////////////////////GATE ACCESS SETTING///////////////////////////////

        private const string RequireGateEntryFormSettingKey = "RequireGateEntryForm";
        private const string RequireGatePasswordSettingKey = "RequireGatePassword";
        private CheckBox chk_requireGateEntryForm;
        private CheckBox chk_requireGatePassword;

        // Adds two toggles to Setup Management, both defaulting to checked (existing behavior)
        // when never saved:
        // - RequireGateEntryForm: when unchecked, Open skips GateAccessForm entirely and opens
        //   the gate directly with no visitor/member logging.
        // - RequireGatePassword: applies identically whether or not the form is shown — when
        //   unchecked, no password prompt on Open either way.
        private void AddGateEntryFormSetting()
        {
            var lblSection = new Label
            {
                Text = "Gate Access",
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 432)
            };
            subpnl_setup.Controls.Add(lblSection);

            chk_requireGateEntryForm = new CheckBox
            {
                Name = "chk_requireGateEntryForm",
                Text = "Require entry form (member/visitor + reason) before opening a gate",
                AutoSize = true,
                Location = new Point(16, 456)
            };
            chk_requireGatePassword = new CheckBox
            {
                Name = "chk_requireGatePassword",
                Text = "Require password before opening a gate (with or without the entry form)",
                AutoSize = true,
                Location = new Point(16, 480)
            };
            try
            {
                string formSetting = _dbHelper.GetSetting(RequireGateEntryFormSettingKey);
                chk_requireGateEntryForm.Checked = string.IsNullOrWhiteSpace(formSetting) || formSetting.Equals("true", StringComparison.OrdinalIgnoreCase);

                string passwordSetting = _dbHelper.GetSetting(RequireGatePasswordSettingKey);
                chk_requireGatePassword.Checked = string.IsNullOrWhiteSpace(passwordSetting) || passwordSetting.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Setup][GateEntryForm] Failed to load settings: {ex.Message}");
                chk_requireGateEntryForm.Checked = true;
                chk_requireGatePassword.Checked = true;
            }
            subpnl_setup.Controls.Add(chk_requireGateEntryForm);
            subpnl_setup.Controls.Add(chk_requireGatePassword);

            var btnSaveGateSetting = new Button
            {
                Text = "Save",
                Location = new Point(16, 508),
                Size = new Size(90, 26)
            };
            btnSaveGateSetting.Click += (s, e) =>
            {
                try
                {
                    _dbHelper.SaveSetting(RequireGateEntryFormSettingKey, chk_requireGateEntryForm.Checked ? "true" : "false");
                    _dbHelper.SaveSetting(RequireGatePasswordSettingKey, chk_requireGatePassword.Checked ? "true" : "false");
                    LogHelper.Log($"[Setup][GateEntryForm] Saved {RequireGateEntryFormSettingKey}={chk_requireGateEntryForm.Checked}, {RequireGatePasswordSettingKey}={chk_requireGatePassword.Checked}");
                    MessageBox.Show("Gate access settings saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[Setup][GateEntryForm][ERROR] {ex.Message}");
                    MessageBox.Show("Failed to save gate access settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            subpnl_setup.Controls.Add(btnSaveGateSetting);
        }

        //////////////////////////////////OFFLINE MEMBER MANAGEMENT///////////////////////////////

        private Panel pnl_offlineMembers;
        private ListView lst_offlineMembers;
        private TextBox txt_offlineMemberSearch;
        private Label nav_offlineMembers;
        private Label btn_offlineMembers;
        private Panel tile_offlineMembers;
        private List<OfflineMember> _offlineMembersCache = new List<OfflineMember>();

        private ListView lst_allMembers;
        private TextBox txt_allMembersSearch;
        private List<Member> _allMembersCache = new List<Member>();

        private void BuildOfflineMemberManagementUi()
        {
            AddOfflineMembersWelcomeTile();
            AddOfflineMembersNavTab();
            AddOfflineMembersSidebarButton();
            BuildOfflineMembersPanel();
        }

        // Mirrors tile_member's exact style/size so it flows into the welcome-screen tile grid
        // (pnl_welcome_tiles is a FlowLayoutPanel, so it lands right after Setup Management).
        private void AddOfflineMembersWelcomeTile()
        {
            var iconLbl = new Label
            {
                Font = new Font("Segoe UI Emoji", 26F),
                ForeColor = Color.FromArgb(18, 23, 49),
                Location = new Point(0, 18),
                Size = new Size(180, 52),
                Text = "🗂️",
                TextAlign = ContentAlignment.MiddleCenter
            };
            var nameLbl = new Label
            {
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 35, 60),
                Location = new Point(10, 74),
                Size = new Size(160, 22),
                Text = "Offline Management",
                TextAlign = ContentAlignment.MiddleCenter
            };
            var descLbl = new Label
            {
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.FromArgb(120, 125, 145),
                Location = new Point(10, 98),
                Size = new Size(160, 38),
                Text = "Enroll and track\nlocally-created members",
                TextAlign = ContentAlignment.TopCenter
            };

            tile_offlineMembers = new Panel
            {
                Name = "tile_offlineMembers",
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 18, 18),
                Size = new Size(180, 160)
            };
            tile_offlineMembers.Controls.Add(iconLbl);
            tile_offlineMembers.Controls.Add(nameLbl);
            tile_offlineMembers.Controls.Add(descLbl);

            foreach (Control c in new Control[] { tile_offlineMembers, iconLbl, nameLbl, descLbl })
                c.Click += btn_offlineMembers_Click;

            pnl_welcome_tiles.Controls.Add(tile_offlineMembers);
        }

        // Resize the existing 5 top-nav tabs to make room for a 6th ("Offline Members") tab,
        // keeping pnl_topnav's total width unchanged so nothing overflows the form.
        private void AddOfflineMembersNavTab()
        {
            var navLabels = new[] { nav_device, nav_user, nav_staff, nav_member, nav_setup };
            int totalWidth = pnl_topnav.Width; // 881
            int slots = navLabels.Length + 1;  // 6
            int evenWidth = totalWidth / slots; // 146
            int x = 0;
            foreach (var lbl in navLabels)
            {
                lbl.Location = new Point(x, 0);
                lbl.Width = evenWidth;
                x += evenWidth;
            }

            nav_offlineMembers = new Label
            {
                Name = "nav_offlineMembers",
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Font = nav_setup.Font,
                ForeColor = Color.White,
                Location = new Point(x, 0),
                Size = new Size(totalWidth - x, nav_setup.Height), // remainder fills the tab strip exactly
                Text = "Offline Management",
                TextAlign = ContentAlignment.MiddleCenter
            };
            nav_offlineMembers.Click += btn_offlineMembers_Click;
            nav_offlineMembers.MouseEnter += NavBtn_MouseEnter;
            nav_offlineMembers.MouseLeave += NavBtn_MouseLeave;
            pnl_topnav.Controls.Add(nav_offlineMembers);
        }

        // panel2 mirrors the same left-sidebar buttons as pnl_topnav's tabs (kept in sync via
        // SetActiveButton); added here for parity even though panel2 itself is currently hidden.
        private void AddOfflineMembersSidebarButton()
        {
            btn_offlineMembers = new Label
            {
                Name = "btn_offlineMembers",
                BackColor = Color.Transparent,
                Font = btn_setupmanage.Font,
                ForeColor = Color.White,
                Location = new Point(btn_setupmanage.Left, btn_setupmanage.Bottom),
                Padding = new Padding(10, 0, 0, 0),
                Size = btn_setupmanage.Size,
                Text = "Offline Management",
                TextAlign = ContentAlignment.MiddleLeft
            };
            btn_offlineMembers.Click += btn_offlineMembers_Click;
            btn_offlineMembers.MouseHover += SidebarButton_MouseHover;
            btn_offlineMembers.MouseLeave += SidebarButton_MouseLeave;
            panel2.Controls.Add(btn_offlineMembers);
        }

        private void BuildOfflineMembersPanel()
        {
            pnl_offlineMembers = new Panel
            {
                Name = "pnl_offlineMembers",
                BackColor = Color.WhiteSmoke,
                Location = new Point(1114, 750), // off-canvas until shown, matches pnl_attendence's pattern
                Size = pnl_attendence.Size,
                Visible = false
            };
            Controls.Add(pnl_offlineMembers);
            // Dynamically-added controls land on top of the z-order by default, which would
            // permanently cover the navy header + top-nav tabs (added earlier by InitializeComponent).
            // Send it to the back once, like every other section panel already sits behind them.
            pnl_offlineMembers.SendToBack();

            // Like tab_attendence (y=156) inside pnl_attendence, this panel's top ~156px sit
            // behind the navy header + top-nav tabs (siblings on the Form, drawn in front of
            // every section panel) — content must start below that, not at y=8.
            const int contentTop = 164;

            var tabs = new TabControl
            {
                Name = "tab_offlineMemberMgmt",
                Location = new Point(8, contentTop),
                Size = new Size(pnl_offlineMembers.Width - 16, pnl_offlineMembers.Height - contentTop - 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            pnl_offlineMembers.Controls.Add(tabs);

            var tabOffline = new TabPage("Offline Members");
            var tabAllMembers = new TabPage("Members");
            tabs.TabPages.Add(tabOffline);
            tabs.TabPages.Add(tabAllMembers);

            BuildOfflineMembersTab(tabOffline);
            BuildAllMembersTab(tabAllMembers);
        }

        private void BuildOfflineMembersTab(TabPage tab)
        {
            var btnAddOfflineMember = new Button
            {
                Name = "btn_addOfflineMember",
                Text = "Add Offline Member",
                Location = new Point(8, 8),
                Size = new Size(160, 28)
            };
            btnAddOfflineMember.Click += (s, e) =>
            {
                using (var form = new AddOfflineMemberForm())
                {
                    form.ShowDialog(this);
                }
                LoadOfflineMembersList();
            };
            tab.Controls.Add(btnAddOfflineMember);


            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(btnAddOfflineMember.Right + 16, 14),
                AutoSize = true
            };
            tab.Controls.Add(lblSearch);

            txt_offlineMemberSearch = new TextBox
            {
                Name = "txt_offlineMemberSearch",
                Location = new Point(lblSearch.Right + 6, 11),
                Size = new Size(200, 23)
            };
            txt_offlineMemberSearch.TextChanged += (s, e) => FilterOfflineMembersList();
            tab.Controls.Add(txt_offlineMemberSearch);

            int ox = txt_offlineMemberSearch.Right + 10;
            cmbOfflineServerSync = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(ox, 11), Width = 110 };
            cmbOfflineServerSync.Items.AddRange(new object[] { "All Sync", "Synced", "Pending" });
            cmbOfflineServerSync.SelectedIndex = 0;
            cmbOfflineServerSync.SelectedIndexChanged += (s, e) => FilterOfflineMembersList();
            tab.Controls.Add(cmbOfflineServerSync);
            ox += 118;

            cmbOfflinePackage = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(ox, 11), Width = 130 };
            cmbOfflinePackage.Items.Add("All Packages");
            cmbOfflinePackage.SelectedIndex = 0;
            cmbOfflinePackage.SelectedIndexChanged += (s, e) => FilterOfflineMembersList();
            tab.Controls.Add(cmbOfflinePackage);
            ox += 138;

            chkOfflineFailure = new CheckBox { Text = "Has Failure Only", Location = new Point(ox, 13), AutoSize = true };
            chkOfflineFailure.CheckedChanged += (s, e) => FilterOfflineMembersList();
            tab.Controls.Add(chkOfflineFailure);

            lst_offlineMembers = new ListView
            {
                Name = "lst_offlineMembers",
                Location = new Point(8, 44),
                Size = new Size(tab.ClientSize.Width - 16, tab.ClientSize.Height - 52),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lst_offlineMembers.Columns.Add("Enrollment", 80);
            lst_offlineMembers.Columns.Add("Name", 130);
            lst_offlineMembers.Columns.Add("Gender", 60);
            lst_offlineMembers.Columns.Add("Phone", 100);
            lst_offlineMembers.Columns.Add("Package", 110);
            lst_offlineMembers.Columns.Add("Periodicity", 80);
            lst_offlineMembers.Columns.Add("Biometric", 130);
            lst_offlineMembers.Columns.Add("Server Sync", 80);
            lst_offlineMembers.Columns.Add("Device Cleanup", 100);
            lst_offlineMembers.Columns.Add("Failure Reason", 85);
            lst_offlineMembers.ItemActivate += LstOfflineMembers_ItemActivate;
            tab.Controls.Add(lst_offlineMembers);
        }

        private void BuildAllMembersTab(TabPage tab)
        {

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(16, 14),
                AutoSize = true
            };
            tab.Controls.Add(lblSearch);

            txt_allMembersSearch = new TextBox
            {
                Name = "txt_allMembersSearch",
                Location = new Point(lblSearch.Right + 6, 11),
                Size = new Size(200, 23)
            };
            txt_allMembersSearch.TextChanged += (s, e) => FilterAllMembersList();
            tab.Controls.Add(txt_allMembersSearch);

            int ax = txt_allMembersSearch.Right + 10;
            cmbAllMemberStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(ax, 11), Width = 110 };
            cmbAllMemberStatus.Items.AddRange(new object[] { "All Status", "active", "inactive", "expired" });
            cmbAllMemberStatus.SelectedIndex = 0;
            cmbAllMemberStatus.SelectedIndexChanged += (s, e) => FilterAllMembersList();
            tab.Controls.Add(cmbAllMemberStatus);
            ax += 118;

            cmbAllMemberBiometric = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(ax, 11), Width = 130 };
            cmbAllMemberBiometric.Items.AddRange(new object[] { "All Biometric", "Has Face", "Has Fingerprint", "Has RFID", "Missing Any" });
            cmbAllMemberBiometric.SelectedIndex = 0;
            cmbAllMemberBiometric.SelectedIndexChanged += (s, e) => FilterAllMembersList();
            tab.Controls.Add(cmbAllMemberBiometric);
            ax += 138;

            cmbAllMemberExpiry = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(ax, 11), Width = 140 };
            cmbAllMemberExpiry.Items.AddRange(new object[] { "Any Expiry", "Expires in 7 days", "Expires in 30 days", "Expired" });
            cmbAllMemberExpiry.SelectedIndex = 0;
            cmbAllMemberExpiry.SelectedIndexChanged += (s, e) => FilterAllMembersList();
            tab.Controls.Add(cmbAllMemberExpiry);

            lst_allMembers = new ListView
            {
                Name = "lst_allMembers",
                Location = new Point(8, 44),
                Size = new Size(tab.ClientSize.Width - 16, tab.ClientSize.Height - 52),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lst_allMembers.Columns.Add("Member ID", 80);
            lst_allMembers.Columns.Add("Name", 160);
            lst_allMembers.Columns.Add("Enrollment", 90);
            lst_allMembers.Columns.Add("Status", 80);
            lst_allMembers.Columns.Add("Biometric", 130);
            lst_allMembers.Columns.Add("Sync Status", 120);
            lst_allMembers.Columns.Add("Package Expiry", 110);
            lst_allMembers.Columns.Add("Action", 80);
            lst_allMembers.ItemActivate += LstAllMembers_ItemActivate;
            tab.Controls.Add(lst_allMembers);
        }

        private void btn_offlineMembers_Click(object sender, EventArgs e)
        {
            if (pnl_offlineMembers.Visible && pnl_offlineMembers.Location == Point.Empty) return;
            HideAllPanels();
            SetHeader("Offline Management", "View members enrolled locally while offline.");
            pnl_topnav.Visible = true;
            pnl_topnav.BringToFront();
            pnl_welcome_header.BringToFront();
            SetActiveButton(btn_offlineMembers);
            SetActiveNavButton(nav_offlineMembers);
            pnl_offlineMembers.Location = new Point(0, 0);
            pnl_offlineMembers.Visible = true;
            LoadOfflineMembersList();
            LoadAllMembersList();
        }

        private void LoadOfflineMembersList()
        {
            try
            {
                _offlineMembersCache = _dbHelper.GetOfflineMembers() ?? new List<OfflineMember>();
                var packagesById = _dbHelper.GetPackages(activeOnly: false).ToDictionary(p => p.PackageId, p => p.Title);

                // Populate package filter with distinct packages used by offline members
                if (cmbOfflinePackage != null)
                {
                    string prevSel = cmbOfflinePackage.SelectedItem?.ToString() ?? "All Packages";
                    cmbOfflinePackage.Items.Clear();
                    cmbOfflinePackage.Items.Add("All Packages");
                    var usedPackages = new System.Collections.Generic.HashSet<string>();
                    foreach (var m in _offlineMembersCache)
                    {
                        if (string.IsNullOrWhiteSpace(m.SubscriptionJson)) continue;
                        try
                        {
                            var sub = JObject.Parse(m.SubscriptionJson);
                            int pkgId = sub["package_id"]?.ToObject<int>() ?? 0;
                            if (packagesById.TryGetValue(pkgId, out var title) && !string.IsNullOrWhiteSpace(title))
                                usedPackages.Add(title);
                        }
                        catch { }
                    }
                    foreach (var pkg in usedPackages.OrderBy(p => p))
                        cmbOfflinePackage.Items.Add(pkg);
                    cmbOfflinePackage.SelectedIndex = cmbOfflinePackage.Items.Contains(prevSel) ? cmbOfflinePackage.Items.IndexOf(prevSel) : 0;
                }

                RenderOfflineMembersList(_offlineMembersCache, packagesById);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DeviceManager] Failed to load offline members: {ex.Message}");
            }
        }

        private void FilterOfflineMembersList()
        {
            var packagesById = _dbHelper.GetPackages(activeOnly: false).ToDictionary(p => p.PackageId, p => p.Title);
            string term = txt_offlineMemberSearch.Text.Trim();

            var filtered = string.IsNullOrEmpty(term)
                ? _offlineMembersCache.ToList()
                : _offlineMembersCache.Where(m =>
                    (m.Name ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (m.Phone ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    m.EnrollmentID.ToString().IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                  ).ToList();

            // Server sync filter
            string syncSel = cmbOfflineServerSync?.SelectedItem?.ToString() ?? "All Sync";
            if (syncSel == "Synced")
                filtered = filtered.Where(m => m.ServerSync).ToList();
            else if (syncSel == "Pending")
                filtered = filtered.Where(m => !m.ServerSync).ToList();

            // Package filter
            string pkgSel = cmbOfflinePackage?.SelectedItem?.ToString() ?? "All Packages";
            if (pkgSel != "All Packages")
            {
                filtered = filtered.Where(m =>
                {
                    if (string.IsNullOrWhiteSpace(m.SubscriptionJson)) return false;
                    try
                    {
                        var sub = JObject.Parse(m.SubscriptionJson);
                        int pkgId = sub["package_id"]?.ToObject<int>() ?? 0;
                        string pkgTitle = packagesById.TryGetValue(pkgId, out var t) ? t : null;
                        return string.Equals(pkgTitle, pkgSel, StringComparison.OrdinalIgnoreCase);
                    }
                    catch { return false; }
                }).ToList();
            }

            // Has Failure filter
            if (chkOfflineFailure?.Checked == true)
            {
                filtered = filtered.Where(m =>
                {
                    var log = _dbHelper.GetLatestOfflineLogForMember(m.ClientId);
                    return log != null && !string.IsNullOrWhiteSpace(log.FailureReason) && log.FailureReason != "-";
                }).ToList();
            }

            RenderOfflineMembersList(filtered, packagesById);
        }

        private void RenderOfflineMembersList(List<OfflineMember> members, Dictionary<int, string> packagesById)
        {
            lst_offlineMembers.Items.Clear();
            foreach (var m in members)
            {
                string packageTitle = "-";
                string periodicity = "-";
                try
                {
                    if (!string.IsNullOrWhiteSpace(m.SubscriptionJson))
                    {
                        var sub = JObject.Parse(m.SubscriptionJson);
                        int packageId = sub["package_id"]?.ToObject<int>() ?? 0;
                        packageTitle = packagesById.TryGetValue(packageId, out var title) ? title : $"#{packageId}";
                        periodicity = sub["periodicity"]?.ToString() ?? "-";
                    }
                }
                catch { /* leave defaults on parse failure */ }

                var offBioList = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(m.RFID)) offBioList.Add("RFID");
                if (!string.IsNullOrWhiteSpace(m.Picture)) offBioList.Add("Face");
                if (m.Fingerprint) offBioList.Add("Fingerprint");
                string offBioText = offBioList.Count > 0 ? string.Join(", ", offBioList) : "-";

                var item = new ListViewItem(m.EnrollmentID.ToString());
                item.SubItems.Add(m.Name ?? "-");
                item.SubItems.Add(m.Gender ?? "-");
                item.SubItems.Add(m.Phone ?? "-");
                item.SubItems.Add(packageTitle);
                item.SubItems.Add(periodicity);
                item.SubItems.Add(offBioText);
                item.SubItems.Add(m.ServerSync ? "Synced" : "Pending");

                string cleanupText = "-";
                if (m.ServerSync)
                {
                    int total = m.DeviceCleanupStatus?.Count ?? 0;
                    int done = m.DeviceCleanupStatus?.Count(kv => kv.Value) ?? 0;
                    cleanupText = total == 0 ? "Done" : $"{done}/{total} devices";
                }
                item.SubItems.Add(cleanupText);

                // Get failure reason from OfflineMemberLog table
                string offlineFailureReason = "-";
                try
                {
                    var logEntry = _dbHelper.GetLatestOfflineLogForMember(m.ClientId);
                    if (logEntry != null)
                    {
                        offlineFailureReason = string.IsNullOrWhiteSpace(logEntry.FailureReason) ? "-" : logEntry.FailureReason;
                    }
                }
                catch { }

                item.SubItems.Add(offlineFailureReason != "-" ? "[View]" : "-");
                item.Tag = m;
                lst_offlineMembers.Items.Add(item);
            }
        }

        private void LstOfflineMembers_ItemActivate(object sender, EventArgs e)
        {
            try
            {
                var listView = sender as ListView;
                if (listView?.SelectedItems.Count != 1) return;

                var item = listView.SelectedItems[0];
                var clickedPoint = listView.PointToClient(Control.MousePosition);
                var hitInfo = listView.HitTest(clickedPoint);
                if (hitInfo.Item == null || hitInfo.SubItem == null) return;

                int columnIndex = item.SubItems.IndexOf(hitInfo.SubItem);
                if (columnIndex != lst_offlineMembers.Columns.Count - 1) return; // Failure Reason is last column
                if (item.SubItems[columnIndex].Text != "[View]") return;

                if (!(item.Tag is OfflineMember om)) return;

                string fullReason = "-";
                try
                {
                    var log = _dbHelper.GetLatestOfflineLogForMember(om.ClientId);
                    if (log != null && !string.IsNullOrWhiteSpace((string)log.FailureReason))
                        fullReason = (string)log.FailureReason;
                }
                catch { }

                if (fullReason != "-")
                {
                    using (var dlg = new FailureReasonDetailForm(fullReason, null, om.EnrollmentID, om.Name, om.Picture, _dbHelper))
                        dlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[LstOfflineMembers_ItemActivate] Error: {ex.Message}");
            }
        }

        private void LoadAllMembersList()
        {
            try
            {
                _allMembersCache = _dbHelper.GetMembers() ?? new List<Member>();
                RenderAllMembersList(_allMembersCache);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DeviceManager] Failed to load members: {ex.Message}");
            }
        }

        private void FilterAllMembersList()
        {
            string term = txt_allMembersSearch.Text.Trim();

            var filtered = string.IsNullOrEmpty(term)
                ? _allMembersCache.ToList()
                : _allMembersCache.Where(m =>
                    (m.Name ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (m.ClientId ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (m.EnrollmentID?.ToString() ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    m.MemberID.ToString().IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                  ).ToList();

            // Status filter
            string statusSel = cmbAllMemberStatus?.SelectedItem?.ToString() ?? "All Status";
            if (statusSel != "All Status")
                filtered = filtered.Where(m => string.Equals(m.Status, statusSel, StringComparison.OrdinalIgnoreCase)).ToList();

            // Biometric filter
            string bioSel = cmbAllMemberBiometric?.SelectedItem?.ToString() ?? "All Biometric";
            if (bioSel == "Has Face")
                filtered = filtered.Where(m => !string.IsNullOrWhiteSpace(m.Picture)).ToList();
            else if (bioSel == "Has Fingerprint")
                filtered = filtered.Where(m => m.Fingerprint).ToList();
            else if (bioSel == "Has RFID")
                filtered = filtered.Where(m => !string.IsNullOrWhiteSpace(m.RFID)).ToList();
            else if (bioSel == "Missing Any")
                filtered = filtered.Where(m => string.IsNullOrWhiteSpace(m.Picture) || !m.Fingerprint || string.IsNullOrWhiteSpace(m.RFID)).ToList();

            // Expiry filter
            string expirySel = cmbAllMemberExpiry?.SelectedItem?.ToString() ?? "Any Expiry";
            if (expirySel != "Any Expiry")
            {
                var now = DateTime.Today;
                filtered = filtered.Where(m =>
                {
                    if (!DateTime.TryParse(m.PackageExpiry, out var expDate)) return false;
                    if (expirySel == "Expired") return expDate < now;
                    if (expirySel == "Expires in 7 days") return expDate >= now && expDate <= now.AddDays(7);
                    if (expirySel == "Expires in 30 days") return expDate >= now && expDate <= now.AddDays(30);
                    return true;
                }).ToList();
            }

            RenderAllMembersList(filtered);
        }

        private void RenderAllMembersList(List<Member> members)
        {
            var allDevices = AppSession.DeviceList ?? new List<Device>();
            int totalDevices = allDevices.Count;

            lst_allMembers.Items.Clear();
            foreach (var m in members)
            {
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

                var allBioList = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(m.RFID)) allBioList.Add("RFID");
                if (!string.IsNullOrWhiteSpace(m.Picture)) allBioList.Add("Face");
                if (m.Fingerprint) allBioList.Add("Fingerprint");
                string allBioText = allBioList.Count > 0 ? string.Join(", ", allBioList) : "-";

                var item = new ListViewItem(m.MemberID.ToString());
                item.SubItems.Add(string.IsNullOrWhiteSpace(m.Name) ? "-" : m.Name);
                item.SubItems.Add(m.EnrollmentID.HasValue && m.EnrollmentID.Value > 0 ? m.EnrollmentID.Value.ToString() : "-");
                item.SubItems.Add(string.IsNullOrWhiteSpace(m.Status) ? "-" : m.Status);
                item.SubItems.Add(allBioText);
                item.SubItems.Add(syncStatusText);
                item.SubItems.Add(string.IsNullOrWhiteSpace(m.PackageExpiry) ? "-" : m.PackageExpiry);
                item.SubItems.Add("Renew");
                item.Tag = m;
                lst_allMembers.Items.Add(item);
            }
        }

        private void LstAllMembers_ItemActivate(object sender, EventArgs e)
        {
            try
            {
                var listView = sender as ListView;
                if (listView?.SelectedItems.Count != 1) return;

                var item = listView.SelectedItems[0];
                var clickedPoint = listView.PointToClient(Control.MousePosition);
                var hitInfo = listView.HitTest(clickedPoint);
                if (hitInfo.Item == null || hitInfo.SubItem == null) return;

                int columnIndex = item.SubItems.IndexOf(hitInfo.SubItem);
                if (columnIndex != listView.Columns.Count - 1) return; // only the Action column opens Renew

                if (!(item.Tag is Member member)) return;

                if (!member.EnrollmentID.HasValue || member.EnrollmentID.Value <= 0)
                {
                    MessageBox.Show("This member has no EnrollmentID and cannot be renewed.", "Renew Membership", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var form = new RenewMembershipForm(member))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadAllMembersList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[LstAllMembers_ItemActivate] Error: {ex.Message}");
            }
        }

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


        //*************************************Setup ListView************************************//
        //*************************************Setup ListView************************************//
        private void SetupDeviceList()
        {
            // Clear only items — columns are stable, no need to redraw them
            listViewDevices.Items.Clear();

            if (listViewDevices.Columns.Count == 0)
            {
                listViewDevices.View = View.Details;
                listViewDevices.FullRowSelect = true;
                listViewDevices.GridLines = true;
                listViewDevices.MultiSelect = false;
                listViewDevices.Columns.Add("ID", 50);
                listViewDevices.Columns.Add("Machine No", 80);
                listViewDevices.Columns.Add("Attendance Type", 120);
                listViewDevices.Columns.Add("Device IP", 140);
                listViewDevices.Columns.Add("Port", 70);
                listViewDevices.Columns.Add("Connection Type", 120);
                listViewDevices.Columns.Add("Status", 90);
                listViewDevices.Columns.Add("Is Staff", 70);
                listViewDevices.Columns[listViewDevices.Columns.Count - 1].Width = -2;
            }

            // Add filter controls on first call
            if (cmbDeviceStatus == null && listViewDevices.Parent != null)
            {
                // Shift the ListView down to make room for the filter row above it
                listViewDevices.Location = new Point(listViewDevices.Left, 36);
                listViewDevices.Height = Math.Max(listViewDevices.Height - 36, 50);

                var devPanel = listViewDevices.Parent;
                cmbDeviceStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(9, 8), Width = 110 };
                cmbDeviceStatus.Items.AddRange(new object[] { "All Status", "Active", "Inactive" });
                cmbDeviceStatus.SelectedIndex = 0;
                cmbDeviceStatus.SelectedIndexChanged += (s, e) => SetupDeviceList();
                devPanel.Controls.Add(cmbDeviceStatus);

                chkDeviceIsStaff = new CheckBox { Text = "Staff Devices Only", Location = new Point(127, 10), AutoSize = true };
                chkDeviceIsStaff.CheckedChanged += (s, e) => SetupDeviceList();
                devPanel.Controls.Add(chkDeviceIsStaff);
            }

            // Load devices from AppSession
            var devices = AppSession.DeviceList ?? new List<Device>();
            var staffDevices = AppSession.StaffDeviceList ?? new List<Device>();

            var allDeviceItems = new List<ListViewItem>();

            // Populate normal devices
            foreach (var d in devices)
            {
                var item = new ListViewItem(d.DeviceId.ToString());
                item.SubItems.Add(d.MachineNo.ToString());
                item.SubItems.Add(d.AttendanceType ? "Check-in" : "Check-out");
                item.SubItems.Add(d.DeviceIP ?? "-");
                item.SubItems.Add(d.Port.ToString());
                item.SubItems.Add(d.ConnectionType ?? "-");
                item.SubItems.Add(d.IsActive ? "Active" : "Inactive");
                item.SubItems.Add("No"); // Is Staff
                item.Tag = d;
                allDeviceItems.Add(item);
            }

            // Populate staff devices
            foreach (var sd in staffDevices)
            {
                var item = new ListViewItem(sd.StaffDeviceId.ToString());
                item.SubItems.Add(sd.MachineNo.ToString());
                var attText = string.IsNullOrWhiteSpace(sd.StaffAttendanceType) ? "-" : sd.StaffAttendanceType;
                item.SubItems.Add(attText);
                item.SubItems.Add(sd.DeviceIP ?? "-");
                item.SubItems.Add(sd.Port.ToString());
                item.SubItems.Add(sd.ConnectionType ?? "-");
                item.SubItems.Add(sd.IsActive ? "Active" : "Inactive");
                item.SubItems.Add("Yes"); // Is Staff
                item.Tag = sd;
                allDeviceItems.Add(item);
            }

            // Apply filters
            string devStatusSel = cmbDeviceStatus?.SelectedItem?.ToString() ?? "All Status";
            if (devStatusSel != "All Status")
                allDeviceItems = allDeviceItems.Where(i => string.Equals(i.SubItems[6].Text, devStatusSel, StringComparison.OrdinalIgnoreCase)).ToList();

            if (chkDeviceIsStaff?.Checked == true)
                allDeviceItems = allDeviceItems.Where(i => i.SubItems[7].Text == "Yes").ToList();

            listViewDevices.Items.AddRange(allDeviceItems.ToArray());
        }

        //*************************************Event Listview Setup************************************//



        //*************************************Local user's data ************************************//

        //Abdul Hadi Code
        public void LoadLocalUserData()
        {
            list_LocalUser.Items.Clear();

            if (list_LocalUser.Columns.Count == 0)
            {
                list_LocalUser.View = View.Details;
                list_LocalUser.FullRowSelect = true;
                list_LocalUser.GridLines = true;
                list_LocalUser.Columns.Add("User ID", 100);
                list_LocalUser.Columns.Add("User Name", 150);
                list_LocalUser.Columns.Add("Role", 100);
                list_LocalUser.Columns.Add("Created", 150);
                list_LocalUser.Columns[list_LocalUser.Columns.Count - 1].Width = -2;
            }


            // Load users from AppSession cache
            List<LocalUser> users = AppSession.AllUsers ?? new List<LocalUser>();

            var visibleUsers = users.Where(u => !string.Equals(u.Role, "developer", StringComparison.OrdinalIgnoreCase)).ToList();

            // Add role filter control on first call
            if (cmbUserRole == null && list_LocalUser.Parent != null)
            {
                var userTab = list_LocalUser.Parent;
                cmbUserRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(9, 21), Width = 130 };
                cmbUserRole.Items.Add("All Roles");
                foreach (var role in visibleUsers.Select(u => u.Role).Distinct().OrderBy(r => r))
                    if (!string.IsNullOrWhiteSpace(role)) cmbUserRole.Items.Add(role);
                cmbUserRole.SelectedIndex = 0;
                cmbUserRole.SelectedIndexChanged += (s, e) => LoadLocalUserData();
                userTab.Controls.Add(cmbUserRole);
            }
            else if (cmbUserRole != null)
            {
                // Refresh roles list without resetting selection
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

            foreach (var user in visibleUsers)
            {
                ListViewItem item = new ListViewItem(user.LocalUserID.ToString());
                item.SubItems.Add(user.UserName);
                item.SubItems.Add(user.Role);
                item.SubItems.Add(user.CreatedAt ?? "-");

                list_LocalUser.Items.Add(item);
            }
        }

        private async void DeviceManager_Load(object sender, EventArgs e)
        {
            try
            {
                // Start the global loader
                Dashboard.GlobalStartLoader("Loading Device Manager...");

                SetupDeviceList(); // creates columns and sample rows

                // Show welcome screen — no panel selected on open
                HideAllPanels();
                SetActiveButton(null);
                SetHeader("Smart Access Manager", "Manage your biometric devices, users, staff, members, and system settings — all in one place.");
                pnl_welcome_header.BringToFront();
                pnl_welcome.Visible = true;

                // Wire hover effects for welcome tiles
                foreach (var tile in new[] { tile_device, tile_user, tile_staff, tile_member, tile_setup, tile_offlineMembers })
                {
                    var t = tile;
                    foreach (Control c in new Control[] { t }.Concat<Control>(t.Controls.Cast<Control>()))
                    {
                        c.MouseEnter += (s, ev) => t.BackColor = Color.FromArgb(235, 238, 255);
                        c.MouseLeave += (s, ev) => t.BackColor = Color.White;
                    }
                }

                pnl_welcome.Visible = true;
                pnl_welcome.BringToFront();

                btn_deleteUser.Enabled = false;
                btn_change_password.Enabled = false;
                btn_editUser.Enabled = false;
                listViewDevices.SelectedItems.Clear(); // Clear selection
                ClearDeviceFields(); // Clear device fields on load
                await UpdateSessionAsync(); // Update session with device data
                InvalidateMemberCache();  // force rebuild on next member list load
                InitializeMemberList();
                InitializeMemberListForStats();       // dynamic name suggestions
                InitializeMembersListView();  // members list view setup
                lst_members.ItemActivate += lst_members_ItemActivate;

                InitializePendingJobsListView();

                // Wire events
                flt_members.Click += flt_members_Click;
                txt_memb_name.TextChanged += txt_memb_name_TextChanged;

                InitializeStaffSuggestions();
                InitializeStaffsListView();
                btn_stafffilter.Click += btn_stafffilter_Click;
                txt_staff.TextChanged += txt_staff_TextChanged;
                lst_staffs.ItemActivate -= lst_staffs_ItemActivate;
                lst_staffs.ItemActivate += lst_staffs_ItemActivate;

                InitializeStaffAttendanceSuggestions();
                InitializeStaffAttendanceListView();
                btn_filterattendencestaff.Click += btn_filterattendencestaff_Click;
                txt_staffname.TextChanged += txt_staffname_TextChanged;
                date_staffattendence.ValueChanged += date_staffattendence_ValueChanged;

                InitializeStaffPendingJobsListView();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DeviceManager] Error during load: {ex.Message}");
            }
            finally
            {
                // Ensure selection stays visible on all ListViews when focus moves away
                foreach (var lv in new ListView[] {
                    listViewDevices, list_LocalUser, lst_members,
                    lst_staffs, lst_member, lst_manualdoor, lst_staffattendence,
                    lst_pendingjobs, lst_staffpendingjobs })
                {
                    lv.HideSelection = false;
                }

                _isInitialized = true;

                // Stop the global loader
                Dashboard.GlobalStopLoader();
            }
        }

        private void DeviceManager_closing(object sender, FormClosingEventArgs e)
        {
            // Stop cache manager auto-refresh
            _cacheManager?.StopAutoRefresh();

            // Dispose pagination controls - all attendance and management lists
            _membersPagination?.Dispose();
            _staffsPagination?.Dispose();
            _memberAttendancePagination?.Dispose();
            _staffAttendancePagination?.Dispose();
            _manualDoorPagination?.Dispose();
            _pendingJobsPagination?.Dispose();
            _staffPendingJobsPagination?.Dispose();
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
            if (pnl_staffsManagment.Visible && lst_members.Visible)
            {
                LoadMembersIntoListView(txt_memb_name.Text?.Trim());
            }
        }

        private void RefreshStaffList()
        {
            // Refresh staff list if currently visible
            if (pnl_staffsManagment.Visible && lst_staffs.Visible)
            {
                LoadStaffsIntoListView(txt_staff.Text?.Trim());
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
                MessageBox.Show("Error loading devices: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (pnl_offlineMembers != null) pnl_offlineMembers.Visible = false;
        }

        private void SetActiveButton(Label active)
        {
            foreach (var btn in new[] { btn_device_management, btn_userManagement,
                                        btn_staffmanagment, btn_attendencemanagment, btn_setupmanage, btn_offlineMembers })
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
                MessageBox.Show("Access denied. Only admin or developer can open Device Management.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            listViewDevices.SelectedItems.Clear();
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
                MessageBox.Show("Access denied. Only admin or developer can open Setup Management.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            foreach (var btn in new[] { nav_device, nav_user, nav_staff, nav_member, nav_setup, nav_offlineMembers })
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


        // On selection of row send deviceId to LoadSelectedDeviceDetails
        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count == 0) return;

            var selectedItem = listViewDevices.SelectedItems[0];
            if (!(selectedItem.Tag is Device selectedDevice)) return;

            // Determine type by available keys
            _selectedIsStaff = selectedDevice.StaffDeviceId > 0;
            _selectedDeviceId = selectedDevice.DeviceId;
            _selectedStaffDeviceId = selectedDevice.StaffDeviceId;

            if (_selectedIsStaff)
                LoadSelectedStaffDeviceDetails(_selectedStaffDeviceId);
            else
                LoadSelectedDeviceDetails(_selectedDeviceId);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txt_Device_id.BorderStyle = BorderStyle.FixedSingle;
        }

        //********************************* Toggle Device Details Fields EditMode ***************************************

        private void ToggleDeviceFieldsEditMode(bool enable)
        {
            var border    = enable ? BorderStyle.Fixed3D : BorderStyle.None;
            var backColor = enable ? EditableColor : ReadOnlyColor;

            foreach (var txt in new[] {
                txt_department, txt_Device_id, txt_finger_capacity, txt_RFID_capacity,
                txt_face_capacity, txt_last_synced, txt_device_status, txt_location,
                txt_timeZone, txt_port, txt_ip, txt_Serial, txt_password })
            {
                txt.ReadOnly    = !enable;
                txt.BorderStyle = border;
                txt.BackColor   = backColor;
            }

            cmb_device_type.Enabled    = enable;
            cmb_connectionType.Enabled = enable;
            cmb_machine_no.Enabled     = enable;
        }


        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count > 0)
            {
                ToggleDeviceFieldsEditMode(true); // Enable editing
                btnEdit.Visible = false;
                btnSave.Visible = true;
                btnSave.Location = btnEdit.Location;
                btn_remove_device.Visible = false;
                btnCancle.Visible = true;
                btnCancle.Location = btn_remove_device.Location;
                btn_format.Visible = true;

            }
            else
            {
                MessageBox.Show("Please select a device to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
            var device = _dbHelper.LoadStaffDevices().FirstOrDefault(d => d.StaffDeviceId == staffDeviceId);
            if (device == null) return;

            txt_Device_id.Text = device.StaffDeviceId.ToString();  // show StaffDeviceId
            txt_Serial.Text = device.DeviceSerial;
            txt_department.Text = device.Department;
            txt_face_capacity.Text = device.FaceCapacity.ToString();
            txt_finger_capacity.Text = device.FingerCapacity.ToString();
            txt_RFID_capacity.Text = device.RFIDCapacity.ToString();
            txt_ip.Text = device.DeviceIP;
            txt_port.Text = device.Port.ToString();
            txt_location.Text = device.Location;
            txt_timeZone.Text = device.TimeZone;
            txt_password.Text = device.Password;
            txt_device_status.Text = device.IsActive ? "Active" : "Inactive";
            txt_last_synced.Text = device.LastSyncTime ?? "Not Synced";
            cmb_device_type.SelectedItem = string.IsNullOrWhiteSpace(device.StaffAttendanceType) ? null : device.StaffAttendanceType;
            cmb_connectionType.SelectedItem = device.ConnectionType;
            cmb_machine_no.SelectedItem = device.MachineNo.ToString();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                MessageBox.Show("No user is currently logged in. Cannot proceed.");
                return;
            }

            // Inline password prompt
            Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Confirm Update - {currentUser} Login",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label label = new Label()
            {
                Left = 10,
                Top = 20,
                Width = 320,
                Text = "Enter your password or backup code to update the device:"
            };

            TextBox inputBox = new TextBox()
            {
                Left = 10,
                Top = 50,
                Width = 320,
                UseSystemPasswordChar = true
            };

            Button okButton = new Button()
            {
                Text = "OK",
                Left = 180,
                Width = 70,
                Top = 85,
                DialogResult = DialogResult.OK
            };

            Button cancelButton = new Button()
            {
                Text = "Cancel",
                Left = 260,
                Width = 70,
                Top = 85,
                DialogResult = DialogResult.Cancel
            };

            prompt.Controls.Add(label);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(okButton);
            prompt.Controls.Add(cancelButton);
            prompt.AcceptButton = okButton;
            prompt.CancelButton = cancelButton;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                string enteredPassword = inputBox.Text;

                if (_dbHelper.VerifyLoginUser(currentUser, enteredPassword))
                {
                    string deviceIp = txt_ip.Text.Trim();

                    if (!await IsValidIpAsync(deviceIp))
                    {
                        MessageBox.Show("Invalid IP address. Please check the IP and try again.", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int machineNoParsed = int.TryParse(cmb_machine_no.SelectedItem?.ToString(), out int machineParsed) ? machineParsed : 0;

                    // For normal devices, guard against IP/Machine conflicts (port uniqueness removed per helper doc)
                    if (!_selectedIsStaff)
                    {
                        string deviceExistsMessage = _dbHelper.IDCheckDeviceExists(deviceIp, machineNoParsed, _selectedDeviceId);
                        if (!string.IsNullOrEmpty(deviceExistsMessage))
                        {
                            MessageBox.Show(deviceExistsMessage, "Device Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    // Staff devices may have different constraints; skipping duplicate guard unless implemented

                    ToggleDeviceFieldsEditMode(false);
                    btnEdit.Visible = true;
                    btnSave.Visible = false;
                    btnCancle.Visible = false;
                    btn_remove_device.Visible = true;
                    btn_format.Visible = false;

                    // Build common model from fields
                    var deviceModel = new Device
                    {
                        Department = txt_department.Text,
                        DeviceSerial = txt_Serial.Text,
                        DeviceIP = deviceIp,
                        Port = int.TryParse(txt_port.Text, out var p) ? p : 0,
                        TimeZone = txt_timeZone.Text,
                        Location = txt_location.Text,
                        Password = txt_password.Text,
                        ConnectionType = cmb_connectionType.SelectedItem?.ToString(),
                        MachineNo = machineNoParsed,
                        IsActive = txt_device_status.Text == "Active",
                        LastSyncTime = txt_last_synced.Text,
                        FaceCapacity = int.TryParse(txt_face_capacity.Text, out var fc) ? fc : 0,
                        RFIDCapacity = int.TryParse(txt_RFID_capacity.Text, out var rc) ? rc : 0,
                        FingerCapacity = int.TryParse(txt_finger_capacity.Text, out var fpc) ? fpc : 0
                    };

                    // Attendance type mapping
                    var attendanceText = cmb_device_type.SelectedItem?.ToString() ?? string.Empty;

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
                        listViewDevices.SelectedItems.Clear();
                        await UpdateSessionAsync(); // Update session with device data
                InvalidateMemberCache();

                        // Notify dashboard to refresh
                        NotifyDashboardRefresh();

                        MessageBox.Show("Device updated successfully.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        aiChatLogger.AddErrorMessage("Device update failed due to duplicate entry. Please check your settings.");
                        MessageBox.Show(ex.Message, "Update Failed - Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Authentication failed. Device update was cancelled.");
                }
            }
            else
            {
                MessageBox.Show("Device update cancelled.");
                listViewDevices.SelectedItems.Clear();
                ClearDeviceFields();
            }
        }




        private void btnCancle_Click(object sender, EventArgs e)
        {
            listViewDevices.SelectedItems.Clear();
            ClearDeviceFields(); // Clear the fields
            // Revert UI to non-editable mode
            ToggleDeviceFieldsEditMode(false);

            // Hide Save/Cancel, show Edit
            btnEdit.Visible = true;
            btn_remove_device.Visible = true;
            btnSave.Visible = false;
            btnCancle.Visible = false;
            btn_format.Visible = true;



            // Optionally reset field values (if you saved originals before editing)
            // ResetDeviceFormValues(originalDeviceData);
        }
        //**************************************************************



        private void LoadSelectedDeviceDetails(int deviceId)
        {
            // Fetch device details from the database based on the selected deviceId
            var device = _dbHelper.LoadDevices().FirstOrDefault(d => d.DeviceId == deviceId);

            if (device != null)
            {
                txt_Device_id.Text = device.DeviceId.ToString();
                txt_Serial.Text = device.DeviceSerial;
                txt_department.Text = device.Department;
                txt_face_capacity.Text = device.FaceCapacity.ToString();
                txt_finger_capacity.Text = device.FingerCapacity.ToString();
                txt_RFID_capacity.Text = device.RFIDCapacity.ToString();
                txt_ip.Text = device.DeviceIP;
                txt_port.Text = device.Port.ToString();
                txt_location.Text = device.Location;
                txt_timeZone.Text = device.TimeZone;
                txt_password.Text = device.Password;
                txt_device_status.Text = device.IsActive ? "Active" : "Inactive";
                txt_last_synced.Text = device.LastSyncTime ?? "Not Synced";
                cmb_device_type.SelectedItem = device.AttendanceType ? "Check-in" : "Check-out";
                cmb_connectionType.SelectedItem = device.ConnectionType;
                cmb_machine_no.SelectedItem = device.MachineNo.ToString();

            }
        }

        public void ClearDeviceFields()
        {
            txt_Device_id.Clear();
            txt_Serial.Clear();
            txt_department.Clear();
            txt_face_capacity.Clear();
            txt_finger_capacity.Clear();
            txt_RFID_capacity.Clear();
            txt_ip.Clear();
            txt_port.Clear();
            txt_location.Clear();
            txt_timeZone.Clear();
            txt_password.Clear();
            txt_device_status.Clear();
            txt_last_synced.Clear();
            cmb_device_type.SelectedIndex = -1;
            cmb_connectionType.SelectedIndex = -1;
            cmb_machine_no.SelectedIndex = -1;
        }


        private void btn_add_device_Click(object sender, EventArgs e)
        {
            ClearDeviceFields();
            listViewDevices.SelectedItems.Clear();
            AddNewDevice adddevice = new AddNewDevice();
            adddevice.ShowDialog();
            SetupDeviceList(); // Refresh the device list after adding a new device
        }

        private async void btn_remove_device_Click(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to remove.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txt_Device_id.Text, out int deviceId))
            {
                MessageBox.Show("Invalid Device ID. Please enter a valid number.");
                return;
            }

            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                MessageBox.Show("No user is currently logged in. Cannot proceed.");
                return;
            }

            // Inline prompt to ask for the password or backup code
            Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Confirm Deletion - {currentUser} Login",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label label = new Label()
            {
                Left = 10,
                Top = 20,
                Width = 320,
                Text = $"Enter your password or backup code to delete Device ID {deviceId}:"
            };

            TextBox inputBox = new TextBox()
            {
                Left = 10,
                Top = 50,
                Width = 320,
                UseSystemPasswordChar = true // This ensures that the password is masked with asterisks (*)
            };

            Button okButton = new Button()
            {
                Text = "OK",
                Left = 180,
                Width = 70,
                Top = 85,
                DialogResult = DialogResult.OK
            };

            Button cancelButton = new Button()
            {
                Text = "Cancel",
                Left = 260,
                Width = 70,
                Top = 85,
                DialogResult = DialogResult.Cancel
            };

            prompt.Controls.Add(label);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(okButton);
            prompt.Controls.Add(cancelButton);
            prompt.AcceptButton = okButton;
            prompt.CancelButton = cancelButton;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                string enteredPassword = inputBox.Text;

                if (!await IsValidIpAsync(txt_ip.Text.Trim()))
                {
                    MessageBox.Show("The device is not reachable. Please check the IP address and try again.", "Ping Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_dbHelper.VerifyLoginUser(currentUser, enteredPassword))
                {
                    if (_selectedIsStaff)
                    {
                        aiChatLogger.AddDeviceMessage($"Preparing to remove staff device from {txt_location.Text}...");
                        aiChatLogger.AddInfoMessage("Clearing all enrollment data from the staff device first...");

                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId(_selectedStaffDeviceId);
                        _dbHelper.DeleteStaffDevice(_selectedStaffDeviceId);
                    }
                    else
                    {
                        aiChatLogger.AddDeviceMessage($"Preparing to remove device from {txt_location.Text}...");
                        aiChatLogger.AddInfoMessage("Clearing all enrollment data from the device first...");

                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByDeviceId(_selectedDeviceId);
                        _dbHelper.DeleteDevice(_selectedDeviceId);
                    }

                    SetupDeviceList(); // Refresh the device list after deletion
                    ClearDeviceFields(); // Clear the fields after deletion
                    listViewDevices.SelectedItems.Clear();
                    await UpdateSessionAsync(); // Update session with device data
                InvalidateMemberCache();

                    // Notify dashboard to refresh
                    NotifyDashboardRefresh();

                    // Add AI messages after successful removal
                    aiChatLogger.AddSuccessMessage($"Device from {txt_location.Text} has been successfully removed!");
                    aiChatLogger.AddInfoMessage("Don't worry, I'm still here to help with your remaining devices.");
                    aiChatLogger.AddDeviceMessage("Device network updated. All enrollment data has been cleared.");

                    MessageBox.Show("Device deleted successfully.");
                }
                else
                {
                    MessageBox.Show("Authentication failed. Device was not deleted.");
                }
            }
            else
            {
                ClearDeviceFields(); // Clear the fields if the user cancels
                listViewDevices.SelectedItems.Clear();
                    MessageBox.Show("Device deletion cancelled.");
                    }
                }


                private async void btnSaveAPI_Click_1(object sender, EventArgs e)
        {
            string apiKey = txtAPIkey.Text.Trim();

            // Step 1: Basic validation
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show("Please enter a valid API Key.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Validate format: Must contain colon (GYMID:SECRETKEY)
            if (!apiKey.Contains(":") || apiKey.Split(':').Length != 2)
            {
                MessageBox.Show("API Key format is invalid. Expected format: gymId:secretKey", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Split on the first colon only
                int sepIndex = apiKey.IndexOf(':');
                if (sepIndex <= 0 || sepIndex == apiKey.Length - 1)
                {
                    MessageBox.Show("Invalid API key format. Expected '<gymId>:<jwt>'.", "Invalid Format",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string gymId = apiKey.Substring(0, sepIndex).Trim();
                string jwt = apiKey.Substring(sepIndex + 1).Trim();

                if (string.IsNullOrWhiteSpace(gymId) || string.IsNullOrWhiteSpace(jwt))
                {
                    MessageBox.Show("Gym ID or token is empty after split.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Persist all parts
                _dbHelper.SaveSetting("APIKey", apiKey);
                _dbHelper.SaveSetting("GymId", gymId);
                _dbHelper.SaveSetting("GWTToken", jwt);

                LogHelper.Log($"[API Key Saved] GymId='{gymId}', JWT length={jwt.Length}.");
                MessageBox.Show("API key saved.\nGym ID and GWT token stored successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[btnSaveAPI_Click_1][ERROR] {ex.Message}");
                MessageBox.Show("Failed to save API key. See logs for details.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btn_addUser_Click(object sender, EventArgs e)
        {
            if (AppSession.UserRole == "user")
            {
                MessageBox.Show("You do not have permission to add users.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private void list_LocalUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list_LocalUser.SelectedItems.Count > 0)
            {
                var selectedItem = list_LocalUser.SelectedItems[0];

                if (int.TryParse(selectedItem.Text, out int userId))
                {
                    selectedUserId = userId;
                    selectedUsername = selectedItem.SubItems[1].Text;
                    selectedRole = selectedItem.SubItems[2].Text;

                    btn_deleteUser.Enabled = true;
                    btn_change_password.Enabled = true;
                    btn_editUser.Enabled = true;
                }
            }
            else
            {
                selectedUserId = null;
                selectedUsername = null;
                selectedRole = null;
                btn_deleteUser.Enabled = false;
                btn_change_password.Enabled = false;
                btn_editUser.Enabled = false;
            }
        }


        //Abdul Hadi Code
        //Event to delete the selected user
        private void btn_deleteUser_Click(object sender, EventArgs e)
        {
            if (!selectedUserId.HasValue)
            {
                MessageBox.Show("Please select a user to delete.", "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Nobody can delete themselves
            if (selectedUserId.Value == AppSession.LoggedInUserID)
            {
                MessageBox.Show("You cannot delete your own account.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentRole = AppSession.UserRole;

            // user cannot delete anyone
            if (currentRole == "user")
            {
                MessageBox.Show("You do not have permission to delete users.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // admin cannot delete developer or another admin
            if (currentRole == "admin" && (selectedRole == "developer" || selectedRole == "admin"))
            {
                MessageBox.Show("You do not have permission to delete this user.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
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
                MessageBox.Show("Please select a user to change password.");
                return;
            }

            string currentRole = AppSession.UserRole;

            // user can only change their own password
            if (currentRole == "user" && selectedUserId.Value != AppSession.LoggedInUserID)
            {
                MessageBox.Show("You can only change your own password.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // admin cannot change password of developer or another admin (unless own)
            if (currentRole == "admin" && selectedUserId.Value != AppSession.LoggedInUserID
                && (selectedRole == "developer" || selectedRole == "admin"))
            {
                MessageBox.Show("You do not have permission to change this user's password.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedUserId.HasValue)
            {
                using (Form prompt = new Form())
                {
                    prompt.Width = 400;
                    prompt.Height = 220;
                    prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                    prompt.Text = "Change Password";
                    prompt.StartPosition = FormStartPosition.CenterScreen;

                    Label textLabel1 = new Label() { Left = 20, Top = 20, Text = "Enter new password (min 8 characters):", Width = 340 };
                    TextBox textBox1 = new TextBox() { Left = 20, Top = 50, Width = 340, PasswordChar = '*' };

                    Label textLabel2 = new Label() { Left = 20, Top = 85, Text = "Confirm new password:", Width = 340 };
                    TextBox textBox2 = new TextBox() { Left = 20, Top = 110, Width = 340, PasswordChar = '*' };

                    Button confirmation = new Button() { Text = "OK", Left = 270, Width = 90, Top = 145, DialogResult = DialogResult.OK };
                    confirmation.Click += (sender2, e2) => { prompt.Close(); };

                    prompt.Controls.Add(textLabel1);
                    prompt.Controls.Add(textBox1);
                    prompt.Controls.Add(textLabel2);
                    prompt.Controls.Add(textBox2);
                    prompt.Controls.Add(confirmation);
                    prompt.AcceptButton = confirmation;

                    if (prompt.ShowDialog() == DialogResult.OK)
                    {
                        string newPassword = textBox1.Text;
                        string confirmPassword = textBox2.Text;

                        if (newPassword.Length < 8)
                        {
                            MessageBox.Show("Password must be at least 8 characters long.");
                            return;
                        }

                        if (newPassword != confirmPassword)
                        {
                            MessageBox.Show("Passwords do not match. Please try again.");
                            return;
                        }

                        DialogResult confirm = MessageBox.Show("Are you sure you want to change the password?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (confirm == DialogResult.Yes)
                        {
                            _dbHelper.UpdateUserPassword(selectedUserId.Value, newPassword);
                            MessageBox.Show("Password changed successfully.");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to change password.");
            }
        }



        //Abdul Hadi Code
        //Event to edit the selected user
        private void btn_editUser_Click_1(object sender, EventArgs e)
        {
            if (!selectedUserId.HasValue || string.IsNullOrEmpty(selectedUsername) || string.IsNullOrEmpty(selectedRole))
            {
                MessageBox.Show("Please select a user to edit.");
                return;
            }

            string currentRole = AppSession.UserRole;

            // user can only edit themselves
            if (currentRole == "user" && selectedUserId.Value != AppSession.LoggedInUserID)
            {
                MessageBox.Show("You can only edit your own account.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // admin cannot edit developer or another admin (unless own)
            if (currentRole == "admin" && selectedUserId.Value != AppSession.LoggedInUserID
                && (selectedRole == "developer" || selectedRole == "admin"))
            {
                MessageBox.Show("You do not have permission to edit this user.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            // Copy the items to a local list on the UI thread
            var items = new List<ListViewItem>();
            foreach (ListViewItem item in listViewDevices.Items)
                items.Add(item);

            await Task.Run(async () =>
            {
                foreach (var item in items)
                {
                    try
                    {
                        string ip = item.SubItems[3].Text;
                        int port = int.Parse(item.SubItems[4].Text);
                        int machineNo = int.Parse(item.SubItems[1].Text);
                        string password = "0"; // Or fetch from DB/device

                        string serial = await DeviceService.GetSerial(ip, port, machineNo, password);

                        // Update UI on the UI thread
                        this.Invoke(new Action(() =>
                        {
                            item.SubItems[6].Text = "Connected";
                            item.SubItems[6].ForeColor = Color.Green;
                        }));
                    }
                    catch
                    {
                        // Handle error, optionally update UI
                    }
                }
            });
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
            if (listViewDevices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to format.", "No Device Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Resolve the selected device object from Tag
            var selectedItem = listViewDevices.SelectedItems[0];
            if (!(selectedItem.Tag is Device selectedDevice))
            {
                MessageBox.Show("Selected row is invalid.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Auth
            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                MessageBox.Show("No user is currently logged in. Cannot proceed.");
                return;
            }

            var confirmResult = MessageBox.Show(
                "WARNING: This will delete ALL user data (Fingerprints, Face, RFID, Passwords) from the terminal via Cloud WebSocket.\n\n" +
                "This action cannot be undone. Are you sure you want to continue?",
                "Format Device Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes) return;

            // Authentication Prompt
            using (Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Confirm Format - {currentUser} Login",
                StartPosition = FormStartPosition.CenterScreen
            })
            {
                var label = new Label { Left = 10, Top = 20, Width = 320, Text = "Enter your password to format the device:" };
                var inputBox = new TextBox { Left = 10, Top = 50, Width = 320, UseSystemPasswordChar = true };
                var okButton = new Button { Text = "Format Device", Left = 150, Width = 100, Top = 85, DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Cancel", Left = 260, Width = 70, Top = 85, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(label); prompt.Controls.Add(inputBox); prompt.Controls.Add(okButton); prompt.Controls.Add(cancelButton);
                prompt.AcceptButton = okButton; prompt.CancelButton = cancelButton;

                if (prompt.ShowDialog() != DialogResult.OK) return;

                if (!_dbHelper.VerifyLoginUser(currentUser, inputBox.Text))
                {
                    MessageBox.Show("Authentication failed. Device formatting was cancelled.");
                    return;
                }
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
                    MessageBox.Show("Device Serial Number not found. Ensure the device is registered.", "Device Unreachable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    MessageBox.Show("All user data has been cleared from terminal and database Successfully.",
                        "Format Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to reach device via WebSocket. Please ensure the terminal is online.",
                        "Format Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while formatting the device: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                SetupDeviceList();
                ClearDeviceFields();
                listViewDevices.SelectedItems.Clear();
                await UpdateSessionAsync();
                NotifyDashboardRefresh();
            }
        }

        /////////////////////////MEMBER MANAGMENT///////////////////////////////

        // Call this in DeviceManager_Load after InitializeMemberList()
        private void InitializeMembersListView()
        {
            lst_members.Items.Clear();

            if (lst_members.Columns.Count == 0)
            {
                lst_members.View = View.Details;
                lst_members.FullRowSelect = true;
                lst_members.GridLines = true;
                lst_members.Columns.Add("Member ID", 80);
                lst_members.Columns.Add("Name", 140);
                lst_members.Columns.Add("Enrollment No", 100);
                lst_members.Columns.Add("Status", 70);
                lst_members.Columns.Add("Biometric", 130);
                lst_members.Columns.Add("Sync Status", 110);
                lst_members.Columns.Add("Package Expiry", 110);
                lst_members.Columns.Add("Failure Reason", 85);
                lst_members.Columns.Add("Actions", 80);
            }

            // Add second filter row if not already added
            if (cmbMemberStatus == null && tab_members != null)
            {
                int x = 290;
                cmbMemberStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(x, 11), Width = 110 };
                cmbMemberStatus.Items.AddRange(new object[] { "All Status", "active", "inactive", "expired" });
                cmbMemberStatus.SelectedIndex = 0;
                cmbMemberStatus.SelectedIndexChanged += (s, e) => LoadMembersIntoListView(txt_memb_name.Text?.Trim());
                tab_members.Controls.Add(cmbMemberStatus);
                x += 118;

                cmbMemberSync = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(x, 11), Width = 130 };
                cmbMemberSync.Items.AddRange(new object[] { "All Sync", "Fully Synced", "Partially Synced", "Not Synced" });
                cmbMemberSync.SelectedIndex = 0;
                cmbMemberSync.SelectedIndexChanged += (s, e) => LoadMembersIntoListView(txt_memb_name.Text?.Trim());
                tab_members.Controls.Add(cmbMemberSync);
                x += 138;

                chkMemberFailure = new CheckBox { Text = "Has Failure Only", Location = new Point(x, 13), AutoSize = true };
                chkMemberFailure.CheckedChanged += (s, e) => LoadMembersIntoListView(txt_memb_name.Text?.Trim());
                tab_members.Controls.Add(chkMemberFailure);
            }

            LoadMembersIntoListView(null);
        }

        // Reuse your function as-is for txt_memb_name dynamic suggestions:
        // Map txt_memb_name to txt_membername internally (if different control names exist)
        private void InitializeMemberListForStats()
        {
            // Load all members from AppSession
            List<Member> members = AppSession.AllMembers ?? new List<Member>();
            LogHelper.Log($"Initialized statistics suggestions with {members.Count} members from session.");

            // Configure the AutoCompleteSource and AutoCompleteMode
            txt_memb_name.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txt_memb_name.AutoCompleteSource = AutoCompleteSource.CustomSource;

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
            txt_memb_name.AutoCompleteCustomSource = memberCollection;
        }

        // Button click handler for __flt_members__ to filter by name
        private void flt_members_Click(object sender, EventArgs e)
        {
            string nameFilter = txt_memb_name.Text?.Trim();
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
                // Build cache once; subsequent calls with a filter just query the cache
                if (_cachedMemberItems == null)
                    _cachedMemberItems = BuildAllMemberItems();

                List<ListViewItem> displayItems;
                if (string.IsNullOrWhiteSpace(nameFilter))
                {
                    displayItems = _cachedMemberItems.ToList();
                }
                else
                {
                    displayItems = _cachedMemberItems
                        .Where(item => item.Tag is Member m &&
                                       !string.IsNullOrWhiteSpace(m.Name) &&
                                       m.Name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                // Status filter
                if (cmbMemberStatus != null && cmbMemberStatus.SelectedIndex > 0)
                {
                    string statusFilter = cmbMemberStatus.SelectedItem.ToString();
                    displayItems = displayItems.Where(item => item.Tag is Member m &&
                        (m.Status ?? "").Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Sync filter (SyncStatus is subitem index 5)
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

                // Has Failure filter (Failure Reason is subitem index 7)
                if (chkMemberFailure != null && chkMemberFailure.Checked)
                {
                    displayItems = displayItems.Where(item =>
                        item.SubItems.Count > 7 && item.SubItems[7].Text == "[View]").ToList();
                }

                if (_membersPagination == null && lst_members.Parent != null)
                    _membersPagination = new ListViewPagination(lst_members, lst_members.Parent);

                if (_membersPagination != null)
                {
                    _membersPagination.SetItems(displayItems);
                }
                else
                {
                    lst_members.BeginUpdate();
                    lst_members.Items.Clear();
                    lst_members.Items.AddRange(displayItems.ToArray());
                    lst_members.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load members: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txt_memb_name_TextChanged(object sender, EventArgs e)
        {
            if (!_isInitialized) return;
            var filter = txt_memb_name.Text?.Trim();
            LoadMembersIntoListView(string.IsNullOrEmpty(filter) ? null : filter);
        }

        /////////////////////////PENDING JOBS///////////////////////////////

        // Same "Pending: N" figure shown on the Dashboard — driven by the Log/StaffLog tables,
        // where a member/staff+device row has at least one biometric whose SyncStatus is still false.
        // Data comes straight from AppSession (kept current by CacheManager); no manual refresh needed —
        // OnCacheUpdated repaints these lists whenever CacheManager's periodic refresh picks up a change.
        private void InitializePendingJobsListView()
        {
            lst_pendingjobs.Items.Clear();

            if (lst_pendingjobs.Columns.Count == 0)
            {
                lst_pendingjobs.View = View.Details;
                lst_pendingjobs.FullRowSelect = true;
                lst_pendingjobs.GridLines = true;
                lst_pendingjobs.Columns.Add("Member ID", 90);
                lst_pendingjobs.Columns.Add("Name", 170);
                lst_pendingjobs.Columns.Add("Enrollment No", 110);
                lst_pendingjobs.Columns.Add("Device", 170);
                lst_pendingjobs.Columns.Add("Pending For", 190);
                lst_pendingjobs.Columns.Add("Since", 130);
                lst_pendingjobs.Columns[lst_pendingjobs.Columns.Count - 1].Width = -2;
            }

            // Add filter controls — anchored to the right so they don't overlap lbl_pendingjobs_info
            var pendingTab = lst_pendingjobs.Parent;
            if (pendingTab != null && cmbPendingDevice == null)
            {
                // Position from the right edge; tab inner width ~857
                cmbPendingFor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(727, 11), Width = 120, Anchor = AnchorStyles.Right | AnchorStyles.Top };
                cmbPendingFor.Items.AddRange(new object[] { "All Types", "Fingerprint", "Face", "RFID" });
                cmbPendingFor.SelectedIndex = 0;
                cmbPendingFor.SelectedIndexChanged += (s, e) => RefreshMemberPendingJobsList();
                pendingTab.Controls.Add(cmbPendingFor);

                cmbPendingDevice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(569, 11), Width = 150, Anchor = AnchorStyles.Right | AnchorStyles.Top };
                cmbPendingDevice.Items.Add("All Devices");
                foreach (var d in AppSession.DeviceList ?? new List<Device>())
                    cmbPendingDevice.Items.Add(!string.IsNullOrWhiteSpace(d.Location) ? d.Location : d.DeviceSerial ?? $"Device {d.DeviceId}");
                cmbPendingDevice.SelectedIndex = 0;
                cmbPendingDevice.SelectedIndexChanged += (s, e) => RefreshMemberPendingJobsList();
                pendingTab.Controls.Add(cmbPendingDevice);
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

                var displayItems = new List<ListViewItem>(unsyncedEntries.Count);

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
                    if (string.IsNullOrEmpty(pendingFor)) continue; // nothing actually pending on this row

                    var item = new ListViewItem(entry.MemberID.ToString());
                    item.SubItems.Add(name);
                    item.SubItems.Add(entry.EnrollmentID > 0 ? entry.EnrollmentID.ToString() : "-");
                    item.SubItems.Add(deviceName);
                    item.SubItems.Add(pendingFor);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(entry.UpdatedAt) ? "-" : entry.UpdatedAt);
                    item.Tag = entry;
                    displayItems.Add(item);
                }

                // Apply filters
                string devSel = cmbPendingDevice?.SelectedItem?.ToString() ?? "All Devices";
                if (devSel != "All Devices")
                    displayItems = displayItems.Where(i => string.Equals(i.SubItems[3].Text, devSel, StringComparison.OrdinalIgnoreCase)).ToList();

                string typeSel = cmbPendingFor?.SelectedItem?.ToString() ?? "All Types";
                if (typeSel != "All Types")
                    displayItems = displayItems.Where(i => i.SubItems[4].Text.IndexOf(typeSel, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                if (_pendingJobsPagination == null && lst_pendingjobs.Parent != null)
                    _pendingJobsPagination = new ListViewPagination(lst_pendingjobs, lst_pendingjobs.Parent);

                if (_pendingJobsPagination != null)
                {
                    _pendingJobsPagination.SetItems(displayItems);
                }
                else
                {
                    lst_pendingjobs.BeginUpdate();
                    lst_pendingjobs.Items.Clear();
                    lst_pendingjobs.Items.AddRange(displayItems.ToArray());
                    lst_pendingjobs.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RefreshMemberPendingJobsList] Error: {ex.Message}");
            }
        }

        private void InitializeStaffPendingJobsListView()
        {
            lst_staffpendingjobs.Items.Clear();

            if (lst_staffpendingjobs.Columns.Count == 0)
            {
                lst_staffpendingjobs.View = View.Details;
                lst_staffpendingjobs.FullRowSelect = true;
                lst_staffpendingjobs.GridLines = true;
                lst_staffpendingjobs.Columns.Add("Staff ID", 90);
                lst_staffpendingjobs.Columns.Add("Name", 170);
                lst_staffpendingjobs.Columns.Add("Enrollment No", 110);
                lst_staffpendingjobs.Columns.Add("Device", 170);
                lst_staffpendingjobs.Columns.Add("Pending For", 190);
                lst_staffpendingjobs.Columns.Add("Since", 130);
                lst_staffpendingjobs.Columns[lst_staffpendingjobs.Columns.Count - 1].Width = -2;
            }

            // Add filter controls — anchored to the right so they don't overlap lbl_staffpendingjobs_info
            var staffPendingTab = lst_staffpendingjobs.Parent;
            if (staffPendingTab != null && cmbStaffPendingDevice == null)
            {
                cmbStaffPendingFor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(727, 11), Width = 120, Anchor = AnchorStyles.Right | AnchorStyles.Top };
                cmbStaffPendingFor.Items.AddRange(new object[] { "All Types", "Fingerprint", "Face", "RFID" });
                cmbStaffPendingFor.SelectedIndex = 0;
                cmbStaffPendingFor.SelectedIndexChanged += (s, e) => RefreshStaffPendingJobsList();
                staffPendingTab.Controls.Add(cmbStaffPendingFor);

                cmbStaffPendingDevice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(569, 11), Width = 150, Anchor = AnchorStyles.Right | AnchorStyles.Top };
                cmbStaffPendingDevice.Items.Add("All Devices");
                foreach (var d in AppSession.StaffDeviceList ?? new List<Device>())
                    cmbStaffPendingDevice.Items.Add(!string.IsNullOrWhiteSpace(d.Location) ? d.Location : d.DeviceSerial ?? $"Device {d.DeviceId}");
                cmbStaffPendingDevice.SelectedIndex = 0;
                cmbStaffPendingDevice.SelectedIndexChanged += (s, e) => RefreshStaffPendingJobsList();
                staffPendingTab.Controls.Add(cmbStaffPendingDevice);
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

                var displayItems = new List<ListViewItem>(unsyncedEntries.Count);

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
                    if (string.IsNullOrEmpty(pendingFor)) continue; // nothing actually pending on this row

                    var item = new ListViewItem(entry.StaffID ?? "-");
                    item.SubItems.Add(name);
                    item.SubItems.Add(entry.EnrollmentID.HasValue && entry.EnrollmentID.Value > 0 ? entry.EnrollmentID.ToString() : "-");
                    item.SubItems.Add(deviceName);
                    item.SubItems.Add(pendingFor);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(entry.UpdatedAt) ? "-" : entry.UpdatedAt);
                    item.Tag = entry;
                    displayItems.Add(item);
                }

                // Apply filters
                string sDevSel = cmbStaffPendingDevice?.SelectedItem?.ToString() ?? "All Devices";
                if (sDevSel != "All Devices")
                    displayItems = displayItems.Where(i => string.Equals(i.SubItems[3].Text, sDevSel, StringComparison.OrdinalIgnoreCase)).ToList();

                string sTypeSel = cmbStaffPendingFor?.SelectedItem?.ToString() ?? "All Types";
                if (sTypeSel != "All Types")
                    displayItems = displayItems.Where(i => i.SubItems[4].Text.IndexOf(sTypeSel, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                if (_staffPendingJobsPagination == null && lst_staffpendingjobs.Parent != null)
                    _staffPendingJobsPagination = new ListViewPagination(lst_staffpendingjobs, lst_staffpendingjobs.Parent);

                if (_staffPendingJobsPagination != null)
                {
                    _staffPendingJobsPagination.SetItems(displayItems);
                }
                else
                {
                    lst_staffpendingjobs.BeginUpdate();
                    lst_staffpendingjobs.Items.Clear();
                    lst_staffpendingjobs.Items.AddRange(displayItems.ToArray());
                    lst_staffpendingjobs.EndUpdate();
                }
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
            txt_membername.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txt_membername.AutoCompleteSource = AutoCompleteSource.CustomSource;

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
            txt_membername.AutoCompleteCustomSource = memberCollection;
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
            lst_member.Items.Clear();
            lst_member.View = View.Details;
            lst_member.FullRowSelect = true;
            lst_member.GridLines = true;

            lst_member.Columns.Clear();
            lst_member.Columns.Add("Log ID", 70);
            lst_member.Columns.Add("Member ID", 80);
            lst_member.Columns.Add("Member Name", 120);
            lst_member.Columns.Add("Check-In Time", 150);
            lst_member.Columns.Add("Check-Out Time", 150);
            lst_member.Columns[lst_member.Columns.Count - 1].Width = -2;

            // Get filter criteria
            string memberNameFilter = txt_membername.Text.Trim();
            DateTime? selectedDate = date_member.Checked ? date_member.Value.Date : (DateTime?)null;

            var attendanceLogs = AppSession.AllAttendanceLogs ?? new List<dynamic>();
            var members = AppSession.AllMembers ?? new List<Member>();

            // Apply filters
            var filteredLogs = attendanceLogs.Where(log =>
            {
                // Filter by member name using AppSession cache
                var member = members.FirstOrDefault(m => m.MemberID == log.MemberID);
                string memberName = member?.Name ?? "-";

                if (!string.IsNullOrEmpty(memberNameFilter))
                {
                    if (string.IsNullOrEmpty(memberName) || !memberName.Contains(memberNameFilter))
                    {
                        return false;
                    }
                }

                // Filter by date
                if (selectedDate.HasValue)
                {
                    DateTime? checkInTime = ParseDateTime(log.CheckInTime);
                    DateTime? checkOutTime = ParseDateTime(log.CheckOutTime);

                    if ((!checkInTime.HasValue || checkInTime.Value.Date != selectedDate.Value) &&
                        (!checkOutTime.HasValue || checkOutTime.Value.Date != selectedDate.Value))
                    {
                        return false;
                    }
                }

                return true;
            }).ToList();

            var allItems = new List<ListViewItem>();

            foreach (var log in filteredLogs)
            {
                var item = new ListViewItem(log.LogID.ToString());
                item.SubItems.Add(log.MemberID.ToString());

                // Fetch member name from AppSession cache
                var member = members.FirstOrDefault(m => m.MemberID == log.MemberID);
                item.SubItems.Add(member?.Name ?? "-");

                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckInTime) ? "-" : log.CheckInTime);
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckOutTime) ? "-" : log.CheckOutTime);
                allItems.Add(item);
            }

            // Initialize pagination if not already done
            if (_memberAttendancePagination == null && lst_member.Parent != null)
            {
                _memberAttendancePagination = new ListViewPagination(lst_member, lst_member.Parent);
            }

            // Use pagination to display items
            if (_memberAttendancePagination != null)
            {
                _memberAttendancePagination.SetItems(allItems);
            }
            else
            {
                // Fallback: add items directly if pagination not available
                lst_member.BeginUpdate();
                lst_member.Items.AddRange(allItems.ToArray());
                lst_member.EndUpdate();
            }
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
            lst_member.Items.Clear();
            lst_member.View = View.Details;
            lst_member.FullRowSelect = true;
            lst_member.GridLines = true;

            lst_member.Columns.Clear();
            lst_member.Columns.Add("Log ID", 70);
            lst_member.Columns.Add("Member ID", 80);
            lst_member.Columns.Add("Member Name", 120);
            lst_member.Columns.Add("Check-In Time", 150);
            lst_member.Columns.Add("Check-Out Time", 150);
            lst_member.Columns[lst_member.Columns.Count - 1].Width = -2;

            var attendanceLogs = AppSession.AllAttendanceLogs ?? new List<dynamic>();
            var members = AppSession.AllMembers ?? new List<Member>();

            var allItems = new List<ListViewItem>();

            foreach (var log in attendanceLogs)
            {
                var item = new ListViewItem(log.LogID.ToString());
                item.SubItems.Add(log.MemberID.ToString());

                // Fetch member name from AppSession cache
                var member = members.FirstOrDefault(m => m.MemberID == log.MemberID);
                item.SubItems.Add(member?.Name ?? "-");

                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckInTime) ? "-" : log.CheckInTime);
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckOutTime) ? "-" : log.CheckOutTime);
                allItems.Add(item);
            }

            // Initialize pagination if not already done
            if (_memberAttendancePagination == null && lst_member.Parent != null)
            {
                _memberAttendancePagination = new ListViewPagination(lst_member, lst_member.Parent);
            }

            // Use pagination to display items
            if (_memberAttendancePagination != null)
            {
                _memberAttendancePagination.SetItems(allItems);
            }
            else
            {
                // Fallback: add items directly if pagination not available
                lst_member.BeginUpdate();
                lst_member.Items.AddRange(allItems.ToArray());
                lst_member.EndUpdate();
            }
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
            lst_manualdoor.Items.Clear();
            lst_manualdoor.View = View.Details;
            lst_manualdoor.FullRowSelect = true;
            lst_manualdoor.GridLines = true;

            lst_manualdoor.Columns.Clear();
            lst_manualdoor.Columns.Add("Gate ID", 50);
            lst_manualdoor.Columns.Add("Device IP", 100);
            lst_manualdoor.Columns.Add("Member ID", 80);
            lst_manualdoor.Columns.Add("Member Name", 120);
            lst_manualdoor.Columns.Add("Is Visitor", 70);
            lst_manualdoor.Columns.Add("Visitor Name", 120);
            lst_manualdoor.Columns.Add("Visitor Phone", 100);
            lst_manualdoor.Columns.Add("Reason", 150);
            lst_manualdoor.Columns.Add("Accessed Time", 150);
            lst_manualdoor.Columns.Add("Username", 80);
            lst_manualdoor.Columns[lst_manualdoor.Columns.Count - 1].Width = -2;

            // Add visitor filter control on first call
            if (cmbDoorVisitor == null && lst_manualdoor.Parent != null)
            {
                var doorTab = lst_manualdoor.Parent;
                int dvx = btn_filter_manualdoor != null ? btn_filter_manualdoor.Right + 10 : 300;
                cmbDoorVisitor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(dvx, 6), Width = 130 };
                cmbDoorVisitor.Items.AddRange(new object[] { "All", "Members Only", "Visitors Only" });
                cmbDoorVisitor.SelectedIndex = 0;
                cmbDoorVisitor.SelectedIndexChanged += (s, e) => LoadFilteredManualDoorLogs();
                doorTab.Controls.Add(cmbDoorVisitor);
            }

            DateTime selectedDate = date_manualdoor.Value.Date; // Get the date part

            var gateAccessLogs = AppSession.AllGateLogs ?? new List<GateAccessed>();
            var members = AppSession.AllMembers ?? new List<Member>();

            // Apply date filter
            var filteredLogs = gateAccessLogs.Where(log => log.AccessedTime.Date == selectedDate).ToList();

            // Apply visitor filter
            string visitorSel = cmbDoorVisitor?.SelectedItem?.ToString() ?? "All";
            if (visitorSel == "Members Only")
                filteredLogs = filteredLogs.Where(log => !log.IsVisitor).ToList();
            else if (visitorSel == "Visitors Only")
                filteredLogs = filteredLogs.Where(log => log.IsVisitor).ToList();

            var allItems = new List<ListViewItem>();

            foreach (var log in filteredLogs)
            {
                var item = new ListViewItem(log.GateId.ToString());
                item.SubItems.Add(log.DeviceIP);
                item.SubItems.Add(log.MemberId.ToString());

                // Fetch member name from AppSession cache
                var member = members.FirstOrDefault(m => m.MemberID == log.MemberId);
                item.SubItems.Add(member?.Name ?? "-");

                item.SubItems.Add(log.IsVisitor.ToString());
                item.SubItems.Add(log.VisitorName);
                item.SubItems.Add(log.VisitorPhone);
                item.SubItems.Add(log.Reason);
                item.SubItems.Add(log.AccessedTime.ToString());
                item.SubItems.Add(log.Username);
                allItems.Add(item);
            }

            // Initialize pagination if not already done
            if (_manualDoorPagination == null && lst_manualdoor.Parent != null)
            {
                _manualDoorPagination = new ListViewPagination(lst_manualdoor, lst_manualdoor.Parent);
            }

            // Use pagination to display items
            if (_manualDoorPagination != null)
            {
                _manualDoorPagination.SetItems(allItems);
            }
            else
            {
                // Fallback: add items directly if pagination not available
                lst_manualdoor.BeginUpdate();
                lst_manualdoor.Items.AddRange(allItems.ToArray());
                lst_manualdoor.EndUpdate();
            }
        }

        private void lst_manualdoor_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadManualDoorLogs();
        }

        private void LoadManualDoorLogs()
        {
            lst_manualdoor.Items.Clear();
            lst_manualdoor.View = View.Details;
            lst_manualdoor.FullRowSelect = true;
            lst_manualdoor.GridLines = true;

            lst_manualdoor.Columns.Clear();
            lst_manualdoor.Columns.Add("Gate ID", 50);
            lst_manualdoor.Columns.Add("Device IP", 100);
            lst_manualdoor.Columns.Add("Member ID", 80);
            lst_manualdoor.Columns.Add("Member Name", 120);
            lst_manualdoor.Columns.Add("Is Visitor", 70);
            lst_manualdoor.Columns.Add("Visitor Name", 120);
            lst_manualdoor.Columns.Add("Visitor Phone", 100);
            lst_manualdoor.Columns.Add("Reason", 150);
            lst_manualdoor.Columns.Add("Accessed Time", 150);
            lst_manualdoor.Columns.Add("Username", 80);
            lst_manualdoor.Columns[lst_manualdoor.Columns.Count - 1].Width = -2;

            var gateAccessLogs = AppSession.AllGateLogs ?? new List<GateAccessed>();
            var members = AppSession.AllMembers ?? new List<Member>();

            var allItems = new List<ListViewItem>();

            foreach (var log in gateAccessLogs)
            {
                var item = new ListViewItem(log.GateId.ToString());
                item.SubItems.Add(log.DeviceIP);
                item.SubItems.Add(log.MemberId.ToString());

                // Fetch member name from AppSession cache
                var member = members.FirstOrDefault(m => m.MemberID == log.MemberId);
                item.SubItems.Add(member?.Name ?? "-");

                item.SubItems.Add(log.IsVisitor.ToString());
                item.SubItems.Add(log.VisitorName);
                item.SubItems.Add(log.VisitorPhone);
                item.SubItems.Add(log.Reason);
                item.SubItems.Add(log.AccessedTime.ToString());
                item.SubItems.Add(log.Username);
                allItems.Add(item);
            }

            // Initialize pagination if not already done
            if (_manualDoorPagination == null && lst_manualdoor.Parent != null)
            {
                _manualDoorPagination = new ListViewPagination(lst_manualdoor, lst_manualdoor.Parent);
            }

            // Use pagination to display items
            if (_manualDoorPagination != null)
            {
                _manualDoorPagination.SetItems(allItems);
            }
            else
            {
                // Fallback: add items directly if pagination not available
                lst_manualdoor.BeginUpdate();
                lst_manualdoor.Items.AddRange(allItems.ToArray());
                lst_manualdoor.EndUpdate();
            }
        }


        /////////////////////////////////SETUP/////////////////////////////////////////
        private void btn_savedebounce_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txt_debounce.Text) || !int.TryParse(txt_debounce.Text, out int seconds))
                {
                    MessageBox.Show("Please enter a valid numeric value for debounce seconds.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (seconds < 0)
                {
                    MessageBox.Show("Debounce seconds cannot be negative.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Persist as integer (string storage), key: AttendanceDebounceSeconds
                _dbHelper.SaveSetting("AttendanceDebounceSeconds", seconds.ToString());

                LogHelper.Log($"[Debounce] Saved AttendanceDebounceSeconds = {seconds}");
                MessageBox.Show("Debounce setting saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Debounce][ERROR] {ex.Message}");
                MessageBox.Show("Failed to save debounce setting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void btn_getdb_Click(object sender, EventArgs e)
        {
            btn_getdb.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                LogHelper.Log("Starting backup bundle (DB snapshot + Logs + GLogs) email.");

                bool ok = await CommonUtils.CreateAndEmailDbLogsAndGLogsAsync(_dbHelper);

                if (ok)
                {
                    MessageBox.Show(
                        "Backup bundle has been sent via email (DB snapshot + Logs ZIP + GLogs ZIP).",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Failed to send backup bundle email. Check Logs for details.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in btn_getdb_Click: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btn_getdb.Enabled = true;
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
                MessageBox.Show($"Local time and GMT saved.\nLocalTime: {nowLocal:yyyy-MM-dd HH:mm:ss}\nGMT: {gmt}",
                    "Time Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[btn_settime_Click][ERROR] {ex.Message}");
                MessageBox.Show("Failed to save local time and GMT. See logs for details.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////



        ////////////////////////////////////STAFF MANAGEMENT HELPERS////////////////////////////////////

        // initialize AutoComplete suggestions for txt_staff using DB: _dbHelper.GetStaffs()
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

                txt_staff.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txt_staff.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var staffCollection = new AutoCompleteStringCollection();
                staffCollection.AddRange(names.ToArray());

                txt_staff.AutoCompleteCustomSource = staffCollection;
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

                // Initialize pagination if not already done
                if (_staffsPagination == null && lst_staffs.Parent != null)
                {
                    _staffsPagination = new ListViewPagination(lst_staffs, lst_staffs.Parent);
                }

                // Use pagination to display items
                if (_staffsPagination != null)
                {
                    _staffsPagination.SetItems(allItems);
                }
                else
                {
                    // Fallback: add items directly if pagination not available
                    lst_staffs.BeginUpdate();
                    lst_staffs.Items.Clear();
                    lst_staffs.Items.AddRange(allItems.ToArray());
                    lst_staffs.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load staff list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // setup lst_staffs columns and initial load
        private void InitializeStaffsListView()
        {
            lst_staffs.BeginUpdate();
            lst_staffs.Clear();
            lst_staffs.View = View.Details;
            lst_staffs.FullRowSelect = true;
            lst_staffs.GridLines = true;

            // Columns matching Members pattern
            lst_staffs.Columns.Add("Staff ID", 70);
            lst_staffs.Columns.Add("Name", 140);
            lst_staffs.Columns.Add("Status", 80);
            lst_staffs.Columns.Add("Biometric", 130);
            lst_staffs.Columns.Add("Enrollment ID", 100);
            lst_staffs.Columns.Add("Sync Status", 110);
            lst_staffs.Columns.Add("Failure Reason", 85);
            lst_staffs.Columns.Add("Actions", 100);

            lst_staffs.EndUpdate();

            // Add filter controls to the tab
            var staffTab = lst_staffs.Parent;
            if (staffTab != null && cmbStaffStatus == null)
            {
                int sx = btn_stafffilter != null ? btn_stafffilter.Right + 10 : 290;
                cmbStaffStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(sx, 11), Width = 110 };
                cmbStaffStatus.Items.AddRange(new object[] { "All Status", "active", "inactive" });
                cmbStaffStatus.SelectedIndex = 0;
                cmbStaffStatus.SelectedIndexChanged += (s, e) => LoadStaffsIntoListView(txt_staff?.Text?.Trim());
                staffTab.Controls.Add(cmbStaffStatus);
                sx += 118;

                cmbStaffSync = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(sx, 11), Width = 120 };
                cmbStaffSync.Items.AddRange(new object[] { "All Sync", "Fully Synced", "Not Synced" });
                cmbStaffSync.SelectedIndex = 0;
                cmbStaffSync.SelectedIndexChanged += (s, e) => LoadStaffsIntoListView(txt_staff?.Text?.Trim());
                staffTab.Controls.Add(cmbStaffSync);
                sx += 128;

                chkStaffFailure = new CheckBox { Text = "Has Failure Only", Location = new Point(sx, 13), AutoSize = true };
                chkStaffFailure.CheckedChanged += (s, e) => LoadStaffsIntoListView(txt_staff?.Text?.Trim());
                staffTab.Controls.Add(chkStaffFailure);
            }

            // Initial full load
            LoadStaffsIntoListView(null);
        }


        // button click handler to filter by txt_staff text (unchanged)
        private void btn_stafffilter_Click(object sender, EventArgs e)
        {
            string nameFilter = txt_staff.Text?.Trim();
            LoadStaffsIntoListView(string.IsNullOrEmpty(nameFilter) ? null : nameFilter);
        }

        // optional live filtering as user types (unchanged)
        private void txt_staff_TextChanged(object sender, EventArgs e)
        {
            if (!_isInitialized) return;
            var filter = txt_staff.Text?.Trim();
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

                    if (columnIndex == lst_members.Columns.Count - 1) // Actions column [View]
                    {
                        if (item.Tag is Member member)
                        {
                            ShowMemberDeviceStatus(member);
                        }
                    }
                    else if (columnIndex == lst_members.Columns.Count - 2) // Failure Reason [View] column
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
                MessageBox.Show("This member does not have an Enrollment ID assigned.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int enrollmentId = member.EnrollmentID.Value;
            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

            if (allDevices.Count == 0)
            {
                MessageBox.Show("No devices available to check.", "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Error retrieving member status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // AutoComplete for txt_staffname using known staff names from session
        private void InitializeStaffAttendanceSuggestions()
        {
            try
            {
                txt_staffname.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txt_staffname.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var staffList = AppSession.AllStaff ?? new List<Staff>();
                var names = new AutoCompleteStringCollection();

                foreach (var s in staffList)
                {
                    if (!string.IsNullOrWhiteSpace(s.Name))
                        names.Add(s.Name);
                }

                txt_staffname.AutoCompleteCustomSource = names;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[InitializeStaffAttendanceSuggestions][ERROR] {ex.Message}");
            }
        }

        //  setup lst_staffattendence columns
        private void InitializeStaffAttendanceListView()
        {
            lst_staffattendence.BeginUpdate();
            lst_staffattendence.Clear();
            lst_staffattendence.View = View.Details;
            lst_staffattendence.FullRowSelect = true;
            lst_staffattendence.GridLines = true;

            lst_staffattendence.Columns.Add("Log ID", 80);
            lst_staffattendence.Columns.Add("Enrollment ID", 110);
            lst_staffattendence.Columns.Add("Attendee ID", 100);
            lst_staffattendence.Columns.Add("Name", 160);
            lst_staffattendence.Columns.Add("Check-In", 160);
            lst_staffattendence.Columns.Add("Check-Out", 160);
            lst_staffattendence.Columns[lst_staffattendence.Columns.Count - 1].Width = -2;

            lst_staffattendence.EndUpdate();

            // Initial load
            LoadFilteredStaffAttendanceLogs();
        }


        // unified filter loader (by optional name and/or date)
        private void LoadFilteredStaffAttendanceLogs()
        {
            try
            {
                lst_staffattendence.BeginUpdate();
                lst_staffattendence.Items.Clear();

                string nameFilter = txt_staffname.Text?.Trim();
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
                        DateTime? inTime = ParseDateTime(SafeToString(log, "CheckInTime"));
                        DateTime? outTime = ParseDateTime(SafeToString(log, "CheckOutTime"));

                        bool matches =
                            (inTime.HasValue && inTime.Value.Date == selectedDate) ||
                            (outTime.HasValue && outTime.Value.Date == selectedDate);

                        if (!matches) return false;
                    }

                    return true;
                }).ToList();

                var allItems = new List<ListViewItem>();

                foreach (var log in filtered)
                {
                    string logId = SafeToString(log, "LogID");
                    string enrollmentId = SafeToString(log, "EnrollmentID");
                    if (string.IsNullOrEmpty(enrollmentId))
                        enrollmentId = SafeToString(log, "EnrollmentId");

                    string attendeeId = SafeToString(log, "AttendeeID");
                    if (string.IsNullOrEmpty(attendeeId))
                        attendeeId = SafeToString(log, "StaffID");

                    string name = SafeToString(log, "Name");
                    string checkIn = SafeToString(log, "CheckInTime");
                    string checkOut = SafeToString(log, "CheckOutTime");

                    var item = new ListViewItem(string.IsNullOrWhiteSpace(logId) ? "-" : logId);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(enrollmentId) ? "-" : enrollmentId);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(attendeeId) ? "-" : attendeeId);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(name) ? "-" : name);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(checkIn) ? "-" : checkIn);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(checkOut) ? "-" : checkOut);

                    item.Tag = new { EnrollmentId = enrollmentId, Name = name, AttendeeId = attendeeId };
                    allItems.Add(item);
                }

                // Initialize pagination if not already done
                if (_staffAttendancePagination == null && lst_staffattendence.Parent != null)
                {
                    _staffAttendancePagination = new ListViewPagination(lst_staffattendence, lst_staffattendence.Parent);
                }

                // Use pagination to display items
                if (_staffAttendancePagination != null)
                {
                    _staffAttendancePagination.SetItems(allItems);
                }
                else
                {
                    // Fallback: add items directly if pagination not available
                    lst_staffattendence.Items.AddRange(allItems.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load staff attendance logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lst_staffattendence.EndUpdate();
            }
        }

        
        // button click handler
        private void btn_filterattendencestaff_Click(object sender, EventArgs e)
        {
            LoadFilteredStaffAttendanceLogs();
        }

        // live filter on typing
        private void txt_staffname_TextChanged(object sender, EventArgs e)
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

                    if (columnIndex == lst_staffs.Columns.Count - 1) // Actions column [View]
                    {
                        dynamic staffTag = item.Tag;
                        string staffIdStr = staffTag.StaffID;
                        string enrollmentStr = staffTag.EnrollmentID;
                        string staffName = staffTag.Name;

                        // Open staff details form with Enroll/Delete buttons
                        ShowStaffDetailsForm(staffIdStr, staffName, enrollmentStr);
                    }
                    else if (columnIndex == lst_staffs.Columns.Count - 2 && item.SubItems[columnIndex].Text == "[View]") // Failure Reason [View]
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
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ShowStaffDetailsForm(string staffId, string staffName, string enrollmentId)
        {
            if (string.IsNullOrEmpty(enrollmentId) || !int.TryParse(enrollmentId, out int enrollmentIdInt) || enrollmentIdInt <= 0)
            {
                MessageBox.Show("This staff member does not have a valid Enrollment ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

            if (allDevices.Count == 0)
            {
                MessageBox.Show("No devices available to check.", "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DeleteStaffFromListView(string staffIdentifier, string staffName, string enrollmentIdStr = null)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(staffName))
            {
                MessageBox.Show("Staff name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(
                    $"Cannot delete '{staffName}'. No numeric Enrollment ID found for this staff.\n\n" +
                    "Please ensure the staff record has a valid EnrollmentID (numeric) in the database.",
                    "Missing Enrollment ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogHelper.Log($"[DeleteStaffFromListView] Aborted - EnrollmentID missing for staff '{staffName}' (Identifier: {staffIdentifier})");
                return;
            }

            int enrollId = enrollmentId.Value;

            // Confirm deletion with user
            var confirmResult = MessageBox.Show(
                $"Are you sure you want to DELETE all biometric data for '{staffName}' (Enrollment ID: {enrollId}) from all devices?\n\n" +
                "This action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes)
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
                    MessageBox.Show(
                        $"✓ Staff '{staffName}' (Enrollment ID: {enrollId}) deleted successfully from all devices!",
                        "Deletion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LogHelper.Log($"[DeleteStaffFromListView] ✓ Successfully deleted staff '{staffName}' (Enrollment ID: {enrollId})");

                    // Refresh the staff list view
                    LoadStaffsIntoListView(txt_staff.Text?.Trim());
                }
                else
                {
                    LogHelper.Log($"[DeleteStaffFromListView] Deletion cancelled or failed for staff '{staffName}' (Enrollment ID: {enrollId})");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DeleteStaffFromListView] ❌ Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"❌ Deletion failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Staff name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show(
                        $"Cannot enroll '{staffName}'. No numeric Enrollment ID found for this staff.\n\n" +
                        "Please ensure the staff record has a valid EnrollmentID (numeric) in the database.",
                        "Missing Enrollment ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogHelper.Log($"[EnrollStaffFromListView] Aborted - EnrollmentID missing for staff '{staffName}' (Identifier: {staffIdentifier})");
                    return;
                }

                int enrollId = enrollmentId.Value;

                // Check for duplicate enrollment in the in-memory buffer by enrollment id
                var already = _enrolledStaffBuffer.FirstOrDefault(x => x.Id == enrollId);
                if (already != default)
                {
                    MessageBox.Show($"Enrollment ID {enrollId} is already enrolled in the buffer. Cannot enroll twice.",
                        "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show($"❌ No Card ID or Face Image found for '{staffName}'.\n\nCannot enroll without biometric data.",
                        "Insufficient Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show(
                        $"✓ Staff '{staffName}' (Enrollment ID: {enrollId}) enrolled successfully!\n\n" +
                        $"Card ID: {(hasCard ? staffCardId.ToString() : "Not used")}\n" +
                        $"Face Image: {(hasImage ? "Yes" : "No")}",
                        "Enrollment Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                MessageBox.Show($"❌ Enrollment failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private List<Device> ShowDeviceSelectionDialog(List<Device> devices, string title = "Select Devices")
        {
            var selected = new List<Device>();

            if (devices == null || devices.Count == 0)
                return selected;

            using (var dlg = new Form())
            {
                dlg.Text = title;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.Width = 520;
                dlg.Height = 420;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ShowInTaskbar = false;

                var clb = new CheckedListBox
                {
                    Dock = DockStyle.Top,
                    Height = 320,
                    CheckOnClick = true
                };

                // Keep device ordering aligned with the passed list so indices map correctly
                foreach (var device in devices)
                {
                    string label = !string.IsNullOrWhiteSpace(device.Location)
                        ? $"{device.Location} ({device.DeviceIP})"
                        : device.DeviceIP ?? "(unknown)";
                    clb.Items.Add(label, true); // default: pre-check all
                }

                var btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Width = 100,
                    Left = dlg.ClientSize.Width - 220,
                    Top = clb.Bottom + 8,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Width = 100,
                    Left = dlg.ClientSize.Width - 110,
                    Top = clb.Bottom + 8,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                dlg.Controls.Add(clb);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                // Show as modal on the UI thread
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return selected;

                // Map checked indices back to device list
                foreach (int idx in clb.CheckedIndices)
                {
                    if (idx >= 0 && idx < devices.Count)
                        selected.Add(devices[idx]);
                }
            }

            return selected;
        }

        private async Task<bool> EnrollStaffAcrossAllDevicesAsync(int enrollId, string userName, long cardId, string imagePath)
        {
            var normalDevices = AppSession.DeviceList ?? new List<Device>();

            if (normalDevices.Count == 0)
            {
                MessageBox.Show("No normal devices are available to enroll.", "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("No normal devices are available.", "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                string folderId = txt_drivefolderid.Text.Trim();
                if (string.IsNullOrWhiteSpace(folderId))
                {
                    MessageBox.Show("Please enter a valid Google Drive Folder ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _dbHelper.SaveSetting("GoogleDriveFolderId", folderId);
                LogHelper.Log($"[Backup] Saved GoogleDriveFolderId: {folderId}");
                MessageBox.Show("Google Drive Folder ID saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                string json = txt_drivecredentials.Text.Trim();
                if (string.IsNullOrWhiteSpace(json))
                {
                    MessageBox.Show("Please paste the credentials JSON content.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Verify if it's valid JSON before saving
                try { Newtonsoft.Json.Linq.JObject.Parse(json); }
                catch { MessageBox.Show("The content is not a valid JSON string.", "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                _dbHelper.SaveEncryptedSetting("GoogleDriveCredentialsJson", json);
                LogHelper.Log("[Backup] Saved and Encrypted Google Drive Credentials.");
                MessageBox.Show("Google Drive credentials saved and encrypted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txt_drivecredentials.Clear(); // Clear for security

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
                MessageBox.Show("Retention policy saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Backup][ERROR] btn_save_retentiondays: {ex.Message}");
                MessageBox.Show("Failed to save retention policy.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Backup preferences updated:\nLogs: {includeLogs}\nGLogs: {includeGLogs}\nDatabase: {includeDB}",
                    "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Backup][ERROR] btn_save_backuptype: {ex.Message}");
                MessageBox.Show("Failed to save backup preferences.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Google Drive Folder ID is not configured. Please save it first.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(retentionStr, out int days) || days <= 0)
                {
                    MessageBox.Show("Retention policy is invalid or not set. Please save a value above 0 in Retention Days.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3. User Confirmation
                var confirm = MessageBox.Show($"Are you sure you want to scan Google Drive and PERMANENTLY DELETE backup files older than {days} days?",
                                            "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                // 4. Execution
                btn_backupcleanup.Enabled = false;
                Cursor = Cursors.WaitCursor;

                int deleted = await CommonUtils.CleanupOldDriveFilesAsync(folderId, days, _dbHelper);

                // 5. Result Feedback
                if (deleted >= 0)
                {
                    MessageBox.Show($"Drive cleanup successful!\nTotal files removed: {deleted}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Drive cleanup failed. Please check the system logs for details (likely an authentication error).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[UI] Error in btn_backupcleanup_Click: {ex.Message}");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_backupcleanup.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void btn_test_connection_Click(object sender, EventArgs e)
        {
            // Trigger initial backup after 5 seconds
            _backupService?.TriggerBackupAfterDelay(TimeSpan.FromSeconds(5));
        }

        private void subpnl_setup_Paint(object sender, PaintEventArgs e)
        {

        }

        //////////////////////////////////////////////////////////////////////////////////////
    }
}