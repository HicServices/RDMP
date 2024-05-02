// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Helps generate SELECT and GROUP By queries for ISqlQueryBuilders.  This includes all the shared functionality such
///     as finding all IFilters, Lookups,
///     which tables to JOIN on etc.  Also handles CustomLine injection which is where you inject arbitrary lines into the
///     query at specific points (See CustomLine).
/// </summary>
public class SqlQueryBuilderHelper
{
    /// <summary>
    ///     Returns all IFilters that are in the root IContainer or any subcontainers
    /// </summary>
    /// <param name="currentContainer"></param>
    /// <returns></returns>
    public static List<IFilter> GetAllFiltersUsedInContainerTreeRecursively(IContainer currentContainer)
    {
        //Note: This returns IsDisabled objects since it is used by cloning systems

        var toAdd = new List<IFilter>();

        //if there is a container
        if (currentContainer != null)
        {
            if (currentContainer.GetSubContainers() != null)
                foreach (var subContainer in currentContainer.GetSubContainers())
                    //recursively add all subcontainers' filters
                    toAdd.AddRange(GetAllFiltersUsedInContainerTreeRecursively(subContainer));

            //add filters
            if (currentContainer.GetFilters() != null)
                toAdd.AddRange(currentContainer.GetFilters());
        }

        return toAdd;
    }

    /// <inheritdoc cref="QueryTimeColumn.SetLookupStatus" />
    public static void FindLookups(ISqlQueryBuilder qb)
    {
        //if there is only one table then user us selecting stuff from the lookup table only
        if (qb.TablesUsedInQuery.Count == 1)
            return;

        QueryTimeColumn.SetLookupStatus(qb.SelectColumns.ToArray(), qb.TablesUsedInQuery);
    }


