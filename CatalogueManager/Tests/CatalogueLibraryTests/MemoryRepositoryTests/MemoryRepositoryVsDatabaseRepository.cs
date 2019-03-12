using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.MemoryRepositoryTests
{
    class MemoryRepositoryVsDatabaseRepository:DatabaseTests
    {
        //Fields that can be safely ignored when comparing an object created in memory with one created into the database.
        private static readonly string[] IgnorePropertiesWhenDiffing = new[] {"ID","Repository","CatalogueRepository","SoftwareVersion"};
        
        [Test]
        public void TestMemoryRepository_CatalogueConstructor()
        {
            var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository.GetServerDefaults());

            Catalogue memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
            Catalogue dbCatalogue = new Catalogue(CatalogueRepository,"My New Catalogue");

            
            foreach (PropertyInfo property in typeof(Catalogue).GetProperties())
            {
                if(IgnorePropertiesWhenDiffing.Contains(property.Name))
                    continue;

                var memValue = property.GetValue(memCatalogue);
                var dbValue = property.GetValue(dbCatalogue);
                
                //all other properties should be legit
                Assert.AreEqual(memValue,dbValue, "Property {0} differed Memory={1} and Db={2}",property.Name,memValue,dbValue);
            }
        }
        
    }
}