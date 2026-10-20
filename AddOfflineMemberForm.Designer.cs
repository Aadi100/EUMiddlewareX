namespace smart_attendance_middleware
{
    partial class AddOfflineMemberForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
                StopCamera();
                _latestFrame?.Dispose();
                _croppedPhoto?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.lblHeaderSubtitle = new System.Windows.Forms.Label();
            this.pnlPhoto = new System.Windows.Forms.Panel();
            this.lblPhotoTitle = new System.Windows.Forms.Label();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.cmbCameraDevice = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.btnTakePicture = new smart_attendance_middleware.Controls.RoundedButton();
            this.btnBrowsePicture = new smart_attendance_middleware.Controls.RoundedButton();
            this.lblPhotoHint = new System.Windows.Forms.Label();
            this.pnlInfoCard = new System.Windows.Forms.Panel();
            this.lblInfoTitle = new System.Windows.Forms.Label();
            this.lblFirstNameLabel = new System.Windows.Forms.Label();
            this.lblFirstNameStar = new System.Windows.Forms.Label();
            this.txtFirstName = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLastNameLabel = new System.Windows.Forms.Label();
            this.lblLastNameStar = new System.Windows.Forms.Label();
            this.txtLastName = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblGenderLabel = new System.Windows.Forms.Label();
            this.cmbGender = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblPhoneLabel = new System.Windows.Forms.Label();
            this.txtPhone = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblRfidLabel = new System.Windows.Forms.Label();
            this.txtRfid = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.pnlSubCard = new System.Windows.Forms.Panel();
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.lblPackageLabel = new System.Windows.Forms.Label();
            this.lblPackageStar = new System.Windows.Forms.Label();
            this.cmbPackage = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblPeriodicityLabel = new System.Windows.Forms.Label();
            this.lblPeriodicityStar = new System.Windows.Forms.Label();
            this.cmbPeriodicity = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblStartDateLabel = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.lblPaymentDateLabel = new System.Windows.Forms.Label();
            this.dtpPaymentCollectedAt = new System.Windows.Forms.DateTimePicker();
            this.lblPaymentMethodLabel = new System.Windows.Forms.Label();
            this.cmbPaymentMethod = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.pnlSummary = new System.Windows.Forms.Panel();
            this.lblAmountDueTitle = new System.Windows.Forms.Label();
            this.lblAmountDue = new System.Windows.Forms.Label();
            this.lblMembershipRunsTitle = new System.Windows.Forms.Label();
            this.lblMembershipRuns = new System.Windows.Forms.Label();
            this.lblSyncsWhenOnline = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlFooterSep = new System.Windows.Forms.Panel();
            this.btnCancel = new smart_attendance_middleware.Controls.RoundedButton();
            this.btnSave = new smart_attendance_middleware.Controls.RoundedButton();
            this.pnlHeader.SuspendLayout();
            this.pnlPhoto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.pnlInfoCard.SuspendLayout();
            this.pnlSubCard.SuspendLayout();
            this.pnlSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(32)))), ((int)(((byte)(64)))));
            this.pnlHeader.Controls.Add(this.lblHeaderTitle);
            this.pnlHeader.Controls.Add(this.lblHeaderSubtitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(960, 80);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.ForeColor = System.Drawing.Color.White;
            this.lblHeaderTitle.Location = new System.Drawing.Point(22, 14);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(194, 25);
            this.lblHeaderTitle.TabIndex = 0;
            this.lblHeaderTitle.Text = "Add Offline Member";
            // 
            // lblHeaderSubtitle
            // 
            this.lblHeaderSubtitle.AutoSize = true;
            this.lblHeaderSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHeaderSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(178)))), ((int)(((byte)(210)))));
            this.lblHeaderSubtitle.Location = new System.Drawing.Point(24, 48);
            this.lblHeaderSubtitle.Name = "lblHeaderSubtitle";
            this.lblHeaderSubtitle.Size = new System.Drawing.Size(284, 15);
            this.lblHeaderSubtitle.TabIndex = 1;
            this.lblHeaderSubtitle.Text = "Enrollment will sync automatically when back online";
            // 
            // pnlPhoto
            // 
            this.pnlPhoto.BackColor = System.Drawing.Color.White;
            this.pnlPhoto.Controls.Add(this.lblPhotoTitle);
            this.pnlPhoto.Controls.Add(this.picPreview);
            this.pnlPhoto.Controls.Add(this.cmbCameraDevice);
            this.pnlPhoto.Controls.Add(this.btnTakePicture);
            this.pnlPhoto.Controls.Add(this.btnBrowsePicture);
            this.pnlPhoto.Controls.Add(this.lblPhotoHint);
            this.pnlPhoto.Location = new System.Drawing.Point(20, 92);
            this.pnlPhoto.Name = "pnlPhoto";
            this.pnlPhoto.Size = new System.Drawing.Size(200, 494);
            this.pnlPhoto.TabIndex = 1;
            this.pnlPhoto.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPhoto_Paint);
            // 
            // lblPhotoTitle
            // 
            this.lblPhotoTitle.AutoSize = true;
            this.lblPhotoTitle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPhotoTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblPhotoTitle.Location = new System.Drawing.Point(12, 14);
            this.lblPhotoTitle.Name = "lblPhotoTitle";
            this.lblPhotoTitle.Size = new System.Drawing.Size(95, 13);
            this.lblPhotoTitle.TabIndex = 0;
            this.lblPhotoTitle.Text = "MEMBER PHOTO";
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(246)))));
            this.picPreview.Location = new System.Drawing.Point(12, 36);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(176, 176);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 1;
            this.picPreview.TabStop = false;
            this.picPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.picPreview_Paint);
            // 
            // cmbCameraDevice
            // 
            this.cmbCameraDevice.BackColor = System.Drawing.Color.Transparent;
            this.cmbCameraDevice.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbCameraDevice.DataSource = null;
            this.cmbCameraDevice.DisplayMember = "";
            this.cmbCameraDevice.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbCameraDevice.Location = new System.Drawing.Point(12, 224);
            this.cmbCameraDevice.Name = "cmbCameraDevice";
            this.cmbCameraDevice.Radius = 10;
            this.cmbCameraDevice.SelectedIndex = -1;
            this.cmbCameraDevice.SelectedItem = null;
            this.cmbCameraDevice.Size = new System.Drawing.Size(176, 36);
            this.cmbCameraDevice.TabIndex = 2;
            this.cmbCameraDevice.ValueMember = "";
            this.cmbCameraDevice.Visible = false;
            this.cmbCameraDevice.SelectedIndexChanged += new System.EventHandler(this.CmbCameraDevice_SelectedIndexChanged);
            // 
            // btnTakePicture
            // 
            this.btnTakePicture.BackColor = System.Drawing.Color.Transparent;
            this.btnTakePicture.BorderColor = System.Drawing.Color.Transparent;
            this.btnTakePicture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTakePicture.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnTakePicture.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnTakePicture.ForeColor = System.Drawing.Color.White;
            this.btnTakePicture.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(52)))), ((int)(((byte)(110)))));
            this.btnTakePicture.Location = new System.Drawing.Point(12, 268);
            this.btnTakePicture.Name = "btnTakePicture";
            this.btnTakePicture.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(32)))), ((int)(((byte)(64)))));
            this.btnTakePicture.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnTakePicture.Radius = 10;
            this.btnTakePicture.Size = new System.Drawing.Size(176, 40);
            this.btnTakePicture.TabIndex = 3;
            this.btnTakePicture.Text = "Take picture";
            this.btnTakePicture.Click += new System.EventHandler(this.BtnTakePicture_Click);
            // 
            // btnBrowsePicture
            // 
            this.btnBrowsePicture.BackColor = System.Drawing.Color.Transparent;
            this.btnBrowsePicture.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnBrowsePicture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowsePicture.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnBrowsePicture.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnBrowsePicture.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.btnBrowsePicture.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(235)))));
            this.btnBrowsePicture.Location = new System.Drawing.Point(12, 316);
            this.btnBrowsePicture.Name = "btnBrowsePicture";
            this.btnBrowsePicture.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnBrowsePicture.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnBrowsePicture.Radius = 10;
            this.btnBrowsePicture.Size = new System.Drawing.Size(176, 40);
            this.btnBrowsePicture.TabIndex = 4;
            this.btnBrowsePicture.Text = "Browse file";
            this.btnBrowsePicture.Click += new System.EventHandler(this.BtnBrowsePicture_Click);
            // 
            // lblPhotoHint
            // 
            this.lblPhotoHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPhotoHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblPhotoHint.Location = new System.Drawing.Point(12, 366);
            this.lblPhotoHint.Name = "lblPhotoHint";
            this.lblPhotoHint.Size = new System.Drawing.Size(176, 60);
            this.lblPhotoHint.TabIndex = 5;
            this.lblPhotoHint.Text = "JPG or PNG, up to 2 MB.\nA clear face photo helps\nface-reader enrollment.";
            // 
            // pnlInfoCard
            // 
            this.pnlInfoCard.BackColor = System.Drawing.Color.White;
            this.pnlInfoCard.Controls.Add(this.lblInfoTitle);
            this.pnlInfoCard.Controls.Add(this.lblFirstNameLabel);
            this.pnlInfoCard.Controls.Add(this.lblFirstNameStar);
            this.pnlInfoCard.Controls.Add(this.txtFirstName);
            this.pnlInfoCard.Controls.Add(this.lblLastNameLabel);
            this.pnlInfoCard.Controls.Add(this.lblLastNameStar);
            this.pnlInfoCard.Controls.Add(this.txtLastName);
            this.pnlInfoCard.Controls.Add(this.lblGenderLabel);
            this.pnlInfoCard.Controls.Add(this.cmbGender);
            this.pnlInfoCard.Controls.Add(this.lblPhoneLabel);
            this.pnlInfoCard.Controls.Add(this.txtPhone);
            this.pnlInfoCard.Controls.Add(this.lblRfidLabel);
            this.pnlInfoCard.Controls.Add(this.txtRfid);
            this.pnlInfoCard.Location = new System.Drawing.Point(234, 92);
            this.pnlInfoCard.Name = "pnlInfoCard";
            this.pnlInfoCard.Size = new System.Drawing.Size(706, 212);
            this.pnlInfoCard.TabIndex = 2;
            this.pnlInfoCard.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlInfoCard_Paint);
            // 
            // lblInfoTitle
            // 
            this.lblInfoTitle.AutoSize = true;
            this.lblInfoTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblInfoTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblInfoTitle.Location = new System.Drawing.Point(16, 14);
            this.lblInfoTitle.Name = "lblInfoTitle";
            this.lblInfoTitle.Size = new System.Drawing.Size(156, 20);
            this.lblInfoTitle.TabIndex = 0;
            this.lblInfoTitle.Text = "Member information";
            // 
            // lblFirstNameLabel
            // 
            this.lblFirstNameLabel.AutoSize = true;
            this.lblFirstNameLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblFirstNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblFirstNameLabel.Location = new System.Drawing.Point(16, 46);
            this.lblFirstNameLabel.Name = "lblFirstNameLabel";
            this.lblFirstNameLabel.Size = new System.Drawing.Size(62, 15);
            this.lblFirstNameLabel.TabIndex = 1;
            this.lblFirstNameLabel.Text = "First name";
            // 
            // lblFirstNameStar
            // 
            this.lblFirstNameStar.AutoSize = true;
            this.lblFirstNameStar.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblFirstNameStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblFirstNameStar.Location = new System.Drawing.Point(84, 46);
            this.lblFirstNameStar.Name = "lblFirstNameStar";
            this.lblFirstNameStar.Size = new System.Drawing.Size(12, 15);
            this.lblFirstNameStar.TabIndex = 2;
            this.lblFirstNameStar.Text = "*";
            // 
            // txtFirstName
            // 
            this.txtFirstName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txtFirstName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txtFirstName.BackColor = System.Drawing.Color.Transparent;
            this.txtFirstName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtFirstName.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtFirstName.Location = new System.Drawing.Point(16, 66);
            this.txtFirstName.MaxLength = 32767;
            this.txtFirstName.Name = "txtFirstName";
            this.txtFirstName.PasswordChar = '\0';
            this.txtFirstName.Placeholder = "";
            this.txtFirstName.Radius = 10;
            this.txtFirstName.ReadOnly = false;
            this.txtFirstName.Size = new System.Drawing.Size(208, 42);
            this.txtFirstName.TabIndex = 3;
            this.txtFirstName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtFirstName.UseSystemPasswordChar = false;
            // 
            // lblLastNameLabel
            // 
            this.lblLastNameLabel.AutoSize = true;
            this.lblLastNameLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLastNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblLastNameLabel.Location = new System.Drawing.Point(246, 46);
            this.lblLastNameLabel.Name = "lblLastNameLabel";
            this.lblLastNameLabel.Size = new System.Drawing.Size(61, 15);
            this.lblLastNameLabel.TabIndex = 4;
            this.lblLastNameLabel.Text = "Last name";
            // 
            // lblLastNameStar
            // 
            this.lblLastNameStar.AutoSize = true;
            this.lblLastNameStar.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLastNameStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblLastNameStar.Location = new System.Drawing.Point(313, 46);
            this.lblLastNameStar.Name = "lblLastNameStar";
            this.lblLastNameStar.Size = new System.Drawing.Size(12, 15);
            this.lblLastNameStar.TabIndex = 5;
            this.lblLastNameStar.Text = "*";
            // 
            // txtLastName
            // 
            this.txtLastName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txtLastName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txtLastName.BackColor = System.Drawing.Color.Transparent;
            this.txtLastName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtLastName.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtLastName.Location = new System.Drawing.Point(482, 64);
            this.txtLastName.MaxLength = 32767;
            this.txtLastName.Name = "txtLastName";
            this.txtLastName.PasswordChar = '\0';
            this.txtLastName.Placeholder = "";
            this.txtLastName.Radius = 10;
            this.txtLastName.ReadOnly = false;
            this.txtLastName.Size = new System.Drawing.Size(208, 42);
            this.txtLastName.TabIndex = 6;
            this.txtLastName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtLastName.UseSystemPasswordChar = false;
            // 
            // lblGenderLabel
            // 
            this.lblGenderLabel.AutoSize = true;
            this.lblGenderLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblGenderLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblGenderLabel.Location = new System.Drawing.Point(16, 126);
            this.lblGenderLabel.Name = "lblGenderLabel";
            this.lblGenderLabel.Size = new System.Drawing.Size(45, 15);
            this.lblGenderLabel.TabIndex = 7;
            this.lblGenderLabel.Text = "Gender";
            // 
            // cmbGender
            // 
            this.cmbGender.BackColor = System.Drawing.Color.Transparent;
            this.cmbGender.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbGender.DataSource = null;
            this.cmbGender.DisplayMember = "";
            this.cmbGender.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbGender.Location = new System.Drawing.Point(16, 146);
            this.cmbGender.Name = "cmbGender";
            this.cmbGender.Radius = 10;
            this.cmbGender.SelectedIndex = -1;
            this.cmbGender.SelectedItem = null;
            this.cmbGender.Size = new System.Drawing.Size(329, 42);
            this.cmbGender.TabIndex = 8;
            this.cmbGender.ValueMember = "";
            // 
            // lblPhoneLabel
            // 
            this.lblPhoneLabel.AutoSize = true;
            this.lblPhoneLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPhoneLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblPhoneLabel.Location = new System.Drawing.Point(361, 126);
            this.lblPhoneLabel.Name = "lblPhoneLabel";
            this.lblPhoneLabel.Size = new System.Drawing.Size(41, 15);
            this.lblPhoneLabel.TabIndex = 9;
            this.lblPhoneLabel.Text = "Phone";
            // 
            // txtPhone
            // 
            this.txtPhone.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txtPhone.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txtPhone.BackColor = System.Drawing.Color.Transparent;
            this.txtPhone.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtPhone.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtPhone.Location = new System.Drawing.Point(361, 146);
            this.txtPhone.MaxLength = 32767;
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.PasswordChar = '\0';
            this.txtPhone.Placeholder = "";
            this.txtPhone.Radius = 10;
            this.txtPhone.ReadOnly = false;
            this.txtPhone.Size = new System.Drawing.Size(329, 42);
            this.txtPhone.TabIndex = 10;
            this.txtPhone.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtPhone.UseSystemPasswordChar = false;
            // 
            // lblRfidLabel
            // 
            this.lblRfidLabel.AutoSize = true;
            this.lblRfidLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblRfidLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblRfidLabel.Location = new System.Drawing.Point(479, 46);
            this.lblRfidLabel.Name = "lblRfidLabel";
            this.lblRfidLabel.Size = new System.Drawing.Size(57, 15);
            this.lblRfidLabel.TabIndex = 11;
            this.lblRfidLabel.Text = "RFID card";
            // 
            // txtRfid
            // 
            this.txtRfid.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txtRfid.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txtRfid.BackColor = System.Drawing.Color.Transparent;
            this.txtRfid.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtRfid.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtRfid.Location = new System.Drawing.Point(249, 64);
            this.txtRfid.MaxLength = 32767;
            this.txtRfid.Name = "txtRfid";
            this.txtRfid.PasswordChar = '\0';
            this.txtRfid.Placeholder = "";
            this.txtRfid.Radius = 10;
            this.txtRfid.ReadOnly = false;
            this.txtRfid.Size = new System.Drawing.Size(208, 42);
            this.txtRfid.TabIndex = 12;
            this.txtRfid.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txtRfid.UseSystemPasswordChar = false;
            // 
            // pnlSubCard
            // 
            this.pnlSubCard.BackColor = System.Drawing.Color.White;
            this.pnlSubCard.Controls.Add(this.lblSubTitle);
            this.pnlSubCard.Controls.Add(this.lblPackageLabel);
            this.pnlSubCard.Controls.Add(this.lblPackageStar);
            this.pnlSubCard.Controls.Add(this.cmbPackage);
            this.pnlSubCard.Controls.Add(this.lblPeriodicityLabel);
            this.pnlSubCard.Controls.Add(this.lblPeriodicityStar);
            this.pnlSubCard.Controls.Add(this.cmbPeriodicity);
            this.pnlSubCard.Controls.Add(this.lblStartDateLabel);
            this.pnlSubCard.Controls.Add(this.dtpStartDate);
            this.pnlSubCard.Controls.Add(this.lblPaymentDateLabel);
            this.pnlSubCard.Controls.Add(this.dtpPaymentCollectedAt);
            this.pnlSubCard.Controls.Add(this.lblPaymentMethodLabel);
            this.pnlSubCard.Controls.Add(this.cmbPaymentMethod);
            this.pnlSubCard.Location = new System.Drawing.Point(234, 313);
            this.pnlSubCard.Name = "pnlSubCard";
            this.pnlSubCard.Size = new System.Drawing.Size(706, 198);
            this.pnlSubCard.TabIndex = 3;
            this.pnlSubCard.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSubCard_Paint);
            // 
            // lblSubTitle
            // 
            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblSubTitle.Location = new System.Drawing.Point(16, 14);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Size = new System.Drawing.Size(166, 20);
            this.lblSubTitle.TabIndex = 0;
            this.lblSubTitle.Text = "Subscription & payment";
            // 
            // lblPackageLabel
            // 
            this.lblPackageLabel.AutoSize = true;
            this.lblPackageLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPackageLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblPackageLabel.Location = new System.Drawing.Point(249, 46);
            this.lblPackageLabel.Name = "lblPackageLabel";
            this.lblPackageLabel.Size = new System.Drawing.Size(51, 15);
            this.lblPackageLabel.TabIndex = 1;
            this.lblPackageLabel.Text = "Package";
            // 
            // lblPackageStar
            // 
            this.lblPackageStar.AutoSize = true;
            this.lblPackageStar.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPackageStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblPackageStar.Location = new System.Drawing.Point(301, 46);
            this.lblPackageStar.Name = "lblPackageStar";
            this.lblPackageStar.Size = new System.Drawing.Size(12, 15);
            this.lblPackageStar.TabIndex = 2;
            this.lblPackageStar.Text = "*";
            // 
            // cmbPackage
            // 
            this.cmbPackage.BackColor = System.Drawing.Color.Transparent;
            this.cmbPackage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbPackage.DataSource = null;
            this.cmbPackage.DisplayMember = "";
            this.cmbPackage.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbPackage.Location = new System.Drawing.Point(249, 66);
            this.cmbPackage.Name = "cmbPackage";
            this.cmbPackage.Radius = 10;
            this.cmbPackage.SelectedIndex = -1;
            this.cmbPackage.SelectedItem = null;
            this.cmbPackage.Size = new System.Drawing.Size(208, 42);
            this.cmbPackage.TabIndex = 3;
            this.cmbPackage.ValueMember = "";
            this.cmbPackage.SelectedIndexChanged += new System.EventHandler(this.CmbPackage_SelectedIndexChanged);
            // 
            // lblPeriodicityLabel
            // 
            this.lblPeriodicityLabel.AutoSize = true;
            this.lblPeriodicityLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPeriodicityLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblPeriodicityLabel.Location = new System.Drawing.Point(16, 48);
            this.lblPeriodicityLabel.Name = "lblPeriodicityLabel";
            this.lblPeriodicityLabel.Size = new System.Drawing.Size(63, 15);
            this.lblPeriodicityLabel.TabIndex = 4;
            this.lblPeriodicityLabel.Text = "Periodicity";
            // 
            // lblPeriodicityStar
            // 
            this.lblPeriodicityStar.AutoSize = true;
            this.lblPeriodicityStar.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPeriodicityStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblPeriodicityStar.Location = new System.Drawing.Point(80, 48);
            this.lblPeriodicityStar.Name = "lblPeriodicityStar";
            this.lblPeriodicityStar.Size = new System.Drawing.Size(12, 15);
            this.lblPeriodicityStar.TabIndex = 5;
            this.lblPeriodicityStar.Text = "*";
            // 
            // cmbPeriodicity
            // 
            this.cmbPeriodicity.BackColor = System.Drawing.Color.Transparent;
            this.cmbPeriodicity.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbPeriodicity.DataSource = null;
            this.cmbPeriodicity.DisplayMember = "";
            this.cmbPeriodicity.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbPeriodicity.Location = new System.Drawing.Point(16, 66);
            this.cmbPeriodicity.Name = "cmbPeriodicity";
            this.cmbPeriodicity.Radius = 10;
            this.cmbPeriodicity.SelectedIndex = -1;
            this.cmbPeriodicity.SelectedItem = null;
            this.cmbPeriodicity.Size = new System.Drawing.Size(208, 42);
            this.cmbPeriodicity.TabIndex = 6;
            this.cmbPeriodicity.ValueMember = "";
            this.cmbPeriodicity.SelectedIndexChanged += new System.EventHandler(this.cmbPeriodicity_SelectedIndexChanged);
            // 
            // lblStartDateLabel
            // 
            this.lblStartDateLabel.AutoSize = true;
            this.lblStartDateLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStartDateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblStartDateLabel.Location = new System.Drawing.Point(16, 126);
            this.lblStartDateLabel.Name = "lblStartDateLabel";
            this.lblStartDateLabel.Size = new System.Drawing.Size(57, 15);
            this.lblStartDateLabel.TabIndex = 7;
            this.lblStartDateLabel.Text = "Start date";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStartDate.Location = new System.Drawing.Point(16, 146);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(329, 24);
            this.dtpStartDate.TabIndex = 8;
            this.dtpStartDate.ValueChanged += new System.EventHandler(this.dtpStartDate_ValueChanged);
            // 
            // lblPaymentDateLabel
            // 
            this.lblPaymentDateLabel.AutoSize = true;
            this.lblPaymentDateLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPaymentDateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblPaymentDateLabel.Location = new System.Drawing.Point(361, 126);
            this.lblPaymentDateLabel.Name = "lblPaymentDateLabel";
            this.lblPaymentDateLabel.Size = new System.Drawing.Size(118, 15);
            this.lblPaymentDateLabel.TabIndex = 9;
            this.lblPaymentDateLabel.Text = "Payment collected at";
            // 
            // dtpPaymentCollectedAt
            // 
            this.dtpPaymentCollectedAt.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpPaymentCollectedAt.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpPaymentCollectedAt.Location = new System.Drawing.Point(361, 146);
            this.dtpPaymentCollectedAt.Name = "dtpPaymentCollectedAt";
            this.dtpPaymentCollectedAt.Size = new System.Drawing.Size(329, 24);
            this.dtpPaymentCollectedAt.TabIndex = 10;
            // 
            // lblPaymentMethodLabel
            // 
            this.lblPaymentMethodLabel.AutoSize = true;
            this.lblPaymentMethodLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPaymentMethodLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(88)))), ((int)(((byte)(112)))));
            this.lblPaymentMethodLabel.Location = new System.Drawing.Point(482, 46);
            this.lblPaymentMethodLabel.Name = "lblPaymentMethodLabel";
            this.lblPaymentMethodLabel.Size = new System.Drawing.Size(99, 15);
            this.lblPaymentMethodLabel.TabIndex = 11;
            this.lblPaymentMethodLabel.Text = "Payment method";
            // 
            // cmbPaymentMethod
            // 
            this.cmbPaymentMethod.BackColor = System.Drawing.Color.Transparent;
            this.cmbPaymentMethod.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbPaymentMethod.DataSource = null;
            this.cmbPaymentMethod.DisplayMember = "";
            this.cmbPaymentMethod.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbPaymentMethod.Location = new System.Drawing.Point(482, 66);
            this.cmbPaymentMethod.Name = "cmbPaymentMethod";
            this.cmbPaymentMethod.Radius = 10;
            this.cmbPaymentMethod.SelectedIndex = -1;
            this.cmbPaymentMethod.SelectedItem = null;
            this.cmbPaymentMethod.Size = new System.Drawing.Size(208, 42);
            this.cmbPaymentMethod.TabIndex = 12;
            this.cmbPaymentMethod.ValueMember = "";
            // 
            // pnlSummary
            // 
            this.pnlSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlSummary.Controls.Add(this.lblAmountDueTitle);
            this.pnlSummary.Controls.Add(this.lblAmountDue);
            this.pnlSummary.Controls.Add(this.lblMembershipRunsTitle);
            this.pnlSummary.Controls.Add(this.lblMembershipRuns);
            this.pnlSummary.Controls.Add(this.lblSyncsWhenOnline);
            this.pnlSummary.Location = new System.Drawing.Point(234, 524);
            this.pnlSummary.Name = "pnlSummary";
            this.pnlSummary.Size = new System.Drawing.Size(706, 62);
            this.pnlSummary.TabIndex = 4;
            this.pnlSummary.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSummary_Paint);
            // 
            // lblAmountDueTitle
            // 
            this.lblAmountDueTitle.AutoSize = true;
            this.lblAmountDueTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAmountDueTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblAmountDueTitle.Location = new System.Drawing.Point(16, 8);
            this.lblAmountDueTitle.Name = "lblAmountDueTitle";
            this.lblAmountDueTitle.Size = new System.Drawing.Size(74, 15);
            this.lblAmountDueTitle.TabIndex = 0;
            this.lblAmountDueTitle.Text = "Amount due";
            // 
            // lblAmountDue
            // 
            this.lblAmountDue.AutoSize = true;
            this.lblAmountDue.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblAmountDue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblAmountDue.Location = new System.Drawing.Point(16, 26);
            this.lblAmountDue.Name = "lblAmountDue";
            this.lblAmountDue.Size = new System.Drawing.Size(24, 20);
            this.lblAmountDue.TabIndex = 1;
            this.lblAmountDue.Text = "—";
            // 
            // lblMembershipRunsTitle
            // 
            this.lblMembershipRunsTitle.AutoSize = true;
            this.lblMembershipRunsTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMembershipRunsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblMembershipRunsTitle.Location = new System.Drawing.Point(200, 8);
            this.lblMembershipRunsTitle.Name = "lblMembershipRunsTitle";
            this.lblMembershipRunsTitle.Size = new System.Drawing.Size(100, 15);
            this.lblMembershipRunsTitle.TabIndex = 2;
            this.lblMembershipRunsTitle.Text = "Membership runs";
            // 
            // lblMembershipRuns
            // 
            this.lblMembershipRuns.AutoSize = true;
            this.lblMembershipRuns.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblMembershipRuns.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblMembershipRuns.Location = new System.Drawing.Point(200, 26);
            this.lblMembershipRuns.Name = "lblMembershipRuns";
            this.lblMembershipRuns.Size = new System.Drawing.Size(24, 20);
            this.lblMembershipRuns.TabIndex = 3;
            this.lblMembershipRuns.Text = "—";
            // 
            // lblSyncsWhenOnline
            // 
            this.lblSyncsWhenOnline.AutoSize = true;
            this.lblSyncsWhenOnline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(237)))), ((int)(((byte)(213)))));
            this.lblSyncsWhenOnline.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSyncsWhenOnline.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(100)))), ((int)(((byte)(0)))));
            this.lblSyncsWhenOnline.Location = new System.Drawing.Point(534, 20);
            this.lblSyncsWhenOnline.Name = "lblSyncsWhenOnline";
            this.lblSyncsWhenOnline.Padding = new System.Windows.Forms.Padding(8, 3, 8, 3);
            this.lblSyncsWhenOnline.Size = new System.Drawing.Size(131, 21);
            this.lblSyncsWhenOnline.TabIndex = 4;
            this.lblSyncsWhenOnline.Text = "● Syncs when online";
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatus.ForeColor = System.Drawing.Color.Firebrick;
            this.lblStatus.Location = new System.Drawing.Point(20, 580);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(920, 20);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlFooterSep
            // 
            this.pnlFooterSep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(238)))));
            this.pnlFooterSep.Location = new System.Drawing.Point(0, 606);
            this.pnlFooterSep.Name = "pnlFooterSep";
            this.pnlFooterSep.Size = new System.Drawing.Size(960, 1);
            this.pnlFooterSep.TabIndex = 6;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.btnCancel.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(235)))));
            this.btnCancel.Location = new System.Drawing.Point(700, 616);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnCancel.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnCancel.Radius = 10;
            this.btnCancel.Size = new System.Drawing.Size(110, 42);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.Transparent;
            this.btnSave.BorderColor = System.Drawing.Color.Transparent;
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnSave.Location = new System.Drawing.Point(820, 616);
            this.btnSave.Name = "btnSave";
            this.btnSave.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnSave.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnSave.Radius = 10;
            this.btnSave.Size = new System.Drawing.Size(130, 42);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Save member";
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // AddOfflineMemberForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(960, 668);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlPhoto);
            this.Controls.Add(this.pnlInfoCard);
            this.Controls.Add(this.pnlSubCard);
            this.Controls.Add(this.pnlSummary);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pnlFooterSep);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddOfflineMemberForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Offline Member";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlPhoto.ResumeLayout(false);
            this.pnlPhoto.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.pnlInfoCard.ResumeLayout(false);
            this.pnlInfoCard.PerformLayout();
            this.pnlSubCard.ResumeLayout(false);
            this.pnlSubCard.PerformLayout();
            this.pnlSummary.ResumeLayout(false);
            this.pnlSummary.PerformLayout();
            this.ResumeLayout(false);

        }

        // Designer fields
        private System.Windows.Forms.Panel      pnlHeader;
        private System.Windows.Forms.Label      lblHeaderTitle;
        private System.Windows.Forms.Label      lblHeaderSubtitle;

        private System.Windows.Forms.Panel      pnlPhoto;
        private System.Windows.Forms.Label      lblPhotoTitle;
        private System.Windows.Forms.PictureBox picPreview;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbCameraDevice;
        private smart_attendance_middleware.Controls.RoundedButton   btnTakePicture;
        private smart_attendance_middleware.Controls.RoundedButton   btnBrowsePicture;
        private System.Windows.Forms.Label      lblPhotoHint;

        private System.Windows.Forms.Panel      pnlInfoCard;
        private System.Windows.Forms.Label      lblInfoTitle;
        private System.Windows.Forms.Label      lblFirstNameLabel;
        private System.Windows.Forms.Label      lblFirstNameStar;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtFirstName;
        private System.Windows.Forms.Label      lblLastNameLabel;
        private System.Windows.Forms.Label      lblLastNameStar;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtLastName;
        private System.Windows.Forms.Label      lblGenderLabel;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbGender;
        private System.Windows.Forms.Label      lblPhoneLabel;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtPhone;
        private System.Windows.Forms.Label      lblRfidLabel;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtRfid;

        private System.Windows.Forms.Panel      pnlSubCard;
        private System.Windows.Forms.Label      lblSubTitle;
        private System.Windows.Forms.Label      lblPackageLabel;
        private System.Windows.Forms.Label      lblPackageStar;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbPackage;
        private System.Windows.Forms.Label      lblPeriodicityLabel;
        private System.Windows.Forms.Label      lblPeriodicityStar;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbPeriodicity;
        private System.Windows.Forms.Label      lblStartDateLabel;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label      lblPaymentDateLabel;
        private System.Windows.Forms.DateTimePicker dtpPaymentCollectedAt;
        private System.Windows.Forms.Label      lblPaymentMethodLabel;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbPaymentMethod;

        private System.Windows.Forms.Panel      pnlSummary;
        private System.Windows.Forms.Label      lblAmountDueTitle;
        private System.Windows.Forms.Label      lblAmountDue;
        private System.Windows.Forms.Label      lblMembershipRunsTitle;
        private System.Windows.Forms.Label      lblMembershipRuns;
        private System.Windows.Forms.Label      lblSyncsWhenOnline;

        private System.Windows.Forms.Label      lblStatus;
        private System.Windows.Forms.Panel      pnlFooterSep;
        private smart_attendance_middleware.Controls.RoundedButton   btnCancel;
        private smart_attendance_middleware.Controls.RoundedButton   btnSave;
    }
}
