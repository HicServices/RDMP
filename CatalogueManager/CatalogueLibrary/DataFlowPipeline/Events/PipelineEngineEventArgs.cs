using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.DataFlowPipeline.Events
{
    /// <summary>
    /// Handler for events which relate to an IDataFlowPipelineEngine (starting / completing etc)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
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