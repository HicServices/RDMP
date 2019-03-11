using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using NUnit.Framework;

namespace CatalogueLibraryTests.MemoryRepositoryTests
{
    class MemoryRepositoryTests
    {
        readonly MemoryCatalogueRepository _repo = new MemoryCatalogueRepository();

        [OneTimeSetUp]
        public void Setup()
        {
            ImplementationManager.Load(typeof(MicrosoftSQLImplementation).Assembly);
        }

        [Test]
        public void TestMemoryRepository_CatalogueConstructor()
        {
            Catalogue memCatalogue = new Catalogue(_repo, "My New Catalogue");

            Assert.AreEqual(memCatalogue, _repo.GetObjectByID<Catalogue>(memCatalogue.ID));
        }

        [Test]
        public void TestMemoryRepository_QueryBuilder()
        {
            Catalogue memCatalogue = new Catalogue(_repo, "My New Catalogue");

            CatalogueItem myCol = new CatalogueItem(_repo,memCatalogue,"MyCol1");

            var ti = new TableInfo(_repo, "My table");
            var col = new ColumnInfo(_repo, "Mycol", "varchar(10)", ti);

            ExtractionInformation ei = new ExtractionInformation(_repo, myCol, col, col.Name);

            Assert.AreEqual(memCatalogue, _repo.GetObjectByID<Catalogue>(memCatalogue.ID));

            var qb = new QueryBuilder(null,null);
            qb.AddColumnRange(memCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));
            
            Assert.AreEqual("",qb.SQL);
        }
    }
}
