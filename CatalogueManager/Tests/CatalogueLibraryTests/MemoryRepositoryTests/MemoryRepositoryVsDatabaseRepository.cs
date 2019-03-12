using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.MemoryRepositoryTests
{
    class MemoryRepositoryVsDatabaseRepository:DatabaseTests
    {
        readonly MemoryCatalogueRepository _memoryRepository = new MemoryCatalogueRepository();

        [Test]
        public void TestMemoryRepository_CatalogueConstructor()
        {

            Catalogue memCatalogue = new Catalogue(_memoryRepository, "My New Catalogue");
            Catalogue dbCatalogue = new Catalogue(CatalogueRepository,"My New Catalogue");

            
            foreach (PropertyInfo property in typeof(Catalogue).GetProperties())
            {
                if(property.Name.Equals("ID") || property.Name.Equals("Repository"))
                    continue;
                
                var memValue = property.GetValue(memCatalogue);
                var dbValue = property.GetValue(dbCatalogue);
                
                //all other properties should be legit
                Assert.AreEqual(memValue,dbValue, "Property {0} differed Memory={1} and Db={2}",property.Name,memValue,dbValue);
            }

        }
        
    }
}