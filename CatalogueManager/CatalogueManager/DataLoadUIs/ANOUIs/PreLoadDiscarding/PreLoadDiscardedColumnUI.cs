using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding
{
    /// <summary>
    /// Lets you configure a column in a dataset as discarded during the load process (either completely - Oblivion or stored in an identifiers only area - StoreInIdentifiersDump).  For
    /// full details of how to configure a PreLoadDiscardedColumn see ConfigurePreLoadDiscardedColumns
    /// </summary>
    public partial class PreLoadDiscardedColumnUI : PreLoadDiscardedColumnUI_Design, ISaveableUI
    {
        private PreLoadDiscardedColumn _preLoadDiscardedColumn;
        public event EventHandler Saved;


        public PreLoadDiscardedColumnUI()
        {
            InitializeComponent();
            ddDestination.DataSource = Enum.GetValues(typeof (DiscardedColumnDestination));
            AssociatedCollection = RDMPCollection.Tables;
        }

        public override void SetDatabaseObject(IActivateItems activator, PreLoadDiscardedColumn databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _preLoadDiscardedColumn = databaseObject;

            tbID.Text = _preLoadDiscardedColumn.ID.ToString();
            tbRuntimeColumnName.Text = _preLoadDiscardedColumn.RuntimeColumnName;
            tbSqlDataType.Text = _preLoadDiscardedColumn.SqlDataType;
            ddDestination.SelectedItem = _preLoadDiscardedColumn.Destination;

            objectSaverButton1.SetupFor(_preLoadDiscardedColumn,activator.RefreshBus);
        }

        private void ddDestination_SelectedIndexChanged(object sender, EventArgs e)
        {
           _preLoadDiscardedColumn.Destination = (DiscardedColumnDestination) ddDestination.SelectedItem;
        }

        private void tbRuntimeColumnName_TextChanged(object sender, EventArgs e)
        {
           _preLoadDiscardedColumn.RuntimeColumnName = tbRuntimeColumnName.Text;
        }
        
        private void tbSqlDataType_TextChanged(object sender, EventArgs e)
        {
            _preLoadDiscardedColumn.SqlDataType = tbSqlDataType.Text;

            if(!string.IsNullOrWhiteSpace(tbSqlDataType.Text))
            {
                lblErrorInType.Visible = false;
                try
                {
                    SMOTypeLookup lookup = new SMOTypeLookup();
                    lookup.GetSMODataTypeForSqlStringDataType(tbSqlDataType.Text);
                }
                catch (Exception ex)
                {
                    lblErrorInType.Visible = true;
                    lblErrorInType.Text = ex.Message;
                }
            }
            
        }

        private void RunChecks()
        {
            IdentifierDumper dumper;
            try
            {
                dumper = new IdentifierDumper((TableInfo) _preLoadDiscardedColumn.TableInfo);
            }
            catch (Exception e)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail, e));
                return;
            }
            
            checksUI1.StartChecking(dumper);
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            RunChecks();
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<PreLoadDiscardedColumnUI_Design, UserControl>))]
    public abstract class PreLoadDiscardedColumnUI_Design : RDMPSingleDatabaseObjectControl<PreLoadDiscardedColumn>
    {
        
    }
}

