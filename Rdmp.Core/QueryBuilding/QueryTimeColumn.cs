// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
/// The SELECT portion of QueryBuilder is built up via AddColumn which takes an IColumn.  Each IColumn is a single line of SELECT Sql which might be as
/// simple as the name of a column but might be a method with an alias or even a count e.g. 'sum(distinct mycol) as Total'.  These IColumns are wrapped by
/// QueryTimeColumn which is a wrapper for IColumn which is gradually populated with facts discovered during QueryBuilding such as whether it is from a Lookup
/// Table, whether it maps to an underlying ColumnInfo etc.  These facts are used later on by QueryBuilder to decide which tables/joins are needed in the FROM
/// section of the query etc
/// </summary>
public class QueryTimeColumn : IComparable
{
    /// <summary>
    /// The <see cref="UnderlyingColumn"/> is from a <see cref="Lookup"/> and is a description column but there was no associated
    /// foreign key column found in the query.
    /// </summary>
    public bool IsIsolatedLookupDescription { get; set; }

    /// <summary>
    /// The <see cref="UnderlyingColumn"/> is NOT from a <see cref="Lookup"/> but it is a code column (foreign key) which could be linked to a <see cref="Lookup"/>.
    /// The <see cref="Lookup"/> will be included in the query if one or more description columns follow this column in the query
    /// </summary>
    public bool IsLookupForeignKey { get; private set; }

    /// <summary>
    /// The <see cref="UnderlyingColumn"/> is from a <see cref="Lookup"/> and is a description column and there WAS an associated foreign key column previously found in the query.
    /// </summary>
    public bool IsLookupDescription { get; private set; }

    /// <summary>
    /// The alias given to the <see cref="Lookup"/> table this column belongs to (if any).  This allows you to have the same description column several times in the query e.g.
    /// SendingLocation, Description, ReleaseLocation, Description
    /// </summary>
    public int LookupTableAlias { get; private set; }

    /// <summary>
    /// The <see cref="Lookup"/> that this column is related in the context of the query being generated
    /// </summary>
    public Lookup LookupTable { get; private set; }

    /// <summary>
    /// The SELECT column definition including extraction options such as Order and HashOnDataRelease etc
    /// </summary>
    public IColumn IColumn { get; set; }

    /// <summary>
    /// The actual database model layer column.  The same <see cref="ColumnInfo"/> can appear multiple times in the same query e.g. if extracting DateOfBirth and YearOfBirth where
    /// these are both transforms of the same underlying column.
    /// </summary>
    public ColumnInfo UnderlyingColumn { get; set; }

    /// <summary>
    /// Creates a new <see cref="QueryTimeColumn"/> ready for annotation with facts as they are discovered during query building
    /// </summary>
    /// <param name="column"></param>
    public QueryTimeColumn(IColumn column)
    {
        IColumn = column;
        UnderlyingColumn = column.ColumnInfo;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => IColumn == null ? -1 : IColumn.ID;

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is QueryTimeColumn == false)
            throw new Exception(".Equals only works for objects of type QueryTimeColumn");

