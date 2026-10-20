using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    public class RoundedButton : Control
    {
        private Color _backColor    = Color.FromArgb(59, 130, 246);
        private Color _hoverColor   = Color.FromArgb(37, 99, 235);
        private Color _pressedColor = Color.FromArgb(29, 78, 216);
        private Color _foreColor    = Color.White;
        private Color _borderColor  = Color.Transparent;
        private int   _radius       = 10;
        private bool  _isHovered    = false;
        private bool  _isPressed    = false;

        public DialogResult DialogResult { get; set; } = DialogResult.None;

        public Color NormalColor  { get => _backColor;    set { _backColor    = value; Invalidate(); } }
        public Color HoverColor   { get => _hoverColor;   set { _hoverColor   = value; Invalidate(); } }
        public Color PressedColor { get => _pressedColor; set { _pressedColor = value; Invalidate(); } }
        public Color BorderColor  { get => _borderColor;  set { _borderColor  = value; Invalidate(); } }
        public int   Radius       { get => _radius;       set { _radius       = value; Invalidate(); } }

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Cursor    = Cursors.Hand;
            Size      = new Size(140, 40);
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { _isPressed = true;  Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e)   { _isPressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (DialogResult != DialogResult.None)
            {
                var form = FindForm();
                if (form != null) form.DialogResult = DialogResult;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g  = e.Graphics;
            g.SmoothingMode      = SmoothingMode.AntiAlias;
            g.TextRenderingHint  = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rc   = new Rectangle(1, 1, Width - 2, Height - 2);
            var fill = _isPressed ? _pressedColor : _isHovered ? _hoverColor : _backColor;

            using (var path = RoundedPath(rc, _radius))
            {
                using (var br = new SolidBrush(fill))
                    g.FillPath(br, path);

                if (_borderColor != Color.Transparent)
                    using (var pen = new Pen(_borderColor, 1.5f))
                        g.DrawPath(pen, path);
            }

            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var br = new SolidBrush(ForeColor))
                g.DrawString(Text, Font, br, new RectangleF(8, 0, Width - 16, Height), sf);
        }

        private GraphicsPath RoundedPath(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
