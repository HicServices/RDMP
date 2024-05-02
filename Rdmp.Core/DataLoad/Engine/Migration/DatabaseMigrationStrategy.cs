// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi.Connections;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.Logging;

namespace Rdmp.Core.DataLoad.Engine.Migration;

public delegate void TableMigrationComplete(string tableName, int numInserts, int numUpdates);

/// <summary>
///     See OverwriteMigrationStrategy
/// </summary>
public abstract class DatabaseMigrationStrategy
{
    protected IDataLoadInfo _dataLoadInfo;
    protected IManagedConnection _managedConnection;
    protected const int Timeout = 60000;

    public abstract void MigrateTable(IDataLoadJob toMigrate, MigrationColumnSet columnsToMigrate, int dataLoadInfoID,
        GracefulCancellationToken cancellationToken, ref int inserts, ref int updates);

    protected DatabaseMigrationStrategy(IManagedConnection connection)
    {
        _managedConnection = connection;
    }

    public void Execute(IDataLoadJob job, IEnumerable<MigrationColumnSet> toMigrate, IDataLoadInfo dataLoadInfo,
        GracefulCancellationToken cancellationToken)
    {
        _dataLoadInfo = dataLoadInfo;

        // Column set for each table we are migrating
        foreach (var columnsToMigrate in toMigrate)
        {
            var inserts = 0;
            var updates = 0;
            var tableLoadInfo = dataLoadInfo.CreateTableLoadInfo("",
                columnsToMigrate.DestinationTable.GetFullyQualifiedName(),
                new[] { new DataSource(columnsToMigrate.SourceTable.GetFullyQualifiedName(), DateTime.Now) }, 0);
            try
            {
                MigrateTable(job, columnsToMigrate, dataLoadInfo.ID, cancellationToken, ref inserts, ref updates);
                OnTableMigrationCompleteHandler(columnsToMigrate.DestinationTable.GetFullyQualifiedName(), inserts,
                    updates);
                tableLoadInfo.Inserts = inserts;
                tableLoadInfo.Updates = updates;
                tableLoadInfo.Notes = "Part of Transaction";
            }
            finally
            {
                tableLoadInfo.CloseAndArchive();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    public event TableMigrationComplete TableMigrationCompleteHandler;

    protected virtual void OnTableMigrationCompleteHandler(string tableName, int numInserts, int numUpdates)
    {
        TableMigrationCompleteHandler?.Invoke(tableName, numInserts, numUpdates);
    }
}