// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using System.Linq;

namespace Rdmp.UI.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCatalogueItemsNode : RDMPCommandExecutionProposal<CatalogueItemsNode>
    {
        public ProposeExecutionWhenTargetIsCatalogueItemsNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(CatalogueItemsNode target)
        {
            return false;
        }

        public override void Activate(CatalogueItemsNode target)
        {
        }

        public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, CatalogueItemsNode target, InsertOption insertOption = InsertOption.Default)
        {
            var colInfo = cmd as ColumnInfoCombineable;
            var tableInfo = cmd as TableInfoCombineable;

            if (colInfo != null)
                return new ExecuteCommandAddNewCatalogueItem(ItemActivator, target.Catalogue, colInfo) { Category = target.Category };
            
            if (tableInfo != null)
                return new ExecuteCommandAddNewCatalogueItem(ItemActivator, target.Catalogue, tableInfo.TableInfo.ColumnInfos) { Category = target.Category };

            // when dropping onto Core or Supplemental etc
            if(cmd is CatalogueItemCombineable ciCombine && target.Category != null)
            {
                // drag to a new category to change the extractability
                return new ExecuteCommandChangeExtractionCategory(ItemActivator, 
                    ciCombine.CatalogueItems.Select(ci => ci.ExtractionInformation)
                    .Where(e => e != null).ToArray(),
                    target.Category);
            }

            return null;
        }
    }
}