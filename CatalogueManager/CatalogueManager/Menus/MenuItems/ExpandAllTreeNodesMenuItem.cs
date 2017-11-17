using System;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Menus.MenuItems
{
    public class ExpandAllTreeNodesMenuItem : ToolStripMenuItem
    {
        private TreeListView _tree;
        private object _rootToExpandFrom;

        public ExpandAllTreeNodesMenuItem(TreeListView tree, object rootToCollapseTo) : base("Expand All Nodes")
        {
            _tree = tree;
            _rootToExpandFrom = rootToCollapseTo;
            Image = CatalogueIcons.ExpandAllNodes;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            _tree.Expand(_rootToExpandFrom);
            foreach (var o in _tree.GetChildren(_rootToExpandFrom))
                ExpandRecursively(o);
        }

        private void ExpandRecursively(object o)
        {
            _tree.Expand(o);

            foreach (var child in _tree.GetChildren(o))
                ExpandRecursively(child);
        }

    }
}