using System;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class FacePopupForm : Form
    {
        // Theme colors (match Dashboard dark navy)
        private static readonly Color ThemeBack = Color.FromArgb(8, 8, 46);
        private static readonly Color ThemePanel = Color.FromArgb(18, 18, 66);
        private static readonly Color ThemePanelGradient = Color.FromArgb(10, 10, 54);
        private static readonly Color AccentCheckIn = Color.FromArgb(0, 168, 84);
        private static readonly Color AccentCheckOut = Color.FromArgb(220, 53, 69);

        // Default display duration (seconds) – adjust here
        private const int DEFAULT_DISPLAY_SECONDS = 7;

        private int _memberId;
        private int _enrollmentId;
        private string _action;
        private DateTime _eventTime;
        private int _displayMillis;
        private bool _isPermanent;

        private Timer _lifetimeTimer;

        public FacePopupForm(int memberId, int enrollmentId, string action, DateTime time)
            : this(memberId, enrollmentId, action, time, DEFAULT_DISPLAY_SECONDS) { }

        public FacePopupForm(int memberId, int enrollmentId, string action, DateTime time, int displaySeconds, bool isPermanent = false)
        {
            _isPermanent = isPermanent;
            InitializeComponent();
            DoubleBuffered = true;

            this.ShowInTaskbar = false;

            // Runtime style adjustments
            panelContainer.Paint += PanelContainer_Paint;
            UpdateData(memberId, enrollmentId, action, time, displaySeconds);
            ApplyRoundedCorners(16);

            // Position bottom-right
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            Left = wa.Right - Width - 24;
            Top = wa.Bottom - Height - 24;
        }

        public void UpdateData(int memberId, int enrollmentId, string action, DateTime time, int displaySeconds = DEFAULT_DISPLAY_SECONDS)
        {
            _memberId = memberId;
            _enrollmentId = enrollmentId;
            _action = action ?? "";
            _eventTime = time;
            if (displaySeconds <= 0) displaySeconds = DEFAULT_DISPLAY_SECONDS;
            _displayMillis = displaySeconds * 1000;

            if (LabelInfo != null)
            {
                LabelInfo.Text =
                    $"{_action.ToUpperInvariant()}\r\nEnroll: {_enrollmentId}  Time: {_eventTime:HH:mm:ss}";
            }

            ApplyAccent();
            LoadImage();

            // Re-trigger visibility and timer whenever data is updated
            StartAnimation();
        }

        public void ShowPopup()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(ShowPopup));
                return;
            }

            // Ensure the form is visible and the timer is (re)started
            if (!this.Visible)
            {
                this.Show();
            }

            this.Opacity = 1.0;
            this.BringToFront();
            StartAnimation();
        }

        private Control LabelInfo => this.Controls.Find("labelInfo", true).FirstOrDefault();

        private void ApplyAccent()
        {
            bool isCheckIn = _action.Equals("check-in", StringComparison.OrdinalIgnoreCase);
            panelAccent.BackColor = isCheckIn ? AccentCheckIn : AccentCheckOut;
        }

        private void StartAnimation()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(StartAnimation));
                return;
            }

            this.Opacity = 1;

            if (_lifetimeTimer == null)
            {
                _lifetimeTimer = new Timer();
                _lifetimeTimer.Tick += LifetimeTimer_Tick;
            }

            _lifetimeTimer.Stop();
            _lifetimeTimer.Interval = _displayMillis;
            _lifetimeTimer.Start();
        }

        private void LifetimeTimer_Tick(object sender, EventArgs e)
        {
            _lifetimeTimer.Stop();
            if (_isPermanent)
            {
                this.Hide(); // Use Hide() safely
            }
            else
            {
                this.Close();
            }
        }

        private void PanelContainer_Paint(object sender, PaintEventArgs e)
        {
            Rectangle r = panelContainer.ClientRectangle;
            using (LinearGradientBrush lg = new LinearGradientBrush(r, ThemePanel, ThemePanelGradient, 90f))
            {
                e.Graphics.FillRectangle(lg, r);
            }
            using (Pen border = new Pen(Color.FromArgb(40, 40, 100), 1))
            {
                e.Graphics.DrawRectangle(border, 0, 0, r.Width - 1, r.Height - 1);
            }
        }

        private void ApplyRoundedCorners(int radius)
        {
            try
            {
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int d = radius * 2;
                    path.AddArc(0, 0, d, d, 180, 90);
                    path.AddArc(Width - d - 1, 0, d, d, 270, 90);
                    path.AddArc(Width - d - 1, Height - d - 1, d, d, 0, 90);
                    path.AddArc(0, Height - d - 1, d, d, 90, 90);
                    path.CloseFigure();
                    Region = new Region(path);
                }
            }
            catch { /* ignore */ }
        }

        private void LoadImage()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string faceDir = Path.Combine(baseDir, "FACE");
                string[] candidates =
                {
                    Path.Combine(faceDir, _memberId + ".jpg"),
                    Path.Combine(faceDir, _memberId + ".png"),
                    Path.Combine(faceDir, _enrollmentId + ".jpg"),
                    Path.Combine(faceDir, _enrollmentId + ".png")
                };

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            pictureBoxFace.Image = Image.FromStream(fs);
                        }
                        return;
                    }
                }

                // Fallback visual
                Bitmap bmp = new Bitmap(pictureBoxFace.Width, pictureBoxFace.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.FromArgb(30, 30, 90));
                    string txt = "NO IMAGE";
                    using (Font f = new Font("Segoe UI", 16, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.FromArgb(110, 110, 160)))
                    {
                        SizeF sz = g.MeasureString(txt, f);
                        g.DrawString(txt, f, b,
                            (bmp.Width - sz.Width) / 2f,
                            (bmp.Height - sz.Height) / 2f);
                    }
                }
                pictureBoxFace.Image = bmp;
            }
            catch (Exception ex)
            {
                LogHelper.Log("[FacePopup][LoadImage][ERROR] " + ex.Message);
            }
        }

        // Designer event (kept for safety)
        private void FacePopupForm_Load(object sender, EventArgs e) { }

        // Legacy timer (not used for lifecycle now, but kept wired)
        private void closeTimer_Tick(object sender, EventArgs e)
        {
            // No-op.
        }

        public void ForceClose()
        {
            Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                const int WS_EX_TOPMOST = 0x00000008;
                const int WS_EX_NOACTIVATE = 0x08000000;

                var cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                cp.ExStyle |= WS_EX_TOPMOST | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (_lifetimeTimer != null)
                {
                    _lifetimeTimer.Stop();
                    _lifetimeTimer.Dispose();
                    _lifetimeTimer = null;
                }
                var img = pictureBoxFace.Image;
                if (img != null)
                {
                    pictureBoxFace.Image = null;
                    img.Dispose();
                }
            }
            catch { }
            base.OnFormClosed(e);
        }
    }
}