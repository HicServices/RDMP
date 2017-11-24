using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using ReusableUIComponents;

namespace CatalogueManager.Menus
{
    public class AllAutomationServerSlotsNodeMenu : RDMPContextMenuStrip
    {
        public AllAutomationServerSlotsNodeMenu(RDMPContextMenuStripArgs args, AllAutomationServerSlotsNode databaseEntity)
            : base(args, null)
        {
            Add(new ExecuteCommandCreateNewAutomationSlot(_activator));

            Add(new ExecuteCommandPushToRemotes(_activator));
        }
    }
}