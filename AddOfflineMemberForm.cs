using AForge.Video;
using AForge.Video.DirectShow;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class AddOfflineMemberForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private Bitmap _croppedPhoto;

        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private Bitmap _latestFrame;
        private bool _cameraActive;

        public AddOfflineMemberForm()
        {
            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();
            cmbGender.Items.AddRange(new object[] { "male", "female", "other" });
            cmbPaymentMethod.Items.AddRange(new object[] { "cash", "card", "bank_transfer", "online" });
            cmbPaymentMethod.SelectedIndex = 0;
            LoadPackages();
        }

        // ── Paint handlers ───────────────────────────────────────────────────

        private void pnlPhoto_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(220, 224, 238)))
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, pnlPhoto.Width - 1, pnlPhoto.Height - 1));
        }

        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            if (picPreview.Image != null) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var br = new SolidBrush(Color.FromArgb(190, 196, 214)))
            {
                int cx = picPreview.Width / 2, cy = picPreview.Height / 2;
                e.Graphics.FillEllipse(br, cx - 34, cy - 58, 68, 68);
                e.Graphics.FillEllipse(br, cx - 56, cy + 16, 112, 76);
            }
        }

        private void pnlInfoCard_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(220, 224, 238)))
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, pnlInfoCard.Width - 1, pnlInfoCard.Height - 1));
            using (var br = new SolidBrush(Color.FromArgb(59, 130, 246)))
                e.Graphics.FillRectangle(br, 0, 0, 4, pnlInfoCard.Height);
        }

        private void pnlSubCard_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(220, 224, 238)))
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, pnlSubCard.Width - 1, pnlSubCard.Height - 1));
            using (var br = new SolidBrush(Color.FromArgb(16, 185, 129)))
                e.Graphics.FillRectangle(br, 0, 0, 4, pnlSubCard.Height);
        }

        private void pnlSummary_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(220, 224, 238)))
            {
                e.Graphics.DrawLine(pen, 0, 0, pnlSummary.Width, 0);
                e.Graphics.DrawLine(pen, 0, pnlSummary.Height - 1, pnlSummary.Width, pnlSummary.Height - 1);
            }
        }

        // ── Summary bar ──────────────────────────────────────────────────────

        private void cmbPeriodicity_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterPackagesByPeriodicity();
        }

        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            try
            {
                var pkg = cmbPackage.SelectedItem as Package;
                string periodicity = cmbPeriodicity.SelectedItem?.ToString();

                if (pkg == null || string.IsNullOrWhiteSpace(periodicity))
                {
                    lblAmountDue.Text      = "—";
                    lblMembershipRuns.Text = "—";
                    return;
                }

                string amountText = "—";
                if (!string.IsNullOrWhiteSpace(pkg.PricingTierJson))
                {
                    try
                    {
                        var tiers = JObject.Parse(pkg.PricingTierJson);
                        if (tiers[periodicity] != null)
                            amountText = "Rs " + string.Format("{0:N0}", (double)tiers[periodicity]);
                    }
                    catch { }
                }
                lblAmountDue.Text = amountText;

                DateTime start = dtpStartDate.Value.Date;
                DateTime end   = ComputeEndDate(start, periodicity);
                lblMembershipRuns.Text = start.ToString("dd MMM yyyy") + "  →  " + end.ToString("dd MMM yyyy");
            }
            catch { }
        }

        private static DateTime ComputeEndDate(DateTime start, string periodicity)
        {
            if (string.IsNullOrWhiteSpace(periodicity)) return start;
            string[] parts = periodicity.Split('-');
            if (parts.Length < 2 || !int.TryParse(parts[0], out int n)) return start;
            string unit = parts[1].ToLowerInvariant();
            if (unit.StartsWith("day"))   return start.AddDays(n);
            if (unit.StartsWith("month")) return start.AddMonths(n);
            if (unit.StartsWith("year"))  return start.AddYears(n);
            if (unit.StartsWith("week"))  return start.AddDays(n * 7);
            return start;
        }

        // ── Package / periodicity loading ────────────────────────────────────

        private System.Collections.Generic.List<Package> _allPackages;

        private void LoadPackages()
        {
            try
            {
                _allPackages = _dbHelper.GetPackages(activeOnly: true);

                // Collect all unique periodicity keys across all packages
                var periodicities = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var pkg in _allPackages)
                {
                    if (string.IsNullOrWhiteSpace(pkg.PricingTierJson)) continue;
                    try
                    {
                        var tiers = JObject.Parse(pkg.PricingTierJson);
                        foreach (var prop in tiers.Properties())
                            periodicities.Add(prop.Name);
                    }
                    catch { }
                }

                cmbPeriodicity.Items.Clear();
                foreach (var p in periodicities)
                    cmbPeriodicity.Items.Add(p);

                if (cmbPeriodicity.Items.Count > 0)
                    cmbPeriodicity.SelectedIndex = 0;  // triggers FilterPackagesByPeriodicity
                else if (_allPackages.Count == 0)
                    lblStatus.Text = "No packages found locally — wait for the catalog sync to complete.";
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Failed to load packages: {ex.Message}");
                lblStatus.Text = "Failed to load packages: " + ex.Message;
            }
        }

        private void FilterPackagesByPeriodicity()
        {
            string selected = cmbPeriodicity.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected) || _allPackages == null) { UpdateSummary(); return; }

            var matching = _allPackages.FindAll(pkg =>
            {
                if (string.IsNullOrWhiteSpace(pkg.PricingTierJson)) return false;
                try { return JObject.Parse(pkg.PricingTierJson)[selected] != null; }
                catch { return false; }
            });

            cmbPackage.DisplayMember = "Title";
            cmbPackage.ValueMember   = "PackageId";
            cmbPackage.DataSource    = matching;

            if (matching.Count == 0)
                lblStatus.Text = $"No packages offer '{selected}'.";
            else
                lblStatus.Text = "";

            UpdateSummary();
        }

        private void CmbPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSummary();
        }

        // ── Camera ───────────────────────────────────────────────────────────

        private void BtnTakePicture_Click(object sender, EventArgs e)
        {
            if (!_cameraActive) StartCameraPreview();
            else CaptureFrameAndCrop();
        }

        private void StartCameraPreview()
        {
            lblStatus.Text = string.Empty;
            try
            {
                if (_videoDevices == null)
                    _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (_videoDevices.Count == 0) { lblStatus.Text = "No camera found on this device."; return; }

                if (cmbCameraDevice.Items.Count == 0)
                    foreach (FilterInfo device in _videoDevices)
                        cmbCameraDevice.Items.Add(device.Name);

                cmbCameraDevice.Visible = _videoDevices.Count > 1;
                if (cmbCameraDevice.SelectedIndex < 0) cmbCameraDevice.SelectedIndex = 0;

                // Always open explicitly — don't rely on SelectedIndexChanged which
                // is suppressed when Visible=false and _cameraActive=false (single camera).
                OpenSelectedCamera();

                _cameraActive = true;
                btnTakePicture.Text = "Capture photo";
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Camera start error: {ex.Message}");
                lblStatus.Text = "Failed to start camera: " + ex.Message;
            }
        }

        private void CmbCameraDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Only switch camera if already in active/visible state (user changed device).
            // On first launch StartCameraPreview() calls OpenSelectedCamera() directly.
            if (_cameraActive) OpenSelectedCamera();
        }

        private void OpenSelectedCamera()
        {
            StopCamera(keepUiState: true);
            int index = cmbCameraDevice.SelectedIndex;
            if (index < 0 || _videoDevices == null || index >= _videoDevices.Count) return;
            try
            {
                _videoSource = new VideoCaptureDevice(_videoDevices[index].MonikerString);
                _videoSource.NewFrame += VideoSource_NewFrame;
                _videoSource.Start();
                _cameraActive = true;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Camera open error: {ex.Message}");
                lblStatus.Text = "Failed to open camera: " + ex.Message;
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                var frame = (Bitmap)eventArgs.Frame.Clone();
                var old = _latestFrame;
                _latestFrame = frame;
                if (IsHandleCreated && !IsDisposed)
                    BeginInvoke((Action)(() => {
                        var prev = picPreview.Image;
                        picPreview.Image = (Bitmap)frame.Clone();
                        prev?.Dispose();
                    }));
                old?.Dispose();
            }
            catch (Exception ex) { LogHelper.Log($"[AddOfflineMemberForm] Camera frame error: {ex.Message}"); }
        }

        private void CaptureFrameAndCrop()
        {
            if (_latestFrame == null) { lblStatus.Text = "No frame available yet — please wait for the preview to start."; return; }
            using (var raw = (Bitmap)_latestFrame.Clone()) { StopCamera(); ShowCropDialog(raw); }
        }

        private void StopCamera(bool keepUiState = false)
        {
            try
            {
                var src = _videoSource;
                _videoSource = null;
                if (src != null && src.IsRunning)
                {
                    src.NewFrame -= VideoSource_NewFrame;
                    src.SignalToStop();
                    // WaitForStop() blocks the UI thread — run it on a background thread.
                    System.Threading.Tasks.Task.Run(() => { try { src.WaitForStop(); } catch { } });
                }
            }
            catch (Exception ex) { LogHelper.Log($"[AddOfflineMemberForm] Camera stop error: {ex.Message}"); }

            if (!keepUiState)
            {
                _cameraActive = false;
                cmbCameraDevice.Visible = false;
                btnTakePicture.Text = "Take picture";
            }
        }

        // ── Photo browse / crop ──────────────────────────────────────────────

        private void BtnBrowsePicture_Click(object sender, EventArgs e)
        {
            StopCamera();
            using (var ofd = new OpenFileDialog {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title  = "Select a photo" })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                try { using (var raw = new Bitmap(ofd.FileName)) ShowCropDialog(raw); }
                catch (Exception ex)
                {
                    LogHelper.Log($"[AddOfflineMemberForm] Failed to load image: {ex.Message}");
                    lblStatus.Text = "Failed to load image: " + ex.Message;
                }
            }
        }

        private void ShowCropDialog(Bitmap source)
        {
            using (var cropForm = new CropForm(source))
            {
                if (cropForm.ShowDialog(this) != DialogResult.OK || cropForm.CroppedImage == null) return;
                _croppedPhoto?.Dispose();
                _croppedPhoto = cropForm.CroppedImage;
                picPreview.Image?.Dispose();
                picPreview.Image = (Bitmap)_croppedPhoto.Clone();
            }
        }

        private void BtnSameAsPhone_Click(object sender, EventArgs e)
        {
            txtWhatsapp.Text = txtPhone.Text;
        }

        // ── Save ─────────────────────────────────────────────────────────────

        private void btnCancel_Click(object sender, EventArgs e)
        {
            StopCamera();
            Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;

            string firstName = txtFirstName.Text.Trim();
            string lastName  = txtLastName.Text.Trim();
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                lblStatus.Text = "First name and last name are required.";
                return;
            }

            bool hasPhoto = _croppedPhoto != null;
            bool hasRfid  = !string.IsNullOrWhiteSpace(txtRfid.Text);
            if (!hasPhoto && !hasRfid)
            {
                lblStatus.Text = "At least a photo or RFID card is required.";
                return;
            }

            var selectedPackage = cmbPackage.SelectedItem as Package;
            if (selectedPackage == null) { lblStatus.Text = "Please select a package."; return; }
            if (cmbPeriodicity.SelectedItem == null) { lblStatus.Text = "Please select a periodicity for the selected package."; return; }

            btnSave.Enabled   = false;
            btnCancel.Enabled = false;

            try
            {
                var subscription = new JObject
                {
                    ["package_id"]           = selectedPackage.PackageId,
                    ["periodicity"]          = cmbPeriodicity.SelectedItem.ToString(),
                    ["start_date"]           = dtpStartDate.Value.ToString("yyyy-MM-dd"),
                    ["payment_collected_at"] = dtpPaymentCollectedAt.Value.ToString("yyyy-MM-dd"),
                    ["payment_method"]       = cmbPaymentMethod.SelectedItem?.ToString() ?? "cash"
                };

                var created = _dbHelper.InsertOfflineMember(
                    firstName, lastName,
                    cmbGender.SelectedItem?.ToString(),
                    txtPhone.Text.Trim(),
                    subscription,
                    picture: null,
                    rfid: string.IsNullOrWhiteSpace(txtRfid.Text) ? null : txtRfid.Text.Trim(),
                    fingerprint: false,
                    email: string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                    whatsapp: string.IsNullOrWhiteSpace(txtWhatsapp.Text) ? null : txtWhatsapp.Text.Trim());

                if (_croppedPhoto != null)
                {
                    string picturePath = SavePhotoForEnrollment(created.EnrollmentID);
                    if (picturePath != null)
                        _dbHelper.UpdateOfflineMemberPicture(created.ClientId, picturePath);
                }

                _ = Task.Run(async () => {
                    try
                    {
                        var svc = ServiceManager.OfflineEnrollmentSubscriptionService;
                        if (svc != null) await svc.PushOfflineEntriesToDevicesAsync();
                        else await new DeviceFunctions().ProcessAllOfflineEntries();
                    }
                    catch (Exception ex) { LogHelper.Log($"[AddOfflineMemberForm] ProcessAllOfflineEntries failed: {ex.Message}"); }
                });

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Failed to save offline member: {ex.Message}");
                lblStatus.ForeColor = Color.Firebrick;
                lblStatus.Text = "Failed to save: " + ex.Message;
            }
            finally
            {
                btnSave.Enabled   = true;
                btnCancel.Enabled = true;
            }
        }

        // ── Photo encoding ───────────────────────────────────────────────────

        private const long MaxFaceFileBytes = 1000 * 1024;
        private const int MaxFaceDimension  = 480;

        private string SavePhotoForEnrollment(int enrollmentId)
        {
            try
            {
                string faceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "offlinefaces");
                Directory.CreateDirectory(faceDir);
                string path = Path.Combine(faceDir, enrollmentId + ".jpg");
                byte[] jpegBytes = EncodeFaceJpegUnderLimit(_croppedPhoto, MaxFaceDimension, MaxFaceFileBytes);
                File.WriteAllBytes(path, jpegBytes);
                LogHelper.Log($"[AddOfflineMemberForm] Saved face photo for EnrollmentID {enrollmentId}: {jpegBytes.Length / 1024}KB at {path}");
                return path;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Failed to save photo for EnrollmentID {enrollmentId}: {ex.Message}");
                return null;
            }
        }

        private static byte[] EncodeFaceJpegUnderLimit(Bitmap source, int maxDimension, long maxBytes)
        {
            var jpegCodec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            byte[] lastAttempt = null;

            for (int pass = 0; pass < 6; pass++)
            {
                double scale = Math.Min(1.0, (double)maxDimension / Math.Max(source.Width, source.Height));
                int targetW  = Math.Max(1, (int)Math.Round(source.Width  * scale));
                int targetH  = Math.Max(1, (int)Math.Round(source.Height * scale));

                using (var flattened = new Bitmap(targetW, targetH, PixelFormat.Format24bppRgb))
                {
                    using (var g = Graphics.FromImage(flattened))
                    {
                        g.Clear(Color.White);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(source, new Rectangle(0, 0, targetW, targetH));
                    }
                    for (int quality = 90; quality >= 30; quality -= 15)
                    {
                        using (var encoderParams = new EncoderParameters(1))
                        using (var ms = new MemoryStream())
                        {
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                            flattened.Save(ms, jpegCodec, encoderParams);
                            lastAttempt = ms.ToArray();
                            if (lastAttempt.Length <= maxBytes) return lastAttempt;
                        }
                    }
                }
                maxDimension = (int)(maxDimension * 0.75);
            }
            return lastAttempt;
        }
    }
}
