using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    public class RDMPContextMenuStripArgs
    {
        public IActivateItems ItemActivator { get; set; }
        public object CurrentlyPinnedObject { get; set; }
        public object Masquerader { get; set; }

        public RDMPContextMenuStripArgs(IActivateItems itemActivator)
        {
            ItemActivator = itemActivator;
        }
    }
}