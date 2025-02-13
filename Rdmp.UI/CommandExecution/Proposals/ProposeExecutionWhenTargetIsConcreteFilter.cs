// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ExtractionUIs.FilterUIs;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsConcreteFilter : RDMPCommandExecutionProposal<ConcreteFilter>
{
    public ProposeExecutionWhenTargetIsConcreteFilter(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(ConcreteFilter target) => true;

    public override void Activate(ConcreteFilter target)
    {
        ItemActivator.Activate<ExtractionFilterUI, ConcreteFilter>(target);
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, ConcreteFilter targetFilter,
        InsertOption insertOption = InsertOption.Default)
    {
        return cmd switch
        {
            FilterCombineable sourceFilterCommand =>
                !sourceFilterCommand.Filter.Equals(targetFilter) && sourceFilterCommand.Filter is ConcreteFilter filter && sourceFilterCommand.Filter.FilterContainer_ID == targetFilter.FilterContainer_ID ? new ExecuteCommandReorderFilter(ItemActivator, filter, targetFilter, insertOption)
                    : null,
            _ => null
        };
    }
}