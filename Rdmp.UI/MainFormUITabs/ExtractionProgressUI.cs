using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Checks;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.MainFormUITabs
{
    public partial class ExtractionProgressUI : ExtractionProgressUI_Design, ISaveableUI
    {
        public ExtractionProgress ExtractionProgress { get => (ExtractionProgress)DatabaseObject; }

        public ExtractionProgressUI()
        {
            InitializeComponent();
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, ExtractionProgress databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID, "Text", "ID", d => d.ID);
            Bind(tbDaysPerBatch, "Text", "NumberOfDaysPerBatch", d => d.NumberOfDaysPerBatch);
        }

        public override void SetDatabaseObject(IActivateItems activator, ExtractionProgress databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            

            tbStartDate.Text = databaseObject.StartDate == null ? "": databaseObject.StartDate.Value.ToString("yyyy-MM-dd");
            tbEndDate.Text = databaseObject.EndDate == null ? "" : databaseObject.EndDate.Value.ToString("yyyy-MM-dd");
            tbProgress.Text = databaseObject.ProgressDate == null ? "" : databaseObject.ProgressDate.Value.ToString("yyyy-MM-dd");

            var cata = databaseObject.SelectedDataSets.GetCatalogue();
            ddColumn.DataSource = cata.GetAllExtractionInformation();

            try
            {
                ddColumn.SelectedItem = databaseObject.ExtractionInformation;
                ragSmiley1.Reset();
                ExtractionProgress.ValidateSelectedColumn(ragSmiley1, databaseObject.ExtractionInformation);
            }
            catch (Exception)
            {
                // could be that the user has deleted this ExtractionInformation
                ddColumn.SelectedItem = null;
            }
        }


        private void tbDate_TextChanged(object sender, System.EventArgs e)
        {
            if(sender == tbStartDate)
            {
                SetDate(tbStartDate, (v) => ExtractionProgress.StartDate = v);
            }

            if (sender == tbEndDate)
            {
                SetDate(tbEndDate, (v) => ExtractionProgress.EndDate = v);
            }

            if (sender == tbProgress)
            {
                SetDate(tbProgress, (v) => ExtractionProgress.ProgressDate = v);
            }

        }

        private void ddColumn_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ragSmiley1.Reset();
            if(ddColumn.SelectedItem is ExtractionInformation ei)
            {
                ExtractionProgress.ValidateSelectedColumn(ragSmiley1, ei);
                ExtractionProgress.ExtractionInformation_ID = ei.ID;
            }
        }

        private void btnPickColumn_Click(object sender, EventArgs e)
        {
            var col  = (ExtractionInformation)Activator.SelectOne("Column", ddColumn.Items.Cast<object>().OfType<ExtractionInformation>().ToArray());

            if(col != null)
            {
                ddColumn.SelectedItem = col;
                ragSmiley1.Reset();
                ExtractionProgress.ValidateSelectedColumn(ragSmiley1, col);
                ExtractionProgress.ExtractionInformation_ID = col.ID;
            }
        }
    }


    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionProgressUI_Design, UserControl>))]
    public abstract class ExtractionProgressUI_Design : RDMPSingleDatabaseObjectControl<ExtractionProgress>
    {

    }
}
