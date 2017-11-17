using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportManager.Collections.Nodes
{
    public class LinkedCohortNode:IMasqueradeAs
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

        public object MasqueradingAs()
        {
            return Cohort;
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

        protected bool Equals(LinkedCohortNode other)
        {
            return Equals(Configuration, other.Configuration) && Equals(Cohort, other.Cohort);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LinkedCohortNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Configuration != null ? Configuration.GetHashCode() : 0)*397) ^ (Cohort != null ? Cohort.GetHashCode() : 0);
            }
        }
    }
}
