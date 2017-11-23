using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Providers;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ProjectUI;
using DataExportManager.ProjectUI.Graphs;
using RDMPStartup;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.Menus
{
    public class SelectedDataSetsMenu : RDMPContextMenuStrip
    {
        private readonly SelectedDataSets _selectedDataSet;
        private ExtractionConfiguration _extractionConfiguration;

        public SelectedDataSetsMenu(IActivateItems activator, SelectedDataSets selectedDataSet, RDMPCollectionCommonFunctionality collection): base(activator, selectedDataSet, collection)
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

            Add(new ExecuteCommandViewSelectedDatasetsExtractionSql(_activator).SetTarget(selectedDataSet));

            AddCommonMenuItems();
        }


        private void AddFilterContainer()
        {
            var container = new FilterContainer(RepositoryLocator.DataExportRepository);
            _selectedDataSet.RootFilterContainer_ID = container.ID;
            _selectedDataSet.SaveToDatabase();

            Publish(_selectedDataSet);
        }

        private void GenerateExtractionGraphs(params AggregateConfiguration[] graphsToExecute)
        {
            try
            {
                foreach (AggregateConfiguration graph in graphsToExecute)
                {
                    var args = new ExtractionAggregateGraphObjectCollection(_selectedDataSet, graph);
                    new ExecuteCommandExecuteExtractionAggregateGraph(_activator,args).Execute();
                }

                
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}