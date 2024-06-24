// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Tests.CohortCreation.QueryTests;
using System.Linq;

namespace Rdmp.Core.Tests.CohortCreation;

public class CohortVersioningTests : CohortQueryBuilderWithCacheTests
{

    private CohortIdentificationConfiguration GenerateCIC()
    {
        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic")
        {
            QueryCachingServer_ID = externalDatabaseServer.ID
        };
        cic.SaveToDatabase();
        cic.CreateRootContainerIfNotExists();
        return cic;
    }

    [Test]
    public void TestCreationOfNewVersionOfCohort()
    {
        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
        { DisallowInput = true };
        var cic = GenerateCIC();
        var cmd = new ExecuteCommandCreateVersionOfCohortConfiguration(activator, cic);
        Assert.DoesNotThrow(() => cmd.Execute());
        var newCic = CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ClonedFrom_ID", cic.ID);
        Assert.That(newCic, Has.Length.EqualTo(1));
        Assert.That(newCic.First().Version, Is.EqualTo(1));

        Assert.DoesNotThrow(() => cmd.Execute());
        newCic = CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ClonedFrom_ID", cic.ID);
        Assert.That(newCic, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(newCic.First().Version, Is.EqualTo(1));
            Assert.That(newCic.Last().Version, Is.EqualTo(2));
        });
    }

    [Test]
    public void TestCreatingCohortVersionWithName()
    {
        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
        { DisallowInput = true };
        var cic = GenerateCIC();
        var cmd = new ExecuteCommandCreateVersionOfCohortConfiguration(activator, cic, "MyName!");
        Assert.DoesNotThrow(() => cmd.Execute());
        var newCic = CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ClonedFrom_ID", cic.ID);
        Assert.That(newCic, Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(newCic.First().Version, Is.EqualTo(1));
            Assert.That(newCic.First().Name, Is.EqualTo("MyName!"));
        });
    }
}
