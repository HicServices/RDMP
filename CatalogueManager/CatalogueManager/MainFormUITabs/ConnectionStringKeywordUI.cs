using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.ConnectionStringDefaults;
using ReusableUIComponents;

namespace CatalogueManager.MainFormUITabs
{
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

            pbDatabaseProvider.Image = _activator.CoreIconProvider.GetImage(_keyword.DatabaseType);

            objectSaverButton1.SetupFor(databaseObject,activator.RefreshBus);
        }

        private void ddDatabaseType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if(_keyword == null)
                return;
            
            var type = (DatabaseType)ddDatabaseType.SelectedValue;
            _keyword.DatabaseType = type;
            pbDatabaseProvider.Image = _activator.CoreIconProvider.GetImage(type);
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            _keyword.Name = tbName.Text;
            Check();
        }

        private void tbValue_TextChanged(object sender, EventArgs e)
        {
            _keyword.Value = tbValue.Text;
            Check();
        }

        private void Check()
        {
            ragSmiley.StartChecking(_keyword);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ConnectionStringKeywordUI_Design, UserControl>))]
    public abstract class ConnectionStringKeywordUI_Design: RDMPSingleDatabaseObjectControl<ConnectionStringKeyword>
    {
    }
}
