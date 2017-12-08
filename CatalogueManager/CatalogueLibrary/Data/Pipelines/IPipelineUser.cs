namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Interface primarily for interacting with PipelineSelectionUIFactory.  Provides consumers with a method (Getter) for determining the currently configured Pipeline
    /// of a class as well as a method for committing changes to this Pipeline.
    /// </summary>
    public interface IPipelineUser
    {
        PipelineGetter Getter { get; }
        PipelineSetter Setter { get; }
    }
}