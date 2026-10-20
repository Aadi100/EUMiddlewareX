namespace smart_attendance_middleware
{
    partial class VisitorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.lblInfoOp = new System.Windows.Forms.Label();
            this.lblInfoOpVal = new System.Windows.Forms.Label();
            this.lblInfoSep = new System.Windows.Forms.Label();
            this.lblInfoDev = new System.Windows.Forms.Label();
            this.lblInfoDevVal = new System.Windows.Forms.Label();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.lblMemberLbl = new System.Windows.Forms.Label();
            this.txtMemberName = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.pnlVisitorTog = new System.Windows.Forms.Panel();
            this.chkVisitor = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.lbl_chkVisitor = new System.Windows.Forms.Label();
            this.lblVisitorHint = new System.Windows.Forms.Label();
            this.pnlVisitorFlds = new System.Windows.Forms.Panel();
            this.lblVisNameLbl = new System.Windows.Forms.Label();
            this.txtVisitorName = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblVisPhoneLbl = new System.Windows.Forms.Label();
            this.txtVisitorPhone = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblTrainerLbl = new System.Windows.Forms.Label();
            this.cmbTrainer = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblReasonLbl = new System.Windows.Forms.Label();
            this.txtReason = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnCancel = new smart_attendance_middleware.Controls.RoundedButton();
            this.btnSave = new smart_attendance_middleware.Controls.RoundedButton();
            this.txtLoggedInUser = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.txtDeviceIP = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.pnlHeader.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlVisitorTog.SuspendLayout();
            this.pnlVisitorFlds.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(32)))), ((int)(((byte)(64)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(400, 72);
            this.pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(132, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Visitor Access";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(170)))), ((int)(((byte)(210)))));
            this.lblSubtitle.Location = new System.Drawing.Point(21, 40);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(163, 15);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Log a member or visitor entry";
            // 
            // pnlInfo
            // 
            this.pnlInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(237)))), ((int)(((byte)(248)))));
            this.pnlInfo.Controls.Add(this.lblInfoOp);
            this.pnlInfo.Controls.Add(this.lblInfoOpVal);
            this.pnlInfo.Controls.Add(this.lblInfoSep);
            this.pnlInfo.Controls.Add(this.lblInfoDev);
            this.pnlInfo.Controls.Add(this.lblInfoDevVal);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlInfo.Location = new System.Drawing.Point(0, 72);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(400, 36);
            this.pnlInfo.TabIndex = 1;
            // 
            // lblInfoOp
            // 
            this.lblInfoOp.AutoSize = true;
            this.lblInfoOp.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoOp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(150)))));
            this.lblInfoOp.Location = new System.Drawing.Point(16, 10);
            this.lblInfoOp.Name = "lblInfoOp";
            this.lblInfoOp.Size = new System.Drawing.Size(57, 15);
            this.lblInfoOp.TabIndex = 0;
            this.lblInfoOp.Text = "Operator:";
            // 
            // lblInfoOpVal
            // 
            this.lblInfoOpVal.AutoSize = true;
            this.lblInfoOpVal.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblInfoOpVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblInfoOpVal.Location = new System.Drawing.Point(72, 10);
            this.lblInfoOpVal.Name = "lblInfoOpVal";
            this.lblInfoOpVal.Size = new System.Drawing.Size(0, 15);
            this.lblInfoOpVal.TabIndex = 1;
            // 
            // lblInfoSep
            // 
            this.lblInfoSep.AutoSize = true;
            this.lblInfoSep.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoSep.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(195)))), ((int)(((byte)(220)))));
            this.lblInfoSep.Location = new System.Drawing.Point(195, 10);
            this.lblInfoSep.Name = "lblInfoSep";
            this.lblInfoSep.Size = new System.Drawing.Size(10, 15);
            this.lblInfoSep.TabIndex = 2;
            this.lblInfoSep.Text = "|";
            // 
            // lblInfoDev
            // 
            this.lblInfoDev.AutoSize = true;
            this.lblInfoDev.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblInfoDev.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(110)))), ((int)(((byte)(150)))));
            this.lblInfoDev.Location = new System.Drawing.Point(210, 10);
            this.lblInfoDev.Name = "lblInfoDev";
            this.lblInfoDev.Size = new System.Drawing.Size(45, 15);
            this.lblInfoDev.TabIndex = 3;
            this.lblInfoDev.Text = "Device:";
            // 
            // lblInfoDevVal
            // 
            this.lblInfoDevVal.AutoSize = true;
            this.lblInfoDevVal.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblInfoDevVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblInfoDevVal.Location = new System.Drawing.Point(253, 10);
            this.lblInfoDevVal.Name = "lblInfoDevVal";
            this.lblInfoDevVal.Size = new System.Drawing.Size(0, 15);
            this.lblInfoDevVal.TabIndex = 4;
            // 
            // pnlBody
            // 
            this.pnlBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlBody.Controls.Add(this.lblMemberLbl);
            this.pnlBody.Controls.Add(this.txtMemberName);
            this.pnlBody.Controls.Add(this.pnlVisitorTog);
            this.pnlBody.Controls.Add(this.pnlVisitorFlds);
            this.pnlBody.Controls.Add(this.lblTrainerLbl);
            this.pnlBody.Controls.Add(this.cmbTrainer);
            this.pnlBody.Controls.Add(this.lblReasonLbl);
            this.pnlBody.Controls.Add(this.txtReason);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 108);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(400, 320);
            this.pnlBody.TabIndex = 0;
            // 
            // lblMemberLbl
            // 
            this.lblMemberLbl.AutoSize = true;
            this.lblMemberLbl.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMemberLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblMemberLbl.Location = new System.Drawing.Point(20, 16);
            this.lblMemberLbl.Name = "lblMemberLbl";
            this.lblMemberLbl.Size = new System.Drawing.Size(92, 15);
            this.lblMemberLbl.TabIndex = 0;
            this.lblMemberLbl.Text = "MEMBER NAME";
            // 
            // txtMemberName
            // 
            this.txtMemberName.BackColor = System.Drawing.Color.Transparent;
            this.txtMemberName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtMemberName.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtMemberName.Location = new System.Drawing.Point(20, 34);
            this.txtMemberName.Name = "txtMemberName";
            this.txtMemberName.Placeholder = "";
            this.txtMemberName.Radius = 10;
            this.txtMemberName.Size = new System.Drawing.Size(360, 42);
            this.txtMemberName.TabIndex = 1;
            // 
            // pnlVisitorTog
            // 
            this.pnlVisitorTog.BackColor = System.Drawing.Color.Transparent;
            this.pnlVisitorTog.Controls.Add(this.chkVisitor);
            this.pnlVisitorTog.Controls.Add(this.lbl_chkVisitor);
            this.pnlVisitorTog.Controls.Add(this.lblVisitorHint);
            this.pnlVisitorTog.Location = new System.Drawing.Point(20, 68);
            this.pnlVisitorTog.Name = "pnlVisitorTog";
            this.pnlVisitorTog.Size = new System.Drawing.Size(360, 24);
            this.pnlVisitorTog.TabIndex = 2;
            // 
            // chkVisitor
            // 
            this.chkVisitor.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkVisitor.Location = new System.Drawing.Point(0, 4);
            this.chkVisitor.Name = "chkVisitor";
            this.chkVisitor.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(204)))), ((int)(((byte)(216)))));
            this.chkVisitor.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.chkVisitor.Size = new System.Drawing.Size(44, 24);
            this.chkVisitor.TabIndex = 0;
            this.chkVisitor.CheckedChanged += new System.EventHandler(this.chkVisitor_CheckedChanged);
            // 
            // lbl_chkVisitor
            // 
            this.lbl_chkVisitor.AutoSize = true;
            this.lbl_chkVisitor.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbl_chkVisitor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lbl_chkVisitor.Location = new System.Drawing.Point(50, 6);
            this.lbl_chkVisitor.Name = "lbl_chkVisitor";
            this.lbl_chkVisitor.Size = new System.Drawing.Size(102, 17);
            this.lbl_chkVisitor.TabIndex = 1;
            this.lbl_chkVisitor.Text = "This is a visitor";
            // 
            // lblVisitorHint
            // 
            this.lblVisitorHint.AutoSize = true;
            this.lblVisitorHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblVisitorHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblVisitorHint.Location = new System.Drawing.Point(140, 5);
            this.lblVisitorHint.Name = "lblVisitorHint";
            this.lblVisitorHint.Size = new System.Drawing.Size(173, 15);
            this.lblVisitorHint.TabIndex = 2;
            this.lblVisitorHint.Text = "(visitor fields will appear below)";
            // 
            // pnlVisitorFlds
            // 
            this.pnlVisitorFlds.BackColor = System.Drawing.Color.Transparent;
            this.pnlVisitorFlds.Controls.Add(this.lblVisNameLbl);
            this.pnlVisitorFlds.Controls.Add(this.txtVisitorName);
            this.pnlVisitorFlds.Controls.Add(this.lblVisPhoneLbl);
            this.pnlVisitorFlds.Controls.Add(this.txtVisitorPhone);
            this.pnlVisitorFlds.Location = new System.Drawing.Point(20, 100);
            this.pnlVisitorFlds.Name = "pnlVisitorFlds";
            this.pnlVisitorFlds.Size = new System.Drawing.Size(360, 96);
            this.pnlVisitorFlds.TabIndex = 3;
            this.pnlVisitorFlds.Visible = false;
            // 
            // lblVisNameLbl
            // 
            this.lblVisNameLbl.AutoSize = true;
            this.lblVisNameLbl.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblVisNameLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblVisNameLbl.Location = new System.Drawing.Point(0, 0);
            this.lblVisNameLbl.Name = "lblVisNameLbl";
            this.lblVisNameLbl.Size = new System.Drawing.Size(85, 15);
            this.lblVisNameLbl.TabIndex = 0;
            this.lblVisNameLbl.Text = "VISITOR NAME";
            // 
            // txtVisitorName
            // 
            this.txtVisitorName.BackColor = System.Drawing.Color.Transparent;
            this.txtVisitorName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtVisitorName.Enabled = false;
            this.txtVisitorName.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtVisitorName.Location = new System.Drawing.Point(0, 16);
            this.txtVisitorName.Name = "txtVisitorName";
            this.txtVisitorName.Placeholder = "";
            this.txtVisitorName.Radius = 10;
            this.txtVisitorName.Size = new System.Drawing.Size(360, 42);
            this.txtVisitorName.TabIndex = 1;
            // 
            // lblVisPhoneLbl
            // 
            this.lblVisPhoneLbl.AutoSize = true;
            this.lblVisPhoneLbl.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblVisPhoneLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblVisPhoneLbl.Location = new System.Drawing.Point(0, 50);
            this.lblVisPhoneLbl.Name = "lblVisPhoneLbl";
            this.lblVisPhoneLbl.Size = new System.Drawing.Size(91, 15);
            this.lblVisPhoneLbl.TabIndex = 2;
            this.lblVisPhoneLbl.Text = "VISITOR PHONE";
            // 
            // txtVisitorPhone
            // 
            this.txtVisitorPhone.BackColor = System.Drawing.Color.Transparent;
            this.txtVisitorPhone.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtVisitorPhone.Enabled = false;
            this.txtVisitorPhone.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtVisitorPhone.Location = new System.Drawing.Point(0, 66);
            this.txtVisitorPhone.Name = "txtVisitorPhone";
            this.txtVisitorPhone.Placeholder = "";
            this.txtVisitorPhone.Radius = 10;
            this.txtVisitorPhone.Size = new System.Drawing.Size(360, 42);
            this.txtVisitorPhone.TabIndex = 3;
            // 
            // lblTrainerLbl
            // 
            this.lblTrainerLbl.AutoSize = true;
            this.lblTrainerLbl.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblTrainerLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblTrainerLbl.Location = new System.Drawing.Point(20, 104);
            this.lblTrainerLbl.Name = "lblTrainerLbl";
            this.lblTrainerLbl.Size = new System.Drawing.Size(112, 15);
            this.lblTrainerLbl.TabIndex = 4;
            this.lblTrainerLbl.Text = "TRAINER  (optional)";
            // 
            // cmbTrainer
            // 
            this.cmbTrainer.BackColor = System.Drawing.Color.White;
            this.cmbTrainer.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbTrainer.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbTrainer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.cmbTrainer.Location = new System.Drawing.Point(20, 120);
            this.cmbTrainer.Name = "cmbTrainer";
            this.cmbTrainer.Radius = 10;
            this.cmbTrainer.Size = new System.Drawing.Size(360, 42);
            this.cmbTrainer.TabIndex = 4;
            // 
            // lblReasonLbl
            // 
            this.lblReasonLbl.AutoSize = true;
            this.lblReasonLbl.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblReasonLbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblReasonLbl.Location = new System.Drawing.Point(20, 160);
            this.lblReasonLbl.Name = "lblReasonLbl";
            this.lblReasonLbl.Size = new System.Drawing.Size(110, 15);
            this.lblReasonLbl.TabIndex = 5;
            this.lblReasonLbl.Text = "REASON  (optional)";
            // 
            // txtReason
            // 
            this.txtReason.BackColor = System.Drawing.Color.Transparent;
            this.txtReason.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtReason.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtReason.Location = new System.Drawing.Point(20, 176);
            this.txtReason.Name = "txtReason";
            this.txtReason.Placeholder = "";
            this.txtReason.Radius = 10;
            this.txtReason.Size = new System.Drawing.Size(360, 60);
            this.txtReason.TabIndex = 5;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Controls.Add(this.btnSave);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 428);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(400, 52);
            this.pnlFooter.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.btnCancel.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(235)))));
            this.btnCancel.Location = new System.Drawing.Point(200, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnCancel.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnCancel.Radius = 10;
            this.btnCancel.Size = new System.Drawing.Size(86, 30);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.Transparent;
            this.btnSave.BorderColor = System.Drawing.Color.Transparent;
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnSave.Location = new System.Drawing.Point(294, 11);
            this.btnSave.Name = "btnSave";
            this.btnSave.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnSave.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnSave.Radius = 10;
            this.btnSave.Size = new System.Drawing.Size(98, 30);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Open Gate";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtLoggedInUser
            // 
            this.txtLoggedInUser.BackColor = System.Drawing.Color.Transparent;
            this.txtLoggedInUser.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtLoggedInUser.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtLoggedInUser.Location = new System.Drawing.Point(0, 0);
            this.txtLoggedInUser.Name = "txtLoggedInUser";
            this.txtLoggedInUser.Placeholder = "";
            this.txtLoggedInUser.Radius = 10;
            this.txtLoggedInUser.Size = new System.Drawing.Size(1, 42);
            this.txtLoggedInUser.TabIndex = 4;
            this.txtLoggedInUser.Visible = false;
            // 
            // txtDeviceIP
            // 
            this.txtDeviceIP.BackColor = System.Drawing.Color.Transparent;
            this.txtDeviceIP.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtDeviceIP.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtDeviceIP.Location = new System.Drawing.Point(0, 0);
            this.txtDeviceIP.Name = "txtDeviceIP";
            this.txtDeviceIP.Placeholder = "";
            this.txtDeviceIP.Radius = 10;
            this.txtDeviceIP.Size = new System.Drawing.Size(1, 42);
            this.txtDeviceIP.TabIndex = 5;
            this.txtDeviceIP.Visible = false;
            // 
            // VisitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(400, 480);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlInfo);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.txtLoggedInUser);
            this.Controls.Add(this.txtDeviceIP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VisitorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Visitor Access";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.pnlBody.ResumeLayout(false);
            this.pnlBody.PerformLayout();
            this.pnlVisitorTog.ResumeLayout(false);
            this.pnlVisitorTog.PerformLayout();
            this.pnlVisitorFlds.ResumeLayout(false);
            this.pnlVisitorFlds.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel    pnlHeader;
        private System.Windows.Forms.Label    lblTitle;
        private System.Windows.Forms.Label    lblSubtitle;
        private System.Windows.Forms.Panel    pnlInfo;
        private System.Windows.Forms.Label    lblInfoOp;
        private System.Windows.Forms.Label    lblInfoOpVal;
        private System.Windows.Forms.Label    lblInfoSep;
        private System.Windows.Forms.Label    lblInfoDev;
        private System.Windows.Forms.Label    lblInfoDevVal;
        private System.Windows.Forms.Panel    pnlBody;
        private System.Windows.Forms.Label    lblMemberLbl;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtMemberName;
        private System.Windows.Forms.Panel    pnlVisitorTog;
        private smart_attendance_middleware.Controls.ToggleSwitch chkVisitor;
        private System.Windows.Forms.Label lbl_chkVisitor;
        private System.Windows.Forms.Label    lblVisitorHint;
        private System.Windows.Forms.Panel    pnlVisitorFlds;
        private System.Windows.Forms.Label    lblVisNameLbl;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtVisitorName;
        private System.Windows.Forms.Label    lblVisPhoneLbl;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtVisitorPhone;
        private System.Windows.Forms.Label    lblTrainerLbl;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbTrainer;
        private System.Windows.Forms.Label    lblReasonLbl;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtReason;
        private System.Windows.Forms.Panel    pnlFooter;
        private smart_attendance_middleware.Controls.RoundedButton btnSave;
        private smart_attendance_middleware.Controls.RoundedButton btnCancel;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtLoggedInUser;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtDeviceIP;
    }
}




