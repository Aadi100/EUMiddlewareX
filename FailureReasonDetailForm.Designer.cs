namespace smart_attendance_middleware
{
    partial class FailureReasonDetailForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components         = new System.ComponentModel.Container();
            this.pnlHeader          = new System.Windows.Forms.Panel();
            this.lblHeaderTitle     = new System.Windows.Forms.Label();
            this.lblReasonTitle     = new System.Windows.Forms.Label();
            this.pnlReasonBorder    = new System.Windows.Forms.Panel();
            this.rtbReason          = new System.Windows.Forms.RichTextBox();
            this.btnClose           = new smart_attendance_middleware.Controls.RoundedButton();

            this.pnlHeader.SuspendLayout();
            this.pnlReasonBorder.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 60;
            this.pnlHeader.Controls.Add(this.lblHeaderTitle);

            // lblHeaderTitle
            this.lblHeaderTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblHeaderTitle.ForeColor = System.Drawing.Color.White;
            this.lblHeaderTitle.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblHeaderTitle.Padding   = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.lblHeaderTitle.Text      = "Sync Failure Detail";

            // lblReasonTitle
            this.lblReasonTitle.AutoSize  = true;
            this.lblReasonTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblReasonTitle.ForeColor = System.Drawing.Color.FromArgb(80, 85, 110);
            this.lblReasonTitle.Location  = new System.Drawing.Point(24, 70);
            this.lblReasonTitle.Text      = "Failure Reason";

            // pnlReasonBorder
            this.pnlReasonBorder.BackColor   = System.Drawing.Color.White;
            this.pnlReasonBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlReasonBorder.Location    = new System.Drawing.Point(24, 92);
            this.pnlReasonBorder.Size        = new System.Drawing.Size(592, 74);
            this.pnlReasonBorder.Controls.Add(this.rtbReason);

            // rtbReason
            this.rtbReason.BackColor   = System.Drawing.Color.White;
            this.rtbReason.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbReason.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.rtbReason.Location    = new System.Drawing.Point(6, 4);
            this.rtbReason.ReadOnly    = true;
            this.rtbReason.ScrollBars  = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbReason.Size        = new System.Drawing.Size(578, 66);
            this.rtbReason.WordWrap    = true;

            // btnClose
            this.btnClose.NormalColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.btnClose.HoverColor  = System.Drawing.Color.FromArgb(30, 38, 80);
            this.btnClose.ForeColor   = System.Drawing.Color.White;
            this.btnClose.Font        = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.Cursor      = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Location    = new System.Drawing.Point(265, 180);
            this.btnClose.Size        = new System.Drawing.Size(110, 36);
            this.btnClose.Text        = "Close";
            this.btnClose.Click      += new System.EventHandler(this.BtnClose_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(248, 249, 252);
            this.ClientSize          = new System.Drawing.Size(640, 230);
            this.Font                = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Sync Failure Detail";

            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.lblReasonTitle);
            this.Controls.Add(this.pnlReasonBorder);
            this.Controls.Add(this.btnClose);

            this.pnlHeader.ResumeLayout(false);
            this.pnlReasonBorder.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel       pnlHeader;
        private System.Windows.Forms.Label       lblHeaderTitle;
        private System.Windows.Forms.Label       lblReasonTitle;
        private System.Windows.Forms.Panel       pnlReasonBorder;
        private System.Windows.Forms.RichTextBox rtbReason;
        private smart_attendance_middleware.Controls.RoundedButton btnClose;
    }
}
