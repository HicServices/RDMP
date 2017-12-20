using CatalogueLibrary.Data.Pipelines;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// Factory for turning an IPipeline into a runnable engine.  See IDataFlowPipelineEngineFactory Generic T for full description
    /// </summary>
    public interface IDataFlowPipelineEngineFactory
    {
        IDataFlowPipelineEngine Create(IPipeline pipeline, IDataLoadEventListener listener);
    }
}