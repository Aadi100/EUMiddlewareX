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

            // Inline prompt to ask for the password
            Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = $"Password Required - {_loggedInUser}",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label label = new Label()
            {
                Left = 10,
                Top = 20,
                Width = 320,
                Text = "Enter your password:"
            };

            TextBox inputBox = new TextBox()
            {
                Left = 10,
                Top = 50,
                Width = 320,
                UseSystemPasswordChar = true // This ensures that the password is masked with asterisks (*)
            };

            Button okButton = new Button()
            {
                Text = "OK",
                Left = 180,
                Width = 70,
                Top = 85,
                DialogResult = DialogResult.OK
            };

            Button cancelButton = new Button()
            {
                Text = "Cancel",
                Left = 260,
                Width = 70,
                Top = 85,
                DialogResult = DialogResult.Cancel
            };

            prompt.Controls.Add(label);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(okButton);
            prompt.Controls.Add(cancelButton);
            prompt.AcceptButton = okButton;
            prompt.CancelButton = cancelButton;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                string password = inputBox.Text;
                LogHelper.Log("Password entered.");

                // Verify the password using the entered value.
                if (!_dbHelper.VerifyLoginUser(_loggedInUser, password))
                {
                    MessageBox.Show("Incorrect password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogHelper.Log("Incorrect password entered.");
                    return;
                }
                LogHelper.Log("Password verified successfully.");

                int memberId = 0;
                if (!chkVisitor.Checked)
                {
                    // Get the selected member's ID from the database.
                    List<Member> members = _dbHelper.GetMembers();
                    var selectedMember = members.FirstOrDefault(m => m.Name == txtMemberName.Text);
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
                    // Validate visitor details.
                    if (string.IsNullOrWhiteSpace(txtVisitorName.Text) || string.IsNullOrWhiteSpace(txtVisitorPhone.Text))
                    {
                        MessageBox.Show("Please enter visitor name and phone number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LogHelper.Log("Visitor details missing.");
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
            else
            {
                LogHelper.Log("Password prompt cancelled.");
            }
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