using System;
using System.ComponentModel;
using System.Drawing;
using smart_attendance_middleware;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    [ToolboxItem(true)]
    public class StatCard : Panel
    {
        private string _title = "Title";
        private string _value = "0";
        private Color _valueColor = Color.FromArgb(30, 38, 90);
        private const int Radius = 12;

        public StatCard()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(240, 242, 247);
            Size = new Size(205, 90);
            this.Resize += (s, e) => ApplyRegion();
        }

        [Browsable(true), Category("Stat Card")]
        public string Title { get => _title; set { _title = value; Invalidate(); } }

        [Browsable(true), Category("Stat Card")]
        public string Value { get => _value; set { _value = value; Invalidate(); } }

        [Browsable(true), Category("Stat Card")]
        public Color ValueColor { get => _valueColor; set { _valueColor = value; Invalidate(); } }

        public void SetValue(string text) { _value = text; Invalidate(); }
        public void SetValue(int n) { SetValue(n.ToString()); }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ApplyRegion(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            using (var tb = new SolidBrush(Color.FromArgb(100, 108, 132)))
                g.DrawString(_title, new Font("Segoe UI", 8.5f, FontStyle.Regular), tb, 12f, 10f);

            using (var vb = new SolidBrush(_valueColor))
                g.DrawString(_value, new Font("Segoe UI", 22f, FontStyle.Bold), vb, 8f, 28f);
        }

        private void ApplyRegion()
        {
            if (Width > 0 && Height > 0)
            {
                var path = new GraphicsPath();
                var b = new Rectangle(0, 0, Width, Height);
                path.AddArc(b.Left, b.Top, Radius * 2, Radius * 2, 180, 90);
                path.AddArc(b.Right - Radius * 2, b.Top, Radius * 2, Radius * 2, 270, 90);
                path.AddArc(b.Right - Radius * 2, b.Bottom - Radius * 2, Radius * 2, Radius * 2, 0, 90);
                path.AddArc(b.Left, b.Bottom - Radius * 2, Radius * 2, Radius * 2, 90, 90);
                path.CloseFigure();
                this.Region = new Region(path);
            }
        }
    }
}
