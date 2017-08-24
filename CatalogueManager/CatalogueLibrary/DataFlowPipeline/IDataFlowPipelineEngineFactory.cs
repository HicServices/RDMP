using CatalogueLibrary.Data.Pipelines;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    public interface IDataFlowPipelineEngineFactory
    {
        IDataFlowPipelineEngine Create(IPipeline pipeline, IDataLoadEventListener listener);
    }
}