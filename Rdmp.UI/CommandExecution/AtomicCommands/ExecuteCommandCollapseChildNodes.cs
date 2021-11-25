// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCollapseChildNodes : BasicUICommandExecution,IAtomicCommand
    {
        private readonly RDMPCollectionCommonFunctionality _commonFunctionality;
        private readonly object _rootToCollapseTo;

        public ExecuteCommandCollapseChildNodes(IActivateItems activator,RDMPCollectionCommonFunctionality commonFunctionality, object rootToCollapseTo) : base(activator)
        {
            _commonFunctionality = commonFunctionality;
            _rootToCollapseTo = rootToCollapseTo;

            // collapse all with no node selected collapses whole tree
            if (_rootToCollapseTo is RDMPCollection)
            {
                return;
            }

            if (!_commonFunctionality.Tree.IsExpanded(rootToCollapseTo))
                SetImpossible("Node is not expanded");
        }

        public override string GetCommandName()
        {
            if (_rootToCollapseTo is RDMPCollection && string.IsNullOrWhiteSpace(OverrideCommandName))
            {
                return "Collapse All";
            }

            return base.GetCommandName();
        }

        public override void Execute()
        {
            base.Execute();
            
            _commonFunctionality.Tree.BeginUpdate();
            try
            {

                if (_rootToCollapseTo is RDMPCollection)
                {
                    _commonFunctionality.Tree.CollapseAll();
                    return;
                }

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

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.collapseAllNodes;
        }
    }
}
