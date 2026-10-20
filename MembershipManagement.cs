using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class MembershipManagement : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private List<OfflineMember> _offlineMembersCache = new List<OfflineMember>();
        private List<Member> _allMembersCache = new List<Member>();

        public MembershipManagement()
        {
            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            _dbHelper = new DatabaseHelper();
        }

        private void MembershipManagement_Load(object sender, EventArgs e)
        {
            tabPanel.SetTabs(new[] { "Offline Members", "Members" });
            tabPanel.TabChanged += tabPanel_TabChanged;
            tabPanel.SelectTab(0);

            cardListViewOfflineMembers.SetColumns(
                new[] { "MEMBER", "PACKAGE", "PERIODICITY", "BIOMETRIC", "SYNC", "CLEANUP", "FAILURE" },
                new[] { 52, 190, 318, 406, 560, 663, 752 });

            cardListViewAllMembers.SetColumns(
                new[] { "MEMBER", "ENROLLMENT", "STATUS", "BIOMETRIC", "SYNC", "PACKAGE EXPIRY", "RENEW" },
                new[] { 52, 252, 340, 434, 560, 676, 774 });

            cmbOfflineServerSync.SelectedIndex = 0;
            cmbAllMemberStatus.SelectedIndex = 0;
            cmbAllMemberBiometric.SelectedIndex = 0;
            cmbAllMemberExpiry.SelectedIndex = 0;

            LoadOfflineMembersList();
            LoadAllMembersList();
        }

        private void tabPanel_TabChanged(object sender, int index)
        {
            pnlOfflineContent.Visible = (index == 0);
            pnlAllMembersContent.Visible = (index == 1);
        }

        // ── Offline members ───────────────────────────────────────────────────────

        private void btn_addOfflineMember_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new AddOfflineMemberForm())
                    form.ShowDialog(this);
                LoadOfflineMembersList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open form:\n" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txt_offlineMemberSearch_TextChanged(object sender, EventArgs e) => FilterOfflineMembersList();
        private void cmbOfflineFilter_Changed(object sender, EventArgs e) => FilterOfflineMembersList();

        private void LoadOfflineMembersList()
        {
            try
            {
                _offlineMembersCache = _dbHelper.GetOfflineMembers() ?? new List<OfflineMember>();
                var packagesById = _dbHelper.GetPackages(activeOnly: false).ToDictionary(p => p.PackageId, p => p.Title);

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
                cmbOfflinePackage.SelectedIndex = cmbOfflinePackage.Items.Contains(prevSel)
                    ? cmbOfflinePackage.Items.IndexOf(prevSel) : 0;

                RenderOfflineMembersList(_offlineMembersCache, packagesById);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[MembershipManagement] Failed to load offline members: {ex.Message}");
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

            string syncSel = cmbOfflineServerSync.SelectedItem?.ToString() ?? "All Sync";
            if (syncSel == "Synced")       filtered = filtered.Where(m => m.ServerSync).ToList();
            else if (syncSel == "Pending") filtered = filtered.Where(m => !m.ServerSync).ToList();

            string pkgSel = cmbOfflinePackage.SelectedItem?.ToString() ?? "All Packages";
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

            if (chkOfflineFailure.Checked)
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
            int statWaiting = members.Count(m => !m.ServerSync);
            int statSynced  = members.Count(m => m.ServerSync);
            lblStatWaiting.Text = statWaiting.ToString();
            lblStatSynced.Text  = statSynced.ToString();
            lblStatFailed.Text  = "—";
            lblOfflineEmpty.Visible = members.Count == 0;

            cardListViewOfflineMembers.SetRowFactory(members.Count, idx =>
            {
                var m = members[idx];

                string packageTitle = "-", periodicity = "-";
                try {
                    if (!string.IsNullOrWhiteSpace(m.SubscriptionJson)) {
                        var sub = JObject.Parse(m.SubscriptionJson);
                        int pkgId = sub["package_id"]?.ToObject<int>() ?? 0;
                        packageTitle = packagesById.TryGetValue(pkgId, out var t) ? t : $"#{pkgId}";
                        periodicity = sub["periodicity"]?.ToString() ?? "-";
                    }
                } catch { }

                var bioList = new List<string>();
                if (!string.IsNullOrWhiteSpace(m.RFID)) bioList.Add("RFID");
                if (!string.IsNullOrWhiteSpace(m.Picture)) bioList.Add("Face");
                if (m.Fingerprint) bioList.Add("Fp");
                string bioText = bioList.Count > 0 ? string.Join(", ", bioList) : "-";

                string syncText = m.ServerSync ? "Synced" : "Pending";
                string cleanupText = "-";
                if (m.ServerSync) {
                    int total = m.DeviceCleanupStatus?.Count ?? 0;
                    int done  = m.DeviceCleanupStatus?.Count(kv => kv.Value) ?? 0;
                    cleanupText = total == 0 ? "Done" : $"{done}/{total}";
                }

                bool hasFailure = false;
                try {
                    var log = _dbHelper.GetLatestOfflineLogForMember(m.ClientId);
                    hasFailure = log != null && !string.IsNullOrWhiteSpace(log.FailureReason) && log.FailureReason != "-";
                } catch { }

                var accent = Color.FromArgb(59, 130, 246);
                var red    = Color.FromArgb(239, 68, 68);
                var green  = Color.FromArgb(16, 185, 129);
                var orange = Color.FromArgb(249, 115, 22);
                var gray   = Color.FromArgb(156, 163, 175);
                var navy   = Color.FromArgb(30, 38, 90);
                var muted  = Color.FromArgb(130, 138, 165);

                var card = CardListView.MakeCard(900, 52);

                string initial = (!string.IsNullOrWhiteSpace(m.Name)) ? m.Name.Substring(0, 1) : "?";
                card.Controls.Add(CardListView.MakeAvatarLabel(initial, accent, 8, 8, 36));
                card.Controls.Add(new Label { Text = m.Name ?? "-",         Location = new Point(52,  6), Width = 130, Height = 18, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = navy,  BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = m.Phone ?? "-",        Location = new Point(52, 26), Width = 130, Height = 16, Font = new Font("Segoe UI", 8f, FontStyle.Regular),  ForeColor = muted, BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = packageTitle,          Location = new Point(190,17), Width = 120, Height = 18, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),    ForeColor = navy,  BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = periodicity,           Location = new Point(318,17), Width = 80,  Height = 18, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),    ForeColor = muted, BackColor = Color.Transparent });

                if (bioList.Count == 0)
                    card.Controls.Add(CardListView.MakePill("No biometric", Color.FromArgb(255, 237, 213), orange, 406, 15, 110, 22));
                else
                    card.Controls.Add(CardListView.MakePill(string.Join(", ", bioList), Color.FromArgb(229, 231, 235), gray, 406, 15, 110, 22));

                bool synced = m.ServerSync;
                card.Controls.Add(CardListView.MakePill(
                    synced ? "● Synced" : "● Pending",
                    synced ? Color.FromArgb(209, 250, 229) : Color.FromArgb(255, 237, 213),
                    synced ? green : orange, 560, 15, 95, 22));

                card.Controls.Add(new Label { Text = cleanupText, Location = new Point(663, 17), Width = 80, Height = 18, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular), ForeColor = muted, BackColor = Color.Transparent });

                if (hasFailure) {
                    var capturedMember = m;
                    var btnView = CardListView.MakeActionButton("Failure", Color.White, red, 752, 13, 60, 26);
                    btnView.Click += (s, ev) => {
                        string fullReason = "-";
                        try {
                            var log = _dbHelper.GetLatestOfflineLogForMember(capturedMember.ClientId);
                            if (log != null && !string.IsNullOrWhiteSpace(log.FailureReason))
                                fullReason = log.FailureReason;
                        } catch { }
                        if (fullReason != "-")
                            using (var dlg = new FailureReasonDetailForm(fullReason, null, capturedMember.EnrollmentID, capturedMember.Name, capturedMember.Picture, _dbHelper))
                                dlg.ShowDialog(this);
                    };
                    card.Controls.Add(btnView);
                } else {
                    card.Controls.Add(new Label { Text = "-", Location = new Point(752, 17), Width = 40, Height = 18, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular), ForeColor = muted, BackColor = Color.Transparent });
                }

                return card;
            });
        }

        // ── All members ───────────────────────────────────────────────────────────

        private void txt_allMembersSearch_TextChanged(object sender, EventArgs e) => FilterAllMembersList();
        private void cmbAllMembersFilter_Changed(object sender, EventArgs e) => FilterAllMembersList();

        private void LoadAllMembersList()
        {
            try
            {
                _allMembersCache = _dbHelper.GetMembers() ?? new List<Member>();
                RenderAllMembersList(_allMembersCache);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[MembershipManagement] Failed to load members: {ex.Message}");
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

            string statusSel = cmbAllMemberStatus.SelectedItem?.ToString() ?? "All Status";
            if (statusSel != "All Status")
                filtered = filtered.Where(m => string.Equals(m.Status, statusSel, StringComparison.OrdinalIgnoreCase)).ToList();

            string bioSel = cmbAllMemberBiometric.SelectedItem?.ToString() ?? "All Biometric";
            if (bioSel == "Has Face")
                filtered = filtered.Where(m => !string.IsNullOrWhiteSpace(m.Picture)).ToList();
            else if (bioSel == "Has Fingerprint")
                filtered = filtered.Where(m => m.Fingerprint).ToList();
            else if (bioSel == "Has RFID")
                filtered = filtered.Where(m => !string.IsNullOrWhiteSpace(m.RFID)).ToList();
            else if (bioSel == "Missing Any")
                filtered = filtered.Where(m => string.IsNullOrWhiteSpace(m.Picture) || !m.Fingerprint || string.IsNullOrWhiteSpace(m.RFID)).ToList();

            string expirySel = cmbAllMemberExpiry.SelectedItem?.ToString() ?? "Any Expiry";
            if (expirySel != "Any Expiry")
            {
                var now = DateTime.Today;
                filtered = filtered.Where(m =>
                {
                    if (!DateTime.TryParse(m.PackageExpiry, out var expDate)) return false;
                    if (expirySel == "Expired")            return expDate < now;
                    if (expirySel == "Expires in 7 days")  return expDate >= now && expDate <= now.AddDays(7);
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
            lbl_allMembersCount.Text = $"{members.Count} members";

            cardListViewAllMembers.SetRowFactory(members.Count, idx =>
            {
                var m = members[idx];

                string syncText = "-";
                if (totalDevices > 0) {
                    int synced = 0;
                    foreach (var d in allDevices)
                        if (m.SyncStatus != null && m.SyncStatus.TryGetValue(d.DeviceId, out bool ok) && ok) synced++;
                    syncText = $"{synced}/{totalDevices}";
                }

                var bioList = new List<string>();
                if (!string.IsNullOrWhiteSpace(m.RFID)) bioList.Add("RFID");
                if (!string.IsNullOrWhiteSpace(m.Picture)) bioList.Add("Face");
                if (m.Fingerprint) bioList.Add("Fp");
                string bioText = bioList.Count > 0 ? string.Join(", ", bioList) : "-";

                string status = string.IsNullOrWhiteSpace(m.Status) ? "-" : m.Status;
                bool isActive = status.Equals("active", StringComparison.OrdinalIgnoreCase);

                var accent = Color.FromArgb(59, 130, 246);
                var green  = Color.FromArgb(16, 185, 129);
                var orange = Color.FromArgb(249, 115, 22);
                var gray   = Color.FromArgb(156, 163, 175);
                var navy   = Color.FromArgb(30, 38, 90);
                var muted  = Color.FromArgb(130, 138, 165);

                var card = CardListView.MakeCard(900, 52);

                string initial = (!string.IsNullOrWhiteSpace(m.Name)) ? m.Name.Substring(0, 1) : "?";
                card.Controls.Add(CardListView.MakeAvatarLabel(initial, accent, 8, 8, 36));
                card.Controls.Add(new Label { Text = string.IsNullOrWhiteSpace(m.Name) ? "-" : m.Name, Location = new Point(52,  6), Width = 192, Height = 18, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = navy,  BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = $"ID {m.MemberID}",                               Location = new Point(52, 26), Width = 192, Height = 16, Font = new Font("Segoe UI", 8f, FontStyle.Regular),  ForeColor = muted, BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = m.EnrollmentID.HasValue && m.EnrollmentID.Value > 0 ? m.EnrollmentID.Value.ToString() : "-", Location = new Point(252, 17), Width = 80, Height = 18, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular), ForeColor = navy, BackColor = Color.Transparent });

                card.Controls.Add(CardListView.MakePill(
                    isActive ? "● active" : $"● {status}",
                    isActive ? Color.FromArgb(209, 250, 229) : Color.FromArgb(229, 231, 235),
                    isActive ? green : gray, 340, 15, 85, 22));

                if (bioList.Count == 0)
                    card.Controls.Add(CardListView.MakePill("No biometric", Color.FromArgb(255, 237, 213), orange, 434, 15, 110, 22));
                else
                    card.Controls.Add(CardListView.MakePill(string.Join(", ", bioList), Color.FromArgb(229, 231, 235), gray, 434, 15, 110, 22));

                bool notSynced = syncText.StartsWith("0/") || syncText == "-";
                card.Controls.Add(CardListView.MakePill(
                    syncText == "-" ? "● No sync" : $"● {syncText}",
                    notSynced ? Color.FromArgb(255, 237, 213) : Color.FromArgb(209, 250, 229),
                    notSynced ? orange : green, 560, 15, 108, 22));

                card.Controls.Add(new Label { Text = string.IsNullOrWhiteSpace(m.PackageExpiry) ? "-" : m.PackageExpiry, Location = new Point(676, 17), Width = 90, Height = 18, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular), ForeColor = muted, BackColor = Color.Transparent });

                var capturedMember = m;
                var btnRenew = CardListView.MakeActionButton("Renew", accent, Color.White, 774, 13, 72, 26);
                btnRenew.Click += (s, ev) => {
                    if (!capturedMember.EnrollmentID.HasValue || capturedMember.EnrollmentID.Value <= 0) {
                        MessageBox.Show("This member has no EnrollmentID and cannot be renewed.", "Renew Membership", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    using (var form = new RenewMembershipForm(capturedMember)) {
                        if (form.ShowDialog(this) == DialogResult.OK)
                            LoadAllMembersList();
                    }
                };
                card.Controls.Add(btnRenew);

                return card;
            });
        }
    }
}
