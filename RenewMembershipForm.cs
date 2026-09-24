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
            Width = 520;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.WhiteSmoke;

            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = BrandNavy };
            var lblTitle = new Label
            {
                Text = $"Renew Membership — {_member.Name}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 14)
            };
            header.Controls.Add(lblTitle);
            Controls.Add(header);

            var layout = new TableLayoutPanel
            {
                Location = new Point(16, 64),
                Size = new Size(480, 340),
                ColumnCount = 2,
                AutoSize = true
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
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

            cmbPackage = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPackage.SelectedIndexChanged += CmbPackage_SelectedIndexChanged;
            AddRow("Package*", cmbPackage);

            cmbPeriodicity = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddRow("Periodicity*", cmbPeriodicity);

            cmbTrainer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddRow("Trainer", cmbTrainer);

            dtpStartDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            AddRow("Start Date", dtpStartDate);

            dtpPaymentCollectedAt = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            AddRow("Payment Collected At", dtpPaymentCollectedAt);

            cmbPaymentMethod = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPaymentMethod.Items.AddRange(new object[] { "cash", "card", "pos_card", "bank_transfer", "online" });
            cmbPaymentMethod.SelectedIndex = 0;
            AddRow("Payment Method", cmbPaymentMethod);

            txtTransactionRef = new TextBox();
            AddRow("Transaction Ref No", txtTransactionRef);

            lblStatus = new Label
            {
                AutoSize = false,
                Location = new Point(16, 410),
                Size = new Size(480, 40),
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

            btnSave = new Button { Text = "Renew", Width = 100, BackColor = BrandNavy, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Cancel", Width = 90 };
            btnCancel.Click += (s, e) => Close();

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);
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
