using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.AggregationUIs.Advanced;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
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
            ItemActivator.Activate<AggregateEditor, AggregateConfiguration>(target);
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
