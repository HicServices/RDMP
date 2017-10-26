using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly AggregateConfigurationCommand _sourceAggregateCommand;
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;

        public ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain(IActivateItems activator, AggregateConfigurationCommand sourceAggregateCommand, CohortAggregateContainer targetCohortAggregateContainer)
        {
            _activator = activator;
            _sourceAggregateCommand = sourceAggregateCommand;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            if (!_sourceAggregateCommand.CohortIdentificationConfigurationIfAny.Equals(_targetCohortAggregateContainer.GetCohortIdentificationConfiguration()))
                SetImpossible("Aggregate belongs to a different CohortIdentificationConfiguration");

            
            if(_sourceAggregateCommand.JoinableUsersIfAny.Any())
                SetImpossible("The following Cohort Set(s) use this PatientIndex table:" + string.Join(",",_sourceAggregateCommand.JoinableUsersIfAny.Select(j=>j.ToString())));

        }

        public override void Execute()
        {
            base.Execute();

            //remove it from it's old container (really shouldn't be in any!) 
            if(_sourceAggregateCommand.ContainerIfAny != null)
                _sourceAggregateCommand.ContainerIfAny.RemoveChild(_sourceAggregateCommand.Aggregate);

            var dialog = new YesNoYesToAllDialog();

            //remove any non IsExtractionIdentifier columns
            foreach (var dimension in _sourceAggregateCommand.Aggregate.AggregateDimensions)
                if (!dimension.IsExtractionIdentifier)
                    if (
                        dialog.ShowDialog(
                            "Changing to a CohortSet means deleting AggregateDimension '" + dimension + "'.  Ok?",
                            "Delete Aggregate Dimension") ==
                        DialogResult.Yes)
                        dimension.DeleteInDatabase();
                    else
                        return;
            
            //make it is no longer a joinable
            _sourceAggregateCommand.JoinableDeclarationIfAny.DeleteInDatabase();

            //add  it to the new container
            _targetCohortAggregateContainer.AddChild(_sourceAggregateCommand.Aggregate, 0);

            //refresh the entire configuration
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_sourceAggregateCommand.CohortIdentificationConfigurationIfAny));
        }
    }
}