    /// <summary>
    ///     Must be called only after the ISqlQueryBuilder.TablesUsedInQuery has been set (see GetTablesUsedInQuery).  This
    ///     method will resolve how
    ///     the various tables can be linked together.  Throws QueryBuildingException if it is not possible to join the tables
    ///     with any known
    ///     JoinInfos / Lookup knowledge
    /// </summary>
    /// <param name="qb"></param>
    /// <returns></returns>
    public static List<JoinInfo> FindRequiredJoins(ISqlQueryBuilder qb)
    {
        var Joins = new List<JoinInfo>();

        if (qb.TablesUsedInQuery == null)
            throw new NullReferenceException(
                "You must populate TablesUsedInQuery before calling FindRequiredJoins, try calling GetTablesUsedInQuery");

        //there are no tables so how could there be any joins!
        if (!qb.TablesUsedInQuery.Any())
            throw new QueryBuildingException(
                "Query has no TableInfos! Make sure your query has at least one column with an underlying ColumnInfo / TableInfo set - possibly you have deleted the TableInfo? this would result in orphan CatalogueItem");

        ICatalogueRepository cataRepository;
        try
        {
            cataRepository = (ICatalogueRepository)qb.TablesUsedInQuery.Select(t => t.Repository).Distinct().Single();
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Tables ({string.Join(",", qb.TablesUsedInQuery)}) do not seem to come from the same repository", e);
        }

        foreach (TableInfo table1 in qb.TablesUsedInQuery)
        foreach (TableInfo table2 in qb.TablesUsedInQuery)
            if (table1.ID != table2.ID) //each table must join with a single other table
            {
                //figure out which of the users columns is from table 1 to join using
                var availableJoins = cataRepository.JoinManager.GetAllJoinInfosBetweenColumnInfoSets(
                    table1.ColumnInfos.ToArray(),
                    table2.ColumnInfos.ToArray());

                if (availableJoins.Length == 0)
                    continue; //try another table

                var comboJoinResolved = false;

                //if there are more than 1 join info between the two tables then we need to either do a combo join or complain to user
                if (availableJoins.Length > 1)
                {
                    var additionalErrorMessageWhyWeCantDoComboJoin = "";
                    //if there are multiple joins but they all join between the same 2 tables in the same direction
                    if (availableJoins.Select(j => j.PrimaryKey.TableInfo_ID).Distinct().Count() == 1
                        &&
                        availableJoins.Select(j => j.ForeignKey.TableInfo_ID).Distinct().Count() == 1)
                        if (availableJoins.Select(j => j.ExtractionJoinType).Distinct().Count() == 1)
                        {
                            //add as combo join
                            for (var i = 1; i < availableJoins.Length; i++)
                                availableJoins[0].AddQueryBuildingTimeComboJoinDiscovery(availableJoins[i]);
                            comboJoinResolved = true;
                        }
                        else
                        {
                            additionalErrorMessageWhyWeCantDoComboJoin =
                                " Although joins are all between the same tables in the same direction, the ExtractionJoinTypes are different (e.g. LEFT and RIGHT) which prevents forming a Combo AND based join using both relationships";
                        }
                    else
                        additionalErrorMessageWhyWeCantDoComboJoin =
                            " The Joins do not go in the same direction e.g. Table1.FK=>Table=2.PK and then a reverse relationship Table2.FK=>Table1.PK, in this case the system cannot do a Combo AND based join";

                    var possibleJoinsWere = availableJoins.Select(s => $"JoinInfo[{s}]")
                        .Aggregate((a, b) => a + Environment.NewLine + b);

                    if (!comboJoinResolved)
                        throw new QueryBuildingException(
                            $"Found {availableJoins.Length} possible Joins for {table1.Name} and {table2.Name}, did not know which to use.  Available joins were:{Environment.NewLine}{possibleJoinsWere}{Environment.NewLine} It was not possible to configure a Composite Join because:{Environment.NewLine}{additionalErrorMessageWhyWeCantDoComboJoin}");
                }

                if (!Joins.Contains(availableJoins[0]))
                    Joins.Add(availableJoins[0]);
            }

        if (qb.TablesUsedInQuery.Count - GetDistinctRequiredLookups(qb).Count() - Joins.Count > 1)
            throw new QueryBuildingException(
                $"There were {qb.TablesUsedInQuery.Count} Tables involved in assembling this query ( {qb.TablesUsedInQuery.Aggregate("", (s, n) => $"{s}{n},").TrimEnd(',')}) of which  {GetDistinctRequiredLookups(qb).Count()} were Lookups and {Joins.Count} were JoinInfos, this leaves 2+ tables unjoined (no JoinInfo found)");


        //make sure there are not multiple primary key tables (those should be configured as lookups
        if (Joins.Count > 0 && qb.PrimaryExtractionTable == null)
        {
            var primaryKeyTables = new List<string>(Joins.Select(p => p.PrimaryKey.TableInfo.Name).Distinct());

            if (primaryKeyTables.Count > 1)
            {
                //there are multiple primary key tables... see if we are configured to support them
                var primaryKeyTablesAsString = primaryKeyTables.Aggregate((a, b) => $"{a},{b}");
                throw new QueryBuildingException(
                    $"Found {primaryKeyTables.Count} primary key tables but PrimaryExtractionTable (Fix this by setting one TableInfo as 'IsPrimaryExtractionTable'), primary key tables identified include: {primaryKeyTablesAsString}");
            }
        }

        return qb.PrimaryExtractionTable != null && qb.TablesUsedInQuery.Contains(qb.PrimaryExtractionTable) == false
            ? throw new QueryBuildingException(
                "Specified PrimaryExtractionTable was not found amongst the chosen extraction columns")
            : Joins;
    }

