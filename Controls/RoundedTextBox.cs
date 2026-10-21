using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    public class RoundedTextBox : UserControl
    {
        private readonly TextBox _inner;
        private Color _borderColor        = Color.FromArgb(209, 213, 219);
        private Color _focusBorderColor   = Color.FromArgb(59, 130, 246);
        private Color _placeholderColor   = Color.FromArgb(156, 163, 175);
        private int   _radius             = 10;
        private string _placeholder       = "";
        private bool  _isFocused          = false;

        public string Placeholder { get => _placeholder; set { _placeholder = value; _inner?.Invalidate(); Invalidate(); } }
        public Color  BorderColor      { get => _borderColor;      set { _borderColor      = value; Invalidate(); } }
        public Color  FocusBorderColor { get => _focusBorderColor; set { _focusBorderColor = value; Invalidate(); } }
        public int    Radius           { get => _radius;           set { _radius           = value; Invalidate(); } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool   UseSystemPasswordChar { get => _inner.UseSystemPasswordChar; set => _inner.UseSystemPasswordChar = value; }
        public void   Clear() => _inner?.Clear();
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public char   PasswordChar { get => _inner.PasswordChar; set => _inner.PasswordChar = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool   ReadOnly { get => _inner.ReadOnly; set { _inner.ReadOnly = value; Invalidate(); } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int    MaxLength { get => _inner.MaxLength; set => _inner.MaxLength = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HorizontalAlignment TextAlign { get => _inner.TextAlign; set => _inner.TextAlign = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AutoCompleteMode AutoCompleteMode { get => _inner.AutoCompleteMode; set => _inner.AutoCompleteMode = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AutoCompleteSource AutoCompleteSource { get => _inner.AutoCompleteSource; set => _inner.AutoCompleteSource = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AutoCompleteStringCollection AutoCompleteCustomSource { get => _inner.AutoCompleteCustomSource; set => _inner.AutoCompleteCustomSource = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text { get => _inner?.Text ?? ""; set { if (_inner != null) _inner.Text = value; } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Font Font { get => _inner?.Font ?? base.Font; set { if (_inner != null) _inner.Font = value; Invalidate(); } }

        public RoundedTextBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size      = new Size(260, 42);

            _inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor   = Color.White,
                ForeColor   = Color.FromArgb(30, 38, 90),
                Font        = new Font("Segoe UI", 10f),
                Multiline   = false,
            };
            _inner.GotFocus  += (s, e) => { _isFocused = true;  Invalidate(); };
            _inner.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
            _inner.TextChanged += (s, e) => { Invalidate(); OnTextChanged(e); };

            Controls.Add(_inner);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (_inner == null) return;
            int pad = _radius / 2 + 4;
            _inner.Location = new Point(pad + 6, (Height - _inner.PreferredHeight) / 2);
            _inner.Width    = Width - (pad + 6) * 2;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rc  = new Rectangle(1, 1, Width - 2, Height - 2);
            var border = DesignMode ? _borderColor : (_isFocused ? _focusBorderColor : _borderColor);

            using (var path = RoundedPath(rc, _radius))
            {
                using (var br = new SolidBrush(Color.White))
                    g.FillPath(br, path);
                using (var pen = new Pen(border, _isFocused ? 2f : 1.5f))
                    g.DrawPath(pen, path);
            }

            // Placeholder / design-mode label
            if (DesignMode)
            {
                string label = !string.IsNullOrEmpty(_placeholder) ? _placeholder : Name;
                int pad2 = _radius / 2 + 10;
                using (var br = new SolidBrush(Color.FromArgb(156, 163, 175)))
                    g.DrawString(label, new Font("Segoe UI", 10f), br,
                        new RectangleF(pad2, 0, Width - pad2 * 2, Height),
                        new StringFormat { LineAlignment = StringAlignment.Center });
                return;
            }
            if (!_isFocused && string.IsNullOrEmpty(_inner.Text) && !string.IsNullOrEmpty(_placeholder))
            {
                int pad = _radius / 2 + 10;
                using (var br = new SolidBrush(_placeholderColor))
                    g.DrawString(_placeholder, _inner.Font, br,
                        new RectangleF(pad, 0, Width - pad * 2, Height),
                        new StringFormat { LineAlignment = StringAlignment.Center });
            }
        }

        private GraphicsPath RoundedPath(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
