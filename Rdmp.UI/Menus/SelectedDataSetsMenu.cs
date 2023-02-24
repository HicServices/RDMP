// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ProjectUI.Graphs;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus;

internal class SelectedDataSetsMenu : RDMPContextMenuStrip
{
    private readonly SelectedDataSets _selectedDataSet;
    private IExtractionConfiguration _extractionConfiguration;

    public SelectedDataSetsMenu(RDMPContextMenuStripArgs args, SelectedDataSets selectedDataSet): base(args, selectedDataSet)
    {
        _selectedDataSet = selectedDataSet;
        _extractionConfiguration = _selectedDataSet.ExtractionConfiguration;

        ReBrandActivateAs("Edit Extractable Columns", RDMPConcept.ExtractionConfiguration, OverlayKind.Edit);

        Add(new ExecuteCommandExecuteExtractionConfiguration(_activator, selectedDataSet) { Weight = 4f});

        Add(new ExecuteCommandRelease(_activator) { Weight = 4.1f }.SetTarget(selectedDataSet));



        Add(new ExecuteCommandViewThenVsNowSql(_activator, selectedDataSet) { Weight = 5.1f });


        /////////////////// Extraction Graphs //////////////////////////////
        var cata = selectedDataSet.ExtractableDataSet.Catalogue;
            
        // If the Catalogue has been deleted, don't build Catalogue specific menu items
        if(cata == null)
            return;

        var availableGraphs = cata.AggregateConfigurations.Where(a => !a.IsCohortIdentificationAggregate).ToArray();

        foreach (var graph in availableGraphs)
        {
            Add(new ExecuteCommandExecuteExtractionAggregateGraph(_activator, new ExtractionAggregateGraphObjectCollection(_selectedDataSet, graph))
            {
                SuggestedCategory = "Graph",
                OverrideCommandName = graph.Name,
                Weight = 5.2f
            });
        }
        ////////////////////////////////////////////////////////////////////
            

        Add(new ExecuteCommandOpenExtractionDirectory(_activator, selectedDataSet));
    }

}