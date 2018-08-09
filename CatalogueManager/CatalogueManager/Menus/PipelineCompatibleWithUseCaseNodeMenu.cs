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
        }
        public PipelineMenu(RDMPContextMenuStripArgs args, StandardPipelineUseCaseNode node): base(args, node)
        {
            Add(new ExecuteCommandCreateNewPipeline(_activator, node.UseCase));
        }
        public PipelineMenu(RDMPContextMenuStripArgs args, Pipeline pipeline): base(args, pipeline)
        {
            Add(new ExecuteCommandCreateNewPipeline(_activator, null));
        }
    }
}
