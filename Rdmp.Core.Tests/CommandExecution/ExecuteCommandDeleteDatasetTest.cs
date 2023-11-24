using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Tests.CommandExecution
{
    internal class ExecuteCommandDeleteDatasetTest: CommandCliTests
    {

        [Test]
        public void TestDeleteExistingDataset()
        {
            var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), "dataset");
            Assert.DoesNotThrow(() => cmd.Execute());
            Assert.AreEqual(GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Dataset>().Length, 1);
            var founddataset = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Dataset>().Where(ds => ds.Name == "dataset").First();
            var delCmd = new ExecuteCommandDeleteDataset(GetMockActivator(), founddataset);
            Assert.DoesNotThrow(() => delCmd.Execute());
            Assert.AreEqual(GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Dataset>().Length, 0);
        }

        [Test]
        public void TestDeleteNonExistantDataset()
        {
            Assert.AreEqual(GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Dataset>().Length, 0);
            var founddataset = new Core.Curation.Data.Dataset();
            var delCmd = new ExecuteCommandDeleteDataset(GetMockActivator(), founddataset);
            Assert.Throws<NullReferenceException>(() => delCmd.Execute());
            Assert.AreEqual(GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Dataset>().Length, 0);
        }
    }
}
