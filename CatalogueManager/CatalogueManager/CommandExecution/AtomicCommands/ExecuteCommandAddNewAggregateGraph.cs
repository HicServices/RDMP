using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

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

        public override string GetCommandHelp()
        {
            return "Add a new graph for understanding the data in a dataset e.g. number of records per year";
        }

        public override void Execute()
        {
            base.Execute();

            var newAggregate = new AggregateConfiguration(_activator.RepositoryLocator.CatalogueRepository,_catalogue,"New Aggregate " + Guid.NewGuid());
            Publish(_catalogue);
            Activate(newAggregate);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Add);
        }
    }
}