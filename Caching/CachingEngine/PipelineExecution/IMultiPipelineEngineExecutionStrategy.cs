using System.Collections.Generic;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Progress;

namespace CachingEngine.PipelineExecution
{
    /// <summary>
    /// Logic for locking and executing multiple IDataFlowPipelineEngine at once (single threaded loop is valid, it doesn't have to be async)
    /// </summary>
    public interface IMultiPipelineEngineExecutionStrategy
    {
        void Execute(IEnumerable<IDataFlowPipelineEngine> engines, GracefulCancellationToken cancellationToken, IDataLoadEventListener listener);
    }
}