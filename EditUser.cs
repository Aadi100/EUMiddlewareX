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

            cmbRole.Items.Clear();
            cmbRole.Items.Add("admin");
            cmbRole.Items.Add("user");

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
                ModernMessageBox.Show(this, "Validation", "Role is required.", MsgType.Warning);
                return;
            }

            // Validate password only if one was entered
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword.Length < 8)
                {
                    ModernMessageBox.Show(this, "Validation", "Password must be at least 8 characters long.", MsgType.Warning);
                    return;
                }
                if (newPassword != confirmPw)
                {
                    ModernMessageBox.Show(this, "Validation", "Passwords do not match.", MsgType.Warning);
                    return;
                }
            }

            try
            {
                _dbHelper.UpdateUserInfo(_userId, _currentUsername, newRole);

                if (!string.IsNullOrEmpty(newPassword))
                    _dbHelper.UpdateUserPassword(_userId, newPassword);

                ModernMessageBox.Show(this, "Success", "User updated successfully.", MsgType.Success);

                var dm = Application.OpenForms.OfType<DeviceManager>().FirstOrDefault();
                if (dm != null)
                    dm.LoadLocalUserData();

                this.Close();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Error", $"Failed to update user: {ex.Message}", MsgType.Error);
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
