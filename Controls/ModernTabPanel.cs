using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using smart_attendance_middleware;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    [ToolboxItem(true)]
    public class ModernTabPanel : Panel
    {
        private readonly List<string> _tabTitles = new List<string>();
        private Rectangle[] _tabRects = new Rectangle[0];
        private int _activeIndex = -1;
        private string[] _designTabNames = new string[0];

        [Category("Appearance")]
        [Description("Tab names shown in the VS Designer (design-time only).")]
        public string DesignTabNames
        {
            get => string.Join("|", _designTabNames);
            set
            {
                _designTabNames = string.IsNullOrWhiteSpace(value)
                    ? new string[0]
                    : value.Split('|');
                Invalidate();
            }
        }

        private static readonly Color _bg       = Color.FromArgb(245, 246, 250);
        private static readonly Color _navyText = Color.FromArgb(30, 38, 90);
        private static readonly Color _muted    = Color.FromArgb(130, 138, 165);
        private static readonly Color _border   = Color.FromArgb(220, 224, 238);

        public event EventHandler<int> TabChanged;

        public ModernTabPanel()
        {
            BackColor = _bg;
            Height = 44;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        [Browsable(false)]
        public int ActiveTabIndex { get { return _activeIndex; } }

        public void SetTabs(string[] titles)
        {
            _tabTitles.Clear();
            _tabTitles.AddRange(titles);
            _activeIndex = titles.Length > 0 ? 0 : -1;
            Invalidate();
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= _tabTitles.Count) return;
            _activeIndex = index;
            Invalidate();
            if (TabChanged != null) TabChanged(this, index);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            for (int i = 0; i < _tabRects.Length; i++)
            {
                if (_tabRects[i].Contains(e.Location))
                {
                    SelectTab(i);
                    return;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(_bg);

            if (_tabTitles.Count == 0)
            {
                if (DesignMode && _designTabNames.Length > 0)
                {
                    // Render placeholder tabs using DesignTabNames
                    var placeholders = _designTabNames;
                    var rects = new Rectangle[placeholders.Length];
                    int dx = 8;
                    for (int i = 0; i < placeholders.Length; i++)
                    {
                        using (var f = new Font("Segoe UI", i == 0 ? 9.5f : 9f, i == 0 ? FontStyle.Bold : FontStyle.Regular))
                        {
                            int tw = (int)g.MeasureString(placeholders[i], f).Width;
                            int tabW = tw + 24; int tabH = 28; int tabY = (Height - tabH) / 2;
                            rects[i] = new Rectangle(dx, tabY, tabW, tabH);
                            dx += tabW + 4;
                        }
                    }
                    for (int i = 0; i < placeholders.Length; i++)
                    {
                        bool active = i == 0;
                        var font = new Font("Segoe UI", active ? 9.5f : 9f, active ? FontStyle.Bold : FontStyle.Regular);
                        if (active)
                        {
                            using (var path = RoundedRect(rects[i], 6))
                            using (var bg2 = new SolidBrush(Color.White))
                            { g.FillPath(bg2, path); using (var pen = new Pen(_border, 1f)) g.DrawPath(pen, path); }
                        }
                        using (var sf2 = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        using (var br2 = new SolidBrush(active ? _navyText : _muted))
                            g.DrawString(placeholders[i], font, br2, new RectangleF(rects[i].X, rects[i].Y, rects[i].Width, rects[i].Height), sf2);
                    }
                }
                else if (DesignMode)
                {
                    using (var f = new Font("Segoe UI", 9f))
                    using (var br = new SolidBrush(_muted))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("ModernTabPanel", f, br, new RectangleF(0, 0, Width, Height), sf);
                    }
                }
                return;
            }

            _tabRects = new Rectangle[_tabTitles.Count];
                int x = 8;
                for (int i = 0; i < _tabTitles.Count; i++)
                {
                    var font  = (i == _activeIndex) ? new Font("Segoe UI", 9.5f, FontStyle.Bold) : new Font("Segoe UI", 9f, FontStyle.Regular);
                    int textW = (int)g.MeasureString(_tabTitles[i], font).Width;
                    int tabW  = textW + 24;
                    int tabH  = 28;
                    int tabY  = (Height - tabH) / 2;
                    _tabRects[i] = new Rectangle(x, tabY, tabW, tabH);
                    x += tabW + 4;
                }

                for (int i = 0; i < _tabTitles.Count; i++)
                {
                    var rect   = _tabRects[i];
                    bool active = (i == _activeIndex);
                    var font   = active ? new Font("Segoe UI", 9.5f, FontStyle.Bold) : new Font("Segoe UI", 9f, FontStyle.Regular);
                    var fg     = active ? _navyText : _muted;

                    if (active)
                    {
                        using (var path = RoundedRect(rect, 6))
                        using (var bg = new SolidBrush(Color.White))
                        {
                            g.FillPath(bg, path);
                            using (var pen = new Pen(_border, 1f))
                                g.DrawPath(pen, path);
                        }
                    }

                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var br = new SolidBrush(fg))
                        g.DrawString(_tabTitles[i], font, br, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), sf);
                }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int rad)
        {
            var path = new GraphicsPath();
            int d = rad * 2;
            path.AddArc(r.X,         r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

