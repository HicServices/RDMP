using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class LoadProgressMenu : RDMPContextMenuStrip
    {
        public LoadProgressMenu(IActivateItems activator, LoadProgress loadProgress, RDMPCollectionCommonFunctionality collection) : base(activator,loadProgress, collection)
        {
            Add(new ExecuteCommandUnlockLockable(activator, loadProgress));
            Add(new ExecuteCommandAddCachingSupportToLoadProgress(activator, loadProgress));

            AddCommonMenuItems();

        }
    }
}