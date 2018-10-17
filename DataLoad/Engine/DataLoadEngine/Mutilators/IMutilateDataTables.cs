using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Mutilators
{
    /// <summary>
    /// A user configurable component which will run during Data Load Engine execution and result in the modification of an existing table in of the load
    /// stages (RAW, STAGING or LIVE).  For example a PrimaryKeyCollisionResolverMutilation will delete records out of the table such that the Primary Key
    /// is unique (based on a column preference order). 
    /// </summary>
    public interface IMutilateDataTables : ICheckable, IDisposeAfterDataLoad
    {
        /// <summary>
        /// Called after construction to tell you where you will be running.  Note that at Checks time this might not exist yet (if you are in RAW/STAGING)
        /// </summary>
        /// <param name="dbInfo"></param>
        /// <param name="loadStage"></param>
        void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage);
        ExitCodeType Mutilate(IDataLoadJob job);
    }
}
