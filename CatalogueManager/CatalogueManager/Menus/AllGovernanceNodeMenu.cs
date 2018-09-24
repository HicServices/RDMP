using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Copying.Commands;

namespace CatalogueManager.Menus
{
    class AllGovernanceNodeMenu:RDMPContextMenuStrip
    {
        public AllGovernanceNodeMenu(RDMPContextMenuStripArgs args, AllGovernanceNode o) : base(args, o)
        {
            Add(new ExecuteCommandCreateNewGovernancePeriod(_activator));
        }
    }
}
