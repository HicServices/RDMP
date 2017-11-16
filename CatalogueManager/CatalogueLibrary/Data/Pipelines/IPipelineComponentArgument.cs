using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Pipelines
{
    public interface IPipelineComponentArgument : IArgument, IMapsDirectlyToDatabaseTable, ISaveable, IDeleteable,IHasDependencies
    {
        int PipelineComponent_ID { get; set; }
    }
}