    /// <summary>
    ///     Returns all <see cref="Lookup" /> linked to for the FROM section of the query
    /// </summary>
    /// <param name="qb"></param>
    /// <returns></returns>
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
    ///     Make sure you have set your Filters and SelectColumns properties before calling this method so that it can find
    ///     table dependencies
    /// </summary>
    /// <param name="qb"></param>
    /// <param name="primaryExtractionTable"></param>
    /// <param name="forceJoinsToTheseTables"></param>
    /// <returns></returns>
    public static List<ITableInfo> GetTablesUsedInQuery(ISqlQueryBuilder qb, out ITableInfo primaryExtractionTable,
        ITableInfo[] forceJoinsToTheseTables = null)
    {
        if (qb.SelectColumns == null)
            throw new QueryBuildingException("ISqlQueryBuilder.SelectedColumns is null");

        if (qb.SelectColumns.Count == 0)
            throw new QueryBuildingException("There are no columns in the SELECT query");

        var toReturn = new List<ITableInfo>(forceJoinsToTheseTables ?? Array.Empty<ITableInfo>());

        if (forceJoinsToTheseTables != null)
        {
            if (forceJoinsToTheseTables.Count(t => t.IsPrimaryExtractionTable) > 1)
                primaryExtractionTable = PickBestPrimaryExtractionTable(qb,
                                             forceJoinsToTheseTables.Where(t => t.IsPrimaryExtractionTable).ToArray())
                                         ?? throw new QueryBuildingException(
                                             "Found 2+ tables marked IsPrimaryExtractionTable in force joined tables");
            else
                primaryExtractionTable = forceJoinsToTheseTables.SingleOrDefault(t => t.IsPrimaryExtractionTable);
        }
        else
        {
            primaryExtractionTable = null;
        }


        //get all the tables based on selected columns
        foreach (var toExtract in qb.SelectColumns)
        {
            if (toExtract.UnderlyingColumn == null)
                continue;

            if (qb.CheckSyntax)
                toExtract.CheckSyntax();

            var table = toExtract.UnderlyingColumn.TableInfo;

            if (!toReturn.Contains(table))
            {
                toReturn.Add(table);

                if (table.IsPrimaryExtractionTable)
                    if (primaryExtractionTable == null)
                        primaryExtractionTable = table;
                    else
                        primaryExtractionTable = PickBestPrimaryExtractionTable(qb, primaryExtractionTable, table)
                                                 ?? throw new QueryBuildingException(
                                                     $"There are multiple tables marked as IsPrimaryExtractionTable:{primaryExtractionTable.Name}(ID={primaryExtractionTable.ID}) and {table.Name}(ID={table.ID})");
            }
        }

        //get other tables we might need because they are referenced by filters
        if (qb.Filters != null && qb.Filters.Any())
        {
            foreach (var filter in qb.Filters)
            {
                var col = filter.GetColumnInfoIfExists();
                if (col != null)
                {
                    var tableInfoOfFilter = col.TableInfo;
                    if (!toReturn.Contains(tableInfoOfFilter))
                        toReturn.Add(tableInfoOfFilter);
                }
            }

            toReturn = AddOpportunisticJoins(toReturn, qb.Filters);
        }

        //Some TableInfos might be TableValuedFunctions or for some other reason have a paramter associated with them
        qb.ParameterManager.AddParametersFor(toReturn);


        return toReturn;
    }

    /// <summary>
    ///     Picks between two <see cref="ITableInfo" /> both of which are <see cref="TableInfo.IsPrimaryExtractionTable" /> and
    ///     returns
    ///     the 'winner' (best to start joining from).  returns null if there is no clear better one
    /// </summary>
    /// <param name="qb"></param>
    /// <param name="tables"></param>
    /// <returns></returns>
    /// <exception cref="QueryBuildingException"></exception>
    private static ITableInfo PickBestPrimaryExtractionTable(ISqlQueryBuilder qb, params ITableInfo[] tables)
    {
        if (tables.Length == 0)
            throw new ArgumentException(
                $"At least one table must be provided to {nameof(PickBestPrimaryExtractionTable)}");

        // if there is only one choice
        if (tables.Length == 1)
            return tables[0]; // go with that

        // what tables have IsExtractionIdentifier column(s)?
        var extractionIdentifierTables = qb.SelectColumns
            .Where(c => c.IColumn?.IsExtractionIdentifier ?? false)
            .Select(t => t.UnderlyingColumn?.TableInfo_ID)
            .Where(id => id != null)
            .ToArray();

        if (extractionIdentifierTables.Length == 1)
        {
            var id = extractionIdentifierTables[0];

            foreach (var t in tables)
                if (id == t.ID)
                    return t;

            // IsExtractionIdentifier column is from neither of these tables, bad times
        }

        // no clear winner
        return null;
    }

