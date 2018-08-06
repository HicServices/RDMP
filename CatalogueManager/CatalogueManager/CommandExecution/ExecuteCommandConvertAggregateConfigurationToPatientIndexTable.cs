using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandConvertAggregateConfigurationToPatientIndexTable : BasicUICommandExecution
    {
        private readonly AggregateConfigurationCommand _sourceAggregateConfigurationCommand;
        private readonly CohortIdentificationConfiguration _cohortIdentificationConfiguration;
        
        public ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(IActivateItems activator, AggregateConfigurationCommand sourceAggregateConfigurationCommand,CohortIdentificationConfiguration cohortIdentificationConfiguration) : base(activator)
        {
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
            new JoinableCohortAggregateConfiguration(Activator.RepositoryLocator.CatalogueRepository, _cohortIdentificationConfiguration, sourceAggregate);
            Publish(_cohortIdentificationConfiguration);
        }
    }
}