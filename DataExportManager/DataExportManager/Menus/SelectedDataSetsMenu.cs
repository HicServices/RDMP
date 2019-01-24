using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Menus;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ProjectUI.Graphs;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace DataExportManager.Menus
{
    class SelectedDataSetsMenu : RDMPContextMenuStrip
    {
        private readonly SelectedDataSets _selectedDataSet;
        private IExtractionConfiguration _extractionConfiguration;

        public SelectedDataSetsMenu(RDMPContextMenuStripArgs args, SelectedDataSets selectedDataSet): base(args, selectedDataSet)
        {
            _selectedDataSet = selectedDataSet;
            _extractionConfiguration = _selectedDataSet.ExtractionConfiguration;

            var root = selectedDataSet.RootFilterContainer;

            Add(new ExecuteCommandShow(_activator, selectedDataSet.ExtractableDataSet.Catalogue, int.MaxValue) { OverrideCommandName = "Show Catalogue" });

            Add(new ExecuteCommandExecuteExtractionConfiguration(_activator, selectedDataSet));

            Add(new ExecuteCommandRelease(_activator).SetTarget(selectedDataSet));

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

            var addRootFilter = new ToolStripMenuItem("Add Filter Container", _activator.CoreIconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Add) , (s, e) => AddFilterContainer());
            addRootFilter.Enabled = root == null;
            Items.Add(addRootFilter);

            Add(new ExecuteCommandViewSelectedDataSetsExtractionSql(_activator).SetTarget(selectedDataSet));
            
            Add(new ExecuteCommandViewThenVsNowSql(_activator, selectedDataSet));
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