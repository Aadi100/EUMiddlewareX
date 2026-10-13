using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    /// <summary>
    /// A dark rounded status card for the Dashboard header bar.
    /// Shows a coloured dot (optional), a small top label, and bold bottom text.
    /// Drag from the toolbox onto the header panel in the designer.
    /// Update at runtime via SetStatus() or the individual properties.
    /// </summary>
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(Panel))]
    [DefaultProperty("TopLabel")]
    public class HeaderStatusCard : Panel
    {
        // ── Backing fields ────────────────────────────────────────────────────
        private string _topLabel   = "Internet";
        private string _bottomText = "Connecting";
        private bool   _connected  = false;
        private bool   _showDot    = true;
        private Color  _cardColor  = Color.FromArgb(22, 26, 65);
        private const  int Radius  = 10;

        // ── Constructor ───────────────────────────────────────────────────────
        public HeaderStatusCard()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.BackColor   = _cardColor;
            this.BorderStyle = BorderStyle.None;
            this.Size        = new Size(130, 52);
            this.Resize     += (s, e) => ApplyRegion();
        }

        // ── Design-time properties ────────────────────────────────────────────

        [Browsable(true), Category("Status Card"), Description("Small label shown above the main text (e.g. Internet, Cloud, Last sync).")]
        public string TopLabel
        {
            get => _topLabel;
            set { _topLabel = value; Invalidate(); }
        }

        [Browsable(true), Category("Status Card"), Description("Main bold text shown below the top label.")]
        public string BottomText
        {
            get => _bottomText;
            set { _bottomText = value; Invalidate(); }
        }

        [Browsable(true), Category("Status Card"), Description("When true the dot is green; when false it is red.")]
        public bool IsConnected
        {
            get => _connected;
            set { _connected = value; Invalidate(); }
        }

        [Browsable(true), Category("Status Card"), Description("Show the coloured status dot. Disable for cards like 'Last sync'.")]
        public bool ShowDot
        {
            get => _showDot;
            set { _showDot = value; Invalidate(); }
        }

        [Browsable(true), Category("Status Card"), Description("Background colour of the card.")]
        public Color CardColor
        {
            get => _cardColor;
            set { _cardColor = value; ApplyRegion(); Invalidate(); }
        }

        // ── Runtime API ───────────────────────────────────────────────────────

        /// <summary>Update status and redraw in one call.</summary>
        public void SetStatus(bool connected, string bottomText = null)
        {
            _connected  = connected;
            if (bottomText != null) _bottomText = bottomText;
            Invalidate();
        }

        /// <summary>Update just the bottom text (e.g. for Last sync time).</summary>
        public void SetText(string bottomText)
        {
            _bottomText = bottomText;
            Invalidate();
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Fill — Region already clips to rounded shape
            g.Clear(_cardColor);

            int lp = 12;
            int cx = lp;

            // Coloured dot
            if (_showDot)
            {
                Color dc = _connected ? Color.FromArgb(72, 199, 116) : Color.FromArgb(220, 53, 69);
                g.FillEllipse(new SolidBrush(dc), cx, Height / 2 - 4, 8, 8);
                cx += 14;
            }

            // Top small label (skipped when empty)
            if (!string.IsNullOrEmpty(_topLabel))
            {
                using (var f = new Font("Segoe UI", 7.5f))
                using (var b = new SolidBrush(Color.FromArgb(140, 155, 195)))
                    g.DrawString(_topLabel, f, b, cx, 11);
            }

            // Bottom bold text — centred vertically when there is no top label
            using (var f = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                float textY = string.IsNullOrEmpty(_topLabel)
                    ? (Height - f.GetHeight(g)) / 2f
                    : 28f;
                g.DrawString(_bottomText, f, b, cx, textY);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ApplyRegion()
        {
            if (Width > 0 && Height > 0)
                this.Region = new Region(RoundedRect(new Rectangle(0, 0, Width, Height), Radius));
        }

        private static GraphicsPath RoundedRect(Rectangle b, int r)
        {
            var p = new GraphicsPath();
            p.AddArc(b.Left,           b.Top,            r * 2, r * 2, 180, 90);
            p.AddArc(b.Right - r * 2,  b.Top,            r * 2, r * 2, 270, 90);
            p.AddArc(b.Right - r * 2,  b.Bottom - r * 2, r * 2, r * 2,   0, 90);
            p.AddArc(b.Left,           b.Bottom - r * 2, r * 2, r * 2,  90, 90);
            p.CloseFigure();
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
