using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.DataFlowPipeline.Events
{
    /// <summary>
    /// Handler for events which relate to an IDataFlowPipelineEngine (starting / completing etc)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void PipelineEngineEventHandler(object sender, PipelineEngineEventArgs args);

    /// <summary>
    /// Events that relate to an <see cref="IDataFlowPipelineEngine"/>
    /// </summary>
    public class PipelineEngineEventArgs
    {
        /// <summary>
        /// The sender of the event
        /// </summary>
        public IDataFlowPipelineEngine PipelineEngine { get; private set; }

        public PipelineEngineEventArgs(IDataFlowPipelineEngine p)
        {
            PipelineEngine = p;
        }
    }
}