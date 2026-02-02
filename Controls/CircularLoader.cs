using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    public class CircularLoader : Control
    {
        private readonly Timer _timer;
        private float _angle;
        private int _thickness = 10;
        private int _sweep = 270;

        // Updated default gradient to match logo (pink -> violet -> blue)
        private Color _startColor = Color.FromArgb(255, 91, 190);       // Pink
        private Color _middleColor = Color.FromArgb(122, 68, 242);      // Violet
        private Color _endColor = Color.FromArgb(63, 107, 255);         // Blue

        private bool _enableShadow = true;

        [Browsable(true)]
        public int Thickness
        {
            get => _thickness;
            set { _thickness = Math.Max(2, value); Invalidate(); }
        }

        [Browsable(true)]
        public int SweepAngle
        {
            get => _sweep;
            set { _sweep = Math.Min(360, Math.Max(30, value)); Invalidate(); }
        }

        [Browsable(true)]
        public Color StartColor
        {
            get => _startColor;
            set { _startColor = value; Invalidate(); }
        }

        [Browsable(true)]
        public Color MiddleColor
        {
            get => _middleColor;
            set { _middleColor = value; Invalidate(); }
        }

        [Browsable(true)]
        public Color EndColor
        {
            get => _endColor;
            set { _endColor = value; Invalidate(); }
        }

        [Browsable(true)]
        public bool EnableShadow
        {
            get => _enableShadow;
            set { _enableShadow = value; Invalidate(); }
        }

        public CircularLoader()
        {
            DoubleBuffered = true;
            Size = new Size(120, 120);
            _timer = new Timer { Interval = 16 }; // ~60 FPS
            _timer.Tick += (s, e) =>
            {
                _angle += 4f;
                if (_angle >= 360f) _angle -= 360f;
                Invalidate();
            };
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            int pad = _thickness + 6;
            var arcRect = new Rectangle(rect.X + pad, rect.Y + pad, rect.Width - pad * 2, rect.Height - pad * 2);

            // Shadow
            if (_enableShadow)
            {
                using (var shadowPath = new GraphicsPath())
                {
                    var shadowRect = new Rectangle(arcRect.X - 4, arcRect.Y - 4, arcRect.Width + 8, arcRect.Height + 8);
                    shadowPath.AddEllipse(shadowRect);
                    using (var pgb = new PathGradientBrush(shadowPath))
                    {
                        pgb.CenterColor = Color.FromArgb(40, Color.Black);
                        pgb.SurroundColors = new[] { Color.FromArgb(0, Color.Black) };
                        e.Graphics.FillEllipse(pgb, shadowRect);
                    }
                }
            }

            // Inner circle (background)
            using (var innerPath = new GraphicsPath())
            {
                innerPath.AddEllipse(arcRect);
                using (var innerBrush = new SolidBrush(Color.FromArgb(20, 16, 58))) // Slightly dark to fit logo theme
                {
                    e.Graphics.FillPath(innerBrush, innerPath);
                }
            }

            // Multi-stop gradient arc (logo style)
            using (var lg = new LinearGradientBrush(arcRect, _startColor, _endColor, LinearGradientMode.ForwardDiagonal))
            {
                if (_middleColor != Color.Empty)
                {
                    var blend = new ColorBlend
                    {
                        Colors = new[] { _startColor, _middleColor, _endColor },
                        Positions = new[] { 0f, 0.5f, 1f }
                    };
                    lg.InterpolationColors = blend;
                }

                using (var pen = new Pen(lg, _thickness))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    e.Graphics.DrawArc(pen, arcRect, _angle, _sweep);
                }
            }

            // Inner cover for donut depth
            int innerCover = _thickness + 4;
            var coverRect = new Rectangle(arcRect.X + innerCover, arcRect.Y + innerCover,
                arcRect.Width - innerCover * 2, arcRect.Height - innerCover * 2);
            using (var coverBrush = new SolidBrush(Color.FromArgb(30, 24, 70))) // Harmonized with dark palette
            {
                e.Graphics.FillEllipse(coverBrush, coverRect);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer?.Dispose();
            base.Dispose(disposing);
        }
    }
}