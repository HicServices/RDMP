// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.collapseAllNodes;
        }
    }
}