    private static List<ITableInfo> AddOpportunisticJoins(List<ITableInfo> toReturn, List<IFilter> filters)
    {
        //there must be at least one TableInfo here to do this... but we are going to look up all available JoinInfos from these tables to identify opportunistic joins
        foreach (var table in toReturn.ToArray())
        {
            var available =
                table.CatalogueRepository.JoinManager.GetAllJoinInfosWhereTableContains(table, JoinInfoType.AnyKey);

            foreach (var newAvailableJoin in available)
            foreach (var availableTable in new[]
                         { newAvailableJoin.PrimaryKey.TableInfo, newAvailableJoin.ForeignKey.TableInfo })
                //if it's a never before seen table
                if (!toReturn.Contains(availableTable))
                    //are there any filters which reference the available TableInfo
                    if (filters.Any(f => f.WhereSQL != null && f.WhereSQL.ToLower().Contains(
                            $"{availableTable.Name.ToLower()}.")))
                        toReturn.Add(availableTable);
        }


        return toReturn;
    }

    /// <summary>
    ///     Generates the FROM sql including joins for all the <see cref="TableInfo" /> required by the
    ///     <see cref="ISqlQueryBuilder" />.  <see cref="JoinInfo" /> must exist for
    ///     this process to work
    /// </summary>
    /// <param name="qb"></param>
    /// <returns></returns>
    public static string GetFROMSQL(ISqlQueryBuilder qb)
    {
        //add the from bit
        var toReturn = $"FROM {Environment.NewLine}";

        if (qb.TablesUsedInQuery.Count == 0)
            throw new QueryBuildingException(
                "There are no tables involved in the query: We were asked to compute the FROM SQL but qb.TablesUsedInQuery was of length 0");

        //IDs of tables we already have in our FROM section
        var tablesAddedSoFar = new HashSet<int>();

        //sometimes we find joins between tables that turn out not to be needed e.g. if there are multiple
        //routes through the system e.g. Test_FourTables_MultipleRoutes
        var unneededJoins = new HashSet<JoinInfo>();

        if (qb.JoinsUsedInQuery.Count == 0)
        {
            ITableInfo firstTable = null;

            //is there only one table involved in the query?
            if (qb.TablesUsedInQuery.Count == 1)
            {
                firstTable = qb.TablesUsedInQuery[0];
            }
            else if (qb.TablesUsedInQuery.Count(t => t.IsPrimaryExtractionTable) ==
                     1) //has the user picked one to be primary?
            {
                firstTable = qb.TablesUsedInQuery.Single(t => t.IsPrimaryExtractionTable);

                //has user tried to make a lookup table the primary table!
                if (TableIsLookupTable(firstTable, qb))
                    throw new QueryBuildingException(
                        $"Lookup tables cannot be marked IsPrimaryExtractionTable (Offender ={firstTable})");
            }
            else
            {
                //User has not picked one and there are multiple!

                //can we discard all tables but one based on the fact that they are look up tables?
                //maybe! lookup tables are tables where there is an underlying column from that table that is a lookup description
                var winners =
                    qb.TablesUsedInQuery.Where(t =>
                            !TableIsLookupTable(t, qb))
                        .ToArray();

                //if we have discarded all but 1 it is the only table that does not have any lookup descriptions in it so clearly the correct table to start joins from
                if (winners.Length == 1)
                    firstTable = winners[0];
                else
                    throw new QueryBuildingException(
                        $"There were {qb.TablesUsedInQuery.Count} Tables ({string.Join(",", qb.TablesUsedInQuery)}) involved in the query, some of them might have been lookup tables but there was no clear table to start joining from, either mark one of the TableInfos IsPrimaryExtractionTable or refine your query columns / create new lookup relationships");
            }

            toReturn += firstTable.Name; //simple case "FROM tableX"
        }
        else if (qb.PrimaryExtractionTable != null)
        {
            //user has specified which table to start from
            toReturn += qb.PrimaryExtractionTable.Name;

            //now find any joins which involve the primary extraction table
            for (var i = 0; i < qb.JoinsUsedInQuery.Count; i++)
                if (qb.JoinsUsedInQuery[i].PrimaryKey.TableInfo_ID == qb.PrimaryExtractionTable.ID)
                {
                    var fkTableId = qb.JoinsUsedInQuery[i].ForeignKey.TableInfo_ID;

                    //don't double JOIN to the same table twice even using different routes (see Test_FourTables_MultipleRoutes)
                    if (!tablesAddedSoFar.Contains(fkTableId))
                    {
                        //we are joining to a table where the PrimaryExtractionTable is the PK in the relationship so join into the foreign key side
                        toReturn += JoinHelper.GetJoinSQLForeignKeySideOnly(qb.JoinsUsedInQuery[i]) +
                                    Environment.NewLine;
                        tablesAddedSoFar.Add(fkTableId);
                    }
                }
                else if (qb.JoinsUsedInQuery[i].ForeignKey.TableInfo_ID == qb.PrimaryExtractionTable.ID)
                {
                    var pkTableId = qb.JoinsUsedInQuery[i].PrimaryKey.TableInfo_ID;

                    //don't double JOIN to the same table twice even using different routes (see Test_FourTables_MultipleRoutes)
                    if (!tablesAddedSoFar.Contains(pkTableId))
                    {
                        //we are joining to a table where the PrimaryExtractionTable is the FK in the relationship so join into the primary key side
                        toReturn += JoinHelper.GetJoinSQLPrimaryKeySideOnly(qb.JoinsUsedInQuery[i]) +
                                    Environment.NewLine;
                        tablesAddedSoFar.Add(pkTableId);
                    }
                }

            //now add any joins which don't involve the primary table
            for (var i = 0; i < qb.JoinsUsedInQuery.Count; i++)
                if (qb.JoinsUsedInQuery[i].ForeignKey.TableInfo_ID != qb.PrimaryExtractionTable.ID &&
                    qb.JoinsUsedInQuery[i].PrimaryKey.TableInfo_ID != qb.PrimaryExtractionTable.ID)
                {
                    var pkTableID = qb.JoinsUsedInQuery[i].PrimaryKey.TableInfo_ID;
                    var fkTableID = qb.JoinsUsedInQuery[i].ForeignKey.TableInfo_ID;


                    //if we have already seen foreign key table before
                    //if we already have
                    if (tablesAddedSoFar.Contains(fkTableID) && tablesAddedSoFar.Contains(pkTableID))
                    {
                        unneededJoins.Add(qb.JoinsUsedInQuery[i]);
                    }
                    else if (tablesAddedSoFar.Contains(fkTableID))
                    {
                        toReturn += JoinHelper.GetJoinSQLPrimaryKeySideOnly(qb.JoinsUsedInQuery[i]) +
                                    Environment.NewLine; //add primary
                        tablesAddedSoFar.Add(pkTableID);
                    }
                    else
                        //else if we have already seen primary key table before
                    if (tablesAddedSoFar.Contains(pkTableID))
                    {
                        toReturn += JoinHelper.GetJoinSQLForeignKeySideOnly(qb.JoinsUsedInQuery[i]) +
                                    Environment.NewLine; //add foreign instead
                        tablesAddedSoFar.Add(fkTableID);
                    }

                    else
                    {
                        throw new NotImplementedException(
                            "We are having to add a Join for a table that is not 1 level down from the PrimaryExtractionTable");
                    }
                }
        }
        else
        {
            //user has not specified which table to start from so just output them all in a random order (hopefully FindRequiredJoins bombed out if they tried to do anything too mental)
            toReturn += JoinHelper.GetJoinSQL(qb.JoinsUsedInQuery[0]) +
                        Environment.NewLine; //"FROM ForeignKeyTable JOIN PrimaryKeyTable ON ..."

            //any subsequent joins
            for (var i = 1; i < qb.JoinsUsedInQuery.Count; i++)
                toReturn += JoinHelper.GetJoinSQLForeignKeySideOnly(qb.JoinsUsedInQuery[i]) +
                            Environment.NewLine; //right side only (ForeignKeyTable)
        }

        //any subsequent lookup joins
        foreach (var column in qb.SelectColumns)
            if (
                (column.IsLookupForeignKey && column.IsLookupForeignKeyActuallyUsed(qb.SelectColumns))
                ||
                column.IsIsolatedLookupDescription)
                toReturn += JoinHelper.GetJoinSQLPrimaryKeySideOnly(column.LookupTable, column.LookupTableAlias) +
                            Environment.NewLine;

        //remove any joins that didn't turn out to be needed when joining the tables
        foreach (var j in unneededJoins)
            qb.JoinsUsedInQuery.Remove(j);

        return toReturn;
    }

