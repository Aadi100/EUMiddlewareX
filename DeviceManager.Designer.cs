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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeviceManager));
            this.lbl_debounce = new System.Windows.Forms.Label();
            this.lbl_retention = new System.Windows.Forms.Label();
            this.lbl_drivefolderid = new System.Windows.Forms.Label();
            this.lbl_drivecredentials = new System.Windows.Forms.Label();
            this.lbl_apikey = new System.Windows.Forms.Label();
            this.pnl_welcome = new System.Windows.Forms.Panel();
            this.pnl_welcome_tiles = new System.Windows.Forms.Panel();
            this.pnl_welcome_header = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lbl_welcome_title = new System.Windows.Forms.Label();
            this.lbl_welcome_sub = new System.Windows.Forms.Label();
            this.pnl_topnav = new System.Windows.Forms.Panel();
            this.nav_device = new System.Windows.Forms.Label();
            this.nav_user = new System.Windows.Forms.Label();
            this.nav_staff = new System.Windows.Forms.Label();
            this.nav_member = new System.Windows.Forms.Label();
            this.nav_setup = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_setupmanage = new System.Windows.Forms.Label();
            this.btn_attendencemanagment = new System.Windows.Forms.Label();
            this.btn_staffmanagment = new System.Windows.Forms.Label();
            this.btn_userManagement = new System.Windows.Forms.Label();
            this.btn_device_management = new System.Windows.Forms.Label();
            this.pnl_UserManagement = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pnl_attendence = new System.Windows.Forms.Panel();
            this.tab_attendence = new System.Windows.Forms.TabControl();
            this.tab_members = new System.Windows.Forms.TabPage();
            this.lbl_hasFailureOnly = new System.Windows.Forms.Label();
            this.tab_memberAttendence = new System.Windows.Forms.TabPage();
            this.panel5 = new System.Windows.Forms.Panel();
            this.date_member = new System.Windows.Forms.DateTimePicker();
            this.tab_manualdoor = new System.Windows.Forms.TabPage();
            this.panel6 = new System.Windows.Forms.Panel();
            this.date_manualdoor = new System.Windows.Forms.DateTimePicker();
            this.tab_pendingjobs = new System.Windows.Forms.TabPage();
            this.lbl_pendingjobs_info = new System.Windows.Forms.Label();
            this.tab_staffpendingjobs = new System.Windows.Forms.TabPage();
            this.lbl_staffpendingjobs_info = new System.Windows.Forms.Label();
            this.lbl_chkStaffFailure = new System.Windows.Forms.Label();
            this.pnl_setup = new System.Windows.Forms.Panel();
            this.card_api = new System.Windows.Forms.Panel();
            this.lbl_card_api_title = new System.Windows.Forms.Label();
            this.lbl_card_api_sub = new System.Windows.Forms.Label();
            this.card_sync = new System.Windows.Forms.Panel();
            this.lbl_card_sync_title = new System.Windows.Forms.Label();
            this.lbl_card_sync_sub = new System.Windows.Forms.Label();
            this.lbl_card_sync_retentionlabel = new System.Windows.Forms.Label();
            this.lbl_card_sync_retentionfooter = new System.Windows.Forms.Label();
            this.card_backup = new System.Windows.Forms.Panel();
            this.lbl_card_backup_title = new System.Windows.Forms.Label();
            this.lbl_card_backup_sub = new System.Windows.Forms.Label();
            this.lbl_backupConnectionStatus = new System.Windows.Forms.Label();
            this.lbl_card_backup_type_label = new System.Windows.Forms.Label();
            this.chk_logs = new System.Windows.Forms.CheckBox();
            this.chk_glogs = new System.Windows.Forms.CheckBox();
            this.chk_database = new System.Windows.Forms.CheckBox();
            this.lbl_card_backup_drivefolder_label = new System.Windows.Forms.Label();
            this.lbl_card_backup_creds_label = new System.Windows.Forms.Label();
            this.card_maint = new System.Windows.Forms.Panel();
            this.lbl_card_maint_title = new System.Windows.Forms.Label();
            this.lbl_card_maint_sub = new System.Windows.Forms.Label();
            this.lbl_maint_cleanup_desc = new System.Windows.Forms.Label();
            this.lbl_maint_settime_desc = new System.Windows.Forms.Label();
            this.lbl_maint_getdb_desc = new System.Windows.Forms.Label();
            this.card_gate = new System.Windows.Forms.Panel();
            this.lbl_card_gate_title = new System.Windows.Forms.Label();
            this.lbl_card_gate_sub = new System.Windows.Forms.Label();
            this.lbl_gate_entrylabel = new System.Windows.Forms.Label();
            this.lbl_gate_entrydesc = new System.Windows.Forms.Label();
            this.lbl_gate_passwordlabel = new System.Windows.Forms.Label();
            this.lbl_gate_passworddesc = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.num_retention = new System.Windows.Forms.NumericUpDown();
            this.pnl_backup = new System.Windows.Forms.Panel();
            this.lbl_backuptype = new System.Windows.Forms.Label();
            this.pnl_staffsManagment = new System.Windows.Forms.Panel();
            this.tab_staff = new System.Windows.Forms.TabControl();
            this.tab_staffmembers = new System.Windows.Forms.TabPage();
            this.tab_staffattendence = new System.Windows.Forms.TabPage();
            this.panel8 = new System.Windows.Forms.Panel();
            this.date_staffattendence = new System.Windows.Forms.DateTimePicker();
            this.pnl_device = new System.Windows.Forms.Panel();
            this.lbl_staffDevicesOnly = new System.Windows.Forms.Label();
            this.pnl_deviceManager = new System.Windows.Forms.Panel();
            this.tile_device = new smart_attendance_middleware.Controls.TileCard();
            this.tile_user = new smart_attendance_middleware.Controls.TileCard();
            this.tile_staff = new smart_attendance_middleware.Controls.TileCard();
            this.tile_member = new smart_attendance_middleware.Controls.TileCard();
            this.tile_setup = new smart_attendance_middleware.Controls.TileCard();
            this.rtb_APIkey = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.rb_SaveAPI = new smart_attendance_middleware.Controls.RoundedButton();
            this.rtb_debounce = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.rcmb_retention = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.rb_btn_sync_save = new smart_attendance_middleware.Controls.RoundedButton();
            this.rtb_drivefolderid = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.rtb_drivecredentials = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.rb_test_connection = new smart_attendance_middleware.Controls.RoundedButton();
            this.rb_btn_savebackup = new smart_attendance_middleware.Controls.RoundedButton();
            this.rb_backupcleanup = new smart_attendance_middleware.Controls.RoundedButton();
            this.rb_settime = new smart_attendance_middleware.Controls.RoundedButton();
            this.rb_getdb = new smart_attendance_middleware.Controls.RoundedButton();
            this.chk_requireGateEntryForm = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.chk_requireGatePassword = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.rb_btn_gate_save = new smart_attendance_middleware.Controls.RoundedButton();
            this.rtb_memb_name = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.rb_flt_members = new smart_attendance_middleware.Controls.RoundedButton();
            this.cmbMemberSync = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.chkMemberFailure = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.clv_members = new smart_attendance_middleware.Controls.CardListView();
            this.rtb_membername = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.rb_filter_member = new smart_attendance_middleware.Controls.RoundedButton();
            this.clv_memberAttendance = new smart_attendance_middleware.Controls.CardListView();
            this.rb_filter_manualdoor = new smart_attendance_middleware.Controls.RoundedButton();
            this.cmbDoorVisitor = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.clv_manualDoor = new smart_attendance_middleware.Controls.CardListView();
            this.clv_pendingJobs = new smart_attendance_middleware.Controls.CardListView();
            this.cmbPendingDevice = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.cmbPendingFor = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.rtb_staff = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.rb_stafffilter = new smart_attendance_middleware.Controls.RoundedButton();
            this.cmbStaffStatus = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.cmbStaffSync = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.chkStaffFailure = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.clv_staffs = new smart_attendance_middleware.Controls.CardListView();
            this.rtb_staffname = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.rb_filterattendencestaff = new smart_attendance_middleware.Controls.RoundedButton();
            this.clv_staffAttendance = new smart_attendance_middleware.Controls.CardListView();
            this.clv_staffPendingJobs = new smart_attendance_middleware.Controls.CardListView();
            this.cmbStaffPendingDevice = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.cmbStaffPendingFor = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.rb_add_device = new smart_attendance_middleware.Controls.RoundedButton();
            this.chkDeviceIsStaff = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.clv_devices = new smart_attendance_middleware.Controls.CardListView();
            this.rb_addUser = new smart_attendance_middleware.Controls.RoundedButton();
            this.cmbUserRole = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.clv_localUsers = new smart_attendance_middleware.Controls.CardListView();
            this.rb_savedebounce = new smart_attendance_middleware.Controls.RoundedButton();
            this.rb_save_retentiondays = new smart_attendance_middleware.Controls.RoundedButton();
            this.rb_save_drivefolderid = new smart_attendance_middleware.Controls.RoundedButton();
            this.rb_save_drivecredentials = new smart_attendance_middleware.Controls.RoundedButton();
            this.rb_save_backuptype = new smart_attendance_middleware.Controls.RoundedButton();
            this.pnl_welcome.SuspendLayout();
            this.pnl_welcome_tiles.SuspendLayout();
            this.pnl_welcome_header.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnl_topnav.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnl_UserManagement.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pnl_attendence.SuspendLayout();
            this.tab_attendence.SuspendLayout();
            this.tab_members.SuspendLayout();
            this.tab_memberAttendence.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tab_manualdoor.SuspendLayout();
            this.panel6.SuspendLayout();
            this.tab_pendingjobs.SuspendLayout();
            this.tab_staffpendingjobs.SuspendLayout();
            this.pnl_setup.SuspendLayout();
            this.card_api.SuspendLayout();
            this.card_sync.SuspendLayout();
            this.card_backup.SuspendLayout();
            this.card_maint.SuspendLayout();
            this.card_gate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_retention)).BeginInit();
            this.pnl_staffsManagment.SuspendLayout();
            this.tab_staff.SuspendLayout();
            this.tab_staffmembers.SuspendLayout();
            this.tab_staffattendence.SuspendLayout();
            this.panel8.SuspendLayout();
            this.pnl_device.SuspendLayout();
            this.pnl_deviceManager.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_debounce
            // 
            this.lbl_debounce.AutoSize = true;
            this.lbl_debounce.Location = new System.Drawing.Point(16, 54);
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
            // pnl_welcome
            // 
            this.pnl_welcome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnl_welcome.Controls.Add(this.pnl_welcome_tiles);
            this.pnl_welcome.Location = new System.Drawing.Point(0, 110);
            this.pnl_welcome.Name = "pnl_welcome";
            this.pnl_welcome.Size = new System.Drawing.Size(881, 614);
            this.pnl_welcome.TabIndex = 200;
            // 
            // pnl_welcome_tiles
            // 
            this.pnl_welcome_tiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnl_welcome_tiles.Controls.Add(this.tile_device);
            this.pnl_welcome_tiles.Controls.Add(this.tile_user);
            this.pnl_welcome_tiles.Controls.Add(this.tile_staff);
            this.pnl_welcome_tiles.Controls.Add(this.tile_member);
            this.pnl_welcome_tiles.Controls.Add(this.tile_setup);
            this.pnl_welcome_tiles.Location = new System.Drawing.Point(0, 0);
            this.pnl_welcome_tiles.Name = "pnl_welcome_tiles";
            this.pnl_welcome_tiles.Size = new System.Drawing.Size(881, 614);
            this.pnl_welcome_tiles.TabIndex = 1;
            // 
            // pnl_welcome_header
            // 
            this.pnl_welcome_header.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(23)))), ((int)(((byte)(49)))));
            this.pnl_welcome_header.Controls.Add(this.pictureBox1);
            this.pnl_welcome_header.Controls.Add(this.lbl_welcome_title);
            this.pnl_welcome_header.Controls.Add(this.lbl_welcome_sub);
            this.pnl_welcome_header.Location = new System.Drawing.Point(0, 0);
            this.pnl_welcome_header.Name = "pnl_welcome_header";
            this.pnl_welcome_header.Size = new System.Drawing.Size(881, 110);
            this.pnl_welcome_header.TabIndex = 300;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::smart_attendance_middleware.Properties.Resources.icon_facede_tection;
            this.pictureBox1.Location = new System.Drawing.Point(20, 15);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(79, 79);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // lbl_welcome_title
            // 
            this.lbl_welcome_title.BackColor = System.Drawing.Color.Transparent;
            this.lbl_welcome_title.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lbl_welcome_title.ForeColor = System.Drawing.Color.White;
            this.lbl_welcome_title.Location = new System.Drawing.Point(120, 24);
            this.lbl_welcome_title.Name = "lbl_welcome_title";
            this.lbl_welcome_title.Size = new System.Drawing.Size(500, 42);
            this.lbl_welcome_title.TabIndex = 0;
            this.lbl_welcome_title.Text = "Smart Access Manager";
            this.lbl_welcome_title.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lbl_welcome_sub
            // 
            this.lbl_welcome_sub.BackColor = System.Drawing.Color.Transparent;
            this.lbl_welcome_sub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_welcome_sub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(190)))), ((int)(((byte)(220)))));
            this.lbl_welcome_sub.Location = new System.Drawing.Point(122, 70);
            this.lbl_welcome_sub.Name = "lbl_welcome_sub";
            this.lbl_welcome_sub.Size = new System.Drawing.Size(580, 22);
            this.lbl_welcome_sub.TabIndex = 1;
            this.lbl_welcome_sub.Text = "Manage your biometric devices, users, staff, members, and system settings — all i" +
    "n one place.";
            // 
            // pnl_topnav
            // 
            this.pnl_topnav.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(23)))), ((int)(((byte)(49)))));
            this.pnl_topnav.Controls.Add(this.nav_device);
            this.pnl_topnav.Controls.Add(this.nav_user);
            this.pnl_topnav.Controls.Add(this.nav_staff);
            this.pnl_topnav.Controls.Add(this.nav_member);
            this.pnl_topnav.Controls.Add(this.nav_setup);
            this.pnl_topnav.Location = new System.Drawing.Point(0, 110);
            this.pnl_topnav.Name = "pnl_topnav";
            this.pnl_topnav.Size = new System.Drawing.Size(881, 46);
            this.pnl_topnav.TabIndex = 2;
            // 
            // nav_device
            // 
            this.nav_device.BackColor = System.Drawing.Color.Transparent;
            this.nav_device.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nav_device.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nav_device.ForeColor = System.Drawing.Color.White;
            this.nav_device.Location = new System.Drawing.Point(0, 0);
            this.nav_device.Name = "nav_device";
            this.nav_device.Size = new System.Drawing.Size(176, 46);
            this.nav_device.TabIndex = 0;
            this.nav_device.Text = "Device Management";
            this.nav_device.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.nav_device.Click += new System.EventHandler(this.btn_device_management_Click);
            this.nav_device.MouseEnter += new System.EventHandler(this.NavBtn_MouseEnter);
            this.nav_device.MouseLeave += new System.EventHandler(this.NavBtn_MouseLeave);
            // 
            // nav_user
            // 
            this.nav_user.BackColor = System.Drawing.Color.Transparent;
            this.nav_user.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nav_user.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nav_user.ForeColor = System.Drawing.Color.White;
            this.nav_user.Location = new System.Drawing.Point(176, 0);
            this.nav_user.Name = "nav_user";
            this.nav_user.Size = new System.Drawing.Size(176, 46);
            this.nav_user.TabIndex = 1;
            this.nav_user.Text = "User Management";
            this.nav_user.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.nav_user.Click += new System.EventHandler(this.btn_userManagement_Click);
            this.nav_user.MouseEnter += new System.EventHandler(this.NavBtn_MouseEnter);
            this.nav_user.MouseLeave += new System.EventHandler(this.NavBtn_MouseLeave);
            // 
            // nav_staff
            // 
            this.nav_staff.BackColor = System.Drawing.Color.Transparent;
            this.nav_staff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nav_staff.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nav_staff.ForeColor = System.Drawing.Color.White;
            this.nav_staff.Location = new System.Drawing.Point(352, 0);
            this.nav_staff.Name = "nav_staff";
            this.nav_staff.Size = new System.Drawing.Size(176, 46);
            this.nav_staff.TabIndex = 2;
            this.nav_staff.Text = "Staff Management";
            this.nav_staff.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.nav_staff.Click += new System.EventHandler(this.btn_attendance_Click);
            this.nav_staff.MouseEnter += new System.EventHandler(this.NavBtn_MouseEnter);
            this.nav_staff.MouseLeave += new System.EventHandler(this.NavBtn_MouseLeave);
            // 
            // nav_member
            // 
            this.nav_member.BackColor = System.Drawing.Color.Transparent;
            this.nav_member.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nav_member.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nav_member.ForeColor = System.Drawing.Color.White;
            this.nav_member.Location = new System.Drawing.Point(528, 0);
            this.nav_member.Name = "nav_member";
            this.nav_member.Size = new System.Drawing.Size(176, 46);
            this.nav_member.TabIndex = 3;
            this.nav_member.Text = "Member Management";
            this.nav_member.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.nav_member.Click += new System.EventHandler(this.btn_attendencemanagment_Click);
            this.nav_member.MouseEnter += new System.EventHandler(this.NavBtn_MouseEnter);
            this.nav_member.MouseLeave += new System.EventHandler(this.NavBtn_MouseLeave);
            // 
            // nav_setup
            // 
            this.nav_setup.BackColor = System.Drawing.Color.Transparent;
            this.nav_setup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nav_setup.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nav_setup.ForeColor = System.Drawing.Color.White;
            this.nav_setup.Location = new System.Drawing.Point(704, 0);
            this.nav_setup.Name = "nav_setup";
            this.nav_setup.Size = new System.Drawing.Size(177, 46);
            this.nav_setup.TabIndex = 4;
            this.nav_setup.Text = "Setup Management";
            this.nav_setup.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.nav_setup.Click += new System.EventHandler(this.btn_setupmanage_Click);
            this.nav_setup.MouseEnter += new System.EventHandler(this.NavBtn_MouseEnter);
            this.nav_setup.MouseLeave += new System.EventHandler(this.NavBtn_MouseLeave);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(23)))), ((int)(((byte)(49)))));
            this.panel2.Controls.Add(this.btn_setupmanage);
            this.panel2.Controls.Add(this.btn_attendencemanagment);
            this.panel2.Controls.Add(this.btn_staffmanagment);
            this.panel2.Controls.Add(this.btn_userManagement);
            this.panel2.Controls.Add(this.btn_device_management);
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
            // pnl_UserManagement
            // 
            this.pnl_UserManagement.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_UserManagement.Controls.Add(this.panel3);
            this.pnl_UserManagement.Location = new System.Drawing.Point(2010, 12);
            this.pnl_UserManagement.Name = "pnl_UserManagement";
            this.pnl_UserManagement.Size = new System.Drawing.Size(881, 724);
            this.pnl_UserManagement.TabIndex = 5;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.panel3.Controls.Add(this.rb_addUser);
            this.panel3.Controls.Add(this.cmbUserRole);
            this.panel3.Controls.Add(this.clv_localUsers);
            this.panel3.Location = new System.Drawing.Point(22, 168);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(837, 545);
            this.panel3.TabIndex = 2;
            // 
            // pnl_attendence
            // 
            this.pnl_attendence.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_attendence.Controls.Add(this.tab_attendence);
            this.pnl_attendence.Location = new System.Drawing.Point(2906, 12);
            this.pnl_attendence.Name = "pnl_attendence";
            this.pnl_attendence.Size = new System.Drawing.Size(881, 724);
            this.pnl_attendence.TabIndex = 4;
            // 
            // tab_attendence
            // 
            this.tab_attendence.Controls.Add(this.tab_members);
            this.tab_attendence.Controls.Add(this.tab_memberAttendence);
            this.tab_attendence.Controls.Add(this.tab_manualdoor);
            this.tab_attendence.Controls.Add(this.tab_pendingjobs);
            this.tab_attendence.Location = new System.Drawing.Point(8, 168);
            this.tab_attendence.Name = "tab_attendence";
            this.tab_attendence.SelectedIndex = 0;
            this.tab_attendence.Size = new System.Drawing.Size(865, 545);
            this.tab_attendence.TabIndex = 83;
            this.tab_attendence.SelectedIndexChanged += new System.EventHandler(this.tab_attendence_SelectedIndexChanged);
            // 
            // tab_members
            // 
            this.tab_members.Controls.Add(this.rtb_memb_name);
            this.tab_members.Controls.Add(this.rb_flt_members);
            this.tab_members.Controls.Add(this.cmbMemberSync);
            this.tab_members.Controls.Add(this.chkMemberFailure);
            this.tab_members.Controls.Add(this.lbl_hasFailureOnly);
            this.tab_members.Controls.Add(this.clv_members);
            this.tab_members.Location = new System.Drawing.Point(4, 22);
            this.tab_members.Name = "tab_members";
            this.tab_members.Size = new System.Drawing.Size(857, 519);
            this.tab_members.TabIndex = 2;
            this.tab_members.Text = "Members";
            this.tab_members.UseVisualStyleBackColor = true;
            // 
            // lbl_hasFailureOnly
            // 
            this.lbl_hasFailureOnly.AutoSize = true;
            this.lbl_hasFailureOnly.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_hasFailureOnly.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.lbl_hasFailureOnly.Location = new System.Drawing.Point(470, 12);
            this.lbl_hasFailureOnly.Name = "lbl_hasFailureOnly";
            this.lbl_hasFailureOnly.Size = new System.Drawing.Size(63, 15);
            this.lbl_hasFailureOnly.TabIndex = 0;
            this.lbl_hasFailureOnly.Text = "Has failure";
            // 
            // tab_memberAttendence
            // 
            this.tab_memberAttendence.Controls.Add(this.panel5);
            this.tab_memberAttendence.Location = new System.Drawing.Point(4, 22);
            this.tab_memberAttendence.Name = "tab_memberAttendence";
            this.tab_memberAttendence.Padding = new System.Windows.Forms.Padding(3);
            this.tab_memberAttendence.Size = new System.Drawing.Size(857, 519);
            this.tab_memberAttendence.TabIndex = 0;
            this.tab_memberAttendence.Text = "Attendence";
            this.tab_memberAttendence.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.rtb_membername);
            this.panel5.Controls.Add(this.date_member);
            this.panel5.Controls.Add(this.rb_filter_member);
            this.panel5.Controls.Add(this.clv_memberAttendance);
            this.panel5.Location = new System.Drawing.Point(0, 1);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(854, 515);
            this.panel5.TabIndex = 85;
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
            // tab_manualdoor
            // 
            this.tab_manualdoor.Controls.Add(this.panel6);
            this.tab_manualdoor.Location = new System.Drawing.Point(4, 22);
            this.tab_manualdoor.Name = "tab_manualdoor";
            this.tab_manualdoor.Padding = new System.Windows.Forms.Padding(3);
            this.tab_manualdoor.Size = new System.Drawing.Size(857, 519);
            this.tab_manualdoor.TabIndex = 1;
            this.tab_manualdoor.Text = "Manual Door ";
            this.tab_manualdoor.UseVisualStyleBackColor = true;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.date_manualdoor);
            this.panel6.Controls.Add(this.rb_filter_manualdoor);
            this.panel6.Controls.Add(this.cmbDoorVisitor);
            this.panel6.Controls.Add(this.clv_manualDoor);
            this.panel6.Location = new System.Drawing.Point(3, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(851, 516);
            this.panel6.TabIndex = 84;
            // 
            // date_manualdoor
            // 
            this.date_manualdoor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.date_manualdoor.Location = new System.Drawing.Point(142, 10);
            this.date_manualdoor.Name = "date_manualdoor";
            this.date_manualdoor.Size = new System.Drawing.Size(140, 23);
            this.date_manualdoor.TabIndex = 83;
            this.date_manualdoor.ValueChanged += new System.EventHandler(this.date_manualdoor_ValueChanged);
            // 
            // tab_pendingjobs
            // 
            this.tab_pendingjobs.Controls.Add(this.clv_pendingJobs);
            this.tab_pendingjobs.Controls.Add(this.lbl_pendingjobs_info);
            this.tab_pendingjobs.Controls.Add(this.cmbPendingDevice);
            this.tab_pendingjobs.Controls.Add(this.cmbPendingFor);
            this.tab_pendingjobs.Location = new System.Drawing.Point(4, 22);
            this.tab_pendingjobs.Name = "tab_pendingjobs";
            this.tab_pendingjobs.Padding = new System.Windows.Forms.Padding(3);
            this.tab_pendingjobs.Size = new System.Drawing.Size(857, 519);
            this.tab_pendingjobs.TabIndex = 3;
            this.tab_pendingjobs.Text = "Pending Jobs";
            this.tab_pendingjobs.UseVisualStyleBackColor = true;
            // 
            // lbl_pendingjobs_info
            // 
            this.lbl_pendingjobs_info.AutoSize = true;
            this.lbl_pendingjobs_info.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_pendingjobs_info.ForeColor = System.Drawing.Color.DimGray;
            this.lbl_pendingjobs_info.Location = new System.Drawing.Point(433, 15);
            this.lbl_pendingjobs_info.Name = "lbl_pendingjobs_info";
            this.lbl_pendingjobs_info.Size = new System.Drawing.Size(416, 15);
            this.lbl_pendingjobs_info.TabIndex = 96;
            this.lbl_pendingjobs_info.Text = "Members/devices waiting for biometric data to sync — updates automatically";
            // 
            // tab_staffpendingjobs
            // 
            this.tab_staffpendingjobs.Controls.Add(this.clv_staffPendingJobs);
            this.tab_staffpendingjobs.Controls.Add(this.lbl_staffpendingjobs_info);
            this.tab_staffpendingjobs.Controls.Add(this.cmbStaffPendingDevice);
            this.tab_staffpendingjobs.Controls.Add(this.cmbStaffPendingFor);
            this.tab_staffpendingjobs.Location = new System.Drawing.Point(4, 22);
            this.tab_staffpendingjobs.Name = "tab_staffpendingjobs";
            this.tab_staffpendingjobs.Padding = new System.Windows.Forms.Padding(3);
            this.tab_staffpendingjobs.Size = new System.Drawing.Size(857, 519);
            this.tab_staffpendingjobs.TabIndex = 1;
            this.tab_staffpendingjobs.Text = "Pending Jobs";
            this.tab_staffpendingjobs.UseVisualStyleBackColor = true;
            // 
            // lbl_staffpendingjobs_info
            // 
            this.lbl_staffpendingjobs_info.AutoSize = true;
            this.lbl_staffpendingjobs_info.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_staffpendingjobs_info.ForeColor = System.Drawing.Color.DimGray;
            this.lbl_staffpendingjobs_info.Location = new System.Drawing.Point(462, 14);
            this.lbl_staffpendingjobs_info.Name = "lbl_staffpendingjobs_info";
            this.lbl_staffpendingjobs_info.Size = new System.Drawing.Size(387, 15);
            this.lbl_staffpendingjobs_info.TabIndex = 96;
            this.lbl_staffpendingjobs_info.Text = "Staff/devices waiting for biometric data to sync — updates automatically";
            // 
            // lbl_chkStaffFailure
            // 
            this.lbl_chkStaffFailure.AutoSize = true;
            this.lbl_chkStaffFailure.BackColor = System.Drawing.Color.Transparent;
            this.lbl_chkStaffFailure.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_chkStaffFailure.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lbl_chkStaffFailure.Location = new System.Drawing.Point(587, 17);
            this.lbl_chkStaffFailure.Name = "lbl_chkStaffFailure";
            this.lbl_chkStaffFailure.Size = new System.Drawing.Size(93, 15);
            this.lbl_chkStaffFailure.TabIndex = 95;
            this.lbl_chkStaffFailure.Text = "Has Failure Only";
            // 
            // pnl_setup
            // 
            this.pnl_setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.pnl_setup.Controls.Add(this.card_api);
            this.pnl_setup.Controls.Add(this.card_sync);
            this.pnl_setup.Controls.Add(this.card_backup);
            this.pnl_setup.Controls.Add(this.card_maint);
            this.pnl_setup.Controls.Add(this.card_gate);
            this.pnl_setup.Location = new System.Drawing.Point(2010, 750);
            this.pnl_setup.Name = "pnl_setup";
            this.pnl_setup.Size = new System.Drawing.Size(881, 724);
            this.pnl_setup.TabIndex = 85;
            // 
            // card_api
            // 
            this.card_api.BackColor = System.Drawing.Color.White;
            this.card_api.Controls.Add(this.lbl_card_api_title);
            this.card_api.Controls.Add(this.lbl_card_api_sub);
            this.card_api.Controls.Add(this.rtb_APIkey);
            this.card_api.Controls.Add(this.rb_SaveAPI);
            this.card_api.Location = new System.Drawing.Point(20, 168);
            this.card_api.Name = "card_api";
            this.card_api.Size = new System.Drawing.Size(535, 96);
            this.card_api.TabIndex = 300;
            this.card_api.Paint += new System.Windows.Forms.PaintEventHandler(this.card_Paint);
            // 
            // lbl_card_api_title
            // 
            this.lbl_card_api_title.AutoSize = true;
            this.lbl_card_api_title.BackColor = System.Drawing.Color.White;
            this.lbl_card_api_title.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lbl_card_api_title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lbl_card_api_title.Location = new System.Drawing.Point(16, 12);
            this.lbl_card_api_title.Name = "lbl_card_api_title";
            this.lbl_card_api_title.Size = new System.Drawing.Size(62, 20);
            this.lbl_card_api_title.TabIndex = 301;
            this.lbl_card_api_title.Text = "API key";
            // 
            // lbl_card_api_sub
            // 
            this.lbl_card_api_sub.BackColor = System.Drawing.Color.White;
            this.lbl_card_api_sub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_api_sub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_card_api_sub.Location = new System.Drawing.Point(16, 34);
            this.lbl_card_api_sub.Name = "lbl_card_api_sub";
            this.lbl_card_api_sub.Size = new System.Drawing.Size(390, 18);
            this.lbl_card_api_sub.TabIndex = 302;
            this.lbl_card_api_sub.Text = "Used to sync members with your cloud account.";
            // 
            // card_sync
            // 
            this.card_sync.BackColor = System.Drawing.Color.White;
            this.card_sync.Controls.Add(this.lbl_card_sync_title);
            this.card_sync.Controls.Add(this.lbl_card_sync_sub);
            this.card_sync.Controls.Add(this.lbl_debounce);
            this.card_sync.Controls.Add(this.rtb_debounce);
            this.card_sync.Controls.Add(this.lbl_card_sync_retentionlabel);
            this.card_sync.Controls.Add(this.rcmb_retention);
            this.card_sync.Controls.Add(this.lbl_card_sync_retentionfooter);
            this.card_sync.Controls.Add(this.rb_btn_sync_save);
            this.card_sync.Location = new System.Drawing.Point(20, 276);
            this.card_sync.Name = "card_sync";
            this.card_sync.Size = new System.Drawing.Size(535, 164);
            this.card_sync.TabIndex = 310;
            this.card_sync.Paint += new System.Windows.Forms.PaintEventHandler(this.card_Paint);
            // 
            // lbl_card_sync_title
            // 
            this.lbl_card_sync_title.AutoSize = true;
            this.lbl_card_sync_title.BackColor = System.Drawing.Color.White;
            this.lbl_card_sync_title.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lbl_card_sync_title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lbl_card_sync_title.Location = new System.Drawing.Point(16, 12);
            this.lbl_card_sync_title.Name = "lbl_card_sync_title";
            this.lbl_card_sync_title.Size = new System.Drawing.Size(101, 20);
            this.lbl_card_sync_title.TabIndex = 311;
            this.lbl_card_sync_title.Text = "Sync settings";
            // 
            // lbl_card_sync_sub
            // 
            this.lbl_card_sync_sub.BackColor = System.Drawing.Color.White;
            this.lbl_card_sync_sub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_sync_sub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_card_sync_sub.Location = new System.Drawing.Point(16, 32);
            this.lbl_card_sync_sub.Name = "lbl_card_sync_sub";
            this.lbl_card_sync_sub.Size = new System.Drawing.Size(510, 18);
            this.lbl_card_sync_sub.TabIndex = 312;
            this.lbl_card_sync_sub.Text = "How fast device events are processed and how long logs are kept.";
            // 
            // lbl_card_sync_retentionlabel
            // 
            this.lbl_card_sync_retentionlabel.AutoSize = true;
            this.lbl_card_sync_retentionlabel.BackColor = System.Drawing.Color.White;
            this.lbl_card_sync_retentionlabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_sync_retentionlabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.lbl_card_sync_retentionlabel.Location = new System.Drawing.Point(232, 54);
            this.lbl_card_sync_retentionlabel.Name = "lbl_card_sync_retentionlabel";
            this.lbl_card_sync_retentionlabel.Size = new System.Drawing.Size(93, 15);
            this.lbl_card_sync_retentionlabel.TabIndex = 313;
            this.lbl_card_sync_retentionlabel.Text = "Retention (days)";
            // 
            // lbl_card_sync_retentionfooter
            // 
            this.lbl_card_sync_retentionfooter.AutoSize = true;
            this.lbl_card_sync_retentionfooter.BackColor = System.Drawing.Color.White;
            this.lbl_card_sync_retentionfooter.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lbl_card_sync_retentionfooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lbl_card_sync_retentionfooter.Location = new System.Drawing.Point(17, 110);
            this.lbl_card_sync_retentionfooter.Name = "lbl_card_sync_retentionfooter";
            this.lbl_card_sync_retentionfooter.Size = new System.Drawing.Size(168, 13);
            this.lbl_card_sync_retentionfooter.TabIndex = 314;
            this.lbl_card_sync_retentionfooter.Text = "Retention 0 keeps data forever.";
            // 
            // card_backup
            // 
            this.card_backup.BackColor = System.Drawing.Color.White;
            this.card_backup.Controls.Add(this.lbl_card_backup_title);
            this.card_backup.Controls.Add(this.lbl_card_backup_sub);
            this.card_backup.Controls.Add(this.lbl_backupConnectionStatus);
            this.card_backup.Controls.Add(this.lbl_card_backup_type_label);
            this.card_backup.Controls.Add(this.chk_logs);
            this.card_backup.Controls.Add(this.chk_glogs);
            this.card_backup.Controls.Add(this.chk_database);
            this.card_backup.Controls.Add(this.lbl_card_backup_drivefolder_label);
            this.card_backup.Controls.Add(this.rtb_drivefolderid);
            this.card_backup.Controls.Add(this.lbl_card_backup_creds_label);
            this.card_backup.Controls.Add(this.rtb_drivecredentials);
            this.card_backup.Controls.Add(this.rb_test_connection);
            this.card_backup.Controls.Add(this.rb_btn_savebackup);
            this.card_backup.Location = new System.Drawing.Point(20, 452);
            this.card_backup.Name = "card_backup";
            this.card_backup.Size = new System.Drawing.Size(535, 252);
            this.card_backup.TabIndex = 87;
            this.card_backup.Paint += new System.Windows.Forms.PaintEventHandler(this.card_Paint);
            // 
            // lbl_card_backup_title
            // 
            this.lbl_card_backup_title.AutoSize = true;
            this.lbl_card_backup_title.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lbl_card_backup_title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lbl_card_backup_title.Location = new System.Drawing.Point(16, 14);
            this.lbl_card_backup_title.Name = "lbl_card_backup_title";
            this.lbl_card_backup_title.Size = new System.Drawing.Size(151, 19);
            this.lbl_card_backup_title.TabIndex = 0;
            this.lbl_card_backup_title.Text = "Google Drive Backup";
            // 
            // lbl_card_backup_sub
            // 
            this.lbl_card_backup_sub.AutoSize = true;
            this.lbl_card_backup_sub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_backup_sub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_card_backup_sub.Location = new System.Drawing.Point(16, 34);
            this.lbl_card_backup_sub.Name = "lbl_card_backup_sub";
            this.lbl_card_backup_sub.Size = new System.Drawing.Size(277, 15);
            this.lbl_card_backup_sub.TabIndex = 0;
            this.lbl_card_backup_sub.Text = "Configure automatic cloud backup to Google Drive";
            // 
            // lbl_backupConnectionStatus
            // 
            this.lbl_backupConnectionStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(243)))), ((int)(((byte)(199)))));
            this.lbl_backupConnectionStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lbl_backupConnectionStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(119)))), ((int)(((byte)(6)))));
            this.lbl_backupConnectionStatus.Location = new System.Drawing.Point(389, 29);
            this.lbl_backupConnectionStatus.Name = "lbl_backupConnectionStatus";
            this.lbl_backupConnectionStatus.Size = new System.Drawing.Size(130, 22);
            this.lbl_backupConnectionStatus.TabIndex = 0;
            this.lbl_backupConnectionStatus.Text = "? Not connected";
            this.lbl_backupConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_card_backup_type_label
            // 
            this.lbl_card_backup_type_label.AutoSize = true;
            this.lbl_card_backup_type_label.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_backup_type_label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_card_backup_type_label.Location = new System.Drawing.Point(16, 58);
            this.lbl_card_backup_type_label.Name = "lbl_card_backup_type_label";
            this.lbl_card_backup_type_label.Size = new System.Drawing.Size(49, 15);
            this.lbl_card_backup_type_label.TabIndex = 0;
            this.lbl_card_backup_type_label.Text = "Include:";
            // 
            // chk_logs
            // 
            this.chk_logs.AutoSize = true;
            this.chk_logs.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_logs.Location = new System.Drawing.Point(16, 76);
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
            this.chk_glogs.Location = new System.Drawing.Point(130, 76);
            this.chk_glogs.Name = "chk_glogs";
            this.chk_glogs.Size = new System.Drawing.Size(64, 21);
            this.chk_glogs.TabIndex = 71;
            this.chk_glogs.Text = "Glogs";
            this.chk_glogs.UseVisualStyleBackColor = true;
            // 
            // chk_database
            // 
            this.chk_database.AutoSize = true;
            this.chk_database.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_database.Location = new System.Drawing.Point(228, 76);
            this.chk_database.Name = "chk_database";
            this.chk_database.Size = new System.Drawing.Size(46, 21);
            this.chk_database.TabIndex = 74;
            this.chk_database.Text = "DB";
            this.chk_database.UseVisualStyleBackColor = true;
            // 
            // lbl_card_backup_drivefolder_label
            // 
            this.lbl_card_backup_drivefolder_label.AutoSize = true;
            this.lbl_card_backup_drivefolder_label.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_backup_drivefolder_label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_card_backup_drivefolder_label.Location = new System.Drawing.Point(16, 103);
            this.lbl_card_backup_drivefolder_label.Name = "lbl_card_backup_drivefolder_label";
            this.lbl_card_backup_drivefolder_label.Size = new System.Drawing.Size(84, 15);
            this.lbl_card_backup_drivefolder_label.TabIndex = 0;
            this.lbl_card_backup_drivefolder_label.Text = "Drive Folder ID";
            // 
            // lbl_card_backup_creds_label
            // 
            this.lbl_card_backup_creds_label.AutoSize = true;
            this.lbl_card_backup_creds_label.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_backup_creds_label.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_card_backup_creds_label.Location = new System.Drawing.Point(232, 105);
            this.lbl_card_backup_creds_label.Name = "lbl_card_backup_creds_label";
            this.lbl_card_backup_creds_label.Size = new System.Drawing.Size(97, 15);
            this.lbl_card_backup_creds_label.TabIndex = 0;
            this.lbl_card_backup_creds_label.Text = "Credentials JSON";
            // 
            // card_maint
            // 
            this.card_maint.BackColor = System.Drawing.Color.White;
            this.card_maint.Controls.Add(this.lbl_card_maint_title);
            this.card_maint.Controls.Add(this.lbl_card_maint_sub);
            this.card_maint.Controls.Add(this.rb_backupcleanup);
            this.card_maint.Controls.Add(this.lbl_maint_cleanup_desc);
            this.card_maint.Controls.Add(this.rb_settime);
            this.card_maint.Controls.Add(this.lbl_maint_settime_desc);
            this.card_maint.Controls.Add(this.rb_getdb);
            this.card_maint.Controls.Add(this.lbl_maint_getdb_desc);
            this.card_maint.Location = new System.Drawing.Point(572, 168);
            this.card_maint.Name = "card_maint";
            this.card_maint.Size = new System.Drawing.Size(289, 248);
            this.card_maint.TabIndex = 340;
            this.card_maint.Paint += new System.Windows.Forms.PaintEventHandler(this.card_Paint);
            // 
            // lbl_card_maint_title
            // 
            this.lbl_card_maint_title.AutoSize = true;
            this.lbl_card_maint_title.BackColor = System.Drawing.Color.White;
            this.lbl_card_maint_title.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lbl_card_maint_title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lbl_card_maint_title.Location = new System.Drawing.Point(16, 12);
            this.lbl_card_maint_title.Name = "lbl_card_maint_title";
            this.lbl_card_maint_title.Size = new System.Drawing.Size(99, 20);
            this.lbl_card_maint_title.TabIndex = 341;
            this.lbl_card_maint_title.Text = "Maintenance";
            // 
            // lbl_card_maint_sub
            // 
            this.lbl_card_maint_sub.BackColor = System.Drawing.Color.White;
            this.lbl_card_maint_sub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_maint_sub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_card_maint_sub.Location = new System.Drawing.Point(16, 32);
            this.lbl_card_maint_sub.Name = "lbl_card_maint_sub";
            this.lbl_card_maint_sub.Size = new System.Drawing.Size(257, 18);
            this.lbl_card_maint_sub.TabIndex = 342;
            this.lbl_card_maint_sub.Text = "Run when something looks off.";
            // 
            // lbl_maint_cleanup_desc
            // 
            this.lbl_maint_cleanup_desc.BackColor = System.Drawing.Color.White;
            this.lbl_maint_cleanup_desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lbl_maint_cleanup_desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_maint_cleanup_desc.Location = new System.Drawing.Point(14, 93);
            this.lbl_maint_cleanup_desc.Name = "lbl_maint_cleanup_desc";
            this.lbl_maint_cleanup_desc.Size = new System.Drawing.Size(261, 16);
            this.lbl_maint_cleanup_desc.TabIndex = 344;
            this.lbl_maint_cleanup_desc.Text = "Remove logs past the retention period.";
            // 
            // lbl_maint_settime_desc
            // 
            this.lbl_maint_settime_desc.BackColor = System.Drawing.Color.White;
            this.lbl_maint_settime_desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lbl_maint_settime_desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_maint_settime_desc.Location = new System.Drawing.Point(14, 152);
            this.lbl_maint_settime_desc.Name = "lbl_maint_settime_desc";
            this.lbl_maint_settime_desc.Size = new System.Drawing.Size(261, 16);
            this.lbl_maint_settime_desc.TabIndex = 345;
            this.lbl_maint_settime_desc.Text = "Sync every device clock to this PC.";
            // 
            // lbl_maint_getdb_desc
            // 
            this.lbl_maint_getdb_desc.BackColor = System.Drawing.Color.White;
            this.lbl_maint_getdb_desc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lbl_maint_getdb_desc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_maint_getdb_desc.Location = new System.Drawing.Point(14, 211);
            this.lbl_maint_getdb_desc.Name = "lbl_maint_getdb_desc";
            this.lbl_maint_getdb_desc.Size = new System.Drawing.Size(261, 16);
            this.lbl_maint_getdb_desc.TabIndex = 346;
            this.lbl_maint_getdb_desc.Text = "Back up immediately instead of waiting.";
            // 
            // card_gate
            // 
            this.card_gate.BackColor = System.Drawing.Color.White;
            this.card_gate.Controls.Add(this.lbl_card_gate_title);
            this.card_gate.Controls.Add(this.lbl_card_gate_sub);
            this.card_gate.Controls.Add(this.chk_requireGateEntryForm);
            this.card_gate.Controls.Add(this.lbl_gate_entrylabel);
            this.card_gate.Controls.Add(this.lbl_gate_entrydesc);
            this.card_gate.Controls.Add(this.chk_requireGatePassword);
            this.card_gate.Controls.Add(this.lbl_gate_passwordlabel);
            this.card_gate.Controls.Add(this.lbl_gate_passworddesc);
            this.card_gate.Controls.Add(this.rb_btn_gate_save);
            this.card_gate.Location = new System.Drawing.Point(572, 428);
            this.card_gate.Name = "card_gate";
            this.card_gate.Size = new System.Drawing.Size(289, 232);
            this.card_gate.TabIndex = 88;
            this.card_gate.Paint += new System.Windows.Forms.PaintEventHandler(this.card_Paint);
            // 
            // lbl_card_gate_title
            // 
            this.lbl_card_gate_title.AutoSize = true;
            this.lbl_card_gate_title.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lbl_card_gate_title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lbl_card_gate_title.Location = new System.Drawing.Point(16, 14);
            this.lbl_card_gate_title.Name = "lbl_card_gate_title";
            this.lbl_card_gate_title.Size = new System.Drawing.Size(88, 19);
            this.lbl_card_gate_title.TabIndex = 0;
            this.lbl_card_gate_title.Text = "Gate Access";
            // 
            // lbl_card_gate_sub
            // 
            this.lbl_card_gate_sub.AutoSize = true;
            this.lbl_card_gate_sub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_card_gate_sub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_card_gate_sub.Location = new System.Drawing.Point(16, 34);
            this.lbl_card_gate_sub.Name = "lbl_card_gate_sub";
            this.lbl_card_gate_sub.Size = new System.Drawing.Size(176, 15);
            this.lbl_card_gate_sub.TabIndex = 0;
            this.lbl_card_gate_sub.Text = "Control gate entry requirements";
            // 
            // lbl_gate_entrylabel
            // 
            this.lbl_gate_entrylabel.BackColor = System.Drawing.Color.White;
            this.lbl_gate_entrylabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lbl_gate_entrylabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lbl_gate_entrylabel.Location = new System.Drawing.Point(68, 62);
            this.lbl_gate_entrylabel.Name = "lbl_gate_entrylabel";
            this.lbl_gate_entrylabel.Size = new System.Drawing.Size(209, 18);
            this.lbl_gate_entrylabel.TabIndex = 10;
            this.lbl_gate_entrylabel.Text = "Require entry form";
            // 
            // lbl_gate_entrydesc
            // 
            this.lbl_gate_entrydesc.BackColor = System.Drawing.Color.White;
            this.lbl_gate_entrydesc.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lbl_gate_entrydesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_gate_entrydesc.Location = new System.Drawing.Point(68, 81);
            this.lbl_gate_entrydesc.Name = "lbl_gate_entrydesc";
            this.lbl_gate_entrydesc.Size = new System.Drawing.Size(209, 15);
            this.lbl_gate_entrydesc.TabIndex = 11;
            this.lbl_gate_entrydesc.Text = "Member or visitor and a reason before opening.";
            // 
            // lbl_gate_passwordlabel
            // 
            this.lbl_gate_passwordlabel.BackColor = System.Drawing.Color.White;
            this.lbl_gate_passwordlabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lbl_gate_passwordlabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(24)))), ((int)(((byte)(39)))));
            this.lbl_gate_passwordlabel.Location = new System.Drawing.Point(68, 112);
            this.lbl_gate_passwordlabel.Name = "lbl_gate_passwordlabel";
            this.lbl_gate_passwordlabel.Size = new System.Drawing.Size(209, 18);
            this.lbl_gate_passwordlabel.TabIndex = 12;
            this.lbl_gate_passwordlabel.Text = "Require password";
            // 
            // lbl_gate_passworddesc
            // 
            this.lbl_gate_passworddesc.BackColor = System.Drawing.Color.White;
            this.lbl_gate_passworddesc.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lbl_gate_passworddesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lbl_gate_passworddesc.Location = new System.Drawing.Point(68, 131);
            this.lbl_gate_passworddesc.Name = "lbl_gate_passworddesc";
            this.lbl_gate_passworddesc.Size = new System.Drawing.Size(209, 15);
            this.lbl_gate_passworddesc.TabIndex = 13;
            this.lbl_gate_passworddesc.Text = "Ask for a password, with or without the form.";
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 100);
            this.panel1.TabIndex = 0;
            // 
            // num_retention
            // 
            this.num_retention.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.num_retention.Location = new System.Drawing.Point(13, 152);
            this.num_retention.Name = "num_retention";
            this.num_retention.Size = new System.Drawing.Size(91, 26);
            this.num_retention.TabIndex = 70;
            this.num_retention.Visible = false;
            // 
            // pnl_backup
            // 
            this.pnl_backup.Location = new System.Drawing.Point(0, 0);
            this.pnl_backup.Name = "pnl_backup";
            this.pnl_backup.Size = new System.Drawing.Size(200, 100);
            this.pnl_backup.TabIndex = 0;
            // 
            // lbl_backuptype
            // 
            this.lbl_backuptype.AutoSize = true;
            this.lbl_backuptype.Location = new System.Drawing.Point(0, 0);
            this.lbl_backuptype.Name = "lbl_backuptype";
            this.lbl_backuptype.Size = new System.Drawing.Size(71, 13);
            this.lbl_backuptype.TabIndex = 87;
            this.lbl_backuptype.Text = "Backup Type";
            this.lbl_backuptype.Visible = false;
            // 
            // pnl_staffsManagment
            // 
            this.pnl_staffsManagment.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_staffsManagment.Controls.Add(this.tab_staff);
            this.pnl_staffsManagment.Location = new System.Drawing.Point(2906, 750);
            this.pnl_staffsManagment.Name = "pnl_staffsManagment";
            this.pnl_staffsManagment.Size = new System.Drawing.Size(881, 724);
            this.pnl_staffsManagment.TabIndex = 84;
            // 
            // tab_staff
            // 
            this.tab_staff.Controls.Add(this.tab_staffmembers);
            this.tab_staff.Controls.Add(this.tab_staffattendence);
            this.tab_staff.Controls.Add(this.tab_staffpendingjobs);
            this.tab_staff.Location = new System.Drawing.Point(8, 168);
            this.tab_staff.Name = "tab_staff";
            this.tab_staff.SelectedIndex = 0;
            this.tab_staff.Size = new System.Drawing.Size(865, 545);
            this.tab_staff.TabIndex = 83;
            this.tab_staff.SelectedIndexChanged += new System.EventHandler(this.tab_staff_SelectedIndexChanged);
            // 
            // tab_staffmembers
            // 
            this.tab_staffmembers.Controls.Add(this.rtb_staff);
            this.tab_staffmembers.Controls.Add(this.rb_stafffilter);
            this.tab_staffmembers.Controls.Add(this.cmbStaffStatus);
            this.tab_staffmembers.Controls.Add(this.cmbStaffSync);
            this.tab_staffmembers.Controls.Add(this.chkStaffFailure);
            this.tab_staffmembers.Controls.Add(this.lbl_chkStaffFailure);
            this.tab_staffmembers.Controls.Add(this.clv_staffs);
            this.tab_staffmembers.Location = new System.Drawing.Point(4, 22);
            this.tab_staffmembers.Name = "tab_staffmembers";
            this.tab_staffmembers.Size = new System.Drawing.Size(857, 519);
            this.tab_staffmembers.TabIndex = 2;
            this.tab_staffmembers.Text = "Staffs";
            this.tab_staffmembers.UseVisualStyleBackColor = true;
            // 
            // tab_staffattendence
            // 
            this.tab_staffattendence.Controls.Add(this.panel8);
            this.tab_staffattendence.Location = new System.Drawing.Point(4, 22);
            this.tab_staffattendence.Name = "tab_staffattendence";
            this.tab_staffattendence.Padding = new System.Windows.Forms.Padding(3);
            this.tab_staffattendence.Size = new System.Drawing.Size(857, 519);
            this.tab_staffattendence.TabIndex = 0;
            this.tab_staffattendence.Text = "Attendence";
            this.tab_staffattendence.UseVisualStyleBackColor = true;
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this.rtb_staffname);
            this.panel8.Controls.Add(this.date_staffattendence);
            this.panel8.Controls.Add(this.rb_filterattendencestaff);
            this.panel8.Controls.Add(this.clv_staffAttendance);
            this.panel8.Location = new System.Drawing.Point(0, 1);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(854, 518);
            this.panel8.TabIndex = 85;
            // 
            // date_staffattendence
            // 
            this.date_staffattendence.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.date_staffattendence.Location = new System.Drawing.Point(160, 9);
            this.date_staffattendence.Name = "date_staffattendence";
            this.date_staffattendence.Size = new System.Drawing.Size(140, 23);
            this.date_staffattendence.TabIndex = 83;
            // 
            // pnl_device
            // 
            this.pnl_device.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.pnl_device.Controls.Add(this.rb_add_device);
            this.pnl_device.Controls.Add(this.chkDeviceIsStaff);
            this.pnl_device.Controls.Add(this.lbl_staffDevicesOnly);
            this.pnl_device.Controls.Add(this.clv_devices);
            this.pnl_device.Location = new System.Drawing.Point(22, 168);
            this.pnl_device.Name = "pnl_device";
            this.pnl_device.Size = new System.Drawing.Size(837, 545);
            this.pnl_device.TabIndex = 2;
            // 
            // lbl_staffDevicesOnly
            // 
            this.lbl_staffDevicesOnly.AutoSize = true;
            this.lbl_staffDevicesOnly.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbl_staffDevicesOnly.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.lbl_staffDevicesOnly.Location = new System.Drawing.Point(64, 14);
            this.lbl_staffDevicesOnly.Name = "lbl_staffDevicesOnly";
            this.lbl_staffDevicesOnly.Size = new System.Drawing.Size(57, 15);
            this.lbl_staffDevicesOnly.TabIndex = 0;
            this.lbl_staffDevicesOnly.Text = "Staff only";
            // 
            // pnl_deviceManager
            // 
            this.pnl_deviceManager.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnl_deviceManager.Controls.Add(this.pnl_device);
            this.pnl_deviceManager.Location = new System.Drawing.Point(1114, 12);
            this.pnl_deviceManager.Name = "pnl_deviceManager";
            this.pnl_deviceManager.Size = new System.Drawing.Size(881, 724);
            this.pnl_deviceManager.TabIndex = 0;
            // 
            // tile_device
            // 
            this.tile_device.BackColor = System.Drawing.Color.Transparent;
            this.tile_device.Badge = "";
            this.tile_device.BadgeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.tile_device.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_device.Description = "Add, edit and monitor biometric devices.";
            this.tile_device.Highlighted = true;
            this.tile_device.Icon = "??";
            this.tile_device.Location = new System.Drawing.Point(24, 24);
            this.tile_device.Name = "tile_device";
            this.tile_device.Size = new System.Drawing.Size(256, 175);
            this.tile_device.TabIndex = 0;
            this.tile_device.Title = "Device Management";
            this.tile_device.Click += new System.EventHandler(this.btn_device_management_Click);
            // 
            // tile_user
            // 
            this.tile_user.BackColor = System.Drawing.Color.Transparent;
            this.tile_user.Badge = "";
            this.tile_user.BadgeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.tile_user.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_user.Description = "System users and their permissions.";
            this.tile_user.Highlighted = false;
            this.tile_user.Icon = "??";
            this.tile_user.Location = new System.Drawing.Point(298, 24);
            this.tile_user.Name = "tile_user";
            this.tile_user.Size = new System.Drawing.Size(256, 175);
            this.tile_user.TabIndex = 1;
            this.tile_user.Title = "User Management";
            this.tile_user.Click += new System.EventHandler(this.btn_userManagement_Click);
            // 
            // tile_staff
            // 
            this.tile_staff.BackColor = System.Drawing.Color.Transparent;
            this.tile_staff.Badge = "";
            this.tile_staff.BadgeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.tile_staff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_staff.Description = "Staff list and attendance.";
            this.tile_staff.Highlighted = false;
            this.tile_staff.Icon = "??";
            this.tile_staff.Location = new System.Drawing.Point(572, 24);
            this.tile_staff.Name = "tile_staff";
            this.tile_staff.Size = new System.Drawing.Size(256, 175);
            this.tile_staff.TabIndex = 2;
            this.tile_staff.Title = "Staff Management";
            this.tile_staff.Click += new System.EventHandler(this.btn_attendance_Click);
            // 
            // tile_member
            // 
            this.tile_member.BackColor = System.Drawing.Color.Transparent;
            this.tile_member.Badge = "";
            this.tile_member.BadgeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.tile_member.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_member.Description = "Member records, offline entries, renewals.";
            this.tile_member.Highlighted = false;
            this.tile_member.Icon = "??";
            this.tile_member.Location = new System.Drawing.Point(24, 217);
            this.tile_member.Name = "tile_member";
            this.tile_member.Size = new System.Drawing.Size(256, 175);
            this.tile_member.TabIndex = 3;
            this.tile_member.Title = "Member Management";
            this.tile_member.Click += new System.EventHandler(this.btn_attendencemanagment_Click);
            // 
            // tile_setup
            // 
            this.tile_setup.BackColor = System.Drawing.Color.Transparent;
            this.tile_setup.Badge = "";
            this.tile_setup.BadgeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.tile_setup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tile_setup.Description = "API keys, backups and settings.";
            this.tile_setup.Highlighted = false;
            this.tile_setup.Icon = "?";
            this.tile_setup.Location = new System.Drawing.Point(298, 217);
            this.tile_setup.Name = "tile_setup";
            this.tile_setup.Size = new System.Drawing.Size(256, 175);
            this.tile_setup.TabIndex = 5;
            this.tile_setup.Title = "Setup Management";
            this.tile_setup.Click += new System.EventHandler(this.btn_setupmanage_Click);
            // 
            // rtb_APIkey
            // 
            this.rtb_APIkey.BackColor = System.Drawing.Color.Transparent;
            this.rtb_APIkey.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtb_APIkey.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtb_APIkey.Location = new System.Drawing.Point(16, 56);
            this.rtb_APIkey.Name = "rtb_APIkey";
            this.rtb_APIkey.Placeholder = "API Key";
            this.rtb_APIkey.Radius = 10;
            this.rtb_APIkey.Size = new System.Drawing.Size(386, 36);
            this.rtb_APIkey.PasswordChar = '*';
            this.rtb_APIkey.UseSystemPasswordChar = true;
            this.rtb_APIkey.TabIndex = 42;
            // 
            // rb_SaveAPI
            // 
            this.rb_SaveAPI.BackColor = System.Drawing.Color.Transparent;
            this.rb_SaveAPI.BorderColor = System.Drawing.Color.Transparent;
            this.rb_SaveAPI.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_SaveAPI.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_SaveAPI.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_SaveAPI.ForeColor = System.Drawing.Color.White;
            this.rb_SaveAPI.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_SaveAPI.Location = new System.Drawing.Point(417, 54);
            this.rb_SaveAPI.Name = "rb_SaveAPI";
            this.rb_SaveAPI.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_SaveAPI.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_SaveAPI.Radius = 10;
            this.rb_SaveAPI.Size = new System.Drawing.Size(109, 36);
            this.rb_SaveAPI.TabIndex = 39;
            this.rb_SaveAPI.Text = "Save";
            this.rb_SaveAPI.Click += new System.EventHandler(this.btnSaveAPI_Click_1);
            // 
            // rtb_debounce
            // 
            this.rtb_debounce.BackColor = System.Drawing.Color.Transparent;
            this.rtb_debounce.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtb_debounce.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtb_debounce.Location = new System.Drawing.Point(16, 70);
            this.rtb_debounce.Name = "rtb_debounce";
            this.rtb_debounce.Placeholder = "";
            this.rtb_debounce.Radius = 10;
            this.rtb_debounce.Size = new System.Drawing.Size(200, 36);
            this.rtb_debounce.TabIndex = 44;
            // 
            // rcmb_retention
            // 
            this.rcmb_retention.BackColor = System.Drawing.Color.White;
            this.rcmb_retention.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rcmb_retention.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rcmb_retention.Location = new System.Drawing.Point(228, 70);
            this.rcmb_retention.Name = "rcmb_retention";
            this.rcmb_retention.Radius = 8;
            this.rcmb_retention.Size = new System.Drawing.Size(143, 36);
            this.rcmb_retention.TabIndex = 97;
            // 
            // rb_btn_sync_save
            // 
            this.rb_btn_sync_save.BackColor = System.Drawing.Color.Transparent;
            this.rb_btn_sync_save.BorderColor = System.Drawing.Color.Transparent;
            this.rb_btn_sync_save.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_btn_sync_save.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_btn_sync_save.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_btn_sync_save.ForeColor = System.Drawing.Color.White;
            this.rb_btn_sync_save.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_btn_sync_save.Location = new System.Drawing.Point(16, 126);
            this.rb_btn_sync_save.Name = "rb_btn_sync_save";
            this.rb_btn_sync_save.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_btn_sync_save.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_btn_sync_save.Radius = 10;
            this.rb_btn_sync_save.Size = new System.Drawing.Size(503, 34);
            this.rb_btn_sync_save.TabIndex = 91;
            this.rb_btn_sync_save.Text = "Save sync settings";
            this.rb_btn_sync_save.Click += new System.EventHandler(this.btn_sync_save_Click);
            // 
            // rtb_drivefolderid
            // 
            this.rtb_drivefolderid.BackColor = System.Drawing.Color.Transparent;
            this.rtb_drivefolderid.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtb_drivefolderid.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtb_drivefolderid.Location = new System.Drawing.Point(16, 123);
            this.rtb_drivefolderid.Name = "rtb_drivefolderid";
            this.rtb_drivefolderid.Placeholder = "Google Drive Folder ID";
            this.rtb_drivefolderid.Radius = 10;
            this.rtb_drivefolderid.Size = new System.Drawing.Size(196, 36);
            this.rtb_drivefolderid.TabIndex = 56;
            // 
            // rtb_drivecredentials
            // 
            this.rtb_drivecredentials.BackColor = System.Drawing.Color.Transparent;
            this.rtb_drivecredentials.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtb_drivecredentials.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtb_drivecredentials.Location = new System.Drawing.Point(232, 123);
            this.rtb_drivecredentials.Name = "rtb_drivecredentials";
            this.rtb_drivecredentials.Placeholder = "Paste credentials.json content here";
            this.rtb_drivecredentials.Radius = 10;
            this.rtb_drivecredentials.Size = new System.Drawing.Size(287, 85);
            this.rtb_drivecredentials.TabIndex = 57;
            // 
            // rb_test_connection
            // 
            this.rb_test_connection.BackColor = System.Drawing.Color.Transparent;
            this.rb_test_connection.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_test_connection.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_test_connection.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_test_connection.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_test_connection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_test_connection.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.rb_test_connection.Location = new System.Drawing.Point(118, 211);
            this.rb_test_connection.Name = "rb_test_connection";
            this.rb_test_connection.NormalColor = System.Drawing.Color.White;
            this.rb_test_connection.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this.rb_test_connection.Radius = 10;
            this.rb_test_connection.Size = new System.Drawing.Size(196, 34);
            this.rb_test_connection.TabIndex = 76;
            this.rb_test_connection.Text = "Connect Google Drive";
            this.rb_test_connection.Click += new System.EventHandler(this.btn_test_connection_Click);
            // 
            // rb_btn_savebackup
            // 
            this.rb_btn_savebackup.BackColor = System.Drawing.Color.Transparent;
            this.rb_btn_savebackup.BorderColor = System.Drawing.Color.Transparent;
            this.rb_btn_savebackup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_btn_savebackup.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_btn_savebackup.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_btn_savebackup.ForeColor = System.Drawing.Color.White;
            this.rb_btn_savebackup.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_btn_savebackup.Location = new System.Drawing.Point(320, 211);
            this.rb_btn_savebackup.Name = "rb_btn_savebackup";
            this.rb_btn_savebackup.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_btn_savebackup.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_btn_savebackup.Radius = 10;
            this.rb_btn_savebackup.Size = new System.Drawing.Size(199, 34);
            this.rb_btn_savebackup.TabIndex = 90;
            this.rb_btn_savebackup.Text = "Save backup settings";
            this.rb_btn_savebackup.Click += new System.EventHandler(this.btn_savebackup_Click);
            // 
            // rb_backupcleanup
            // 
            this.rb_backupcleanup.BackColor = System.Drawing.Color.Transparent;
            this.rb_backupcleanup.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_backupcleanup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_backupcleanup.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_backupcleanup.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_backupcleanup.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_backupcleanup.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.rb_backupcleanup.Location = new System.Drawing.Point(12, 56);
            this.rb_backupcleanup.Name = "rb_backupcleanup";
            this.rb_backupcleanup.NormalColor = System.Drawing.Color.White;
            this.rb_backupcleanup.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this.rb_backupcleanup.Radius = 10;
            this.rb_backupcleanup.Size = new System.Drawing.Size(265, 34);
            this.rb_backupcleanup.TabIndex = 75;
            this.rb_backupcleanup.Text = "Clean up old data";
            this.rb_backupcleanup.Click += new System.EventHandler(this.btn_backupcleanup_Click);
            // 
            // rb_settime
            // 
            this.rb_settime.BackColor = System.Drawing.Color.Transparent;
            this.rb_settime.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_settime.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_settime.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_settime.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_settime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_settime.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.rb_settime.Location = new System.Drawing.Point(12, 115);
            this.rb_settime.Name = "rb_settime";
            this.rb_settime.NormalColor = System.Drawing.Color.White;
            this.rb_settime.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this.rb_settime.Radius = 10;
            this.rb_settime.Size = new System.Drawing.Size(265, 34);
            this.rb_settime.TabIndex = 50;
            this.rb_settime.Text = "Set device time";
            this.rb_settime.Click += new System.EventHandler(this.btn_settime_Click);
            // 
            // rb_getdb
            // 
            this.rb_getdb.BackColor = System.Drawing.Color.Transparent;
            this.rb_getdb.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_getdb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_getdb.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_getdb.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_getdb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_getdb.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.rb_getdb.Location = new System.Drawing.Point(12, 174);
            this.rb_getdb.Name = "rb_getdb";
            this.rb_getdb.NormalColor = System.Drawing.Color.White;
            this.rb_getdb.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(234)))), ((int)(((byte)(254)))));
            this.rb_getdb.Radius = 10;
            this.rb_getdb.Size = new System.Drawing.Size(265, 34);
            this.rb_getdb.TabIndex = 47;
            this.rb_getdb.Text = "Run backup now";
            this.rb_getdb.Click += new System.EventHandler(this.btn_getdb_Click);
            // 
            // chk_requireGateEntryForm
            // 
            this.chk_requireGateEntryForm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chk_requireGateEntryForm.Location = new System.Drawing.Point(16, 62);
            this.chk_requireGateEntryForm.Name = "chk_requireGateEntryForm";
            this.chk_requireGateEntryForm.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(204)))), ((int)(((byte)(216)))));
            this.chk_requireGateEntryForm.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.chk_requireGateEntryForm.Size = new System.Drawing.Size(44, 24);
            this.chk_requireGateEntryForm.TabIndex = 1;
            // 
            // chk_requireGatePassword
            // 
            this.chk_requireGatePassword.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chk_requireGatePassword.Location = new System.Drawing.Point(16, 112);
            this.chk_requireGatePassword.Name = "chk_requireGatePassword";
            this.chk_requireGatePassword.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(204)))), ((int)(((byte)(216)))));
            this.chk_requireGatePassword.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.chk_requireGatePassword.Size = new System.Drawing.Size(44, 24);
            this.chk_requireGatePassword.TabIndex = 2;
            // 
            // rb_btn_gate_save
            // 
            this.rb_btn_gate_save.BackColor = System.Drawing.Color.Transparent;
            this.rb_btn_gate_save.BorderColor = System.Drawing.Color.Transparent;
            this.rb_btn_gate_save.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_btn_gate_save.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_btn_gate_save.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_btn_gate_save.ForeColor = System.Drawing.Color.White;
            this.rb_btn_gate_save.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_btn_gate_save.Location = new System.Drawing.Point(12, 186);
            this.rb_btn_gate_save.Name = "rb_btn_gate_save";
            this.rb_btn_gate_save.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_btn_gate_save.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_btn_gate_save.Radius = 10;
            this.rb_btn_gate_save.Size = new System.Drawing.Size(265, 34);
            this.rb_btn_gate_save.TabIndex = 3;
            this.rb_btn_gate_save.Text = "Save gate settings";
            this.rb_btn_gate_save.Click += new System.EventHandler(this.btn_gate_save_Click);
            // 
            // rtb_memb_name
            // 
            this.rtb_memb_name.BackColor = System.Drawing.Color.Transparent;
            this.rtb_memb_name.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtb_memb_name.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtb_memb_name.Location = new System.Drawing.Point(140, 9);
            this.rtb_memb_name.Name = "rtb_memb_name";
            this.rtb_memb_name.Placeholder = "";
            this.rtb_memb_name.Radius = 10;
            this.rtb_memb_name.Size = new System.Drawing.Size(188, 23);
            this.rtb_memb_name.TabIndex = 89;
            // 
            // rb_flt_members
            // 
            this.rb_flt_members.BackColor = System.Drawing.Color.Transparent;
            this.rb_flt_members.BorderColor = System.Drawing.Color.Transparent;
            this.rb_flt_members.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_flt_members.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_flt_members.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_flt_members.ForeColor = System.Drawing.Color.White;
            this.rb_flt_members.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_flt_members.Location = new System.Drawing.Point(334, 7);
            this.rb_flt_members.Name = "rb_flt_members";
            this.rb_flt_members.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_flt_members.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_flt_members.Radius = 10;
            this.rb_flt_members.Size = new System.Drawing.Size(80, 28);
            this.rb_flt_members.TabIndex = 91;
            this.rb_flt_members.Text = "Filter";
            // 
            // cmbMemberSync
            // 
            this.cmbMemberSync.BackColor = System.Drawing.Color.White;
            this.cmbMemberSync.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbMemberSync.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbMemberSync.Location = new System.Drawing.Point(4, 8);
            this.cmbMemberSync.Name = "cmbMemberSync";
            this.cmbMemberSync.Radius = 8;
            this.cmbMemberSync.Size = new System.Drawing.Size(130, 24);
            this.cmbMemberSync.TabIndex = 95;
            this.cmbMemberSync.SelectedIndexChanged += new System.EventHandler(this.cmbMemberSync_SelectedIndexChanged);
            // 
            // chkMemberFailure
            // 
            this.chkMemberFailure.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkMemberFailure.Location = new System.Drawing.Point(420, 8);
            this.chkMemberFailure.Name = "chkMemberFailure";
            this.chkMemberFailure.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(204)))), ((int)(((byte)(216)))));
            this.chkMemberFailure.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.chkMemberFailure.Size = new System.Drawing.Size(44, 24);
            this.chkMemberFailure.TabIndex = 96;
            this.chkMemberFailure.CheckedChanged += new System.EventHandler(this.chkMemberFailure_CheckedChanged);
            // 
            // clv_members
            // 
            this.clv_members.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_members.Location = new System.Drawing.Point(7, 41);
            this.clv_members.Name = "clv_members";
            this.clv_members.Size = new System.Drawing.Size(847, 475);
            this.clv_members.TabIndex = 92;
            // 
            // rtb_membername
            // 
            this.rtb_membername.BackColor = System.Drawing.Color.Transparent;
            this.rtb_membername.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtb_membername.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtb_membername.Location = new System.Drawing.Point(9, 9);
            this.rtb_membername.Name = "rtb_membername";
            this.rtb_membername.Placeholder = "";
            this.rtb_membername.Radius = 10;
            this.rtb_membername.Size = new System.Drawing.Size(100, 23);
            this.rtb_membername.TabIndex = 82;
            this.rtb_membername.TextChanged += new System.EventHandler(this.txt_membername_TextChanged);
            // 
            // rb_filter_member
            // 
            this.rb_filter_member.BackColor = System.Drawing.Color.Transparent;
            this.rb_filter_member.BorderColor = System.Drawing.Color.Transparent;
            this.rb_filter_member.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_filter_member.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_filter_member.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_filter_member.ForeColor = System.Drawing.Color.White;
            this.rb_filter_member.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_filter_member.Location = new System.Drawing.Point(261, 6);
            this.rb_filter_member.Name = "rb_filter_member";
            this.rb_filter_member.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_filter_member.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_filter_member.Radius = 10;
            this.rb_filter_member.Size = new System.Drawing.Size(80, 28);
            this.rb_filter_member.TabIndex = 85;
            this.rb_filter_member.Text = "Filter";
            this.rb_filter_member.Click += new System.EventHandler(this.btn_filter_member_Click);
            // 
            // clv_memberAttendance
            // 
            this.clv_memberAttendance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_memberAttendance.Location = new System.Drawing.Point(6, 40);
            this.clv_memberAttendance.Name = "clv_memberAttendance";
            this.clv_memberAttendance.Size = new System.Drawing.Size(845, 472);
            this.clv_memberAttendance.TabIndex = 86;
            // 
            // rb_filter_manualdoor
            // 
            this.rb_filter_manualdoor.BackColor = System.Drawing.Color.Transparent;
            this.rb_filter_manualdoor.BorderColor = System.Drawing.Color.Transparent;
            this.rb_filter_manualdoor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_filter_manualdoor.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_filter_manualdoor.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_filter_manualdoor.ForeColor = System.Drawing.Color.White;
            this.rb_filter_manualdoor.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_filter_manualdoor.Location = new System.Drawing.Point(295, 8);
            this.rb_filter_manualdoor.Name = "rb_filter_manualdoor";
            this.rb_filter_manualdoor.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_filter_manualdoor.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_filter_manualdoor.Radius = 10;
            this.rb_filter_manualdoor.Size = new System.Drawing.Size(79, 25);
            this.rb_filter_manualdoor.TabIndex = 85;
            this.rb_filter_manualdoor.Text = "Filter";
            this.rb_filter_manualdoor.Click += new System.EventHandler(this.btn_filter_manualdoor_Click);
            // 
            // cmbDoorVisitor
            // 
            this.cmbDoorVisitor.BackColor = System.Drawing.Color.Transparent;
            this.cmbDoorVisitor.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbDoorVisitor.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbDoorVisitor.Location = new System.Drawing.Point(6, 11);
            this.cmbDoorVisitor.Name = "cmbDoorVisitor";
            this.cmbDoorVisitor.Radius = 10;
            this.cmbDoorVisitor.Size = new System.Drawing.Size(130, 22);
            this.cmbDoorVisitor.TabIndex = 87;
            this.cmbDoorVisitor.SelectedIndexChanged += new System.EventHandler(this.cmbDoorVisitor_SelectedIndexChanged);
            // 
            // clv_manualDoor
            // 
            this.clv_manualDoor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_manualDoor.Location = new System.Drawing.Point(6, 46);
            this.clv_manualDoor.Name = "clv_manualDoor";
            this.clv_manualDoor.Size = new System.Drawing.Size(842, 467);
            this.clv_manualDoor.TabIndex = 86;
            // 
            // clv_pendingJobs
            // 
            this.clv_pendingJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_pendingJobs.Location = new System.Drawing.Point(6, 41);
            this.clv_pendingJobs.Name = "clv_pendingJobs";
            this.clv_pendingJobs.Size = new System.Drawing.Size(844, 472);
            this.clv_pendingJobs.TabIndex = 98;
            // 
            // cmbPendingDevice
            // 
            this.cmbPendingDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPendingDevice.BackColor = System.Drawing.Color.Transparent;
            this.cmbPendingDevice.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbPendingDevice.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbPendingDevice.Location = new System.Drawing.Point(9, 10);
            this.cmbPendingDevice.Name = "cmbPendingDevice";
            this.cmbPendingDevice.Radius = 10;
            this.cmbPendingDevice.Size = new System.Drawing.Size(150, 22);
            this.cmbPendingDevice.TabIndex = 99;
            this.cmbPendingDevice.SelectedIndexChanged += new System.EventHandler(this.cmbPendingDevice_SelectedIndexChanged);
            // 
            // cmbPendingFor
            // 
            this.cmbPendingFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPendingFor.BackColor = System.Drawing.Color.Transparent;
            this.cmbPendingFor.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbPendingFor.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbPendingFor.Location = new System.Drawing.Point(167, 10);
            this.cmbPendingFor.Name = "cmbPendingFor";
            this.cmbPendingFor.Radius = 10;
            this.cmbPendingFor.Size = new System.Drawing.Size(120, 22);
            this.cmbPendingFor.TabIndex = 100;
            this.cmbPendingFor.SelectedIndexChanged += new System.EventHandler(this.cmbPendingFor_SelectedIndexChanged);
            // 
            // rtb_staff
            // 
            this.rtb_staff.BackColor = System.Drawing.Color.Transparent;
            this.rtb_staff.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtb_staff.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtb_staff.Location = new System.Drawing.Point(7, 9);
            this.rtb_staff.Name = "rtb_staff";
            this.rtb_staff.Placeholder = "";
            this.rtb_staff.Radius = 10;
            this.rtb_staff.Size = new System.Drawing.Size(188, 26);
            this.rtb_staff.TabIndex = 89;
            // 
            // rb_stafffilter
            // 
            this.rb_stafffilter.BackColor = System.Drawing.Color.Transparent;
            this.rb_stafffilter.BorderColor = System.Drawing.Color.Transparent;
            this.rb_stafffilter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_stafffilter.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_stafffilter.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_stafffilter.ForeColor = System.Drawing.Color.White;
            this.rb_stafffilter.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_stafffilter.Location = new System.Drawing.Point(201, 7);
            this.rb_stafffilter.Name = "rb_stafffilter";
            this.rb_stafffilter.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_stafffilter.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_stafffilter.Radius = 10;
            this.rb_stafffilter.Size = new System.Drawing.Size(80, 28);
            this.rb_stafffilter.TabIndex = 91;
            this.rb_stafffilter.Text = "Filter";
            // 
            // cmbStaffStatus
            // 
            this.cmbStaffStatus.BackColor = System.Drawing.Color.Transparent;
            this.cmbStaffStatus.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbStaffStatus.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbStaffStatus.Location = new System.Drawing.Point(293, 9);
            this.cmbStaffStatus.Name = "cmbStaffStatus";
            this.cmbStaffStatus.Radius = 10;
            this.cmbStaffStatus.Size = new System.Drawing.Size(110, 26);
            this.cmbStaffStatus.TabIndex = 92;
            this.cmbStaffStatus.SelectedIndexChanged += new System.EventHandler(this.cmbStaffStatus_SelectedIndexChanged);
            // 
            // cmbStaffSync
            // 
            this.cmbStaffSync.BackColor = System.Drawing.Color.Transparent;
            this.cmbStaffSync.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbStaffSync.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbStaffSync.Location = new System.Drawing.Point(411, 11);
            this.cmbStaffSync.Name = "cmbStaffSync";
            this.cmbStaffSync.Radius = 10;
            this.cmbStaffSync.Size = new System.Drawing.Size(120, 24);
            this.cmbStaffSync.TabIndex = 93;
            this.cmbStaffSync.SelectedIndexChanged += new System.EventHandler(this.cmbStaffSync_SelectedIndexChanged);
            // 
            // chkStaffFailure
            // 
            this.chkStaffFailure.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkStaffFailure.Location = new System.Drawing.Point(537, 11);
            this.chkStaffFailure.Name = "chkStaffFailure";
            this.chkStaffFailure.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(204)))), ((int)(((byte)(216)))));
            this.chkStaffFailure.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.chkStaffFailure.Size = new System.Drawing.Size(44, 24);
            this.chkStaffFailure.TabIndex = 94;
            this.chkStaffFailure.CheckedChanged += new System.EventHandler(this.chkStaffFailure_CheckedChanged);
            // 
            // clv_staffs
            // 
            this.clv_staffs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_staffs.Location = new System.Drawing.Point(7, 41);
            this.clv_staffs.Name = "clv_staffs";
            this.clv_staffs.Size = new System.Drawing.Size(847, 475);
            this.clv_staffs.TabIndex = 92;
            // 
            // rtb_staffname
            // 
            this.rtb_staffname.BackColor = System.Drawing.Color.Transparent;
            this.rtb_staffname.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtb_staffname.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtb_staffname.Location = new System.Drawing.Point(9, 9);
            this.rtb_staffname.Name = "rtb_staffname";
            this.rtb_staffname.Placeholder = "";
            this.rtb_staffname.Radius = 10;
            this.rtb_staffname.Size = new System.Drawing.Size(145, 23);
            this.rtb_staffname.TabIndex = 82;
            // 
            // rb_filterattendencestaff
            // 
            this.rb_filterattendencestaff.BackColor = System.Drawing.Color.Transparent;
            this.rb_filterattendencestaff.BorderColor = System.Drawing.Color.Transparent;
            this.rb_filterattendencestaff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_filterattendencestaff.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_filterattendencestaff.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_filterattendencestaff.ForeColor = System.Drawing.Color.White;
            this.rb_filterattendencestaff.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_filterattendencestaff.Location = new System.Drawing.Point(306, 6);
            this.rb_filterattendencestaff.Name = "rb_filterattendencestaff";
            this.rb_filterattendencestaff.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_filterattendencestaff.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_filterattendencestaff.Radius = 10;
            this.rb_filterattendencestaff.Size = new System.Drawing.Size(80, 28);
            this.rb_filterattendencestaff.TabIndex = 85;
            this.rb_filterattendencestaff.Text = "Filter";
            // 
            // clv_staffAttendance
            // 
            this.clv_staffAttendance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_staffAttendance.Location = new System.Drawing.Point(9, 40);
            this.clv_staffAttendance.Name = "clv_staffAttendance";
            this.clv_staffAttendance.Size = new System.Drawing.Size(838, 481);
            this.clv_staffAttendance.TabIndex = 86;
            // 
            // clv_staffPendingJobs
            // 
            this.clv_staffPendingJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_staffPendingJobs.Location = new System.Drawing.Point(9, 41);
            this.clv_staffPendingJobs.Name = "clv_staffPendingJobs";
            this.clv_staffPendingJobs.Size = new System.Drawing.Size(841, 472);
            this.clv_staffPendingJobs.TabIndex = 98;
            // 
            // cmbStaffPendingDevice
            // 
            this.cmbStaffPendingDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbStaffPendingDevice.BackColor = System.Drawing.Color.Transparent;
            this.cmbStaffPendingDevice.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbStaffPendingDevice.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbStaffPendingDevice.Location = new System.Drawing.Point(9, 12);
            this.cmbStaffPendingDevice.Name = "cmbStaffPendingDevice";
            this.cmbStaffPendingDevice.Radius = 10;
            this.cmbStaffPendingDevice.Size = new System.Drawing.Size(150, 22);
            this.cmbStaffPendingDevice.TabIndex = 99;
            this.cmbStaffPendingDevice.SelectedIndexChanged += new System.EventHandler(this.cmbStaffPendingDevice_SelectedIndexChanged);
            // 
            // cmbStaffPendingFor
            // 
            this.cmbStaffPendingFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbStaffPendingFor.BackColor = System.Drawing.Color.Transparent;
            this.cmbStaffPendingFor.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbStaffPendingFor.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbStaffPendingFor.Location = new System.Drawing.Point(167, 12);
            this.cmbStaffPendingFor.Name = "cmbStaffPendingFor";
            this.cmbStaffPendingFor.Radius = 10;
            this.cmbStaffPendingFor.Size = new System.Drawing.Size(120, 22);
            this.cmbStaffPendingFor.TabIndex = 100;
            this.cmbStaffPendingFor.SelectedIndexChanged += new System.EventHandler(this.cmbStaffPendingFor_SelectedIndexChanged);
            // 
            // rb_add_device
            // 
            this.rb_add_device.BackColor = System.Drawing.Color.Transparent;
            this.rb_add_device.BorderColor = System.Drawing.Color.Transparent;
            this.rb_add_device.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_add_device.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_add_device.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_add_device.ForeColor = System.Drawing.Color.White;
            this.rb_add_device.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_add_device.Location = new System.Drawing.Point(672, 4);
            this.rb_add_device.Name = "rb_add_device";
            this.rb_add_device.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_add_device.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_add_device.Radius = 10;
            this.rb_add_device.Size = new System.Drawing.Size(149, 32);
            this.rb_add_device.TabIndex = 3;
            this.rb_add_device.Text = "Add New Device";
            this.rb_add_device.Click += new System.EventHandler(this.btn_add_device_Click);
            // 
            // chkDeviceIsStaff
            // 
            this.chkDeviceIsStaff.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDeviceIsStaff.Location = new System.Drawing.Point(14, 10);
            this.chkDeviceIsStaff.Name = "chkDeviceIsStaff";
            this.chkDeviceIsStaff.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(204)))), ((int)(((byte)(216)))));
            this.chkDeviceIsStaff.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.chkDeviceIsStaff.Size = new System.Drawing.Size(44, 24);
            this.chkDeviceIsStaff.TabIndex = 93;
            this.chkDeviceIsStaff.CheckedChanged += new System.EventHandler(this.chkDeviceIsStaff_CheckedChanged);
            // 
            // clv_devices
            // 
            this.clv_devices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_devices.Location = new System.Drawing.Point(4, 41);
            this.clv_devices.Name = "clv_devices";
            this.clv_devices.Size = new System.Drawing.Size(817, 501);
            this.clv_devices.TabIndex = 41;
            // 
            // rb_addUser
            // 
            this.rb_addUser.BackColor = System.Drawing.Color.Transparent;
            this.rb_addUser.BorderColor = System.Drawing.Color.Transparent;
            this.rb_addUser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_addUser.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_addUser.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_addUser.ForeColor = System.Drawing.Color.White;
            this.rb_addUser.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_addUser.Location = new System.Drawing.Point(702, 10);
            this.rb_addUser.Name = "rb_addUser";
            this.rb_addUser.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_addUser.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_addUser.Radius = 10;
            this.rb_addUser.Size = new System.Drawing.Size(121, 33);
            this.rb_addUser.TabIndex = 4;
            this.rb_addUser.Text = "Add New User";
            this.rb_addUser.Click += new System.EventHandler(this.btn_addUser_Click);
            // 
            // cmbUserRole
            // 
            this.cmbUserRole.BackColor = System.Drawing.Color.White;
            this.cmbUserRole.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbUserRole.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbUserRole.Location = new System.Drawing.Point(15, 10);
            this.cmbUserRole.Name = "cmbUserRole";
            this.cmbUserRole.Radius = 8;
            this.cmbUserRole.Size = new System.Drawing.Size(130, 33);
            this.cmbUserRole.TabIndex = 94;
            this.cmbUserRole.SelectedIndexChanged += new System.EventHandler(this.cmbUserRole_SelectedIndexChanged);
            // 
            // clv_localUsers
            // 
            this.clv_localUsers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.clv_localUsers.Location = new System.Drawing.Point(3, 53);
            this.clv_localUsers.Name = "clv_localUsers";
            this.clv_localUsers.Size = new System.Drawing.Size(831, 489);
            this.clv_localUsers.TabIndex = 40;
            // 
            // rb_savedebounce
            // 
            this.rb_savedebounce.BackColor = System.Drawing.Color.Transparent;
            this.rb_savedebounce.BorderColor = System.Drawing.Color.Transparent;
            this.rb_savedebounce.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_savedebounce.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_savedebounce.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_savedebounce.ForeColor = System.Drawing.Color.White;
            this.rb_savedebounce.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_savedebounce.Location = new System.Drawing.Point(109, 89);
            this.rb_savedebounce.Name = "rb_savedebounce";
            this.rb_savedebounce.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_savedebounce.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_savedebounce.Radius = 10;
            this.rb_savedebounce.Size = new System.Drawing.Size(120, 32);
            this.rb_savedebounce.TabIndex = 41;
            this.rb_savedebounce.Text = "Save";
            this.rb_savedebounce.Visible = false;
            this.rb_savedebounce.Click += new System.EventHandler(this.btn_savedebounce_Click);
            // 
            // rb_save_retentiondays
            // 
            this.rb_save_retentiondays.BackColor = System.Drawing.Color.Transparent;
            this.rb_save_retentiondays.BorderColor = System.Drawing.Color.Transparent;
            this.rb_save_retentiondays.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_save_retentiondays.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_save_retentiondays.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_save_retentiondays.ForeColor = System.Drawing.Color.White;
            this.rb_save_retentiondays.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_save_retentiondays.Location = new System.Drawing.Point(109, 149);
            this.rb_save_retentiondays.Name = "rb_save_retentiondays";
            this.rb_save_retentiondays.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_save_retentiondays.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_save_retentiondays.Radius = 10;
            this.rb_save_retentiondays.Size = new System.Drawing.Size(120, 32);
            this.rb_save_retentiondays.TabIndex = 69;
            this.rb_save_retentiondays.Text = "Save";
            this.rb_save_retentiondays.Visible = false;
            this.rb_save_retentiondays.Click += new System.EventHandler(this.btn_save_retentiondays_Click);
            // 
            // rb_save_drivefolderid
            // 
            this.rb_save_drivefolderid.BackColor = System.Drawing.Color.Transparent;
            this.rb_save_drivefolderid.BorderColor = System.Drawing.Color.Transparent;
            this.rb_save_drivefolderid.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_save_drivefolderid.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_save_drivefolderid.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_save_drivefolderid.ForeColor = System.Drawing.Color.White;
            this.rb_save_drivefolderid.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_save_drivefolderid.Location = new System.Drawing.Point(208, 84);
            this.rb_save_drivefolderid.Name = "rb_save_drivefolderid";
            this.rb_save_drivefolderid.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_save_drivefolderid.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_save_drivefolderid.Radius = 10;
            this.rb_save_drivefolderid.Size = new System.Drawing.Size(90, 32);
            this.rb_save_drivefolderid.TabIndex = 62;
            this.rb_save_drivefolderid.Text = "Save";
            this.rb_save_drivefolderid.Visible = false;
            this.rb_save_drivefolderid.Click += new System.EventHandler(this.btn_save_drivefolderid_Click);
            // 
            // rb_save_drivecredentials
            // 
            this.rb_save_drivecredentials.BackColor = System.Drawing.Color.Transparent;
            this.rb_save_drivecredentials.BorderColor = System.Drawing.Color.Transparent;
            this.rb_save_drivecredentials.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_save_drivecredentials.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_save_drivecredentials.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_save_drivecredentials.ForeColor = System.Drawing.Color.White;
            this.rb_save_drivecredentials.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_save_drivecredentials.Location = new System.Drawing.Point(705, 31);
            this.rb_save_drivecredentials.Name = "rb_save_drivecredentials";
            this.rb_save_drivecredentials.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_save_drivecredentials.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_save_drivecredentials.Radius = 10;
            this.rb_save_drivecredentials.Size = new System.Drawing.Size(90, 32);
            this.rb_save_drivecredentials.TabIndex = 63;
            this.rb_save_drivecredentials.Text = "Save";
            this.rb_save_drivecredentials.Visible = false;
            this.rb_save_drivecredentials.Click += new System.EventHandler(this.btn_save_drivecredentials_Click);
            // 
            // rb_save_backuptype
            // 
            this.rb_save_backuptype.BackColor = System.Drawing.Color.Transparent;
            this.rb_save_backuptype.BorderColor = System.Drawing.Color.Transparent;
            this.rb_save_backuptype.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rb_save_backuptype.DialogResult = System.Windows.Forms.DialogResult.None;
            this.rb_save_backuptype.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.rb_save_backuptype.ForeColor = System.Drawing.Color.White;
            this.rb_save_backuptype.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.rb_save_backuptype.Location = new System.Drawing.Point(210, 38);
            this.rb_save_backuptype.Name = "rb_save_backuptype";
            this.rb_save_backuptype.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rb_save_backuptype.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.rb_save_backuptype.Radius = 10;
            this.rb_save_backuptype.Size = new System.Drawing.Size(90, 32);
            this.rb_save_backuptype.TabIndex = 73;
            this.rb_save_backuptype.Text = "Save";
            this.rb_save_backuptype.Visible = false;
            this.rb_save_backuptype.Click += new System.EventHandler(this.btn_save_backuptype_Click);
            // 
            // DeviceManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(8)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(880, 723);
            this.Controls.Add(this.pnl_topnav);
            this.Controls.Add(this.pnl_welcome);
            this.Controls.Add(this.pnl_setup);
            this.Controls.Add(this.pnl_attendence);
            this.Controls.Add(this.pnl_staffsManagment);
            this.Controls.Add(this.pnl_deviceManager);
            this.Controls.Add(this.pnl_UserManagement);
            this.Controls.Add(this.pnl_welcome_header);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DeviceManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Device Manager";
            this.Load += new System.EventHandler(this.DeviceManager_Load);
            this.pnl_welcome.ResumeLayout(false);
            this.pnl_welcome_tiles.ResumeLayout(false);
            this.pnl_welcome_header.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnl_topnav.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.pnl_UserManagement.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.pnl_attendence.ResumeLayout(false);
            this.tab_attendence.ResumeLayout(false);
            this.tab_members.ResumeLayout(false);
            this.tab_members.PerformLayout();
            this.tab_memberAttendence.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.tab_manualdoor.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.tab_pendingjobs.ResumeLayout(false);
            this.tab_pendingjobs.PerformLayout();
            this.tab_staffpendingjobs.ResumeLayout(false);
            this.tab_staffpendingjobs.PerformLayout();
            this.pnl_setup.ResumeLayout(false);
            this.card_api.ResumeLayout(false);
            this.card_api.PerformLayout();
            this.card_sync.ResumeLayout(false);
            this.card_sync.PerformLayout();
            this.card_backup.ResumeLayout(false);
            this.card_backup.PerformLayout();
            this.card_maint.ResumeLayout(false);
            this.card_maint.PerformLayout();
            this.card_gate.ResumeLayout(false);
            this.card_gate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.num_retention)).EndInit();
            this.pnl_staffsManagment.ResumeLayout(false);
            this.tab_staff.ResumeLayout(false);
            this.tab_staffmembers.ResumeLayout(false);
            this.tab_staffmembers.PerformLayout();
            this.tab_staffattendence.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.pnl_device.ResumeLayout(false);
            this.pnl_device.PerformLayout();
            this.pnl_deviceManager.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label btn_device_management;
        private System.Windows.Forms.Label btn_staffmanagment;
        private System.Windows.Forms.Label btn_userManagement;
        private smart_attendance_middleware.Controls.RoundedTextBox rtb_APIkey;
        private smart_attendance_middleware.Controls.RoundedButton rb_SaveAPI;
        private System.Windows.Forms.Panel pnl_UserManagement;
        private smart_attendance_middleware.Controls.RoundedButton rb_addUser;
        private System.Windows.Forms.Panel panel3;
        private smart_attendance_middleware.Controls.CardListView clv_localUsers;
        private Label btn_attendencemanagment;
        private Panel pnl_attendence;
        private TabControl tab_attendence;
        private TabPage tab_memberAttendence;
        private TabPage tab_manualdoor;
        private Panel panel6;
        private DateTimePicker date_manualdoor;
        private smart_attendance_middleware.Controls.RoundedButton rb_filter_manualdoor;
        private smart_attendance_middleware.Controls.CardListView clv_manualDoor;
        private TabPage tab_pendingjobs;
        private Label lbl_pendingjobs_info;
        private smart_attendance_middleware.Controls.CardListView clv_pendingJobs;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbPendingDevice;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbPendingFor;
        private TabPage tab_staffpendingjobs;
        private Label lbl_staffpendingjobs_info;
        private smart_attendance_middleware.Controls.CardListView clv_staffPendingJobs;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbStaffPendingDevice;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbStaffPendingFor;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbDoorVisitor;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbStaffStatus;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbStaffSync;
        private smart_attendance_middleware.Controls.ToggleSwitch chkStaffFailure;
        private System.Windows.Forms.Label lbl_chkStaffFailure;
        private Panel panel5;
        private smart_attendance_middleware.Controls.RoundedTextBox rtb_membername;
        private DateTimePicker date_member;
        private smart_attendance_middleware.Controls.RoundedButton rb_filter_member;
        private smart_attendance_middleware.Controls.CardListView clv_memberAttendance;
        private Panel pnl_setup;
        private smart_attendance_middleware.Controls.RoundedTextBox rtb_debounce;
        private smart_attendance_middleware.Controls.RoundedButton rb_savedebounce;
        private Label btn_setupmanage;
        private smart_attendance_middleware.Controls.RoundedButton rb_getdb;
        private smart_attendance_middleware.Controls.RoundedButton rb_settime;
        private System.Windows.Forms.TabPage tab_members;
        private smart_attendance_middleware.Controls.RoundedTextBox rtb_memb_name;
        private smart_attendance_middleware.Controls.RoundedButton rb_flt_members;
        private smart_attendance_middleware.Controls.CardListView clv_members;
        private Panel pnl_staffsManagment;
        private TabControl tab_staff;
        private TabPage tab_staffmembers;
        private smart_attendance_middleware.Controls.RoundedTextBox rtb_staff;
        private smart_attendance_middleware.Controls.RoundedButton rb_stafffilter;
        private smart_attendance_middleware.Controls.CardListView clv_staffs;
        private TabPage tab_staffattendence;
        private Panel panel8;
        private smart_attendance_middleware.Controls.RoundedTextBox rtb_staffname;
        private DateTimePicker date_staffattendence;
        private smart_attendance_middleware.Controls.RoundedButton rb_filterattendencestaff;
        private smart_attendance_middleware.Controls.CardListView clv_staffAttendance;
        private smart_attendance_middleware.Controls.RoundedButton rb_save_drivefolderid;
        private smart_attendance_middleware.Controls.RoundedTextBox rtb_drivecredentials;
        private smart_attendance_middleware.Controls.RoundedTextBox rtb_drivefolderid;
        private smart_attendance_middleware.Controls.RoundedButton rb_save_drivecredentials;
        private CheckBox chk_database;
        private smart_attendance_middleware.Controls.RoundedButton rb_save_backuptype;
        private CheckBox chk_logs;
        private CheckBox chk_glogs;
        private NumericUpDown num_retention;
        private smart_attendance_middleware.Controls.RoundedButton rb_save_retentiondays;
        private smart_attendance_middleware.Controls.RoundedButton rb_backupcleanup;
        private smart_attendance_middleware.Controls.RoundedButton rb_test_connection;
        private System.Windows.Forms.Label lbl_debounce;
        private System.Windows.Forms.Label lbl_retention;
        private System.Windows.Forms.Label lbl_drivefolderid;
        private System.Windows.Forms.Label lbl_drivecredentials;
        private System.Windows.Forms.Label lbl_apikey;
        private System.Windows.Forms.Panel pnl_backup;
        private System.Windows.Forms.Label lbl_backuptype;
        private System.Windows.Forms.Panel card_api;
        private System.Windows.Forms.Label lbl_card_api_title;
        private System.Windows.Forms.Label lbl_card_api_sub;
        private System.Windows.Forms.Panel card_sync;
        private System.Windows.Forms.Label lbl_card_sync_title;
        private System.Windows.Forms.Label lbl_card_sync_sub;
        private System.Windows.Forms.Label lbl_card_sync_retentionlabel;
        private System.Windows.Forms.Label lbl_card_sync_retentionfooter;
        private System.Windows.Forms.Panel card_maint;
        private System.Windows.Forms.Label lbl_card_maint_title;
        private System.Windows.Forms.Label lbl_card_maint_sub;
        private System.Windows.Forms.Label lbl_maint_cleanup_desc;
        private System.Windows.Forms.Label lbl_maint_settime_desc;
        private System.Windows.Forms.Label lbl_maint_getdb_desc;
        private System.Windows.Forms.Panel card_backup;
        private System.Windows.Forms.Label lbl_card_backup_title;
        private System.Windows.Forms.Label lbl_card_backup_sub;
        private System.Windows.Forms.Label lbl_card_backup_type_label;
        private System.Windows.Forms.Label lbl_card_backup_drivefolder_label;
        private System.Windows.Forms.Label lbl_card_backup_creds_label;
        private System.Windows.Forms.Label lbl_backupConnectionStatus;
        private smart_attendance_middleware.Controls.RoundedButton rb_btn_savebackup;
        private System.Windows.Forms.Panel card_gate;
        private System.Windows.Forms.Label lbl_card_gate_title;
        private System.Windows.Forms.Label lbl_card_gate_sub;
        private System.Windows.Forms.Label lbl_gate_entrylabel;
        private System.Windows.Forms.Label lbl_gate_entrydesc;
        private System.Windows.Forms.Label lbl_gate_passwordlabel;
        private System.Windows.Forms.Label lbl_gate_passworddesc;
        private smart_attendance_middleware.Controls.ToggleSwitch chk_requireGateEntryForm;
        private smart_attendance_middleware.Controls.ToggleSwitch chk_requireGatePassword;
        private smart_attendance_middleware.Controls.RoundedButton rb_btn_gate_save;
        private smart_attendance_middleware.Controls.RoundedButton rb_btn_sync_save;
        private smart_attendance_middleware.Controls.ToggleSwitch chkDeviceIsStaff;
        private System.Windows.Forms.Label lbl_staffDevicesOnly;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbUserRole;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbMemberSync;
        private smart_attendance_middleware.Controls.ToggleSwitch chkMemberFailure;
        private System.Windows.Forms.Label lbl_hasFailureOnly;
        private smart_attendance_middleware.Controls.RoundedComboBox rcmb_retention;

        private Panel panel1;
        private System.Windows.Forms.Panel pnl_welcome;
        private System.Windows.Forms.Panel pnl_topnav;
        private System.Windows.Forms.Label nav_device;
        private System.Windows.Forms.Label nav_user;
        private System.Windows.Forms.Label nav_staff;
        private System.Windows.Forms.Label nav_member;
        private System.Windows.Forms.Label nav_setup;
        private System.Windows.Forms.Panel pnl_welcome_header;
        private System.Windows.Forms.Label lbl_welcome_title;
        private System.Windows.Forms.Label lbl_welcome_sub;
        private System.Windows.Forms.Panel pnl_welcome_tiles;
        private smart_attendance_middleware.Controls.TileCard tile_device;
        private smart_attendance_middleware.Controls.TileCard tile_user;
        private smart_attendance_middleware.Controls.TileCard tile_staff;
        private smart_attendance_middleware.Controls.TileCard tile_member;
        private smart_attendance_middleware.Controls.TileCard tile_setup;
        private Panel pnl_device;
        private smart_attendance_middleware.Controls.CardListView clv_devices;
        private smart_attendance_middleware.Controls.RoundedButton rb_add_device;
        private Panel pnl_deviceManager;
    }
}





