namespace smart_attendance_middleware
{
    partial class EditDevice
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddNewDevice));
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.lblSectionDevice = new System.Windows.Forms.Label();
            this.pnlSepDevice = new System.Windows.Forms.Panel();
            this.lblLDeviceType = new System.Windows.Forms.Label();
            this.cmb_device_type = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblLConnectionType = new System.Windows.Forms.Label();
            this.cmb_connectionType = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblLSerial = new System.Windows.Forms.Label();
            this.txt_Serial = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLMachineNo = new System.Windows.Forms.Label();
            this.cmb_machine_no = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblLDepartment = new System.Windows.Forms.Label();
            this.txt_department = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLLocation = new System.Windows.Forms.Label();
            this.txt_location = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.chk_isstaff = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.lblStaffTitle = new System.Windows.Forms.Label();
            this.lblStaffDesc = new System.Windows.Forms.Label();
            this.lblSectionConnection = new System.Windows.Forms.Label();
            this.pnlSepConnection = new System.Windows.Forms.Panel();
            this.lblLDeviceIP = new System.Windows.Forms.Label();
            this.txt_ip = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLPort = new System.Windows.Forms.Label();
            this.txt_port = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLPassword = new System.Windows.Forms.Label();
            this.txt_password = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLTimeZone = new System.Windows.Forms.Label();
            this.cmb_timeZone = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblSectionCapacity = new System.Windows.Forms.Label();
            this.pnlSepCapacity = new System.Windows.Forms.Panel();
            this.lblLFaceCapacity = new System.Windows.Forms.Label();
            this.txt_face_capacity = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLRFIDCapacity = new System.Windows.Forms.Label();
            this.txt_RFID_capacity = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLFingerCapacity = new System.Windows.Forms.Label();
            this.txt_finger_capacity = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btn_testConnection = new smart_attendance_middleware.Controls.RoundedButton();
            this.lbl_saving_status = new System.Windows.Forms.Label();
            this.btnCancle = new smart_attendance_middleware.Controls.RoundedButton();
            this.btnSave = new smart_attendance_middleware.Controls.RoundedButton();
            this.pnlHeader.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(26, 32, 64);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(960, 68);
            this.pnlHeader.TabIndex = 0;
            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(24, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Edit Device";
            // lblSubtitle
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(150, 165, 215);
            this.lblSubtitle.Location = new System.Drawing.Point(24, 42);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Update the biometric device configuration";
            // pnlContent
            this.pnlContent.AutoScroll = true;
            this.pnlContent.BackColor = System.Drawing.Color.White;
            this.pnlContent.Controls.Add(this.lblSectionDevice);
            this.pnlContent.Controls.Add(this.pnlSepDevice);
            this.pnlContent.Controls.Add(this.lblLDeviceType);
            this.pnlContent.Controls.Add(this.cmb_device_type);
            this.pnlContent.Controls.Add(this.lblLConnectionType);
            this.pnlContent.Controls.Add(this.cmb_connectionType);
            this.pnlContent.Controls.Add(this.lblLSerial);
            this.pnlContent.Controls.Add(this.txt_Serial);
            this.pnlContent.Controls.Add(this.lblLMachineNo);
            this.pnlContent.Controls.Add(this.cmb_machine_no);
            this.pnlContent.Controls.Add(this.lblLDepartment);
            this.pnlContent.Controls.Add(this.txt_department);
            this.pnlContent.Controls.Add(this.lblLLocation);
            this.pnlContent.Controls.Add(this.txt_location);
            this.pnlContent.Controls.Add(this.chk_isstaff);
            this.pnlContent.Controls.Add(this.lblStaffTitle);
            this.pnlContent.Controls.Add(this.lblStaffDesc);
            this.pnlContent.Controls.Add(this.lblSectionConnection);
            this.pnlContent.Controls.Add(this.pnlSepConnection);
            this.pnlContent.Controls.Add(this.lblLDeviceIP);
            this.pnlContent.Controls.Add(this.txt_ip);
            this.pnlContent.Controls.Add(this.lblLPort);
            this.pnlContent.Controls.Add(this.txt_port);
            this.pnlContent.Controls.Add(this.lblLPassword);
            this.pnlContent.Controls.Add(this.txt_password);
            this.pnlContent.Controls.Add(this.lblLTimeZone);
            this.pnlContent.Controls.Add(this.cmb_timeZone);
            this.pnlContent.Controls.Add(this.lblSectionCapacity);
            this.pnlContent.Controls.Add(this.pnlSepCapacity);
            this.pnlContent.Controls.Add(this.lblLFaceCapacity);
            this.pnlContent.Controls.Add(this.txt_face_capacity);
            this.pnlContent.Controls.Add(this.lblLRFIDCapacity);
            this.pnlContent.Controls.Add(this.txt_RFID_capacity);
            this.pnlContent.Controls.Add(this.lblLFingerCapacity);
            this.pnlContent.Controls.Add(this.txt_finger_capacity);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 68);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(960, 412);
            this.pnlContent.TabIndex = 1;
            // lblSectionDevice
            this.lblSectionDevice.AutoSize = true;
            this.lblSectionDevice.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSectionDevice.ForeColor = System.Drawing.Color.FromArgb(30, 38, 90);
            this.lblSectionDevice.Location = new System.Drawing.Point(24, 18);
            this.lblSectionDevice.Name = "lblSectionDevice";
            this.lblSectionDevice.TabIndex = 0;
            this.lblSectionDevice.Text = "Device";
            // pnlSepDevice
            this.pnlSepDevice.BackColor = System.Drawing.Color.FromArgb(220, 224, 238);
            this.pnlSepDevice.Location = new System.Drawing.Point(24, 36);
            this.pnlSepDevice.Name = "pnlSepDevice";
            this.pnlSepDevice.Size = new System.Drawing.Size(912, 1);
            this.pnlSepDevice.TabIndex = 1;
            // lblLDeviceType
            this.lblLDeviceType.AutoSize = true;
            this.lblLDeviceType.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLDeviceType.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLDeviceType.Location = new System.Drawing.Point(24, 40);
            this.lblLDeviceType.Name = "lblLDeviceType";
            this.lblLDeviceType.TabIndex = 2;
            this.lblLDeviceType.Text = "Device type *";
            // cmb_device_type
            this.cmb_device_type.BackColor = System.Drawing.Color.Transparent;
            this.cmb_device_type.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.cmb_device_type.DataSource = null;
            this.cmb_device_type.DisplayMember = "";
            this.cmb_device_type.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.cmb_device_type.Location = new System.Drawing.Point(24, 56);
            this.cmb_device_type.Name = "cmb_device_type";
            this.cmb_device_type.Radius = 10;
            this.cmb_device_type.SelectedIndex = -1;
            this.cmb_device_type.SelectedItem = null;
            this.cmb_device_type.Size = new System.Drawing.Size(218, 36);
            this.cmb_device_type.TabIndex = 3;
            this.cmb_device_type.ValueMember = "";
            // lblLConnectionType
            this.lblLConnectionType.AutoSize = true;
            this.lblLConnectionType.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLConnectionType.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLConnectionType.Location = new System.Drawing.Point(495, 40);
            this.lblLConnectionType.Name = "lblLConnectionType";
            this.lblLConnectionType.TabIndex = 4;
            this.lblLConnectionType.Text = "Connection type *";
            // cmb_connectionType
            this.cmb_connectionType.BackColor = System.Drawing.Color.Transparent;
            this.cmb_connectionType.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.cmb_connectionType.DataSource = null;
            this.cmb_connectionType.DisplayMember = "";
            this.cmb_connectionType.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.cmb_connectionType.Location = new System.Drawing.Point(497, 58);
            this.cmb_connectionType.Name = "cmb_connectionType";
            this.cmb_connectionType.Radius = 10;
            this.cmb_connectionType.SelectedIndex = -1;
            this.cmb_connectionType.SelectedItem = null;
            this.cmb_connectionType.Size = new System.Drawing.Size(218, 36);
            this.cmb_connectionType.TabIndex = 5;
            this.cmb_connectionType.ValueMember = "";
            // lblLSerial
            this.lblLSerial.AutoSize = true;
            this.lblLSerial.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLSerial.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLSerial.Location = new System.Drawing.Point(24, 108);
            this.lblLSerial.Name = "lblLSerial";
            this.lblLSerial.TabIndex = 6;
            this.lblLSerial.Text = "Device serial";
            // txt_Serial
            this.txt_Serial.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_Serial.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_Serial.BackColor = System.Drawing.Color.Transparent;
            this.txt_Serial.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_Serial.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_Serial.Location = new System.Drawing.Point(24, 124);
            this.txt_Serial.MaxLength = 32767;
            this.txt_Serial.Name = "txt_Serial";
            this.txt_Serial.PasswordChar = '\0';
            this.txt_Serial.Placeholder = "";
            this.txt_Serial.Radius = 10;
            this.txt_Serial.ReadOnly = true;
            this.txt_Serial.Size = new System.Drawing.Size(218, 36);
            this.txt_Serial.TabIndex = 7;
            this.txt_Serial.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_Serial.UseSystemPasswordChar = false;
            // lblLMachineNo
            this.lblLMachineNo.AutoSize = true;
            this.lblLMachineNo.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLMachineNo.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLMachineNo.Location = new System.Drawing.Point(259, 108);
            this.lblLMachineNo.Name = "lblLMachineNo";
            this.lblLMachineNo.TabIndex = 8;
            this.lblLMachineNo.Text = "Machine no *";
            // cmb_machine_no
            this.cmb_machine_no.BackColor = System.Drawing.Color.Transparent;
            this.cmb_machine_no.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.cmb_machine_no.DataSource = null;
            this.cmb_machine_no.DisplayMember = "";
            this.cmb_machine_no.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.cmb_machine_no.Location = new System.Drawing.Point(259, 124);
            this.cmb_machine_no.Name = "cmb_machine_no";
            this.cmb_machine_no.Radius = 10;
            this.cmb_machine_no.SelectedIndex = -1;
            this.cmb_machine_no.SelectedItem = null;
            this.cmb_machine_no.Size = new System.Drawing.Size(218, 36);
            this.cmb_machine_no.TabIndex = 9;
            this.cmb_machine_no.ValueMember = "";
            // lblLDepartment
            this.lblLDepartment.AutoSize = true;
            this.lblLDepartment.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLDepartment.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLDepartment.Location = new System.Drawing.Point(259, 40);
            this.lblLDepartment.Name = "lblLDepartment";
            this.lblLDepartment.TabIndex = 10;
            this.lblLDepartment.Text = "Department";
            // txt_department
            this.txt_department.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_department.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_department.BackColor = System.Drawing.Color.Transparent;
            this.txt_department.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_department.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_department.Location = new System.Drawing.Point(259, 58);
            this.txt_department.MaxLength = 32767;
            this.txt_department.Name = "txt_department";
            this.txt_department.PasswordChar = '\0';
            this.txt_department.Placeholder = "";
            this.txt_department.Radius = 10;
            this.txt_department.ReadOnly = false;
            this.txt_department.Size = new System.Drawing.Size(218, 36);
            this.txt_department.TabIndex = 11;
            this.txt_department.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_department.UseSystemPasswordChar = false;
            // lblLLocation
            this.lblLLocation.AutoSize = true;
            this.lblLLocation.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLLocation.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLLocation.Location = new System.Drawing.Point(728, 40);
            this.lblLLocation.Name = "lblLLocation";
            this.lblLLocation.TabIndex = 12;
            this.lblLLocation.Text = "Location";
            // txt_location
            this.txt_location.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_location.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_location.BackColor = System.Drawing.Color.Transparent;
            this.txt_location.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_location.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_location.Location = new System.Drawing.Point(725, 58);
            this.txt_location.MaxLength = 32767;
            this.txt_location.Name = "txt_location";
            this.txt_location.PasswordChar = '\0';
            this.txt_location.Placeholder = "";
            this.txt_location.Radius = 10;
            this.txt_location.ReadOnly = false;
            this.txt_location.Size = new System.Drawing.Size(218, 36);
            this.txt_location.TabIndex = 13;
            this.txt_location.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_location.UseSystemPasswordChar = false;
            // chk_isstaff
            this.chk_isstaff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chk_isstaff.Location = new System.Drawing.Point(24, 171);
            this.chk_isstaff.Name = "chk_isstaff";
            this.chk_isstaff.OffColor = System.Drawing.Color.FromArgb(200, 204, 216);
            this.chk_isstaff.OnColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.chk_isstaff.Size = new System.Drawing.Size(44, 24);
            this.chk_isstaff.TabIndex = 14;
            // lblStaffTitle
            this.lblStaffTitle.AutoSize = true;
            this.lblStaffTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStaffTitle.ForeColor = System.Drawing.Color.FromArgb(30, 38, 90);
            this.lblStaffTitle.Location = new System.Drawing.Point(76, 171);
            this.lblStaffTitle.Name = "lblStaffTitle";
            this.lblStaffTitle.TabIndex = 15;
            this.lblStaffTitle.Text = "Staff device";
            // lblStaffDesc
            this.lblStaffDesc.AutoSize = true;
            this.lblStaffDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStaffDesc.ForeColor = System.Drawing.Color.FromArgb(130, 138, 165);
            this.lblStaffDesc.Location = new System.Drawing.Point(76, 189);
            this.lblStaffDesc.Name = "lblStaffDesc";
            this.lblStaffDesc.TabIndex = 16;
            this.lblStaffDesc.Text = "Opens doors for staff only, not for members.";
            // lblSectionConnection
            this.lblSectionConnection.AutoSize = true;
            this.lblSectionConnection.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSectionConnection.ForeColor = System.Drawing.Color.FromArgb(30, 38, 90);
            this.lblSectionConnection.Location = new System.Drawing.Point(24, 220);
            this.lblSectionConnection.Name = "lblSectionConnection";
            this.lblSectionConnection.TabIndex = 17;
            this.lblSectionConnection.Text = "Connection";
            // pnlSepConnection
            this.pnlSepConnection.BackColor = System.Drawing.Color.FromArgb(220, 224, 238);
            this.pnlSepConnection.Location = new System.Drawing.Point(24, 238);
            this.pnlSepConnection.Name = "pnlSepConnection";
            this.pnlSepConnection.Size = new System.Drawing.Size(912, 1);
            this.pnlSepConnection.TabIndex = 18;
            // lblLDeviceIP
            this.lblLDeviceIP.AutoSize = true;
            this.lblLDeviceIP.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLDeviceIP.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLDeviceIP.Location = new System.Drawing.Point(24, 245);
            this.lblLDeviceIP.Name = "lblLDeviceIP";
            this.lblLDeviceIP.TabIndex = 19;
            this.lblLDeviceIP.Text = "Device IP *";
            // txt_ip
            this.txt_ip.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_ip.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_ip.BackColor = System.Drawing.Color.Transparent;
            this.txt_ip.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_ip.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_ip.Location = new System.Drawing.Point(24, 261);
            this.txt_ip.MaxLength = 32767;
            this.txt_ip.Name = "txt_ip";
            this.txt_ip.PasswordChar = '\0';
            this.txt_ip.Placeholder = "";
            this.txt_ip.Radius = 10;
            this.txt_ip.ReadOnly = false;
            this.txt_ip.Size = new System.Drawing.Size(218, 36);
            this.txt_ip.TabIndex = 20;
            this.txt_ip.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_ip.UseSystemPasswordChar = false;
            // lblLPort
            this.lblLPort.AutoSize = true;
            this.lblLPort.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLPort.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLPort.Location = new System.Drawing.Point(256, 245);
            this.lblLPort.Name = "lblLPort";
            this.lblLPort.TabIndex = 21;
            this.lblLPort.Text = "Port *";
            // txt_port
            this.txt_port.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_port.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_port.BackColor = System.Drawing.Color.Transparent;
            this.txt_port.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_port.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_port.Location = new System.Drawing.Point(259, 263);
            this.txt_port.MaxLength = 32767;
            this.txt_port.Name = "txt_port";
            this.txt_port.PasswordChar = '\0';
            this.txt_port.Placeholder = "";
            this.txt_port.Radius = 10;
            this.txt_port.ReadOnly = false;
            this.txt_port.Size = new System.Drawing.Size(218, 36);
            this.txt_port.TabIndex = 22;
            this.txt_port.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_port.UseSystemPasswordChar = false;
            // lblLPassword
            this.lblLPassword.AutoSize = true;
            this.lblLPassword.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLPassword.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLPassword.Location = new System.Drawing.Point(500, 245);
            this.lblLPassword.Name = "lblLPassword";
            this.lblLPassword.TabIndex = 23;
            this.lblLPassword.Text = "Password";
            // txt_password
            this.txt_password.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_password.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_password.BackColor = System.Drawing.Color.Transparent;
            this.txt_password.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_password.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_password.Location = new System.Drawing.Point(497, 263);
            this.txt_password.MaxLength = 32767;
            this.txt_password.Name = "txt_password";
            this.txt_password.PasswordChar = '\0';
            this.txt_password.Placeholder = "";
            this.txt_password.Radius = 10;
            this.txt_password.ReadOnly = false;
            this.txt_password.Size = new System.Drawing.Size(218, 36);
            this.txt_password.TabIndex = 24;
            this.txt_password.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_password.UseSystemPasswordChar = false;
            // lblLTimeZone
            this.lblLTimeZone.AutoSize = true;
            this.lblLTimeZone.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLTimeZone.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLTimeZone.Location = new System.Drawing.Point(730, 245);
            this.lblLTimeZone.Name = "lblLTimeZone";
            this.lblLTimeZone.TabIndex = 25;
            this.lblLTimeZone.Text = "Time zone";
            // cmb_timeZone
            this.cmb_timeZone.BackColor = System.Drawing.Color.Transparent;
            this.cmb_timeZone.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.cmb_timeZone.DataSource = null;
            this.cmb_timeZone.DisplayMember = "";
            this.cmb_timeZone.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.cmb_timeZone.Location = new System.Drawing.Point(730, 263);
            this.cmb_timeZone.Name = "cmb_timeZone";
            this.cmb_timeZone.Radius = 10;
            this.cmb_timeZone.SelectedIndex = -1;
            this.cmb_timeZone.SelectedItem = null;
            this.cmb_timeZone.Size = new System.Drawing.Size(218, 36);
            this.cmb_timeZone.TabIndex = 26;
            this.cmb_timeZone.ValueMember = "";
            // lblSectionCapacity
            this.lblSectionCapacity.AutoSize = true;
            this.lblSectionCapacity.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSectionCapacity.ForeColor = System.Drawing.Color.FromArgb(30, 38, 90);
            this.lblSectionCapacity.Location = new System.Drawing.Point(24, 316);
            this.lblSectionCapacity.Name = "lblSectionCapacity";
            this.lblSectionCapacity.TabIndex = 27;
            this.lblSectionCapacity.Text = "Capacity";
            // pnlSepCapacity
            this.pnlSepCapacity.BackColor = System.Drawing.Color.FromArgb(220, 224, 238);
            this.pnlSepCapacity.Location = new System.Drawing.Point(24, 334);
            this.pnlSepCapacity.Name = "pnlSepCapacity";
            this.pnlSepCapacity.Size = new System.Drawing.Size(912, 1);
            this.pnlSepCapacity.TabIndex = 28;
            // lblLFaceCapacity
            this.lblLFaceCapacity.AutoSize = true;
            this.lblLFaceCapacity.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLFaceCapacity.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLFaceCapacity.Location = new System.Drawing.Point(24, 338);
            this.lblLFaceCapacity.Name = "lblLFaceCapacity";
            this.lblLFaceCapacity.TabIndex = 29;
            this.lblLFaceCapacity.Text = "Face capacity";
            // txt_face_capacity
            this.txt_face_capacity.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_face_capacity.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_face_capacity.BackColor = System.Drawing.Color.Transparent;
            this.txt_face_capacity.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_face_capacity.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_face_capacity.Location = new System.Drawing.Point(24, 354);
            this.txt_face_capacity.MaxLength = 32767;
            this.txt_face_capacity.Name = "txt_face_capacity";
            this.txt_face_capacity.PasswordChar = '\0';
            this.txt_face_capacity.Placeholder = "";
            this.txt_face_capacity.Radius = 10;
            this.txt_face_capacity.ReadOnly = false;
            this.txt_face_capacity.Size = new System.Drawing.Size(280, 36);
            this.txt_face_capacity.TabIndex = 30;
            this.txt_face_capacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_face_capacity.UseSystemPasswordChar = false;
            // lblLRFIDCapacity
            this.lblLRFIDCapacity.AutoSize = true;
            this.lblLRFIDCapacity.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLRFIDCapacity.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLRFIDCapacity.Location = new System.Drawing.Point(328, 338);
            this.lblLRFIDCapacity.Name = "lblLRFIDCapacity";
            this.lblLRFIDCapacity.TabIndex = 31;
            this.lblLRFIDCapacity.Text = "RFID capacity";
            // txt_RFID_capacity
            this.txt_RFID_capacity.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_RFID_capacity.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_RFID_capacity.BackColor = System.Drawing.Color.Transparent;
            this.txt_RFID_capacity.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_RFID_capacity.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_RFID_capacity.Location = new System.Drawing.Point(328, 354);
            this.txt_RFID_capacity.MaxLength = 32767;
            this.txt_RFID_capacity.Name = "txt_RFID_capacity";
            this.txt_RFID_capacity.PasswordChar = '\0';
            this.txt_RFID_capacity.Placeholder = "";
            this.txt_RFID_capacity.Radius = 10;
            this.txt_RFID_capacity.ReadOnly = false;
            this.txt_RFID_capacity.Size = new System.Drawing.Size(280, 36);
            this.txt_RFID_capacity.TabIndex = 32;
            this.txt_RFID_capacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_RFID_capacity.UseSystemPasswordChar = false;
            // lblLFingerCapacity
            this.lblLFingerCapacity.AutoSize = true;
            this.lblLFingerCapacity.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLFingerCapacity.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lblLFingerCapacity.Location = new System.Drawing.Point(632, 338);
            this.lblLFingerCapacity.Name = "lblLFingerCapacity";
            this.lblLFingerCapacity.TabIndex = 33;
            this.lblLFingerCapacity.Text = "Finger capacity";
            // txt_finger_capacity
            this.txt_finger_capacity.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_finger_capacity.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_finger_capacity.BackColor = System.Drawing.Color.Transparent;
            this.txt_finger_capacity.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.txt_finger_capacity.FocusBorderColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.txt_finger_capacity.Location = new System.Drawing.Point(632, 354);
            this.txt_finger_capacity.MaxLength = 32767;
            this.txt_finger_capacity.Name = "txt_finger_capacity";
            this.txt_finger_capacity.PasswordChar = '\0';
            this.txt_finger_capacity.Placeholder = "";
            this.txt_finger_capacity.Radius = 10;
            this.txt_finger_capacity.ReadOnly = false;
            this.txt_finger_capacity.Size = new System.Drawing.Size(280, 36);
            this.txt_finger_capacity.TabIndex = 34;
            this.txt_finger_capacity.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_finger_capacity.UseSystemPasswordChar = false;
            // pnlFooter
            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Controls.Add(this.btn_testConnection);
            this.pnlFooter.Controls.Add(this.lbl_saving_status);
            this.pnlFooter.Controls.Add(this.btnCancle);
            this.pnlFooter.Controls.Add(this.btnSave);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 480);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(960, 65);
            this.pnlFooter.TabIndex = 2;
            this.pnlFooter.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlFooter_Paint);
            // btn_testConnection
            this.btn_testConnection.BackColor = System.Drawing.Color.Transparent;
            this.btn_testConnection.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.btn_testConnection.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_testConnection.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btn_testConnection.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btn_testConnection.ForeColor = System.Drawing.Color.FromArgb(30, 38, 90);
            this.btn_testConnection.HoverColor = System.Drawing.Color.FromArgb(220, 224, 235);
            this.btn_testConnection.Location = new System.Drawing.Point(24, 16);
            this.btn_testConnection.Name = "btn_testConnection";
            this.btn_testConnection.NormalColor = System.Drawing.Color.FromArgb(240, 242, 248);
            this.btn_testConnection.PressedColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btn_testConnection.Radius = 10;
            this.btn_testConnection.Size = new System.Drawing.Size(160, 32);
            this.btn_testConnection.TabIndex = 0;
            this.btn_testConnection.Text = "Test connection";
            this.btn_testConnection.Click += new System.EventHandler(this.btn_testConnection_Click);
            // lbl_saving_status
            this.lbl_saving_status.AutoSize = true;
            this.lbl_saving_status.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_saving_status.ForeColor = System.Drawing.Color.FromArgb(130, 138, 165);
            this.lbl_saving_status.Location = new System.Drawing.Point(196, 24);
            this.lbl_saving_status.Name = "lbl_saving_status";
            this.lbl_saving_status.TabIndex = 1;
            this.lbl_saving_status.Text = "Ready";
            // btnCancle
            this.btnCancle.BackColor = System.Drawing.Color.Transparent;
            this.btnCancle.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.btnCancle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancle.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnCancle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCancle.ForeColor = System.Drawing.Color.FromArgb(30, 38, 90);
            this.btnCancle.HoverColor = System.Drawing.Color.FromArgb(220, 224, 235);
            this.btnCancle.Location = new System.Drawing.Point(690, 16);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.NormalColor = System.Drawing.Color.FromArgb(240, 242, 248);
            this.btnCancle.PressedColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnCancle.Radius = 10;
            this.btnCancle.Size = new System.Drawing.Size(110, 32);
            this.btnCancle.TabIndex = 2;
            this.btnCancle.Text = "Cancel";
            this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
            // btnSave
            this.btnSave.BackColor = System.Drawing.Color.Transparent;
            this.btnSave.BorderColor = System.Drawing.Color.Transparent;
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.HoverColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnSave.Location = new System.Drawing.Point(812, 16);
            this.btnSave.Name = "btnSave";
            this.btnSave.NormalColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.btnSave.PressedColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnSave.Radius = 10;
            this.btnSave.Size = new System.Drawing.Size(120, 32);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save Changes";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // EditDevice
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.ClientSize = new System.Drawing.Size(960, 545);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "EditDevice";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edit Device";
            this.Load += new System.EventHandler(this.EditDevice_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Label lblSectionDevice;
        private System.Windows.Forms.Panel pnlSepDevice;
        private System.Windows.Forms.Label lblLDeviceType;
        private smart_attendance_middleware.Controls.RoundedComboBox cmb_device_type;
        private System.Windows.Forms.Label lblLConnectionType;
        private smart_attendance_middleware.Controls.RoundedComboBox cmb_connectionType;
        private System.Windows.Forms.Label lblLSerial;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_Serial;
        private System.Windows.Forms.Label lblLMachineNo;
        private smart_attendance_middleware.Controls.RoundedComboBox cmb_machine_no;
        private System.Windows.Forms.Label lblLDepartment;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_department;
        private System.Windows.Forms.Label lblLLocation;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_location;
        private smart_attendance_middleware.Controls.ToggleSwitch chk_isstaff;
        private System.Windows.Forms.Label lblStaffTitle;
        private System.Windows.Forms.Label lblStaffDesc;
        private System.Windows.Forms.Label lblSectionConnection;
        private System.Windows.Forms.Panel pnlSepConnection;
        private System.Windows.Forms.Label lblLDeviceIP;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_ip;
        private System.Windows.Forms.Label lblLPort;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_port;
        private System.Windows.Forms.Label lblLPassword;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_password;
        private System.Windows.Forms.Label lblLTimeZone;
        private smart_attendance_middleware.Controls.RoundedComboBox cmb_timeZone;
        private System.Windows.Forms.Label lblSectionCapacity;
        private System.Windows.Forms.Panel pnlSepCapacity;
        private System.Windows.Forms.Label lblLFaceCapacity;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_face_capacity;
        private System.Windows.Forms.Label lblLRFIDCapacity;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_RFID_capacity;
        private System.Windows.Forms.Label lblLFingerCapacity;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_finger_capacity;
        private System.Windows.Forms.Panel pnlFooter;
        private smart_attendance_middleware.Controls.RoundedButton btn_testConnection;
        private System.Windows.Forms.Label lbl_saving_status;
        private smart_attendance_middleware.Controls.RoundedButton btnCancle;
        private smart_attendance_middleware.Controls.RoundedButton btnSave;
    }
}
