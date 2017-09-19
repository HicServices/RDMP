namespace CatalogueLibrary.Data.Pipelines
{
    public interface IPipelineUser
    {
        PipelineGetter Getter { get; }
        PipelineSetter Setter { get; }
    }
}