    private static bool TableIsLookupTable(ITableInfo tableInfo, ISqlQueryBuilder qb)
    {
        return
            //tables where there is any columns which
            qb.SelectColumns.Any(
                //are lookup descriptions and belong to this table
                c => c.IsLookupDescription && c.UnderlyingColumn.TableInfo_ID == tableInfo.ID);
    }


    /// <summary>
    ///     Add a custom line of code into the query at the specified position.  This will be maintained throughout the
    ///     lifespan of the object such that if
    ///     you add other columns etc then your code will still be included at the appropriate position.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="text"></param>
    /// <param name="positionToInsert"></param>
    public static CustomLine AddCustomLine(ISqlQueryBuilder builder, string text, QueryComponent positionToInsert)
    {
        var toAdd = new CustomLine(text, positionToInsert);

        if (positionToInsert == QueryComponent.GroupBy || positionToInsert == QueryComponent.OrderBy ||
            positionToInsert == QueryComponent.FROM || positionToInsert == QueryComponent.Having)
            throw new QueryBuildingException(
                $"Cannot inject custom lines into QueryBuilders at location {positionToInsert}");

        if (positionToInsert == QueryComponent.WHERE)
            if (text.Trim().StartsWith("AND ") || text.Trim().StartsWith("OR "))
                throw new Exception(
                    $"Custom filters are always AND, you should not specify the operator AND/OR, you passed\"{text}\"");

        builder.CustomLines.Add(toAdd);
        return toAdd;
    }

