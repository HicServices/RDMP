using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.QueryBuilding.Parameters;
using MapsDirectlyToDatabaseTable;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode;

namespace CatalogueLibrary.QueryBuilding
{
    public class AggregateBuilder : ISqlQueryBuilder
    {
        private readonly TableInfo[] _forceJoinsToTheseTables;

        public string SQL { get
        {
            if (SQLOutOfDate)
                RegenerateSQL();

            return _sql;
        } }
        public string LimitationSQL { get; private set; }
        public string LabelWithComment { get; set; }
        private List<AggregateCountColumn> _countColumns;

        public string HavingSQL { get; set; }
        public AggregateTopX AggregateTopX { get; set; }

        public int LineCount { get; private set; }

        public List<QueryTimeColumn> SelectColumns { get; private set; }
        public List<TableInfo> TablesUsedInQuery { get; private set; }

        public List<JoinInfo> JoinsUsedInQuery { get; private set; }
        private List<string> _customJoinLines = new List<string>();   //Impromptu joins the user wants rammed into his query wherever we are doing proper legit joining (e.g. to munge an aggregate into a forced join against a cohort)

        public List<IFilter> Filters { get; private set; }

        public IContainer RootFilterContainer { get; set; }
        public bool CheckSyntax { get; set; }
        public TableInfo PrimaryExtractionTable { get; private set; }
        public bool Sort { get { return true; } set {throw new NotSupportedException();} }

        public int CurrentLine
        {
            get { return 0; }
        }

        public ParameterManager ParameterManager { get; private set; }

        string _sql;
        private bool SQLOutOfDate = true;

        public ICollectSqlParameters QueryLevelParameterProvider { get; set; }


        public bool DoNotWriteOutOrderBy { get; set; }

        public bool DoNotWriteOutParameters
        {
            get { return _doNotWriteOutParameters; }
            set
            {
                //no change
                if(value == _doNotWriteOutParameters)
                    return;

                _doNotWriteOutParameters = value;

                //if the user is telling us not to write out parameters we had better clear any knowledge we had of parameters from previous runs
                if(value)
                    ParameterManager.ClearNonGlobals();

                SQLOutOfDate = true;
            }
        }

        /// <summary>
        /// when adding columns you have the option of either including them in groupby (default) or omitting them from groupby.  If ommitted then the columns will be used to decide how to build the FROM statement (which tables to join etc) but not included in the SELECT and GROUP BY sections of the query
        /// </summary>
        private readonly List<IColumn> _skipGroupByForThese = new List<IColumn>();

        public AggregateBuilder(string limitationSQL, string countSQL,AggregateConfiguration aggregateConfigurationIfAny,TableInfo[] forceJoinsToTheseTables)
            : this(limitationSQL, countSQL != null?new[] { countSQL }:new string[0], aggregateConfigurationIfAny)
        {
            _forceJoinsToTheseTables = forceJoinsToTheseTables;
        }

        public AggregateBuilder(string limitationSQL, string countSQL, AggregateConfiguration aggregateConfigurationIfAny)
            : this(limitationSQL, new[] { countSQL }, aggregateConfigurationIfAny)
        {
            

        }

        public AggregateBuilder(string limitationSQL, string[] countSQLColumns, AggregateConfiguration aggregateConfigurationIfAny)
        {
            LimitationSQL = limitationSQL;
            ParameterManager = new ParameterManager();

            _countColumns = new List<AggregateCountColumn>();

            foreach (var countSqlColumn in countSQLColumns)
            {
                var c = new AggregateCountColumn(countSqlColumn);
                c.Order = int.MaxValue;//order these last
                _countColumns.Add(c);
            }

            SelectColumns = new List<QueryTimeColumn>();

            AddColumnRange(_countColumns.ToArray());

            LabelWithComment = aggregateConfigurationIfAny != null ? aggregateConfigurationIfAny.Name : "";

            QueryLevelParameterProvider = aggregateConfigurationIfAny;
            
            if (aggregateConfigurationIfAny != null)
            {
                HavingSQL = aggregateConfigurationIfAny.HavingSQL;
                AggregateTopX = aggregateConfigurationIfAny.GetTopXIfAny();
            }
        }


        public void AddColumn(IColumn col)
        {
            AddColumn(col,true);
        }

