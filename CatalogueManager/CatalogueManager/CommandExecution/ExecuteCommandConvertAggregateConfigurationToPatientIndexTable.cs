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
        private readonly JoinableCollectionNode _targetJoinableCollectionNode;
        private readonly IActivateItems _activator;

        public ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(IActivateItems activator, AggregateConfigurationCommand sourceAggregateConfigurationCommand, JoinableCollectionNode targetJoinableCollectionNode)
        {
            _activator = activator;
            _sourceAggregateConfigurationCommand = sourceAggregateConfigurationCommand;
            _targetJoinableCollectionNode = targetJoinableCollectionNode;

            if(sourceAggregateConfigurationCommand.JoinableDeclarationIfAny != null)
                SetImpossible("Aggregate is already a Patient Index Table");

            if(_sourceAggregateConfigurationCommand.CohortIdentificationConfigurationIfAny != null &&
                _sourceAggregateConfigurationCommand.CohortIdentificationConfigurationIfAny.ID != _targetJoinableCollectionNode.Configuration.ID)
                SetImpossible("Aggregate '" + _sourceAggregateConfigurationCommand.Aggregate + "'  belongs to a different Cohort Identification Configuration");

        }

        public override void Execute()
        {
            base.Execute();

            var sourceAggregate = _sourceAggregateConfigurationCommand.Aggregate;
            var cic = _targetJoinableCollectionNode.Configuration;

            //make sure it is not part of any folders
            var parent = sourceAggregate.GetCohortAggregateContainerIfAny();
            if (parent != null)
                parent.RemoveChild(sourceAggregate);

            //create a new patient index table usage allowance for this aggregate
            new JoinableCohortAggregateConfiguration(_activator.RepositoryLocator.CatalogueRepository, cic, sourceAggregate);
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(cic));
        }
    }
}