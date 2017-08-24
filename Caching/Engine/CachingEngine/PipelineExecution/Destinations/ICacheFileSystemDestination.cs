using System.ComponentModel.Composition;
using CachingEngine.Layouts;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;

namespace CachingEngine.PipelineExecution.Destinations
{
    public interface ICacheFileSystemDestination : IPipelineRequirement<IHICProjectDirectory>
    {
        ICacheLayout CreateCacheLayout();
    }
}