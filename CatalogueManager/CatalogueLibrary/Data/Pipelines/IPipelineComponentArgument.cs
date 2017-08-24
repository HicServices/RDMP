using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Pipelines
{
    public interface IPipelineComponentArgument : IArgument, IMapsDirectlyToDatabaseTable, ISaveable, IDeleteable
    {
        int PipelineComponent_ID { get; set; }
    }
}