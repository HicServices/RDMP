using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.CredentialsUIs
{
    /// <summary>
    /// Allows you to change a stored username/password (DataAccessCredentials).  For more information about how passwords are encrypted See PasswordEncryptionKeyLocationUI
    /// </summary>
    public partial class DataAccessCredentialsUI : DataAccessCredentialsUI_Design
    {
        private DataAccessCredentials _credentials;
        private bool _enableSaveUsernamePassword;
        
        public DataAccessCredentialsUI()
        {
            InitializeComponent();
        }

        private void UpdateFormComponents()
        {
            try
            {
                tbName.Text = _credentials.Name;
                tbUsername.Text = _credentials.Username;
                tbPassword.Text = _credentials.Password;

                if (_credentials.Username == null || _credentials.GetDecryptedPassword() == null)
                {
                    btnClearUsernamePassword.Visible = false;
                    tbUsername.Text = null;
                    tbPassword.Text = null;
                    tbUsername.ReadOnly = false;
                    tbPassword.ReadOnly = false;
                    _enableSaveUsernamePassword = true;
                }
                else
                {
                    btnClearUsernamePassword.Visible = true;
                    tbUsername.ReadOnly = true;
                    tbPassword.ReadOnly = true;
                    _enableSaveUsernamePassword = false;
                }

            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }
        
        private void btnClearUsernamePassword_Click(object sender, EventArgs e)
        {
            tbUsername.Text = null;
            tbPassword.Text = null;
            _credentials.Username = null;
            _credentials.Password = null;
            _credentials.SaveToDatabase();
            UpdateFormComponents();
        }

        private void EditCredentialControl_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _credentials.Name = tbName.Text;
                if (_enableSaveUsernamePassword == true)
                {
                    _credentials.Username = tbUsername.Text;
                    _credentials.Password = tbPassword.Text;
                }
                _credentials.SaveToDatabase();
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_credentials));
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        public override void SetDatabaseObject(IActivateItems activator, DataAccessCredentials databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            
            _credentials = databaseObject;
            _enableSaveUsernamePassword = true;
            UpdateFormComponents();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataAccessCredentialsUI_Design, UserControl>))]
    public abstract class DataAccessCredentialsUI_Design:RDMPSingleDatabaseObjectControl<DataAccessCredentials>
    {
    }
}
