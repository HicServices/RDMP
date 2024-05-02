// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Naming;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Sharing.Refactoring.Exceptions;

namespace Rdmp.Core.Sharing.Refactoring;

/// <summary>
///     Handles making changes to SelectSQL properties that revolve around changing which underlying table/column drives
///     the SQL.  For example when a user
///     renames a TableInfo and wants to refactor the changes into all the ColumnInfos that underly it and all the
///     ExtractionInformations that come from
///     those ColumnInfos and then all the CohortIdentificationConfigurations, AggregateConfigurations etc etc.
/// </summary>
public class SelectSQLRefactorer
{
    /// <summary>
    ///     Replaces all references to the given table with the new table name in a columns SelectSQL.  This will also save the
    ///     column.  Ensure
    ///     that new tableName is in fact fully qualified e.g. '[db]..[tbl]'
    /// </summary>
    /// <param name="column"></param>
    /// <param name="tableName"></param>
    /// <param name="newFullySpecifiedTableName"></param>
    public static void RefactorTableName(IColumn column, IHasFullyQualifiedNameToo tableName,
        string newFullySpecifiedTableName)
    {
        if (column.ColumnInfo == null)
            throw new RefactoringException($"Cannot refactor '{column}' because its ColumnInfo was null");

        var fullyQualifiedName = tableName.GetFullyQualifiedName();
        if (!column.SelectSQL.Contains(fullyQualifiedName))
            throw new RefactoringException(
                $"IColumn '{column}' did not contain the fully specified table name during refactoring ('{fullyQualifiedName}'");

        if (!newFullySpecifiedTableName.Contains('.'))
            throw new RefactoringException(
                $"Replacement table name was not fully specified, value passed was '{newFullySpecifiedTableName}' which did not contain any dots");

        column.SelectSQL = column.SelectSQL.Replace(fullyQualifiedName, newFullySpecifiedTableName);
        Save(column);
    }

    /// <summary>
    ///     Replaces all references to the given table with the new table name in a ColumnInfo.  This will also save the
    ///     column.    Ensure
    ///     that new tableName is in fact fully qualified e.g. '[db]..[tbl]'
    /// </summary>
    /// <param name="column"></param>
    /// <param name="tableName"></param>
    /// <param name="newFullySpecifiedTableName"></param>
    public static void RefactorTableName(ColumnInfo column, IHasFullyQualifiedNameToo tableName,
        string newFullySpecifiedTableName)
    {
        var fullyQualifiedName = tableName.GetFullyQualifiedName();

        if (!column.Name.StartsWith(fullyQualifiedName))
            throw new RefactoringException(
                $"ColumnInfo '{column}' did not start with the fully specified table name during refactoring ('{fullyQualifiedName}'");

        if (!newFullySpecifiedTableName.Contains('.'))
            throw new RefactoringException(
                $"Replacement table name was not fully specified, value passed was '{newFullySpecifiedTableName}' which did not contain any dots");

        column.Name = column.Name.Replace(fullyQualifiedName, newFullySpecifiedTableName);
        column.SaveToDatabase();
    }

    protected static void Save(object o)
    {
        if (o is ISaveable s)
            s.SaveToDatabase();
    }

    /// <summary>
    ///     Replaces all references to the given table with the new table name in a columns SelectSQL.  This will also save the
    ///     column.  Ensure
    ///     that newFullySpecifiedColumnName is in fact fully qualified too e.g. [mydb]..[mytable].[mycol]
    /// </summary>
    /// <param name="column"></param>
    /// <param name="columnName"></param>
    /// <param name="newFullySpecifiedColumnName"></param>
    /// <param name="strict">
    ///     Determines behaviour when column SelectSQL does not contain a reference to columnName.  True will
    ///     throw a RefactoringException, false will return without making any changes
    /// </param>
    public static void RefactorColumnName(IColumn column, IHasFullyQualifiedNameToo columnName,
        string newFullySpecifiedColumnName, bool strict = true)
    {
        var fullyQualifiedName = columnName.GetFullyQualifiedName();

        if (!column.SelectSQL.Contains(fullyQualifiedName))
            if (strict)
                throw new RefactoringException(
                    $"IColumn '{column}' did not contain the fully specified column name during refactoring ('{fullyQualifiedName}'");
            else
                return;

        if (newFullySpecifiedColumnName.Count(c => c == '.') < 2)
            throw new RefactoringException(
                $"Replacement column name was not fully specified, value passed was '{newFullySpecifiedColumnName}' which should have had at least 2 dots");

        column.SelectSQL = column.SelectSQL.Replace(fullyQualifiedName, newFullySpecifiedColumnName);

        Save(column);
    }

