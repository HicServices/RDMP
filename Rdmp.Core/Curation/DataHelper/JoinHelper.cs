// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Curation.DataHelper;

/// <summary>
///     Generates ANSI Sql for joining tables together in the FROM line of an SQL query
/// </summary>
public class JoinHelper
{
    /// <summary>
    ///     Assembles ANSI Sql for the JOIN section of a query including any supplemental join columns (e.g. T1 LEFT JOIN T2 on
    ///     T1.A = T2.A AND T1.B = T2.B)
    /// </summary>
    /// <param name="join"></param>
    /// <returns></returns>
    public static string GetJoinSQL(IJoin join)
    {
        TableInfo fkTable = null;
        if (join.ForeignKey != null)
            fkTable = join.ForeignKey.TableInfo;

        TableInfo pkTable = null;
        if (join.PrimaryKey != null)
            pkTable = join.PrimaryKey.TableInfo;

        var foreignTable = fkTable == null ? "" : fkTable.Name;
        var primaryTable = pkTable == null ? "" : pkTable.Name;

        var key1 = join.ForeignKey == null ? "" : join.ForeignKey.Name;
        var key2 = join.PrimaryKey == null ? "" : join.PrimaryKey.Name;

        var joinType = join.ExtractionJoinType.ToString();

        var SQL =
            $"{foreignTable} {joinType} JOIN {primaryTable}{GetOnSql(join, foreignTable, primaryTable, key1, key2, out var hasCustomSql)}";

        SQL = AppendCollation(SQL, join.Collation);

        if (hasCustomSql)
            return SQL;

        SQL = AppendSupplementalJoins(SQL, join);

        return SQL;
    }

    private static string GetOnSql(IJoin join, string key1Table, string key2Table, string key1, string key2,
        out bool hasCustomSql)
    {
        var custom = join.GetCustomJoinSql();

        if (!string.IsNullOrWhiteSpace(custom))
        {
            hasCustomSql = true;
            custom = custom.Replace("{0}", key1Table);
            custom = custom.Replace("{1}", key2Table);

            // remove newlines in users SQL
            custom = Regex.Replace(custom, "\r?\n", " ");

            return $" ON {custom}";
        }

        hasCustomSql = false;

        return $" ON {key1} = {key2}";
    }

    /// <summary>
    ///     Returns the first half of the join with an inverted join type
    ///     <para>
    ///         Explanation:joins are defined as FK table JOIN_TYPE PK table so if you are requesting a join to the FK table
    ///         it is assumed you are coming from the pk table therefore the join type is INVERTED i.e. LEFT becomes RIGHT
    ///     </para>
    /// </summary>
    /// <param name="join"></param>
    /// <returns></returns>
    public static string GetJoinSQLForeignKeySideOnly(IJoin join)
    {
        TableInfo fkTable = null;
        if (join.ForeignKey != null)
            fkTable = join.ForeignKey.TableInfo;

        TableInfo pkTable = null;
        if (join.PrimaryKey != null)
            pkTable = join.PrimaryKey.TableInfo;

        var foreignTable = fkTable == null ? "" : fkTable.Name;
        var primaryTable = pkTable == null ? "" : pkTable.Name;

        var key1 = join.ForeignKey == null ? "" : join.ForeignKey.Name;
        var key2 = join.PrimaryKey == null ? "" : join.PrimaryKey.Name;

        var SQL =
            $" {join.GetInvertedJoinType()} JOIN {foreignTable}{GetOnSql(join, foreignTable, primaryTable, key1, key2, out var hasCustomSql)}";

        SQL = AppendCollation(SQL, join);

        if (hasCustomSql)
            return SQL;

        SQL = AppendSupplementalJoins(SQL, join);


        return SQL;
    }


