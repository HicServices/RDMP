// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;

internal class IdentifierDumperSynchronizer
{
    private readonly IdentifierDumper _parent;
    private readonly ExternalDatabaseServer _dump;

    public IdentifierDumperSynchronizer(IdentifierDumper dumper, ExternalDatabaseServer dump)
    {
        _parent = dumper;
        _dump = dump;
    }


    public void Synchronize(ICheckNotifier notifier)
    {
        //no need to check dump because it probably doesn't even have one
        if (!_parent.HasAtLeastOneColumnToStoreInDump)
            return;


        //dump database is required so check connection to it
        var server = DataAccessPortal.ExpectServer(_dump, DataAccessContext.DataLoad);
        var tables = server.GetCurrentDatabase().DiscoverTables(false);

        using var con = server.GetConnection();
        try
        {
            con.Open();
        }
        catch (SqlException e)
        {
            throw new ANOConfigurationException("Anonymisation database is not accessible", e);
        }

        var identifiersTable = _parent.GetRuntimeName(); //gets the dump table runtime name

        //find primary keys
        var primaryKeyColumnInfos = _parent.TableInfo.ColumnInfos.Where(col => col.IsPrimaryKey).ToArray();

        //make sure it has a primary key
        if (!primaryKeyColumnInfos.Any())
            throw new ANOConfigurationException(
                $"No primary keys are defined on TableInfo called {_parent.TableInfo.GetRuntimeName()} (ID={_parent.TableInfo.ID})");

        //make sure there is an _Identifiers table for the dataset
        if (!tables.Any(t => t.GetRuntimeName().Equals(identifiersTable)))
            if (notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Table {identifiersTable} was not found in IdentifierDump {_dump}", CheckResult.Fail, null,
                    $"Create new identifier dump called {identifiersTable} using the current primary key ColumnInfos ({string.Join(",", primaryKeyColumnInfos.Select(c => c.GetRuntimeName()))}) and currently configured dump columns")))
                _parent.CreateIdentifierDumpTable(primaryKeyColumnInfos);
            else
                throw new ANOConfigurationException(
                    $"User rejected change to fix Identifier dump {identifiersTable} not being found on server");


        var columnsInTheIdentifiersDumpTable =
            server.ExpectDatabase(_dump.Database).ExpectTable(identifiersTable).DiscoverColumns();

        #region Pk Mismatches between dump and live

        //Are all origin primary keys in the dump and also primary keys in the dump?
        foreach (var originPk in primaryKeyColumnInfos)
        {
            var expectedColName = originPk.GetRuntimeName(LoadStage.AdjustRaw);

            var match =
                columnsInTheIdentifiersDumpTable.SingleOrDefault(c => c.GetRuntimeName().Equals(expectedColName)) ??
                throw new ANOConfigurationException(
                    $"Column {originPk} is a primary key column but is not in Identifier dump table {identifiersTable}");
            if (!match.IsPrimaryKey)
                throw new ANOConfigurationException(
                    $"Column {originPk} is a primary key column but in Identifier dump {identifiersTable} it is not part of the primary key");
        }

        foreach (var dumpPk in columnsInTheIdentifiersDumpTable.Where(c => c.IsPrimaryKey))
            if (!primaryKeyColumnInfos.Any(p => p.GetRuntimeName(LoadStage.AdjustRaw).Equals(dumpPk.GetRuntimeName())))
                throw new ANOConfigurationException(
                    $"Column {dumpPk.GetFullyQualifiedName()} in Identifier dump {identifiersTable} is part of the primary key but the LIVE table primary key columns does not include it");

        #endregion

        #region missing columns

        //fetch all the columns in the _Identifiers table - so we can make sure the correct columns exist for all PreLoadDiscardedColumns
        var missingColumns = new List<string>();

        //extra columns
        foreach (var columnNameInDump in columnsInTheIdentifiersDumpTable.Select(c => c.GetRuntimeName()))
        {
            if (primaryKeyColumnInfos.Any(pk =>
                    pk.GetRuntimeName(LoadStage.AdjustRaw).Equals(columnNameInDump))) //its a primary key so expected
                continue;

            if (_parent.ColumnsToRouteToSomewhereElse.Any(d =>
                    d.GetRuntimeName().Equals(columnNameInDump))) //it's something we were expecting to dump
                continue;

            //these are also expected don't warn user about them
            if (columnNameInDump is SpecialFieldNames.ValidFrom or SpecialFieldNames.DataLoadRunID)
                continue;

            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Column {columnNameInDump} was found in the IdentifierDump table {identifiersTable} but was not one of the primary keys or a PreLoadDiscardedColumn",
                    CheckResult.Warning));
        }

        //for each column that we are supposed to dump, make sure it is actually in the dump table
        foreach (var column in _parent.ColumnsToRouteToSomewhereElse.Where(static c => c.GoesIntoIdentifierDump()))
        {
            var colInIdentifierDumpDatabase =
                columnsInTheIdentifiersDumpTable.SingleOrDefault(c =>
                    c.GetRuntimeName().Equals(column.RuntimeColumnName));

            //if there are not any columns in the dump that have the same runtime name as the current preloaddiscarded column
            if (colInIdentifierDumpDatabase == null)
            {
                //get the user to agree to add it to the table?
                if (notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Missing column {column.RuntimeColumnName} in table {identifiersTable}",
                        CheckResult.Fail, null, "Add missing column")))
                    AddColumnToDump(column, con); //add it because user agreed
                else
                    missingColumns.Add(column.RuntimeColumnName); //user disagreed, record it as a failure
            }
            else
            {
                //column is there, make sure the type is correct
                if (string.IsNullOrWhiteSpace(column.Data_type))
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"PreLoadDiscardedColumn {column} does not have a data type recorded in the Catalogue!",
                            CheckResult.Fail));
                else if (!column.Data_type.Equals(colInIdentifierDumpDatabase.DataType.SQLType))
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"PreLoadDiscardedColumn{column} has data type {column.Data_type} in the Catalogue but appears as {colInIdentifierDumpDatabase.DataType.SQLType} in the actual IdentifierDump",
                            CheckResult.Fail));
            }
        }

        //for each primary key, make sure it is also in the dump
        foreach (var primaryKeyName in primaryKeyColumnInfos.Select(pk => pk.GetRuntimeName(LoadStage.AdjustRaw)))
            if (
                //if there are not any columns in the dump with the same name as the current primary key
                !columnsInTheIdentifiersDumpTable.Any(
                    c => c.GetRuntimeName().Equals(primaryKeyName)))
                missingColumns.Add(primaryKeyName);

        if (missingColumns.Any())
            throw new ANOConfigurationException(
                $"Fields missing from table {identifiersTable} :{missingColumns.Aggregate(Environment.NewLine, (s, v) => $"{s},{v}")}");

        #endregion

        #region mismatch of Type

        //fetch all the columns in the _Identifiers table - so we can make sure the correct columns exist for all PreLoadDiscardedColumns
        var allColumnsInLiveDatabase = _parent.TableInfo.ColumnInfos.ToArray();

        var typeMismatchesMessages = new List<string>();


        foreach (var columnInIdentifierDump in columnsInTheIdentifiersDumpTable)
        {
            //try to find a ColumnInfo in the catalogue that has the same name as the identifier dump column we found when interrogating the database
            var columnThatShouldHaveTheSameType
                = allColumnsInLiveDatabase.FirstOrDefault(
                    col => col.GetRuntimeName().Equals(columnInIdentifierDump.GetRuntimeName()));

            //we straight up found a column in the dump that doesn't exist in the metadata, that's fine (presumably the user nuked the column at some point and left the archival dumped stuff still in the dump)
            if (columnThatShouldHaveTheSameType == null)
                continue;

            var rawType = columnThatShouldHaveTheSameType.GetRuntimeDataType(LoadStage.AdjustRaw);
            if (!rawType.Equals(columnInIdentifierDump.DataType.SQLType))
                typeMismatchesMessages.Add(
                    $"Column {columnInIdentifierDump} in Identifier Dump {identifiersTable} mismatch {columnInIdentifierDump.DataType.SQLType} vs {rawType} (in catalogue - using GetRuntimeDataType(LoadStage.AdjustRaw))");
        }

        if (typeMismatchesMessages.Any())
            throw new ANOConfigurationException(
                $"Fields have unexpected types in table {identifiersTable} :{typeMismatchesMessages.Aggregate(Environment.NewLine, (s, v) => s + Environment.NewLine + v)}");

        #endregion
    }


    private void AddColumnToDump(PreLoadDiscardedColumn column, DbConnection con)
    {
        using var cmdAlter = DatabaseCommandHelper.GetCommand(
            $"Alter table {_parent.GetRuntimeName()} ADD {column.RuntimeColumnName} {column.SqlDataType}", con);
        cmdAlter.ExecuteNonQuery();
    }
}