using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace smart_attendance_middleware

{
    public partial class AddNewDevice : Form
    {

        private DatabaseHelper _dbHelper;
        private Processor _processor;
        private WebSocketManager _wsManager;
        private DeviceFunctions _deviceFunctions = new DeviceFunctions();


        private string deviceSerialNo ;
        public AddNewDevice()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
            _processor = new Processor(_dbHelper, _wsManager,_deviceFunctions);
        }

        private void AddNewDevice_Load(object sender, EventArgs e)
        {
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            txt_department.Clear();
            txt_Serial.Clear();
            txt_ip.Clear();
            txt_port.Clear();
            txt_timeZone.Clear();
            txt_location.Clear();
            txt_face_capacity.Clear();
            txt_RFID_capacity.Clear();
            txt_finger_capacity.Clear();
            txt_password.Clear();

            cmb_connectionType.SelectedIndex = -1;
            cmb_machine_no.SelectedIndex = -1;
            cmb_device_type.SelectedIndex = -1;

            this.Close();

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
                        dashboard.Invoke(new Action(async () => await dashboard.RefreshDevicePanelAsync()));
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

        //private async void btnSave_Click(object sender, EventArgs e)
        //{
        //    lbl_saving_status.Text = "Checking the device, please wait...";
        //    lbl_saving_status.Refresh();
        //    btnSave.Enabled = false;

        //    try
        //    {
        //        bool isFirstDevice = _dbHelper.IsFirstDevice();
        //        // Basic validation (unchanged)
        //        if (string.IsNullOrWhiteSpace(txt_department.Text) ||
        //            string.IsNullOrWhiteSpace(txt_ip.Text) ||
        //            string.IsNullOrWhiteSpace(txt_port.Text) ||
        //            string.IsNullOrWhiteSpace(txt_timeZone.Text) ||
        //            string.IsNullOrWhiteSpace(txt_location.Text) ||
        //            (string.IsNullOrWhiteSpace(txt_face_capacity.Text) &&
        //             string.IsNullOrWhiteSpace(txt_RFID_capacity.Text) &&
        //             string.IsNullOrWhiteSpace(txt_finger_capacity.Text)) ||
        //            cmb_device_type.SelectedItem == null ||
        //            cmb_connectionType.SelectedItem == null ||
        //            cmb_machine_no.SelectedItem == null)
        //        {
        //            MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        string ip = txt_ip.Text.Trim();
        //        int port = int.TryParse(txt_port.Text, out int parsedPort) ? parsedPort : 5005;
        //        int machineNo = Convert.ToInt32(cmb_machine_no.SelectedItem);
        //        string password = txt_password.Text.Trim();

        //        // Check only IP and machine number
        //        string checkResult = _dbHelper.CheckDeviceExists(ip, machineNo);
        //        if (checkResult != "Neither IP nor Machine No exists.")
        //        {
        //            lbl_saving_status.Text = checkResult;
        //            MessageBox.Show(checkResult, "Device Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        // Ping the device with retries
        //        bool isReachable = await PingDeviceWithRetries(ip, maxAttempts: 5, delayBetweenAttempts: 2000);
        //        if (!isReachable)
        //        {
        //            lbl_saving_status.Text = "Device connection failed";
        //            MessageBox.Show($"Could not connect to device at {ip} after 5 attempts. Please check the device network connection.",
        //                "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        lbl_saving_status.Text = "Connecting to device and retrieving serial number...";
        //        lbl_saving_status.Refresh();

        //        string serial = await DeviceService.GetSerial(ip, port, machineNo, password);

        //        lbl_saving_status.Text = "Device found and serial retrieved.";
        //        txt_Serial.Text = serial;
        //        txt_Serial.Refresh();

        //        DialogResult result = MessageBox.Show(
        //            $"Device connected successfully.\n\nSerial Number: {serial}\nIP Address: {ip}\n\nWould you like to save this device?",
        //            "Confirm Device Addition",
        //            MessageBoxButtons.YesNo,
        //            MessageBoxIcon.Question);

        //        if (result == DialogResult.No)
        //        {
        //            MessageBox.Show(
        //                "You've cancelled the device addition.\n\nNo changes have been made.",
        //                "Device Not Added",
        //                MessageBoxButtons.OK,
        //                MessageBoxIcon.Information);
        //            return;
        //        }

        //        bool attendanceType = cmb_device_type.SelectedItem.ToString() == "Check-in";
        //        bool isActive = true;

        //        Device newDevice = new Device
        //        {
        //            Department = txt_department.Text,
        //            AttendanceType = attendanceType,
        //            DeviceSerial = serial,
        //            DeviceIP = ip,
        //            Port = port,
        //            TimeZone = txt_timeZone.Text.Trim(),
        //            Location = txt_location.Text.Trim(),
        //            ConnectionType = cmb_connectionType.SelectedItem.ToString(),
        //            IsActive = isActive,
        //            LastSyncTime = null,
        //            FaceCapacity = int.TryParse(txt_face_capacity.Text, out int faceCap) ? faceCap : 0,
        //            RFIDCapacity = int.TryParse(txt_RFID_capacity.Text, out int rfidCap) ? rfidCap : 0,
        //            FingerCapacity = int.TryParse(txt_finger_capacity.Text, out int fingerCap) ? fingerCap : 0,
        //            MachineNo = machineNo,
        //            Password = password
        //        };

        //        _dbHelper.SaveDevice(newDevice);
        //        _processor.update_session();
        //        lbl_saving_status.Text = "Device added successfully.";

        //        // Notify dashboard to refresh
        //        NotifyDashboardRefresh();

        //        MessageBox.Show("Device has been saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        //        // Only process user workflow if this isn't the first device (after message box)
        //        if (!isFirstDevice)
        //        {
        //            lbl_saving_status.Text = "Processing user workflow...";
        //            lbl_saving_status.Refresh();

        //            await _deviceFunctions.ProcessUserWorkflowAsync();
        //            lbl_saving_status.Text = "Completed user workflow...";

        //        }

        //        this.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error saving device: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        btnSave.Enabled = true;
        //    }
        //}

        private async void btnSave_Click(object sender, EventArgs e)
        {
            lbl_saving_status.Text = "Checking the device, please wait...";
            lbl_saving_status.Refresh();
            btnSave.Enabled = false;

            try
            {
                bool isFirstDevice = _dbHelper.IsFirstDevice();
                // Basic validation (unchanged)
                if (string.IsNullOrWhiteSpace(txt_department.Text) ||
                    string.IsNullOrWhiteSpace(txt_ip.Text) ||
                    string.IsNullOrWhiteSpace(txt_port.Text) ||
                    string.IsNullOrWhiteSpace(txt_timeZone.Text) ||
                    string.IsNullOrWhiteSpace(txt_location.Text) ||
                    (string.IsNullOrWhiteSpace(txt_face_capacity.Text) &&
                     string.IsNullOrWhiteSpace(txt_RFID_capacity.Text) &&
                     string.IsNullOrWhiteSpace(txt_finger_capacity.Text)) ||
                    cmb_device_type.SelectedItem == null ||
                    cmb_connectionType.SelectedItem == null ||
                    cmb_machine_no.SelectedItem == null)
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string ip = txt_ip.Text.Trim();
                int port = int.TryParse(txt_port.Text, out int parsedPort) ? parsedPort : 5005;
                int machineNo = Convert.ToInt32(cmb_machine_no.SelectedItem);
                string password = txt_password.Text.Trim();

                // Check only IP and machine number (normal Devices table check)
                string checkResult = _dbHelper.CheckDeviceExists(ip, machineNo);
                if (checkResult != "Neither IP nor Machine No exists.")
                {
                    lbl_saving_status.Text = checkResult;
                    MessageBox.Show(checkResult, "Device Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Ping the device with retries
                bool isReachable = await PingDeviceWithRetries(ip, maxAttempts: 5, delayBetweenAttempts: 2000);
                if (!isReachable)
                {
                    lbl_saving_status.Text = "Device connection failed";
                    MessageBox.Show($"Could not connect to device at {ip} after 5 attempts. Please check the device network connection.",
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                lbl_saving_status.Text = "Connecting to device and retrieving serial number...";
                lbl_saving_status.Refresh();

                string serial = await DeviceService.GetSerial(ip, port, machineNo, password);

                lbl_saving_status.Text = "Device found and serial retrieved.";
                txt_Serial.Text = serial;
                txt_Serial.Refresh();

                DialogResult result = MessageBox.Show(
                    $"Device connected successfully.\n\nSerial Number: {serial}\nIP Address: {ip}\n\nWould you like to save this device?",
                    "Confirm Device Addition",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    MessageBox.Show(
                        "You've cancelled the device addition.\n\nNo changes have been made.",
                        "Device Not Added",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Map device type once from UI
                string attendanceText = cmb_device_type.SelectedItem.ToString().Trim(); // "Check-in", "Check-out", or "Check-in/Check-out"
                bool isActive = true;

                // Create common device object (fields common to both)
                var deviceModel = new Device
                {
                    Department = txt_department.Text,
                    DeviceSerial = serial,
                    DeviceIP = ip,
                    Port = port,
                    TimeZone = txt_timeZone.Text.Trim(),
                    Location = txt_location.Text.Trim(),
                    ConnectionType = cmb_connectionType.SelectedItem.ToString(),
                    IsActive = isActive,
                    LastSyncTime = null,
                    FaceCapacity = int.TryParse(txt_face_capacity.Text, out int faceCap) ? faceCap : 0,
                    RFIDCapacity = int.TryParse(txt_RFID_capacity.Text, out int rfidCap) ? rfidCap : 0,
                    FingerCapacity = int.TryParse(txt_finger_capacity.Text, out int fingerCap) ? fingerCap : 0,
                    MachineNo = machineNo,
                    Password = password
                };

                // Staff vs normal branch
                if (chk_isstaff != null && chk_isstaff.Checked)
                {
                    // Staff device supports all three: "Check-in", "Check-out", "Check-in/Check-out"
                    deviceModel.StaffAttendanceType = attendanceText;

                    // Duplicate guard for staff devices
                    if (_dbHelper.StaffDeviceExists(ip, machineNo))
                    {
                        lbl_saving_status.Text = "Staff device with same IP or Machine No already exists.";
                        MessageBox.Show("Staff device with same IP or Machine No already exists.", "Duplicate Staff Device", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _dbHelper.SaveStaffDevice(deviceModel);
                    lbl_saving_status.Text = "Staff device added successfully.";
                }
                else
                {
                    // Normal devices only support boolean check-in/check-out.
                    if (string.Equals(attendanceText, "Check-in/Check-out", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Normal devices cannot be 'Check-in/Check-out'. Please select 'Check-in' or 'Check-out', or mark as Staff device.", "Invalid Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    deviceModel.AttendanceType = string.Equals(attendanceText, "Check-in", StringComparison.OrdinalIgnoreCase);
                    _dbHelper.SaveDevice(deviceModel);
                    lbl_saving_status.Text = "Device added successfully.";
                }

                _processor.update_session();

                // Notify dashboard to refresh
                NotifyDashboardRefresh();

                MessageBox.Show("Device has been saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Only process user workflow if this isn't the first device (after message box)
                if (!isFirstDevice)
                {
                    lbl_saving_status.Text = "Processing user workflow...";
                    lbl_saving_status.Refresh();

                    await _deviceFunctions.ProcessUserWorkflowAsync();
                    lbl_saving_status.Text = "Completed user workflow...";
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving device: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        public async Task<bool> PingDeviceWithRetries(string ipAddress, int maxAttempts = 5, int delayBetweenAttempts = 2000, int timeout = 2000)
        {
            try
            {
                int attempt = 1;
                bool isReachable = false;

                using (var ping = new Ping())
                {
                    while (attempt <= maxAttempts && !isReachable)
                    {
                        lbl_saving_status.Text = $"Pinging device (attempt {attempt} of {maxAttempts})...";
                        lbl_saving_status.Refresh();

                        try
                        {
                            var reply = await ping.SendPingAsync(ipAddress, timeout);
                            if (reply.Status == IPStatus.Success)
                            {
                                isReachable = true;
                                break;
                            }
                        }
                        catch (PingException)
                        {
                            // Ignore and retry
                        }

                        attempt++;
                        if (attempt <= maxAttempts)
                        {
                            await Task.Delay(delayBetweenAttempts);
                        }
                    }
                }

                return isReachable;
            }
            finally
            {

            }
        }




        private void btn_load_Click(object sender, EventArgs e)
        {
            txt_password.Text = "0"; 
            txt_port.Text = "5005";
            txt_location.Text = "Gate";
            txt_timeZone.Text = "+5";
            txt_face_capacity.Text = "100";
            txt_department.Text = "HR";
            cmb_connectionType.SelectedItem = "LAN";
            cmb_device_type.SelectedItem = "Check-in";
            cmb_machine_no.SelectedItem = "1";
        }

        private void pnl_device_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
