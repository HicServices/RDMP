using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandReOrderAggregate : BasicUICommandExecution
    {
        private readonly AggregateConfigurationCommand _sourceAggregateCommand;
        private CohortAggregateContainer _parentContainer;
        
        private IOrderable _targetOrder;
        private readonly InsertOption _insertOption;

        public ExecuteCommandReOrderAggregate(IActivateItems activator, AggregateConfigurationCommand sourceAggregateCommand, AggregateConfiguration targetAggregateConfiguration, InsertOption insertOption) :this(activator,targetAggregateConfiguration,insertOption)
        {
            _sourceAggregateCommand = sourceAggregateCommand;
            _parentContainer = targetAggregateConfiguration.GetCohortAggregateContainerIfAny();

            if(_parentContainer == null)
            {
                SetImpossible("Target Aggregate " + targetAggregateConfiguration + " is not part of any cohort identification set containers");
                return;
            }

            if (!sourceAggregateCommand.ContainerIfAny.Equals(_parentContainer))
            {
                SetImpossible("Cannot ReOrder, you should first move both objects into the same container (UNION / EXCEPT / INTERSECT)");
                return;
            }
        }

        public ExecuteCommandReOrderAggregate(IActivateItems activator, AggregateConfigurationCommand sourceAggregateCommand, CohortAggregateContainer targetCohortAggregateContainer, InsertOption insertOption):this(activator,targetCohortAggregateContainer,insertOption)
        {
            _sourceAggregateCommand = sourceAggregateCommand;
            _parentContainer = targetCohortAggregateContainer.GetParentContainerIfAny();
            
            if (!sourceAggregateCommand.ContainerIfAny.Equals(_parentContainer))
            {
                SetImpossible("Cannot ReOrder, you should first move both objects into the same container (UNION / EXCEPT / INTERSECT)");
                return;
            }
        }

        private ExecuteCommandReOrderAggregate(IActivateItems activator, IOrderable target, InsertOption insertOption):base(activator)
        {
            _targetOrder = target;
            _insertOption = insertOption;
        }

        public override void Execute()
        {
            base.Execute();

            var source = _sourceAggregateCommand.Aggregate;
            _sourceAggregateCommand.ContainerIfAny.RemoveChild(source);

            int targetOrder = _targetOrder.Order;
            
            _parentContainer.CreateInsertionPointAtOrder(source,targetOrder , _insertOption == InsertOption.InsertAbove);
            _parentContainer.AddChild(source, targetOrder);

            //refresh the parent container
            Publish(_parentContainer);
            

        }
    }
}