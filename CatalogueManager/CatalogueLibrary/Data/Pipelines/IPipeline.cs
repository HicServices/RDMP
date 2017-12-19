using System.Linq;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// See Pipeline
    /// </summary>
    public interface IPipeline : IMapsDirectlyToDatabaseTable, ISaveable, IDeleteable
    {
        string Name { get; set; }
        string Description { get; set; }

        int? DestinationPipelineComponent_ID { get; set; }
        int? SourcePipelineComponent_ID { get; set; }

        IOrderedEnumerable<IPipelineComponent> PipelineComponents { get; }
        IPipelineComponent Destination { get; }
        IPipelineComponent Source { get; }
    }

    
}