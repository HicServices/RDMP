using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class PermissionWindowUsedByCacheProgressNodeMenu : RDMPContextMenuStrip
    {
        public PermissionWindowUsedByCacheProgressNodeMenu(IActivateItems activator, PermissionWindowUsedByCacheProgressNode permissionWindowUsage, RDMPCollectionCommonFunctionality collection)
            : base(activator, null, collection)
        {
            Add(new ExecuteCommandUnlockLockable(activator, permissionWindowUsage.PermissionWindow));
        }
    }
}