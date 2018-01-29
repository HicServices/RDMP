using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.Job;
using DataLoadEngine.LoadProcess;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Standard
{
    /// <summary>
    /// DLE component resonsible for streaming data off the RAW database and writing it to the STAGING database.  Happens one table at a time with the actual
    /// implementation of moving data in MigrateRAWTableToStaging (See MigrateRAWTableToStaging).
    /// </summary>
    public class MigrateRAWToStaging : DataLoadComponent
    {
        private readonly HICDatabaseConfiguration _databaseConfiguration;
        private readonly HICLoadConfigurationFlags _loadConfigurationFlags;

        private readonly Stack<IDisposeAfterDataLoad> _toDispose = new Stack<IDisposeAfterDataLoad>();

        private readonly List<MigrateRAWTableToStaging> _tableMigrations = new List<MigrateRAWTableToStaging>();
        
        public MigrateRAWToStaging(HICDatabaseConfiguration databaseConfiguration, HICLoadConfigurationFlags loadConfigurationFlags)
        {
            _databaseConfiguration = databaseConfiguration;
            _loadConfigurationFlags = loadConfigurationFlags;

            Description = "Migrate RAW to Staging";
            SkipComponent = !_loadConfigurationFlags.DoLoadToStaging;
        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if (Skip(job)) 
                return ExitCodeType.Error;


            // To be on the safe side, we will create/destroy the staging tables on a per-load basis
            if (_databaseConfiguration.RequiresStagingTableCreation)
                CreateStagingTables(job);
            
            DoMigration(job, cancellationToken);

            return ExitCodeType.Success;
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            while (_toDispose.Any())
                _toDispose.Pop().LoadCompletedSoDispose(exitCode, postLoadEventListener);
        }

        private void DoMigration(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            foreach (var regularTableInfo in job.RegularTablesToLoad)
                MigrateRAWTableToStaging(job, regularTableInfo, false, cancellationToken);
            
            foreach (var lookupTableInfo in job.LookupTablesToLoad)
                MigrateRAWTableToStaging(job, lookupTableInfo, true, cancellationToken);
        }


        private void MigrateRAWTableToStaging(IDataLoadJob job, TableInfo tableInfo, bool isLookupTable, GracefulCancellationToken cancellationToken)
        {
            var component = new MigrateRAWTableToStaging(tableInfo, isLookupTable, _databaseConfiguration);
            _tableMigrations.Add(component);
            component.Run(job, cancellationToken);
        }

        private void CreateStagingTables(IDataLoadJob job)
        {
            var cloner = new DatabaseCloner(_databaseConfiguration);
            job.CreateTablesInStage(cloner, LoadBubble.Staging);
         
        }
    }
}