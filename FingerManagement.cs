using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware
{
    public partial class FingerManagement : Form
    {
        private DatabaseHelper _dbHelper;
        private ListViewPagination _pagination;

        public FingerManagement()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
        }

        private async void FingerManagement_Load(object sender, EventArgs e)
        {
            InitializeListView();
            _pagination = new ListViewPagination(lst_fingerprints, this);
            await LoadAllFingerprintStatusAsync();
        }

        private void InitializeListView()
        {
            lst_fingerprints.View = View.Details;
            lst_fingerprints.FullRowSelect = true;
            lst_fingerprints.GridLines = true;
            lst_fingerprints.Columns.Clear();

            lst_fingerprints.Columns.Add("Member ID", 80);
            lst_fingerprints.Columns.Add("Name", 150);
            lst_fingerprints.Columns.Add("Enrollment ID", 100);
            lst_fingerprints.Columns.Add("Status", 100);
            lst_fingerprints.Columns.Add("Action (Enroll)", 90);
            lst_fingerprints.Columns.Add("Action (Delete)", 90);
        }

        private async Task LoadAllFingerprintStatusAsync()
        {
            try
            {
                // Get members who are supposed to have fingerprints (Fingerprint = 1)
                // and their actual status from the Fingerprints table.
                List<dynamic> membersStatus = await _dbHelper.GetMembersFingerprintStatusAsync();
                List<ListViewItem> items = new List<ListViewItem>();

                foreach (var member in membersStatus)
                {
                    ListViewItem item = new ListViewItem(member.MemberID.ToString());
                    item.SubItems.Add(member.Name ?? "N/A");
                    item.SubItems.Add(member.EnrollmentID?.ToString() ?? "NULL");

                    string status = member.Status;
                    item.SubItems.Add(status);

                    // Add "buttons" as subitems
                    item.SubItems.Add("Enroll");
                    item.SubItems.Add("Delete");

                    if (status == "Missing")
                    {
                        item.ForeColor = Color.Red;
                    }
                    else
                    {
                        item.ForeColor = Color.Green;
                    }

                    items.Add(item);
                }

                _pagination.SetItems(items);
                LogHelper.Log($"Listed {membersStatus.Count} members with fingerprint requirements.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error loading fingerprint status: {ex.Message}");
                MessageBox.Show("Failed to load members. See logs for more info.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void lst_fingerprints_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = lst_fingerprints.HitTest(e.X, e.Y);
            if (info.Item == null || info.SubItem == null) return;

            int subItemIndex = info.Item.SubItems.IndexOf(info.SubItem);
            string enrollmentId = info.Item.SubItems[2].Text;

            if (string.IsNullOrEmpty(enrollmentId) || enrollmentId == "NULL")
            {
                if (subItemIndex >= 4)
                {
                    MessageBox.Show("This member does not have a valid Enrollment ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            try
            {
                // Action Enroll (subItemIndex 4)
                if (subItemIndex == 4)
                {
                    Fingerprint fingerprintForm = new Fingerprint();
                    fingerprintForm.EnrollmentID = enrollmentId;
                    fingerprintForm.StartPosition = FormStartPosition.CenterParent;
                    fingerprintForm.ShowDialog();
                    await LoadAllFingerprintStatusAsync();
                }
                // Action Delete (subItemIndex 5)
                else if (subItemIndex == 5)
                {
                    var confirmResult = MessageBox.Show(
                        $"Are you sure you want to delete the fingerprint for Enrollment ID: {enrollmentId}?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        if (_dbHelper.DeleteFingerprint(int.Parse(enrollmentId)))
                        {
                            MessageBox.Show("Fingerprint deleted successfully. You can now re-enroll.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadAllFingerprintStatusAsync();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete fingerprint or fingerprint not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing action: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.Log($"Error in lst_fingerprints_MouseClick: {ex.Message}");
            }
        }
    }
}
