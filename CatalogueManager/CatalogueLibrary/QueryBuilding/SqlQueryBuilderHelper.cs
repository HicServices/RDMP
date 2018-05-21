using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;

using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.QueryBuilding
{
    /// <summary>
    /// Helps generate SELECT and GROUP By queries for ISqlQueryBuilders.  This includes all the shared functionality such as finding all IFilters, Lookups,
    /// which tables to JOIN on etc.  Also handles CustomLine injection which is where you inject arbitrary lines into the query at specific points (See CustomLine).
    /// </summary>
    public class SqlQueryBuilderHelper
    {
        /// <summary>
        /// Returns all IFilters that are in the root IContainer or any subcontainers
        /// </summary>
        /// <param name="currentContainer"></param>
        /// <returns></returns>
        public static List<IFilter> GetAllFiltersUsedInContainerTreeRecursively(IContainer currentContainer)
        {
            List<IFilter> toAdd = new List<IFilter>();

            //if there is a container
            if (currentContainer != null)
            {
                if (currentContainer.GetSubContainers() != null)
                    foreach (var subContainer in currentContainer.GetSubContainers())
                    {
                        //recursively add all subcontainers' filters
                        toAdd.AddRange(GetAllFiltersUsedInContainerTreeRecursively(subContainer));
                    }

                //add filters
                if (currentContainer.GetFilters() != null)
                    toAdd.AddRange(currentContainer.GetFilters());
            }

            return toAdd;
        }

        public static void FindLookups(ISqlQueryBuilder qb)
        {
            //if there is only one table then user us selecting stuff from the lookup table only 
            if (qb.TablesUsedInQuery.Count == 1)
                return;

            if (!qb.Sort)
                throw new QueryBuildingException("Query Builder was told not to sort, but there are multiple tables (some of which may be lookups) which will likely break if the columns are not in the correct order");

            QueryTimeColumn.SetLookupStatus(qb.SelectColumns.ToArray(), qb.TablesUsedInQuery);
        }


        /// <summary>
        /// Must be called only after the ISqlQueryBuilder.TablesUsedInQuery has been set (see GetTablesUsedInQuery).  This method will resolve how 
        /// the various tables can be linked together.  Throws QueryBuildingException if it is not possible to join the tables with any known 
        /// JoinInfos / Lookup knowledge
        /// </summary>
        /// <param name="qb"></param>
        /// <returns></returns>
        public static List<JoinInfo> FindRequiredJoins(ISqlQueryBuilder qb)
        {
            List<JoinInfo> Joins = new List<JoinInfo>();

            if (qb.TablesUsedInQuery == null)
                throw new NullReferenceException("You must populate TablesUsedInQuery before calling FindRequiredJoins, try calling GetTablesUsedInQuery");

            //there are no tables so how could there be any joins!
            if (!qb.TablesUsedInQuery.Any())
                throw new QueryBuildingException("Query has no TableInfos! Make sure your query has at least one column with an underlying ColumnInfo / TableInfo set - possibly you have deleted the TableInfo? this would result in orphan CatalogueItem");

            CatalogueRepository cataRepository;
            try
            {
                cataRepository = (CatalogueRepository)qb.TablesUsedInQuery.Select(t => t.Repository).Distinct().Single();
            }
            catch (Exception e)
            {
                throw new Exception("Tables (" + string.Join(",",qb.TablesUsedInQuery)+") do not seem to come from the same repository",e);
            }

            foreach (TableInfo table1 in qb.TablesUsedInQuery)
            {
                foreach (TableInfo table2 in qb.TablesUsedInQuery)
                    if (table1.ID != table2.ID) //each table must join with a single other table
                    {
                        //figure out which of the users columns is from table 1 to join using
                        JoinInfo[] availableJoins = cataRepository.JoinInfoFinder.GetAllJoinInfosBetweenColumnInfoSets(
                            table1.ColumnInfos.ToArray(),
                            table2.ColumnInfos.ToArray());

                        if (availableJoins.Length == 0)
                            continue; //try another table

                        bool comboJoinResolved = false;

                        //if there are more than 1 join info between the two tables then we need to either do a combo join or complain to user
                        if (availableJoins.Length > 1)
                        {
                            string additionalErrorMessageWhyWeCantDoComboJoin = "";
                            //if there are multiple joins but they all join between the same 2 tables in the same direction
                            if(availableJoins.Select(j=>j.PrimaryKey.TableInfo_ID).Distinct().Count() ==1
                                &&
                                availableJoins.Select(j=>j.ForeignKey.TableInfo_ID).Distinct().Count() ==1)
                                if (availableJoins.Select(j => j.ExtractionJoinType).Distinct().Count() == 1)
                                {
                                    //add as combo join
                                    for(int i =1 ; i<availableJoins.Length;i++)
                                        availableJoins[0].AddQueryBuildingTimeComboJoinDiscovery(availableJoins[i]);
                                    comboJoinResolved = true;
                                }
                                else
                                    additionalErrorMessageWhyWeCantDoComboJoin =
                                        " Although joins are all between the same tables in the same direction, the ExtractionJoinTypes are different (e.g. LEFT and RIGHT) which prevents forming a Combo AND based join using both relationships";
                            else
                            {
                                additionalErrorMessageWhyWeCantDoComboJoin =
                                    " The Joins do not go in the same direction e.g. Table1.FK=>Table=2.PK and then a reverse relationship Table2.FK=>Table1.PK, in this case the system cannot do a Combo AND based join";
                            }

                            string possibleJoinsWere = availableJoins.Select(s => "JoinInfo[" + s.ToString() + "]").Aggregate((a, b) => a + Environment.NewLine + b);

                            if(!comboJoinResolved)
                                throw new QueryBuildingException("Found " + availableJoins.Length + " possible Joins for " + table1.Name +
                                                " and " + table2.Name + ", did not know which to use.  Available joins were:" + Environment.NewLine + possibleJoinsWere +
                                                Environment.NewLine + " It was not possible to configure a Composite Join because:" + Environment.NewLine + additionalErrorMessageWhyWeCantDoComboJoin);
                        }
                        
                        if (!Joins.Contains(availableJoins[0]))
                            Joins.Add(availableJoins[0]);
                    }

            }

            if (qb.TablesUsedInQuery.Count - GetDistinctRequiredLookups(qb).Count() - Joins.Count > 1)
                throw new QueryBuildingException("There were " + qb.TablesUsedInQuery.Count + " Tables involved in assembling this query ( "+qb.TablesUsedInQuery.Aggregate("",(s,n)=>s+n+",").TrimEnd(',')+") of which  " + GetDistinctRequiredLookups(qb).Count() + " were Lookups and " + Joins.Count + " were JoinInfos, this leaves 2+ tables unjoined (no JoinInfo found)");


            //make sure there are not multiple primary key tables (those should be configured as lookups
            if (Joins.Count > 0 && qb.PrimaryExtractionTable == null)
            {
                List<string> primaryKeyTables = new List<string>(Joins.Select(p => p.PrimaryKey.TableInfo.Name).Distinct());

                if (primaryKeyTables.Count > 1)
                {
                    //there are multiple primary key tables... see if we are configured to support them
                    string primaryKeyTablesAsString = primaryKeyTables.Aggregate((a, b) => a + "," + b);
                    throw new QueryBuildingException("Found " + primaryKeyTables.Count + " primary key tables but PrimaryExtractionTable (Fix this by setting one TableInfo as 'IsPrimaryExtractionTable'), primary key tables identified include: " + primaryKeyTablesAsString);
                }
            }

            if (qb.PrimaryExtractionTable != null && qb.TablesUsedInQuery.Contains(qb.PrimaryExtractionTable) == false)
                throw new QueryBuildingException("Specified PrimaryExtractionTable was not found amongst the chosen extraction columns");


            return Joins;

        }

        public static IEnumerable<Lookup> GetRequiredLookups(ISqlQueryBuilder qb)
        {
            return from column in qb.SelectColumns where column.IsLookupDescription select column.LookupTable;
        }

        public static IEnumerable<Lookup> GetDistinctRequiredLookups(ISqlQueryBuilder qb)
        {
            //from all columns
            return from column in qb.SelectColumns
                   where 
                   (
                        column.IsLookupForeignKey 
                        && 
                        column.IsLookupForeignKeyActuallyUsed(qb.SelectColumns) 
                   )
                   || 
                        column.IsIsolatedLookupDescription //this is when there are no foreign key columns in the SelectedColumns set but there is still a lookup description field so we have to link to the table anyway
                   select column.LookupTable;
        }

        /// <summary>
        /// Make sure you have set your Filters and SelectColumns properties before calling this method so that it can find table dependencies
        /// </summary>
        /// <param name="qb"></param>
        /// <param name="primaryExtractionTable"></param>
        /// <returns></returns>
        public static List<TableInfo> GetTablesUsedInQuery(ISqlQueryBuilder qb, out TableInfo primaryExtractionTable)
        {
            if (qb.SelectColumns == null )
                throw new QueryBuildingException("ISqlQueryBuilder.SelectedColumns is null");

            if(qb.SelectColumns.Count == 0)
                throw new QueryBuildingException("ISqlQueryBuilder.SelectedColumns is empty, use .AddColumn to add a column to the query builder");

            List<TableInfo> toReturn = new List<TableInfo>();
            primaryExtractionTable = null;
            

            //get all the tables based on selected columns
            foreach (QueryTimeColumn toExtract in qb.SelectColumns)
            {
                if (toExtract.UnderlyingColumn == null)
                    continue;

                if (qb.CheckSyntax)
                    toExtract.CheckSyntax();

                TableInfo table = toExtract.UnderlyingColumn.TableInfo;

                if (!toReturn.Contains(table))
                {
                    toReturn.Add(table);

                    if (table.IsPrimaryExtractionTable)
                        if (primaryExtractionTable == null)
                            primaryExtractionTable = table;
                        else
                            throw new QueryBuildingException("There are multiple tables marked as IsPrimaryExtractionTable:" +
                                                qb.PrimaryExtractionTable.Name + "(ID=" + qb.PrimaryExtractionTable.ID +
                                                ") and " + table.Name + "(ID=" + table.ID + ")");
                }
            }

            //get other tables we might need because they are referenced by filters
            if(qb.Filters != null && qb.Filters.Any())
            {
                foreach (IFilter filter in qb.Filters)
                {
                    ColumnInfo col = filter.GetColumnInfoIfExists();
                    if (col != null)
                    {
                        var tableInfoOfFilter = col.TableInfo;
                        if(!toReturn.Contains(tableInfoOfFilter))
                            toReturn.Add(tableInfoOfFilter);
                    }
                }

                toReturn = AddOpportunisticJoins(toReturn, qb.Filters);

            }

            //Some TableInfos might be TableValuedFunctions or for some other reason have a paramter associated with them
            qb.ParameterManager.AddParametersFor(toReturn);
                

            return toReturn;
        }

        private static List<TableInfo> AddOpportunisticJoins(List<TableInfo> toReturn, List<IFilter> filters)
        {
            //there must be at least one TableInfo here to do this... but we are going to look up all available JoinInfos from these tables to identify opportunistic joins
            foreach(var table in toReturn.ToArray())
            {
                var cataRepo = ((CatalogueRepository) table.Repository);
                var available = cataRepo.JoinInfoFinder.GetAllJoinInfosWhereTableContains(table, JoinInfoType.AnyKey);
                
                foreach (JoinInfo newAvailableJoin in available)
                {
                    foreach (var availableTable in new TableInfo[]{newAvailableJoin.PrimaryKey.TableInfo,newAvailableJoin.ForeignKey.TableInfo})
                    {
                        //if it's a never before seen table
                        if (!toReturn.Contains(availableTable))
                        {
                            //are there any filters which reference the available TableInfo
                            if (filters.Any(f =>f.WhereSQL != null && f.WhereSQL.ToLower().Contains(availableTable.Name.ToLower() + ".")))
                            {
                                toReturn.Add(availableTable);
                            }
                        }
                    }
                        
                }
            }
            

            return toReturn;
        }

        public static string GetFROMSQL(ISqlQueryBuilder qb,out int[] JoinsUsedInQuery_LineNumbers)
        {
            //add the from bit
            JoinsUsedInQuery_LineNumbers = new int[qb.JoinsUsedInQuery.Count];
            
            string toReturn = "FROM " + qb.TakeNewLine();

            if(qb.TablesUsedInQuery.Count == 0)
                throw new QueryBuildingException("There are no tables involved in the query: We were asked to compute the FROM SQL but qb.TablesUsedInQuery was of length 0");

            if (qb.JoinsUsedInQuery.Count == 0)
            {
                TableInfo firstTable = null;

                //is there only one table involved in the query?
                if (qb.TablesUsedInQuery.Count == 1)
                    firstTable = qb.TablesUsedInQuery[0];
                else if (qb.TablesUsedInQuery.Count(t => t.IsPrimaryExtractionTable) == 1) //has the user picked one to be primary?
                {
                    firstTable = qb.TablesUsedInQuery.Single(t => t.IsPrimaryExtractionTable);
                    
                    //has user tried to make a lookup table the primary table!
                    if(TableIsLookupTable(firstTable,qb))
                        throw new QueryBuildingException("Lookup tables cannot be marked IsPrimaryExtractionTable (Offender ="+firstTable+")");
                }
                else
                {
                    //User has not picked one and there are multiple!

                    //can we discard all tables but one based on the fact that they are look up tables?
                    //maybe! lookup tables are tables where there is an underlying column from that table that is a lookup description
                    var winners = 
                        qb.TablesUsedInQuery.Where(t=> 
                            !TableIsLookupTable(t,qb))
                                .ToArray();
                    
                    //if we have discarded all but 1 it is the only table that does not have any lookup descriptions in it so clearly the correct table to start joins from
                    if (winners.Count() == 1)
                        firstTable = winners[0];
                    else
                        throw new QueryBuildingException("There were " + qb.TablesUsedInQuery.Count + " Tables ("+String.Join(",",qb.TablesUsedInQuery)+") involved in the query, some of them might have been lookup tables but there was no clear table to start joining from, either mark one of the TableInfos IsPrimaryExtractionTable or refine your query columns / create new lookup relationships");
                }
                
                toReturn += firstTable.Name; //simple case "FROM tableX"
            }
            else
                if (qb.PrimaryExtractionTable != null)
                {
                    //user has specified which table to start from 
                    toReturn += qb.PrimaryExtractionTable.Name;

                    List<int> tablesAddedSoFar = new List<int>();

                    //now find any joins which involve the primary extraction table
                    for (int i = 0; i < qb.JoinsUsedInQuery.Count; i++)
                        if (qb.JoinsUsedInQuery[i].PrimaryKey.TableInfo_ID == qb.PrimaryExtractionTable.ID)
                        {
                            //we are joining to a table where the PrimaryExtractionTable is the PK in the relationship so join into the foreign key side
                            JoinsUsedInQuery_LineNumbers[i] = qb.CurrentLine;
                            toReturn += JoinHelper.GetJoinSQLForeignKeySideOnly(qb.JoinsUsedInQuery[i]) + qb.TakeNewLine();
                            tablesAddedSoFar.Add(qb.JoinsUsedInQuery[i].ForeignKey.TableInfo_ID);
                        }
                        else
                            if (qb.JoinsUsedInQuery[i].ForeignKey.TableInfo_ID == qb.PrimaryExtractionTable.ID)
                            {
                                //we are joining to a table where the PrimaryExtractionTable is the FK in the relationship so join into the primary key side
                                JoinsUsedInQuery_LineNumbers[i] = qb.CurrentLine;
                                toReturn += JoinHelper.GetJoinSQLPrimaryKeySideOnly(qb.JoinsUsedInQuery[i]) + qb.TakeNewLine();
                                tablesAddedSoFar.Add(qb.JoinsUsedInQuery[i].PrimaryKey.TableInfo_ID);
                            }

                    //now add any joins which don't involve the primary table
                    for (int i = 0; i < qb.JoinsUsedInQuery.Count; i++)
                        if (qb.JoinsUsedInQuery[i].ForeignKey.TableInfo_ID != qb.PrimaryExtractionTable.ID &&
                            qb.JoinsUsedInQuery[i].PrimaryKey.TableInfo_ID != qb.PrimaryExtractionTable.ID)
                        {

                            //if we have already seen foreign key table before 
                            if (tablesAddedSoFar.Contains(qb.JoinsUsedInQuery[i].ForeignKey.TableInfo_ID))
                                toReturn += JoinHelper.GetJoinSQLPrimaryKeySideOnly(qb.JoinsUsedInQuery[i]) + qb.TakeNewLine();  //add primary
                            else
                                //else if we have already seen primary key table before
                                if (tablesAddedSoFar.Contains(qb.JoinsUsedInQuery[i].PrimaryKey.TableInfo_ID))
                                    toReturn += JoinHelper.GetJoinSQLForeignKeySideOnly(qb.JoinsUsedInQuery[i]) + qb.TakeNewLine();  //add foreign instead
                                else
                                    throw new NotImplementedException("We are having to add a Join for a table that is not 1 level down from the PrimaryExtractionTable");
                        }

                }
                else
                {
                    //user has not specified which table to start from so just output them all in a random order (hopefully FindRequiredJoins bombed out if they tried to do anything too mental)
                    JoinsUsedInQuery_LineNumbers[0] = qb.CurrentLine;
                    toReturn += JoinHelper.GetJoinSQL(qb.JoinsUsedInQuery[0]) + qb.TakeNewLine(); //"FROM ForeignKeyTable JOIN PrimaryKeyTable ON ..."

                    //any subsequent joins
                    for (int i = 1; i < qb.JoinsUsedInQuery.Count; i++)
                    {
                        JoinsUsedInQuery_LineNumbers[i] = qb.CurrentLine;
                        toReturn += JoinHelper.GetJoinSQLForeignKeySideOnly(qb.JoinsUsedInQuery[i]) + qb.TakeNewLine(); //right side only (ForeignKeyTable)
                    }
                }

            //any subsequent lookup joins
            foreach (QueryTimeColumn column in qb.SelectColumns)
            {
                if (
                    (column.IsLookupForeignKey && column.IsLookupForeignKeyActuallyUsed(qb.SelectColumns))
                    ||
                    column.IsIsolatedLookupDescription)
                    toReturn += JoinHelper.GetJoinSQLPrimaryKeySideOnly(column.LookupTable, column.LookupTableAlias) + qb.TakeNewLine();
            }


            return toReturn;
        }

        private static bool TableIsLookupTable(TableInfo tableInfo,ISqlQueryBuilder qb)
        {
            return
                //tables where there is any columns which 
                qb.SelectColumns.Any(
                    //are lookup descriptions and belong to this table
                    c => c.IsLookupDescription && c.UnderlyingColumn.TableInfo_ID == tableInfo.ID);
        }


        /// <summary>
        /// Add a custom line of code into the query at the specified position.  This will be maintained throughout the lifespan of the object such that if
        /// you add other columns etc then your code will still be included at the appropriate position.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="text"></param>
        /// <param name="positionToInsert"></param>
        public static CustomLine AddCustomLine(ISqlQueryBuilder builder, string text, QueryComponent positionToInsert)
        {
            CustomLine toAdd = new CustomLine(text, positionToInsert);

            if(positionToInsert == QueryComponent.GroupBy || positionToInsert == QueryComponent.OrderBy || positionToInsert == QueryComponent.FROM || positionToInsert == QueryComponent.Having)
                throw new QueryBuildingException("Cannot inject custom lines into QueryBuilders at location " + positionToInsert);

            if (positionToInsert == QueryComponent.WHERE)
                if (text.Trim().StartsWith("AND ") || text.Trim().StartsWith("OR "))
                    throw new Exception("Custom filters are always AND, you should not specify the operator AND/OR, you passed\"" + text + "\"");

            builder.CustomLines.Add(toAdd);
            return toAdd;
        }

        public static string GetWHERESQL(ISqlQueryBuilder qb, out int[] Filters_LineNumbers)
        {
            Filters_LineNumbers = new int[qb.Filters.Count];
            string toReturn = "";
            
            var emptyFilters = qb.Filters.Where(f => string.IsNullOrWhiteSpace(f.WhereSQL)).ToArray();

            if (emptyFilters.Any())
                throw new QueryBuildingException("The following empty filters were found in the query:" + Environment.NewLine + string.Join(Environment.NewLine, emptyFilters.Select(f => f.Name)));
            
            //recursively iterate the filter containers joining them up with their operation (AND or OR) and doing tab indentation etc
            if (qb.Filters.Count > 0)
            {
                toReturn += qb.TakeNewLine();
                toReturn += "WHERE" + qb.TakeNewLine();

                toReturn = WriteContainerTreeRecursively(toReturn, 0, qb.RootFilterContainer, qb, ref Filters_LineNumbers);
            }

            return toReturn;
        }

        private static string WriteContainerTreeRecursively(string toReturn, int tabDepth, IContainer currentContainer, ISqlQueryBuilder qb, ref int[] Filters_LineNumbers)
        {
            string tabs = "";
            //see how far we have to tab in
            for (int i = 0; i < tabDepth; i++)
                tabs += "\t";

            //output starting bracket
            toReturn += tabs + "(" + qb.TakeNewLine();

            //see if we have subcontainers
            IContainer[] subcontainers = currentContainer.GetSubContainers();

            if (subcontainers != null)
                for (int i = 0; i < subcontainers.Length; i++)
                {
                    toReturn = WriteContainerTreeRecursively(toReturn, tabDepth + 1, subcontainers[i], qb, ref Filters_LineNumbers);

                    //there are more subcontainers to come
                    if (i + 1 < subcontainers.Length)
                        toReturn += tabs + currentContainer.Operation + qb.TakeNewLine();
                }

            //get all the filters in the current container
            IFilter[] filtersInContainer = currentContainer.GetFilters();
            

            //if there are both filters and containers we need to join the trees with the operator (e.g. AND)
            if (subcontainers != null && subcontainers.Length >= 1 && filtersInContainer != null && filtersInContainer.Length >= 1)
                toReturn += currentContainer.Operation + qb.TakeNewLine();

            //output each filter (and record the line number of it) also make sure it is tabbed in correctly
            for (int i = 0; i < filtersInContainer.Count(); i++)
            {
                if (qb.CheckSyntax)
                    filtersInContainer[i].Check(new ThrowImmediatelyCheckNotifier());

                toReturn += tabs + @"/*" + filtersInContainer[i].Name + @"*/" + qb.TakeNewLine();

                //record the line number that we wrote this out on
                Filters_LineNumbers[qb.Filters.FindIndex(f => f.ID == filtersInContainer[i].ID)] = qb.CurrentLine;

                // the filter may span multiple lines, so collapse it to a single line cleaning up any whitespace issues, e.g. to avoid double spaces in the collapsed version
                var trimmedFilters = (filtersInContainer[i].WhereSQL??"")
                    .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());
                var singleLineWhereSQL = string.Join(" ", trimmedFilters);
                toReturn += tabs + singleLineWhereSQL + qb.TakeNewLine();

                //if there are more filters to come
                if (i + 1 < filtersInContainer.Count())
                    toReturn += tabs + currentContainer.Operation + qb.TakeNewLine();

            }

            toReturn += tabs + ")" + qb.TakeNewLine();

            return toReturn;
        }

        public static IQuerySyntaxHelper GetSyntaxHelper(List<TableInfo> tablesUsedInQuery)
        {
            if (!tablesUsedInQuery.Any())
                throw new QueryBuildingException("Could not pick an IQuerySyntaxHelper because the there were no TableInfos used in the query");
            

            var databaseTypes = tablesUsedInQuery.Select(t => t.DatabaseType).Distinct().ToArray();
            if(databaseTypes.Length > 1)
                throw new QueryBuildingException("Cannot build query because there are multiple DatabaseTypes involved in the query:" + string.Join(",",
                    tablesUsedInQuery.Select(t=>t.GetRuntimeName() + "(" + t.DatabaseType + ")")));

            var helper = new DatabaseHelperFactory(databaseTypes.Single()).CreateInstance();
            return helper.GetQuerySyntaxHelper();
        }

        public static void HandleTopX(ISqlQueryBuilder queryBuilder, IQuerySyntaxHelper syntaxHelper, int topX)
        {
            //if we have a lingering custom line from last time
            ClearTopX(queryBuilder);

            //if we are expected to have a topx
            var response = syntaxHelper.HowDoWeAchieveTopX(topX);
            queryBuilder.TopXCustomLine = AddCustomLine(queryBuilder,response.SQL, response.Location);
            queryBuilder.TopXCustomLine.Role = CustomLineRole.TopX;
        }

        public static void ClearTopX(ISqlQueryBuilder queryBuilder)
        {
            //if we have a lingering custom line from last time
            if (queryBuilder.TopXCustomLine != null)
            {
                queryBuilder.CustomLines.Remove(queryBuilder.TopXCustomLine); //remove it
                queryBuilder.SQLOutOfDate = true;
            }
        }


        public static IEnumerable<CustomLine> GetCustomLinesSQLForStage(ISqlQueryBuilder queryBuilder, QueryComponent stage)
        {
            var lines = queryBuilder.CustomLines.Where(c => c.LocationToInsert == stage).ToArray();
            
            if (!lines.Any())//no lines
                yield break;

           
            //Custom Filters (for people who can't be bothered to implement IFilter or when IContainer doesnt support ramming in additional Filters at runtime because you feel like it ) - these all get AND together and a WHERE is put at the start if needed
            //if there are custom lines being rammed into the Filter section
            if (stage == QueryComponent.WHERE)
            {
                //if we haven't put a WHERE yet, put one in
                if (queryBuilder.Filters.Count == 0)
                    yield return new CustomLine("WHERE" ,QueryComponent.WHERE);
                else
                    yield return new CustomLine("AND" , QueryComponent.WHERE); //otherwise just AND it with every other filter we currently have configured

                //add user custom Filter lines
                for (int i = 0; i < lines.Count(); i++)
                {
                    yield return lines[i];

                    if (i + 1 < lines.Count())
                        yield return new CustomLine("AND" , QueryComponent.WHERE);
                }
                yield break;
            }
            
            //not a custom filter (which requires ANDing - see above) so this is the rest of the cases
            foreach (CustomLine line in lines)
                yield return line;
        }
    }
}
