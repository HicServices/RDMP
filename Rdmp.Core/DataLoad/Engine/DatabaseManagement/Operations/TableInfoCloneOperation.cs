// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.DataLoad.Extensions;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;

/// <summary>
/// Creates RAW / STAGING tables during a data load (See LoadMetadata).  Tables created are based on the live schema.  Depending on stage though certain
/// changes will be made.  For example RAW tables will not have any constraints (primary keys, not null etc) and will also contain all PreLoadDiscardedColumns.
/// 
/// <para>This class is powered by SMO and is Microsoft Sql Server specific.</para>
/// </summary>
public class TableInfoCloneOperation
{
    private readonly HICDatabaseConfiguration _hicDatabaseConfiguration;
    private readonly TableInfo _tableInfo;
    private readonly LoadBubble _copyToBubble;
    private readonly IDataLoadEventListener _listener;

    public bool DropHICColumns { get; set; }
    public bool DropIdentityColumns { get; set; }
    public bool AllowNulls { get; set; }


    private bool _operationSucceeded;

    public TableInfoCloneOperation(HICDatabaseConfiguration hicDatabaseConfiguration, TableInfo tableInfo,
        LoadBubble copyToBubble, IDataLoadEventListener listener)
    {
        _hicDatabaseConfiguration = hicDatabaseConfiguration;
        _tableInfo = tableInfo;
        _copyToBubble = copyToBubble;
        _listener = listener;
        DropIdentityColumns = true;
    }


    public void Execute()
    {
        if (_operationSucceeded)
            throw new Exception("Operation already executed once");

        var liveDb = DataAccessPortal.ExpectDatabase(_tableInfo, DataAccessContext.DataLoad);
        var destTableName = _tableInfo.GetRuntimeName(_copyToBubble, _hicDatabaseConfiguration.DatabaseNamer);


        var discardedColumns = _tableInfo.PreLoadDiscardedColumns
            .Where(c => c.Destination == DiscardedColumnDestination.Dilute).ToArray();

        CloneTable(liveDb, _hicDatabaseConfiguration.DeployInfo[_copyToBubble],
            _tableInfo.Discover(DataAccessContext.DataLoad), destTableName, DropHICColumns, DropIdentityColumns,
            AllowNulls, discardedColumns);

        _operationSucceeded = true;
    }


    public void Undo()
    {
        if (!_operationSucceeded)
            throw new Exception("Cannot undo operation because it has not yet been executed");

        var tableToRemove = _tableInfo.GetRuntimeName(_copyToBubble, _hicDatabaseConfiguration.DatabaseNamer);
        if (_hicDatabaseConfiguration.DeployInfo[_copyToBubble].Exists())
            RemoveTableFromDatabase(tableToRemove, _hicDatabaseConfiguration.DeployInfo[_copyToBubble]);
    }


    public static void RemoveTableFromDatabase(string tableName, DiscoveredDatabase dbInfo)
    {
        if (!IsNukable(dbInfo, tableName))
            throw new Exception(
                "This method nukes a table in a database! for obvious reasons this is only allowed on databases with a suffix _STAGING/_RAW");

        dbInfo.ExpectTable(tableName).Drop();
    }


    private static bool IsNukable(DiscoveredDatabase dbInfo, string tableName) =>
        tableName.EndsWith("_STAGING", StringComparison.CurrentCultureIgnoreCase) ||
        tableName.EndsWith("_RAW", StringComparison.CurrentCultureIgnoreCase)
        ||
        dbInfo.GetRuntimeName().EndsWith("_STAGING", StringComparison.CurrentCultureIgnoreCase) ||
        dbInfo.GetRuntimeName().EndsWith("_RAW", StringComparison.CurrentCultureIgnoreCase);

    public void CloneTable(DiscoveredDatabase srcDatabaseInfo, DiscoveredDatabase destDatabaseInfo,
        DiscoveredTable sourceTable, string destTableName, bool dropHICColumns, bool dropIdentityColumns,
        bool allowNulls, PreLoadDiscardedColumn[] dilutionColumns)
    {
        if (!sourceTable.Exists())
            throw new Exception($"Table {sourceTable} does not exist on {srcDatabaseInfo}");


        //new table will start with the same name as the as the old scripted one
        var newTable = destDatabaseInfo.ExpectTable(destTableName);

        var sql = sourceTable.ScriptTableCreation(allowNulls, allowNulls,
            false /*False because we want to drop these columns entirely not just flip to int*/, newTable);

        _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Creating table with SQL:{sql}"));

        using (var con = destDatabaseInfo.Server.GetConnection())
        {
            con.Open();
            using var cmd = destDatabaseInfo.Server.GetCommand(sql, con);
            cmd.ExecuteNonQuery();
        }

        if (!newTable.Exists())
            throw new Exception(
                $"Table '{newTable}' not found in {destDatabaseInfo} despite running table creation SQL!");

        foreach (var column in newTable.DiscoverColumns())
        {
            var drop = false;
            var colName = column.GetRuntimeName();

            if (column.IsAutoIncrement)
                drop = true;

            //drop hic_ columns
            if (SpecialFieldNames.IsHicPrefixed(colName) && dropHICColumns)
                drop = true;

            //if the ColumnInfo is explicitly marked to be ignored
            if (_tableInfo.ColumnInfos.Any(c =>
                    c.IgnoreInLoads && c.GetRuntimeName(_copyToBubble.ToLoadStage()).Equals(colName)))
            {
                _listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        $"{colName} will be dropped because it is marked IgnoreInLoads"));
                drop = true;
            }


            //also drop any columns we have specifically been told to ignore in the DLE configuration
            if (_hicDatabaseConfiguration.IgnoreColumns != null &&
                _hicDatabaseConfiguration.IgnoreColumns.IsMatch(colName))
            {
                _listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        $"{colName} will be dropped because it is matches the global ignores pattern ({_hicDatabaseConfiguration.IgnoreColumns})"));
                drop = true;
            }

            ////drop the data load run ID field and validFrom fields, we don't need them in STAGING or RAW, it will be hard coded in the MERGE migration with a fixed value anyway.
            //if (colName.Equals(SpecialFieldNames.DataLoadRunID) || colName.Equals(SpecialFieldNames.ValidFrom))
            //    drop = true;

            var dilution = dilutionColumns.SingleOrDefault(c => c.GetRuntimeName().Equals(colName));

            if (dilution != null)
            {
                _listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        $"Altering diluted column {colName} to {dilution.Data_type}"));
                column.DataType.AlterTypeTo(dilution.Data_type);
            }

            if (drop)
                newTable.DropColumn(column);
        }
    }
}