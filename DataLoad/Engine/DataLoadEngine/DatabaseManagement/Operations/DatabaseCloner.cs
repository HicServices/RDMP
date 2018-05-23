using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DatabaseManagement.Operations
{
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
        
        /// <summary>
        /// Do not use in the context of DLE (or any time where you have a cached list of TableInfos you could use instead as it will be faster to use the overload that takes TableInfo (if you use this method it has to go back to the Catalogue database to figure out what the TableInfos are)
        /// </summary>
        /// <param name="Catalogues"></param>
        /// <param name="copyToStage"></param>
        /// <param name="tableNamingScheme"></param>
        /// <param name="includeLookupTables"></param>
        public void CreateTablesInDatabaseFromCatalogueInfo(IEnumerable<ICatalogue> Catalogues, LoadBubble copyToStage, INameDatabasesAndTablesDuringLoads tableNamingScheme, bool includeLookupTables)
        {
            if (copyToStage == LoadBubble.Live)
                throw new Exception("Please don't try to create tables in the live database");

            foreach (var tableInfo in Catalogues.SelectMany(catalogue => catalogue.GetTableInfoList(includeLookupTables)).Distinct())
                CreateTablesInDatabaseFromCatalogueInfo(tableInfo, copyToStage);
        }

        public void CreateTablesInDatabaseFromCatalogueInfo(TableInfo tableInfo, LoadBubble copyToStage)
        {
            if (copyToStage == LoadBubble.Live)
                throw new Exception("Please don't try to create tables in the live database");

            var destDbInfo = _hicDatabaseConfiguration.DeployInfo[copyToStage];

            var cloneOperation = new TableInfoCloneOperation(_hicDatabaseConfiguration,tableInfo, copyToStage)
            {
                DropHICColumns = copyToStage == LoadBubble.Raw,//don't drop columns like hic_sourceID, these are optional for population (and don't get Diff'ed) but should still be there
                AllowNulls = copyToStage == LoadBubble.Raw
            };

            cloneOperation.Execute();
            _tablesCreated.Add(cloneOperation);

            
            if (copyToStage == LoadBubble.Raw)
            {
                var tableName = tableInfo.GetRuntimeName(copyToStage, _hicDatabaseConfiguration.DatabaseNamer);

                string[] existingColumns = tableInfo.ColumnInfos.Select(c => c.GetRuntimeName(LoadStage.AdjustRaw)).ToArray();

                foreach (PreLoadDiscardedColumn preLoadDiscardedColumn in tableInfo.PreLoadDiscardedColumns)
                {
                    //this column does not get dropped so will be in live TableInfo
                    if (preLoadDiscardedColumn.Destination == DiscardedColumnDestination.Dilute)
                        continue;

                    if (existingColumns.Any(e=>e.Equals(preLoadDiscardedColumn.GetRuntimeName(LoadStage.AdjustRaw))))
                        throw new Exception("There is a column called " + preLoadDiscardedColumn.GetRuntimeName(LoadStage.AdjustRaw) + " as both a PreLoadDiscardedColumn and in the TableInfo (live table), you should either drop the column from the live table or remove it as a PreLoadDiscarded column");


                    //add all the preload discarded columns because they could be routed to ANO store or sent to oblivion
                    DatabaseOperations.AddColumnToTable(destDbInfo, tableName, preLoadDiscardedColumn.RuntimeColumnName, preLoadDiscardedColumn.SqlDataType);
                }

                //deal with anonymisation transforms e.g. ANOCHI of datatype varchar(12) would have to become a column called CHI of datatype varchar(10) on creation in RAW
                var columnInfosWithANOTransforms = tableInfo.ColumnInfos.Where(c => c.ANOTable_ID != null).ToArray();
                if(columnInfosWithANOTransforms.Any())
                    foreach (ColumnInfo col in columnInfosWithANOTransforms)
                    {
                        var liveName = col.GetRuntimeName(LoadStage.PostLoad);
                        var rawName = col.GetRuntimeName(LoadStage.AdjustRaw);

                        var rawDataType = col.GetRuntimeDataType(LoadStage.AdjustRaw);

                        DatabaseOperations.DropColumnFromTable(destDbInfo, tableName, liveName);
                        DatabaseOperations.AddColumnToTable(destDbInfo,tableName,rawName,rawDataType);
                    }
            }
        }

        

        public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            //dont bother cleaning up if it bombed
            if (exitCode == ExitCodeType.Error )
                return;

            //its Abort,Success or LoadNotRequired
            foreach (var cloneOperation in _tablesCreated)
            {
                cloneOperation.Undo();
            }

            foreach (var dbInfo in _databasesCreated)
                dbInfo.Drop();
        }

   
    }
}