    /// <summary>
    ///     Gets the JOIN Sql for the JoinInfo as foreign key JOIN primary key on fk.col1 = pk.col2.  Pass in a number
    ///     in order to have the primary key table be assigned an alias e.g. 1 to give it t1
    ///     <para>
    ///         Because join type refers to FK join PK and you are requesting "X" + " JOIN PK table on x" then the join is
    ///         inverted e.g. LEFT => RIGHT and RIGHT => LEFT
    ///         unless it is a lookup join which is always LEFT
    ///     </para>
    /// </summary>
    /// <param name="join"></param>
    /// <param name="aliasNumber"></param>
    /// <returns></returns>
    public static string GetJoinSQLPrimaryKeySideOnly(IJoin join, int aliasNumber = -1)
    {
        TableInfo fkTable = null;
        if (join.ForeignKey != null)
            fkTable = join.ForeignKey.TableInfo;

        TableInfo pkTable = null;
        if (join.PrimaryKey != null)
            pkTable = join.PrimaryKey.TableInfo;

        var foreignTable = fkTable == null ? "" : fkTable.Name;
        var primaryTable = pkTable == null ? "" : pkTable.Name;

        //null check... could be required for display purposes where you have set up half the join when this is called
        var key1 = join.ForeignKey == null ? "" : join.ForeignKey.Name;
        var key2 = join.PrimaryKey == null ? "" : join.PrimaryKey.Name;

        string toReturn;
        bool hasCustomSql;

        //The lookup table is not being assigned an alias
        if (aliasNumber == -1)
        {
            toReturn =
                $" {join.ExtractionJoinType} JOIN {primaryTable}{GetOnSql(join, foreignTable, primaryTable, key1, key2, out hasCustomSql)}";
        }
        else
        {
            var lookupAlias = GetLookupTableAlias(aliasNumber);

            //the lookup table IS being assigned an alias so append As X after table name and change key2 of the join to X.col instead of tablename.col
            toReturn =
                $" {join.ExtractionJoinType} JOIN {primaryTable}{GetLookupTableAlias(aliasNumber, true)}{GetOnSql(join, foreignTable, lookupAlias, key1, key2.Replace(pkTable.Name, lookupAlias), out hasCustomSql)}";
        }

        toReturn = AppendCollation(toReturn, join);

        if (hasCustomSql)
            return toReturn;

        toReturn = AppendSupplementalJoins(toReturn, join, aliasNumber);

        return toReturn;
    }

    /// <summary>
    ///     Gets the suffix for a given lookup table number
    /// </summary>
    /// <param name="aliasNumber">the lookup number e.g. 1 gives lookup_1</param>
    /// <param name="requirePrefix">
    ///     pass in true if you require the prefix " AS " (may vary depending on database context in
    ///     future e.g. perhaps MySql refers to tables by different alias syntax)
    /// </param>
    /// <returns></returns>
    public static string GetLookupTableAlias(int aliasNumber, bool requirePrefix = false)
    {
        return requirePrefix ? $" AS lookup_{aliasNumber}" : $"lookup_{aliasNumber}";
    }


    [Pure]
    private static string AppendSupplementalJoins(string sql, IJoin join, int aliasNumber = -1)
    {
        var supplementalJoins = join.GetSupplementalJoins();

        if (supplementalJoins != null)
            foreach (var supplementalJoin in supplementalJoins)
            {
                var rightHalf = supplementalJoin.PrimaryKey.ToString();

                if (aliasNumber != -1)
                {
                    var lookupTable = join.PrimaryKey.TableInfo;
                    rightHalf = rightHalf.Replace(lookupTable.Name, GetLookupTableAlias(aliasNumber));
                }

                sql += $" AND {supplementalJoin.ForeignKey} = {rightHalf}";
                sql = AppendCollation(sql, supplementalJoin);
            }


        return sql;
    }


    [Pure]
    private static string AppendCollation(string sql, ISupplementalJoin supplementalJoin)
    {
        return AppendCollation(sql, supplementalJoin.Collation);
    }

    [Pure]
    private static string AppendCollation(string sql, IJoin join)
    {
        return AppendCollation(sql, join.Collation);
    }

    [Pure]
    private static string AppendCollation(string sql, string collation)
    {
        return !string.IsNullOrWhiteSpace(collation) ? $"{sql} collate {collation}" : sql;
    }
}