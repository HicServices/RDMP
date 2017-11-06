using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueManager.Icons.IconProvision;

namespace CatalogueManager.Menus.MenuItems
{
    public class CollapseAllTreeNodesMenuItem : ToolStripMenuItem
    {
        private readonly TreeListView _tree;
        private readonly object _rootToCollapseTo;

        public CollapseAllTreeNodesMenuItem(TreeListView tree, object rootToCollapseTo) : base("Collapse Child Nodes")
        {
            _tree = tree;
            _rootToCollapseTo = rootToCollapseTo;
            Image = CatalogueIcons.collapseAllNodes;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            foreach (var o in _tree.GetChildren(_rootToCollapseTo))
                CollapseRecursively(o);
        }

        private void CollapseRecursively(object o)
        {
            foreach (var child in _tree.GetChildren(o))
                CollapseRecursively(child);

            _tree.Collapse(o);
        }
    }
}