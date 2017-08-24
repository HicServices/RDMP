using CohortManagerLibrary.Execution;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CohortManager.SubComponents.EmptyLineElements
{
    public class CohortIdentificationHeader
    {
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
            return "Cohort Identification Criteria";
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
    }
}
