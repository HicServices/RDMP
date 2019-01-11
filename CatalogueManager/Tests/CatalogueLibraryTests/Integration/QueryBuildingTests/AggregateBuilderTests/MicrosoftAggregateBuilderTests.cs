using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.QueryBuilding;
using NUnit.Framework;

namespace CatalogueLibraryTests.Integration.QueryBuildingTests.AggregateBuilderTests
{
    public class MicrosoftAggregateBuilderTests:AggregateBuilderTestsBase
    {
       
        
        [Test]
        public void TestAggregateBuilding_NoConfigurationOneDimension()
        {
            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", null);
            builder.AddColumn(_dimension1);

            Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
Col1,
count(*) AS MyCount
FROM 
T1
group by 
Col1
order by 
Col1"),CollapseWhitespace(builder.SQL));
        }


        [Test]
        public void TestAggregateBuilding_NoConfigurationTwoDimension()
        {
            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", null);
            builder.AddColumn(_dimension1);
            builder.AddColumn(_dimension2);

            Assert.AreEqual(CollapseWhitespace(CollapseWhitespace(@"/**/
SELECT
Col1,
Col2,
count(*) AS MyCount
FROM 
T1
group by 
Col1,
Col2
order by 
Col1,
Col2")),CollapseWhitespace(builder.SQL));
        }

        [Test]
        public void TestAggregateBuilding_ConfigurationTwoDimension()
        {
            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", _configuration);
            builder.AddColumn(_dimension1);
            builder.AddColumn(_dimension2);

            Assert.AreEqual(CollapseWhitespace(@"/*MyConfig*/
SELECT 
Col1,
Col2,
count(*) AS MyCount
FROM 
T1
group by 
Col1,
Col2
order by 
Col1,
Col2"), CollapseWhitespace(builder.SQL));
        }

        [Test]
        public void TwoTopXObjects()
        {
            var topX1 = new AggregateTopX(CatalogueRepository, _configuration, 10);
            var ex = Assert.Throws<SqlException>(() => new AggregateTopX(CatalogueRepository, _configuration, 10));

            Assert.IsTrue(ex.Message.Contains("ix_OneTopXPerAggregateConfiguration"));
            topX1.DeleteInDatabase();
        }

        [TestCase("count(*)",true)]
        [TestCase("count(*)", false)]
        [TestCase("max(Col1)",true)]
        [TestCase("max(Col2)", false)]
        public void TestAggregateBuilding_NoConfigurationTwoDimension_Top10(string countColField,bool asc)
        {
            var topX = new AggregateTopX(CatalogueRepository, _configuration, 10);
            topX.OrderByDirection = asc
                ? AggregateTopXOrderByDirection.Ascending
                : AggregateTopXOrderByDirection.Descending;
            topX.SaveToDatabase();

            var beforeCountSQL = _configuration.CountSQL;
            _configuration.CountSQL = countColField;

            var builder = _configuration.GetQueryBuilder();
            
            Assert.AreEqual(CollapseWhitespace(@"/*MyConfig*/
SELECT 
TOP 10
Col1,
Col2,
"+countColField+@" AS MyCount
FROM 
T1
group by 
Col1,
Col2
order by 
"+countColField+" " + (asc?"asc":"desc")),CollapseWhitespace(builder.SQL));

            _configuration.CountSQL = beforeCountSQL;
            topX.DeleteInDatabase();
        }


        [TestCase(true)]
        [TestCase(false)]
        public void TestAggregateBuilding_NoConfigurationTwoDimension_Top10DimensionOrder(bool asc)
        {
            var topX = new AggregateTopX(CatalogueRepository, _configuration, 10);
            topX.OrderByDimensionIfAny_ID = _dimension1.ID;
            topX.OrderByDirection = asc
                ? AggregateTopXOrderByDirection.Ascending
                : AggregateTopXOrderByDirection.Descending;
            topX.SaveToDatabase();
            
            var builder = _configuration.GetQueryBuilder();

            Assert.AreEqual(CollapseWhitespace(@"/*MyConfig*/
SELECT 
TOP 10
Col1,
Col2,
count(*) AS MyCount
FROM 
T1
group by 
Col1,
Col2
order by 
Col1 " + (asc ? "asc" : "desc")), CollapseWhitespace(builder.SQL));
            
            topX.DeleteInDatabase();
        }

        [Test]
        public void TestAggregateBuilding_NoConfigurationNoDimensions()
        {
            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", null,new []{_ti});
            
            Assert.AreEqual(CollapseWhitespace(@"/**/
SELECT 
count(*) AS MyCount
FROM 
T1"), CollapseWhitespace(builder.SQL));
        }

    }
}
