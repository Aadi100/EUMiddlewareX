namespace smart_attendance_middleware
{
    partial class CropForm
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
            this.components       = new System.ComponentModel.Container();

            this.pnlHeader        = new System.Windows.Forms.Panel();
            this.lblTitle         = new System.Windows.Forms.Label();
            this.lblHint          = new System.Windows.Forms.Label();
            this.picCanvas        = new System.Windows.Forms.PictureBox();
            this.pnlFooter        = new System.Windows.Forms.Panel();
            this.btnCrop          = new smart_attendance_middleware.Controls.RoundedButton();
            this.btnCancel        = new smart_attendance_middleware.Controls.RoundedButton();

            ((System.ComponentModel.ISupportInitialize)(this.picCanvas)).BeginInit();
            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(18, 23, 49);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 56;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblHint);

            // lblTitle
            this.lblTitle.AutoSize  = true;
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location  = new System.Drawing.Point(16, 10);
            this.lblTitle.Text      = "Crop Photo";

            // lblHint
            this.lblHint.AutoSize  = true;
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(190, 195, 220);
            this.lblHint.Font      = new System.Drawing.Font("Segoe UI", 8F);
            this.lblHint.Location  = new System.Drawing.Point(16, 34);
            this.lblHint.Text      = "Drag inside the circle to reposition · drag the corner handle to resize";

            // picCanvas — size/location set dynamically in constructor
            this.picCanvas.BackColor   = System.Drawing.Color.Black;
            this.picCanvas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picCanvas.SizeMode    = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picCanvas.Location    = new System.Drawing.Point(16, 56);
            this.picCanvas.Size        = new System.Drawing.Size(640, 480);
            this.picCanvas.Paint      += new System.Windows.Forms.PaintEventHandler(this.PicCanvas_Paint);
            this.picCanvas.MouseDown  += new System.Windows.Forms.MouseEventHandler(this.PicCanvas_MouseDown);
            this.picCanvas.MouseMove  += new System.Windows.Forms.MouseEventHandler(this.PicCanvas_MouseMove);
            this.picCanvas.MouseUp    += new System.Windows.Forms.MouseEventHandler(this.PicCanvas_MouseUp);

            // pnlFooter — docked to bottom
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(245, 246, 250);
            this.pnlFooter.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height    = 64;
            this.pnlFooter.Controls.Add(this.btnCrop);
            this.pnlFooter.Controls.Add(this.btnCancel);

            // btnCrop
            this.btnCrop.NormalColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.btnCrop.HoverColor  = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnCrop.ForeColor   = System.Drawing.Color.White;
            this.btnCrop.Font        = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCrop.Cursor      = System.Windows.Forms.Cursors.Hand;
            this.btnCrop.Size        = new System.Drawing.Size(120, 36);
            this.btnCrop.Text        = "Crop & Use";
            this.btnCrop.Click      += new System.EventHandler(this.BtnCrop_Click);

            // btnCancel
            this.btnCancel.NormalColor = System.Drawing.Color.FromArgb(240, 242, 248);
            this.btnCancel.HoverColor  = System.Drawing.Color.FromArgb(220, 224, 235);
            this.btnCancel.BorderColor = System.Drawing.Color.FromArgb(209, 213, 219);
            this.btnCancel.ForeColor   = System.Drawing.Color.FromArgb(30, 38, 90);
            this.btnCancel.Font        = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Cursor      = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.Size        = new System.Drawing.Size(90, 36);
            this.btnCancel.Text        = "Cancel";
            this.btnCancel.Click      += new System.EventHandler(this.BtnCancel_Click);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(37, 37, 40);
            this.ClientSize          = new System.Drawing.Size(672, 600);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Crop Photo";

            this.Controls.Add(this.picCanvas);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            ((System.ComponentModel.ISupportInitialize)(this.picCanvas)).EndInit();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // Fields
        private System.Windows.Forms.Panel   pnlHeader;
        private System.Windows.Forms.Label   lblTitle;
        private System.Windows.Forms.Label   lblHint;
        private System.Windows.Forms.PictureBox picCanvas;
        private System.Windows.Forms.Panel   pnlFooter;
        private smart_attendance_middleware.Controls.RoundedButton btnCrop;
        private smart_attendance_middleware.Controls.RoundedButton btnCancel;
    }
}
