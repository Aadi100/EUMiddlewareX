using System;
using System.ComponentModel;
using System.Drawing;
using smart_attendance_middleware;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    /// <summary>
    /// A generic owner-drawn ListView with a modern clean style.
    /// Drop it onto any form or panel from the Toolbox, add columns
    /// and rows exactly as you would a standard ListView.
    ///
    /// Features (all configurable in the Designer):
    ///   • Custom header — light background, small-caps text, separator line
    ///   • BoldColumnIndex  — renders that column bold (default 0)
    ///   • PillColumnIndex  — renders that column as a rounded pill chip (default -1 = none)
    ///   • Per-area color properties
    /// </summary>
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(ListView))]
    [DefaultProperty("PillColumnIndex")]
    public class ModernListView : ListView
    {
        // ── Properties ────────────────────────────────────────────────────────

        private int _boldColumnIndex   = 0;
        private int _pillColumnIndex   = -1;
        private int _buttonColumnIndex = -1;

        public event EventHandler<int> ButtonColumnItemClicked;

        [Browsable(true), Category("Modern List"), DefaultValue(0),
         Description("Column index whose text is rendered bold.")]
        public int BoldColumnIndex
        {
            get => _boldColumnIndex;
            set { _boldColumnIndex = value; Invalidate(); }
        }

        [Browsable(true), Category("Modern List"), DefaultValue(-1),
         Description("Column index rendered as a pill chip. Set -1 to disable.")]
        public int PillColumnIndex
        {
            get => _pillColumnIndex;
            set { _pillColumnIndex = value; Invalidate(); }
        }

        [Browsable(true), Category("Modern List"), DefaultValue(-1),
         Description("Column index rendered as a clickable View button. Set -1 to disable.")]
        public int ButtonColumnIndex
        {
            get => _buttonColumnIndex;
            set { _buttonColumnIndex = value; Invalidate(); }
        }

        // Colours
        private Color _headerBackColor   = Color.FromArgb(248, 249, 252);
        private Color _headerForeColor   = Color.FromArgb(130, 138, 165);
        private Color _separatorColor    = Color.FromArgb(235, 237, 244);
        private Color _boldForeColor     = Color.FromArgb(30,  38,  90);
        private Color _normalForeColor   = Color.FromArgb(80,  88,  112);
        private Color _pillBackColor     = Color.FromArgb(255, 215, 215);
        private Color _pillForeColor     = Color.FromArgb(185, 40,  40);

        [Browsable(true), Category("Modern List")] public Color HeaderBackColor   { get => _headerBackColor;  set { _headerBackColor  = value; Invalidate(); } }
        [Browsable(true), Category("Modern List")] public Color HeaderForeColor   { get => _headerForeColor;  set { _headerForeColor  = value; Invalidate(); } }
        [Browsable(true), Category("Modern List")] public Color SeparatorColor    { get => _separatorColor;   set { _separatorColor   = value; Invalidate(); } }
        [Browsable(true), Category("Modern List")] public Color BoldForeColor     { get => _boldForeColor;    set { _boldForeColor    = value; Invalidate(); } }
        [Browsable(true), Category("Modern List")] public Color NormalForeColor   { get => _normalForeColor;  set { _normalForeColor  = value; Invalidate(); } }
        [Browsable(true), Category("Modern List")] public Color PillBackColor     { get => _pillBackColor;    set { _pillBackColor    = value; Invalidate(); } }
        [Browsable(true), Category("Modern List")] public Color PillForeColor     { get => _pillForeColor;    set { _pillForeColor    = value; Invalidate(); } }

        // ── Constructor ───────────────────────────────────────────────────────

        public ModernListView()
        {
            OwnerDraw                       = true;
            GridLines                       = false;
            FullRowSelect                   = true;
            MultiSelect                     = false;
            View                            = View.Details;
            HeaderStyle                     = ColumnHeaderStyle.Nonclickable;
            UseCompatibleStateImageBehavior = false;
            BorderStyle                     = BorderStyle.None;
            Font                            = new Font("Segoe UI", 12f, FontStyle.Bold);

            DrawColumnHeader += OnDrawColumnHeader;
            DrawItem         += OnDrawItem;
            DrawSubItem      += OnDrawSubItem;
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var bg = new SolidBrush(_headerBackColor))
                e.Graphics.FillRectangle(bg, e.Bounds);

            using (var pen = new Pen(_separatorColor))
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (var b = new SolidBrush(_headerForeColor))
            {
                var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
                var r  = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 10, e.Bounds.Height);
                e.Graphics.DrawString(e.Header.Text.ToUpper(), new Font("Segoe UI", 8f, FontStyle.Regular), b, r, sf);
            }
        }

        private void OnDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
            using (var bg = new SolidBrush(Color.White))
                e.Graphics.FillRectangle(bg, e.Bounds);
            using (var pen = new Pen(_separatorColor))
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var    g      = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            string text   = e.SubItem.Text;
            var    bounds = e.Bounds;
            var    sf     = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };

            if (e.ColumnIndex == _buttonColumnIndex && !string.IsNullOrEmpty(text) && text != "-")
            {
                DrawViewButton(g, bounds);
            }
            else if (e.ColumnIndex == _pillColumnIndex && !string.IsNullOrEmpty(text))
            {
                DrawPill(g, bounds, text);
            }
            else if (e.ColumnIndex == _boldColumnIndex)
            {
                using (var b = new SolidBrush(_boldForeColor))
                    g.DrawString(text, new Font("Segoe UI", 9f, FontStyle.Bold), b, new Rectangle(bounds.X + 10, bounds.Y, bounds.Width - 10, bounds.Height), sf);
            }
            else
            {
                using (var b = new SolidBrush(_normalForeColor))
                    g.DrawString(text, new Font("Segoe UI", 9f, FontStyle.Regular), b, new Rectangle(bounds.X + 8, bounds.Y, bounds.Width - 8, bounds.Height), sf);
            }
        }

        private void DrawViewButton(Graphics g, Rectangle bounds)
        {
            int btnW = 46, btnH = 22;
            int btnX = bounds.X + 8;
            int btnY = bounds.Y + (bounds.Height - btnH) / 2;
            const int r = 6;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = new GraphicsPath())
            {
                path.AddArc(btnX,              btnY,              r * 2, r * 2, 180, 90);
                path.AddArc(btnX + btnW - r*2, btnY,              r * 2, r * 2, 270, 90);
                path.AddArc(btnX + btnW - r*2, btnY + btnH - r*2, r * 2, r * 2,   0, 90);
                path.AddArc(btnX,              btnY + btnH - r*2, r * 2, r * 2,  90, 90);
                path.CloseFigure();
                using (var bg = new SolidBrush(Color.FromArgb(254, 226, 226)))
                    g.FillPath(bg, path);
            }
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (var tb = new SolidBrush(Color.FromArgb(220, 53, 69)))
            {
                var sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
                g.DrawString("View", new Font("Segoe UI", 8f, FontStyle.Regular), tb, new System.Drawing.RectangleF(btnX, btnY, btnW, btnH), sf);
            }
        }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_buttonColumnIndex < 0) return;
            var info = HitTest(e.X, e.Y);
            if (info?.Item == null || info.SubItem == null) return;
            int colIdx = info.Item.SubItems.IndexOf(info.SubItem);
            if (colIdx != _buttonColumnIndex) return;
            string text = info.SubItem.Text;
            if (string.IsNullOrEmpty(text) || text == "-") return;
            ButtonColumnItemClicked?.Invoke(this, info.Item.Index);
        }

        private void DrawPill(Graphics g, Rectangle bounds, string text)
        {
            {
                var sz    = g.MeasureString(text, new Font("Segoe UI", 8.5f, FontStyle.Regular));
                int pillW = (int)sz.Width + 18;
                int pillH = 22;
                int pillX = bounds.X + 8;
                int pillY = bounds.Y + (bounds.Height - pillH) / 2;
                const int r = 11;

                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var path = new GraphicsPath())
                {
                    path.AddArc(pillX,                 pillY,                 r * 2, r * 2, 180, 90);
                    path.AddArc(pillX + pillW - r * 2, pillY,                 r * 2, r * 2, 270, 90);
                    path.AddArc(pillX + pillW - r * 2, pillY + pillH - r * 2, r * 2, r * 2,   0, 90);
                    path.AddArc(pillX,                 pillY + pillH - r * 2, r * 2, r * 2,  90, 90);
                    path.CloseFigure();

                    using (var bg = new SolidBrush(_pillBackColor))
                        g.FillPath(bg, path);
                }

                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                using (var tb = new SolidBrush(_pillForeColor))
                {
                    var csf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
                    g.DrawString(text, new Font("Segoe UI", 8.5f, FontStyle.Regular), tb, new System.Drawing.RectangleF(pillX, pillY, pillW, pillH), csf);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DrawColumnHeader -= OnDrawColumnHeader;
                DrawItem         -= OnDrawItem;
                DrawSubItem      -= OnDrawSubItem;
            }
            base.Dispose(disposing);
        }
    }
}
