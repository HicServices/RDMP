// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CatalogueLibrary.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace Rdmp.UI.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCatalogueItemsNode : RDMPCommandExecutionProposal<CatalogueItemsNode>
    {
        public ProposeExecutionWhenTargetIsCatalogueItemsNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(CatalogueItemsNode target)
        {
            return true;
        }

        public override void Activate(CatalogueItemsNode target)
        {
            var cmd = new ExecuteCommandBulkProcessCatalogueItems(ItemActivator, target.Catalogue);
            cmd.Execute();
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, CatalogueItemsNode target, InsertOption insertOption = InsertOption.Default)
        {
            var colInfo = cmd as ColumnInfoCommand;
            var tableInfo = cmd as TableInfoCommand;

            if (colInfo != null)
                return new ExecuteCommandAddNewCatalogueItem(ItemActivator, target.Catalogue, colInfo);

            if (tableInfo != null)
                return new ExecuteCommandAddNewCatalogueItem(ItemActivator, target.Catalogue, tableInfo.TableInfo.ColumnInfos);

            return null;
        }
    }
}