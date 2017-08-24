using System;
using System.Data.SqlClient;
using CatalogueLibrary.Triggers;
using DataLoadEngine.DatabaseManagement;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    internal class HousekeepingTests : DatabaseTests
    {
        [Test]
        public void TestCheckUpdateTrigger()
        {
            
            // set up a test database
            const string databaseName = "DatabaseTests_TestCheckUpdateTriggerIsEnabled";
            const string tableName = "TestTable";

            var smoServer = new Server(new ServerConnection(new SqlConnection(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString)));

            var database = CreateDatabaseAndReplaceIfExists(smoServer, databaseName);
            var table = new Table(database, tableName);
            table.Columns.Add(new Column(table, "Id", DataType.Int));

            database.Tables.Add(table);

            database.Create();
            table.Create();

            var trigger = new Trigger(table, "TestTable_OnUpdate");
            trigger.TextHeader = "CREATE TRIGGER [" + databaseName + "]..[TestTable_OnUpdate] ON [dbo].[" + tableName +
                                 "] AFTER DELETE AS";
            trigger.TextMode = false;
            trigger.TextBody = " RAISERROR('MESSAGE',16,10) ";
            trigger.Insert = false;
            trigger.Update = true;
            trigger.Delete = false;
            trigger.ImplementationType = ImplementationType.TransactSql;
            trigger.Create();


            var dbInfo = ToDiscoveredDatabase(
                new SqlConnectionStringBuilder(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString)
                {
                    InitialCatalog = databaseName
                });

            var triggerImplementer = new TriggerImplementer(dbInfo, tableName);
            var isEnabled = triggerImplementer.CheckUpdateTriggerIsEnabledOnServer();
            Assert.AreEqual(TriggerImplementer.TriggerStatus.Enabled, isEnabled);

            // disable the trigger and test correct reporting
            using (var con = new SqlConnection(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString))
            {
                con.Open();
                var cmd =
                    new SqlCommand(
                        "USE [" + databaseName + "]; DISABLE TRIGGER TestTable_OnUpdate ON [" + databaseName + "]..[" +
                        tableName + "]", con);
                cmd.ExecuteNonQuery();
            }


            isEnabled = triggerImplementer.CheckUpdateTriggerIsEnabledOnServer();
            Assert.AreEqual(TriggerImplementer.TriggerStatus.Disabled, isEnabled);

            if (smoServer.Name.Equals("CONSUS"))
                throw new Exception("Never drop databases on CONSUS please");
            smoServer.KillDatabase(databaseName);
        }

        private Database CreateDatabaseAndReplaceIfExists(Server server, string databaseName)
        {
            if (server.Databases.Contains(databaseName))
                server.KillDatabase(databaseName);

            return new Database(server, databaseName);
        }

        [Test]
        public void TestCloneDatabase()
        {
            /* var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["UnitTestConnection"].ConnectionString);
            var server = new Server(new ServerConnection(builder.DataSource));
            var destDatabaseName = "DatabaseTests_TestCloneDatabase_Destination";
            var srcDatabase = CreateDatabaseAndReplaceIfExists(server, "DatabaseTests_TestCloneDatabase_Source");
            var table = new Table(srcDatabase, "Table1");
            table.Columns.Add(new Column(table, "Id", DataType.Int));
            table.Columns.Add(new Column(table, "Name", DataType.VarCharMax));
            srcDatabase.Tables.Add(table);
            srcDatabase.Create();
            table.Create();

            var srcDbInfo = new DiscoveredDatabase(builder.DataSource, srcDatabase.Name);
            var destDbInfo = new DiscoveredDatabase(builder.DataSource, destDatabaseName);

            try
            {
                DatabaseOperations.CloneDatabaseSchema(srcDbInfo, destDbInfo);

                Assert.IsNotNull(server.Databases[destDatabaseName], "Has destination database been created?");
                var destDatabase = server.Databases[destDatabaseName];

                // Test that the destination database schema matches
                Assert.IsNotNull(destDatabase.Tables[table.Name], "Has destination table been created?");

                var destTable = destDatabase.Tables[table.Name];
                Assert.AreEqual(2, destTable.Columns.Count, "Do column counts match?");
                Assert.AreEqual("Id", destTable.Columns[0].Name, "Does first column name match?");
                Assert.AreEqual(DataType.Int.ToString(), destTable.Columns[0].DataType.ToString(), "Does first column type match?");
            }
            finally
            {

                if (server.Name.Equals("CONSUS"))
                    throw new Exception("Never drop databases on CONSUS please");
                server.KillDatabase(srcDatabase.Name);
                server.KillDatabase(destDatabaseName);
            }*/
        }

    }

}

