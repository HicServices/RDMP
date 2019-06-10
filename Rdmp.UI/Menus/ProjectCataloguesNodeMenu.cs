using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.Menus
{
    class ProjectCataloguesNodeMenu : RDMPContextMenuStrip
    {
        public ProjectCataloguesNodeMenu(RDMPContextMenuStripArgs args, ProjectCataloguesNode node) : base(args, node)
        {
            Add(new ExecuteCommandMakeCatalogueProjectSpecific(_activator).SetTarget(node.Project));
        }
    }
}
