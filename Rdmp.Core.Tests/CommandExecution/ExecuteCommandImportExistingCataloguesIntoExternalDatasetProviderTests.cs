using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.DataExport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Tests.CommandExecution
{
    class ExecuteCommandImportExistingCataloguesIntoExternalDatasetProviderTests: CommandCliTests
    {
        [Test]
        public void TestImportInternalCataloguesOnly()
        {
            var _cata1 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset1");
            _cata1.IsInternalDataset = true;
            var _cata2 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset2");
            _cata1.SaveToDatabase();
            _cata2.SaveToDatabase();

            var configuration = new DatasetProviderConfiguration(GetMockActivator().RepositoryLocator.CatalogueRepository, "test", "test", "test",1,"test");
            var provider = new InternalDatasetProvider(GetMockActivator(),configuration,null);
            var cmd = new ExecuteCommandImportExistingCataloguesIntoExternalDatasetProvider(GetMockActivator(), provider, false,true, false, false,false);
            Assert.That(cmd.GetCatalogues().Count, Is.EqualTo(1));
        }
        [Test]
        public void TestImportProjectSpecificCataloguesOnly()
        {
            var _cata1 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset1");

            var _cata2 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset2");
            _cata1.SaveToDatabase();
            _cata2.SaveToDatabase();

            var ext = new ExtractableDataSet(GetMockActivator().RepositoryLocator.DataExportRepository, _cata1);
            ext.SaveToDatabase();

            var configuration = new DatasetProviderConfiguration(GetMockActivator().RepositoryLocator.CatalogueRepository, "test", "test", "test", 1, "test");
            var provider = new InternalDatasetProvider(GetMockActivator(), configuration, null);
            var cmd = new ExecuteCommandImportExistingCataloguesIntoExternalDatasetProvider(GetMockActivator(), provider, false, false, true, false, false);
            Assert.That(cmd.GetCatalogues().Count, Is.EqualTo(1));
        }
        [Test]
        public void TestImportDeprecatedCataloguesOnly()
        {
            var _cata1 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset1");
            _cata1.IsDeprecated = true;
            var _cata2 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset2");
            _cata1.SaveToDatabase();
            _cata2.SaveToDatabase();

            var configuration = new DatasetProviderConfiguration(GetMockActivator().RepositoryLocator.CatalogueRepository, "test", "test", "test", 1, "test");
            var provider = new InternalDatasetProvider(GetMockActivator(), configuration, null);
            var cmd = new ExecuteCommandImportExistingCataloguesIntoExternalDatasetProvider(GetMockActivator(), provider, false, false, false, true, false);
            Assert.That(cmd.GetCatalogues().Count, Is.EqualTo(1));
        }
    }
}
