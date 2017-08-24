using System;
using System.Data.Common;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace Tests.OtherProviders
{
    public class CrossPlatformTests:DatabaseTests
    {
        private readonly string _dbName = TestDatabaseNames.GetConsistentName("CrossPlatform");

        DiscoveredServer server;
        DiscoveredDatabase database;

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void DropIfNotExists(DatabaseType type)
        {
            GetCleanedServer(type, out server, out database);
            
            //we are about to write some bespoke SQL, get the helper            
            IQuerySyntaxHelper syntaxHelper = server.GetQuerySyntaxHelper();
            string tblName = syntaxHelper.EnsureFullyQualified(_dbName,null, "DropIfNotExistsTable");//will handle case problems with ORACLE
            
            //the table that will be created/recreated below
            DiscoveredTable table = database.ExpectTable(tblName);
            Assert.IsFalse(table.Exists());//shouldn't exist yet
            
            using (var con = server.GetConnection())
            {
                con.Open();

                string createStatement = "CREATE TABLE " + tblName + "( MyNumber varchar(5) DEFAULT 'Sand')";

                //test basic query
                server.GetCommand(createStatement, con).ExecuteNonQuery();
                Assert.IsTrue(table.Exists());
                
                //wrap it with if not exists
                string wrappedCreate = table.Helper.WrapStatementWithIfTableExistanceMatches(false,new StringLiteralSqlInContext(createStatement,false), tblName);
                
                //now we can execute it multiple times no problem
                server.GetCommand(wrappedCreate, con).ExecuteNonQuery();
                server.GetCommand(wrappedCreate, con).ExecuteNonQuery();
                Assert.IsTrue(table.Exists());

                //now we can drop it and recreate it using the wrapped query
                table.Drop();
                server.GetCommand(wrappedCreate, con).ExecuteNonQuery();
                Assert.IsTrue(table.Exists());

                string dropCode = "DROP TABLE " + tblName;
                string wrappedDropCode = table.Helper.WrapStatementWithIfTableExistanceMatches(true,new StringLiteralSqlInContext(dropCode,false), tblName);
                
                //drop it with the wrapping
                server.GetCommand(wrappedDropCode, con).ExecuteNonQuery();
                Assert.IsFalse(table.Exists());//now it shouldnt exist

                //and we can repeat the command without crashing it
                server.GetCommand(wrappedDropCode, con).ExecuteNonQuery();
                server.GetCommand(wrappedDropCode, con).ExecuteNonQuery();
                
                Assert.IsFalse(database.ExpectTable(tblName).Exists());

                //but without the wrapping it blows up
                try
                {
                    server.GetCommand(dropCode, con).ExecuteNonQuery();
                    Assert.Fail("Expected it to crash before here");
                }
                catch (Exception)
                {
                    Assert.Pass();
                }
                
            }
        }

        private void GetCleanedServer(DatabaseType type, out DiscoveredServer server, out DiscoveredDatabase database)
        {
            DbConnectionStringBuilder builder;

            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    builder = ServerICanCreateRandomDatabasesAndTablesOn;
                    break;
                case DatabaseType.MYSQLServer:
                    builder = MySQlServer;
                    break;
                case DatabaseType.Oracle:
                    builder = OracleServer;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            if(builder == null)
                Assert.Inconclusive();

            server = new DiscoveredServer(builder);
            
            if(!server.Exists())
                Assert.Inconclusive();
            server.TestConnection();

            database = server.ExpectDatabase(_dbName);

            if (database.Exists())
            {
                foreach (DiscoveredTable discoveredTable in database.DiscoverTables(false))
                    discoveredTable.Drop();

                database.Drop();
                Assert.IsFalse(database.Exists());
            }

            server.CreateDatabase(_dbName);

            server.ChangeDatabase(_dbName);
            
            Assert.IsTrue(database.Exists());
        }

        [TearDown]
        public void TearDown()
        {
            if(database != null && database.Exists())
                database.ForceDrop();
        }
    }
}
