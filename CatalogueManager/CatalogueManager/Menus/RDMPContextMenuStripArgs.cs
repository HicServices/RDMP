using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus
{
    public class RDMPContextMenuStripArgs
    {
        public IActivateItems ItemActivator { get; set; }
        public object CurrentlyPinnedObject { get; set; }
        public IMasqueradeAs Masquerader { get; set; }
        
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

        /// <summary>
        /// Returns the first Parent control of <see cref="Tree"/> in the Windows Forms Controls Parent hierarchy which is Type T
        /// 
        /// <para>returns null if no Parent is found of the supplied Type </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetTreeParentControlOfType<T>() where T : Control
        {
            var p = Tree.Parent;

            while (p != null)
            {
                if (p is T)
                    return (T)p;

                p = p.Parent;
            }

            return null;
        }
    }
}