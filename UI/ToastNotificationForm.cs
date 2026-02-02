using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.UI
{
    // Lightweight borderless toast that auto-dismisses (no fade).
    public class ToastNotificationForm : Form
    {
        private readonly Timer _lifeTimer;
        private readonly int _displayMilliseconds;
        private Label _titleLabel;
        private Label _messageLabel;
        private PictureBox _iconBox;
        private bool _disposed;

        // Theme taken from loader / face popup style
        private static readonly Color ThemePanel = Color.FromArgb(18, 18, 66);
        private static readonly Color ThemePanelGradient = Color.FromArgb(10, 10, 54);
        private static readonly Color BorderColor = Color.FromArgb(40, 40, 100);

        public ToastNotificationForm(string title, string message, Icon icon = null, int displayMilliseconds = 5000)
        {
            _displayMilliseconds = displayMilliseconds;
            InitializeComponents(title, message, icon);

            _lifeTimer = new Timer { Interval = _displayMilliseconds };
            _lifeTimer.Tick += LifeTimer_Tick;
        }

        private void LifeTimer_Tick(object sender, EventArgs e)
        {
            _lifeTimer.Stop();
            Close();
        }

        private void InitializeComponents(string title, string message, Icon icon)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            Width = 340;
            Height = 110;
            BackColor = ThemePanel;
            Opacity = 0.95; // Immediate visibility (no fade)
            Padding = new Padding(12);
            DoubleBuffered = true;

            _iconBox = new PictureBox
            {
                Width = 48,
                Height = 48,
                Left = 14,
                Top = 14,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            _iconBox.Image = (icon ?? SystemIcons.Information).ToBitmap();

            _titleLabel = new Label
            {
                AutoSize = false,
                Left = _iconBox.Right + 12,
                Top = 14,
                Width = Width - _iconBox.Right - 26,
                Height = 22,
                Text = title,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                ForeColor = Color.White
            };

            _messageLabel = new Label
            {
                AutoSize = false,
                Left = _iconBox.Right + 12,
                Top = _titleLabel.Bottom + 4,
                Width = Width - _iconBox.Right - 26,
                Height = 48,
                Text = message,
                Font = new Font("Segoe UI", 9.2f),
                ForeColor = Color.Gainsboro
            };

            Controls.Add(_iconBox);
            Controls.Add(_titleLabel);
            Controls.Add(_messageLabel);

            ApplyRoundedRegion(14);
        }

        private void ApplyRoundedRegion(int radius)
        {
            var path = new GraphicsPath();
            int r2 = radius * 2;
            path.AddArc(0, 0, r2, r2, 180, 90);
            path.AddArc(Width - r2 - 1, 0, r2, r2, 270, 90);
            path.AddArc(Width - r2 - 1, Height - r2 - 1, r2, r2, 0, 90);
            path.AddArc(0, Height - r2 - 1, r2, r2, 90, 90);
            path.CloseFigure();
            Region = new Region(path);
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int WS_EX_TOPMOST = 0x00000008;
                const int WS_EX_NOACTIVATE = 0x08000000;
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle r = ClientRectangle;

            using (var lg = new LinearGradientBrush(r, ThemePanel, ThemePanelGradient, 90f))
            {
                e.Graphics.FillRectangle(lg, r);
            }

            using (var accentBrush = new SolidBrush(Color.FromArgb(0, 168, 84)))
            {
                e.Graphics.FillRectangle(accentBrush, new Rectangle(0, 0, 4, r.Height));
            }

            using (var pen = new Pen(BorderColor, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, r.Width - 1, r.Height - 1);
            }
        }

        public void ShowAtBottomRight()
        {
            int margin = 14;
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            Left = workingArea.Right - Width - margin;
            Top = workingArea.Bottom - Height - margin;

            // Stack upwards
            foreach (Form open in Application.OpenForms)
            {
                if (open == this || !(open is ToastNotificationForm)) continue;
                if (Math.Abs(open.Bottom - Bottom) < Height)
                {
                    Top = open.Top - Height - 8;
                }
            }

            Show();
            _lifeTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Ensure timer stopped before disposal cascade
            if (_lifeTimer != null) _lifeTimer.Stop();
            base.OnFormClosed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_lifeTimer != null)
                    {
                        _lifeTimer.Tick -= LifeTimer_Tick;
                        _lifeTimer.Stop();
                        _lifeTimer.Dispose();
                    }
                    if (_iconBox != null)
                    {
                        var img = _iconBox.Image;
                        _iconBox.Image = null;
                        if (img != null) img.Dispose();
                        _iconBox.Dispose();
                    }
                    if (_titleLabel != null) _titleLabel.Dispose();
                    if (_messageLabel != null) _messageLabel.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }

    public static class ToastNotifier
    {
        public static void Show(string title, string message, Icon icon = null, int displayMs = 5000)
        {
            try
            {
                if (Application.MessageLoop)
                {
                    var toast = new ToastNotificationForm(title, message, icon, displayMs);
                    toast.ShowAtBottomRight();
                }
                else
                {
                    Application.OpenForms[0]?.BeginInvoke(new Action(() =>
                    {
                        var toast = new ToastNotificationForm(title, message, icon, displayMs);
                        toast.ShowAtBottomRight();
                    }));
                }
            }
            catch
            {
                // Silent fail to avoid disrupting core logic.
            }
        }
    }
}