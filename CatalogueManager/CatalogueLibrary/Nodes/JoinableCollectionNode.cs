using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Nodes
{
    public class JoinableCollectionNode:IOrderable
    {
        public CohortIdentificationConfiguration Configuration { get; set; }
        public JoinableCohortAggregateConfiguration[] Joinables { get; set; }

        public JoinableCollectionNode(CohortIdentificationConfiguration configuration, JoinableCohortAggregateConfiguration[] joinables)
        {
            Configuration = configuration;
            Joinables = joinables;
        }

        public string GetCatalogueName()
        {
            return "";
        }

        public IMapsDirectlyToDatabaseTable Child
        {
            get { return null; }
        }

        public IDataAccessPoint[] GetDataAccessPoints()
        {
            return null;
        }

        public override string ToString()
        {
            return "Patient Index Table(s)";
        }

        public string FinalRowCount()
        {
            return "";
        }
        public int? CumulativeRowCount { set; get; }
        

        public string GetStateDescription()
        {
            return "";
        }

        public string Order()
        {
            return "";
        }
        
        public string ElapsedTime = "";

        public string GetCachedQueryUseCount()
        {
            return "";
        }

        public string DescribePurpose()
        {

            return @"Drop Aggregates (datasets) here to create patient index tables (Tables with interesting
patient specific dates/fields which you need to use in other datasets). For example if you are
interested in studying hospitalisations for condition X and all other patient identification 
criteria are 'in the 6 months' / 'in the 12 months' post hospitalisation date per patient)";
        }

        protected bool Equals(JoinableCollectionNode other)
        {
            return Equals(Configuration, other.Configuration);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JoinableCollectionNode) obj);
        }

        public override int GetHashCode()
        {
            return (Configuration != null ? Configuration.GetHashCode() : 0) * GetType().GetHashCode();
        }

        int IOrderable.Order
        {
            get { return 9999; }
            set { }
        }
    }
}
