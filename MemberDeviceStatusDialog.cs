using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using smart_attendance_middleware.Services;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware
{
    public class MemberDeviceStatusDialog : Form
    {
        private Member _member;
        private List<(Device Device, DeviceService.UserInfo Info, string Error)> _data;

        public MemberDeviceStatusDialog(Member member, List<(Device Device, DeviceService.UserInfo Info, string Error)> data)
        {
            _member = member;
            _data = data;
            this.Font = new Font("Segoe UI", 9F);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Member Device Status Detail";

            // Dynamically size the form based on number of devices
            int deviceCount = _data?.Count ?? 1;
            int estimatedHeight = 70 + (deviceCount * 200) + 70; // header + cards + footer
            int formHeight = Math.Min(Math.Max(estimatedHeight, 500), 800); // Constrain between 500 and 800

            this.Size = new Size(600, formHeight);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new Size(600, 400);
            this.BackColor = Color.FromArgb(245, 246, 247);

            // Header Panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(18, 23, 49),
                Padding = new Padding(15, 0, 0, 0)
            };

            Label lblHeader = new Label
            {
                Text = _member.Name,
                Font = new Font("Segoe UI Semibold", 14F),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            headerPanel.Controls.Add(lblHeader);

            Label lblSubHeader = new Label
            {
                Text = $"Enrollment ID: {_member.EnrollmentID} | Member ID: {_member.MemberID}",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Location = new Point(17, 40)
            };
            headerPanel.Controls.Add(lblSubHeader);

            // Content Panel
            FlowLayoutPanel mainPanel = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                AutoScroll = true, 
                Padding = new Padding(15, 15, 15, 15),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false,
                Margin = new Padding(0)
            };

            // Footer Panel
            Panel footer = new Panel 
            { 
                Dock = DockStyle.Bottom, 
                Height = 70, 
                BackColor = Color.White,
                Padding = new Padding(0, 0, 15, 0)
            };
            this.Controls.Add(footer);

            Button btnOk = new Button 
            { 
                Text = "Close", 
                DialogResult = DialogResult.OK, 
                Width = 100, 
                Height = 36, 
                Location = new Point(450, 17), 
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(18, 23, 49),
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            footer.Controls.Add(btnOk);

            Button btnSync = new Button 
            { 
                Text = "Sync", 
                Width = 150, 
                Height = 36, 
                Location = new Point(290, 17), 
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F),
                Cursor = Cursors.Hand
            };
            btnSync.FlatAppearance.BorderSize = 0;
            btnSync.Click += async (s, ev) => 
            {
                // Check if membership is expired
                if (!string.IsNullOrWhiteSpace(_member.PackageExpiry))
                {
                    if (DateTime.TryParse(_member.PackageExpiry, out DateTime expiryDate))
                    {
                        if (expiryDate.Date < DateTime.Today)
                        {
                            MessageBox.Show("Member is expired.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                this.Enabled = false;
                Dashboard.GlobalStartLoader("Syncing membership status...");
                int enrollId = _member.EnrollmentID.Value;
                int success = 0;
                foreach (var item in _data) 
                {
                    if (item.Info != null && item.Info.Enabled == 0) 
                    {
                        try 
                        {
                            Dashboard.GlobalUpdateLoaderMessage($"Syncing on {item.Device.Location ?? item.Device.DeviceIP}...");
                            bool ok = await DeviceService.EnableUser(item.Device.DeviceIP, item.Device.Port, item.Device.MachineNo, item.Device.Password, enrollId, item.Device.MachineNo, 12);
                            if (ok) success++;
                        } catch { }
                    }
                }
                Dashboard.GlobalStopLoader();
                MessageBox.Show($"Membership synced successfully on {success} devices.", "Sync Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };
            footer.Controls.Add(btnSync);

            // Add panels to form in correct docking order (Fill should be added last or brought to front)
            this.Controls.Add(headerPanel);
            this.Controls.Add(footer);
            this.Controls.Add(mainPanel);
            mainPanel.BringToFront();

            // Populate Cards
            int targetWidth = mainPanel.Width > 100 ? mainPanel.Width - 50 : 520;
            foreach (var item in _data)
            {
                Panel card = new Panel 
                { 
                    Width = targetWidth,
                    MinimumSize = new Size(targetWidth, 0),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.White,
                    Padding = new Padding(0),
                    Margin = new Padding(0, 0, 0, 15)
                };
                mainPanel.Controls.Add(card);

                // Status Strip
                Panel statusStrip = new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 6,
                    BackColor = item.Error != null ? Color.FromArgb(232, 17, 35) : 
                               (item.Info == null ? Color.FromArgb(255, 185, 0) : Color.FromArgb(16, 124, 16))
                };
                card.Controls.Add(statusStrip);

                Label deviceTitle = new Label 
                { 
                    Text = item.Device.Location ?? "Unnamed Device", 
                    Font = new Font("Segoe UI Semibold", 11F),
                    ForeColor = Color.FromArgb(18, 23, 49),
                    AutoSize = true,
                    Location = new Point(15, 12)
                };
                card.Controls.Add(deviceTitle);

                Label deviceIp = new Label
                {
                    Text = $"IP: {item.Device.DeviceIP}",
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(16, 32)
                };
                card.Controls.Add(deviceIp);

                TableLayoutPanel detailsTable = new TableLayoutPanel
                {
                    Location = new Point(15, 60),
                    Width = targetWidth - 30,
                    MinimumSize = new Size(targetWidth - 30, 0),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 2,
                    RowCount = 4,
                    Margin = new Padding(0)
                };
                card.Controls.Add(detailsTable);
                detailsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
                detailsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

                if (item.Error != null)
                {
                    Label errLabel = new Label { Text = "Connection Status:", ForeColor = Color.Gray, AutoSize = true };
                    Label errVal = new Label { Text = "Unreachable", ForeColor = Color.FromArgb(232, 17, 35), Font = new Font("Segoe UI Semibold", 9F), AutoSize = true };
                    detailsTable.Controls.Add(errLabel, 0, 0);
                    detailsTable.Controls.Add(errVal, 1, 0);

                    Label errMsg = new Label { Text = "Error Detail:", ForeColor = Color.Gray, AutoSize = true };
                    Label errMsgVal = new Label { Text = item.Error, ForeColor = Color.DimGray, AutoSize = true, MaximumSize = new Size(280, 40) };
                    detailsTable.Controls.Add(errMsg, 0, 1);
                    detailsTable.Controls.Add(errMsgVal, 1, 1);
                }
                else if (item.Info == null || string.IsNullOrEmpty(item.Info.AliasId))
                {
                    Label stLabel = new Label { Text = "Enrollment status:", ForeColor = Color.Gray, AutoSize = true };
                    Label stVal = new Label { Text = "Not Enrolled", ForeColor = Color.FromArgb(255, 185, 0), Font = new Font("Segoe UI Semibold", 9F), AutoSize = true };
                    detailsTable.Controls.Add(stLabel, 0, 0);
                    detailsTable.Controls.Add(stVal, 1, 0);
                }
                else
                {
                    AddDetailRow(detailsTable, "RFID Card:", string.IsNullOrEmpty(item.Info.CardNumber) || item.Info.CardNumber == "0" ? "None" : item.Info.CardNumber, 0);
                    AddDetailRow(detailsTable, "Biometrics:", $"Face: {(item.Info.FaceFlag == 1 ? "Yes" : "No")} | FP: {(item.Info.FPFlag == 1 ? "Yes" : "No")}", 1);

                    string statusText = item.Info.Enabled == 1 ? "Enabled" : "Disabled";
                    Color statusColor = item.Info.Enabled == 1 ? Color.FromArgb(16, 124, 16) : Color.FromArgb(232, 17, 35);
                    AddDetailRow(detailsTable, "User Status:", statusText, 2, statusColor);
                }
            }
        }

        private void AddDetailRow(TableLayoutPanel panel, string label, string value, int row, Color? valueColor = null)
        {
            panel.Controls.Add(new Label { Text = label, ForeColor = Color.Gray, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            panel.Controls.Add(new Label { 
                Text = value, 
                ForeColor = valueColor ?? Color.FromArgb(64, 64, 64), 
                Font = new Font("Segoe UI Semibold", 9F), 
                AutoSize = true, 
                Anchor = AnchorStyles.Left 
            }, 1, row);
        }
    }
}
