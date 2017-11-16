using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class PermissionWindowUsedByCacheProgressNodeMenu : RDMPContextMenuStrip
    {
        public PermissionWindowUsedByCacheProgressNodeMenu(IActivateItems activator, PermissionWindowUsedByCacheProgressNode permissionWindowUsage)
            : base(activator, null)
        {
            Add(new ExecuteCommandUnlockLockable(activator, permissionWindowUsage.PermissionWindow));
        }
    }
}