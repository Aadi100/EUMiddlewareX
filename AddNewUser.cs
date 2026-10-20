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
                MessageBox.Show("Username and password cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string backupCode;
            bool success = _dbHelper.AddUser(userName, role, password, out backupCode);

            if (success)
            {
                aiChatLogger.AddCelebrationMessage($"Welcome to the team, {userName}! Your account has been created successfully.");
                aiChatLogger.AddUserMessage($"New {role} user '{userName}' has joined the attendance system!");
                aiChatLogger.AddSecurityMessage("Don't forget to save your backup code in a secure location!");

                string message = $"Important: Securely save your backup code.\nUse it if you forget your password.\nBackup Code: {backupCode}";
                ShowBackupCodeMessageBox(message, "Success", backupCode);

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
                MessageBox.Show("Failed to create user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void AddNewUser_Load(object sender, EventArgs e) { }

        private void ShowBackupCodeMessageBox(string message, string title, string backupCode)
        {
            int messageHeight = (message.Length / 50) * 30 + 60;
            int formHeight    = Math.Max(messageHeight, 200) + 60;

            using (var msgBoxForm = new Form
            {
                Size            = new Size(400, formHeight),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition   = FormStartPosition.CenterScreen,
                TopMost         = true,
                BackColor       = Color.FromArgb(45, 45, 48),
                ForeColor       = Color.White,
                Opacity         = 0.95
            })
            {
                var lblMessage = new Label
                {
                    Text      = message + "\n\n" + backupCode,
                    AutoSize  = false,
                    Dock      = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Padding   = new Padding(20),
                    Height    = messageHeight
                };

                var btnClose = new Button
                {
                    Text      = "OK",
                    Size      = new Size(100, 40),
                    Location  = new Point(75, formHeight - 60),
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White
                };
                btnClose.Click += (s, ev) => msgBoxForm.Close();

                var btnCopy = new Button
                {
                    Text      = "Copy",
                    Size      = new Size(150, 40),
                    Location  = new Point(200, formHeight - 60),
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White
                };
                btnCopy.Click += (s, ev) =>
                {
                    Clipboard.SetText(backupCode);
                    MessageBox.Show("Backup code copied to clipboard.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                msgBoxForm.Controls.Add(lblMessage);
                msgBoxForm.Controls.Add(btnClose);
                msgBoxForm.Controls.Add(btnCopy);
                msgBoxForm.ShowDialog();
            }
        }
    }
}
