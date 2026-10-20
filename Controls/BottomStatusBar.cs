using System;
using System.Collections.Generic;
using System.Drawing;
using smart_attendance_middleware;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    /// <summary>
    /// Full-width bottom status bar.
    /// Left  — "Device connection" + per-device coloured pill + status text.
    /// Right — App memory | SQL Server | Uptime | Signed in | Role
    /// </summary>
    public class BottomStatusBar : Panel
    {
        // ── State ─────────────────────────────────────────────────────────────
        private List<bool> _pills    = new List<bool>(); // true=connected, false=offline
        private bool       _noDev   = true;

        private int    _appMemMb  = 0;
        private int?   _sqlMemMb  = null;
        private TimeSpan _uptime  = TimeSpan.Zero;
        private string _userName  = "";
        private string _role      = "";

        // ── Colours ───────────────────────────────────────────────────────────
        private static readonly Color BgColor      = Color.FromArgb(240, 241, 246);
        private static readonly Color TextGray      = Color.FromArgb(100, 108, 132);
        private static readonly Color TextDark      = Color.FromArgb(30,  38,  90);
        private static readonly Color PillGreen     = Color.FromArgb(60,  179, 113);
        private static readonly Color PillRed       = Color.FromArgb(220,  53,  69);
        private static readonly Color PillGray      = Color.FromArgb(190, 190, 200);
        private static readonly Color GreenText     = Color.FromArgb(34,  139,  34);
        private static readonly Color OrangeText    = Color.FromArgb(200, 100,  0);
        private static readonly Color SepColor      = Color.FromArgb(190, 190, 205);

        // ── Constructor ───────────────────────────────────────────────────────
        public BottomStatusBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint  |
                     ControlStyles.ResizeRedraw, true);
            BackColor = BgColor;
            Height    = 28;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void UpdateDevices(int connected, int total)
        {
            _noDev = total == 0;
            _pills.Clear();
            for (int i = 0; i < total; i++)
                _pills.Add(i < connected);
            Invalidate();
        }

        public void UpdateDevices(List<bool> perDeviceConnected)
        {
            _noDev = perDeviceConnected == null || perDeviceConnected.Count == 0;
            _pills = perDeviceConnected ?? new List<bool>();
            Invalidate();
        }

        public void UpdateStats(int appMemMb, TimeSpan uptime, string userName, string role)
        {
            _appMemMb = appMemMb;
            _uptime   = uptime;
            _userName = userName ?? "";
            _role     = role ?? "";
            Invalidate();
        }

        public void UpdateSqlMemory(int? sqlMemMb)
        {
            _sqlMemMb = sqlMemMb;
            Invalidate();
        }

        // ── Paint ─────────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Background
            g.Clear(BgColor);

            // Top border line
            using (var pen = new Pen(Color.FromArgb(210, 212, 225)))
                g.DrawLine(pen, 0, 0, Width, 0);

            int cy = Height / 2; // vertical center

            // ── LEFT: Device connection ────────────────────────────────────
            int lx = 12;

            using (var labelBrush = new SolidBrush(TextGray))
            {
                string hdr = "Device connection";
                var sz = g.MeasureString(hdr, new Font("Segoe UI", 8.5f, FontStyle.Regular));
                g.DrawString(hdr, new Font("Segoe UI", 8.5f, FontStyle.Regular), labelBrush, lx, cy - sz.Height / 2);
                lx += (int)sz.Width + 8;
            }

            // Pills
            const int pillW = 28, pillH = 14, pillGap = 4;
            int pillTop = cy - pillH / 2;

            if (_noDev)
            {
                DrawPill(g, lx, pillTop, pillW, pillH, PillGray);
                lx += pillW + 8;
            }
            else
            {
                foreach (bool conn in _pills)
                {
                    DrawPill(g, lx, pillTop, pillW, pillH, conn ? PillGreen : PillRed);
                    lx += pillW + pillGap;
                }
                lx += 6;
            }

            // Status text
            int offline   = 0;
            int connected = 0;
            foreach (bool c in _pills) { if (c) connected++; else offline++; }

            string statusText;
            Color  statusColor;
            if (_noDev)
            {
                statusText  = "No devices registered";
                statusColor = TextGray;
            }
            else if (offline == 0)
            {
                statusText  = "All connected";
                statusColor = GreenText;
            }
            else if (connected == 0)
            {
                statusText  = "All offline";
                statusColor = PillRed;
            }
            else
            {
                statusText  = $"{offline} offline";
                statusColor = OrangeText;
            }

            using (var sb = new SolidBrush(statusColor))
            {
                var sz = g.MeasureString(statusText, new Font("Segoe UI", 8.5f, FontStyle.Bold));
                g.DrawString(statusText, new Font("Segoe UI", 8.5f, FontStyle.Bold), sb, lx, cy - sz.Height / 2);
            }

            // ── RIGHT: Stats ───────────────────────────────────────────────
            var stats = BuildStats();
            int rx = Width - 10;

            for (int i = stats.Count - 1; i >= 0; i--)
            {
                // Separator "|"
                if (i < stats.Count - 1)
                {
                    using (var sb = new SolidBrush(SepColor))
                    {
                        var sz = g.MeasureString("|", new Font("Segoe UI", 8f, FontStyle.Regular));
                        rx -= (int)sz.Width;
                        g.DrawString("|", new Font("Segoe UI", 8f, FontStyle.Regular), sb, rx, cy - sz.Height / 2);
                        rx -= 6;
                    }
                }

                DrawStat(g, ref rx, cy, stats[i].label, stats[i].value);
                rx -= 10;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void DrawPill(Graphics g, int x, int y, int w, int h, Color color)
        {
            using (var path = new GraphicsPath())
            {
                int r = h / 2;
                path.AddArc(x,         y, h, h, 90, 180);
                path.AddArc(x + w - h, y, h, h, 270, 180);
                path.CloseFigure();
                using (var brush = new SolidBrush(color))
                    g.FillPath(brush, path);
            }
        }

        private void DrawStat(Graphics g, ref int rightX, int cy, string label, string value)
        {
            // Draw value (bold) then label to the left
            using (var darkBrush = new SolidBrush(TextDark))
            using (var grayBrush = new SolidBrush(TextGray))
            {
                var valSz = g.MeasureString(value, new Font("Segoe UI", 8.5f, FontStyle.Bold));
                rightX -= (int)valSz.Width;
                g.DrawString(value, new Font("Segoe UI", 8.5f, FontStyle.Bold), darkBrush, rightX, cy - valSz.Height / 2);

                rightX -= 3;
                var lblSz = g.MeasureString(label, new Font("Segoe UI", 8.5f, FontStyle.Regular));
                rightX -= (int)lblSz.Width;
                g.DrawString(label, new Font("Segoe UI", 8.5f, FontStyle.Regular), grayBrush, rightX, cy - lblSz.Height / 2);
            }
        }

        private List<(string label, string value)> BuildStats()
        {
            var list = new List<(string, string)>();

            if (!string.IsNullOrEmpty(_role))
                list.Add(("Role: ", _role));

            if (!string.IsNullOrEmpty(_userName))
                list.Add(("Signed in: ", _userName));

            string uptimeStr = _uptime.TotalHours >= 1
                ? $"{(int)_uptime.TotalHours}h {_uptime.Minutes}m"
                : $"{_uptime.Minutes}m {_uptime.Seconds}s";
            list.Add(("Uptime ", uptimeStr));

            list.Add(("SQL Server ", _sqlMemMb.HasValue ? $"{_sqlMemMb.Value} MB" : "N/A"));
            list.Add(("App memory ", $"{_appMemMb} MB"));

            return list;
        }
    }
}
