using System;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    [Flags]
    public enum PipelineUsage
    {
        None = 0,
        /// <summary>
        /// The usage context of the pipeline is that program will always set it's own destination therefore no Pipelines can be configured which have their own user defined
        /// destination. When used in DataFlowPipelineContextFactory this prevents the addition of IDataFlowDestination components
        /// </summary>
        FixedDestination = 1,
        LoadsSingleTableInfo = 2,
        LogsToTableLoadInfo = 4,
        LoadsSingleFlatFile = 8,
    }
}