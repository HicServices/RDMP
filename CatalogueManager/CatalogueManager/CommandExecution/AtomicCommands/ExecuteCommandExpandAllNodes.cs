using System.Drawing;
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

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.ExpandAllNodes;
        }
    }
}
