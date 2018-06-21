using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class PermissionWindowUsedByCacheProgressNodeMenu : RDMPContextMenuStrip
    {
        public PermissionWindowUsedByCacheProgressNodeMenu(RDMPContextMenuStripArgs args, PermissionWindowUsedByCacheProgressNode permissionWindowUsage)
            : base(args, permissionWindowUsage)
        {
            Add(new ExecuteCommandUnlockLockable(_activator, permissionWindowUsage.PermissionWindow));
        }
    }
}