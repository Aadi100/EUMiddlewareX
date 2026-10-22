using smart_attendance_middleware.Classes;
using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class AddNewUser : Form
    {
        private DatabaseHelper _dbHelper;
        private bool _showPassword = false;
        private bool _showConfirm  = false;

        public AddNewUser()
        {
            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
        }

        public void btn_CreateUser_Click(object sender, EventArgs e)
        {
            string userName        = rtxtUserName.Text.Trim();
            string role            = cmbRole.Text.Trim().ToLower();
            string password        = rtxtPassword.Text;
            string confirmPassword = rtxtConfirmPw.Text;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ModernMessageBox.Show(this, "Error", "Username and password cannot be empty.", MsgType.Error);
                return;
            }

            if (password.Length < 8)
            {
                ModernMessageBox.Show(this, "Error", "Password must be at least 8 characters long.", MsgType.Error);
                return;
            }

            if (password != confirmPassword)
            {
                ModernMessageBox.Show(this, "Error", "Passwords do not match.", MsgType.Error);
                return;
            }

            string backupCode;
            bool success = _dbHelper.AddUser(userName, role, password, out backupCode);

            if (success)
            {
                aiChatLogger.AddCelebrationMessage($"Welcome to the team, {userName}! Your account has been created successfully.");
                aiChatLogger.AddUserMessage($"New {role} user '{userName}' has joined the attendance system!");
                aiChatLogger.AddSecurityMessage("Don't forget to save your backup code in a secure location!");

                ModernMessageBox.ShowBackupCode(this, backupCode);

                rtxtUserName.Text    = "";
                rtxtPassword.Text    = "";
                rtxtConfirmPw.Text   = "";
                cmbRole.SelectedIndex = -1;

                var dm = Application.OpenForms.OfType<DeviceManager>().FirstOrDefault();
                if (dm != null)
                    dm.LoadLocalUserData();

                this.Close();
            }
            else
            {
                ModernMessageBox.Show(this, "Error", "Failed to create user.", MsgType.Error);
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            rtxtUserName.Text    = "";
            rtxtPassword.Text    = "";
            rtxtConfirmPw.Text   = "";
            cmbRole.SelectedIndex = -1;
            this.Close();
        }

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            _showPassword = !_showPassword;
            rtxtPassword.PasswordChar       = _showPassword ? '\0' : '*';
            btnTogglePassword.Text          = _showPassword ? "○" : "●";
        }

        private void btnToggleConfirm_Click(object sender, EventArgs e)
        {
            _showConfirm = !_showConfirm;
            rtxtConfirmPw.PasswordChar     = _showConfirm ? '\0' : '*';
            btnToggleConfirm.Text          = _showConfirm ? "○" : "●";
        }

        private void AddNewUser_Load(object sender, EventArgs e)
        {
            cmbRole.Items.Clear();
            cmbRole.Items.Add("admin");
            cmbRole.Items.Add("user");
            cmbRole.SelectedIndex = 1;
        }

    }
}
