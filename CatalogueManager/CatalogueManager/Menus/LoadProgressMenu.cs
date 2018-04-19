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
        public LoadProgressMenu(RDMPContextMenuStripArgs args,LoadProgress loadProgress) : base(args,loadProgress)
        {
            Add(new ExecuteCommandUnlockLockable(_activator, loadProgress));
            Add(new ExecuteCommandCreateNewCacheProgress(_activator, loadProgress));

        }
    }
}