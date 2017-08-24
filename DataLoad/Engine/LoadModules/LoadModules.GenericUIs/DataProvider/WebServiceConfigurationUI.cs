using System;
using System.ComponentModel.Composition;
using System.Data;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using LoadModules.Generic.DataProvider;

namespace LoadModules.GenericUIs.DataProvider
{
    /// <summary>
    /// Allows you to specify and store an encrypted set of credentials in the Catalogue database for a web service endpoint.  The exact interpretation of Endpoint, MaxBufferSize and 
    /// MaxReceivedMessageSize are up to the specific use case of the dialog.  The dialog allows [DemandsInitialization] arguments of plugin classes to securely store the location of 
    /// a web service in the Catalogue database.
    ///</summary>
    [Export(typeof(ICustomUI<>))]
    public partial class WebServiceConfigurationUI : Form, ICustomUI<WebServiceConfiguration>
    {
        public ICatalogueRepository CatalogueRepository { get; set; }

        public WebServiceConfigurationUI()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }
        
        public void SetGenericUnderlyingObjectTo(ICustomUIDrivenClass value, DataTable previewIfAvailable)
        {
            SetUnderlyingObjectTo((WebServiceConfiguration)value, previewIfAvailable);
        }

        public ICustomUIDrivenClass GetGenericFinalStateOfUnderlyingObject()
        {
            return GetFinalStateOfUnderlyingObject();
        }

        public void SetUnderlyingObjectTo(WebServiceConfiguration value, DataTable previewIfAvailable)
        {
            var config = value ?? new WebServiceConfiguration(CatalogueRepository);
            tbEndpoint.Text = config.Endpoint;
            tbUsername.Text = config.Username;

            try
            {
                tbPassword.Text = config.GetDecryptedPassword();
            }
            catch (Exception)
            {
                if (
                    MessageBox.Show("Could not decrypt password, would you like to clear it?", "Clear Password",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    config.Password = "";
                else
                    throw;
            }
            tbMaxBufferSize.Text = config.MaxBufferSize.ToString();
            tbMaxReceivedMessageSize.Text = config.MaxReceivedMessageSize.ToString();
        }

        public WebServiceConfiguration GetFinalStateOfUnderlyingObject()
        {
            return new WebServiceConfiguration(CatalogueRepository)
            {
                Endpoint = tbEndpoint.Text,
                Username = tbUsername.Text,
                Password = tbPassword.Text,
                MaxBufferSize = Convert.ToInt32(tbMaxBufferSize.Text),
                MaxReceivedMessageSize = Convert.ToInt32(tbMaxReceivedMessageSize.Text)
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void WebServiceConfigurationUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CatalogueRepository == null)
                return;

            if (DialogResult != DialogResult.OK)
                if (MessageBox.Show("Close without saving?", "Cancel Changes", MessageBoxButtons.YesNo) !=
                    DialogResult.Yes)
                    e.Cancel = true;
        }
    }
}
