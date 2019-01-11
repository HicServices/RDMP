using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using FAnsi.Discovery;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.DataProvider
{
    /// <summary>
    /// DLE component ostensibly responsible for 'fetching data'.  This typically involves fetching data and saving it into the IHICProjectDirectory (e.g. into 
    /// ForLoading) ready for loading by later components.
    /// </summary>
    public interface IDataProvider : IDisposeAfterDataLoad,ICheckable
    {
        void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo);
        ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken);
    }
}