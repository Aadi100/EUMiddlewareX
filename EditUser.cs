using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class EditUser : Form
    {
        private int _userId;
        private DatabaseHelper _dbHelper;


        public EditUser(int userId, string currentUsername, string currentRole)
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _userId = userId;
            txt_UserName.Text = currentUsername;
            txt_Role.Text = currentRole;
        }

        private void txt_Role_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_UserName_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            string newUsername = txt_UserName.Text.Trim();
            string newRole = txt_Role.Text.Trim();

            if (!string.IsNullOrWhiteSpace(newUsername) && !string.IsNullOrWhiteSpace(newRole))
            {

                _dbHelper.UpdateUserInfo(_userId, newUsername, newRole);
                MessageBox.Show("User info updated successfully.");
                Close();
            }
            else
            {
                MessageBox.Show("Both username and role are required.");
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();

        }


    }
}
