namespace smart_attendance_middleware
{
    partial class ModernMessageBox
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlOuter   = new System.Windows.Forms.Panel();
            this.pnlHeader  = new System.Windows.Forms.Panel();
            this.lblTitle   = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlFooter  = new System.Windows.Forms.Panel();
            this.btnLeft    = new System.Windows.Forms.Button();
            this.btnRight   = new System.Windows.Forms.Button();

            this.pnlOuter.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // pnlOuter
            this.pnlOuter.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.pnlOuter.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlOuter.Controls.Add(this.pnlContent);
            this.pnlOuter.Controls.Add(this.pnlFooter);
            this.pnlOuter.Controls.Add(this.pnlHeader);

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.White;
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 76;
            this.pnlHeader.Padding   = new System.Windows.Forms.Padding(24, 16, 24, 0);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlHeader_Paint);

            // lblTitle
            this.lblTitle.AutoSize  = false;
            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Height    = 30;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 13f, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(30, 38, 90);
            this.lblTitle.Text      = "Title";
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;

            // lblSubtitle
            this.lblSubtitle.AutoSize  = false;
            this.lblSubtitle.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.lblSubtitle.Height    = 20;
            this.lblSubtitle.Font      = new System.Drawing.Font("Segoe UI", 9f);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(130, 138, 165);
            this.lblSubtitle.Text      = "Subtitle";
            this.lblSubtitle.BackColor = System.Drawing.Color.Transparent;

            // pnlContent
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Padding   = new System.Windows.Forms.Padding(24, 14, 24, 0);

            // pnlFooter
            this.pnlFooter.BackColor = System.Drawing.Color.White;
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height    = 62;
            this.pnlFooter.Padding   = new System.Windows.Forms.Padding(24, 11, 24, 11);
            this.pnlFooter.Controls.Add(this.btnLeft);
            this.pnlFooter.Controls.Add(this.btnRight);
            this.pnlFooter.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlFooter_Paint);

            // btnRight  (added first so it docks to the right edge)
            this.btnRight.Size      = new System.Drawing.Size(140, 38);
            this.btnRight.Dock      = System.Windows.Forms.DockStyle.Right;
            this.btnRight.BackColor = System.Drawing.Color.FromArgb(30, 38, 90);
            this.btnRight.ForeColor = System.Drawing.Color.White;
            this.btnRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRight.FlatAppearance.BorderSize = 0;
            this.btnRight.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.btnRight.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnRight.Text      = "OK";

            // btnLeft
            this.btnLeft.Size      = new System.Drawing.Size(110, 38);
            this.btnLeft.Dock      = System.Windows.Forms.DockStyle.Left;
            this.btnLeft.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.btnLeft.ForeColor = System.Drawing.Color.FromArgb(107, 114, 128);
            this.btnLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLeft.FlatAppearance.BorderSize = 0;
            this.btnLeft.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.btnLeft.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnLeft.Text      = "Cancel";
            this.btnLeft.Visible   = false;

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(500, 280);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.ShowInTaskbar       = false;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.BackColor           = System.Drawing.Color.FromArgb(245, 246, 250);
            this.Controls.Add(this.pnlOuter);

            this.pnlOuter.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel  pnlOuter;
        private System.Windows.Forms.Panel  pnlHeader;
        private System.Windows.Forms.Label  lblTitle;
        private System.Windows.Forms.Label  lblSubtitle;
        private System.Windows.Forms.Panel  pnlContent;
        private System.Windows.Forms.Panel  pnlFooter;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
    }
}
