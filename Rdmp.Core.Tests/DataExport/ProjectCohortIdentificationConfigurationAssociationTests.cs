// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Tests.DataExport;

[Category("Unit")]
public class ProjectCohortIdentificationConfigurationAssociationTests
{
    [Test]
    public void TestOrphanCic()
    {
        var memory = new MemoryDataExportRepository();
        var cic = new CohortIdentificationConfiguration(memory, "Mycic");
        var p = new Project(memory, "my proj");
        p.AssociateWithCohortIdentification(cic);

        //fetch the instance
        var cicAssoc = memory.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>().Single();

        Assert.Multiple(() =>
        {
            //relationship from p should resolve to the association link
            Assert.That(p.ProjectCohortIdentificationConfigurationAssociations[0], Is.EqualTo(cicAssoc));

            //relationship from p should resolve to the cic
            Assert.That(p.GetAssociatedCohortIdentificationConfigurations()[0], Is.EqualTo(cic));
        });

        //in order to make it an orphan we have to suppress the system default behaviour of cascading across the deletion
        var obscure = memory.ObscureDependencyFinder as CatalogueObscureDependencyFinder;
        obscure?.OtherDependencyFinders.Clear();

        //make the assoc an orphan
        cic.DeleteInDatabase();
        cicAssoc.ClearAllInjections();

        //assoc should still exist
        Assert.That(p.ProjectCohortIdentificationConfigurationAssociations[0], Is.EqualTo(cicAssoc));
        Assert.Multiple(() =>
        {
            Assert.That(p.ProjectCohortIdentificationConfigurationAssociations[0].CohortIdentificationConfiguration, Is.Null);

            //relationship from p should resolve to the cic
            Assert.That(p.GetAssociatedCohortIdentificationConfigurations(), Is.Empty);
        });

        //error should be reported in top right of program
        var ex = Assert.Throws<Exception>(() =>
            new DataExportChildProvider(new RepositoryProvider(memory), null, ThrowImmediatelyCheckNotifier.Quiet,
                null));
        Assert.That(
            ex.Message, Does.Match(@"Failed to find Associated Cohort Identification Configuration with ID \d+ which was supposed to be associated with my proj"));

        //but UI should still respond
        var childProvider = new DataExportChildProvider(new RepositoryProvider(memory), null,
            IgnoreAllErrorsCheckNotifier.Instance, null);

        //the orphan cic should not appear in the tree view under Project=>Cohorts=>Associated Cics
        var cohorts = childProvider.GetChildren(p).OfType<ProjectCohortsNode>().Single();
        var cics = childProvider.GetChildren(cohorts).OfType<ProjectCohortIdentificationConfigurationAssociationsNode>()
            .First();

        Assert.That(childProvider.GetChildren(cics), Is.Empty);
    }
}