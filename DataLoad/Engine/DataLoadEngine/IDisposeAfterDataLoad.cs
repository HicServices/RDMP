using CatalogueLibrary;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine
{
    /// <summary>
    /// Interface for all data load components which allows for post load cleanup (even if the load crashed).  See DataLoadProcess.
    /// </summary>
    public interface IDisposeAfterDataLoad
    {
        void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener);
    }
}
