using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandAddCatalogueToCohortIdentificationSetContainer : BasicUICommandExecution
    {
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

        public ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(IActivateItems activator,CatalogueCommand catalogueCommand, CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
        {
            _catalogueCommand = catalogueCommand;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            if(!catalogueCommand.ContainsAtLeastOneExtractionIdentifier)
                SetImpossible("Catalogue " + catalogueCommand.Catalogue + " does not contain any IsExtractionIdentifier columns");
        }

        public override void Execute()
        {
            base.Execute();

            
            var cmd = _catalogueCommand.GenerateAggregateConfigurationFor(_targetCohortAggregateContainer,!SkipMandatoryFilterCreation);
            if(cmd != null)
            {
                _postImportCommand = new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(Activator,cmd, _targetCohortAggregateContainer);
                _postImportCommand.Execute();
            }
        }
    }
}