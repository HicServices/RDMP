using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace CatalogueManager.CredentialsUIs
{
    /// <summary>
    /// Allows you to change a stored username/password (DataAccessCredentials).  For more information about how passwords are encrypted See PasswordEncryptionKeyLocationUI
    /// </summary>
    public partial class DataAccessCredentialsUI : DataAccessCredentialsUI_Design, ISaveableUI
    {
        public DataAccessCredentialsUI()
        {
            InitializeComponent();

            AssociatedCollection = RDMPCollection.Tables;
        }
        
        protected override void SetBindings(BinderWithErrorProviderFactory rules, DataAccessCredentials databaseObject)
        {
            base.SetBindings(rules, databaseObject);
            
            Bind(tbName,"Text","Name",c=>c.Name);
            Bind(tbUsername, "Text", "Username", c => c.Username);
            Bind(tbPassword, "Text", "Password", c => c.Password);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataAccessCredentialsUI_Design, UserControl>))]
    public abstract class DataAccessCredentialsUI_Design:RDMPSingleDatabaseObjectControl<DataAccessCredentials>
    {
    }
}
