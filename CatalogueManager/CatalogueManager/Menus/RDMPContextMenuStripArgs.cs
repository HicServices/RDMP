using BrightIdeasSoftware;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    public class RDMPContextMenuStripArgs
    {
        public IActivateItems ItemActivator { get; set; }
        public object CurrentlyPinnedObject { get; set; }
        public object Masquerader { get; set; }
        
        public TreeListView Tree { get; set; }
        public object Model { get; set; }

        public RDMPContextMenuStripArgs(IActivateItems itemActivator)
        {
            ItemActivator = itemActivator;
        }

        public RDMPContextMenuStripArgs(IActivateItems itemActivator, TreeListView tree, object model):this(itemActivator)
        {
            Tree = tree;
            Model = model;
        }
    }
}