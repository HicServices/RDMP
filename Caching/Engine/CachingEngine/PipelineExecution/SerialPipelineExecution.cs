using System.Collections.Generic;
using CachingEngine.Locking;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Progress;

namespace CachingEngine.PipelineExecution
{
    /// <summary>
    /// Strategy for executing several IDataFlowPipelineEngines one after the other in serial.  This will fully exhaust each IDataFlowPipelineEngine one 
    /// after the other.
    /// </summary>
    public class SerialPipelineExecution : IMultiPipelineEngineExecutionStrategy
    {
        public IEngineLockProvider EngineLockProvider { get; set; }

        public void Execute(IEnumerable<IDataFlowPipelineEngine> engines, GracefulCancellationToken cancellationToken, IDataLoadEventListener listener)
        {
            // Execute each pipeline to completion before starting the next
            foreach (var engine in engines)
            {
                if (EngineLockProvider.IsLocked(engine))
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, EngineLockProvider.Details(engine) + ": locked, continuing with next engine"));
                    continue;
                }

                EngineLockProvider.Lock(engine);
                engine.ExecutePipeline(cancellationToken);
                EngineLockProvider.Unlock(engine);
                
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
    }
}