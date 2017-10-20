using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMoveCohortAggregateContainerIntoSubContainer : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly CohortAggregateContainerCommand _sourceCohortAggregateContainer;
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;

        public ExecuteCommandMoveCohortAggregateContainerIntoSubContainer(IActivateItems activator, CohortAggregateContainerCommand sourceCohortAggregateContainer, CohortAggregateContainer targetCohortAggregateContainer)
        {
            _activator = activator;
            _sourceCohortAggregateContainer = sourceCohortAggregateContainer;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            if(_sourceCohortAggregateContainer.AllSubContainersRecursively.Contains(_targetCohortAggregateContainer))
                SetImpossible("Cannot move a container into one of it's own subcontainers");
        }
    }
}