using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using Tests.Common;

namespace ReusableCodeTests
{
    public class CrossPlatformTests:DatabaseTests
    {
        private readonly string _dbName = TestDatabaseNames.GetConsistentName("CrossPlatform");

        DiscoveredServer server;
        DiscoveredDatabase database;
        

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TestTableCreation(DatabaseType type)
        {
            database = GetCleanedServer(type,_dbName, out server, out database);

            var tbl = database.ExpectTable("CreatedTable");
            
            if(tbl.Exists())
                tbl.Drop();

            var syntaxHelper = server.GetQuerySyntaxHelper();

            database.CreateTable(tbl.GetRuntimeName(), new[]
            {
                new DatabaseColumnRequest("name", "varchar(10)", false){IsPrimaryKey=true},
                new DatabaseColumnRequest("foreignName", "nvarchar(7)"){IsPrimaryKey=true},
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
            Assert.IsTrue(name.IsPrimaryKey);

            var normalisedName = syntaxHelper.GetRuntimeName("foreignName"); //some database engines don't like capital letters?
            var foreignName = colsDictionary[normalisedName];
            Assert.AreEqual(false, foreignName.AllowNulls);//because it is part of the primary key we ignored the users request about nullability
            Assert.AreEqual(7, foreignName.DataType.GetLengthIfString());
            Assert.AreEqual(typeof(string), syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(foreignName.DataType.SQLType));
            Assert.IsTrue(foreignName.IsPrimaryKey);

            var address = colsDictionary["address"];
            Assert.AreEqual(500, address.DataType.GetLengthIfString());
            Assert.AreEqual(true, address.AllowNulls);
            Assert.AreEqual(typeof(string), syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(address.DataType.SQLType));
            Assert.IsFalse(address.IsPrimaryKey);

            var dob = colsDictionary["dob"];
            Assert.AreEqual(-1, dob.DataType.GetLengthIfString());
            Assert.AreEqual(false, dob.AllowNulls);
            Assert.AreEqual(typeof(DateTime), syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(dob.DataType.SQLType));
            Assert.IsFalse(dob.IsPrimaryKey);

            var score = colsDictionary["score"];
            Assert.AreEqual(true, score.AllowNulls);
            Assert.AreEqual(5,score.DataType.GetDigitsBeforeAndAfterDecimalPointIfDecimal().Item1);
            Assert.AreEqual(3, score.DataType.GetDigitsBeforeAndAfterDecimalPointIfDecimal().Item2);

            Assert.AreEqual(typeof(decimal), syntaxHelper.TypeTranslater.GetCSharpTypeForSQLDBType(score.DataType.SQLType));

            tbl.Drop();
        }

        

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer, "decimal(4,2)", "-23.00")]
        [TestCase(DatabaseType.MicrosoftSQLServer, "decimal(3,1)", "23.0")]
        [TestCase(DatabaseType.MicrosoftSQLServer, "int", "0")]
        [TestCase(DatabaseType.MicrosoftSQLServer, "decimal(1,0)", "00.0")]
        [TestCase(DatabaseType.MicrosoftSQLServer, "int", "-24")]
        [TestCase(DatabaseType.MYSQLServer, "decimal(4,2)", "-23.00")]
        [TestCase(DatabaseType.MYSQLServer, "int", "-25")]
        [TestCase(DatabaseType.MYSQLServer, "int", "0")]
        public void TypeConsensusBetweenDataTypeComputerAndDiscoveredTableTest(DatabaseType type, string datatType,string insertValue)
        {
            database = GetCleanedServer(type,_dbName, out server, out database);

            var tbl = database.ExpectTable("TestTableCreationStrangeTypology");

            if (tbl.Exists())
                tbl.Drop();

            var dt = new DataTable("TestTableCreationStrangeTypology");
            dt.Columns.Add("mycol");
            dt.Rows.Add(insertValue);

            var c = new DataTypeComputer();

            var tt = tbl.GetQuerySyntaxHelper().TypeTranslater;
            c.AdjustToCompensateForValue(insertValue);

            database.CreateTable(tbl.GetRuntimeName(),dt);

            Assert.AreEqual(datatType, c.GetSqlDBType(tt));

            Assert.AreEqual(datatType,tbl.DiscoverColumn("mycol").DataType.SQLType);
            Assert.AreEqual(1,tbl.GetRowCount());

            tbl.Drop();
        }

        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void ForeignKeyCreationTest(DatabaseType type)
        {
            database = GetCleanedServer(type, _dbName, out server, out database);

            var tblParent = database.CreateTable("Parent", new[]
            {
                new DatabaseColumnRequest("ID",new DatabaseTypeRequest(typeof(int))){IsPrimaryKey =  true},
                new DatabaseColumnRequest("Name",new DatabaseTypeRequest(typeof(string),10)), //varchar(10)
            });

            var parentIdPkCol = tblParent.DiscoverColumn("ID");
            
            var parentIdFkCol = new DatabaseColumnRequest("Parent_ID", new DatabaseTypeRequest(typeof (int)));

            var tblChild = database.CreateTable("Child", new[]
            {
                parentIdFkCol,
                new DatabaseColumnRequest("ChildName",new DatabaseTypeRequest(typeof(string),10)), //varchar(10)
            }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn>()
            {
                {parentIdFkCol, parentIdPkCol}
            },true);

            using (var intoParent = tblParent.BeginBulkInsert())
            {
                var dt = new DataTable();
                dt.Columns.Add("ID");
                dt.Columns.Add("Name");

                dt.Rows.Add(1, "Bob");
                dt.Rows.Add(2, "Frank");

                intoParent.Upload(dt);
            }

            using (var con = tblChild.Database.Server.GetConnection())
            {
                con.Open();

                var cmd = tblParent.Database.Server.GetCommand("INSERT INTO " + tblChild.GetRuntimeName() + " VALUES (100,'chucky')", con);
                
                //violation of fk
                Assert.That(() => cmd.ExecuteNonQuery(), Throws.Exception);

                tblParent.Database.Server.GetCommand("INSERT INTO " + tblChild.GetRuntimeName() + " VALUES (1,'chucky')", con).ExecuteNonQuery();
                tblParent.Database.Server.GetCommand("INSERT INTO " + tblChild.GetRuntimeName() + " VALUES (1,'chucky2')", con).ExecuteNonQuery();
            }
            
            Assert.AreEqual(2,tblParent.GetRowCount());
            Assert.AreEqual(2, tblChild.GetRowCount());

            using (var con = tblParent.Database.Server.GetConnection())
            {
                con.Open();

                var cmd = tblParent.Database.Server.GetCommand("DELETE FROM " + tblParent.GetRuntimeName(), con);
                cmd.ExecuteNonQuery();
            }
            
            Assert.AreEqual(0,tblParent.GetRowCount());
            Assert.AreEqual(0, tblChild.GetRowCount());
        }

