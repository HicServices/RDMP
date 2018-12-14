using System.Windows.Forms;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    internal class CacheProgressMenu : RDMPContextMenuStrip
    {
        public CacheProgressMenu(RDMPContextMenuStripArgs args, CacheProgress cacheProgress)
            : base(args, cacheProgress)
        {
            Add(new ExecuteCommandEditCacheProgress(args.ItemActivator, cacheProgress));

            Add(new ExecuteCommandSetPermissionWindow(_activator,cacheProgress));
            
            ReBrandActivateAs("Execute Caching",RDMPConcept.CacheProgress,OverlayKind.Execute);
        }
    }
}