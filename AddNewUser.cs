using smart_attendance_middleware.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class AddNewUser : Form
    {

        private DatabaseHelper _dbHelper;

        public AddNewUser()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
        }

        //Abdul Hadi Code
        //Btn to create new user
        public void btn_CreateUser_Click(object sender, EventArgs e)
        {
            string userName = txt_UserName.Text.Trim();
            string role = cmb_role.Text.Trim().ToLower();
            string password = txt_Password.Text;
            string confirmPassword = txt_ConfirmPassword.Text;

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
                // Add AI celebration messages for new user
                aiChatLogger.AddCelebrationMessage($"Welcome to the team, {userName}! Your account has been created successfully.");
                aiChatLogger.AddUserMessage($"New {role} user '{userName}' has joined the attendance system!");
                aiChatLogger.AddSecurityMessage("Don't forget to save your backup code in a secure location!");
                
                string message = $"Important: Securely save your backup code.\nUse it if you forget your password.\nBackup Code: {backupCode}";
                ShowBackupCodeMessageBox(message, "Success", backupCode);

                txt_UserName.Clear();
                txt_Password.Clear();
                txt_ConfirmPassword.Clear();


                var dm = Application.OpenForms.OfType<DeviceManager>().FirstOrDefault();
                if (dm != null)
                {
                    dm.RefreshLocalUserData();
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to create user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Abdul Hadi Code
        private void Deviceanager()
        {
            DeviceManager devicemanager = new DeviceManager();
            Application.Run(devicemanager);
        }

        private void ShowBackupCodeMessageBox(string message, string title, string backupCode)
        {
            int messageHeight = (message.Length / 50) * 30 + 60;
            int formHeight = Math.Max(messageHeight, 200) + 60;

            Form msgBoxForm = new Form()
            {
                Size = new Size(400, formHeight),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Opacity = 0.95
            };

            Label lblMessage = new Label()
            {
                Text = message + "\n\n" + backupCode,
                AutoSize = false,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Padding = new Padding(20),
                Height = messageHeight
            };

            Button btnClose = new Button()
            {
                Text = "OK",
                Size = new Size(100, 40),
                Location = new Point(75, formHeight - 60),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };
            btnClose.Click += (s, e) => msgBoxForm.Close();

            Button btnCopy = new Button()
            {
                Text = "Copy",
                Size = new Size(150, 40),
                Location = new Point(200, formHeight - 60),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(backupCode);
                MessageBox.Show("Backup code copied to clipboard.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            msgBoxForm.Controls.Add(lblMessage);
            msgBoxForm.Controls.Add(btnClose);
            msgBoxForm.Controls.Add(btnCopy);

            msgBoxForm.ShowDialog();
        }

        private void txt_UserName_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_Role_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_Password_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_ConfirmPassword_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            txt_UserName.Clear();
            txt_Password.Clear();
            txt_ConfirmPassword.Clear();
            cmb_role.SelectedIndex = -1;

            this.Close();
        }

        private void AddNewUser_Load(object sender, EventArgs e)
        {

        }
    }
}
