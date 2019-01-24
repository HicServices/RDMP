using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace DataLoadEngine.DatabaseManagement.Operations
{
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
        
        public bool DropHICColumns { get; set; }
        public bool DropIdentityColumns { get; set; }
        public bool AllowNulls { get; set; }
        
        private bool _operationSucceeded = false;

        public TableInfoCloneOperation(HICDatabaseConfiguration hicDatabaseConfiguration, TableInfo tableInfo, LoadBubble copyToBubble)
        {
            _hicDatabaseConfiguration = hicDatabaseConfiguration;
            _tableInfo = tableInfo;
            _copyToBubble = copyToBubble;
            
            DropIdentityColumns = true;
        }


        public void Execute()
        {
            if(_operationSucceeded)
                throw new Exception("Operation already executed once");

            var liveDb = DataAccessPortal.GetInstance().ExpectDatabase(_tableInfo, DataAccessContext.DataLoad);
            var destTableName = _tableInfo.GetRuntimeName(_copyToBubble, _hicDatabaseConfiguration.DatabaseNamer);


            var discardedColumns = _tableInfo.PreLoadDiscardedColumns.Where(c => c.Destination == DiscardedColumnDestination.Dilute).ToArray();

            DatabaseOperations.CloneTable(liveDb, _hicDatabaseConfiguration.DeployInfo[_copyToBubble], _tableInfo.Discover(DataAccessContext.DataLoad), destTableName, DropHICColumns, DropIdentityColumns, AllowNulls, discardedColumns);
            
            _operationSucceeded = true;
        }


        public void Undo()
        {
            if(!_operationSucceeded)
                throw new Exception("Cannot undo operation because it has not yet been executed");

            var tableToRemove = _tableInfo.GetRuntimeName(_copyToBubble, _hicDatabaseConfiguration.DatabaseNamer);
            DatabaseOperations.RemoveTableFromDatabase(tableToRemove, _hicDatabaseConfiguration.DeployInfo[_copyToBubble]);
        }
    }
}
