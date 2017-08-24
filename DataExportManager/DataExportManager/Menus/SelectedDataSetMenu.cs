using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Providers;
using DataExportManager.ItemActivation;
using DataExportManager.ProjectUI;
using DataExportManager.ProjectUI.Graphs;
using RDMPStartup;
using ReusableUIComponents;

namespace DataExportManager.Menus
{
    public class SelectedDataSetMenu : RDMPContextMenuStrip
    {
        private readonly SelectedDataSets _selectedDataSet;
        private ExtractionConfiguration _extractionConfiguration;

        public SelectedDataSetMenu(IActivateDataExportItems activator, SelectedDataSets selectedDataSet):base(activator,selectedDataSet)
        {
            _selectedDataSet = selectedDataSet;
            _extractionConfiguration = _selectedDataSet.ExtractionConfiguration;

            var root = selectedDataSet.RootFilterContainer;

            /////////////////// Extraction Graphs //////////////////////////////
            var graphs = new ToolStripMenuItem("View Extraction Graphs", CatalogueIcons.Graph);
            
            var availableGraphs = selectedDataSet.ExtractableDataSet.Catalogue.AggregateConfigurations.Where(a => !a.IsCohortIdentificationAggregate).ToArray();

            foreach (AggregateConfiguration ac in availableGraphs)
                graphs.DropDownItems.Add(ac.ToString(), CatalogueIcons.Graph, (s, e) => GenerateExtractionGraphs(ac));

            if(availableGraphs.Length > 1)
                graphs.DropDownItems.Add("All", null, (s,e)=>GenerateExtractionGraphs(availableGraphs));

            //must not be relased and must have graphs available and must have a cohort
            graphs.Enabled = !_extractionConfiguration.IsReleased && graphs.DropDownItems.Count > 0 && _extractionConfiguration.Cohort_ID != null;
            Items.Add(graphs);
            ////////////////////////////////////////////////////////////////////

            var addRootFilter = new ToolStripMenuItem("Add Filter Container", activator.CoreIconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Add) , (s, e) => AddFilterContainer());
            addRootFilter.Enabled = root == null;
            Items.Add(addRootFilter);

            var viewSQL = new ToolStripMenuItem("View Extraction SQL", CatalogueIcons.SQL,(s,e)=>ViewSQL());
            //must have datasets and have a cohort configured
            viewSQL.Enabled = _selectedDataSet.ExtractionConfiguration.Cohort_ID != null;
            Items.Add(viewSQL);

            AddCommonMenuItems();
        }


        private void ViewSQL()
        {
            ((IActivateDataExportItems)_activator).ActivateViewExtractionSQL(this, _selectedDataSet);
        }


        private void AddFilterContainer()
        {
            var container = new FilterContainer(RepositoryLocator.DataExportRepository);
            _selectedDataSet.RootFilterContainer_ID = container.ID;
            _selectedDataSet.SaveToDatabase();

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_selectedDataSet));
        }

        private void GenerateExtractionGraphs(params AggregateConfiguration[] graphsToExecute)
        {
            try
            {
                foreach (AggregateConfiguration graph in graphsToExecute)
                {
                    var args = new ExtractionAggregateGraphObjectCollection(_selectedDataSet, graph);
                    ((IActivateDataExportItems)_activator).ExecuteExtractionExtractionAggregateGraph(this,args);
                }

                
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}