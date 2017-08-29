using CatalogueLibrary.Data;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.QueryBuildingTests.AggregateBuilderTests
{
    public class MySqlAggregateBuilderTests : AggregateBuilderTestsBase
    {
        [Test]
        public void Test_AggregateBuilder_MySql_Top32()
        {
            _ti.DatabaseType = DatabaseType.MYSQLServer;
            _ti.SaveToDatabase();

            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", null);
            builder.AddColumn(_dimension1);

            var topx = new AggregateTopX(CatalogueRepository, _configuration, 32);
            topx.OrderByDimensionIfAny_ID = _dimension1.ID;
            topx.SaveToDatabase();

            builder.AggregateTopX = topx;
            
            
            Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
Col1,
count(*) AS MyCount
FROM 
T1
group by 
Col1
order by 
Col1 desc
LIMIT 32"),CollapseWhitespace(builder.SQL.Trim()));


            topx.DeleteInDatabase();
        }
        [Test]
        public void Test_AggregateBuilder_MySql_Top31OrderByCountAsc()
        {
            _ti.DatabaseType = DatabaseType.MYSQLServer;
            _ti.SaveToDatabase();

            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", null);
            builder.AddColumn(_dimension1);

            var topx = new AggregateTopX(CatalogueRepository, _configuration, 31);
            topx.OrderByDirection = AggregateTopXOrderByDirection.Ascending;
            builder.AggregateTopX = topx;
            

            Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
Col1,
count(*) AS MyCount
FROM 
T1
group by 
Col1
order by 
count(*) asc
LIMIT 31"), CollapseWhitespace(builder.SQL));


            topx.DeleteInDatabase();
        }
    }
}