    /// <summary>
    ///     Determines whether the SelectSQL of the specified IColumn includes fully specified refactorable references.  If the
    ///     SelectSQL is properly
    ///     formed then the underlying column should appear fully specified at least once in the SelectSQL e.g.
    ///     UPPER([db]..[tbl].[col])
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public static bool IsRefactorable(IColumn column)
    {
        return GetReasonNotRefactorable(column) == null;
    }

    /// <summary>
    ///     Determines whether the SelectSQL of the specified IColumn includes fully specified refactorable references.
    ///     Returns the reason why
    ///     the IColumn is not IsRefactorable (or null if it is).
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public static string GetReasonNotRefactorable(IColumn column)
    {
        var ci = column.ColumnInfo;

        if (ci == null)
            return $"Cannot refactor '{column}' because its ColumnInfo was null";

        if (!column.SelectSQL.Contains(ci.Name))
            return
                $"IColumn '{column}' did not contain the fully specified column name of its underlying ColumnInfo ('{ci.Name}') during refactoring";

        var fullyQualifiedName = ci.TableInfo.GetFullyQualifiedName();

        return !column.SelectSQL.Contains(fullyQualifiedName)
            ? $"IColumn '{column}' did not contain the fully specified table name ('{fullyQualifiedName}') during refactoring"
            : null;
    }

    /// <summary>
    ///     Returns true if the <paramref name="tableInfo" /> supports refactoring (e.g. renaming). See
    ///     <see cref="GetReasonNotRefactorable(ITableInfo)" />
    ///     for the reason why
    /// </summary>
    /// <param name="tableInfo"></param>
    /// <returns></returns>
    public static bool IsRefactorable(ITableInfo tableInfo)
    {
        return GetReasonNotRefactorable(tableInfo) == null;
    }

    /// <summary>
    ///     Returns the reason why <paramref name="table" /> is not refactorable e.g. if its name is not properly qualified
    ///     with database.  Returns null if it is refactorable
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public static string GetReasonNotRefactorable(ITableInfo table)
    {
        if (string.IsNullOrWhiteSpace(table.Name))
            return "Table has no Name property, this should be the fully qualified database table name";

        if (string.IsNullOrWhiteSpace(table.Database))
            return "Table does not have its Database property set";

        //ensure database and Name match correctly
        var syntaxHelper = table.GetQuerySyntaxHelper();
        var db = table.GetDatabaseRuntimeName(LoadStage.PostLoad);

        if (!table.Name.StartsWith(syntaxHelper.EnsureWrapped(db)))
            return $"Table with Name '{table.Name}' has incorrect database property '{table.Database}'";

        return table.Name != table.GetFullyQualifiedName()
            ? $"Table name '{table.Name}' did not match the expected fully qualified name '{table.GetFullyQualifiedName()}'"
            : null;
    }

    /// <summary>
    ///     Changes the name of the <paramref name="tableInfo" /> to a new name (which must be fully qualified). This will also
    ///     update any <see cref="ColumnInfo" /> and <see cref="ExtractionInformation" /> objects declared against the
    ///     <paramref name="tableInfo" />.
    ///     <para>
    ///         If you have transforms etc in <see cref="ExtractionInformation" /> then these may not be successfully
    ///         refactored
    ///     </para>
    /// </summary>
    /// <param name="tableInfo"></param>
    /// <param name="newFullyQualifiedTableName"></param>
    /// <returns>Total number of changes made in columns and table name</returns>
    public static int RefactorTableName(ITableInfo tableInfo, string newFullyQualifiedTableName)
    {
        return RefactorTableName(tableInfo, tableInfo.GetFullyQualifiedName(), newFullyQualifiedTableName);
    }

