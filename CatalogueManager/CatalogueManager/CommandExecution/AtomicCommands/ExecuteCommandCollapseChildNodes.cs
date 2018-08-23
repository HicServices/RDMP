using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BrightIdeasSoftware;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCollapseChildNodes : BasicUICommandExecution,IAtomicCommand
    {
        private readonly RDMPCollectionCommonFunctionality _commonFunctionality;
        private readonly object _rootToCollapseTo;

        public ExecuteCommandCollapseChildNodes(IActivateItems activator,RDMPCollectionCommonFunctionality commonFunctionality, object rootToCollapseTo) : base(activator)
        {
            _commonFunctionality = commonFunctionality;
            _rootToCollapseTo = rootToCollapseTo;

            if (!_commonFunctionality.Tree.IsExpanded(rootToCollapseTo))
                SetImpossible("Node is not expanded");
        }

        public override void Execute()
        {
            base.Execute();
            
            _commonFunctionality.Tree.BeginUpdate();
            try
            {

                //collapse all children
                foreach (object o in _commonFunctionality.CoreChildProvider.GetAllChildrenRecursively(_rootToCollapseTo))
                    if (_commonFunctionality.Tree.IsExpanded(o))
                        _commonFunctionality.Tree.Collapse(o);

                //and collapse the root
                _commonFunctionality.Tree.Collapse(_rootToCollapseTo);

                //then expand it to depth 1
                _commonFunctionality.ExpandToDepth(1,_rootToCollapseTo);

                var index = _commonFunctionality.Tree.IndexOf(_rootToCollapseTo);
                if(index != -1)
                    _commonFunctionality.Tree.EnsureVisible(index);
            }
            finally
            {
                _commonFunctionality.Tree.EndUpdate();
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.collapseAllNodes;
        }
    }
}
