using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Tests.CohortCreation.QueryTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Tests.CohortCreation;

public class CohortVersioningTests : CohortQueryBuilderWithCacheTests
{

    private IBasicActivateItems MakeMockActivator()
    {
        var mock = Substitute.For<IBasicActivateItems>();
        mock.RepositoryLocator.Returns(RepositoryLocator);
        mock.GetDelegates().Returns(new List<CommandInvokerDelegate>());
        return mock;
    }

    [Test]
    public void TestCreationOfNewVersionOfCohort()
    {
        var cic = new CohortIdentificationConfiguration(CatalogueRepository, "mycic")
        {
            QueryCachingServer_ID = externalDatabaseServer.ID
        };
        cic.SaveToDatabase();
        var cmd = new ExecuteCommandCreateVersionOfCohortConfiguration(MakeMockActivator(), cic);
        Assert.DoesNotThrow(() => cmd.Execute());
        var newCic = CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ClonedFrom_ID", cic.ID);
        Assert.That(newCic.Length, Is.EqualTo(1));
        Assert.That(newCic.First().Version, Is.EqualTo(1));
    }
}
