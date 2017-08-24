using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.QueryBuildingTests.QueryBuilderTests
{
    public class MySqlQueryBuilderTests : DatabaseTests
    {
        [Test]
        public void TestQueryBuilder_MySql_Normal()
        {
            var t = new TableInfo(CatalogueRepository, "`db`.`tbl`");
            t.DatabaseType = DatabaseType.MYSQLServer;
            t.SaveToDatabase();

            var col = new ColumnInfo(CatalogueRepository, "`db`.`tbl`.`col`","varchar(10)",t);
            Assert.AreEqual("col",col.GetRuntimeName());

            var cata = new Catalogue(CatalogueRepository,"cata");
            var catalogueItem = new CatalogueItem(CatalogueRepository, cata, "col");
            var extractionInfo = new ExtractionInformation(CatalogueRepository, catalogueItem, col, col.Name);
            
            var qb = new QueryBuilder(null, null);
            qb.AddColumn(extractionInfo);
            Assert.AreEqual(
                @"SELECT 
`db`.`tbl`.`col`
FROM 
`db`.`tbl`"
                , qb.SQL.Trim());

        }
        [Test]
        public void TestQueryBuilder_MySql_Top35()
        {
            var t = new TableInfo(CatalogueRepository, "`db`.`tbl`");
            t.DatabaseType = DatabaseType.MYSQLServer;
            t.SaveToDatabase();

            var col = new ColumnInfo(CatalogueRepository, "`db`.`tbl`.`col`", "varchar(10)", t);
            Assert.AreEqual("col", col.GetRuntimeName());

            var cata = new Catalogue(CatalogueRepository, "cata");
            var catalogueItem = new CatalogueItem(CatalogueRepository, cata, "col");
            var extractionInfo = new ExtractionInformation(CatalogueRepository, catalogueItem, col, col.Name);

            var qb = new QueryBuilder(null, null);
            qb.TopX = 35;
            qb.AddColumn(extractionInfo);
            Assert.AreEqual(
                CollapseWhitespace(
                @"SELECT 
`db`.`tbl`.`col`
FROM 
`db`.`tbl`
LIMIT 35")
                , CollapseWhitespace(qb.SQL));


            //editting the topX should invalidate the SQL automatically
            qb.TopX = 50;
            Assert.AreEqual(
                CollapseWhitespace(
                @"SELECT 
`db`.`tbl`.`col`
FROM 
`db`.`tbl`
LIMIT 50")
                , CollapseWhitespace(qb.SQL));

        }
    }
}
