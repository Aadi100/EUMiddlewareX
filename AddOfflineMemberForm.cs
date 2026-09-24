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
    /// <summary>
    /// Lets front-desk staff enroll a member while offline. The subscription section is
    /// populated from the locally-synced Packages catalog (see OfflineEnrollmentSubscriptionService).
    /// Saves into dbo.OfflineMembers with a decrementing EnrollmentID (starts at 999998).
    /// The face photo (captured from a webcam or browsed from disk, then circle-cropped)
    /// is saved to <AppBaseDir>\offlinefaces\<EnrollmentID>.png.
    /// </summary>
    public class AddOfflineMemberForm : Form
    {
        private static readonly Color BrandNavy = Color.FromArgb(18, 23, 49);
        private static readonly Color PanelBack = Color.WhiteSmoke;

        private readonly DatabaseHelper _dbHelper;
        private Bitmap _croppedPhoto;

        // Inline camera preview state
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private Bitmap _latestFrame;
        private bool _cameraActive;

        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtPhone;
        private TextBox txtRfid;
        private ComboBox cmbGender;
        private ComboBox cmbPackage;
        private ComboBox cmbPeriodicity;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpPaymentCollectedAt;
        private ComboBox cmbPaymentMethod;
        private PictureBox picPreview;
        private ComboBox cmbCameraDevice;
        private Button btnTakePicture;
        private Button btnBrowsePicture;
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;

        public AddOfflineMemberForm()
        {
            _dbHelper = new DatabaseHelper();
            _dbHelper.InitializeDatabase();

            BuildUi();
            LoadPackages();
        }

        private void BuildUi()
        {
            Text = "Add Offline Member";
            Width = 660;
            Height = 660;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = PanelBack;

            // ---- Header bar (matches the app's dark-navy brand chrome) ----
            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = BrandNavy };
            var lblTitle = new Label
            {
                Text = "Add Offline Member",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 14)
            };
            header.Controls.Add(lblTitle);
            Controls.Add(header);

            // ---- Photo panel (left) — live camera preview shows inline, no popup ----
            var photoPanel = new Panel
            {
                Location = new Point(16, 64),
                Size = new Size(200, 300),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(photoPanel);

            picPreview = new PictureBox
            {
                Location = new Point(8, 8),
                Size = new Size(184, 184),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Gainsboro,
                BorderStyle = BorderStyle.FixedSingle
            };
            photoPanel.Controls.Add(picPreview);

            cmbCameraDevice = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(8, 198),
                Width = 184,
                Visible = false
            };
            cmbCameraDevice.SelectedIndexChanged += CmbCameraDevice_SelectedIndexChanged;
            photoPanel.Controls.Add(cmbCameraDevice);

            btnTakePicture = new Button { Text = "Take Picture", Location = new Point(8, 228), Width = 184 };
            btnTakePicture.Click += BtnTakePicture_Click;
            photoPanel.Controls.Add(btnTakePicture);

            btnBrowsePicture = new Button { Text = "Browse...", Location = new Point(8, 258), Width = 184 };
            btnBrowsePicture.Click += BtnBrowsePicture_Click;
            photoPanel.Controls.Add(btnBrowsePicture);

            // ---- Fields panel (right) ----
            var layout = new TableLayoutPanel
            {
                Location = new Point(228, 64),
                Size = new Size(410, 420),
                ColumnCount = 2,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            void AddRow(string label, Control control)
            {
                int row = layout.RowCount;
                layout.RowCount++;
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) }, 0, row);
                control.Dock = DockStyle.Fill;
                control.Margin = new Padding(3, 4, 3, 4);
                layout.Controls.Add(control, 1, row);
            }

            txtFirstName = new TextBox();
            AddRow("First Name*", txtFirstName);

            txtLastName = new TextBox();
            AddRow("Last Name*", txtLastName);

            cmbGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            AddRow("Gender", cmbGender);

            txtPhone = new TextBox();
            AddRow("Phone", txtPhone);

            txtRfid = new TextBox();
            AddRow("RFID", txtRfid);

            cmbPackage = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPackage.SelectedIndexChanged += CmbPackage_SelectedIndexChanged;
            AddRow("Package*", cmbPackage);

            cmbPeriodicity = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddRow("Periodicity*", cmbPeriodicity);

            dtpStartDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            AddRow("Start Date", dtpStartDate);

            dtpPaymentCollectedAt = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            AddRow("Payment Collected At", dtpPaymentCollectedAt);

            cmbPaymentMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPaymentMethod.Items.AddRange(new object[] { "cash", "card", "bank_transfer", "online" });
            cmbPaymentMethod.SelectedIndex = 0;
            AddRow("Payment Method", cmbPaymentMethod);

            lblStatus = new Label
            {
                AutoSize = false,
                Location = new Point(16, 520),
                Size = new Size(622, 40),
                ForeColor = Color.Firebrick,
                TextAlign = ContentAlignment.TopLeft
            };
            Controls.Add(lblStatus);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 56,
                Padding = new Padding(16)
            };

            btnSave = new Button { Text = "Save", Width = 100, BackColor = BrandNavy, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Cancel", Width = 100 };
            btnCancel.Click += (s, e) => Close();

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);
            Controls.Add(buttonPanel);

            FormClosed += (s, e) => StopCamera();
        }

        private void LoadPackages()
        {
            try
            {
                var packages = _dbHelper.GetPackages(activeOnly: true);
                cmbPackage.DisplayMember = "Title";
                cmbPackage.ValueMember = "PackageId";
                cmbPackage.DataSource = packages;

                if (packages.Count == 0)
                {
                    lblStatus.Text = "No packages found locally — wait for the catalog sync to complete, or check your connection.";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Failed to load packages: {ex.Message}");
                lblStatus.Text = "Failed to load packages: " + ex.Message;
            }
        }

        private void CmbPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbPeriodicity.Items.Clear();

            var selected = cmbPackage.SelectedItem as Package;
            if (selected == null || string.IsNullOrWhiteSpace(selected.PricingTierJson))
                return;

            try
            {
                var pricingTier = JObject.Parse(selected.PricingTierJson);
                foreach (var prop in pricingTier.Properties())
                {
                    cmbPeriodicity.Items.Add(prop.Name);
                }
                if (cmbPeriodicity.Items.Count > 0)
                    cmbPeriodicity.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Failed to parse pricing tier: {ex.Message}");
            }
        }

        private void BtnTakePicture_Click(object sender, EventArgs e)
        {
            if (!_cameraActive)
            {
                StartCameraPreview();
            }
            else
            {
                CaptureFrameAndCrop();
            }
        }

        private void StartCameraPreview()
        {
            lblStatus.Text = string.Empty;
            try
            {
                if (_videoDevices == null)
                    _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (_videoDevices.Count == 0)
                {
                    lblStatus.Text = "No camera found on this device.";
                    return;
                }

                if (cmbCameraDevice.Items.Count == 0)
                {
                    foreach (FilterInfo device in _videoDevices)
                        cmbCameraDevice.Items.Add(device.Name);
                }
                cmbCameraDevice.Visible = _videoDevices.Count > 1;
                if (cmbCameraDevice.SelectedIndex < 0)
                    cmbCameraDevice.SelectedIndex = 0;
                else
                    OpenSelectedCamera();

                _cameraActive = true;
                btnTakePicture.Text = "Capture Photo";
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Camera start error: {ex.Message}");
                lblStatus.Text = "Failed to start camera: " + ex.Message;
            }
        }

        private void CmbCameraDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cameraActive || cmbCameraDevice.Visible)
                OpenSelectedCamera();
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
                {
                    BeginInvoke((Action)(() =>
                    {
                        var previousImage = picPreview.Image;
                        picPreview.Image = (Bitmap)frame.Clone();
                        previousImage?.Dispose();
                    }));
                }

                old?.Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Camera frame error: {ex.Message}");
            }
        }

        private void CaptureFrameAndCrop()
        {
            if (_latestFrame == null)
            {
                lblStatus.Text = "No frame available yet — please wait for the preview to start.";
                return;
            }

            using (var raw = (Bitmap)_latestFrame.Clone())
            {
                StopCamera();
                ShowCropDialog(raw);
            }
        }

        private void StopCamera(bool keepUiState = false)
        {
            try
            {
                if (_videoSource != null && _videoSource.IsRunning)
                {
                    _videoSource.NewFrame -= VideoSource_NewFrame;
                    _videoSource.SignalToStop();
                    _videoSource.WaitForStop();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AddOfflineMemberForm] Camera stop error: {ex.Message}");
            }
            finally
            {
                _videoSource = null;
            }

            if (!keepUiState)
            {
                _cameraActive = false;
                cmbCameraDevice.Visible = false;
                btnTakePicture.Text = "Take Picture";
            }
        }

        private void BtnBrowsePicture_Click(object sender, EventArgs e)
        {
            StopCamera();

            using (var ofd = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select a photo"
            })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    using (var raw = new Bitmap(ofd.FileName))
                    {
                        ShowCropDialog(raw);
                    }
                }
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
                if (cropForm.ShowDialog(this) != DialogResult.OK || cropForm.CroppedImage == null)
                    return;

                _croppedPhoto?.Dispose();
                _croppedPhoto = cropForm.CroppedImage;

                picPreview.Image?.Dispose();
                picPreview.Image = (Bitmap)_croppedPhoto.Clone();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;

            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                lblStatus.Text = "First name and last name are required.";
                return;
            }

            var selectedPackage = cmbPackage.SelectedItem as Package;
            if (selectedPackage == null)
            {
                lblStatus.Text = "Please select a package.";
                return;
            }

            if (cmbPeriodicity.SelectedItem == null)
            {
                lblStatus.Text = "Please select a periodicity for the selected package.";
                return;
            }

            btnSave.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                var subscription = new JObject
                {
                    ["package_id"] = selectedPackage.PackageId,
                    ["periodicity"] = cmbPeriodicity.SelectedItem.ToString(),
                    ["start_date"] = dtpStartDate.Value.ToString("yyyy-MM-dd"),
                    ["payment_collected_at"] = dtpPaymentCollectedAt.Value.ToString("yyyy-MM-dd"),
                    ["payment_method"] = cmbPaymentMethod.SelectedItem?.ToString() ?? "cash"
                };

                // Creates the OfflineMembers row AND one OfflineMemberLog row per (member) device.
                var created = _dbHelper.InsertOfflineMember(
                    firstName,
                    lastName,
                    cmbGender.SelectedItem?.ToString(),
                    txtPhone.Text.Trim(),
                    subscription,
                    picture: null,
                    rfid: string.IsNullOrWhiteSpace(txtRfid.Text) ? null : txtRfid.Text.Trim(),
                    fingerprint: false
                );

                // The face file is named after the EnrollmentID, which is only known once the
                // row exists — save it now and patch the Picture column + OfflineMemberLog rows.
                if (_croppedPhoto != null)
                {
                    string picturePath = SavePhotoForEnrollment(created.EnrollmentID);
                    if (picturePath != null)
                    {
                        _dbHelper.UpdateOfflineMemberPicture(created.ClientId, picturePath);
                    }
                }

                // Push the freshly-created logs (RFID/Face/Fingerprint) out to every device in the
                // background — the form closes immediately rather than waiting on device I/O.
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var deviceFunctions = new DeviceFunctions();
                        await deviceFunctions.ProcessAllOfflineEntries();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[AddOfflineMemberForm] ProcessAllOfflineEntries failed: {ex.Message}");
                    }
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
                btnSave.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        // Devices reject face photos over a small size cap ("file is too big"). The circular
        // crop is saved as an opaque JPEG (flattened onto white — devices don't need the alpha
        // mask), downscaled and quality-reduced as needed to stay at or under 1000 KB.
        private const long MaxFaceFileBytes = 1000 * 1024;
        private const int MaxFaceDimension = 480;

        private string SavePhotoForEnrollment(int enrollmentId)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string faceDir = Path.Combine(baseDir, "offlinefaces");
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

        /// <summary>
        /// Flattens a (possibly transparent) source bitmap onto white, downscales it to fit
        /// within maxDimension, then encodes as JPEG — stepping quality down and, if that alone
        /// isn't enough, shrinking dimensions further — until the result is at or under maxBytes.
        /// </summary>
        private static byte[] EncodeFaceJpegUnderLimit(Bitmap source, int maxDimension, long maxBytes)
        {
            var jpegCodec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            int width = source.Width;
            int height = source.Height;

            byte[] lastAttempt = null;

            for (int pass = 0; pass < 6; pass++)
            {
                double scale = Math.Min(1.0, (double)maxDimension / Math.Max(width, height));
                int targetW = Math.Max(1, (int)Math.Round(source.Width * scale));
                int targetH = Math.Max(1, (int)Math.Round(source.Height * scale));

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

                            if (lastAttempt.Length <= maxBytes)
                                return lastAttempt;
                        }
                    }
                }

                // Still too big even at the lowest quality — shrink the target dimension and retry.
                maxDimension = (int)(maxDimension * 0.75);
            }

            // Best effort after all passes; caller still gets a JPEG well below the original PNG size.
            return lastAttempt;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopCamera();
                _latestFrame?.Dispose();
                _croppedPhoto?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
