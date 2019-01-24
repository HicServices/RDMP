using System;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using DataLoadEngine.Migration.QueryBuilding;
using FAnsi.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// Migrates records from STAGING database tables to LIVE database tables by applying the OverwriteMigrationStrategy (MERGE statement - colloquially known
    /// as UPSERT) using a shared database transaction for all tables (if one fails they will all rollback).
    /// </summary>
    public class MigrationHost 
    {
        private readonly DiscoveredDatabase _sourceDbInfo;
        private readonly DiscoveredDatabase _destinationDbInfo;
        private readonly MigrationConfiguration _migrationConfig;
        private readonly HICDatabaseConfiguration _databaseConfiguration;
        private OverwriteMigrationStrategy _migrationStrategy;

        public MigrationHost(DiscoveredDatabase sourceDbInfo, DiscoveredDatabase destinationDbInfo, MigrationConfiguration migrationConfig, HICDatabaseConfiguration databaseConfiguration)
        {
            _sourceDbInfo = sourceDbInfo;
            _destinationDbInfo = destinationDbInfo;
            _migrationConfig = migrationConfig;
            _databaseConfiguration = databaseConfiguration;
        }

        public void Migrate(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if (_sourceDbInfo.DiscoverTables(false).All(t=>t.IsEmpty()))
                throw new Exception("The source database '" + _sourceDbInfo.GetRuntimeName()+ "' on " + _sourceDbInfo.Server.Name + " is empty. There is nothing to migrate.");

            using (var managedConnectionToDestination = _destinationDbInfo.Server.BeginNewTransactedConnection())
            {
                try
                {
                    // This will eventually be provided by factory/externally based on LoadMetadata (only one strategy for now)
                    _migrationStrategy = new OverwriteMigrationStrategy(managedConnectionToDestination);
                    _migrationStrategy.TableMigrationCompleteHandler += (name, inserts, updates) =>
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Migrate table "+ name +" from STAGING to " + _destinationDbInfo.GetRuntimeName() + ": " + inserts + " inserts, " + updates + " updates"));

                    //migrate all tables (both lookups and live tables in the same way)
                    var dataColsToMigrate = _migrationConfig.CreateMigrationColumnSetFromTableInfos(job.RegularTablesToLoad, job.LookupTablesToLoad, new StagingToLiveMigrationFieldProcessor(_databaseConfiguration.UpdateButDoNotDiff));

                    // Migrate the data columns
                    _migrationStrategy.Execute(job,dataColsToMigrate, job.DataLoadInfo, cancellationToken);

                    managedConnectionToDestination.ManagedTransaction.CommitAndCloseConnection();
                    job.DataLoadInfo.CloseAndMarkComplete();
                }
                catch (OperationCanceledException)
                {
                    managedConnectionToDestination.ManagedTransaction.AbandonAndCloseConnection();
                }
                catch (Exception ex)
                {
                    try
                    {
                        managedConnectionToDestination.ManagedTransaction.AbandonAndCloseConnection();
                    }
                    catch (Exception)
                    {
                        throw new Exception("Failed to rollback after exception, see inner exception for details of original problem",ex);
                    }
                    throw;
                }
            }
        }
    }
}