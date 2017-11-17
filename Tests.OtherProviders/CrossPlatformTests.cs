using System;
using System.Data.Common;
using System.Linq;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
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

                //try adding a column
                Assert.AreEqual(table.DiscoverColumns().Count(),1);
                table.AddColumn("Dave",new DatabaseTypeRequest(typeof(int)),true);
                Assert.AreEqual(table.DiscoverColumns().Count(),2);

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



        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TestTableCreation(DatabaseType type)
        {
            GetCleanedServer(type, out server, out database);

            var tbl = database.ExpectTable("CreatedTable");
            
            if(tbl.Exists())
                tbl.Drop();

            var syntaxHelper = server.GetQuerySyntaxHelper();

            database.CreateTable(tbl.GetRuntimeName(), new[]
            {
                new DatabaseColumnRequest("name", "varchar(10)", false),
                new DatabaseColumnRequest("foreignName", "nvarchar(7)"),
                new DatabaseColumnRequest("address", new DatabaseTypeRequest(typeof (string), 500)),
                new DatabaseColumnRequest("dob", new DatabaseTypeRequest(typeof (DateTime)),false),
                new DatabaseColumnRequest("score",
                    new DatabaseTypeRequest(typeof (decimal), null, new Tuple<int, int>(5, 3))) //<- e.g. 12345.123 

            });

            Assert.IsTrue(tbl.Exists());

            var colsDictionary = tbl.DiscoverColumns().ToDictionary(k=>k.GetRuntimeName(),v=>v);

            var name = colsDictionary["name"];
            Assert.AreEqual(10,name.DataType.GetLengthIfString());
            Assert.AreEqual(false,name.AllowNulls);
            Assert.AreEqual(typeof(string),syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(name.DataType.SQLType));

            var normalisedName = syntaxHelper.GetRuntimeName("foreignName"); //some database engines don't like capital letters?
            var foreignName = colsDictionary[normalisedName];
            Assert.AreEqual(true, foreignName.AllowNulls);
            Assert.AreEqual(7, foreignName.DataType.GetLengthIfString());
            Assert.AreEqual(typeof(string), syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(foreignName.DataType.SQLType));
            
            var address = colsDictionary["address"];
            Assert.AreEqual(500, address.DataType.GetLengthIfString());
            Assert.AreEqual(true, address.AllowNulls);
            Assert.AreEqual(typeof(string), syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(address.DataType.SQLType));

            var dob = colsDictionary["dob"];
            Assert.AreEqual(-1, dob.DataType.GetLengthIfString());
            Assert.AreEqual(false, dob.AllowNulls);
            Assert.AreEqual(typeof(DateTime), syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(dob.DataType.SQLType));

            var score = colsDictionary["score"];
            Assert.AreEqual(true, score.AllowNulls);
            Assert.AreEqual(5,score.DataType.GetDigitsBeforeAndAfterDecimalPointIfDecimal().First);
            Assert.AreEqual(3, score.DataType.GetDigitsBeforeAndAfterDecimalPointIfDecimal().Second);

            Assert.AreEqual(typeof(decimal), syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(score.DataType.SQLType));

            tbl.Drop();
        }

        private void GetCleanedServer(DatabaseType type, out DiscoveredServer server, out DiscoveredDatabase database)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn;
                    break;
                case DatabaseType.MYSQLServer:
                    server = DiscoveredMySqlServer;
                    break;
                case DatabaseType.Oracle:
                    server = DiscoveredOracleServer;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            if(server == null)
                Assert.Inconclusive();

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
