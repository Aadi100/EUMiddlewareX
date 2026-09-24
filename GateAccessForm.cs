using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware
{
    public partial class GateAccessForm : Form
    {
        private string _deviceIP;
        private DatabaseHelper _dbHelper;
        private string _loggedInUser;

        public GateAccessForm(string deviceIP, DatabaseHelper dbHelper, string loggedInUser)
        {
            InitializeComponent();
            _deviceIP = deviceIP;
            _dbHelper = dbHelper;
            _loggedInUser = loggedInUser;

            // Initialize the member list with suggestions
            InitializeMemberList();

            // Set the logged-in user and device ID in the form
            txtLoggedInUser.Text = _loggedInUser;
            txtDeviceIP.Text = _deviceIP;

            // Ensure visitor fields are disabled by default
            txtVisitorName.Enabled = false;
            txtVisitorPhone.Enabled = false;
            LogHelper.Log($"GateAccessForm initialized for user: {_loggedInUser} on device: {_deviceIP}");
        }

        private void GateAccessForm_Load(object sender, EventArgs e)
        {
            LogHelper.Log("GateAccessForm loaded.");
        }

        private void InitializeMemberList()
        {
            // Load all members from the database
            List<Member> members = _dbHelper.GetMembers();
            LogHelper.Log($"Loaded {members.Count} members from the database.");

            // Configure the AutoCompleteSource and AutoCompleteMode
            txtMemberName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtMemberName.AutoCompleteSource = AutoCompleteSource.CustomSource;

            // Create a StringCollection to store member names
            AutoCompleteStringCollection memberCollection = new AutoCompleteStringCollection();

            // Add each member's name to the collection
            foreach (var member in members)
            {
                memberCollection.Add(member.Name);
            }

            // Assign the collection to the AutoCompleteCustomSource
            txtMemberName.AutoCompleteCustomSource = memberCollection;
            LogHelper.Log("Member list initialized with suggestions.");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            LogHelper.Log("Save button clicked.");

            // Same "RequireGatePassword" rule as the no-form direct-open path.
            if (CommonUtils.IsGatePasswordRequired(_dbHelper) && !CommonUtils.PromptForPassword(_loggedInUser, _dbHelper))
            {
                return;
            }

            int memberId = 0;
            if (!chkVisitor.Checked)
            {
                // Get the selected member's ID from the database.
                List<Member> members = _dbHelper.GetMembers();
                var selectedMember = members.FirstOrDefault(m => string.Equals(m.Name?.Trim(), txtMemberName.Text?.Trim(), StringComparison.OrdinalIgnoreCase));
                if (selectedMember == null)
                {
                    MessageBox.Show("Please select a valid member.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogHelper.Log("Invalid member selected.");
                    return;
                }
                memberId = selectedMember.MemberID;
                LogHelper.Log($"Selected member ID: {memberId}, Name: {selectedMember.Name}");
            }
            else
            {
                // Only the visitor's name is required — phone and reason are optional for visitors.
                if (string.IsNullOrWhiteSpace(txtVisitorName.Text))
                {
                    MessageBox.Show("Please enter the visitor's name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogHelper.Log("Visitor name missing.");
                    return;
                }
                LogHelper.Log($"Visitor Name: {txtVisitorName.Text}, Phone: {txtVisitorPhone.Text}");
            }

            // Create a new GateAccessed object.
            GateAccessed gateAccessed = new GateAccessed
            {
                DeviceIP = _deviceIP,
                MemberId = chkVisitor.Checked ? 0 : memberId,
                IsVisitor = chkVisitor.Checked,
                VisitorName = chkVisitor.Checked ? txtVisitorName.Text : null,
                VisitorPhone = chkVisitor.Checked ? txtVisitorPhone.Text : null,
                Reason = txtReason.Text,
                AccessedTime = DateTime.Now,
                Username = _loggedInUser
            };
            LogHelper.Log("GateAccessed object created.");

            // Insert the gate access record into the database.
            _dbHelper.InsertGateAccessed(gateAccessed);
            LogHelper.Log("Gate access recorded in the database.");
            this.DialogResult = DialogResult.OK;
            this.Close();
            LogHelper.Log("Gate access recorded successfully. Form closed.");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            LogHelper.Log("Cancel button clicked.");
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            LogHelper.Log("Gate access cancelled. Form closed.");
        }

        private void chkVisitor_CheckedChanged(object sender, EventArgs e)
        {
            // If the visitor checkbox is checked, disable the member name textbox and enable visitor fields
            txtMemberName.Enabled = !chkVisitor.Checked;
            txtVisitorName.Enabled = chkVisitor.Checked;
            txtVisitorPhone.Enabled = chkVisitor.Checked;
            LogHelper.Log($"Visitor checkbox checked: {chkVisitor.Checked}");
        }
    }
}