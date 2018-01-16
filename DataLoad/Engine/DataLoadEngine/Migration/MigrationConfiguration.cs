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
        private readonly LoadBubble _fromDatabaseTableNamingConvention;
        private readonly LoadBubble _toDatabaseTableNamingConvention;
        private readonly INameDatabasesAndTablesDuringLoads _namer;

        public MigrationConfiguration(DiscoveredDatabase fromDatabaseInfo, LoadBubble fromDatabaseTableNamingConvention, LoadBubble toDatabaseTableNamingConvention, INameDatabasesAndTablesDuringLoads namer)
        {
            _fromDatabaseInfo = fromDatabaseInfo;
            _fromDatabaseTableNamingConvention = fromDatabaseTableNamingConvention;
            _toDatabaseTableNamingConvention = toDatabaseTableNamingConvention;
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
                var fromTableName = tableInfo.GetRuntimeNameFor(_namer, _fromDatabaseTableNamingConvention);
                var toTableName = tableInfo.GetRuntimeNameFor(_namer, _toDatabaseTableNamingConvention);

                DiscoveredTable fromTable = _fromDatabaseInfo.ExpectTable(fromTableName);
                DiscoveredTable toTable = DataAccessPortal.GetInstance()
                    .ExpectDatabase(tableInfo, DataAccessContext.DataLoad)
                    .ExpectTable(toTableName);

                if(!fromTable.Exists())
                    if(lookupTableInfos.Contains(tableInfo))//its a lookup table which doesn't exist in from (Staging) - nevermind
                        continue;
                    else
                        throw new Exception("Table " + fromTableName + " was not found on on server " + _fromDatabaseInfo.Server + " (Database " + _fromDatabaseInfo + ")"); //its not a lookup table if it isn't in STAGING thats a problem!

                var sourceFields = fromTable.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();
                var destinationFields = toTable.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();

                columnSet.Add(new MigrationColumnSet(tableInfo.GetDatabaseRuntimeName(),fromTableName, toTableName, sourceFields, destinationFields,tableInfo.ColumnInfos, migrationFieldProcessor));
            }

            return columnSet;
        }
    }
}