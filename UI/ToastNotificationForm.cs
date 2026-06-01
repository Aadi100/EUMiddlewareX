using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.UI
{
    [ToolboxItem(true)]
    // Lightweight toast component that can be placed on a form.
    public class ToastNotificationControl : UserControl
    {
        private readonly Timer _lifeTimer;
        private int _displayMilliseconds;
        private Label _titleLabel;
        private Label _messageLabel;
        private PictureBox _iconBox;
        private bool _disposed;

        // Theme taken from loader / face popup style
        private static readonly Color ThemePanel = Color.FromArgb(18, 18, 66);
        private static readonly Color ThemePanelGradient = Color.FromArgb(10, 10, 54);
        private static readonly Color BorderColor = Color.FromArgb(40, 40, 100);

        public ToastNotificationControl()
        {
            _displayMilliseconds = 5000;
            InitializeComponents();

            _lifeTimer = new Timer();
            _lifeTimer.Tick += LifeTimer_Tick;
        }

        private void LifeTimer_Tick(object sender, EventArgs e)
        {
            _lifeTimer.Stop();
            this.Visible = false;   
        }

        private void InitializeComponents()
        {
            // forcing hard size constraints to override designer defaults
            this.Size = new Size(260, 70);
            this.MinimumSize = new Size(260, 70);
            this.MaximumSize = new Size(260, 70);
            
            BackColor = ThemePanel;
            Padding = new Padding(6);
            DoubleBuffered = true;
            Visible = true; // Show in designer by default

            _iconBox = new PictureBox
            {
                Width = 28,
                Height = 28,
                Left = 10,
                Top = 10,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = SystemIcons.Information.ToBitmap()
            };

            _titleLabel = new Label
            {
                AutoSize = false,
                Left = 46, 
                Top = 10,
                Width = Width - 56,
                Height = 18,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "Notification Title"
            };

            _messageLabel = new Label
            {
                AutoSize = false,
                Left = 46,
                Top = 28,
                Width = Width - 56,
                Height = 32,
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = Color.Gainsboro,
                Text = "Notification message appears here."
            };

            Controls.Add(_iconBox);
            Controls.Add(_titleLabel);
            Controls.Add(_messageLabel);

            ApplyRoundedRegion(10);
        }

        public void UpdateToast(string title, string message, Icon icon = null, int displayMs = 5000)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateToast(title, message, icon, displayMs)));
                return;
            }

            _displayMilliseconds = displayMs;
            _titleLabel.Text = title;
            _messageLabel.Text = message;
            _iconBox.Image = (icon ?? SystemIcons.Information).ToBitmap();

            this.Visible = true;

            // Use SetChildIndex instead of BringToFront to avoid window activation
            if (this.Parent != null)
            {
                this.Parent.Controls.SetChildIndex(this, 0);
            }

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

                string message = $"{name}\n{defaultMsg}";

                Icon icon = SystemIcons.Information;
                if (title.IndexOf("Created", StringComparison.OrdinalIgnoreCase) >= 0) icon = SystemIcons.Information;
                else if (title.IndexOf("Updated", StringComparison.OrdinalIgnoreCase) >= 0) icon = SystemIcons.Application;
                else if (title.IndexOf("Deleted", StringComparison.OrdinalIgnoreCase) >= 0) icon = SystemIcons.Warning;

                UpdateToast(title, message, icon);
            }
            catch { }
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
}