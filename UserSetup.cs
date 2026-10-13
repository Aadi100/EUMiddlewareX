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


        //Abdul Hadi Code
        //MessageBox to display Backup Code
        private void ShowBackupCodeMessageBox(string message, string title, string backupCode)
        {
            int messageHeight = (message.Length / 50) * 30 + 60; 
            int formHeight = Math.Max(messageHeight, 200); 
            formHeight = formHeight + 60; 

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
                Text = message,
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
            int? userId = _dbHelper.FirstAddUser(userName, role, password, out backupCode);

            if (userId != null)
            {
                AppSession.LoggedInUser = userName;
                AppSession.UserRole = role;
                AppSession.LoggedInUserID = userId.Value;

                string message = $"Important: Securely save your backup code.\nUse it if you forget your password.\nBackup Code: {backupCode}";
                ShowBackupCodeMessageBox(message, "Success", backupCode);

                // Show Dashboard on SAME UI thread
                var dash = new Dashboard();
                dash.FormClosed += (s, args) => this.Close();
                this.Hide();
                dash.Show();
            }
            else
            {
                MessageBox.Show("Failed to create user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

