namespace DataLoadEngine.Migration
{
    public class MigrationHelper
    {
        private readonly string _liveConnectionString;
        private readonly string _stagingDatabaseName;
        private readonly string _liveDatabaseName;

        public MigrationHelper(string liveConnectionString, string stagingDatabaseName, string liveDatabaseName)
        {
            _liveConnectionString = liveConnectionString;
            _stagingDatabaseName = stagingDatabaseName;
            _liveDatabaseName = liveDatabaseName;
        }

        /*
        public int CalculateExpectedNumberOfInsertsForTable(string tableName, List<string> columnNames, List<string> primaryKeyColumnNames, int dataLoadInfoID)
        {
            // First, script the live database schema so it can be recreated as a temporary table
            // todo: should this be stored in the catalogue?
            var tempTableName = "#" + tableName + "_TEMP";
            var tableScript = ScriptTable(_liveConnectionString, _liveDatabaseName, tableName);
            var createStatements = tableScript.Cast<string>().Where(s => s.Contains("CREATE TABLE [dbo].[" + tableName + "]")).ToList();
            
            if (createStatements.Count == 0)
                throw new Exception("Create statement was not scripted, so don't know how to create a temporary clone");

            if (createStatements.Count > 1)
                throw new Exception("Multiple create statements (" + createStatements.Count + ") in the table script output: " + createStatements.Aggregate("", (s, s1) => s + ", " + s1));

            var createStatement = createStatements[0].Replace("CREATE TABLE [dbo].[" + tableName + "]",
                "CREATE TABLE [" + _stagingDatabaseName + "]..[" + tempTableName + "]");

            var builder = new SqlConnectionStringBuilder(_liveConnectionString);
            builder.InitialCatalog = _stagingDatabaseName;
            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                conn.Open();

                try
                {
                    var cmd = new SqlCommand(createStatement, conn);
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    throw new Exception("Could not create the temporary table " + tempTableName + " in " +
                                        _stagingDatabaseName + " on " + builder.DataSource + ": " + e);
                }

                // now insert into the temp table
                var sql = MigrationQueryHelper.BuildMergeQuery(columnNames, primaryKeyColumnNames,
                    "[" + _stagingDatabaseName + "]..[" + tableName + "]", tempTableName,
                    dataLoadInfoID);

                try
                {
                    var cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();

                    var count = new SqlCommand("SELECT COUNT(*) FROM [" + _stagingDatabaseName + "]..[" + tempTableName + "]", conn);
                    return Convert.ToInt32(count.ExecuteScalar());
                }
                catch (SqlException e)
                {
                    throw new Exception("Could not run merge query: " + e);
                }
            }
        }

        private StringCollection ScriptTable(string connectionString, string databaseName, string tableName)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var connection = new ServerConnection(builder.DataSource);
            var server = new Server(connection);
            var database = server.Databases[databaseName];
            if (database == null)
                throw new Exception("The database '" + databaseName + "' does not exist on server " + builder.DataSource);

            var scriptOptions = new ScriptingOptions
            {
                ScriptData = false,
                IncludeDatabaseContext = true,
                DriAll = false
            };

            if (database.Tables.Count == 0)
                throw new Exception("The database '" + databaseName + "' contains no tables");

            if (database.Tables[tableName] == null)
                throw new Exception("The table '" + tableName + "' does not exist in database '" + databaseName + "' on server '" + builder.DataSource + "'");

            return database.Tables[tableName].Script(scriptOptions);            
        }
         * */
    }
}