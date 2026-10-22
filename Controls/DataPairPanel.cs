using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using smart_attendance_middleware;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    public class DataPairItem
    {
        public string Label { get; set; }
        public string Type { get; set; } // Staff or Member
        public string Status { get; set; } // ONLINE, OFFLINE, RETRYING, etc.
        public List<DataElement> Elements { get; set; }

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int MachineNo { get; set; }
        public string Password { get; set; }

        public DataPairItem()
        {
            Elements = new List<DataElement>();
        }
    }

    public class DataElement
    {
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public Color BackColor { get; set; }
        public bool IsLink { get; set; }
        public string Tooltip { get; set; }
        public int MaxWidth { get; set; }

        public DataElement(string text, Color? textColor = null, bool isLink = false, string tooltip = "", Color? backColor = null)
        {
            Text = text;
            TextColor = textColor ?? Color.FromArgb(0, 102, 204);
            BackColor = backColor ?? Color.Transparent;
            IsLink = isLink;
            Tooltip = tooltip;
            MaxWidth = 0;
        }
    }

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(Panel))]
    public class DataPairPanel : Panel
    {
        private List<DataPairItem> _dataPairs;
        private Dictionary<int, Color> _borderColors = new Dictionary<int, Color>();
        private int _padding = 8;

        public event Func<DataPairItem, System.Threading.Tasks.Task> OpenDoorRequested;

        public DataPairPanel()
        {
            _dataPairs = new List<DataPairItem>();
            this.AutoScroll = true;
            this.BackColor = Color.Transparent;
            this.BorderStyle = BorderStyle.None;
            this.DoubleBuffered = true;
        }

        public void UpdateItemStatus(int index, string status)
        {
            if (index < 0 || index >= _dataPairs.Count) return;

            var item = _dataPairs[index];
            item.Status = status;

            if (item.Elements.Count > 2)
            {
                string statusUpper = status.ToUpper();
                Color statusColor = statusUpper == "ONLINE" ? Color.FromArgb(40, 167, 69)
                                  : statusUpper == "RETRYING" ? Color.Orange
                                  : Color.FromArgb(220, 53, 69);
                item.Elements[2].Text = statusUpper;
                item.Elements[2].TextColor = statusColor;
            }

            // Rebuild layout to reflect status change (card background and right panel differ by status)
            if (this.IsHandleCreated && !this.IsDisposed)
                this.BeginInvoke(new Action(RefreshLayout));
        }

        // --- Public API ---

        public void SetData(List<DataPairItem> dataPairs)
        {
            _dataPairs = dataPairs ?? new List<DataPairItem>();
            _borderColors.Clear();
            RefreshLayout();
        }

        public void AddDataPair(string label, List<DataElement> elements, string type = "")
        {
            _dataPairs.Add(new DataPairItem { Label = label, Elements = elements, Type = type });
            RefreshLayout();
        }

        public void AddDataPair(string label, string value, Color? valueColor = null)
        {
            AddDataPair(label, new List<DataElement> { new DataElement(value, valueColor) });
        }

        public void Clear()
        {
            _dataPairs.Clear();
            _borderColors.Clear();
            this.Controls.Clear();
            this.AutoScrollPosition = new Point(0, 0);
        }

        public List<DataPairItem> GetData() => new List<DataPairItem>(_dataPairs);

        // Kept for backward compatibility
        [Browsable(true), Category("DataPairPanel")]
        public int LabelWidth { get; set; } = 120;

        [Browsable(true), Category("DataPairPanel")]
        public int ChildPanelHeight { get; set; } = 90;

        [Browsable(true), Category("DataPairPanel")]
        public new int Padding
        {
            get => _padding;
            set { _padding = value; RefreshLayout(); }
        }

        [Browsable(true), Category("DataPairPanel")]
        public Color ChildPanelBackColor { get; set; } = Color.White;

        [Browsable(true), Category("DataPairPanel")]
        public Color BorderColor { get; set; } = Color.FromArgb(220, 220, 220);

        [Browsable(true), Category("DataPairPanel")]
        public Color LabelColor { get; set; } = Color.FromArgb(51, 51, 51);

        [Browsable(true), Category("DataPairPanel")]
        public Color ValueColor { get; set; } = Color.FromArgb(0, 102, 204);

        [Browsable(true), Category("DataPairPanel")]
        public bool AlternateColors { get; set; } = false;

        // --- Layout ---

        private void RefreshLayout()
        {
            this.SuspendLayout();
            this.Controls.Clear();

            if (_dataPairs.Count == 0)
            {
                this.Controls.Add(CreateEmptyState());
            }
            else
            {
                int y = _padding;
                for (int i = 0; i < _dataPairs.Count; i++)
                {
                    var card = CreateCard(_dataPairs[i], i);
                    card.Top = y;
                    this.Controls.Add(card);
                    y += card.Height + _padding;
                }
            }

            this.ResumeLayout();
        }

        private Panel CreateEmptyState()
        {
            Panel p = new Panel
            {
                Width     = this.DisplayRectangle.Width,
                Height    = this.Height > 0 ? this.Height : 400,
                Left      = 0,
                Top       = 0,
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            p.Paint += (s, pe) =>
            {
                var panel = (Panel)s;
                var g = pe.Graphics;
                g.SmoothingMode     = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int cx = panel.Width / 2;
                int cy = panel.Height / 2 - 30;

                // Outer light-blue circle
                int outerR = 70;
                g.FillEllipse(new SolidBrush(Color.FromArgb(210, 225, 255)),
                    cx - outerR, cy - outerR, outerR * 2, outerR * 2);

                // Inner medium-blue circle
                int innerR = 50;
                g.FillEllipse(new SolidBrush(Color.FromArgb(170, 200, 255)),
                    cx - innerR, cy - innerR, innerR * 2, innerR * 2);

                // White document rectangle (rounded)
                var docRect = new Rectangle(cx - 20, cy - 26, 40, 50);
                using (var docPath = RoundedRect(docRect, 6))
                    g.FillPath(Brushes.White, docPath);

                // Document lines
                using (var linePen = new Pen(Color.FromArgb(180, 200, 240), 2.5f))
                {
                    g.DrawLine(linePen, cx - 11, cy - 10, cx + 11, cy - 10);
                    g.DrawLine(linePen, cx - 11, cy,      cx + 11, cy);
                    g.DrawLine(linePen, cx - 11, cy + 10, cx + 5,  cy + 10);
                }

                // Blue "+" circle (bottom-right of inner circle)
                int plusR = 14;
                int px = cx + innerR - 8;
                int py = cy + innerR - 8;
                g.FillEllipse(new SolidBrush(Color.FromArgb(37, 99, 234)), px - plusR, py - plusR, plusR * 2, plusR * 2);
                using (var plusPen = new Pen(Color.White, 2.5f))
                {
                    g.DrawLine(plusPen, px - 6, py,     px + 6, py);
                    g.DrawLine(plusPen, px,     py - 6, px,     py + 6);
                }

                // "No devices added yet"
                string title = "No devices added yet";
                {
                    var sz = g.MeasureString(title, new Font("Segoe UI", 10f, FontStyle.Bold));
                    g.DrawString(title, new Font("Segoe UI", 10f, FontStyle.Bold), new SolidBrush(Color.FromArgb(30, 30, 30)),
                        cx - sz.Width / 2, cy + outerR + 16);
                }

                // Sub-text (two lines)
                string sub = "Add a biometric device to start\nsyncing members and check-ins.";
                using (var subBrush = new SolidBrush(Color.FromArgb(130, 130, 130)))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(sub, new Font("Segoe UI", 8.5f, FontStyle.Regular), subBrush,
                        new RectangleF(cx - 130, cy + outerR + 44, 260, 50), sf);
                }
            };

            return p;
        }

        private Panel CreateCard(DataPairItem item, int index)
        {
            string statusText = item.Elements.Count > 2 ? item.Elements[2].Text.ToUpper() : "OFFLINE";
            bool isOnline    = statusText.Contains("ONLINE");
            bool isRetrying  = statusText.Contains("RETRYING");

            Color borderColor = (isOnline || isRetrying) ? Color.FromArgb(220, 220, 220)
                              :                           Color.FromArgb(240, 185, 185);
            Color cardBg      = (isOnline || isRetrying) ? Color.White
                              :                           Color.FromArgb(255, 248, 248);
            Color dotColor    = isOnline   ? Color.FromArgb(40, 167, 69)
                              : isRetrying ? Color.Orange
                              :              Color.FromArgb(220, 53, 69);

            _borderColors[index] = borderColor;

            int cardH = 90;
            int lp = 14; // left pad
            int tp = 16; // top pad for name row

            Panel card = new Panel
            {
                Width  = this.DisplayRectangle.Width - (2 * _padding),
                Height = cardH,
                Left   = _padding,
                Tag    = index,
                BorderStyle = BorderStyle.None,
                BackColor   = cardBg,
                AutoScroll  = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // --- Status dot ---
            Panel dot = new Panel
            {
                Width     = 10,
                Height    = 10,
                BackColor = dotColor,
                Left      = lp,
                Top       = tp + 3
            };
            var dotPath = new GraphicsPath();
            dotPath.AddEllipse(0, 0, dot.Width, dot.Height);
            dot.Region = new Region(dotPath);
            card.Controls.Add(dot);

            // --- Device name ---
            Label nameLabel = new Label
            {
                Text    = item.Label ?? "Device",
                Left    = lp + 16,
                Top     = tp,
                AutoSize = true,
                Font     = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 25, 25),
                MaximumSize = new Size(card.Width - 160, 0),
                AutoEllipsis = true
            };
            card.Controls.Add(nameLabel);

            // --- IP (element[1]: "IP: x.x.x.x") ---
            if (item.Elements.Count > 1)
            {
                string ipRaw = item.Elements[1].Text ?? "";
                // strip "IP: " prefix for cleaner display
                if (ipRaw.StartsWith("IP:", StringComparison.OrdinalIgnoreCase))
                    ipRaw = ipRaw.Substring(3).Trim();
                if (ipRaw.StartsWith("Device IP:", StringComparison.OrdinalIgnoreCase))
                    ipRaw = ipRaw.Substring(10).Trim();

                // append port if available
                if (item.Port > 0)
                    ipRaw = $"{ipRaw} : {item.Port}";

                Label ipLabel = new Label
                {
                    Text      = ipRaw,
                    Left      = lp + 16,
                    Top       = tp + 20,
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(130, 130, 130)
                };
                card.Controls.Add(ipLabel);
            }

            // --- Machine chip (element[0]: "Machine No: X") ---
            if (item.Elements.Count > 0)
            {
                string machRaw = item.Elements[0].Text ?? "";
                if (machRaw.StartsWith("Machine No:", StringComparison.OrdinalIgnoreCase))
                    machRaw = machRaw.Substring("Machine No:".Length).Trim();
                string chipText = "Machine " + machRaw;

                Label chip = new Label
                {
                    Text      = chipText,
                    AutoSize  = true,
                    Font      = new Font("Segoe UI", 8f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(90, 90, 90),
                    BackColor = Color.Transparent,
                    Left      = lp + 14,
                    Top       = tp + 44,
                    Padding   = new Padding(0)
                };
                // Measure text to size the chip
                using (var g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var sz = g.MeasureString(chipText, chip.Font);
                    chip.Width  = (int)sz.Width + 16;
                    chip.Height = (int)sz.Height + 6;
                }
                chip.Top = tp + 42;

                chip.Paint += (s, pe) =>
                {
                    var lbl = (Label)s;
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    var rect = new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1);
                    using (var path = RoundedRect(rect, 5))
                    using (var brush = new SolidBrush(Color.FromArgb(235, 235, 235)))
                    {
                        pe.Graphics.FillPath(brush, path);
                        var sf = new StringFormat
                        {
                            Alignment     = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        pe.Graphics.DrawString(lbl.Text, lbl.Font, new SolidBrush(lbl.ForeColor),
                            new RectangleF(0, 0, lbl.Width, lbl.Height), sf);
                    }
                };
                card.Controls.Add(chip);

                // Staff type chip (only for Staff devices)
                if (item.Type?.Equals("Staff", StringComparison.OrdinalIgnoreCase) == true)
                {
                    string typeText = "Staff";
                    Label typeChip = new Label
                    {
                        Text      = typeText,
                        AutoSize  = true,
                        Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(99, 60, 180),
                        BackColor = Color.Transparent,
                        Left      = chip.Left + chip.Width + 6,
                        Top       = chip.Top,
                        Padding   = new Padding(0)
                    };
                    using (var g2 = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        var sz2 = g2.MeasureString(typeText, typeChip.Font);
                        typeChip.Width  = (int)sz2.Width + 16;
                        typeChip.Height = chip.Height;
                    }
                    typeChip.Paint += (s, pe) =>
                    {
                        Label lbl = s as Label;
                        pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (var path = RoundedRect(new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1), 5))
                        using (var brush = new SolidBrush(Color.FromArgb(235, 228, 255)))
                        {
                            pe.Graphics.FillPath(brush, path);
                            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            pe.Graphics.DrawString(lbl.Text, lbl.Font, new SolidBrush(lbl.ForeColor),
                                new RectangleF(0, 0, lbl.Width, lbl.Height), sf);
                        }
                    };
                    card.Controls.Add(typeChip);
                }
            }

            // --- Right column ---
            int btnW    = 95;
            int rightLeft = card.Width - btnW - lp;

            if (isOnline)
            {
                // Rounded outlined "Open door" button
                Button openBtn = new Button
                {
                    Text      = "Open door",
                    Width     = btnW,
                    Height    = 30,
                    Left      = rightLeft,
                    Top       = (cardH - 30) / 2,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(0, 102, 204),
                    BackColor = Color.White,
                    Cursor    = Cursors.Hand,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                    TabStop   = false
                };
                openBtn.FlatAppearance.BorderColor        = Color.FromArgb(0, 102, 204);
                openBtn.FlatAppearance.BorderSize         = 1;
                openBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 242, 255);
                openBtn.FlatAppearance.CheckedBackColor   = Color.Transparent;

                openBtn.Paint += (s, pe) =>
                {
                    var btn = (Button)s;
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    pe.Graphics.Clear(btn.BackColor);
                    var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                    using (var path = RoundedRect(rect, 12))
                    using (var pen  = new Pen(Color.FromArgb(0, 102, 204), 1.2f))
                    using (var bg   = new SolidBrush(btn.BackColor))
                    {
                        pe.Graphics.FillPath(bg, path);
                        pe.Graphics.DrawPath(pen, path);
                        var sf = new StringFormat
                        {
                            Alignment     = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        pe.Graphics.DrawString(btn.Text, btn.Font,
                            new SolidBrush(btn.ForeColor),
                            new RectangleF(0, 0, btn.Width, btn.Height), sf);
                    }
                };

                var capturedItem = item;
                openBtn.Click += async (s, e) =>
                {
                    if (OpenDoorRequested != null)
                        await OpenDoorRequested.Invoke(capturedItem);
                };
                card.Controls.Add(openBtn);
            }
            else if (isRetrying)
            {
                // Amber "Retrying..." rounded badge
                int badgeTop = (cardH / 2) - 28;

                Panel retryBadge = new Panel
                {
                    Width     = btnW,
                    Height    = 24,
                    BackColor = Color.FromArgb(255, 200, 100),
                    Left      = rightLeft,
                    Top       = badgeTop,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right
                };
                retryBadge.Paint += (s, pe) =>
                {
                    var p = (Panel)s;
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    pe.Graphics.Clear(p.Parent?.BackColor ?? Color.Transparent);
                    using (var path  = RoundedRect(new Rectangle(0, 0, p.Width, p.Height), 10))
                    using (var brush = new SolidBrush(Color.FromArgb(255, 200, 100)))
                    {
                        pe.Graphics.FillPath(brush, path);
                        var sf = new StringFormat
                        {
                            Alignment     = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        pe.Graphics.DrawString("Open door",
                            new Font("Segoe UI", 8.5f, FontStyle.Bold),
                            new SolidBrush(Color.FromArgb(140, 70, 0)),
                            new RectangleF(0, 0, p.Width, p.Height), sf);
                    }
                };
                card.Controls.Add(retryBadge);

                // "Please wait" sub-text
                Label waitLbl = new Label
                {
                    Text      = "Please wait",
                    Width     = btnW,
                    Height    = 18,
                    Left      = rightLeft,
                    Top       = badgeTop + 28,
                    TextAlign = ContentAlignment.MiddleRight,
                    Font      = new Font("Segoe UI", 8f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(140, 70, 0),
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right
                };
                card.Controls.Add(waitLbl);
            }
            else
            {
                // Pink "Offline" rounded badge
                int badgeTop = (cardH / 2) - 28;

                Panel offlineBadge = new Panel
                {
                    Width     = btnW,
                    Height    = 24,
                    BackColor = Color.FromArgb(255, 213, 213),
                    Left      = rightLeft,
                    Top       = badgeTop,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right
                };
                offlineBadge.Paint += (s, pe) =>
                {
                    var p = (Panel)s;
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    pe.Graphics.Clear(p.Parent?.BackColor ?? Color.Transparent);
                    using (var path  = RoundedRect(new Rectangle(0, 0, p.Width, p.Height), 10))
                    using (var brush = new SolidBrush(Color.FromArgb(255, 213, 213)))
                    {
                        pe.Graphics.FillPath(brush, path);
                        var sf = new StringFormat
                        {
                            Alignment     = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        pe.Graphics.DrawString("Offline",
                            new Font("Segoe UI", 8.5f, FontStyle.Bold),
                            new SolidBrush(Color.FromArgb(185, 40, 40)),
                            new RectangleF(0, 0, p.Width, p.Height), sf);
                    }
                };
                card.Controls.Add(offlineBadge);

                // "No response" text
                Label noResp = new Label
                {
                    Text      = "No response",
                    Width     = btnW,
                    Height    = 18,
                    Left      = rightLeft,
                    Top       = badgeTop + 28,
                    TextAlign = ContentAlignment.MiddleRight,
                    Font      = new Font("Segoe UI", 8f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(185, 40, 40),
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right
                };
                card.Controls.Add(noResp);
            }

            // --- Rounded card border ---
            card.Paint += (s, pe) =>
            {
                var p = (Panel)s;
                int idx = (int)p.Tag;
                Color bc = _borderColors.ContainsKey(idx) ? _borderColors[idx] : Color.FromArgb(220, 220, 220);
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedRect(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10))
                using (var pen  = new Pen(bc, 1.5f))
                {
                    pe.Graphics.DrawPath(pen, path);
                }
            };

            return card;
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(bounds.Left,                  bounds.Top,                   radius * 2, radius * 2, 180, 90);
            path.AddArc(bounds.Right - radius * 2,    bounds.Top,                   radius * 2, radius * 2, 270, 90);
            path.AddArc(bounds.Right - radius * 2,    bounds.Bottom - radius * 2,   radius * 2, radius * 2,   0, 90);
            path.AddArc(bounds.Left,                  bounds.Bottom - radius * 2,   radius * 2, radius * 2,  90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_dataPairs?.Count > 0)
                RefreshLayout();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

