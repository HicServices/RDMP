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
        /// <summary>
        /// Component for whom this <see cref="IPipelineComponentArgument"/> provides a value for.  There will be one <see cref="IPipelineComponentArgument"/>
        /// per public property with <see cref="DemandsInitializationAttribute"/> on the <see cref="PipelineComponent.Class"/>.
        /// </summary>
        int PipelineComponent_ID { get; set; }
    }
}