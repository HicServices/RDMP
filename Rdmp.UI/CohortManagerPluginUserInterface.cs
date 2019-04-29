// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PluginChildProvision;
using Rdmp.UI.SubComponents.Graphs;

namespace Rdmp.UI
{
    public class CohortManagerPluginUserInterface:PluginUserInterface
    {
        public CohortManagerPluginUserInterface(IActivateItems itemActivator) : base(itemActivator)
        {
        
        }

        public override ToolStripMenuItem[] GetAdditionalRightClickMenuItems(object o)
        {
            #region Aggregate Graphs (Generate graphs in which we combine a cohort aggregate (or container) with an aggregate graph)

            var aggregate = o as AggregateConfiguration;
            var aggregateContainer = o as CohortAggregateContainer;

            //if it is a cohort aggregate (but not joinables since they don't match patients they match records and select many columns)
            if (aggregate != null && aggregate.IsCohortIdentificationAggregate && !aggregate.IsJoinablePatientIndexTable())
            {
                //with a cic (it really should do!)
                var cic = aggregate.GetCohortIdentificationConfigurationIfAny();

                if (cic != null)
                {
                    //find other non cohort aggregates (graphs) 
                    var graphsAvailableInCatalogue = CohortSummaryQueryBuilder.GetAllCompatibleSummariesForCohort(aggregate);

                    //and offer graph generation for the cohort subsets
                    var matchRecords = new ToolStripMenuItem("Graph Matching Records Only",ItemActivator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph));
                    var matchIdentifiers = new ToolStripMenuItem("Graph All Records For Matching Patients",ItemActivator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph));

                    matchRecords.Enabled = graphsAvailableInCatalogue.Any();
                    matchIdentifiers.Enabled = graphsAvailableInCatalogue.Any() && cic.QueryCachingServer_ID != null;

                    foreach (AggregateConfiguration graph in graphsAvailableInCatalogue)
                    {
                        //records in
                        matchRecords.DropDownItems.Add(
                            GetMenuItem(
                            new ExecuteCommandViewCohortAggregateGraph(ItemActivator,new CohortSummaryAggregateGraphObjectCollection(aggregate, graph,CohortSummaryAdjustment.WhereRecordsIn))
                            ));

                        //extraction identifiers in
                        matchIdentifiers.DropDownItems.Add(
                            GetMenuItem(
                            new ExecuteCommandViewCohortAggregateGraph(ItemActivator, new CohortSummaryAggregateGraphObjectCollection(aggregate, graph, CohortSummaryAdjustment.WhereExtractionIdentifiersIn))
                            ));
                    }

                    return new[] {matchRecords, matchIdentifiers};
                }
            }

            //if it's an aggregate container e.g. EXCEPT/UNION/INTERSECT
            if (aggregateContainer != null)
            {
                var cic = aggregateContainer.GetCohortIdentificationConfiguration();

                //this requires cache to exist (and be populated for the container)
                if (cic != null && cic.QueryCachingServer_ID != null)
                {
                    var matchIdentifiers = new ToolStripMenuItem("Graph All Records For Matching Patients", ItemActivator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph));

                    var availableGraphs = ItemActivator.CoreChildProvider.AllAggregateConfigurations.Where(g => !g.IsCohortIdentificationAggregate).ToArray();
                    var allCatalogues = ItemActivator.CoreChildProvider.AllCatalogues;

                    if (availableGraphs.Any())
                    {

                        foreach (var cata in allCatalogues.OrderBy(c => c.Name))
                        {
                            var cataGraphs = availableGraphs.Where(g => g.Catalogue_ID == cata.ID).ToArray();
                            
                            //if there are no graphs belonging to the Catalogue skip it
                            if(!cataGraphs.Any())
                                continue;

                            //otherwise create a subheading for it
                            var catalogueSubheading = new ToolStripMenuItem(cata.Name, CatalogueIcons.Catalogue);

                            //add graph for each in the Catalogue
                            foreach (var graph in cataGraphs)
                            {
                                catalogueSubheading.DropDownItems.Add(
                                    GetMenuItem(
                                        new ExecuteCommandViewCohortAggregateGraph(ItemActivator, new CohortSummaryAggregateGraphObjectCollection(aggregateContainer, graph))
                                        ));
                            }

                            matchIdentifiers.DropDownItems.Add(catalogueSubheading);
                        }

                        return new [] { matchIdentifiers };
                    }
                }
            }
            #endregion

            var cicAssocNode = o as ProjectCohortIdentificationConfigurationAssociationsNode;

            if (cicAssocNode != null)
                return
                    GetMenuArray(
                        new ExecuteCommandCreateNewCohortIdentificationConfiguration(ItemActivator).SetTarget(cicAssocNode.Project)
                        );

            return null;
        }

    }
}
