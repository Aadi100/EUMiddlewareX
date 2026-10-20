namespace smart_attendance_middleware
{
    partial class UserSetup
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
            this.components = new System.ComponentModel.Container();

            this.label1                    = new System.Windows.Forms.Label();
            this.label2                    = new System.Windows.Forms.Label();
            this.label3                    = new System.Windows.Forms.Label();
            this.txt_username              = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.txt_password              = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.txt_confirmpass           = new smart_attendance_middleware.Controls.RoundedTextBox();
            this.btnTogglePassword         = new System.Windows.Forms.Label();
            this.btnToggleConfirmPassword  = new System.Windows.Forms.Label();
            this.chkBox_is_Admin           = new smart_attendance_middleware.Controls.ToggleSwitch();
            this.lbl_is_Admin              = new System.Windows.Forms.Label();
            this.btn_close_window          = new System.Windows.Forms.Label();
            this.linkLbl_terms             = new System.Windows.Forms.LinkLabel();
            this.panel1                    = new System.Windows.Forms.Panel();
            this.label6                    = new System.Windows.Forms.Label();
            this.label5                    = new System.Windows.Forms.Label();
            this.label4                    = new System.Windows.Forms.Label();
            this.pictureBox2               = new System.Windows.Forms.PictureBox();
            this.pictureBox1               = new System.Windows.Forms.PictureBox();
            this.label8                    = new System.Windows.Forms.Label();
            this.btnCreateAccount          = new smart_attendance_middleware.Controls.RoundedButton();

            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();

            // label8 — title
            this.label8.BackColor  = System.Drawing.Color.White;
            this.label8.Font       = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.label8.Location   = new System.Drawing.Point(466, 55);
            this.label8.Name       = "label8";
            this.label8.Size       = new System.Drawing.Size(290, 60);
            this.label8.TabIndex   = 23;
            this.label8.Text       = "Create Gym Entry Control Login";
            this.label8.TextAlign  = System.Drawing.ContentAlignment.MiddleCenter;

            // label1 — Username label
            this.label1.AutoSize  = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(130, 138, 165);
            this.label1.Location  = new System.Drawing.Point(468, 132);
            this.label1.Name      = "label1";
            this.label1.TabIndex  = 0;
            this.label1.Text      = "User Name";

            // txt_username — RoundedTextBox
            this.txt_username.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txt_username.Location = new System.Drawing.Point(468, 150);
            this.txt_username.Name     = "txt_username";
            this.txt_username.Size     = new System.Drawing.Size(288, 36);
            this.txt_username.TabIndex = 3;
            this.txt_username.TextChanged += new System.EventHandler(this.txt_username_TextChanged);

            // label2 — Password label
            this.label2.AutoSize  = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(130, 138, 165);
            this.label2.Location  = new System.Drawing.Point(468, 202);
            this.label2.Name      = "label2";
            this.label2.TabIndex  = 1;
            this.label2.Text      = "Password";

            // txt_password — RoundedTextBox
            this.txt_password.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txt_password.Location = new System.Drawing.Point(468, 220);
            this.txt_password.Name     = "txt_password";
            this.txt_password.Size     = new System.Drawing.Size(288, 36);
            this.txt_password.TabIndex = 4;
            this.txt_password.TextChanged += new System.EventHandler(this.txt_password_TextChanged);

            // btnTogglePassword — eye icon
            this.btnTogglePassword.AutoSize  = false;
            this.btnTogglePassword.BackColor = System.Drawing.Color.Transparent;
            this.btnTogglePassword.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnTogglePassword.Font      = new System.Drawing.Font("Segoe UI", 12F);
            this.btnTogglePassword.ForeColor = System.Drawing.Color.Gray;
            this.btnTogglePassword.Location  = new System.Drawing.Point(728, 224);
            this.btnTogglePassword.Name      = "btnTogglePassword";
            this.btnTogglePassword.Size      = new System.Drawing.Size(28, 28);
            this.btnTogglePassword.TabIndex  = 41;
            this.btnTogglePassword.Text      = "👁";
            this.btnTogglePassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnTogglePassword.Click    += new System.EventHandler(this.btnTogglePassword_Click);

            // label3 — Confirm Password label
            this.label3.AutoSize  = true;
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(130, 138, 165);
            this.label3.Location  = new System.Drawing.Point(468, 272);
            this.label3.Name      = "label3";
            this.label3.TabIndex  = 2;
            this.label3.Text      = "Confirm Password";

            // txt_confirmpass — RoundedTextBox
            this.txt_confirmpass.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txt_confirmpass.Location = new System.Drawing.Point(468, 290);
            this.txt_confirmpass.Name     = "txt_confirmpass";
            this.txt_confirmpass.Size     = new System.Drawing.Size(288, 36);
            this.txt_confirmpass.TabIndex = 5;
            this.txt_confirmpass.TextChanged += new System.EventHandler(this.txt_confirmpass_TextChanged);

            // btnToggleConfirmPassword — eye icon
            this.btnToggleConfirmPassword.AutoSize  = false;
            this.btnToggleConfirmPassword.BackColor = System.Drawing.Color.Transparent;
            this.btnToggleConfirmPassword.Cursor    = System.Windows.Forms.Cursors.Hand;
            this.btnToggleConfirmPassword.Font      = new System.Drawing.Font("Segoe UI", 12F);
            this.btnToggleConfirmPassword.ForeColor = System.Drawing.Color.Gray;
            this.btnToggleConfirmPassword.Location  = new System.Drawing.Point(728, 294);
            this.btnToggleConfirmPassword.Name      = "btnToggleConfirmPassword";
            this.btnToggleConfirmPassword.Size      = new System.Drawing.Size(28, 28);
            this.btnToggleConfirmPassword.TabIndex  = 42;
            this.btnToggleConfirmPassword.Text      = "👁";
            this.btnToggleConfirmPassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnToggleConfirmPassword.Click    += new System.EventHandler(this.btnToggleConfirmPassword_Click);

            // chkBox_is_Admin — ToggleSwitch
            this.chkBox_is_Admin.Checked  = true;
            this.chkBox_is_Admin.Location = new System.Drawing.Point(468, 348);
            this.chkBox_is_Admin.Name     = "chkBox_is_Admin";
            this.chkBox_is_Admin.Size     = new System.Drawing.Size(44, 24);
            this.chkBox_is_Admin.TabIndex = 6;
            this.chkBox_is_Admin.CheckedChanged += new System.EventHandler(this.chkBox_is_Admin_CheckedChanged);

            // lbl_is_Admin — companion label
            this.lbl_is_Admin.AutoSize  = true;
            this.lbl_is_Admin.BackColor = System.Drawing.Color.White;
            this.lbl_is_Admin.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lbl_is_Admin.ForeColor = System.Drawing.Color.DimGray;
            this.lbl_is_Admin.Location  = new System.Drawing.Point(518, 352);
            this.lbl_is_Admin.Name      = "lbl_is_Admin";
            this.lbl_is_Admin.TabIndex  = 43;
            this.lbl_is_Admin.Text      = "Make this user an administrator";

            // btnCreateAccount
            this.btnCreateAccount.NormalColor = System.Drawing.Color.FromArgb(3, 70, 214);
            this.btnCreateAccount.HoverColor  = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnCreateAccount.Cursor      = System.Windows.Forms.Cursors.Hand;
            this.btnCreateAccount.Font        = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCreateAccount.ForeColor   = System.Drawing.Color.White;
            this.btnCreateAccount.Location    = new System.Drawing.Point(502, 392);
            this.btnCreateAccount.Name        = "btnCreateAccount";
            this.btnCreateAccount.Size        = new System.Drawing.Size(218, 42);
            this.btnCreateAccount.TabIndex    = 41;
            this.btnCreateAccount.Text        = "Create Account";
            this.btnCreateAccount.Click      += new System.EventHandler(this.btnCreateAccount_Click);

            // linkLbl_terms
            this.linkLbl_terms.BackColor  = System.Drawing.Color.Transparent;
            this.linkLbl_terms.Font       = new System.Drawing.Font("Segoe UI", 8F);
            this.linkLbl_terms.Location   = new System.Drawing.Point(468, 448);
            this.linkLbl_terms.Name       = "linkLbl_terms";
            this.linkLbl_terms.Size       = new System.Drawing.Size(290, 44);
            this.linkLbl_terms.TabIndex   = 10;
            this.linkLbl_terms.TabStop    = true;
            this.linkLbl_terms.Text       = "By creating an account, you agree to abide by our terms and conditions.";
            this.linkLbl_terms.TextAlign  = System.Drawing.ContentAlignment.MiddleCenter;

            // btn_close_window
            this.btn_close_window.AutoSize  = true;
            this.btn_close_window.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btn_close_window.ForeColor = System.Drawing.Color.Gray;
            this.btn_close_window.Location  = new System.Drawing.Point(791, 9);
            this.btn_close_window.Name      = "btn_close_window";
            this.btn_close_window.TabIndex  = 8;
            this.btn_close_window.Text      = "X";
            this.btn_close_window.Click    += new System.EventHandler(this.btn_close_window_Click);

            // panel1 — left dark panel
            this.panel1.BackColor = System.Drawing.Color.FromArgb(8, 8, 46);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name     = "panel1";
            this.panel1.Size     = new System.Drawing.Size(397, 562);
            this.panel1.TabIndex = 19;

            this.label6.AutoSize  = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Font      = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location  = new System.Drawing.Point(91, 242);
            this.label6.Name      = "label6";
            this.label6.TabIndex  = 22;
            this.label6.Text      = "Smart Gym Entry Control";

            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location  = new System.Drawing.Point(37, 434);
            this.label5.Name      = "label5";
            this.label5.Size      = new System.Drawing.Size(323, 43);
            this.label5.Text      = "This is the first-time setup for your device login. This login is local and used to manage this device only.";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.label4.AutoSize  = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font      = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location  = new System.Drawing.Point(96, 210);
            this.label4.Name      = "label4";
            this.label4.TabIndex  = 20;
            this.label4.Text      = "Fitness Marvel";

            this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox2.Image     = global::smart_attendance_middleware.Properties.Resources.face_logo;
            this.pictureBox2.Location  = new System.Drawing.Point(131, 281);
            this.pictureBox2.Name      = "pictureBox2";
            this.pictureBox2.Size      = new System.Drawing.Size(134, 142);
            this.pictureBox2.TabIndex  = 19;
            this.pictureBox2.TabStop   = false;

            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image     = global::smart_attendance_middleware.Properties.Resources.logo;
            this.pictureBox1.Location  = new System.Drawing.Point(165, 110);
            this.pictureBox1.Name      = "pictureBox1";
            this.pictureBox1.Size      = new System.Drawing.Size(66, 93);
            this.pictureBox1.TabIndex  = 18;
            this.pictureBox1.TabStop   = false;

            // Form
            this.AutoScaleDimensions  = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode        = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor            = System.Drawing.Color.White;
            this.ClientSize           = new System.Drawing.Size(822, 562);
            this.FormBorderStyle      = System.Windows.Forms.FormBorderStyle.None;
            this.Margin               = new System.Windows.Forms.Padding(2);
            this.Name                 = "UserSetup";
            this.StartPosition        = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                 = "Form1";
            this.Load                += new System.EventHandler(this.Form1_Load);

            this.Controls.Add(this.btn_close_window);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_username);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_password);
            this.Controls.Add(this.btnTogglePassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_confirmpass);
            this.Controls.Add(this.btnToggleConfirmPassword);
            this.Controls.Add(this.chkBox_is_Admin);
            this.Controls.Add(this.lbl_is_Admin);
            this.Controls.Add(this.btnCreateAccount);
            this.Controls.Add(this.linkLbl_terms);
            this.Controls.Add(this.panel1);

            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label                              label1;
        private System.Windows.Forms.Label                              label2;
        private System.Windows.Forms.Label                              label3;
        private smart_attendance_middleware.Controls.RoundedTextBox     txt_username;
        private smart_attendance_middleware.Controls.RoundedTextBox     txt_password;
        private smart_attendance_middleware.Controls.RoundedTextBox     txt_confirmpass;
        private System.Windows.Forms.Label                              btnTogglePassword;
        private System.Windows.Forms.Label                              btnToggleConfirmPassword;
        private smart_attendance_middleware.Controls.ToggleSwitch       chkBox_is_Admin;
        private System.Windows.Forms.Label                              lbl_is_Admin;
        private System.Windows.Forms.Label                              btn_close_window;
        private System.Windows.Forms.LinkLabel                          linkLbl_terms;
        private System.Windows.Forms.Panel                              panel1;
        private System.Windows.Forms.Label                              label6;
        private System.Windows.Forms.Label                              label5;
        private System.Windows.Forms.Label                              label4;
        private System.Windows.Forms.PictureBox                         pictureBox2;
        private System.Windows.Forms.PictureBox                         pictureBox1;
        private System.Windows.Forms.Label                              label8;
        private smart_attendance_middleware.Controls.RoundedButton       btnCreateAccount;
    }
}
