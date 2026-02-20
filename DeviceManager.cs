using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace smart_attendance_middleware
{
    public partial class DeviceManager : Form
    {
        private DatabaseHelper _dbHelper;
        private WebSocketManager _wsManager;
        private Processor _processor;
        private DeviceFunctions _deviceFunctions = new DeviceFunctions();
        //private AxFP_CLOCK pOcxObject;

        private readonly List<(int Id, string Name, long CardId, string ImagePath)> _staffEntries = new List<(int, string, long, string)>();
        private readonly List<(int Id, string Name, string ImagePath)> _enrolledStaffBuffer= new List<(int Id, string Name, string ImagePath)>();
        private CancellationTokenSource _cardListenCts = null;

        private bool _selectedIsStaff = false;
        private int _selectedDeviceId = 0;      // Devices.DeviceId
        private int _selectedStaffDeviceId = 0; // StaffDevices.StaffDeviceId



        public DeviceManager()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
            listViewDevices.SelectedIndexChanged += listViewDevices_SelectedIndexChanged;
            InitializeWebSocketManager();


            _processor = new Processor(_dbHelper, _wsManager, _deviceFunctions);
            _wsManager.LiveUpdate += WsManager_LiveUpdate;
            this.Shown += DeviceManager_Shown;

        }

        private void InitializeWebSocketManager()
        {
            _wsManager = new WebSocketManager(_dbHelper);
            // _wsManager.LiveUpdate += WsManager_LiveUpdate; // Delegate to Processor instead
        }
        private void WsManager_LiveUpdate(object sender, LiveUpdateEventArgs e)
        {
            if (e.Data is JObject jdata)
            {
                _processor.HandleLiveUserUpdate(e.EventType, jdata);
                //AppendLog($"☁️ Live event handled: {e.EventType}");
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        //*************************************Setup ListView************************************//
        //*************************************Setup ListView************************************//
        private void SetupDeviceList()
        {
            // Clear existing items and columns
            listViewDevices.Clear();

            // Configure ListView properties
            listViewDevices.View = View.Details;
            listViewDevices.FullRowSelect = true;
            listViewDevices.GridLines = true;
            listViewDevices.MultiSelect = false;

            // Add columns (adjust widths as needed)
            listViewDevices.Columns.Add("ID", 50);                // DeviceId or StaffDeviceId
            listViewDevices.Columns.Add("Machine No", 80);
            listViewDevices.Columns.Add("Attendance Type", 120);  // boolean for Devices, text for StaffDevices
            listViewDevices.Columns.Add("Device IP", 140);
            listViewDevices.Columns.Add("Port", 70);
            listViewDevices.Columns.Add("Connection Type", 120);
            listViewDevices.Columns.Add("Status", 90);            // Active/Inactive
            listViewDevices.Columns.Add("Is Staff", 70);          // NEW COLUMN
            listViewDevices.Columns[listViewDevices.Columns.Count - 1].Width = -2;

            // Load devices from database
            var devices = _dbHelper.LoadDevices() ?? new List<Device>();
            var staffDevices = _dbHelper.LoadStaffDevices() ?? new List<Device>();

            // Populate normal devices
            foreach (var d in devices)
            {
                var item = new ListViewItem(d.DeviceId.ToString());
                item.SubItems.Add(d.MachineNo.ToString());
                item.SubItems.Add(d.AttendanceType ? "Check-in" : "Check-out");
                item.SubItems.Add(d.DeviceIP ?? "-");
                item.SubItems.Add(d.Port.ToString());
                item.SubItems.Add(d.ConnectionType ?? "-");
                item.SubItems.Add(d.IsActive ? "Active" : "Inactive");
                item.SubItems.Add("No"); // Is Staff

                // Store the full device object
                item.Tag = d;
                listViewDevices.Items.Add(item);
            }

            // Populate staff devices
            foreach (var sd in staffDevices)
            {
                var item = new ListViewItem(sd.StaffDeviceId.ToString());
                item.SubItems.Add(sd.MachineNo.ToString());

                // StaffAttendanceType is TEXT: "Check-in" / "Check-out" / "Check-in/Check-out"
                var attText = string.IsNullOrWhiteSpace(sd.StaffAttendanceType) ? "-" : sd.StaffAttendanceType;
                item.SubItems.Add(attText);

                item.SubItems.Add(sd.DeviceIP ?? "-");
                item.SubItems.Add(sd.Port.ToString());

                // These fields may be null if not stored in StaffDevices schema
                item.SubItems.Add(sd.ConnectionType ?? "-");
                item.SubItems.Add(sd.IsActive ? "Active" : "Inactive");
                item.SubItems.Add("Yes"); // Is Staff

                item.Tag = sd;
                listViewDevices.Items.Add(item);
            }
        }

        //*************************************Event Listview Setup************************************//



        //*************************************Local user's data ************************************//

        //Abdul Hadi Code
        private void LoadLocalUserData()
        {
            list_LocalUser.Clear();

            // Set ListView to details view
            list_LocalUser.View = View.Details;
            list_LocalUser.FullRowSelect = true;
            list_LocalUser.GridLines = true;

            // Add columns
            list_LocalUser.Columns.Add("User ID", 100);
            list_LocalUser.Columns.Add("User Name", 150);
            list_LocalUser.Columns.Add("Role", 100);
            list_LocalUser.Columns.Add("Created", 150);
            list_LocalUser.Columns[list_LocalUser.Columns.Count - 1].Width = -2;


            // Load users from the database
            List<LocalUser> users = _dbHelper.LoadUsers();

            foreach (var user in users)
            {
                ListViewItem item = new ListViewItem(user.LocalUserID.ToString());
                item.SubItems.Add(user.UserName);
                item.SubItems.Add(user.Role);
                item.SubItems.Add(user.CreatedAt ?? "-");

                list_LocalUser.Items.Add(item);
            }
        }

        public void RefreshLocalUserData()
        {
            list_LocalUser.Clear();
            list_LocalUser.View = View.Details;
            list_LocalUser.FullRowSelect = true;
            list_LocalUser.GridLines = true;

            list_LocalUser.Columns.Clear();
            list_LocalUser.Columns.Add("User ID", 100);
            list_LocalUser.Columns.Add("User Name", 150);
            list_LocalUser.Columns.Add("Role", 100);
            list_LocalUser.Columns.Add("Created", 150);
            list_LocalUser.Columns[list_LocalUser.Columns.Count - 1].Width = -2;

            var users = _dbHelper.LoadUsers();
            foreach (var user in users)
            {
                var item = new ListViewItem(user.LocalUserID.ToString());
                item.SubItems.Add(user.UserName);
                item.SubItems.Add(user.Role);
                item.SubItems.Add(user.CreatedAt ?? "-");
                list_LocalUser.Items.Add(item);
            }
        }




        private void DeviceManager_Load(object sender, EventArgs e)
        {
            SetupDeviceList(); // creates columns and sample rows
            pnl_deviceManager.Visible = true; // Show device manager panel
            pnl_deviceManager.Location = new Point(186, 0); // Set location
            pnl_staffsManagment.Visible = false;
            btn_deleteUser.Enabled = false;
            btn_change_password.Enabled = false;
            btn_editUser.Enabled = false;
            lbl_apikey_status.Text = "";
            listViewDevices.SelectedItems.Clear(); // Clear selection
            ClearDeviceFields(); // Clear device fields on load
            update_session(); // Update session with device data
            InitializeMemberList();
            InitializeMemberListForStats();       // dynamic name suggestions
            InitializeMembersListView();  // members list view setup

            // Wire events
            flt_members.Click += flt_members_Click;
            txt_memb_name.TextChanged += txt_memb_name_TextChanged;

            InitializeStaffSuggestions();
            InitializeStaffsListView();
            btn_stafffilter.Click += btn_stafffilter_Click;
            txt_staff.TextChanged += txt_staff_TextChanged;
            lst_staffs.ItemActivate -= lst_staffs_ItemActivate;
            lst_staffs.ItemActivate += lst_staffs_ItemActivate;

            InitializeStaffAttendanceSuggestions();
            InitializeStaffAttendanceListView();
            btn_filterattendencestaff.Click += btn_filterattendencestaff_Click;
            txt_staffname.TextChanged += txt_staffname_TextChanged;
            date_staffattendence.ValueChanged += date_staffattendence_ValueChanged;

        }

        private void DeviceManager_closing(object sender, FormClosingEventArgs e)
        {
            StopCardIdListening();
        }

        public void update_session()
        {
            try
            {
                List<Device> devices = _dbHelper.LoadDevices();
                AppSession.DeviceList = devices;


                List<Device> staffDevices = _dbHelper.LoadStaffDevices();
                AppSession.StaffDeviceList = staffDevices;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading devices: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //////////////////////////////////SIDE BUTTONS///////////////////////////////
        private void btn_device_management_Click(object sender, EventArgs e)
        {

            // Button Color Seetup
            btn_device_management.BackColor = Color.WhiteSmoke;
            btn_device_management.ForeColor = Color.Black;

            btn_apiKey.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_apiKey.ForeColor = Color.White;

            btn_userManagement.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_userManagement.ForeColor = Color.White;

            btn_staffmanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_staffmanagment.ForeColor = Color.White;

            btn_attendencemanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_attendencemanagment.ForeColor = Color.White;

            btn_setupmanage.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_setupmanage.ForeColor = Color.White;
            // End of Button Color Seetup


            pnl_UserManagement.Visible = false; // Hide user management panel
            pnl_deviceManager.Visible = true;
            pnl_APIkey.Visible = false;
            pnl_staffsManagment.Visible = false;
            pnl_setup.Visible = false;
            pnl_deviceManager.Location = new Point(186, 0);
            ClearDeviceFields(); // Clear device fields when switching back to device management
            listViewDevices.SelectedItems.Clear();

        }

        private void btn_apiKey_Click(object sender, EventArgs e)
        {

            // Button Color Seetup
            btn_device_management.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_device_management.ForeColor = Color.White;
            //**************************************************************************************
            btn_apiKey.BackColor = Color.WhiteSmoke;
            btn_apiKey.ForeColor = Color.Black;
            //**************************************************************************************
            btn_userManagement.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_userManagement.ForeColor = Color.White;

            btn_staffmanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_staffmanagment.ForeColor = Color.White;

            btn_attendencemanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_attendencemanagment.ForeColor = Color.White;

            btn_setupmanage.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_setupmanage.ForeColor = Color.White;
            // End of Button Color Seetup

            pnl_deviceManager.Visible = false;
            pnl_attendence.Visible = false;
            pnl_UserManagement.Visible = false;
            pnl_APIkey.Visible = true;
            pnl_staffsManagment.Visible = false;
            pnl_APIkey.Location = new Point(186, 0);
        }

        private void btn_userManagement_Click(object sender, EventArgs e)
        {

            // Button Color Seetup
            btn_device_management.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_device_management.ForeColor = Color.White;

            btn_apiKey.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_apiKey.ForeColor = Color.White;

            btn_setupmanage.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_setupmanage.ForeColor = Color.White;
            //*************************************************************************************
            btn_userManagement.BackColor = Color.WhiteSmoke;
            btn_userManagement.ForeColor = Color.Black;
            //*************************************************************************************
            btn_staffmanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_staffmanagment.ForeColor = Color.White;

            btn_attendencemanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_attendencemanagment.ForeColor = Color.White;
            // End of Button Color Seetup

            pnl_deviceManager.Visible = false;
            pnl_attendence.Visible = false;
            pnl_APIkey.Visible = false;
            pnl_UserManagement.Location = new Point(186, 0);
            pnl_UserManagement.Visible = true;
            pnl_staffsManagment.Visible = false;
            pnl_setup.Visible = false;

            LoadLocalUserData();
        }

        private void btn_attendance_Click(object sender, EventArgs e)
        {
            // End of Button Color Seetup
            btn_device_management.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_device_management.ForeColor = Color.White;

            btn_apiKey.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_apiKey.ForeColor = Color.White;

            btn_userManagement.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_userManagement.ForeColor = Color.White;

            btn_setupmanage.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_setupmanage.ForeColor = Color.White;
            //****************************************************************************
            btn_staffmanagment.BackColor = Color.WhiteSmoke;
            btn_staffmanagment.ForeColor = Color.Black;
            //****************************************************************************

            btn_attendencemanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_attendencemanagment.ForeColor = Color.White;

            // Show staff panel, hide others
            pnl_staffsManagment.Visible = true;
            pnl_staffsManagment.Location = new Point(186, 0);

            pnl_deviceManager.Visible = false;
            pnl_attendence.Visible = false;
            pnl_setup.Visible = false;
            pnl_UserManagement.Visible = false;
            pnl_APIkey.Visible = false;
            // End of Button Color Seetup

        }

        private void btn_attendencemanagment_Click(object sender, EventArgs e)
        {
            // Button Color Setup
            btn_device_management.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_device_management.ForeColor = Color.White;

            btn_apiKey.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_apiKey.ForeColor = Color.White;

            btn_userManagement.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_userManagement.ForeColor = Color.White;

            btn_staffmanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_staffmanagment.ForeColor = Color.White;

            btn_setupmanage.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_setupmanage.ForeColor = Color.White;

            //****************************************************************************
            btn_attendencemanagment.BackColor = Color.WhiteSmoke;
            btn_attendencemanagment.ForeColor = Color.Black;
            //****************************************************************************
            // Show attendance panel, hide others
            pnl_attendence.Visible = true;
            pnl_attendence.Location = new Point(186, 0);

            pnl_deviceManager.Visible = false;
            pnl_UserManagement.Visible = false;
            pnl_APIkey.Visible = false;
            pnl_staffsManagment.Visible = false;
            pnl_setup.Visible = false;
            // End of Button Color Setup

            LoadMemberAttendanceLogs();
            LoadManualDoorLogs();
        }

        private void btn_setupmanage_Click(object sender, EventArgs e)
        {
            // Button Color Setup
            btn_device_management.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_device_management.ForeColor = Color.White;

            btn_apiKey.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_apiKey.ForeColor = Color.White;

            btn_userManagement.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_userManagement.ForeColor = Color.White;

            btn_staffmanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_staffmanagment.ForeColor = Color.White;

            btn_attendencemanagment.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_attendencemanagment.ForeColor = Color.White;

            //****************************************************************************
            btn_setupmanage.BackColor = Color.WhiteSmoke;
            btn_setupmanage.ForeColor = Color.Black;
            //****************************************************************************
            // Show attendance panel, hide others
            pnl_setup.Visible = true;
            pnl_setup.Location = new Point(186, 0);

            pnl_attendence.Visible = false;
            pnl_deviceManager.Visible = false;
            pnl_UserManagement.Visible = false;
            pnl_APIkey.Visible = false;
            pnl_staffsManagment.Visible = false;
            // End of Button Color Setup

            StartStaffCardIdListening();


        }


        /////////////////////////////////////////////////////////////////////////////////

        private void btn_apiKey_MouseHover(object sender, EventArgs e)
        {
            if (sender is Label btn)
            {
                // If label is not already selected (WhiteSmoke), apply hover color
                if (btn.BackColor != Color.WhiteSmoke)
                {
                    btn.BackColor = ColorTranslator.FromHtml("#1E3C64"); // Your hover color
                    btn.ForeColor = Color.White;
                }

                btn.Cursor = Cursors.Hand;
            }

        }

        private void btn_apiKey_Leave(object sender, EventArgs e)
        {

        }

        private void btn_apiKey_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Label btn)
            {
                // Only revert color if it's not WhiteSmoke (optional)
                if (btn.BackColor != Color.WhiteSmoke)
                {
                    btn.BackColor = Color.FromArgb(8, 23, 49); // Your normal color
                    btn.ForeColor = Color.White;
                }

                btn.Cursor = Cursors.Default;
            }

        }

        private void btn_userManagement_MouseHover(object sender, EventArgs e)
        {
            if (sender is Label btn)
            {
                // If label is not already selected (WhiteSmoke), apply hover color
                if (btn.BackColor != Color.WhiteSmoke)
                {
                    btn.BackColor = ColorTranslator.FromHtml("#1E3C64"); // Your hover color
                    btn.ForeColor = Color.White;
                }

                btn.Cursor = Cursors.Hand;
            }
        }

        private void btn_userManagement_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Label btn)
            {
                // Only revert color if it's not WhiteSmoke (optional)
                if (btn.BackColor != Color.WhiteSmoke)
                {
                    btn.BackColor = Color.FromArgb(8, 23, 49); // Your normal color
                    btn.ForeColor = Color.White;
                }

                btn.Cursor = Cursors.Default;
            }
        }

        private void btn_attendance_MouseHover(object sender, EventArgs e)
        {
            if (sender is Label btn)
            {
                // If label is not already selected (WhiteSmoke), apply hover color
                if (btn.BackColor != Color.WhiteSmoke)
                {
                    btn.BackColor = ColorTranslator.FromHtml("#1E3C64"); // Your hover color
                    btn.ForeColor = Color.White;
                }

                btn.Cursor = Cursors.Hand;
            }
        }

        private void btn_attendance_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Label btn)
            {
                // Only revert color if it's not WhiteSmoke (optional)
                if (btn.BackColor != Color.WhiteSmoke)
                {
                    btn.BackColor = Color.FromArgb(8, 23, 49); // Your normal color
                    btn.ForeColor = Color.White;
                }

                btn.Cursor = Cursors.Default;
            }
        }

        private void btn_device_management_MouseHover(object sender, EventArgs e)
        {
            if (sender is Label btn)
            {
                // If label is not already selected (WhiteSmoke), apply hover color
                if (btn.BackColor != Color.WhiteSmoke)
                {
                    btn.BackColor = ColorTranslator.FromHtml("#1E3C64"); // Your hover color
                    btn.ForeColor = Color.White;
                }

                btn.Cursor = Cursors.Hand;
            }
        }

        private void btn_device_management_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Label btn)
            {
                // Only revert color if it's not WhiteSmoke (optional)
                if (btn.BackColor != Color.WhiteSmoke)
                {
                    btn.BackColor = Color.FromArgb(8, 23, 49); // Your normal color
                    btn.ForeColor = Color.White;
                }

                btn.Cursor = Cursors.Default;
            }
        }


        // On selection of row send deviceId to LoadSelectedDeviceDetails
        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count == 0) return;

            var selectedItem = listViewDevices.SelectedItems[0];
            if (!(selectedItem.Tag is Device selectedDevice)) return;

            // Determine type by available keys
            _selectedIsStaff = selectedDevice.StaffDeviceId > 0;
            _selectedDeviceId = selectedDevice.DeviceId;
            _selectedStaffDeviceId = selectedDevice.StaffDeviceId;

            if (_selectedIsStaff)
                LoadSelectedStaffDeviceDetails(_selectedStaffDeviceId);
            else
                LoadSelectedDeviceDetails(_selectedDeviceId);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txt_Device_id.BorderStyle = BorderStyle.FixedSingle;
        }

        //********************************* Toggle Device Details Fields EditMode ***************************************

        private void ToggleDeviceFieldsEditMode(bool enable)
        {
            // Text fields
            txt_department.ReadOnly = !enable;
            txt_department.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_Device_id.ReadOnly = !enable;
            txt_Device_id.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_finger_capacity.ReadOnly = !enable;
            txt_finger_capacity.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_RFID_capacity.ReadOnly = !enable;
            txt_RFID_capacity.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_face_capacity.ReadOnly = !enable;
            txt_face_capacity.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_last_synced.ReadOnly = !enable;
            txt_last_synced.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_device_status.ReadOnly = !enable;
            txt_device_status.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_location.ReadOnly = !enable;
            txt_location.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_timeZone.ReadOnly = !enable;
            txt_timeZone.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_port.ReadOnly = !enable;
            txt_port.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_ip.ReadOnly = !enable;
            txt_ip.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_Serial.ReadOnly = !enable;
            txt_Serial.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            txt_password.ReadOnly = !enable;
            txt_password.BorderStyle = enable ? BorderStyle.Fixed3D : BorderStyle.None;

            // Combo fields
            cmb_device_type.Enabled = enable;
            cmb_connectionType.Enabled = enable;
            cmb_machine_no.Enabled = enable;

        }


        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count > 0)
            {
                ToggleDeviceFieldsEditMode(true); // Enable editing
                btnEdit.Visible = false;
                btnSave.Visible = true;
                btnSave.Location = btnEdit.Location;
                btn_remove_device.Visible = false;
                btnCancle.Visible = true;
                btnCancle.Location = btn_remove_device.Location;
                btn_format.Visible = true;

            }
            else
            {
                MessageBox.Show("Please select a device to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private bool IsValidIp(string ipAddress)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(ipAddress);

                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private void LoadSelectedStaffDeviceDetails(int staffDeviceId)
        {
            var device = _dbHelper.LoadStaffDevices().FirstOrDefault(d => d.StaffDeviceId == staffDeviceId);
            if (device == null) return;

            txt_Device_id.Text = device.StaffDeviceId.ToString();  // show StaffDeviceId
            txt_Serial.Text = device.DeviceSerial;
            txt_department.Text = device.Department;
            txt_face_capacity.Text = device.FaceCapacity.ToString();
            txt_finger_capacity.Text = device.FingerCapacity.ToString();
            txt_RFID_capacity.Text = device.RFIDCapacity.ToString();
            txt_ip.Text = device.DeviceIP;
            txt_port.Text = device.Port.ToString();
            txt_location.Text = device.Location;
            txt_timeZone.Text = device.TimeZone;
            txt_password.Text = device.Password;
            txt_device_status.Text = device.IsActive ? "Active" : "Inactive";
            txt_last_synced.Text = device.LastSyncTime ?? "Not Synced";
            cmb_device_type.SelectedItem = string.IsNullOrWhiteSpace(device.StaffAttendanceType) ? null : device.StaffAttendanceType;
            cmb_connectionType.SelectedItem = device.ConnectionType;
            cmb_machine_no.SelectedItem = device.MachineNo.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                MessageBox.Show("No user is currently logged in. Cannot proceed.");
                return;
            }

            // Inline password prompt
            Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Confirm Update - {currentUser} Login",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label label = new Label()
            {
                Left = 10,
                Top = 20,
                Width = 320,
                Text = "Enter your password or backup code to update the device:"
            };

            TextBox inputBox = new TextBox()
            {
                Left = 10,
                Top = 50,
                Width = 320,
                UseSystemPasswordChar = true
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

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                string enteredPassword = inputBox.Text;

                if (_dbHelper.VerifyLoginUser(currentUser, enteredPassword))
                {
                    string deviceIp = txt_ip.Text.Trim();

                    if (!IsValidIp(deviceIp))
                    {
                        MessageBox.Show("Invalid IP address. Please check the IP and try again.", "Invalid IP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int machineNoParsed = int.TryParse(cmb_machine_no.SelectedItem?.ToString(), out int machineParsed) ? machineParsed : 0;

                    // For normal devices, guard against IP/Machine conflicts (port uniqueness removed per helper doc)
                    if (!_selectedIsStaff)
                    {
                        string deviceExistsMessage = _dbHelper.IDCheckDeviceExists(deviceIp, machineNoParsed, _selectedDeviceId);
                        if (!string.IsNullOrEmpty(deviceExistsMessage))
                        {
                            MessageBox.Show(deviceExistsMessage, "Device Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    // Staff devices may have different constraints; skipping duplicate guard unless implemented

                    ToggleDeviceFieldsEditMode(false);
                    btnEdit.Visible = true;
                    btnSave.Visible = false;
                    btnCancle.Visible = false;
                    btn_remove_device.Visible = true;
                    btn_format.Visible = false;

                    // Build common model from fields
                    var deviceModel = new Device
                    {
                        Department = txt_department.Text,
                        DeviceSerial = txt_Serial.Text,
                        DeviceIP = deviceIp,
                        Port = int.TryParse(txt_port.Text, out var p) ? p : 0,
                        TimeZone = txt_timeZone.Text,
                        Location = txt_location.Text,
                        Password = txt_password.Text,
                        ConnectionType = cmb_connectionType.SelectedItem?.ToString(),
                        MachineNo = machineNoParsed,
                        IsActive = txt_device_status.Text == "Active",
                        LastSyncTime = txt_last_synced.Text,
                        FaceCapacity = int.TryParse(txt_face_capacity.Text, out var fc) ? fc : 0,
                        RFIDCapacity = int.TryParse(txt_RFID_capacity.Text, out var rc) ? rc : 0,
                        FingerCapacity = int.TryParse(txt_finger_capacity.Text, out var fpc) ? fpc : 0
                    };

                    // Attendance type mapping
                    var attendanceText = cmb_device_type.SelectedItem?.ToString() ?? string.Empty;

                    try
                    {
                        if (_selectedIsStaff)
                        {
                            // Staff update
                            deviceModel.StaffDeviceId = _selectedStaffDeviceId;
                            deviceModel.StaffAttendanceType = attendanceText; // supports "Check-in/Check-out"

                            _dbHelper.UpdateStaffDevice(deviceModel);
                        }
                        else
                        {
                            // Normal device update (boolean only)
                            deviceModel.DeviceId = _selectedDeviceId;
                            deviceModel.AttendanceType = string.Equals(attendanceText, "Check-in", StringComparison.OrdinalIgnoreCase);

                            _dbHelper.UpdateDevice(deviceModel);
                        }

                        // Notify AI Chat Logger about the device update
                        aiChatLogger.AddDeviceMessage($"Device '{deviceModel.Location}' configuration updated successfully!");
                        aiChatLogger.AddSuccessMessage("All changes saved. Your device is ready to go!");

                        SetupDeviceList();
                        ClearDeviceFields();
                        listViewDevices.SelectedItems.Clear();
                        update_session(); // Update session with device data

                        // Notify dashboard to refresh
                        NotifyDashboardRefresh();

                        MessageBox.Show("Device updated successfully.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        aiChatLogger.AddErrorMessage("Device update failed due to duplicate entry. Please check your settings.");
                        MessageBox.Show(ex.Message, "Update Failed - Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Authentication failed. Device update was cancelled.");
                }
            }
            else
            {
                MessageBox.Show("Device update cancelled.");
                listViewDevices.SelectedItems.Clear();
                ClearDeviceFields();
            }
        }




        private void btnCancle_Click(object sender, EventArgs e)
        {
            listViewDevices.SelectedItems.Clear();
            ClearDeviceFields(); // Clear the fields
            // Revert UI to non-editable mode
            ToggleDeviceFieldsEditMode(false);

            // Hide Save/Cancel, show Edit
            btnEdit.Visible = true;
            btn_remove_device.Visible = true;
            btnSave.Visible = false;
            btnCancle.Visible = false;
            btn_format.Visible = true;



            // Optionally reset field values (if you saved originals before editing)
            // ResetDeviceFormValues(originalDeviceData);
        }
        //**************************************************************



        private void LoadSelectedDeviceDetails(int deviceId)
        {
            // Fetch device details from the database based on the selected deviceId
            var device = _dbHelper.LoadDevices().FirstOrDefault(d => d.DeviceId == deviceId);

            if (device != null)
            {
                txt_Device_id.Text = device.DeviceId.ToString();
                txt_Serial.Text = device.DeviceSerial;
                txt_department.Text = device.Department;
                txt_face_capacity.Text = device.FaceCapacity.ToString();
                txt_finger_capacity.Text = device.FingerCapacity.ToString();
                txt_RFID_capacity.Text = device.RFIDCapacity.ToString();
                txt_ip.Text = device.DeviceIP;
                txt_port.Text = device.Port.ToString();
                txt_location.Text = device.Location;
                txt_timeZone.Text = device.TimeZone;
                txt_password.Text = device.Password;
                txt_device_status.Text = device.IsActive ? "Active" : "Inactive";
                txt_last_synced.Text = device.LastSyncTime ?? "Not Synced";
                cmb_device_type.SelectedItem = device.AttendanceType ? "Check-in" : "Check-out";
                cmb_connectionType.SelectedItem = device.ConnectionType;
                cmb_machine_no.SelectedItem = device.MachineNo.ToString();

            }
        }

        public void ClearDeviceFields()
        {
            txt_Device_id.Clear();
            txt_Serial.Clear();
            txt_department.Clear();
            txt_face_capacity.Clear();
            txt_finger_capacity.Clear();
            txt_RFID_capacity.Clear();
            txt_ip.Clear();
            txt_port.Clear();
            txt_location.Clear();
            txt_timeZone.Clear();
            txt_password.Clear();
            txt_device_status.Clear();
            txt_last_synced.Clear();
            cmb_device_type.SelectedIndex = -1;
            cmb_connectionType.SelectedIndex = -1;
            cmb_machine_no.SelectedIndex = -1;
        }


        private void btn_add_device_Click(object sender, EventArgs e)
        {
            ClearDeviceFields();
            listViewDevices.SelectedItems.Clear();
            AddNewDevice adddevice = new AddNewDevice();
            adddevice.ShowDialog();
            SetupDeviceList(); // Refresh the device list after adding a new device
        }

        private async void btn_remove_device_Click(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to remove.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txt_Device_id.Text, out int deviceId))
            {
                MessageBox.Show("Invalid Device ID. Please enter a valid number.");
                return;
            }

            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                MessageBox.Show("No user is currently logged in. Cannot proceed.");
                return;
            }

            // Inline prompt to ask for the password or backup code
            Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Confirm Deletion - {currentUser} Login",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label label = new Label()
            {
                Left = 10,
                Top = 20,
                Width = 320,
                Text = $"Enter your password or backup code to delete Device ID {deviceId}:"
            };

            TextBox inputBox = new TextBox()
            {
                Left = 10,
                Top = 50,
                Width = 320,
                UseSystemPasswordChar = true // This ensures that the password is masked with asterisks (*)
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

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                string enteredPassword = inputBox.Text;

                if (!IsValidIp(txt_ip.Text.Trim()))
                {
                    MessageBox.Show("The device is not reachable. Please check the IP address and try again.", "Ping Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_dbHelper.VerifyLoginUser(currentUser, enteredPassword))
                {
                    if (_selectedIsStaff)
                    {
                        aiChatLogger.AddDeviceMessage($"Preparing to remove staff device from {txt_location.Text}...");
                        aiChatLogger.AddInfoMessage("Clearing all enrollment data from the staff device first...");

                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId(_selectedStaffDeviceId);
                        _dbHelper.DeleteStaffDevice(_selectedStaffDeviceId);
                    }
                    else
                    {
                        aiChatLogger.AddDeviceMessage($"Preparing to remove device from {txt_location.Text}...");
                        aiChatLogger.AddInfoMessage("Clearing all enrollment data from the device first...");

                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByDeviceId(_selectedDeviceId);
                        _dbHelper.DeleteDevice(_selectedDeviceId);
                    }

                    SetupDeviceList(); // Refresh the device list after deletion
                    ClearDeviceFields(); // Clear the fields after deletion
                    listViewDevices.SelectedItems.Clear();
                    update_session(); // Update session with device data

                    // Notify dashboard to refresh
                    NotifyDashboardRefresh();

                    // Add AI messages after successful removal
                    aiChatLogger.AddSuccessMessage($"Device from {txt_location.Text} has been successfully removed!");
                    aiChatLogger.AddInfoMessage("Don't worry, I'm still here to help with your remaining devices.");
                    aiChatLogger.AddDeviceMessage("Device network updated. All enrollment data has been cleared.");

                    MessageBox.Show("Device deleted successfully.");
                }
                else
                {
                    MessageBox.Show("Authentication failed. Device was not deleted.");
                }
            }
            else
            {
                ClearDeviceFields(); // Clear the fields if the user cancels
                listViewDevices.SelectedItems.Clear();
                MessageBox.Show("Device deletion cancelled.");
            }
        }


        private void txt_Device_id_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_department_TextChanged(object sender, EventArgs e)
        {

        }


        private void cmb_device_type_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txt_Serial_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_ip_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_port_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_timeZone_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_location_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmb_connectionType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txt_device_status_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_last_synced_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_face_capacity_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_RFID_capacity_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_finger_capacity_TextChanged(object sender, EventArgs e)
        {


        }

        private void btn_editUser_Click(object sender, EventArgs e)
        {
        }

        private void txtAPIkey_TextChanged(object sender, EventArgs e)
        {

        }


        private async void btnSaveAPI_Click_1(object sender, EventArgs e)
        {
            string apiKey = txtAPIkey.Text.Trim();

            // Step 1: Basic validation
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                lbl_apikey_status.Text = "API Key is required.";
                MessageBox.Show("Please enter a valid API Key.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Validate format: Must contain colon (GYMID:SECRETKEY)
            if (!apiKey.Contains(":") || apiKey.Split(':').Length != 2)
            {
                lbl_apikey_status.Text = "Invalid API Key format.";
                MessageBox.Show("API Key format is invalid. Expected format: gymId:secretKey", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Split on the first colon only
                int sepIndex = apiKey.IndexOf(':');
                if (sepIndex <= 0 || sepIndex == apiKey.Length - 1)
                {
                    MessageBox.Show("Invalid API key format. Expected '<gymId>:<jwt>'.", "Invalid Format",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string gymId = apiKey.Substring(0, sepIndex).Trim();
                string jwt = apiKey.Substring(sepIndex + 1).Trim();

                if (string.IsNullOrWhiteSpace(gymId) || string.IsNullOrWhiteSpace(jwt))
                {
                    MessageBox.Show("Gym ID or token is empty after split.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Persist all parts
                _dbHelper.SaveSetting("APIKey", apiKey);
                _dbHelper.SaveSetting("GymId", gymId);
                _dbHelper.SaveSetting("GWTToken", jwt);

                LogHelper.Log($"[API Key Saved] GymId='{gymId}', JWT length={jwt.Length}.");
                MessageBox.Show("API key saved.\nGym ID and GWT token stored successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[btnSaveAPI_Click_1][ERROR] {ex.Message}");
                MessageBox.Show("Failed to save API key. See logs for details.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btn_addUser_Click(object sender, EventArgs e)
        {
            //this.Close();
            Thread newThread = new Thread(Newuser);
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }

        //Abdul Hadi Code
        private void Newuser()
        {
            AddNewUser newuser = new AddNewUser();
            Application.Run(newuser);
        }

        private int? selectedUserId = null;
        private string selectedUsername = null;
        private string selectedRole = null;



        //Abdul Hadi Code
        //Event to select an item in the list and pass its id
        private void list_LocalUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (list_LocalUser.SelectedItems.Count > 0)
            {
                var selectedItem = list_LocalUser.SelectedItems[0];

                if (int.TryParse(selectedItem.Text, out int userId))
                {
                    selectedUserId = userId;
                    selectedUsername = selectedItem.SubItems[1].Text;
                    selectedRole = selectedItem.SubItems[2].Text;

                    btn_deleteUser.Enabled = true;
                    btn_change_password.Enabled = true;
                    btn_editUser.Enabled = true;
                }
            }
            else
            {
                selectedUserId = null;
                selectedUsername = null;
                selectedRole = null;
                btn_deleteUser.Enabled = false;
                btn_change_password.Enabled = false;
                btn_editUser.Enabled = false;
            }
        }


        //Abdul Hadi Code
        //Event to delete the selected user
        private void btn_deleteUser_Click(object sender, EventArgs e)
        {

            if (AppSession.UserRole == "admin" && AppSession.LoggedInUserID != selectedUserId.Value)
            {
                if (selectedUserId.HasValue)
                {
                    var confirm = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm == DialogResult.Yes)
                    {
                        // Add AI message before deletion
                        aiChatLogger.AddUserMessage($"Removing user {selectedUsername} from the system...");

                        _dbHelper.DeleteUserById(selectedUserId.Value);
                        LoadLocalUserData(); // Refresh list
                        selectedUserId = null;

                        // Add AI message after successful deletion
                        aiChatLogger.AddSuccessMessage($"User {selectedUsername} has been successfully removed from the system.");
                        aiChatLogger.AddInfoMessage("User management completed. The team roster has been updated.");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a user to delete.", "No User Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("You cannot delete your own account.");
                return;

            }

        }

        //Abdul Hadi Code
        //Event to change the password of the selected user
        private void btn_change_password_Click(object sender, EventArgs e)
        {
            if (selectedUserId.HasValue)
            {
                using (Form prompt = new Form())
                {
                    prompt.Width = 400;
                    prompt.Height = 220;
                    prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                    prompt.Text = "Change Password";
                    prompt.StartPosition = FormStartPosition.CenterScreen;

                    Label textLabel1 = new Label() { Left = 20, Top = 20, Text = "Enter new password (min 8 characters):", Width = 340 };
                    TextBox textBox1 = new TextBox() { Left = 20, Top = 50, Width = 340, PasswordChar = '*' };

                    Label textLabel2 = new Label() { Left = 20, Top = 85, Text = "Confirm new password:", Width = 340 };
                    TextBox textBox2 = new TextBox() { Left = 20, Top = 110, Width = 340, PasswordChar = '*' };

                    Button confirmation = new Button() { Text = "OK", Left = 270, Width = 90, Top = 145, DialogResult = DialogResult.OK };
                    confirmation.Click += (sender2, e2) => { prompt.Close(); };

                    prompt.Controls.Add(textLabel1);
                    prompt.Controls.Add(textBox1);
                    prompt.Controls.Add(textLabel2);
                    prompt.Controls.Add(textBox2);
                    prompt.Controls.Add(confirmation);
                    prompt.AcceptButton = confirmation;

                    if (prompt.ShowDialog() == DialogResult.OK)
                    {
                        string newPassword = textBox1.Text;
                        string confirmPassword = textBox2.Text;

                        if (newPassword.Length < 8)
                        {
                            MessageBox.Show("Password must be at least 8 characters long.");
                            return;
                        }

                        if (newPassword != confirmPassword)
                        {
                            MessageBox.Show("Passwords do not match. Please try again.");
                            return;
                        }

                        DialogResult confirm = MessageBox.Show("Are you sure you want to change the password?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (confirm == DialogResult.Yes)
                        {
                            _dbHelper.UpdateUserPassword(selectedUserId.Value, newPassword);
                            MessageBox.Show("Password changed successfully.");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to change password.");
            }
        }



        //Abdul Hadi Code
        //Event to edit the selected user
        private void btn_editUser_Click_1(object sender, EventArgs e)
        {
            if (selectedUserId.HasValue && !string.IsNullOrEmpty(selectedUsername) && !string.IsNullOrEmpty(selectedRole))
            {
                using (EditUser editForm = new EditUser(selectedUserId.Value, selectedUsername, selectedRole))
                {
                    editForm.ShowDialog(); // Logic and DB update handled in EditUser form
                }

                LoadLocalUserData(); // Refresh the local user list after editing
            }
            else
            {
                MessageBox.Show("Please select a user to edit.");
            }
        }

        private void cmb_machine_no_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lbl_password_Click(object sender, EventArgs e)
        {

        }

        private void txt_password_TextChanged(object sender, EventArgs e)
        {

        }

        private void listViewDevices_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private async void CheckAllDeviceConnections()
        {
            // Copy the items to a local list on the UI thread
            var items = new List<ListViewItem>();
            foreach (ListViewItem item in listViewDevices.Items)
                items.Add(item);

            await Task.Run(async () =>
            {
                foreach (var item in items)
                {
                    try
                    {
                        string ip = item.SubItems[3].Text;
                        int port = int.Parse(item.SubItems[4].Text);
                        int machineNo = int.Parse(item.SubItems[1].Text);
                        string password = "0"; // Or fetch from DB/device

                        string serial = await DeviceService.GetSerial(ip, port, machineNo, password);

                        // Update UI on the UI thread
                        this.Invoke(new Action(() =>
                        {
                            item.SubItems[6].Text = "Connected";
                            item.SubItems[6].ForeColor = Color.Green;
                        }));
                    }
                    catch
                    {
                        // Handle error, optionally update UI
                    }
                }
            });
        }

        private void NotifyDashboardRefresh()
        {
            try
            {
                // Find the Dashboard form and refresh its device status
                foreach (Form form in Application.OpenForms)
                {
                    if (form is Dashboard dashboard)
                    {
                        dashboard.Invoke(new Action(async () => await dashboard.RefreshDeviceStatus()));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't interrupt the current operation
                LogHelper.Log($"Error notifying dashboard refresh: {ex.Message}");
            }
        }


        private async void DeviceManager_Shown(object sender, EventArgs e)
        {
            CheckAllDeviceConnections();

        }

        private void btnApiPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                txtAPIkey.Text = Clipboard.GetText();
            }
            else
            {
                MessageBox.Show("Clipboard does not contain any text.", "Paste Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private async void btn_format_Click(object sender, EventArgs e)
        {
            // Require a selection
            if (listViewDevices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to format.", "No Device Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Resolve the selected device object from Tag
            var selectedItem = listViewDevices.SelectedItems[0];
            if (!(selectedItem.Tag is Device selectedDevice))
            {
                MessageBox.Show("Selected row is invalid.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate common fields
            if (string.IsNullOrEmpty(txt_ip.Text) || string.IsNullOrEmpty(txt_port.Text) || cmb_machine_no.SelectedItem == null)
            {
                MessageBox.Show("Missing device information. Please select a valid device.", "Incomplete Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Auth
            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                MessageBox.Show("No user is currently logged in. Cannot proceed.");
                return;
            }

            var confirmResult = MessageBox.Show(
                "WARNING: This will delete ALL user data from the device including fingerprints, face templates, and RFID card information.\n\n" +
                "This action cannot be undone. Are you sure you want to continue?",
                "Format Device Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes) return;

            using (Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Confirm Format - {currentUser} Login",
                StartPosition = FormStartPosition.CenterScreen
            })
            {
                var label = new Label { Left = 10, Top = 20, Width = 320, Text = "Enter your password to format the device:" };
                var inputBox = new TextBox { Left = 10, Top = 50, Width = 320, UseSystemPasswordChar = true };
                var okButton = new Button { Text = "Format Device", Left = 150, Width = 100, Top = 85, DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Cancel", Left = 260, Width = 70, Top = 85, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(label);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(okButton);
                prompt.Controls.Add(cancelButton);
                prompt.AcceptButton = okButton;
                prompt.CancelButton = cancelButton;

                if (prompt.ShowDialog() != DialogResult.OK) return;

                string enteredPassword = inputBox.Text;
                if (!_dbHelper.VerifyLoginUser(currentUser, enteredPassword))
                {
                    MessageBox.Show("Authentication failed. Device formatting was cancelled.");
                    return;
                }
            }

            try
            {
                // Extract device info
                string ip = txt_ip.Text.Trim();
                int port = int.Parse(txt_port.Text.Trim());
                int machineNo = int.Parse(cmb_machine_no.SelectedItem.ToString());
                string password = txt_password.Text.Trim();

                // Reachability
                if (!IsValidIp(ip))
                {
                    MessageBox.Show("The device is not reachable. Please check the connection and try again.",
                        "Device Unreachable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Cursor = Cursors.WaitCursor;

                // Perform wipe on device (same API for both types)
                bool success = await DeviceService.EmptyEnrollmentData(ip, port, machineNo, password);

                // Optional: also clear local logs per device type
                if (success)
                {
                    // Determine staff vs normal via StaffDeviceId presence
                    bool isStaff = selectedDevice.StaffDeviceId > 0;

                    if (isStaff)
                    {
                        // Staff-specific cleanup in DB/logs
                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId(selectedDevice.StaffDeviceId);
                    }
                    else
                    {
                        // Normal device cleanup in DB/logs
                        await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByDeviceId(selectedDevice.DeviceId);
                    }

                    MessageBox.Show("Device has been successfully formatted. All user data has been removed.",
                        "Format Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to format device. Please try again.",
                        "Format Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while formatting the device: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;

                // Refresh UI/session
                SetupDeviceList();
                ClearDeviceFields();
                listViewDevices.SelectedItems.Clear();
                update_session();
                NotifyDashboardRefresh();
            }
        }


        /////////////////////////MEMBER MANAGMENT///////////////////////////////

        // Call this in DeviceManager_Load after InitializeMemberList()
        private void InitializeMembersListView()
        {
            // Configure lst_members
            lst_members.Clear();
            lst_members.View = View.Details;
            lst_members.FullRowSelect = true;
            lst_members.GridLines = true;

            // Columns
            lst_members.Columns.Add("Member ID", 90);
            lst_members.Columns.Add("Name", 160);
            lst_members.Columns.Add("Enrollment No", 110);
            lst_members.Columns.Add("Status", 90);
            lst_members.Columns.Add("RFID", 120);
            lst_members.Columns.Add("Picture", 160);
            lst_members.Columns.Add("Fingerprint", 100);
            lst_members.Columns.Add("Sync Status", 140);
            lst_members.Columns.Add("Package Expiry", 130);
            lst_members.Columns[lst_members.Columns.Count - 1].Width = -2;

            LoadMembersIntoListView(null);
        }

        // Reuse your function as-is for txt_memb_name dynamic suggestions:
        // Map txt_memb_name to txt_membername internally (if different control names exist)
        private void InitializeMemberListForStats()
        {
            // Load all members from the database
            List<Member> members = _dbHelper.GetMembers();
            LogHelper.Log($"Loaded {members.Count} members from the database.");

            // Configure the AutoCompleteSource and AutoCompleteMode
            txt_memb_name.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txt_memb_name.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Create a StringCollection to store member names
            AutoCompleteStringCollection memberCollection = new AutoCompleteStringCollection();

            // Add each member's name to the collection
            foreach (var member in members)
            {
                memberCollection.Add(member.Name);
            }

            // Assign the collection to the AutoCompleteCustomSource
            txt_memb_name.AutoCompleteCustomSource = memberCollection;
            LogHelper.Log("Member list initialized with suggestions.");
        }

        // Button click handler for __flt_members__ to filter by name
        private void flt_members_Click(object sender, EventArgs e)
        {
            string nameFilter = txt_memb_name.Text?.Trim();
            LoadMembersIntoListView(string.IsNullOrEmpty(nameFilter) ? null : nameFilter);
        }

        // Core loader: populates lst_members with full data and optional name filter
        private void LoadMembersIntoListView(string nameFilter)
        {
            try
            {
                lst_members.BeginUpdate();
                lst_members.Items.Clear();

                var members = _dbHelper.GetMembers() ?? new List<Member>();

                // Apply name filter if provided
                if (!string.IsNullOrWhiteSpace(nameFilter))
                {
                    members = members.Where(m => !string.IsNullOrWhiteSpace(m.Name) &&
                                                 m.Name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                                     .ToList();
                }

                // For sync status across devices
                var allDevices = AppSession.DeviceList ?? new List<Device>();
                int totalDevices = allDevices.Count;

                foreach (var m in members)
                {
                    // Resolve fields safely
                    string memberIdText = m.MemberID.ToString();
                    string nameText = string.IsNullOrWhiteSpace(m.Name) ? "-" : m.Name;
                    string enrollmentText = m.EnrollmentID.HasValue && m.EnrollmentID.Value > 0
                        ? m.EnrollmentID.Value.ToString()
                        : "-";
                    string statusText = string.IsNullOrWhiteSpace(m.Status) ? "-" : m.Status;
                    string rfidText = string.IsNullOrWhiteSpace(m.RFID) ? "-" : m.RFID;
                    string pictureText = string.IsNullOrWhiteSpace(m.Picture) ? "-" : System.IO.Path.GetFileName(m.Picture);

                    // Fingerprint flag: try common property names; fall back to "-"
                    string fingerprintText = "-";
                    try
                    {
                        // If Member has a boolean or flag indicating fingerprint presence
                        // Examples: m.HasFingerprint, m.FingerprintFlag, m.FingerprintCount
                        var hasFpProp = m.GetType().GetProperty("HasFingerprint");
                        var fpFlagProp = m.GetType().GetProperty("FingerprintFlag");
                        var fpCountProp = m.GetType().GetProperty("FingerprintCount");

                        if (hasFpProp != null)
                        {
                            bool hasFp = (bool)(hasFpProp.GetValue(m) ?? false);
                            fingerprintText = hasFp ? "Yes" : "No";
                        }
                        else if (fpFlagProp != null)
                        {
                            var val = fpFlagProp.GetValue(m);
                            fingerprintText = (val != null && val.ToString() == "1") ? "Yes" : "No";
                        }
                        else if (fpCountProp != null)
                        {
                            int cnt = Convert.ToInt32(fpCountProp.GetValue(m) ?? 0);
                            fingerprintText = cnt > 0 ? $"Yes ({cnt})" : "No";
                        }
                    }
                    catch { /* ignore reflection issues */ }

                    // Sync status across all devices
                    string syncStatusText = "-";
                    if (totalDevices > 0)
                    {
                        int syncedCount = 0;
                        foreach (var d in allDevices)
                        {
                            try
                            {
                                var (deviceExists, syncStatus, _) = _dbHelper.GetMemberDeviceSyncStatus(m.MemberID, d.DeviceId);
                                if (deviceExists && syncStatus) syncedCount++;
                            }
                            catch { /* ignore per-device errors */ }
                        }
                        syncStatusText = $"{syncedCount}/{totalDevices} devices";
                    }

                    // Package expiry (try common property names)
                    string pkgExpiryText = "-";
                    try
                    {
                        var expiryProp = m.GetType().GetProperty("PackageExpiry") ?? m.GetType().GetProperty("ExpiryDate");
                        if (expiryProp != null)
                        {
                            var val = expiryProp.GetValue(m);
                            if (val is DateTime dt)
                                pkgExpiryText = dt.ToString("yyyy-MM-dd");
                            else if (val != null)
                                pkgExpiryText = val.ToString();
                        }
                    }
                    catch { /* ignore */ }

                    var item = new ListViewItem(memberIdText);
                    item.SubItems.Add(nameText);
                    item.SubItems.Add(enrollmentText);
                    item.SubItems.Add(statusText);
                    item.SubItems.Add(rfidText);
                    item.SubItems.Add(pictureText);
                    item.SubItems.Add(fingerprintText);
                    item.SubItems.Add(syncStatusText);
                    item.SubItems.Add(pkgExpiryText);
                    item.Tag = m;

                    lst_members.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load members: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lst_members.EndUpdate();
            }
        }

        // Refresh suggestions when text changes significantly (debounced if needed)
        private void txt_memb_name_TextChanged(object sender, EventArgs e)
        {
            // No-op; AutoComplete handles suggestions using InitializeMemberList()
            // If you want live filtering as you type:
            var filter = txt_memb_name.Text?.Trim();
            LoadMembersIntoListView(string.IsNullOrEmpty(filter) ? null : filter);
        }

        private void InitializeMemberList()
        {
            // Load all members from the database
            List<Member> members = _dbHelper.GetMembers();
            LogHelper.Log($"Loaded {members.Count} members from the database.");

            // Configure the AutoCompleteSource and AutoCompleteMode
            txt_membername.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txt_membername.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Create a StringCollection to store member names
            AutoCompleteStringCollection memberCollection = new AutoCompleteStringCollection();

            // Add each member's name to the collection
            foreach (var member in members)
            {
                memberCollection.Add(member.Name);
            }

            // Assign the collection to the AutoCompleteCustomSource
            txt_membername.AutoCompleteCustomSource = memberCollection;
            LogHelper.Log("Member list initialized with suggestions.");
        }


        private void txt_membername_TextChanged(object sender, EventArgs e)
        {

        }

        private void date_member_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btn_filter_member_Click(object sender, EventArgs e)
        {
            LoadFilteredMemberAttendanceLogs();
        }

        private void LoadFilteredMemberAttendanceLogs()
        {
            lst_member.Items.Clear();
            lst_member.View = View.Details;
            lst_member.FullRowSelect = true;
            lst_member.GridLines = true;

            lst_member.Columns.Clear();
            lst_member.Columns.Add("Log ID", 70);
            lst_member.Columns.Add("Member ID", 80);
            lst_member.Columns.Add("Member Name", 120);
            lst_member.Columns.Add("Check-In Time", 150);
            lst_member.Columns.Add("Check-Out Time", 150);
            lst_member.Columns[lst_member.Columns.Count - 1].Width = -2;

            // Get filter criteria
            string memberNameFilter = txt_membername.Text.Trim();
            DateTime? selectedDate = date_member.Checked ? date_member.Value.Date : (DateTime?)null;

            var attendanceLogs = _dbHelper.GetAllAttendanceLogs();

            // Apply filters
            var filteredLogs = attendanceLogs.Where(log =>
            {
                // Filter by member name
                if (!string.IsNullOrEmpty(memberNameFilter))
                {
                    string memberName = _dbHelper.GetMemberName(log.MemberID);
                    if (string.IsNullOrEmpty(memberName) || !memberName.Contains(memberNameFilter))
                    {
                        return false;
                    }
                }

                // Filter by date
                if (selectedDate.HasValue)
                {
                    DateTime? checkInTime = ParseDateTime(log.CheckInTime);
                    DateTime? checkOutTime = ParseDateTime(log.CheckOutTime);

                    if ((!checkInTime.HasValue || checkInTime.Value.Date != selectedDate.Value) &&
                        (!checkOutTime.HasValue || checkOutTime.Value.Date != selectedDate.Value))
                    {
                        return false;
                    }
                }

                return true;
            }).ToList();

            foreach (var log in filteredLogs)
            {
                var item = new ListViewItem(log.LogID.ToString());
                item.SubItems.Add(log.MemberID.ToString());

                // Fetch member name from the database
                string memberName = _dbHelper.GetMemberName(log.MemberID);
                item.SubItems.Add(string.IsNullOrWhiteSpace(memberName) ? "-" : memberName);

                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckInTime) ? "-" : log.CheckInTime);
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckOutTime) ? "-" : log.CheckOutTime);
                lst_member.Items.Add(item);
            }
        }

        private DateTime? ParseDateTime(string dateTimeString)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                return null;
            }

            if (DateTime.TryParse(dateTimeString, out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }

            return null;
        }


        private void lst_member_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMemberAttendanceLogs();
        }

        private void LoadMemberAttendanceLogs()
        {
            lst_member.Items.Clear();
            lst_member.View = View.Details;
            lst_member.FullRowSelect = true;
            lst_member.GridLines = true;

            lst_member.Columns.Clear();
            lst_member.Columns.Add("Log ID", 70);
            lst_member.Columns.Add("Member ID", 80);
            lst_member.Columns.Add("Member Name", 120);
            lst_member.Columns.Add("Check-In Time", 150);
            lst_member.Columns.Add("Check-Out Time", 150);
            lst_member.Columns[lst_member.Columns.Count - 1].Width = -2;

            var attendanceLogs = _dbHelper.GetAllAttendanceLogs();

            foreach (var log in attendanceLogs)
            {
                var item = new ListViewItem(log.LogID.ToString());
                item.SubItems.Add(log.MemberID.ToString()); // Changed from log.EnrollmentID.ToString()

                // Fetch member name from the database
                string memberName = _dbHelper.GetMemberName(log.MemberID);
                item.SubItems.Add(string.IsNullOrWhiteSpace(memberName) ? "-" : memberName); // Add member name to the subitems

                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckInTime) ? "-" : log.CheckInTime);
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckOutTime) ? "-" : log.CheckOutTime);
                lst_member.Items.Add(item);
            }
        }

        private void date_manualdoor_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btn_filter_manualdoor_Click(object sender, EventArgs e)
        {
            LoadFilteredManualDoorLogs();
        }

        private void LoadFilteredManualDoorLogs()
        {
            lst_manualdoor.Items.Clear();
            lst_manualdoor.View = View.Details;
            lst_manualdoor.FullRowSelect = true;
            lst_manualdoor.GridLines = true;

            lst_manualdoor.Columns.Clear();
            lst_manualdoor.Columns.Add("Gate ID", 50);
            lst_manualdoor.Columns.Add("Device IP", 100);
            lst_manualdoor.Columns.Add("Member ID", 80);
            lst_manualdoor.Columns.Add("Member Name", 120);
            lst_manualdoor.Columns.Add("Is Visitor", 70);
            lst_manualdoor.Columns.Add("Visitor Name", 120);
            lst_manualdoor.Columns.Add("Visitor Phone", 100);
            lst_manualdoor.Columns.Add("Reason", 150);
            lst_manualdoor.Columns.Add("Accessed Time", 150);
            lst_manualdoor.Columns.Add("Username", 80);
            lst_manualdoor.Columns[lst_manualdoor.Columns.Count - 1].Width = -2;

            DateTime selectedDate = date_manualdoor.Value.Date; // Get the date part

            var gateAccessLogs = _dbHelper.GetAllGateAccesseds();

            // Apply date filter
            var filteredLogs = gateAccessLogs.Where(log => log.AccessedTime.Date == selectedDate).ToList();

            foreach (var log in filteredLogs)
            {
                var item = new ListViewItem(log.GateId.ToString());
                item.SubItems.Add(log.DeviceIP);
                item.SubItems.Add(log.MemberId.ToString());

                // Fetch member name from the database
                string memberName = _dbHelper.GetMemberName(log.MemberId);
                item.SubItems.Add(string.IsNullOrWhiteSpace(memberName) ? "-" : memberName);

                item.SubItems.Add(log.IsVisitor.ToString());
                item.SubItems.Add(log.VisitorName);
                item.SubItems.Add(log.VisitorPhone);
                item.SubItems.Add(log.Reason);
                item.SubItems.Add(log.AccessedTime.ToString());
                item.SubItems.Add(log.Username);
                lst_manualdoor.Items.Add(item);
            }
        }

        private void lst_manualdoor_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadManualDoorLogs();
        }

        private void LoadManualDoorLogs()
        {
            lst_manualdoor.Items.Clear();
            lst_manualdoor.View = View.Details;
            lst_manualdoor.FullRowSelect = true;
            lst_manualdoor.GridLines = true;

            lst_manualdoor.Columns.Clear();
            lst_manualdoor.Columns.Add("Gate ID", 50);
            lst_manualdoor.Columns.Add("Device IP", 100);
            lst_manualdoor.Columns.Add("Member ID", 80);
            lst_manualdoor.Columns.Add("Member Name", 120); // Add Member Name column
            lst_manualdoor.Columns.Add("Is Visitor", 70);
            lst_manualdoor.Columns.Add("Visitor Name", 120);
            lst_manualdoor.Columns.Add("Visitor Phone", 100);
            lst_manualdoor.Columns.Add("Reason", 150);
            lst_manualdoor.Columns.Add("Accessed Time", 150);
            lst_manualdoor.Columns.Add("Username", 80);
            lst_manualdoor.Columns[lst_manualdoor.Columns.Count - 1].Width = -2;

            var gateAccessLogs = _dbHelper.GetAllGateAccesseds();

            foreach (var log in gateAccessLogs)
            {
                var item = new ListViewItem(log.GateId.ToString());
                item.SubItems.Add(log.DeviceIP);
                item.SubItems.Add(log.MemberId.ToString());

                // Fetch member name from the database
                string memberName = _dbHelper.GetMemberName(log.MemberId);
                item.SubItems.Add(string.IsNullOrWhiteSpace(memberName) ? "-" : memberName); // Add member name

                item.SubItems.Add(log.IsVisitor.ToString());
                item.SubItems.Add(log.VisitorName);
                item.SubItems.Add(log.VisitorPhone);
                item.SubItems.Add(log.Reason);
                item.SubItems.Add(log.AccessedTime.ToString());
                item.SubItems.Add(log.Username);
                lst_manualdoor.Items.Add(item);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////



        private void pnl_device_Paint(object sender, PaintEventArgs e)
        {

        }



        /////////////////////////////////SETUP/////////////////////////////////////////
        private void btn_savedebounce_Click(object sender, EventArgs e)
        {
            try
            {
                // minutes -> seconds
                int minutes = (int)num_debounce.Value;
                if (minutes < 0)
                {
                    MessageBox.Show("Debounce minutes cannot be negative.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int seconds = checked(minutes * 60);

                // Persist as integer (string storage), key: AttendanceDebounceSeconds
                _dbHelper.SaveSetting("AttendanceDebounceSeconds", seconds.ToString());

                LogHelper.Log($"[Debounce] Saved AttendanceDebounceSeconds = {seconds}");
                MessageBox.Show("Debounce setting saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Debounce value is too large.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[Debounce][ERROR] {ex.Message}");
                MessageBox.Show("Failed to save debounce setting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void num_debounce_ValueChanged(object sender, EventArgs e)
        {

        }



        private void txt_enrollid_TextChanged(object sender, EventArgs e)
        {

        }

        private async void btn_getinfo_Click(object sender, EventArgs e)
        {
            string memberIdText = txt_enrollid.Text.Trim();
            if (string.IsNullOrWhiteSpace(memberIdText) || !int.TryParse(memberIdText, out int memberId))
            {
                MessageBox.Show("Please enter a valid numeric member ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get member and resolve EnrollmentID from MemberID
            var member = _dbHelper.GetMember(memberId);
            if (member == null)
            {
                MessageBox.Show("No member found for the provided member ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!member.EnrollmentID.HasValue || member.EnrollmentID.Value <= 0)
            {
                MessageBox.Show("This member does not have an Enrollment ID assigned.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int enrollmentId = member.EnrollmentID.Value;

            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null)
                allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null)
                allDevices.AddRange(AppSession.StaffDeviceList);

            if (allDevices.Count == 0)
            {
                MessageBox.Show("No devices available in the session.", "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string memberNameFromDb = !string.IsNullOrWhiteSpace(member.Name)
                ? member.Name
                : (_dbHelper.GetMemberName(memberId) ?? string.Empty);

            LogHelper.Log($"Starting GetUserInfo for EnrollmentID={enrollmentId} (from MemberID={memberId})");

            var results = new List<string>();
            foreach (var device in allDevices)
            {
                try
                {
                    // Use EnrollmentID for device lookup
                    var userInfo = await DeviceService.GetUserInfo(device.DeviceIP, device.Port, device.MachineNo, device.Password, enrollmentId.ToString());

                    // Prefer DB name if available; else use device info
                    string resolvedName = !string.IsNullOrWhiteSpace(memberNameFromDb)
                        ? memberNameFromDb
                        : (string.IsNullOrWhiteSpace(userInfo.UserName) ? "(name not found)" : userInfo.UserName);

                    string formattedInfo = $"Alias ID: {userInfo.AliasId}\n" +
                                           $"User Name: {resolvedName}\n" +
                                           $"Card Number: {userInfo.CardNumber}\n" +
                                           $"Face Flag: {userInfo.FaceFlag}\n" +
                                           $"FP Flag: {userInfo.FPFlag}\n" +
                                           $"Enabled: {userInfo.Enabled}\n" +
                                           $"Privilege: {userInfo.Privilege}";

                    string deviceLabel = !string.IsNullOrWhiteSpace(device.Location) ? device.Location : device.DeviceIP;
                    results.Add($"Device {deviceLabel} (IP: {device.DeviceIP}):\n{formattedInfo}");

                    LogHelper.Log($"Successfully retrieved user info for EnrollmentID={enrollmentId} (MemberID={memberId}) on device {deviceLabel} (IP: {device.DeviceIP})");
                }
                catch (Exception ex)
                {
                    string deviceLabel = !string.IsNullOrWhiteSpace(device.Location) ? device.Location : device.DeviceIP;
                    results.Add($"Device {deviceLabel} (IP: {device.DeviceIP}): Error - {ex.Message}");
                    LogHelper.Log($"Error retrieving user info for EnrollmentID={enrollmentId} (MemberID={memberId}) on device {deviceLabel} (IP: {device.DeviceIP}): {ex.Message}");
                }
            }

            string message = string.Join("\n\n", results);
            MessageBox.Show(message, "User Info Results", MessageBoxButtons.OK, MessageBoxIcon.Information);

            LogHelper.Log($"Completed GetUserInfo for EnrollmentID={enrollmentId} (MemberID={memberId}), processed {allDevices.Count} devices");
        }

        private async void btn_getdb_Click(object sender, EventArgs e)
        {
            btn_getdb.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                LogHelper.Log("Starting backup bundle (DB snapshot + Logs + GLogs) email.");

                bool ok = await CommonUtils.CreateAndEmailDbLogsAndGLogsAsync(_dbHelper);

                if (ok)
                {
                    MessageBox.Show(
                        "Backup bundle has been sent via email (DB snapshot + Logs ZIP + GLogs ZIP).",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Failed to send backup bundle email. Check Logs for details.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error in btn_getdb_Click: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btn_getdb.Enabled = true;
            }
        }

        private async void btn_enableuser_Click(object sender, EventArgs e)
        {
            btn_enableuser.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                string memberIdText = txt_enrollid?.Text?.Trim();
                if (string.IsNullOrWhiteSpace(memberIdText) || !int.TryParse(memberIdText, out int memberId))
                {
                    MessageBox.Show("Please enter a valid numeric member ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Resolve member and EnrollmentID
                var member = _dbHelper.GetMember(memberId);
                if (member == null)
                {
                    MessageBox.Show("No member found for the provided member ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!member.EnrollmentID.HasValue || member.EnrollmentID.Value <= 0)
                {
                    MessageBox.Show("This member does not have an Enrollment ID assigned.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int enrollmentId = member.EnrollmentID.Value;
                string memberNameFromDb = !string.IsNullOrWhiteSpace(member.Name)
                    ? member.Name
                    : (_dbHelper.GetMemberName(memberId) ?? string.Empty);

                var allDevices = new List<Device>();
                if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
                if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

                if (allDevices.Count == 0)
                {
                    MessageBox.Show("No devices available in the session.", "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var reportLines = new List<string>();

                foreach (var device in allDevices)
                {
                    try
                    {
                        string ip = device.DeviceIP;
                        int port = device.Port;
                        int machineNo = device.MachineNo;
                        string password = device.Password ?? "0";

                        // Read current user name using EnrollmentID
                        string userNameDevice = await DeviceService.GetUserName(ip, port, machineNo, password, enrollmentId, machineNo);

                        // Enable using EnrollmentID
                        bool enabled = await DeviceService.EnableUser(ip, port, machineNo, password, enrollmentId, machineNo, 12);

                        string deviceLabel = !string.IsNullOrWhiteSpace(device.Location) ? device.Location : ip;
                        string resolvedName = !string.IsNullOrWhiteSpace(memberNameFromDb)
                            ? memberNameFromDb
                            : (string.IsNullOrWhiteSpace(userNameDevice) ? "(name not found)" : userNameDevice);
                        string status = enabled ? "Enabled" : "Enable FAILED";

                        reportLines.Add($"{deviceLabel} [{ip}]: {resolvedName} (MemberID={memberId}, EnrollmentID={enrollmentId}) -> {status}");
                    }
                    catch (Exception ex)
                    {
                        string deviceLabel = !string.IsNullOrWhiteSpace(device?.Location) ? device.Location : device?.DeviceIP ?? "Unknown device";
                        reportLines.Add($"{deviceLabel}: Error - {ex.Message}");
                        try { LogHelper.Log($"btn_enableuser_Click: Error enabling on {device?.DeviceIP}: {ex.Message}"); } catch { }
                    }
                }

                string message = string.Join(Environment.NewLine, reportLines);
                MessageBox.Show(message, "Enable User Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                btn_enableuser.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private async void btn_disableuser_Click(object sender, EventArgs e)
        {
            btn_disableuser.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                string memberIdText = txt_enrollid?.Text?.Trim();
                if (string.IsNullOrWhiteSpace(memberIdText) || !int.TryParse(memberIdText, out int memberId))
                {
                    MessageBox.Show("Please enter a valid numeric member ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Resolve member and EnrollmentID
                var member = _dbHelper.GetMember(memberId);
                if (member == null)
                {
                    MessageBox.Show("No member found for the provided member ID.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!member.EnrollmentID.HasValue || member.EnrollmentID.Value <= 0)
                {
                    MessageBox.Show("This member does not have an Enrollment ID assigned.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int enrollmentId = member.EnrollmentID.Value;
                string memberNameFromDb = !string.IsNullOrWhiteSpace(member.Name)
                    ? member.Name
                    : (_dbHelper.GetMemberName(memberId) ?? string.Empty);

                var allDevices = new List<Device>();
                if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
                if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

                if (allDevices.Count == 0)
                {
                    MessageBox.Show("No devices available in the session.", "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var reportLines = new List<string>();

                foreach (var device in allDevices)
                {
                    try
                    {
                        string ip = device.DeviceIP;
                        int port = device.Port;
                        int machineNo = device.MachineNo;
                        string password = device.Password ?? "0";

                        // Read current user name using EnrollmentID
                        string userNameDevice = await DeviceService.GetUserName(ip, port, machineNo, password, enrollmentId, machineNo);

                        // Disable using EnrollmentID
                        bool disabled = await DeviceService.DisableUser(ip, port, machineNo, password, enrollmentId, machineNo, 12);

                        string deviceLabel = !string.IsNullOrWhiteSpace(device.Location) ? device.Location : ip;
                        string resolvedName = !string.IsNullOrWhiteSpace(memberNameFromDb)
                            ? memberNameFromDb
                            : (string.IsNullOrWhiteSpace(userNameDevice) ? "(name not found)" : userNameDevice);
                        string status = disabled ? "Disabled" : "Disable FAILED";

                        reportLines.Add($"{deviceLabel} [{ip}]: {resolvedName} (MemberID={memberId}, EnrollmentID={enrollmentId}) -> {status}");
                    }
                    catch (Exception ex)
                    {
                        string deviceLabel = !string.IsNullOrWhiteSpace(device?.Location) ? device.Location : device?.DeviceIP ?? "Unknown device";
                        reportLines.Add($"{deviceLabel}: Error - {ex.Message}");
                        try { LogHelper.Log($"btn_disableuser_Click: Error disabling on {device?.DeviceIP}: {ex.Message}"); } catch { }
                    }
                }

                string message = string.Join(Environment.NewLine, reportLines);
                MessageBox.Show(message, "Disable User Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                btn_disableuser.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void btn_settime_Click(object sender, EventArgs e)
        {
            try
            {
                // Get current local time and offset
                var nowLocal = DateTime.Now;
                var nowOffset = DateTimeOffset.Now.Offset; // includes DST if applicable

                // Build GMT string: "+HH:mm" or "-HH:mm"
                string sign = nowOffset < TimeSpan.Zero ? "-" : "+";
                var abs = nowOffset.Duration();
                string gmt = $"{sign}{abs.Hours:D2}:{abs.Minutes:D2}";

                // Persist settings
                _dbHelper.SaveSetting("LocalTime", nowLocal.ToString("yyyy-MM-dd HH:mm:ss"));
                _dbHelper.SaveSetting("GMT", gmt);

                LogHelper.Log($"[Setup] Saved LocalTime='{nowLocal:yyyy-MM-dd HH:mm:ss}', GMT='{gmt}'");
                MessageBox.Show($"Local time and GMT saved.\nLocalTime: {nowLocal:yyyy-MM-dd HH:mm:ss}\nGMT: {gmt}",
                    "Time Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[btn_settime_Click][ERROR] {ex.Message}");
                MessageBox.Show("Failed to save local time and GMT. See logs for details.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Dedicated listener for STAFF card input (uses the same CTS slot as other listeners).
        private void StartStaffCardIdListening()
        {
            if (txt_staffcardid == null) return;

            txt_staffcardid.ReadOnly = true;

            // Stop any previous listening session
            StopCardIdListening();

            _cardListenCts = new CancellationTokenSource();
            var token = _cardListenCts.Token;

            // Clear UI immediately
            txt_staffcardid.Text = string.Empty;

            // Background loop: attempt read -> wait 4s -> repeat
            Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            await smart_attendance_middleware.Helpers.OcxFpreaderInvoker.Run(reader =>
                            {
                                if (token.IsCancellationRequested) return;

                                // Attempt to connect to the USB reader for this cycle
                                if (!reader.ConnectUsb())
                                {
                                    return;
                                }

                                try
                                {
                                    // Poll for card presence for a short window (e.g., up to ~1 second)
                                    int cardId = 0;
                                    var start = DateTime.UtcNow;
                                    while (!token.IsCancellationRequested && !reader.DetectCard(ref cardId))
                                    {
                                        Thread.Sleep(200);

                                        // Safety: don't block forever in a single cycle
                                        if ((DateTime.UtcNow - start).TotalMilliseconds > 1000)
                                            break;
                                    }

                                    // Update UI if we detected a non-zero cardId and we're not cancelled
                                    if (!token.IsCancellationRequested && cardId != 0)
                                    {
                                        try
                                        {
                                            this.BeginInvoke(new Action(() =>
                                            {
                                                txt_staffcardid.Text = cardId.ToString();
                                            }));
                                        }
                                        catch
                                        {
                                            // ignore UI invoke failures
                                        }
                                    }
                                }
                                finally
                                {
                                    try { reader.DisConnectUsb(); } catch { /* ignore */ }
                                }
                            });
                        }
                        catch (Exception exCycle)
                        {
                            try { LogHelper.Log($"Staff card listening cycle error: {exCycle.Message}"); } catch { /* ignore */ }
                        }

                        // Wait 4 seconds between cycles, honoring cancellation
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(4), token);
                        }
                        catch (TaskCanceledException)
                        {
                            break; // cancellation requested
                        }
                    }
                }
                catch (Exception ex)
                {
                    try { LogHelper.Log($"Staff card listening error: {ex.Message}"); } catch { /* ignore */ }
                }
            }, token);
        }
        private void StopCardIdListening()
        {
            try
            {
                _cardListenCts?.Cancel();
                _cardListenCts?.Dispose();
                _cardListenCts = null;
            }
            catch { /* ignore */ }
        }

        private void btn_attendence_sim_Click(object sender, EventArgs e)
        {
            // Open the SimulatorForm (normal UI). Simulation is controlled from that form.
            try
            {
                var form = new SimulatorForm(_dbHelper, _wsManager);

                // Optional: show as non-modal and bring to front
                form.Show();
                form.BringToFront();

                LogHelper.Log("Opened Simulator UI.");
            }
            catch (Exception ex)
            {
                LogHelper.Log("[SIM][UI][ERROR] " + ex.Message);
                MessageBox.Show("Failed to open Simulator UI: " + ex.Message, "Simulator",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////



        ////////////////////////////////////STAFF MANAGEMENT HELPERS////////////////////////////////////

        // initialize AutoComplete suggestions for txt_staff using DB: _dbHelper.GetStaffs()
        private void InitializeStaffSuggestions()
        {
            try
            {
                var staffList = _dbHelper.GetStaffs() ?? new List<Staff>();

                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in staffList)
                {
                    var name = s?.GetType().GetProperty("Name")?.GetValue(s)?.ToString();
                    if (string.IsNullOrWhiteSpace(name)) name = SafeToString(s, "Name");
                    if (!string.IsNullOrWhiteSpace(name))
                        names.Add(name.Trim());
                }

                txt_staff.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txt_staff.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var staffCollection = new AutoCompleteStringCollection();
                staffCollection.AddRange(names.ToArray());

                txt_staff.AutoCompleteCustomSource = staffCollection;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[InitializeStaffSuggestions][ERROR] {ex.Message}");
            }
        }

        // core loader with optional name filter, backed by DB: _dbHelper.GetStaffs()
        private void LoadStaffsIntoListView(string nameFilter)
        {
            try
            {
                lst_staffs.BeginUpdate();
                lst_staffs.Items.Clear();

                var staffList = _dbHelper.GetStaffs() ?? new List<Staff>();

                if (!string.IsNullOrWhiteSpace(nameFilter))
                {
                    staffList = staffList
                        .Where(s =>
                        {
                            var name = s?.GetType().GetProperty("Name")?.GetValue(s)?.ToString();
                            if (string.IsNullOrWhiteSpace(name)) name = SafeToString(s, "Name");
                            return !string.IsNullOrWhiteSpace(name) &&
                                   name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                        })
                        .ToList();
                }

                // Get all staff devices for sync status calculation
                var allDevices = AppSession.StaffDeviceList ?? new List<Device>();
                int totalDevices = allDevices.Count;

                foreach (var s in staffList)
                {
                    // Resolve common fields
                    string staffIdText =
                        s?.GetType().GetProperty("StaffID")?.GetValue(s)?.ToString() ??
                        s?.GetType().GetProperty("Id")?.GetValue(s)?.ToString() ??
                        "-";

                    string nameText =
                        s?.GetType().GetProperty("Name")?.GetValue(s)?.ToString() ??
                        "-";

                    string statusText =
                        s?.GetType().GetProperty("Status")?.GetValue(s)?.ToString() ??
                        "-";

                    string rfidText =
                        s?.GetType().GetProperty("RFID")?.GetValue(s)?.ToString() ??
                        "-";

                    string pictureRaw =
                        s?.GetType().GetProperty("Picture")?.GetValue(s)?.ToString() ??
                        string.Empty;
                    string pictureText = string.IsNullOrWhiteSpace(pictureRaw) ? "-" : Path.GetFileName(pictureRaw);

                    // Fingerprint flag resolution
                    string fingerprintText = "-";
                    try
                    {
                        var hasFpProp = s.GetType().GetProperty("HasFingerprint");
                        var fpFlagProp = s.GetType().GetProperty("FingerprintFlag");
                        var fpCountProp = s.GetType().GetProperty("FingerprintCount");

                        if (hasFpProp != null)
                        {
                            bool hasFp = (bool)(hasFpProp.GetValue(s) ?? false);
                            fingerprintText = hasFp ? "Yes" : "No";
                        }
                        else if (fpFlagProp != null)
                        {
                            var val = fpFlagProp.GetValue(s);
                            fingerprintText = (val != null && val.ToString() == "1") ? "Yes" : "No";
                        }
                        else if (fpCountProp != null)
                        {
                            int cnt = Convert.ToInt32(fpCountProp.GetValue(s) ?? 0);
                            fingerprintText = cnt > 0 ? $"Yes ({cnt})" : "No";
                        }
                    }
                    catch { }

                    string enrollmentIdText =
                        s?.GetType().GetProperty("EnrollmentID")?.GetValue(s)?.ToString() ??
                        s?.GetType().GetProperty("EnrollmentId")?.GetValue(s)?.ToString() ??
                        "-";

                    // Sync status across devices (like Members)
                    string syncStatusText = "-";
                    if (totalDevices > 0)
                    {
                        int syncedCount = 0;
                        // For staff, iterate staff devices and check sync per device
                        // Using similar pattern as members: for each device, check if staff is synced
                        foreach (var d in allDevices)
                        {
                            try
                            {
                                // Assuming DB helper has similar method for staff device sync status
                                // If not available, you can calculate based on staff enrollment presence
                                var hasSyncProp = s.GetType().GetProperty("SyncStatus");
                                if (hasSyncProp != null && hasSyncProp.GetValue(s) is Dictionary<int, bool> syncDict)
                                {
                                    int deviceKey = d.StaffDeviceId > 0 ? d.StaffDeviceId : d.DeviceId;
                                    if (syncDict.TryGetValue(deviceKey, out bool isSynced) && isSynced)
                                        syncedCount++;
                                }
                            }
                            catch { }
                        }
                        syncStatusText = $"{syncedCount}/{totalDevices} devices";
                    }

                    // Create list item with all fields
                    var item = new ListViewItem(string.IsNullOrWhiteSpace(staffIdText) ? "-" : staffIdText);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(nameText) ? "-" : nameText);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(statusText) ? "-" : statusText);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(rfidText) ? "-" : rfidText);
                    item.SubItems.Add(pictureText);
                    item.SubItems.Add(fingerprintText);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(enrollmentIdText) ? "-" : enrollmentIdText);
                    item.SubItems.Add(syncStatusText);

                    // Actions column: clickable link-style text
                    item.SubItems.Add("[Enroll]");  // Will be styled as link

                    // Store staff object for action handlers
                    item.Tag = new { StaffID = staffIdText, Name = nameText, EnrollmentID = enrollmentIdText, StaffObject = s };

                    lst_staffs.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load staff list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lst_staffs.EndUpdate();
            }
        }

        // setup lst_staffs columns and initial load
        private void InitializeStaffsListView()
        {
            lst_staffs.BeginUpdate();
            lst_staffs.Clear();
            lst_staffs.View = View.Details;
            lst_staffs.FullRowSelect = true;
            lst_staffs.GridLines = true;

            // Columns matching Members pattern
            lst_staffs.Columns.Add("Staff ID", 80);
            lst_staffs.Columns.Add("Name", 160);
            lst_staffs.Columns.Add("Status", 100);
            lst_staffs.Columns.Add("RFID", 120);
            lst_staffs.Columns.Add("Picture", 180);
            lst_staffs.Columns.Add("Fingerprint", 110);
            lst_staffs.Columns.Add("Enrollment ID", 110);
            lst_staffs.Columns.Add("Sync Status", 140);
            lst_staffs.Columns.Add("Actions", 120);  
            lst_staffs.Columns[lst_staffs.Columns.Count - 1].Width = -2;

            lst_staffs.EndUpdate();

            // Initial full load
            LoadStaffsIntoListView(null);
        }


        // button click handler to filter by txt_staff text (unchanged)
        private void btn_stafffilter_Click(object sender, EventArgs e)
        {
            string nameFilter = txt_staff.Text?.Trim();
            LoadStaffsIntoListView(string.IsNullOrEmpty(nameFilter) ? null : nameFilter);
        }

        // optional live filtering as user types (unchanged)
        private void txt_staff_TextChanged(object sender, EventArgs e)
        {
            var filter = txt_staff.Text?.Trim();
            LoadStaffsIntoListView(string.IsNullOrEmpty(filter) ? null : filter);
        }


        // AutoComplete for txt_staffname using known staff names
        private void InitializeStaffAttendanceSuggestions()
        {
            try
            {

                txt_staffname.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txt_staffname.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var names = new AutoCompleteStringCollection();
                foreach (var s in _staffEntries)
                {
                    if (!string.IsNullOrWhiteSpace(s.Name))
                        names.Add(s.Name);
                }

                txt_staffname.AutoCompleteCustomSource = names;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[InitializeStaffAttendanceSuggestions][ERROR] {ex.Message}");
            }
        }

        //  setup lst_staffattendence columns
        private void InitializeStaffAttendanceListView()
        {
            lst_staffattendence.BeginUpdate();
            lst_staffattendence.Clear();
            lst_staffattendence.View = View.Details;
            lst_staffattendence.FullRowSelect = true;
            lst_staffattendence.GridLines = true;

            lst_staffattendence.Columns.Add("Log ID", 80);
            lst_staffattendence.Columns.Add("Enrollment ID", 110);
            lst_staffattendence.Columns.Add("Attendee ID", 100);
            lst_staffattendence.Columns.Add("Name", 160);
            lst_staffattendence.Columns.Add("Check-In", 160);
            lst_staffattendence.Columns.Add("Check-Out", 160);
            lst_staffattendence.Columns[lst_staffattendence.Columns.Count - 1].Width = -2;

            lst_staffattendence.EndUpdate();

            // Initial load
            LoadFilteredStaffAttendanceLogs();
        }


        // unified filter loader (by optional name and/or date)
        private void LoadFilteredStaffAttendanceLogs()
        {
            try
            {
                lst_staffattendence.BeginUpdate();
                lst_staffattendence.Items.Clear();

                string nameFilter = txt_staffname.Text?.Trim();
                bool filterByDate = date_staffattendence.Checked;
                DateTime selectedDate = date_staffattendence.Value.Date;

                var logs = _dbHelper.GetAllStaffAttendanceLogs() ?? new List<dynamic>();

                var filtered = logs.Where(log =>
                {
                    if (!string.IsNullOrEmpty(nameFilter))
                    {
                        string logName = SafeToString(log, "Name");
                        if (string.IsNullOrEmpty(logName) || logName.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                    }

                    if (filterByDate)
                    {
                        DateTime? inTime = ParseDateTime(SafeToString(log, "CheckInTime"));
                        DateTime? outTime = ParseDateTime(SafeToString(log, "CheckOutTime"));

                        bool matches =
                            (inTime.HasValue && inTime.Value.Date == selectedDate) ||
                            (outTime.HasValue && outTime.Value.Date == selectedDate);

                        if (!matches) return false;
                    }

                    return true;
                }).ToList();

                foreach (var log in filtered)
                {
                    string logId = SafeToString(log, "LogID");
                    string enrollmentId = SafeToString(log, "EnrollmentID");
                    if (string.IsNullOrEmpty(enrollmentId))
                        enrollmentId = SafeToString(log, "EnrollmentId");

                    string attendeeId = SafeToString(log, "AttendeeID");
                    if (string.IsNullOrEmpty(attendeeId))
                        attendeeId = SafeToString(log, "StaffID");

                    string name = SafeToString(log, "Name");
                    string checkIn = SafeToString(log, "CheckInTime");
                    string checkOut = SafeToString(log, "CheckOutTime");

                    var item = new ListViewItem(string.IsNullOrWhiteSpace(logId) ? "-" : logId);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(enrollmentId) ? "-" : enrollmentId);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(attendeeId) ? "-" : attendeeId);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(name) ? "-" : name);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(checkIn) ? "-" : checkIn);
                    item.SubItems.Add(string.IsNullOrWhiteSpace(checkOut) ? "-" : checkOut);

                    item.Tag = new { EnrollmentId = enrollmentId, Name = name, AttendeeId = attendeeId };
                    lst_staffattendence.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load staff attendance logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lst_staffattendence.EndUpdate();
            }
        }

        
        // button click handler
        private void btn_filterattendencestaff_Click(object sender, EventArgs e)
        {
            LoadFilteredStaffAttendanceLogs();
        }

        // live filter on typing
        private void txt_staffname_TextChanged(object sender, EventArgs e)
        {
            LoadFilteredStaffAttendanceLogs();
        }

        // re-filter when date changes
        private void date_staffattendence_ValueChanged(object sender, EventArgs e)
        {
            LoadFilteredStaffAttendanceLogs();
        }

        // Utility: safely get dynamic property by common key names
        private string SafeToString(dynamic obj, string key)
        {
            try
            {
                var dict = obj as IDictionary<string, object>;
                if (dict != null && dict.ContainsKey(key))
                    return dict[key]?.ToString();

                // Try reflection
                var prop = obj.GetType().GetProperty(key);
                if (prop != null)
                {
                    var val = prop.GetValue(obj, null);
                    return val?.ToString();
                }
            }
            catch { /* ignore */ }

            return string.Empty;
        }

        // Handle click on Actions column 
        private void lst_staffs_ItemActivate(object sender, EventArgs e)
        {
            try
            {
                var listView = sender as ListView;
                if (listView?.SelectedItems.Count != 1) return;

                var item = listView.SelectedItems[0];
                var clickedPoint = listView.PointToClient(Control.MousePosition);
                var hitInfo = listView.HitTest(clickedPoint);

                // If Actions column (index 8) was clicked
                if (hitInfo.Item != null && hitInfo.SubItem != null)
                {
                    int columnIndex = item.SubItems.IndexOf(hitInfo.SubItem);

                    if (columnIndex == 8) // Actions column
                    {
                        dynamic staffTag = item.Tag;
                        string staffIdStr = staffTag.StaffID;        // may be GUID or numeric string
                        string enrollmentStr = staffTag.EnrollmentID; // enrollment id if present
                        string staffName = staffTag.Name;

                        // Pass both the staff identifier and any enrollment id we already have
                        EnrollStaffFromListView(staffIdStr, staffName, enrollmentStr);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[lst_staffs_ItemActivate] Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void EnrollStaffFromListView(string staffIdentifier, string staffName, string enrollmentIdStr = null)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(staffName))
            {
                MessageBox.Show("Staff name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;

            try
            {
                LogHelper.Log($"[EnrollStaffFromListView] Starting enrollment for staff '{staffName}' (Identifier: {staffIdentifier})");

                // Resolve the staff record from DB using common keys (StaffID or Id)
                int? enrollmentId = null;
                object dbStaff = null;
                try
                {
                    var staffList = _dbHelper.GetStaffs() ?? new List<Staff>();
                    dbStaff = staffList.FirstOrDefault(s =>
                    {
                        var sid = s?.GetType().GetProperty("StaffID")?.GetValue(s)?.ToString();
                        var id = s?.GetType().GetProperty("Id")?.GetValue(s)?.ToString();
                        return string.Equals(sid, staffIdentifier, StringComparison.OrdinalIgnoreCase)
                               || string.Equals(id, staffIdentifier, StringComparison.OrdinalIgnoreCase);
                    });

                    if (dbStaff != null)
                    {
                        // Try extract enrollment id from DB staff record (common property names)
                        var enrolProp = dbStaff.GetType().GetProperty("EnrollmentID") ?? dbStaff.GetType().GetProperty("EnrollmentId");
                        if (enrolProp != null)
                        {
                            var val = enrolProp.GetValue(dbStaff)?.ToString();
                            if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out int parsedEnrol) && parsedEnrol > 0)
                                enrollmentId = parsedEnrol;
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    LogHelper.Log($"[EnrollStaffFromListView] Warning - DB extraction failed: {dbEx.Message}");
                }

                // If enrollmentId not provided in tag / db, try parse enrollmentIdStr argument
                if (!enrollmentId.HasValue && !string.IsNullOrWhiteSpace(enrollmentIdStr))
                {
                    if (int.TryParse(enrollmentIdStr, out int parsed))
                        enrollmentId = parsed;
                }

                // If still no enrollmentId, we cannot safely call the device APIs that require numeric enrollment IDs
                if (!enrollmentId.HasValue || enrollmentId.Value <= 0)
                {
                    MessageBox.Show(
                        $"Cannot enroll '{staffName}'. No numeric Enrollment ID found for this staff.\n\n" +
                        "Please ensure the staff record has a valid EnrollmentID (numeric) in the database.",
                        "Missing Enrollment ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogHelper.Log($"[EnrollStaffFromListView] Aborted - EnrollmentID missing for staff '{staffName}' (Identifier: {staffIdentifier})");
                    return;
                }

                int enrollId = enrollmentId.Value;

                // Check for duplicate enrollment in the in-memory buffer by enrollment id
                var already = _enrolledStaffBuffer.FirstOrDefault(x => x.Id == enrollId);
                if (already != default)
                {
                    MessageBox.Show($"Enrollment ID {enrollId} is already enrolled in the buffer. Cannot enroll twice.",
                        "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Extract card ID and face image from DB staff object if available
                long staffCardId = 0;
                string imagePathFromDb = null;
                if (dbStaff != null)
                {
                    try
                    {
                        var rfidProp = dbStaff.GetType().GetProperty("RFID");
                        if (rfidProp != null && long.TryParse(rfidProp.GetValue(dbStaff)?.ToString() ?? "0", out long rfid))
                        {
                            staffCardId = rfid;
                            LogHelper.Log($"[EnrollStaffFromListView] Found Card ID: {staffCardId} for enrollment {enrollId}");
                        }

                        var picProp = dbStaff.GetType().GetProperty("Picture");
                        if (picProp != null)
                        {
                            imagePathFromDb = picProp.GetValue(dbStaff)?.ToString();
                            LogHelper.Log($"[EnrollStaffFromListView] Found image path: {(string.IsNullOrWhiteSpace(imagePathFromDb) ? "None" : imagePathFromDb)} for enrollment {enrollId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[EnrollStaffFromListView] Warning extracting RFID/Picture: {ex.Message}");
                    }
                }

                // Copy image from DB path to local FACE directory (use "FACE" folder)
                string storedImagePath = null;
                if (!string.IsNullOrWhiteSpace(imagePathFromDb) && File.Exists(imagePathFromDb))
                {
                    try
                    {
                        // Ensure FACE directory exists and use it
                        string faceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FACE");
                        Directory.CreateDirectory(faceDir);

                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(imagePathFromDb)}";
                        storedImagePath = Path.Combine(faceDir, fileName);
                        File.Copy(imagePathFromDb, storedImagePath, true);
                        LogHelper.Log($"[EnrollStaffFromListView] Image copied to: {storedImagePath}");
                    }
                    catch (Exception imgEx)
                    {
                        LogHelper.Log($"[EnrollStaffFromListView] Warning - Image copy failed: {imgEx.Message}");
                        storedImagePath = null; // Allow enrollment without image
                    }
                }

                // Validate: at least one biometric required
                bool hasImage = storedImagePath != null && File.Exists(storedImagePath);
                bool hasCard = staffCardId > 0;

                if (!hasImage && !hasCard)
                {
                    MessageBox.Show($"❌ No Card ID or Face Image found for '{staffName}'.\n\nCannot enroll without biometric data.",
                        "Insufficient Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogHelper.Log($"[EnrollStaffFromListView] Enrollment cancelled - no biometric data for staff '{staffName}' (EnrollmentID: {enrollId})");
                    return;
                }

                // Add to in-memory buffer using the numeric enrollment ID
                _enrolledStaffBuffer.Add((enrollId, staffName, storedImagePath ?? string.Empty));

                // Enroll across all devices using the numeric enrollment ID
                await EnrollStaffAcrossAllDevicesAsync(enrollId, staffName, staffCardId, storedImagePath ?? string.Empty);

                MessageBox.Show(
                    $"✓ Staff '{staffName}' (Enrollment ID: {enrollId}) enrolled successfully!\n\n" +
                    $"Card ID: {(hasCard ? staffCardId.ToString() : "Not used")}\n" +
                    $"Face Image: {(hasImage ? "Yes" : "No")}",
                    "Enrollment Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LogHelper.Log($"[EnrollStaffFromListView] ✓ Successfully enrolled staff '{staffName}' (Enrollment ID: {enrollId})");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[EnrollStaffFromListView] ❌ Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"❌ Enrollment failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private List<Device> ShowDeviceSelectionDialog(List<Device> devices)
        {
            var selected = new List<Device>();

            if (devices == null || devices.Count == 0)
                return selected;

            using (var dlg = new Form())
            {
                dlg.Text = "Select Devices to Enroll";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.Width = 520;
                dlg.Height = 420;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ShowInTaskbar = false;

                var clb = new CheckedListBox
                {
                    Dock = DockStyle.Top,
                    Height = 320,
                    CheckOnClick = true
                };

                // Keep device ordering aligned with the passed list so indices map correctly
                foreach (var device in devices)
                {
                    string label = !string.IsNullOrWhiteSpace(device.Location)
                        ? $"{device.Location} ({device.DeviceIP})"
                        : device.DeviceIP ?? "(unknown)";
                    clb.Items.Add(label, true); // default: pre-check all
                }

                var btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Width = 100,
                    Left = dlg.ClientSize.Width - 220,
                    Top = clb.Bottom + 8,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Width = 100,
                    Left = dlg.ClientSize.Width - 110,
                    Top = clb.Bottom + 8,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                dlg.Controls.Add(clb);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                // Show as modal on the UI thread
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return selected;

                // Map checked indices back to device list
                foreach (int idx in clb.CheckedIndices)
                {
                    if (idx >= 0 && idx < devices.Count)
                        selected.Add(devices[idx]);
                }
            }

            return selected;
        }

        private async Task EnrollStaffAcrossAllDevicesAsync(int enrollId, string userName, long cardId, string imagePath)
        {
            // Only consider normal devices (exclude staff devices)
            var normalDevices = AppSession.DeviceList ?? new List<Device>();

            if (normalDevices.Count == 0)
            {
                MessageBox.Show("No normal devices are available to enroll.", "No Devices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LogHelper.Log("[EnrollStaffAcrossAllDevicesAsync] No normal devices in session.");
                return;
            }

            // Ask user to select which devices to enroll to
            List<Device> targetDevices;
            try
            {
                targetDevices = ShowDeviceSelectionDialog(normalDevices);
            }
            catch (Exception exDlg)
            {
                LogHelper.Log($"[EnrollStaffAcrossAllDevicesAsync] Device selection dialog failed: {exDlg.Message}");
                MessageBox.Show("Failed to show device selection dialog. See logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (targetDevices == null || targetDevices.Count == 0)
            {
                LogHelper.Log("[EnrollStaffAcrossAllDevicesAsync] Enrollment cancelled by user (no devices selected).");
                MessageBox.Show("Enrollment cancelled. No devices were selected.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Start global dashboard loader
            try
            {
                Dashboard.GlobalStartLoader($"Enrolling \"{userName}\" to {targetDevices.Count} device(s)...");

                // Proceed to enroll only on the selected devices
                int processed = 0;
                foreach (var device in targetDevices)
                {
                    processed++;
                    try
                    {
                        string ip = device.DeviceIP;
                        int port = device.Port;
                        int machineNo = device.MachineNo;
                        string password = device.Password ?? "0";

                        string deviceLabel = !string.IsNullOrWhiteSpace(device.Location) ? device.Location : ip;
                        Dashboard.GlobalUpdateLoaderMessage($"[{processed}/{targetDevices.Count}] Enrolling on {deviceLabel} ({ip})...");

                        // 1) Set username
                        bool nameOk = await DeviceService.SetUserName(ip, port, machineNo, password, enrollId, userName);
                        if (!nameOk)
                        {
                            LogHelper.Log($"Failed SetUserName on {ip}:{port} machineNo={machineNo} enroll={enrollId}");
                            continue; // skip RFID/face if name failed
                        }

                        // 2) Add RFID (only if cardId present)
                        if (cardId > 0)
                        {
                            bool rfidOk = await DeviceService.AddRfidCard(ip, port, machineNo, password, enrollId, cardId, machineNo);
                            if (!rfidOk)
                                LogHelper.Log($"Failed AddRfidCard on {ip}:{port} machineNo={machineNo} enroll={enrollId} card={cardId}");
                        }

                        // 3) Add face (only if image available)
                        if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                        {
                            bool faceOk = await DeviceService.AddFace(ip, port, machineNo, password, enrollId, imagePath);
                            if (!faceOk)
                                LogHelper.Log($"Failed AddFace on {ip}:{port} machineNo={machineNo} enroll={enrollId}");
                        }

                        LogHelper.Log($"Staff enrollment completed for {userName} ({enrollId}) on {ip}:{port}");
                        Dashboard.GlobalUpdateLoaderMessage($"[{processed}/{targetDevices.Count}] Completed on {deviceLabel} ({ip})");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Error enrolling staff {userName} ({enrollId}) on device {device?.DeviceIP}: {ex.Message}");
                        Dashboard.GlobalUpdateLoaderMessage($"[{processed}/{targetDevices.Count}] Error on {device?.DeviceIP}: {ex.Message}");
                    }
                }

                // brief final message before hiding loader
                Dashboard.GlobalUpdateLoaderMessage($"Enrollment completed for \"{userName}\".");
            }
            finally
            {
                // Ensure loader is stopped even if exceptions occur
                try
                {
                    Dashboard.GlobalStopLoader();
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Error stopping global loader: {ex.Message}");
                }
            }
        }
        private void txt_staffname_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void date_staffattendence_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void btn_filterattendencestaff_Click_1(object sender, EventArgs e)
        {

        }

        private void lst_staffattendence_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txt_staff_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void btn_stafffilter_Click_1(object sender, EventArgs e)
        {

        }

        private void lst_staffs_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btn_cardcopy_Click(object sender, EventArgs e)
        {
            try
            {
                var text = txt_staffcardid?.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("No card ID to copy.", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Clipboard.SetText(text);
                MessageBox.Show("Card ID copied to clipboard.", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"btn_cardcopy_Click error: {ex.Message}");
                MessageBox.Show("Failed to copy card ID to clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //////////////////////////////////////////////////////////////////////////////////////
    }
}