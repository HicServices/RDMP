// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.DataLoad.Triggers.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using TypeGuesser;

namespace Rdmp.Core.DataLoad.Triggers.Implementations;

/// <summary>
/// Trigger implementer for that creates archive triggers on tables.  This is a prerequisite for the RDMP DLE and ensures that
/// when updates in a load replace live records the old state is persisted.
/// </summary>
public abstract partial class TriggerImplementer : ITriggerImplementer
{
    protected readonly bool _createDataLoadRunIdAlso;

    protected readonly DiscoveredServer _server;
    protected readonly DiscoveredTable _table;
    protected readonly DiscoveredTable _archiveTable;
    protected DiscoveredColumn[] _columns;
    protected readonly DiscoveredColumn[] _primaryKeys;

    /// <summary>
    /// Trigger implementer for that creates a trigger on <paramref name="table"/> which records old UPDATE
    /// records into an _Archive table.  <paramref name="table"/> must have primary keys
    /// </summary>
    /// <param name="table"></param>
    /// <param name="createDataLoadRunIDAlso"></param>
    protected TriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso = true)
    {
        _server = table.Database.Server;
        _table = table;
        _archiveTable = _table.Database.ExpectTable($"{table.GetRuntimeName()}_Archive", table.Schema);
        _columns = table.DiscoverColumns();
        _primaryKeys = _columns.Where(c => c.IsPrimaryKey).ToArray();

        _createDataLoadRunIdAlso = createDataLoadRunIDAlso;
    }

    public abstract void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger);

    public virtual string CreateTrigger(ICheckNotifier notifier)
    {
        if (!_primaryKeys.Any())
            throw new TriggerException("There must be at least 1 primary key");

        //if _Archive exists skip creating it
        var skipCreatingArchive = _archiveTable.Exists();

        //check _Archive does not already exist
        foreach (var forbiddenColumnName in new[] { "hic_validTo", "hic_userID", "hic_status" })
            if (_columns.Any(c =>
                    c.GetRuntimeName().Equals(forbiddenColumnName, StringComparison.CurrentCultureIgnoreCase)))
                throw new TriggerException(
                    $"Table {_table} already contains a column called {forbiddenColumnName} this column is reserved for Archiving");

        var b_mustCreate_validFrom = !_columns.Any(c =>
            c.GetRuntimeName().Equals(SpecialFieldNames.ValidFrom, StringComparison.CurrentCultureIgnoreCase));
        var b_mustCreate_dataloadRunId =
            !_columns.Any(c =>
                c.GetRuntimeName()
                    .Equals(SpecialFieldNames.DataLoadRunID, StringComparison.CurrentCultureIgnoreCase)) &&
            _createDataLoadRunIdAlso;

        //forces column order dataloadrunID then valid from (doesnt prevent these being in the wrong place in the record but hey ho - possibly not an issue anyway since probably the 3 values in the archive are what matters for order - see the Trigger which populates *,X,Y,Z where * is all columns in mane table
        if (b_mustCreate_dataloadRunId && !b_mustCreate_validFrom)
            throw new TriggerException(
                $"Cannot create trigger because table contains {SpecialFieldNames.ValidFrom} but not {SpecialFieldNames.DataLoadRunID} (ID must be placed before valid from in column order)");

        //must add validFrom outside of transaction if we want SMO to pick it up
        if (b_mustCreate_dataloadRunId)
            _table.AddColumn(SpecialFieldNames.DataLoadRunID, new DatabaseTypeRequest(typeof(int)), true,
                UserSettings.ArchiveTriggerTimeout);

        var syntaxHelper = _server.GetQuerySyntaxHelper();


        //must add validFrom outside of transaction if we want SMO to pick it up
        if (b_mustCreate_validFrom)
            AddValidFrom(_table, syntaxHelper);

        //if we created columns we need to update _column
        if (b_mustCreate_dataloadRunId || b_mustCreate_validFrom)
            _columns = _table.DiscoverColumns();

        var sql = WorkOutArchiveTableCreationSQL();

        if (!skipCreatingArchive)
        {
            using var con = _server.GetConnection();
            con.Open();

            using (var cmdCreateArchive = _server.GetCommand(sql, con))
            {
                cmdCreateArchive.CommandTimeout = UserSettings.ArchiveTriggerTimeout;
                cmdCreateArchive.ExecuteNonQuery();
            }


            _archiveTable.AddColumn("hic_validTo", new DatabaseTypeRequest(typeof(DateTime)), true,
                UserSettings.ArchiveTriggerTimeout);
            _archiveTable.AddColumn("hic_userID", new DatabaseTypeRequest(typeof(string), 128), true,
                UserSettings.ArchiveTriggerTimeout);
            _archiveTable.AddColumn("hic_status", new DatabaseTypeRequest(typeof(string), 1), true,
                UserSettings.ArchiveTriggerTimeout);
        }

        return sql;
    }

    protected virtual void AddValidFrom(DiscoveredTable table, IQuerySyntaxHelper syntaxHelper)
    {
        var dateTimeDatatype =
            syntaxHelper.TypeTranslater.GetSQLDBTypeForCSharpType(new DatabaseTypeRequest(typeof(DateTime)));
        var nowFunction = syntaxHelper.GetScalarFunctionSql(MandatoryScalarFunctions.GetTodaysDate);

        _table.AddColumn(SpecialFieldNames.ValidFrom, $" {dateTimeDatatype} DEFAULT {nowFunction}", true,
            UserSettings.ArchiveTriggerTimeout);
    }


    private string WorkOutArchiveTableCreationSQL()
    {
        //script original table
        var createTableSQL = _table.ScriptTableCreation(true, true, true);

        var toReplaceTableName = $"CREATE TABLE {_table.GetFullyQualifiedName()}";

        if (!createTableSQL.Contains(toReplaceTableName))
            throw new Exception($"Expected to find occurrence of {toReplaceTableName} in the SQL {createTableSQL}");

        //rename table
        createTableSQL = createTableSQL.Replace(toReplaceTableName,
            $"CREATE TABLE {_archiveTable.GetFullyQualifiedName()}");

        //drop identity bit
        createTableSQL = RemoveIdentities().Replace(createTableSQL, "");

        return createTableSQL;
    }

    public abstract TriggerStatus GetTriggerStatus();

    /// <summary>
    /// Returns true if the trigger exists and the method body of the trigger matches the expected method body.  This exists to handle
    /// the situation where a trigger is created on a table then the schema of the live table or the archive table is altered subsequently.
    /// 
    /// <para>The best way to implement this is to regenerate the trigger and compare it to the current code fetched from the ddl</para>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
    {
        //check server has trigger and it is on
        var isEnabledSimple = GetTriggerStatus();

        if (isEnabledSimple == TriggerStatus.Disabled || isEnabledSimple == TriggerStatus.Missing)
            return false;

        CheckColumnDefinitionsMatchArchive();

        return true;
    }

    private void CheckColumnDefinitionsMatchArchive()
    {
        var errors = new List<string>();

        var archiveTableCols = _archiveTable.DiscoverColumns().ToArray();

        foreach (var col in _columns)
        {
            var colInArchive = archiveTableCols.SingleOrDefault(c => c.GetRuntimeName().Equals(col.GetRuntimeName()));

            if (colInArchive == null)
                errors.Add(
                    $"Column {col.GetRuntimeName()} appears in Table '{_table}' but not in archive table '{_archiveTable}'");
            else if (!AreCompatibleDatatypes(col.DataType, colInArchive.DataType))
                errors.Add(
                    $"Column {col.GetRuntimeName()} has data type '{col.DataType}' in '{_table}' but in Archive table '{_archiveTable}' it is defined as '{colInArchive.DataType}'");
        }

        if (errors.Any())
            throw new IrreconcilableColumnDifferencesInArchiveException(
                $"The following column mismatch errors were seen:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }

    private static bool AreCompatibleDatatypes(DiscoveredDataType mainDataType, DiscoveredDataType archiveDataType)
    {
        var t1 = mainDataType.SQLType;
        var t2 = archiveDataType.SQLType;

        if (t1.Equals(t2, StringComparison.CurrentCultureIgnoreCase))
            return true;

        return t1.Contains("identity", StringComparison.OrdinalIgnoreCase) &&
               t1.Replace("identity", "", StringComparison.OrdinalIgnoreCase).Trim()
                   .Equals(t2.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"IDENTITY\(\d+,\d+\)")]
    private static partial Regex RemoveIdentities();
}