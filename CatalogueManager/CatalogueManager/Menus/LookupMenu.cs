using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class LookupMenu : RDMPContextMenuStrip
    {
        public LookupMenu(RDMPContextMenuStripArgs args, Lookup lookup) : base(args, lookup)
        {
            Add(new ExecuteCommandBrowseLookup(args.ItemActivator, lookup));
        }
    }
}