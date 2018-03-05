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
    /// <summary>
    /// Interface for minimum requirements of a cache destination pipeline component.  See abstract base class CacheFilesystemDestination for details on what this is.  This
    /// interface exists so that DLE (and other) processes can process the pipeline destination (e.g. to read files out of it again) without having to know the exact caching
    /// context etc.  Any ICacheFileSystemDestination must be able to CreateCacheLayout based solely on an IHICProjectDirectory
    /// </summary>
    public interface ICacheFileSystemDestination : IPipelineRequirement<IHICProjectDirectory>
    {
        ICacheLayout CreateCacheLayout();
    }
}