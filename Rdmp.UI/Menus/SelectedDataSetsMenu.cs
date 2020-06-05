// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ProjectUI.Graphs;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.Icons.IconProvision;


namespace Rdmp.UI.Menus
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

            AddGoTo<Catalogue>(selectedDataSet.ExtractableDataSet.Catalogue_ID);

            Add(new ExecuteCommandExecuteExtractionConfiguration(_activator, selectedDataSet));

            Add(new ExecuteCommandRelease(_activator).SetTarget(selectedDataSet));

            /////////////////// Extraction Graphs //////////////////////////////
            var graphs = new ToolStripMenuItem("View Extraction Graphs", CatalogueIcons.Graph);
            
            var availableGraphs = selectedDataSet.ExtractableDataSet.Catalogue.AggregateConfigurations.Where(a => !a.IsCohortIdentificationAggregate).ToArray();

            foreach (AggregateConfiguration ac in availableGraphs)
                graphs.DropDownItems.Add(ac.ToString(), CatalogueIcons.Graph, (s, e) => GenerateExtractionGraphs(ac));

            if(availableGraphs.Length > 1)
                graphs.DropDownItems.Add("All", null, (s,e)=>GenerateExtractionGraphs(availableGraphs));

            //must have graphs available and must have a cohort
            graphs.Enabled = graphs.DropDownItems.Count > 0 && _extractionConfiguration.Cohort_ID != null;
            Items.Add(graphs);
            ////////////////////////////////////////////////////////////////////
            
            Items.Add(new ToolStripSeparator());

            var addRootFilter = new ToolStripMenuItem("Add Filter Container", _activator.CoreIconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Add) , (s, e) => AddFilterContainer());
            addRootFilter.Enabled = root == null;
            Items.Add(addRootFilter);
            
            Add(new ExecuteCommandCreateNewFilter(_activator,
                new DeployedExtractionFilterFactory(_activator.RepositoryLocator.DataExportRepository),
                () => {
                    selectedDataSet.CreateRootContainerIfNotExists();
                    return selectedDataSet.RootFilterContainer;
                }));

            Add(new ExecuteCommandCreateNewFilterFromCatalogue(_activator,
                selectedDataSet.ExtractableDataSet.Catalogue,
                () =>
                {
                    selectedDataSet.CreateRootContainerIfNotExists();
                    return selectedDataSet.RootFilterContainer;
                }));
            
            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandViewSelectedDataSetsExtractionSql(_activator).SetTarget(selectedDataSet));
            
            Add(new ExecuteCommandViewThenVsNowSql(_activator, selectedDataSet));

            Add(new ExecuteCommandOpenExtractionDirectory(_activator, selectedDataSet));
        }


        private void AddFilterContainer()
        {
            var container = new FilterContainer(RepositoryLocator.DataExportRepository);
            _selectedDataSet.RootFilterContainer_ID = container.ID;
            _selectedDataSet.SaveToDatabase();

            Publish(_selectedDataSet);
            Emphasise(container);
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