namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Interface primarily for interacting with PipelineSelectionUIFactory.  Provides consumers with a method (Getter) for determining the currently configured Pipeline
    /// of a class as well as a method for committing changes to this Pipeline.
    /// </summary>
    public interface IPipelineUser
    {
        /// <summary>
        /// Delegate for returning the referenced <see cref="Pipeline"/> for the <see cref="IPipelineUser"/>
        /// </summary>
        PipelineGetter Getter { get; }
        
        /// <summary>
        /// Delegate for changing the referenced <see cref="Pipeline"/> for the <see cref="IPipelineUser"/>
        /// </summary>
        PipelineSetter Setter { get; }
    }
}