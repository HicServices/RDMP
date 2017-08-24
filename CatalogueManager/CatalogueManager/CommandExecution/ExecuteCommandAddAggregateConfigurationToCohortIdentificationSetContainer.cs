using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer :BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly AggregateConfigurationCommand _aggregateConfigurationCommand;
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;

        public AggregateConfiguration AggregateCreatedIfAny { get; private set; }

        public ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IActivateItems activator,AggregateConfigurationCommand aggregateConfigurationCommand, CohortAggregateContainer targetCohortAggregateContainer)
        {
            _activator = activator;
            _aggregateConfigurationCommand = aggregateConfigurationCommand;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            if(aggregateConfigurationCommand.AllContainersInTreeIfPartOfOne.Contains(_targetCohortAggregateContainer))
                SetImpossible("AggregateConfiguration " + aggregateConfigurationCommand + " is already part of this Cohort Identification Configuration");

        }

        public override void Execute()
        {
            base.Execute();

            var cic = _targetCohortAggregateContainer.GetCohortIdentificationConfiguration();

            AggregateConfiguration child;

            //it's possible that the user already thinks this cic is a cohort set for the aggregate in which case we shouldn't reimport it as a duplicate
            if (cic.IsValidNamedConfiguration(_aggregateConfigurationCommand.Aggregate))
                child = _aggregateConfigurationCommand.Aggregate;
            else
                //it belongs to a different cic or it isn't a cic aggregate or... pick one
                child = cic.ImportAggregateConfigurationAsIdentifierList(_aggregateConfigurationCommand.Aggregate,CohortCommandHelper.PickOneExtractionIdentifier);

            //current contents
            var contents = _targetCohortAggregateContainer.GetOrderedContents().ToArray();

            //insert it at the begining of the contents
            int minimumOrder = 0;
            if(contents.Any())
                minimumOrder = contents.Min(o => o.Order);

            //bump everyone down to make room
            _targetCohortAggregateContainer.CreateInsertionPointAtOrder(child,minimumOrder,true);
            _targetCohortAggregateContainer.AddChild(child,minimumOrder);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_targetCohortAggregateContainer));

            AggregateCreatedIfAny = child;
        }
    }
}