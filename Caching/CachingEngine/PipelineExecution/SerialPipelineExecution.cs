using System.Collections.Generic;
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
        public void Execute(IEnumerable<IDataFlowPipelineEngine> engines, GracefulCancellationToken cancellationToken, IDataLoadEventListener listener)
        {
            // Execute each pipeline to completion before starting the next
            foreach (var engine in engines)
            {
                engine.ExecutePipeline(cancellationToken);
                
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
    }
}