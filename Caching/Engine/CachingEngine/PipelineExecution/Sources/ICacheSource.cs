using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;

namespace CachingEngine.PipelineExecution.Sources
{
    public interface ICacheSource : IPipelineRequirement<ICacheFetchRequestProvider>, IPipelineRequirement<IPermissionWindow>
    {
    }
}