using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Copying;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMoveCohortAggregateContainerIntoSubContainer : BasicUICommandExecution
    {
        private readonly CohortAggregateContainerCommand _sourceCohortAggregateContainer;
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;

        public ExecuteCommandMoveCohortAggregateContainerIntoSubContainer(IActivateItems activator, CohortAggregateContainerCommand sourceCohortAggregateContainer, CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
        {    
            _sourceCohortAggregateContainer = sourceCohortAggregateContainer;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            if(_sourceCohortAggregateContainer.AllSubContainersRecursively.Contains(_targetCohortAggregateContainer))
                SetImpossible("Cannot move a container into one of it's own subcontainers");

            if(_sourceCohortAggregateContainer.AggregateContainer.Equals(_targetCohortAggregateContainer))
                SetImpossible("Cannot move a container into itself");
        }

        public override void Execute()
        {
            base.Execute();
            
            var cic = _sourceCohortAggregateContainer.AggregateContainer.GetCohortIdentificationConfiguration();
            var srcContainer = _sourceCohortAggregateContainer.AggregateContainer;
            srcContainer.MakeIntoAnOrphan();
            _targetCohortAggregateContainer.AddChild(srcContainer);
            Publish(cic);
        }
    }
}