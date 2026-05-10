using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    /// <summary>
    /// A rectangular animated gradient loader.
    /// When running  → a glowing gradient bar sweeps back and forth.
    /// When stopped  → displays a customisable idle message.
    /// Drop onto any form from the Toolbox — no popup, no modal.
    /// </summary>
    public class RectangularLoader : Control
    {
        // ── Timer ──────────────────────────────────────────────────────────────
        private readonly Timer _timer;
        private float _position;
        private int   _direction = 1;
        private bool  _running;

        // ── Appearance ─────────────────────────────────────────────────────────
        private int   _barHeight    = 6;
        private int   _cornerRadius = 10;
        private Color _startColor   = Color.FromArgb(255, 91, 190);
        private Color _middleColor  = Color.FromArgb(122, 68, 242);
        private Color _endColor     = Color.FromArgb(63, 107, 255);
        private Color _trackColor   = Color.FromArgb(45, 38, 90);
        private Color _bgColor      = Color.FromArgb(30, 24, 70);
        private Color _idleForeColor = Color.FromArgb(140, 130, 180);
        private string _idleText    = "● Idle — waiting for activity";
        private string _activeText  = "";
        private float  _sweepWidth  = 0.35f;

        // ── Properties ─────────────────────────────────────────────────────────
        [Browsable(true), Category("Loader")]
        public int BarHeight
        {
            get => _barHeight;
            set { _barHeight = Math.Max(2, value); Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public Color StartColor
        {
            get => _startColor;
            set { _startColor = value; Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public Color MiddleColor
        {
            get => _middleColor;
            set { _middleColor = value; Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public Color EndColor
        {
            get => _endColor;
            set { _endColor = value; Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public Color LoaderBackColor
        {
            get => _bgColor;
            set { _bgColor = value; Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public Color IdleForeColor
        {
            get => _idleForeColor;
            set { _idleForeColor = value; Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public string IdleText
        {
            get => _idleText;
            set { _idleText = value; if (!_running) Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public string ActiveText
        {
            get => _activeText;
            set { _activeText = value; if (_running) Invalidate(); }
        }

        [Browsable(true), Category("Loader")]
        public float SweepWidth
        {
            get => _sweepWidth;
            set { _sweepWidth = Math.Min(1f, Math.Max(0.05f, value)); }
        }

        public bool IsRunning => _running;

        // ── Constructor ────────────────────────────────────────────────────────
        public RectangularLoader()
        {
            // Enable transparent-background support BEFORE assigning BackColor
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer         |
                     ControlStyles.AllPaintingInWmPaint          |
                     ControlStyles.ResizeRedraw                  |
                     ControlStyles.UserPaint, true);

            UpdateStyles();

            Size      = new Size(300, 70);
            BackColor = Color.Transparent;   // now safe — style flag is set

            _timer = new Timer { Interval = 16 };
            _timer.Tick += (s, e) =>
            {
                _position += _direction * 0.018f;
                if (_position >= 1f) { _position = 1f; _direction = -1; }
                if (_position <= 0f) { _position = 0f; _direction =  1; }
                Invalidate();
            };
        }

        // ── Public API ─────────────────────────────────────────────────────────
        /// <summary>Starts the sweep animation. Optionally sets the label shown while active.</summary>
        public void Start(string activeLabel = null)
        {
            _running = true;
            if (activeLabel != null) _activeText = activeLabel;
            _timer.Start();
            Invalidate();
        }

        /// <summary>Stops the animation and shows the idle message.</summary>
        public void Stop()
        {
            _running = false;
            _timer.Stop();
            Invalidate();
        }

        // ── Painting ───────────────────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g      = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var bounds = ClientRectangle;
            if (bounds.Width < 4 || bounds.Height < 4) return;

            // 1. Background rounded rect
            using (var bgPath  = RoundedRect(bounds, _cornerRadius))
            using (var bgBrush = new SolidBrush(_bgColor))
                g.FillPath(bgBrush, bgPath);

            // 2. Track bar (centred vertically)
            int trackY   = (bounds.Height - _barHeight) / 2;
            int trackPad = Math.Max(_cornerRadius, 8);
            var trackRect = new Rectangle(
                trackPad, trackY,
                bounds.Width - trackPad * 2, _barHeight);

            if (trackRect.Width > 0 && trackRect.Height > 0)
            {
                using (var trackPath  = RoundedRect(trackRect, _barHeight / 2))
                using (var trackBrush = new SolidBrush(_trackColor))
                    g.FillPath(trackBrush, trackPath);
            }

            if (_running)
            {
                // 3. Animated gradient sweep bar
                float sweepPx   = trackRect.Width * _sweepWidth;
                float maxLeft   = trackRect.Width - sweepPx;
                float sweepLeft = trackRect.X + _position * maxLeft;
                var   sweepRect = new RectangleF(sweepLeft, trackY, sweepPx, _barHeight);

                if (sweepRect.Width > 1 && sweepRect.Height > 0)
                {
                    using (var lg = new LinearGradientBrush(
                        new PointF(sweepRect.Left, sweepRect.Top),
                        new PointF(sweepRect.Right, sweepRect.Top),
                        _startColor, _endColor))
                    {
                        var blend = new ColorBlend
                        {
                            Colors    = new[] { _startColor, _middleColor, _endColor },
                            Positions = new[] { 0f, 0.5f, 1f }
                        };
                        lg.InterpolationColors = blend;

                        using (var sweepPath = RoundedRect(Rectangle.Round(sweepRect), _barHeight / 2))
                            g.FillPath(lg, sweepPath);
                    }

                    // Soft glow on the sweep bar
                    using (var glowPath  = RoundedRect(Rectangle.Round(sweepRect), _barHeight / 2))
                    using (var glowBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                        g.FillPath(glowBrush, glowPath);
                }

                // 4. Active label below the bar
                if (!string.IsNullOrEmpty(_activeText))
                    DrawCentredLabel(g, bounds, trackY + _barHeight, _activeText,
                                     Color.FromArgb(200, 200, 220));
            }
            else
            {
                // 5. Idle state text below the bar
                DrawCentredLabel(g, bounds, trackY + _barHeight, _idleText, _idleForeColor);
            }

            // 6. Subtle border
            using (var borderPath = RoundedRect(
                new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1), _cornerRadius))
            using (var borderPen = new Pen(Color.FromArgb(60, 130, 120, 200), 1f))
                g.DrawPath(borderPen, borderPath);
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private void DrawCentredLabel(Graphics g, Rectangle bounds, int barBottom,
                                      string text, Color color)
        {
            if (string.IsNullOrEmpty(text)) return;

            using (var font  = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point))
            using (var brush = new SolidBrush(color))
            {
                var sf = new StringFormat
                {
                    Alignment     = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                var labelRect = new Rectangle(
                    bounds.X + 8,
                    barBottom + 4,
                    bounds.Width - 16,
                    bounds.Height - barBottom - 8);

                if (labelRect.Height > 0)
                    g.DrawString(text, font, brush, labelRect, sf);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }
            int d = radius * 2;
            path.AddArc(rect.X,          rect.Y,           d, d, 180, 90);
            path.AddArc(rect.Right - d,  rect.Y,           d, d, 270, 90);
            path.AddArc(rect.Right - d,  rect.Bottom - d,  d, d,   0, 90);
            path.AddArc(rect.X,          rect.Bottom - d,  d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer?.Dispose();
            base.Dispose(disposing);
        }
    }
}