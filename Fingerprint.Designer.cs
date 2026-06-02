using smart_attendance_middleware.Properties;

namespace smart_attendance_middleware
{
    partial class Fingerprint
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Fingerprint));
            this.txtsecond = new System.Windows.Forms.TextBox();
            this.btn_setmonitor = new System.Windows.Forms.Button();
            this.Message = new System.Windows.Forms.TextBox();
            this.TB_Privilege = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TB_FP_NUM = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TB_ID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.button3 = new System.Windows.Forms.Button();
            this.EnrollFPWithStr = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.axFpreader1 = new AxFPREADERLib.AxFpreader();
            this.btn_retry = new System.Windows.Forms.Button();
            this.btn_release = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.axFpreader1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // txtsecond
            // 
            this.txtsecond.Location = new System.Drawing.Point(964, 146);
            this.txtsecond.Margin = new System.Windows.Forms.Padding(4);
            this.txtsecond.Name = "txtsecond";
            this.txtsecond.Size = new System.Drawing.Size(64, 20);
            this.txtsecond.TabIndex = 33;
            this.txtsecond.Text = "0";
            // 
            // btn_setmonitor
            // 
            this.btn_setmonitor.Location = new System.Drawing.Point(864, 110);
            this.btn_setmonitor.Margin = new System.Windows.Forms.Padding(4);
            this.btn_setmonitor.Name = "btn_setmonitor";
            this.btn_setmonitor.Size = new System.Drawing.Size(164, 28);
            this.btn_setmonitor.TabIndex = 32;
            this.btn_setmonitor.Text = "setmonitorsecond";
            this.btn_setmonitor.UseVisualStyleBackColor = true;
            this.btn_setmonitor.Click += new System.EventHandler(this.btn_setmonitor_Click);
            // 
            // Message
            // 
            this.Message.Location = new System.Drawing.Point(13, 452);
            this.Message.Margin = new System.Windows.Forms.Padding(4);
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Size = new System.Drawing.Size(283, 20);
            this.Message.TabIndex = 31;
            this.Message.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TB_Privilege
            // 
            this.TB_Privilege.Location = new System.Drawing.Point(896, 188);
            this.TB_Privilege.Margin = new System.Windows.Forms.Padding(4);
            this.TB_Privilege.Name = "TB_Privilege";
            this.TB_Privilege.Size = new System.Drawing.Size(132, 20);
            this.TB_Privilege.TabIndex = 28;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(893, 149);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Privilege:";
            // 
            // TB_FP_NUM
            // 
            this.TB_FP_NUM.Location = new System.Drawing.Point(141, 381);
            this.TB_FP_NUM.Margin = new System.Windows.Forms.Padding(4);
            this.TB_FP_NUM.Name = "TB_FP_NUM";
            this.TB_FP_NUM.Size = new System.Drawing.Size(132, 20);
            this.TB_FP_NUM.TabIndex = 29;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(85, 384);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "FP Num:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TB_ID
            // 
            this.TB_ID.Location = new System.Drawing.Point(141, 348);
            this.TB_ID.Margin = new System.Windows.Forms.Padding(4);
            this.TB_ID.Name = "TB_ID";
            this.TB_ID.ReadOnly = true;
            this.TB_ID.Size = new System.Drawing.Size(132, 20);
            this.TB_ID.TabIndex = 30;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(112, 351);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "ID:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox.ErrorImage = null;
            this.pictureBox.ImageLocation = "";
            this.pictureBox.InitialImage = null;
            this.pictureBox.Location = new System.Drawing.Point(13, 13);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(283, 318);
            this.pictureBox.TabIndex = 24;
            this.pictureBox.TabStop = false;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(864, 74);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(164, 28);
            this.button3.TabIndex = 21;
            this.button3.Text = "DeleteAll";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // EnrollFPWithStr
            // 
            this.EnrollFPWithStr.Location = new System.Drawing.Point(864, 38);
            this.EnrollFPWithStr.Margin = new System.Windows.Forms.Padding(4);
            this.EnrollFPWithStr.Name = "EnrollFPWithStr";
            this.EnrollFPWithStr.Size = new System.Drawing.Size(164, 28);
            this.EnrollFPWithStr.TabIndex = 22;
            this.EnrollFPWithStr.Text = "EnrollFPWithCloud ";
            this.EnrollFPWithStr.UseVisualStyleBackColor = true;
            this.EnrollFPWithStr.Click += new System.EventHandler(this.EnrollFPWithStr_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 416);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(283, 28);
            this.button1.TabIndex = 23;
            this.button1.Text = "EnrollFP";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //
            // axFpreader1
            // 
            this.axFpreader1.Enabled = true;
            this.axFpreader1.Location = new System.Drawing.Point(387, 285);
            this.axFpreader1.Margin = new System.Windows.Forms.Padding(4);
            this.axFpreader1.Name = "axFpreader1";
            this.axFpreader1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axFpreader1.OcxState")));
            this.axFpreader1.Size = new System.Drawing.Size(125, 63);
            this.axFpreader1.TabIndex = 8;
            // 
            // btn_retry
            // 
            this.btn_retry.Location = new System.Drawing.Point(864, 146);
            this.btn_retry.Margin = new System.Windows.Forms.Padding(4);
            this.btn_retry.Name = "btn_retry";
            this.btn_retry.Size = new System.Drawing.Size(164, 28);
            this.btn_retry.TabIndex = 34;
            this.btn_retry.Text = "Cancel & Retry";
            this.btn_retry.UseVisualStyleBackColor = true;
            this.btn_retry.Click += new System.EventHandler(this.btn_retry_Click);
            // 
            // btn_release
            // 
            this.btn_release.Location = new System.Drawing.Point(864, 180);
            this.btn_release.Margin = new System.Windows.Forms.Padding(4);
            this.btn_release.Name = "btn_release";
            this.btn_release.Size = new System.Drawing.Size(164, 28);
            this.btn_release.TabIndex = 35;
            this.btn_release.Text = "Release Device";
            this.btn_release.UseVisualStyleBackColor = true;
            this.btn_release.Click += new System.EventHandler(this.btn_release_Click);
            // 
            // 
            // Fingerprint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 497);
            this.Controls.Add(this.txtsecond);
            this.Controls.Add(this.btn_setmonitor);
            this.Controls.Add(this.axFpreader1);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.TB_Privilege);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TB_FP_NUM);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TB_ID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.EnrollFPWithStr);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btn_retry);
            this.Controls.Add(this.btn_release);
            this.Name = "Fingerprint";
            this.Text = "Fingerprint";
            this.Load += new System.EventHandler(this.Fingerprint_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axFpreader1)).EndInit();

            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtsecond;
        private System.Windows.Forms.Button btn_setmonitor;
        private System.Windows.Forms.TextBox Message;
        private System.Windows.Forms.TextBox TB_Privilege;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TB_FP_NUM;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TB_ID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button EnrollFPWithStr;
        private System.Windows.Forms.Button button1;
        private AxFPREADERLib.AxFpreader axFpreader1;
        private System.Windows.Forms.Button btn_retry;
        private System.Windows.Forms.Button btn_release;

    }
}
