using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class PermissionWindowUsageMenu : RDMPContextMenuStrip
    {
        public PermissionWindowUsageMenu(IActivateItems activator, PermissionWindowUsedByCacheProgress permissionWindowUsage):base(activator,null)
        {
            Items.Add(AtomicCommandUIFactory.CreateMenuItem(new ExecuteCommandUnlockLockable(activator, permissionWindowUsage.PermissionWindow)));

        }
    }
}