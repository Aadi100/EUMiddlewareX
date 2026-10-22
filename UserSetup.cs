using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using smart_attendance_middleware.Classes;





namespace smart_attendance_middleware
{
    public partial class UserSetup : Form
    {
        private bool _passwordVisible = false;
        private bool _confirmPasswordVisible = false;

        private DatabaseHelper _dbHelper;
        private WebSocketManager _wsManager;
        private Processor _processor;

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

        public UserSetup()
        {
            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            //LogHelper.UIAppender = AppendConsole;


            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();


            InitializeWebSocketManager();

            _processor = new Processor(_dbHelper, _wsManager, (DeviceFunctions)null);
            _wsManager.LiveUpdate += WsManager_LiveUpdate;
        }

        private void WsManager_LiveUpdate(object sender, LiveUpdateEventArgs e)
        {
            if (e.Data is JObject jdata)
            {
                _processor.HandleLiveUserUpdate(e.EventType, jdata);
                //AppendLog($"☁️ Live event handled: {e.EventType}");
            }
        }
        private void InitializeWebSocketManager()
        {
            _wsManager = new WebSocketManager(_dbHelper);
            // _wsManager.LiveUpdate += WsManager_LiveUpdate; // Delegate to Processor instead
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            txt_password.PasswordChar = '*';
            txt_confirmpass.PasswordChar = '*';

            txt_password.MaxLength = 20;
            txt_confirmpass.MaxLength = 20;

            btnTogglePassword.BringToFront();
            btnToggleConfirmPassword.BringToFront();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            //this istest command
        }

        private void btn_close_window_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }



        //Abdul Hadi Code
        private void Dashboard()
        {
            Dashboard dashboard = new Dashboard();
            Application.Run(dashboard);
        }





        private void txt_username_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_password_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_confirmpass_TextChanged(object sender, EventArgs e)
        {

        }

        private void chkBox_is_Admin_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click_1(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            string userName = txt_username.Text.Trim();
            string password = txt_password.Text;
            string confirmPassword = txt_confirmpass.Text;
            string role = chkBox_is_Admin.Checked ? "admin" : "user";

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
            int? userId = _dbHelper.FirstAddUser(userName, role, password, out backupCode);

            if (userId != null)
            {
                AppSession.LoggedInUser = userName;
                AppSession.UserRole = role;
                AppSession.LoggedInUserID = userId.Value;

                ModernMessageBox.ShowBackupCode(this, backupCode);

                // Show Dashboard on SAME UI thread
                var dash = new Dashboard();
                dash.FormClosed += (s, args) => this.Close();
                this.Hide();
                dash.Show();
            }
            else
            {
                ModernMessageBox.Show(this, "Error", "Failed to create user.", MsgType.Error);
            }
        }

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            txt_password.PasswordChar = _passwordVisible ? '\0' : '*';
            btnTogglePassword.Text = _passwordVisible ? "🙈" : "👁";
        }

        private void btnToggleConfirmPassword_Click(object sender, EventArgs e)
        {
            _confirmPasswordVisible = !_confirmPasswordVisible;
            txt_confirmpass.PasswordChar = _confirmPasswordVisible ? '\0' : '*';
            btnToggleConfirmPassword.Text = _confirmPasswordVisible ? "🙈" : "👁";
        }
    }
}


