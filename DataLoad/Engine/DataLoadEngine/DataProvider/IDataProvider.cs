using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.DataProvider
{
    public interface IDataProvider : IDisposeAfterDataLoad,ICheckable
    {
        void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo);
        ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken);
    }
}