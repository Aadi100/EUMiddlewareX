namespace smart_attendance_middleware
{
    partial class GateAccessForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtLoggedInUser = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDeviceIP = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMemberName = new System.Windows.Forms.TextBox();
            this.chkVisitor = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtVisitorName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtVisitorPhone = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtReason = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Logged In User";
            // 
            // txtLoggedInUser
            // 
            this.txtLoggedInUser.Location = new System.Drawing.Point(114, 10);
            this.txtLoggedInUser.Name = "txtLoggedInUser";
            this.txtLoggedInUser.ReadOnly = true;
            this.txtLoggedInUser.Size = new System.Drawing.Size(268, 20);
            this.txtLoggedInUser.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Device IP :";
            // 
            // txtDeviceIP
            // 
            this.txtDeviceIP.Location = new System.Drawing.Point(114, 45);
            this.txtDeviceIP.Name = "txtDeviceIP";
            this.txtDeviceIP.ReadOnly = true;
            this.txtDeviceIP.Size = new System.Drawing.Size(268, 20);
            this.txtDeviceIP.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Member Name";
            // 
            // txtMemberName
            // 
            this.txtMemberName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtMemberName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtMemberName.Location = new System.Drawing.Point(114, 81);
            this.txtMemberName.Name = "txtMemberName";
            this.txtMemberName.Size = new System.Drawing.Size(268, 20);
            this.txtMemberName.TabIndex = 5;
            // 
            // chkVisitor
            // 
            this.chkVisitor.AutoSize = true;
            this.chkVisitor.Location = new System.Drawing.Point(114, 117);
            this.chkVisitor.Name = "chkVisitor";
            this.chkVisitor.Size = new System.Drawing.Size(54, 17);
            this.chkVisitor.TabIndex = 6;
            this.chkVisitor.Text = "Visitor";
            this.chkVisitor.UseVisualStyleBackColor = true;
            this.chkVisitor.CheckedChanged += new System.EventHandler(this.chkVisitor_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 145);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Visitor Name:";
            // 
            // txtVisitorName
            // 
            this.txtVisitorName.Location = new System.Drawing.Point(114, 142);
            this.txtVisitorName.Name = "txtVisitorName";
            this.txtVisitorName.Size = new System.Drawing.Size(268, 20);
            this.txtVisitorName.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 175);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Visitor Phone:";
            // 
            // txtVisitorPhone
            // 
            this.txtVisitorPhone.Location = new System.Drawing.Point(114, 172);
            this.txtVisitorPhone.Name = "txtVisitorPhone";
            this.txtVisitorPhone.Size = new System.Drawing.Size(268, 20);
            this.txtVisitorPhone.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 205);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Reason:";
            // 
            // txtReason
            // 
            this.txtReason.Location = new System.Drawing.Point(114, 202);
            this.txtReason.Multiline = true;
            this.txtReason.Name = "txtReason";
            this.txtReason.Size = new System.Drawing.Size(268, 87);
            this.txtReason.TabIndex = 12;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(226, 337);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 15;
            this.btnSave.Text = "Open";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(307, 337);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // GateAccessForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 372);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtReason);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtVisitorPhone);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtVisitorName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.chkVisitor);
            this.Controls.Add(this.txtMemberName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtDeviceIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLoggedInUser);
            this.Controls.Add(this.label1);
            this.Name = "GateAccessForm";
            this.Text = "Gate Access Form";
            this.Load += new System.EventHandler(this.GateAccessForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLoggedInUser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDeviceIP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMemberName;
        private System.Windows.Forms.CheckBox chkVisitor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtVisitorName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtVisitorPhone;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtReason;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}