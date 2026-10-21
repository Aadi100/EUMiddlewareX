using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    [ToolboxItem(true)]
    public class TileCard : Control
    {
        private string _icon        = "🖥";
        private string _title       = "Title";
        private string _description = "Description";
        private string _badge       = "";
        private Color  _badgeColor  = Color.FromArgb(16, 185, 129);
        private bool   _highlighted = false;
        private bool   _isHovered   = false;

        private static readonly Color ClrCard      = Color.White;
        private static readonly Color ClrBorder    = Color.FromArgb(226, 228, 238);
        private static readonly Color ClrBorderSel = Color.FromArgb(37, 99, 234);
        private static readonly Color ClrIconBg    = Color.FromArgb(236, 240, 251);
        private static readonly Color ClrIcon      = Color.FromArgb(30, 38, 90);
        private static readonly Color ClrTitle     = Color.FromArgb(18, 23, 49);
        private static readonly Color ClrDesc      = Color.FromArgb(100, 108, 132);
        private static readonly Color ClrOpen      = Color.FromArgb(37, 99, 234);
        private static readonly Color ClrHoverBg   = Color.FromArgb(248, 249, 255);

        [Category("Appearance")]
        public string Icon { get => _icon; set { _icon = value; Invalidate(); } }

        [Category("Appearance")]
        public string Title { get => _title; set { _title = value; Invalidate(); } }

        [Category("Appearance")]
        public string Description { get => _description; set { _description = value; Invalidate(); } }

        [Category("Appearance")]
        public string Badge { get => _badge; set { _badge = value; Invalidate(); } }

        [Category("Appearance")]
        public Color BadgeColor { get => _badgeColor; set { _badgeColor = value; Invalidate(); } }

        [Category("Appearance")]
        public bool Highlighted { get => _highlighted; set { _highlighted = value; Invalidate(); } }

        public TileCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size      = new Size(256, 175);
            Cursor    = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _isHovered = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g  = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = Width;
            int h = Height;
            const int radius = 12;

            if (DesignMode)
            {
                var cardRect2 = new Rectangle(1, 1, w - 2, h - 2);
                using (var path = RoundedPath(cardRect2, radius))
                {
                    using (var br = new SolidBrush(Color.White)) g.FillPath(br, path);
                    var bc = _highlighted ? ClrBorderSel : ClrBorder;
                    using (var pen = new Pen(bc, _highlighted ? 2f : 1f)) g.DrawPath(pen, path);
                }
                const int pad2 = 16; const int iconSize2 = 52;
                var iconRect2 = new Rectangle(pad2, pad2, iconSize2, iconSize2);
                using (var path = RoundedPath(iconRect2, 10))
                using (var br = new SolidBrush(ClrIconBg)) g.FillPath(br, path);
                using (var iconFont = new Font("Segoe UI Emoji", 18F))
                using (var br = new SolidBrush(ClrIcon))
                { var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }; g.DrawString(_icon, iconFont, br, new RectangleF(iconRect2.X, iconRect2.Y, iconRect2.Width, iconRect2.Height), sf); }
                int ty = pad2 + iconSize2 + 10;
                using (var f = new Font("Segoe UI", 9.5F, FontStyle.Bold)) using (var br = new SolidBrush(ClrTitle)) g.DrawString(_title, f, br, new RectangleF(pad2, ty, w - pad2 * 2, 22));
                using (var f = new Font("Segoe UI", 8F)) using (var br = new SolidBrush(ClrDesc)) g.DrawString(_description, f, br, new RectangleF(pad2, ty + 24, w - pad2 * 2, 32));
                using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold)) using (var br = new SolidBrush(ClrOpen))
                { var sf = new StringFormat { Alignment = StringAlignment.Far }; g.DrawString("Open →", f, br, new RectangleF(pad2, h - 28, w - pad2 * 2, 20), sf); }
                return;
            }

            var cardRect = new Rectangle(1, 1, w - 2, h - 2);

            // Card background + border
            using (var path = RoundedPath(cardRect, radius))
            {
                var fillColor = _isHovered ? ClrHoverBg : ClrCard;
                using (var br = new SolidBrush(fillColor))
                    g.FillPath(br, path);

                var borderColor = _highlighted ? ClrBorderSel : ClrBorder;
                float borderW   = _highlighted ? 2f : 1f;
                using (var pen = new Pen(borderColor, borderW))
                    g.DrawPath(pen, path);
            }

            const int pad      = 16;
            const int iconSize = 52;
            const int iconR    = 10;

            // Icon box background (rounded)
            var iconRect = new Rectangle(pad, pad, iconSize, iconSize);
            using (var path = RoundedPath(iconRect, iconR))
            using (var br = new SolidBrush(ClrIconBg))
                g.FillPath(br, path);

            // Icon emoji
            using (var iconFont = new Font("Segoe UI Emoji", 18F))
            using (var br = new SolidBrush(ClrIcon))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_icon, iconFont, br, new RectangleF(iconRect.X, iconRect.Y, iconRect.Width, iconRect.Height), sf);
            }

            // Badge (top-right pill)
            if (!string.IsNullOrEmpty(_badge))
            {
                using (var badgeFont = new Font("Segoe UI", 7.5F, FontStyle.Bold))
                {
                    var badgeSize = g.MeasureString(_badge, badgeFont);
                    int bw = (int)badgeSize.Width + 14;
                    int bh = 20;
                    int bx = w - bw - 8;
                    int by = 10;
                    var badgeRect = new Rectangle(bx, by, bw, bh);
                    using (var path = RoundedPath(badgeRect, bh / 2))
                    using (var br = new SolidBrush(_badgeColor))
                        g.FillPath(br, path);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    using (var br = new SolidBrush(Color.White))
                        g.DrawString(_badge, badgeFont, br, new RectangleF(bx, by, bw, bh), sf);
                }
            }

            // Title
            int titleY = pad + iconSize + 10;
            using (var titleFont = new Font("Segoe UI", 9.5F, FontStyle.Bold))
            using (var br = new SolidBrush(ClrTitle))
                g.DrawString(_title, titleFont, br, new RectangleF(pad, titleY, w - pad * 2, 22));

            // Description
            int descY = titleY + 24;
            using (var descFont = new Font("Segoe UI", 8F))
            using (var br = new SolidBrush(ClrDesc))
                g.DrawString(_description, descFont, br, new RectangleF(pad, descY, w - pad * 2, 32));

            // "Open →" link (bottom-right)
            using (var openFont = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (var br = new SolidBrush(ClrOpen))
            {
                var sf   = new StringFormat { Alignment = StringAlignment.Far };
                var text = "Open →";
                g.DrawString(text, openFont, br, new RectangleF(pad, h - 28, w - pad * 2, 20), sf);
            }
        }

        private static GraphicsPath RoundedPath(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            int d = rad * 2;
            p.AddArc(r.X,           r.Y,            d, d, 180, 90);
            p.AddArc(r.Right - d,   r.Y,            d, d, 270, 90);
            p.AddArc(r.Right - d,   r.Bottom - d,   d, d,   0, 90);
            p.AddArc(r.X,           r.Bottom - d,   d, d,  90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