        /// <summary>
        /// Overload lets you include columns for the purposes of FROM creation but not have them also appear in GROUP BY sections
        /// </summary>
        /// <param name="col"></param>
        /// <param name="includeAsGroupBy"></param>
        public void AddColumn(IColumn col, bool includeAsGroupBy)
        {
            SelectColumns.Add(new QueryTimeColumn(col));

            if (!includeAsGroupBy)
                _skipGroupByForThese.Add(col);
        }

        public void AddColumnRange(IColumn[] columnsToAdd)
        {
            AddColumnRange(columnsToAdd, true);
        }


        public void AddColumnRange(IColumn[] columnsToAdd, bool includeAsGroupBy)
        {
            foreach (IColumn column in columnsToAdd)
            {
                SelectColumns.Add(new QueryTimeColumn(column));

                if (!includeAsGroupBy)
                    _skipGroupByForThese.Add(column);
            }
        }

        public int GetLineNumberForColumn(IColumn column)
        {
            throw new NotImplementedException();
        }

        private int _pivotID=-1;
        private bool _doNotWriteOutParameters;

        public void SetPivotToDimensionID(AggregateDimension pivot)
        {
            //ensure it has an alias
            if (string.IsNullOrWhiteSpace(pivot.Alias))
            {
                pivot.Alias = "MyPivot";
                pivot.SaveToDatabase();
            }

            _pivotID = pivot.ID;
        }

