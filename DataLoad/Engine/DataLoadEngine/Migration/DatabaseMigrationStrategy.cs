using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CatalogueLibrary.DataFlowPipeline;
using HIC.Logging;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Migration
{
    public delegate void TableMigrationComplete(string tableName, int numInserts, int numUpdates);

    /// <summary>
    /// See OverwriteMigrationStrategy
    /// </summary>
    abstract public class DatabaseMigrationStrategy
    {
        protected string _sourceDatabaseName;
        protected IDataLoadInfo _dataLoadInfo;
        protected IManagedConnection _managedConnection;
        protected const int Timeout = 60000;

        abstract public void MigrateTable(MigrationColumnSet columnsToMigrate, ILogManager logManager, int dataLoadInfoID, GracefulCancellationToken cancellationToken, ref int inserts, ref int updates);

        protected DatabaseMigrationStrategy( string sourceDatabaseName, IManagedConnection connection)
        {
            _sourceDatabaseName = sourceDatabaseName;
            _managedConnection = connection;
        }

        public void Execute(IEnumerable<MigrationColumnSet> toMigrate, IDataLoadInfo dataLoadInfo, ILogManager logManager, GracefulCancellationToken cancellationToken)
        {
            _dataLoadInfo = dataLoadInfo;

            // Column set for each table we are migrating
            foreach (var columnsToMigrate in toMigrate)
            {
                var inserts = 0;
                var updates = 0;
                var tableLoadInfo = dataLoadInfo.CreateTableLoadInfo("", columnsToMigrate.DestinationDatabase + "." + columnsToMigrate.DestinationTableName, new[] { new DataSource(_sourceDatabaseName + "." + columnsToMigrate.SourceTableName, DateTime.Now) }, 0);
                try
                {
                    MigrateTable(columnsToMigrate, logManager, dataLoadInfo.ID, cancellationToken, ref inserts, ref updates);
                    OnTableMigrationCompleteHandler(columnsToMigrate.DestinationTableName, inserts, updates);
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

        protected string CreateUpdateQuery(MigrationColumnSet columnsToMigrate, int dataLoadRunID)
        {
            var sourceTable = "[" + _sourceDatabaseName + "]..[" + columnsToMigrate.SourceTableName + "]";
            var destTable = "[" + columnsToMigrate.DestinationDatabase + "]..[" + columnsToMigrate.DestinationTableName + "]";

            var queryHelper = new LiveMigrationQueryHelper(columnsToMigrate, dataLoadRunID);
            return queryHelper.CreateUpdateQuery(sourceTable, destTable);
        }

        public event TableMigrationComplete TableMigrationCompleteHandler;

        protected virtual void OnTableMigrationCompleteHandler(string tableName, int numInserts, int numUpdates)
        {
            if (TableMigrationCompleteHandler != null) TableMigrationCompleteHandler(tableName, numInserts, numUpdates);
        }
    }
}