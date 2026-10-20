namespace smart_attendance_middleware
{
    partial class MembershipManagement
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.lblFormSubtitle = new System.Windows.Forms.Label();
            this.pnlNav = new System.Windows.Forms.Panel();
            this.tabPanel = new smart_attendance_middleware.Controls.ModernTabPanel();
            this.pnlContentHost = new System.Windows.Forms.Panel();
            this.pnlAllMembersContent = new System.Windows.Forms.Panel();
            this.pnlAllMembersCard = new System.Windows.Forms.Panel();
            this.cardListViewAllMembers = new smart_attendance_middleware.Controls.CardListView();
            this.pnlAllMembersFilters = new System.Windows.Forms.Panel();
            this.txt_allMembersSearch = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.cmbAllMemberStatus = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.cmbAllMemberBiometric = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.cmbAllMemberExpiry = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lbl_allMembersCount = new System.Windows.Forms.Label();
            this.pnlOfflineContent = new System.Windows.Forms.Panel();
            this.pnlOfflineCard = new System.Windows.Forms.Panel();
            this.cardListViewOfflineMembers = new smart_attendance_middleware.Controls.CardListView();
            this.lblOfflineEmpty = new System.Windows.Forms.Label();
            this.pnlOfflineFilters = new System.Windows.Forms.Panel();
            this.btn_addOfflineMember = new smart_attendance_middleware.Controls.RoundedButton();
            this.txt_offlineMemberSearch = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.cmbOfflineServerSync = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.cmbOfflinePackage = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.chkOfflineFailure = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.lbl_chkOfflineFailure = new System.Windows.Forms.Label();
            this.pnlOfflineStats = new System.Windows.Forms.Panel();
            this.pnlStatWaiting = new System.Windows.Forms.Panel();
            this.lblStatWaitingTitle = new System.Windows.Forms.Label();
            this.lblStatWaiting = new System.Windows.Forms.Label();
            this.pnlStatFailed = new System.Windows.Forms.Panel();
            this.lblStatFailedTitle = new System.Windows.Forms.Label();
            this.lblStatFailed = new System.Windows.Forms.Label();
            this.pnlStatSynced = new System.Windows.Forms.Panel();
            this.lblStatSyncedTitle = new System.Windows.Forms.Label();
            this.lblStatSynced = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlNav.SuspendLayout();
            this.pnlContentHost.SuspendLayout();
            this.pnlAllMembersContent.SuspendLayout();
            this.pnlAllMembersCard.SuspendLayout();
            this.pnlAllMembersFilters.SuspendLayout();
            this.pnlOfflineContent.SuspendLayout();
            this.pnlOfflineCard.SuspendLayout();
            this.pnlOfflineFilters.SuspendLayout();
            this.pnlOfflineStats.SuspendLayout();
            this.pnlStatWaiting.SuspendLayout();
            this.pnlStatFailed.SuspendLayout();
            this.pnlStatSynced.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(32)))), ((int)(((byte)(64)))));
            this.pnlHeader.Controls.Add(this.lblFormTitle);
            this.pnlHeader.Controls.Add(this.lblFormSubtitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(980, 58);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblFormTitle
            // 
            this.lblFormTitle.AutoSize = true;
            this.lblFormTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblFormTitle.ForeColor = System.Drawing.Color.White;
            this.lblFormTitle.Location = new System.Drawing.Point(20, 12);
            this.lblFormTitle.Name = "lblFormTitle";
            this.lblFormTitle.Size = new System.Drawing.Size(236, 25);
            this.lblFormTitle.TabIndex = 0;
            this.lblFormTitle.Text = "Membership Management";
            // 
            // lblFormSubtitle
            // 
            this.lblFormSubtitle.AutoSize = true;
            this.lblFormSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblFormSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(165)))), ((int)(((byte)(215)))));
            this.lblFormSubtitle.Location = new System.Drawing.Point(20, 36);
            this.lblFormSubtitle.Name = "lblFormSubtitle";
            this.lblFormSubtitle.Size = new System.Drawing.Size(229, 15);
            this.lblFormSubtitle.TabIndex = 1;
            this.lblFormSubtitle.Text = "Manage members and offline enrollments";
            // 
            // pnlNav
            // 
            this.pnlNav.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlNav.Controls.Add(this.tabPanel);
            this.pnlNav.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlNav.Location = new System.Drawing.Point(0, 58);
            this.pnlNav.Name = "pnlNav";
            this.pnlNav.Size = new System.Drawing.Size(980, 44);
            this.pnlNav.TabIndex = 1;
            // 
            // tabPanel
            // 
            this.tabPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.tabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPanel.Location = new System.Drawing.Point(0, 0);
            this.tabPanel.Name = "tabPanel";
            this.tabPanel.Size = new System.Drawing.Size(980, 44);
            this.tabPanel.TabIndex = 0;
            // 
            // pnlContentHost
            // 
            this.pnlContentHost.Controls.Add(this.pnlAllMembersContent);
            this.pnlContentHost.Controls.Add(this.pnlOfflineContent);
            this.pnlContentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContentHost.Location = new System.Drawing.Point(0, 102);
            this.pnlContentHost.Name = "pnlContentHost";
            this.pnlContentHost.Size = new System.Drawing.Size(980, 558);
            this.pnlContentHost.TabIndex = 2;
            // 
            // pnlAllMembersContent
            // 
            this.pnlAllMembersContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlAllMembersContent.Controls.Add(this.pnlAllMembersCard);
            this.pnlAllMembersContent.Controls.Add(this.pnlAllMembersFilters);
            this.pnlAllMembersContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAllMembersContent.Location = new System.Drawing.Point(0, 0);
            this.pnlAllMembersContent.Name = "pnlAllMembersContent";
            this.pnlAllMembersContent.Size = new System.Drawing.Size(980, 558);
            this.pnlAllMembersContent.TabIndex = 0;
            this.pnlAllMembersContent.Visible = false;
            // 
            // pnlAllMembersCard
            // 
            this.pnlAllMembersCard.BackColor = System.Drawing.Color.White;
            this.pnlAllMembersCard.Controls.Add(this.cardListViewAllMembers);
            this.pnlAllMembersCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAllMembersCard.Location = new System.Drawing.Point(0, 56);
            this.pnlAllMembersCard.Name = "pnlAllMembersCard";
            this.pnlAllMembersCard.Padding = new System.Windows.Forms.Padding(12, 0, 12, 12);
            this.pnlAllMembersCard.Size = new System.Drawing.Size(980, 502);
            this.pnlAllMembersCard.TabIndex = 1;
            // 
            // cardListViewAllMembers
            // 
            this.cardListViewAllMembers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.cardListViewAllMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardListViewAllMembers.Location = new System.Drawing.Point(12, 0);
            this.cardListViewAllMembers.Name = "cardListViewAllMembers";
            this.cardListViewAllMembers.Size = new System.Drawing.Size(956, 490);
            this.cardListViewAllMembers.TabIndex = 0;
            // 
            // pnlAllMembersFilters
            // 
            this.pnlAllMembersFilters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlAllMembersFilters.Controls.Add(this.txt_allMembersSearch);
            this.pnlAllMembersFilters.Controls.Add(this.cmbAllMemberStatus);
            this.pnlAllMembersFilters.Controls.Add(this.cmbAllMemberBiometric);
            this.pnlAllMembersFilters.Controls.Add(this.cmbAllMemberExpiry);
            this.pnlAllMembersFilters.Controls.Add(this.lbl_allMembersCount);
            this.pnlAllMembersFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAllMembersFilters.Location = new System.Drawing.Point(0, 0);
            this.pnlAllMembersFilters.Name = "pnlAllMembersFilters";
            this.pnlAllMembersFilters.Size = new System.Drawing.Size(980, 56);
            this.pnlAllMembersFilters.TabIndex = 0;
            // 
            // txt_allMembersSearch
            // 
            this.txt_allMembersSearch.Location = new System.Drawing.Point(12, 10);
            this.txt_allMembersSearch.Name = "txt_allMembersSearch";
            this.txt_allMembersSearch.Size = new System.Drawing.Size(200, 42);
            this.txt_allMembersSearch.TabIndex = 0;
            this.txt_allMembersSearch.TextChanged += new System.EventHandler(this.txt_allMembersSearch_TextChanged);
            // 
            // cmbAllMemberStatus
            //
            this.cmbAllMemberStatus.Items.AddRange(new object[] {
            "All Status",
            "active",
            "inactive",
            "expired"});
            this.cmbAllMemberStatus.Location = new System.Drawing.Point(224, 10);
            this.cmbAllMemberStatus.Name = "cmbAllMemberStatus";
            this.cmbAllMemberStatus.Size = new System.Drawing.Size(120, 42);
            this.cmbAllMemberStatus.TabIndex = 1;
            this.cmbAllMemberStatus.SelectedIndexChanged += new System.EventHandler(this.cmbAllMembersFilter_Changed);
            // 
            // cmbAllMemberBiometric
            //
            this.cmbAllMemberBiometric.Items.AddRange(new object[] {
            "All Biometric",
            "Has Face",
            "Has Fingerprint",
            "Has RFID",
            "Missing Any"});
            this.cmbAllMemberBiometric.Location = new System.Drawing.Point(356, 10);
            this.cmbAllMemberBiometric.Name = "cmbAllMemberBiometric";
            this.cmbAllMemberBiometric.Size = new System.Drawing.Size(140, 42);
            this.cmbAllMemberBiometric.TabIndex = 2;
            this.cmbAllMemberBiometric.SelectedIndexChanged += new System.EventHandler(this.cmbAllMembersFilter_Changed);
            // 
            // cmbAllMemberExpiry
            //
            this.cmbAllMemberExpiry.Items.AddRange(new object[] {
            "Any Expiry",
            "Expires in 7 days",
            "Expires in 30 days",
            "Expired"});
            this.cmbAllMemberExpiry.Location = new System.Drawing.Point(508, 10);
            this.cmbAllMemberExpiry.Name = "cmbAllMemberExpiry";
            this.cmbAllMemberExpiry.Size = new System.Drawing.Size(150, 42);
            this.cmbAllMemberExpiry.TabIndex = 3;
            this.cmbAllMemberExpiry.SelectedIndexChanged += new System.EventHandler(this.cmbAllMembersFilter_Changed);
            // 
            // lbl_allMembersCount
            // 
            this.lbl_allMembersCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_allMembersCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_allMembersCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lbl_allMembersCount.Location = new System.Drawing.Point(830, 18);
            this.lbl_allMembersCount.Name = "lbl_allMembersCount";
            this.lbl_allMembersCount.Size = new System.Drawing.Size(110, 20);
            this.lbl_allMembersCount.TabIndex = 4;
            this.lbl_allMembersCount.Text = "0 members";
            this.lbl_allMembersCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlOfflineContent
            // 
            this.pnlOfflineContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlOfflineContent.Controls.Add(this.pnlOfflineCard);
            this.pnlOfflineContent.Controls.Add(this.pnlOfflineFilters);
            this.pnlOfflineContent.Controls.Add(this.pnlOfflineStats);
            this.pnlOfflineContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOfflineContent.Location = new System.Drawing.Point(0, 0);
            this.pnlOfflineContent.Name = "pnlOfflineContent";
            this.pnlOfflineContent.Size = new System.Drawing.Size(980, 558);
            this.pnlOfflineContent.TabIndex = 1;
            // 
            // pnlOfflineCard
            // 
            this.pnlOfflineCard.BackColor = System.Drawing.Color.White;
            this.pnlOfflineCard.Controls.Add(this.cardListViewOfflineMembers);
            this.pnlOfflineCard.Controls.Add(this.lblOfflineEmpty);
            this.pnlOfflineCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOfflineCard.Location = new System.Drawing.Point(0, 136);
            this.pnlOfflineCard.Name = "pnlOfflineCard";
            this.pnlOfflineCard.Padding = new System.Windows.Forms.Padding(12, 0, 12, 12);
            this.pnlOfflineCard.Size = new System.Drawing.Size(980, 422);
            this.pnlOfflineCard.TabIndex = 2;
            // 
            // cardListViewOfflineMembers
            // 
            this.cardListViewOfflineMembers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.cardListViewOfflineMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardListViewOfflineMembers.Location = new System.Drawing.Point(12, 0);
            this.cardListViewOfflineMembers.Name = "cardListViewOfflineMembers";
            this.cardListViewOfflineMembers.Size = new System.Drawing.Size(956, 410);
            this.cardListViewOfflineMembers.TabIndex = 0;
            // 
            // lblOfflineEmpty
            // 
            this.lblOfflineEmpty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOfflineEmpty.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblOfflineEmpty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblOfflineEmpty.Location = new System.Drawing.Point(12, 0);
            this.lblOfflineEmpty.Name = "lblOfflineEmpty";
            this.lblOfflineEmpty.Size = new System.Drawing.Size(956, 410);
            this.lblOfflineEmpty.TabIndex = 1;
            this.lblOfflineEmpty.Text = "No offline members yet — use Add Offline Member to enroll someone while the cloud" +
    " is unreachable.";
            this.lblOfflineEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblOfflineEmpty.Visible = false;
            // 
            // pnlOfflineFilters
            // 
            this.pnlOfflineFilters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlOfflineFilters.Controls.Add(this.btn_addOfflineMember);
            this.pnlOfflineFilters.Controls.Add(this.txt_offlineMemberSearch);
            this.pnlOfflineFilters.Controls.Add(this.cmbOfflineServerSync);
            this.pnlOfflineFilters.Controls.Add(this.cmbOfflinePackage);
            this.pnlOfflineFilters.Controls.Add(this.chkOfflineFailure);
            this.pnlOfflineFilters.Controls.Add(this.lbl_chkOfflineFailure);
            this.pnlOfflineFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOfflineFilters.Location = new System.Drawing.Point(0, 80);
            this.pnlOfflineFilters.Name = "pnlOfflineFilters";
            this.pnlOfflineFilters.Size = new System.Drawing.Size(980, 56);
            this.pnlOfflineFilters.TabIndex = 1;
            // 
            // btn_addOfflineMember
            // 
            this.btn_addOfflineMember.NormalColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.btn_addOfflineMember.HoverColor = System.Drawing.Color.FromArgb(15, 130, 58);
            this.btn_addOfflineMember.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_addOfflineMember.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btn_addOfflineMember.ForeColor = System.Drawing.Color.White;
            this.btn_addOfflineMember.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_addOfflineMember.Location = new System.Drawing.Point(803, 12);
            this.btn_addOfflineMember.Name = "btn_addOfflineMember";
            this.btn_addOfflineMember.Size = new System.Drawing.Size(165, 32);
            this.btn_addOfflineMember.TabIndex = 0;
            this.btn_addOfflineMember.Text = "+ Add Offline Member";
            this.btn_addOfflineMember.Click += new System.EventHandler(this.btn_addOfflineMember_Click);
            // 
            // txt_offlineMemberSearch
            // 
            this.txt_offlineMemberSearch.Location = new System.Drawing.Point(12, 10);
            this.txt_offlineMemberSearch.Name = "txt_offlineMemberSearch";
            this.txt_offlineMemberSearch.Size = new System.Drawing.Size(190, 42);
            this.txt_offlineMemberSearch.TabIndex = 1;
            this.txt_offlineMemberSearch.TextChanged += new System.EventHandler(this.txt_offlineMemberSearch_TextChanged);
            // 
            // cmbOfflineServerSync
            //
            this.cmbOfflineServerSync.Items.AddRange(new object[] {
            "All Sync",
            "Synced",
            "Pending"});
            this.cmbOfflineServerSync.Location = new System.Drawing.Point(216, 10);
            this.cmbOfflineServerSync.Name = "cmbOfflineServerSync";
            this.cmbOfflineServerSync.Size = new System.Drawing.Size(120, 42);
            this.cmbOfflineServerSync.TabIndex = 2;
            this.cmbOfflineServerSync.SelectedIndexChanged += new System.EventHandler(this.cmbOfflineFilter_Changed);
            // 
            // cmbOfflinePackage
            //
            this.cmbOfflinePackage.Location = new System.Drawing.Point(348, 10);
            this.cmbOfflinePackage.Name = "cmbOfflinePackage";
            this.cmbOfflinePackage.Size = new System.Drawing.Size(140, 42);
            this.cmbOfflinePackage.TabIndex = 3;
            this.cmbOfflinePackage.SelectedIndexChanged += new System.EventHandler(this.cmbOfflineFilter_Changed);
            // 
            // chkOfflineFailure
            this.chkOfflineFailure.Location = new System.Drawing.Point(502, 16);
            this.chkOfflineFailure.Name = "chkOfflineFailure";
            this.chkOfflineFailure.Size = new System.Drawing.Size(44, 24);
            this.chkOfflineFailure.TabIndex = 4;
            this.chkOfflineFailure.CheckedChanged += new System.EventHandler(this.cmbOfflineFilter_Changed);
            // lbl_chkOfflineFailure
            this.lbl_chkOfflineFailure.AutoSize = true;
            this.lbl_chkOfflineFailure.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_chkOfflineFailure.ForeColor = System.Drawing.Color.FromArgb(80, 88, 112);
            this.lbl_chkOfflineFailure.Location = new System.Drawing.Point(550, 20);
            this.lbl_chkOfflineFailure.Text = "Has failure only";
            // 
            // pnlOfflineStats
            // 
            this.pnlOfflineStats.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlOfflineStats.Controls.Add(this.pnlStatWaiting);
            this.pnlOfflineStats.Controls.Add(this.pnlStatFailed);
            this.pnlOfflineStats.Controls.Add(this.pnlStatSynced);
            this.pnlOfflineStats.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOfflineStats.Location = new System.Drawing.Point(0, 0);
            this.pnlOfflineStats.Name = "pnlOfflineStats";
            this.pnlOfflineStats.Size = new System.Drawing.Size(980, 80);
            this.pnlOfflineStats.TabIndex = 0;
            // 
            // pnlStatWaiting
            // 
            this.pnlStatWaiting.BackColor = System.Drawing.Color.White;
            this.pnlStatWaiting.Controls.Add(this.lblStatWaitingTitle);
            this.pnlStatWaiting.Controls.Add(this.lblStatWaiting);
            this.pnlStatWaiting.Location = new System.Drawing.Point(12, 12);
            this.pnlStatWaiting.Name = "pnlStatWaiting";
            this.pnlStatWaiting.Size = new System.Drawing.Size(300, 56);
            this.pnlStatWaiting.TabIndex = 0;
            // 
            // lblStatWaitingTitle
            // 
            this.lblStatWaitingTitle.AutoSize = true;
            this.lblStatWaitingTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatWaitingTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblStatWaitingTitle.Location = new System.Drawing.Point(12, 8);
            this.lblStatWaitingTitle.Name = "lblStatWaitingTitle";
            this.lblStatWaitingTitle.Size = new System.Drawing.Size(89, 15);
            this.lblStatWaitingTitle.TabIndex = 0;
            this.lblStatWaitingTitle.Text = "Waiting to sync";
            // 
            // lblStatWaiting
            // 
            this.lblStatWaiting.AutoSize = true;
            this.lblStatWaiting.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatWaiting.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(158)))), ((int)(((byte)(11)))));
            this.lblStatWaiting.Location = new System.Drawing.Point(10, 26);
            this.lblStatWaiting.Name = "lblStatWaiting";
            this.lblStatWaiting.Size = new System.Drawing.Size(26, 30);
            this.lblStatWaiting.TabIndex = 1;
            this.lblStatWaiting.Text = "0";
            // 
            // pnlStatFailed
            // 
            this.pnlStatFailed.BackColor = System.Drawing.Color.White;
            this.pnlStatFailed.Controls.Add(this.lblStatFailedTitle);
            this.pnlStatFailed.Controls.Add(this.lblStatFailed);
            this.pnlStatFailed.Location = new System.Drawing.Point(326, 12);
            this.pnlStatFailed.Name = "pnlStatFailed";
            this.pnlStatFailed.Size = new System.Drawing.Size(300, 56);
            this.pnlStatFailed.TabIndex = 1;
            // 
            // lblStatFailedTitle
            // 
            this.lblStatFailedTitle.AutoSize = true;
            this.lblStatFailedTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatFailedTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblStatFailedTitle.Location = new System.Drawing.Point(12, 8);
            this.lblStatFailedTitle.Name = "lblStatFailedTitle";
            this.lblStatFailedTitle.Size = new System.Drawing.Size(38, 15);
            this.lblStatFailedTitle.TabIndex = 0;
            this.lblStatFailedTitle.Text = "Failed";
            // 
            // lblStatFailed
            // 
            this.lblStatFailed.AutoSize = true;
            this.lblStatFailed.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatFailed.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.lblStatFailed.Location = new System.Drawing.Point(10, 26);
            this.lblStatFailed.Name = "lblStatFailed";
            this.lblStatFailed.Size = new System.Drawing.Size(26, 30);
            this.lblStatFailed.TabIndex = 1;
            this.lblStatFailed.Text = "0";
            // 
            // pnlStatSynced
            // 
            this.pnlStatSynced.BackColor = System.Drawing.Color.White;
            this.pnlStatSynced.Controls.Add(this.lblStatSyncedTitle);
            this.pnlStatSynced.Controls.Add(this.lblStatSynced);
            this.pnlStatSynced.Location = new System.Drawing.Point(640, 12);
            this.pnlStatSynced.Name = "pnlStatSynced";
            this.pnlStatSynced.Size = new System.Drawing.Size(300, 56);
            this.pnlStatSynced.TabIndex = 2;
            // 
            // lblStatSyncedTitle
            // 
            this.lblStatSyncedTitle.AutoSize = true;
            this.lblStatSyncedTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatSyncedTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblStatSyncedTitle.Location = new System.Drawing.Point(12, 8);
            this.lblStatSyncedTitle.Name = "lblStatSyncedTitle";
            this.lblStatSyncedTitle.Size = new System.Drawing.Size(78, 15);
            this.lblStatSyncedTitle.TabIndex = 0;
            this.lblStatSyncedTitle.Text = "Synced today";
            // 
            // lblStatSynced
            // 
            this.lblStatSynced.AutoSize = true;
            this.lblStatSynced.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatSynced.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.lblStatSynced.Location = new System.Drawing.Point(10, 26);
            this.lblStatSynced.Name = "lblStatSynced";
            this.lblStatSynced.Size = new System.Drawing.Size(26, 30);
            this.lblStatSynced.TabIndex = 1;
            this.lblStatSynced.Text = "0";
            // 
            // MembershipManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(980, 660);
            this.Controls.Add(this.pnlContentHost);
            this.Controls.Add(this.pnlNav);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "MembershipManagement";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Membership Management";
            this.Load += new System.EventHandler(this.MembershipManagement_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlNav.ResumeLayout(false);
            this.pnlContentHost.ResumeLayout(false);
            this.pnlAllMembersContent.ResumeLayout(false);
            this.pnlAllMembersCard.ResumeLayout(false);
            this.pnlAllMembersFilters.ResumeLayout(false);
            this.pnlAllMembersFilters.PerformLayout();
            this.pnlOfflineContent.ResumeLayout(false);
            this.pnlOfflineCard.ResumeLayout(false);
            this.pnlOfflineFilters.ResumeLayout(false);
            this.pnlOfflineFilters.PerformLayout();
            this.pnlOfflineStats.ResumeLayout(false);
            this.pnlStatWaiting.ResumeLayout(false);
            this.pnlStatWaiting.PerformLayout();
            this.pnlStatFailed.ResumeLayout(false);
            this.pnlStatFailed.PerformLayout();
            this.pnlStatSynced.ResumeLayout(false);
            this.pnlStatSynced.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblFormTitle;
        private System.Windows.Forms.Label lblFormSubtitle;
        private System.Windows.Forms.Panel pnlNav;
        private smart_attendance_middleware.Controls.ModernTabPanel tabPanel;
        private System.Windows.Forms.Panel pnlContentHost;
        private System.Windows.Forms.Panel pnlOfflineContent;
        private System.Windows.Forms.Panel pnlOfflineStats;
        private System.Windows.Forms.Panel pnlStatWaiting;
        private System.Windows.Forms.Label lblStatWaitingTitle;
        private System.Windows.Forms.Label lblStatWaiting;
        private System.Windows.Forms.Panel pnlStatFailed;
        private System.Windows.Forms.Label lblStatFailedTitle;
        private System.Windows.Forms.Label lblStatFailed;
        private System.Windows.Forms.Panel pnlStatSynced;
        private System.Windows.Forms.Label lblStatSyncedTitle;
        private System.Windows.Forms.Label lblStatSynced;
        private System.Windows.Forms.Panel pnlOfflineFilters;
        private smart_attendance_middleware.Controls.RoundedButton btn_addOfflineMember;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_offlineMemberSearch;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbOfflineServerSync;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbOfflinePackage;
        private smart_attendance_middleware.Controls.ToggleSwitch chkOfflineFailure;
        private System.Windows.Forms.Label lbl_chkOfflineFailure;
        private System.Windows.Forms.Panel pnlOfflineCard;
        private System.Windows.Forms.Label lblOfflineEmpty;
        private smart_attendance_middleware.Controls.CardListView cardListViewOfflineMembers;
        private System.Windows.Forms.Panel pnlAllMembersContent;
        private System.Windows.Forms.Panel pnlAllMembersFilters;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_allMembersSearch;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbAllMemberStatus;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbAllMemberBiometric;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbAllMemberExpiry;
        private System.Windows.Forms.Label lbl_allMembersCount;
        private System.Windows.Forms.Panel pnlAllMembersCard;
        private smart_attendance_middleware.Controls.CardListView cardListViewAllMembers;
    }
}




