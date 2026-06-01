using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class Fingerprintmanagment : Form
    {
        private DatabaseHelper _dbHelper;

        public Fingerprintmanagment()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
        }

        private async void Fingerprintmanagment_Load(object sender, EventArgs e)
        {
            InitializeListView();
            await LoadMissingFingerprintMembersAsync();
            lst_missing_fingerprints.DoubleClick += lst_missing_fingerprints_DoubleClick;
        }

        private void InitializeListView()
        {
            lst_missing_fingerprints.View = View.Details;
            lst_missing_fingerprints.FullRowSelect = true;
            lst_missing_fingerprints.Columns.Clear();

            lst_missing_fingerprints.Columns.Add("Member ID", 80);
            lst_missing_fingerprints.Columns.Add("Name", 150);
            lst_missing_fingerprints.Columns.Add("Enrollment ID", 100);
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

        private void lst_missing_fingerprints_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