    /// <summary>
    ///     Generates the WHERE section of the query for the <see cref="ISqlQueryBuilder" /> based on recursively processing
    ///     the <see cref="ISqlQueryBuilder.RootFilterContainer" />
    /// </summary>
    /// <param name="qb"></param>
    /// <returns>WHERE block or empty string if there are no <see cref="IContainer" /></returns>
    public static string GetWHERESQL(ISqlQueryBuilder qb)
    {
        var toReturn = "";

        //if the root filter container is disabled don't render it
        if (!IsEnabled(qb.RootFilterContainer))
            return "";

        var emptyFilters = qb.Filters.Where(f => string.IsNullOrWhiteSpace(f.WhereSQL)).ToArray();

        if (emptyFilters.Any())
            throw new QueryBuildingException(
                $"The following empty filters were found in the query:{Environment.NewLine}{string.Join(Environment.NewLine, emptyFilters.Select(f => f.Name))}");

        //recursively iterate the filter containers joining them up with their operation (AND or OR) and doing tab indentation etc
        if (qb.Filters.Any())
        {
            var filtersSql = WriteContainerTreeRecursively(toReturn, 0, qb.RootFilterContainer, qb);

            if (!string.IsNullOrWhiteSpace(filtersSql))
            {
                toReturn += Environment.NewLine;
                toReturn += $"WHERE{Environment.NewLine}";
                toReturn += filtersSql;
            }
        }

        return toReturn;
    }

