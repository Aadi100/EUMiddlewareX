using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Controls;

namespace smart_attendance_middleware
{
    public partial class FingerManagement : Form
    {
        private DatabaseHelper _dbHelper;
        private List<FingerprintItem> _allItems = new List<FingerprintItem>();

        private struct FingerprintItem
        {
            public string MemberId;
            public string Name;
            public string EnrollId;
            public string Status;
        }

        public FingerManagement()
        {
            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            _dbHelper = new DatabaseHelper();
        }

        private async void FingerManagement_Load(object sender, EventArgs e)
        {
            cmb_deviceFilter.Items.Clear();
            cmb_deviceFilter.Items.Add("Enroll on: All Devices");
            var devices = AppSession.DeviceList;
            if (devices != null)
                foreach (var d in devices)
                    cmb_deviceFilter.Items.Add($"Enroll on: {d.Location ?? d.Department ?? "Device " + d.MachineNo}");
            cmb_deviceFilter.SelectedIndex = 0;

            cardListViewFingerprints.SetColumns(
                new[] { "MEMBER", "ENROLLMENT", "STATUS", "" },
                new[] { 52, 300, 400, 512 });

            await LoadAllFingerprintStatusAsync();
        }

        private async Task LoadAllFingerprintStatusAsync()
        {
            try
            {
                List<dynamic> membersStatus = await _dbHelper.GetMembersFingerprintStatusAsync();
                _allItems = membersStatus.Select(m => new FingerprintItem {
                    MemberId = m.MemberID.ToString(),
                    Name     = (string)(m.Name ?? "N/A"),
                    EnrollId = m.EnrollmentID?.ToString() ?? "NULL",
                    Status   = (string)(m.Status ?? "")
                }).ToList();

                int missingCount = _allItems.Count(i => i.Status == "Missing");
                lbl_missingCount.Text = $"● {missingCount} missing";

                ApplyRowFactory(_allItems);
                LogHelper.Log($"Listed {membersStatus.Count} members with fingerprint requirements.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error loading fingerprint status: {ex.Message}");
                ModernMessageBox.Show(this, "Error", "Failed to load members. See logs for more info.", MsgType.Error);
            }
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            FilterList();
        }

        private void cmb_deviceFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Device filter is visual only
        }

        private void FilterList()
        {
            string term = txt_search.Text.Trim();
            List<FingerprintItem> filtered = string.IsNullOrEmpty(term)
                ? _allItems
                : _allItems.Where(i =>
                    i.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    i.EnrollId.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    i.MemberId.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                  ).ToList();
            ApplyRowFactory(filtered);
        }

        private void ApplyRowFactory(List<FingerprintItem> items)
        {
            var accent = Color.FromArgb(59, 130, 246);
            var red    = Color.FromArgb(239, 68, 68);
            var navy   = Color.FromArgb(30, 38, 90);
            var muted  = Color.FromArgb(130, 138, 165);
            var green  = Color.FromArgb(16, 185, 129);

            cardListViewFingerprints.SetRowFactory(items.Count, idx =>
            {
                var item = items[idx];
                var card = CardListView.MakeCard(900, 52);

                string initial = (!string.IsNullOrWhiteSpace(item.Name) && item.Name != "N/A")
                    ? item.Name.Substring(0, 1) : "?";
                card.Controls.Add(CardListView.MakeAvatarLabel(initial, accent, 8, 8, 36));
                card.Controls.Add(new Label { Text = item.Name,            Location = new Point(52,  6), Width = 240, Height = 18, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = navy,  BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = $"ID {item.MemberId}",Location = new Point(52, 26), Width = 240, Height = 16, Font = new Font("Segoe UI", 8f, FontStyle.Regular),  ForeColor = muted, BackColor = Color.Transparent });
                card.Controls.Add(new Label { Text = item.EnrollId,        Location = new Point(300,17), Width = 90,  Height = 18, Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),    ForeColor = navy,  BackColor = Color.Transparent });

                bool isMissing = item.Status == "Missing";
                card.Controls.Add(CardListView.MakePill(
                    isMissing ? "Missing" : "● Enrolled",
                    isMissing ? Color.FromArgb(254, 226, 226) : Color.FromArgb(209, 250, 229),
                    isMissing ? red : green, 400, 15, 90, 22));

                string capturedEnrollId = item.EnrollId;
                var btnEnroll = CardListView.MakeActionButton("Enroll fingerprint", accent, Color.White, 512, 13, 160, 26);
                btnEnroll.Click += async (s, ev) => {
                    if (string.IsNullOrEmpty(capturedEnrollId) || capturedEnrollId == "NULL") {
                        ModernMessageBox.Show(this, "Error", "This member does not have a valid Enrollment ID.", MsgType.Warning);
                        return;
                    }
                    var form = new Fingerprint();
                    form.EnrollmentID = capturedEnrollId;
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.ShowDialog();
                    await LoadAllFingerprintStatusAsync();
                };
                card.Controls.Add(btnEnroll);

                var btnDelete = CardListView.MakeActionButton("Delete", Color.White, red, 682, 13, 80, 26);
                btnDelete.Click += async (s, ev) => {
                    if (string.IsNullOrEmpty(capturedEnrollId) || capturedEnrollId == "NULL") {
                        ModernMessageBox.Show(this, "Error", "This member does not have a valid Enrollment ID.", MsgType.Warning);
                        return;
                    }
                    if (ModernMessageBox.ShowConfirm(this, "Confirm Delete", $"Delete fingerprint for Enrollment ID: {capturedEnrollId}?", "Yes", "No", MsgType.Warning)) {
                        if (_dbHelper.DeleteFingerprint(int.Parse(capturedEnrollId))) {
                            ModernMessageBox.Show(this, "Success", "Fingerprint deleted successfully. You can now re-enroll.", MsgType.Success);
                            await LoadAllFingerprintStatusAsync();
                        } else {
                            ModernMessageBox.Show(this, "Error", "Failed to delete fingerprint or fingerprint not found.", MsgType.Error);
                        }
                    }
                };
                card.Controls.Add(btnDelete);

                return card;
            });
        }
    }
}
