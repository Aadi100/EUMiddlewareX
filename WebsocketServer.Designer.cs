namespace smart_attendance_middleware
{
    partial class WebsocketServer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lbl_status = new System.Windows.Forms.Label();
            this.tabControlLogs = new System.Windows.Forms.TabControl();
            this.tabConnected = new System.Windows.Forms.TabPage();
            this.lvConnectedDevices = new System.Windows.Forms.ListView();
            this.colDeviceSn = new System.Windows.Forms.ColumnHeader();
            this.colDeviceIp = new System.Windows.Forms.ColumnHeader();
            this.colDeviceStatus = new System.Windows.Forms.ColumnHeader();
            this.colDeviceConnectedSince = new System.Windows.Forms.ColumnHeader();
            this.tabAll = new System.Windows.Forms.TabPage();
            this.txtLogsAll = new System.Windows.Forms.TextBox();
            this.tabIncoming = new System.Windows.Forms.TabPage();
            this.txtLogsIncoming = new System.Windows.Forms.TextBox();
            this.tabOutgoing = new System.Windows.Forms.TabPage();
            this.txtLogsOutgoing = new System.Windows.Forms.TextBox();
            this.tabAttendance = new System.Windows.Forms.TabPage();
            this.txtLogsAttendance = new System.Windows.Forms.TextBox();
            this.pnlCommand = new System.Windows.Forms.Panel();
            this.lbl_sn = new System.Windows.Forms.Label();
            this.cmb_deviceSn = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lbl_cmd = new System.Windows.Forms.Label();
            this.txt_cmd = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lbl_params = new System.Windows.Forms.Label();
            this.txt_params = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.btn_sendCommand = new smart_attendance_middleware.Controls.RoundedButton();
            this.tabControlLogs.SuspendLayout();
            this.tabConnected.SuspendLayout();
            this.tabAll.SuspendLayout();
            this.tabIncoming.SuspendLayout();
            this.tabOutgoing.SuspendLayout();
            this.tabAttendance.SuspendLayout();
            this.pnlCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_status
            // 
            this.lbl_status.AutoSize = true;
            this.lbl_status.Location = new System.Drawing.Point(16, 12);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(97, 13);
            this.lbl_status.TabIndex = 16;
            this.lbl_status.Text = "Status: Initializing...";
            // 
            // tabControlLogs
            // 
            this.tabControlLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlLogs.Controls.Add(this.tabConnected);
            this.tabControlLogs.Controls.Add(this.tabAll);
            this.tabControlLogs.Controls.Add(this.tabIncoming);
            this.tabControlLogs.Controls.Add(this.tabOutgoing);
            this.tabControlLogs.Controls.Add(this.tabAttendance);
            this.tabControlLogs.Location = new System.Drawing.Point(19, 152);
            this.tabControlLogs.Name = "tabControlLogs";
            this.tabControlLogs.SelectedIndex = 0;
            this.tabControlLogs.Size = new System.Drawing.Size(643, 606);
            this.tabControlLogs.TabIndex = 17;
            //
            // tabConnected
            //
            this.tabConnected.Controls.Add(this.lvConnectedDevices);
            this.tabConnected.Location = new System.Drawing.Point(4, 22);
            this.tabConnected.Name = "tabConnected";
            this.tabConnected.Padding = new System.Windows.Forms.Padding(3);
            this.tabConnected.Size = new System.Drawing.Size(635, 580);
            this.tabConnected.TabIndex = 4;
            this.tabConnected.Text = "Connected Devices";
            //
            // lvConnectedDevices
            //
            this.lvConnectedDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDeviceSn,
            this.colDeviceIp,
            this.colDeviceStatus,
            this.colDeviceConnectedSince});
            this.lvConnectedDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvConnectedDevices.FullRowSelect = true;
            this.lvConnectedDevices.GridLines = true;
            this.lvConnectedDevices.HideSelection = false;
            this.lvConnectedDevices.Location = new System.Drawing.Point(3, 3);
            this.lvConnectedDevices.Name = "lvConnectedDevices";
            this.lvConnectedDevices.Size = new System.Drawing.Size(629, 574);
            this.lvConnectedDevices.TabIndex = 0;
            this.lvConnectedDevices.UseCompatibleStateImageBehavior = false;
            this.lvConnectedDevices.View = System.Windows.Forms.View.Details;
            //
            // colDeviceSn
            //
            this.colDeviceSn.Text = "Serial Number";
            this.colDeviceSn.Width = 160;
            //
            // colDeviceIp
            //
            this.colDeviceIp.Text = "IP Address";
            this.colDeviceIp.Width = 130;
            //
            // colDeviceStatus
            //
            this.colDeviceStatus.Text = "Status";
            this.colDeviceStatus.Width = 90;
            //
            // colDeviceConnectedSince
            //
            this.colDeviceConnectedSince.Text = "Connected Since";
            this.colDeviceConnectedSince.Width = 150;
            //
            // tabAll
            //
            this.tabAll.Controls.Add(this.txtLogsAll);
            this.tabAll.Location = new System.Drawing.Point(4, 22);
            this.tabAll.Name = "tabAll";
            this.tabAll.Padding = new System.Windows.Forms.Padding(3);
            this.tabAll.Size = new System.Drawing.Size(635, 580);
            this.tabAll.TabIndex = 0;
            this.tabAll.Text = "All Logs";
            // 
            // txtLogsAll
            // 
            this.txtLogsAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogsAll.Location = new System.Drawing.Point(3, 3);
            this.txtLogsAll.Multiline = true;
            this.txtLogsAll.Name = "txtLogsAll";
            this.txtLogsAll.ReadOnly = true;
            this.txtLogsAll.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogsAll.Size = new System.Drawing.Size(629, 574);
            this.txtLogsAll.TabIndex = 0;
            this.txtLogsAll.WordWrap = false;
            // 
            // tabIncoming
            // 
            this.tabIncoming.Controls.Add(this.txtLogsIncoming);
            this.tabIncoming.Location = new System.Drawing.Point(4, 22);
            this.tabIncoming.Name = "tabIncoming";
            this.tabIncoming.Padding = new System.Windows.Forms.Padding(3);
            this.tabIncoming.Size = new System.Drawing.Size(840, 617);
            this.tabIncoming.TabIndex = 1;
            this.tabIncoming.Text = "Incoming (Devices)";
            // 
            // txtLogsIncoming
            // 
            this.txtLogsIncoming.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogsIncoming.Location = new System.Drawing.Point(3, 3);
            this.txtLogsIncoming.Multiline = true;
            this.txtLogsIncoming.Name = "txtLogsIncoming";
            this.txtLogsIncoming.ReadOnly = true;
            this.txtLogsIncoming.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogsIncoming.Size = new System.Drawing.Size(834, 611);
            this.txtLogsIncoming.TabIndex = 0;
            this.txtLogsIncoming.WordWrap = false;
            // 
            // tabOutgoing
            // 
            this.tabOutgoing.Controls.Add(this.txtLogsOutgoing);
            this.tabOutgoing.Location = new System.Drawing.Point(4, 22);
            this.tabOutgoing.Name = "tabOutgoing";
            this.tabOutgoing.Padding = new System.Windows.Forms.Padding(3);
            this.tabOutgoing.Size = new System.Drawing.Size(840, 617);
            this.tabOutgoing.TabIndex = 2;
            this.tabOutgoing.Text = "Outgoing (Commands)";
            // 
            // txtLogsOutgoing
            // 
            this.txtLogsOutgoing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogsOutgoing.Location = new System.Drawing.Point(3, 3);
            this.txtLogsOutgoing.Multiline = true;
            this.txtLogsOutgoing.Name = "txtLogsOutgoing";
            this.txtLogsOutgoing.ReadOnly = true;
            this.txtLogsOutgoing.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogsOutgoing.Size = new System.Drawing.Size(834, 611);
            this.txtLogsOutgoing.TabIndex = 0;
            this.txtLogsOutgoing.WordWrap = false;
            // 
            // tabAttendance
            // 
            this.tabAttendance.Controls.Add(this.txtLogsAttendance);
            this.tabAttendance.Location = new System.Drawing.Point(4, 22);
            this.tabAttendance.Name = "tabAttendance";
            this.tabAttendance.Padding = new System.Windows.Forms.Padding(3);
            this.tabAttendance.Size = new System.Drawing.Size(840, 617);
            this.tabAttendance.TabIndex = 3;
            this.tabAttendance.Text = "Attendance (Raw Data)";
            // 
            // txtLogsAttendance
            // 
            this.txtLogsAttendance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogsAttendance.Location = new System.Drawing.Point(3, 3);
            this.txtLogsAttendance.Multiline = true;
            this.txtLogsAttendance.Name = "txtLogsAttendance";
            this.txtLogsAttendance.ReadOnly = true;
            this.txtLogsAttendance.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogsAttendance.Size = new System.Drawing.Size(834, 611);
            this.txtLogsAttendance.TabIndex = 0;
            this.txtLogsAttendance.WordWrap = false;
            // 
            // pnlCommand
            // 
            this.pnlCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCommand.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCommand.Controls.Add(this.lbl_sn);
            this.pnlCommand.Controls.Add(this.cmb_deviceSn);
            this.pnlCommand.Controls.Add(this.lbl_cmd);
            this.pnlCommand.Controls.Add(this.txt_cmd);
            this.pnlCommand.Controls.Add(this.lbl_params);
            this.pnlCommand.Controls.Add(this.txt_params);
            this.pnlCommand.Controls.Add(this.btn_sendCommand);
            this.pnlCommand.Location = new System.Drawing.Point(19, 28);
            this.pnlCommand.Name = "pnlCommand";
            this.pnlCommand.Size = new System.Drawing.Size(643, 118);
            this.pnlCommand.TabIndex = 18;
            // 
            // lbl_sn
            // 
            this.lbl_sn.AutoSize = true;
            this.lbl_sn.Location = new System.Drawing.Point(12, 45);
            this.lbl_sn.Name = "lbl_sn";
            this.lbl_sn.Size = new System.Drawing.Size(62, 13);
            this.lbl_sn.TabIndex = 0;
            this.lbl_sn.Text = "Device SN:";
            // 
            // cmb_deviceSn
            //
            this.cmb_deviceSn.Location = new System.Drawing.Point(80, 42);
            this.cmb_deviceSn.Name = "cmb_deviceSn";
            this.cmb_deviceSn.Size = new System.Drawing.Size(180, 42);
            this.cmb_deviceSn.TabIndex = 1;
            // 
            // lbl_cmd
            // 
            this.lbl_cmd.AutoSize = true;
            this.lbl_cmd.Location = new System.Drawing.Point(275, 45);
            this.lbl_cmd.Name = "lbl_cmd";
            this.lbl_cmd.Size = new System.Drawing.Size(34, 13);
            this.lbl_cmd.TabIndex = 2;
            this.lbl_cmd.Text = "CMD:";
            // 
            // txt_cmd
            // 
            this.txt_cmd.Location = new System.Drawing.Point(316, 42);
            this.txt_cmd.Name = "txt_cmd";
            this.txt_cmd.Size = new System.Drawing.Size(140, 42);
            this.txt_cmd.TabIndex = 3;
            // 
            // lbl_params
            // 
            this.lbl_params.AutoSize = true;
            this.lbl_params.Location = new System.Drawing.Point(12, 75);
            this.lbl_params.Name = "lbl_params";
            this.lbl_params.Size = new System.Drawing.Size(76, 13);
            this.lbl_params.TabIndex = 4;
            this.lbl_params.Text = "JSON Params:";
            // 
            // txt_params
            // 
            this.txt_params.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_params.Location = new System.Drawing.Point(80, 72);
            this.txt_params.Name = "txt_params";
            this.txt_params.Size = new System.Drawing.Size(370, 42);
            this.txt_params.TabIndex = 5;
            // 
            // btn_sendCommand
            // 
            this.btn_sendCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_sendCommand.NormalColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.btn_sendCommand.HoverColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btn_sendCommand.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btn_sendCommand.ForeColor = System.Drawing.Color.White;
            this.btn_sendCommand.Location = new System.Drawing.Point(545, 56);
            this.btn_sendCommand.Name = "btn_sendCommand";
            this.btn_sendCommand.Size = new System.Drawing.Size(90, 50);
            this.btn_sendCommand.TabIndex = 6;
            this.btn_sendCommand.Text = "Send Command";
            this.btn_sendCommand.Click += new System.EventHandler(this.btn_sendCommand_Click);
            // 
            // WebsocketServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 770);
            this.Controls.Add(this.pnlCommand);
            this.Controls.Add(this.tabControlLogs);
            this.Controls.Add(this.lbl_status);
            this.Name = "WebsocketServer";
            this.Text = "Websocket Client Manager";
            this.Load += new System.EventHandler(this.WebsocketServer_Load);
            this.tabControlLogs.ResumeLayout(false);
            this.tabConnected.ResumeLayout(false);
            this.tabAll.ResumeLayout(false);
            this.tabAll.PerformLayout();
            this.tabIncoming.ResumeLayout(false);
            this.tabIncoming.PerformLayout();
            this.tabOutgoing.ResumeLayout(false);
            this.tabOutgoing.PerformLayout();
            this.tabAttendance.ResumeLayout(false);
            this.tabAttendance.PerformLayout();
            this.pnlCommand.ResumeLayout(false);
            this.pnlCommand.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.TabControl tabControlLogs;
        private System.Windows.Forms.TabPage tabConnected;
        private System.Windows.Forms.ListView lvConnectedDevices;
        private System.Windows.Forms.ColumnHeader colDeviceSn;
        private System.Windows.Forms.ColumnHeader colDeviceIp;
        private System.Windows.Forms.ColumnHeader colDeviceStatus;
        private System.Windows.Forms.ColumnHeader colDeviceConnectedSince;
        private System.Windows.Forms.TabPage tabAll;
        private System.Windows.Forms.TextBox txtLogsAll;
        private System.Windows.Forms.TabPage tabIncoming;
        private System.Windows.Forms.TextBox txtLogsIncoming;
        private System.Windows.Forms.TabPage tabOutgoing;
        private System.Windows.Forms.TextBox txtLogsOutgoing;
        private System.Windows.Forms.TabPage tabAttendance;
        private System.Windows.Forms.TextBox txtLogsAttendance;
        private System.Windows.Forms.Panel pnlCommand;
        private System.Windows.Forms.Label lbl_sn;
        private smart_attendance_middleware.Controls.RoundedComboBox cmb_deviceSn;
        private System.Windows.Forms.Label lbl_cmd;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_cmd;
        private System.Windows.Forms.Label lbl_params;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_params;
        private smart_attendance_middleware.Controls.RoundedButton btn_sendCommand;
    }
}




