// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;

/// <summary>
/// Clones databases and tables using ColumnInfos, and records operations so the cloning can be undone.
/// </summary>
public class DatabaseCloner : IDisposeAfterDataLoad
{
    private readonly HICDatabaseConfiguration _hicDatabaseConfiguration;
    private readonly List<TableInfoCloneOperation> _tablesCreated;
    private readonly List<DiscoveredDatabase> _databasesCreated;

    public DatabaseCloner(HICDatabaseConfiguration hicDatabaseConfiguration)
    {
        _hicDatabaseConfiguration = hicDatabaseConfiguration;
        _tablesCreated = new List<TableInfoCloneOperation>();
        _databasesCreated = new List<DiscoveredDatabase>();
    }

    public DiscoveredDatabase CreateDatabaseForStage(LoadBubble stageToCreateDatabase)
    {
        if (stageToCreateDatabase == LoadBubble.Live)
            throw new Exception("Please don't try to create databases on the live server");

        var dbInfo = _hicDatabaseConfiguration.DeployInfo[stageToCreateDatabase];

        dbInfo.Server.CreateDatabase(dbInfo.GetRuntimeName());

        _databasesCreated.Add(dbInfo);

        return dbInfo;
    }

    public void CreateTablesInDatabaseFromCatalogueInfo(IDataLoadEventListener listener, TableInfo tableInfo,
        LoadBubble copyToStage, bool allowReservedPrefixColumns)
    {
        if (copyToStage == LoadBubble.Live)
            throw new Exception("Please don't try to create tables in the live database");

        var destDbInfo = _hicDatabaseConfiguration.DeployInfo[copyToStage];

        var cloneOperation = new TableInfoCloneOperation(_hicDatabaseConfiguration, tableInfo, copyToStage, listener)
        {
            DropHICColumns = !allowReservedPrefixColumns,
            AllowNulls = copyToStage == LoadBubble.Raw
        };

        cloneOperation.Execute();
        _tablesCreated.Add(cloneOperation);


        if (copyToStage == LoadBubble.Raw)
        {
            var tableName = tableInfo.GetRuntimeName(copyToStage, _hicDatabaseConfiguration.DatabaseNamer);

            var table = destDbInfo.ExpectTable(tableName);

            var existingColumns = tableInfo.ColumnInfos.Select(c => c.GetRuntimeName(LoadStage.AdjustRaw)).ToArray();

            foreach (var preLoadDiscardedColumn in tableInfo.PreLoadDiscardedColumns)
            {
                //this column does not get dropped so will be in live TableInfo
                if (preLoadDiscardedColumn.Destination == DiscardedColumnDestination.Dilute)
                    continue;

                if (existingColumns.Any(e => e.Equals(preLoadDiscardedColumn.GetRuntimeName(LoadStage.AdjustRaw))))
                    throw new Exception(
                        $"There is a column called {preLoadDiscardedColumn.GetRuntimeName(LoadStage.AdjustRaw)} as both a PreLoadDiscardedColumn and in the TableInfo (live table), you should either drop the column from the live table or remove it as a PreLoadDiscarded column");

                //add all the preload discarded columns because they could be routed to ANO store or sent to oblivion
                AddColumnToTable(table, preLoadDiscardedColumn.RuntimeColumnName, preLoadDiscardedColumn.SqlDataType,
                    listener);
            }

            //deal with anonymisation transforms e.g. ANOCHI of datatype varchar(12) would have to become a column called CHI of datatype varchar(10) on creation in RAW
            var columnInfosWithANOTransforms = tableInfo.ColumnInfos.Where(c => c.ANOTable_ID != null).ToArray();
            if (columnInfosWithANOTransforms.Any())
                foreach (var col in columnInfosWithANOTransforms)
                {
                    var liveName = col.GetRuntimeName(LoadStage.PostLoad);
                    var rawName = col.GetRuntimeName(LoadStage.AdjustRaw);

                    var rawDataType = col.GetRuntimeDataType(LoadStage.AdjustRaw);

                    DropColumnFromTable(table, liveName, listener);
                    AddColumnToTable(table, rawName, rawDataType, listener);
                }
        }
    }

    private void AddColumnToTable(DiscoveredTable table, string desiredColumnName, string desiredColumnType,
        IDataLoadEventListener listener)
    {
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Adding column '{desiredColumnName}' with datatype '{desiredColumnType}' to table '{table.GetFullyQualifiedName()}'"));
        table.AddColumn(desiredColumnName, desiredColumnType, true, 500);
    }

    private void DropColumnFromTable(DiscoveredTable table, string columnName, IDataLoadEventListener listener)
    {
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Dropping column '{columnName}' from table '{table.GetFullyQualifiedName()}'"));
        var col = table.DiscoverColumn(columnName);
        table.DropColumn(col);
    }


    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        //don't bother cleaning up if it bombed
        if (exitCode == ExitCodeType.Error)
            return;

        //it's Abort,Success or LoadNotRequired
        foreach (var cloneOperation in _tablesCreated) cloneOperation.Undo();

        foreach (var dbInfo in _databasesCreated)
            dbInfo.Drop();
    }
}