    /// <inheritdoc cref="RefactorTableName(ITableInfo, string)" />
    public static int RefactorTableName(ITableInfo tableInfo, string oldFullyQualifiedTableName,
        string newFullyQualifiedTableName)
    {
        if (!IsRefactorable(tableInfo))
            throw new RefactoringException(
                $"TableInfo {tableInfo} is not refactorable because {GetReasonNotRefactorable(tableInfo)}");

        var updatesMade = 0;

        //if it's a new name
        if (tableInfo.Name != newFullyQualifiedTableName)
        {
            tableInfo.Name = newFullyQualifiedTableName;
            Save(tableInfo);
            updatesMade++;
        }

        //Rename all ColumnInfos that belong to this TableInfo
        foreach (var columnInfo in tableInfo.ColumnInfos)
            updatesMade += RefactorTableName(columnInfo, oldFullyQualifiedTableName, newFullyQualifiedTableName);

        return updatesMade;
    }

    /// <summary>
    ///     Replaces the <paramref name="oldFullyQualifiedTableName" /> with the <paramref name="newFullyQualifiedTableName" />
    ///     in the
    ///     given <paramref name="columnInfo" /> and any <see cref="ExtractionInformation" /> declared against it.
    /// </summary>
    /// <param name="columnInfo"></param>
    /// <param name="oldFullyQualifiedTableName"></param>
    /// <param name="newFullyQualifiedTableName"></param>
    /// <returns>
    ///     Total number of changes made including <paramref name="columnInfo" /> and any
    ///     <see cref="ExtractionInformation" /> declared on it
    /// </returns>
    public static int RefactorTableName(ColumnInfo columnInfo, string oldFullyQualifiedTableName,
        string newFullyQualifiedTableName)
    {
        var updatesMade = 0;

        //run what they asked for
        updatesMade += RefactorTableNameImpl(columnInfo, oldFullyQualifiedTableName, newFullyQualifiedTableName);

        //these are all the things that could appear spattered throughout the old columns
        var oldPrefixes = new List<string> { "..", ".dbo.", ".[dbo]." };

        //this is what they said they wanted in the refactoring
        var newPrefix = oldPrefixes.FirstOrDefault(newFullyQualifiedTableName.Contains);

        //if they are trying to standardise
        if (newPrefix != null)
            foreach (var old in oldPrefixes)
                if (!string.Equals(old, newPrefix))
                    updatesMade += RefactorTableNameImpl(columnInfo, oldFullyQualifiedTableName.Replace(newPrefix, old),
                        newFullyQualifiedTableName);

        return updatesMade;
    }

    private static int RefactorTableNameImpl(ColumnInfo columnInfo, string oldFullyQualifiedTableName,
        string newFullyQualifiedTableName)
    {
        var updatesMade = 0;

        var extractionInformations = columnInfo.ExtractionInformations.ToArray();

        foreach (var extractionInformation in extractionInformations)
            if (extractionInformation.SelectSQL.Contains(oldFullyQualifiedTableName))
            {
                var newvalue =
                    extractionInformation.SelectSQL.Replace(oldFullyQualifiedTableName, newFullyQualifiedTableName);

                if (!extractionInformation.SelectSQL.Equals(newvalue))
                {
                    extractionInformation.SelectSQL = newvalue;
                    extractionInformation.SaveToDatabase();
                    updatesMade++;
                }
            }

        //rename ColumnInfos
        if (columnInfo.Name.StartsWith(oldFullyQualifiedTableName))
        {
            columnInfo.Name = Regex.Replace(columnInfo.Name, $"^{Regex.Escape(oldFullyQualifiedTableName)}",
                newFullyQualifiedTableName);
            columnInfo.SaveToDatabase();
            updatesMade++;
        }

        return updatesMade;
    }
}