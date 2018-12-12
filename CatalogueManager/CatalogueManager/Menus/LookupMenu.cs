using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.JoinsAndLookups;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class LookupMenu : RDMPContextMenuStrip
    {
        public LookupMenu(RDMPContextMenuStripArgs args, Lookup lookup) : base(args, lookup)
        {
            Items.Add("Lookup Browser", null, (s, e) => args.ItemActivator.Activate<LookupBrowserUI, Lookup>(lookup));
        }
    }
}