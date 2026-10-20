using smart_attendance_middleware.Classes;
using System;
using System.Linq;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class EditUser : Form
    {
        private int _userId;
        private string _currentUsername;
        private DatabaseHelper _dbHelper;
        private bool _showNew     = false;
        private bool _showConfirm = false;

        public EditUser(int userId, string currentUsername, string currentRole)
        {
            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            _dbHelper        = new DatabaseHelper();
            _userId          = userId;
            _currentUsername = currentUsername;

            rtxtUserName.Text = currentUsername;

            // Pre-select current role in dropdown
            for (int i = 0; i < cmbRole.Items.Count; i++)
            {
                if (string.Equals(cmbRole.Items[i].ToString(), currentRole, StringComparison.OrdinalIgnoreCase))
                {
                    cmbRole.SelectedIndex = i;
                    break;
                }
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            string newRole      = cmbRole.Text.Trim();
            string newPassword  = rtxtNewPassword.Text;
            string confirmPw    = rtxtConfirmNewPw.Text;

            if (string.IsNullOrWhiteSpace(newRole))
            {
                MessageBox.Show("Role is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate password only if one was entered
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword.Length < 8)
                {
                    MessageBox.Show("Password must be at least 8 characters long.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (newPassword != confirmPw)
                {
                    MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                _dbHelper.UpdateUserInfo(_userId, _currentUsername, newRole);

                if (!string.IsNullOrEmpty(newPassword))
                    _dbHelper.UpdateUserPassword(_userId, newPassword);

                MessageBox.Show("User updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var dm = Application.OpenForms.OfType<DeviceManager>().FirstOrDefault();
                if (dm != null)
                    dm.LoadLocalUserData();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnToggleNew_Click(object sender, EventArgs e)
        {
            _showNew = !_showNew;
            rtxtNewPassword.PasswordChar = _showNew ? '\0' : '*';
            btnToggleNew.Text            = _showNew ? "○" : "●";
        }

        private void btnToggleConfirmNew_Click(object sender, EventArgs e)
        {
            _showConfirm = !_showConfirm;
            rtxtConfirmNewPw.PasswordChar = _showConfirm ? '\0' : '*';
            btnToggleConfirmNew.Text      = _showConfirm ? "○" : "●";
        }
    }
}
