using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    [ToolboxItem(true)]
    public class ToggleSwitch : Control
    {
        private bool _checked = false;
        private Color _onColor  = Color.FromArgb(59, 130, 246);
        private Color _offColor = Color.FromArgb(200, 204, 216);

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer, true);

            Size   = new Size(44, 24);
            Cursor = Cursors.Hand;
        }

        public event EventHandler CheckedChanged;

        [DefaultValue(false)]
        [Category("Behavior")]
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
            }
        }

        [Category("Appearance")]
        public Color OnColor
        {
            get { return _onColor; }
            set { _onColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color OffColor
        {
            get { return _offColor; }
            set { _offColor = value; Invalidate(); }
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int w = Width;
            int h = Height;
            int r = h / 2;

            bool checkedState = DesignMode ? false : _checked;

            // Track
            var trackRect = new Rectangle(0, 0, w - 1, h - 1);
            using (var path = RoundedRect(trackRect, r))
            using (var brush = new SolidBrush(checkedState ? _onColor : _offColor))
                e.Graphics.FillPath(brush, path);

            // Knob
            int knobSize = h - 6;
            int knobX = checkedState ? w - knobSize - 4 : 4;
            int knobY = (h - knobSize) / 2;

            var knobRect = new Rectangle(knobX, knobY, knobSize, knobSize);
            using (var brush = new SolidBrush(Color.White))
                e.Graphics.FillEllipse(brush, knobRect);

            // Subtle knob shadow
            using (var pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1f))
                e.Graphics.DrawEllipse(pen, knobRect);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int rad)
        {
            var path = new GraphicsPath();
            int d = rad * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
