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
    public class ExecuteCommandExpandAllNodes : BasicUICommandExecution,IAtomicCommand
    {
        private TreeListView _tree;
        private object _rootToExpandFrom;

        public ExecuteCommandExpandAllNodes(IActivateItems activator,TreeListView tree, object rootToCollapseTo) : base(activator)
        {
            _tree = tree;
            _rootToExpandFrom = rootToCollapseTo;
            
            if(!tree.CanExpand(rootToCollapseTo))
                SetImpossible("Node cannot be expanded");
        }

        public override void Execute()
        {
            base.Execute();

            HashSet<object> expanded = new HashSet<object>(_tree.ExpandedObjects.OfType<object>());
            try
            {
                expanded.Add(_rootToExpandFrom);

                foreach (var o in _tree.GetChildren(_rootToExpandFrom))
                    ExpandRecursively(o,expanded);

                _tree.ExpandedObjects = expanded;
            }
            finally
            {
                _tree.RebuildAll(true);
            }
        }

        private void ExpandRecursively(object o, HashSet<object> expanded)
        {
            expanded.Add(o);

            foreach (var child in Activator.CoreChildProvider.GetChildren(o))
                ExpandRecursively(child,expanded);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.ExpandAllNodes;
        }
    }
}
