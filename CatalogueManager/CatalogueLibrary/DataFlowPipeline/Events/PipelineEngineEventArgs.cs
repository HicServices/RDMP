using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.DataFlowPipeline.Events
{
    public delegate void PipelineEngineEventHandler(object sender, PipelineEngineEventArgs args);

    public class PipelineEngineEventArgs
    {
        public IDataFlowPipelineEngine PipelineEngine { get; private set; }

        public PipelineEngineEventArgs(IDataFlowPipelineEngine p)
        {
            PipelineEngine = p;
        }
    }
}