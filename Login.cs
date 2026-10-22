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
        private bool _passwordVisible = false;

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
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
        }

        private void btn_close_window_Click(object sender, EventArgs e)
        {
            this.Close();
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
                ModernMessageBox.Show(this, "Error", "Username and password/backup code cannot be empty.", MsgType.Error);
                return;
            }

            // Get both login status and role
            var (isLoggedIn, userRole, userId) = _dbHelper.LoginUser(userName, passwordOrBackup);

            if (!isLoggedIn)
            {
                ModernMessageBox.Show(this, "Error", "Invalid username or password/backup code.", MsgType.Error);
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

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            txt_password.PasswordChar = _passwordVisible ? '\0' : '*';
            btnTogglePassword.Text = _passwordVisible ? "🙈" : "👁";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string backupCode = ModernMessageBox.ShowPasswordPrompt(this,
                "Login with Backup Code", "Enter your backup code to sign in",
                "Backup code:", "Sign In");

            if (backupCode != null)
            {
                backupCode = backupCode.Trim();
                string userName = txt_username.Text.Trim();

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(backupCode))
                {
                    ModernMessageBox.Show(this, "Error", "Username and backup code cannot be empty.", MsgType.Error);
                    return;
                }

                // Get both login status and role
                var (isLoggedIn, userRole, userId) = _dbHelper.LoginUser(userName, backupCode);

                if (!isLoggedIn)
                {
                    ModernMessageBox.Show(this, "Error", "Invalid username or password/backup code.", MsgType.Error);
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
