using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Rules;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace CatalogueManager.ANOEngineeringUIs
{
    /// <summary>
    /// Displays the live status of an ANOTable (is it pushed or not) and how many rows it has.  Also allows dropping/changing the anonymisation schema (number of
    /// integers / characters to use in anonymous format) when the ANOTable is not pushed to the ANOStore.Database.
    /// </summary>
    public partial class ANOTableUI : ANOTableUI_Design
    {
        private ANOTable _anoTable;

        public ANOTableUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.Catalogue;
        }

        public override void SetDatabaseObject(IActivateItems activator, ANOTable databaseObject)
        {
            _anoTable = databaseObject;
            base.SetDatabaseObject(activator, databaseObject);

            lblServer.Text = _anoTable.Server.Name;

            AddChecks(databaseObject);
            StartChecking();

            SetEnabledness();
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, ANOTable databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID,"Text","ID",a=>a.ID);
            Bind(nIntegers,"Value","NumberOfIntegersToUseInAnonymousRepresentation",a=>a.NumberOfIntegersToUseInAnonymousRepresentation);
            Bind(nCharacters,"Value","NumberOfCharactersToUseInAnonymousRepresentation",a=>a.NumberOfCharactersToUseInAnonymousRepresentation);
            Bind(tbName,"Text","TableName",a=>a.TableName);
            Bind(tbSuffix,"Text","Suffix",a=>a.Suffix);
        }

        private void SetEnabledness()
        {
            var pushedTable = _anoTable.GetPushedTable();
            bool isPushed = pushedTable != null;

            nIntegers.Enabled = !isPushed;
            nCharacters.Enabled = !isPushed;
            btnFinalise.Enabled = !isPushed;
            tbInputDataType.Enabled = !isPushed;

            btnDropANOTable.Enabled = isPushed;
            gbPushedTable.Visible = isPushed;

            if (isPushed)
            {

                tbInputDataType.Text = _anoTable.GetRuntimeDataType(LoadStage.AdjustRaw);

                lblANOTableName.Text = pushedTable.GetRuntimeName();
                var cols = pushedTable.DiscoverColumns();

                lblPrivate.Text = cols[0].GetRuntimeName() + " " + cols[0].DataType.SQLType;
                lblPublic.Text = cols[1].GetRuntimeName() + " " + cols[1].DataType.SQLType;

                lblRowCount.Text = pushedTable.GetRowCount() + " rows";
            }
        }

        private void btnFinalise_Click(object sender, EventArgs e)
        {
            ragSmiley1.Reset();
            _anoTable.PushToANOServerAsNewTable(tbInputDataType.Text,ragSmiley1);
            SetEnabledness();
        }

        private void btnDropANOTable_Click(object sender, EventArgs e)
        {
            ragSmiley1.Reset();
            try
            {
                _anoTable.DeleteANOTableInANOStore();
            }
            catch (Exception exception)
            {
                ragSmiley1.OnCheckPerformed(new CheckEventArgs("Drop failed", CheckResult.Fail, exception));
            }
            SetEnabledness();
        }

        private void nIntegers_ValueChanged(object sender, EventArgs e)
        {
            _anoTable.NumberOfIntegersToUseInAnonymousRepresentation = (int) nIntegers.Value;

        }

        private void nCharacters_ValueChanged(object sender, EventArgs e)
        {
            _anoTable.NumberOfCharactersToUseInAnonymousRepresentation = (int)nCharacters.Value;
        }
    }
    
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ANOTableUI_Design, UserControl>))]
    public abstract class ANOTableUI_Design : RDMPSingleDatabaseObjectControl<ANOTable>
    {
    }
}
