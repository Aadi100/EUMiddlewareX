namespace smart_attendance_middleware
{
    partial class FingerManagement
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lst_fingerprints = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // lst_fingerprints
            // 
            this.lst_fingerprints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lst_fingerprints.HideSelection = false;
            this.lst_fingerprints.Location = new System.Drawing.Point(0, 0);
            this.lst_fingerprints.Name = "lst_fingerprints";
            this.lst_fingerprints.Size = new System.Drawing.Size(800, 450);
            this.lst_fingerprints.TabIndex = 0;
            this.lst_fingerprints.UseCompatibleStateImageBehavior = false;
            this.lst_fingerprints.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lst_fingerprints_MouseClick);
            // 
            // FingerManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lst_fingerprints);
            this.Name = "FingerManagement";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Fingerprint Management";
            this.Load += new System.EventHandler(this.FingerManagement_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lst_fingerprints;
    }
}
