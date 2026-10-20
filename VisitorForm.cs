using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware
{
    public partial class VisitorForm : Form
    {
        private string _deviceIP;
        private DatabaseHelper _dbHelper;
        private string _loggedInUser;

        // Y positions — member mode: member fields visible (52px tall), toggle at y=68
        // visitor mode: member fields hidden, toggle shifts up to y=16 (saves 52px)
        private const int MemberTogY_Member  = 68;
        private const int MemberTogY_Visitor = 16;
        private const int TrainerY_Member    = 104;
        private const int TrainerY_Visitor   = 152;  // 16(tog) + 24(tog h) + 8 + 96(visitor panel) + 8
        private const int ReasonY_Member     = 160;
        private const int ReasonY_Visitor    = 208;

        public VisitorForm(string deviceIP, DatabaseHelper dbHelper, string loggedInUser)
        {
            InitializeComponent();
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath); } catch { }
            _deviceIP    = deviceIP;
            _dbHelper    = dbHelper;
            _loggedInUser = loggedInUser;

            InitializeMemberList();
            LoadTrainers();

            lblInfoOpVal.Text    = _loggedInUser;
            lblInfoDevVal.Text   = _deviceIP;
            txtLoggedInUser.Text = _loggedInUser;
            txtDeviceIP.Text     = _deviceIP;

            txtVisitorName.Enabled  = false;
            txtVisitorPhone.Enabled = false;

            LogHelper.Log($"VisitorForm initialized for user: {_loggedInUser} on device: {_deviceIP}");
        }

        private void GateAccessForm_Load(object sender, EventArgs e) { }

        private void InitializeMemberList()
        {
            var members = _dbHelper.GetMembers();
            txtMemberName.AutoCompleteMode   = AutoCompleteMode.SuggestAppend;
            txtMemberName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            var col = new AutoCompleteStringCollection();
            foreach (var m in members) col.Add(m.Name);
            txtMemberName.AutoCompleteCustomSource = col;
        }

        private void LoadTrainers()
        {
            try
            {
                var trainers = _dbHelper.GetTrainers();
                // Add a blank entry so trainer is optional
                var list = new List<Trainer> { new Trainer { TrainerId = null, FirstName = "— None —", LastName = "" } };
                list.AddRange(trainers);
                cmbTrainer.DisplayMember = "Name";
                cmbTrainer.ValueMember   = "TrainerId";
                cmbTrainer.DataSource    = list;
                cmbTrainer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[VisitorForm] Failed to load trainers: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (CommonUtils.IsGatePasswordRequired(_dbHelper) && !CommonUtils.PromptForPassword(_loggedInUser, _dbHelper))
                return;

            int memberId = 0;
            if (!chkVisitor.Checked)
            {
                var members = _dbHelper.GetMembers();
                var selected = members.FirstOrDefault(m => string.Equals(m.Name?.Trim(), txtMemberName.Text?.Trim(), StringComparison.OrdinalIgnoreCase));
                if (selected == null)
                {
                    MessageBox.Show("Please select a valid member.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                memberId = selected.MemberID;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtVisitorName.Text))
                {
                    MessageBox.Show("Please enter the visitor's name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            var selectedTrainer = cmbTrainer.SelectedItem as Trainer;
            string trainerId = selectedTrainer?.TrainerId; // null if "— None —"

            var record = new GateAccessed
            {
                DeviceIP    = _deviceIP,
                MemberId    = chkVisitor.Checked ? 0 : memberId,
                IsVisitor   = chkVisitor.Checked,
                VisitorName = chkVisitor.Checked ? txtVisitorName.Text : null,
                VisitorPhone= chkVisitor.Checked ? txtVisitorPhone.Text : null,
                Reason      = string.IsNullOrWhiteSpace(txtReason.Text) ? null : txtReason.Text,
                TrainerId   = trainerId,
                AccessedTime= DateTime.Now,
                Username    = _loggedInUser
            };
            _dbHelper.InsertGateAccessed(record);
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }

        private void chkVisitor_CheckedChanged(object sender, EventArgs e)
        {
            bool isVisitor = chkVisitor.Checked;

            // Hide/show member name field
            lblMemberLbl.Visible    = !isVisitor;
            txtMemberName.Visible   = !isVisitor;
            txtMemberName.Enabled   = !isVisitor;

            // Slide toggle up when member fields are hidden
            pnlVisitorTog.Location  = new Point(20, isVisitor ? MemberTogY_Visitor : MemberTogY_Member);

            // Show visitor fields below the toggle
            int togBottom = (isVisitor ? MemberTogY_Visitor : MemberTogY_Member) + 24 + 8;
            pnlVisitorFlds.Location = new Point(20, togBottom);
            pnlVisitorFlds.Visible  = isVisitor;
            txtVisitorName.Enabled  = isVisitor;
            txtVisitorPhone.Enabled = isVisitor;

            int trainerY = isVisitor ? TrainerY_Visitor : TrainerY_Member;
            int reasonY  = isVisitor ? ReasonY_Visitor  : ReasonY_Member;

            lblTrainerLbl.Location = new Point(20, trainerY);
            cmbTrainer.Location    = new Point(20, trainerY + 16);
            lblReasonLbl.Location  = new Point(20, reasonY);
            txtReason.Location     = new Point(20, reasonY + 16);
        }
    }
}
