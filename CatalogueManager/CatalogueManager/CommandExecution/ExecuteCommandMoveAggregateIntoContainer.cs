using System;
using System.Reflection;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMoveAggregateIntoContainer : BasicCommandExecution
    {
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;
        private readonly AggregateConfigurationCommand _sourceAggregateCommand;
        private readonly IActivateItems _activator;

        public ExecuteCommandMoveAggregateIntoContainer(IActivateItems activator, AggregateConfigurationCommand sourceAggregateCommand, CohortAggregateContainer targetCohortAggregateContainer)
        {
            _activator = activator;
            _sourceAggregateCommand = sourceAggregateCommand;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            var cic = _sourceAggregateCommand.CohortIdentificationConfigurationIfAny;

            if(cic != null && !cic.Equals(_targetCohortAggregateContainer.GetCohortIdentificationConfiguration()))
                SetImpossible("Aggregate belongs to a different CohortIdentificationConfiguration");

            if(_sourceAggregateCommand.ContainerIfAny != null &&  _sourceAggregateCommand.ContainerIfAny.Equals(targetCohortAggregateContainer))
                SetImpossible("Aggregate is already in container");
        }

        public override void Execute()
        {
            base.Execute();

            //remove it from it's old container
            var oldContainer = _sourceAggregateCommand.ContainerIfAny;
            
            if(oldContainer != null)
                oldContainer.RemoveChild(_sourceAggregateCommand.Aggregate);

            //add  it to the new container
            _targetCohortAggregateContainer.AddChild(_sourceAggregateCommand.Aggregate,0);
            
            
            //refresh the entire configuration
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_targetCohortAggregateContainer.GetCohortIdentificationConfiguration()));
        }
    }
}