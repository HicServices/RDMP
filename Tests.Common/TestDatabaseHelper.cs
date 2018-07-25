using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using CatalogueLibrary.Data;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace Tests.Common
{
    [Obsolete("Just use DiscoveredDatabase directly please")]
    /// <summary>
    /// Manages a test database, including creation and disposal. Intended for use at test fixture level, i.e. shared by multiple tests.
    /// </summary>
    public class TestDatabaseHelper
    {
        public DiscoveredDatabase DiscoveredDatabase { get; private set; }

        public TestDatabaseHelper(DiscoveredDatabase database)
        {
            DiscoveredDatabase = database;
        }

        public void Create()
        {
            DiscoveredDatabase.Create(true);
        }

        public void Destroy()
        {
            DiscoveredDatabase.ForceDrop();
        }
        
        public void CreateTableWithColumnDefinitions(string tableName, string columnDefinitions)
        {
            using (var conn = DiscoveredDatabase.Server.GetConnection())
            {
                conn.Open();
                CreateTableWithColumnDefinitions(tableName, columnDefinitions, conn);
            }
        }

        public void CreateTableWithColumnDefinitions(string tableName, string columnDefinitions, DbConnection conn)
        {
            var sql = "CREATE TABLE " + tableName + " (" + columnDefinitions + ")";
            DiscoveredDatabase.Server.GetCommand(sql, conn).ExecuteNonQuery();
        }
    }
}