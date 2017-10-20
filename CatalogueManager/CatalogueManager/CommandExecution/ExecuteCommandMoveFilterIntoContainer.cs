using System;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMoveFilterIntoContainer : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly FilterCommand _filterCommand;
        private readonly IContainer _targetContainer;

        public ExecuteCommandMoveFilterIntoContainer(IActivateItems activator,FilterCommand filterCommand, IContainer targetContainer)
        {
            _activator = activator;
            _filterCommand = filterCommand;
            _targetContainer = targetContainer;

            if(!filterCommand.AllContainersInEntireTreeFromRootDown.Contains(targetContainer))
                SetImpossible("Filters can only be moved within their own container tree");
        }

        public override void Execute()
        {
            base.Execute();

            _filterCommand.Filter.FilterContainer_ID = _targetContainer.ID;
            ((DatabaseEntity)_filterCommand.Filter).SaveToDatabase();
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs((DatabaseEntity) _targetContainer));
        }
    }
}