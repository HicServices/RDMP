using CatalogueLibrary.Nodes;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.Providers.Nodes
{
    /// <summary>
    /// Collection of all saved cohort lists (See <see cref="ExtractableCohort"/>).  These are divided by sources (<see cref="ExternalCohortTable"/>)
    /// </summary>
    public class AllCohortsNode: SingletonNode
    {
        public AllCohortsNode()
            : base("ALL Saved Cohorts")
        {
            
        }
    }
}
