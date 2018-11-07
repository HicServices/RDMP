using System;
using CatalogueLibrary.Data;
using HIC.Logging;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    /// <summary>
    /// Default options for creating common pipeline contexts
    /// </summary>
    [Flags]
    public enum PipelineUsage
    {
        /// <summary>
        /// There are no special flags for this pipeline context yet
        /// </summary>
        None = 0,

        /// <summary>
        /// The usage context of the pipeline is that program will always set it's own destination therefore no Pipelines can be configured which have their own user defined
        /// destination. When used in DataFlowPipelineContextFactory this prevents the addition of IDataFlowDestination components
        /// </summary>
        FixedDestination = 1,
        
        /// <summary>
        /// Pipeline puts data into a single <see cref="TableInfo"/>
        /// </summary>
        LoadsSingleTableInfo = 2,

        /// <summary>
        /// Pipeline must log to an already existing <see cref="TableLoadInfo"/>
        /// </summary>
        LogsToTableLoadInfo = 4,

        /// <summary>
        /// Pipeline takes as input a file which is expected to be read by the source
        /// </summary>
        LoadsSingleFlatFile = 8,

        /// <summary>
        /// The pipeline cannot have a source, the source instance is provided at runtime by the environment
        /// </summary>
        FixedSource = 16,
    }
}