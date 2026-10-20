namespace smart_attendance_middleware
{
    partial class DeviceStatusDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components  = new System.ComponentModel.Container();
            this.pnlHeader   = new System.Windows.Forms.Panel();
            this.lblEntityName = new System.Windows.Forms.Label();
            this.lblEntityInfo = new System.Windows.Forms.Label();
            this.pnlFooter   = new System.Windows.Forms.Panel();
            this.btnClose    = new System.Windows.Forms.Button();
            this.mainPanel   = new System.Windows.Forms.FlowLayoutPanel();

            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.pnlHeader.Controls.Add(this.lblEntityName);
            this.pnlHeader.Controls.Add(this.lblEntityInfo);
            this.pnlHeader.Dock    = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height  = 80;
            this.pnlHeader.Name    = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);

            // lblEntityName
            this.lblEntityName.AutoSize  = true;
            this.lblEntityName.Font      = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblEntityName.ForeColor = System.Drawing.Color.White;
            this.lblEntityName.Location  = new System.Drawing.Point(15, 14);
            this.lblEntityName.Name      = "lblEntityName";
            this.lblEntityName.Text      = "Member Name";

            // lblEntityInfo
            this.lblEntityInfo.AutoSize  = true;
            this.lblEntityInfo.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEntityInfo.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.lblEntityInfo.Location  = new System.Drawing.Point(17, 44);
            this.lblEntityInfo.Name      = "lblEntityInfo";
            this.lblEntityInfo.Text      = "Enrollment ID: — | Member ID: —";

            // pnlFooter
            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Controls.Add(this.btnClose);
            this.pnlFooter.Dock   = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height = 70;
            this.pnlFooter.Name   = "pnlFooter";

            // btnClose
            this.btnClose.BackColor    = System.Drawing.Color.White;
            this.btnClose.Cursor       = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor    = System.Drawing.Color.FromArgb(18, 23, 49);
            this.btnClose.Location     = new System.Drawing.Point(560, 17);
            this.btnClose.Name         = "btnClose";
            this.btnClose.Size         = new System.Drawing.Size(100, 36);
            this.btnClose.Text         = "Close";

            // mainPanel
            this.mainPanel.AutoScroll    = true;
            this.mainPanel.AutoSize      = false;
            this.mainPanel.Dock          = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.mainPanel.Margin        = new System.Windows.Forms.Padding(0);
            this.mainPanel.Name          = "mainPanel";
            this.mainPanel.Padding       = new System.Windows.Forms.Padding(15);
            this.mainPanel.WrapContents  = false;

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(245, 246, 247);
            this.ClientSize          = new System.Drawing.Size(700, 680);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.Font                = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox         = true;
            this.MinimizeBox         = true;
            this.MinimumSize         = new System.Drawing.Size(650, 500);
            this.Name                = "DeviceStatusDialog";
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Device Status Detail";

            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel          pnlHeader;
        private System.Windows.Forms.Label          lblEntityName;
        private System.Windows.Forms.Label          lblEntityInfo;
        private System.Windows.Forms.Panel          pnlFooter;
        private System.Windows.Forms.Button         btnClose;
        private System.Windows.Forms.FlowLayoutPanel mainPanel;
    }
}
