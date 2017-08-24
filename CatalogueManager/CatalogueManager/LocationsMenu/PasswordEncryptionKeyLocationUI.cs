using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.LocationsMenu
{
    /// <summary>
    /// RDMP supports both Integrated Security (Windows User Account Security) and SQL Authentication.  The later requires the storing of usernames and passwords for sending at query time
    /// to the destination server.  In order to do this in a secure way RDMP encrypts them using 4096-bit RSA public/private key encryption.  By default this will use a PrivateKey that is
    /// part of the RDMP codebase but it is recommended that you create your own Private Key.  Without your own private key someone could decompile this software and decrypt the passwords
    /// stored in your RDMP database if it ever became compromised.
    /// 
    /// This control lets you create a custom 4096 bit RSA Private Key file.  The location of this file is stored in the RDMP database but the file itself should be held under access 
    /// control (see UserManual.docx).  This ensures that passwords are only compromised if both the RDMP database and the Windows user account file system (where the private key is held)
    /// are both compromised.
    /// 
    /// It is only possible to have one key at any one time and once you generate a new one all your previously created passwords will be irretrievable so it is advisable to set this up
    /// on day one otherwise you will have to reset all the passwords stored in the RDMP database.
    /// </summary>
    public partial class PasswordEncryptionKeyLocationUI : RDMPUserControl
    {
        private PasswordEncryptionKeyLocation _location;

        public PasswordEncryptionKeyLocationUI()
        {
            InitializeComponent();
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (VisualStudioDesignMode) //dont go looking up the key if you are visual studio in designer mode!
                return;

            _location = new PasswordEncryptionKeyLocation(RepositoryLocator.CatalogueRepository);

            SetEnabledness();
        }

        private void SetEnabledness()
        {
            string keyLocation = _location.GetKeyFileLocation();
            tbCertificate.Text = keyLocation;

            btnShowDirectory.Enabled = keyLocation != null;
            btnDeleteKeyLocation.Enabled = keyLocation != null;
            btnCreateKeyFile.Enabled = keyLocation == null;

        }

        private void tbCertificate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _location.ChangeLocation(tbCertificate.Text);
                lblLocationInvalid.Text = "";
                btnShowDirectory.Enabled = true;
            }
            catch (Exception ex)
            {
                lblLocationInvalid.Text = ex.Message;
                lblLocationInvalid.ForeColor = Color.Red;
                btnShowDirectory.Enabled = false;
            }
        }

        private void btnShowDirectory_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new FileInfo(tbCertificate.Text).Directory.FullName);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

            
        }

        private void btnCreateKeyFile_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = "MyRDMPKey.key";
                sfd.Filter = "*.key|RDMP RSA Parameters Key File";
                sfd.CreatePrompt = true;
                sfd.CheckPathExists = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                    _location.CreateNewKeyFile(sfd.FileName);

                SetEnabledness();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void btnDeleteKeyLocation_Click(object sender, EventArgs e)
        {
            try
            {
                if(MessageBox.Show(
                    "You are about to delete the RDMPs record of where the key file is to decrypt passwords, if you do this all currently configured password will become inaccessible (EVEN IF YOU CREATE A NEW KEY, YOU WILL NOT BE ABLE TO GET THE CURRENT PASSWORDS BACK), are you sure you want to do this?",
                    "Confirm deleting location of decryption key file", MessageBoxButtons.YesNo, MessageBoxIcon.Stop) == DialogResult.Yes)
                    _location.DeleteKey();

                SetEnabledness();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

            

        }

        
    }
}
