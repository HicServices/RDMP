using CatalogueLibrary.Data.Pipelines;
using ReusableLibraryCode.Progress;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// Factory for turning an IPipeline into a runnable engine.  See IDataFlowPipelineEngineFactory Generic T for full description
    /// </summary>
    public interface IDataFlowPipelineEngineFactory
    {
        /// <summary>
        /// Turns the blueprint <see cref="IPipeline"/> into a runnable instance of <see cref="IDataFlowPipelineEngine"/>.  This engine will be uninitialized
        /// to start with. 
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="listener"></param>
        /// <returns></returns>
        IDataFlowPipelineEngine Create(IPipeline pipeline, IDataLoadEventListener listener);
    }
}