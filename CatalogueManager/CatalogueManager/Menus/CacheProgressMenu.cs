using CatalogueLibrary.Data.Cache;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    internal class CacheProgressMenu : RDMPContextMenuStrip
    {
        public CacheProgressMenu(RDMPContextMenuStripArgs args, CacheProgress cacheProgress)
            : base(args, cacheProgress)
        {
            Items.Add("Edit", null, (s, e) => _activator.Activate<CacheProgressUI, CacheProgress>(cacheProgress));

            Add(new ExecuteCommandSetPermissionWindow(_activator,cacheProgress));
            
            ReBrandActivateAs("Execute Caching",RDMPConcept.CacheProgress,OverlayKind.Execute);
        }
    }
}