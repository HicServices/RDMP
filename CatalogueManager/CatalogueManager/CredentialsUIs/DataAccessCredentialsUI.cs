using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.CredentialsUIs
{
    /// <summary>
    /// Allows you to change a stored username/password (DataAccessCredentials).  For more information about how passwords are encrypted See PasswordEncryptionKeyLocationUI
    /// </summary>
    public partial class DataAccessCredentialsUI : DataAccessCredentialsUI_Design, ISaveableUI
    {
        private DataAccessCredentials _credentials;
        
        public DataAccessCredentialsUI()
        {
            InitializeComponent();

            AssociatedCollection = RDMPCollection.Tables;
        }
        
        private bool _bLoading;
        public override void SetDatabaseObject(IActivateItems activator, DataAccessCredentials databaseObject)
        {
            _bLoading = true;
            try
            {
                base.SetDatabaseObject(activator,databaseObject);
            
                _credentials = databaseObject;

                tbName.Text = _credentials.Name;
                tbUsername.Text = _credentials.Username;
                tbPassword.Text = _credentials.Password;

                objectSaverButton1.SetupFor(databaseObject, activator.RefreshBus);
            }
            finally
            {
                _bLoading = false;
            }
        }

        private void tb_TextChanged(object sender, EventArgs e)
        {
            if(_bLoading)
                return;
            
            try
            {
                var tb = (TextBox)sender;

                if (tb == tbName)
                    _credentials.Name = tb.Text;
                if (tb == tbUsername)
                    _credentials.Username = tb.Text;
                if (tb == tbPassword)
                    _credentials.Password = tb.Text;
            
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataAccessCredentialsUI_Design, UserControl>))]
    public abstract class DataAccessCredentialsUI_Design:RDMPSingleDatabaseObjectControl<DataAccessCredentials>
    {
    }
}
