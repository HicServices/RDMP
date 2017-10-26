using System.Collections.Concurrent;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ScintillaNET;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandReOrderAggregateContainer : BasicCommandExecution 
    {
        private IActivateItems _activator;
        private readonly CohortAggregateContainerCommand _sourceCohortAggregateContainerCommand;

        private CohortAggregateContainer _targetParent;
        private IOrderable _targetOrderable;
        private readonly InsertOption _insertOption;

        public ExecuteCommandReOrderAggregateContainer(IActivateItems activator, CohortAggregateContainerCommand sourceCohortAggregateContainerCommand, CohortAggregateContainer targetCohortAggregateContainer, InsertOption insertOption):this(targetCohortAggregateContainer,insertOption)
        {
            _activator = activator;
            _sourceCohortAggregateContainerCommand = sourceCohortAggregateContainerCommand;
            
            _targetParent = targetCohortAggregateContainer.GetParentContainerIfAny();
            
            //reorder is only possible within a container
            if (!_sourceCohortAggregateContainerCommand.ParentContainerIfAny.Equals(_targetParent))
                SetImpossible("First move containers to share the same parent container");

            if(_insertOption == InsertOption.Default)
                SetImpossible("Insert must be above/below");
        }

        public ExecuteCommandReOrderAggregateContainer(IActivateItems activator, CohortAggregateContainerCommand sourceCohortAggregateContainerCommand, AggregateConfiguration targetAggregate, InsertOption insertOption):this(targetAggregate,insertOption)
        {
            _activator = activator;
            _sourceCohortAggregateContainerCommand = sourceCohortAggregateContainerCommand;
            _targetParent = targetAggregate.GetCohortAggregateContainerIfAny();

            //if they do not share the same parent container
            if(!_sourceCohortAggregateContainerCommand.ParentContainerIfAny.Equals(_targetParent))
                SetImpossible("First move objects into the same parent container");
        }

        private ExecuteCommandReOrderAggregateContainer(IOrderable orderable, InsertOption insertOption)
        {
            _targetOrderable = orderable;
            _insertOption = insertOption;

        }

        public override void Execute()
        {
            base.Execute();

            var source = _sourceCohortAggregateContainerCommand.AggregateContainer;

            int order = _targetOrderable.Order;
            
            _targetParent.CreateInsertionPointAtOrder(source,order,_insertOption == InsertOption.InsertAbove);
            source.Order = order;
            source.SaveToDatabase();

            //refresh the parent container
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_sourceCohortAggregateContainerCommand.ParentContainerIfAny));
        }
    }
}