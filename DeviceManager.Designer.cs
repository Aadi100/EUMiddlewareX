using System.Windows.Forms;

namespace smart_attendance_middleware
{
    partial class DeviceManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_add_device = new System.Windows.Forms.Button();
            this.pnl_deviceManager = new System.Windows.Forms.Panel();
            this.pnl_device = new System.Windows.Forms.Panel();
            this.btn_format = new System.Windows.Forms.Button();
            this.txt_password = new System.Windows.Forms.TextBox();
            this.lbl_password = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmb_machine_no = new System.Windows.Forms.ComboBox();
            this.listViewDevices = new System.Windows.Forms.ListView();
            this.btnCancle = new System.Windows.Forms.Button();
            this.cmb_connectionType = new System.Windows.Forms.ComboBox();
            this.cmb_device_type = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.txt_finger_capacity = new System.Windows.Forms.TextBox();
            this.txt_RFID_capacity = new System.Windows.Forms.TextBox();
            this.btn_remove_device = new System.Windows.Forms.Button();
            this.txt_face_capacity = new System.Windows.Forms.TextBox();
            this.txt_last_synced = new System.Windows.Forms.TextBox();
            this.txt_device_status = new System.Windows.Forms.TextBox();
            this.btnEdit = new System.Windows.Forms.Button();
            this.txt_location = new System.Windows.Forms.TextBox();
            this.txt_timeZone = new System.Windows.Forms.TextBox();
            this.txt_port = new System.Windows.Forms.TextBox();
            this.txt_ip = new System.Windows.Forms.TextBox();
            this.txt_Serial = new System.Windows.Forms.TextBox();
            this.txt_department = new System.Windows.Forms.TextBox();
            this.txt_Device_id = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnl_staffhandling = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label41 = new System.Windows.Forms.Label();
            this.txt_staffcardid = new System.Windows.Forms.TextBox();
            this.btn_deletestff = new System.Windows.Forms.Button();
            this.lst_stff = new System.Windows.Forms.ListView();
            this.btn_namestff = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btn_stffenroll = new System.Windows.Forms.Button();
            this.btn_image = new System.Windows.Forms.Button();
            this.txt_stffname = new System.Windows.Forms.TextBox();
            this.txt_stffid = new System.Windows.Forms.TextBox();
            this.label51 = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_setupmanage = new System.Windows.Forms.Label();
            this.btn_attendencemanagment = new System.Windows.Forms.Label();
            this.btn_staffmanagment = new System.Windows.Forms.Label();
            this.btn_userManagement = new System.Windows.Forms.Label();
            this.btn_apiKey = new System.Windows.Forms.Label();
            this.btn_device_management = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnl_APIkey = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.lbl_apikey_status = new System.Windows.Forms.Label();
            this.btnApiPaste = new System.Windows.Forms.Label();
            this.txtAPIkey = new System.Windows.Forms.TextBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnSaveAPI = new System.Windows.Forms.Button();
            this.label33 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.pnl_UserManagement = new System.Windows.Forms.Panel();
            this.btn_addUser = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btn_editUser = new System.Windows.Forms.Button();
            this.list_LocalUser = new System.Windows.Forms.ListView();
            this.btn_change_password = new System.Windows.Forms.Button();
            this.btn_deleteUser = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.pnl_attendence = new System.Windows.Forms.Panel();
            this.tab_attendence = new System.Windows.Forms.TabControl();
            this.tab_members = new System.Windows.Forms.TabPage();
            this.txt_memb_name = new System.Windows.Forms.TextBox();
            this.flt_members = new System.Windows.Forms.Button();
            this.lst_members = new System.Windows.Forms.ListView();
            this.tab_memberAttendence = new System.Windows.Forms.TabPage();
            this.panel5 = new System.Windows.Forms.Panel();
            this.txt_membername = new System.Windows.Forms.TextBox();
            this.date_member = new System.Windows.Forms.DateTimePicker();
            this.btn_filter_member = new System.Windows.Forms.Button();
            this.lst_member = new System.Windows.Forms.ListView();
            this.tab_manualdoor = new System.Windows.Forms.TabPage();
            this.panel6 = new System.Windows.Forms.Panel();
            this.date_manualdoor = new System.Windows.Forms.DateTimePicker();
            this.btn_filter_manualdoor = new System.Windows.Forms.Button();
            this.lst_manualdoor = new System.Windows.Forms.ListView();
            this.label48 = new System.Windows.Forms.Label();
            this.pnl_setup = new System.Windows.Forms.Panel();
            this.subpnl_setup = new System.Windows.Forms.Panel();
            this.btn_attendence_sim = new System.Windows.Forms.Button();
            this.btn_settime = new System.Windows.Forms.Button();
            this.btn_disableuser = new System.Windows.Forms.Button();
            this.btn_enableuser = new System.Windows.Forms.Button();
            this.btn_getdb = new System.Windows.Forms.Button();
            this.btn_getinfo = new System.Windows.Forms.Button();
            this.txt_enrollid = new System.Windows.Forms.TextBox();
            this.num_debounce = new System.Windows.Forms.NumericUpDown();
            this.lbl_debounce = new System.Windows.Forms.Label();
            this.btn_savedebounce = new System.Windows.Forms.Button();
            this.label42 = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.pnl_staffsManagment = new System.Windows.Forms.Panel();
            this.tab_staff = new System.Windows.Forms.TabControl();
            this.tab_staffmembers = new System.Windows.Forms.TabPage();
            this.txt_staff = new System.Windows.Forms.TextBox();
            this.btn_stafffilter = new System.Windows.Forms.Button();
            this.lst_staffs = new System.Windows.Forms.ListView();
            this.tab_staffattendence = new System.Windows.Forms.TabPage();
            this.panel8 = new System.Windows.Forms.Panel();
            this.txt_staffname = new System.Windows.Forms.TextBox();
            this.date_staffattendence = new System.Windows.Forms.DateTimePicker();
            this.btn_filterattendencestaff = new System.Windows.Forms.Button();
            this.lst_staffattendence = new System.Windows.Forms.ListView();
            this.lbl_staffmanagment = new System.Windows.Forms.Label();
            this.pnl_deviceManager.SuspendLayout();
            this.pnl_device.SuspendLayout();
            this.pnl_staffhandling.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnl_APIkey.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.pnl_UserManagement.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pnl_attendence.SuspendLayout();
            this.tab_attendence.SuspendLayout();
            this.tab_members.SuspendLayout();
            this.tab_memberAttendence.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tab_manualdoor.SuspendLayout();
            this.panel6.SuspendLayout();
            this.pnl_setup.SuspendLayout();
            this.subpnl_setup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_debounce)).BeginInit();
            this.pnl_staffsManagment.SuspendLayout();
            this.tab_staff.SuspendLayout();
            this.tab_staffmembers.SuspendLayout();
            this.tab_staffattendence.SuspendLayout();
            this.panel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_add_device
            // 
            this.btn_add_device.Location = new System.Drawing.Point(576, 58);
            this.btn_add_device.Name = "btn_add_device";
            this.btn_add_device.Size = new System.Drawing.Size(105, 32);
            this.btn_add_device.TabIndex = 3;
            this.btn_add_device.Text = "Add New Device";
            this.btn_add_device.UseVisualStyleBackColor = true;
            this.btn_add_device.Click += new System.EventHandler(this.btn_add_device_Click);
            // 
            // pnl_deviceManager
            // 
            this.pnl_deviceManager.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_deviceManager.Controls.Add(this.btn_add_device);
            this.pnl_deviceManager.Controls.Add(this.pnl_device);
            this.pnl_deviceManager.Controls.Add(this.label2);
            this.pnl_deviceManager.Controls.Add(this.label1);
            this.pnl_deviceManager.Location = new System.Drawing.Point(186, 0);
            this.pnl_deviceManager.Name = "pnl_deviceManager";
            this.pnl_deviceManager.Size = new System.Drawing.Size(700, 576);
            this.pnl_deviceManager.TabIndex = 0;
            this.pnl_deviceManager.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // pnl_device
            // 
            this.pnl_device.BackColor = System.Drawing.Color.White;
            this.pnl_device.Controls.Add(this.btn_format);
            this.pnl_device.Controls.Add(this.txt_password);
            this.pnl_device.Controls.Add(this.lbl_password);
            this.pnl_device.Controls.Add(this.label4);
            this.pnl_device.Controls.Add(this.cmb_machine_no);
            this.pnl_device.Controls.Add(this.listViewDevices);
            this.pnl_device.Controls.Add(this.btnCancle);
            this.pnl_device.Controls.Add(this.cmb_connectionType);
            this.pnl_device.Controls.Add(this.cmb_device_type);
            this.pnl_device.Controls.Add(this.btnSave);
            this.pnl_device.Controls.Add(this.txt_finger_capacity);
            this.pnl_device.Controls.Add(this.txt_RFID_capacity);
            this.pnl_device.Controls.Add(this.btn_remove_device);
            this.pnl_device.Controls.Add(this.txt_face_capacity);
            this.pnl_device.Controls.Add(this.txt_last_synced);
            this.pnl_device.Controls.Add(this.txt_device_status);
            this.pnl_device.Controls.Add(this.btnEdit);
            this.pnl_device.Controls.Add(this.txt_location);
            this.pnl_device.Controls.Add(this.txt_timeZone);
            this.pnl_device.Controls.Add(this.txt_port);
            this.pnl_device.Controls.Add(this.txt_ip);
            this.pnl_device.Controls.Add(this.txt_Serial);
            this.pnl_device.Controls.Add(this.txt_department);
            this.pnl_device.Controls.Add(this.txt_Device_id);
            this.pnl_device.Controls.Add(this.label17);
            this.pnl_device.Controls.Add(this.label18);
            this.pnl_device.Controls.Add(this.label19);
            this.pnl_device.Controls.Add(this.label20);
            this.pnl_device.Controls.Add(this.label21);
            this.pnl_device.Controls.Add(this.label22);
            this.pnl_device.Controls.Add(this.label23);
            this.pnl_device.Controls.Add(this.label16);
            this.pnl_device.Controls.Add(this.label15);
            this.pnl_device.Controls.Add(this.label14);
            this.pnl_device.Controls.Add(this.label13);
            this.pnl_device.Controls.Add(this.label12);
            this.pnl_device.Controls.Add(this.label11);
            this.pnl_device.Controls.Add(this.label10);
            this.pnl_device.Controls.Add(this.label9);
            this.pnl_device.Location = new System.Drawing.Point(22, 101);
            this.pnl_device.Name = "pnl_device";
            this.pnl_device.Size = new System.Drawing.Size(660, 424);
            this.pnl_device.TabIndex = 2;
            this.pnl_device.Paint += new System.Windows.Forms.PaintEventHandler(this.pnl_device_Paint);
            // 
            // btn_format
            // 
            this.btn_format.Location = new System.Drawing.Point(288, 376);
            this.btn_format.Name = "btn_format";
            this.btn_format.Size = new System.Drawing.Size(113, 32);
            this.btn_format.TabIndex = 51;
            this.btn_format.Text = "Format";
            this.btn_format.UseVisualStyleBackColor = true;
            this.btn_format.Visible = false;
            this.btn_format.Click += new System.EventHandler(this.btn_format_Click);
            // 
            // txt_password
            // 
            this.txt_password.BackColor = System.Drawing.Color.White;
            this.txt_password.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_password.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_password.Location = new System.Drawing.Point(122, 359);
            this.txt_password.Name = "txt_password";
            this.txt_password.PasswordChar = '*';
            this.txt_password.Size = new System.Drawing.Size(72, 16);
            this.txt_password.TabIndex = 50;
            this.txt_password.TextChanged += new System.EventHandler(this.txt_password_TextChanged);
            // 
            // lbl_password
            // 
            this.lbl_password.AutoSize = true;
            this.lbl_password.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_password.Location = new System.Drawing.Point(11, 358);
            this.lbl_password.Name = "lbl_password";
            this.lbl_password.Size = new System.Drawing.Size(69, 17);
            this.lbl_password.TabIndex = 49;
            this.lbl_password.Text = "Password";
            this.lbl_password.Click += new System.EventHandler(this.lbl_password_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(382, 125);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 17);
            this.label4.TabIndex = 43;
            this.label4.Text = "Machine No";
            // 
            // cmb_machine_no
            // 
            this.cmb_machine_no.BackColor = System.Drawing.Color.White;
            this.cmb_machine_no.FormattingEnabled = true;
            this.cmb_machine_no.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmb_machine_no.Location = new System.Drawing.Point(508, 124);
            this.cmb_machine_no.Name = "cmb_machine_no";
            this.cmb_machine_no.Size = new System.Drawing.Size(135, 21);
            this.cmb_machine_no.TabIndex = 42;
            this.cmb_machine_no.SelectedIndexChanged += new System.EventHandler(this.cmb_machine_no_SelectedIndexChanged);
            // 
            // listViewDevices
            // 
            this.listViewDevices.HideSelection = false;
            this.listViewDevices.Location = new System.Drawing.Point(0, 0);
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.Size = new System.Drawing.Size(660, 99);
            this.listViewDevices.TabIndex = 41;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
            this.listViewDevices.SelectedIndexChanged += new System.EventHandler(this.listViewDevices_SelectedIndexChanged_1);
            // 
            // btnCancle
            // 
            this.btnCancle.Location = new System.Drawing.Point(169, 376);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(113, 32);
            this.btnCancle.TabIndex = 40;
            this.btnCancle.Text = "Cancel";
            this.btnCancle.UseVisualStyleBackColor = true;
            this.btnCancle.Visible = false;
            this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
            // 
            // cmb_connectionType
            // 
            this.cmb_connectionType.BackColor = System.Drawing.Color.White;
            this.cmb_connectionType.FormattingEnabled = true;
            this.cmb_connectionType.Items.AddRange(new object[] {
            "NET",
            "LAN",
            "USB"});
            this.cmb_connectionType.Location = new System.Drawing.Point(508, 178);
            this.cmb_connectionType.Name = "cmb_connectionType";
            this.cmb_connectionType.Size = new System.Drawing.Size(135, 21);
            this.cmb_connectionType.TabIndex = 37;
            this.cmb_connectionType.SelectedIndexChanged += new System.EventHandler(this.cmb_connectionType_SelectedIndexChanged);
            // 
            // cmb_device_type
            // 
            this.cmb_device_type.FormattingEnabled = true;
            this.cmb_device_type.Items.AddRange(new object[] {
            "Check-in",
            "Check-out",
            "Check-in/Check-out"});
            this.cmb_device_type.Location = new System.Drawing.Point(123, 212);
            this.cmb_device_type.Name = "cmb_device_type";
            this.cmb_device_type.Size = new System.Drawing.Size(135, 21);
            this.cmb_device_type.TabIndex = 36;
            this.cmb_device_type.SelectedIndexChanged += new System.EventHandler(this.cmb_device_type_SelectedIndexChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(288, 376);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(113, 32);
            this.btnSave.TabIndex = 39;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Visible = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txt_finger_capacity
            // 
            this.txt_finger_capacity.BackColor = System.Drawing.Color.White;
            this.txt_finger_capacity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_finger_capacity.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_finger_capacity.Location = new System.Drawing.Point(506, 336);
            this.txt_finger_capacity.Name = "txt_finger_capacity";
            this.txt_finger_capacity.Size = new System.Drawing.Size(136, 16);
            this.txt_finger_capacity.TabIndex = 35;
            this.txt_finger_capacity.TextChanged += new System.EventHandler(this.txt_finger_capacity_TextChanged);
            // 
            // txt_RFID_capacity
            // 
            this.txt_RFID_capacity.BackColor = System.Drawing.Color.White;
            this.txt_RFID_capacity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_RFID_capacity.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_RFID_capacity.Location = new System.Drawing.Point(506, 306);
            this.txt_RFID_capacity.Name = "txt_RFID_capacity";
            this.txt_RFID_capacity.Size = new System.Drawing.Size(136, 16);
            this.txt_RFID_capacity.TabIndex = 34;
            this.txt_RFID_capacity.TextChanged += new System.EventHandler(this.txt_RFID_capacity_TextChanged);
            // 
            // btn_remove_device
            // 
            this.btn_remove_device.Location = new System.Drawing.Point(407, 376);
            this.btn_remove_device.Name = "btn_remove_device";
            this.btn_remove_device.Size = new System.Drawing.Size(113, 32);
            this.btn_remove_device.TabIndex = 38;
            this.btn_remove_device.Text = "Remove Device";
            this.btn_remove_device.UseVisualStyleBackColor = true;
            this.btn_remove_device.Click += new System.EventHandler(this.btn_remove_device_Click);
            // 
            // txt_face_capacity
            // 
            this.txt_face_capacity.BackColor = System.Drawing.Color.White;
            this.txt_face_capacity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_face_capacity.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_face_capacity.Location = new System.Drawing.Point(506, 276);
            this.txt_face_capacity.Name = "txt_face_capacity";
            this.txt_face_capacity.Size = new System.Drawing.Size(136, 16);
            this.txt_face_capacity.TabIndex = 33;
            this.txt_face_capacity.TextChanged += new System.EventHandler(this.txt_face_capacity_TextChanged);
            // 
            // txt_last_synced
            // 
            this.txt_last_synced.BackColor = System.Drawing.Color.White;
            this.txt_last_synced.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_last_synced.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_last_synced.Location = new System.Drawing.Point(506, 246);
            this.txt_last_synced.Name = "txt_last_synced";
            this.txt_last_synced.Size = new System.Drawing.Size(136, 16);
            this.txt_last_synced.TabIndex = 32;
            this.txt_last_synced.TextChanged += new System.EventHandler(this.txt_last_synced_TextChanged);
            // 
            // txt_device_status
            // 
            this.txt_device_status.BackColor = System.Drawing.Color.White;
            this.txt_device_status.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_device_status.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_device_status.Location = new System.Drawing.Point(507, 216);
            this.txt_device_status.Name = "txt_device_status";
            this.txt_device_status.Size = new System.Drawing.Size(136, 16);
            this.txt_device_status.TabIndex = 31;
            this.txt_device_status.TextChanged += new System.EventHandler(this.txt_device_status_TextChanged);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(528, 376);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(113, 32);
            this.btnEdit.TabIndex = 5;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // txt_location
            // 
            this.txt_location.BackColor = System.Drawing.Color.White;
            this.txt_location.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_location.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_location.Location = new System.Drawing.Point(506, 151);
            this.txt_location.Name = "txt_location";
            this.txt_location.Size = new System.Drawing.Size(136, 16);
            this.txt_location.TabIndex = 29;
            this.txt_location.TextChanged += new System.EventHandler(this.txt_location_TextChanged);
            // 
            // txt_timeZone
            // 
            this.txt_timeZone.BackColor = System.Drawing.Color.White;
            this.txt_timeZone.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_timeZone.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_timeZone.Location = new System.Drawing.Point(122, 336);
            this.txt_timeZone.Name = "txt_timeZone";
            this.txt_timeZone.Size = new System.Drawing.Size(136, 16);
            this.txt_timeZone.TabIndex = 28;
            this.txt_timeZone.TextChanged += new System.EventHandler(this.txt_timeZone_TextChanged);
            // 
            // txt_port
            // 
            this.txt_port.BackColor = System.Drawing.Color.White;
            this.txt_port.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_port.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_port.Location = new System.Drawing.Point(122, 306);
            this.txt_port.Name = "txt_port";
            this.txt_port.Size = new System.Drawing.Size(136, 16);
            this.txt_port.TabIndex = 27;
            this.txt_port.TextChanged += new System.EventHandler(this.txt_port_TextChanged);
            // 
            // txt_ip
            // 
            this.txt_ip.BackColor = System.Drawing.Color.White;
            this.txt_ip.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_ip.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_ip.Location = new System.Drawing.Point(122, 276);
            this.txt_ip.Name = "txt_ip";
            this.txt_ip.Size = new System.Drawing.Size(136, 16);
            this.txt_ip.TabIndex = 26;
            this.txt_ip.TextChanged += new System.EventHandler(this.txt_ip_TextChanged);
            // 
            // txt_Serial
            // 
            this.txt_Serial.BackColor = System.Drawing.Color.White;
            this.txt_Serial.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Serial.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Serial.Location = new System.Drawing.Point(122, 246);
            this.txt_Serial.Name = "txt_Serial";
            this.txt_Serial.Size = new System.Drawing.Size(136, 16);
            this.txt_Serial.TabIndex = 25;
            this.txt_Serial.TextChanged += new System.EventHandler(this.txt_Serial_TextChanged);
            // 
            // txt_department
            // 
            this.txt_department.BackColor = System.Drawing.Color.White;
            this.txt_department.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_department.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_department.Location = new System.Drawing.Point(123, 182);
            this.txt_department.Name = "txt_department";
            this.txt_department.Size = new System.Drawing.Size(136, 16);
            this.txt_department.TabIndex = 23;
            this.txt_department.TextChanged += new System.EventHandler(this.txt_department_TextChanged);
            // 
            // txt_Device_id
            // 
            this.txt_Device_id.BackColor = System.Drawing.Color.White;
            this.txt_Device_id.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_Device_id.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_Device_id.Location = new System.Drawing.Point(123, 151);
            this.txt_Device_id.Name = "txt_Device_id";
            this.txt_Device_id.Size = new System.Drawing.Size(136, 16);
            this.txt_Device_id.TabIndex = 22;
            this.txt_Device_id.TextChanged += new System.EventHandler(this.txt_Device_id_TextChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(384, 332);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(106, 17);
            this.label17.TabIndex = 21;
            this.label17.Text = "Finger Capacity";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(384, 302);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(97, 17);
            this.label18.TabIndex = 20;
            this.label18.Text = "RFID Capacity";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(384, 272);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(97, 17);
            this.label19.TabIndex = 19;
            this.label19.Text = "Face Capacity";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label20.Location = new System.Drawing.Point(384, 242);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(86, 17);
            this.label20.TabIndex = 18;
            this.label20.Text = "Last Synced";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(384, 212);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(95, 17);
            this.label21.TabIndex = 17;
            this.label21.Text = "Device Status";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(383, 182);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(115, 17);
            this.label22.TabIndex = 16;
            this.label22.Text = "Connection Type";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(382, 152);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(62, 17);
            this.label23.TabIndex = 15;
            this.label23.Text = "Location";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(14, 332);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(76, 17);
            this.label16.TabIndex = 14;
            this.label16.Text = "Time Zone";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(14, 302);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(34, 17);
            this.label15.TabIndex = 13;
            this.label15.Text = "Port";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(14, 272);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(67, 17);
            this.label14.TabIndex = 12;
            this.label14.Text = "Device IP";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(14, 242);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(91, 17);
            this.label13.TabIndex = 11;
            this.label13.Text = "Device Serial";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(14, 212);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(87, 17);
            this.label12.TabIndex = 10;
            this.label12.Text = "Device Type";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(14, 182);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(82, 17);
            this.label11.TabIndex = 9;
            this.label11.Text = "Department";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(14, 152);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(68, 17);
            this.label10.TabIndex = 8;
            this.label10.Text = "Device ID";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(10, 111);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(128, 24);
            this.label9.TabIndex = 7;
            this.label9.Text = "Device Details";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.DimGray;
            this.label2.Location = new System.Drawing.Point(17, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(507, 43);
            this.label2.TabIndex = 1;
            this.label2.Text = "Manage your biometric devices, API keys, users, and attendance logs — all in one " +
    "place.";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label1.Location = new System.Drawing.Point(13, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Smart Access Manager";
            // 
            // pnl_staffhandling
            // 
            this.pnl_staffhandling.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_staffhandling.Controls.Add(this.panel1);
            this.pnl_staffhandling.Controls.Add(this.label51);
            this.pnl_staffhandling.Controls.Add(this.label52);
            this.pnl_staffhandling.Location = new System.Drawing.Point(892, 600);
            this.pnl_staffhandling.Name = "pnl_staffhandling";
            this.pnl_staffhandling.Size = new System.Drawing.Size(700, 576);
            this.pnl_staffhandling.TabIndex = 6;
            this.pnl_staffhandling.Visible = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label41);
            this.panel1.Controls.Add(this.txt_staffcardid);
            this.panel1.Controls.Add(this.btn_deletestff);
            this.panel1.Controls.Add(this.lst_stff);
            this.panel1.Controls.Add(this.btn_namestff);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.btn_stffenroll);
            this.panel1.Controls.Add(this.btn_image);
            this.panel1.Controls.Add(this.txt_stffname);
            this.panel1.Controls.Add(this.txt_stffid);
            this.panel1.Location = new System.Drawing.Point(6, 66);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(691, 503);
            this.panel1.TabIndex = 58;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label41.Location = new System.Drawing.Point(218, 14);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(53, 17);
            this.label41.TabIndex = 9;
            this.label41.Text = "Card Id";
            // 
            // txt_staffcardid
            // 
            this.txt_staffcardid.Location = new System.Drawing.Point(220, 35);
            this.txt_staffcardid.Name = "txt_staffcardid";
            this.txt_staffcardid.Size = new System.Drawing.Size(100, 20);
            this.txt_staffcardid.TabIndex = 8;
            // 
            // btn_deletestff
            // 
            this.btn_deletestff.Location = new System.Drawing.Point(492, 31);
            this.btn_deletestff.Name = "btn_deletestff";
            this.btn_deletestff.Size = new System.Drawing.Size(75, 23);
            this.btn_deletestff.TabIndex = 7;
            this.btn_deletestff.Text = "Delete";
            this.btn_deletestff.UseVisualStyleBackColor = true;
            this.btn_deletestff.Click += new System.EventHandler(this.btn_deletestff_Click);
            // 
            // lst_stff
            // 
            this.lst_stff.HideSelection = false;
            this.lst_stff.Location = new System.Drawing.Point(8, 79);
            this.lst_stff.Name = "lst_stff";
            this.lst_stff.Size = new System.Drawing.Size(680, 420);
            this.lst_stff.TabIndex = 6;
            this.lst_stff.UseCompatibleStateImageBehavior = false;
            // 
            // btn_namestff
            // 
            this.btn_namestff.AutoSize = true;
            this.btn_namestff.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_namestff.Location = new System.Drawing.Point(111, 14);
            this.btn_namestff.Name = "btn_namestff";
            this.btn_namestff.Size = new System.Drawing.Size(45, 17);
            this.btn_namestff.TabIndex = 5;
            this.btn_namestff.Text = "Name";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(5, 14);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 17);
            this.label7.TabIndex = 4;
            this.label7.Text = "ID";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // btn_stffenroll
            // 
            this.btn_stffenroll.Location = new System.Drawing.Point(411, 31);
            this.btn_stffenroll.Name = "btn_stffenroll";
            this.btn_stffenroll.Size = new System.Drawing.Size(75, 23);
            this.btn_stffenroll.TabIndex = 3;
            this.btn_stffenroll.Text = "Enroll";
            this.btn_stffenroll.UseVisualStyleBackColor = true;
            this.btn_stffenroll.Click += new System.EventHandler(this.btn_stffenroll_Click);
            // 
            // btn_image
            // 
            this.btn_image.Location = new System.Drawing.Point(330, 31);
            this.btn_image.Name = "btn_image";
            this.btn_image.Size = new System.Drawing.Size(75, 23);
            this.btn_image.TabIndex = 2;
            this.btn_image.Text = "Add Face";
            this.btn_image.UseVisualStyleBackColor = true;
            this.btn_image.Click += new System.EventHandler(this.btn_image_Click);
            // 
            // txt_stffname
            // 
            this.txt_stffname.Location = new System.Drawing.Point(114, 34);
            this.txt_stffname.Name = "txt_stffname";
            this.txt_stffname.Size = new System.Drawing.Size(100, 20);
            this.txt_stffname.TabIndex = 1;
            // 
            // txt_stffid
            // 
            this.txt_stffid.Location = new System.Drawing.Point(8, 34);
            this.txt_stffid.Name = "txt_stffid";
            this.txt_stffid.Size = new System.Drawing.Size(100, 20);
            this.txt_stffid.TabIndex = 0;
            // 
            // label51
            // 
            this.label51.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label51.ForeColor = System.Drawing.Color.DimGray;
            this.label51.Location = new System.Drawing.Point(3, 32);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(507, 43);
            this.label51.TabIndex = 57;
            this.label51.Text = "Add, update, or remove staff users.";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label52.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label52.Location = new System.Drawing.Point(3, 6);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(190, 26);
            this.label52.TabIndex = 56;
            this.label52.Text = "Staff Management";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(23)))), ((int)(((byte)(49)))));
            this.panel2.Controls.Add(this.btn_setupmanage);
            this.panel2.Controls.Add(this.btn_attendencemanagment);
            this.panel2.Controls.Add(this.btn_staffmanagment);
            this.panel2.Controls.Add(this.btn_userManagement);
            this.panel2.Controls.Add(this.btn_apiKey);
            this.panel2.Controls.Add(this.btn_device_management);
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(187, 576);
            this.panel2.TabIndex = 2;
            // 
            // btn_setupmanage
            // 
            this.btn_setupmanage.BackColor = System.Drawing.Color.Transparent;
            this.btn_setupmanage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_setupmanage.ForeColor = System.Drawing.Color.White;
            this.btn_setupmanage.Location = new System.Drawing.Point(0, 357);
            this.btn_setupmanage.Name = "btn_setupmanage";
            this.btn_setupmanage.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_setupmanage.Size = new System.Drawing.Size(191, 46);
            this.btn_setupmanage.TabIndex = 7;
            this.btn_setupmanage.Text = "Setup Managment";
            this.btn_setupmanage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_setupmanage.Click += new System.EventHandler(this.btn_setupmanage_Click);
            // 
            // btn_attendencemanagment
            // 
            this.btn_attendencemanagment.BackColor = System.Drawing.Color.Transparent;
            this.btn_attendencemanagment.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_attendencemanagment.ForeColor = System.Drawing.Color.White;
            this.btn_attendencemanagment.Location = new System.Drawing.Point(0, 297);
            this.btn_attendencemanagment.Name = "btn_attendencemanagment";
            this.btn_attendencemanagment.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_attendencemanagment.Size = new System.Drawing.Size(191, 46);
            this.btn_attendencemanagment.TabIndex = 5;
            this.btn_attendencemanagment.Text = "Attendence Managment";
            this.btn_attendencemanagment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_attendencemanagment.Click += new System.EventHandler(this.btn_attendencemanagment_Click);
            // 
            // btn_staffmanagment
            // 
            this.btn_staffmanagment.BackColor = System.Drawing.Color.Transparent;
            this.btn_staffmanagment.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_staffmanagment.ForeColor = System.Drawing.Color.White;
            this.btn_staffmanagment.Location = new System.Drawing.Point(-2, 246);
            this.btn_staffmanagment.Name = "btn_staffmanagment";
            this.btn_staffmanagment.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_staffmanagment.Size = new System.Drawing.Size(191, 46);
            this.btn_staffmanagment.TabIndex = 4;
            this.btn_staffmanagment.Text = "Staff Managment";
            this.btn_staffmanagment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_staffmanagment.Click += new System.EventHandler(this.btn_attendance_Click);
            this.btn_staffmanagment.MouseLeave += new System.EventHandler(this.btn_attendance_MouseLeave);
            this.btn_staffmanagment.MouseHover += new System.EventHandler(this.btn_attendance_MouseHover);
            // 
            // btn_userManagement
            // 
            this.btn_userManagement.BackColor = System.Drawing.Color.Transparent;
            this.btn_userManagement.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_userManagement.ForeColor = System.Drawing.Color.White;
            this.btn_userManagement.Location = new System.Drawing.Point(0, 200);
            this.btn_userManagement.Name = "btn_userManagement";
            this.btn_userManagement.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_userManagement.Size = new System.Drawing.Size(187, 46);
            this.btn_userManagement.TabIndex = 3;
            this.btn_userManagement.Text = "User Management";
            this.btn_userManagement.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_userManagement.Click += new System.EventHandler(this.btn_userManagement_Click);
            this.btn_userManagement.MouseLeave += new System.EventHandler(this.btn_userManagement_MouseLeave);
            this.btn_userManagement.MouseHover += new System.EventHandler(this.btn_userManagement_MouseHover);
            // 
            // btn_apiKey
            // 
            this.btn_apiKey.BackColor = System.Drawing.Color.Transparent;
            this.btn_apiKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_apiKey.ForeColor = System.Drawing.Color.White;
            this.btn_apiKey.Location = new System.Drawing.Point(0, 154);
            this.btn_apiKey.Name = "btn_apiKey";
            this.btn_apiKey.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_apiKey.Size = new System.Drawing.Size(191, 46);
            this.btn_apiKey.TabIndex = 2;
            this.btn_apiKey.Text = "API Key";
            this.btn_apiKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_apiKey.Click += new System.EventHandler(this.btn_apiKey_Click);
            this.btn_apiKey.Leave += new System.EventHandler(this.btn_apiKey_Leave);
            this.btn_apiKey.MouseLeave += new System.EventHandler(this.btn_apiKey_MouseLeave);
            this.btn_apiKey.MouseHover += new System.EventHandler(this.btn_apiKey_MouseHover);
            // 
            // btn_device_management
            // 
            this.btn_device_management.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btn_device_management.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_device_management.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(23)))), ((int)(((byte)(46)))));
            this.btn_device_management.Location = new System.Drawing.Point(0, 108);
            this.btn_device_management.Name = "btn_device_management";
            this.btn_device_management.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_device_management.Size = new System.Drawing.Size(187, 46);
            this.btn_device_management.TabIndex = 1;
            this.btn_device_management.Text = "Device Management";
            this.btn_device_management.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_device_management.Click += new System.EventHandler(this.btn_device_management_Click);
            this.btn_device_management.MouseLeave += new System.EventHandler(this.btn_device_management_MouseLeave);
            this.btn_device_management.MouseHover += new System.EventHandler(this.btn_device_management_MouseHover);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::smart_attendance_middleware.Properties.Resources.icon_facede_tection;
            this.pictureBox1.Location = new System.Drawing.Point(40, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(79, 79);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pnl_APIkey
            // 
            this.pnl_APIkey.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_APIkey.Controls.Add(this.panel4);
            this.pnl_APIkey.Controls.Add(this.label33);
            this.pnl_APIkey.Controls.Add(this.label34);
            this.pnl_APIkey.Location = new System.Drawing.Point(892, 0);
            this.pnl_APIkey.Name = "pnl_APIkey";
            this.pnl_APIkey.Size = new System.Drawing.Size(700, 576);
            this.pnl_APIkey.TabIndex = 4;
            this.pnl_APIkey.Visible = false;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.Controls.Add(this.lbl_apikey_status);
            this.panel4.Controls.Add(this.btnApiPaste);
            this.panel4.Controls.Add(this.txtAPIkey);
            this.panel4.Controls.Add(this.pictureBox2);
            this.panel4.Controls.Add(this.btnSaveAPI);
            this.panel4.Location = new System.Drawing.Point(21, 101);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(660, 424);
            this.panel4.TabIndex = 2;
            // 
            // lbl_apikey_status
            // 
            this.lbl_apikey_status.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_apikey_status.Location = new System.Drawing.Point(53, 284);
            this.lbl_apikey_status.Name = "lbl_apikey_status";
            this.lbl_apikey_status.Size = new System.Drawing.Size(538, 23);
            this.lbl_apikey_status.TabIndex = 44;
            this.lbl_apikey_status.Text = "label7";
            this.lbl_apikey_status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnApiPaste
            // 
            this.btnApiPaste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(150)))), ((int)(((byte)(254)))));
            this.btnApiPaste.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApiPaste.ForeColor = System.Drawing.Color.White;
            this.btnApiPaste.Location = new System.Drawing.Point(536, 162);
            this.btnApiPaste.Name = "btnApiPaste";
            this.btnApiPaste.Size = new System.Drawing.Size(55, 53);
            this.btnApiPaste.TabIndex = 43;
            this.btnApiPaste.Text = "Paste";
            this.btnApiPaste.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnApiPaste.Click += new System.EventHandler(this.btnApiPaste_Click);
            // 
            // txtAPIkey
            // 
            this.txtAPIkey.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtAPIkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAPIkey.Location = new System.Drawing.Point(53, 178);
            this.txtAPIkey.Name = "txtAPIkey";
            this.txtAPIkey.PasswordChar = '*';
            this.txtAPIkey.Size = new System.Drawing.Size(467, 17);
            this.txtAPIkey.TabIndex = 42;
            this.txtAPIkey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::smart_attendance_middleware.Properties.Resources.txt_place_holder_api;
            this.pictureBox2.Location = new System.Drawing.Point(28, 162);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(585, 53);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox2.TabIndex = 41;
            this.pictureBox2.TabStop = false;
            // 
            // btnSaveAPI
            // 
            this.btnSaveAPI.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveAPI.Location = new System.Drawing.Point(264, 249);
            this.btnSaveAPI.Name = "btnSaveAPI";
            this.btnSaveAPI.Size = new System.Drawing.Size(113, 32);
            this.btnSaveAPI.TabIndex = 39;
            this.btnSaveAPI.Text = "Save";
            this.btnSaveAPI.UseVisualStyleBackColor = true;
            this.btnSaveAPI.Click += new System.EventHandler(this.btnSaveAPI_Click_1);
            // 
            // label33
            // 
            this.label33.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.ForeColor = System.Drawing.Color.DimGray;
            this.label33.Location = new System.Drawing.Point(16, 47);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(507, 43);
            this.label33.TabIndex = 1;
            this.label33.Text = "Paste the API Key from your Fitness Marvel Smart Attendance settings to connect t" +
    "his device.";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label34.Location = new System.Drawing.Point(12, 19);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(215, 26);
            this.label34.TabIndex = 0;
            this.label34.Text = "API Key Configration";
            // 
            // pnl_UserManagement
            // 
            this.pnl_UserManagement.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_UserManagement.Controls.Add(this.btn_addUser);
            this.pnl_UserManagement.Controls.Add(this.panel3);
            this.pnl_UserManagement.Controls.Add(this.label5);
            this.pnl_UserManagement.Controls.Add(this.label6);
            this.pnl_UserManagement.Location = new System.Drawing.Point(1610, 0);
            this.pnl_UserManagement.Name = "pnl_UserManagement";
            this.pnl_UserManagement.Size = new System.Drawing.Size(700, 576);
            this.pnl_UserManagement.TabIndex = 5;
            this.pnl_UserManagement.Visible = false;
            // 
            // btn_addUser
            // 
            this.btn_addUser.Location = new System.Drawing.Point(572, 63);
            this.btn_addUser.Name = "btn_addUser";
            this.btn_addUser.Size = new System.Drawing.Size(105, 32);
            this.btn_addUser.TabIndex = 4;
            this.btn_addUser.Text = "Add New User";
            this.btn_addUser.UseVisualStyleBackColor = true;
            this.btn_addUser.Click += new System.EventHandler(this.btn_addUser_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.btn_editUser);
            this.panel3.Controls.Add(this.list_LocalUser);
            this.panel3.Controls.Add(this.btn_change_password);
            this.panel3.Controls.Add(this.btn_deleteUser);
            this.panel3.Location = new System.Drawing.Point(22, 101);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(660, 424);
            this.panel3.TabIndex = 2;
            // 
            // btn_editUser
            // 
            this.btn_editUser.Location = new System.Drawing.Point(540, 326);
            this.btn_editUser.Name = "btn_editUser";
            this.btn_editUser.Size = new System.Drawing.Size(113, 32);
            this.btn_editUser.TabIndex = 41;
            this.btn_editUser.Text = "Edit";
            this.btn_editUser.UseVisualStyleBackColor = true;
            this.btn_editUser.Click += new System.EventHandler(this.btn_editUser_Click_1);
            // 
            // list_LocalUser
            // 
            this.list_LocalUser.HideSelection = false;
            this.list_LocalUser.Location = new System.Drawing.Point(7, 7);
            this.list_LocalUser.Name = "list_LocalUser";
            this.list_LocalUser.Size = new System.Drawing.Size(648, 301);
            this.list_LocalUser.TabIndex = 40;
            this.list_LocalUser.UseCompatibleStateImageBehavior = false;
            this.list_LocalUser.SelectedIndexChanged += new System.EventHandler(this.list_LocalUser_SelectedIndexChanged);
            // 
            // btn_change_password
            // 
            this.btn_change_password.Location = new System.Drawing.Point(418, 326);
            this.btn_change_password.Name = "btn_change_password";
            this.btn_change_password.Size = new System.Drawing.Size(113, 32);
            this.btn_change_password.TabIndex = 39;
            this.btn_change_password.Text = "Change Password";
            this.btn_change_password.UseVisualStyleBackColor = true;
            this.btn_change_password.Click += new System.EventHandler(this.btn_change_password_Click);
            // 
            // btn_deleteUser
            // 
            this.btn_deleteUser.Location = new System.Drawing.Point(296, 326);
            this.btn_deleteUser.Name = "btn_deleteUser";
            this.btn_deleteUser.Size = new System.Drawing.Size(113, 32);
            this.btn_deleteUser.TabIndex = 5;
            this.btn_deleteUser.Text = "Delete User";
            this.btn_deleteUser.UseVisualStyleBackColor = true;
            this.btn_deleteUser.Click += new System.EventHandler(this.btn_deleteUser_Click);
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.DimGray;
            this.label5.Location = new System.Drawing.Point(17, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(507, 43);
            this.label5.TabIndex = 1;
            this.label5.Text = "Add, update, or remove users with role-based access for this device.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label6.Location = new System.Drawing.Point(13, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(191, 26);
            this.label6.TabIndex = 0;
            this.label6.Text = "User Management";
            // 
            // pnl_attendence
            // 
            this.pnl_attendence.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_attendence.Controls.Add(this.tab_attendence);
            this.pnl_attendence.Controls.Add(this.label48);
            this.pnl_attendence.Location = new System.Drawing.Point(2326, 601);
            this.pnl_attendence.Name = "pnl_attendence";
            this.pnl_attendence.Size = new System.Drawing.Size(703, 575);
            this.pnl_attendence.TabIndex = 4;
            this.pnl_attendence.Visible = false;
            // 
            // tab_attendence
            // 
            this.tab_attendence.Controls.Add(this.tab_members);
            this.tab_attendence.Controls.Add(this.tab_memberAttendence);
            this.tab_attendence.Controls.Add(this.tab_manualdoor);
            this.tab_attendence.Location = new System.Drawing.Point(8, 71);
            this.tab_attendence.Name = "tab_attendence";
            this.tab_attendence.SelectedIndex = 0;
            this.tab_attendence.Size = new System.Drawing.Size(692, 497);
            this.tab_attendence.TabIndex = 83;
            // 
            // tab_members
            // 
            this.tab_members.Controls.Add(this.txt_memb_name);
            this.tab_members.Controls.Add(this.flt_members);
            this.tab_members.Controls.Add(this.lst_members);
            this.tab_members.Location = new System.Drawing.Point(4, 22);
            this.tab_members.Name = "tab_members";
            this.tab_members.Size = new System.Drawing.Size(684, 471);
            this.tab_members.TabIndex = 2;
            this.tab_members.Text = "Members";
            this.tab_members.UseVisualStyleBackColor = true;
            // 
            // txt_memb_name
            // 
            this.txt_memb_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_memb_name.Location = new System.Drawing.Point(7, 9);
            this.txt_memb_name.Name = "txt_memb_name";
            this.txt_memb_name.Size = new System.Drawing.Size(188, 23);
            this.txt_memb_name.TabIndex = 89;
            // 
            // flt_members
            // 
            this.flt_members.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flt_members.Location = new System.Drawing.Point(201, 7);
            this.flt_members.Name = "flt_members";
            this.flt_members.Size = new System.Drawing.Size(80, 28);
            this.flt_members.TabIndex = 91;
            this.flt_members.Text = "Filter";
            // 
            // lst_members
            // 
            this.lst_members.FullRowSelect = true;
            this.lst_members.GridLines = true;
            this.lst_members.HideSelection = false;
            this.lst_members.Location = new System.Drawing.Point(7, 41);
            this.lst_members.Name = "lst_members";
            this.lst_members.Size = new System.Drawing.Size(669, 427);
            this.lst_members.TabIndex = 92;
            this.lst_members.UseCompatibleStateImageBehavior = false;
            this.lst_members.View = System.Windows.Forms.View.Details;
            // 
            // tab_memberAttendence
            // 
            this.tab_memberAttendence.Controls.Add(this.panel5);
            this.tab_memberAttendence.Location = new System.Drawing.Point(4, 22);
            this.tab_memberAttendence.Name = "tab_memberAttendence";
            this.tab_memberAttendence.Padding = new System.Windows.Forms.Padding(3);
            this.tab_memberAttendence.Size = new System.Drawing.Size(684, 471);
            this.tab_memberAttendence.TabIndex = 0;
            this.tab_memberAttendence.Text = "Attendence";
            this.tab_memberAttendence.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.txt_membername);
            this.panel5.Controls.Add(this.date_member);
            this.panel5.Controls.Add(this.btn_filter_member);
            this.panel5.Controls.Add(this.lst_member);
            this.panel5.Location = new System.Drawing.Point(0, 1);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(684, 467);
            this.panel5.TabIndex = 85;
            // 
            // txt_membername
            // 
            this.txt_membername.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_membername.Location = new System.Drawing.Point(9, 9);
            this.txt_membername.Name = "txt_membername";
            this.txt_membername.Size = new System.Drawing.Size(100, 23);
            this.txt_membername.TabIndex = 82;
            this.txt_membername.TextChanged += new System.EventHandler(this.txt_membername_TextChanged);
            // 
            // date_member
            // 
            this.date_member.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.date_member.Location = new System.Drawing.Point(115, 9);
            this.date_member.Name = "date_member";
            this.date_member.Size = new System.Drawing.Size(140, 23);
            this.date_member.TabIndex = 83;
            this.date_member.ValueChanged += new System.EventHandler(this.date_member_ValueChanged);
            // 
            // btn_filter_member
            // 
            this.btn_filter_member.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_filter_member.Location = new System.Drawing.Point(261, 6);
            this.btn_filter_member.Name = "btn_filter_member";
            this.btn_filter_member.Size = new System.Drawing.Size(80, 28);
            this.btn_filter_member.TabIndex = 85;
            this.btn_filter_member.Text = "Filter";
            this.btn_filter_member.Click += new System.EventHandler(this.btn_filter_member_Click);
            // 
            // lst_member
            // 
            this.lst_member.FullRowSelect = true;
            this.lst_member.GridLines = true;
            this.lst_member.HideSelection = false;
            this.lst_member.Location = new System.Drawing.Point(9, 40);
            this.lst_member.Name = "lst_member";
            this.lst_member.Size = new System.Drawing.Size(669, 424);
            this.lst_member.TabIndex = 86;
            this.lst_member.UseCompatibleStateImageBehavior = false;
            this.lst_member.View = System.Windows.Forms.View.Details;
            this.lst_member.SelectedIndexChanged += new System.EventHandler(this.lst_member_SelectedIndexChanged);
            // 
            // tab_manualdoor
            // 
            this.tab_manualdoor.Controls.Add(this.panel6);
            this.tab_manualdoor.Location = new System.Drawing.Point(4, 22);
            this.tab_manualdoor.Name = "tab_manualdoor";
            this.tab_manualdoor.Padding = new System.Windows.Forms.Padding(3);
            this.tab_manualdoor.Size = new System.Drawing.Size(684, 471);
            this.tab_manualdoor.TabIndex = 1;
            this.tab_manualdoor.Text = "Manual Door ";
            this.tab_manualdoor.UseVisualStyleBackColor = true;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.date_manualdoor);
            this.panel6.Controls.Add(this.btn_filter_manualdoor);
            this.panel6.Controls.Add(this.lst_manualdoor);
            this.panel6.Location = new System.Drawing.Point(3, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(675, 465);
            this.panel6.TabIndex = 84;
            // 
            // date_manualdoor
            // 
            this.date_manualdoor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.date_manualdoor.Location = new System.Drawing.Point(9, 6);
            this.date_manualdoor.Name = "date_manualdoor";
            this.date_manualdoor.Size = new System.Drawing.Size(140, 23);
            this.date_manualdoor.TabIndex = 83;
            this.date_manualdoor.ValueChanged += new System.EventHandler(this.date_manualdoor_ValueChanged);
            // 
            // btn_filter_manualdoor
            // 
            this.btn_filter_manualdoor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_filter_manualdoor.Location = new System.Drawing.Point(155, 4);
            this.btn_filter_manualdoor.Name = "btn_filter_manualdoor";
            this.btn_filter_manualdoor.Size = new System.Drawing.Size(79, 25);
            this.btn_filter_manualdoor.TabIndex = 85;
            this.btn_filter_manualdoor.Text = "Filter";
            this.btn_filter_manualdoor.Click += new System.EventHandler(this.btn_filter_manualdoor_Click);
            // 
            // lst_manualdoor
            // 
            this.lst_manualdoor.FullRowSelect = true;
            this.lst_manualdoor.GridLines = true;
            this.lst_manualdoor.HideSelection = false;
            this.lst_manualdoor.Location = new System.Drawing.Point(9, 35);
            this.lst_manualdoor.Name = "lst_manualdoor";
            this.lst_manualdoor.Size = new System.Drawing.Size(663, 430);
            this.lst_manualdoor.TabIndex = 86;
            this.lst_manualdoor.UseCompatibleStateImageBehavior = false;
            this.lst_manualdoor.View = System.Windows.Forms.View.Details;
            this.lst_manualdoor.SelectedIndexChanged += new System.EventHandler(this.lst_manualdoor_SelectedIndexChanged);
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label48.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label48.Location = new System.Drawing.Point(3, 26);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(225, 26);
            this.label48.TabIndex = 56;
            this.label48.Text = "Member Management";
            // 
            // pnl_setup
            // 
            this.pnl_setup.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_setup.Controls.Add(this.subpnl_setup);
            this.pnl_setup.Controls.Add(this.label42);
            this.pnl_setup.Controls.Add(this.label43);
            this.pnl_setup.Location = new System.Drawing.Point(1610, 600);
            this.pnl_setup.Name = "pnl_setup";
            this.pnl_setup.Size = new System.Drawing.Size(703, 576);
            this.pnl_setup.TabIndex = 85;
            this.pnl_setup.Visible = false;
            // 
            // subpnl_setup
            // 
            this.subpnl_setup.BackColor = System.Drawing.Color.White;
            this.subpnl_setup.Controls.Add(this.btn_attendence_sim);
            this.subpnl_setup.Controls.Add(this.btn_settime);
            this.subpnl_setup.Controls.Add(this.btn_disableuser);
            this.subpnl_setup.Controls.Add(this.btn_enableuser);
            this.subpnl_setup.Controls.Add(this.btn_getdb);
            this.subpnl_setup.Controls.Add(this.btn_getinfo);
            this.subpnl_setup.Controls.Add(this.txt_enrollid);
            this.subpnl_setup.Controls.Add(this.num_debounce);
            this.subpnl_setup.Controls.Add(this.lbl_debounce);
            this.subpnl_setup.Controls.Add(this.btn_savedebounce);
            this.subpnl_setup.Location = new System.Drawing.Point(20, 80);
            this.subpnl_setup.Name = "subpnl_setup";
            this.subpnl_setup.Size = new System.Drawing.Size(680, 489);
            this.subpnl_setup.TabIndex = 2;
            // 
            // btn_attendence_sim
            // 
            this.btn_attendence_sim.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_attendence_sim.Location = new System.Drawing.Point(271, 133);
            this.btn_attendence_sim.Name = "btn_attendence_sim";
            this.btn_attendence_sim.Size = new System.Drawing.Size(113, 32);
            this.btn_attendence_sim.TabIndex = 51;
            this.btn_attendence_sim.Text = "Simulator";
            this.btn_attendence_sim.UseVisualStyleBackColor = true;
            this.btn_attendence_sim.Click += new System.EventHandler(this.btn_attendence_sim_Click);
            // 
            // btn_settime
            // 
            this.btn_settime.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_settime.Location = new System.Drawing.Point(152, 133);
            this.btn_settime.Name = "btn_settime";
            this.btn_settime.Size = new System.Drawing.Size(113, 32);
            this.btn_settime.TabIndex = 50;
            this.btn_settime.Text = "Set Time";
            this.btn_settime.UseVisualStyleBackColor = true;
            this.btn_settime.Click += new System.EventHandler(this.btn_settime_Click);
            // 
            // btn_disableuser
            // 
            this.btn_disableuser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_disableuser.Location = new System.Drawing.Point(391, 87);
            this.btn_disableuser.Name = "btn_disableuser";
            this.btn_disableuser.Size = new System.Drawing.Size(113, 32);
            this.btn_disableuser.TabIndex = 49;
            this.btn_disableuser.Text = "Disable";
            this.btn_disableuser.UseVisualStyleBackColor = true;
            this.btn_disableuser.Click += new System.EventHandler(this.btn_disableuser_Click);
            // 
            // btn_enableuser
            // 
            this.btn_enableuser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_enableuser.Location = new System.Drawing.Point(271, 87);
            this.btn_enableuser.Name = "btn_enableuser";
            this.btn_enableuser.Size = new System.Drawing.Size(113, 32);
            this.btn_enableuser.TabIndex = 48;
            this.btn_enableuser.Text = "Enable";
            this.btn_enableuser.UseVisualStyleBackColor = true;
            this.btn_enableuser.Click += new System.EventHandler(this.btn_enableuser_Click);
            // 
            // btn_getdb
            // 
            this.btn_getdb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_getdb.Location = new System.Drawing.Point(16, 133);
            this.btn_getdb.Name = "btn_getdb";
            this.btn_getdb.Size = new System.Drawing.Size(113, 32);
            this.btn_getdb.TabIndex = 47;
            this.btn_getdb.Text = "Get Details";
            this.btn_getdb.UseVisualStyleBackColor = true;
            this.btn_getdb.Click += new System.EventHandler(this.btn_getdb_Click);
            // 
            // btn_getinfo
            // 
            this.btn_getinfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_getinfo.Location = new System.Drawing.Point(152, 87);
            this.btn_getinfo.Name = "btn_getinfo";
            this.btn_getinfo.Size = new System.Drawing.Size(113, 32);
            this.btn_getinfo.TabIndex = 46;
            this.btn_getinfo.Text = "Get Info";
            this.btn_getinfo.UseVisualStyleBackColor = true;
            this.btn_getinfo.Click += new System.EventHandler(this.btn_getinfo_Click);
            // 
            // txt_enrollid
            // 
            this.txt_enrollid.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_enrollid.Location = new System.Drawing.Point(16, 87);
            this.txt_enrollid.Name = "txt_enrollid";
            this.txt_enrollid.Size = new System.Drawing.Size(113, 26);
            this.txt_enrollid.TabIndex = 45;
            this.txt_enrollid.TextChanged += new System.EventHandler(this.txt_enrollid_TextChanged);
            // 
            // num_debounce
            // 
            this.num_debounce.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.num_debounce.Location = new System.Drawing.Point(16, 43);
            this.num_debounce.Name = "num_debounce";
            this.num_debounce.Size = new System.Drawing.Size(113, 26);
            this.num_debounce.TabIndex = 44;
            this.num_debounce.ValueChanged += new System.EventHandler(this.num_debounce_ValueChanged);
            // 
            // lbl_debounce
            // 
            this.lbl_debounce.AutoSize = true;
            this.lbl_debounce.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_debounce.Location = new System.Drawing.Point(12, 13);
            this.lbl_debounce.Name = "lbl_debounce";
            this.lbl_debounce.Size = new System.Drawing.Size(83, 20);
            this.lbl_debounce.TabIndex = 43;
            this.lbl_debounce.Text = "Debounce";
            // 
            // btn_savedebounce
            // 
            this.btn_savedebounce.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_savedebounce.Location = new System.Drawing.Point(152, 39);
            this.btn_savedebounce.Name = "btn_savedebounce";
            this.btn_savedebounce.Size = new System.Drawing.Size(113, 32);
            this.btn_savedebounce.TabIndex = 41;
            this.btn_savedebounce.Text = "Save";
            this.btn_savedebounce.UseVisualStyleBackColor = true;
            this.btn_savedebounce.Click += new System.EventHandler(this.btn_savedebounce_Click);
            // 
            // label42
            // 
            this.label42.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label42.ForeColor = System.Drawing.Color.DimGray;
            this.label42.Location = new System.Drawing.Point(17, 47);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(507, 43);
            this.label42.TabIndex = 1;
            this.label42.Text = "Configuration Tab";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label43.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label43.Location = new System.Drawing.Point(13, 19);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(190, 26);
            this.label43.TabIndex = 0;
            this.label43.Text = "Setup Managment";
            // 
            // pnl_staffsManagment
            // 
            this.pnl_staffsManagment.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_staffsManagment.Controls.Add(this.tab_staff);
            this.pnl_staffsManagment.Controls.Add(this.lbl_staffmanagment);
            this.pnl_staffsManagment.Location = new System.Drawing.Point(2323, 0);
            this.pnl_staffsManagment.Name = "pnl_staffsManagment";
            this.pnl_staffsManagment.Size = new System.Drawing.Size(703, 575);
            this.pnl_staffsManagment.TabIndex = 84;
            this.pnl_staffsManagment.Visible = false;
            // 
            // tab_staff
            // 
            this.tab_staff.Controls.Add(this.tab_staffmembers);
            this.tab_staff.Controls.Add(this.tab_staffattendence);
            this.tab_staff.Location = new System.Drawing.Point(8, 75);
            this.tab_staff.Name = "tab_staff";
            this.tab_staff.SelectedIndex = 0;
            this.tab_staff.Size = new System.Drawing.Size(692, 497);
            this.tab_staff.TabIndex = 83;
            // 
            // tab_staffmembers
            // 
            this.tab_staffmembers.Controls.Add(this.txt_staff);
            this.tab_staffmembers.Controls.Add(this.btn_stafffilter);
            this.tab_staffmembers.Controls.Add(this.lst_staffs);
            this.tab_staffmembers.Location = new System.Drawing.Point(4, 22);
            this.tab_staffmembers.Name = "tab_staffmembers";
            this.tab_staffmembers.Size = new System.Drawing.Size(684, 471);
            this.tab_staffmembers.TabIndex = 2;
            this.tab_staffmembers.Text = "Staffs";
            this.tab_staffmembers.UseVisualStyleBackColor = true;
            // 
            // txt_staff
            // 
            this.txt_staff.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_staff.Location = new System.Drawing.Point(7, 9);
            this.txt_staff.Name = "txt_staff";
            this.txt_staff.Size = new System.Drawing.Size(188, 23);
            this.txt_staff.TabIndex = 89;
            this.txt_staff.TextChanged += new System.EventHandler(this.txt_staff_TextChanged_1);
            // 
            // btn_stafffilter
            // 
            this.btn_stafffilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_stafffilter.Location = new System.Drawing.Point(201, 7);
            this.btn_stafffilter.Name = "btn_stafffilter";
            this.btn_stafffilter.Size = new System.Drawing.Size(80, 28);
            this.btn_stafffilter.TabIndex = 91;
            this.btn_stafffilter.Text = "Filter";
            this.btn_stafffilter.Click += new System.EventHandler(this.btn_stafffilter_Click_1);
            // 
            // lst_staffs
            // 
            this.lst_staffs.FullRowSelect = true;
            this.lst_staffs.GridLines = true;
            this.lst_staffs.HideSelection = false;
            this.lst_staffs.Location = new System.Drawing.Point(7, 41);
            this.lst_staffs.Name = "lst_staffs";
            this.lst_staffs.Size = new System.Drawing.Size(669, 427);
            this.lst_staffs.TabIndex = 92;
            this.lst_staffs.UseCompatibleStateImageBehavior = false;
            this.lst_staffs.View = System.Windows.Forms.View.Details;
            this.lst_staffs.SelectedIndexChanged += new System.EventHandler(this.lst_staffs_SelectedIndexChanged);
            // 
            // tab_staffattendence
            // 
            this.tab_staffattendence.Controls.Add(this.panel8);
            this.tab_staffattendence.Location = new System.Drawing.Point(4, 22);
            this.tab_staffattendence.Name = "tab_staffattendence";
            this.tab_staffattendence.Padding = new System.Windows.Forms.Padding(3);
            this.tab_staffattendence.Size = new System.Drawing.Size(684, 471);
            this.tab_staffattendence.TabIndex = 0;
            this.tab_staffattendence.Text = "Attendence";
            this.tab_staffattendence.UseVisualStyleBackColor = true;
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.txt_staffname);
            this.panel8.Controls.Add(this.date_staffattendence);
            this.panel8.Controls.Add(this.btn_filterattendencestaff);
            this.panel8.Controls.Add(this.lst_staffattendence);
            this.panel8.Location = new System.Drawing.Point(0, 1);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(684, 467);
            this.panel8.TabIndex = 85;
            // 
            // txt_staffname
            // 
            this.txt_staffname.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_staffname.Location = new System.Drawing.Point(9, 9);
            this.txt_staffname.Name = "txt_staffname";
            this.txt_staffname.Size = new System.Drawing.Size(100, 23);
            this.txt_staffname.TabIndex = 82;
            this.txt_staffname.TextChanged += new System.EventHandler(this.txt_staffname_TextChanged_1);
            // 
            // date_staffattendence
            // 
            this.date_staffattendence.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.date_staffattendence.Location = new System.Drawing.Point(115, 9);
            this.date_staffattendence.Name = "date_staffattendence";
            this.date_staffattendence.Size = new System.Drawing.Size(140, 23);
            this.date_staffattendence.TabIndex = 83;
            this.date_staffattendence.ValueChanged += new System.EventHandler(this.date_staffattendence_ValueChanged_1);
            // 
            // btn_filterattendencestaff
            // 
            this.btn_filterattendencestaff.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_filterattendencestaff.Location = new System.Drawing.Point(261, 6);
            this.btn_filterattendencestaff.Name = "btn_filterattendencestaff";
            this.btn_filterattendencestaff.Size = new System.Drawing.Size(80, 28);
            this.btn_filterattendencestaff.TabIndex = 85;
            this.btn_filterattendencestaff.Text = "Filter";
            this.btn_filterattendencestaff.Click += new System.EventHandler(this.btn_filterattendencestaff_Click_1);
            // 
            // lst_staffattendence
            // 
            this.lst_staffattendence.FullRowSelect = true;
            this.lst_staffattendence.GridLines = true;
            this.lst_staffattendence.HideSelection = false;
            this.lst_staffattendence.Location = new System.Drawing.Point(9, 40);
            this.lst_staffattendence.Name = "lst_staffattendence";
            this.lst_staffattendence.Size = new System.Drawing.Size(669, 424);
            this.lst_staffattendence.TabIndex = 86;
            this.lst_staffattendence.UseCompatibleStateImageBehavior = false;
            this.lst_staffattendence.View = System.Windows.Forms.View.Details;
            this.lst_staffattendence.SelectedIndexChanged += new System.EventHandler(this.lst_staffattendence_SelectedIndexChanged);
            // 
            // lbl_staffmanagment
            // 
            this.lbl_staffmanagment.AutoSize = true;
            this.lbl_staffmanagment.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_staffmanagment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbl_staffmanagment.Location = new System.Drawing.Point(3, 26);
            this.lbl_staffmanagment.Name = "lbl_staffmanagment";
            this.lbl_staffmanagment.Size = new System.Drawing.Size(190, 26);
            this.lbl_staffmanagment.TabIndex = 56;
            this.lbl_staffmanagment.Text = "Staff Management";
            // 
            // DeviceManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(3041, 1181);
            this.Controls.Add(this.pnl_staffsManagment);
            this.Controls.Add(this.pnl_setup);
            this.Controls.Add(this.pnl_staffhandling);
            this.Controls.Add(this.pnl_attendence);
            this.Controls.Add(this.pnl_APIkey);
            this.Controls.Add(this.pnl_UserManagement);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.pnl_deviceManager);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DeviceManager";
            this.Text = "Device Manager";
            this.Load += new System.EventHandler(this.DeviceManager_Load);
            this.pnl_deviceManager.ResumeLayout(false);
            this.pnl_deviceManager.PerformLayout();
            this.pnl_device.ResumeLayout(false);
            this.pnl_device.PerformLayout();
            this.pnl_staffhandling.ResumeLayout(false);
            this.pnl_staffhandling.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnl_APIkey.ResumeLayout(false);
            this.pnl_APIkey.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.pnl_UserManagement.ResumeLayout(false);
            this.pnl_UserManagement.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.pnl_attendence.ResumeLayout(false);
            this.pnl_attendence.PerformLayout();
            this.tab_attendence.ResumeLayout(false);
            this.tab_members.ResumeLayout(false);
            this.tab_members.PerformLayout();
            this.tab_memberAttendence.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.tab_manualdoor.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.pnl_setup.ResumeLayout(false);
            this.pnl_setup.PerformLayout();
            this.subpnl_setup.ResumeLayout(false);
            this.subpnl_setup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_debounce)).EndInit();
            this.pnl_staffsManagment.ResumeLayout(false);
            this.pnl_staffsManagment.PerformLayout();
            this.tab_staff.ResumeLayout(false);
            this.tab_staffmembers.ResumeLayout(false);
            this.tab_staffmembers.PerformLayout();
            this.tab_staffattendence.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnl_deviceManager;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel pnl_device;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label btn_device_management;
        private System.Windows.Forms.Label btn_apiKey;
        private System.Windows.Forms.Label btn_staffmanagment;
        private System.Windows.Forms.Label btn_userManagement;
        private System.Windows.Forms.Button btn_add_device;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txt_department;
        private System.Windows.Forms.TextBox txt_Device_id;
        private System.Windows.Forms.TextBox txt_finger_capacity;
        private System.Windows.Forms.TextBox txt_RFID_capacity;
        private System.Windows.Forms.TextBox txt_face_capacity;
        private System.Windows.Forms.TextBox txt_last_synced;
        private System.Windows.Forms.TextBox txt_device_status;
        private System.Windows.Forms.TextBox txt_location;
        private System.Windows.Forms.TextBox txt_timeZone;
        private System.Windows.Forms.TextBox txt_port;
        private System.Windows.Forms.TextBox txt_ip;
        private System.Windows.Forms.TextBox txt_Serial;
        private System.Windows.Forms.ComboBox cmb_device_type;
        private System.Windows.Forms.ComboBox cmb_connectionType;
        private System.Windows.Forms.Button btn_remove_device;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Panel pnl_APIkey;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label btnApiPaste;
        private System.Windows.Forms.TextBox txtAPIkey;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnSaveAPI;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Panel pnl_UserManagement;
        private System.Windows.Forms.Button btn_addUser;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btn_editUser;
        private System.Windows.Forms.ListView list_LocalUser;
        private System.Windows.Forms.Button btn_change_password;
        private System.Windows.Forms.Button btn_deleteUser;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;

        private System.Windows.Forms.ListView listViewDevices;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmb_machine_no;
        private System.Windows.Forms.Label lbl_apikey_status;
        private System.Windows.Forms.TextBox txt_password;
        private System.Windows.Forms.Label lbl_password;
        private System.Windows.Forms.Button btn_format;
        private Label btn_attendencemanagment;
        private Panel pnl_attendence;
        private TabControl tab_attendence;
        private TabPage tab_memberAttendence;
        private TabPage tab_manualdoor;
        private Panel panel6;
        private DateTimePicker date_manualdoor;
        private Button btn_filter_manualdoor;
        private ListView lst_manualdoor;
        private Label label48;
        private Panel panel5;
        private TextBox txt_membername;
        private DateTimePicker date_member;
        private Button btn_filter_member;
        private ListView lst_member;
        private Panel pnl_staffhandling;
        private Label label51;
        private Label label52;
        private Panel panel1;
        private TextBox txt_stffname;
        private TextBox txt_stffid;
        private Label btn_namestff;
        private Label label7;
        private Button btn_stffenroll;
        private Button btn_image;
        private ListView lst_stff;
        private Button btn_deletestff;
        private Label label41;
        private TextBox txt_staffcardid;
        private Panel pnl_setup;
        private Panel subpnl_setup;
        private Button btn_savedebounce;
        private Label label42;
        private Label label43;
        private Label lbl_debounce;
        private NumericUpDown num_debounce;
        private Label btn_setupmanage;
        private Button btn_getinfo;
        private TextBox txt_enrollid;
        private Button btn_getdb;
        private Button btn_enableuser;
        private Button btn_disableuser;
        private Button btn_settime;
        private TabPage tab_members;
        private TextBox txt_memb_name;
        private Button flt_members;
        private ListView lst_members;
        private Button btn_attendence_sim;
        private Button btnCancle;
        private Panel pnl_staffsManagment;
        private TabControl tab_staff;
        private TabPage tab_staffmembers;
        private TextBox txt_staff;
        private Button btn_stafffilter;
        private ListView lst_staffs;
        private TabPage tab_staffattendence;
        private Panel panel8;
        private TextBox txt_staffname;
        private DateTimePicker date_staffattendence;
        private Button btn_filterattendencestaff;
        private ListView lst_staffattendence;
        private Label lbl_staffmanagment;
    }
}