        [TestCase(DatabaseType.MYSQLServer,true)]
        [TestCase(DatabaseType.MYSQLServer, false)]
        [TestCase(DatabaseType.MicrosoftSQLServer,true)]
        [TestCase(DatabaseType.MicrosoftSQLServer, false)]
        public void ForeignKeyCreationTest_TwoColumns(DatabaseType type, bool cascadeDelete)
        {
            database = GetCleanedServer(type, _dbName, out server, out database);

            var tblParent = database.CreateTable("Parent", new[]
            {
                new DatabaseColumnRequest("ID1",new DatabaseTypeRequest(typeof(int))){IsPrimaryKey =  true}, //varchar(10)
                new DatabaseColumnRequest("ID2",new DatabaseTypeRequest(typeof(int))){IsPrimaryKey =  true}, //varchar(10)
                new DatabaseColumnRequest("Name",new DatabaseTypeRequest(typeof(string),10)), //varchar(10)
            });

            var parentIdPkCol1 = tblParent.DiscoverColumn("ID1");
            var parentIdPkCol2 = tblParent.DiscoverColumn("ID2");

            var parentIdFkCol1 = new DatabaseColumnRequest("Parent_ID1", new DatabaseTypeRequest(typeof(int)));
            var parentIdFkCol2 = new DatabaseColumnRequest("Parent_ID2", new DatabaseTypeRequest(typeof(int)));

            var tblChild = database.CreateTable("Child", new[]
            {
                parentIdFkCol1,
                parentIdFkCol2,
                new DatabaseColumnRequest("ChildName",new DatabaseTypeRequest(typeof(string),10)), //varchar(10)
            }, new Dictionary<DatabaseColumnRequest, DiscoveredColumn>()
            {
                {parentIdFkCol1,parentIdPkCol1},
                {parentIdFkCol2,parentIdPkCol2}
            }, cascadeDelete);

            using (var intoParent = tblParent.BeginBulkInsert())
            {
                var dt = new DataTable();
                dt.Columns.Add("ID1");
                dt.Columns.Add("ID2");
                dt.Columns.Add("Name");

                dt.Rows.Add(1,2, "Bob");
                
                intoParent.Upload(dt);
            }

            using (var con = tblChild.Database.Server.GetConnection())
            {
                con.Open();

                var cmd = tblParent.Database.Server.GetCommand("INSERT INTO " + tblChild.GetRuntimeName() + " VALUES (1,3,'chucky')", con);

                //violation of fk
                Assert.That(() => cmd.ExecuteNonQuery(), Throws.Exception);

                tblParent.Database.Server.GetCommand("INSERT INTO " + tblChild.GetRuntimeName() + " VALUES (1,2,'chucky')", con).ExecuteNonQuery();
                tblParent.Database.Server.GetCommand("INSERT INTO " + tblChild.GetRuntimeName() + " VALUES (1,2,'chucky2')", con).ExecuteNonQuery();
            }

            Assert.AreEqual(1, tblParent.GetRowCount());
            Assert.AreEqual(2, tblChild.GetRowCount());

            using (var con = tblParent.Database.Server.GetConnection())
            {
                con.Open();
                var cmd = tblParent.Database.Server.GetCommand("DELETE FROM " + tblParent.GetRuntimeName(), con);

                if (cascadeDelete)
                {
                    cmd.ExecuteNonQuery();
                    Assert.AreEqual(0, tblParent.GetRowCount());
                    Assert.AreEqual(0, tblChild.GetRowCount());
                }
                else
                {
                    //no cascade deletes so the query should crash on violation of fk constraint
                    Assert.That(() => cmd.ExecuteNonQuery(), Throws.Exception);
                }
            }
        }

        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void CreateMaxVarcharColumns(DatabaseType type)
        {
            database = GetCleanedServer(type, _dbName, out server, out database);

            var tbl = database.CreateTable("TestDistincting", new[]
            {
                new DatabaseColumnRequest("Field1",new DatabaseTypeRequest(typeof(string),int.MaxValue)), //varchar(max)
                new DatabaseColumnRequest("Field2",new DatabaseTypeRequest(typeof(string),null)), //varchar(???)
                new DatabaseColumnRequest("Field3",new DatabaseTypeRequest(typeof(string),1000)), //varchar(???)
                new DatabaseColumnRequest("Field4",new DatabaseTypeRequest(typeof(string),5000)), //varchar(???)
                new DatabaseColumnRequest("Field5",new DatabaseTypeRequest(typeof(string),10000)), //varchar(???)
                new DatabaseColumnRequest("Field6",new DatabaseTypeRequest(typeof(string),10)), //varchar(10)
            });

            Assert.IsTrue(tbl.Exists());

            Assert.GreaterOrEqual(tbl.DiscoverColumn("Field1").DataType.GetLengthIfString(),4000);
            Assert.GreaterOrEqual(tbl.DiscoverColumn("Field2").DataType.GetLengthIfString(), 1000); // unknown size should be at least 1k? that seems sensible
            Assert.AreEqual(10,tbl.DiscoverColumn("Field6").DataType.GetLengthIfString());
        }

        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void AddColumnTest(DatabaseType type)
        {
            database = GetCleanedServer(type, _dbName, out server, out database);

            //create a single column table with primary key
            var tbl = database.CreateTable("TestDistincting", new[]
            {
                new DatabaseColumnRequest("Field1",new DatabaseTypeRequest(typeof(string),100)){IsPrimaryKey = true} //varchar(max)
            });


            //table should exist
            Assert.IsTrue(tbl.Exists());

            //column should be varchar(100)
            Assert.AreEqual(100, tbl.DiscoverColumn("Field1").DataType.GetLengthIfString());

            //and should be a primary key
            Assert.IsTrue(tbl.DiscoverColumn("Field1").IsPrimaryKey);

            //ALTER TABLE to ADD COLUMN of date type
            tbl.AddColumn("Field2", new DatabaseTypeRequest(typeof(DateTime)), true, 1000);

            //new column should exist
            var newCol = tbl.DiscoverColumn("Field2");

            //and should have a type of datetime as requested
            var typeCreated = newCol.DataType.SQLType;
            var tt = database.Server.GetQuerySyntaxHelper().TypeTranslater;
            Assert.AreEqual(typeof(DateTime), tt.GetCSharpTypeForSQLDBType(typeCreated));

            var fieldsToAlter = new List<string>(new []{"Field1", "Field2"});

            //sql server can't handle altering primary key columns or anything with a foreign key on it too!
            if (type == DatabaseType.MicrosoftSQLServer)
                fieldsToAlter.Remove("Field1"); 

            foreach (string fieldName in new String[]{})
            {

                //ALTER TABLE, ALTER COLUMN of date type each of these to be now varchar(10)s

                //discover the column
                newCol = tbl.DiscoverColumn(fieldName);

                //ALTER the column to varchar(10)
                var newTypeCSharp = new DatabaseTypeRequest(typeof(string), 10);
                var newTypeSql = tt.GetSQLDBTypeForCSharpType(newTypeCSharp);
                newCol.DataType.AlterTypeTo(newTypeSql);

                //rediscover it
                newCol = tbl.DiscoverColumn(fieldName);

                //make sure the type change happened
                Assert.AreEqual(10, newCol.DataType.GetLengthIfString());
            }

            //and should still be a primary key
            Assert.IsTrue(tbl.DiscoverColumn("Field1").IsPrimaryKey);
            //and should not be a primary key
            Assert.IsFalse(tbl.DiscoverColumn("Field2").IsPrimaryKey);
        }

        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void ChangeDatabaseShouldNotAffectOriginalConnectionString_Test(DatabaseType type)
        {
            var database1 = GetCleanedServer(type, _dbName, out server, out database);
            var stringBefore = database1.Server.Builder.ConnectionString;

            var database2 = database1.Server.ExpectDatabase("SomeOtherDb");

            Assert.AreEqual(stringBefore, database1.Server.Builder.ConnectionString);
        }


