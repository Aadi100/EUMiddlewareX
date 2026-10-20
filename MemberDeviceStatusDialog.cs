using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using smart_attendance_middleware.Classes;
using Newtonsoft.Json.Linq;

namespace smart_attendance_middleware
{
    public partial class DeviceStatusDialog : Form
    {
        private object _entity;  // Can be Member or Staff
        private List<(Device Device, JObject Details, string Error)> _data;
        private bool _isMember;
        private Action _onEnroll;
        private Action _onDelete;

        public DeviceStatusDialog(object entity, List<(Device Device, JObject Details, string Error)> data, Action onEnroll = null, Action onDelete = null)
        {
            _entity = entity;
            _data = data;
            _isMember = entity is Member;
            _onEnroll = onEnroll;
            _onDelete = onDelete;
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            BuildUi();
        }

        // Keep old constructor name for backward compatibility
        public DeviceStatusDialog(Member member, List<(Device Device, JObject Details, string Error)> data)
            : this((object)member, data) { }

        private void BuildUi()
        {
            // Get properties dynamically from entity (Member or Staff)
            string entityName = GetPropertyValue("Name")?.ToString() ?? "Unknown";
            string enrollmentId = GetPropertyValue("EnrollmentID")?.ToString() ?? "N/A";
            string entityId = GetPropertyValue(_isMember ? "MemberID" : "StaffID")?.ToString() ?? "N/A";
            string entityTypeLabel = _isMember ? "Member ID" : "Staff ID";

            this.Text = $"{(_isMember ? "Member" : "Staff")} Device Status Detail";

            int deviceCount = _data?.Count ?? 1;
            int estimatedHeight = 80 + (deviceCount * 310) + 70;
            this.Height = Math.Min(Math.Max(estimatedHeight, 550), 900);

            // ── Fill designer-declared header labels ─────────────────────────────
            lblEntityName.Text = entityName;
            lblEntityInfo.Text = $"Enrollment ID: {enrollmentId} | {entityTypeLabel}: {entityId}";

            // Resolve local face image
            string localFacePath = null;
            if (int.TryParse(entityId, out int id) && id > 0)
                localFacePath = ResolveLocalFacePath(id);

            // ── Extra footer buttons (dynamic) ───────────────────────────────────
            if (!_isMember)
            {
                Button btnEnroll = new Button
                {
                    Text = "Enroll",
                    Width = 100, Height = 36,
                    Location = new Point(270, 17),
                    BackColor = Color.FromArgb(0, 120, 212),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnEnroll.Click += (s, e) => { this.Close(); _onEnroll?.Invoke(); };
                pnlFooter.Controls.Add(btnEnroll);

                Button btnDelete = new Button
                {
                    Text = "Delete",
                    Width = 100, Height = 36,
                    Location = new Point(380, 17),
                    BackColor = Color.FromArgb(220, 53, 69),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnDelete.Click += (s, e) => { this.Close(); _onDelete?.Invoke(); };
                pnlFooter.Controls.Add(btnDelete);
            }
            else
            {
                Button btnSync = new Button
                {
                    Text = "Sync Status",
                    Width = 150, Height = 36,
                    Location = new Point(395, 17),
                    BackColor = Color.FromArgb(0, 120, 212),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnSync.Click += async (s, ev) =>
                {
                    if (_isMember)
                    {
                        string packageExpiry = GetPropertyValue("PackageExpiry")?.ToString();
                        if (!string.IsNullOrWhiteSpace(packageExpiry) &&
                            DateTime.TryParse(packageExpiry, out DateTime expiryDate) &&
                            expiryDate.Date < DateTime.Today)
                        {
                            MessageBox.Show("Member membership has expired. Sync cancelled.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    this.Enabled = false;
                    Dashboard.GlobalStartLoader("Syncing status via Cloud WebSocket...");

                    string enrollIdStr = GetPropertyValue("EnrollmentID")?.ToString();
                    if (!int.TryParse(enrollIdStr, out int enrollId))
                    {
                        MessageBox.Show("Invalid Enrollment ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Enabled = true;
                        return;
                    }

                    int success = 0, attempted = 0;
                    foreach (var item in _data)
                    {
                        var enabled = item.Details?["enable"]?.Value<int>();
                        if (enabled.HasValue && enabled.Value == 0)
                        {
                            attempted++;
                            string sn = CommonUtils.GetDeviceSnByIp(item.Device.DeviceIP);
                            if (!string.IsNullOrEmpty(sn))
                            {
                                try
                                {
                                    Dashboard.GlobalUpdateLoaderMessage($"Contacting {item.Device.Location ?? item.Device.DeviceIP}...");
                                    bool ok = await WebsocketServer.ExecuteServiceCommand(sn, "enableuser",
                                        Newtonsoft.Json.JsonConvert.SerializeObject(new { enrollid = enrollId, enflag = 1 }));
                                    if (ok) success++;
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Log($"[Sync-Error] {item.Device.DeviceIP}: {ex.Message}");
                                }
                            }
                        }
                    }

                    Dashboard.GlobalStopLoader();
                    this.Enabled = true;

                    string entityType = _isMember ? "Member" : "Staff";
                    MessageBox.Show(attempted > 0
                        ? $"Sync commands dispatched to {success} of {attempted} devices."
                        : $"{entityType} already enabled on all devices.",
                        "Sync Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                };
                pnlFooter.Controls.Add(btnSync);
            }

            // ── Device Cards ────────────────────────────────────────────────────
            const int targetWidth = 640;

            Image localFaceImg = null;
            if (localFacePath != null)
                try { localFaceImg = Image.FromFile(localFacePath); } catch { }

            foreach (var item in _data)
            {
                Panel card = new Panel
                {
                    Width = targetWidth,
                    MinimumSize = new Size(targetWidth, 0),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 0, 15)
                };
                mainPanel.Controls.Add(card);

                bool hasError = item.Error != null;
                bool notEnrolled = !hasError && (item.Details == null || item.Details["result"]?.Value<bool>() == false);
                Color stripColor = hasError ? Color.FromArgb(232, 17, 35)
                                 : notEnrolled ? Color.FromArgb(255, 185, 0)
                                 : Color.FromArgb(16, 124, 16);

                card.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = stripColor });

                // Resolve face images for this card
                Image deviceFaceImg = null;
                string faceBase64 = item.Details?["facerecord"]?.ToString();
                if (!string.IsNullOrEmpty(faceBase64))
                    deviceFaceImg = CommonUtils.Base64ToImage(faceBase64);

                bool hasImages = localFaceImg != null || deviceFaceImg != null;
                const int imgSize = 100;
                const int imgPad = 8;
                // Images sit on the right; each image is imgSize + label(16) + gap
                int imagesBlockWidth = hasImages
                    ? ((localFaceImg != null ? imgSize + imgPad : 0) + (deviceFaceImg != null ? imgSize + imgPad : 0))
                    : 0;

                // Right-side image panel
                if (hasImages)
                {
                    Panel photoPanel = new Panel
                    {
                        Width = imagesBlockWidth,
                        Height = imgSize + 20,
                        BackColor = Color.White,
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    };
                    // Position from right edge of card
                    photoPanel.Location = new Point(targetWidth - imagesBlockWidth - 10, 12);
                    card.Controls.Add(photoPanel);

                    int x = 0;
                    if (localFaceImg != null)
                    {
                        photoPanel.Controls.Add(new Label { Text = "Local", AutoSize = true, Location = new Point(x, 0), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8f, FontStyle.Regular) });
                        var pb = new PictureBox { Image = localFaceImg, Size = new Size(imgSize, imgSize), Location = new Point(x, 16), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
                        pb.Click += (s, ev) => ShowImagePreview(localFaceImg, "Local Photo — " + entityName);
                        photoPanel.Controls.Add(pb);
                        x += imgSize + imgPad;
                    }
                    if (deviceFaceImg != null)
                    {
                        photoPanel.Controls.Add(new Label { Text = "Device", AutoSize = true, Location = new Point(x, 0), ForeColor = Color.Gray, Font = new Font("Segoe UI", 8f, FontStyle.Regular) });
                        var pb = new PictureBox { Image = deviceFaceImg, Size = new Size(imgSize, imgSize), Location = new Point(x, 16), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
                        pb.Click += (s, ev) => ShowImagePreview(deviceFaceImg, "Device Photo — " + entityName);
                        photoPanel.Controls.Add(pb);
                    }
                }

                // Left info column
                const int infoLeft = 14;
                int infoWidth = targetWidth - infoLeft - imagesBlockWidth - 20;

                card.Controls.Add(new Label
                {
                    Text = item.Device.Location ?? "Unnamed Device",
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(18, 23, 49),
                    AutoSize = true,
                    Location = new Point(infoLeft, 12)
                });
                card.Controls.Add(new Label
                {
                    Text = $"IP: {item.Device.DeviceIP}",
                    Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(infoLeft + 1, 32)
                });

                var table = new TableLayoutPanel
                {
                    Location = new Point(infoLeft, 55),
                    Width = infoWidth,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 2,
                    Margin = new Padding(0)
                };
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                card.Controls.Add(table);

                if (hasError)
                {
                    AddRow(table, "Connection:", "Unreachable", 0, Color.FromArgb(232, 17, 35));
                    AddRow(table, "Detail:", item.Error, 1);
                }
                else if (notEnrolled)
                {
                    AddRow(table, "Status:", "Not Enrolled", 0, Color.FromArgb(255, 185, 0));
                }
                else
                {
                    var d = item.Details;
                    string card_ = d["card"]?.Value<long>() > 0 ? d["card"].ToString() : "None";
                    string fp = d["fpflag"]?.Value<int>() == 1 ? "Yes" : "No";
                    string face = d["faceflag"]?.Value<int>() == 1 ? "Yes" : "No";
                    int enableVal = d["enable"]?.Value<int>() ?? 0;
                    AddRow(table, "RFID Card:", card_, 0);
                    AddRow(table, "Biometrics:", $"Face: {face} | FP: {fp}", 1);
                    AddRow(table, "User Status:", enableVal == 1 ? "Enabled" : "Disabled", 2,
                        enableVal == 1 ? Color.FromArgb(16, 124, 16) : Color.FromArgb(232, 17, 35));
                }
            }
        }

        private void AddRow(TableLayoutPanel panel, string label, string value, int row, Color? valueColor = null)
        {
            panel.Controls.Add(new Label { Text = label, ForeColor = Color.Gray, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            panel.Controls.Add(new Label
            {
                Text = value,
                ForeColor = valueColor ?? Color.FromArgb(64, 64, 64),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                AutoSize = true,
                MaximumSize = new Size(300, 60),
                Anchor = AnchorStyles.Left
            }, 1, row);
        }

        private void ShowImagePreview(Image img, string title)
        {
            var f = new Form
            {
                Text = title,
                Size = new Size(Math.Min(img.Width + 40, 800), Math.Min(img.Height + 60, 700)),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };
            f.Controls.Add(new PictureBox { Image = img, Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom });
            f.ShowDialog(this);
        }

        private object GetPropertyValue(string propertyName)
        {
            try
            {
                var prop = _entity.GetType().GetProperty(propertyName);
                return prop?.GetValue(_entity);
            }
            catch
            {
                return null;
            }
        }

        private static string ResolveLocalFacePath(int? memberId)
        {
            if (!memberId.HasValue) return null;
            string faceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FACE");
            foreach (string ext in new[] { ".jpg", ".jpeg", ".png" })
            {
                string p = Path.Combine(faceDir, memberId.Value + ext);
                if (File.Exists(p)) return p;
            }
            return null;
        }
    }
}

