using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Nodes.PipelineNodes;

namespace CatalogueManager.Menus
{
    class PipelineMenu : RDMPContextMenuStrip
    {
        public PipelineMenu(RDMPContextMenuStripArgs args, Pipeline pipeline): base(args, pipeline)
        {
        }

        public PipelineMenu(RDMPContextMenuStripArgs args, PipelineCompatibleWithUseCaseNode node): this(args, node.Pipeline)
        {
        }
    }
}
