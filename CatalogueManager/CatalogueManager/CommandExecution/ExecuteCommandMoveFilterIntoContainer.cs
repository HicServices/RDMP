using System;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandMoveFilterIntoContainer : BasicUICommandExecution
    {
        private readonly FilterCommand _filterCommand;
        private readonly IContainer _targetContainer;

        public ExecuteCommandMoveFilterIntoContainer(IActivateItems activator,FilterCommand filterCommand, IContainer targetContainer) : base(activator)
        {
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
            Publish((DatabaseEntity) _targetContainer);
        }
    }
}