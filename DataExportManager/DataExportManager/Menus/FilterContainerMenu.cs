using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.Menus
{
    class FilterContainerMenu : RDMPContextMenuStrip
    {
        private readonly FilterContainer _filterContainer;
        private ExtractionFilter[] _importableFilters;

        public FilterContainerMenu(RDMPContextMenuStripArgs args, FilterContainer filterContainer): base(args, filterContainer)
        {
            _filterContainer = filterContainer;
            
            _importableFilters = filterContainer.GetSelectedDataSetIfAny().ExtractableDataSet.Catalogue.GetAllFilters();

            string operationTarget = filterContainer.Operation == FilterContainerOperation.AND ? "OR" : "AND";

            Items.Add("Set Operation to " + operationTarget, null, (s, e) => FlipContainerOperation());

            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandCreateNewFilter(args.ItemActivator,new DeployedExtractionFilterFactory(args.ItemActivator.RepositoryLocator.DataExportRepository),filterContainer));

            Items.Add(new ToolStripMenuItem("Import Filter From Catalogue", null, (s, e) => ImportFilter()));

            Items.Add(new ToolStripSeparator());
            Items.Add("Add SubContainer", _activator.CoreIconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Add), (s, e) => AddSubcontainer());

        }

        private void FlipContainerOperation()
        {
            _filterContainer.Operation = _filterContainer.Operation == FilterContainerOperation.AND
                ? FilterContainerOperation.OR
                : FilterContainerOperation.AND;

            _filterContainer.SaveToDatabase();
            Publish(_filterContainer);
        }

        private void AddSubcontainer()
        {
            var newContainer = new FilterContainer(RepositoryLocator.DataExportRepository);
            _filterContainer.AddChild(newContainer);
            Publish(_filterContainer);
        }

        private void ImportFilter()
        {
           var newFilter = _activator.AdvertiseCatalogueFiltersToUser(_filterContainer, _importableFilters);

            if(newFilter != null)
            {
                _filterContainer.AddChild((DeployedExtractionFilter)newFilter);
                Publish(_filterContainer);
            }

        }
    }
}