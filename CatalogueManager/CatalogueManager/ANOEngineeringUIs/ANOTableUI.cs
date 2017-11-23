using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace CatalogueManager.ANOEngineeringUIs
{
    public partial class ANOTableUI : ANOTableUI_Design
    {
        private ANOTable _anoTable;

        public ANOTableUI()
        {
            InitializeComponent();
        }

        public override void SetDatabaseObject(IActivateItems activator, ANOTable databaseObject)
        {
            _anoTable = databaseObject;
            base.SetDatabaseObject(activator, databaseObject);

            tbID.Text = _anoTable.ID.ToString();
            nIntegers.Value = _anoTable.NumberOfIntegersToUseInAnonymousRepresentation;
            nCharacters.Value = _anoTable.NumberOfCharactersToUseInAnonymousRepresentation;
            tbName.Text = _anoTable.TableName;
            tbSuffix.Text = _anoTable.Suffix;
            
            lblServer.Text = _anoTable.Server.Name;

            SetEnabledness();
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
