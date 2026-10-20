using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware.Controls
{
    public class RoundedComboBox : UserControl
    {
        private readonly ComboBox _inner;
        private Color _borderColor      = Color.FromArgb(209, 213, 219);
        private Color _focusBorderColor = Color.FromArgb(59, 130, 246);
        private int   _radius           = 10;
        private bool  _isFocused        = false;

        public Color BorderColor      { get => _borderColor;      set { _borderColor      = value; Invalidate(); } }
        public Color FocusBorderColor { get => _focusBorderColor; set { _focusBorderColor = value; Invalidate(); } }
        public int   Radius           { get => _radius;           set { _radius           = value; Invalidate(); } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ComboBox.ObjectCollection Items => _inner.Items;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource { get => _inner.DataSource; set => _inner.DataSource = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DisplayMember { get => _inner.DisplayMember; set => _inner.DisplayMember = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ValueMember   { get => _inner.ValueMember;   set => _inner.ValueMember   = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem  { get => _inner.SelectedItem;  set => _inner.SelectedItem  = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int    SelectedIndex { get => _inner.SelectedIndex; set => _inner.SelectedIndex = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text { get => _inner.Text; set => _inner.Text = value; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Font Font { get => _inner.Font; set { _inner.Font = value; } }

        public event EventHandler SelectedIndexChanged;

        public RoundedComboBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size      = new Size(260, 42);

            _inner = new ComboBox
            {
                DropDownStyle  = ComboBoxStyle.DropDownList,
                FlatStyle      = FlatStyle.Flat,
                BackColor      = Color.White,
                ForeColor      = Color.FromArgb(30, 38, 90),
                Font           = new Font("Segoe UI", 10f),
                FormattingEnabled = true,
            };
            _inner.GotFocus  += (s, e) => { _isFocused = true;  Invalidate(); };
            _inner.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
            _inner.SelectedIndexChanged += (s, e) => { Invalidate(); SelectedIndexChanged?.Invoke(this, e); };

            Controls.Add(_inner);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (_inner == null) return;
            int pad = _radius / 2 + 4;
            _inner.Location = new Point(pad + 4, (Height - _inner.PreferredHeight) / 2);
            _inner.Width    = Width - (pad + 4) * 2;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rc     = new Rectangle(1, 1, Width - 2, Height - 2);
            var border = _isFocused ? _focusBorderColor : _borderColor;

            using (var path = RoundedPath(rc, _radius))
            {
                using (var br = new SolidBrush(Color.White))
                    g.FillPath(br, path);
                using (var pen = new Pen(border, _isFocused ? 2f : 1.5f))
                    g.DrawPath(pen, path);
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
