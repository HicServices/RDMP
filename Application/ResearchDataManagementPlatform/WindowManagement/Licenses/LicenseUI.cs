using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ResearchDataManagementPlatform.WindowManagement.UserSettings;
using ReusableUIComponents;

namespace ResearchDataManagementPlatform.WindowManagement.Licenses
{
    public partial class LicenseUI : Form
    {
        public LicenseUI()
        {
            InitializeComponent();

            try
            {
                rtLicense.Text = GetTextFrom("LICENSE");
                rtThirdPartyLicense.Text = GetTextFrom("LIBRARYLICENSES");
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);   
            }
        }

        private string GetTextFrom(string resourceFilename)
        {
            string expectedResourceName = "ResearchDataManagementPlatform.WindowManagement.Licenses." + resourceFilename;
            using (var stream = typeof(LicenseUI).Assembly.GetManifestResourceStream(expectedResourceName))
            {
                if(stream == null)
                    throw new Exception("Could not find EmbeddedResource '" + expectedResourceName + "'");

                return new StreamReader(stream).ReadToEnd();
            }
        }

        private bool allowClose = false;
        private void btnAccept_Click(object sender, EventArgs e)
        {
            UserSettingsFile.GetInstance().LicenseAccepted = true;
            allowClose = true;
            this.Close();
        }

        private void btnDecline_Click(object sender, EventArgs e)
        {
            UserSettingsFile.GetInstance().LicenseAccepted = false;
            allowClose = true;
            Process.GetCurrentProcess().Kill();
        }

        private void LicenseUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!UserSettingsFile.GetInstance().LicenseAccepted && !allowClose)
            {
                e.Cancel = true;
                MessageBox.Show("You have not accepted/declined the license");
            }


        }
    }
}
