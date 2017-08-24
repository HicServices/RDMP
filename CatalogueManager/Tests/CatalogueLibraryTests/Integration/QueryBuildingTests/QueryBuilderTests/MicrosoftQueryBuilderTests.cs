using CatalogueLibrary.Data;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.QueryBuildingTests.QueryBuilderTests
{
    class MicrosoftQueryBuilderTests:DatabaseTests
    {
        [Test]
        public void TestQueryBuilder_MicrosoftSQLServer_Top35()
        {
            var t = new TableInfo(CatalogueRepository, "[db]..[tbl]");
            t.DatabaseType = DatabaseType.MicrosoftSQLServer;
            t.SaveToDatabase();

            var col = new ColumnInfo(CatalogueRepository, "[db]..[tbl].[col]", "varchar(10)", t);
            Assert.AreEqual("col", col.GetRuntimeName());

            var cata = new Catalogue(CatalogueRepository, "cata");
            var catalogueItem = new CatalogueItem(CatalogueRepository, cata, "col");
            var extractionInfo = new ExtractionInformation(CatalogueRepository, catalogueItem, col, col.Name);

            var qb = new CatalogueLibrary.QueryBuilding.QueryBuilder(null, null);
            qb.TopX = 35;
            qb.AddColumn(extractionInfo);
            Assert.AreEqual(
                CollapseWhitespace(
                @"SELECT 
TOP 35
[db]..[tbl].[col]
FROM 
[db]..[tbl]")
                , CollapseWhitespace(qb.SQL));


            //editting the topX should invalidate the SQL automatically
            qb.TopX = 50;
            Assert.AreEqual(
                CollapseWhitespace(
                @"SELECT 
TOP 50
[db]..[tbl].[col]
FROM 
[db]..[tbl]")
                ,CollapseWhitespace(qb.SQL));

        }
    }
}
