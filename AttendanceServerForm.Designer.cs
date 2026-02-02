namespace smart_attendance_middleware
{
    partial class AttendanceServerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.ListView listView1;
        private AxFPCLOCK_SVRLib.AxFPCLOCK_Svr axFPCLOCK_Svr1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttendanceServerForm));
            this.textPort = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.axFPCLOCK_Svr1 = new AxFPCLOCK_SVRLib.AxFPCLOCK_Svr();
            ((System.ComponentModel.ISupportInitialize)(this.axFPCLOCK_Svr1)).BeginInit();
            this.SuspendLayout();
            // 
            // textPort
            // 
            this.textPort.Location = new System.Drawing.Point(54, 15);
            this.textPort.Name = "textPort";
            this.textPort.ReadOnly = true;
            this.textPort.Size = new System.Drawing.Size(90, 20);
            this.textPort.TabIndex = 0;
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(18, 18);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(29, 13);
            this.labelPort.TabIndex = 100;
            this.labelPort.Text = "Port:";
            // 
            // listView1
            // 
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(21, 155);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(860, 385);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // axFPCLOCK_Svr1
            // 
            this.axFPCLOCK_Svr1.Enabled = true;
            this.axFPCLOCK_Svr1.Location = new System.Drawing.Point(21, 45);
            this.axFPCLOCK_Svr1.Name = "axFPCLOCK_Svr1";
            this.axFPCLOCK_Svr1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axFPCLOCK_Svr1.OcxState")));
            this.axFPCLOCK_Svr1.Size = new System.Drawing.Size(1, 1);
            this.axFPCLOCK_Svr1.TabIndex = 2;
            this.axFPCLOCK_Svr1.OnReceiveGLogData += new AxFPCLOCK_SVRLib._DFPCLOCK_SvrEvents_OnReceiveGLogDataEventHandler(this.axFPCLOCK_Svr1_OnReceiveGLogData);
            // 
            // AttendanceServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 561);
            this.Controls.Add(this.axFPCLOCK_Svr1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.textPort);
            this.Name = "AttendanceServerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Attendance Server";
            ((System.ComponentModel.ISupportInitialize)(this.axFPCLOCK_Svr1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}