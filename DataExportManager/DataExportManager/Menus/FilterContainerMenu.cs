using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Providers;
using DataExportManager.ItemActivation;
using RDMPStartup;

namespace DataExportManager.Menus
{
    public class FilterContainerMenu : RDMPContextMenuStrip
    {
        private readonly FilterContainer _filterContainer;
        private readonly SelectedDataSets _rootSelectedDataSets;
        private ExtractionFilter[] _importableFilters;

        public FilterContainerMenu(IActivateDataExportItems activator, FilterContainer filterContainer, SelectedDataSets rootSelectedDataSets):base(activator,filterContainer)
        {
            _filterContainer = filterContainer;
            _rootSelectedDataSets= rootSelectedDataSets;
            _importableFilters = _rootSelectedDataSets.ExtractableDataSet.Catalogue.GetAllFilters();

            string operationTarget = filterContainer.Operation == FilterContainerOperation.AND ? "OR" : "AND";

            Items.Add("Set Operation to " + operationTarget, null, (s, e) => FlipContainerOperation());
            
            var addFilter = new ToolStripMenuItem("Add New Filter", activator.CoreIconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Add));
            addFilter.DropDownItems.Add("Blank", null, (s, e) => AddBlankFilter());
            
            var import = new ToolStripMenuItem("From Catalogue", null, (s, e) => ImportFilter());
            import.Enabled = _importableFilters.Any();
            addFilter.DropDownItems.Add(import);

            Items.Add(addFilter);

            Items.Add("Add SubContainer", activator.CoreIconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Add), (s, e) => AddSubcontainer());

            AddCommonMenuItems();
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
            var newContainer = new FilterContainer(RepositoryLocator.DataExportRepository);
            _filterContainer.AddChild(newContainer);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_filterContainer));
        }

        private void ImportFilter()
        {
           var newFilter = _activator.AdvertiseCatalogueFiltersToUser(_filterContainer, _importableFilters);

            if(newFilter != null)
            {
                _filterContainer.AddChild((DeployedExtractionFilter)newFilter);
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_filterContainer));
            }

        }

        

        private void AddBlankFilter()
        {
            var newFilter = new DeployedExtractionFilter(RepositoryLocator.DataExportRepository, "New Filter " + Guid.NewGuid(),_filterContainer);
            _activator.ActivateFilter(this,newFilter);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_filterContainer));
        }
    }
}