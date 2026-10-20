namespace smart_attendance_middleware
{
    partial class AddNewUser
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
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblUserNameStar = new System.Windows.Forms.Label();
            this.rtxtUserName = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.lblRole = new System.Windows.Forms.Label();
            this.lblRoleStar = new System.Windows.Forms.Label();
            this.cmbRole = new smart_attendance_middleware.Controls.RoundedComboBox();
            this.lblRoleHint = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblPasswordStar = new System.Windows.Forms.Label();
            this.rtxtPassword = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.btnTogglePassword = new smart_attendance_middleware.Controls.RoundedButton();
            this.lblConfirmPw = new System.Windows.Forms.Label();
            this.lblConfirmPwStar = new System.Windows.Forms.Label();
            this.rtxtConfirmPw = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.btnToggleConfirm = new smart_attendance_middleware.Controls.RoundedButton();
            this.pnlSeparator = new System.Windows.Forms.Panel();
            this.btn_Cancel = new smart_attendance_middleware.Controls.RoundedButton();
            this.btn_CreateUser = new smart_attendance_middleware.Controls.RoundedButton();
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
            this.lblTitle.Size = new System.Drawing.Size(139, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Add New User";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(178)))), ((int)(((byte)(210)))));
            this.lblSubtitle.Location = new System.Drawing.Point(24, 54);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(450, 22);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Create a user and choose what they can access.";
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblUserName.Location = new System.Drawing.Point(36, 114);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(67, 15);
            this.lblUserName.TabIndex = 10;
            this.lblUserName.Text = "User name";
            // 
            // lblUserNameStar
            // 
            this.lblUserNameStar.AutoSize = true;
            this.lblUserNameStar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUserNameStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblUserNameStar.Location = new System.Drawing.Point(108, 114);
            this.lblUserNameStar.Name = "lblUserNameStar";
            this.lblUserNameStar.Size = new System.Drawing.Size(12, 15);
            this.lblUserNameStar.TabIndex = 11;
            this.lblUserNameStar.Text = "*";
            // 
            // rtxtUserName
            // 
            this.rtxtUserName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.rtxtUserName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.rtxtUserName.BackColor = System.Drawing.Color.Transparent;
            this.rtxtUserName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtxtUserName.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtxtUserName.Location = new System.Drawing.Point(36, 134);
            this.rtxtUserName.MaxLength = 32767;
            this.rtxtUserName.Name = "rtxtUserName";
            this.rtxtUserName.PasswordChar = '\0';
            this.rtxtUserName.Placeholder = "";
            this.rtxtUserName.Radius = 10;
            this.rtxtUserName.ReadOnly = false;
            this.rtxtUserName.Size = new System.Drawing.Size(428, 38);
            this.rtxtUserName.TabIndex = 1;
            this.rtxtUserName.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.rtxtUserName.UseSystemPasswordChar = false;
            // 
            // lblRole
            // 
            this.lblRole.AutoSize = true;
            this.lblRole.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblRole.Location = new System.Drawing.Point(36, 192);
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
            this.lblRoleStar.Location = new System.Drawing.Point(63, 192);
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
            this.cmbRole.Location = new System.Drawing.Point(36, 212);
            this.cmbRole.Name = "cmbRole";
            this.cmbRole.Radius = 10;
            this.cmbRole.SelectedIndex = -1;
            this.cmbRole.SelectedItem = null;
            this.cmbRole.Size = new System.Drawing.Size(428, 42);
            this.cmbRole.TabIndex = 2;
            this.cmbRole.ValueMember = "";
            // 
            // lblRoleHint
            // 
            this.lblRoleHint.AutoSize = true;
            this.lblRoleHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblRoleHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.lblRoleHint.Location = new System.Drawing.Point(38, 256);
            this.lblRoleHint.Name = "lblRoleHint";
            this.lblRoleHint.Size = new System.Drawing.Size(272, 15);
            this.lblRoleHint.TabIndex = 22;
            this.lblRoleHint.Text = "The role decides which screens this user can open.";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblPassword.Location = new System.Drawing.Point(36, 282);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(59, 15);
            this.lblPassword.TabIndex = 30;
            this.lblPassword.Text = "Password";
            // 
            // lblPasswordStar
            // 
            this.lblPasswordStar.AutoSize = true;
            this.lblPasswordStar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPasswordStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblPasswordStar.Location = new System.Drawing.Point(96, 282);
            this.lblPasswordStar.Name = "lblPasswordStar";
            this.lblPasswordStar.Size = new System.Drawing.Size(12, 15);
            this.lblPasswordStar.TabIndex = 31;
            this.lblPasswordStar.Text = "*";
            // 
            // rtxtPassword
            // 
            this.rtxtPassword.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.rtxtPassword.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.rtxtPassword.BackColor = System.Drawing.Color.Transparent;
            this.rtxtPassword.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtxtPassword.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtxtPassword.Location = new System.Drawing.Point(36, 302);
            this.rtxtPassword.MaxLength = 32767;
            this.rtxtPassword.Name = "rtxtPassword";
            this.rtxtPassword.PasswordChar = '*';
            this.rtxtPassword.Placeholder = "";
            this.rtxtPassword.Radius = 10;
            this.rtxtPassword.ReadOnly = false;
            this.rtxtPassword.Size = new System.Drawing.Size(428, 38);
            this.rtxtPassword.TabIndex = 3;
            this.rtxtPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.rtxtPassword.UseSystemPasswordChar = false;
            // 
            // btnTogglePassword
            // 
            this.btnTogglePassword.BackColor = System.Drawing.Color.Transparent;
            this.btnTogglePassword.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnTogglePassword.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTogglePassword.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnTogglePassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnTogglePassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.btnTogglePassword.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(235)))));
            this.btnTogglePassword.Location = new System.Drawing.Point(430, 310);
            this.btnTogglePassword.Name = "btnTogglePassword";
            this.btnTogglePassword.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnTogglePassword.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnTogglePassword.Radius = 10;
            this.btnTogglePassword.Size = new System.Drawing.Size(30, 22);
            this.btnTogglePassword.TabIndex = 32;
            this.btnTogglePassword.TabStop = false;
            this.btnTogglePassword.Text = "●";
            this.btnTogglePassword.Click += new System.EventHandler(this.btnTogglePassword_Click);
            // 
            // lblConfirmPw
            // 
            this.lblConfirmPw.AutoSize = true;
            this.lblConfirmPw.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblConfirmPw.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(38)))), ((int)(((byte)(90)))));
            this.lblConfirmPw.Location = new System.Drawing.Point(36, 360);
            this.lblConfirmPw.Name = "lblConfirmPw";
            this.lblConfirmPw.Size = new System.Drawing.Size(107, 15);
            this.lblConfirmPw.TabIndex = 40;
            this.lblConfirmPw.Text = "Confirm password";
            // 
            // lblConfirmPwStar
            // 
            this.lblConfirmPwStar.AutoSize = true;
            this.lblConfirmPwStar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblConfirmPwStar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblConfirmPwStar.Location = new System.Drawing.Point(162, 360);
            this.lblConfirmPwStar.Name = "lblConfirmPwStar";
            this.lblConfirmPwStar.Size = new System.Drawing.Size(12, 15);
            this.lblConfirmPwStar.TabIndex = 41;
            this.lblConfirmPwStar.Text = "*";
            // 
            // rtxtConfirmPw
            // 
            this.rtxtConfirmPw.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
            this.rtxtConfirmPw.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.None;
            this.rtxtConfirmPw.BackColor = System.Drawing.Color.Transparent;
            this.rtxtConfirmPw.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.rtxtConfirmPw.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.rtxtConfirmPw.Location = new System.Drawing.Point(36, 380);
            this.rtxtConfirmPw.MaxLength = 32767;
            this.rtxtConfirmPw.Name = "rtxtConfirmPw";
            this.rtxtConfirmPw.PasswordChar = '*';
            this.rtxtConfirmPw.Placeholder = "";
            this.rtxtConfirmPw.Radius = 10;
            this.rtxtConfirmPw.ReadOnly = false;
            this.rtxtConfirmPw.Size = new System.Drawing.Size(428, 38);
            this.rtxtConfirmPw.TabIndex = 4;
            this.rtxtConfirmPw.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.rtxtConfirmPw.UseSystemPasswordChar = false;
            // 
            // btnToggleConfirm
            // 
            this.btnToggleConfirm.BackColor = System.Drawing.Color.Transparent;
            this.btnToggleConfirm.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(219)))));
            this.btnToggleConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnToggleConfirm.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btnToggleConfirm.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnToggleConfirm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(138)))), ((int)(((byte)(165)))));
            this.btnToggleConfirm.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(235)))));
            this.btnToggleConfirm.Location = new System.Drawing.Point(430, 388);
            this.btnToggleConfirm.Name = "btnToggleConfirm";
            this.btnToggleConfirm.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btnToggleConfirm.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btnToggleConfirm.Radius = 10;
            this.btnToggleConfirm.Size = new System.Drawing.Size(30, 22);
            this.btnToggleConfirm.TabIndex = 42;
            this.btnToggleConfirm.TabStop = false;
            this.btnToggleConfirm.Text = "●";
            this.btnToggleConfirm.Click += new System.EventHandler(this.btnToggleConfirm_Click);
            // 
            // pnlSeparator
            // 
            this.pnlSeparator.Location = new System.Drawing.Point(0, 444);
            this.pnlSeparator.Name = "pnlSeparator";
            this.pnlSeparator.Size = new System.Drawing.Size(500, 1);
            this.pnlSeparator.TabIndex = 50;
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
            this.btn_Cancel.Location = new System.Drawing.Point(240, 457);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(242)))), ((int)(((byte)(248)))));
            this.btn_Cancel.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btn_Cancel.Radius = 10;
            this.btn_Cancel.Size = new System.Drawing.Size(110, 42);
            this.btn_Cancel.TabIndex = 6;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_CreateUser
            // 
            this.btn_CreateUser.BackColor = System.Drawing.Color.Transparent;
            this.btn_CreateUser.BorderColor = System.Drawing.Color.Transparent;
            this.btn_CreateUser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_CreateUser.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btn_CreateUser.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btn_CreateUser.ForeColor = System.Drawing.Color.White;
            this.btn_CreateUser.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btn_CreateUser.Location = new System.Drawing.Point(358, 457);
            this.btn_CreateUser.Name = "btn_CreateUser";
            this.btn_CreateUser.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btn_CreateUser.PressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(78)))), ((int)(((byte)(216)))));
            this.btn_CreateUser.Radius = 10;
            this.btn_CreateUser.Size = new System.Drawing.Size(130, 42);
            this.btn_CreateUser.TabIndex = 5;
            this.btn_CreateUser.Text = "Create user";
            this.btn_CreateUser.Click += new System.EventHandler(this.btn_CreateUser_Click);
            // 
            // AddNewUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(500, 512);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.lblUserNameStar);
            this.Controls.Add(this.rtxtUserName);
            this.Controls.Add(this.lblRole);
            this.Controls.Add(this.lblRoleStar);
            this.Controls.Add(this.cmbRole);
            this.Controls.Add(this.lblRoleHint);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblPasswordStar);
            this.Controls.Add(this.rtxtPassword);
            this.Controls.Add(this.btnTogglePassword);
            this.Controls.Add(this.lblConfirmPw);
            this.Controls.Add(this.lblConfirmPwStar);
            this.Controls.Add(this.rtxtConfirmPw);
            this.Controls.Add(this.btnToggleConfirm);
            this.Controls.Add(this.pnlSeparator);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_CreateUser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddNewUser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add New User";
            this.Load += new System.EventHandler(this.AddNewUser_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblUserNameStar;
        private smart_attendance_middleware.Controls.RoundedTextBox rtxtUserName;
        private System.Windows.Forms.Label lblRole;
        private System.Windows.Forms.Label lblRoleStar;
        private smart_attendance_middleware.Controls.RoundedComboBox cmbRole;
        private System.Windows.Forms.Label lblRoleHint;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblPasswordStar;
        private smart_attendance_middleware.Controls.RoundedTextBox rtxtPassword;
        private smart_attendance_middleware.Controls.RoundedButton btnTogglePassword;
        private System.Windows.Forms.Label lblConfirmPw;
        private System.Windows.Forms.Label lblConfirmPwStar;
        private smart_attendance_middleware.Controls.RoundedTextBox rtxtConfirmPw;
        private smart_attendance_middleware.Controls.RoundedButton btnToggleConfirm;
        private System.Windows.Forms.Panel pnlSeparator;
        private smart_attendance_middleware.Controls.RoundedButton btn_Cancel;
        private smart_attendance_middleware.Controls.RoundedButton btn_CreateUser;
    }
}





