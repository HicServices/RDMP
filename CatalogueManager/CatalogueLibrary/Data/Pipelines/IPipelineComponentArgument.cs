using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// See PipelineComponentArgument
    /// </summary>
    public interface IPipelineComponentArgument : IArgument, IMapsDirectlyToDatabaseTable, ISaveable, IDeleteable,IHasDependencies
    {
        int PipelineComponent_ID { get; set; }
    }
}