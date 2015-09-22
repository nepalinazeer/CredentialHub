using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CredentialHub.Views;

namespace CredentialHub
{
    public partial class MainForm : Form
    {
        private const string FileName = "credentials.crhub";
        private List<Credential> _credentials; 

        public MainForm()
        {
            InitializeComponent();

            _credentials = new List<Credential>();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadCredentials();
        }


        #region MainView Tab       

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadCredentials();
        }

        /// <summary>
        /// Loads credentials from the credential file and populates grid view
        /// </summary>
        public void LoadCredentials()
        {
            try
            {
                _credentials = (List<Credential>) FileEncryptionRijndael.ReadFromFileAndDecrypt(FileName);
                var bindingList = new BindingList<Credential>(_credentials);
                var source = new BindingSource(bindingList, null);
                dgViewCredentials.DataSource = source;
                ShowHidePassword();
                dgViewCredentials.Columns["Id"].Visible = false;

                //set some default column width
                for (int i = 0; i < dgViewCredentials.Columns.Count; i++)
                {
                    dgViewCredentials.Columns[i].Width = 230;
                }

            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("You haven't added any credentials yet.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        /// <summary>
        /// Event when a grid cell is clicked
        /// If the cell happens to be from the Url column, it launches firefox, enters the url and 
        /// attempts to enter username and password provided for auto login
        /// Does this using selenium webdriver.
        /// Also updates color of currently selected row to red
        /// </summary>
        private void dgViewCredentials_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                ResetDataGridViewStyle();

                if (e.RowIndex != -1)
                {
                    dgViewCredentials.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
                    string url = dgViewCredentials.Rows[e.RowIndex].Cells[4].Value.ToString();

                    if (dgViewCredentials.CurrentCell.ColumnIndex.Equals(4) && !string.IsNullOrEmpty(url))
                    {                                               
                        OpenQA.Selenium.IWebDriver driver = new OpenQA.Selenium.Firefox.FirefoxDriver();
                        driver.Navigate().GoToUrl(url);
                        driver.Manage().Window.Maximize();

                        string usernameField = string.Empty;;
                        string passwordField = string.Empty;
                        var allInputFields = driver.FindElements(OpenQA.Selenium.By.XPath("//input"));

                        foreach (var field in allInputFields)
                        {
                           
                            if (string.IsNullOrEmpty(usernameField) & 
                                (field.GetAttribute("type").Equals("text") || field.GetAttribute("type").Equals("email")) &
                                (!field.GetAttribute("title").Equals("search") & !field.GetAttribute("name").Equals("q") & !field.GetAttribute("name").Equals("search")))
                            {
                                usernameField = field.GetAttribute("name");
                                
                            }
                           

                            if (string.IsNullOrEmpty(passwordField) & field.GetAttribute("type").Equals("password"))
                            {                                
                                passwordField = field.GetAttribute("name");
                           
                            }
                            
                           
                        }

                        /*
                        MessageBox.Show("Current fields\n" +
                                        "Username: " + usernameField + "\n" +
                                        "Password:" + passwordField);
                         * */
                                                
                         OpenQA.Selenium.IWebElement searchInput = null;
                         if (!string.IsNullOrEmpty(usernameField))
                         {
                             searchInput = driver.FindElement(OpenQA.Selenium.By.Name(usernameField));
                             searchInput.SendKeys(dgViewCredentials.Rows[e.RowIndex].Cells[2].Value.ToString());
                         }

                         if (!string.IsNullOrEmpty(passwordField))
                         {
                             searchInput = driver.FindElement(OpenQA.Selenium.By.Name(passwordField));
                             searchInput.SendKeys(dgViewCredentials.Rows[e.RowIndex].Cells[3].Value.ToString());
                         }

                         if (!string.IsNullOrEmpty(usernameField) & !string.IsNullOrEmpty(passwordField))
                         {
                             searchInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                         }

                         usernameField = string.Empty; 
                         passwordField = string.Empty;

                         driver = null;
                        
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Resets backcolor of all rows to white, which is the default color
        /// </summary>
        private void ResetDataGridViewStyle()
        {
            foreach (DataGridViewRow row in dgViewCredentials.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }
        }

        /// <summary>
        /// Depending on the checkbox status, shows/hide password column in grid
        /// </summary>
        private void ShowHidePassword()
        {
            try
            {
                dgViewCredentials.Columns["Password"].Visible = chkBoxShowPassword.Checked;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
                    
        }

        /// <summary>
        /// Providese right-click context menu option with ability to edit/delete credentials from the grid
        /// </summary>
        private void dgViewCredentials_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ResetDataGridViewStyle();

                int currentMouseOverRow = dgViewCredentials.HitTest(e.X, e.Y).RowIndex;
                dgViewCredentials.Rows[currentMouseOverRow].DefaultCellStyle.BackColor = Color.Red;

                ContextMenu m = new ContextMenu();
                m.MenuItems.Add((new MenuItem("Delete", (o, ev) =>
                {
                    var cred = _credentials[currentMouseOverRow];
                    if (cred != null)
                    {
                        var confirmDelete = MessageBox.Show(string.Format("Delete credential {0}? ", cred.Name) ,
                                            "Confirm Delete!!", MessageBoxButtons.YesNo);

                        if (confirmDelete == DialogResult.Yes)
                        {
                           _credentials.Remove(cred);
                            FileEncryptionRijndael.EncryptAndSaveToFile(_credentials, FileName);
                            LoadCredentials();
                        }
                    }
                })));

                m.MenuItems.Add(new MenuItem("Edit", (o, ev) =>
                {
                    UpdateDialog ud = new UpdateDialog(FileName, _credentials[currentMouseOverRow]);
                    ud.StartPosition = FormStartPosition.Manual;
                    ud.Location = new Point(this.Location.X + (this.Width - ud.Width) / 2, this.Location.Y + (this.Height - ud.Height) / 2);
                    ud.Show(this);
                }));

                m.Show(dgViewCredentials, new Point(e.X, e.Y));
            }
        }
        #endregion

        private void addNewCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddDialog ad = new AddDialog(FileName);
            ad.StartPosition = FormStartPosition.Manual;
            ad.Location = new Point(this.Location.X + (this.Width - ad.Width) / 2, this.Location.Y + (this.Height - ad.Height) / 2);
            ad.Show(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void chkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            ShowHidePassword();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.StartPosition = FormStartPosition.Manual;
            about.Location = new Point(this.Location.X + (this.Width - about.Width) / 2, this.Location.Y + (this.Height - about.Height) / 2);
            about.Show(this);
        }

       

    }
}
