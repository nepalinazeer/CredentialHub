using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CredentialHub.Views
{
    public partial class AddDialog : Form
    {
        public string FileName { get; set; }

        private List<Credential> _creds = null;
        private bool _credAdded = false;

        public AddDialog(string fileName)
        {
            InitializeComponent();
            try
            {
                FileName = fileName;
                if(File.Exists(FileName))
                    _creds = (List<Credential>)FileEncryptionRijndael.ReadFromFileAndDecrypt(FileName);
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

        private void btnAddCred_Click(object sender, EventArgs e)
        {
                        
            try
            {
                if(_creds == null)
                    _creds = new List<Credential>();

                if (string.IsNullOrEmpty(txtBoxName.Text.Trim()))
                {
                    MessageBox.Show("Name field cannot be empty.");
                    return;
                }

                var cred = _creds.FirstOrDefault(x => x.Name.Equals(txtBoxName.Text.Trim()));

                if (cred == null)
                {
                    _creds.Add(new Credential()
                    {
                        Id = Guid.NewGuid(),
                        Name = txtBoxName.Text.Trim(),
                        UserName = txtBoxUserName.Text.Trim(),
                        Password = txtBoxPassword.Text.Trim(),
                        Url = txtBoxURL.Text.Trim(),
                        Comment = txtBoxComment.Text.Trim()

                    });

                    _credAdded = true;
                    ClearControls();
                }
                else
                {
                    MessageBox.Show(string.Format("Credential with name {0} already exists.\nPlease try a different name.", txtBoxName.Text.Trim()));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void AddDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_credAdded)
                {
                    FileEncryptionRijndael.EncryptAndSaveToFile(_creds, FileName);

                    MainForm parent = (MainForm) this.Owner;
                    parent.LoadCredentials();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearControls()
        {
            txtBoxName.Text = string.Empty;
            txtBoxUserName.Text = string.Empty;
            txtBoxPassword.Text = string.Empty;
            txtBoxURL.Text = string.Empty;
            txtBoxComment.Text = string.Empty;
        }

        private void txtBoxName_TextChanged(object sender, EventArgs e)
        {
            btnAddCred.Enabled = !string.IsNullOrEmpty(txtBoxName.Text.Trim());
        }
    }
}