    private static string WriteContainerTreeRecursively(string toReturn, int tabDepth, IContainer currentContainer,
        ISqlQueryBuilder qb)
    {
        var tabs = "";
        //see how far we have to tab in
        for (var i = 0; i < tabDepth; i++)
            tabs += "\t";

        //get all the filters in the current container
        var filtersInContainer = currentContainer.GetFilters().Where(IsEnabled).ToArray();

        //see if we have subcontainers
        var subcontainers = currentContainer.GetSubContainers().Where(IsEnabled).ToArray();

        //if there are no filters or subcontainers return nothing
        if (!filtersInContainer.Any() && !subcontainers.Any())
            return "";

        //output starting bracket
        toReturn += $"{tabs}({Environment.NewLine}";

        //write out subcontainers
        for (var i = 0; i < subcontainers.Length; i++)
        {
            toReturn = WriteContainerTreeRecursively(toReturn, tabDepth + 1, subcontainers[i], qb);

            //there are more subcontainers to come
            if (i + 1 < subcontainers.Length)
                toReturn += tabs + currentContainer.Operation + Environment.NewLine;
        }

        //if there are both filters and containers we need to join the trees with the operator (e.g. AND)
        if (subcontainers.Length >= 1 && filtersInContainer.Length >= 1)
            toReturn += currentContainer.Operation + Environment.NewLine;

        //output each filter also make sure it is tabbed in correctly
        for (var i = 0; i < filtersInContainer.Length; i++)
        {
            if (qb.CheckSyntax)
                filtersInContainer[i].Check(ThrowImmediatelyCheckNotifier.Quiet);

            toReturn += $@"{tabs}/*{filtersInContainer[i].Name}*/{Environment.NewLine}";

            // the filter may span multiple lines, so collapse it to a single line cleaning up any whitespace issues, e.g. to avoid double spaces in the collapsed version
            var trimmedFilters = (filtersInContainer[i].WhereSQL ?? "")
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());
            var singleLineWhereSQL = string.Join(" ", trimmedFilters);
            toReturn += tabs + singleLineWhereSQL + Environment.NewLine;

            //if there are more filters to come
            if (i + 1 < filtersInContainer.Length)
                toReturn += tabs + currentContainer.Operation + Environment.NewLine;
        }

        toReturn += $"{tabs}){Environment.NewLine}";

