using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.DatabaseManagement.Operations
{
    /// <summary>
    /// Creates RAW / STAGING tables during a data load (See LoadMetadata).  Tables created are based on the live schema.  Depending on stage though certain
    /// changes will be made.  For example RAW tables will not have any constraints (primary keys, not null etc) and will also contain all PreLoadDiscardedColumns.
    /// 
    /// This class is powered by SMO and is Microsoft Sql Server specific.
    /// </summary>
    public class TableInfoCloneOperation
    {
        private readonly HICDatabaseConfiguration _hicDatabaseConfiguration;
        private readonly TableInfo _tableInfo;
        private readonly LoadBubble _copyToStage;
        
        public bool DropHICColumns { get; set; }
        public bool DropIdentityColumns { get; set; }
        public bool AllowNulls { get; set; }

        public ScriptingOptions ScriptingOptions { get; set; }

        private bool _operationSucceeded = false;

        public TableInfoCloneOperation(HICDatabaseConfiguration hicDatabaseConfiguration, TableInfo tableInfo, LoadBubble copyToStage)
        {
            _hicDatabaseConfiguration = hicDatabaseConfiguration;
            _tableInfo = tableInfo;
            _copyToStage = copyToStage;
            
            DropIdentityColumns = true;
        }


        public void Execute()
        {
            if(_operationSucceeded)
                throw new Exception("Operation already executed once");

            var liveDb = DataAccessPortal.GetInstance().ExpectDatabase(_tableInfo, DataAccessContext.DataLoad);
            var destTableName = _tableInfo.GetRuntimeNameFor(_hicDatabaseConfiguration.DatabaseNamer, _copyToStage);

            var smoTypeLookup = new SMOTypeLookup();

            Dictionary<string, DataType> dilutionDictionary = _tableInfo.PreLoadDiscardedColumns.Where(c => c.Destination == DiscardedColumnDestination.Dilute)
                .ToDictionary(c => c.RuntimeColumnName, v =>smoTypeLookup.GetSMODataTypeForSqlStringDataType(v.SqlDataType));

            DatabaseOperations.CloneTable(liveDb, _hicDatabaseConfiguration.DeployInfo[_copyToStage],  _tableInfo.GetRuntimeName(), destTableName, DropHICColumns, DropIdentityColumns, AllowNulls, ScriptingOptions,dilutionDictionary);

            
            _operationSucceeded = true;
        }


        public void Undo()
        {
            if(!_operationSucceeded)
                throw new Exception("Cannot undo operation because it has not yet been executed");

            var tableToRemove = _tableInfo.GetRuntimeNameFor(_hicDatabaseConfiguration.DatabaseNamer,_copyToStage);
            DatabaseOperations.RemoveTableFromDatabase(tableToRemove, _hicDatabaseConfiguration.DeployInfo[_copyToStage]);
        }
    }
}