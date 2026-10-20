namespace smart_attendance_middleware
{
    partial class EditUser
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
            this.lblUserNameField = new System.Windows.Forms.Label();
            this.rtxtUserName = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblLocked = new System.Windows.Forms.Label();
            this.lblUserNameHint = new System.Windows.Forms.Label();
            this.lblRole = new System.Windows.Forms.Label();
            this.lblRoleStar = new System.Windows.Forms.Label();
            this.cmbRole = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.pnlSectionSep = new System.Windows.Forms.Panel();
            this.lblChangePwTitle = new System.Windows.Forms.Label();
            this.lblOptional = new System.Windows.Forms.Label();
            this.lblNewPassword = new System.Windows.Forms.Label();
            this.rtxtNewPassword = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.btnToggleNew = new smart_attendance_middleware.Controls.RoundedButton();
            this.lblConfirmNewPw = new System.Windows.Forms.Label();
            this.rtxtConfirmNewPw = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.btnToggleConfirmNew = new smart_attendance_middleware.Controls.RoundedButton();
            this.pnlSeparator = new System.Windows.Forms.Panel();
            this.btn_Cancel = new smart_attendance_middleware.Controls.RoundedButton();
            this.btn_update = new smart_attendance_middleware.Controls.RoundedButton();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(32)))), ((int)(((byte)(64)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(500, 90);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(22, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(91, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Edit User";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(178)))), ((int)(((byte)(210)))));
            this.lblSubtitle.Location = new System.Drawing.Point(24, 54);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(450, 22);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Change a user\'s role or reset their password.";
            // 
            // lblUserNameField
            // 
            this.lblUserNameField.AutoSize = true;
            this.lblUserNameField.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblUserNameField.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblUserNameField.Location = new System.Drawing.Point(36, 110);
            this.lblUserNameField.Name = "lblUserNameField";
            this.lblUserNameField.Size = new System.Drawing.Size(63, 15);
            this.lblUserNameField.TabIndex = 10;
            this.lblUserNameField.Text = "User name";
            // 
            // rtxtUserName
            // 
            this.rtxtUserName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.rtxtUserName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.rtxtUserName.BackColor = System.Drawing.Color.Transparent;
            this.rtxtUserName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtxtUserName.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtxtUserName.Location = new System.Drawing.Point(36, 130);
            this.rtxtUserName.MaxLength = 32767;
            this.rtxtUserName.Name = "rtxtUserName";
            this.rtxtUserName.PasswordChar = '\0';
            this.rtxtUserName.Placeholder = "";
            this.rtxtUserName.Radius = 10;
            this.rtxtUserName.ReadOnly = false;
            this.rtxtUserName.Size = new System.Drawing.Size(428, 38);
            this.rtxtUserName.TabIndex = 11;
            this.rtxtUserName.TabStop = false;
            this.rtxtUserName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.rtxtUserName.UseSystemPasswordChar = false;
            // 
            // lblLocked
            // 
            this.lblLocked.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLocked.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.lblLocked.Location = new System.Drawing.Point(388, 142);
            this.lblLocked.Name = "lblLocked";
            this.lblLocked.Size = new System.Drawing.Size(62, 22);
            this.lblLocked.TabIndex = 12;
            this.lblLocked.Text = "Locked";
            this.lblLocked.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblUserNameHint
            // 
            this.lblUserNameHint.AutoSize = true;
            this.lblUserNameHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblUserNameHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblUserNameHint.Location = new System.Drawing.Point(38, 174);
            this.lblUserNameHint.Name = "lblUserNameHint";
            this.lblUserNameHint.Size = new System.Drawing.Size(171, 15);
            this.lblUserNameHint.TabIndex = 13;
            this.lblUserNameHint.Text = "User name cannot be changed.";
            // 
            // lblRole
            // 
            this.lblRole.AutoSize = true;
            this.lblRole.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblRole.Location = new System.Drawing.Point(36, 202);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(32, 15);
            this.lblRole.TabIndex = 20;
            this.lblRole.Text = "Role";
            // 
            // lblRoleStar
            // 
            this.lblRoleStar.AutoSize = true;
            this.lblRoleStar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRoleStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblRoleStar.Location = new System.Drawing.Point(63, 202);
            this.lblRoleStar.Name = "lblRoleStar";
            this.lblRoleStar.Size = new System.Drawing.Size(12, 15);
            this.lblRoleStar.TabIndex = 21;
            this.lblRoleStar.Text = "*";
            // 
            // cmbRole
            // 
            this.cmbRole.BackColor = System.Drawing.Color.Transparent;
            this.cmbRole.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.cmbRole.DataSource = null;
            this.cmbRole.DisplayMember = "";
            this.cmbRole.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.cmbRole.Location = new System.Drawing.Point(36, 222);
            this.cmbRole.Name = "cmbRole";
            this.cmbRole.Radius = 10;
            this.cmbRole.SelectedIndex = -1;
            this.cmbRole.SelectedItem = null;
            this.cmbRole.Size = new System.Drawing.Size(428, 42);
            this.cmbRole.TabIndex = 1;
            this.cmbRole.ValueMember = "";
            // 
            // pnlSectionSep
            // 
            this.pnlSectionSep.Location = new System.Drawing.Point(36, 280);
            this.pnlSectionSep.Name = "pnlSectionSep";
            this.pnlSectionSep.Size = new System.Drawing.Size(428, 1);
            this.pnlSectionSep.TabIndex = 30;
            // 
            // lblChangePwTitle
            // 
            this.lblChangePwTitle.AutoSize = true;
            this.lblChangePwTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblChangePwTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblChangePwTitle.Location = new System.Drawing.Point(36, 296);
            this.lblChangePwTitle.Name = "lblChangePwTitle";
            this.lblChangePwTitle.Size = new System.Drawing.Size(132, 20);
            this.lblChangePwTitle.TabIndex = 31;
            this.lblChangePwTitle.Text = "Change password";
            // 
            // lblOptional
            // 
            this.lblOptional.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblOptional.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.lblOptional.Location = new System.Drawing.Point(362, 298);
            this.lblOptional.Name = "lblOptional";
            this.lblOptional.Size = new System.Drawing.Size(66, 22);
            this.lblOptional.TabIndex = 32;
            this.lblOptional.Text = "Optional";
            this.lblOptional.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblNewPassword
            // 
            this.lblNewPassword.AutoSize = true;
            this.lblNewPassword.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblNewPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblNewPassword.Location = new System.Drawing.Point(36, 334);
            this.lblNewPassword.Name = "lblNewPassword";
            this.lblNewPassword.Size = new System.Drawing.Size(84, 15);
            this.lblNewPassword.TabIndex = 40;
            this.lblNewPassword.Text = "New password";
            // 
            // rtxtNewPassword
            // 
            this.rtxtNewPassword.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.rtxtNewPassword.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.rtxtNewPassword.BackColor = System.Drawing.Color.Transparent;
            this.rtxtNewPassword.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtxtNewPassword.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtxtNewPassword.Location = new System.Drawing.Point(36, 354);
            this.rtxtNewPassword.MaxLength = 32767;
            this.rtxtNewPassword.Name = "rtxtNewPassword";
            this.rtxtNewPassword.PasswordChar = '*';
            this.rtxtNewPassword.Placeholder = "";
            this.rtxtNewPassword.Radius = 10;
            this.rtxtNewPassword.ReadOnly = false;
            this.rtxtNewPassword.Size = new System.Drawing.Size(428, 38);
            this.rtxtNewPassword.TabIndex = 2;
            this.rtxtNewPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.rtxtNewPassword.UseSystemPasswordChar = false;
            // 
            // btnToggleNew
            // 
            this.btnToggleNew.BackColor = System.Drawing.Color.Transparent;
            this.btnToggleNew.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnToggleNew.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnToggleNew.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnToggleNew.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnToggleNew.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.btnToggleNew.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(235)))));
            this.btnToggleNew.Location = new System.Drawing.Point(430, 362);
            this.btnToggleNew.Name = "btnToggleNew";
            this.btnToggleNew.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnToggleNew.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnToggleNew.Radius = 10;
            this.btnToggleNew.Size = new System.Drawing.Size(30, 22);
            this.btnToggleNew.TabIndex = 41;
            this.btnToggleNew.TabStop = false;
            this.btnToggleNew.Text = "●";
            this.btnToggleNew.Click += new System.EventHandler(this.btnToggleNew_Click);
            // 
            // lblConfirmNewPw
            // 
            this.lblConfirmNewPw.AutoSize = true;
            this.lblConfirmNewPw.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblConfirmNewPw.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblConfirmNewPw.Location = new System.Drawing.Point(36, 406);
            this.lblConfirmNewPw.Name = "lblConfirmNewPw";
            this.lblConfirmNewPw.Size = new System.Drawing.Size(129, 15);
            this.lblConfirmNewPw.TabIndex = 50;
            this.lblConfirmNewPw.Text = "Confirm new password";
            // 
            // rtxtConfirmNewPw
            // 
            this.rtxtConfirmNewPw.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.rtxtConfirmNewPw.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.rtxtConfirmNewPw.BackColor = System.Drawing.Color.Transparent;
            this.rtxtConfirmNewPw.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtxtConfirmNewPw.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtxtConfirmNewPw.Location = new System.Drawing.Point(36, 426);
            this.rtxtConfirmNewPw.MaxLength = 32767;
            this.rtxtConfirmNewPw.Name = "rtxtConfirmNewPw";
            this.rtxtConfirmNewPw.PasswordChar = '*';
            this.rtxtConfirmNewPw.Placeholder = "";
            this.rtxtConfirmNewPw.Radius = 10;
            this.rtxtConfirmNewPw.ReadOnly = false;
            this.rtxtConfirmNewPw.Size = new System.Drawing.Size(428, 38);
            this.rtxtConfirmNewPw.TabIndex = 3;
            this.rtxtConfirmNewPw.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.rtxtConfirmNewPw.UseSystemPasswordChar = false;
            // 
            // btnToggleConfirmNew
            // 
            this.btnToggleConfirmNew.BackColor = System.Drawing.Color.Transparent;
            this.btnToggleConfirmNew.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnToggleConfirmNew.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnToggleConfirmNew.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnToggleConfirmNew.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnToggleConfirmNew.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.btnToggleConfirmNew.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(235)))));
            this.btnToggleConfirmNew.Location = new System.Drawing.Point(430, 434);
            this.btnToggleConfirmNew.Name = "btnToggleConfirmNew";
            this.btnToggleConfirmNew.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnToggleConfirmNew.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnToggleConfirmNew.Radius = 10;
            this.btnToggleConfirmNew.Size = new System.Drawing.Size(30, 22);
            this.btnToggleConfirmNew.TabIndex = 51;
            this.btnToggleConfirmNew.TabStop = false;
            this.btnToggleConfirmNew.Text = "●";
            this.btnToggleConfirmNew.Click += new System.EventHandler(this.btnToggleConfirmNew_Click);
            // 
            // pnlSeparator
            // 
            this.pnlSeparator.Location = new System.Drawing.Point(0, 484);
            this.pnlSeparator.Name = "pnlSeparator";
            this.pnlSeparator.Size = new System.Drawing.Size(500, 1);
            this.pnlSeparator.TabIndex = 60;
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.BackColor = System.Drawing.Color.Transparent;
            this.btn_Cancel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btn_Cancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btn_Cancel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btn_Cancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.btn_Cancel.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(235)))));
            this.btn_Cancel.Location = new System.Drawing.Point(240, 496);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btn_Cancel.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btn_Cancel.Radius = 10;
            this.btn_Cancel.Size = new System.Drawing.Size(110, 42);
            this.btn_Cancel.TabIndex = 6;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_update
            // 
            this.btn_update.BackColor = System.Drawing.Color.Transparent;
            this.btn_update.BorderColor = System.Drawing.Color.Transparent;
            this.btn_update.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_update.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btn_update.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btn_update.ForeColor = System.Drawing.Color.White;
            this.btn_update.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btn_update.Location = new System.Drawing.Point(358, 496);
            this.btn_update.Name = "btn_update";
            this.btn_update.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btn_update.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btn_update.Radius = 10;
            this.btn_update.Size = new System.Drawing.Size(130, 42);
            this.btn_update.TabIndex = 5;
            this.btn_update.Text = "Update user";
            this.btn_update.Click += new System.EventHandler(this.btn_update_Click);
            // 
            // EditUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(500, 550);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.lblUserNameField);
            this.Controls.Add(this.rtxtUserName);
            this.Controls.Add(this.lblLocked);
            this.Controls.Add(this.lblUserNameHint);
            this.Controls.Add(this.lblRole);
            this.Controls.Add(this.lblRoleStar);
            this.Controls.Add(this.cmbRole);
            this.Controls.Add(this.pnlSectionSep);
            this.Controls.Add(this.lblChangePwTitle);
            this.Controls.Add(this.lblOptional);
            this.Controls.Add(this.lblNewPassword);
            this.Controls.Add(this.rtxtNewPassword);
            this.Controls.Add(this.btnToggleNew);
            this.Controls.Add(this.lblConfirmNewPw);
            this.Controls.Add(this.rtxtConfirmNewPw);
            this.Controls.Add(this.btnToggleConfirmNew);
            this.Controls.Add(this.pnlSeparator);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_update);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditUser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Edit User";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblUserNameField;
        private smart_attendance_middleware.Controls.RoundedTextBox rtxtUserName;
        private System.Windows.Forms.Label lblLocked;
        private System.Windows.Forms.Label lblUserNameHint;
        private System.Windows.Forms.Label lblRole;
        private System.Windows.Forms.Label lblRoleStar;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbRole;
        private System.Windows.Forms.Panel pnlSectionSep;
        private System.Windows.Forms.Label lblChangePwTitle;
        private System.Windows.Forms.Label lblOptional;
        private System.Windows.Forms.Label lblNewPassword;
        private smart_attendance_middleware.Controls.RoundedTextBox rtxtNewPassword;
        private smart_attendance_middleware.Controls.RoundedButton btnToggleNew;
        private System.Windows.Forms.Label lblConfirmNewPw;
        private smart_attendance_middleware.Controls.RoundedTextBox rtxtConfirmNewPw;
        private smart_attendance_middleware.Controls.RoundedButton btnToggleConfirmNew;
        private System.Windows.Forms.Panel pnlSeparator;
        private smart_attendance_middleware.Controls.RoundedButton btn_Cancel;
        private smart_attendance_middleware.Controls.RoundedButton btn_update;
    }
}





