using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BrightIdeasSoftware;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCollapseChildNodes : BasicUICommandExecution,IAtomicCommand
    {
        private readonly TreeListView _tree;
        private readonly object _rootToCollapseTo;

        public ExecuteCommandCollapseChildNodes(IActivateItems activator,TreeListView tree, object rootToCollapseTo) : base(activator)
        {
            _tree = tree;
            _rootToCollapseTo = rootToCollapseTo;

            if(!tree.IsExpanded(rootToCollapseTo))
                SetImpossible("Node is not expanded");
        }

        public override void Execute()
        {
            base.Execute();

            HashSet<object> expanded = new HashSet<object> (_tree.ExpandedObjects.OfType<object>());

            try
            {
                foreach (var o in _tree.GetChildren(_rootToCollapseTo))
                    CollapseRecursively(o,expanded);
            }
            finally
            {
                _tree.ExpandedObjects = expanded;
                _tree.RebuildAll(true);
            }
        }

        private void CollapseRecursively(object o, HashSet<object> expanded)
        {
            foreach (var child in _tree.GetChildren(o))
                CollapseRecursively(child,expanded);

            if(expanded.Contains(o))
                expanded.Remove(o);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.collapseAllNodes;
        }
    }
}
