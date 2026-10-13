using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    /// <summary>
    /// Draws a row of coloured capsule pills representing per-device connection state.
    /// Call UpdateDevices(connected, total) to repaint.
    /// </summary>
    public class PillsPanel : Panel
    {
        private List<bool> _pills = new List<bool>();
        private bool _noDev = true;

        private static readonly Color PillGreen = Color.FromArgb(60, 179, 113);
        private static readonly Color PillRed   = Color.FromArgb(220, 53,  69);
        private static readonly Color PillGray  = Color.FromArgb(190, 190, 200);

        public PillsPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer  |
                     ControlStyles.AllPaintingInWmPaint   |
                     ControlStyles.ResizeRedraw            |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(160, 16);
        }

        public void UpdateDevices(int connected, int total)
        {
            _noDev = total == 0;
            _pills.Clear();
            for (int i = 0; i < total; i++)
                _pills.Add(i < connected);

            // Resize to fit pills exactly so labels after us can anchor to our right edge
            const int pw = 28, pg = 4;
            Width = _noDev ? pw : total * pw + (total - 1) * pg;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // Let base handle transparent background painting
            base.OnPaintBackground(new PaintEventArgs(g, ClientRectangle));

            const int pillW = 28, pillH = 14, gap = 4;
            int y = (Height - pillH) / 2;
            int x = 0;

            if (_noDev)
            {
                DrawPill(g, x, y, pillW, pillH, PillGray);
            }
            else
            {
                foreach (bool conn in _pills)
                {
                    DrawPill(g, x, y, pillW, pillH, conn ? PillGreen : PillRed);
                    x += pillW + gap;
                }
            }
        }

        private static void DrawPill(Graphics g, int x, int y, int w, int h, Color color)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(x,         y, h, h, 90, 180);
                path.AddArc(x + w - h, y, h, h, 270, 180);
                path.CloseFigure();
                using (var brush = new SolidBrush(color))
                    g.FillPath(brush, path);
            }
        }
    }
}