        var other = obj as QueryTimeColumn;
        return
            other.IColumn.Equals(IColumn);
    }

    /// <inheritdoc/>
    public int CompareTo(object obj) =>
        obj is QueryTimeColumn
            ? IColumn.Order -
              (obj as QueryTimeColumn).IColumn.Order
            : 0;

    /// <summary>
    /// Computes and records the <see cref="Lookup"/> related facts about all the <see cref="QueryTimeColumn"/> provided when building a query which requires the
    /// supplied list of <paramref name="tablesUsedInQuery"/>.
    /// </summary>
    /// <param name="ColumnsInOrder"></param>
    /// <param name="tablesUsedInQuery"></param>
    public static void SetLookupStatus(QueryTimeColumn[] ColumnsInOrder, List<ITableInfo> tablesUsedInQuery)
    {
        ColumnInfo lastForeignKeyFound = null;
        var lookupTablesFound = 0;

        var firstTable = tablesUsedInQuery.FirstOrDefault();

        var allAvailableLookups = Array.Empty<Lookup>();

        if (firstTable != null)
            allAvailableLookups = firstTable.Repository.GetAllObjects<Lookup>();

        foreach (var column in ColumnsInOrder)
        {
            //it is a custom column
            if (column.UnderlyingColumn == null)
                continue;

            var foreignKeyLookupInvolvement = allAvailableLookups
                .Where(l => l.ForeignKey_ID == column.UnderlyingColumn.ID).ToArray();
            var lookupDescriptionInvolvement = allAvailableLookups
                .Where(l => l.Description_ID == column.UnderlyingColumn.ID).ToArray();

            if (foreignKeyLookupInvolvement.Select(l => l.PrimaryKey.TableInfo_ID).Distinct().Count() > 1)
                throw new Exception(
                    $"Column {column.UnderlyingColumn} is configured as a foreign key for multiple different Lookup tables");

            if (foreignKeyLookupInvolvement.Length > 0)
            {
                if (lookupDescriptionInvolvement.Length > 0)
                    throw new QueryBuildingException(
                        $"Column {column.UnderlyingColumn} is both a Lookup.ForeignKey and a Lookup.Description");


                lastForeignKeyFound = column.UnderlyingColumn;
                column.IsLookupForeignKey = true;
                column.IsLookupDescription = false;
                column.LookupTableAlias = ++lookupTablesFound;
                column.LookupTable = foreignKeyLookupInvolvement[0];
            }

            if (lookupDescriptionInvolvement.Length > 0)
            {
                var lookupDescriptionIsIsolated = false;

                //we have not found any foreign keys yet: that's a problem
                if (lastForeignKeyFound == null)
                {
                    var potentialWinners =
                        lookupDescriptionInvolvement.Where(
                            l => tablesUsedInQuery.Any(t => t.ID == l.ForeignKey.TableInfo_ID)).ToArray();

                    if (potentialWinners.Length ==
                        1) //or there are many options but only one which is in our existing table collection
                    {
                        lastForeignKeyFound =
                            potentialWinners[0]
                                .ForeignKey; //use if there aren't multiple foreign keys to pick from (which would result in uncertainty)
                        lookupDescriptionIsIsolated = true;
                    }
                    else
                        //otherwise there are multiple foreign keys for this description and the user has not put in a foreign key to let us choose the correct one
                    {
                        throw new QueryBuildingException(
                            $"Found lookup description before encountering any lookup foreign keys (Description column was {column.UnderlyingColumn}) - make sure you always order Descriptions after their Foreign key and ensure they are in a contiguous block");
                    }
                }

                var correctLookupDescriptionInvolvement = lookupDescriptionInvolvement
                    .Where(lookup => lookup.ForeignKey.ID == lastForeignKeyFound.ID).ToArray();

                if (correctLookupDescriptionInvolvement.Length == 0)
                {
                    //so there are no compatible foreign keys or the columns are a jumbled mess

                    //either way the last seen fk (or guessed fk) isn't right.  So what fks could potentially be used with the Column?
                    var probableCorrectColumn = lookupDescriptionInvolvement.Where(
                            l =>
                                //any lookup where there is...
                                ColumnsInOrder.Any(
                                    qtc =>
                                        //a column with an ID equal to the fk
                                        qtc.UnderlyingColumn != null && qtc.UnderlyingColumn.ID == l.ForeignKey_ID))
                        .ToArray();


                    var suggestions = "";
                    if (probableCorrectColumn.Any())
                        suggestions =
                            $"Possible foreign keys include:{string.Join(",", probableCorrectColumn.Select(l => l.ForeignKey))}";

                    throw new QueryBuildingException(
                        $"Encountered Lookup Description Column ({column.IColumn}) after first encountering Foreign Key ({lastForeignKeyFound}).  Lookup description columns (_Desc) must come after the associated Foreign key.{suggestions}");
                }

                if (correctLookupDescriptionInvolvement.Length > 1)
                    throw new QueryBuildingException(
                        $"Lookup description {column.UnderlyingColumn} appears to be configured as a Lookup Description twice with the same Lookup Table");

                column.IsIsolatedLookupDescription = lookupDescriptionIsIsolated;
                column.IsLookupForeignKey = false;
                column.IsLookupDescription = true;
                column.LookupTableAlias =
                    lookupTablesFound; // must belong to same one as previously encountered foreign key
                column.LookupTable = correctLookupDescriptionInvolvement[0];

                //see if there are any supplemental joins to tables that are not involved in the query
                var supplementalJoins = correctLookupDescriptionInvolvement[0].GetSupplementalJoins();

                if (supplementalJoins != null)
                    foreach (var supplementalJoin in supplementalJoins)
                        if (tablesUsedInQuery.All(t => t.ID != supplementalJoin.ForeignKey.TableInfo_ID))
                            throw new QueryBuildingException(
                                $"Lookup requires supplemental join to column {supplementalJoin.ForeignKey} which is contained in a table that is not part of the SELECT column collection");
            }
        }
    }


    /// <summary>
    /// Returns the line of SELECT Sql for this column that will appear in the final query
    /// </summary>
    /// <param name="hashingPattern"></param>
    /// <param name="salt"></param>
    /// <param name="syntaxHelper"></param>
    /// <returns></returns>
    public string GetSelectSQL(string hashingPattern, string salt, IQuerySyntaxHelper syntaxHelper)
    {
        var toReturn = IColumn.SelectSQL;

        //deal with hashing
        if (string.IsNullOrWhiteSpace(salt) == false && IColumn.HashOnDataRelease)
        {
            if (string.IsNullOrWhiteSpace(IColumn.Alias))
                throw new ArgumentException(
                    $"IExtractableColumn {IColumn} is missing an Alias (required for hashing)");

            //if there is no custom hashing pattern
            toReturn = string.IsNullOrWhiteSpace(hashingPattern)
                ? syntaxHelper.HowDoWeAchieveMd5(toReturn)
                : //use the DBMS specific one
                string.Format(hashingPattern, toReturn, salt); //otherwise use the custom one
        }

        // the SELECT SQL may span multiple lines, so collapse it to a single line cleaning up any whitespace issues, e.g. to avoid double spaces in the collapsed version
        var trimmedSelectSQL =
            toReturn.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());
        toReturn = string.Join(" ", trimmedSelectSQL);

        //append alias to the end of the line if there is an alias
        if (!string.IsNullOrWhiteSpace(IColumn.Alias))
            toReturn += syntaxHelper.AliasPrefix + IColumn.Alias.Trim();

        //cannot be both, we check for this earlier (see SetLookupStatus)
        Debug.Assert(!(IsLookupDescription && IsLookupForeignKey));

        //replace table name with table alias if it is a LookupDescription
        if (IsLookupDescription)
        {
            var tableName = LookupTable.PrimaryKey.TableInfo.Name;

            if (!toReturn.Contains(tableName))
                throw new Exception(
                    $"Column \"{toReturn}\" is a Lookup Description but its SELECT SQL does not include the Lookup table name \"{tableName}\"");

            toReturn = toReturn.Replace(tableName, JoinHelper.GetLookupTableAlias(LookupTableAlias));
        }

        //actually don't need to do anything special for LookupForeignKeys

        return toReturn;
    }

    /// <summary>
    /// Runs checks on the <see cref="Core.QueryBuilding.IColumn"/> and translates any failures into <see cref="SyntaxErrorException"/>
    /// </summary>
    public void CheckSyntax()
    {
        //make sure to only throw SyntaxErrorException errors in here
        try
        {
            IColumn.Check(ThrowImmediatelyCheckNotifier.Quiet);
            var runtimeName = IColumn.GetRuntimeName();

            if (string.IsNullOrWhiteSpace(runtimeName))
                throw new SyntaxErrorException("no runtime name");
        }
        catch (SyntaxErrorException exception)
        {
            throw new SyntaxErrorException(
                $"Syntax failure on IExtractableColumn with SelectSQL=\"{IColumn.SelectSQL}\"", exception);
        }
    }

    /// <summary>
    /// For a given column that <see cref="IsLookupForeignKey"/> returns true if there is an associated column from the lookup (i.e. a description column). This
    /// should determine whether or not to link to the table in the FROM section of the query.
    /// </summary>
    /// <param name="selectColumns"></param>
    /// <returns></returns>
    public bool IsLookupForeignKeyActuallyUsed(List<QueryTimeColumn> selectColumns)
    {
        if (!IsLookupForeignKey)
            return false;

        //see if the description is used anywhere in the actual query columns!
        return selectColumns.Any(c => c.IsLookupDescription && c.LookupTable.ID == LookupTable.ID);
    }
}