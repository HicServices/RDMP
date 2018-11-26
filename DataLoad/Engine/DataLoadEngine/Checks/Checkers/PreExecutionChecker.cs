using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Triggers;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Migration;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Checks.Checkers
{
    /// <summary>
    /// Checks DLE databases (RAW, STAGING, LIVE) are in a correct state ahead of running a data load (See LoadMetadata).
    /// </summary>
    public class PreExecutionChecker :  ICheckable
    {
        private readonly ILoadMetadata _loadMetadata;
        private readonly IList<ICatalogue> _cataloguesToLoad;
        private readonly HICDatabaseConfiguration _databaseConfiguration;
        
        public PreExecutionChecker(ILoadMetadata loadMetadata, HICDatabaseConfiguration overrideDatabaseConfiguration) 
        {
            _loadMetadata = loadMetadata;
            _databaseConfiguration = overrideDatabaseConfiguration ?? new HICDatabaseConfiguration(loadMetadata);
            _cataloguesToLoad = loadMetadata.GetAllCatalogues().ToList();
        }

        private void PreExecutionStagingDatabaseCheck(bool skipLookups)
        {
            var allTableInfos = _cataloguesToLoad.SelectMany(catalogue => catalogue.GetTableInfoList(!skipLookups)).Distinct().ToList();
            CheckDatabaseExistsForStage(LoadBubble.Staging, "STAGING database found", "STAGING database not found");

            if (_databaseConfiguration.RequiresStagingTableCreation)
            {
                CheckTablesDoNotExistOnStaging(allTableInfos);
            }
            else
            {
                CheckTablesAreEmptyInDatabaseOnServer();
                CheckColumnInfosMatchWithWhatIsInDatabaseAtStage(allTableInfos, LoadBubble.Staging);
                CheckStandardColumnsArePresentInStaging(allTableInfos);
            }
        }

        private void CheckDatabaseExistsForStage(LoadBubble deploymentStage, string successMessage, string failureMessage)
        {
            var dbInfo = _databaseConfiguration.DeployInfo[deploymentStage];

            if (!dbInfo.Exists())
            {
                var createDatabase = _notifier.OnCheckPerformed(new CheckEventArgs(failureMessage + ": " + dbInfo, CheckResult.Fail, null, "Create " + dbInfo.GetRuntimeName() + " on " + dbInfo.Server.Name));

                
                if (createDatabase)
                    dbInfo.Server.CreateDatabase(dbInfo.GetRuntimeName());
            }
            else
                _notifier.OnCheckPerformed(new CheckEventArgs(successMessage + ": " + dbInfo, CheckResult.Success, null));
        }

        private void CheckTablesDoNotExistOnStaging(IEnumerable<TableInfo> allTableInfos)
        {
            DiscoveredDatabase stagingDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Staging];
            var alreadyExistingTableInfosThatShouldntBeThere = new List<string>();

            var tableNames = allTableInfos.Select(info => info.GetRuntimeName(LoadBubble.Staging, _databaseConfiguration.DatabaseNamer));
            foreach (string tableName in tableNames)
            {
                if (stagingDbInfo.ExpectTable(tableName).Exists())
                    alreadyExistingTableInfosThatShouldntBeThere.Add(tableName);
            }

            if (alreadyExistingTableInfosThatShouldntBeThere.Any())
            {
                bool nukeTables;

                nukeTables = _notifier.OnCheckPerformed(new CheckEventArgs(
                        "The following tables: '" +
                        alreadyExistingTableInfosThatShouldntBeThere.Aggregate("", (s, n) => s + n + ",") +
                        "' exists in the Staging database (" + stagingDbInfo.GetRuntimeName() +
                        ") but the database load configuration requires that tables are created during the load process",
                        CheckResult.Fail, null, "Drop the tables"));

                if (nukeTables)
                    DatabaseOperations.RemoveTablesFromDatabase(alreadyExistingTableInfosThatShouldntBeThere,
                        stagingDbInfo);
            }
            else
                _notifier.OnCheckPerformed(new CheckEventArgs("Staging table is clear", CheckResult.Success, null));
        }

        private void CheckTablesAreEmptyInDatabaseOnServer()
        {
            var stagingDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Staging];

            foreach (var table in stagingDbInfo.DiscoverTables(false))
            {
                if(!table.IsEmpty())
                    _notifier.OnCheckPerformed(new CheckEventArgs("Table " + table + " in staging database '" + stagingDbInfo.GetRuntimeName() + "' is not empty on " + stagingDbInfo.Server.Name, CheckResult.Fail));
                else
                    _notifier.OnCheckPerformed(new CheckEventArgs("Staging database is empty (" + stagingDbInfo + ")", CheckResult.Success, null));
            }
            
        }

        // Check that the column infos from the catalogue match up with what is actually in the staging databases
        private void CheckColumnInfosMatchWithWhatIsInDatabaseAtStage(IEnumerable<TableInfo> allTableInfos, LoadBubble deploymentStage)
        {
            var dbInfo = _databaseConfiguration.DeployInfo[deploymentStage];
            foreach (var tableInfo in allTableInfos)
            {
                var columnNames = tableInfo.ColumnInfos.Select(info => info.GetRuntimeName()).ToList();

                if (!columnNames.Any())
                    _notifier.OnCheckPerformed(new CheckEventArgs("Table '" + tableInfo.GetRuntimeName() + "' has no ColumnInfos", CheckResult.Fail, null));

                string tableName = tableInfo.GetRuntimeName(deploymentStage, _databaseConfiguration.DatabaseNamer);
                var table = dbInfo.ExpectTable(tableName);
                    
                if(!table.Exists())
                    throw new Exception("PreExecutionChecker spotted that table does not exist:" + table + " it was about to check whether the TableInfo matched the columns or not");
            }
        }

        private void CheckStandardColumnsArePresentInStaging(IEnumerable<TableInfo> allTableInfos)
        {
            // check standard columns are present in staging database
            var standardColumnNames = new List<string>();
            CheckStandardColumnsArePresentForStage(allTableInfos, standardColumnNames, LoadBubble.Staging, LoadBubble.Staging);
        }
        private void CheckStandardColumnsArePresentInLive(IEnumerable<TableInfo> allTableInfos)
        {
            // check standard columns are present in live database
            var standardColumnNames = MigrationColumnSet.GetStandardColumnNames();
            CheckStandardColumnsArePresentForStage(allTableInfos, standardColumnNames, LoadBubble.Live, LoadBubble.Live);
        }

        private void CheckStandardColumnsArePresentForStage(IEnumerable<TableInfo> allTableInfos, List<string> columnNames, LoadBubble deploymentStage, LoadBubble tableNamingConvention)
        {
            var dbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Live];
            foreach (var tableInfo in allTableInfos)
            {
                var tableName = tableInfo.GetRuntimeName(tableNamingConvention, _databaseConfiguration.DatabaseNamer);
                try
                {
                    //only supply the schema if it is live/archive (RAW / STAGING never have schema name)
                    string schema = deploymentStage >= LoadBubble.Live ? tableInfo.Schema : null;

                    var cols = dbInfo.ExpectTable(tableName, schema).DiscoverColumns().ToArray();
                    
                    string missing = string.Join(",",
                        columnNames.Where(
                            req =>
                                !cols.Any(c => c.GetRuntimeName().Equals(req, StringComparison.CurrentCultureIgnoreCase))));
                    
                    if (!string.IsNullOrWhiteSpace(missing))
                        throw new Exception(dbInfo + " does not contain columns: " + missing);

                }
                catch (Exception e)
                {
                    _notifier.OnCheckPerformed(new CheckEventArgs("Standard columns (" + string.Join(",", columnNames) + ") not included in database structure for table '" + tableName + "'", CheckResult.Fail, e));
                }
            }

            _notifier.OnCheckPerformed(new CheckEventArgs(deploymentStage + " database '" + dbInfo + "' is correctly configured", CheckResult.Success, null));
        }

        private void PreExecutionDatabaseCheck()
        {
            var allNonLookups = _cataloguesToLoad.SelectMany(catalogue => catalogue.GetTableInfoList(false)).Distinct().ToList();
            CheckDatabaseExistsForStage(LoadBubble.Live, "LIVE database found", "LIVE database not found");
            CheckColumnInfosMatchWithWhatIsInDatabaseAtStage(allNonLookups, LoadBubble.Live);
            
            CheckStandardColumnsArePresentInLive(allNonLookups);
            CheckUpdateTriggers(allNonLookups);
            CheckRAWDatabaseIsNotPresent();
        }

        private void CheckRAWDatabaseIsNotPresent()
        {
            var rawDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Raw];

            // Check that the raw database is not present
            if (!rawDbInfo.Exists()) return;

            var shouldDrop = _notifier.OnCheckPerformed(new CheckEventArgs("RAW database '" + rawDbInfo + "' exists", CheckResult.Fail, null, "Drop database " + rawDbInfo));
            
            if(!rawDbInfo.GetRuntimeName().EndsWith("_RAW",StringComparison.CurrentCultureIgnoreCase))
                throw new Exception("rawDbInfo database name did not end with _RAW! It was:" + rawDbInfo.GetRuntimeName()+ " (Why is the system trying to drop this database?)");
            if (shouldDrop)
            {
                foreach (DiscoveredTable t in rawDbInfo.DiscoverTables(true))
                {
                    _notifier.OnCheckPerformed(new CheckEventArgs("Dropping table " + t.GetFullyQualifiedName() + "...", CheckResult.Success));
                    t.Drop();
                }

                _notifier.OnCheckPerformed(new CheckEventArgs("Finally dropping database" + rawDbInfo + "...", CheckResult.Success));
                rawDbInfo.Drop();
            }
        }

        private void CheckUpdateTriggers(IEnumerable<TableInfo> allTableInfos)
        {
            // Check that the update triggers are present/enabled
            foreach (var tableInfo in allTableInfos)
            {
                TriggerChecks checker = new TriggerChecks(_databaseConfiguration.DeployInfo[LoadBubble.Live].ExpectTable(tableInfo.GetRuntimeName(),tableInfo.Schema));
                checker.Check(_notifier);
            }
        }

 
        private ICheckNotifier _notifier;

 
        public void Check(ICheckNotifier notifier)
        {
            //extra super not threadsafe eh?
            _notifier = notifier;

            AtLeastOneTaskCheck();

            PreExecutionStagingDatabaseCheck(false);
            PreExecutionDatabaseCheck();
            
        }

        private void AtLeastOneTaskCheck()
        {
            if (!_loadMetadata.ProcessTasks.Any())
                _notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "There are no ProcessTasks defined for '" + _loadMetadata +
                        "'",
                        CheckResult.Fail));
        }
    }
      
}