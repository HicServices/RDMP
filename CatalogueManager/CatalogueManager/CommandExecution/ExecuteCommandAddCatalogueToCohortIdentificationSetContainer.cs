using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandAddCatalogueToCohortIdentificationSetContainer : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly CatalogueCommand _catalogueCommand;
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;

        private ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer _postImportCommand;

        public bool SkipMandatoryFilterCreation { get; set; }

        public AggregateConfiguration AggregateCreatedIfAny
        {
            get
            {
                if (_postImportCommand == null)
                    return null;

                return _postImportCommand.AggregateCreatedIfAny;
            }
        }


        public ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(IActivateItems activator,CatalogueCommand catalogueCommand, CohortAggregateContainer targetCohortAggregateContainer)
        {
            _activator = activator;
            _catalogueCommand = catalogueCommand;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;
        }

        public override void Execute()
        {
            base.Execute();

            
            var cmd = _catalogueCommand.GenerateAggregateConfigurationFor(_targetCohortAggregateContainer,!SkipMandatoryFilterCreation);
            if(cmd != null)
            {
                _postImportCommand = new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator,cmd, _targetCohortAggregateContainer);
                _postImportCommand.Execute();
            }
        }
    }
}