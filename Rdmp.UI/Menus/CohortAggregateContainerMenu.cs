// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.SubComponents.Graphs;

namespace Rdmp.UI.Menus;

internal class CohortAggregateContainerMenu : RDMPContextMenuStrip
{
    public CohortAggregateContainerMenu(RDMPContextMenuStripArgs args, CohortAggregateContainer container) : base(args,
        container)
    {
        // Don't add the 'Edit' button but do allow double clicking
        args.SkipCommand<ExecuteCommandActivate>();

        var cic = container.GetCohortIdentificationConfiguration();

        //Add Graph results of container commands

        //this requires cache to exist (and be populated for the container)
        if (cic is { QueryCachingServer_ID: not null })
        {
            var matchIdentifiers = new ToolStripMenuItem("Graph All Records For Matching Patients",
                _activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph).ImageToBitmap());

            var availableGraphs = _activator.CoreChildProvider.AllAggregateConfigurations
                .Value.Where(g => !g.IsCohortIdentificationAggregate).ToArray();
            var allCatalogues = _activator.CoreChildProvider.AllCatalogues;

            if (availableGraphs.Any())
            {
                foreach (var cata in allCatalogues.Value.OrderBy(c => c.Name))
                {
                    var cataGraphs = availableGraphs.Where(g => g.Catalogue_ID == cata.ID).ToArray();

                    //if there are no graphs belonging to the Catalogue skip it
                    if (!cataGraphs.Any())
                        continue;

                    //otherwise create a subheading for it
                    var catalogueSubheading =
                        new ToolStripMenuItem(cata.Name, CatalogueIcons.Catalogue.ImageToBitmap());

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
}