        /// <summary>
        /// Populates _sql (SQL property) and resolves all parameters, filters containers etc.  Basically Finalizes this query builder 
        /// </summary>
        public void RegenerateSQL()
        {
            if (Sort)
                SelectColumns.Sort();

            ParameterManager.ClearNonGlobals();

            if(QueryLevelParameterProvider != null)
                ParameterManager.AddParametersFor(QueryLevelParameterProvider,ParameterLevel.QueryLevel);

            QueryTimeColumn pivotDimension = null;

            if(_pivotID != -1)
                try
                {
                    pivotDimension = SelectColumns.Single(
                        qtc => qtc.IColumn is AggregateDimension 
                               &&
                               ((AggregateDimension)qtc.IColumn).ID == _pivotID);


                    wrapPivotDimensionWithCleaningSql(pivotDimension);
                }
                catch (Exception e)
                {
                    throw new QueryBuildingException("Problem occurred when trying to find PivotDimension ID " + _pivotID + " in SelectColumns list",e);
                }

            //work out the axis (if there is one)
            AggregateContinuousDateAxis axis = null;
            AggregateDimension axisAppliesToDimension = null; 
            
            foreach (AggregateDimension dimension in SelectColumns.Select(c=>c.IColumn).Where(e=>e is AggregateDimension))
            {
                var availableAxis =dimension.AggregateContinuousDateAxis;

                if(availableAxis != null)
                    if (axis != null)
                        throw new QueryBuildingException(
                            "Multiple dimensions have an AggregateContinuousDateAxis within the same configuration (Dimensions " + axisAppliesToDimension.GetRuntimeName() + " and " + dimension.GetRuntimeName() + ")");
                    else
                    {
                        axis = availableAxis;
                        axisAppliesToDimension = dimension;
                    }
            }

            if (pivotDimension != null)
                if (pivotDimension.IColumn == axisAppliesToDimension)
                    throw new QueryBuildingException("Column " + pivotDimension.IColumn + " is both a PIVOT and has an AXIS configured on it, you cannot have both.");
            
            //work out all the filters 
            Filters = SqlQueryBuilderHelper.GetAllFiltersUsedInContainerTreeRecursively(RootFilterContainer);

             //tell the manager about them
            ParameterManager.AddParametersFor(Filters);


            TableInfo primary;
            TablesUsedInQuery = SqlQueryBuilderHelper.GetTablesUsedInQuery(this, out primary);

            //if user wants to force join to some other tables that don't appear in the SELECT list, who are we to stop him!
            if (_forceJoinsToTheseTables != null)
                foreach (TableInfo t in _forceJoinsToTheseTables)
                {
                    if (!TablesUsedInQuery.Contains(t))
                    {
                        TablesUsedInQuery.Add(t);
                        ParameterManager.AddParametersFor(t);
                    }
                
                    //if user has force joined to a primary extraction table
                    if(t.IsPrimaryExtractionTable)
                        if (primary == null) //we don't currently know the primary (i.e. none of the SELECT columns were from primary tables so use this table as primary)
                            primary = t;
                        else if (primary.ID == t.ID) //we know the primary already but it is the same table so thats fine
                            continue;
                        else
                            //this isn't fine
                            throw new QueryBuildingException("You chose to FORCE a join to table " + t + " which is marked IsPrimaryExtractionTable but you have also selected a column called " + primary + " which is also an IsPrimaryExtractionTable (cannot have 2 different primary extraction tables)");
                }

            this.PrimaryExtractionTable = primary;

            SqlQueryBuilderHelper.FindLookups(this);

            JoinsUsedInQuery = SqlQueryBuilderHelper.FindRequiredJoins(this);

            _sql = "";

            string parameterSQL = "";

            //assuming we were not told to ignore the writing out of parameters!
            if (!DoNotWriteOutParameters)
                foreach (ISqlParameter parameter in ParameterManager.GetFinalResolvedParametersList())
                    parameterSQL += QueryBuilder.GetParameterDeclarationSQL(parameter);

            _sql += parameterSQL;

            if (pivotDimension != null)
                _sql += GetPivotDISTINCTValueGettingSql(pivotDimension);

            if (pivotDimension != null)
                _sql += GetPivotPrefixSql(axis, pivotDimension, parameterSQL);
            else
                if (axis != null)
                    _sql += GetAxisGenerationSql(axis, null);

            //put the name in as SQL comments followed by the SQL e.g. the name of an AggregateConfiguration or whatever
            if(LabelWithComment != null)
                _sql += "/*" + LabelWithComment +"*/" + Environment.NewLine;

            _sql += "SELECT ";
            
            //if there is no top X or an axis is specified (in which case the TopX applies to the PIVOT if any not the axis)
            if(AggregateTopX == null || axis != null)
                _sql += LimitationSQL + TakeNewLine();
            else
                if(!string.IsNullOrWhiteSpace(LimitationSQL))
                    throw new QueryBuildingException("You cannot have both an AggregateTopX and LimitationSQL ('" + LimitationSQL + "')");
                else
                    _sql += "TOP " + AggregateTopX.TopX + TakeNewLine();
            

            //put in all the selected columns (which are not being skipped because they aren't a part of group by)
            foreach (QueryTimeColumn col in SelectColumns.Where(col => !_skipGroupByForThese.Contains(col.IColumn)))
            {
                if(col.IColumn.HashOnDataRelease)
                    throw new QueryBuildingException("Column " + col.IColumn.GetRuntimeName() + " is marked as HashOnDataRelease and therefore cannot be used as an Aggregate dimension");

                if (col.IColumn is AggregateCountColumn && axis != null)
                {
                    //replace (*) with (AxisColumn) -- allowing for whitespaces in the brackets
                    _sql += Regex.Replace(col.GetSelectSQL(null,null), @"\(\s*\*\s*\)", "(" + axisAppliesToDimension.SelectSQL + ")")+ TakeNewLine();;
                    continue;
                }

                if (col.IColumn == axisAppliesToDimension)
                {
                    if (pivotDimension != null)
                        _sql += SqlSyntaxHelper.EscapeQuotesForDynamicSql(axis.WrapWithIntervalFunction("axis.dt")) + " joinDt," + TakeNewLine();
                    else
                        _sql += axis.WrapWithIntervalFunction("axis.dt") + " joinDt," + TakeNewLine();
                }
                else
                {
                    if (pivotDimension != null)
                        _sql += SqlSyntaxHelper.EscapeQuotesForDynamicSql(col.GetSelectSQL(null,null)) + "," + TakeNewLine();
                    else
                        _sql += col.GetSelectSQL(null,null) + "," + TakeNewLine();
                }
            }

            //get rid of the trailing comma
            _sql = _sql.TrimEnd('\n', '\r', ',');
            _sql += Environment.NewLine;

            _sql += GetFromSQL(pivotDimension!= null);

            if (axis != null)
                _sql += GetJoinSQLForAxis(axis, axisAppliesToDimension) + TakeNewLine();
            
            int[] whoCares;
            _sql += SqlQueryBuilderHelper.GetWHERESQL(this, out whoCares, pivotDimension != null);

            _sql += TakeNewLine();


            //now are there columns that...
            if (SelectColumns.Count(col => 
                !(col.IColumn is AggregateCountColumn)  //are not count(*) style columns
                &&
                !_skipGroupByForThese.Contains(col.IColumn)) > 0) //and are not being skipped for GROUP BY
            {

                //yes there are! better group by then!
                _sql += "group by " + TakeNewLine();

                foreach (var col in SelectColumns)
                {
                    if (col.IColumn is AggregateCountColumn)
                        continue;

                    //was added with skip for group by enabled
                    if (_skipGroupByForThese.Contains(col.IColumn))
                        continue;

                    //if it is an axis column just add the reference to the axis
                    if (col.IColumn == axisAppliesToDimension)
                    {
                        if (pivotDimension != null)
                            _sql += SqlSyntaxHelper.EscapeQuotesForDynamicSql(axis.WrapWithIntervalFunction("axis.dt")) + "," + TakeNewLine();
                        else
                            _sql += axis.WrapWithIntervalFunction("axis.dt") + "," + TakeNewLine();
                        
                        continue;
                    }

                    string select;
                    string alias;

                    RDMPQuerySyntaxHelper.SplitLineIntoSelectSQLAndAlias(col.GetSelectSQL(null, null), out select, out alias);

                    if (pivotDimension != null)
                        _sql += SqlSyntaxHelper.EscapeQuotesForDynamicSql(select) + "," + TakeNewLine();
                    else
                        _sql += select + "," + TakeNewLine();
                }
                _sql = _sql.TrimEnd('\n', '\r', ',') + Environment.NewLine;
                    //clear trailing last comma (and then put the newline back on again)

                _sql += GetHavingSql();

                //order by only if we are not pivotting
                if (pivotDimension == null && !DoNotWriteOutOrderBy)
                {
                    _sql += "order by " + TakeNewLine();

                    //if theres a top X (with an explicit order by)
                    if (AggregateTopX != null)
                        _sql += AggregateTopX.GetOrderBySQL(_countColumns) + TakeNewLine();
                    else
                        foreach (var col in SelectColumns)
                        {
                            if (col.IColumn is AggregateCountColumn)
                                continue;

                            //was added with skip for group by enabled
                            if (_skipGroupByForThese.Contains(col.IColumn))
                                continue;

                            //if it is an axis column just add the reference to the axis
                            if (col.IColumn == axisAppliesToDimension)
                            {
                                _sql += axis.WrapWithIntervalFunction("axis.dt") + "," + TakeNewLine();
                                continue;
                            }

                            string select;
                            string alias;

                            RDMPQuerySyntaxHelper.SplitLineIntoSelectSQLAndAlias(col.GetSelectSQL(null, null),
                                out select,
                                out alias);

                            if (pivotDimension != null)
                                _sql += SqlSyntaxHelper.EscapeQuotesForDynamicSql(select) + "," + TakeNewLine();
                            else
                                _sql += select + "," + TakeNewLine();
                        }
                }
            }
            else
                _sql += GetHavingSql();

            _sql = _sql.TrimEnd('\n', '\r', ',');

            if (pivotDimension != null)
                _sql += GetPivotPostfixSql(axisAppliesToDimension, pivotDimension);

        }

