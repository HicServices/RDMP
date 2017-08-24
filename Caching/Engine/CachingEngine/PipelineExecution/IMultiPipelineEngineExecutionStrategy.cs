using System.Collections.Generic;
using CachingEngine.Locking;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Progress;

namespace CachingEngine.PipelineExecution
{
    public interface IMultiPipelineEngineExecutionStrategy
    {
        IEngineLockProvider EngineLockProvider { get; set; }

        void Execute(IEnumerable<IDataFlowPipelineEngine> engines, GracefulCancellationToken cancellationToken, IDataLoadEventListener listener);
    }
}