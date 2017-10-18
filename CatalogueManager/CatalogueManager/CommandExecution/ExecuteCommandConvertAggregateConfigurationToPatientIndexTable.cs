using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Nodes;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandConvertAggregateConfigurationToPatientIndexTable : BasicCommandExecution
    {
        private readonly AggregateConfigurationCommand _sourceAggregateConfigurationCommand;
        private readonly CohortIdentificationConfiguration _cohortIdentificationConfiguration;
        private readonly IActivateItems _activator;

        public ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(IActivateItems activator, AggregateConfigurationCommand sourceAggregateConfigurationCommand,CohortIdentificationConfiguration cohortIdentificationConfiguration)
        {
            _activator = activator;
            _sourceAggregateConfigurationCommand = sourceAggregateConfigurationCommand;
            _cohortIdentificationConfiguration = cohortIdentificationConfiguration;

            if(sourceAggregateConfigurationCommand.JoinableDeclarationIfAny != null)
                SetImpossible("Aggregate is already a Patient Index Table");

            if(_sourceAggregateConfigurationCommand.CohortIdentificationConfigurationIfAny != null &&
                _sourceAggregateConfigurationCommand.CohortIdentificationConfigurationIfAny.ID != _cohortIdentificationConfiguration.ID)
                SetImpossible("Aggregate '" + _sourceAggregateConfigurationCommand.Aggregate + "'  belongs to a different Cohort Identification Configuration");

        }

        public override void Execute()
        {
            base.Execute();

            var sourceAggregate = _sourceAggregateConfigurationCommand.Aggregate;
            
            //make sure it is not part of any folders
            var parent = sourceAggregate.GetCohortAggregateContainerIfAny();
            if (parent != null)
                parent.RemoveChild(sourceAggregate);

            //create a new patient index table usage allowance for this aggregate
            new JoinableCohortAggregateConfiguration(_activator.RepositoryLocator.CatalogueRepository, _cohortIdentificationConfiguration, sourceAggregate);
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_cohortIdentificationConfiguration));
        }
    }
}