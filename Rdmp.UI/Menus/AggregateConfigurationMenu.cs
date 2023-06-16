// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.SubComponents.Graphs;

namespace Rdmp.UI.Menus;

[System.ComponentModel.DesignerCategory("")]
internal class AggregateConfigurationMenu :RDMPContextMenuStrip
{
    public AggregateConfigurationMenu(RDMPContextMenuStripArgs args, AggregateConfiguration aggregate): base(args, aggregate)
    {
        if (aggregate.IsCohortIdentificationAggregate)
        {
            args.SkipCommand<ExecuteCommandSetPivot>();
            args.SkipCommand<ExecuteCommandSetAxis>();

            Add(new ExecuteCommandAddDimension(_activator, aggregate) { SuggestedCategory = "Add" });
            args.SkipCommand<ExecuteCommandAddDimension>();
        }

        //if it is a cohort aggregate (but not joinables since they don't match patients they match records and select many columns)
        if ( aggregate.IsCohortIdentificationAggregate && !aggregate.IsJoinablePatientIndexTable())
        {
            //with a cic (it really should do!)
            var cic = aggregate.GetCohortIdentificationConfigurationIfAny();
                
            if (cic != null)
            {
                //find other non cohort aggregates (graphs) 
                AggregateConfiguration[] graphsAvailableInCatalogue;

                try
                {
                    graphsAvailableInCatalogue = CohortSummaryQueryBuilder.GetAllCompatibleSummariesForCohort(aggregate);
                }
                catch (Exception)
                {
                    // Occurs if the AggregateConfiguration is badly set up e.g. has too many extraction identifiers
                    graphsAvailableInCatalogue = Array.Empty<AggregateConfiguration>();
                }

                //and offer graph generation for the cohort subsets
                var matchRecords = new ToolStripMenuItem("Graph Matching Records Only",_activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph).ImageToBitmap());
                var matchIdentifiers = new ToolStripMenuItem("Graph All Records For Matching Patients",_activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph).ImageToBitmap());

                matchRecords.Enabled = graphsAvailableInCatalogue.Any();
                matchIdentifiers.Enabled = graphsAvailableInCatalogue.Any() && cic.QueryCachingServer_ID != null;

                foreach (var graph in graphsAvailableInCatalogue)
                {
                    //records in
                    Add(new ExecuteCommandViewCohortAggregateGraph(_activator,new CohortSummaryAggregateGraphObjectCollection(aggregate, graph, CohortSummaryAdjustment.WhereRecordsIn)),
                        Keys.None,
                        matchRecords);

                    //extraction identifiers in
                    Add(
                        new ExecuteCommandViewCohortAggregateGraph(_activator, new CohortSummaryAggregateGraphObjectCollection(aggregate, graph, CohortSummaryAdjustment.WhereExtractionIdentifiersIn)),
                        Keys.None,
                        matchIdentifiers
                    );
                }

                //Create new graph menu item
                var miGraph = new ToolStripMenuItem("Graph");
                miGraph.DropDownItems.Add(matchRecords);
                miGraph.DropDownItems.Add(matchIdentifiers);
                Items.Add(miGraph);
            }
        }

    }
}