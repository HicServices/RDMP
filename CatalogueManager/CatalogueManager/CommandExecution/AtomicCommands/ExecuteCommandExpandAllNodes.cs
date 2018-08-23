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
    public class ExecuteCommandExpandAllNodes : BasicUICommandExecution,IAtomicCommand
    {
        private readonly RDMPCollectionCommonFunctionality _commonFunctionality;
        private object _rootToExpandFrom;

        public ExecuteCommandExpandAllNodes(IActivateItems activator,RDMPCollectionCommonFunctionality commonFunctionality, object rootToCollapseTo) : base(activator)
        {
            _commonFunctionality = commonFunctionality;
            _rootToExpandFrom = rootToCollapseTo;
            
            if(!commonFunctionality.Tree.CanExpand(rootToCollapseTo))
                SetImpossible("Node cannot be expanded");
        }

        public override void Execute()
        {
            base.Execute();

            _commonFunctionality.Tree.BeginUpdate();
            try
            {
                _commonFunctionality.ExpandToDepth(int.MaxValue,_rootToExpandFrom);

                var index = _commonFunctionality.Tree.IndexOf(_rootToExpandFrom);
                if (index != -1)
                    _commonFunctionality.Tree.EnsureVisible(index);
            }
            finally
            {
                _commonFunctionality.Tree.EndUpdate();
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.ExpandAllNodes;
        }
    }
}
