using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CredentialHub.Views
{
    public partial class LoginDialog : Form
    {
        private string password = "1234";
        public LoginDialog()
        {
            InitializeComponent();
            lblError.Text = "";
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!txtBoxPassword.Text.Equals(password))
                lblError.Text = "Invalid key. Please try again!";
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
