using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace Tests.OtherProviders.Microsoft
{
    [TestFixture]
    public class BasicMicrosoftFunctionalityTests : DatabaseTests
    {
        DiscoveredServer server;
        private DiscoveredDatabase _database;
        private Exception _setupException;

        private readonly string _testDatabaseName = TestDatabaseNames.GetConsistentName("BOB");

        [TestFixtureSetUp]
        public void CreateTestDatabase()
        {
            try
            {
                server = new DiscoveredServer(ServerICanCreateRandomDatabasesAndTablesOn);
                if (!server.Exists())
                    Assert.Inconclusive();

                //cleanup 
                _database = server.ExpectDatabase(_testDatabaseName);

                DeleteTestDatabase();

                Assert.IsFalse(_database.Exists());

                //create test database
                server.CreateDatabase(_testDatabaseName);
            
                //create a table in it
                using (var con = server.GetConnection())
                {
                    con.Open();

               
                    var cmdCreate =
                        server.GetCommand(@"CREATE TABLE " + _testDatabaseName + @"..Fish 
(
id int not null,
name varchar(10) null,
height decimal(2,1) not null,
chi char(10),
myfloat float,
mydouble double precision,
chiAsNumeric numeric(10,0),
teenynumber smallint,
CONSTRAINT pk_Fish PRIMARY KEY (id, height)
)",
                            con);
                    cmdCreate.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                _setupException = e;
            }
        }

        [TestFixtureTearDown]
        public void DeleteTestDatabase()
        {
            if (!_database.Exists()) return;

            if ((_database.ExpectTable("Fish").Exists()))
            {
                _database.ExpectTable("Fish").Drop();
                Assert.IsFalse(_database.ExpectTable("Fish").Exists());
            }
            _database.Drop();
        }
        [Test]
        public void ListDatabases()
        {
            if (_setupException != null)
                throw _setupException;

            var databases = server.DiscoverDatabases();
            Assert.IsTrue(databases.Count(db => db.GetRuntimeName().Equals(_testDatabaseName)) == 1);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void CreateAndDestroy(bool simulateExceptionOnConnection)
        {
            using (var con = new SqlConnection(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString))
            {
                con.Open();

                var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("BasicMicrosoftFunctionalityTests");
                
                //remnant
                db.ForceDrop();
                Assert.IsFalse(Exists(con, "BasicMicrosoftFunctionalityTests"));

                if (simulateExceptionOnConnection)
                    try
                    {
                        using (var con2 = db.Server.GetConnection())
                        {
                            con2.Open();
                        }
                    }
                    catch (Exception e)
                    {
                    }

                SqlConnection.ClearAllPools();//Required because the stale exception breaks future attempts to connect

                db.Server.CreateDatabase("BasicMicrosoftFunctionalityTests");
                Assert.IsTrue(Exists(con, "BasicMicrosoftFunctionalityTests"));
                
                db.Drop();
                Assert.IsFalse(Exists(con, "BasicMicrosoftFunctionalityTests"));
            }
        }
        private bool Exists(SqlConnection con, string dbname)
        {
            return Convert.ToBoolean(new SqlCommand("select case when exists (select * from sys.databases where name = '" + dbname + "') then 1 else 0 end", con).ExecuteScalar());
        }

        [Test]
        public void ListTables()
        {
            var tables = server.ExpectDatabase(_testDatabaseName).DiscoverTables(false);

            Assert.IsTrue(tables.Count(t => t.GetRuntimeName().Equals("Fish")) == 1, "Failed to find table Fish, tables were " + string.Join(",",tables.Select(t=>t.GetRuntimeName())));
        }

        [Test]
        public void ListColumns()
        {
            var cols = server.ExpectDatabase(_testDatabaseName).ExpectTable("Fish").DiscoverColumns();

            Assert.AreEqual("int",cols.Single(c=>c.GetRuntimeName().Equals("id")).DataType.SQLType);
            Assert.AreEqual("varchar(10)", cols.Single(c => c.GetRuntimeName().Equals("name")).DataType.SQLType);
            Assert.AreEqual("decimal(2,1)", cols.Single(c => c.GetRuntimeName().Equals("height")).DataType.SQLType);

            Assert.AreEqual("char(10)", cols.Single(c => c.GetRuntimeName().Equals("chi")).DataType.SQLType);
            Assert.AreEqual("float", cols.Single(c => c.GetRuntimeName().Equals("myfloat")).DataType.SQLType);
            Assert.AreEqual("float", cols.Single(c => c.GetRuntimeName().Equals("mydouble")).DataType.SQLType);
            Assert.AreEqual("numeric(10,0)", cols.Single(c => c.GetRuntimeName().Equals("chiAsNumeric")).DataType.SQLType);
            Assert.AreEqual("smallint", cols.Single(c => c.GetRuntimeName().Equals("teenynumber")).DataType.SQLType);   
        }

        [Test]
        public void GetPrimaryKeys()
        {
            var cols = server.ExpectDatabase(_testDatabaseName).ExpectTable("Fish").DiscoverColumns();

            Assert.IsTrue(cols.Single(c=>c.GetRuntimeName().Equals("id")).IsPrimaryKey);
            Assert.IsTrue(cols.Single(c=>c.GetRuntimeName().Equals("height")).IsPrimaryKey);

            Assert.IsFalse(cols.Single(c => c.GetRuntimeName().Equals("chi")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c=>c.GetRuntimeName().Equals("myfloat")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c => c.GetRuntimeName().Equals("chiAsNumeric")).IsPrimaryKey);
            Assert.IsFalse(cols.Single(c => c.GetRuntimeName().Equals("teenynumber")).IsPrimaryKey);
        }

        [Test]
        public void RowCount_WithTransaction()
        {
            var table = server.ExpectDatabase(_testDatabaseName).ExpectTable("Fish");

            //now rows to start with
            Assert.AreEqual(0, table.GetRowCount());

            using (var trans = server.BeginNewTransactedConnection())
            {
                server.GetCommand("INSERT INTO " + _testDatabaseName + "..Fish VALUES (1,'flibble',1.5,'0101010101',2.5,1.1,1000,1)", trans).ExecuteNonQuery();

                Assert.AreEqual(1, table.GetRowCount(trans.ManagedTransaction));

                trans.ManagedTransaction.AbandonAndCloseConnection();
            }

            Assert.AreEqual(0, table.GetRowCount());
        }

        [Test]
        public void RowCount()
        {
            var table = server.ExpectDatabase(_testDatabaseName).ExpectTable("Fish");
            
            //now rows to start with
            Assert.AreEqual(0,table.GetRowCount());

            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand("INSERT INTO " + _testDatabaseName + "..Fish VALUES (1,'flibble',1.5,'0101010101',2.5,1.1,1000,1)", con)
                    .ExecuteNonQuery();

                Assert.AreEqual(1, table.GetRowCount());

                server.GetCommand("DELETE FROM " + _testDatabaseName + "..Fish", con)
                    .ExecuteNonQuery();

                Assert.AreEqual(0, table.GetRowCount());
            }
        }

        [Test]
        public void CannotDelete()
        {
            var table = _database.ExpectTable("Fish");

            Assert.IsTrue(table.Exists());

            using (var connection = server.BeginNewTransactedConnection())
            {
                var t = connection.ManagedTransaction;

                //table should be there
                Assert.IsTrue(table.Exists(t));
            
                //drop it within transaction
                table.Drop(t);

                //shouldn't be there within transaction
                Assert.IsFalse(table.Exists(t));
            
                //abandon transaction
                t.AbandonAndCloseConnection();
                
            }
            //table should be there still
            Assert.IsTrue(table.Exists());

            //rollback meant that table was there so it should refuse to drop it
            Assert.Throws<InvalidOperationException>(() => _database.Drop());
        }
    }
}
