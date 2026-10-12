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
            Width = 780;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 246, 250);

            // ── Header ──────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = BrandNavy };
            header.Controls.Add(new Label
            {
                Text = "Add Offline Member",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 12)
            });
            header.Controls.Add(new Label
            {
                Text = "Enrollment will sync automatically when back online",
                ForeColor = Color.FromArgb(160, 170, 200),
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true,
                Location = new Point(22, 40)
            });
            Controls.Add(header);

            // ── Photo column ─────────────────────────────────────────────────
            var photoCard = new Panel
            {
                Location = new Point(20, 86),
                Size = new Size(210, 540),
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            photoCard.Paint += (s, e) =>
            {
                var r = new System.Drawing.Rectangle(0, 0, photoCard.Width - 1, photoCard.Height - 1);
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 222, 230)), r);
            };
            Controls.Add(photoCard);

            var photoLabel = new Label
            {
                Text = "MEMBER PHOTO",
                ForeColor = Color.FromArgb(130, 136, 160),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 14)
            };
            photoCard.Controls.Add(photoLabel);

            picPreview = new PictureBox
            {
                Location = new Point(12, 34),
                Size = new Size(186, 186),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(240, 241, 246),
                BorderStyle = BorderStyle.None
            };
            picPreview.Paint += (s, e) =>
            {
                if (picPreview.Image == null)
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    var icon = "👤";
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString(icon, new Font("Segoe UI Emoji", 40F), Brushes.Silver,
                        new RectangleF(0, 0, picPreview.Width, picPreview.Height), sf);
                }
            };
            photoCard.Controls.Add(picPreview);

            cmbCameraDevice = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 230),
                Width = 186,
                Visible = false,
                Font = new Font("Segoe UI", 8.5F)
            };
            cmbCameraDevice.SelectedIndexChanged += CmbCameraDevice_SelectedIndexChanged;
            photoCard.Controls.Add(cmbCameraDevice);

            btnTakePicture = new Button
            {
                Text = "📷  Take Picture",
                Location = new Point(12, 256),
                Width = 186,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = BrandNavy,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnTakePicture.FlatAppearance.BorderSize = 0;
            btnTakePicture.Click += BtnTakePicture_Click;
            photoCard.Controls.Add(btnTakePicture);

            btnBrowsePicture = new Button
            {
                Text = "📁  Browse File",
                Location = new Point(12, 298),
                Width = 186,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 241, 246),
                ForeColor = Color.FromArgb(60, 65, 90),
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnBrowsePicture.FlatAppearance.BorderColor = Color.FromArgb(200, 202, 215);
            btnBrowsePicture.FlatAppearance.BorderSize = 1;
            btnBrowsePicture.Click += BtnBrowsePicture_Click;
            photoCard.Controls.Add(btnBrowsePicture);

            // ── Right: two section cards ──────────────────────────────────────
            int cardLeft = 244;
            int cardWidth = 498;

            Panel MakeCard(int top, int height, string sectionTitle, Color accentColor)
            {
                var card = new Panel
                {
                    Location = new Point(cardLeft, top),
                    Size = new Size(cardWidth, height),
                    BackColor = Color.White
                };
                card.Paint += (s, e) =>
                {
                    var r = new System.Drawing.Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 222, 230)), r);
                    e.Graphics.FillRectangle(new SolidBrush(accentColor), 0, 0, 4, card.Height);
                };
                card.Controls.Add(new Label
                {
                    Text = sectionTitle,
                    ForeColor = Color.FromArgb(50, 55, 80),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(16, 12)
                });
                Controls.Add(card);
                return card;
            }

            TableLayoutPanel MakeGrid(Panel card, int top)
            {
                var grid = new TableLayoutPanel
                {
                    Location = new Point(12, top),
                    Size = new Size(card.Width - 24, card.Height - top - 10),
                    ColumnCount = 2,
                    AutoSize = false,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None
                };
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 148));
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                card.Controls.Add(grid);
                return grid;
            }

            void AddRow(TableLayoutPanel grid, string labelText, Control ctrl, bool required = false)
            {
                int row = grid.RowCount++;
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
                var lbl = new Label
                {
                    Text = labelText,
                    ForeColor = Color.FromArgb(90, 96, 120),
                    Font = new Font("Segoe UI", 8.5F),
                    AutoSize = false,
                    Size = new Size(140, 26),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Margin = new Padding(0, 8, 4, 0)
                };
                grid.Controls.Add(lbl, 0, row);
                ctrl.Dock = DockStyle.Fill;
                ctrl.Margin = new Padding(0, 4, 4, 4);
                ctrl.Font = new Font("Segoe UI", 9F);
                grid.Controls.Add(ctrl, 1, row);
            }

            // Section 1 — Member Information
            var infoCard = MakeCard(86, 236, "Member Information", Color.FromArgb(41, 98, 255));
            var infoGrid = MakeGrid(infoCard, 36);
            infoGrid.RowCount = 0;

            txtFirstName = new TextBox();
            AddRow(infoGrid, "First Name *", txtFirstName);
            txtLastName = new TextBox();
            AddRow(infoGrid, "Last Name *", txtLastName);
            cmbGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            AddRow(infoGrid, "Gender", cmbGender);
            txtPhone = new TextBox();
            AddRow(infoGrid, "Phone", txtPhone);
            txtRfid = new TextBox();
            AddRow(infoGrid, "RFID", txtRfid);

            // Section 2 — Subscription & Payment
            var subCard = MakeCard(334, 226, "Subscription & Payment", Color.FromArgb(0, 180, 140));
            var subGrid = MakeGrid(subCard, 36);
            subGrid.RowCount = 0;

            cmbPackage = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPackage.SelectedIndexChanged += CmbPackage_SelectedIndexChanged;
            AddRow(subGrid, "Package *", cmbPackage);
            cmbPeriodicity = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddRow(subGrid, "Periodicity *", cmbPeriodicity);
            dtpStartDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            AddRow(subGrid, "Start Date", dtpStartDate);
            dtpPaymentCollectedAt = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            AddRow(subGrid, "Payment Collected At", dtpPaymentCollectedAt);
            cmbPaymentMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPaymentMethod.Items.AddRange(new object[] { "cash", "card", "bank_transfer", "online" });
            cmbPaymentMethod.SelectedIndex = 0;
            AddRow(subGrid, "Payment Method", cmbPaymentMethod);

            // ── Status label ─────────────────────────────────────────────────
            lblStatus = new Label
            {
                AutoSize = false,
                Location = new Point(20, 574),
                Size = new Size(720, 22),
                ForeColor = Color.Firebrick,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblStatus);

            // ── Bottom button bar ─────────────────────────────────────────────
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.White
            };
            buttonPanel.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 222, 230)), 0, 0, buttonPanel.Width, 0);

            btnCancel = new Button
            {
                Text = "Cancel",
                Width = 100,
                Height = 34,
                Location = new Point(buttonPanel.Width - 222, 12),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(80, 86, 110),
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 202, 215);
            btnCancel.Click += (s, e) => Close();

            btnSave = new Button
            {
                Text = "Save Member",
                Width = 120,
                Height = 34,
                Location = new Point(buttonPanel.Width - 136, 12),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = BrandNavy,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnSave);
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
                        var offlineService = ServiceManager.OfflineEnrollmentSubscriptionService;
                        if (offlineService != null)
                            await offlineService.PushOfflineEntriesToDevicesAsync();
                        else
                            await new DeviceFunctions().ProcessAllOfflineEntries();
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
