using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Providers.Nodes
{
    public class LinkedCohortNode : IMasqueradeAs, IDeletableWithCustomMessage
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

        public void DeleteInDatabase()
        {
            Configuration.Cohort_ID = null;
            Configuration.SaveToDatabase();
        }

        public string GetDeleteMessage()
        {
            return "clear the cohort for ExtractionConfiguration '" + Configuration + "'";
        }
    }
}
