using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandLinkCatalogueToDatasetTests : CommandCliTests
{

    [Test]
    public void TestLinkCatalogueToDataset()
    {
        var _cata1 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset1");
        var _cata2 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset2");
        _cata1.SaveToDatabase();
        _cata2.SaveToDatabase();
        var _t1 = new TableInfo(GetMockActivator().RepositoryLocator.CatalogueRepository, "T1");
        var _t2 = new TableInfo(GetMockActivator().RepositoryLocator.CatalogueRepository, "T2");
        _t1.SaveToDatabase();
        _t2.SaveToDatabase();
        var _c1 = new ColumnInfo(GetMockActivator().RepositoryLocator.CatalogueRepository, "test.db", "varchar(10)", _t1);
        var _c2 = new ColumnInfo(GetMockActivator().RepositoryLocator.CatalogueRepository, "test.db", "int", _t2);
        _c1.SaveToDatabase();
        _c2.SaveToDatabase();
        var _ci1 = new CatalogueItem(GetMockActivator().RepositoryLocator.CatalogueRepository, _cata1, "PrivateIdentifierA");
        _ci1.SetColumnInfo(_c1);
        var _ci2 = new CatalogueItem(GetMockActivator().RepositoryLocator.CatalogueRepository, _cata2, "PrivateIdentifierB");
        _ci2.SetColumnInfo(_c2);
        _ci1.SaveToDatabase();
        _ci2.SaveToDatabase();


        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), "dataset");
        Assert.DoesNotThrow(cmd.Execute);
        var founddataset = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>().First();
        var foundCatalogue = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().First(static c => c.Name == "Dataset1");
        var linkCmd = new ExecuteCommandLinkCatalogueToDataset(GetMockActivator(), foundCatalogue, founddataset);
        Assert.DoesNotThrow(linkCmd.Execute);
        var columInfo = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<ColumnInfo>();
        foreach (var ci in columInfo)
        {
            Assert.That(ci.Dataset_ID, Is.EqualTo(founddataset.ID));
        }
        founddataset.DeleteInDatabase();
        foundCatalogue.DeleteInDatabase();

    }
    [Test]
    public void TestLinkCatalogueToDatasetNotAll()
    {
        var _cata1 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset1");
        var _cata2 = new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository, "Dataset2");
        _cata1.SaveToDatabase();
        _cata2.SaveToDatabase();
        var _t1 = new TableInfo(GetMockActivator().RepositoryLocator.CatalogueRepository, "T1");
        var _t2 = new TableInfo(GetMockActivator().RepositoryLocator.CatalogueRepository, "T2");
        _t1.SaveToDatabase();
        _t2.SaveToDatabase();
        var _c1 = new ColumnInfo(GetMockActivator().RepositoryLocator.CatalogueRepository, "test.db", "varchar(10)", _t1);
        var _c2 = new ColumnInfo(GetMockActivator().RepositoryLocator.CatalogueRepository, "test.db", "int", _t2);
        _c1.SaveToDatabase();
        _c2.SaveToDatabase();
        var _ci1 = new CatalogueItem(GetMockActivator().RepositoryLocator.CatalogueRepository, _cata1, "PrivateIdentifierA");
        _ci1.SetColumnInfo(_c1);
        var _ci2 = new CatalogueItem(GetMockActivator().RepositoryLocator.CatalogueRepository, _cata2, "PrivateIdentifierB");
        _ci2.SetColumnInfo(_c2);
        _ci1.SaveToDatabase();
        _ci2.SaveToDatabase();


        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), "dataset");
        Assert.DoesNotThrow(cmd.Execute);
        var founddataset = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>().First();
        var foundCatalogue = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().First(c => c.Name == "Dataset1");
        var linkCmd = new ExecuteCommandLinkCatalogueToDataset(GetMockActivator(), foundCatalogue, founddataset, false);
        Assert.DoesNotThrow(linkCmd.Execute);
        var columInfo = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>().Where(ci => _cata1.CatalogueItems.Contains(ci));
        foreach (var ci in columInfo)
        {
            Assert.That(ci.ColumnInfo.Dataset_ID, Is.EqualTo(founddataset.ID));
        }

        var columInfo2 = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<CatalogueItem>().Where(ci => _cata2.CatalogueItems.Contains(ci));
        foreach (var ci in columInfo2)
        {
            Assert.That(ci.ColumnInfo.Dataset_ID, Is.Null);
        }

    }
    [Test]
    public void TestLinkCatalogueToDatasetBadCatalogue()
    {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), "dataset");
        Assert.DoesNotThrow(cmd.Execute);
        var founddataset = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>().First();
        var linkCmd = new ExecuteCommandLinkCatalogueToDataset(GetMockActivator(), null, founddataset, false);
        Assert.Throws<ImpossibleCommandException>(linkCmd.Execute);
    }

    [Test]
    public void TestLinkCatalogueToDatasetBadDataset()
    {
        var cmd = new ExecuteCommandCreateDataset(GetMockActivator(), "dataset");
        Assert.DoesNotThrow(cmd.Execute);
        var founddataset = GetMockActivator().RepositoryLocator.CatalogueRepository.GetAllObjects<Core.Curation.Data.Datasets.Dataset>().First();
        var linkCmd = new ExecuteCommandLinkCatalogueToDataset(GetMockActivator(), new Catalogue(GetMockActivator().RepositoryLocator.CatalogueRepository,"catalogue"), null, false);
        Assert.Throws<ImpossibleCommandException>(linkCmd.Execute);
    }

    [Test] 
    public void TestLinkCatalogueToDatasetBadEverything()
    {
        var linkCmd = new ExecuteCommandLinkCatalogueToDataset(GetMockActivator(), null, null, false);
        Assert.Throws<ImpossibleCommandException>(linkCmd.Execute);
    }
}