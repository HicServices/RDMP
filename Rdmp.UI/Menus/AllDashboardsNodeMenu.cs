using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.Menus
{
    class AllDashboardsNodeMenu : RDMPContextMenuStrip
    {

        public AllDashboardsNodeMenu(RDMPContextMenuStripArgs args, AllDashboardsNode node)
            : base(args, node)
        {
            Add(new ExecuteCommandCreateNewDashboard(_activator));
        }
    }
}
