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
            this.lbl_debounce = new System.Windows.Forms.Label();
            this.lbl_retention = new System.Windows.Forms.Label();
            this.lbl_drivefolderid = new System.Windows.Forms.Label();
            this.lbl_drivecredentials = new System.Windows.Forms.Label();
            this.lbl_apikey = new System.Windows.Forms.Label();
            this.btn_add_device = new System.Windows.Forms.Button();
            this.pnl_welcome = new System.Windows.Forms.Panel();
            this.pnl_welcome_header = new System.Windows.Forms.Panel();
            this.lbl_welcome_title = new System.Windows.Forms.Label();
            this.lbl_welcome_sub = new System.Windows.Forms.Label();
            this.pnl_welcome_tiles = new System.Windows.Forms.FlowLayoutPanel();
            this.tile_device = new System.Windows.Forms.Panel();
            this.lbl_tile_device_icon = new System.Windows.Forms.Label();
            this.lbl_tile_device_name = new System.Windows.Forms.Label();
            this.lbl_tile_device_desc = new System.Windows.Forms.Label();
            this.tile_user = new System.Windows.Forms.Panel();
            this.lbl_tile_user_icon = new System.Windows.Forms.Label();
            this.lbl_tile_user_name = new System.Windows.Forms.Label();
            this.lbl_tile_user_desc = new System.Windows.Forms.Label();
            this.tile_staff = new System.Windows.Forms.Panel();
            this.lbl_tile_staff_icon = new System.Windows.Forms.Label();
            this.lbl_tile_staff_name = new System.Windows.Forms.Label();
            this.lbl_tile_staff_desc = new System.Windows.Forms.Label();
            this.tile_member = new System.Windows.Forms.Panel();
            this.lbl_tile_member_icon = new System.Windows.Forms.Label();
            this.lbl_tile_member_name = new System.Windows.Forms.Label();
            this.lbl_tile_member_desc = new System.Windows.Forms.Label();
            this.tile_setup = new System.Windows.Forms.Panel();
            this.lbl_tile_setup_icon = new System.Windows.Forms.Label();
            this.lbl_tile_setup_name = new System.Windows.Forms.Label();
            this.lbl_tile_setup_desc = new System.Windows.Forms.Label();
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_setupmanage = new System.Windows.Forms.Label();
            this.btn_attendencemanagment = new System.Windows.Forms.Label();
            this.btn_staffmanagment = new System.Windows.Forms.Label();
            this.btn_userManagement = new System.Windows.Forms.Label();
            this.btn_device_management = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtAPIkey = new System.Windows.Forms.TextBox();
            this.btnSaveAPI = new System.Windows.Forms.Button();
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
            this.btn_test_connection = new System.Windows.Forms.Button();
            this.btn_backupcleanup = new System.Windows.Forms.Button();
            this.chk_database = new System.Windows.Forms.CheckBox();
            this.btn_save_backuptype = new System.Windows.Forms.Button();
            this.chk_logs = new System.Windows.Forms.CheckBox();
            this.chk_glogs = new System.Windows.Forms.CheckBox();
            this.num_retention = new System.Windows.Forms.NumericUpDown();
            this.btn_save_retentiondays = new System.Windows.Forms.Button();
            this.btn_save_drivecredentials = new System.Windows.Forms.Button();
            this.btn_save_drivefolderid = new System.Windows.Forms.Button();
            this.txt_drivecredentials = new System.Windows.Forms.TextBox();
            this.txt_drivefolderid = new System.Windows.Forms.TextBox();
            this.btn_settime = new System.Windows.Forms.Button();
            this.btn_getdb = new System.Windows.Forms.Button();
            this.txt_debounce = new System.Windows.Forms.TextBox();
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
            this.pnl_backup = new System.Windows.Forms.Panel();
            this.lbl_backuptype = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnl_deviceManager.SuspendLayout();
            this.pnl_device.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.num_retention)).BeginInit();
            this.pnl_staffsManagment.SuspendLayout();
            this.tab_staff.SuspendLayout();
            this.tab_staffmembers.SuspendLayout();
            this.tab_staffattendence.SuspendLayout();
            this.panel8.SuspendLayout();
            this.pnl_backup.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnl_welcome.SuspendLayout();
            this.pnl_welcome_header.SuspendLayout();
            this.pnl_welcome_tiles.SuspendLayout();
            this.tile_device.SuspendLayout();
            this.tile_user.SuspendLayout();
            this.tile_staff.SuspendLayout();
            this.tile_member.SuspendLayout();
            this.tile_setup.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_debounce
            // 
            this.lbl_debounce.AutoSize = true;
            this.lbl_debounce.Location = new System.Drawing.Point(13, 76);
            this.lbl_debounce.Name = "lbl_debounce";
            this.lbl_debounce.Size = new System.Drawing.Size(83, 13);
            this.lbl_debounce.TabIndex = 78;
            this.lbl_debounce.Text = "Debounce Time";
            // 
            // lbl_retention
            // 
            this.lbl_retention.AutoSize = true;
            this.lbl_retention.Location = new System.Drawing.Point(13, 136);
            this.lbl_retention.Name = "lbl_retention";
            this.lbl_retention.Size = new System.Drawing.Size(80, 13);
            this.lbl_retention.TabIndex = 79;
            this.lbl_retention.Text = "Retention Days";
            // 
            // lbl_drivefolderid
            // 
            this.lbl_drivefolderid.AutoSize = true;
            this.lbl_drivefolderid.Location = new System.Drawing.Point(13, 74);
            this.lbl_drivefolderid.Name = "lbl_drivefolderid";
            this.lbl_drivefolderid.Size = new System.Drawing.Size(78, 13);
            this.lbl_drivefolderid.TabIndex = 85;
            this.lbl_drivefolderid.Text = "Drive Folder ID";
            // 
            // lbl_drivecredentials
            // 
            this.lbl_drivecredentials.AutoSize = true;
            this.lbl_drivecredentials.Location = new System.Drawing.Point(304, 18);
            this.lbl_drivecredentials.Name = "lbl_drivecredentials";
            this.lbl_drivecredentials.Size = new System.Drawing.Size(87, 13);
            this.lbl_drivecredentials.TabIndex = 86;
            this.lbl_drivecredentials.Text = "Drive Credentials";
            // 
            // lbl_apikey
            // 
            this.lbl_apikey.AutoSize = true;
            this.lbl_apikey.Location = new System.Drawing.Point(13, 19);
            this.lbl_apikey.Name = "lbl_apikey";
            this.lbl_apikey.Size = new System.Drawing.Size(45, 13);
            this.lbl_apikey.TabIndex = 77;
            this.lbl_apikey.Text = "API Key";
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
            // 
            // listViewDevices
            // 
            this.listViewDevices.HideSelection = false;
            this.listViewDevices.Location = new System.Drawing.Point(0, 0);
            this.listViewDevices.Name = "listViewDevices";
            this.listViewDevices.Size = new System.Drawing.Size(660, 99);
            this.listViewDevices.TabIndex = 41;
            this.listViewDevices.UseCompatibleStateImageBehavior = false;
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
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(23)))), ((int)(((byte)(49)))));
            this.panel2.Controls.Add(this.btn_setupmanage);
            this.panel2.Controls.Add(this.btn_attendencemanagment);
            this.panel2.Controls.Add(this.btn_staffmanagment);
            this.panel2.Controls.Add(this.btn_userManagement);
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
            this.btn_setupmanage.Location = new System.Drawing.Point(0, 292);
            this.btn_setupmanage.Name = "btn_setupmanage";
            this.btn_setupmanage.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_setupmanage.Size = new System.Drawing.Size(187, 46);
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
            this.btn_attendencemanagment.Location = new System.Drawing.Point(0, 246);
            this.btn_attendencemanagment.Name = "btn_attendencemanagment";
            this.btn_attendencemanagment.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_attendencemanagment.Size = new System.Drawing.Size(187, 46);
            this.btn_attendencemanagment.TabIndex = 5;
            this.btn_attendencemanagment.Text = "Member Managment";
            this.btn_attendencemanagment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_attendencemanagment.Click += new System.EventHandler(this.btn_attendencemanagment_Click);
            // 
            // btn_staffmanagment
            // 
            this.btn_staffmanagment.BackColor = System.Drawing.Color.Transparent;
            this.btn_staffmanagment.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_staffmanagment.ForeColor = System.Drawing.Color.White;
            this.btn_staffmanagment.Location = new System.Drawing.Point(0, 200);
            this.btn_staffmanagment.Name = "btn_staffmanagment";
            this.btn_staffmanagment.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btn_staffmanagment.Size = new System.Drawing.Size(187, 46);
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
            this.btn_userManagement.Location = new System.Drawing.Point(0, 154);
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
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // txtAPIkey
            // 
            this.txtAPIkey.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAPIkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAPIkey.Location = new System.Drawing.Point(13, 35);
            this.txtAPIkey.Name = "txtAPIkey";
            this.txtAPIkey.PasswordChar = '*';
            this.txtAPIkey.Size = new System.Drawing.Size(504, 26);
            this.txtAPIkey.TabIndex = 42;
            // 
            // btnSaveAPI
            // 
            this.btnSaveAPI.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveAPI.Location = new System.Drawing.Point(523, 31);
            this.btnSaveAPI.Name = "btnSaveAPI";
            this.btnSaveAPI.Size = new System.Drawing.Size(108, 32);
            this.btnSaveAPI.TabIndex = 39;
            this.btnSaveAPI.Text = "Save";
            this.btnSaveAPI.UseVisualStyleBackColor = true;
            this.btnSaveAPI.Click += new System.EventHandler(this.btnSaveAPI_Click_1);
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
            this.pnl_attendence.Location = new System.Drawing.Point(898, 584);
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
            this.lst_members.Size = new System.Drawing.Size(669, 408);
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
            this.lst_member.Size = new System.Drawing.Size(669, 403);
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
            this.pnl_setup.Location = new System.Drawing.Point(183, 583);
            this.pnl_setup.Name = "pnl_setup";
            this.pnl_setup.Size = new System.Drawing.Size(703, 576);
            this.pnl_setup.TabIndex = 85;
            this.pnl_setup.Visible = false;
            // 
            // subpnl_setup
            // 
            this.subpnl_setup.Controls.Add(this.panel1);
            this.subpnl_setup.Controls.Add(this.pnl_backup);
            this.subpnl_setup.Location = new System.Drawing.Point(20, 71);
            this.subpnl_setup.Name = "subpnl_setup";
            this.subpnl_setup.Size = new System.Drawing.Size(680, 498);
            this.subpnl_setup.TabIndex = 2;
            this.subpnl_setup.Paint += new System.Windows.Forms.PaintEventHandler(this.subpnl_setup_Paint);
            // 
            // btn_test_connection
            // 
            this.btn_test_connection.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_test_connection.Location = new System.Drawing.Point(543, 78);
            this.btn_test_connection.Name = "btn_test_connection";
            this.btn_test_connection.Size = new System.Drawing.Size(90, 46);
            this.btn_test_connection.TabIndex = 76;
            this.btn_test_connection.Text = "Connect";
            this.btn_test_connection.UseVisualStyleBackColor = true;
            this.btn_test_connection.Click += new System.EventHandler(this.btn_test_connection_Click);
            // 
            // btn_backupcleanup
            // 
            this.btn_backupcleanup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_backupcleanup.Location = new System.Drawing.Point(523, 109);
            this.btn_backupcleanup.Name = "btn_backupcleanup";
            this.btn_backupcleanup.Size = new System.Drawing.Size(108, 32);
            this.btn_backupcleanup.TabIndex = 75;
            this.btn_backupcleanup.Text = "Cleanup";
            this.btn_backupcleanup.UseVisualStyleBackColor = true;
            this.btn_backupcleanup.Click += new System.EventHandler(this.btn_backupcleanup_Click);
            // 
            // chk_database
            // 
            this.chk_database.AutoSize = true;
            this.chk_database.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_database.Location = new System.Drawing.Point(156, 42);
            this.chk_database.Name = "chk_database";
            this.chk_database.Size = new System.Drawing.Size(46, 21);
            this.chk_database.TabIndex = 74;
            this.chk_database.Text = "DB";
            this.chk_database.UseVisualStyleBackColor = true;
            // 
            // btn_save_backuptype
            // 
            this.btn_save_backuptype.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save_backuptype.Location = new System.Drawing.Point(208, 34);
            this.btn_save_backuptype.Name = "btn_save_backuptype";
            this.btn_save_backuptype.Size = new System.Drawing.Size(90, 32);
            this.btn_save_backuptype.TabIndex = 73;
            this.btn_save_backuptype.Text = "Save";
            this.btn_save_backuptype.UseVisualStyleBackColor = true;
            this.btn_save_backuptype.Click += new System.EventHandler(this.btn_save_backuptype_Click);
            // 
            // chk_logs
            // 
            this.chk_logs.AutoSize = true;
            this.chk_logs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_logs.Location = new System.Drawing.Point(22, 42);
            this.chk_logs.Name = "chk_logs";
            this.chk_logs.Size = new System.Drawing.Size(58, 21);
            this.chk_logs.TabIndex = 72;
            this.chk_logs.Text = "Logs";
            this.chk_logs.UseVisualStyleBackColor = true;
            // 
            // chk_glogs
            // 
            this.chk_glogs.AutoSize = true;
            this.chk_glogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_glogs.Location = new System.Drawing.Point(86, 42);
            this.chk_glogs.Name = "chk_glogs";
            this.chk_glogs.Size = new System.Drawing.Size(64, 21);
            this.chk_glogs.TabIndex = 71;
            this.chk_glogs.Text = "Glogs";
            this.chk_glogs.UseVisualStyleBackColor = true;
            // 
            // num_retention
            // 
            this.num_retention.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.num_retention.Location = new System.Drawing.Point(13, 152);
            this.num_retention.Name = "num_retention";
            this.num_retention.Size = new System.Drawing.Size(91, 26);
            this.num_retention.TabIndex = 70;
            // 
            // btn_save_retentiondays
            // 
            this.btn_save_retentiondays.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save_retentiondays.Location = new System.Drawing.Point(109, 149);
            this.btn_save_retentiondays.Name = "btn_save_retentiondays";
            this.btn_save_retentiondays.Size = new System.Drawing.Size(120, 32);
            this.btn_save_retentiondays.TabIndex = 69;
            this.btn_save_retentiondays.Text = "Save";
            this.btn_save_retentiondays.UseVisualStyleBackColor = true;
            this.btn_save_retentiondays.Click += new System.EventHandler(this.btn_save_retentiondays_Click);
            // 
            // btn_save_drivecredentials
            // 
            this.btn_save_drivecredentials.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save_drivecredentials.Location = new System.Drawing.Point(543, 31);
            this.btn_save_drivecredentials.Name = "btn_save_drivecredentials";
            this.btn_save_drivecredentials.Size = new System.Drawing.Size(90, 32);
            this.btn_save_drivecredentials.TabIndex = 63;
            this.btn_save_drivecredentials.Text = "Save";
            this.btn_save_drivecredentials.UseVisualStyleBackColor = true;
            this.btn_save_drivecredentials.Click += new System.EventHandler(this.btn_save_drivecredentials_Click);
            // 
            // btn_save_drivefolderid
            // 
            this.btn_save_drivefolderid.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save_drivefolderid.Location = new System.Drawing.Point(208, 84);
            this.btn_save_drivefolderid.Name = "btn_save_drivefolderid";
            this.btn_save_drivefolderid.Size = new System.Drawing.Size(90, 32);
            this.btn_save_drivefolderid.TabIndex = 62;
            this.btn_save_drivefolderid.Text = "Save";
            this.btn_save_drivefolderid.UseVisualStyleBackColor = true;
            this.btn_save_drivefolderid.Click += new System.EventHandler(this.btn_save_drivefolderid_Click);
            // 
            // txt_drivecredentials
            // 
            this.txt_drivecredentials.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_drivecredentials.Location = new System.Drawing.Point(304, 34);
            this.txt_drivecredentials.Multiline = true;
            this.txt_drivecredentials.Name = "txt_drivecredentials";
            this.txt_drivecredentials.Size = new System.Drawing.Size(233, 90);
            this.txt_drivecredentials.TabIndex = 57;
            // 
            // txt_drivefolderid
            // 
            this.txt_drivefolderid.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_drivefolderid.Location = new System.Drawing.Point(13, 90);
            this.txt_drivefolderid.Name = "txt_drivefolderid";
            this.txt_drivefolderid.Size = new System.Drawing.Size(189, 26);
            this.txt_drivefolderid.TabIndex = 56;
            // 
            // btn_settime
            // 
            this.btn_settime.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_settime.Location = new System.Drawing.Point(523, 70);
            this.btn_settime.Name = "btn_settime";
            this.btn_settime.Size = new System.Drawing.Size(108, 32);
            this.btn_settime.TabIndex = 50;
            this.btn_settime.Text = "Set Time";
            this.btn_settime.UseVisualStyleBackColor = true;
            this.btn_settime.Click += new System.EventHandler(this.btn_settime_Click);
            // 
            // btn_getdb
            // 
            this.btn_getdb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_getdb.Location = new System.Drawing.Point(523, 154);
            this.btn_getdb.Name = "btn_getdb";
            this.btn_getdb.Size = new System.Drawing.Size(108, 32);
            this.btn_getdb.TabIndex = 47;
            this.btn_getdb.Text = "Manual Backup";
            this.btn_getdb.UseVisualStyleBackColor = true;
            this.btn_getdb.Click += new System.EventHandler(this.btn_getdb_Click);
            // 
            // txt_debounce
            // 
            this.txt_debounce.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_debounce.Location = new System.Drawing.Point(13, 92);
            this.txt_debounce.Name = "txt_debounce";
            this.txt_debounce.Size = new System.Drawing.Size(90, 26);
            this.txt_debounce.TabIndex = 44;
            // 
            // btn_savedebounce
            // 
            this.btn_savedebounce.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_savedebounce.Location = new System.Drawing.Point(109, 89);
            this.btn_savedebounce.Name = "btn_savedebounce";
            this.btn_savedebounce.Size = new System.Drawing.Size(120, 32);
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
            this.pnl_staffsManagment.Location = new System.Drawing.Point(892, 3);
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
            // 
            // btn_stafffilter
            // 
            this.btn_stafffilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_stafffilter.Location = new System.Drawing.Point(201, 7);
            this.btn_stafffilter.Name = "btn_stafffilter";
            this.btn_stafffilter.Size = new System.Drawing.Size(80, 28);
            this.btn_stafffilter.TabIndex = 91;
            this.btn_stafffilter.Text = "Filter";
            // 
            // lst_staffs
            // 
            this.lst_staffs.FullRowSelect = true;
            this.lst_staffs.GridLines = true;
            this.lst_staffs.HideSelection = false;
            this.lst_staffs.Location = new System.Drawing.Point(7, 41);
            this.lst_staffs.Name = "lst_staffs";
            this.lst_staffs.Size = new System.Drawing.Size(669, 405);
            this.lst_staffs.TabIndex = 92;
            this.lst_staffs.UseCompatibleStateImageBehavior = false;
            this.lst_staffs.View = System.Windows.Forms.View.Details;
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
            // 
            // date_staffattendence
            // 
            this.date_staffattendence.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.date_staffattendence.Location = new System.Drawing.Point(115, 9);
            this.date_staffattendence.Name = "date_staffattendence";
            this.date_staffattendence.Size = new System.Drawing.Size(140, 23);
            this.date_staffattendence.TabIndex = 83;
            // 
            // btn_filterattendencestaff
            // 
            this.btn_filterattendencestaff.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_filterattendencestaff.Location = new System.Drawing.Point(261, 6);
            this.btn_filterattendencestaff.Name = "btn_filterattendencestaff";
            this.btn_filterattendencestaff.Size = new System.Drawing.Size(80, 28);
            this.btn_filterattendencestaff.TabIndex = 85;
            this.btn_filterattendencestaff.Text = "Filter";
            // 
            // lst_staffattendence
            // 
            this.lst_staffattendence.FullRowSelect = true;
            this.lst_staffattendence.GridLines = true;
            this.lst_staffattendence.HideSelection = false;
            this.lst_staffattendence.Location = new System.Drawing.Point(9, 40);
            this.lst_staffattendence.Name = "lst_staffattendence";
            this.lst_staffattendence.Size = new System.Drawing.Size(669, 401);
            this.lst_staffattendence.TabIndex = 86;
            this.lst_staffattendence.UseCompatibleStateImageBehavior = false;
            this.lst_staffattendence.View = System.Windows.Forms.View.Details;
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
            // pnl_backup
            // 
            this.pnl_backup.Controls.Add(this.lbl_backuptype);
            this.pnl_backup.Controls.Add(this.txt_drivefolderid);
            this.pnl_backup.Controls.Add(this.txt_drivecredentials);
            this.pnl_backup.Controls.Add(this.btn_save_drivefolderid);
            this.pnl_backup.Controls.Add(this.btn_save_drivecredentials);
            this.pnl_backup.Controls.Add(this.lbl_drivefolderid);
            this.pnl_backup.Controls.Add(this.btn_test_connection);
            this.pnl_backup.Controls.Add(this.lbl_drivecredentials);
            this.pnl_backup.Controls.Add(this.chk_glogs);
            this.pnl_backup.Controls.Add(this.chk_database);
            this.pnl_backup.Controls.Add(this.chk_logs);
            this.pnl_backup.Controls.Add(this.btn_save_backuptype);
            this.pnl_backup.Location = new System.Drawing.Point(16, 276);
            this.pnl_backup.Name = "pnl_backup";
            this.pnl_backup.Size = new System.Drawing.Size(642, 146);
            this.pnl_backup.TabIndex = 87;
            // 
            // lbl_backuptype
            // 
            this.lbl_backuptype.AutoSize = true;
            this.lbl_backuptype.Location = new System.Drawing.Point(19, 18);
            this.lbl_backuptype.Name = "lbl_backuptype";
            this.lbl_backuptype.Size = new System.Drawing.Size(71, 13);
            this.lbl_backuptype.TabIndex = 87;
            this.lbl_backuptype.Text = "Backup Type";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btn_backupcleanup);
            this.panel1.Controls.Add(this.btn_settime);
            this.panel1.Controls.Add(this.lbl_apikey);
            this.panel1.Controls.Add(this.btn_getdb);
            this.panel1.Controls.Add(this.lbl_debounce);
            this.panel1.Controls.Add(this.btn_savedebounce);
            this.panel1.Controls.Add(this.lbl_retention);
            this.panel1.Controls.Add(this.txt_debounce);
            this.panel1.Controls.Add(this.txtAPIkey);
            this.panel1.Controls.Add(this.btn_save_retentiondays);
            this.panel1.Controls.Add(this.btnSaveAPI);
            this.panel1.Controls.Add(this.num_retention);
            this.panel1.Location = new System.Drawing.Point(16, 22);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(642, 232);
            this.panel1.TabIndex = 88;
            //
            // pnl_welcome_header
            //
            this.pnl_welcome_header.Controls.Add(this.lbl_welcome_title);
            this.pnl_welcome_header.Controls.Add(this.lbl_welcome_sub);
            this.pnl_welcome_header.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl_welcome_header.Height = 110;
            this.pnl_welcome_header.BackColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.pnl_welcome_header.Name = "pnl_welcome_header";
            this.pnl_welcome_header.TabIndex = 0;
            //
            // lbl_welcome_title
            //
            this.lbl_welcome_title.AutoSize = false;
            this.lbl_welcome_title.BackColor = System.Drawing.Color.Transparent;
            this.lbl_welcome_title.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lbl_welcome_title.ForeColor = System.Drawing.Color.White;
            this.lbl_welcome_title.Location = new System.Drawing.Point(30, 24);
            this.lbl_welcome_title.Name = "lbl_welcome_title";
            this.lbl_welcome_title.Size = new System.Drawing.Size(500, 42);
            this.lbl_welcome_title.TabIndex = 0;
            this.lbl_welcome_title.Text = "Smart Access Manager";
            this.lbl_welcome_title.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // lbl_welcome_sub
            //
            this.lbl_welcome_sub.AutoSize = false;
            this.lbl_welcome_sub.BackColor = System.Drawing.Color.Transparent;
            this.lbl_welcome_sub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_welcome_sub.ForeColor = System.Drawing.Color.FromArgb(180, 190, 220);
            this.lbl_welcome_sub.Location = new System.Drawing.Point(32, 70);
            this.lbl_welcome_sub.Name = "lbl_welcome_sub";
            this.lbl_welcome_sub.Size = new System.Drawing.Size(580, 22);
            this.lbl_welcome_sub.TabIndex = 1;
            this.lbl_welcome_sub.Text = "Manage your biometric devices, users, staff, members, and system settings — all in one place.";
            this.lbl_welcome_sub.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // tile_device
            //
            this.tile_device.BackColor = System.Drawing.Color.White;
            this.tile_device.Controls.Add(this.lbl_tile_device_icon);
            this.tile_device.Controls.Add(this.lbl_tile_device_name);
            this.tile_device.Controls.Add(this.lbl_tile_device_desc);
            this.tile_device.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_device.Margin = new System.Windows.Forms.Padding(0, 0, 18, 18);
            this.tile_device.Name = "tile_device";
            this.tile_device.Size = new System.Drawing.Size(180, 160);
            this.tile_device.TabIndex = 0;
            this.tile_device.Click += new System.EventHandler(this.btn_device_management_Click);
            //
            // lbl_tile_device_icon
            //
            this.lbl_tile_device_icon.AutoSize = false;
            this.lbl_tile_device_icon.Font = new System.Drawing.Font("Segoe UI Emoji", 26F);
            this.lbl_tile_device_icon.ForeColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.lbl_tile_device_icon.Location = new System.Drawing.Point(0, 18);
            this.lbl_tile_device_icon.Name = "lbl_tile_device_icon";
            this.lbl_tile_device_icon.Size = new System.Drawing.Size(180, 52);
            this.lbl_tile_device_icon.TabIndex = 0;
            this.lbl_tile_device_icon.Text = "🖥";
            this.lbl_tile_device_icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_device_icon.Click += new System.EventHandler(this.btn_device_management_Click);
            //
            // lbl_tile_device_name
            //
            this.lbl_tile_device_name.AutoSize = false;
            this.lbl_tile_device_name.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbl_tile_device_name.ForeColor = System.Drawing.Color.FromArgb(30, 35, 60);
            this.lbl_tile_device_name.Location = new System.Drawing.Point(10, 74);
            this.lbl_tile_device_name.Name = "lbl_tile_device_name";
            this.lbl_tile_device_name.Size = new System.Drawing.Size(160, 22);
            this.lbl_tile_device_name.TabIndex = 1;
            this.lbl_tile_device_name.Text = "Device Management";
            this.lbl_tile_device_name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_device_name.Click += new System.EventHandler(this.btn_device_management_Click);
            //
            // lbl_tile_device_desc
            //
            this.lbl_tile_device_desc.AutoSize = false;
            this.lbl_tile_device_desc.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lbl_tile_device_desc.ForeColor = System.Drawing.Color.FromArgb(120, 125, 145);
            this.lbl_tile_device_desc.Location = new System.Drawing.Point(10, 98);
            this.lbl_tile_device_desc.Name = "lbl_tile_device_desc";
            this.lbl_tile_device_desc.Size = new System.Drawing.Size(160, 38);
            this.lbl_tile_device_desc.TabIndex = 2;
            this.lbl_tile_device_desc.Text = "Add, edit and monitor\nbiometric devices";
            this.lbl_tile_device_desc.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbl_tile_device_desc.Click += new System.EventHandler(this.btn_device_management_Click);
            //
            // tile_user
            //
            this.tile_user.BackColor = System.Drawing.Color.White;
            this.tile_user.Controls.Add(this.lbl_tile_user_icon);
            this.tile_user.Controls.Add(this.lbl_tile_user_name);
            this.tile_user.Controls.Add(this.lbl_tile_user_desc);
            this.tile_user.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_user.Margin = new System.Windows.Forms.Padding(0, 0, 18, 18);
            this.tile_user.Name = "tile_user";
            this.tile_user.Size = new System.Drawing.Size(180, 160);
            this.tile_user.TabIndex = 1;
            this.tile_user.Click += new System.EventHandler(this.btn_userManagement_Click);
            //
            // lbl_tile_user_icon
            //
            this.lbl_tile_user_icon.AutoSize = false;
            this.lbl_tile_user_icon.Font = new System.Drawing.Font("Segoe UI Emoji", 26F);
            this.lbl_tile_user_icon.ForeColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.lbl_tile_user_icon.Location = new System.Drawing.Point(0, 18);
            this.lbl_tile_user_icon.Name = "lbl_tile_user_icon";
            this.lbl_tile_user_icon.Size = new System.Drawing.Size(180, 52);
            this.lbl_tile_user_icon.TabIndex = 0;
            this.lbl_tile_user_icon.Text = "👤";
            this.lbl_tile_user_icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_user_icon.Click += new System.EventHandler(this.btn_userManagement_Click);
            //
            // lbl_tile_user_name
            //
            this.lbl_tile_user_name.AutoSize = false;
            this.lbl_tile_user_name.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbl_tile_user_name.ForeColor = System.Drawing.Color.FromArgb(30, 35, 60);
            this.lbl_tile_user_name.Location = new System.Drawing.Point(10, 74);
            this.lbl_tile_user_name.Name = "lbl_tile_user_name";
            this.lbl_tile_user_name.Size = new System.Drawing.Size(160, 22);
            this.lbl_tile_user_name.TabIndex = 1;
            this.lbl_tile_user_name.Text = "User Management";
            this.lbl_tile_user_name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_user_name.Click += new System.EventHandler(this.btn_userManagement_Click);
            //
            // lbl_tile_user_desc
            //
            this.lbl_tile_user_desc.AutoSize = false;
            this.lbl_tile_user_desc.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lbl_tile_user_desc.ForeColor = System.Drawing.Color.FromArgb(120, 125, 145);
            this.lbl_tile_user_desc.Location = new System.Drawing.Point(10, 98);
            this.lbl_tile_user_desc.Name = "lbl_tile_user_desc";
            this.lbl_tile_user_desc.Size = new System.Drawing.Size(160, 38);
            this.lbl_tile_user_desc.TabIndex = 2;
            this.lbl_tile_user_desc.Text = "Manage system users\nand their permissions";
            this.lbl_tile_user_desc.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbl_tile_user_desc.Click += new System.EventHandler(this.btn_userManagement_Click);
            //
            // tile_staff
            //
            this.tile_staff.BackColor = System.Drawing.Color.White;
            this.tile_staff.Controls.Add(this.lbl_tile_staff_icon);
            this.tile_staff.Controls.Add(this.lbl_tile_staff_name);
            this.tile_staff.Controls.Add(this.lbl_tile_staff_desc);
            this.tile_staff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_staff.Margin = new System.Windows.Forms.Padding(0, 0, 18, 18);
            this.tile_staff.Name = "tile_staff";
            this.tile_staff.Size = new System.Drawing.Size(180, 160);
            this.tile_staff.TabIndex = 2;
            this.tile_staff.Click += new System.EventHandler(this.btn_attendance_Click);
            //
            // lbl_tile_staff_icon
            //
            this.lbl_tile_staff_icon.AutoSize = false;
            this.lbl_tile_staff_icon.Font = new System.Drawing.Font("Segoe UI Emoji", 26F);
            this.lbl_tile_staff_icon.ForeColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.lbl_tile_staff_icon.Location = new System.Drawing.Point(0, 18);
            this.lbl_tile_staff_icon.Name = "lbl_tile_staff_icon";
            this.lbl_tile_staff_icon.Size = new System.Drawing.Size(180, 52);
            this.lbl_tile_staff_icon.TabIndex = 0;
            this.lbl_tile_staff_icon.Text = "👔";
            this.lbl_tile_staff_icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_staff_icon.Click += new System.EventHandler(this.btn_attendance_Click);
            //
            // lbl_tile_staff_name
            //
            this.lbl_tile_staff_name.AutoSize = false;
            this.lbl_tile_staff_name.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbl_tile_staff_name.ForeColor = System.Drawing.Color.FromArgb(30, 35, 60);
            this.lbl_tile_staff_name.Location = new System.Drawing.Point(10, 74);
            this.lbl_tile_staff_name.Name = "lbl_tile_staff_name";
            this.lbl_tile_staff_name.Size = new System.Drawing.Size(160, 22);
            this.lbl_tile_staff_name.TabIndex = 1;
            this.lbl_tile_staff_name.Text = "Staff Management";
            this.lbl_tile_staff_name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_staff_name.Click += new System.EventHandler(this.btn_attendance_Click);
            //
            // lbl_tile_staff_desc
            //
            this.lbl_tile_staff_desc.AutoSize = false;
            this.lbl_tile_staff_desc.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lbl_tile_staff_desc.ForeColor = System.Drawing.Color.FromArgb(120, 125, 145);
            this.lbl_tile_staff_desc.Location = new System.Drawing.Point(10, 98);
            this.lbl_tile_staff_desc.Name = "lbl_tile_staff_desc";
            this.lbl_tile_staff_desc.Size = new System.Drawing.Size(160, 38);
            this.lbl_tile_staff_desc.TabIndex = 2;
            this.lbl_tile_staff_desc.Text = "View and manage\nstaff attendance";
            this.lbl_tile_staff_desc.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbl_tile_staff_desc.Click += new System.EventHandler(this.btn_attendance_Click);
            //
            // tile_member
            //
            this.tile_member.BackColor = System.Drawing.Color.White;
            this.tile_member.Controls.Add(this.lbl_tile_member_icon);
            this.tile_member.Controls.Add(this.lbl_tile_member_name);
            this.tile_member.Controls.Add(this.lbl_tile_member_desc);
            this.tile_member.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_member.Margin = new System.Windows.Forms.Padding(0, 0, 18, 18);
            this.tile_member.Name = "tile_member";
            this.tile_member.Size = new System.Drawing.Size(180, 160);
            this.tile_member.TabIndex = 3;
            this.tile_member.Click += new System.EventHandler(this.btn_attendencemanagment_Click);
            //
            // lbl_tile_member_icon
            //
            this.lbl_tile_member_icon.AutoSize = false;
            this.lbl_tile_member_icon.Font = new System.Drawing.Font("Segoe UI Emoji", 26F);
            this.lbl_tile_member_icon.ForeColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.lbl_tile_member_icon.Location = new System.Drawing.Point(0, 18);
            this.lbl_tile_member_icon.Name = "lbl_tile_member_icon";
            this.lbl_tile_member_icon.Size = new System.Drawing.Size(180, 52);
            this.lbl_tile_member_icon.TabIndex = 0;
            this.lbl_tile_member_icon.Text = "🏋";
            this.lbl_tile_member_icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_member_icon.Click += new System.EventHandler(this.btn_attendencemanagment_Click);
            //
            // lbl_tile_member_name
            //
            this.lbl_tile_member_name.AutoSize = false;
            this.lbl_tile_member_name.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbl_tile_member_name.ForeColor = System.Drawing.Color.FromArgb(30, 35, 60);
            this.lbl_tile_member_name.Location = new System.Drawing.Point(10, 74);
            this.lbl_tile_member_name.Name = "lbl_tile_member_name";
            this.lbl_tile_member_name.Size = new System.Drawing.Size(160, 22);
            this.lbl_tile_member_name.TabIndex = 1;
            this.lbl_tile_member_name.Text = "Member Management";
            this.lbl_tile_member_name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_member_name.Click += new System.EventHandler(this.btn_attendencemanagment_Click);
            //
            // lbl_tile_member_desc
            //
            this.lbl_tile_member_desc.AutoSize = false;
            this.lbl_tile_member_desc.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lbl_tile_member_desc.ForeColor = System.Drawing.Color.FromArgb(120, 125, 145);
            this.lbl_tile_member_desc.Location = new System.Drawing.Point(10, 98);
            this.lbl_tile_member_desc.Name = "lbl_tile_member_desc";
            this.lbl_tile_member_desc.Size = new System.Drawing.Size(160, 38);
            this.lbl_tile_member_desc.TabIndex = 2;
            this.lbl_tile_member_desc.Text = "Track member records\nand attendance logs";
            this.lbl_tile_member_desc.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbl_tile_member_desc.Click += new System.EventHandler(this.btn_attendencemanagment_Click);
            //
            // tile_setup
            //
            this.tile_setup.BackColor = System.Drawing.Color.White;
            this.tile_setup.Controls.Add(this.lbl_tile_setup_icon);
            this.tile_setup.Controls.Add(this.lbl_tile_setup_name);
            this.tile_setup.Controls.Add(this.lbl_tile_setup_desc);
            this.tile_setup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_setup.Margin = new System.Windows.Forms.Padding(0, 0, 18, 18);
            this.tile_setup.Name = "tile_setup";
            this.tile_setup.Size = new System.Drawing.Size(180, 160);
            this.tile_setup.TabIndex = 4;
            this.tile_setup.Click += new System.EventHandler(this.btn_setupmanage_Click);
            //
            // lbl_tile_setup_icon
            //
            this.lbl_tile_setup_icon.AutoSize = false;
            this.lbl_tile_setup_icon.Font = new System.Drawing.Font("Segoe UI Emoji", 26F);
            this.lbl_tile_setup_icon.ForeColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.lbl_tile_setup_icon.Location = new System.Drawing.Point(0, 18);
            this.lbl_tile_setup_icon.Name = "lbl_tile_setup_icon";
            this.lbl_tile_setup_icon.Size = new System.Drawing.Size(180, 52);
            this.lbl_tile_setup_icon.TabIndex = 0;
            this.lbl_tile_setup_icon.Text = "⚙";
            this.lbl_tile_setup_icon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_setup_icon.Click += new System.EventHandler(this.btn_setupmanage_Click);
            //
            // lbl_tile_setup_name
            //
            this.lbl_tile_setup_name.AutoSize = false;
            this.lbl_tile_setup_name.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbl_tile_setup_name.ForeColor = System.Drawing.Color.FromArgb(30, 35, 60);
            this.lbl_tile_setup_name.Location = new System.Drawing.Point(10, 74);
            this.lbl_tile_setup_name.Name = "lbl_tile_setup_name";
            this.lbl_tile_setup_name.Size = new System.Drawing.Size(160, 22);
            this.lbl_tile_setup_name.TabIndex = 1;
            this.lbl_tile_setup_name.Text = "Setup Management";
            this.lbl_tile_setup_name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_tile_setup_name.Click += new System.EventHandler(this.btn_setupmanage_Click);
            //
            // lbl_tile_setup_desc
            //
            this.lbl_tile_setup_desc.AutoSize = false;
            this.lbl_tile_setup_desc.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lbl_tile_setup_desc.ForeColor = System.Drawing.Color.FromArgb(120, 125, 145);
            this.lbl_tile_setup_desc.Location = new System.Drawing.Point(10, 98);
            this.lbl_tile_setup_desc.Name = "lbl_tile_setup_desc";
            this.lbl_tile_setup_desc.Size = new System.Drawing.Size(160, 38);
            this.lbl_tile_setup_desc.TabIndex = 2;
            this.lbl_tile_setup_desc.Text = "Configure API keys,\nbackups, and settings";
            this.lbl_tile_setup_desc.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lbl_tile_setup_desc.Click += new System.EventHandler(this.btn_setupmanage_Click);
            //
            // pnl_welcome_tiles
            //
            this.pnl_welcome_tiles.Controls.Add(this.tile_device);
            this.pnl_welcome_tiles.Controls.Add(this.tile_user);
            this.pnl_welcome_tiles.Controls.Add(this.tile_staff);
            this.pnl_welcome_tiles.Controls.Add(this.tile_member);
            this.pnl_welcome_tiles.Controls.Add(this.tile_setup);
            this.pnl_welcome_tiles.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pnl_welcome_tiles.WrapContents = true;
            this.pnl_welcome_tiles.Location = new System.Drawing.Point(0, 110);
            this.pnl_welcome_tiles.Name = "pnl_welcome_tiles";
            this.pnl_welcome_tiles.Padding = new System.Windows.Forms.Padding(30, 20, 10, 10);
            this.pnl_welcome_tiles.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.pnl_welcome_tiles.Size = new System.Drawing.Size(697, 462);
            this.pnl_welcome_tiles.TabIndex = 1;
            //
            // pnl_welcome
            //
            this.pnl_welcome.Controls.Add(this.pnl_welcome_header);
            this.pnl_welcome.Controls.Add(this.pnl_welcome_tiles);
            this.pnl_welcome.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.pnl_welcome.Location = new System.Drawing.Point(186, 0);
            this.pnl_welcome.Name = "pnl_welcome";
            this.pnl_welcome.Size = new System.Drawing.Size(697, 572);
            this.pnl_welcome.TabIndex = 200;
            this.pnl_welcome.Visible = false;
            //
            // DeviceManager
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(883, 572);
            this.Controls.Add(this.pnl_welcome);
            this.Controls.Add(this.pnl_staffsManagment);
            this.Controls.Add(this.pnl_setup);
            this.Controls.Add(this.pnl_attendence);
            this.Controls.Add(this.pnl_UserManagement);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.pnl_deviceManager);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DeviceManager";
            this.Text = "Device Manager";
            this.Load += new System.EventHandler(this.DeviceManager_Load);
            this.pnl_welcome.ResumeLayout(false);
            this.pnl_welcome_header.ResumeLayout(false);
            this.pnl_welcome_tiles.ResumeLayout(false);
            this.tile_device.ResumeLayout(false);
            this.tile_user.ResumeLayout(false);
            this.tile_staff.ResumeLayout(false);
            this.tile_member.ResumeLayout(false);
            this.tile_setup.ResumeLayout(false);
            this.pnl_deviceManager.ResumeLayout(false);
            this.pnl_deviceManager.PerformLayout();
            this.pnl_device.ResumeLayout(false);
            this.pnl_device.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
            ((System.ComponentModel.ISupportInitialize)(this.num_retention)).EndInit();
            this.pnl_staffsManagment.ResumeLayout(false);
            this.pnl_staffsManagment.PerformLayout();
            this.tab_staff.ResumeLayout(false);
            this.tab_staffmembers.ResumeLayout(false);
            this.tab_staffmembers.PerformLayout();
            this.tab_staffattendence.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.pnl_backup.ResumeLayout(false);
            this.pnl_backup.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.TextBox txtAPIkey;
        private System.Windows.Forms.Button btnSaveAPI;
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
        private Panel pnl_setup;
        private Panel subpnl_setup;
        private TextBox txt_debounce;
        private Button btn_savedebounce;
        private Label label42;
        private Label label43;
        private Label btn_setupmanage;
        private Button btn_getdb;
        private Button btn_settime;
        private TabPage tab_members;
        private TextBox txt_memb_name;
        private Button flt_members;
        private ListView lst_members;
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
        private Button btn_save_drivefolderid;
        private TextBox txt_drivecredentials;
        private TextBox txt_drivefolderid;
        private Button btn_save_drivecredentials;
        private CheckBox chk_database;
        private Button btn_save_backuptype;
        private CheckBox chk_logs;
        private CheckBox chk_glogs;
        private NumericUpDown num_retention;
        private Button btn_save_retentiondays;
        private Button btn_backupcleanup;
        private Button btn_test_connection;
        private System.Windows.Forms.Label lbl_debounce;
        private System.Windows.Forms.Label lbl_retention;
        private System.Windows.Forms.Label lbl_drivefolderid;
        private System.Windows.Forms.Label lbl_drivecredentials;
        private System.Windows.Forms.Label lbl_apikey;
        private Panel pnl_backup;
        private Label lbl_backuptype;
        private Panel panel1;
        private System.Windows.Forms.Panel pnl_welcome;
        private System.Windows.Forms.Panel pnl_welcome_header;
        private System.Windows.Forms.Label lbl_welcome_title;
        private System.Windows.Forms.Label lbl_welcome_sub;
        private System.Windows.Forms.FlowLayoutPanel pnl_welcome_tiles;
        private System.Windows.Forms.Panel tile_device;
        private System.Windows.Forms.Label lbl_tile_device_icon;
        private System.Windows.Forms.Label lbl_tile_device_name;
        private System.Windows.Forms.Label lbl_tile_device_desc;
        private System.Windows.Forms.Panel tile_user;
        private System.Windows.Forms.Label lbl_tile_user_icon;
        private System.Windows.Forms.Label lbl_tile_user_name;
        private System.Windows.Forms.Label lbl_tile_user_desc;
        private System.Windows.Forms.Panel tile_staff;
        private System.Windows.Forms.Label lbl_tile_staff_icon;
        private System.Windows.Forms.Label lbl_tile_staff_name;
        private System.Windows.Forms.Label lbl_tile_staff_desc;
        private System.Windows.Forms.Panel tile_member;
        private System.Windows.Forms.Label lbl_tile_member_icon;
        private System.Windows.Forms.Label lbl_tile_member_name;
        private System.Windows.Forms.Label lbl_tile_member_desc;
        private System.Windows.Forms.Panel tile_setup;
        private System.Windows.Forms.Label lbl_tile_setup_icon;
        private System.Windows.Forms.Label lbl_tile_setup_name;
        private System.Windows.Forms.Label lbl_tile_setup_desc;
    }
}