        private void wrapPivotDimensionWithCleaningSql(QueryTimeColumn pivotDimension)
        {
            pivotDimension.WrapIColumnSelectSql("LTRIM(RTRIM(REPLACE(", ",',','')))");
        }

        private string GetHavingSql()
        {
            string toReturn = "";

            //HAVING
            if (!string.IsNullOrWhiteSpace(HavingSQL))
            {
                toReturn += "HAVING" + TakeNewLine();
                toReturn += HavingSQL + TakeNewLine();
            }
            return toReturn;
        }


        private string GetFromSQL(bool escapeQuotes)
        {
            int[] whoCares;
            string toReturn = SqlQueryBuilderHelper.GetFROMSQL(this, out whoCares);
            foreach (string customJoinLine in _customJoinLines)
            {
                toReturn += customJoinLine;
                toReturn += TakeNewLine();
            }

            if(escapeQuotes)
                toReturn =  SqlSyntaxHelper.EscapeQuotesForDynamicSql(toReturn);

            return toReturn;
        }

        private string GetJoinSQLForAxis(AggregateContinuousDateAxis axis, AggregateDimension axisAppliesToDimension)
        {
            return " RIGHT JOIN  @dateAxis axis ON " + axis.GetJOINSqlWithIntervalFunction(axisAppliesToDimension.SelectSQL,"axis.dt");
        }

