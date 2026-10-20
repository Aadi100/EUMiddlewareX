namespace smart_attendance_middleware
{
    partial class FingerManagement
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.pnlFilterBar = new System.Windows.Forms.Panel();
            this.txt_search = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.cmb_deviceFilter = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lbl_missingCount = new System.Windows.Forms.Label();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.cardListViewFingerprints = new smart_attendance_middleware.Controls.CardListView();
            this.pnlHeader.SuspendLayout();
            this.pnlFilterBar.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlCard.SuspendLayout();
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
            this.pnlHeader.Size = new System.Drawing.Size(1100, 68);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(24, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(236, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Fingerprint Management";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(165)))), ((int)(((byte)(215)))));
            this.lblSubtitle.Location = new System.Drawing.Point(24, 42);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(247, 15);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Members who still need a fingerprint enrolled";
            // 
            // pnlFilterBar
            // 
            this.pnlFilterBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlFilterBar.Controls.Add(this.txt_search);
            this.pnlFilterBar.Controls.Add(this.cmb_deviceFilter);
            this.pnlFilterBar.Controls.Add(this.lbl_missingCount);
            this.pnlFilterBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterBar.Location = new System.Drawing.Point(0, 68);
            this.pnlFilterBar.Name = "pnlFilterBar";
            this.pnlFilterBar.Size = new System.Drawing.Size(1100, 56);
            this.pnlFilterBar.TabIndex = 1;
            // 
            // txt_search
            // 
            this.txt_search.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.txt_search.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.txt_search.BackColor = System.Drawing.Color.Transparent;
            this.txt_search.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.txt_search.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.txt_search.Location = new System.Drawing.Point(16, 10);
            this.txt_search.MaxLength = 32767;
            this.txt_search.Name = "txt_search";
            this.txt_search.PasswordChar = '\0';
            this.txt_search.Placeholder = "";
            this.txt_search.Radius = 10;
            this.txt_search.ReadOnly = false;
            this.txt_search.Size = new System.Drawing.Size(300, 36);
            this.txt_search.TabIndex = 0;
            this.txt_search.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.txt_search.UseSystemPasswordChar = false;
            this.txt_search.TextChanged += new System.EventHandler(this.txt_search_TextChanged);
            // 
            // cmb_deviceFilter
            // 
            this.cmb_deviceFilter.BackColor = System.Drawing.Color.Transparent;
            this.cmb_deviceFilter.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmb_deviceFilter.DataSource = null;
            this.cmb_deviceFilter.DisplayMember = "";
            this.cmb_deviceFilter.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmb_deviceFilter.Location = new System.Drawing.Point(328, 10);
            this.cmb_deviceFilter.Name = "cmb_deviceFilter";
            this.cmb_deviceFilter.Radius = 10;
            this.cmb_deviceFilter.SelectedIndex = -1;
            this.cmb_deviceFilter.SelectedItem = null;
            this.cmb_deviceFilter.Size = new System.Drawing.Size(260, 36);
            this.cmb_deviceFilter.TabIndex = 1;
            this.cmb_deviceFilter.ValueMember = "";
            this.cmb_deviceFilter.SelectedIndexChanged += new System.EventHandler(this.cmb_deviceFilter_SelectedIndexChanged);
            // 
            // lbl_missingCount
            // 
            this.lbl_missingCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_missingCount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(228)))), ((int)(((byte)(228)))));
            this.lbl_missingCount.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbl_missingCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lbl_missingCount.Location = new System.Drawing.Point(1010, 16);
            this.lbl_missingCount.Name = "lbl_missingCount";
            this.lbl_missingCount.Size = new System.Drawing.Size(80, 26);
            this.lbl_missingCount.TabIndex = 2;
            this.lbl_missingCount.Text = "● 0 missing";
            this.lbl_missingCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.pnlContent.Controls.Add(this.pnlCard);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 124);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(12);
            this.pnlContent.Size = new System.Drawing.Size(1100, 556);
            this.pnlContent.TabIndex = 2;
            // 
            // pnlCard
            // 
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Controls.Add(this.cardListViewFingerprints);
            this.pnlCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCard.Location = new System.Drawing.Point(12, 12);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(1076, 532);
            this.pnlCard.TabIndex = 0;
            // 
            // cardListViewFingerprints
            // 
            this.cardListViewFingerprints.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.cardListViewFingerprints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardListViewFingerprints.Location = new System.Drawing.Point(0, 0);
            this.cardListViewFingerprints.Name = "cardListViewFingerprints";
            this.cardListViewFingerprints.Size = new System.Drawing.Size(1076, 532);
            this.cardListViewFingerprints.TabIndex = 0;
            // 
            // FingerManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1100, 680);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlFilterBar);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "FingerManagement";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Fingerprint Management";
            this.Load += new System.EventHandler(this.FingerManagement_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlFilterBar.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlFilterBar;
        private smart_attendance_middleware.Controls.RoundedTextBox txt_search;
        private smart_attendance_middleware.Controls.RoundedComboBox cmb_deviceFilter;
        private System.Windows.Forms.Label lbl_missingCount;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlCard;
        private smart_attendance_middleware.Controls.CardListView cardListViewFingerprints;
    }
}


