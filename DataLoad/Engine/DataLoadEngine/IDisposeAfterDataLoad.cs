using CatalogueLibrary;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine
{
    public interface IDisposeAfterDataLoad
    {
        void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener);
    }
}
