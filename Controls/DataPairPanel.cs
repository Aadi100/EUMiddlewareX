using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    /// <summary>
    /// Represents a single data pair item with label and multiple value elements.
    /// </summary>
    public class DataPairItem
    {
        public string Label { get; set; }
        public string Type { get; set; } // Staff or Member
        public string Status { get; set; } // ONLINE, OFFLINE, RETRYING, etc.
        public List<DataElement> Elements { get; set; }

        // Device Connection Details for Door Opening
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int MachineNo { get; set; }
        public string Password { get; set; }

        public DataPairItem()
        {
            Elements = new List<DataElement>();
        }
    }

    /// <summary>
    /// Represents a single data element within a pair (text, link, badge, etc.).
    /// </summary>
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
            MaxWidth = 0; // 0 means no limit
        }
    }

    /// <summary>
    /// A custom panel control that displays data in pairs.
    /// Each pair is displayed in a separate child panel.
    /// Each pair can contain multiple data elements (text, links, badges, etc.).
    /// Can be dragged and dropped from the toolbox onto a form.
    /// </summary>
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(Panel))]
    public class DataPairPanel : Panel
    {
        private List<DataPairItem> _dataPairs;
        private Dictionary<int, Color> _borderColors = new Dictionary<int, Color>();
        private int _childPanelHeight = 50;
        private int _padding = 10;
        private Color _labelColor = Color.FromArgb(51, 51, 51);
        private Color _valueColor = Color.FromArgb(0, 102, 204);
        private Color _childPanelBackColor = Color.FromArgb(240, 240, 240);
        private Color _borderColor = Color.FromArgb(200, 200, 200);
        private Font _labelFont = new Font("Segoe UI", 9, FontStyle.Bold);
        private Font _valueFont = new Font("Segoe UI", 9, FontStyle.Regular);
        private Font _linkFont = new Font("Segoe UI", 9, FontStyle.Underline);
        private bool _alternateColors = true;
        private int _labelWidth = 120;

        /// <summary>
        /// Event fired when the 'Open' link is clicked for a device.
        /// </summary>
        public event Func<DataPairItem, System.Threading.Tasks.Task> OpenDoorRequested;

        public DataPairPanel()
        {
            _dataPairs = new List<DataPairItem>();
            this.AutoScroll = true;
            this.BackColor = Color.Transparent;
            this.BorderStyle = BorderStyle.None;
            this.DoubleBuffered = true;
            _childPanelHeight = 70; // Set default height for 2-line layout
        }

        /// <summary>
        /// Updates the status and border color of a specific item by index.
        /// </summary>
        /// <param name="index">Index of the item</param>
        /// <param name="status">ONLINE, OFFLINE, or RETRYING</param>
        public void UpdateItemStatus(int index, string status)
        {
            if (index < 0 || index >= _dataPairs.Count) return;

            var item = _dataPairs[index];
            item.Status = status;

            Color borderColor;
            Color statusColor;
            string statusText = status.ToUpper();

            switch (status.ToUpper())
            {
                case "ONLINE":
                    borderColor = Color.FromArgb(230, 230, 230);
                    statusColor = Color.FromArgb(40, 167, 69);
                    break;
                case "RETRYING":
                    borderColor = Color.Orange;
                    statusColor = Color.Orange;
                    break;
                case "OFFLINE":
                default:
                    borderColor = Color.Red;
                    statusColor = Color.Red;
                    break;
            }

            _borderColors[index] = borderColor;
            if (item.Elements.Count > 2)
            {
                item.Elements[2].Text = statusText;
                item.Elements[2].TextColor = statusColor;
            }

            UpdateChildUI(index);
        }

        private void UpdateChildUI(int index)
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(new Action(() => {
                    if (index < this.Controls.Count)
                    {
                        var childPanel = this.Controls[this.Controls.Count - 1 - index] as Panel; // Panels are added in order, but Controls collection order might vary depending on how they are inserted. Actually RefreshLayout clears and adds.
                        // Better to find the specific panel by its index/name if possible or just refresh that specific child.

                        foreach (Control ctrl in this.Controls)
                        {
                            if (ctrl is Panel p && p.Tag != null && (int)p.Tag == index)
                            {
                                p.Invalidate(); // Trigger border repaint
                                UpdateBadgeInPanel(p, _dataPairs[index]);
                                break;
                            }
                        }
                    }
                }));
            }
        }

        private void UpdateBadgeInPanel(Panel childPanel, DataPairItem dataPair)
        {
            if (dataPair.Elements.Count > 2)
            {
                var status = dataPair.Elements[2];
                bool isConnected = status.Text.Equals("ONLINE", StringComparison.OrdinalIgnoreCase);

                // Find badge panel and its label
                foreach (Control c in childPanel.Controls)
                {
                    if (c is Panel badge && badge.Width == 95) // Identifying by layout properties or we could use names
                    {
                        badge.BackColor = isConnected ? Color.FromArgb(232, 250, 240) : Color.FromArgb(255, 240, 240);
                        foreach (Control bc in badge.Controls)
                        {
                            if (bc is Label lbl)
                            {
                                lbl.Text = status.Text;
                                lbl.ForeColor = status.TextColor;
                            }
                            else if (bc.Width == 8) // The dot
                            {
                                bc.BackColor = status.TextColor;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the label column.
        /// </summary>
        [Browsable(true), Category("DataPairPanel")]
        public int LabelWidth
        {
            get => _labelWidth;
            set
            {
                if (value > 0)
                {
                    _labelWidth = value;
                    RefreshLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of each child panel.
        /// </summary>
        [Browsable(true), Category("DataPairPanel")]
        public int ChildPanelHeight
        {
            get => _childPanelHeight;
            set
            {
                if (value > 0)
                {
                    _childPanelHeight = value;
                    RefreshLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the padding between child panels and elements.
        /// </summary>
        [Browsable(true), Category("DataPairPanel")]
        public int Padding
        {
            get => _padding;
            set
            {
                if (value >= 0)
                {
                    _padding = value;
                    RefreshLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color of child panels.
        /// </summary>
        [Browsable(true), Category("DataPairPanel")]
        public Color ChildPanelBackColor
        {
            get => _childPanelBackColor;
            set { _childPanelBackColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the border color of child panels.
        /// </summary>
        [Browsable(true), Category("DataPairPanel")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the label text color.
        /// </summary>
        [Browsable(true), Category("DataPairPanel")]
        public Color LabelColor
        {
            get => _labelColor;
            set { _labelColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the value text color.
        /// </summary>
        [Browsable(true), Category("DataPairPanel")]
        public Color ValueColor
        {
            get => _valueColor;
            set { _valueColor = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets whether to use alternate colors for child panels.
        /// </summary>
        [Browsable(true), Category("DataPairPanel")]
        public bool AlternateColors
        {
            get => _alternateColors;
            set { _alternateColors = value; Invalidate(); }
        }

        /// <summary>
        /// Populates the panel with data pairs using the new flexible DataPairItem structure.
        /// </summary>
        public void SetData(List<DataPairItem> dataPairs)
        {
            if (dataPairs == null)
                dataPairs = new List<DataPairItem>();

            _dataPairs = new List<DataPairItem>(dataPairs);
            RefreshLayout();
        }

        /// <summary>
        /// Adds a single data pair item to the panel.
        /// </summary>
        public void AddDataPair(string label, List<DataElement> elements, string type = "")
        {
            var item = new DataPairItem { Label = label, Elements = elements, Type = type };
            _dataPairs.Add(item);
            RefreshLayout();
        }

        /// <summary>
        /// Adds a single data pair with a simple text value.
        /// </summary>
        public void AddDataPair(string label, string value, Color? valueColor = null)
        {
            var elements = new List<DataElement>
            {
                new DataElement(value, valueColor, false)
            };
            AddDataPair(label, elements);
        }

        /// <summary>
        /// Clears all data pairs from the panel.
        /// </summary>
        public void Clear()
        {
            _dataPairs.Clear();
            this.Controls.Clear();
            this.AutoScrollPosition = new Point(0, 0);
        }

        /// <summary>
        /// Gets the current data pairs.
        /// </summary>
        public List<DataPairItem> GetData()
        {
            return new List<DataPairItem>(_dataPairs);
        }

        private void RefreshLayout()
        {
            this.SuspendLayout();
            this.Controls.Clear();
            int yPosition = _padding;

            for (int i = 0; i < _dataPairs.Count; i++)
            {
                Panel childPanel = CreateChildPanel(_dataPairs[i], i);
                childPanel.Top = yPosition;
                this.Controls.Add(childPanel);
                // Add spacing between cards
                yPosition += childPanel.Height + _padding;
            }

            this.ResumeLayout();
        }

        private Panel CreateChildPanel(DataPairItem dataPair, int index)
        {
            int requiredHeight = 85;

            // Set initial border color based on status if not already tracked
            if (!_borderColors.ContainsKey(index))
            {
                string statusUpper = dataPair.Status?.ToUpper() ?? "";
                if (statusUpper == "ONLINE") _borderColors[index] = Color.FromArgb(230, 230, 230);
                else if (statusUpper == "RETRYING") _borderColors[index] = Color.Orange;
                else if (statusUpper == "OFFLINE") _borderColors[index] = Color.Red;
                else _borderColors[index] = Color.FromArgb(230, 230, 230);
            }

            Panel childPanel = new Panel
            {
                Width = this.DisplayRectangle.Width - (2 * _padding),
                Tag = index, // Used for targeted updates
                Height = requiredHeight,
                Left = _padding,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                AutoScroll = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // 1. Device Name (Top Left)
            Label nameLabel = new Label
            {
                Text = dataPair.Label,
                Left = _padding,
                Top = _padding,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                MaximumSize = new Size(childPanel.Width - 120, 0), // Prevent name from hitting badges
                AutoEllipsis = true
            };
            childPanel.Controls.Add(nameLabel);

            // 2. Machine No
            if (dataPair.Elements.Count > 0)
            {
                string machineText = dataPair.Elements[0].Text;
                if (!machineText.StartsWith("Machine No:", StringComparison.OrdinalIgnoreCase))
                {
                    machineText = "Machine No: " + machineText;
                }

                Label machineLabel = new Label
                {
                    Text = machineText,
                    Left = _padding,
                    Top = nameLabel.Bottom + 2,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(120, 120, 120)
                };
                childPanel.Controls.Add(machineLabel);
            }

            // 3. Device IP
            if (dataPair.Elements.Count > 1)
            {
                string ipText = dataPair.Elements[1].Text;
                if (!ipText.StartsWith("Device IP:", StringComparison.OrdinalIgnoreCase) && !ipText.StartsWith("IP:", StringComparison.OrdinalIgnoreCase))
                {
                    ipText = "Device IP: " + ipText;
                }

                Label ipLabel = new Label
                {
                    Text = ipText,
                    Left = _padding,
                    Top = nameLabel.Bottom + 20,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(120, 120, 120)
                };
                childPanel.Controls.Add(ipLabel);
            }

            // --- RIGHT SIDE COLUMN (Type, Status, Open) ---
            int rightColumnWidth = 95;
            int rightColumnLeft = childPanel.Width - rightColumnWidth - _padding;

            // 4. Type Badge (Staff/Member) - Positioned at Top Right
            if (!string.IsNullOrEmpty(dataPair.Type))
            {
                Label typeLabel = new Label
                {
                    Text = dataPair.Type.ToUpper(),
                    Width = rightColumnWidth,
                    Height = 18,
                    Left = rightColumnLeft,
                    Top = _padding - 2,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                    ForeColor = dataPair.Type.Equals("Staff", StringComparison.OrdinalIgnoreCase) ? Color.DarkSlateBlue : Color.DarkCyan,
                    BackColor = Color.FromArgb(240, 240, 255),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                childPanel.Controls.Add(typeLabel);
            }

            // 5. Status Badge (Right Aligned, Below Type)
            if (dataPair.Elements.Count > 2)
            {
                var status = dataPair.Elements[2];
                bool isConnected = status.Text.ToUpper().Contains("ONLINE") || status.Text.ToLower().Contains("connected");

                Panel badge = new Panel
                {
                    Width = rightColumnWidth,
                    Height = 26,
                    BackColor = isConnected ? Color.FromArgb(232, 250, 240) : Color.FromArgb(255, 240, 240),
                    Left = rightColumnLeft,
                    Top = _padding + 18, // Placed below Type Badge
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                // Status Dot
                Panel dot = new Panel
                {
                    Width = 8,
                    Height = 8,
                    BackColor = status.TextColor,
                    Left = 8,
                    Top = 9
                };
                System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddEllipse(0, 0, dot.Width, dot.Height);
                dot.Region = new Region(gp);
                badge.Controls.Add(dot);

                Label statusLabel = new Label
                {
                    Text = status.Text,
                    Left = 20,
                    Top = 0,
                    Height = 26,
                    Width = 70,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI Semibold", 8.5f, FontStyle.Bold),
                    ForeColor = status.TextColor
                };
                badge.Controls.Add(statusLabel);
                childPanel.Controls.Add(badge);
            }

            // 6. Open Link (Below Status)
            if (dataPair.Elements.Count > 3)
            {
                var link = dataPair.Elements[3];
                LinkLabel openLink = new LinkLabel
                {
                    Text = link.Text,
                    AutoSize = false,
                    Width = rightColumnWidth,
                    TextAlign = ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Font = new Font("Segoe UI", 9f, FontStyle.Underline),
                    LinkColor = Color.FromArgb(0, 0, 150),
                    ActiveLinkColor = Color.Blue,
                    VisitedLinkColor = Color.FromArgb(0, 0, 150)
                };
                openLink.Left = rightColumnLeft;
                openLink.Top = _padding + 48; // Shifted down further
                openLink.LinkClicked += async (s, e) => {
                    if (OpenDoorRequested != null)
                    {
                        await OpenDoorRequested.Invoke(dataPair);
                    }
                    else if (!string.IsNullOrEmpty(link.Tooltip))
                    {
                        MessageBox.Show(link.Tooltip);
                    }
                };
                childPanel.Controls.Add(openLink);
            }

            // Draw a subtle dashed border
            childPanel.Paint += (s, e) => {
                Panel p = s as Panel;
                int itemIndex = (int)p.Tag;
                Color borderColor = _borderColors.ContainsKey(itemIndex) ? _borderColors[itemIndex] : Color.FromArgb(230, 230, 230);

                using (Pen pen = new Pen(borderColor, 1))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                }
            };

            return childPanel;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Refresh layout when the panel is resized to maintain proper sizing
            if (_dataPairs.Count > 0)
            {
                RefreshLayout();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _labelFont?.Dispose();
                _valueFont?.Dispose();
                _linkFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}