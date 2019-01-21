using System;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class PipelineMenu : RDMPContextMenuStrip
    {
        public PipelineMenu(RDMPContextMenuStripArgs args, PipelineCompatibleWithUseCaseNode node): base(args,node)
        {
            Add(new ExecuteCommandCreateNewPipeline(_activator, node.UseCase));
            Add(new ExecuteCommandClonePipeline(_activator, node.Pipeline));
        }
        public PipelineMenu(RDMPContextMenuStripArgs args, StandardPipelineUseCaseNode node): base(args, node)
        {
            Add(new ExecuteCommandCreateNewPipeline(_activator, node.UseCase));

            var type = node.UseCase.GetType();

            args.ExtraKeywordHelpText = string.Format("{0} \r\n {1}",
                type.Name,
                args.ItemActivator.RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(type)
                );

        }
        public PipelineMenu(RDMPContextMenuStripArgs args, Pipeline pipeline): base(args, pipeline)
        {
            Add(new ExecuteCommandCreateNewPipeline(_activator, null));
            Add(new ExecuteCommandClonePipeline(_activator, pipeline));
        }
    }
}
