// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SubComponents.Graphs;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Icons.IconProvision;
using PopupChecksUI = Rdmp.UI.ChecksUI.PopupChecksUI;

namespace Rdmp.UI.Menus
{
    internal class CohortAggregateContainerMenu : RDMPContextMenuStrip
    {
        private CohortAggregateContainer _container;

        public CohortAggregateContainerMenu(RDMPContextMenuStripArgs args, CohortAggregateContainer container): base( args, container)
        {
            _container = container;
            var cic = _container.GetCohortIdentificationConfiguration();


            Items.Add("Add Aggregate(s) into container", _activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Import), (s, e) => AddAggregates());
            Items.Add("Import (Copy of) Cohort Set into container", _activator.CoreIconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import), (s, e) => AddCohortAggregate());
            Add(new ExecuteCommandImportCohortIdentificationConfiguration(_activator,null,container));

            foreach (ToolStripMenuItem item in Items)
                item.Enabled = item.Enabled && (cic != null && !cic.Frozen);

            Add(new ExecuteCommandUnMergeCohortIdentificationConfiguration(_activator,container));

            //Add Graph results of container commands

            //this requires cache to exist (and be populated for the container)
            if (cic != null && cic.QueryCachingServer_ID != null)
            {
                var matchIdentifiers = new ToolStripMenuItem("Graph All Records For Matching Patients", _activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph));

                var availableGraphs = _activator.CoreChildProvider.AllAggregateConfigurations.Where(g => !g.IsCohortIdentificationAggregate).ToArray();
                var allCatalogues = _activator.CoreChildProvider.AllCatalogues;

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
                            Add(
                                new ExecuteCommandViewCohortAggregateGraph(_activator,
                                    new CohortSummaryAggregateGraphObjectCollection(container, graph)), Keys.None,
                                catalogueSubheading);

                        matchIdentifiers.DropDownItems.Add(catalogueSubheading);
                    }

                    Items.Add(matchIdentifiers);
                }
            }
        }

        private void AddCohortAggregate()
        {
            var cohortAggregates = _activator.CoreChildProvider.AllAggregateConfigurations.Where(c=>
                c.IsCohortIdentificationAggregate && !c.IsJoinablePatientIndexTable()).ToArray();

            if (!cohortAggregates.Any())
            {
                MessageBox.Show("You do not currently have any cohort sets");
                return;
            }

            AddAggregates(cohortAggregates);
        }

        private void AddAggregates()
        {

            var nonCohortAggregates = RepositoryLocator.CatalogueRepository.GetAllObjects<AggregateConfiguration>().Where(c => !c.IsCohortIdentificationAggregate).ToArray();

            if (!nonCohortAggregates.Any())
            {
                MessageBox.Show("You do not currently have any non-cohort AggregateConfigurations");
                return;
            }

            AddAggregates(nonCohortAggregates);
        }

        private void AddAggregates(AggregateConfiguration[] userCanPickFrom)
        {

            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator, userCanPickFrom, false, false);
            dialog.AllowMultiSelect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!dialog.MultiSelected.Any())
                    return;

                PopupChecksUI checks = new PopupChecksUI("Adding Aggregate(s)", true);


                foreach (AggregateConfiguration aggregateConfiguration in dialog.MultiSelected)
                {
                    try
                    {
                        var cmd = new AggregateConfigurationCombineable(aggregateConfiguration);
                        var cmdExecution = new ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(_activator, cmd, _container);
                        if (cmdExecution.IsImpossible)
                            checks.OnCheckPerformed(
                                new CheckEventArgs(
                                    "Could not add AggregateConfiguration " + aggregateConfiguration + " because of Reason:" +
                                    cmdExecution.ReasonCommandImpossible, CheckResult.Fail));
                        else
                        {
                            cmdExecution.Execute();
                            checks.OnCheckPerformed(new CheckEventArgs("Successfully added AggregateConfiguration " + aggregateConfiguration, CheckResult.Success));
                        }
                    }
                    catch (Exception e)
                    {
                        checks.OnCheckPerformed(new CheckEventArgs("Failed to add AggregateConfiguration " + aggregateConfiguration + "(see Exception for details)", CheckResult.Fail, e));
                    }
                }
            }
        }

    }
}