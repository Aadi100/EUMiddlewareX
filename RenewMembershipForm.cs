using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    /// <summary>
    /// Renews an existing member's membership while offline. Locally: enables the member on
    /// every device it's already synced to, sets Status=active and clears PackageExpiry
    /// immediately. The subscription itself is queued in dbo.OfflineSubscriptions and pushed to
    /// /api/sync/enrollments later by OfflineEnrollmentSubscriptionService's timer, which then
    /// writes the real package_expiry back once the server confirms it.
    /// </summary>
    public class RenewMembershipForm : Form
    {
        private static readonly Color BrandNavy = Color.FromArgb(18, 23, 49);

        private readonly DatabaseHelper _dbHelper;
        private readonly Member _member;

        private ComboBox cmbPackage;
        private ComboBox cmbPeriodicity;
        private ComboBox cmbTrainer;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpPaymentCollectedAt;
        private ComboBox cmbPaymentMethod;
        private TextBox txtTransactionRef;
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;

        public RenewMembershipForm(Member member)
        {
            _member = member ?? throw new ArgumentNullException(nameof(member));
            _dbHelper = new DatabaseHelper();

            BuildUi();
            LoadPackages();
            LoadTrainers();
        }

        private void BuildUi()
        {
            Text = "Renew Membership";
            Width = 560;
            Height = 620;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 246, 250);

            // ── Header ──────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = BrandNavy };
            header.Controls.Add(new Label
            {
                Text = "Renew Membership",
                ForeColor = Color.FromArgb(160, 170, 200),
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true,
                Location = new Point(58, 18)
            });
            header.Controls.Add(new Label
            {
                Text = _member.Name,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(58, 36)
            });
            // Avatar circle placeholder
            var avatar = new Panel
            {
                Location = new Point(16, 16),
                Size = new Size(36, 36),
                BackColor = Color.FromArgb(41, 98, 255)
            };
            avatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(41, 98, 255)), 0, 0, 35, 35);
                string initials = _member.Name?.Length > 0
                    ? _member.Name.Substring(0, Math.Min(2, _member.Name.Length)).ToUpper()
                    : "?";
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(initials, new Font("Segoe UI", 11F, FontStyle.Bold), Brushes.White,
                    new RectangleF(0, 0, 35, 35), sf);
            };
            header.Controls.Add(avatar);
            Controls.Add(header);

            // ── Helper: section card ─────────────────────────────────────────
            Panel MakeCard(int top, int height, string title, Color accent)
            {
                var card = new Panel
                {
                    Location = new Point(20, top),
                    Size = new Size(Width - 56, height),
                    BackColor = Color.White
                };
                card.Paint += (s, e) =>
                {
                    var r = new System.Drawing.Rectangle(0, 0, card.Width - 1, card.Height - 1);
                    e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 222, 230)), r);
                    e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 4, card.Height);
                };
                card.Controls.Add(new Label
                {
                    Text = title,
                    ForeColor = Color.FromArgb(50, 55, 80),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(16, 12)
                });
                Controls.Add(card);
                return card;
            }

            void AddRow(Panel card, ref int y, string labelText, Control ctrl)
            {
                card.Controls.Add(new Label
                {
                    Text = labelText,
                    ForeColor = Color.FromArgb(90, 96, 120),
                    Font = new Font("Segoe UI", 8.5F),
                    AutoSize = false,
                    Size = new Size(148, 22),
                    Location = new Point(16, y + 4)
                });
                ctrl.Location = new Point(168, y);
                ctrl.Size = new Size(card.Width - 184, 26);
                ctrl.Font = new Font("Segoe UI", 9F);
                card.Controls.Add(ctrl);
                y += 36;
            }

            // ── Section 1: Subscription ──────────────────────────────────────
            var subCard = MakeCard(96, 174, "Subscription Details", Color.FromArgb(41, 98, 255));
            int y1 = 38;

            cmbPackage = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPackage.SelectedIndexChanged += CmbPackage_SelectedIndexChanged;
            AddRow(subCard, ref y1, "Package *", cmbPackage);

            cmbPeriodicity = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddRow(subCard, ref y1, "Periodicity *", cmbPeriodicity);

            cmbTrainer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddRow(subCard, ref y1, "Trainer", cmbTrainer);

            dtpStartDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            AddRow(subCard, ref y1, "Start Date", dtpStartDate);

            // ── Section 2: Payment ───────────────────────────────────────────
            var payCard = MakeCard(282, 174, "Payment Details", Color.FromArgb(0, 180, 140));
            int y2 = 38;

            dtpPaymentCollectedAt = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            AddRow(payCard, ref y2, "Payment Collected At", dtpPaymentCollectedAt);

            cmbPaymentMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPaymentMethod.Items.AddRange(new object[] { "cash", "card", "pos_card", "bank_transfer", "online" });
            cmbPaymentMethod.SelectedIndex = 0;
            AddRow(payCard, ref y2, "Payment Method", cmbPaymentMethod);

            txtTransactionRef = new TextBox();
            AddRow(payCard, ref y2, "Transaction Ref No", txtTransactionRef);

            // ── Status label ─────────────────────────────────────────────────
            lblStatus = new Label
            {
                AutoSize = false,
                Location = new Point(20, 468),
                Size = new Size(Width - 56, 22),
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
                Location = new Point(buttonPanel.Width - 216, 12),
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
                Text = "Renew Membership",
                Width = 140,
                Height = 34,
                Location = new Point(buttonPanel.Width - 160, 12),
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
        }

        private void LoadPackages()
        {
            try
            {
                var packages = _dbHelper.GetPackages(activeOnly: true);
                cmbPackage.DisplayMember = "Title";
                cmbPackage.ValueMember = "PackageId";
                cmbPackage.DataSource = packages;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RenewMembershipForm] Failed to load packages: {ex.Message}");
                lblStatus.Text = "Failed to load packages: " + ex.Message;
            }
        }

        private void LoadTrainers()
        {
            try
            {
                var trainers = _dbHelper.GetTrainers();
                cmbTrainer.DisplayMember = "Name";
                cmbTrainer.ValueMember = "TrainerId";
                cmbTrainer.DataSource = trainers;
                cmbTrainer.SelectedIndex = -1; // trainer is optional — nothing selected by default
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RenewMembershipForm] Failed to load trainers: {ex.Message}");
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
                    cmbPeriodicity.Items.Add(prop.Name);

                if (cmbPeriodicity.Items.Count > 0)
                    cmbPeriodicity.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RenewMembershipForm] Failed to parse pricing tier: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblStatus.Text = string.Empty;

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
                var selectedTrainer = cmbTrainer.SelectedItem as Trainer;

                var subscription = new JObject
                {
                    ["package_id"] = selectedPackage.PackageId,
                    ["periodicity"] = cmbPeriodicity.SelectedItem.ToString(),
                    ["start_date"] = dtpStartDate.Value.ToString("yyyy-MM-dd"),
                    ["payment_collected_at"] = dtpPaymentCollectedAt.Value.ToString("yyyy-MM-dd"),
                    ["payment_method"] = cmbPaymentMethod.SelectedItem?.ToString() ?? "cash"
                };
                if (selectedTrainer != null)
                    subscription["trainer_id"] = selectedTrainer.TrainerId;
                if (!string.IsNullOrWhiteSpace(txtTransactionRef.Text))
                    subscription["transaction_reference_no"] = txtTransactionRef.Text.Trim();

                // 1. Enable the member on every device it's already synced to — fire in the
                //    background so this dialog doesn't block on device I/O.
                _ = Task.Run(() => EnableMemberOnSyncedDevicesAsync());

                // 2. Local, immediate state: active + expiry cleared until the server confirms.
                _dbHelper.RenewMemberLocally(_member.MemberID);

                // 3. Queue the renewal for the background enrollment-sync timer to upload.
                _dbHelper.InsertOfflineSubscription(_member.MemberID, _member.ClientId, subscription);

                // Try uploading right away; retries once after 3s, then the 1-minute timer takes over.
                var offlineService = ServiceManager.OfflineEnrollmentSubscriptionService;
                if (offlineService != null)
                    _ = Task.Run(() => offlineService.SyncPendingRenewalsWithQuickRetryAsync());

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RenewMembershipForm] Failed to queue renewal for MemberID={_member.MemberID}: {ex.Message}");
                lblStatus.Text = "Failed to save: " + ex.Message;
            }
            finally
            {
                btnSave.Enabled = true;
                btnCancel.Enabled = true;
            }
        }

        private async Task EnableMemberOnSyncedDevicesAsync()
        {
            try
            {
                if (!_member.EnrollmentID.HasValue || _member.SyncStatus == null) return;

                var devices = _dbHelper.LoadDevices();
                foreach (var kv in _member.SyncStatus.Where(s => s.Value))
                {
                    var device = devices.FirstOrDefault(d => d.DeviceId == kv.Key);
                    if (device == null) continue;

                    try
                    {
                        var enableParams = new { enrollid = _member.EnrollmentID.Value, enflag = 1 };
                        bool ok = await WebsocketServer.ExecuteServiceCommand(
                            device.DeviceSerial, "enableuser", JsonConvert.SerializeObject(enableParams)).ConfigureAwait(false);

                        LogHelper.Log($"[RenewMembershipForm] Enable-on-device {(ok ? "OK" : "FAILED")} for {_member.Name} on DeviceId={device.DeviceId} ({device.DeviceSerial})");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[RenewMembershipForm] Error enabling {_member.Name} on DeviceId={kv.Key}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RenewMembershipForm] EnableMemberOnSyncedDevicesAsync error: {ex.Message}");
            }
        }
    }
}
