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

        public string GetAxisTableRuntimeName()
        {
            return "dateAxis";
        }

        public string GetAxisTableNameFullyQualified()
        {
            return "dateAxis";
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

        public string GetDatePartBasedEqualsBetweenColumns(AxisIncrement increment, string column1, string column2)
        {
            switch (increment)
            {
                case AxisIncrement.Day:
                    return "DATE("+ column1+")=DATE(" + column2 +")";//truncate any time off column1, column2 is the axis column which never has time anyway
                case AxisIncrement.Month:
                    return string.Format("YEAR({0}) = YEAR({1}) AND MONTH({0}) = MONTH({1})", column1, column2); //for performance
                case AxisIncrement.Year:
                    return GetDatePartOfColumn(increment, column1) + "=" + GetDatePartOfColumn(increment, column2);
                case AxisIncrement.Quarter:
                    return string.Format("YEAR({0}) = YEAR({1}) AND QUARTER ({0}) = QUARTER({1})", column1, column2);
                default:
                    throw new ArgumentOutOfRangeException();
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
            {
                //axis but no pivot

                //output user variables
                string part1 = string.Join(Environment.NewLine, queryLines.Where(c => c.LocationToInsert < QueryComponent.SELECT));

                //output calendar table 
                string part2 = GetDateAxisTableDeclaration(axisIfAny);

                //output the rest of the query
                string part3 = string.Join(Environment.NewLine, queryLines.Where(c =>
                    c.LocationToInsert >= QueryComponent.SELECT && c.LocationToInsert >= QueryComponent.SELECT));

                return string.Join(Environment.NewLine, new[] { part1, part2, part3 });
            }
            else
            {
                //axis and pivot (cannot pivot without axis)
                if (axisIfAny == null)
                    throw new NotSupportedException("Expected there to be both a pivot and an axis");

                return GetPivotSetupSql(queryLines, axisIfAny, pivotColumnIfAny, countColumn);
            }

        }

        private string GetPivotSetupSql(List<CustomLine> lines, IQueryAxis axis, object pivotColumn, object countColumn)
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

            string joinDtColumnWithoutAlias;
            string joinDtColumnAlias;
            syntaxHelper.SplitLineIntoSelectSQLAndAlias(joinDtColumn.Text, out joinDtColumnWithoutAlias, out joinDtColumnAlias);

            return string.Format(@"
{0}

{1}

SET SESSION group_concat_max_len = 1000000; 

SET @columns = NULL;
SELECT
  GROUP_CONCAT(DISTINCT
    CONCAT(
      '{2}(case when {3} = ''',
      b.`Pivot`,
      ''' then {4} end) AS `',
      replace(b.`Pivot`, ' ', ''),'`'
    ) order by b.`CountName` desc
  ) INTO @columns
FROM
(
select {3} AS Pivot, {6} AS CountName
{5}
group by {3}
) as b;


SET @sql =


CONCAT(
'{7} 
{8}'
,@columns,
' {9} 
group by
{10}
order by
{10}
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
                
                //the SELECT line including optionally DISTINCT and maybe anything else?
                syntaxHelper.Escape(string.Join(Environment.NewLine, lines.Where(l => l.LocationToInsert == QueryComponent.SELECT))),

                //the only explicit column we want to include (+@columns obviously which is the dynamic pivot bit)
                syntaxHelper.Escape(joinDtColumn.Text),
                
                //the entire query FROM => WHERE
                syntaxHelper.Escape(string.Join(Environment.NewLine, lines.Where(l => l.LocationToInsert >= QueryComponent.FROM && l.LocationToInsert <= QueryComponent.WHERE))),

                syntaxHelper.Escape(joinDtColumnWithoutAlias)
                );
        }

    }

    /*            Dynamic pivot we are trying to create! like a boss.
      
      use mysql;

    SET @startDate = '2001-01-01';
    SET @endDate = now();

    drop temporary table if exists dateAxis;

    create temporary table dateAxis
    (
	    dt DATE
    );

insert into dateAxis

    SELECT distinct (@startDate + INTERVAL c.number Quarter) AS date
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

delete from dateAxis where dt > @endDate;

SET SESSION group_concat_max_len = 1000000; 

SET @columns = NULL;
SELECT
  GROUP_CONCAT(DISTINCT
    CONCAT(
      'count(case when hb_extract = ''',
      hb_extract,
      ''' then 1 end) AS ',
      replace(hb_extract, ' ', '')
    )
  ) INTO @columns
from `test2`.`biochemistry`;

SET @sql =


CONCAT(
'SELECT 
CONCAT(YEAR(dateAxis.dt),''Q'',QUARTER(dateAxis.dt)) joinDt,'
,@columns,
' FROM 
`test2`.`biochemistry` RIGHT JOIN  dateAxis ON YEAR(`test2`.`biochemistry`.`sample_date`) = YEAR(dateAxis.dt) AND QUARTER (`test2`.`biochemistry`.`sample_date`) = QUARTER(dateAxis.dt)

group by 
CONCAT(YEAR(dateAxis.dt),''Q'',QUARTER(dateAxis.dt))
order by 
CONCAT(YEAR(dateAxis.dt),''Q'',QUARTER(dateAxis.dt))');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;*/
}
