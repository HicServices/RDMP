// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CatalogueLibrary.Data.Aggregation;
using Rdmp.UI.AggregationUIs.Advanced;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace Rdmp.UI.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsAggregateConfiguration:RDMPCommandExecutionProposal<AggregateConfiguration>
    {
        public ProposeExecutionWhenTargetIsAggregateConfiguration(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(AggregateConfiguration target)
        {
            return true;
        }

        public override void Activate(AggregateConfiguration target)
        {
            ItemActivator.Activate<AggregateEditorUI, AggregateConfiguration>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, AggregateConfiguration targetAggregateConfiguration, InsertOption insertOption = InsertOption.Default)
        {
            var sourceAggregateCommand = cmd as AggregateConfigurationCommand;

            //if it is an aggregate being dragged
            if (sourceAggregateCommand != null)
            {
                //source and target are the same
                if (sourceAggregateCommand.Aggregate.Equals(targetAggregateConfiguration))
                    return null;

                //that is part of cohort identification already and being dragged above/below the current aggregate
                if (sourceAggregateCommand.ContainerIfAny != null && insertOption != InsertOption.Default)
                    return new ExecuteCommandReOrderAggregate(ItemActivator, sourceAggregateCommand, targetAggregateConfiguration, insertOption);
            }

            var sourceCohortAggregateContainerCommand = cmd as CohortAggregateContainerCommand;
            if (sourceCohortAggregateContainerCommand != null)
            {
                //can never drag the root container elsewhere
                if (sourceCohortAggregateContainerCommand.ParentContainerIfAny == null)
                    return null;

                //above or below
                if (insertOption != InsertOption.Default)
                    return new ExecuteCommandReOrderAggregateContainer(ItemActivator, sourceCohortAggregateContainerCommand, targetAggregateConfiguration, insertOption);
            }
            return null;
        }
    }
}
