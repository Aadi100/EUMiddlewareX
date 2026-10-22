using System;
using System.Drawing;
using System.Windows.Forms;
using smart_attendance_middleware;
using smart_attendance_middleware.Controls;

namespace smart_attendance_middleware.UI
{
    public class LoaderPopupForm : Form
    {
        private readonly CircularLoader _loader;
        private readonly Label _status;
        public string StatusText
        {
            get => _status.Text;
            set { _status.Text = value; _status.Refresh(); }
        }

        public LoaderPopupForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            TopMost = false;
            BackColor = Color.White;
            Width = 240;
            Height = 260;
            Padding = new Padding(0);
            // Rounded corners
            Region = System.Drawing.Region.FromHrgn(
                NativeMethods.CreateRoundRectRgn(0, 0, Width, Height, 26, 26));

            _loader = new CircularLoader
            {
                Location = new Point((ClientSize.Width - 120) / 2, 30)
            };

            // Load app icon as the logo inside the spinner
            try
            {
                string icoPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "favicon.ico");
                if (System.IO.File.Exists(icoPath))
                {
                    using (var ico = new Icon(icoPath))
                        _loader.LogoImage = ico.ToBitmap();
                }
                else
                {
                    using (var ico = Icon.ExtractAssociatedIcon(Application.ExecutablePath))
                        _loader.LogoImage = ico?.ToBitmap();
                }
            }
            catch { }
            _status = new Label
            {
                Text = "Initializing...",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 75),
                TextAlign = ContentAlignment.TopCenter,
                AutoSize = false,
                Width = ClientSize.Width - 40,
                Height = 70,
                Location = new Point(20, 160)
            };

            Controls.Add(_loader);
            Controls.Add(_status);

            Shown += (s, e) => _loader.Start();
            FormClosed += (s, e) => _loader.Stop();

            // Soft drop shadow
            Paint += (s, e) =>
            {
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(new Rectangle(10, Height - 40, Width - 20, 30));
                    using (var brush = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(50, 0, 0, 0);
                        brush.SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) };
                        e.Graphics.FillPath(brush, path);
                    }
                }
            };
        }

        public void SafeClose()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(SafeClose));
                return;
            }
            _loader.Stop();
            Close();
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern IntPtr CreateRoundRectRgn(
                int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
                int nWidthEllipse, int nHeightEllipse);
        }
    }
}
