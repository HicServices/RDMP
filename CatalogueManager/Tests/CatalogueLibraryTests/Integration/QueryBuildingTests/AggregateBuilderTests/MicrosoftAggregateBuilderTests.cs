using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.QueryBuilding;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;

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
            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null, "count(*)", _configuration);
            builder.AddColumn(_dimension2);

            switch (increment)
            {
                case AxisIncrement.Day:
                    Assert.AreEqual(
                        CollapseWhitespace(
@"
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
Convert(date, axis.dt) AS joinDt,
count(*) AS MyCount
FROM 
T1
RIGHT JOIN  @dateAxis axis ON  Convert(date, Col2)=axis.dt
group by 
Convert(date, axis.dt)
order by 
Convert(date, axis.dt) "), CollapseWhitespace(builder.SQL));
                    break;
                case AxisIncrement.Month:
                    Assert.AreEqual(CollapseWhitespace(@"
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
CONVERT(nvarchar(7),axis.dt,126) AS joinDt,
count(*) AS MyCount
FROM 
T1
RIGHT JOIN  @dateAxis axis ON YEAR(Col2) = YEAR(axis.dt) AND MONTH(Col2) = MONTH(axis.dt)
group by 
CONVERT(nvarchar(7),axis.dt,126)
order by 
CONVERT(nvarchar(7),axis.dt,126)"),CollapseWhitespace(builder.SQL));
                    break;
                case AxisIncrement.Year:

                    Assert.AreEqual(CollapseWhitespace(@"
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
YEAR(axis.dt) AS joinDt,
count(*) AS MyCount
FROM 
T1
RIGHT JOIN  @dateAxis axis ON  YEAR(Col2)= YEAR(axis.dt)
group by 
 YEAR(axis.dt)
order by 
 YEAR(axis.dt)"), CollapseWhitespace(builder.SQL));
                    break;
                case AxisIncrement.Quarter:
                    Assert.AreEqual(CollapseWhitespace(@"
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
DATENAME(year, axis.dt) +'Q' + DATENAME(quarter,axis.dt) AS joinDt,
count(*) AS MyCount
FROM 
T1
RIGHT JOIN  @dateAxis axis ON YEAR(Col2) = YEAR(axis.dt) AND DATEPART(QUARTER, Col2) = DATEPART(QUARTER, axis.dt)
group by 
 DATENAME(year, axis.dt) +'Q' + DATENAME(quarter,axis.dt)
order by 
 DATENAME(year, axis.dt) +'Q' + DATENAME(quarter,axis.dt)"),CollapseWhitespace(builder.SQL));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("increment");
            }
        }

        

        [Test]
        public void TestAggregateBuilding_Pivot()
        {
            var builder = new CatalogueLibrary.QueryBuilding.AggregateBuilder(null,"count(*)", _configuration);

            var axis = new AggregateContinuousDateAxis(CatalogueRepository, _dimension2);
            axis.StartDate = "'2001-01-01'";
            axis.EndDate = "'2003-01-01'";
            axis.AxisIncrement = AxisIncrement.Year;
            axis.SaveToDatabase();

            builder.AddColumn(_dimension1);
            builder.AddColumn(_dimension2);
            builder.SetPivotToDimensionID(_dimension1);
            
            Assert.AreEqual(CollapseWhitespace(@"/*DYNAMICALLY FETCH COLUMN VALUES FOR USE IN PIVOT*/
DECLARE @Columns as VARCHAR(MAX)

/*Get distinct values of the PIVOT Column if you have columns with values T and F and Z this will produce [T],[F],[Z] and you will end up with a pivot against these values*/
set @Columns = (
/*MyConfig*/
SELECT
',' + QUOTENAME(LTRIM(RTRIM(REPLACE(Col1,',','')))) as [text()] 
FROM 
T1
WHERE ( LTRIM(RTRIM(REPLACE(Col1,',',''))) IS NOT NULL and LTRIM(RTRIM(REPLACE(Col1,',',''))) <> '' )

group by 
LTRIM(RTRIM(REPLACE(Col1,',','')))
order by 
count(*) desc
FOR XML PATH('') 
)

set @Columns = SUBSTRING(@Columns,2,LEN(@Columns))

DECLARE @FinalSelectList as VARCHAR(MAX)
SET @FinalSelectList = 'joinDt'

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

/*DYNAMIC PIVOT*/
declare @Query varchar(MAX)

SET @Query = '


    DECLARE	@startDate DATE
    DECLARE	@endDate DATE

    SET @startDate = ''2001-01-01''
    SET @endDate = ''2003-01-01''

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


/*Would normally be Select * but must make it IsNull to ensure we see 0s instead of null*/
select '+@FinalSelectList+'
from
(

/*MyConfig*/
SELECT 
LTRIM(RTRIM(REPLACE(Col1,'','',''''))) AS MyPivot,
 YEAR(axis.dt) AS joinDt,
count(*) AS MyCount
FROM 
T1
RIGHT JOIN  @dateAxis axis ON  YEAR(Col2)= YEAR(axis.dt)
group by 
Col1,
 YEAR(axis.dt)

) s
PIVOT
(
	sum(MyCount)
	for MyPivot in ('+@Columns+') --The dynamic Column list we just fetched at top of query
) piv'

EXECUTE(@Query)
"), CollapseWhitespace(builder.SQL));

            axis.DeleteInDatabase();
        }


    }
}
