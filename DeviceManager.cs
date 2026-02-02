using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using PdfFont = iTextSharp.text.Font;


namespace smart_attendance_middleware
{
    public partial class DeviceManager : Form
    {
        private DatabaseHelper _dbHelper;
        private WebSocketManager _wsManager;
        private Processor _processor;
        private DeviceFunctions _deviceFunctions = new DeviceFunctions();
        //private AxFP_CLOCK pOcxObject;

        private readonly string _staffFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "staff.txt");
        private readonly List<(int Id, string Name, long CardId, string ImagePath)> _staffEntries = new List<(int, string, long, string)>();
        private string _selectedImagePath = null; // original user-selected path (source)
        private string _storedImagePath = null;   // copied image path we persist/use
        private readonly string _staffImagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StaffImages");
        private readonly List<(int Id, string Name, string ImagePath)> _enrolledStaffBuffer = new List<(int, string, string)>(); // in-memory array requested

        // Add these fields near the other staff fields at the top of the class
        private readonly string _privMembersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "privilegemember.txt");
        private readonly string _privImagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PrivilegeImages");
        private readonly List<(int Id, string Name, long CardId, string ImagePath)> _privMembers = new List<(int, string, long, string)>();
        private string _privSelectedImagePath = null; // original user-selected path
        private string _privStoredImagePath = null;   // copied image path we persist/use
        private CancellationTokenSource _cardListenCts = null;



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
        private void SetupDeviceList()
        {
            // Clear existing items and columns
            listViewDevices.Clear(); // Assuming you've renamed your control to listViewDevices

            // Configure ListView properties
            listViewDevices.View = View.Details;
            listViewDevices.FullRowSelect = true;
            listViewDevices.GridLines = true;
            listViewDevices.MultiSelect = false; // Single selection
                                                 //   listViewDevices.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);


            // Add columns (widths can be adjusted as needed)
            listViewDevices.Columns.Add("ID", 50);
            listViewDevices.Columns.Add("Machine No", 50);
            listViewDevices.Columns.Add("Attendance Type", 100);
            listViewDevices.Columns.Add("Device IP", 120);
            listViewDevices.Columns.Add("Port", 60);
            listViewDevices.Columns.Add("Connection Type", 100);
            listViewDevices.Columns.Add("Status", 80);
            listViewDevices.Columns[listViewDevices.Columns.Count - 1].Width = -2; // to handle the last column width automatically



            // Load devices from database
            var devices = _dbHelper.LoadDevices();

            // Populate the ListView
            foreach (var device in devices)
            {
                var item = new ListViewItem(device.DeviceId.ToString());
                item.SubItems.Add(device.MachineNo.ToString());
                item.SubItems.Add(device.AttendanceType ? "Check-in" : "Check-out");
                item.SubItems.Add(device.DeviceIP);
                item.SubItems.Add(device.Port.ToString());
                item.SubItems.Add(device.ConnectionType);
                item.SubItems.Add(device.IsActive ? "Active" : "Inactive");

                // Store the full device object in the Tag property for later access
                item.Tag = device;

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
            pnl_staffhandling.Visible = false;
            btn_deleteUser.Enabled = false;
            btn_change_password.Enabled = false;
            btn_editUser.Enabled = false;
            lbl_apikey_status.Text = "";
            listViewDevices.SelectedItems.Clear(); // Clear selection
            ClearDeviceFields(); // Clear device fields on load
            update_session(); // Update session with device data
            staff_DeviceId.Text = _dbHelper.GetNextStaffDeviceId().ToString();
            staff_DeviceId.ReadOnly = true;
            InitializeMemberList();
            EnsureStaffFileExists();
            LoadStaffEntriesFromFile();
            PopulateStaffListView();
            PrepareNextStaffId();
            lst_stff.DoubleClick += lst_stff_DoubleClick;
            EnsurePrivilegeMembersFileExists();
            LoadPrivilegeMembersFromFile();
            PopulatePrivilegeMembersListView();
            PrepareNextPrivilegeMemberId();

            // Make txt_privId and txt_scanid readonly at load
            txt_privId.ReadOnly = true;
            txt_scanid.ReadOnly = true;
            lst_mempriv.DoubleClick += lst_mempriv_DoubleClick;
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

                SetupStaffDevicesList();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading devices: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



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
            pnl_staffhandling.Visible = false;
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

            pnl_privilege.Visible = false;
            pnl_deviceManager.Visible = false;
            pnl_attendence.Visible = false;
            pnl_UserManagement.Visible = false;
            pnl_APIkey.Visible = true;
            pnl_staffhandling.Visible = false;
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

            pnl_privilege.Visible = false;
            pnl_deviceManager.Visible = false;
            pnl_attendence.Visible = false;
            pnl_APIkey.Visible = false;
            pnl_UserManagement.Location = new Point(186, 0);
            pnl_UserManagement.Visible = true;
            pnl_staffhandling.Visible = false;
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

            btn_privilegememb.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_privilegememb.ForeColor = Color.White;

            // Show staff panel, hide others
            pnl_staffhandling.Visible = true;
            pnl_staffhandling.Location = new Point(186, 0);

            pnl_deviceManager.Visible = false;
            pnl_attendence.Visible = false;
            pnl_privilege.Visible = false;
            pnl_setup.Visible = false;
            pnl_UserManagement.Visible = false;
            pnl_APIkey.Visible = false;
            // End of Button Color Seetup

            LoadAttendanceLogs();
            SetupStaffDevicesList();
            StartStaffCardIdListening();
        }

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


        //Abdul Hadi Code
        // On selection of row send deviceId to LoadSelectedDeviceDetails
        private void listViewDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewDevices.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewDevices.SelectedItems[0];

                // Get the Device object stored in the Tag property
                if (selectedItem.Tag is Device selectedDevice)
                {
                    LoadSelectedDeviceDetails(selectedDevice.DeviceId);
                }
            }
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



        //Abdul Hadi Code
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

        //Abdul Hadi Code
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


                    // Check if device already exists with the same IP, MachineNo, or Port
                    int MachineNo = int.TryParse(cmb_machine_no.SelectedItem.ToString(), out int machine) ? machine : 0;
                    int port = int.TryParse(txt_port.Text, out int devicePort) ? devicePort : 0;
                    string deviceExistsMessage = _dbHelper.IDCheckDeviceExists(deviceIp, MachineNo, int.Parse(txt_Device_id.Text));


                    if (!string.IsNullOrEmpty(deviceExistsMessage))
                    {
                        MessageBox.Show(deviceExistsMessage, "Device Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }


                    ToggleDeviceFieldsEditMode(false);
                    btnEdit.Visible = true;
                    btnSave.Visible = false;
                    btnCancle.Visible = false;
                    btn_remove_device.Visible = true;
                    btn_format.Visible = false;

                    Device device = new Device
                    {
                        DeviceId = int.Parse(txt_Device_id.Text),
                        Department = txt_department.Text,
                        AttendanceType = cmb_device_type.SelectedItem.ToString() == "Check-in",
                        DeviceSerial = txt_Serial.Text,
                        DeviceIP = deviceIp,
                        Port = int.Parse(txt_port.Text),
                        TimeZone = txt_timeZone.Text,
                        Location = txt_location.Text,
                        Password = txt_password.Text,
                        ConnectionType = cmb_connectionType.SelectedItem.ToString(),
                        MachineNo = int.TryParse(cmb_machine_no.SelectedItem.ToString(), out int machineNo) ? machineNo : 0,
                        IsActive = txt_device_status.Text == "Active",
                        LastSyncTime = txt_last_synced.Text,
                        FaceCapacity = int.Parse(txt_face_capacity.Text),
                        RFIDCapacity = int.Parse(txt_RFID_capacity.Text),
                        FingerCapacity = int.Parse(txt_finger_capacity.Text)
                    };

                    try
                    {
                        _dbHelper.UpdateDevice(device);

                        // Notify AI Chat Logger about the device update
                        aiChatLogger.AddDeviceMessage($"Device '{device.Location}' configuration updated successfully!");
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

        //Abdul Hadi Code
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
                    // Add AI message before device removal
                    aiChatLogger.AddDeviceMessage($"Preparing to remove device from {txt_location.Text}...");
                    aiChatLogger.AddInfoMessage("Clearing all enrollment data from the device first...");
                    
                    await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByDeviceId(deviceId);
                    _dbHelper.DeleteDevice(deviceId);
                    
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
                //  MessageBox.Show("API Key format is invalid. Expected format: gymId:secretKey", "Format Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lbl_apikey_status.Text = "Checking the API Key. Please wait...";
            _wsManager.Initialize(apiKey);

            try
            {
                await _wsManager.ConnectAsync();
                bool isSubscribed = await _wsManager.SubscribeAsync();

                if (isSubscribed)
                {
                    lbl_apikey_status.Text = "API Key is valid.";
                    _dbHelper.SaveSetting("APIKey", apiKey);
                    MessageBox.Show("API Key saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lbl_apikey_status.Text = "API Key is invalid.";
                    MessageBox.Show("Please enter a Valid API Key before saving.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the connection or subscription
                MessageBox.Show("Incorrect API Key. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        //private void btn_format_Click(object sender, EventArgs e)
        //{

        //}

        private async void btn_format_Click(object sender, EventArgs e)
        {
            // Check if a device is selected
            if (listViewDevices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a device to format.", "No Device Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get device details from form fields
            if (string.IsNullOrEmpty(txt_ip.Text) || string.IsNullOrEmpty(txt_port.Text) ||
                cmb_machine_no.SelectedItem == null)
            {
                MessageBox.Show("Missing device information. Please select a valid device.",
                    "Incomplete Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the current logged in user for authentication
            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                MessageBox.Show("No user is currently logged in. Cannot proceed.");
                return;
            }

            // Confirm with the user before proceeding
            DialogResult confirmResult = MessageBox.Show(
                "WARNING: This will delete ALL user data from the device including fingerprints, " +
                "face templates, and RFID card information.\n\n" +
                "This action cannot be undone. Are you sure you want to continue?",
                "Format Device Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            // Prompt for password to authorize this critical action
            using (Form prompt = new Form())
            {
                prompt.Width = 360;
                prompt.Height = 160;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = $"Confirm Format - {currentUser} Login";
                prompt.StartPosition = FormStartPosition.CenterScreen;

                Label label = new Label()
                {
                    Left = 10,
                    Top = 20,
                    Width = 320,
                    Text = "Enter your password to format the device:"
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
                    Text = "Format Device",
                    Left = 150,
                    Width = 100,
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

                    if (!_dbHelper.VerifyLoginUser(currentUser, enteredPassword))
                    {
                        MessageBox.Show("Authentication failed. Device formatting was cancelled.");
                        return;
                    }

                    try
                    {
                        // Extract device information
                        string ip = txt_ip.Text.Trim();
                        int port = int.Parse(txt_port.Text.Trim());
                        int machineNo = int.Parse(cmb_machine_no.SelectedItem.ToString());
                        string password = txt_password.Text.Trim();

                        // Check if device is reachable
                        if (!IsValidIp(ip))
                        {
                            MessageBox.Show("The device is not reachable. Please check the connection and try again.",
                                "Device Unreachable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Show processing indicator
                        Cursor = Cursors.WaitCursor;

                        // Format the device by emptying enrollment data
                        bool success = await DeviceService.EmptyEnrollmentData(ip, port, machineNo, password);

                        if (success)
                        {
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
                        // Restore cursor
                        Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void pnl_staffManagement_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void staff_btnDeleteDevice_Click(object sender, EventArgs e)
        {
           
        }

        private void staff_btnEditDevice_Click(object sender, EventArgs e)
        {
            
        }

        // Add this method to toggle staff device fields edit mode
        private void ToggleStaffDeviceFieldsEditMode(bool enable)
        {
            // Enable/disable staff device form fields
            staff_txtDepartment.ReadOnly = !enable;
            staff_txtPort.ReadOnly = !enable;
            staff_txtTimeZone.ReadOnly = !enable;
            staff_txtLocation.ReadOnly = !enable;
            staff_txtPassword.ReadOnly = !enable;

            // These should remain readonly during edit since they can't be changed
            staff_txtDeviceIP.ReadOnly = true;
            staff_txtDeviceSerial.ReadOnly = true;
            staff_DeviceId.ReadOnly = true;

            // Enable/disable combo boxes
            staff_cmbDeviceType.Enabled = enable;
            staff_cmbMachineNo.Enabled = enable;
        }

        private async void staff_btnAddDevice_Click(object sender, EventArgs e)
        {
           
        }

        private void staff_DeviceId_TextChanged(object sender, EventArgs e)
        {

        }

        private void staff_txtFilterId_TextChanged(object sender, EventArgs e)
        {

        }

        private void staff_dtpFilterDate_ValueChanged(object sender, EventArgs e)
        {

        }

        private void staff_cmbFilterDeviceType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void staff_lvAttendance_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void LoadAttendanceLogs()
        {
            staff_lvAttendance.Items.Clear();
            staff_lvAttendance.View = View.Details;
            staff_lvAttendance.FullRowSelect = true;
            staff_lvAttendance.GridLines = true;

            staff_lvAttendance.Columns.Clear();
            staff_lvAttendance.Columns.Add("Log ID", 70);
            staff_lvAttendance.Columns.Add("Enroll ID", 80);
            staff_lvAttendance.Columns.Add("Name", 120);
            staff_lvAttendance.Columns.Add("Check-In Time", 150);
            staff_lvAttendance.Columns.Add("Check-Out Time", 150);
            staff_lvAttendance.Columns[staff_lvAttendance.Columns.Count - 1].Width = -2;

            var attendanceLogs = _dbHelper.GetAllStaffAttendanceLogs();

            foreach (var log in attendanceLogs)
            {
                var item = new ListViewItem(log.LogID.ToString());
                item.SubItems.Add(log.EnrollmentID.ToString());
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.Name) ? "-" : log.Name);
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckInTime) ? "-" : log.CheckInTime);
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckOutTime) ? "-" : log.CheckOutTime);
                staff_lvAttendance.Items.Add(item);
            }
        }

        private DateTime? ParseDate(object value)
        {
            if (value == null) return null;
            if (value is DateTime dt) return dt;
            DateTime parsed;
            return DateTime.TryParse(value.ToString(), out parsed) ? (DateTime?)parsed : null;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            
        }

        // Add this method to setup the staff devices ListView
        private void SetupStaffDevicesList()
        {
            // Clear existing items and columns
            lst_staffdevices.Clear();

            // Configure ListView properties
            lst_staffdevices.View = View.Details;
            lst_staffdevices.FullRowSelect = true;
            lst_staffdevices.GridLines = true;
            lst_staffdevices.MultiSelect = false; // Single selection

            // Add columns
            lst_staffdevices.Columns.Add("Device ID", 80);
            lst_staffdevices.Columns.Add("IP Address", 120);
            lst_staffdevices.Columns.Add("Location", 150);
            lst_staffdevices.Columns.Add("Department", 120);
            lst_staffdevices.Columns.Add("Type", 100);
            lst_staffdevices.Columns[lst_staffdevices.Columns.Count - 1].Width = -2; // Auto-size last column

            // Load staff devices from database
            var staffDevices = _dbHelper.LoadStaffDevices();

            // Populate the ListView
            foreach (var device in staffDevices)
            {
                var item = new ListViewItem(device.StaffDeviceId.ToString());
                item.SubItems.Add(device.DeviceIP);
                item.SubItems.Add(device.Location);
                item.SubItems.Add(device.Department);
                item.SubItems.Add(device.StaffAttendanceType);

                // Store the full device object in the Tag property for later access
                item.Tag = device;

                lst_staffdevices.Items.Add(item);
            }
        }

        // Add this method to handle staff device selection
        private void lst_staffdevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lst_staffdevices.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = lst_staffdevices.SelectedItems[0];

                // Get the Device object stored in the Tag property
                if (selectedItem.Tag is Device selectedDevice)
                {
                    LoadSelectedStaffDeviceDetails(selectedDevice.StaffDeviceId);
                }
            }
            else
            {
                // Clear fields when no selection
                ClearStaffDeviceFields();
            }
        }

        // Add this method to load selected staff device details
        private void LoadSelectedStaffDeviceDetails(int staffDeviceId)
        {
            var device = _dbHelper.LoadStaffDevices().FirstOrDefault(d => d.StaffDeviceId == staffDeviceId);

            if (device != null)
            {
                staff_DeviceId.Text = device.StaffDeviceId.ToString();
                staff_txtDeviceIP.Text = device.DeviceIP;
                staff_txtPort.Text = device.Port.ToString();
                staff_txtTimeZone.Text = device.TimeZone;
                staff_txtLocation.Text = device.Location;
                staff_txtPassword.Text = device.Password;
                staff_txtDeviceSerial.Text = device.DeviceSerial;
                staff_txtDepartment.Text = device.Department;
                staff_cmbDeviceType.SelectedItem = device.StaffAttendanceType;
                staff_cmbMachineNo.SelectedItem = device.MachineNo.ToString();
            }
        }

        // Add this method to clear staff device fields
        private void ClearStaffDeviceFields()
        {
            staff_DeviceId.Clear();
            staff_txtDeviceIP.Clear();
            staff_txtPort.Clear();
            staff_txtTimeZone.Clear();
            staff_txtLocation.Clear();
            staff_txtPassword.Clear();
            staff_txtDeviceSerial.Clear();
            staff_txtDepartment.Clear();
            staff_cmbDeviceType.SelectedIndex = -1;
            staff_cmbMachineNo.SelectedIndex = -1;
        }

        private async void staff_btnAddDevice_Click_1(object sender, EventArgs e)
        {
            staff_btnAddDevice.Enabled = false;
            try
            {
                // 1. Trim and validate all required fields
                string department = staff_txtDepartment?.Text?.Trim();
                string ip = staff_txtDeviceIP?.Text?.Trim();
                string portText = staff_txtPort?.Text?.Trim();
                string timeZone = staff_txtTimeZone?.Text?.Trim();
                string location = staff_txtLocation?.Text?.Trim();
                string password = staff_txtPassword?.Text?.Trim();
                string staffdeviceType = staff_cmbDeviceType?.SelectedItem?.ToString();
                string machineNoText = staff_cmbMachineNo?.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(ip) ||
                    string.IsNullOrWhiteSpace(portText) ||
                    string.IsNullOrWhiteSpace(timeZone) ||
                    string.IsNullOrWhiteSpace(location) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(department) ||
                    string.IsNullOrWhiteSpace(staffdeviceType) ||
                    string.IsNullOrWhiteSpace(machineNoText))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(portText, out int port) || port <= 0)
                {
                    MessageBox.Show("Port must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!int.TryParse(machineNoText, out int machineNo) || machineNo <= 0)
                {
                    MessageBox.Show("Machine No must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Check for duplicate staff device by IP
                if (_dbHelper.StaffDeviceExists(ip, machineNo))
                {
                    MessageBox.Show("A staff device with the same IP already exists.", "Duplicate Device", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3. Ping device
                var addNewDevice = new AddNewDevice();
                bool isReachable = await addNewDevice.PingDeviceWithRetries(ip, 5, 2000, 2000);
                if (!isReachable)
                {
                    MessageBox.Show($"Could not connect to device at {ip} after 5 attempts. Please check the device network connection.",
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 4. Get serial and set textbox (readonly)
                string serial = await DeviceService.GetSerial(ip, port, machineNo, password);
                staff_txtDeviceSerial.Text = serial;
                staff_txtDeviceSerial.ReadOnly = true;

                // 5. Save staff device using StaffDeviceId
                Device newDevice = new Device
                {
                    StaffDeviceId = _dbHelper.GetNextStaffDeviceId(), // Use StaffDeviceId here
                    Department = department,
                    DeviceSerial = serial,
                    DeviceIP = ip,
                    Port = port,
                    TimeZone = timeZone,
                    Location = location,
                    Password = password,
                    MachineNo = machineNo,
                    StaffAttendanceType = staffdeviceType,
                };
                _dbHelper.SaveStaffDevice(newDevice);

                MessageBox.Show("Staff device added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                staff_btnAddDevice.Enabled = true;
                staff_txtDeviceIP.Text = "";
                staff_txtPort.Text = "";
                staff_txtTimeZone.Text = "";
                staff_txtLocation.Text = "";
                staff_txtPassword.Text = "";
                staff_txtDeviceSerial.Text = "";
                staff_DeviceId.Text = _dbHelper.GetNextStaffDeviceId().ToString();
                staff_DeviceId.ReadOnly = true;
                update_session();

            }
        }

        private async void staff_btnDeleteDevice_Click_1(object sender, EventArgs e)
        {
            // Check if a device is selected in the ListView
            if (lst_staffdevices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a staff device from the list to delete.", "No Device Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected device from the ListView
            ListViewItem selectedItem = lst_staffdevices.SelectedItems[0];
            if (!(selectedItem.Tag is Device selectedDevice))
            {
                MessageBox.Show("Invalid device selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectedDeviceId = selectedDevice.StaffDeviceId;

            // Confirm deletion
            var confirm = MessageBox.Show(
                $"Are you sure you want to delete Staff Device:\n\nID: {selectedDeviceId}\nLocation: {selectedDevice.Location}\nIP: {selectedDevice.DeviceIP}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            // Prompt for user authentication
            string currentUser = AppSession.LoggedInUser;
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                MessageBox.Show("No user is currently logged in. Cannot proceed.");
                return;
            }

            using (Form prompt = new Form())
            {
                prompt.Width = 360;
                prompt.Height = 160;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = $"Confirm Deletion - {currentUser} Login";
                prompt.StartPosition = FormStartPosition.CenterScreen;

                Label label = new Label()
                {
                    Left = 10,
                    Top = 20,
                    Width = 320,
                    Text = $"Enter your password or backup code to delete Staff Device ID {selectedDeviceId}:"
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
                        try
                        {
                            // Prevent UI interaction while processing
                            Cursor = Cursors.WaitCursor;
                            staff_btnDeleteDevice.Enabled = false;
                            await _deviceFunctions.EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId(selectedDeviceId);

                            _dbHelper.DeleteStaffDevice(selectedDeviceId);


                            // Remove only the selected item from the existing ListView (do not rebuild or re-show it)
                            if (lst_staffdevices.Items.Contains(selectedItem))
                                lst_staffdevices.Items.Remove(selectedItem);

                            ClearStaffDeviceFields();
                            lst_staffdevices.SelectedItems.Clear();
                            update_session();

                            MessageBox.Show("Staff device deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        finally
                        {
                            staff_btnDeleteDevice.Enabled = true;
                            Cursor = Cursors.Default;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Authentication failed. Staff device was not deleted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void staff_btnEditDevice_Click_1(object sender, EventArgs e)
        {
            if (staff_btnEditDevice.Text == "Edit")
            {
                // Check if a device is selected in the ListView
                if (lst_staffdevices.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a staff device from the list to edit.", "No Device Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the selected device from the ListView
                ListViewItem selectedItem = lst_staffdevices.SelectedItems[0];
                if (!(selectedItem.Tag is Device selectedDevice))
                {
                    MessageBox.Show("Invalid device selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Device details are already loaded in the form fields via the selection event
                // Change the button text to "Save"
                staff_btnEditDevice.Text = "Save";

                // Enable editing of fields (you'll need to create this method for staff device fields)
                ToggleStaffDeviceFieldsEditMode(true);
            }
            else if (staff_btnEditDevice.Text == "Save")
            {
                // Validate the input fields
                string department = staff_txtDepartment?.Text?.Trim();
                string portText = staff_txtPort?.Text?.Trim();
                string timeZone = staff_txtTimeZone?.Text?.Trim();
                string location = staff_txtLocation?.Text?.Trim();
                string password = staff_txtPassword?.Text?.Trim();
                string staffdeviceType = staff_cmbDeviceType?.SelectedItem?.ToString();
                string machineNoText = staff_cmbMachineNo?.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(portText) ||
                    string.IsNullOrWhiteSpace(timeZone) ||
                    string.IsNullOrWhiteSpace(location) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(department) ||
                    string.IsNullOrWhiteSpace(staffdeviceType) ||
                    string.IsNullOrWhiteSpace(machineNoText))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(portText, out int port) || port <= 0)
                {
                    MessageBox.Show("Port must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!int.TryParse(machineNoText, out int machineNo) || machineNo <= 0)
                {
                    MessageBox.Show("Machine No must be a valid positive number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the IP address or Serial is being changed
                int deviceId = int.Parse(staff_DeviceId.Text);
                var originalDevice = _dbHelper.LoadStaffDevices().FirstOrDefault(d => d.StaffDeviceId == deviceId);
                if (originalDevice == null)
                {
                    MessageBox.Show("Original device not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (originalDevice.DeviceIP != staff_txtDeviceIP.Text.Trim() || originalDevice.DeviceSerial != staff_txtDeviceSerial.Text.Trim())
                {
                    MessageBox.Show("IP address and Serial number cannot be changed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update the device in the database
                Device updatedDevice = new Device
                {
                    StaffDeviceId = deviceId,
                    Department = department,
                    DeviceSerial = originalDevice.DeviceSerial, // Keep original serial
                    DeviceIP = originalDevice.DeviceIP,         // Keep original IP
                    Port = port,
                    TimeZone = timeZone,
                    Location = location,
                    Password = password,
                    MachineNo = machineNo,
                    StaffAttendanceType = staffdeviceType,
                };

                try
                {
                    _dbHelper.UpdateStaffDevice(updatedDevice);
                    MessageBox.Show("Device updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reset the button text to "Edit"
                    staff_btnEditDevice.Text = "Edit";

                    // Disable editing of fields
                    ToggleStaffDeviceFieldsEditMode(false);

                    // Refresh the ListView to show updated data
                    SetupStaffDevicesList();
                    update_session();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the device: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

            btn_privilegememb.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_privilegememb.ForeColor = Color.White;

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
            pnl_staffhandling.Visible = false;
            pnl_privilege.Visible = false;
            pnl_setup.Visible = false;
            // End of Button Color Setup

            LoadMemberAttendanceLogs(); 
            LoadManualDoorLogs();
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

        private void btn_downloadpdf_Click(object sender, EventArgs e)
        {
            // Create a SaveFileDialog to let the user choose the save location
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            saveFileDialog.Title = "Save Staff Attendance PDF";
            saveFileDialog.FileName = "StaffAttendance_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Create a new PDF document
                    Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
                    PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    document.Open();

                    // Add title
                    PdfFont titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18f);
                    Paragraph title = new Paragraph("Staff Attendance Report", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);
                    document.Add(new Paragraph("\n"));

                    // Create a table with the same number of columns as the ListView
                    PdfPTable table = new PdfPTable(staff_lvAttendance.Columns.Count);
                    table.WidthPercentage = 100;

                    // Add table headers
                    PdfFont headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);
                    foreach (ColumnHeader column in staff_lvAttendance.Columns)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(column.Text, headerFont));
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }

                    // Add table data
                    PdfFont dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10f);
                    foreach (ListViewItem item in staff_lvAttendance.Items)
                    {
                        // Add the main item text
                        PdfPCell cell = new PdfPCell(new Phrase(item.Text, dataFont));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);

                        // Add subitems
                        foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
                        {
                            if (subItem == item.SubItems[0]) continue; // Skip the first one as it's already added
                            cell = new PdfPCell(new Phrase(subItem.Text, dataFont));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            table.AddCell(cell);
                        }
                    }

                    // Add the table to the document
                    document.Add(table);

                    // Close the document
                    document.Close();

                    MessageBox.Show("PDF generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void pnl_device_Paint(object sender, PaintEventArgs e)
        {

        }

        private void staff_btnFilter_Click_1(object sender, EventArgs e)
        {
            
            string enrollFilter = staff_txtFilterId.Text.Trim();
            DateTime targetDate = staff_dtpFilterDate.Value.Date;

            var attendanceLogs = _dbHelper.GetAllStaffAttendanceLogs(); // dynamic list

            var filtered = attendanceLogs.Where(l =>
            {
                if (!string.IsNullOrEmpty(enrollFilter) && !l.EnrollmentID.ToString().Contains(enrollFilter))
                    return false;

                // Explicitly declare out variables because l.CheckInTime / l.CheckOutTime are dynamic.
                string checkInRaw = l.CheckInTime as string;
                string checkOutRaw = l.CheckOutTime as string;

                DateTime ciDt;
                bool ciMatch = !string.IsNullOrWhiteSpace(checkInRaw) &&
                               DateTime.TryParse(checkInRaw, out ciDt) &&
                               ciDt.Date == targetDate;

                DateTime coDt;
                bool coMatch = !string.IsNullOrWhiteSpace(checkOutRaw) &&
                               DateTime.TryParse(checkOutRaw, out coDt) &&
                               coDt.Date == targetDate;

                return ciMatch || coMatch;
            }).ToList();

            staff_lvAttendance.Items.Clear();
            foreach (var log in filtered)
            {
                var item = new ListViewItem(log.LogID.ToString());
                item.SubItems.Add(log.EnrollmentID.ToString());
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.Name) ? "-" : log.Name);
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckInTime as string) ? "-" : (string)log.CheckInTime);
                item.SubItems.Add(string.IsNullOrWhiteSpace(log.CheckOutTime as string) ? "-" : (string)log.CheckOutTime);
                staff_lvAttendance.Items.Add(item);
            }
        }

        private async void btn_FetchAttendence_Click(object sender, EventArgs e)
        {
            // Show loading state
            btn_FetchAttendence.Enabled = false;

            lbl_fetching.Text = "Fetching staff attendance... Please wait...";
            lbl_fetching.ForeColor = Color.Blue;
            Cursor = Cursors.WaitCursor;

            try
            {
                // Create a CancellationTokenSource
                using (var cts = new CancellationTokenSource())
                {
                    // Log the start of the sync process
                    LogHelper.Log("🔄 Starting staff attendance sync process...");

                    // Pass the required dependencies to the StaffAttendance constructor
                    var staffAttendanceService = new StaffAttendance(_dbHelper, _wsManager);

                    // Call the HandleStaffAttendanceSync method
                    await staffAttendanceService.HandleStaffAttendanceSync(cts.Token);

                    // Log the completion of the sync process
                    LogHelper.Log("✅ Staff attendance sync process completed.");

                    // Load the attendance logs into the ListView
                    LoadAttendanceLogs();
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the sync process
                LogHelper.Log($"❌ Error during staff attendance sync: {ex.Message}");
                MessageBox.Show($"An error occurred during the sync process: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Restore UI state
                lbl_fetching.Text = "Done";
                lbl_fetching.ForeColor = Color.Green;
                btn_FetchAttendence.Enabled = true;
                Cursor = Cursors.Default;

                // Clear the message shortly after to keep the UI clean
                await Task.Delay(1500);
                if (!lbl_fetching.IsDisposed && lbl_fetching.Created)
                {
                    lbl_fetching.Text = "";
                }
            }
        }

        // ================= STAFF TXT SUPPORT =================
        private void EnsureStaffFileExists()
        {
            try
            {
                if (!File.Exists(_staffFilePath))
                {
                    File.WriteAllText(_staffFilePath, string.Empty); // create empty
                }
                if (!Directory.Exists(_staffImagesDir))
                    Directory.CreateDirectory(_staffImagesDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to prepare staff storage: {ex.Message}", "Staff Init Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStaffEntriesFromFile()
        {
            _staffEntries.Clear();
            if (!File.Exists(_staffFilePath)) return;

            foreach (var line in File.ReadAllLines(_staffFilePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('|');
                if (parts.Length < 2) continue;

                int id;
                if (!int.TryParse(parts[0], out id)) continue;

                string name = parts[1];
                long cardId = 0;
                string img = string.Empty;

                if (parts.Length >= 4)
                {
                    // New format: id|name|cardId|imagePath
                    long.TryParse(parts[2], out cardId);
                    img = parts[3];
                }
                else if (parts.Length == 3)
                {
                    // Legacy format: id|name|imagePath
                    img = parts[2];
                    cardId = 0;
                }
                else
                {
                    // Only id|name present
                    cardId = 0;
                    img = string.Empty;
                }

                _staffEntries.Add((id, name, cardId, img));
            }
        }

        private void PopulateStaffListView()
        {
            if (lst_stff == null) return;

            lst_stff.BeginUpdate();
            lst_stff.Clear();
            lst_stff.View = View.Details;
            lst_stff.FullRowSelect = true;
            lst_stff.GridLines = true;
            if (lst_stff.Columns.Count == 0)
            {
                lst_stff.Columns.Add("ID", 60);
                lst_stff.Columns.Add("Name", 160);
                lst_stff.Columns.Add("Card Id", 120);
                lst_stff.Columns.Add("Image", 260);
                lst_stff.Columns[lst_stff.Columns.Count - 1].Width = -2;
            }

            foreach (var e in _staffEntries.OrderBy(x => x.Id))
            {
                var item = new ListViewItem(e.Id.ToString());
                item.SubItems.Add(e.Name);
                item.SubItems.Add(e.CardId == 0 ? "-" : e.CardId.ToString());
                item.SubItems.Add(string.IsNullOrWhiteSpace(e.ImagePath) ? "-" : Path.GetFileName(e.ImagePath));
                item.Tag = e;
                lst_stff.Items.Add(item);
            }
            lst_stff.EndUpdate();
        }

        private void PrepareNextStaffId()
        {
            if (txt_stffid == null) return;
            int next = _staffEntries.Count == 0 ? 1 : _staffEntries.Max(x => x.Id) + 1;
            txt_stffid.Text = next.ToString();
            txt_stffid.ReadOnly = true;
        }

        private void SaveStaffEntryToFile(int id, string name, long cardId, string imagePath)
        {
            // Append as id|name|cardId|imagePath
            var line = $"{id}|{name}|{cardId}|{imagePath}";
            File.AppendAllText(_staffFilePath, line + Environment.NewLine);
            _staffEntries.Add((id, name, cardId, imagePath));
        }

        // ================= IMAGE SELECTION (btn_image) =================
        private void btn_image_Click(object sender, EventArgs e)
        {
            try
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Select Face Image";
                    ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                    ofd.Multiselect = false;
                    if (ofd.ShowDialog() != DialogResult.OK) return;

                    _selectedImagePath = ofd.FileName;
                    // Copy to local StaffImages for consistent path usage
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(_selectedImagePath)}";
                    _storedImagePath = Path.Combine(_staffImagesDir, fileName);
                    File.Copy(_selectedImagePath, _storedImagePath, true);

                    // Simple feedback (optional: show preview in PictureBox if exists)
                    btn_image.Text = "Image Selected";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Image selection failed: {ex.Message}", "Image Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= ENROLL (btn_stffenroll) =================
        private async void btn_stffenroll_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_stffid?.Text) ||
                string.IsNullOrWhiteSpace(txt_stffname?.Text))
            {
                MessageBox.Show("ID and Name are required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id;
            if (!int.TryParse(txt_stffid.Text, out id))
            {
                MessageBox.Show("Invalid ID format.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txt_stffname.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Name cannot be empty.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Optional: staff card ID (if present, we will add RFID as well)
            long staffCardId = 0;
            long.TryParse((txt_staffcardid?.Text ?? string.Empty).Trim(), out staffCardId);

            // Relaxed validation: require at least one of card OR image
            bool hasImage = _storedImagePath != null && File.Exists(_storedImagePath);
            bool hasCard = staffCardId > 0;

            if (!hasImage && !hasCard)
            {
                MessageBox.Show("Please provide either a Card ID or a Face Image to enroll.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btn_stffenroll.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                // Persist to staff.txt with whatever we have (empty image path allowed)
                string imagePathToSave = hasImage ? _storedImagePath : string.Empty;
                SaveStaffEntryToFile(id, name, staffCardId, imagePathToSave);

                // Add to in-memory buffer (face path can be empty)
                _enrolledStaffBuffer.Add((id, name, imagePathToSave));

                // Refresh UI
                PopulateStaffListView();
                PrepareNextStaffId();
                txt_stffname.Text = "";
                _selectedImagePath = null;
                _storedImagePath = null;
                btn_image.Text = "Add Face";
                if (txt_staffcardid != null) txt_staffcardid.Text = string.Empty;

                // Enroll across devices: name, optional RFID, optional face
                // Use the last saved entry's image path (may be empty)
                await EnrollStaffAcrossAllDevicesAsync(id, name, staffCardId, _staffEntries.Last().ImagePath);

                MessageBox.Show("Staff enrolled successfully on available devices.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Enrollment failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_stffenroll.Enabled = true;
                txt_staffcardid.Text = "";
                Cursor = Cursors.Default;
            }
        }

        private async Task EnrollStaffAcrossAllDevicesAsync(int enrollId, string userName, long cardId, string imagePath)
        {
            // Combine both types of devices if needed
            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

            foreach (var device in allDevices)
            {
                try
                {
                    string ip = device.DeviceIP;
                    int port = device.Port;
                    int machineNo = device.MachineNo;
                    string password = device.Password ?? "0";

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
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Error enrolling staff {userName} ({enrollId}) on device {device.DeviceIP}: {ex.Message}");
                }
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

        private void label7_Click(object sender, EventArgs e)
        {

        }

        // helper to rewrite staff.txt after removals
        private void WriteStaffEntriesToFile()
        {
            try
            {
                var lines = _staffEntries.Select(e => $"{e.Id}|{e.Name}|{e.CardId}|{(e.ImagePath ?? string.Empty)}");
                File.WriteAllLines(_staffFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update staff file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // populate ID/Name textboxes on double-click
        private void lst_stff_DoubleClick(object sender, EventArgs e)
        {
            if (lst_stff.SelectedItems.Count == 0) return;

            var item = lst_stff.SelectedItems[0];

            // New tuple with CardId
            if (item.Tag is ValueTuple<int, string, long, string> entry4)
            {
                txt_stffid.Text = entry4.Item1.ToString();
                txt_stffname.Text = entry4.Item2;
                if (txt_staffcardid != null)
                    txt_staffcardid.Text = entry4.Item3 > 0 ? entry4.Item3.ToString() : string.Empty;
                return;
            }

            // Legacy tuple without CardId
            if (item.Tag is ValueTuple<int, string, string> entry3)
            {
                txt_stffid.Text = entry3.Item1.ToString();
                txt_stffname.Text = entry3.Item2;
                if (txt_staffcardid != null)
                    txt_staffcardid.Text = string.Empty;
                return;
            }

            // Fallback from columns
            int parsedId;
            if (int.TryParse(item.Text, out parsedId))
                txt_stffid.Text = parsedId.ToString();
            if (item.SubItems.Count > 1)
                txt_stffname.Text = item.SubItems[1].Text;
            if (txt_staffcardid != null)
            {
                // Column 2 (index 2) is "Card Id" now
                if (item.SubItems.Count > 2)
                    txt_staffcardid.Text = item.SubItems[2].Text == "-" ? string.Empty : item.SubItems[2].Text;
                else
                    txt_staffcardid.Text = string.Empty;
            }
        }

        // delete staff across devices and remove from file + image
        private async void btn_deletestff_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_stffid?.Text))
            {
                MessageBox.Show("Select a staff entry first (double-click on the list).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id;
            if (!int.TryParse(txt_stffid.Text.Trim(), out id))
            {
                MessageBox.Show("Invalid ID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var idx = _staffEntries.FindIndex(s => s.Id == id);
            if (idx < 0)
            {
                MessageBox.Show("Staff entry not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var entry = _staffEntries[idx];
            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

            btn_deletestff.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                // call DeleteEnrollmentData on all devices (backupNumber hardcoded to 12)
                foreach (var device in allDevices)
                {
                    try
                    {
                        string ip = device.DeviceIP;
                        int port = device.Port;
                        int machineNo = device.MachineNo;
                        string password = device.Password ?? "0";
                        int enrollmentNumber = id;
                        int enrollmentMachineId = machineNo; // use device machine no

                        bool ok = await DeviceService.DeleteEnrollmentData(ip, port, machineNo, password, enrollmentNumber, enrollmentMachineId, 12);
                        if (!ok)
                        {
                            LogHelper.Log($"DeleteEnrollmentData failed for {ip}:{port} machineNo={machineNo} enrollment={id}");
                        }
                    }
                    catch (Exception exDev)
                    {
                        LogHelper.Log($"Error deleting enrollment {id} on device {device.DeviceIP}: {exDev.Message}");
                    }
                }

                // remove image (if exists)
                try
                {
                    if (!string.IsNullOrWhiteSpace(entry.ImagePath) && File.Exists(entry.ImagePath))
                        File.Delete(entry.ImagePath);
                }
                catch (Exception imgEx)
                {
                    LogHelper.Log($"Could not delete image for staff {entry.Id}: {imgEx.Message}");
                }

                // remove from in-memory list and rewrite file
                _staffEntries.RemoveAt(idx);
                WriteStaffEntriesToFile();

                // refresh UI
                PopulateStaffListView();
                txt_stffname.Text = "";
                PrepareNextStaffId();

                MessageBox.Show("Staff deleted from devices and removed from file.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Deletion failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_deletestff.Enabled = true;
                txt_staffcardid.Text = "";

                Cursor = Cursors.Default;
            }
        }

        private void btn_privilegememb_Click(object sender, EventArgs e)
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

            btn_setupmanage.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_setupmanage.ForeColor = Color.White;
            //****************************************************************************
            btn_privilegememb.BackColor = Color.WhiteSmoke;
            btn_privilegememb.ForeColor = Color.Black;
            //****************************************************************************
            // Show attendance panel, hide others
            pnl_privilege.Visible = true;
            pnl_privilege.Location = new Point(186, 0);

            pnl_attendence.Visible = false;
            pnl_deviceManager.Visible = false;
            pnl_UserManagement.Visible = false;
            pnl_APIkey.Visible = false;
            pnl_staffhandling.Visible = false;
            pnl_setup.Visible = false;
            // End of Button Color Setup

            StartCardIdListening();
            InitializePrivilegedAttendanceUI();


        }

        // ================= ADD FACE (btn_addfacepriv) =================
        private void btn_addfacepriv_Click(object sender, EventArgs e)
        {
            try
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Select Privilege Member Face Image";
                    ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                    ofd.Multiselect = false;
                    if (ofd.ShowDialog() != DialogResult.OK) return;

                    _privSelectedImagePath = ofd.FileName;
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(_privSelectedImagePath)}";
                    _privStoredImagePath = Path.Combine(_privImagesDir, fileName);
                    File.Copy(_privSelectedImagePath, _privStoredImagePath, true);

                    btn_addfacepriv.Text = "Image Selected";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Image selection failed: {ex.Message}", "Image Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= ENROLL (btn_enrollpriv) =================
        private async void btn_enrollpriv_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txt_privId?.Text) || string.IsNullOrWhiteSpace(txt_prvname?.Text))
            {
                MessageBox.Show("ID and Name are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id;
            if (!int.TryParse(txt_privId.Text.Trim(), out id))
            {
                MessageBox.Show("Invalid ID format.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string name = txt_prvname.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Name cannot be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Card ID may be optional per request; try parse if available
            long cardId = 0;
            long.TryParse(txt_scanid.Text.Trim(), out cardId);

            // Image may also be optional; use selected stored path if present
            string imagePath = _privStoredImagePath;

            btn_enrollpriv.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                // Persist to privilegemember.txt
                SavePrivilegeMemberToFile(id, name, cardId, imagePath ?? string.Empty);

                // Refresh UI
                PopulatePrivilegeMembersListView();
                PrepareNextPrivilegeMemberId();
                txt_prvname.Text = "";
                // reset face selection
                _privSelectedImagePath = null;
                _privStoredImagePath = null;
                btn_addfacepriv.Text = "Add Face";

                // Enroll across all available devices
                await EnrollPrivilegeMemberAcrossAllDevicesAsync(id, name, cardId, imagePath);

                MessageBox.Show("Privilege member enrolled successfully on available devices.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Enrollment failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_enrollpriv.Enabled = true;
                txt_scanid.Text = "";
                _privSelectedImagePath = null;
                _privStoredImagePath = null;

                Cursor = Cursors.Default;
            }
        }

        private async Task EnrollPrivilegeMemberAcrossAllDevicesAsync(int enrollId, string userName, long cardId, string imagePath)
        {
            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

            foreach (var device in allDevices)
            {
                try
                {
                    string ip = device.DeviceIP;
                    int port = device.Port;
                    int machineNo = device.MachineNo;
                    string password = device.Password ?? "0";

                    // 1. Set username
                    bool nameOk = await DeviceService.SetUserName(ip, port, machineNo, password, enrollId, userName, machineNo);
                    if (!nameOk)
                    {
                        LogHelper.Log($"Privilege: SetUserName failed on {ip}:{port} machineNo={machineNo} enroll={enrollId}");
                        continue; // skip RFID/face if name failed
                    }

                    // 2. Add RFID card (if we have a cardId)
                    if (cardId > 0)
                    {
                        bool rfidOk = await DeviceService.AddRfidCard(ip, port, machineNo, password, enrollId, cardId, machineNo);
                        if (!rfidOk)
                        {
                            LogHelper.Log($"Privilege: AddRfidCard failed on {ip}:{port} machineNo={machineNo} enroll={enrollId}");
                        }
                    }

                    // 3. Add face (if we have image)
                    if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                    {
                        bool faceOk = await DeviceService.AddFace(ip, port, machineNo, password, enrollId, imagePath);
                        if (!faceOk)
                        {
                            LogHelper.Log($"Privilege: AddFace failed on {ip}:{port} machineNo={machineNo} enroll={enrollId}");
                        }
                    }

                    LogHelper.Log($"Privilege enrollment completed for {userName} ({enrollId}) on {ip}:{port}");
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Error enrolling privilege member {userName} ({enrollId}) on device {device.DeviceIP}: {ex.Message}");
                }
            }
        }

        // ================= DELETE (btn_delpriv) =================
private async void btn_delpriv_Click(object sender, EventArgs e)
{
    if (string.IsNullOrWhiteSpace(txt_privId?.Text))
    {
        MessageBox.Show("Select a privilege member first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }

    if (!int.TryParse(txt_privId.Text.Trim(), out int id))
    {
        MessageBox.Show("Invalid ID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    var idx = _privMembers.FindIndex(s => s.Id == id);
    if (idx < 0)
    {
        MessageBox.Show("Privilege member not found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }

    var entry = _privMembers[idx];

    btn_delpriv.Enabled = false;
    Cursor = Cursors.WaitCursor;

    try
    {
        // Delete on all devices (face, rfids, etc.). We’ll use DeleteEnrollmentData if available, same as staff deletion
        var allDevices = new List<Device>();
        if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
        if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);

        foreach (var device in allDevices)
        {
            try
            {
                string ip = device.DeviceIP;
                int port = device.Port;
                int machineNo = device.MachineNo;
                string password = device.Password ?? "0";
                int enrollmentNumber = id;
                int enrollmentMachineId = machineNo;

                bool ok = await DeviceService.DeleteEnrollmentData(ip, port, machineNo, password, enrollmentNumber, enrollmentMachineId, 12);
                if (!ok)
                {
                    LogHelper.Log($"Privilege DeleteEnrollmentData failed for {ip}:{port} machineNo={machineNo} enrollment={id}");
                }
            }
            catch (Exception exDev)
            {
                LogHelper.Log($"Error deleting privilege enrollment {id} on device {device.DeviceIP}: {exDev.Message}");
            }
        }

        // remove image (if exists)
        try
        {
            if (!string.IsNullOrWhiteSpace(entry.ImagePath) && File.Exists(entry.ImagePath))
                File.Delete(entry.ImagePath);
        }
        catch (Exception imgEx)
        {
            LogHelper.Log($"Could not delete image for privilege member {entry.Id}: {imgEx.Message}");
        }

        // remove from memory and rewrite file
        _privMembers.RemoveAt(idx);
        WritePrivilegeMembersToFile();

        // refresh UI
        PopulatePrivilegeMembersListView();
        txt_prvname.Text = "";

        txt_scanid.Text = "";

        PrepareNextPrivilegeMemberId();

        MessageBox.Show("Privilege member deleted from devices and removed from file.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Deletion failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        btn_delpriv.Enabled = true;
        Cursor = Cursors.Default;
    }
}

        private void txt_privId_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_prvname_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_scanid_TextChanged(object sender, EventArgs e)
        {

        }

        // ================= PRIVILEGE MEMBERS SUPPORT =================

        private void EnsurePrivilegeMembersFileExists()
        {
            try
            {
                if (!File.Exists(_privMembersFilePath))
                    File.WriteAllText(_privMembersFilePath, string.Empty);
                if (!Directory.Exists(_privImagesDir))
                    Directory.CreateDirectory(_privImagesDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to prepare privilege members storage: {ex.Message}", "Init Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPrivilegeMembersFromFile()
        {
            _privMembers.Clear();
            if (!File.Exists(_privMembersFilePath)) return;

            foreach (var line in File.ReadAllLines(_privMembersFilePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                // Format: id|name|cardId|imagePath
                var parts = line.Split('|');
                if (parts.Length < 3) continue;
                if (!int.TryParse(parts[0], out int id)) continue;
                string name = parts[1];
                long cardId = 0;
                long.TryParse(parts[2], out cardId);
                string img = parts.Length >= 4 ? parts[3] : string.Empty;
                _privMembers.Add((id, name, cardId, img));
            }
        }

        private void PopulatePrivilegeMembersListView()
        {
            if (lst_mempriv == null) return;

            lst_mempriv.BeginUpdate();
            lst_mempriv.Clear();
            lst_mempriv.View = View.Details;
            lst_mempriv.FullRowSelect = true;
            lst_mempriv.GridLines = true;
            if (lst_mempriv.Columns.Count == 0)
            {
                lst_mempriv.Columns.Add("ID", 60);
                lst_mempriv.Columns.Add("Name", 160);
                lst_mempriv.Columns.Add("Card Id", 120);
                lst_mempriv.Columns.Add("Image", 260);
                lst_mempriv.Columns[lst_mempriv.Columns.Count - 1].Width = -2;
            }

            foreach (var e in _privMembers.OrderBy(x => x.Id))
            {
                var item = new ListViewItem(e.Id.ToString());
                item.SubItems.Add(e.Name);
                item.SubItems.Add(e.CardId == 0 ? "-" : e.CardId.ToString());
                item.SubItems.Add(string.IsNullOrWhiteSpace(e.ImagePath) ? "-" : Path.GetFileName(e.ImagePath));
                item.Tag = e;
                lst_mempriv.Items.Add(item);
            }
            lst_mempriv.EndUpdate();
        }

        private void PrepareNextPrivilegeMemberId()
        {
            if (txt_privId == null) return;
            int next = _privMembers.Count == 0 ? 1 : _privMembers.Max(x => x.Id) + 1;
            txt_privId.Text = next.ToString();
            txt_privId.ReadOnly = true;
        }

        private void SavePrivilegeMemberToFile(int id, string name, long cardId, string imagePath)
        {
            var line = $"{id}|{name}|{cardId}|{imagePath}";
            File.AppendAllText(_privMembersFilePath, line + Environment.NewLine);
            _privMembers.Add((id, name, cardId, imagePath));
        }

        private void WritePrivilegeMembersToFile()
        {
            try
            {
                var lines = _privMembers.Select(e => $"{e.Id}|{e.Name}|{e.CardId}|{(e.ImagePath ?? string.Empty)}");
                File.WriteAllLines(_privMembersFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update privilege members file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lst_mempriv_DoubleClick(object sender, EventArgs e)
        {
            if (lst_mempriv.SelectedItems.Count == 0) return;

            var item = lst_mempriv.SelectedItems[0];

            // Prefer Tag (contains full tuple: (int Id, string Name, long CardId, string ImagePath))
            if (item.Tag is ValueTuple<int, string, long, string> entry)
            {
                txt_privId.Text = entry.Item1.ToString();
                txt_prvname.Text = entry.Item2;
                txt_scanid.Text = entry.Item3 > 0 ? entry.Item3.ToString() : string.Empty;

                // Set face image selection indicator and remember stored image path
                if (!string.IsNullOrWhiteSpace(entry.Item4) && File.Exists(entry.Item4))
                {
                    _privStoredImagePath = entry.Item4;
                    _privSelectedImagePath = null; // original source not known here
                    btn_addfacepriv.Text = "Image Selected";
                }
                else
                {
                    _privStoredImagePath = null;
                    _privSelectedImagePath = null;
                    btn_addfacepriv.Text = "Add Face";
                }
                return;
            }

            // Fallback: read from columns if Tag is not set
            int parsedId;
            if (int.TryParse(item.Text, out parsedId))
                txt_privId.Text = parsedId.ToString();

            if (item.SubItems.Count > 1)
                txt_prvname.Text = item.SubItems[1].Text;

            if (item.SubItems.Count > 2)
            {
                var cardText = item.SubItems[2].Text;
                txt_scanid.Text = (cardText == "-" ? string.Empty : cardText);
            }

            // Column 3 (index 3) contains image file name; we cannot reconstruct full path reliably here
            btn_addfacepriv.Text = "Add Face";
            _privStoredImagePath = null;
            _privSelectedImagePath = null;
        }

        // ================= CARD READER LISTENING =================
        private void StartCardIdListening()
        {
            txt_scanid.ReadOnly = true;

            // Stop any previous listening session
            StopCardIdListening();

            _cardListenCts = new CancellationTokenSource();
            var token = _cardListenCts.Token;

            // Clear UI immediately
            txt_scanid.Text = string.Empty;

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
                                    // so the cycle remains snappy and we disconnect between cycles.
                                    int cardId = 0;
                                    var start = DateTime.UtcNow;
                                    while (!token.IsCancellationRequested && !reader.DetectCard(ref cardId))
                                    {
                                        // Sleep lightly to avoid busy wait
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
                                                txt_scanid.Text = cardId.ToString();
                                            }));
                                        }
                                        catch
                                        {
                                            // Ignore UI invoke failures
                                        }
                                    }
                                }
                                finally
                                {
                                    try
                                    {
                                        reader.DisConnectUsb();
                                    }
                                    catch
                                    {
                                        // Ignore disconnect failures
                                    }
                                }
                            });
                        }
                        catch (Exception exCycle)
                        {
                            try { LogHelper.Log($"Card listening cycle error: {exCycle.Message}"); } catch { /* ignore */ }
                        }

                        // Wait 4 seconds between cycles, honoring cancellation
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(4), token);
                        }
                        catch (TaskCanceledException)
                        {
                            // Cancellation requested during delay
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Keep UI responsive; log for diagnostics
                    try { LogHelper.Log($"Card listening error: {ex.Message}"); } catch { /* ignore */ }
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








        private sealed class PrivAttendanceRow
        {
            public int Enroll;
            public string Name;
            public int DevID;
            public string Action;     // normalized "check-in" stored; original row keeps intended action
            public DateTime? CheckIn; // parsed from column 4
            public DateTime? CheckOut;// parsed from column 5
            public string IP;
            public string RawLine;    // original CSV
        }

        private string _privAttendanceFilePath;
        private string _privMembersFilePathLocal;
        private readonly List<PrivAttendanceRow> _privAttendanceCache = new List<PrivAttendanceRow>();
        private DateTime _privAttendanceLastReadUtc = DateTime.MinValue;
        private DateTime _privMembersLastReadUtc = DateTime.MinValue;
        private void InitializePrivilegedAttendanceUI()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _privAttendanceFilePath = Path.Combine(baseDir, "privillegememberattendence.txt");
                _privMembersFilePathLocal = Path.Combine(baseDir, "privilegemember.txt");

                EnsurePrivAttendanceListViewConfigured();
                LoadPrivilegeNamesIntoAutocomplete(forceReload: true);
                LoadPrivAttendanceFile(forceReload: true);
                BindPrivAttendanceRows(); // initial full load

                // Wire events (if not already wired via designer)
                if (btn_filterprv != null)
                    btn_filterprv.Click -= btn_filterprv_Click; // avoid duplicates
                if (btn_filterprv != null)
                    btn_filterprv.Click += btn_filterprv_Click;

                if (txt_privnames != null)
                    txt_privnames.TextChanged -= txt_privnames_TextChanged;
                if (txt_privnames != null)
                    txt_privnames.TextChanged += txt_privnames_TextChanged;

                if (date_priv != null)
                    date_priv.ValueChanged -= date_priv_ValueChanged;
                if (date_priv != null)
                    date_priv.ValueChanged += date_priv_ValueChanged;
            }
            catch (Exception ex)
            {
                LogHelper.Log("[PrivAttendance] Initialization error: " + ex.Message);
            }
        }

        private void EnsurePrivAttendanceListViewConfigured()
        {
            if (lst_privattendence == null)
            {
                lst_privattendence = new ListView
                {
                    Name = "lst_privattendence",
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true,
                    HeaderStyle = ColumnHeaderStyle.Nonclickable,
                    Dock = DockStyle.Fill
                };
                Controls.Add(lst_privattendence);
                lst_privattendence.BringToFront();
            }

            // Always reset columns to avoid duplicates or stale widths
            lst_privattendence.BeginUpdate();
            lst_privattendence.Columns.Clear();

            // Only the requested columns (hide DevID and IP)
            lst_privattendence.Columns.Add("Enroll", 80);
            lst_privattendence.Columns.Add("Name", 180);
            lst_privattendence.Columns.Add("Action", 90);
            lst_privattendence.Columns.Add("Check-In", 160);
            lst_privattendence.Columns.Add("Check-Out", 160);

            lst_privattendence.EndUpdate();
        }

        private void LoadPrivAttendanceFile(bool forceReload = false)
        {
            try
            {
                if (string.IsNullOrEmpty(_privAttendanceFilePath))
                    return;

                if (!File.Exists(_privAttendanceFilePath))
                {
                    _privAttendanceCache.Clear();
                    return;
                }

                var fi = new FileInfo(_privAttendanceFilePath);
                if (!forceReload && fi.LastWriteTimeUtc == _privAttendanceLastReadUtc)
                    return; // no change

                var lines = File.ReadAllLines(_privAttendanceFilePath);
                var list = new List<PrivAttendanceRow>();

                foreach (var raw in lines.Skip(1)) // skip header
                {
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    var cols = raw.Split(',');
                    if (cols.Length < 7) continue;

                    // CSV schema: Enroll,Name,DevID,Action,CheckIn,CheckOut,IP
                    if (!int.TryParse(cols[0].Trim(), out int enroll)) continue;
                    string name = cols[1].Trim();
                    int.TryParse(cols[2].Trim(), out int devId);
                    string action = cols[3].Trim().ToLowerInvariant();
                    DateTime? checkIn = ParseDateTimeNullable(cols[4]);
                    DateTime? checkOut = ParseDateTimeNullable(cols[5]);
                    string ip = cols[6].Trim();

                    list.Add(new PrivAttendanceRow
                    {
                        Enroll = enroll,
                        Name = name,
                        DevID = devId,
                        Action = action,
                        CheckIn = checkIn,
                        CheckOut = checkOut,
                        IP = ip,
                        RawLine = raw
                    });
                }

                _privAttendanceCache.Clear();
                _privAttendanceCache.AddRange(list);
                _privAttendanceLastReadUtc = fi.LastWriteTimeUtc;
            }
            catch (Exception ex)
            {
                LogHelper.Log("[PrivAttendance] Failed reading attendance file: " + ex.Message);
            }
        }

        private void LoadPrivilegeNamesIntoAutocomplete(bool forceReload = false)
        {
            try
            {
                if (txt_privnames == null) return;
                if (string.IsNullOrEmpty(_privMembersFilePathLocal)) return;

                var fi = new FileInfo(_privMembersFilePathLocal);
                if (!fi.Exists)
                {
                    txt_privnames.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                    return;
                }

                if (!forceReload && fi.LastWriteTimeUtc == _privMembersLastReadUtc)
                    return;

                var names = new List<string>();
                foreach (var raw in File.ReadAllLines(_privMembersFilePathLocal))
                {
                    var line = (raw ?? "").Trim();
                    if (line.Length == 0) continue;
                    if (line.StartsWith("#") || line.StartsWith("//") || line.StartsWith(";")) continue;

                    var parts = line.Split(new[] { ',', '=', '|', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;

                    names.Add(parts[1].Trim());
                }

                var ac = new AutoCompleteStringCollection();
                ac.AddRange(names.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToArray());

                txt_privnames.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txt_privnames.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txt_privnames.AutoCompleteCustomSource = ac;

                _privMembersLastReadUtc = fi.LastWriteTimeUtc;
            }
            catch (Exception ex)
            {
                LogHelper.Log("[PrivAttendance] Failed loading privileged names: " + ex.Message);
            }
        }

        private void BindPrivAttendanceRows(IEnumerable<PrivAttendanceRow> src = null)
        {
            EnsurePrivAttendanceListViewConfigured();

            lst_privattendence.BeginUpdate();
            lst_privattendence.Items.Clear();
            lst_privattendence.View = View.Details; // enforce details view

            var rows = src ?? _privAttendanceCache;

            foreach (var r in rows.OrderByDescending(r => r.CheckIn ?? r.CheckOut ?? DateTime.MinValue))
            {
                // Item text -> first column "Enroll"
                var lvi = new ListViewItem(r.Enroll.ToString());

                // Subsequent subitems must match defined columns order
                lvi.SubItems.Add(string.IsNullOrWhiteSpace(r.Name) ? "-" : r.Name);
                lvi.SubItems.Add(string.IsNullOrWhiteSpace(r.Action) ? "-" : r.Action);
                lvi.SubItems.Add(r.CheckIn?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-");
                lvi.SubItems.Add(r.CheckOut?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-");

                lst_privattendence.Items.Add(lvi);
            }

            // Auto resize to content so text isn’t truncated as "…"
            for (int i = 0; i < lst_privattendence.Columns.Count; i++)
            {
                lst_privattendence.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                // Ensure a minimum width for readability
                if (lst_privattendence.Columns[i].Width < 80)
                    lst_privattendence.Columns[i].Width = 80;
            }

            lst_privattendence.EndUpdate();
        }

        // Filter logic invoked by button or typing/date change
        private void ApplyPrivAttendanceFilter()
        {
            LoadPrivAttendanceFile();               // refresh if file changed
            LoadPrivilegeNamesIntoAutocomplete();   // refresh suggestions if names file changed

            string nameFilter = txt_privnames?.Text.Trim();
            bool hasName = !string.IsNullOrEmpty(nameFilter);

            DateTime? dateFilter = null;
            if (date_priv != null)
            {
                // If you want "no date filter" when user hasn't explicitly picked, add UI toggle.
                dateFilter = date_priv.Value.Date;
            }
            bool hasDate = dateFilter.HasValue;

            IEnumerable<PrivAttendanceRow> q = _privAttendanceCache;

            if (hasName)
            {
                q = q.Where(r =>
                    (!string.IsNullOrEmpty(r.Name) &&
                     r.Name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (hasDate)
            {
                q = q.Where(r =>
                {
                    // Match if either CheckIn or CheckOut date equals selected date
                    var ci = r.CheckIn?.Date;
                    var co = r.CheckOut?.Date;
                    return (ci.HasValue && ci.Value == dateFilter.Value) ||
                           (co.HasValue && co.Value == dateFilter.Value) ||
                           // If only one timestamp present (open session), let that count
                           (!ci.HasValue && co.HasValue && co.Value == dateFilter.Value) ||
                           (ci.HasValue && !co.HasValue && ci.Value == dateFilter.Value);
                });
            }

            BindPrivAttendanceRows(q);
        }

        private void btn_filterprv_Click(object sender, EventArgs e)
        {
            ApplyPrivAttendanceFilter();
        }
        private void txt_privnames_TextChanged(object sender, EventArgs e)
        {
            // Optional: live filtering as user types (remove if you only want button-based filtering)
            ApplyPrivAttendanceFilter();
        }

        private void date_priv_ValueChanged(object sender, EventArgs e)
        {
            // Optional: live filtering on date change
            ApplyPrivAttendanceFilter();
        }

        private DateTime? ParseDateTimeNullable(string raw)
        {
            var s = (raw ?? "").Trim();
            if (s.Length == 0) return null;
            DateTime dt;
            if (DateTime.TryParse(s, out dt))
                return dt;
            return null;
        }

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

            btn_privilegememb.BackColor = ColorTranslator.FromHtml("18,23,49");
            btn_privilegememb.ForeColor = Color.White;
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
            pnl_staffhandling.Visible = false;
            pnl_privilege.Visible = false;
            // End of Button Color Setup

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
                            : (string.IsNullOrWhiteSpace(userNameDevice) ? "(name not found)" : $"\"{userNameDevice}\"");
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
                            : (string.IsNullOrWhiteSpace(userNameDevice) ? "(name not found)" : $"\"{userNameDevice}\"");
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
    }
}
