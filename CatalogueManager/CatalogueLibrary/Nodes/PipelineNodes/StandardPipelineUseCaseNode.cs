using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.Nodes.PipelineNodes
{
    /// <summary>
    /// Collection of all the Pipelines compatible with a given use case. 
    /// </summary>
    class StandardPipelineUseCaseNode : SingletonNode
    {
        public PipelineUseCase UseCase { get; set; }

        public StandardPipelineUseCaseNode(string caption, PipelineUseCase useCase) : base(caption)
        {
            UseCase = useCase;
        }
    }
}
