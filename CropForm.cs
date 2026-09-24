using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    /// <summary>
    /// Modal circular-crop dialog: shows the source image, lets the user drag/resize a
    /// circular selection (corner-handle drag like a typical ID-photo cropper), and returns
    /// the cropped, circle-masked result via <see cref="CroppedImage"/>.
    /// </summary>
    public class CropForm : Form
    {
        private static readonly Color BrandNavy = Color.FromArgb(18, 23, 49);

        private readonly Bitmap _sourceImage;
        private readonly PictureBox _picture;
        private Rectangle _selection;
        private bool _dragging;
        private bool _resizing;
        private Point _dragStart;
        private const int HandleSize = 10;

        public Bitmap CroppedImage { get; private set; }

        public CropForm(Bitmap sourceImage)
        {
            _sourceImage = sourceImage ?? throw new ArgumentNullException(nameof(sourceImage));

            Text = "Crop Photo";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(37, 37, 40);

            const int headerHeight = 56;
            const int footerHeight = 64;
            const int pad = 16;

            int maxW = 640, maxH = 480;
            double scale = Math.Min((double)maxW / _sourceImage.Width, (double)maxH / _sourceImage.Height);
            if (scale > 1) scale = 1;
            int displayW = (int)(_sourceImage.Width * scale);
            int displayH = (int)(_sourceImage.Height * scale);

            ClientSize = new Size(displayW + pad * 2, headerHeight + displayH + footerHeight);

            // ---- Header (matches the app's navy branding) ----
            var header = new Panel { Location = new Point(0, 0), Size = new Size(ClientSize.Width, headerHeight), BackColor = BrandNavy };
            var lblTitle = new Label
            {
                Text = "Crop Photo",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(pad, 10)
            };
            var lblHint = new Label
            {
                Text = "Drag inside the circle to reposition · drag the corner handle to resize",
                ForeColor = Color.FromArgb(190, 195, 220),
                Font = new Font("Segoe UI", 8F),
                AutoSize = true,
                Location = new Point(pad, 34)
            };
            header.Controls.Add(lblTitle);
            header.Controls.Add(lblHint);
            Controls.Add(header);

            _picture = new PictureBox
            {
                Location = new Point(pad, headerHeight),
                Size = new Size(displayW, displayH),
                Image = _sourceImage,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };
            _picture.Paint += Picture_Paint;
            _picture.MouseDown += Picture_MouseDown;
            _picture.MouseMove += Picture_MouseMove;
            _picture.MouseUp += (s, e) => { _dragging = false; _resizing = false; };
            Controls.Add(_picture);

            int diameter = Math.Min(displayW, displayH) - 20;
            _selection = new Rectangle(
                (displayW - diameter) / 2,
                (displayH - diameter) / 2,
                diameter, diameter);

            var buttonPanel = new FlowLayoutPanel
            {
                Location = new Point(0, headerHeight + displayH),
                Size = new Size(ClientSize.Width, footerHeight),
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(pad, 14, pad, 14),
                BackColor = Color.FromArgb(245, 245, 247)
            };

            var btnCrop = new Button
            {
                Text = "Crop && Use",
                Width = 120,
                Height = 34,
                BackColor = BrandNavy,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCrop.FlatAppearance.BorderSize = 0;
            btnCrop.Click += BtnCrop_Click;

            var btnCancel = new Button { Text = "Cancel", Width = 90, Height = 34, FlatStyle = FlatStyle.Standard };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            buttonPanel.Controls.Add(btnCrop);
            buttonPanel.Controls.Add(btnCancel);
            Controls.Add(buttonPanel);
        }

        private Rectangle ResizeHandleRect =>
            new Rectangle(_selection.Right - HandleSize / 2, _selection.Bottom - HandleSize / 2, HandleSize, HandleSize);

        private void Picture_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var overlay = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            using (var path = new GraphicsPath())
            {
                path.AddRectangle(_picture.ClientRectangle);
                path.AddEllipse(_selection);
                path.FillMode = FillMode.Alternate;
                g.FillPath(overlay, path);
            }

            using (var pen = new Pen(Color.White, 2) { DashStyle = DashStyle.Dash })
            {
                g.DrawEllipse(pen, _selection);
            }

            // Corner drag handles (visual only — bottom-right doubles as the resize hotspot)
            foreach (var pt in new[]
            {
                new Point(_selection.Left, _selection.Top),
                new Point(_selection.Right, _selection.Top),
                new Point(_selection.Left, _selection.Bottom),
                new Point(_selection.Right, _selection.Bottom)
            })
            {
                var r = new Rectangle(pt.X - HandleSize / 2, pt.Y - HandleSize / 2, HandleSize, HandleSize);
                g.FillRectangle(Brushes.White, r);
                g.DrawRectangle(Pens.Gray, r);
            }
        }

        private void Picture_MouseDown(object sender, MouseEventArgs e)
        {
            if (ResizeHandleRect.Contains(e.Location))
            {
                _resizing = true;
            }
            else if (IsInsideCircle(e.Location))
            {
                _dragging = true;
            }
            _dragStart = e.Location;
        }

        private void Picture_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
            {
                int dx = e.Location.X - _dragStart.X;
                int dy = e.Location.Y - _dragStart.Y;
                int delta = Math.Max(dx, dy);
                int newSize = Math.Max(30, _selection.Width + delta);

                // Keep the selection inside the visible picture box bounds
                newSize = Math.Min(newSize, _picture.Width - _selection.Left);
                newSize = Math.Min(newSize, _picture.Height - _selection.Top);

                _selection = new Rectangle(_selection.Left, _selection.Top, newSize, newSize);
                _dragStart = e.Location;
                _picture.Invalidate();
            }
            else if (_dragging)
            {
                int dx = e.Location.X - _dragStart.X;
                int dy = e.Location.Y - _dragStart.Y;

                int newX = Clamp(_selection.X + dx, 0, _picture.Width - _selection.Width);
                int newY = Clamp(_selection.Y + dy, 0, _picture.Height - _selection.Height);

                _selection = new Rectangle(newX, newY, _selection.Width, _selection.Height);
                _dragStart = e.Location;
                _picture.Invalidate();
            }
            else
            {
                bool overHandle = ResizeHandleRect.Contains(e.Location);
                _picture.Cursor = overHandle ? Cursors.SizeNWSE
                    : (IsInsideCircle(e.Location) ? Cursors.SizeAll : Cursors.Default);
            }
        }

        private bool IsInsideCircle(Point p)
        {
            var center = new PointF(_selection.X + _selection.Width / 2f, _selection.Y + _selection.Height / 2f);
            float radius = _selection.Width / 2f;
            float dx = p.X - center.X;
            float dy = p.Y - center.Y;
            return (dx * dx + dy * dy) <= (radius * radius);
        }

        private static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));

        private void BtnCrop_Click(object sender, EventArgs e)
        {
            // Map the on-screen selection back to source-image pixel coordinates
            double scaleX = (double)_sourceImage.Width / _picture.Width;
            double scaleY = (double)_sourceImage.Height / _picture.Height;

            var srcRect = new Rectangle(
                (int)(_selection.X * scaleX),
                (int)(_selection.Y * scaleY),
                (int)(_selection.Width * scaleX),
                (int)(_selection.Height * scaleY));

            srcRect.Intersect(new Rectangle(0, 0, _sourceImage.Width, _sourceImage.Height));
            if (srcRect.Width <= 0 || srcRect.Height <= 0)
            {
                MessageBox.Show("Invalid crop selection.", "Crop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = new Bitmap(srcRect.Width, srcRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, srcRect.Width, srcRect.Height);
                    g.SetClip(path);
                    g.DrawImage(_sourceImage, new Rectangle(0, 0, srcRect.Width, srcRect.Height), srcRect, GraphicsUnit.Pixel);
                }
            }

            CroppedImage = result;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
