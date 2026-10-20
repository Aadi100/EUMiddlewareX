using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    [ToolboxItem(true)]
    public class CardListView : Panel
    {
        // ── Double-buffered panel ─────────────────────────────────────────────
        private sealed class OwnDrawPanel : Panel
        {
            public OwnDrawPanel() { DoubleBuffered = true; ResizeRedraw = true; }
        }

        private Panel _header;
        private Panel _scroll;
        private Panel _paginationBar;
        private Label _lblShowing;
        private ComboBox _cmbPageSize;
        private FlowLayoutPanel _paginationButtons;

        private string[] _colTitles = new string[0];
        private int[] _colX = new int[0];

        private readonly List<Panel> _allRows = new List<Panel>();
        private int _pageSize = 20;
        private int _currentPage = 1;

        private Func<int, Panel> _rowFactory;
        private int _virtualTotal;

        private int _lastBuiltTotalPages  = -1;
        private int _lastBuiltCurrentPage = -1;

        private static readonly Color _accent   = Color.FromArgb(59, 130, 246);
        private static readonly Color _muted    = Color.FromArgb(130, 138, 165);
        private static readonly Color _navyText = Color.FromArgb(30, 38, 90);
        private static readonly Color _bg       = Color.FromArgb(245, 246, 250);

        // Shared static brush — white used by avatars and button text
        private static readonly SolidBrush _whiteBrush = new SolidBrush(Color.White);

        // shared fonts
        public  static readonly Font F9Bold  = new Font("Segoe UI", 9f,   FontStyle.Bold);
        public  static readonly Font F95Bold = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        public  static readonly Font F8      = new Font("Segoe UI", 8f,   FontStyle.Regular);
        public  static readonly Font F85     = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public  static readonly Font F9      = new Font("Segoe UI", 9f,   FontStyle.Regular);
        private static readonly Font _fontNav       = new Font("Segoe UI", 9f,   FontStyle.Regular);
        private static readonly Font _fontNavBold   = new Font("Segoe UI", 9f,   FontStyle.Bold);
        private static readonly Font _fontShowing   = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        private static readonly Font _fontAvatar    = new Font("Segoe UI", 12f,  FontStyle.Bold);
        private static readonly Font _fontPill      = new Font("Segoe UI", 8f,   FontStyle.Regular);
        private static readonly Font _fontActionBtn = new Font("Segoe UI", 8f,   FontStyle.Regular);
        private static readonly Font _fontColHeader = new Font("Segoe UI", 8.5f, FontStyle.Regular);

        public CardListView()
        {
            BackColor = _bg;
            BorderStyle = BorderStyle.None;

            _header = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = _bg };
            _header.Paint += Header_Paint;

            _BuildPaginationBar();

            _scroll = new OwnDrawPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = _bg,
                Padding = new Padding(0, 4, 0, 4),
            };
            _scroll.Resize += (s, e) => _RefreshPage();

            Controls.Add(_scroll);
            Controls.Add(_paginationBar);
            Controls.Add(_header);
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void SetColumns(string[] titles, int[] xPositions)
        {
            _colTitles = titles ?? new string[0];
            _colX      = xPositions ?? new int[0];
            _header.Invalidate();
        }

        public void AddRow(Panel row)
        {
            _allRows.Add(row);
            _RefreshPage();
        }

        public void AddRows(IEnumerable<Panel> rows)
        {
            foreach (var row in rows) _allRows.Add(row);
            _RefreshPage();
        }

        public void SetRowFactory(int totalCount, Func<int, Panel> factory)
        {
            _rowFactory   = factory;
            _virtualTotal = totalCount;
            _allRows.Clear();
            for (int i = 0; i < totalCount; i++) _allRows.Add(null);
            _currentPage = 1;
            _lastBuiltTotalPages  = -1;
            _lastBuiltCurrentPage = -1;
            _RefreshPage();
        }

        public void ClearRows()
        {
            _rowFactory   = null;
            _virtualTotal = 0;
            _allRows.Clear();
            _scroll.Controls.Clear();
            _currentPage = 1;
            _lastBuiltTotalPages  = -1;
            _lastBuiltCurrentPage = -1;
            _RefreshPage();
        }

        public int RowCount => _allRows.Count;

        private bool _showPagination = true;

        [DefaultValue(true)]
        [Category("Behavior")]
        public bool ShowPagination
        {
            get => _showPagination;
            set
            {
                _showPagination = value;
                if (!value) _paginationBar.Visible = false;
                else _RefreshPage();
            }
        }

        // ── Pagination logic ─────────────────────────────────────────────────

        private int _TotalPages()
        {
            if (_pageSize <= 0 || _allRows.Count == 0) return 1;
            return (int)Math.Ceiling((double)_allRows.Count / _pageSize);
        }

        private void _RefreshPage()
        {
            int total = _allRows.Count;
            if (total == 0)
            {
                _scroll.Controls.Clear();
                _scroll.AutoScrollPosition = new Point(0, 0);
                _UpdateShowingLabel(0, 0, 0);
                _RebuildPageButtons();
                return;
            }

            int totalPages = _TotalPages();
            if (_currentPage > totalPages) _currentPage = totalPages;
            if (_currentPage < 1)          _currentPage = 1;

            int start = (_currentPage - 1) * _pageSize;
            int end   = Math.Min(start + _pageSize, total) - 1;

            var pageRows = new List<Panel>(end - start + 1);
            for (int i = start; i <= end; i++)
            {
                Panel row = _allRows[i];
                if (row == null && _rowFactory != null)
                {
                    row = _rowFactory(i);
                    row?.ResumeLayout(false);
                    _allRows[i] = row;
                }
                if (row != null) pageRows.Add(row);
            }

            _scroll.SuspendLayout();
            _scroll.Controls.Clear();
            _scroll.AutoScrollPosition = new Point(0, 0);

            int y           = 2;
            int scrollWidth = Math.Max(100, _scroll.ClientSize.Width - 16);
            foreach (var row in pageRows)
            {
                row.Top    = y;
                row.Left   = 8;
                row.Width  = scrollWidth;
                row.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                _scroll.Controls.Add(row);
                y += row.Height + 6;
            }

            _scroll.ResumeLayout(false);
            _UpdateShowingLabel(start + 1, end + 1, total);
            _RebuildPageButtons();
            _paginationBar.Visible = _showPagination && total > 0;
        }

        private void _UpdateShowingLabel(int first, int last, int total)
        {
            _lblShowing.Text = total == 0
                ? "No records"
                : $"Showing {first}–{last} of {total}";
        }

        // ── Pagination bar ────────────────────────────────────────────────────

        private Label _lblPerPage;

        private void _BuildPaginationBar()
        {
            _paginationBar = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 48,
                BackColor = Color.FromArgb(245, 246, 250),
                Visible   = false,
            };
            _paginationBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 224, 238), 1f))
                    e.Graphics.DrawLine(pen, 0, 0, _paginationBar.Width, 0);
            };

            _lblShowing = new Label
            {
                AutoSize  = false, Width = 220, Height = 48,
                Location  = new Point(12, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = _fontShowing, ForeColor = _muted, BackColor = Color.Transparent, Text = "",
            };

            _cmbPageSize = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = _fontShowing, Width = 60,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
            };
            _cmbPageSize.Items.AddRange(new object[] { "10", "20", "50", "100" });
            _cmbPageSize.SelectedItem = _pageSize.ToString();
            _cmbPageSize.SelectedIndexChanged += (s, e) =>
            {
                if (int.TryParse(_cmbPageSize.SelectedItem as string, out int v))
                {
                    _pageSize = v; _currentPage = 1;
                    _lastBuiltTotalPages = -1; _lastBuiltCurrentPage = -1;
                    _RefreshPage();
                }
            };

            _lblPerPage = new Label
            {
                AutoSize  = false, Width = 55, Height = 48,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = _fontShowing, ForeColor = _muted, BackColor = Color.Transparent,
                Text      = "Per page", Anchor = AnchorStyles.Right | AnchorStyles.Top,
            };

            _paginationButtons = new FlowLayoutPanel
            {
                AutoSize = true, Height = 48,
                Anchor   = AnchorStyles.Right | AnchorStyles.Top,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
            };

            _paginationBar.Controls.Add(_lblShowing);
            _paginationBar.Controls.Add(_paginationButtons);
            _paginationBar.Controls.Add(_lblPerPage);
            _paginationBar.Controls.Add(_cmbPageSize);
            _paginationBar.Resize += (s, e) => _LayoutPaginationBar();
        }

        private void _LayoutPaginationBar()
        {
            int barH = _paginationBar.Height, barW = _paginationBar.Width;
            _paginationButtons.Location = new Point(barW - _paginationButtons.Width - 12, (barH - 30) / 2);
            _lblPerPage.Location        = new Point(_paginationButtons.Left - _lblPerPage.Width - 4, 0);
            _cmbPageSize.Location       = new Point(_lblPerPage.Left - _cmbPageSize.Width - 8, (barH - _cmbPageSize.Height) / 2);
        }

        private void _RebuildPageButtons()
        {
            int totalPages = _TotalPages();
            if (totalPages == _lastBuiltTotalPages && _currentPage == _lastBuiltCurrentPage)
            {
                _LayoutPaginationBar();
                return;
            }
            _lastBuiltTotalPages  = totalPages;
            _lastBuiltCurrentPage = _currentPage;

            foreach (Control c in _paginationButtons.Controls) c.Dispose();
            _paginationButtons.Controls.Clear();

            if (totalPages <= 1) { _LayoutPaginationBar(); return; }

            _paginationButtons.Controls.Add(_NavBtn("«", () => { _currentPage = 1; _RefreshPage(); }, false));
            _paginationButtons.Controls.Add(_NavBtn("‹", () => { if (_currentPage > 1) { _currentPage--; _RefreshPage(); } }, false));

            int ws = Math.Max(1, _currentPage - 2);
            int we = Math.Min(totalPages, ws + 4);
            ws = Math.Max(1, we - 4);

            for (int p = ws; p <= we; p++)
            {
                int pg = p; bool active = (p == _currentPage);
                _paginationButtons.Controls.Add(_NavBtn(p.ToString(), () => { _currentPage = pg; _RefreshPage(); }, active));
            }

            _paginationButtons.Controls.Add(_NavBtn("›", () => { if (_currentPage < totalPages) { _currentPage++; _RefreshPage(); } }, false));
            _paginationButtons.Controls.Add(_NavBtn("»", () => { _currentPage = totalPages; _RefreshPage(); }, false));
            _paginationButtons.Width = _paginationButtons.PreferredSize.Width;
            _LayoutPaginationBar();
        }

        private Label _NavBtn(string text, Action onClick, bool active)
        {
            Color bg = active ? _accent : Color.White;
            Color fg = active ? Color.White : _navyText;
            Color border = active ? _accent : Color.FromArgb(220, 224, 238);

            var lbl = new Label
            {
                Text = text, Width = 32, Height = 32,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = active ? _fontNavBold : _fontNav,
                ForeColor = fg, BackColor = Color.Transparent,
                Cursor = Cursors.Hand, Margin = new Padding(2, 0, 2, 0),
            };
            lbl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1);
                using (var path = RoundedRect(rect, 8))
                using (var bgBrush = new SolidBrush(bg))
                {
                    e.Graphics.FillPath(bgBrush, path);
                    using (var pen = new Pen(border, 1f)) e.Graphics.DrawPath(pen, path);
                }
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var tbr = new SolidBrush(fg))
                    e.Graphics.DrawString(lbl.Text, lbl.Font, tbr, new RectangleF(0, 0, lbl.Width, lbl.Height), sf);
            };
            lbl.Click += (s, e) => onClick();
            return lbl;
        }

        private void Header_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(_header.BackColor);
            using (var br = new SolidBrush(_muted))
            {
                for (int i = 0; i < _colTitles.Length && i < _colX.Length; i++)
                {
                    using (var sf = new StringFormat { LineAlignment = StringAlignment.Center })
                        e.Graphics.DrawString(_colTitles[i], _fontColHeader, br,
                            new RectangleF(_colX[i] + 8, 0, 160, _header.Height), sf);
                }
            }
        }

        // ── Legacy helpers ───────────────────────────────────────────────────

        public static Panel MakeCard(int width, int height)
        {
            var card = new OwnDrawPanel { Width = width, Height = height, BackColor = Color.White };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 224, 238), 1f))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            card.MouseEnter += (s, e) => { card.BackColor = Color.FromArgb(248, 249, 255); card.Invalidate(); };
            card.MouseLeave += (s, e) => { card.BackColor = Color.White; card.Invalidate(); };
            card.SuspendLayout();
            return card;
        }

        public static Label MakeAvatarLabel(string initial, Color bg, int x, int y, int size = 36)
        {
            var lbl = new Label
            {
                Text = string.IsNullOrEmpty(initial) ? "?" : initial.Substring(0, 1).ToUpperInvariant(),
                Width = size, Height = size, Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = _fontAvatar, ForeColor = Color.White, BackColor = Color.Transparent,
            };
            lbl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(bg))
                    e.Graphics.FillEllipse(br, 0, 0, size - 1, size - 1);
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(lbl.Text, lbl.Font, _whiteBrush, new RectangleF(0, 0, size, size), sf);
            };
            return lbl;
        }

        public static Label MakePill(string text, Color bg, Color fg, int x, int y, int w = 70, int h = 22)
        {
            var lbl = new Label
            {
                Text = text, Width = w, Height = h, Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = _fontPill, ForeColor = fg, BackColor = Color.Transparent,
            };
            lbl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var br = new SolidBrush(bg))
                using (var path = RoundedRect(new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1), 10))
                    e.Graphics.FillPath(br, path);
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var tbr = new SolidBrush(fg))
                    e.Graphics.DrawString(lbl.Text, lbl.Font, tbr, new RectangleF(0, 0, lbl.Width, lbl.Height), sf);
            };
            return lbl;
        }

        public static Button MakeActionButton(string text, Color bg, Color fg, int x, int y, int w = 72, int h = 26)
        {
            var btn = new Button
            {
                Text = text, Width = w, Height = h, Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat, Font = _fontActionBtn,
                ForeColor = fg, BackColor = bg, Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        // ── CardBuilder ───────────────────────────────────────────────────────

        public sealed class CardBuilder
        {
            private struct TxtSpec  { public string Text; public RectangleF Bounds; public Font Font; public Color Color; public bool Ellipsis; public ContentAlignment Align; }
            private struct AvSpec   { public string Initial; public Color Bg; public Rectangle Bounds; }
            private struct PillSpec { public string Text; public Color Bg; public Color Fg; public Rectangle Bounds; }
            private struct BtnSpec  { public string Text; public Color Bg; public Color Fg; public Rectangle Bounds; public Action OnClick; }

            private readonly List<TxtSpec>  _txts  = new List<TxtSpec>();
            private readonly List<AvSpec>   _avs   = new List<AvSpec>();
            private readonly List<PillSpec> _pills = new List<PillSpec>();
            private readonly List<BtnSpec>  _btns  = new List<BtnSpec>();
            private Action _cardClick;

            public CardBuilder Text(string text, int x, int y, int w, int h, Font font, Color color,
                bool ellipsis = false, ContentAlignment align = ContentAlignment.TopLeft)
            {
                _txts.Add(new TxtSpec { Text = text ?? "", Bounds = new RectangleF(x, y, w, h), Font = font, Color = color, Ellipsis = ellipsis, Align = align });
                return this;
            }

            public CardBuilder Avatar(string initial, Color bg, int x, int y, int size = 36)
            {
                string ini = string.IsNullOrWhiteSpace(initial) ? "?" : initial.Substring(0, 1).ToUpperInvariant();
                _avs.Add(new AvSpec { Initial = ini, Bg = bg, Bounds = new Rectangle(x, y, size, size) });
                return this;
            }

            public CardBuilder Pill(string text, Color bg, Color fg, int x, int y, int w = 70, int h = 22)
            {
                if (text != null)
                    _pills.Add(new PillSpec { Text = text, Bg = bg, Fg = fg, Bounds = new Rectangle(x, y, w, h) });
                return this;
            }

            public CardBuilder Button(string text, Color bg, Color fg, int x, int y, int w = 72, int h = 26, Action onClick = null)
            {
                if (text != null)
                    _btns.Add(new BtnSpec { Text = text, Bg = bg, Fg = fg, Bounds = new Rectangle(x, y, w, h), OnClick = onClick });
                return this;
            }

            public CardBuilder CardClick(Action onClick) { _cardClick = onClick; return this; }

            public Panel Build(int width, int height)
            {
                var panel = new OwnDrawPanel { Width = width, Height = height, BackColor = Color.White };

                // Pre-create all GDI resources once — reused on every Paint
                var borderPen   = new Pen(Color.FromArgb(220, 224, 238), 1f);
                var txtBrushes  = new SolidBrush[_txts.Count];
                var avBrushes   = new SolidBrush[_avs.Count];
                var pillBgBr    = new SolidBrush[_pills.Count];
                var pillFgBr    = new SolidBrush[_pills.Count];
                var pillPaths   = new GraphicsPath[_pills.Count];
                var btnNormalBr = new SolidBrush[_btns.Count];
                var btnHoverBr  = new SolidBrush[_btns.Count];
                var btnFgBr     = new SolidBrush[_btns.Count];
                var btnPaths    = new GraphicsPath[_btns.Count];
                var sfCenter    = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                for (int i = 0; i < _txts.Count;  i++) txtBrushes[i] = new SolidBrush(_txts[i].Color);
                for (int i = 0; i < _avs.Count;   i++) avBrushes[i]  = new SolidBrush(_avs[i].Bg);
                for (int i = 0; i < _pills.Count;  i++)
                {
                    pillBgBr[i]  = new SolidBrush(_pills[i].Bg);
                    pillFgBr[i]  = new SolidBrush(_pills[i].Fg);
                    pillPaths[i] = RoundedRect(_pills[i].Bounds, 10);
                }
                for (int i = 0; i < _btns.Count; i++)
                {
                    btnNormalBr[i] = new SolidBrush(_btns[i].Bg);
                    var hc = _btns[i].Bg;
                    btnHoverBr[i] = new SolidBrush(Color.FromArgb(hc.A,
                        Math.Max(0, hc.R - 20), Math.Max(0, hc.G - 20), Math.Max(0, hc.B - 20)));
                    btnFgBr[i]   = new SolidBrush(_btns[i].Fg);
                    btnPaths[i]  = RoundedRect(_btns[i].Bounds, 6);
                }

                var txtSf = new StringFormat[_txts.Count];
                for (int i = 0; i < _txts.Count; i++)
                {
                    var sf = new StringFormat();
                    if (_txts[i].Ellipsis) sf.Trimming = StringTrimming.EllipsisCharacter;
                    sf.FormatFlags |= StringFormatFlags.NoWrap;
                    switch (_txts[i].Align)
                    {
                        case ContentAlignment.MiddleLeft:   sf.LineAlignment = StringAlignment.Center; sf.Alignment = StringAlignment.Near;   break;
                        case ContentAlignment.MiddleCenter: sf.LineAlignment = StringAlignment.Center; sf.Alignment = StringAlignment.Center; break;
                        case ContentAlignment.MiddleRight:  sf.LineAlignment = StringAlignment.Center; sf.Alignment = StringAlignment.Far;    break;
                        case ContentAlignment.TopCenter:    sf.LineAlignment = StringAlignment.Near;   sf.Alignment = StringAlignment.Center; break;
                        default:                            sf.LineAlignment = StringAlignment.Near;   sf.Alignment = StringAlignment.Near;   break;
                    }
                    txtSf[i] = sf;
                }

                var txts  = _txts.ToArray();
                var avs   = _avs.ToArray();
                var pills = _pills.ToArray();
                var btns  = _btns.ToArray();
                int hoveredBtn = -1;

                panel.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode     = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    g.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);

                    for (int i = 0; i < avs.Length; i++)
                    {
                        int sz = avs[i].Bounds.Width;
                        g.FillEllipse(avBrushes[i], avs[i].Bounds.X, avs[i].Bounds.Y, sz - 1, sz - 1);
                        g.DrawString(avs[i].Initial, _fontAvatar, _whiteBrush,
                            new RectangleF(avs[i].Bounds.X, avs[i].Bounds.Y, sz, sz), sfCenter);
                    }

                    for (int i = 0; i < txts.Length; i++)
                    {
                        if (txts[i].Text.Length == 0) continue;
                        g.DrawString(txts[i].Text, txts[i].Font, txtBrushes[i], txts[i].Bounds, txtSf[i]);
                    }

                    for (int i = 0; i < pills.Length; i++)
                    {
                        g.FillPath(pillBgBr[i], pillPaths[i]);
                        var pb = pills[i].Bounds;
                        g.DrawString(pills[i].Text, _fontPill, pillFgBr[i],
                            new RectangleF(pb.X, pb.Y, pb.Width, pb.Height), sfCenter);
                    }

                    for (int i = 0; i < btns.Length; i++)
                    {
                        g.FillPath(i == hoveredBtn ? btnHoverBr[i] : btnNormalBr[i], btnPaths[i]);
                        var bb = btns[i].Bounds;
                        g.DrawString(btns[i].Text, _fontActionBtn, btnFgBr[i],
                            new RectangleF(bb.X, bb.Y, bb.Width, bb.Height), sfCenter);
                    }
                };

                panel.MouseMove += (s, e) =>
                {
                    int prev = hoveredBtn;
                    hoveredBtn = -1;
                    for (int i = 0; i < btns.Length; i++)
                        if (btns[i].Bounds.Contains(e.Location)) { hoveredBtn = i; break; }
                    panel.Cursor = hoveredBtn >= 0 ? Cursors.Hand : Cursors.Default;
                    if (prev != hoveredBtn) panel.Invalidate();
                };

                panel.MouseLeave += (s, e) =>
                {
                    if (hoveredBtn >= 0) { hoveredBtn = -1; panel.Invalidate(); }
                    panel.BackColor = Color.White;
                };

                panel.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(248, 249, 255);

                panel.MouseClick += (s, e) =>
                {
                    for (int i = 0; i < btns.Length; i++)
                        if (btns[i].Bounds.Contains(e.Location)) { btns[i].OnClick?.Invoke(); return; }
                    _cardClick?.Invoke();
                };

                panel.Disposed += (s, e) =>
                {
                    borderPen.Dispose(); sfCenter.Dispose();
                    foreach (var b in txtBrushes)  b?.Dispose();
                    foreach (var b in avBrushes)   b?.Dispose();
                    foreach (var b in pillBgBr)    b?.Dispose();
                    foreach (var b in pillFgBr)    b?.Dispose();
                    foreach (var p in pillPaths)   p?.Dispose();
                    foreach (var b in btnNormalBr) b?.Dispose();
                    foreach (var b in btnHoverBr)  b?.Dispose();
                    foreach (var b in btnFgBr)     b?.Dispose();
                    foreach (var p in btnPaths)    p?.Dispose();
                    foreach (var sf in txtSf)      sf?.Dispose();
                };

                return panel;
            }
        }

        // ── Geometry helpers ─────────────────────────────────────────────────

        internal static GraphicsPath RoundedRect(Rectangle r, int rad)
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
