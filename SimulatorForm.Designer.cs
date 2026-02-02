namespace smart_attendance_middleware
{
    partial class SimulatorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView gridEmits;
        private System.Windows.Forms.DataGridView gridPairs;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblRemaining;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupEmits;
        private System.Windows.Forms.GroupBox groupPairs;
        private System.Windows.Forms.GroupBox groupLog;
        private System.Windows.Forms.TableLayoutPanel layoutRoot;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblRemaining = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.groupEmits = new System.Windows.Forms.GroupBox();
            this.gridEmits = new System.Windows.Forms.DataGridView();
            this.groupPairs = new System.Windows.Forms.GroupBox();
            this.gridPairs = new System.Windows.Forms.DataGridView();
            this.groupLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.layoutRoot.SuspendLayout();
            this.groupEmits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridEmits)).BeginInit();
            this.groupPairs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPairs)).BeginInit();
            this.groupLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(94, 29);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start (1h)";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(112, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(94, 29);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop now";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Location = new System.Drawing.Point(620, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(100, 29);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Stopped";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRemaining
            // 
            this.lblRemaining.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRemaining.Location = new System.Drawing.Point(726, 12);
            this.lblRemaining.Name = "lblRemaining";
            this.lblRemaining.Size = new System.Drawing.Size(170, 29);
            this.lblRemaining.TabIndex = 3;
            this.lblRemaining.Text = "Remaining: 00:00:00";
            this.lblRemaining.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // layoutRoot
            // 
            this.layoutRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                     | System.Windows.Forms.AnchorStyles.Left)
                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutRoot.ColumnCount = 2;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layoutRoot.RowCount = 2;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.layoutRoot.Controls.Add(this.groupEmits, 0, 0);
            this.layoutRoot.Controls.Add(this.groupPairs, 1, 0);
            this.layoutRoot.Controls.Add(this.groupLog, 0, 1);
            this.layoutRoot.SetColumnSpan(this.groupLog, 2);
            this.layoutRoot.Location = new System.Drawing.Point(12, 47);
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.Padding = new System.Windows.Forms.Padding(4);
            this.layoutRoot.Size = new System.Drawing.Size(884, 691);
            this.layoutRoot.TabIndex = 4;
            // 
            // groupEmits
            // 
            this.groupEmits.Controls.Add(this.gridEmits);
            this.groupEmits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupEmits.Location = new System.Drawing.Point(7, 7);
            this.groupEmits.Name = "groupEmits";
            this.groupEmits.Padding = new System.Windows.Forms.Padding(6);
            this.groupEmits.Size = new System.Drawing.Size(433, 408);
            this.groupEmits.TabIndex = 0;
            this.groupEmits.TabStop = false;
            this.groupEmits.Text = "Real-time attendance";
            // 
            // gridEmits
            // 
            this.gridEmits.AllowUserToAddRows = false;
            this.gridEmits.AllowUserToDeleteRows = false;
            this.gridEmits.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridEmits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEmits.Location = new System.Drawing.Point(6, 22);
            this.gridEmits.MultiSelect = false;
            this.gridEmits.Name = "gridEmits";
            this.gridEmits.ReadOnly = true;
            this.gridEmits.RowHeadersVisible = false;
            this.gridEmits.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridEmits.Size = new System.Drawing.Size(421, 380);
            this.gridEmits.TabIndex = 0;
            // 
            // groupPairs
            // 
            this.groupPairs.Controls.Add(this.gridPairs);
            this.groupPairs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupPairs.Location = new System.Drawing.Point(446, 7);
            this.groupPairs.Name = "groupPairs";
            this.groupPairs.Padding = new System.Windows.Forms.Padding(6);
            this.groupPairs.Size = new System.Drawing.Size(431, 408);
            this.groupPairs.TabIndex = 1;
            this.groupPairs.TabStop = false;
            this.groupPairs.Text = "Pairing";
            // 
            // gridPairs
            // 
            this.gridPairs.AllowUserToAddRows = false;
            this.gridPairs.AllowUserToDeleteRows = false;
            this.gridPairs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridPairs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPairs.Location = new System.Drawing.Point(6, 22);
            this.gridPairs.MultiSelect = false;
            this.gridPairs.Name = "gridPairs";
            this.gridPairs.ReadOnly = true;
            this.gridPairs.RowHeadersVisible = false;
            this.gridPairs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridPairs.Size = new System.Drawing.Size(419, 380);
            this.gridPairs.TabIndex = 0;
            // 
            // groupLog
            // 
            this.groupLog.Controls.Add(this.txtLog);
            this.groupLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLog.Location = new System.Drawing.Point(7, 421);
            this.groupLog.Name = "groupLog";
            this.groupLog.Padding = new System.Windows.Forms.Padding(6);
            this.groupLog.Size = new System.Drawing.Size(870, 263);
            this.groupLog.TabIndex = 2;
            this.groupLog.TabStop = false;
            this.groupLog.Text = "Log";
            // 
            // txtLog
            // 
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(6, 22);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(858, 235);
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "";
            // 
            // SimulatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 750);
            this.Controls.Add(this.layoutRoot);
            this.Controls.Add(this.lblRemaining);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Name = "SimulatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Attendance Simulator";
            this.Load += new System.EventHandler(this.SimulatorForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SimulatorForm_FormClosing);
            this.layoutRoot.ResumeLayout(false);
            this.groupEmits.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridEmits)).EndInit();
            this.groupPairs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridPairs)).EndInit();
            this.groupLog.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}