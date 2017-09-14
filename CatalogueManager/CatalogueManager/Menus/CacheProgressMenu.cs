using System.Windows.Forms;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    internal class CacheProgressMenu : RDMPContextMenuStrip
    {
        public CacheProgressMenu(IActivateItems activator, CacheProgress cacheProgress) : base(activator,cacheProgress)
        {
            Items.Add(
                "Execute Cache",
                CatalogueIcons.ExecuteArrow,
                (s,e)=>_activator.ExecuteCacheProgress(this,cacheProgress)
                );

            var window = cacheProgress.GetPermissionWindow();
            
            if(window != null)
                Items.Add(AtomicCommandUIFactory.CreateMenuItem(new ExecuteCommandUnlockLockable(activator, window)));


            AddCommonMenuItems();
        }
    }
}