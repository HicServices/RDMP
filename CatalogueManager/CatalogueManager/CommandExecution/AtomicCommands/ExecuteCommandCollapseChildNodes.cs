using System.Drawing;
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
        
            foreach (var o in _tree.GetChildren(_rootToCollapseTo))
                CollapseRecursively(o);
        }

        private void CollapseRecursively(object o)
        {
            foreach (var child in _tree.GetChildren(o))
                CollapseRecursively(child);

            _tree.Collapse(o);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.collapseAllNodes;
        }
    }
}
