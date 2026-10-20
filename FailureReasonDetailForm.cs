using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    /// <summary>
    /// Shows the full sync failure reason with side-by-side member photos when
    /// the reason is a face duplicate. Images resolved from FACE/ folder first,
    /// then from the Picture URL in the DB.
    /// </summary>
    public partial class FailureReasonDetailForm : Form
    {
        // ── theme ──────────────────────────────────────────────────────────────
        private static readonly Color NavyDark   = Color.FromArgb(18, 23, 49);
        private static readonly Color NavyMid    = Color.FromArgb(30, 38, 80);
        private static readonly Color AccentRed  = Color.FromArgb(210, 45, 45);
        private static readonly Color AccentBlue = Color.FromArgb(37, 99, 235);
        private static readonly Color BgLight    = Color.FromArgb(248, 249, 252);
        private static readonly Color BorderClr  = Color.FromArgb(220, 224, 235);

        // ── inputs ─────────────────────────────────────────────────────────────
        private readonly string _failureReason;
        private readonly int?   _currentMemberId;
        private readonly int?   _currentEnrollmentId;
        private readonly string _currentName;
        private readonly string _currentPictureUrl;
        private readonly DatabaseHelper _dbHelper;

        // ── parsed conflict ────────────────────────────────────────────────────
        private bool   _isFaceConflict;
        private int    _conflictingEnrollId;
        private string _conflictingName;
        private string _conflictingPictureUrl;
        private int?   _conflictingMemberId;
        private string _errorCode;
        private string _errorMessage;

        // ── photo boxes ────────────────────────────────────────────────────────
        private RoundPictureBox _picCurrent;
        private RoundPictureBox _picConflict;

        // ── overloads ──────────────────────────────────────────────────────────

        public FailureReasonDetailForm(string failureReason, Member currentMember, DatabaseHelper dbHelper)
            : this(failureReason,
                   currentMember?.MemberID,
                   currentMember?.EnrollmentID,
                   currentMember?.Name,
                   currentMember?.Picture,
                   dbHelper) { }

        public FailureReasonDetailForm(
            string failureReason,
            int?   currentMemberId,
            int?   currentEnrollmentId,
            string currentName,
            string currentPictureUrl,
            DatabaseHelper dbHelper)
        {
            _failureReason       = failureReason ?? string.Empty;
            _currentMemberId     = currentMemberId;
            _currentEnrollmentId = currentEnrollmentId;
            _currentName         = currentName;
            _currentPictureUrl   = currentPictureUrl;
            _dbHelper            = dbHelper;

            ParseConflict();
            BuildUi();
        }

        // ── parsing ────────────────────────────────────────────────────────────

        private void ParseConflict()
        {
            _isFaceConflict =
                _failureReason.IndexOf("face is double",   StringComparison.OrdinalIgnoreCase) >= 0 ||
                _failureReason.IndexOf("face sync failed", StringComparison.OrdinalIgnoreCase) >= 0;

            // Extract Code: XX
            var codeMatch = Regex.Match(_failureReason, @"Code[:\s]+(\d+)", RegexOptions.IgnoreCase);
            _errorCode = codeMatch.Success ? codeMatch.Groups[1].Value : null;

            // Extract raw device message (after "failed:" or before "|")
            var msgMatch = Regex.Match(_failureReason, @"failed:\s*(.+?)(?:\s*\(Code|\|)", RegexOptions.IgnoreCase);
            _errorMessage = msgMatch.Success ? msgMatch.Groups[1].Value.Trim() : null;

            if (!_isFaceConflict) return;

            // Extract conflicting enrollment id from "id=XXXX"
            var idMatch = Regex.Match(_failureReason, @"\bid=(\d+)");
            if (!idMatch.Success || !int.TryParse(idMatch.Groups[1].Value, out int conflictId)) return;
            _conflictingEnrollId = conflictId;

            try
            {
                var memberId = _dbHelper.GetMemberIdByEnrollment(conflictId);
                if (memberId.HasValue)
                {
                    _conflictingMemberId = memberId.Value;
                    var m = _dbHelper.GetMember(memberId.Value);
                    if (m != null)
                    {
                        _conflictingName       = m.Name;
                        _conflictingPictureUrl = m.Picture;
                    }
                }
            }
            catch { }

            // Fallback: parse name from "Conflicts with: Name"
            if (string.IsNullOrWhiteSpace(_conflictingName))
            {
                var nameMatch = Regex.Match(_failureReason, @"Conflicts with:\s*(.+?)(?:\s*\(|$)", RegexOptions.IgnoreCase);
                if (nameMatch.Success)
                    _conflictingName = nameMatch.Groups[1].Value.Trim();
            }
        }

        // ── UI ─────────────────────────────────────────────────────────────────

        private void BuildUi()
        {
            const int W = 640;

            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }

            // Set dynamic header text
            string icon  = _isFaceConflict ? "⚠" : "✕";
            string title = _isFaceConflict ? "Face Duplicate Detected" : "Sync Failure Detail";
            lblHeaderTitle.Text = $"  {icon}   {title}";

            // Populate reason text box
            rtbReason.Text = _failureReason;

            if (_isFaceConflict)
            {
                // Expand form for photo panels
                ClientSize = new Size(W, 560);

                const int picSize = 180;
                const int panW    = 270;
                const int gap     = (W - panW * 2) / 3;
                int y = 70;

                // VS label
                Controls.Add(new Label
                {
                    Text      = "VS",
                    Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(160, 165, 190),
                    Size      = new Size(36, 36),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point((W - 36) / 2, y + (picSize + 60) / 2 - 18)
                });

                Controls.Add(MakePhotoPanel(
                    x: gap, y: y, w: panW, picSize: picSize,
                    badge: "Current Member", badgeColor: AccentBlue,
                    name: string.IsNullOrWhiteSpace(_currentName) ? "—" : _currentName,
                    enroll: _currentEnrollmentId.HasValue ? $"Enrollment  #{_currentEnrollmentId}" : "—",
                    out _picCurrent));

                Controls.Add(MakePhotoPanel(
                    x: gap * 2 + panW, y: y, w: panW, picSize: picSize,
                    badge: "Conflicts With", badgeColor: AccentRed,
                    name: string.IsNullOrWhiteSpace(_conflictingName) ? "Unknown" : _conflictingName,
                    enroll: _conflictingEnrollId > 0 ? $"Enrollment  #{_conflictingEnrollId}" : "—",
                    out _picConflict));

                y += picSize + 72;

                Controls.Add(new Panel
                {
                    Location  = new Point(24, y),
                    Size      = new Size(W - 48, 1),
                    BackColor = BorderClr
                });
                y += 10;

                // Reposition static controls for face-conflict layout
                lblReasonTitle.Location  = new Point(24, y);
                pnlReasonBorder.Location = new Point(24, y + 22);
                btnClose.Location        = new Point((W - 110) / 2, y + 22 + 82);
            }
            else
            {
                ClientSize = new Size(W, 300);
                btnClose.Location = new Point((W - 110) / 2, 180);
            }

            Load += async (s, ev) =>
            {
                if (_isFaceConflict) await LoadImagesAsync();
            };
        }

        private void BtnClose_Click(object sender, System.EventArgs e) => Close();

        private Panel MakePhotoPanel(int x, int y, int w, int picSize,
                                     string badge, Color badgeColor,
                                     string name, string enroll,
                                     out RoundPictureBox pic)
        {
            var panel = new Panel
            {
                Location  = new Point(x, y),
                Size      = new Size(w, picSize + 62),
                BackColor = Color.White
            };
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderClr, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            // badge pill
            var badgeLbl = new Label
            {
                Text      = badge,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = badgeColor,
                AutoSize  = false,
                Size      = new Size(w - 20, 22),
                Location  = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.Controls.Add(badgeLbl);

            // circular photo
            pic = new RoundPictureBox
            {
                Location  = new Point((w - picSize) / 2, 38),
                Size      = new Size(picSize, picSize),
                SizeMode  = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(238, 240, 248)
            };
            panel.Controls.Add(pic);

            // name
            panel.Controls.Add(new Label
            {
                Text      = name,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = NavyDark,
                Location  = new Point(6, 38 + picSize + 6),
                AutoSize  = false,
                Size      = new Size(w - 12, 20),
                TextAlign = ContentAlignment.MiddleCenter
            });

            // enrollment
            panel.Controls.Add(new Label
            {
                Text      = enroll,
                Font      = new Font("Segoe UI", 8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(110, 115, 140),
                Location  = new Point(6, 38 + picSize + 26),
                AutoSize  = false,
                Size      = new Size(w - 12, 18),
                TextAlign = ContentAlignment.MiddleCenter
            });

            return panel;
        }

        // ── image loading ──────────────────────────────────────────────────────

        private async Task LoadImagesAsync()
        {
            if (_picCurrent != null)
            {
                var img = await Task.Run(() => ResolveImage(_currentMemberId, _currentEnrollmentId, _currentPictureUrl));
                SafeSetImage(_picCurrent, img);
            }
            if (_picConflict != null)
            {
                var img = await Task.Run(() => ResolveImage(_conflictingMemberId,
                    _conflictingEnrollId > 0 ? _conflictingEnrollId : (int?)null,
                    _conflictingPictureUrl));
                SafeSetImage(_picConflict, img);
            }
        }

        private static Image ResolveImage(int? memberId, int? enrollmentId, string pictureUrl)
        {
            string faceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FACE");
            var candidates = new System.Collections.Generic.List<string>();
            if (memberId.HasValue)
            {
                candidates.Add(Path.Combine(faceDir, memberId.Value + ".jpg"));
                candidates.Add(Path.Combine(faceDir, memberId.Value + ".png"));
            }
            if (enrollmentId.HasValue)
            {
                candidates.Add(Path.Combine(faceDir, enrollmentId.Value + ".jpg"));
                candidates.Add(Path.Combine(faceDir, enrollmentId.Value + ".png"));
            }

            foreach (var path in candidates)
            {
                if (!File.Exists(path)) continue;
                try
                {
                    var bytes = File.ReadAllBytes(path);
                    using (var ms = new MemoryStream(bytes))
                        return new Bitmap(ms);
                }
                catch { }
            }

            return TryDownload(pictureUrl);
        }

        private static Image TryDownload(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return null;
            try
            {
                using (var wc = new WebClient())
                using (var ms = new MemoryStream(wc.DownloadData(url)))
                    return new Bitmap(ms);
            }
            catch { return null; }
        }

        private void SafeSetImage(PictureBox box, Image img)
        {
            if (img == null || IsDisposed) return;
            if (box.InvokeRequired) box.Invoke(new Action(() => box.Image = img));
            else box.Image = img;
        }
    }

    // ── helper: circular picture box ──────────────────────────────────────────

    internal class RoundPictureBox : PictureBox
    {
        public RoundPictureBox() { DoubleBuffered = true; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode      = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode  = InterpolationMode.HighQualityBicubic;

            using (var path = RoundedRect(ClientRectangle, 10))
            {
                e.Graphics.SetClip(path);
                base.OnPaint(e);
                e.Graphics.ResetClip();

                // Draw placeholder silhouette when no image
                if (Image == null)
                {
                    e.Graphics.FillPath(new SolidBrush(Color.FromArgb(220, 223, 235)), path);
                    using (var b = new SolidBrush(Color.FromArgb(160, 165, 190)))
                    {
                        var sz = e.Graphics.MeasureString("👤", new Font("Segoe UI", 22f, FontStyle.Bold));
                        e.Graphics.DrawString("👤", new Font("Segoe UI", 22f, FontStyle.Bold), b,
                            (Width - sz.Width) / 2, (Height - sz.Height) / 2);
                    }
                }

                // border
                using (var pen = new Pen(Color.FromArgb(200, 205, 220), 1.5f))
                    e.Graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

