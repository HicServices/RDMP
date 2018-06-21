using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.EntityNaming;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Migration
{
    /// <summary>
    /// Converts a list of TableInfos into MigrationColumnSets to achieve migration of records from STAGING to LIVE during a DLE execution.
    /// </summary>
    public class MigrationConfiguration
    {
        private readonly DiscoveredDatabase _fromDatabaseInfo;
        private readonly LoadBubble _fromBubble;
        private readonly LoadBubble _toBubble;
        private readonly INameDatabasesAndTablesDuringLoads _namer;

        public MigrationConfiguration(DiscoveredDatabase fromDatabaseInfo, LoadBubble fromBubble, LoadBubble toBubble, INameDatabasesAndTablesDuringLoads namer)
        {
            _fromDatabaseInfo = fromDatabaseInfo;
            _fromBubble = fromBubble;
            _toBubble = toBubble;
            _namer = namer;
        }

        public IList<MigrationColumnSet> CreateMigrationColumnSetFromTableInfos(List<TableInfo> tableInfos, List<TableInfo> lookupTableInfos, IMigrationFieldProcessor migrationFieldProcessor)
        {
            //treat null values as empty
            tableInfos = tableInfos ?? new List<TableInfo>();
            lookupTableInfos = lookupTableInfos ?? new List<TableInfo>();

            var columnSet = new List<MigrationColumnSet>();

            foreach (var tableInfo in tableInfos.Union(lookupTableInfos))
            {
                var fromTableName = tableInfo.GetRuntimeName(_fromBubble, _namer);
                var toTableName = tableInfo.GetRuntimeName(_toBubble, _namer);

                DiscoveredTable fromTable = _fromDatabaseInfo.ExpectTable(fromTableName);
                DiscoveredTable toTable = DataAccessPortal.GetInstance()
                    .ExpectDatabase(tableInfo, DataAccessContext.DataLoad)
                    .ExpectTable(toTableName);

                if(!fromTable.Exists())
                    if(lookupTableInfos.Contains(tableInfo))//its a lookup table which doesn't exist in from (Staging) - nevermind
                        continue;
                    else
                        throw new Exception("Table " + fromTableName + " was not found on on server " + _fromDatabaseInfo.Server + " (Database " + _fromDatabaseInfo + ")"); //its not a lookup table if it isn't in STAGING thats a problem!

                columnSet.Add(new MigrationColumnSet(fromTable,toTable, migrationFieldProcessor));
            }

            return columnSet;
        }
    }
}