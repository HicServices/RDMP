using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Importing;
using NUnit.Framework;
using Rhino.Mocks;
using Tests.Common;

namespace CatalogueLibraryTests.ImportTests
{
    public class TestImportingAnObject:DatabaseTests
    {
        [Test]
        public void ImportACatalogue()
        {
            ObjectImporter importer = new ObjectImporter(CatalogueRepository);

            var c = new Catalogue(CatalogueRepository, "omg cata");
            Assert.AreEqual(CatalogueRepository.GetAllCatalogues().Count(), 1);

            var dict = TableRepository.GetPropertyInfos(typeof (Catalogue)).ToDictionary(p => p.Name, p2 => p2.GetValue(c));
            var c2 = (Catalogue)importer.ImportObject(typeof(Catalogue),dict);

            Assert.AreEqual(c.Name, c2.Name);
            Assert.AreNotEqual(c.ID,c2.ID);

            Assert.AreEqual(CatalogueRepository.GetAllCatalogues().Count(),2);

        }
    }
}
