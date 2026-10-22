using smart_attendance_middleware.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public enum MsgType { Info, Success, Warning, Error }

    public partial class ModernMessageBox : Form
    {
        // ── Palette ──────────────────────────────────────────────────────────────
        private static readonly Color _navy    = Color.FromArgb(30, 38, 90);
        private static readonly Color _muted   = Color.FromArgb(130, 138, 165);
        private static readonly Color _bg      = Color.FromArgb(245, 246, 250);
        private static readonly Color _border  = Color.FromArgb(220, 224, 238);
        private static readonly Color _white   = Color.White;

        private static readonly Color _greenFg  = Color.FromArgb(16, 120, 70);
        private static readonly Color _greenBg  = Color.FromArgb(209, 250, 229);
        private static readonly Color _redFg    = Color.FromArgb(185, 28, 28);
        private static readonly Color _redBg    = Color.FromArgb(254, 226, 226);
        private static readonly Color _amberFg  = Color.FromArgb(146, 64, 14);
        private static readonly Color _amberBg  = Color.FromArgb(254, 243, 199);
        private static readonly Color _grayFg   = Color.FromArgb(107, 114, 128);
        private static readonly Color _grayBg   = Color.FromArgb(243, 244, 246);

        private ModernMessageBox() { InitializeComponent(); }

        private void pnlHeader_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            using (var pen = new System.Drawing.Pen(_border))
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
        }

        private void pnlFooter_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            using (var pen = new System.Drawing.Pen(_border))
                e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private static (Color bg, Color fg, Color accent, string icon) TypeStyle(MsgType t)
        {
            switch (t)
            {
                case MsgType.Success: return (_greenBg, _greenFg, Color.FromArgb(16, 185, 129), "✓");
                case MsgType.Warning: return (_amberBg, _amberFg, Color.FromArgb(245, 158, 11), "!");
                case MsgType.Error:   return (_redBg,   _redFg,   Color.FromArgb(239, 68, 68),  "✕");
                default:              return (_navy,     _white,   _navy,                         "i");
            }
        }

        private static Panel MakeAccentBar(Color accent)
        {
            return new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 4,
                BackColor = accent
            };
        }

        // ── Show (info / success / warning / error) ───────────────────────────────
        public static void Show(IWin32Window owner, string title, string message,
            MsgType type = MsgType.Info, string confirmText = "OK")
        {
            var (btnBg, btnFg, accent, _) = TypeStyle(type);
            using (var dlg = new ModernMessageBox())
            {
                dlg.Text             = title;
                dlg.lblTitle.Text    = title;
                dlg.lblSubtitle.Text = "";
                dlg.ClientSize       = new Size(460, 230);

                dlg.pnlOuter.Controls.Add(MakeAccentBar(accent));

                var lbl = new Label
                {
                    Text      = message,
                    AutoSize  = false,
                    Dock      = DockStyle.Fill,
                    Font      = new Font("Segoe UI", 9.5f),
                    ForeColor = _navy,
                    BackColor = Color.Transparent
                };
                dlg.pnlContent.Controls.Add(lbl);

                dlg.btnRight.Text      = confirmText;
                dlg.btnRight.BackColor = btnBg;
                dlg.btnRight.ForeColor = btnFg;
                dlg.btnRight.Click    += (s, e) => dlg.Close();

                dlg.ShowDialog(owner);
            }
        }

        // ── ShowConfirm (Yes / No) ────────────────────────────────────────────────
        public static bool ShowConfirm(IWin32Window owner, string title, string message,
            string confirmText = "Yes", string cancelText = "No", MsgType type = MsgType.Warning)
        {
            var (btnBg, btnFg, accent, _) = TypeStyle(type);
            bool result = false;
            using (var dlg = new ModernMessageBox())
            {
                dlg.Text             = title;
                dlg.lblTitle.Text    = title;
                dlg.lblSubtitle.Text = "";
                dlg.ClientSize       = new Size(460, 230);

                dlg.pnlOuter.Controls.Add(MakeAccentBar(accent));

                var lbl = new Label
                {
                    Text      = message,
                    AutoSize  = false,
                    Dock      = DockStyle.Fill,
                    Font      = new Font("Segoe UI", 9.5f),
                    ForeColor = _navy,
                    BackColor = Color.Transparent
                };
                dlg.pnlContent.Controls.Add(lbl);

                dlg.btnLeft.Text      = cancelText;
                dlg.btnLeft.BackColor = _grayBg;
                dlg.btnLeft.ForeColor = _grayFg;
                dlg.btnLeft.Visible   = true;
                dlg.btnLeft.Click    += (s, e) => { dlg.DialogResult = DialogResult.No;  dlg.Close(); };

                dlg.btnRight.Text      = confirmText;
                dlg.btnRight.BackColor = btnBg;
                dlg.btnRight.ForeColor = btnFg;
                dlg.btnRight.Click    += (s, e) => { dlg.DialogResult = DialogResult.Yes; dlg.Close(); };

                dlg.AcceptButton = dlg.btnRight;
                dlg.CancelButton = dlg.btnLeft;

                result = dlg.ShowDialog(owner) == DialogResult.Yes;
            }
            return result;
        }

        // ── ShowBackupCode ────────────────────────────────────────────────────────
        public static void ShowBackupCode(IWin32Window owner, string backupCode)
        {
            using (var dlg = new ModernMessageBox())
            {
                dlg.Text             = "Backup Code";
                dlg.lblTitle.Text    = "Account Created";
                dlg.lblSubtitle.Text = "Save this backup code in a secure location";
                dlg.ClientSize       = new Size(500, 290);

                dlg.pnlOuter.Controls.Add(MakeAccentBar(Color.FromArgb(16, 185, 129)));

                var lblInfo = new Label
                {
                    Text      = "Use this code to log in if you forget your password.\nStore it somewhere safe — it cannot be recovered.",
                    AutoSize  = false,
                    Dock      = DockStyle.Top,
                    Height    = 36,
                    Font      = new Font("Segoe UI", 9.5f),
                    ForeColor = _navy,
                    BackColor = Color.Transparent
                };

                var pnlCode = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = _white };
                pnlCode.Paint += (s, e) =>
                {
                    using (var pen = new Pen(_border))
                        e.Graphics.DrawRectangle(pen, 0, 0, pnlCode.Width - 1, pnlCode.Height - 1);
                };
                pnlCode.Controls.Add(new Label
                {
                    Text      = backupCode,
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font      = new Font("Consolas", 16f, FontStyle.Bold),
                    ForeColor = _navy,
                    BackColor = Color.Transparent
                });

                dlg.pnlContent.Controls.Add(pnlCode);
                dlg.pnlContent.Controls.Add(lblInfo);

                dlg.btnLeft.Text      = "Copy Code";
                dlg.btnLeft.BackColor = _navy;
                dlg.btnLeft.ForeColor = _white;
                dlg.btnLeft.Visible   = true;
                dlg.btnLeft.Click    += (s, e) =>
                {
                    Clipboard.SetText(backupCode);
                    dlg.btnLeft.Text    = "Copied!";
                    dlg.btnLeft.Enabled = false;
                };

                dlg.btnRight.Text      = "OK, I've saved it";
                dlg.btnRight.BackColor = _greenBg;
                dlg.btnRight.ForeColor = _greenFg;
                dlg.btnRight.Click    += (s, e) => dlg.Close();

                dlg.ShowDialog(owner);
            }
        }

        // ── ShowPasswordPrompt ────────────────────────────────────────────────────
        public static string ShowPasswordPrompt(IWin32Window owner, string title, string subtitle,
            string promptText, string confirmText = "OK")
        {
            string result = null;
            using (var dlg = new ModernMessageBox())
            {
                dlg.Text             = title;
                dlg.lblTitle.Text    = title;
                dlg.lblSubtitle.Text = subtitle;
                dlg.ClientSize       = new Size(460, 240);

                dlg.pnlOuter.Controls.Add(MakeAccentBar(_navy));

                var lbl = new Label
                {
                    Text      = promptText,
                    AutoSize  = false,
                    Dock      = DockStyle.Top,
                    Height    = 24,
                    Font      = new Font("Segoe UI", 9.5f),
                    ForeColor = _navy,
                    BackColor = Color.Transparent
                };
                var txt = new TextBox
                {
                    UseSystemPasswordChar = true,
                    Dock      = DockStyle.Top,
                    Height    = 32,
                    Font      = new Font("Segoe UI", 10f),
                    BackColor = _white,
                    ForeColor = _navy
                };
                dlg.pnlContent.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.Transparent });
                dlg.pnlContent.Controls.Add(txt);
                dlg.pnlContent.Controls.Add(lbl);

                dlg.btnLeft.Text      = "Cancel";
                dlg.btnLeft.BackColor = _grayBg;
                dlg.btnLeft.ForeColor = _grayFg;
                dlg.btnLeft.Visible   = true;
                dlg.btnLeft.Click    += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

                dlg.btnRight.Text      = confirmText;
                dlg.btnRight.BackColor = _navy;
                dlg.btnRight.ForeColor = _white;
                dlg.btnRight.Click    += (s, e) => { dlg.DialogResult = DialogResult.OK; dlg.Close(); };

                dlg.AcceptButton = dlg.btnRight;
                dlg.CancelButton = dlg.btnLeft;

                if (dlg.ShowDialog(owner) == DialogResult.OK)
                    result = txt.Text;
            }
            return result;
        }

        // ── ShowChangePasswordPrompt ──────────────────────────────────────────────
        public static (string newPw, string confirmPw)? ShowChangePasswordPrompt(IWin32Window owner)
        {
            (string, string)? result = null;
            using (var dlg = new ModernMessageBox())
            {
                dlg.Text             = "Change Password";
                dlg.lblTitle.Text    = "Change Password";
                dlg.lblSubtitle.Text = "Enter and confirm your new password";
                dlg.ClientSize       = new Size(460, 300);

                dlg.pnlOuter.Controls.Add(MakeAccentBar(_navy));

                var lbl2 = new Label { Text = "Confirm new password:", AutoSize = false, Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9.5f), ForeColor = _navy, BackColor = Color.Transparent };
                var txt2 = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Top, Height = 32, Font = new Font("Segoe UI", 10f), BackColor = _white, ForeColor = _navy };
                var gap  = new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent };
                var lbl1 = new Label { Text = "New password (min 8 characters):", AutoSize = false, Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI", 9.5f), ForeColor = _navy, BackColor = Color.Transparent };
                var txt1 = new TextBox { UseSystemPasswordChar = true, Dock = DockStyle.Top, Height = 32, Font = new Font("Segoe UI", 10f), BackColor = _white, ForeColor = _navy };

                dlg.pnlContent.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.Transparent });
                dlg.pnlContent.Controls.Add(txt2);
                dlg.pnlContent.Controls.Add(lbl2);
                dlg.pnlContent.Controls.Add(gap);
                dlg.pnlContent.Controls.Add(txt1);
                dlg.pnlContent.Controls.Add(lbl1);

                dlg.btnLeft.Text      = "Cancel";
                dlg.btnLeft.BackColor = _grayBg;
                dlg.btnLeft.ForeColor = _grayFg;
                dlg.btnLeft.Visible   = true;
                dlg.btnLeft.Click    += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

                dlg.btnRight.Text      = "Change Password";
                dlg.btnRight.BackColor = _navy;
                dlg.btnRight.ForeColor = _white;
                dlg.btnRight.Click    += (s, e) => { dlg.DialogResult = DialogResult.OK; dlg.Close(); };

                dlg.AcceptButton = dlg.btnRight;
                dlg.CancelButton = dlg.btnLeft;

                if (dlg.ShowDialog(owner) == DialogResult.OK)
                    result = (txt1.Text, txt2.Text);
            }
            return result;
        }

        // ── ShowDeviceSelector ────────────────────────────────────────────────────
        public static List<Device> ShowDeviceSelector(IWin32Window owner, List<Device> devices,
            string title = "Select Devices")
        {
            var selected = new List<Device>();
            if (devices == null || devices.Count == 0) return selected;

            using (var dlg = new ModernMessageBox())
            {
                dlg.Text             = title;
                dlg.lblTitle.Text    = title;
                dlg.lblSubtitle.Text = "Check each device you want to include";
                dlg.ClientSize       = new Size(520, 420);

                dlg.pnlOuter.Controls.Add(MakeAccentBar(_navy));

                var clb = new CheckedListBox
                {
                    Dock         = DockStyle.Fill,
                    CheckOnClick = true,
                    Font         = new Font("Segoe UI", 9.5f),
                    ForeColor    = _navy,
                    BackColor    = _white,
                    BorderStyle  = BorderStyle.None
                };
                foreach (var d in devices)
                {
                    string label = !string.IsNullOrWhiteSpace(d.Location)
                        ? $"{d.Location}  ({d.DeviceIP})"
                        : d.DeviceIP ?? "(unknown)";
                    clb.Items.Add(label, true);
                }

                dlg.pnlContent.Padding = new Padding(24, 8, 24, 0);
                dlg.pnlContent.Controls.Add(clb);

                dlg.btnLeft.Text      = "Cancel";
                dlg.btnLeft.BackColor = _grayBg;
                dlg.btnLeft.ForeColor = _grayFg;
                dlg.btnLeft.Visible   = true;
                dlg.btnLeft.Click    += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

                dlg.btnRight.Text      = "Confirm";
                dlg.btnRight.BackColor = _navy;
                dlg.btnRight.ForeColor = _white;
                dlg.btnRight.Click    += (s, e) => { dlg.DialogResult = DialogResult.OK; dlg.Close(); };

                dlg.AcceptButton = dlg.btnRight;
                dlg.CancelButton = dlg.btnLeft;

                if (dlg.ShowDialog(owner) == DialogResult.OK)
                    foreach (int idx in clb.CheckedIndices)
                        if (idx >= 0 && idx < devices.Count)
                            selected.Add(devices[idx]);
            }
            return selected;
        }
    }
}
