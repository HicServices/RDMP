using System;
using System.Diagnostics;
using System.Windows.Forms;
using ReusableLibraryCode.Settings;
using ReusableUIComponents;

namespace ResearchDataManagementPlatform.WindowManagement.Licenses
{

    /// <summary>
    /// Displays the open source license for RDMP and so shows the license for all the third party plugins.  You must either accept or decline the license .
    /// Declining will close the Form.  This form is shown for the first time on startup or again any time you have declined the conditions.
    /// </summary>
    public partial class LicenseUI : Form
    {
        public LicenseUI()
        {
            InitializeComponent();

            try
            {
                _main = new License("LICENSE");
                _thirdParth = new License("LIBRARYLICENSES");

                rtLicense.Text = _main.GetLicenseText();
                rtThirdPartyLicense.Text = _thirdParth.GetLicenseText();
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);   
            }
        }

        
        private bool allowClose = false;

        private License _main;
        private License _thirdParth;

        private void btnAccept_Click(object sender, EventArgs e)
        {
            UserSettings.LicenseAccepted = _thirdParth.GetMd5OfLicense();
            allowClose = true;
            this.Close();
        }

        private void btnDecline_Click(object sender, EventArgs e)
        {
            UserSettings.LicenseAccepted = null;
            allowClose = true;
            Process.GetCurrentProcess().Kill();
        }

        private void LicenseUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UserSettings.LicenseAccepted != _thirdParth.GetMd5OfLicense() && !allowClose)
            {
                e.Cancel = true;
                MessageBox.Show("You have not accepted/declined the license");
            }
        }
    }
}
