namespace smart_attendance_middleware
{
    partial class RenewMembershipForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlAvatar = new System.Windows.Forms.Panel();
            this.lblHeaderSub = new System.Windows.Forms.Label();
            this.lblHeaderName = new System.Windows.Forms.Label();
            this.pnlStatusBadge = new System.Windows.Forms.Panel();
            this.lblStatusBadge = new System.Windows.Forms.Label();
            this.pnlSubCard = new System.Windows.Forms.Panel();
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.lblPackageLabel = new System.Windows.Forms.Label();
            this.lblPackageStar = new System.Windows.Forms.Label();
            this.cmbPackage = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblPeriodicityLabel = new System.Windows.Forms.Label();
            this.lblPeriodicityStar = new System.Windows.Forms.Label();
            this.cmbPeriodicity = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblTrainerLabel = new System.Windows.Forms.Label();
            this.cmbTrainer = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblStartDateLabel = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.pnlPayCard = new System.Windows.Forms.Panel();
            this.lblPayTitle = new System.Windows.Forms.Label();
            this.lblPayDateLabel = new System.Windows.Forms.Label();
            this.dtpPaymentCollectedAt = new System.Windows.Forms.DateTimePicker();
            this.lblPayMethodLabel = new System.Windows.Forms.Label();
            this.cmbPaymentMethod = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblTxnRefLabel = new System.Windows.Forms.Label();
            this.txtTransactionRef = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblTxnHint = new System.Windows.Forms.Label();
            this.pnlSummary = new System.Windows.Forms.Panel();
            this.lblAmountDueTitle = new System.Windows.Forms.Label();
            this.lblAmountDue = new System.Windows.Forms.Label();
            this.lblNewEndDateTitle = new System.Windows.Forms.Label();
            this.lblNewEndDate = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlFooterSep = new System.Windows.Forms.Panel();
            this.btnCancel = new smart_attendance_middleware.Controls.RoundedButton();
            this.btnSave = new smart_attendance_middleware.Controls.RoundedButton();
            this.pnlHeader.SuspendLayout();
            this.pnlStatusBadge.SuspendLayout();
            this.pnlSubCard.SuspendLayout();
            this.pnlPayCard.SuspendLayout();
            this.pnlSummary.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(23)))), ((int)(((byte)(49)))));
            this.pnlHeader.Controls.Add(this.pnlAvatar);
            this.pnlHeader.Controls.Add(this.lblHeaderSub);
            this.pnlHeader.Controls.Add(this.lblHeaderName);
            this.pnlHeader.Controls.Add(this.pnlStatusBadge);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(760, 100);
            this.pnlHeader.TabIndex = 0;
            // 
            // pnlAvatar
            // 
            this.pnlAvatar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(98)))), ((int)(((byte)(255)))));
            this.pnlAvatar.Location = new System.Drawing.Point(20, 16);
            this.pnlAvatar.Name = "pnlAvatar";
            this.pnlAvatar.Size = new System.Drawing.Size(68, 68);
            this.pnlAvatar.TabIndex = 0;
            this.pnlAvatar.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlAvatar_Paint);
            // 
            // lblHeaderSub
            // 
            this.lblHeaderSub.AutoSize = true;
            this.lblHeaderSub.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblHeaderSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(170)))), ((int)(((byte)(210)))));
            this.lblHeaderSub.Location = new System.Drawing.Point(100, 22);
            this.lblHeaderSub.Name = "lblHeaderSub";
            this.lblHeaderSub.Size = new System.Drawing.Size(112, 15);
            this.lblHeaderSub.TabIndex = 1;
            this.lblHeaderSub.Text = "Renew membership";
            // 
            // lblHeaderName
            // 
            this.lblHeaderName.AutoSize = true;
            this.lblHeaderName.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblHeaderName.ForeColor = System.Drawing.Color.White;
            this.lblHeaderName.Location = new System.Drawing.Point(100, 42);
            this.lblHeaderName.Name = "lblHeaderName";
            this.lblHeaderName.Size = new System.Drawing.Size(0, 25);
            this.lblHeaderName.TabIndex = 2;
            // 
            // pnlStatusBadge
            // 
            this.pnlStatusBadge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(45)))), ((int)(((byte)(75)))));
            this.pnlStatusBadge.Controls.Add(this.lblStatusBadge);
            this.pnlStatusBadge.Location = new System.Drawing.Point(540, 34);
            this.pnlStatusBadge.Name = "pnlStatusBadge";
            this.pnlStatusBadge.Size = new System.Drawing.Size(180, 32);
            this.pnlStatusBadge.TabIndex = 3;
            this.pnlStatusBadge.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlStatusBadge_Paint);
            // 
            // lblStatusBadge
            // 
            this.lblStatusBadge.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusBadge.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusBadge.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(186)))), ((int)(((byte)(220)))));
            this.lblStatusBadge.Location = new System.Drawing.Point(0, 0);
            this.lblStatusBadge.Name = "lblStatusBadge";
            this.lblStatusBadge.Size = new System.Drawing.Size(180, 32);
            this.lblStatusBadge.TabIndex = 0;
            this.lblStatusBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.pnlSubCard.Controls.Add(this.lblTrainerLabel);
            this.pnlSubCard.Controls.Add(this.cmbTrainer);
            this.pnlSubCard.Controls.Add(this.lblStartDateLabel);
            this.pnlSubCard.Controls.Add(this.dtpStartDate);
            this.pnlSubCard.Location = new System.Drawing.Point(20, 112);
            this.pnlSubCard.Name = "pnlSubCard";
            this.pnlSubCard.Size = new System.Drawing.Size(720, 176);
            this.pnlSubCard.TabIndex = 1;
            this.pnlSubCard.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSubCard_Paint);
            // 
            // lblSubTitle
            // 
            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblSubTitle.Location = new System.Drawing.Point(16, 14);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Size = new System.Drawing.Size(146, 20);
            this.lblSubTitle.TabIndex = 0;
            this.lblSubTitle.Text = "Subscription details";
            // 
            // lblPackageLabel
            // 
            this.lblPackageLabel.AutoSize = true;
            this.lblPackageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPackageLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblPackageLabel.Location = new System.Drawing.Point(16, 46);
            this.lblPackageLabel.Name = "lblPackageLabel";
            this.lblPackageLabel.Size = new System.Drawing.Size(51, 15);
            this.lblPackageLabel.TabIndex = 1;
            this.lblPackageLabel.Text = "Package";
            // 
            // lblPackageStar
            // 
            this.lblPackageStar.AutoSize = true;
            this.lblPackageStar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPackageStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblPackageStar.Location = new System.Drawing.Point(72, 46);
            this.lblPackageStar.Name = "lblPackageStar";
            this.lblPackageStar.Size = new System.Drawing.Size(15, 15);
            this.lblPackageStar.TabIndex = 2;
            this.lblPackageStar.Text = " *";
            // 
            // cmbPackage
            // 
            this.cmbPackage.BackColor = System.Drawing.Color.Transparent;
            this.cmbPackage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbPackage.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbPackage.Location = new System.Drawing.Point(16, 64);
            this.cmbPackage.Name = "cmbPackage";
            this.cmbPackage.Radius = 10;
            this.cmbPackage.Size = new System.Drawing.Size(336, 36);
            this.cmbPackage.TabIndex = 3;
            this.cmbPackage.SelectedIndexChanged += new System.EventHandler(this.CmbPackage_SelectedIndexChanged);
            // 
            // lblPeriodicityLabel
            // 
            this.lblPeriodicityLabel.AutoSize = true;
            this.lblPeriodicityLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPeriodicityLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblPeriodicityLabel.Location = new System.Drawing.Point(368, 46);
            this.lblPeriodicityLabel.Name = "lblPeriodicityLabel";
            this.lblPeriodicityLabel.Size = new System.Drawing.Size(63, 15);
            this.lblPeriodicityLabel.TabIndex = 4;
            this.lblPeriodicityLabel.Text = "Periodicity";
            // 
            // lblPeriodicityStar
            // 
            this.lblPeriodicityStar.AutoSize = true;
            this.lblPeriodicityStar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPeriodicityStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblPeriodicityStar.Location = new System.Drawing.Point(442, 46);
            this.lblPeriodicityStar.Name = "lblPeriodicityStar";
            this.lblPeriodicityStar.Size = new System.Drawing.Size(15, 15);
            this.lblPeriodicityStar.TabIndex = 5;
            this.lblPeriodicityStar.Text = " *";
            // 
            // cmbPeriodicity
            // 
            this.cmbPeriodicity.BackColor = System.Drawing.Color.Transparent;
            this.cmbPeriodicity.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbPeriodicity.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbPeriodicity.Location = new System.Drawing.Point(368, 64);
            this.cmbPeriodicity.Name = "cmbPeriodicity";
            this.cmbPeriodicity.Radius = 10;
            this.cmbPeriodicity.Size = new System.Drawing.Size(336, 36);
            this.cmbPeriodicity.TabIndex = 6;
            this.cmbPeriodicity.SelectedIndexChanged += new System.EventHandler(this.cmbPeriodicity_SelectedIndexChanged);
            // 
            // lblTrainerLabel
            // 
            this.lblTrainerLabel.AutoSize = true;
            this.lblTrainerLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTrainerLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblTrainerLabel.Location = new System.Drawing.Point(16, 112);
            this.lblTrainerLabel.Name = "lblTrainerLabel";
            this.lblTrainerLabel.Size = new System.Drawing.Size(43, 15);
            this.lblTrainerLabel.TabIndex = 7;
            this.lblTrainerLabel.Text = "Trainer";
            // 
            // cmbTrainer
            // 
            this.cmbTrainer.BackColor = System.Drawing.Color.Transparent;
            this.cmbTrainer.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbTrainer.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbTrainer.Location = new System.Drawing.Point(16, 130);
            this.cmbTrainer.Name = "cmbTrainer";
            this.cmbTrainer.Radius = 10;
            this.cmbTrainer.Size = new System.Drawing.Size(336, 36);
            this.cmbTrainer.TabIndex = 8;
            // 
            // lblStartDateLabel
            // 
            this.lblStartDateLabel.AutoSize = true;
            this.lblStartDateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStartDateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblStartDateLabel.Location = new System.Drawing.Point(368, 112);
            this.lblStartDateLabel.Name = "lblStartDateLabel";
            this.lblStartDateLabel.Size = new System.Drawing.Size(57, 15);
            this.lblStartDateLabel.TabIndex = 9;
            this.lblStartDateLabel.Text = "Start date";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStartDate.Location = new System.Drawing.Point(368, 137);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(336, 24);
            this.dtpStartDate.TabIndex = 10;
            this.dtpStartDate.ValueChanged += new System.EventHandler(this.dtpStartDate_ValueChanged);
            // 
            // pnlPayCard
            // 
            this.pnlPayCard.BackColor = System.Drawing.Color.White;
            this.pnlPayCard.Controls.Add(this.lblPayTitle);
            this.pnlPayCard.Controls.Add(this.lblPayDateLabel);
            this.pnlPayCard.Controls.Add(this.dtpPaymentCollectedAt);
            this.pnlPayCard.Controls.Add(this.lblPayMethodLabel);
            this.pnlPayCard.Controls.Add(this.cmbPaymentMethod);
            this.pnlPayCard.Controls.Add(this.lblTxnRefLabel);
            this.pnlPayCard.Controls.Add(this.txtTransactionRef);
            this.pnlPayCard.Controls.Add(this.lblTxnHint);
            this.pnlPayCard.Location = new System.Drawing.Point(20, 294);
            this.pnlPayCard.Name = "pnlPayCard";
            this.pnlPayCard.Size = new System.Drawing.Size(720, 193);
            this.pnlPayCard.TabIndex = 2;
            this.pnlPayCard.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlPayCard_Paint);
            // 
            // lblPayTitle
            // 
            this.lblPayTitle.AutoSize = true;
            this.lblPayTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblPayTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblPayTitle.Location = new System.Drawing.Point(16, 14);
            this.lblPayTitle.Name = "lblPayTitle";
            this.lblPayTitle.Size = new System.Drawing.Size(121, 20);
            this.lblPayTitle.TabIndex = 0;
            this.lblPayTitle.Text = "Payment details";
            // 
            // lblPayDateLabel
            // 
            this.lblPayDateLabel.AutoSize = true;
            this.lblPayDateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPayDateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblPayDateLabel.Location = new System.Drawing.Point(16, 46);
            this.lblPayDateLabel.Name = "lblPayDateLabel";
            this.lblPayDateLabel.Size = new System.Drawing.Size(118, 15);
            this.lblPayDateLabel.TabIndex = 1;
            this.lblPayDateLabel.Text = "Payment collected at";
            // 
            // dtpPaymentCollectedAt
            // 
            this.dtpPaymentCollectedAt.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dtpPaymentCollectedAt.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpPaymentCollectedAt.Location = new System.Drawing.Point(16, 72);
            this.dtpPaymentCollectedAt.Name = "dtpPaymentCollectedAt";
            this.dtpPaymentCollectedAt.Size = new System.Drawing.Size(336, 24);
            this.dtpPaymentCollectedAt.TabIndex = 2;
            // 
            // lblPayMethodLabel
            // 
            this.lblPayMethodLabel.AutoSize = true;
            this.lblPayMethodLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPayMethodLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblPayMethodLabel.Location = new System.Drawing.Point(368, 46);
            this.lblPayMethodLabel.Name = "lblPayMethodLabel";
            this.lblPayMethodLabel.Size = new System.Drawing.Size(99, 15);
            this.lblPayMethodLabel.TabIndex = 3;
            this.lblPayMethodLabel.Text = "Payment method";
            // 
            // cmbPaymentMethod
            // 
            this.cmbPaymentMethod.BackColor = System.Drawing.Color.Transparent;
            this.cmbPaymentMethod.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbPaymentMethod.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbPaymentMethod.Location = new System.Drawing.Point(368, 64);
            this.cmbPaymentMethod.Name = "cmbPaymentMethod";
            this.cmbPaymentMethod.Radius = 10;
            this.cmbPaymentMethod.Size = new System.Drawing.Size(336, 36);
            this.cmbPaymentMethod.TabIndex = 4;
            this.cmbPaymentMethod.SelectedIndexChanged += new System.EventHandler(this.cmbPaymentMethod_SelectedIndexChanged);
            // 
            // lblTxnRefLabel
            // 
            this.lblTxnRefLabel.AutoSize = true;
            this.lblTxnRefLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTxnRefLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblTxnRefLabel.Location = new System.Drawing.Point(16, 112);
            this.lblTxnRefLabel.Name = "lblTxnRefLabel";
            this.lblTxnRefLabel.Size = new System.Drawing.Size(102, 15);
            this.lblTxnRefLabel.TabIndex = 5;
            this.lblTxnRefLabel.Text = "Transaction ref no";
            // 
            // txtTransactionRef
            // 
            this.txtTransactionRef.BackColor = System.Drawing.Color.Transparent;
            this.txtTransactionRef.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txtTransactionRef.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txtTransactionRef.Location = new System.Drawing.Point(16, 130);
            this.txtTransactionRef.Name = "txtTransactionRef";
            this.txtTransactionRef.Placeholder = "";
            this.txtTransactionRef.Radius = 10;
            this.txtTransactionRef.Size = new System.Drawing.Size(688, 36);
            this.txtTransactionRef.TabIndex = 6;
            // 
            // lblTxnHint
            // 
            this.lblTxnHint.AutoSize = true;
            this.lblTxnHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblTxnHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(166)))), ((int)(((byte)(190)))));
            this.lblTxnHint.Location = new System.Drawing.Point(16, 170);
            this.lblTxnHint.Name = "lblTxnHint";
            this.lblTxnHint.Size = new System.Drawing.Size(182, 13);
            this.lblTxnHint.TabIndex = 7;
            this.lblTxnHint.Text = "Hidden when the method is Cash.";
            // 
            // pnlSummary
            // 
            this.pnlSummary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlSummary.Controls.Add(this.lblAmountDueTitle);
            this.pnlSummary.Controls.Add(this.lblAmountDue);
            this.pnlSummary.Controls.Add(this.lblNewEndDateTitle);
            this.pnlSummary.Controls.Add(this.lblNewEndDate);
            this.pnlSummary.Location = new System.Drawing.Point(20, 495);
            this.pnlSummary.Name = "pnlSummary";
            this.pnlSummary.Size = new System.Drawing.Size(720, 72);
            this.pnlSummary.TabIndex = 3;
            this.pnlSummary.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlSummary_Paint);
            // 
            // lblAmountDueTitle
            // 
            this.lblAmountDueTitle.AutoSize = true;
            this.lblAmountDueTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAmountDueTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblAmountDueTitle.Location = new System.Drawing.Point(16, 10);
            this.lblAmountDueTitle.Name = "lblAmountDueTitle";
            this.lblAmountDueTitle.Size = new System.Drawing.Size(74, 15);
            this.lblAmountDueTitle.TabIndex = 0;
            this.lblAmountDueTitle.Text = "Amount due";
            // 
            // lblAmountDue
            // 
            this.lblAmountDue.AutoSize = true;
            this.lblAmountDue.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblAmountDue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblAmountDue.Location = new System.Drawing.Point(16, 30);
            this.lblAmountDue.Name = "lblAmountDue";
            this.lblAmountDue.Size = new System.Drawing.Size(31, 25);
            this.lblAmountDue.TabIndex = 1;
            this.lblAmountDue.Text = "—";
            // 
            // lblNewEndDateTitle
            // 
            this.lblNewEndDateTitle.AutoSize = true;
            this.lblNewEndDateTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblNewEndDateTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblNewEndDateTitle.Location = new System.Drawing.Point(360, 10);
            this.lblNewEndDateTitle.Name = "lblNewEndDateTitle";
            this.lblNewEndDateTitle.Size = new System.Drawing.Size(80, 15);
            this.lblNewEndDateTitle.TabIndex = 2;
            this.lblNewEndDateTitle.Text = "New end date";
            // 
            // lblNewEndDate
            // 
            this.lblNewEndDate.AutoSize = true;
            this.lblNewEndDate.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblNewEndDate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblNewEndDate.Location = new System.Drawing.Point(360, 30);
            this.lblNewEndDate.Name = "lblNewEndDate";
            this.lblNewEndDate.Size = new System.Drawing.Size(31, 25);
            this.lblNewEndDate.TabIndex = 3;
            this.lblNewEndDate.Text = "—";
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatus.ForeColor = System.Drawing.Color.Firebrick;
            this.lblStatus.Location = new System.Drawing.Point(20, 556);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(720, 20);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlFooterSep
            // 
            this.pnlFooterSep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(238)))));
            this.pnlFooterSep.Location = new System.Drawing.Point(0, 582);
            this.pnlFooterSep.Name = "pnlFooterSep";
            this.pnlFooterSep.Size = new System.Drawing.Size(760, 1);
            this.pnlFooterSep.TabIndex = 5;
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
            this.btnCancel.Location = new System.Drawing.Point(466, 592);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnCancel.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnCancel.Radius = 10;
            this.btnCancel.Size = new System.Drawing.Size(120, 40);
            this.btnCancel.TabIndex = 6;
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
            this.btnSave.Location = new System.Drawing.Point(596, 592);
            this.btnSave.Name = "btnSave";
            this.btnSave.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnSave.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnSave.Radius = 10;
            this.btnSave.Size = new System.Drawing.Size(164, 40);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Renew membership";
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // RenewMembershipForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(760, 644);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlSubCard);
            this.Controls.Add(this.pnlPayCard);
            this.Controls.Add(this.pnlSummary);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pnlFooterSep);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenewMembershipForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Renew Membership";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlStatusBadge.ResumeLayout(false);
            this.pnlSubCard.ResumeLayout(false);
            this.pnlSubCard.PerformLayout();
            this.pnlPayCard.ResumeLayout(false);
            this.pnlPayCard.PerformLayout();
            this.pnlSummary.ResumeLayout(false);
            this.pnlSummary.PerformLayout();
            this.ResumeLayout(false);

        }

        // ── Designer fields ──────────────────────────────────────────────────
        private System.Windows.Forms.Panel      pnlHeader;
        private System.Windows.Forms.Panel      pnlAvatar;
        private System.Windows.Forms.Label      lblHeaderSub;
        private System.Windows.Forms.Label      lblHeaderName;
        private System.Windows.Forms.Panel      pnlStatusBadge;
        private System.Windows.Forms.Label      lblStatusBadge;

        private System.Windows.Forms.Panel      pnlSubCard;
        private System.Windows.Forms.Label      lblSubTitle;
        private System.Windows.Forms.Label      lblPackageLabel;
        private System.Windows.Forms.Label      lblPackageStar;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbPackage;
        private System.Windows.Forms.Label      lblPeriodicityLabel;
        private System.Windows.Forms.Label      lblPeriodicityStar;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbPeriodicity;
        private System.Windows.Forms.Label      lblTrainerLabel;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbTrainer;
        private System.Windows.Forms.Label      lblStartDateLabel;
        private System.Windows.Forms.DateTimePicker dtpStartDate;

        private System.Windows.Forms.Panel      pnlPayCard;
        private System.Windows.Forms.Label      lblPayTitle;
        private System.Windows.Forms.Label      lblPayDateLabel;
        private System.Windows.Forms.DateTimePicker dtpPaymentCollectedAt;
        private System.Windows.Forms.Label      lblPayMethodLabel;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbPaymentMethod;
        private System.Windows.Forms.Label      lblTxnRefLabel;
        private smart_attendance_middleware.Controls.RoundedTextBox  txtTransactionRef;
        private System.Windows.Forms.Label      lblTxnHint;

        private System.Windows.Forms.Panel      pnlSummary;
        private System.Windows.Forms.Label      lblAmountDueTitle;
        private System.Windows.Forms.Label      lblAmountDue;
        private System.Windows.Forms.Label      lblNewEndDateTitle;
        private System.Windows.Forms.Label      lblNewEndDate;

        private System.Windows.Forms.Label      lblStatus;
        private System.Windows.Forms.Panel      pnlFooterSep;
        private smart_attendance_middleware.Controls.RoundedButton btnCancel;
        private smart_attendance_middleware.Controls.RoundedButton btnSave;
    }
}
