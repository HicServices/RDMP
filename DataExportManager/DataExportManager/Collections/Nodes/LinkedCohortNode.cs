using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportManager.Collections.Nodes
{
    public class LinkedCohortNode
    {
        public ExtractionConfiguration Configuration { get; set; }
        public IExtractableCohort Cohort { get; set; }

        public LinkedCohortNode(ExtractionConfiguration configuration, IExtractableCohort cohort)
        {
            Configuration = configuration;
            Cohort = cohort;
        }

        public override string ToString()
        {
            return Cohort.ToString();
        }

        public void DeleteWithConfirmation(IActivateItems activator)
        {
            if (MessageBox.Show(
                            "Clear Cohort on ExtractionConfiguration '" + Configuration +
                            "'? (Does not delete Cohort)", "Confirm breaking link to Cohort", MessageBoxButtons.YesNo) ==
                        DialogResult.Yes)
            {
                Configuration.Cohort_ID = null;
                Configuration.SaveToDatabase();
                activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(Configuration));
            }
        }
    }
}