        private string GetPivotPrefixSql(AggregateContinuousDateAxis axis, QueryTimeColumn pivotDimension, string parameterSQL)
        {
            string toReturn="";

            string additionalSelectColumns = string.Join(",",
                SelectColumns.Where(qtc => IsNormalSelectDimension(qtc, axis,pivotDimension))
                    .Select(c => c.IColumn.GetRuntimeName()));

            toReturn =
                    @"
DECLARE @FinalSelectList as VARCHAR(MAX)
SET @FinalSelectList =";


            //if there is no axis
            if(axis == null )
                if (string.IsNullOrWhiteSpace(additionalSelectColumns))//and there are no other columns
                    toReturn += @"''"; //set it to a blank string
                else
                    toReturn += @"'"+additionalSelectColumns+"'"; //set the additionalSelectColumns
            else
            {
                //there IS an axis!
                if (string.IsNullOrWhiteSpace(additionalSelectColumns))//but fortunately no other columns
                    toReturn += @"'joinDt'"; //set it to the axis name
                else
                    toReturn += @"'joinDt,"+additionalSelectColumns+"'"; //set it to the axis name comma the rest of the columns (good luck graphing that or it even executing!)
                
            }

            toReturn += @"
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
";

            //there was no axis and no other columns!
            if (axis == null && string.IsNullOrWhiteSpace(additionalSelectColumns))

                toReturn += @"--if there isn't an axis we must trim the extra comma that would be there after the 'joinDt,' bit
set @FinalSelectList = SUBSTRING(@FinalSelectList,2,LEN(@FinalSelectList))

";
 
            if(axis == null)
                toReturn += @"
--DYNAMIC PIVOT
declare @Query varchar(MAX)

SET @Query = '

{0}

--Would normally be Select * but must make it IsNull to ensure we see 0s instead of null
select '+@FinalSelectList+'
from
(

";
else
            //There is an axis
            toReturn += @"

--DYNAMIC PIVOT
declare @Query varchar(MAX)

SET @Query = '

{0}

" + GetAxisGenerationSql(axis, pivotDimension) + @"

--Would normally be Select * but must make it IsNull to ensure we see 0s instead of null
select '+@FinalSelectList+'
from
(

";
            if(parameterSQL.Contains("''"))
                throw new QueryBuildingException("It looks like one of your sql parameters is initialized to '', this is forbidden as it makes dynamic escaping difficult, your current SQLParameters are :" + Environment.NewLine + parameterSQL);

            return string.Format(toReturn,SqlSyntaxHelper.EscapeQuotesForDynamicSql(parameterSQL));
        }

        private bool IsNormalSelectDimension(QueryTimeColumn qtc, AggregateContinuousDateAxis axisDimensionIfAny, QueryTimeColumn pivotDimensionIfAny)
        {
            //its a count(*) column
            if (qtc.IColumn is AggregateCountColumn)
                return false;

            //its not normal! because its the pivot dimension
            if (qtc == pivotDimensionIfAny)
                return false;

            //there is no axis it is probably normal
            if (axisDimensionIfAny == null)
                return true;

            //if it is an axis column and you are asking if qtc is normal or not and the axis is this qtc then it is not normal!
            if (((AggregateDimension)qtc.IColumn).ID == axisDimensionIfAny.AggregateDimension_ID)
                return false;


            //it is probably normal
            return true;
        }

        private string GetPivotPostfixSql(AggregateDimension axisDimension,QueryTimeColumn pivotDimension)
        {
            var countColumns = SelectColumns.Where(c => c.IColumn is AggregateCountColumn).ToArray();

            if(!countColumns.Any())
                throw new QueryBuildingException("Could not find any " + typeof(AggregateCountColumn).FullName + " columns in the SelectColumns collection");

            if(countColumns.Count() > 1)
                throw new QueryBuildingException("PIVOT can only be used with a single " + typeof(AggregateCountColumn).FullName + " column (you're query configuration has " + countColumns.Count() + ")");

            var countColumn = countColumns.Single();

            if(string.IsNullOrWhiteSpace(countColumn.IColumn.Alias))
                throw new QueryBuildingException("Count columns in Pivot Aggregates must have an Alias e.g. 'Count(*) as bob'");

            string toReturn = string.Format(@") s
PIVOT
(
	sum({0})
	for {1} in ('+@Columns+') --The dynamic Column list we just fetched at top of query
) piv
",
      countColumn.IColumn.GetRuntimeName(),
      pivotDimension.IColumn.GetRuntimeName());

            string orderby = "";
            

            //All other columns should be ordered by as normal
            foreach (QueryTimeColumn column in SelectColumns)
            {
                //column ? is the count(*) As Bob line
                if (column == countColumn)
                    continue;

                //Column ? is the Pivot column
                if (column == pivotDimension)
                    continue;

                if (column.IColumn == axisDimension)
                    orderby += "joinDt," + Environment.NewLine;
                else
                    if (!string.IsNullOrWhiteSpace(column.IColumn.Alias))
                        orderby += column.IColumn.Alias + "," + Environment.NewLine;

            }
            orderby = orderby.TrimEnd('\n', '\r', ',');

            if (!string.IsNullOrWhiteSpace(orderby))
                toReturn += " order by " + Environment.NewLine + orderby;

            toReturn += @"'

EXECUTE(@Query)";

            return toReturn;
        }

        private string GetPivotDISTINCTValueGettingSql(QueryTimeColumn pivotDimension)
        {
          int[] nvm;
            string where = SqlQueryBuilderHelper.GetWHERESQL(this, out nvm, false).TrimStart();
            string from = GetFromSQL(false);

            if (where.StartsWith("WHERE"))
                where = where.Substring("WHERE".Length);


            if (!string.IsNullOrWhiteSpace(where))
                where = " AND " + where;

            string pivotDimensionSQL = pivotDimension.GetSelectSQL(null, null);

            if (pivotDimensionSQL.Contains(RDMPQuerySyntaxHelper.AliasPrefix))
                pivotDimensionSQL = pivotDimensionSQL.Substring(0,pivotDimensionSQL.IndexOf(RDMPQuerySyntaxHelper.AliasPrefix));

            return string.Format(
                @"
--DYNAMICALLY FETCH COLUMN VALUES FOR USE IN PIVOT
DECLARE @Columns as VARCHAR(MAX)

--Get distinct values of the PIVOT Column if you have columns with values T and F and Z this will produce [T],[F],[Z] and you will end up with a pivot against these values
set @Columns = (
select{0}
 ',' + QUOTENAME({1}) as [text()] 
{2}
WHERE {1} IS NOT NULL and {1} <> '' 
{3}
group by 
{1}
order by 
{4}
FOR XML PATH('') 
)

set @Columns = SUBSTRING(@Columns,2,LEN(@Columns))

",

                                                   AggregateTopX != null ? " TOP " + AggregateTopX.TopX: "",
                                                   pivotDimensionSQL,
                                                   from,
                                                   where,
                                                   GetPivotOrderBySQL());
        }

        private string GetPivotOrderBySQL()
        {
            var countColumn = _countColumns.FirstOrDefault();

            if(countColumn == null)
                throw new QueryBuildingException("There is no AggregateCountColumn, how can there be an Order By?");

            if (AggregateTopX == null)
                return  countColumn.SelectSQL + " desc"; //there is no top X so order by the count(*) descending
            
            return AggregateTopX.GetOrderBySQL(_countColumns);
        }


        public string TakeNewLine()
        {
            return Environment.NewLine;
        }

        public IEnumerable<Lookup> GetDistinctRequiredLookups()
        {
            throw new NotImplementedException();
        }

        public CustomLine[] CustomLines { get { return new CustomLine[0]; }}
        


        private string GetAxisGenerationSql(AggregateContinuousDateAxis axis, QueryTimeColumn pivotDimension)
        {

            string startDate = axis.StartDate;
            string endDate = axis.EndDate;

            //if pivot dimension is set then this code appears inside dynamic SQL constant string that will be Exec'd so we have to escape single quotes 
            if (pivotDimension != null)
            {
                startDate = startDate.Replace("'", "''");
                endDate = endDate.Replace("'", "''");
            }



    return String.Format(
@"
    DECLARE	@startDate DATE
    DECLARE	@endDate DATE

    SET @startDate = {0}
    SET @endDate = {1}

    DECLARE @dateAxis TABLE
    (
	    dt DATE
    )

    DECLARE @currentDate DATE = @startDate

    WHILE @currentDate <= @endDate
    BEGIN
	    INSERT INTO @dateAxis 
		    SELECT @currentDate 

	    SET @currentDate = DATEADD({2}, 1, @currentDate)

    END

", startDate,endDate, axis.AxisIncrement);
        }


        public void AddCustomJoinLine(string joinLine)
        {
            _customJoinLines.Add(joinLine);
        }
    }


}
