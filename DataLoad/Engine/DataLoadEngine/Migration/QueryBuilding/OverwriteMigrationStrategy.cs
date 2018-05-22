using System;
using System.Data.SqlClient;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Migration.QueryBuilding
{
    /// <summary>
    /// Migrates from STAGING to LIVE a single table (with a MigrationColumnSet).  This is an UPSERT (new replaces old) operation achieved (in SQL) with MERGE and 
    /// UPDATE (based on primary key).  Both tables must be on the same server.  A MERGE sql statement will be created using LiveMigrationQueryHelper and executed
    /// within a transaction.
    /// </summary>
    public class OverwriteMigrationStrategy : DatabaseMigrationStrategy
    {
        public OverwriteMigrationStrategy(IManagedConnection managedConnection)
            : base(managedConnection)
        {
        }

        public override void MigrateTable(IDataLoadJob job, MigrationColumnSet columnsToMigrate, int dataLoadInfoID, GracefulCancellationToken cancellationToken, ref int inserts, ref int updates)
        {
            var server = columnsToMigrate.DestinationTable.Database.Server;

            var queryHelper = new LiveMigrationQueryHelper(columnsToMigrate, dataLoadInfoID);
            string mergeQuery;
            try
            {
                mergeQuery = queryHelper.BuildMergeQuery();
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException(String.Format("Could not build merge query to migrate from {0} to {1}", columnsToMigrate.SourceTable, columnsToMigrate.DestinationTable), e);
            }

            cancellationToken.ThrowIfCancellationRequested();


            var cmd = server.GetCommand(mergeQuery, _managedConnection);
            cmd.CommandTimeout = Timeout;

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Merge query: " + Environment.NewLine + mergeQuery));

            try
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    switch (reader[0].ToString())
                    {
                        case "INSERT":
                            inserts++;
                            break;
                            // we ignore updates here, these require separate logic
                    }
                }
                reader.Close();

                var updateQuery = CreateUpdateQuery(columnsToMigrate, dataLoadInfoID);
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Update query:" + Environment.NewLine + updateQuery));

                var updateCmd = server.GetCommand(updateQuery, _managedConnection);
                updateCmd.CommandTimeout = Timeout;
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    updates = (int) updateCmd.ExecuteScalar();
                }
                catch (SqlException e)
                {
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Did not successfully perform the update queries: " + updateQuery, e));
                    throw new Exception("Did not successfully perform the update queries: " + updateQuery + " - " + e);
                }
            }
            catch (OperationCanceledException)
            {
                throw; // have to catch and rethrow this because of the catch-all below
            }
            catch (Exception e)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to migrate " + columnsToMigrate.SourceTable + " to " + columnsToMigrate.DestinationTable, e));
                throw new Exception("Failed to migrate " + columnsToMigrate.SourceTable + " to " + columnsToMigrate.DestinationTable + ": " + e);
            }
        }
    }
}