using System.Collections.Generic;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.Data.Pipelines
{
    public interface IPipelineUseCase
    {
        object[] GetInitializationObjects(ICatalogueRepository repository);
        IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines);
        IDataFlowPipelineContext GetContext();
        object ExplicitSource { get; }
        object ExplicitDestination { get; }

        IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener);
    }
}