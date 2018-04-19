using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;

namespace CachingEngine.PipelineExecution.Sources
{
    /// <summary>
    /// Interface for abstract base CacheSource (See CacheSource for description).  All CacheSources should be exposed to and consider both the ICacheFetchRequestProvider
    /// (which tells you what date/time you are supposed to be fetching) and IPermissionWindow (which tells you what real time window you can make requests during e.g. only
    /// attempt to cache data between 9am and 5pm at night)
    /// </summary>
    public interface ICacheSource : IPipelineRequirement<ICacheFetchRequestProvider>, IPipelineRequirement<IPermissionWindow>
    {
    }
}