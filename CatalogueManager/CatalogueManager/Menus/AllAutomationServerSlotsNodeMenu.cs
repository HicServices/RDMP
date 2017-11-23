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
        public AllAutomationServerSlotsNodeMenu(IActivateItems activator, AllAutomationServerSlotsNode databaseEntity, RDMPCollectionCommonFunctionality collection)
            : base(activator, null, collection)
        {
            Add(new ExecuteCommandCreateNewAutomationSlot(activator));

            Add(new ExecuteCommandPushToRemotes(activator));
        }
    }
}