using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Describes the flow of strongly typed objects (usually DataTables) from a source to a destination (e.g. extracting linked cohort data into a flat file ).  
    /// This entity is the serialized version of DataFlowPipelineEngine&lt;T&gt; (built by a DataFlowPipelineEngineFactory&lt;T&gt; ).
    /// 
    /// <para>It is the hanging off point of a sequence of steps e.g. 'clean strings', 'substitute column X for column Y by mapping values off of remote server B'.</para>
    /// 
    /// <para>The functionality of the class is like a microcosm of LoadMetadata (a sequence of predominately reflection driven operations) but it happens in memory 
    /// (rather than in the RAW=>STAGING=>LIVE databases).</para>
    /// 
    /// <para>Any time data flows from one location to another there is usually a pipeline involved (e.g. read from a flat file and bulk insert into a database), it 
    /// may be an empty pipeline but the fact that it is there allows for advanced/freaky user requirements such as:</para>
    ///
    /// <para>"Can we count all dates to the first Monday of the week on all extracts we do from now on? - it's a requirement of our new Data Governance Officer"</para>
    /// 
    /// <para>A Pipeline can be missing either/both a source and destination.  This means that the pipeline can only be used in a situation where the context forces
    /// a particular source/destination (for example if the user is trying to bulk insert a CSV file then the Destination might be a fixed instance of DataTableUploadDestination
    /// initialized with a specific server/database that the user had picked on a user interface).</para>
    /// 
    /// <para>Remember that Pipeline is the serialization, pipelines are used all over the place in RDMP software under different contexts (caching, data extraction etc)
    /// and sometimes we even create DataFlowPipelineEngine on the fly without even having a Pipeline serialization to create it from.</para>
    /// </summary>
    public interface IPipeline : IInjectKnown<IPipelineComponent[]>, INamed
    {
        /// <summary>
        /// Human readable description of the intended purpose of the pipeline as configured by the user.  Should include
        /// what the pipeline is supposed to do.
        /// </summary>
        string Description { get; set; }
        
        /// <summary>
        /// The component acting as the source of the pipeline and producing data (e.g. by reading a flat file).  This
        /// can be null if the <see cref="IPipelineUseCase"/> has a fixed runtime source instance instead.
        /// </summary>
        int? DestinationPipelineComponent_ID { get; set; }

        /// <summary>
        /// The component acting as the destination of the pipeline and consuming data (e.g. writing records to a database).  This
        /// can be null if the <see cref="IPipelineUseCase"/> has a fixed runtime destination instance instead.
        /// </summary>
        int? SourcePipelineComponent_ID { get; set; }

        /// <summary>
        /// All components in the pipeline (including the source and destination)
        /// </summary>
        IList<IPipelineComponent> PipelineComponents { get; }

        /// <inheritdoc cref="DestinationPipelineComponent_ID"/>
        IPipelineComponent Destination { get; }

        /// <inheritdoc cref="SourcePipelineComponent_ID"/>
        IPipelineComponent Source { get; }


        /// <summary>
        /// Creates a complete copy (in the database) of the current <see cref="Pipeline"/> including all new copies of components and arguments
        /// </summary>
        /// <returns></returns>
        Pipeline Clone();

    }

    
}