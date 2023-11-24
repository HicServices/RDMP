
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using System;
using System.Linq;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandCreateDatasetTests : CommandCliTests
{
    [Test]
    public void TestDatasetCreationOKParameters() {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(),"TEST_dataset");
        Assert.DoesNotThrow(()=>cmd.Execute());
    }

    [Test]
    public void TestDatasetCreationNoParameters()
    {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), null);
        Assert.Throws<Exception>(() => cmd.Execute());
    }

    [Test]
    public void TestDatasetCreationOKExtendedParameters()
    {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), "TEST_dataset2","somedoi","some source");
        Assert.DoesNotThrow(() => cmd.Execute());
        var founddataset = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Dataset>().Where(ds => ds.Name == "TEST_dataset2" && ds.DigitalObjectIdentifier == "somedoi" && ds.Source == "some source").First();
        Assert.IsNotNull(founddataset);
    }
}
