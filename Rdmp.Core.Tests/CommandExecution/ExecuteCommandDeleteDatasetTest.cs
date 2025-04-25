using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using System;
using System.Linq;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandDeleteDatasetTest: CommandCliTests
{

    [Test]
    public void TestDeleteExistingDataset()
    {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), "dataset");
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>(), Has.Length.EqualTo(1));
        var founddataset = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>().First(static ds => ds.Name == "dataset");
        var delCmd = new ExecuteCommandDeleteDataset(GetMockActivator(), founddataset);
        Assert.DoesNotThrow(() => delCmd.Execute());
        Assert.That(GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>(), Is.Empty);
    }

    [Test]
    public void TestDeleteNonExistantDataset()
    {
        Assert.That(GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>(), Is.Empty);
        var founddataset = new Core.Curation.Data.Dataset();
        var delCmd = new ExecuteCommandDeleteDataset(GetMockActivator(), founddataset);
        Assert.Throws<NullReferenceException>(() => delCmd.Execute());
        Assert.That(GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>(), Is.Empty);
    }
}