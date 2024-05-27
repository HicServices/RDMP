using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Tests.CohortCreation.QueryTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

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
