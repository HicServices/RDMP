using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using RDMPStartup;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    class AggregateFilterContainerMenu : RDMPContextMenuStrip
    {
        private readonly AggregateFilterContainer _filterContainer;
        private ExtractionFilter[] _importableFilters;

        public AggregateFilterContainerMenu(RDMPContextMenuStripArgs args, AggregateFilterContainer filterContainer): base(args, filterContainer)
        {
            _filterContainer = filterContainer;

            var aggregate = _filterContainer.GetAggregate();

            _importableFilters = aggregate.Catalogue.GetAllFilters();

            string operationTarget = filterContainer.Operation == FilterContainerOperation.AND ? "OR" : "AND";

            Items.Add("Set Operation to " + operationTarget, null, (s, e) => FlipContainerOperation());

            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandCreateNewFilter(args.ItemActivator, new AggregateFilterFactory(args.ItemActivator.RepositoryLocator.CatalogueRepository), filterContainer));

            Items.Add(new ToolStripMenuItem("Import Filter From Catalogue", null, (s, e) => ImportFilter()));

            Items.Add(new ToolStripSeparator());

            Items.Add("Add SubContainer", GetImage(RDMPConcept.FilterContainer,OverlayKind.Add), (s, e) => AddSubcontainer());

        }

        private void FlipContainerOperation()
        {
            _filterContainer.Operation = _filterContainer.Operation == FilterContainerOperation.AND
                ? FilterContainerOperation.OR
                : FilterContainerOperation.AND;

            _filterContainer.SaveToDatabase();
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_filterContainer));
        }

        private void AddSubcontainer()
        {
            var newContainer = new AggregateFilterContainer(RepositoryLocator.CatalogueRepository,FilterContainerOperation.AND);
            _filterContainer.AddChild(newContainer);
            Publish(_filterContainer);
        }

        private void ImportFilter()
        {
            var newFilter = _activator.AdvertiseCatalogueFiltersToUser(_filterContainer, _importableFilters);

            if(newFilter != null)
            {
                _filterContainer.AddChild((AggregateFilter) newFilter);
                Publish(_filterContainer);
            }
        }
    }
}