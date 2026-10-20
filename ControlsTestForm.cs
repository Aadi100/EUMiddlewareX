using smart_attendance_middleware.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public class ControlsTestForm : Form
    {
        public ControlsTestForm()
        {
            Text            = "Controls Test";
            Size            = new Size(480, 560);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            BackColor       = Color.FromArgb(245, 246, 250);
            Font            = new Font("Segoe UI", 9.5f);

            BuildUI();
        }

        private void BuildUI()
        {
            int x = 40, y = 40, w = 400;

            // ── Title ────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text      = "Rounded Controls Test",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 38, 90),
                Location  = new Point(x, y),
                AutoSize  = true,
            };
            Controls.Add(lblTitle);
            y += 36;

            var lblSub = new Label
            {
                Text      = "Testing RoundedTextBox, RoundedComboBox and RoundedButton",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location  = new Point(x, y),
                AutoSize  = true,
            };
            Controls.Add(lblSub);
            y += 40;

            // ── TextBox section ──────────────────────────────────────────
            AddLabel("Full Name", x, y); y += 22;
            var txt1 = new RoundedTextBox { Location = new Point(x, y), Width = w, Height = 42, Placeholder = "Enter your full name" };
            Controls.Add(txt1); y += 56;

            AddLabel("Email Address", x, y); y += 22;
            var txt2 = new RoundedTextBox { Location = new Point(x, y), Width = w, Height = 42, Placeholder = "Enter your email" };
            Controls.Add(txt2); y += 56;

            AddLabel("Password", x, y); y += 22;
            var txt3 = new RoundedTextBox { Location = new Point(x, y), Width = w, Height = 42, Placeholder = "Enter password", UseSystemPasswordChar = true };
            Controls.Add(txt3); y += 56;

            // ── ComboBox section ─────────────────────────────────────────
            AddLabel("Role", x, y); y += 22;
            var cmb = new RoundedComboBox { Location = new Point(x, y), Width = w, Height = 42 };
            cmb.Items.AddRange(new object[] { "Admin", "Manager", "Staff", "Member" });
            cmb.SelectedIndex = 0;
            Controls.Add(cmb); y += 56;

            // ── Button section ───────────────────────────────────────────
            y += 8;
            var btnPrimary = new RoundedButton
            {
                Text     = "Save",
                Location = new Point(x, y),
                Size     = new Size(120, 42),
                NormalColor = Color.FromArgb(59, 130, 246),
                HoverColor  = Color.FromArgb(37, 99, 235),
            };
            Controls.Add(btnPrimary);

            var btnSuccess = new RoundedButton
            {
                Text     = "Confirm",
                Location = new Point(x + 136, y),
                Size     = new Size(120, 42),
                NormalColor = Color.FromArgb(22, 163, 74),
                HoverColor  = Color.FromArgb(15, 130, 58),
            };
            Controls.Add(btnSuccess);

            var btnCancel = new RoundedButton
            {
                Text        = "Cancel",
                Location    = new Point(x + 272, y),
                Size        = new Size(120, 42),
                NormalColor = Color.FromArgb(240, 242, 248),
                HoverColor  = Color.FromArgb(220, 224, 235),
                ForeColor   = Color.FromArgb(30, 38, 90),
                BorderColor = Color.FromArgb(209, 213, 219),
            };
            Controls.Add(btnCancel);

            btnCancel.Click += (s, e) => Close();
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(80, 88, 112),
                Location  = new Point(x, y),
                AutoSize  = true,
            });
        }
    }
}
