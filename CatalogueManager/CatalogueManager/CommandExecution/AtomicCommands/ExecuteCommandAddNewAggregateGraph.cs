using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandAddNewAggregateGraph : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly Catalogue _catalogue;

        public ExecuteCommandAddNewAggregateGraph(IActivateItems activator, Catalogue catalogue) : base(activator)
        {
            _activator = activator;
            _catalogue = catalogue;
        }
        
        public override void Execute()
        {
            base.Execute();

            var newAggregate = new AggregateConfiguration(_activator.RepositoryLocator.CatalogueRepository,_catalogue,"New Aggregate " + Guid.NewGuid());
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_catalogue));
            new ExecuteCommandActivate(_activator,newAggregate).Execute();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Add);
        }
    }
}