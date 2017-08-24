using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.QueryBuilding;
using NUnit.Framework;
using RDMPStartup;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.QueryBuildingTests
{
    public class AggregateBuilderTests:DatabaseTests
    {
        private Catalogue _c;
        private CatalogueItem _cataItem1;
        private CatalogueItem _cataItem2;
        private TableInfo _ti;
        private ColumnInfo _columnInfo1;
        private ColumnInfo _columnInfo2;
        private ExtractionInformation _ei1;
        private ExtractionInformation _ei2;
        private AggregateConfiguration _configuration;
        private AggregateDimension _dimension1;
        private AggregateDimension _dimension2;

        [SetUp]
        public void CreateEntities()
        {
            _c = new Catalogue(CatalogueRepository, "AggregateBuilderTests");
            _cataItem1 = new CatalogueItem(CatalogueRepository, _c, "Col1");
            _cataItem2 = new CatalogueItem(CatalogueRepository, _c, "Col2");

            _ti = new TableInfo(CatalogueRepository, "T1");
            _columnInfo1 = new ColumnInfo(CatalogueRepository, "Col1", "varchar(100)", _ti);
            _columnInfo2 = new ColumnInfo(CatalogueRepository, "Col2", "date", _ti);

            _ei1 = new ExtractionInformation(CatalogueRepository, _cataItem1, _columnInfo1, _columnInfo1.Name);
            _ei2 = new ExtractionInformation(CatalogueRepository, _cataItem2, _columnInfo2, _columnInfo2.Name);

            _configuration = new AggregateConfiguration(CatalogueRepository, _c, "MyConfig");

            _dimension1 = new AggregateDimension(CatalogueRepository, _ei1, _configuration);
            _dimension2 = new AggregateDimension(CatalogueRepository, _ei2, _configuration);

            _dimension1.Order = 1;
            _dimension1.SaveToDatabase();
            _dimension2.Order = 2;
            _dimension2.SaveToDatabase();
        }
        
        [Test]
        public void TestAggregateBuilding_NoConfigurationOneDimension()
        {
            var builder = new AggregateBuilder(null, "count(*)", null);
            builder.AddColumn(_dimension1);

            Assert.AreEqual(@"/**/
SELECT 
Col1,
count(*)
FROM 
T1
group by 
Col1
order by 
Col1", builder.SQL);
        }


        [Test]
        public void TestAggregateBuilding_NoConfigurationTwoDimension()
        {
            var builder = new AggregateBuilder(null, "count(*)", null);
            builder.AddColumn(_dimension1);
            builder.AddColumn(_dimension2);

            Assert.AreEqual(@"/**/
SELECT 
Col1,
Col2,
count(*)
FROM 
T1
group by 
Col1,
Col2
order by 
Col1,
Col2", builder.SQL);
        }

        [Test]
        public void TestAggregateBuilding_ConfigurationTwoDimension()
        {
            var builder = new AggregateBuilder(null, "count(*)", _configuration);
            builder.AddColumn(_dimension1);
            builder.AddColumn(_dimension2);

            Assert.AreEqual(@"/*MyConfig*/
SELECT 
Col1,
Col2,
count(*)
FROM 
T1
group by 
Col1,
Col2
order by 
Col1,
Col2", builder.SQL);
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
                ? AggregateTopX.AggregateTopXOrderByDirection.Ascending
                : AggregateTopX.AggregateTopXOrderByDirection.Descending;
            topX.SaveToDatabase();

            var beforeCountSQL = _configuration.CountSQL;
            _configuration.CountSQL = countColField;

            var builder = _configuration.GetQueryBuilder();
            
            Assert.AreEqual(@"/*MyConfig*/
SELECT TOP 10
Col1,
Col2,
"+countColField+@"
FROM 
T1
group by 
Col1,
Col2
order by 
"+countColField+" " + (asc?"asc":"desc"), builder.SQL);

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
                ? AggregateTopX.AggregateTopXOrderByDirection.Ascending
                : AggregateTopX.AggregateTopXOrderByDirection.Descending;
            topX.SaveToDatabase();
            
            var builder = _configuration.GetQueryBuilder();

            Assert.AreEqual(@"/*MyConfig*/
SELECT TOP 10
Col1,
Col2,
count(*)
FROM 
T1
group by 
Col1,
Col2
order by 
Col1 " + (asc ? "asc" : "desc"), builder.SQL);
            
            topX.DeleteInDatabase();
        }

        [Test]
        public void TestAggregateBuilding_NoConfigurationNoDimensions()
        {
            var builder = new AggregateBuilder(null, "count(*)", null,new []{_ti});
            
            Assert.AreEqual(@"/**/
SELECT 
count(*)
FROM 
T1", builder.SQL);
        }

        [Test]
        [TestCase(AxisIncrement.Year)]
        [TestCase(AxisIncrement.Month)]
        [TestCase(AxisIncrement.Quarter)]
        [TestCase(AxisIncrement.Day)]
        public void TestAggregateBuilding_Axis(AxisIncrement increment)
        {
            var axis = new AggregateContinuousDateAxis(CatalogueRepository, _dimension2);
            axis.AxisIncrement = increment;
            axis.StartDate = "'2012-01-01'";
            axis.EndDate = "GETDATE()";
            axis.SaveToDatabase();
            var builder = new AggregateBuilder(null, "count(*)", _configuration);
            builder.AddColumn(_dimension2);

            switch (increment)
            {
                case AxisIncrement.Day:
                    Assert.AreEqual(@"
    DECLARE	@startDate DATE
    DECLARE	@endDate DATE

    SET @startDate = '2012-01-01'
    SET @endDate = GETDATE()

    DECLARE @dateAxis TABLE
    (
	    dt DATE
    )

    DECLARE @currentDate DATE = @startDate

    WHILE @currentDate <= @endDate
    BEGIN
	    INSERT INTO @dateAxis 
		    SELECT @currentDate 

	    SET @currentDate = DATEADD(Day, 1, @currentDate)

    END

/*MyConfig*/
SELECT 
 Convert(date, axis.dt)  joinDt,
count(Col2)
FROM 
T1 RIGHT JOIN  @dateAxis axis ON  Convert(date, Col2) =axis.dt

group by 
 Convert(date, axis.dt) 
order by 
 Convert(date, axis.dt) ", builder.SQL);
                    break;
                case AxisIncrement.Month:
                    Assert.AreEqual(@"
    DECLARE	@startDate DATE
    DECLARE	@endDate DATE

    SET @startDate = '2012-01-01'
    SET @endDate = GETDATE()

    DECLARE @dateAxis TABLE
    (
	    dt DATE
    )

    DECLARE @currentDate DATE = @startDate

    WHILE @currentDate <= @endDate
    BEGIN
	    INSERT INTO @dateAxis 
		    SELECT @currentDate 

	    SET @currentDate = DATEADD(Month, 1, @currentDate)

    END

/*MyConfig*/
SELECT 
 CONVERT(nvarchar(7),axis.dt,126) joinDt,
count(Col2)
FROM 
T1 RIGHT JOIN  @dateAxis axis ON YEAR(Col2) = YEAR(axis.dt) AND MONTH(Col2) = MONTH(axis.dt)

group by 
 CONVERT(nvarchar(7),axis.dt,126)
order by 
 CONVERT(nvarchar(7),axis.dt,126)", builder.SQL);
                    break;
                case AxisIncrement.Year:

                    Assert.AreEqual(@"
    DECLARE	@startDate DATE
    DECLARE	@endDate DATE

    SET @startDate = '2012-01-01'
    SET @endDate = GETDATE()

    DECLARE @dateAxis TABLE
    (
	    dt DATE
    )

    DECLARE @currentDate DATE = @startDate

    WHILE @currentDate <= @endDate
    BEGIN
	    INSERT INTO @dateAxis 
		    SELECT @currentDate 

	    SET @currentDate = DATEADD(Year, 1, @currentDate)

    END

/*MyConfig*/
SELECT 
 YEAR(axis.dt) joinDt,
count(Col2)
FROM 
T1 RIGHT JOIN  @dateAxis axis ON  YEAR(Col2)= YEAR(axis.dt)

group by 
 YEAR(axis.dt)
order by 
 YEAR(axis.dt)", builder.SQL);
                    break;
                case AxisIncrement.Quarter:
                    Assert.AreEqual(@"
    DECLARE	@startDate DATE
    DECLARE	@endDate DATE

    SET @startDate = '2012-01-01'
    SET @endDate = GETDATE()

    DECLARE @dateAxis TABLE
    (
	    dt DATE
    )

    DECLARE @currentDate DATE = @startDate

    WHILE @currentDate <= @endDate
    BEGIN
	    INSERT INTO @dateAxis 
		    SELECT @currentDate 

	    SET @currentDate = DATEADD(Quarter, 1, @currentDate)

    END

/*MyConfig*/
SELECT 
 DATENAME(year, axis.dt) +'Q' + DATENAME(quarter,axis.dt) joinDt,
count(Col2)
FROM 
T1 RIGHT JOIN  @dateAxis axis ON YEAR(Col2) = YEAR(axis.dt) AND DATEPART(QUARTER, Col2) = DATEPART(QUARTER, axis.dt)

group by 
 DATENAME(year, axis.dt) +'Q' + DATENAME(quarter,axis.dt)
order by 
 DATENAME(year, axis.dt) +'Q' + DATENAME(quarter,axis.dt)", builder.SQL);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("increment");
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TestAggregateBuilding_Pivot(bool forgetToAlias)
        {
            var builder = new AggregateBuilder(null,forgetToAlias? "count(*)":"count(*) as fish", _configuration);

            builder.AddColumn(_dimension1);
            builder.AddColumn(_dimension2);
            builder.SetPivotToDimensionID(_dimension1);

            string sql;

            if (forgetToAlias)
            {
                var ex = Assert.Throws<QueryBuildingException>(() => sql = builder.SQL);
                Assert.AreEqual("Count columns in Pivot Aggregates must have an Alias e.g. 'Count(*) as bob'", ex.Message);
                return;
            }

            Assert.AreEqual(@"
--DYNAMICALLY FETCH COLUMN VALUES FOR USE IN PIVOT
DECLARE @Columns as VARCHAR(MAX)

--Get distinct values of the PIVOT Column if you have columns with values T and F and Z this will produce [T],[F],[Z] and you will end up with a pivot against these values
set @Columns = (
select
 ',' + QUOTENAME(LTRIM(RTRIM(REPLACE(Col1,',','')))) as [text()] 
FROM 
T1
WHERE LTRIM(RTRIM(REPLACE(Col1,',',''))) IS NOT NULL and LTRIM(RTRIM(REPLACE(Col1,',',''))) <> '' 

group by 
LTRIM(RTRIM(REPLACE(Col1,',','')))
order by 
count(*) desc
FOR XML PATH('') 
)

set @Columns = SUBSTRING(@Columns,2,LEN(@Columns))


DECLARE @FinalSelectList as VARCHAR(MAX)
SET @FinalSelectList ='Col2'
--Split up that pesky string in tsql which has the column names up into array elements again
DECLARE @value varchar(8000)
DECLARE @pos INT
DECLARE @len INT
set @pos = 0
set @len = 0

WHILE CHARINDEX(',', @Columns +',', @pos+1)>0
BEGIN
    set @len = CHARINDEX(',', @Columns +',', @pos+1) - @pos
    set @value = SUBSTRING(@Columns +',', @pos, @len)
        
    --We are constructing a version that turns: '[fish],[lama]' into 'ISNULL([fish],0) as [fish], ISNULL([lama],0) as [lama]'
	SET @FinalSelectList = @FinalSelectList + ', ISNULL(' + @value  + ',0) as ' + @value 

    set @pos = CHARINDEX(',', @Columns +',', @pos+@len) +1
END

--DYNAMIC PIVOT
declare @Query varchar(MAX)

SET @Query = '



--Would normally be Select * but must make it IsNull to ensure we see 0s instead of null
select '+@FinalSelectList+'
from
(

/*MyConfig*/
SELECT 
LTRIM(RTRIM(REPLACE(Col1,'','',''''))) AS MyPivot,
Col2,
count(*) AS fish
FROM 
T1
group by 
LTRIM(RTRIM(REPLACE(Col1,'','',''''))),
Col2) s
PIVOT
(
	sum(fish)
	for MyPivot in ('+@Columns+') --The dynamic Column list we just fetched at top of query
) piv
'

EXECUTE(@Query)", builder.SQL);
        }

        [TearDown]
        public void DeleteEntities()
        {
            _configuration.DeleteInDatabase();
            _c.DeleteInDatabase();
            _ti.DeleteInDatabase();
        }

    }
}
