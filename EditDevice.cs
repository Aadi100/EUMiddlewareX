using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class EditDevice : Form
    {
        private DatabaseHelper _dbHelper;
        private Processor _processor;
        private WebSocketManager _wsManager;
        private DeviceFunctions _deviceFunctions = new DeviceFunctions();

        private readonly Device _device;
        private readonly bool _isStaff;

        public EditDevice(Device device, bool isStaff)
        {
            InitializeComponent();
            _device = device;
            _isStaff = isStaff;
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
            _wsManager = new WebSocketManager(_dbHelper);
            _processor = new Processor(_dbHelper, _wsManager, _deviceFunctions);
        }

        private void EditDevice_Load(object sender, EventArgs e)
        {
            // Populate combo items
            cmb_device_type.Items.Clear();
            cmb_device_type.Items.Add("Check-in");
            cmb_device_type.Items.Add("Check-out");
            cmb_device_type.Items.Add("Check-in/Check-out");

            cmb_connectionType.Items.Clear();
            cmb_connectionType.Items.Add("LAN");
            cmb_connectionType.Items.Add("Wi-Fi");
            cmb_connectionType.Items.Add("USB");

            cmb_machine_no.Items.Clear();
            for (int i = 1; i <= 99; i++) cmb_machine_no.Items.Add(i.ToString());

            cmb_timeZone.Items.Clear();
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
                cmb_timeZone.Items.Add(tz.StandardName);

            // Pre-fill fields from device
            txt_department.Text    = _device.Department   ?? "";
            txt_Serial.Text        = _device.DeviceSerial ?? "";
            txt_ip.Text            = _device.DeviceIP     ?? "";
            txt_port.Text          = _device.Port.ToString();
            txt_location.Text      = _device.Location     ?? "";
            txt_password.Text      = _device.Password     ?? "";
            txt_face_capacity.Text   = _device.FaceCapacity.ToString();
            txt_RFID_capacity.Text   = _device.RFIDCapacity.ToString();
            txt_finger_capacity.Text = _device.FingerCapacity.ToString();

            chk_isstaff.Checked = _isStaff;

            // Connection type
            if (!string.IsNullOrEmpty(_device.ConnectionType))
                cmb_connectionType.SelectedItem = _device.ConnectionType;

            // Machine no
            cmb_machine_no.SelectedItem = _device.MachineNo.ToString();

            // TimeZone
            if (!string.IsNullOrEmpty(_device.TimeZone))
                cmb_timeZone.SelectedItem = _device.TimeZone;
            if (cmb_timeZone.SelectedIndex < 0 && cmb_timeZone.Items.Count > 0)
                cmb_timeZone.SelectedIndex = 0;

            // Device / attendance type
            if (_isStaff)
            {
                string staffType = _device.StaffAttendanceType ?? "Check-in";
                cmb_device_type.SelectedItem = staffType;
            }
            else
            {
                cmb_device_type.SelectedItem = _device.AttendanceType ? "Check-in" : "Check-out";
            }
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NotifyDashboardRefresh()
        {
            try
            {
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
                LogHelper.Log($"Error notifying dashboard refresh: {ex.Message}");
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            lbl_saving_status.Text = "Validating...";
            lbl_saving_status.Refresh();
            btnSave.Enabled = false;

            try
            {
                if (string.IsNullOrWhiteSpace(txt_ip.Text) ||
                    string.IsNullOrWhiteSpace(txt_port.Text) ||
                    cmb_timeZone.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txt_location.Text) ||
                    cmb_device_type.SelectedItem == null ||
                    cmb_connectionType.SelectedItem == null ||
                    cmb_machine_no.SelectedItem == null)
                {
                    ModernMessageBox.Show(this, "Validation Error", "Please fill in all required fields.", MsgType.Warning);
                    return;
                }

                string ip        = txt_ip.Text.Trim();
                int    port      = int.TryParse(txt_port.Text, out int pp) ? pp : 5005;
                int    machineNo = int.TryParse(cmb_machine_no.SelectedItem?.ToString(), out int mn) ? mn : 0;
                string password  = txt_password.Text.Trim();
                string attendanceText = cmb_device_type.SelectedItem.ToString().Trim();

                // Conflict check — exclude self
                if (!_isStaff)
                {
                    string conflict = _dbHelper.IDCheckDeviceExists(ip, machineNo, _device.DeviceId);
                    if (!string.IsNullOrEmpty(conflict))
                    {
                        ModernMessageBox.Show(this, "Device Conflict", conflict, MsgType.Warning);
                        return;
                    }
                }

                lbl_saving_status.Text = "Testing connection...";
                lbl_saving_status.Refresh();

                bool reachable = await PingDeviceWithRetries(ip, maxAttempts: 3, delayBetweenAttempts: 1000);
                if (!reachable)
                {
                    lbl_saving_status.Text = "Connection failed";
                    if (!ModernMessageBox.ShowConfirm(this, "Connection Warning", $"Could not reach device at {ip}. Save anyway?", "Save Anyway", "Cancel"))
                        return;
                }

                var deviceModel = new Device
                {
                    Department     = txt_department.Text,
                    DeviceSerial   = txt_Serial.Text.Trim(),
                    DeviceIP       = ip,
                    Port           = port,
                    TimeZone       = cmb_timeZone.SelectedItem?.ToString() ?? "",
                    Location       = txt_location.Text.Trim(),
                    ConnectionType = cmb_connectionType.SelectedItem?.ToString() ?? "",
                    Password       = password,
                    MachineNo      = machineNo,
                    IsActive       = _device.IsActive,
                    LastSyncTime   = _device.LastSyncTime,
                    FaceCapacity   = int.TryParse(txt_face_capacity.Text,   out int fc) ? fc : 0,
                    RFIDCapacity   = int.TryParse(txt_RFID_capacity.Text,   out int rc) ? rc : 0,
                    FingerCapacity = int.TryParse(txt_finger_capacity.Text, out int fg) ? fg : 0,
                };

                if (_isStaff)
                {
                    deviceModel.StaffDeviceId      = _device.StaffDeviceId;
                    deviceModel.StaffAttendanceType = attendanceText;
                    _dbHelper.UpdateStaffDevice(deviceModel);
                }
                else
                {
                    deviceModel.DeviceId       = _device.DeviceId;
                    deviceModel.AttendanceType = string.Equals(attendanceText, "Check-in", StringComparison.OrdinalIgnoreCase);
                    _dbHelper.UpdateDevice(deviceModel);
                }

                _processor.update_session();
                NotifyDashboardRefresh();

                lbl_saving_status.Text = "Saved successfully.";
                ModernMessageBox.Show(this, "Success", "Device updated successfully.", MsgType.Success);
                this.Close();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, "Error", "Error updating device: " + ex.Message, MsgType.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private async Task<bool> PingDeviceWithRetries(string ip, int maxAttempts = 3, int delayBetweenAttempts = 1000, int timeout = 2000)
        {
            try
            {
                using (var ping = new Ping())
                {
                    for (int i = 1; i <= maxAttempts; i++)
                    {
                        lbl_saving_status.Text = $"Pinging device (attempt {i}/{maxAttempts})...";
                        lbl_saving_status.Refresh();
                        try
                        {
                            var reply = await ping.SendPingAsync(ip, timeout);
                            if (reply.Status == IPStatus.Success) return true;
                        }
                        catch (PingException) { }
                        if (i < maxAttempts) await Task.Delay(delayBetweenAttempts);
                    }
                }
                return false;
            }
            catch { return false; }
        }

        private async void btn_testConnection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_ip.Text))
            {
                lbl_saving_status.Text = "Enter device IP first.";
                return;
            }
            btn_testConnection.Enabled = false;
            lbl_saving_status.Text = "Testing...";
            bool ok = await PingDeviceWithRetries(txt_ip.Text.Trim(), maxAttempts: 2, delayBetweenAttempts: 1000);
            lbl_saving_status.Text = ok ? "Connection successful" : "Connection failed";
            btn_testConnection.Enabled = true;
        }

        private void pnlFooter_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 224, 238)))
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
        }
    }
}
