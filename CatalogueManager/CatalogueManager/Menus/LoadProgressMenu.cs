using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class LoadProgressMenu : RDMPContextMenuStrip
    {
        public LoadProgressMenu(IActivateItems activator, LoadProgress loadProgress) : base(activator,loadProgress)
        {
            Items.Add(AtomicCommandUIFactory.CreateMenuItem(new ExecuteCommandUnlockLockable(activator, loadProgress)));

            AddCommonMenuItems();

        }
    }
}