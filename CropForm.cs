using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class CropForm : Form
    {
        private readonly Bitmap _sourceImage;
        private Rectangle _selection;
        private bool _dragging;
        private bool _resizing;
        private Point _dragStart;
        private const int HandleSize = 10;

        public Bitmap CroppedImage { get; private set; }

        public CropForm(Bitmap sourceImage)
        {
            _sourceImage = sourceImage ?? throw new ArgumentNullException(nameof(sourceImage));

            InitializeComponent();

            // Dynamic sizing based on source image
            const int pad = 16;
            int maxW = 640, maxH = 480;
            double scale = Math.Min((double)maxW / _sourceImage.Width, (double)maxH / _sourceImage.Height);
            if (scale > 1) scale = 1;
            int displayW = (int)(_sourceImage.Width * scale);
            int displayH = (int)(_sourceImage.Height * scale);

            picCanvas.Image    = _sourceImage;
            picCanvas.Location = new Point(pad, pnlHeader.Height);
            picCanvas.Size     = new Size(displayW, displayH);

            int formW = displayW + pad * 2;
            ClientSize = new Size(formW, pnlHeader.Height + displayH + pnlFooter.Height);

            // Position footer buttons right-aligned inside pnlFooter
            int btnH = 36;
            int btnY = (pnlFooter.Height - btnH) / 2;
            btnCrop.Size     = new Size(110, btnH);
            btnCancel.Size   = new Size(90, btnH);
            btnCrop.Location   = new Point(formW - pad - btnCrop.Width, btnY);
            btnCancel.Location = new Point(btnCrop.Left - 8 - btnCancel.Width, btnY);

            int diameter = Math.Min(displayW, displayH) - 20;
            _selection = new Rectangle(
                (displayW - diameter) / 2,
                (displayH - diameter) / 2,
                diameter, diameter);
        }

        private Rectangle ResizeHandleRect =>
            new Rectangle(_selection.Right - HandleSize / 2, _selection.Bottom - HandleSize / 2, HandleSize, HandleSize);

        private void PicCanvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var overlay = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            using (var path = new GraphicsPath())
            {
                path.AddRectangle(picCanvas.ClientRectangle);
                path.AddEllipse(_selection);
                path.FillMode = FillMode.Alternate;
                g.FillPath(overlay, path);
            }

            using (var pen = new Pen(Color.White, 2) { DashStyle = DashStyle.Dash })
                g.DrawEllipse(pen, _selection);

            foreach (var pt in new[]
            {
                new Point(_selection.Left,  _selection.Top),
                new Point(_selection.Right, _selection.Top),
                new Point(_selection.Left,  _selection.Bottom),
                new Point(_selection.Right, _selection.Bottom)
            })
            {
                var r = new Rectangle(pt.X - HandleSize / 2, pt.Y - HandleSize / 2, HandleSize, HandleSize);
                g.FillRectangle(Brushes.White, r);
                g.DrawRectangle(Pens.Gray, r);
            }
        }

        private void PicCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (ResizeHandleRect.Contains(e.Location))
                _resizing = true;
            else if (IsInsideCircle(e.Location))
                _dragging = true;
            _dragStart = e.Location;
        }

        private void PicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
            {
                int delta   = Math.Max(e.Location.X - _dragStart.X, e.Location.Y - _dragStart.Y);
                int newSize = Math.Max(30, _selection.Width + delta);
                newSize     = Math.Min(newSize, picCanvas.Width  - _selection.Left);
                newSize     = Math.Min(newSize, picCanvas.Height - _selection.Top);
                _selection  = new Rectangle(_selection.Left, _selection.Top, newSize, newSize);
                _dragStart  = e.Location;
                picCanvas.Invalidate();
            }
            else if (_dragging)
            {
                int newX   = Clamp(_selection.X + e.Location.X - _dragStart.X, 0, picCanvas.Width  - _selection.Width);
                int newY   = Clamp(_selection.Y + e.Location.Y - _dragStart.Y, 0, picCanvas.Height - _selection.Height);
                _selection = new Rectangle(newX, newY, _selection.Width, _selection.Height);
                _dragStart = e.Location;
                picCanvas.Invalidate();
            }
            else
            {
                picCanvas.Cursor = ResizeHandleRect.Contains(e.Location) ? Cursors.SizeNWSE
                    : (IsInsideCircle(e.Location) ? Cursors.SizeAll : Cursors.Default);
            }
        }

        private void PicCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
            _resizing = false;
        }

        private bool IsInsideCircle(Point p)
        {
            float cx = _selection.X + _selection.Width / 2f;
            float cy = _selection.Y + _selection.Height / 2f;
            float r  = _selection.Width / 2f;
            float dx = p.X - cx, dy = p.Y - cy;
            return (dx * dx + dy * dy) <= (r * r);
        }

        private static int Clamp(int v, int min, int max) => Math.Max(min, Math.Min(max, v));

        private void BtnCrop_Click(object sender, EventArgs e)
        {
            double scaleX = (double)_sourceImage.Width  / picCanvas.Width;
            double scaleY = (double)_sourceImage.Height / picCanvas.Height;

            var srcRect = new Rectangle(
                (int)(_selection.X     * scaleX),
                (int)(_selection.Y     * scaleY),
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
                    g.DrawImage(_sourceImage,
                        new Rectangle(0, 0, srcRect.Width, srcRect.Height),
                        srcRect, GraphicsUnit.Pixel);
                }
            }

            CroppedImage   = result;
            DialogResult   = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