        [Test]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void TestDistincting(DatabaseType type)
        {
            database = GetCleanedServer(type, _dbName, out server, out database);

            var tbl = database.CreateTable("TestDistincting",new []
            {
                new DatabaseColumnRequest("Field1",new DatabaseTypeRequest(typeof(string),int.MaxValue)), //varchar(max)
                new DatabaseColumnRequest("Field2",new DatabaseTypeRequest(typeof(DateTime))),
                new DatabaseColumnRequest("Field3",new DatabaseTypeRequest(typeof(int)))
            });

            var dt = new DataTable();
            dt.Columns.Add("Field1");
            dt.Columns.Add("Field2");
            dt.Columns.Add("Field3");

            dt.Rows.Add(new[] {"dave", "2001-01-01", "50"});
            dt.Rows.Add(new[] {"dave", "2001-01-01", "50"});
            dt.Rows.Add(new[] {"dave", "2001-01-01", "50"});
            dt.Rows.Add(new[] {"dave", "2001-01-01", "50"});
            dt.Rows.Add(new[] {"frank", "2001-01-01", "50"});
            dt.Rows.Add(new[] {"frank", "2001-01-01", "50"});
            dt.Rows.Add(new[] {"frank", "2001-01-01", "51"});

            Assert.AreEqual(1,tbl.Database.DiscoverTables(false).Count());
            Assert.AreEqual(0,tbl.GetRowCount());

            using (var insert = tbl.BeginBulkInsert())
                insert.Upload(dt);

            Assert.AreEqual(7, tbl.GetRowCount());

            tbl.MakeDistinct();

            Assert.AreEqual(3, tbl.GetRowCount());
            Assert.AreEqual(1, tbl.Database.DiscoverTables(false).Count());
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            if(database != null && database.Exists())
                database.ForceDrop();
        }
    }
}
