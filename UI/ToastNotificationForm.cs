using System;
using System.ComponentModel;
using System.Drawing;
using smart_attendance_middleware;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace smart_attendance_middleware.UI
{
    public enum ToastType { Info, Success, Warning, Error }

    [ToolboxItem(true)]
    public class ToastNotificationControl : UserControl
    {
        private readonly Timer _lifeTimer;
        private int _displayMilliseconds;
        private Label _titleLabel;
        private Label _messageLabel;
        private bool _disposed;

        private ToastType _toastType = ToastType.Info;

        private static readonly Color BgColor      = Color.FromArgb(26, 32, 64);
        private static readonly Color BorderColor  = Color.FromArgb(45, 52, 90);

        private static readonly Color AccentInfo    = Color.FromArgb(59,  130, 246);
        private static readonly Color AccentSuccess = Color.FromArgb(16,  185, 129);
        private static readonly Color AccentWarning = Color.FromArgb(245, 158,  11);
        private static readonly Color AccentError   = Color.FromArgb(239,  68,  68);

        private const int CardW = 320;
        private const int CardH = 76;
        private const int AccentW = 5;
        private const int IconDiam = 30;
        private const int IconLeft = 18;
        private const int TextLeft = 62;

        public ToastNotificationControl()
        {
            _displayMilliseconds = 5000;
            InitializeComponents();
            _lifeTimer = new Timer();
            _lifeTimer.Tick += LifeTimer_Tick;
        }

        private Color AccentColor
        {
            get
            {
                switch (_toastType)
                {
                    case ToastType.Success: return AccentSuccess;
                    case ToastType.Warning: return AccentWarning;
                    case ToastType.Error:   return AccentError;
                    default:                return AccentInfo;
                }
            }
        }

        private void LifeTimer_Tick(object sender, EventArgs e)
        {
            _lifeTimer.Stop();
            Visible = false;
        }

        private void InitializeComponents()
        {
            Size          = new Size(CardW, CardH);
            BackColor     = BgColor;
            DoubleBuffered = true;
            Visible       = true;

            _titleLabel = new Label
            {
                AutoSize  = false,
                Left      = TextLeft,
                Top       = 16,
                Width     = Width - TextLeft - 10,
                Height    = 20,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text      = "Notification Title"
            };

            _messageLabel = new Label
            {
                AutoSize  = false,
                Left      = TextLeft,
                Top       = 38,
                Width     = Width - TextLeft - 10,
                Height    = 22,
                Font      = new Font("Segoe UI", 8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 170, 200),
                BackColor = Color.Transparent,
                Text      = "Notification message appears here."
            };

            Controls.Add(_titleLabel);
            Controls.Add(_messageLabel);

            ApplyRoundedRegion(10);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_titleLabel != null)   _titleLabel.Width   = Width - TextLeft - 10;
            if (_messageLabel != null) _messageLabel.Width = Width - TextLeft - 10;
            ApplyRoundedRegion(10);
            Invalidate();
        }

        public void UpdateToast(string title, string message, Icon icon = null, int displayMs = 5000)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateToast(title, message, icon, displayMs))); return; }

            _displayMilliseconds = displayMs;
            _titleLabel.Text   = title;
            _messageLabel.Text = message;

            // Map icon to ToastType for accent colour + drawn icon
            if (icon == null || icon == SystemIcons.Information || icon == SystemIcons.Application)
                _toastType = ToastType.Info;
            else if (icon == SystemIcons.Warning)
                _toastType = ToastType.Warning;
            else if (icon == SystemIcons.Error || icon == SystemIcons.Hand)
                _toastType = ToastType.Error;
            else
                _toastType = ToastType.Info;

            Visible = true;
            Invalidate();

            if (Parent != null) Parent.Controls.SetChildIndex(this, 0);

            _lifeTimer.Stop();
            _lifeTimer.Interval = _displayMilliseconds;
            _lifeTimer.Start();
        }

        public void UpdateToastTyped(string title, string message, ToastType type, int displayMs = 5000)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateToastTyped(title, message, type, displayMs))); return; }

            _displayMilliseconds = displayMs;
            _titleLabel.Text   = title;
            _messageLabel.Text = message;
            _toastType         = type;

            Visible = true;
            Invalidate();

            if (Parent != null) Parent.Controls.SetChildIndex(this, 0);

            _lifeTimer.Stop();
            _lifeTimer.Interval = _displayMilliseconds;
            _lifeTimer.Start();
        }

        public void UpdateFromData(Newtonsoft.Json.Linq.JObject data, string title, string defaultMsg)
        {
            try
            {
                int memberId = 0;
                if (data["id"] != null) memberId = data.Value<int>("id");
                else if (data["member_id"] != null) memberId = data.Value<int>("member_id");

                string name = null;
                if (data["name"] != null) name = data.Value<string>("name");
                else if (data["full_name"] != null) name = data.Value<string>("full_name");

                if (string.IsNullOrEmpty(name)) name = $"Member #{memberId}";

                UpdateToastTyped(title, $"{name}\n{defaultMsg}", ResolveType(title));
            }
            catch { }
        }

        private static ToastType ResolveType(string title)
        {
            if (string.IsNullOrEmpty(title)) return ToastType.Info;
            if (title.IndexOf("Deleted",      StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Disconnect",   StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Failed",       StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Error",        StringComparison.OrdinalIgnoreCase) >= 0)
                return ToastType.Error;
            if (title.IndexOf("Warning",      StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Expired",      StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Pending",      StringComparison.OrdinalIgnoreCase) >= 0)
                return ToastType.Warning;
            if (title.IndexOf("Created",      StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Response",     StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Connected",    StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Success",      StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Complete",     StringComparison.OrdinalIgnoreCase) >= 0)
                return ToastType.Success;
            return ToastType.Info;
        }

        private void ApplyRoundedRegion(int radius)
        {
            var path = new GraphicsPath();
            int r2 = radius * 2;
            path.AddArc(0,            0,             r2, r2, 180, 90);
            path.AddArc(Width-r2-1,   0,             r2, r2, 270, 90);
            path.AddArc(Width-r2-1,   Height-r2-1,   r2, r2,   0, 90);
            path.AddArc(0,            Height-r2-1,   r2, r2,  90, 90);
            path.CloseFigure();
            Region = new Region(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode      = SmoothingMode.AntiAlias;
            g.TextRenderingHint  = TextRenderingHint.ClearTypeGridFit;

            var rc = ClientRectangle;

            // Background
            using (var bg = new SolidBrush(BgColor))
                g.FillRectangle(bg, rc);

            // Left accent stripe — Region already clips the card corners, plain rect is enough
            using (var ab = new SolidBrush(AccentColor))
                g.FillRectangle(ab, new Rectangle(0, 0, AccentW, rc.Height));

            // Icon circle
            int iconTop = (rc.Height - IconDiam) / 2;
            var iconRect = new Rectangle(IconLeft, iconTop, IconDiam, IconDiam);
            using (var circleBrush = new SolidBrush(Color.FromArgb(40, AccentColor)))
                g.FillEllipse(circleBrush, iconRect);

            // Icon glyph
            string glyph;
            switch (_toastType)
            {
                case ToastType.Warning: glyph = "!"; break;
                case ToastType.Error:   glyph = "×"; break;
                case ToastType.Success: glyph = "✓"; break;
                default:                glyph = "i"; break;
            }
            using (var gb = new SolidBrush(AccentColor))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(glyph, new Font("Segoe UI", 9.5f, FontStyle.Bold), gb, iconRect, sf);
            }

            // Border
            using (var pen = new Pen(BorderColor, 1))
                g.DrawRectangle(pen, 0, 0, rc.Width - 1, rc.Height - 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_lifeTimer != null) { _lifeTimer.Tick -= LifeTimer_Tick; _lifeTimer.Stop(); _lifeTimer.Dispose(); }
                    if (_titleLabel != null) _titleLabel.Dispose();
                    if (_messageLabel != null) _messageLabel.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}

