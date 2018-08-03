using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.Nodes.PipelineNodes
{
    class StandardPipelineUseCaseNode : SingletonNode
    {
        public PipelineUseCase UseCase { get; set; }

        public StandardPipelineUseCaseNode(string caption, PipelineUseCase useCase) : base(caption)
        {
            UseCase = useCase;
        }
    }
}
