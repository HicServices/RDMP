using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using FAnsi;
using ReusableLibraryCode;
using ReusableUIComponents;

namespace CatalogueManager.MainFormUITabs
{
    /// <summary>
    /// Allows you to set up a <see cref="ConnectionStringKeyword"/> which will be used with all connections made against databases of the given <see cref="DatabaseType"/>.
    /// Take great care when doing this as you can easily render your datasources unreachable by all system users.
    /// </summary>
    public partial class ConnectionStringKeywordUI : ConnectionStringKeywordUI_Design, ISaveableUI
    {
        private ConnectionStringKeyword _keyword;

        public ConnectionStringKeywordUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.Tables;

            ddDatabaseType.DataSource = Enum.GetValues(typeof(DatabaseType));
        }

        public override void SetDatabaseObject(IActivateItems activator, ConnectionStringKeyword databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _keyword = databaseObject;

            ddDatabaseType.SelectedItem = _keyword.DatabaseType;
            tbName.Text = _keyword.Name;
            tbValue.Text = _keyword.Value;
            tbID.Text = _keyword.ID.ToString();

            pbDatabaseProvider.Image = _activator.CoreIconProvider.GetImage(_keyword.DatabaseType);
            
            tbCommandToDelete.Text = "DELETE FROM " + 
                activator.RepositoryLocator.CatalogueRepository.DiscoveredServer.GetCurrentDatabase()
                    .ExpectTable("ConnectionStringKeyword")
                    .GetFullyQualifiedName();

            AddChecks(databaseObject);
            StartChecking();
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, ConnectionStringKeyword databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID, "Text", "ID", k => k.ID);
            Bind(tbName,"Text","Name",k=>k.Name);
            Bind(tbValue, "Text", "Value", k => k.Value);
            Bind(ddDatabaseType, "Text", "DatabaseType", k => k.DatabaseType);
        }

        private void ddDatabaseType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if(_keyword == null)
                return;
            
            var type = (DatabaseType)ddDatabaseType.SelectedValue;
            pbDatabaseProvider.Image = _activator.CoreIconProvider.GetImage(type);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ConnectionStringKeywordUI_Design, UserControl>))]
    public abstract class ConnectionStringKeywordUI_Design: RDMPSingleDatabaseObjectControl<ConnectionStringKeyword>
    {
    }
}
