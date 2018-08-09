using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Injection;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// See Pipeline
    /// </summary>
    public interface IPipeline : IMapsDirectlyToDatabaseTable, ISaveable, IDeleteable, IInjectKnown<IPipelineComponent[]>
    {
        string Name { get; set; }
        string Description { get; set; }

        int? DestinationPipelineComponent_ID { get; set; }
        int? SourcePipelineComponent_ID { get; set; }

        IList<IPipelineComponent> PipelineComponents { get; }
        IPipelineComponent Destination { get; }
        IPipelineComponent Source { get; }
    }

    
}