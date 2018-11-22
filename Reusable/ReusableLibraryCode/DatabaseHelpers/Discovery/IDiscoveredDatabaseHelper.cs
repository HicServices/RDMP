using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Contains all the DatabaseType specific implementation logic required by DiscoveredDatabase.
    /// </summary>
    public interface IDiscoveredDatabaseHelper
    {

        IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, bool includeViews, DbTransaction transaction = null);
        IEnumerable<DiscoveredTableValuedFunction> ListTableValuedFunctions(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, DbTransaction transaction = null);

        DiscoveredStoredprocedure[] ListStoredprocedures(DbConnectionStringBuilder builder, string database);

        IDiscoveredTableHelper GetTableHelper();
        void DropDatabase(DiscoveredDatabase database);

        Dictionary<string, string> DescribeDatabase(DbConnectionStringBuilder builder, string database);

        DiscoveredTable CreateTable(DiscoveredDatabase database, string tableName, DataTable dt, DatabaseColumnRequest[] explicitColumnDefinitions, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete, bool createEmpty = false, string schema = null);
        DiscoveredTable CreateTable(DiscoveredDatabase database, string tableName, DatabaseColumnRequest[] columns, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete, string schema = null);
        DiscoveredTable CreateTable(out Dictionary<string, DataTypeComputer> typeDictionary, DiscoveredDatabase database, string tableName, DataTable dt, DatabaseColumnRequest[] explicitColumnDefinitions, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete, bool createEmpty = false, string schema = null);

        string GetCreateTableSql(DiscoveredDatabase database, string tableName, DatabaseColumnRequest[] columns, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete,string schema = null);

        DirectoryInfo Detach(DiscoveredDatabase database);
        void CreateBackup(DiscoveredDatabase discoveredDatabase, string backupName);
    }
}