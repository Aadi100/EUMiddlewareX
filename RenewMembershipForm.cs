using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    public partial class RenewMembershipForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly Member _member;

        public RenewMembershipForm(Member member)
        {
            _member = member ?? throw new ArgumentNullException(nameof(member));
            _dbHelper = new DatabaseHelper();

            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }

            // Populate header with member data
            lblHeaderName.Text = _member.Name ?? "";
            lblStatusBadge.Text = BuildStatusText();

            // Populate payment methods
            cmbPaymentMethod.Items.Clear();
            cmbPaymentMethod.Items.Add("Cash");
            cmbPaymentMethod.Items.Add("Card");
            cmbPaymentMethod.Items.Add("Bank Transfer");
            cmbPaymentMethod.Items.Add("Online");

            // Select Cash by default
            cmbPaymentMethod.SelectedIndex = 0;
            UpdateTxnRefVisibility();

            LoadPackages();
            LoadTrainers();
        }

        private string BuildStatusText()
        {
            string status = _member.Status ?? "";
            if (status.Equals("active", StringComparison.OrdinalIgnoreCase)) return "Active";
            return "Inactive · no package";
        }

        // ── Paint handlers ────────────────────────────────────────────────────

        private void pnlAvatar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var br = new SolidBrush(Color.FromArgb(41, 98, 255)))
                e.Graphics.FillEllipse(br, 0, 0, pnlAvatar.Width - 1, pnlAvatar.Height - 1);

            string name = _member?.Name ?? "?";
            string initials = name.Length >= 2
                ? name.Substring(0, 2).ToUpper()
                : name.ToUpper();

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                e.Graphics.DrawString(initials, new Font("Segoe UI", 14f, FontStyle.Bold), Brushes.White,
                    new RectangleF(0, 0, pnlAvatar.Width, pnlAvatar.Height), sf);
        }

        private void pnlStatusBadge_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var br = new SolidBrush(Color.FromArgb(40, 45, 75)))
            using (var path = RoundedRect(new Rectangle(0, 0, pnlStatusBadge.Width - 1, pnlStatusBadge.Height - 1), 16))
                e.Graphics.FillPath(br, path);
        }

        private void pnlSubCard_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(220, 224, 238)))
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, pnlSubCard.Width - 1, pnlSubCard.Height - 1));
            using (var br = new SolidBrush(Color.FromArgb(59, 130, 246)))
                e.Graphics.FillRectangle(br, 0, 0, 4, pnlSubCard.Height);
        }

        private void pnlPayCard_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(220, 224, 238)))
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, pnlPayCard.Width - 1, pnlPayCard.Height - 1));
            using (var br = new SolidBrush(Color.FromArgb(0, 180, 140)))
                e.Graphics.FillRectangle(br, 0, 0, 4, pnlPayCard.Height);
        }

        private void pnlSummary_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(220, 224, 238)))
            {
                e.Graphics.DrawLine(pen, 0, 0, pnlSummary.Width, 0);
                e.Graphics.DrawLine(pen, 0, pnlSummary.Height - 1, pnlSummary.Width, pnlSummary.Height - 1);
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

        // ── Summary bar ───────────────────────────────────────────────────────

        private void cmbPeriodicity_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSummary();
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
                    lblAmountDue.Text  = "—";
                    lblNewEndDate.Text = "—";
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

                DateTime end = ComputeEndDate(dtpStartDate.Value.Date, periodicity);
                lblNewEndDate.Text = end.ToString("dd MMM yyyy");
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

        // ── Transaction ref visibility ────────────────────────────────────────

        private void cmbPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTxnRefVisibility();
        }

        private void UpdateTxnRefVisibility()
        {
            string method = cmbPaymentMethod.SelectedItem?.ToString() ?? "";
            bool isCash = method.Equals("Cash", StringComparison.OrdinalIgnoreCase);
            lblTxnRefLabel.Visible    = !isCash;
            txtTransactionRef.Visible = !isCash;
            lblTxnHint.Visible        = !isCash;
        }

        // ── Data loading ──────────────────────────────────────────────────────

        private void LoadPackages()
        {
            try
            {
                var packages = _dbHelper.GetPackages(activeOnly: true);
                cmbPackage.DisplayMember = "Title";
                cmbPackage.ValueMember   = "PackageId";
                cmbPackage.DataSource    = packages;
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
                cmbTrainer.ValueMember   = "TrainerId";
                cmbTrainer.DataSource    = trainers;
                cmbTrainer.SelectedIndex = -1;
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
            {
                UpdateSummary();
                return;
            }

            try
            {
                var pricingTier = JObject.Parse(selected.PricingTierJson);
                foreach (var prop in pricingTier.Properties())
                    cmbPeriodicity.Items.Add(prop.Name);

                if (cmbPeriodicity.Items.Count > 0)
                    cmbPeriodicity.SelectedIndex = 0;
                else
                    UpdateSummary();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[RenewMembershipForm] Failed to parse pricing tier: {ex.Message}");
                UpdateSummary();
            }
        }

        // ── Buttons ───────────────────────────────────────────────────────────

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
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

            btnSave.Enabled   = false;
            btnCancel.Enabled = false;

            try
            {
                var selectedTrainer = cmbTrainer.SelectedItem as Trainer;

                string methodRaw = cmbPaymentMethod.SelectedItem?.ToString() ?? "cash";
                string methodApi = methodRaw.ToLowerInvariant().Replace(" ", "_");

                var subscription = new JObject
                {
                    ["package_id"]           = selectedPackage.PackageId,
                    ["periodicity"]          = cmbPeriodicity.SelectedItem.ToString(),
                    ["start_date"]           = dtpStartDate.Value.ToString("yyyy-MM-dd"),
                    ["payment_collected_at"] = dtpPaymentCollectedAt.Value.ToString("yyyy-MM-dd"),
                    ["payment_method"]       = methodApi
                };
                if (selectedTrainer != null)
                    subscription["trainer_id"] = selectedTrainer.TrainerId;
                if (!string.IsNullOrWhiteSpace(txtTransactionRef.Text))
                    subscription["transaction_reference_no"] = txtTransactionRef.Text.Trim();

                // Enable member on every device it's already synced to (background — don't block UI)
                _ = Task.Run(() => EnableMemberOnSyncedDevicesAsync());

                // Local, immediate state: active + expiry cleared until the server confirms
                _dbHelper.RenewMemberLocally(_member.MemberID);

                // Queue the renewal for the background enrollment-sync timer to upload
                _dbHelper.InsertOfflineSubscription(_member.MemberID, _member.ClientId, subscription);

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
                btnSave.Enabled   = true;
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
