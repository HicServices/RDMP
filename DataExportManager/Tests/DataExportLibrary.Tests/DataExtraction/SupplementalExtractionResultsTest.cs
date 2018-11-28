using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using NUnit.Framework;
using Tests.Common;

namespace DataExportLibrary.Tests.DataExtraction
{
    class SupplementalExtractionResultsTest:DatabaseTests
    {
        [Test]
        public void TestCreating()
        {
            var p = new Project(DataExportRepository, "MyProj");

            var ec = new ExtractionConfiguration(DataExportRepository, p);

            var cata = new Catalogue(CatalogueRepository, "MyCata");
            var tbl = new SupportingSQLTable(CatalogueRepository,cata,"Some global data");

            var othertbl = new SupportingSQLTable(CatalogueRepository, cata, "Some global data");

            var result = new SupplementalExtractionResults(DataExportRepository,ec,"select * from Globalsglba",tbl);

            Assert.IsTrue(result.IsReferenceTo(typeof(SupportingSQLTable)));
            Assert.IsTrue(result.IsReferenceTo(tbl));
            Assert.IsFalse(result.IsReferenceTo(othertbl));

        }
    }
}