        return toReturn;
    }

    /// <summary>
    ///     Containers are enabled if they do not support disabling (<see cref="IDisableable" />) or are
    ///     <see cref="IDisableable.IsDisabled" /> = false
    /// </summary>
    /// <param name="container"></param>
    /// <returns></returns>
    private static bool IsEnabled(IContainer container)
    {
        //skip disabled containers
        return container is not IDisableable { IsDisabled: true };
    }

    /// <summary>
    ///     Filters are enabled if they do not support disabling (<see cref="IDisableable" />) or are
    ///     <see cref="IDisableable.IsDisabled" /> = false
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    private static bool IsEnabled(IFilter filter)
    {
        //skip disabled filters
        return filter is not IDisableable { IsDisabled: true };
    }

    /// <summary>
    ///     Returns the unique database server type <see cref="IQuerySyntaxHelper" /> by evaluating the
    ///     <see cref="TableInfo" /> used in the query.
    ///     <para>
    ///         Throws <see cref="QueryBuildingException" /> if the tables are from mixed server types (e.g. MySql mixed with
    ///         Oracle)
    ///     </para>
    /// </summary>
    /// <param name="tablesUsedInQuery"></param>
    /// <returns></returns>
    public static IQuerySyntaxHelper GetSyntaxHelper(List<ITableInfo> tablesUsedInQuery)
    {
        if (!tablesUsedInQuery.Any())
            throw new QueryBuildingException(
                "Could not pick an IQuerySyntaxHelper because the there were no TableInfos used in the query");


        var databaseTypes = tablesUsedInQuery.Select(t => t.DatabaseType).Distinct().ToArray();
        return databaseTypes.Length > 1
            ? throw new QueryBuildingException(
                $"Cannot build query because there are multiple DatabaseTypes involved in the query:{string.Join(",", tablesUsedInQuery.Select(t => $"{t.GetRuntimeName()}({t.DatabaseType})"))}")
            : DatabaseCommandHelper.For(databaseTypes.Single()).GetQuerySyntaxHelper();
    }

    /// <summary>
    ///     Applies <paramref name="topX" /> to the <see cref="ISqlQueryBuilder" /> as a <see cref="CustomLine" /> based on the
    ///     database engine syntax e.g. LIMIT vs TOP
    ///     and puts in in the correct location in the query (<see cref="QueryComponent" />)
    /// </summary>
    /// <param name="queryBuilder"></param>
    /// <param name="syntaxHelper"></param>
    /// <param name="topX"></param>
    public static void HandleTopX(ISqlQueryBuilder queryBuilder, IQuerySyntaxHelper syntaxHelper, int topX)
    {
        //if we have a lingering custom line from last time
        ClearTopX(queryBuilder);

        //if we are expected to have a topx
        var response = syntaxHelper.HowDoWeAchieveTopX(topX);
        queryBuilder.TopXCustomLine = AddCustomLine(queryBuilder, response.SQL, response.Location);
        queryBuilder.TopXCustomLine.Role = CustomLineRole.TopX;
    }

    /// <summary>
    ///     Removes the SELECT TOP X logic from the supplied <see cref="ISqlQueryBuilder" />
    /// </summary>
    /// <param name="queryBuilder"></param>
    public static void ClearTopX(ISqlQueryBuilder queryBuilder)
    {
        //if we have a lingering custom line from last time
        if (queryBuilder.TopXCustomLine != null)
        {
            queryBuilder.CustomLines.Remove(queryBuilder.TopXCustomLine); //remove it
            queryBuilder.SQLOutOfDate = true;
        }
    }

    /// <summary>
    ///     Returns all <see cref="CustomLine" /> declared in <see cref="ISqlQueryBuilder.CustomLines" /> for the given stage
    ///     but also adds some new ones to ensure valid syntax (for example
    ///     adding the word WHERE/AND depending on whether there is an existing
    ///     <see cref="ISqlQueryBuilder.RootFilterContainer" />.
    /// </summary>
    /// <param name="queryBuilder"></param>
    /// <param name="stage"></param>
    /// <returns></returns>
    public static IEnumerable<CustomLine> GetCustomLinesSQLForStage(ISqlQueryBuilder queryBuilder, QueryComponent stage)
    {
        var lines = queryBuilder.CustomLines.Where(c => c.LocationToInsert == stage).ToArray();

        if (!lines.Any()) //no lines
            yield break;


        //Custom Filters (for people who can't be bothered to implement IFilter or when IContainer doesnt support ramming in additional Filters at runtime because you feel like it ) - these all get AND together and a WHERE is put at the start if needed
        //if there are custom lines being rammed into the Filter section
        if (stage == QueryComponent.WHERE)
        {
            //if we haven't put a WHERE yet, put one in
            if (queryBuilder.Filters.Count == 0)
                yield return new CustomLine("WHERE", QueryComponent.WHERE);
            else
                yield return
                    new CustomLine("AND",
                        QueryComponent
                            .WHERE); //otherwise just AND it with every other filter we currently have configured

            //add user custom Filter lines
            for (var i = 0; i < lines.Length; i++)
            {
                yield return lines[i];

                if (i + 1 < lines.Length)
                    yield return new CustomLine("AND", QueryComponent.WHERE);
            }

            yield break;
        }

        //not a custom filter (which requires ANDing - see above) so this is the rest of the cases
        foreach (var line in lines)
            yield return line;
    }
}