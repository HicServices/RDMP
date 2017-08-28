using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax.Aggregation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql.Aggregation
{
    public class MySqlAggregateHelper : IAggregateHelper
    {
        private string GetDateAxisTableDeclaration(IQueryAxis axis)
        {
            
            //QueryComponent.JoinInfoJoin
            return 
                string.Format(
            @"
use mysql;

    SET @startDate = {0};
    SET @endDate = {1};

    drop temporary table if exists dateAxis;

    create temporary table dateAxis
    (
	    dt DATE
    );

insert into dateAxis

    SELECT distinct (@startDate + INTERVAL c.number {2}) AS date
FROM (SELECT singles + tens + hundreds number FROM 
( SELECT 0 singles
UNION ALL SELECT   1 UNION ALL SELECT   2 UNION ALL SELECT   3
UNION ALL SELECT   4 UNION ALL SELECT   5 UNION ALL SELECT   6
UNION ALL SELECT   7 UNION ALL SELECT   8 UNION ALL SELECT   9
) singles JOIN 
(SELECT 0 tens
UNION ALL SELECT  10 UNION ALL SELECT  20 UNION ALL SELECT  30
UNION ALL SELECT  40 UNION ALL SELECT  50 UNION ALL SELECT  60
UNION ALL SELECT  70 UNION ALL SELECT  80 UNION ALL SELECT  90
) tens  JOIN 
(SELECT 0 hundreds
UNION ALL SELECT  100 UNION ALL SELECT  200 UNION ALL SELECT  300
UNION ALL SELECT  400 UNION ALL SELECT  500 UNION ALL SELECT  600
UNION ALL SELECT  700 UNION ALL SELECT  800 UNION ALL SELECT  900
) hundreds 
ORDER BY number DESC) c  
WHERE c.number BETWEEN 0 and 1000;

delete from dateAxis where dt > @endDate;",
            axis.StartDate,
            axis.EndDate,
            axis.AxisIncrement);
        }

        public string GetDatePartOfColumn(AxisIncrement increment, string columnSql)
        {
            switch (increment)
            {
                case AxisIncrement.Day:
                    return "DATE(" + columnSql + ")";
                case AxisIncrement.Month:
                    return "DATE_FORMAT("+columnSql+",'%Y-%m')";
                case AxisIncrement.Year:
                    return "YEAR(" + columnSql + ")";
                case AxisIncrement.Quarter:
                    return "CONCAT(YEAR(" + columnSql + "),'Q',QUARTER(" + columnSql + "))";
                default:
                    throw new ArgumentOutOfRangeException("increment");
            }

        }

        public string GetPivotPostfixSql(IQueryAxis axis, string countColumnRuntimeName, string pivotColumnRuntimeName,string groupBy, string orderBy)
        {

            string axisColumnSelectLine = GetDatePartOfColumn(axis.AxisIncrement, "dateAxis.dt");

            return
                " group by " + axisColumnSelectLine +
            @"');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;";
        }

        public string BuildAggregate(List<CustomLine> queryLines, IQueryAxis axisIfAny, object pivotColumnIfAny, object countColumn)
        {
            if (pivotColumnIfAny == null && axisIfAny == null)
                return string.Join(Environment.NewLine, queryLines);

            //axis only
            if (pivotColumnIfAny == null)
                return BuildAxisOnlyAggregate(queryLines,axisIfAny,countColumn);
            
            //axis and pivot (cannot pivot without axis)
            if (axisIfAny == null)
                throw new NotSupportedException("Expected there to be both a pivot and an axis");

            return BuildPivotAggregate(queryLines, axisIfAny, pivotColumnIfAny, countColumn);
            
        }

        private string BuildAxisOnlyAggregate(List<CustomLine> lines, IQueryAxis axis, object countColumn)
        {
            var syntaxHelper = new MySqlQuerySyntaxHelper();
            
            var countSelectLine = lines.Single(l => l.LocationToInsert == QueryComponent.QueryTimeColumn && l.RelatedObject == countColumn);

            string countSqlWithoutAlias;
            string countAlias;
            syntaxHelper.SplitLineIntoSelectSQLAndAlias(countSelectLine.Text, out countSqlWithoutAlias, out countAlias);

            //Deal with the axis dimension which is currently `mydb`.`mytbl`.`mycol` and needs to become YEAR(`mydb`.`mytbl`.`mycol`) As joinDt 
            var axisColumn = lines.Single(l => l.LocationToInsert == QueryComponent.QueryTimeColumn && l.RelatedObject == axis);

            string axisColumnWithoutAlias;
            string axisColumnAlias;
            syntaxHelper.SplitLineIntoSelectSQLAndAlias(axisColumn.Text, out axisColumnWithoutAlias, out axisColumnAlias);

            var axisGroupBy = lines.Single(l => l.LocationToInsert == QueryComponent.GroupBy && l.RelatedObject == axis);

            if (string.IsNullOrWhiteSpace(axisColumnAlias))
                axisColumnAlias = "joinDt";

            var axisColumnEndedWithComma = axisColumn.Text.EndsWith(",");
            axisColumn.Text = GetDatePartOfColumn(axis.AxisIncrement, axisColumnWithoutAlias) + " AS " + axisColumnAlias + (axisColumnEndedWithComma?",":"");

            var groupByEndedWithComma = axisGroupBy.Text.EndsWith(",");
            axisGroupBy.Text = GetDatePartOfColumn(axis.AxisIncrement, axisColumnWithoutAlias) + (groupByEndedWithComma ? "," : "");
            

            return string.Format(
                @"
{0}
{1}

SELECT 
{2} AS joinDt,dataset.{3}
FROM
dateAxis
LEFT JOIN
(
    {4}
) dataset
ON dataset.{5} = {2}
ORDER BY 
{2}
"
                ,
                string.Join(Environment.NewLine, lines.Where(c => c.LocationToInsert < QueryComponent.SELECT)),
                GetDateAxisTableDeclaration(axis),

                GetDatePartOfColumn(axis.AxisIncrement,"dateAxis.dt"),
                countAlias,
                
                //the entire query
                string.Join(Environment.NewLine, lines.Where(c => c.LocationToInsert >= QueryComponent.SELECT && c.LocationToInsert <= QueryComponent.GroupBy)),
                axisColumnAlias
                ).Trim();

        }

        private string BuildPivotAggregate(List<CustomLine> lines, IQueryAxis axis, object pivotColumn, object countColumn)
        {
            var syntaxHelper = new MySqlQuerySyntaxHelper();

            var pivotSelectLine = lines.Single(l => l.LocationToInsert == QueryComponent.QueryTimeColumn && l.RelatedObject == pivotColumn);

            string pivotSqlWithoutAlias;
            string pivotAlias;
            syntaxHelper.SplitLineIntoSelectSQLAndAlias(pivotSelectLine.Text, out pivotSqlWithoutAlias, out pivotAlias);

            var countSelectLine = lines.Single(l => l.LocationToInsert == QueryComponent.QueryTimeColumn && l.RelatedObject == countColumn);

            string countSqlWithoutAlias;
            string countAlias;
            syntaxHelper.SplitLineIntoSelectSQLAndAlias(countSelectLine.Text, out countSqlWithoutAlias, out countAlias);

            string aggregateMethod;
            string aggregateParameter;
            syntaxHelper.SplitLineIntoOuterMostMethodAndContents(countSqlWithoutAlias, out aggregateMethod,out aggregateParameter);

            if (aggregateParameter.Equals("*"))
                aggregateParameter = "1";

            var joinDtColumn = lines.Single(l => l.LocationToInsert == QueryComponent.QueryTimeColumn && l.RelatedObject == axis);

            string axisColumnWithoutAlias;
            string axisColumnAlias;
            syntaxHelper.SplitLineIntoSelectSQLAndAlias(joinDtColumn.Text, out axisColumnWithoutAlias, out axisColumnAlias);

            string whereDateColumnNotNull = lines.Any(l => l.LocationToInsert == QueryComponent.WHERE) ? "AND " : "WHERE ";
            whereDateColumnNotNull += axisColumnWithoutAlias + " IS NOT NULL";

            return string.Format(@"
{0}

{1}

SET SESSION group_concat_max_len = 1000000; 

DROP TEMPORARY TABLE IF EXISTS pivotValues;

/*Get the unique values in the pivot column into a temporary table ordered by size of the count*/
CREATE TEMPORARY TABLE pivotValues AS (
SELECT
{3} as piv,
{6} as countName
{5}
{11}
group by
{3}
);

/* Build case when x='fish' then 1 end as 'fish', case when x='cammel' then 1 end as 'cammel' etc*/
SET @columnsSelectCases = NULL;
SELECT
  GROUP_CONCAT(
    CONCAT(
      '{2}(case when {3} = ''', pivotValues.piv, ''' then {4} end) AS `', pivotValues.piv,'`'
    ) order by pivotValues.countName desc
  ) INTO @columnsSelectCases
FROM
pivotValues;

/* Build dataset.fish, dataset.cammel etc*/
SET @columnsSelectFromDataset = NULL;
SELECT
  GROUP_CONCAT(
    CONCAT(
      'dataset.`', pivotValues.piv,'`') order by pivotValues.countName desc
  ) INTO @columnsSelectFromDataset
FROM
pivotValues;


SET @sql =

CONCAT(
'
SELECT 
{7} as joinDt,',@columnsSelectFromDataset,'
FROM
dateAxis
LEFT JOIN
(
    {8}
    {9} AS joinDt,
'
    ,@columnsSelectCases,
'
{10}
group by
{9}
) dataset
ON {7} = dataset.joinDt
ORDER BY 
{7}
');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;",
                string.Join(Environment.NewLine, lines.Where(l => l.LocationToInsert < QueryComponent.SELECT)),
                GetDateAxisTableDeclaration(axis),

                aggregateMethod,
                pivotSqlWithoutAlias,
                
                aggregateParameter,

                //the from including all table joins and where but no calendar table join
                string.Join(Environment.NewLine, lines.Where(l => l.LocationToInsert >= QueryComponent.FROM && l.LocationToInsert <= QueryComponent.WHERE && l.RelatedObject != axis)),

                //the order by (should be count so that heavy populated columns come first)
                countSqlWithoutAlias,
                GetDatePartOfColumn(axis.AxisIncrement, "dateAxis.dt"),

                //the SELECT statement only (no columns)
                string.Join(Environment.NewLine, lines.Where(c => c.LocationToInsert == QueryComponent.SELECT)),

                GetDatePartOfColumn(axis.AxisIncrement,axisColumnWithoutAlias),

                //the rest of the query down to the WHERE
                string.Join(Environment.NewLine, lines.Where(c => c.LocationToInsert >= QueryComponent.FROM && c.LocationToInsert <= QueryComponent.WHERE)),
                whereDateColumnNotNull

                );
        }

        

        //so janky to double select GROUP_Concat just so we can get dataset* except join.dt -- can we do it once into @columns then again into the other

//use mysql;

//    SET @startDate = '1920-01-01';
//    SET @endDate = now();

//    drop temporary table if exists dateAxis;

//    create temporary table dateAxis
//    (
//        dt DATE
//    );

//insert into dateAxis

//    SELECT distinct (@startDate + INTERVAL c.number Year) AS date
//FROM (SELECT singles + tens + hundreds number FROM 
//( SELECT 0 singles
//UNION ALL SELECT   1 UNION ALL SELECT   2 UNION ALL SELECT   3
//UNION ALL SELECT   4 UNION ALL SELECT   5 UNION ALL SELECT   6
//UNION ALL SELECT   7 UNION ALL SELECT   8 UNION ALL SELECT   9
//) singles JOIN 
//(SELECT 0 tens
//UNION ALL SELECT  10 UNION ALL SELECT  20 UNION ALL SELECT  30
//UNION ALL SELECT  40 UNION ALL SELECT  50 UNION ALL SELECT  60
//UNION ALL SELECT  70 UNION ALL SELECT  80 UNION ALL SELECT  90
//) tens  JOIN 
//(SELECT 0 hundreds
//UNION ALL SELECT  100 UNION ALL SELECT  200 UNION ALL SELECT  300
//UNION ALL SELECT  400 UNION ALL SELECT  500 UNION ALL SELECT  600
//UNION ALL SELECT  700 UNION ALL SELECT  800 UNION ALL SELECT  900
//) hundreds 
//ORDER BY number DESC) c  
//WHERE c.number BETWEEN 0 and 1000;

//delete from dateAxis where dt > @endDate;

//SET SESSION group_concat_max_len = 1000000; 

//SET @columns = NULL;
//SELECT
//  GROUP_CONCAT(DISTINCT
//    CONCAT(
//      'count(case when `test`.`biochemistry`.`hb_extract` = ''',
//      b.`Pivot`,
//      ''' then 1 end) AS `',
//      b.`Pivot`,'`'
//    ) order by b.`CountName` desc
//  ) INTO @columns
//FROM
//(
//select `test`.`biochemistry`.`hb_extract` AS Pivot, count(*) AS CountName
//FROM 
//`test`.`biochemistry`
//group by `test`.`biochemistry`.`hb_extract`
//) as b;


//SET @columnNames = NULL;
//SELECT
//  GROUP_CONCAT(DISTINCT
//    CONCAT(
//      'dataset.`',b.`Pivot`,'`') order by b.`CountName` desc
//  ) INTO @columnNames
//FROM
//(
//select `test`.`biochemistry`.`hb_extract` AS Pivot, count(*) AS CountName
//FROM 
//`test`.`biochemistry`
//group by `test`.`biochemistry`.`hb_extract`
//) as b;




//SET @sql =


//CONCAT(
//'
//SELECT 
//YEAR(dateAxis.dt) AS joinDt,',@columnNames,'
//FROM
//dateAxis
//LEFT JOIN
//(
//    /*HbsByYear*/
//SELECT
//    YEAR(`test`.`biochemistry`.`sample_date`) AS joinDt,
//'
//    ,@columns,
//'
//FROM 
//`test`.`biochemistry`
//group by
//YEAR(`test`.`biochemistry`.`sample_date`)
//) dataset
//ON dataset.joinDt = YEAR(dateAxis.dt)
//ORDER BY 
//YEAR(dateAxis.dt)
//');

//PREPARE stmt FROM @sql;
//EXECUTE stmt;
//DEALLOCATE PREPARE stmt;

    }
}
