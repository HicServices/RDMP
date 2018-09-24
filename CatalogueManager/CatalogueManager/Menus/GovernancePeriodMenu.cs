using CatalogueLibrary.Data.Governance;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class GovernancePeriodMenu : RDMPContextMenuStrip
    {
        public GovernancePeriodMenu(RDMPContextMenuStripArgs args, GovernancePeriod period)
            : base(args, period)
        {
            Add(new ExecuteCommandAddNewGovernanceDocument(_activator,period));
        }
    }
}