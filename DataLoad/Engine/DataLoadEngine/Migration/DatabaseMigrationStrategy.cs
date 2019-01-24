using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.Migration.QueryBuilding;
using FAnsi.Connections;
using HIC.Logging;

namespace DataLoadEngine.Migration
{
    public delegate void TableMigrationComplete(string tableName, int numInserts, int numUpdates);

    /// <summary>
    /// See OverwriteMigrationStrategy
    /// </summary>
    abstract public class DatabaseMigrationStrategy
    {
        protected IDataLoadInfo _dataLoadInfo;
        protected IManagedConnection _managedConnection;
        protected const int Timeout = 60000;

        abstract public void MigrateTable(IDataLoadJob toMigrate, MigrationColumnSet columnsToMigrate, int dataLoadInfoID, GracefulCancellationToken cancellationToken, ref int inserts, ref int updates);

        protected DatabaseMigrationStrategy(IManagedConnection connection)
        {
            _managedConnection = connection;
        }

        public void Execute(IDataLoadJob job, IEnumerable<MigrationColumnSet> toMigrate, IDataLoadInfo dataLoadInfo, GracefulCancellationToken cancellationToken)
        {
            _dataLoadInfo = dataLoadInfo;
            
            // Column set for each table we are migrating
            foreach (var columnsToMigrate in toMigrate)
            {
                var inserts = 0;
                var updates = 0;
                var tableLoadInfo = dataLoadInfo.CreateTableLoadInfo("", columnsToMigrate.DestinationTable.GetFullyQualifiedName(), new[] { new DataSource(columnsToMigrate.SourceTable.GetFullyQualifiedName(), DateTime.Now) }, 0);
                try
                {
                    MigrateTable(job,columnsToMigrate, dataLoadInfo.ID, cancellationToken, ref inserts, ref updates);
                    OnTableMigrationCompleteHandler(columnsToMigrate.DestinationTable.GetFullyQualifiedName(), inserts, updates);
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
            if (TableMigrationCompleteHandler != null) TableMigrationCompleteHandler(tableName, numInserts, numUpdates);
        }
    }
}