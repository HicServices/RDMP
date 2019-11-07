// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Triggers;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations
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

            CloneTable(liveDb, _hicDatabaseConfiguration.DeployInfo[_copyToBubble], _tableInfo.Discover(DataAccessContext.DataLoad), destTableName, DropHICColumns, DropIdentityColumns, AllowNulls, discardedColumns);
            
            _operationSucceeded = true;
        }


        public void Undo()
        {
            if(!_operationSucceeded)
                throw new Exception("Cannot undo operation because it has not yet been executed");

            var tableToRemove = _tableInfo.GetRuntimeName(_copyToBubble, _hicDatabaseConfiguration.DatabaseNamer);
            RemoveTableFromDatabase(tableToRemove, _hicDatabaseConfiguration.DeployInfo[_copyToBubble]);
        }

        
        public void RemoveTableFromDatabase(string tableName, DiscoveredDatabase dbInfo)
        {
            if (!IsNukable(dbInfo))
                throw new Exception("This method nukes a table in a database! for obvious reasons this is only allowed on databases with a suffix _STAGING/_RAW");

            dbInfo.ExpectTable(tableName).Drop();
        }

        private bool IsNukable(DiscoveredDatabase dbInfo)
        {
            return dbInfo.GetRuntimeName().EndsWith("_STAGING", StringComparison.CurrentCultureIgnoreCase) || dbInfo.GetRuntimeName().EndsWith("_RAW", StringComparison.CurrentCultureIgnoreCase);
        }

        public void CloneTable(DiscoveredDatabase srcDatabaseInfo, DiscoveredDatabase destDatabaseInfo, DiscoveredTable sourceTable, string destTableName, bool dropHICColumns, bool dropIdentityColumns, bool allowNulls, PreLoadDiscardedColumn[] dillutionColumns)
        {
            if (!sourceTable.Exists())
                throw new Exception("Table " + sourceTable + " does not exist on " + srcDatabaseInfo);


            //new table will start with the same name as the as the old scripted one
            DiscoveredTable newTable = destDatabaseInfo.ExpectTable(destTableName);
            
            var sql = sourceTable.ScriptTableCreation(allowNulls, allowNulls, false /*False because we want to drop these columns entirely not just flip to int*/,newTable); 
            
            using (var con = destDatabaseInfo.Server.GetConnection())
            {
                con.Open();
                using(var cmd = destDatabaseInfo.Server.GetCommand(sql, con))
                    cmd.ExecuteNonQuery();
            }

            if (!newTable.Exists())
                throw new Exception("Table '" + newTable + "' not found in " + destDatabaseInfo + " despite running table creation SQL!");
            
            foreach (DiscoveredColumn column in newTable.DiscoverColumns())
            {
                bool drop = false;
                var colName = column.GetRuntimeName();

                if (column.IsAutoIncrement)
                    drop = true;

                if (SpecialFieldNames.IsHicPrefixed(colName) && dropHICColumns)
                    drop = true;

                //drop the data load run ID field and validFrom fields, we don't need them in STAGING or RAW, it will be hard coded in the MERGE migration with a fixed value anyway.
                if (colName.Equals(SpecialFieldNames.DataLoadRunID) || colName.Equals(SpecialFieldNames.ValidFrom))
                    drop = true;

                var dillution = dillutionColumns.SingleOrDefault(c => c.GetRuntimeName().Equals(colName));

                if (dillution != null)
                    column.DataType.AlterTypeTo(dillution.Data_type);

                if(drop)
                    newTable.DropColumn(column);
            }
        }
    }
}
