using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCohortAggregateContainer : RDMPCommandExecutionProposal<CohortAggregateContainer>
    {
        public ProposeExecutionWhenTargetIsCohortAggregateContainer(IActivateItems activator):base(activator)
        {
            
        }

        public override bool CanActivate(CohortAggregateContainer target)
        {
            return false;
        }

        public override void Activate(CohortAggregateContainer target)
        {
            
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, CohortAggregateContainer targetCohortAggregateContainer, InsertOption insertOption = InsertOption.Default)
        {
           
            //Target is a cohort container (UNION / INTERSECT / EXCEPT)

            //source is catalogue
            var sourceCatalogueCommand = cmd as CatalogueCommand;

            if (sourceCatalogueCommand != null)
                return new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator,sourceCatalogueCommand, targetCohortAggregateContainer);

            //source is aggregate
            var sourceAggregateCommand = cmd as AggregateConfigurationCommand;

            if (sourceAggregateCommand != null)
            {
                //if it is not already involved in cohort identification 
                if(!sourceAggregateCommand.Aggregate.IsCohortIdentificationAggregate)
                    return new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(ItemActivator, sourceAggregateCommand, targetCohortAggregateContainer);

                //it is involved in cohort identification already, presumably it's a reorder?
                if(sourceAggregateCommand.ContainerIfAny != null)
                    if(insertOption == InsertOption.Default)
                        return new ExecuteCommandMoveAggregateIntoContainer(ItemActivator, sourceAggregateCommand, targetCohortAggregateContainer);
                    else
                        return new ExecuteCommandReOrderAggregate(ItemActivator, sourceAggregateCommand, targetCohortAggregateContainer, insertOption);
                
                //it's a patient index table
                if (sourceAggregateCommand.IsPatientIndexTable)
                    return new ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain(ItemActivator, sourceAggregateCommand, targetCohortAggregateContainer);
                
                
                //ok it IS a cic aggregate but it doesn't have any container so it must be an orphan
                return new ExecuteCommandMoveAggregateIntoContainer(ItemActivator, sourceAggregateCommand, targetCohortAggregateContainer);
            }

            //source is another container (UNION / INTERSECT / EXCEPT)
            var sourceCohortAggregateContainerCommand = cmd as CohortAggregateContainerCommand;

            if (sourceCohortAggregateContainerCommand != null)
            {
                //can never drag the root container elsewhere
                if (sourceCohortAggregateContainerCommand.ParentContainerIfAny == null)
                    return null;

                //they are trying to drag it onto it's current parent
                if (sourceCohortAggregateContainerCommand.ParentContainerIfAny.Equals(targetCohortAggregateContainer))
                    return null;

                //its being dragged into a container (move into new container)
                if(insertOption == InsertOption.Default)
                    return new ExecuteCommandMoveCohortAggregateContainerIntoSubContainer(ItemActivator, sourceCohortAggregateContainerCommand, targetCohortAggregateContainer);
                
                //its being dragged above/below a container (reorder)
                return new ExecuteCommandReOrderAggregateContainer(ItemActivator, sourceCohortAggregateContainerCommand, targetCohortAggregateContainer, insertOption);
                
            }
            return null;
        }
    }
}