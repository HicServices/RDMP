using System;
using System.Data;
using System.Data.SqlClient;
using CatalogueLibrary.Data;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace Tests.Common
{
    /// <summary>
    /// Manages a test database, including creation and disposal. Intended for use at test fixture level, i.e. shared by multiple tests.
    /// </summary>
    public class TestDatabaseHelper
    {
        private readonly string _databaseName;
        public SqlConnectionStringBuilder ConnectionDetails { get; private set; }

        private DiscoveredDatabase _DiscoveredDatabase;
        public DiscoveredDatabase DiscoveredDatabase
        {
            get
            {
                if (!_isSetUp)
                    throw new Exception("The helper has not been set up yet, call SetUp first");
                return _DiscoveredDatabase;
            }
        }

        private bool _isSetUp = false;

        public TestDatabaseHelper(string databaseName, SqlConnectionStringBuilder connectionDetails)
        {
            _databaseName = databaseName;
            ConnectionDetails = new SqlConnectionStringBuilder(connectionDetails.ConnectionString);
        }

        public void SetUp()
        {
            using (var conn = new SqlConnection(ConnectionDetails.ConnectionString))
            {
                conn.Open();
                DropIfExistsAndCreateDatabase(_databaseName, conn);
            }

            ConnectionDetails.InitialCatalog = _databaseName;
            _DiscoveredDatabase = new DiscoveredServer(ConnectionDetails).ExpectDatabase(_databaseName);
            
            _isSetUp = true;
        }

        public void TearDown()
        {
            using (var conn = new SqlConnection(ConnectionDetails.ConnectionString))
            {
                conn.Open();
                DropDatabaseIfExists(_databaseName, conn);
            }
        }

        /// <summary>
        /// Defaults to using the CatalogueConnectionString, pass in a SqlConnection to override this
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="conn"></param>
        public void DropDatabaseIfExists(string databaseName, SqlConnection conn)
        {
            CheckConnectionState(conn);
            var sql = "IF EXISTS(select * from sys.databases where name='" + databaseName + "') " +
                      "BEGIN " +
                      "USE master " +
                      "ALTER DATABASE " + databaseName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE " +
                      "DROP DATABASE " + databaseName + " " +
                      "END";
            new SqlCommand(sql, conn).ExecuteNonQuery();
        }

        private static void CheckConnectionState(SqlConnection conn)
        {
            if (conn == null)
                throw new Exception("Please pass in a valid SqlConnection, opened to the data source you wish to use.");

            if (conn.State != ConnectionState.Open)
                throw new Exception(
                    "Please make sure the connection is open first, it is up to the calling code to manage the lifecycle of the connection.");
        }

        protected void CreateDatabase(string databaseName, SqlConnection conn)
        {
            CheckConnectionState(conn);
            var sql = "CREATE DATABASE " + databaseName;
            new SqlCommand(sql, conn).ExecuteNonQuery();
        }

        public void DropAndRecreate()
        {
            using (var conn = new SqlConnection(ConnectionDetails.ConnectionString))
            {
                conn.Open();
                DropIfExistsAndCreateDatabase(_databaseName, conn);
            }
        }

        protected void DropIfExistsAndCreateDatabase(string databaseName, SqlConnection conn)
        {
            DropDatabaseIfExists(databaseName, conn);
            CreateDatabase(databaseName, conn);
        }

        public void DropTable(string tableName)
        {
            using (var conn = new SqlConnection(ConnectionDetails.ConnectionString))
            {
                conn.Open();
                DropTable(tableName, conn);
            }
        }

        public void DropTable(string tableName, SqlConnection conn)
        {
            CheckConnectionState(conn);
            var sql = "IF OBJECT_ID('dbo." + tableName + "', 'U') IS NOT NULL DROP TABLE dbo." + tableName;
            new SqlCommand(sql, conn).ExecuteNonQuery();
        }

        public void CreateTableWithColumnDefinitions(string tableName, string columnDefinitions)
        {
            using (var conn = new SqlConnection(ConnectionDetails.ConnectionString))
            {
                conn.Open();
                CreateTableWithColumnDefinitions(tableName, columnDefinitions, conn);
            }
        }

        public void CreateTableWithColumnDefinitions(string tableName, string columnDefinitions, SqlConnection conn)
        {
            CheckConnectionState(conn);
            var sql = "CREATE TABLE " + tableName + " (" + columnDefinitions + ")";
            new SqlCommand(sql, conn).ExecuteNonQuery();
        }
    }
}