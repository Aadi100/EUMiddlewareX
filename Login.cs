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
using System.Runtime.InteropServices;

namespace smart_attendance_middleware
{
    public partial class Login : Form
    {

        //*******************************************************************************************
        // This is used to make the form draggable by clicking and dragging anywhere on the form.
        //*******************************************************************************************
        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0); // "this" refers to the form
            }
        }
        //*******************************************************************************************

        private DatabaseHelper _dbHelper;
        public Login()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
        }

        private void btn_close_window_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        //Abdul Hadi Code
        private void Dashboard()
        {
            Dashboard dashboard = new Dashboard();
            Application.Run(dashboard);
        }


        //Abdul Hadi Code
        private void Login_Load(object sender, EventArgs e)
        {
            txt_password.PasswordChar = '*';

            if (_dbHelper.AdminUserExists() == false)
            {
                this.Close();
                Thread newThread = new Thread(StartForm1);
                newThread.SetApartmentState(ApartmentState.STA);
                newThread.Start();
            }
        }

        //Abdul Hadi Code
        private void StartForm1()
        {
            UserSetup UserSetup = new UserSetup();
            Application.Run(UserSetup);  
        }


        private void txt_username_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_password_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string userName = txt_username.Text.Trim();
            string passwordOrBackup = txt_password.Text;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(passwordOrBackup))
            {
                MessageBox.Show("Username and password/backup code cannot be empty.", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get both login status and role
            var (isLoggedIn, userRole, userId) = _dbHelper.LoginUser(userName, passwordOrBackup);

            if (!isLoggedIn)
            {
                MessageBox.Show("Invalid username or password/backup code.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Save session
            AppSession.LoggedInUser = userName;
            AppSession.UserRole = userRole;
            AppSession.LoggedInUserID = userId;
            AppSession.LoginTime = DateTime.Now;

            if (userRole == "developer")
                _dbHelper.SaveSetting("DebugMode", "true");

            // Show Dashboard on SAME UI thread
            var dash = new Dashboard();
            // When dashboard closes, close login -> exits Application.Run
            dash.FormClosed += (s, args) => this.Close();
            this.Hide();
            dash.Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Create an inline form to prompt for backup code
            Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Login with Backup Code",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label label = new Label()
            {
                Left = 10,
                Top = 20,
                Width = 320,
                Text = "Enter your backup code:"
            };

            TextBox inputBox = new TextBox()
            {
                Left = 10,
                Top = 50,
                Width = 320,
                UseSystemPasswordChar = true // Mask the backup code input
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

            // Show the form and handle the result
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                string backupCode = inputBox.Text.Trim();
                string userName = txt_username.Text.Trim();

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(backupCode))
                {
                    MessageBox.Show("Username and backup code cannot be empty.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get both login status and role
                var (isLoggedIn, userRole, userId) = _dbHelper.LoginUser(userName, backupCode);

                if (!isLoggedIn)
                {
                    MessageBox.Show("Invalid username or password/backup code.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Save session
                AppSession.LoggedInUser = userName;
                AppSession.UserRole = userRole;
                AppSession.LoggedInUserID = userId;
                AppSession.LoginTime = DateTime.Now;

                if (userRole == "developer")
                    _dbHelper.SaveSetting("DebugMode", "true");

                // Show Dashboard on SAME UI thread
                var dash = new Dashboard();
                // When dashboard closes, close login -> exits Application.Run
                dash.FormClosed += (s, args) => this.Close();
                this.Hide();
                dash.Show();
            }
        }
    }
}
