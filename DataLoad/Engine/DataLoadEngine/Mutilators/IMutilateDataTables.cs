using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Mutilators
{
    public interface IMutilateDataTables : ICheckable, IDisposeAfterDataLoad
    {
        /// <summary>
        /// Called after construction to tell you where you will be running.  Note that at Checks time this might not exist yet (if you are in RAW/STAGING)
        /// </summary>
        /// <param name="dbInfo"></param>
        /// <param name="loadStage"></param>
        void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage);
        ExitCodeType Mutilate(IDataLoadEventListener job);
    }
}
