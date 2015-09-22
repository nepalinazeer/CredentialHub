using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CredentialHub.Views
{
    public partial class UpdateDialog : Form
    {
        public string FileName { get; set; }

        private List<Credential> _creds = null;
        private Guid _selectedId = Guid.Empty;
        private bool _credUdated = false;

        public UpdateDialog(string fileName, Credential cred)
        {
            InitializeComponent();

            FileName = fileName;

            InitializeUI(cred);

        }

        /// <summary>
        /// Initialize UI controls with the information of credential to be updated
        /// </summary>
        /// <param name="cred">Selected credential from MainForm</param>
        private void InitializeUI(Credential cred)
        {
            try
            {
                if (File.Exists(FileName) && cred != null)
                {
                    _creds = (List<Credential>) FileEncryptionRijndael.ReadFromFileAndDecrypt(FileName);

                    _selectedId = cred.Id;
                    txtBoxName.Text = cred.Name;
                    txtBoxUserName.Text = cred.UserName;
                    txtBoxPassword.Text = cred.Password;
                    txtBoxURL.Text = cred.Url;
                    txtBoxComment.Text = cred.Comment;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       
        /// <summary>
        /// Event when update button is clicked
        /// Updates selected credential in the credential list
        /// </summary>
        private void btnUpdateCred_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedId != Guid.Empty)
                {
                    var credential = _creds.FirstOrDefault(x => x.Id == _selectedId);

                    if (credential != null)
                    {
                        if (!string.IsNullOrEmpty(txtBoxName.Text.Trim()))
                        {
                            credential.Name = txtBoxName.Text.Trim();
                            credential.UserName = txtBoxUserName.Text.Trim();
                            credential.Password = txtBoxPassword.Text.Trim();
                            credential.Url = txtBoxURL.Text.Trim();
                            credential.Comment = txtBoxComment.Text.Trim();

                            _credUdated = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Name field cannot be empty.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       
       /// <summary>
       /// Stores updated credential to the file
       /// </summary>
        private void UpdateDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_credUdated)
                {
                    FileEncryptionRijndael.EncryptAndSaveToFile(_creds, FileName);

                    MainForm parent = (MainForm)this.Owner;
                    parent.LoadCredentials();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
