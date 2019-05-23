// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SubComponents.Graphs;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class AggregateConfigurationMenu :RDMPContextMenuStrip
    {
        private readonly AggregateConfiguration _aggregate;

        public AggregateConfigurationMenu(RDMPContextMenuStripArgs args, AggregateConfiguration aggregate): base(args, aggregate)
        {
            _aggregate = aggregate;

            Add(new ExecuteCommandViewSample(args.ItemActivator, aggregate));

            Add(new ExecuteCommandDisableOrEnable(_activator, aggregate));

            //only allow them to execute graph if it is normal aggregate graph
            if (!aggregate.IsCohortIdentificationAggregate)
                Add(new ExecuteCommandExecuteAggregateGraph(_activator, aggregate));

            var addFilterContainer = new ToolStripMenuItem("Add Filter Container", GetImage(RDMPConcept.FilterContainer, OverlayKind.Add), (s, e) => AddFilterContainer());

            //if it doesn't have a root container or a hijacked container shortcut
            addFilterContainer.Enabled = aggregate.RootFilterContainer_ID == null && aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID == null;
            Items.Add(addFilterContainer);

            var addShortcutFilterContainer = new ToolStripMenuItem("Create Shortcut to Another AggregateConfigurations Filter Container",
                GetImage(aggregate, OverlayKind.Shortcut), (s, e) => ChooseHijacker());

            
            //if it doesn't have a root container or a hijacked container shortcut
            addShortcutFilterContainer.Enabled = aggregate.RootFilterContainer_ID == null && aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID == null;

            Items.Add(addShortcutFilterContainer);

            var clearShortcutFilterContainer = new ToolStripMenuItem("Clear Shortcut", GetImage(aggregate, OverlayKind.Shortcut), (s, e) => ClearShortcut());
            clearShortcutFilterContainer.Enabled = aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null;
            Items.Add(clearShortcutFilterContainer);

            Add(new ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(_activator).SetTarget(aggregate));
            
            //if it is a cohort aggregate (but not joinables since they don't match patients they match records and select many columns)
            if ( aggregate.IsCohortIdentificationAggregate && !aggregate.IsJoinablePatientIndexTable())
            {
                //with a cic (it really should do!)
                var cic = aggregate.GetCohortIdentificationConfigurationIfAny();

                if (cic != null)
                {
                    //find other non cohort aggregates (graphs) 
                    var graphsAvailableInCatalogue = CohortSummaryQueryBuilder.GetAllCompatibleSummariesForCohort(aggregate);

                    //and offer graph generation for the cohort subsets
                    var matchRecords = new ToolStripMenuItem("Graph Matching Records Only",_activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph));
                    var matchIdentifiers = new ToolStripMenuItem("Graph All Records For Matching Patients",_activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph));

                    matchRecords.Enabled = graphsAvailableInCatalogue.Any();
                    matchIdentifiers.Enabled = graphsAvailableInCatalogue.Any() && cic.QueryCachingServer_ID != null;

                    foreach (AggregateConfiguration graph in graphsAvailableInCatalogue)
                    {
                        //records in
                        Add(new ExecuteCommandViewCohortAggregateGraph(_activator,new CohortSummaryAggregateGraphObjectCollection(aggregate, graph,CohortSummaryAdjustment.WhereRecordsIn)),
                            Keys.None,
                            matchRecords);

                        //extraction identifiers in
                        Add(
                            new ExecuteCommandViewCohortAggregateGraph(_activator, new CohortSummaryAggregateGraphObjectCollection(aggregate, graph, CohortSummaryAdjustment.WhereExtractionIdentifiersIn)),
                            Keys.None,
                            matchIdentifiers
                            );
                    }

                    Items.Add(matchRecords);
                    Items.Add(matchIdentifiers);
                }
            }
        }

        private void ClearShortcut()
        {
            _aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = null;
            _aggregate.SaveToDatabase();
            Publish(_aggregate);
        }

        private void ChooseHijacker()
        {
            var others
             =
                //get all configurations
             _aggregate.Repository.GetAllObjects<AggregateConfiguration>().Where(a =>
                 //which are not themselves already shortcuts!
                 a.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID == null
                 &&
                     //and which have a filter set!
                 a.RootFilterContainer_ID != null)
                //and are not ourself!
                 .Except(new[] { _aggregate }).ToArray();

            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(others, true, false);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.Selected == null)
                    _aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = null;
                else
                    _aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID =
                        ((AggregateConfiguration) dialog.Selected).ID;

                _aggregate.SaveToDatabase();
                Publish(_aggregate);
            }
        
        }

        private void AddFilterContainer()
        {
            var newContainer = new AggregateFilterContainer(RepositoryLocator.CatalogueRepository, FilterContainerOperation.AND);
            _aggregate.RootFilterContainer_ID = newContainer.ID;
            _aggregate.SaveToDatabase();
            Publish(_aggregate);
        }
    }
}
