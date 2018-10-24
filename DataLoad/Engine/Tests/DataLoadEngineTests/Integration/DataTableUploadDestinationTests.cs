using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataFlowPipeline.Destinations;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class DataTableUploadDestinationTests:DatabaseTests
    {
        [SetUp]
        public void DropTables()
        {
            var table = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("DataTableUploadDestinationTests");
            
            if(table.Exists())
                table.Drop();
        }

        [Test]
        public void DataTableChangesLengths_NoReAlter()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("name", typeof (string));
            dt1.Rows.Add(new []{"Fish"});
            dt1.TableName = "DataTableUploadDestinationTests";

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("name", typeof (string));
            dt2.Rows.Add(new []{"BigFish"});
            dt2.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData( dt1, toConsole,token);
            var ex = Assert.Throws<Exception>(()=>destination.ProcessPipelineData( dt2, toConsole,token));

            Assert.IsTrue(ex.InnerException.Message.Contains("Received an invalid column length from the bcp client for colid 1."));

            destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
        }

        //RDMPDEV-653
        [Test]
        [TestCase(true,10)]
        [TestCase(false,10)]
        public void DataTableChangesLengths_RandomColumnOrder(bool createIdentity,int numberOfRandomisations)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;

            var tbl = db.ExpectTable("RandomOrderTable");
            var random = new Random();

            for (int i =0;i<numberOfRandomisations;i++)
            {
                if (tbl.Exists())
                    tbl.Drop();

                var toConsole = new ThrowImmediatelyDataLoadEventListener();

                int errorIsInColumnOrder = random.Next(3);
                string errorColumn = "";

                string sql = "CREATE TABLE RandomOrderTable (";

                List<string> leftToCreate = new List<string>();

                leftToCreate.Add("name varchar(50),");
                leftToCreate.Add("color varchar(50),");
                leftToCreate.Add("age varchar(50),");

                if(createIdentity)
                    leftToCreate.Add("id int IDENTITY(1,1),");

                bool invalid = false;

                for (int j = 0; j < (createIdentity ? 4 : 3); j++)
                {
                    var toAddNext = random.Next(leftToCreate.Count);

                    string colSql = leftToCreate[toAddNext];

                    leftToCreate.Remove(colSql);

                    if (errorIsInColumnOrder == j)
                    {
                        sql += colSql.Replace("(50)", "(1)");
                        errorColumn = colSql.Substring(0, colSql.IndexOf(" "));

                        if (errorColumn == "id")
                            invalid = true;
                    }

                    else
                        sql += colSql;
                }

                if(invalid)
                    continue;

                sql = sql.TrimEnd(',') + ")";

                Console.Write("About to execute:" + sql);
                
                //problem is with the column name which appears at order 0 in the destination dataset (name with width 1)
                using (var con = db.Server.GetConnection())
                {
                    con.Open();
                    db.Server.GetCommand(sql, con).ExecuteNonQuery();
                }


                //the bulk insert is
                DataTableUploadDestination destination = new DataTableUploadDestination();
                destination.PreInitialize(db, toConsole);

                //order is inverted where name comes out at the end column (index 2)
                DataTable dt1 = new DataTable();
                dt1.Columns.Add("age", typeof (string));
                dt1.Columns.Add("color", typeof (string));
                dt1.Columns.Add("name", typeof (string));

                dt1.Rows.Add("30", "blue", "Fish");
                dt1.TableName = "RandomOrderTable";

                var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, toConsole, token));

                string exceptionMessage = ex.InnerException.Message;
                var interestingBit = exceptionMessage.Substring(exceptionMessage.IndexOf(": <<") + ": ".Length);
                
                string expectedErrorMessage = "<<" + errorColumn + ">> which had value <<"+dt1.Rows[0][errorColumn]+">> destination data type was <<varchar(1)>>";
                Assert.AreEqual(expectedErrorMessage,interestingBit);

                destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
                tbl.Drop();
            }
        }



        //RDMPDEV-653
        [Test]
        public void DataTableChangesLengths_DropColumns()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;

            var tbl = db.ExpectTable("DroppedColumnsTable");
            if(tbl.Exists())
                tbl.Drop();

            string sql = @"CREATE TABLE DroppedColumnsTable (
name varchar(50),
color varchar(50),
age varchar(50)
)

ALTER TABLE DroppedColumnsTable Drop column color
ALTER TABLE DroppedColumnsTable add color varchar(1)
";

            Console.Write("About to execute:" + sql);

            //problem is with the column name which appears at order 0 in the destination dataset (name with width 1)
            using (var con = db.Server.GetConnection())
            {
                con.Open();
                db.Server.GetCommand(sql, con).ExecuteNonQuery();
            }
            
            //the bulk insert is
            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, new ThrowImmediatelyDataLoadEventListener());

            //order is inverted where name comes out at the end column (index 2)
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("age", typeof(string));
            dt1.Columns.Add("color", typeof(string));
            dt1.Columns.Add("name", typeof(string));

            dt1.Rows.Add("30", "blue", "Fish");
            dt1.TableName = "DroppedColumnsTable";

            var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, new ThrowImmediatelyDataLoadEventListener(), token));

            string exceptionMessage = ex.InnerException.Message;
            var interestingBit = exceptionMessage.Substring(exceptionMessage.IndexOf(": <<") + ": ".Length);

            string expectedErrorMessage = "<<color>> which had value <<blue>> destination data type was <<varchar(1)>>";
            Assert.AreEqual(expectedErrorMessage, interestingBit);
            
            destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);

            if(tbl.Exists())
                tbl.Drop();
        }

        [Test]
        public void DataTableEmpty_ThrowHelpfulException()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);

            DataTable dt1 = new DataTable();
            dt1.TableName = "MyEmptyTable";
            var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, toConsole, token));

            destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
            
            Assert.AreEqual("DataTable 'MyEmptyTable' had no Columns!", ex.Message);
        }
        [Test]
        public void DataTableNoRows_ThrowHelpfulException()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("GoTeamGo");
            dt1.TableName = "MyEmptyTable";
            var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, toConsole, token));
            
            destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);

            Assert.AreEqual("DataTable 'MyEmptyTable' had no Rows!", ex.Message);
        }
        [Test]
        public void DataTableChangesLengths_AllowAlter()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();
            var toMemory = new ToMemoryDataLoadEventListener(true);

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("name", typeof(string));
            dt1.Rows.Add(new[] { "Fish" });
            dt1.TableName = "DataTableUploadDestinationTests";

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("name", typeof(string));
            dt2.Rows.Add(new[] { "BigFish" });
            dt2.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData( dt1, toConsole, token);
            Assert.DoesNotThrow(() => destination.ProcessPipelineData( dt2, toMemory, token));

            Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column")));

            destination.Dispose(toConsole, null);
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(2,db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

            
        }

        [Test]
        public void DoubleResizingBetweenIntAndDouble()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("mynum", typeof(double));
            dt1.Rows.Add(new object[] {1});
            dt1.Rows.Add(new object[] { 5 });
            dt1.Rows.Add(new object[] { 15 });
            dt1.Rows.Add(new object[] { 2.5 });
            dt1.Rows.Add(new object[] { 5 });

            dt1.TableName = "DataTableUploadDestinationTests";
            
            destination.ProcessPipelineData(dt1, toConsole, token);
            destination.Dispose(toConsole, null);

            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(5, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
            Assert.AreEqual("decimal(3,1)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
        }


        [TestCase("varchar(3)", 1.5, "x")]//RDMPDEV-932
        [TestCase("varchar(27)", "2001-01-01", "x")] //see DataTypeComputer.MinimumLengthRequiredForDateStringRepresentation
        public void BatchResizing(string expectedDatatypeInDatabase,object batch1Value,object batch2Value)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("mycol");
            dt1.Rows.Add(new[] {batch1Value});
            
            dt1.TableName = "DataTableUploadDestinationTests";
            try
            {
                destination.ProcessPipelineData(dt1, toConsole, token);
            
                DataTable dt2 = new DataTable();
                dt2.Columns.Add("mycol");
                dt2.Rows.Add(new object[] { batch2Value });

                destination.ProcessPipelineData(dt2, toConsole, token);
                destination.Dispose(toConsole, null);
            }
            catch (Exception e)
            {
                destination.Dispose(toConsole, e);
                throw;
            }
            
            Assert.AreEqual(expectedDatatypeInDatabase, db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mycol").DataType.SQLType);
        }

        [TestCase("varchar(24)", "2", "987styb4ih0r9h4322938476", "tinyint")]
        public void BatchResizing_WithExplicitWriteTypes(string expectedDatatypeInDatabase, object batch1Value, object batch2Value, string batch1SqlType)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("mycol");
            dt1.Rows.Add(new[] { batch1Value });

            dt1.TableName = "DataTableUploadDestinationTests";
            try
            {
                destination.AddExplicitWriteType("mycol", batch1SqlType);
                destination.ProcessPipelineData(dt1, toConsole, token);

                DataTable dt2 = new DataTable();
                dt2.Columns.Add("mycol");
                dt2.Rows.Add(new object[] { batch2Value });

                destination.ProcessPipelineData(dt2, toConsole, token);
                destination.Dispose(toConsole, null);
            }
            catch (Exception e)
            {
                destination.Dispose(toConsole, e);
                throw;
            }

            Assert.AreEqual(expectedDatatypeInDatabase, db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mycol").DataType.SQLType);
        }

        [Test]
        public void VeryLongStringIsVarcharMax()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;


            string longBitOfText = "";

            for (int i = 0; i < 9000; i++)
                longBitOfText += 'A';

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("myText");
            dt1.Rows.Add(new object[] { longBitOfText });


            dt1.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData(dt1, toConsole, token);
            destination.Dispose(toConsole, null);

            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(1, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
            Assert.AreEqual("varchar(max)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("myText").DataType.SQLType);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DecimalResizing(bool negative)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();
            var toMemory = new ToMemoryDataLoadEventListener(true);

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("mynum", typeof(string));
            dt1.Rows.Add(new[] { "1.51" });
            dt1.TableName = "DataTableUploadDestinationTests";

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("mynum", typeof(string));
            dt2.Rows.Add(new[] { negative ? "-999.99" : "999.99" });
            dt2.Rows.Add(new[] { "00000.00000" });
            dt2.Rows.Add(new[] { "0" });
            dt2.Rows.Add(new string[] { null });
            dt2.Rows.Add(new [] { "" }); 
            dt2.Rows.Add(new[] { DBNull.Value });
            dt2.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData( dt1, toConsole, token);
            destination.ProcessPipelineData( dt2, toMemory, token);

            Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column ")));

            destination.Dispose(toConsole, null);
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(7, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
            Assert.AreEqual("decimal(5,2)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
        }

        private static object[] _sourceLists = {
                                            new object[] {"decimal(4,3)",new object[]{"0.001"}, new object[]{0.001}},  //case 1
                                            new object[] {"decimal(6,3)",new object[]{"19","0.001","123.001",32.0f}, new object[]{19,0.001,123.001,32.0f}},  //case 2

                                            //Time tests
                                            new object[] {"time",new object[]{"12:01"}, new object[]{new TimeSpan(12,1,0)}},
                                            new object[] {"time",new object[]{"13:00:00"}, new object[]{new TimeSpan(13,0,0)}},

                                            //Send two dates expect datetime in database and resultant data to be legit dates
                                            new object[] {"datetime2",new object[]{"2001-01-01 12:01","2010-01-01"}, new object[]{new DateTime(2001,01,01,12,1,0),new DateTime(2010,01,01,0,0,0)}},

                                            //Mixed data types going from time to date results in us falling back to string
                                            new object[] {"varchar(10)",new object[]{"12:01","2001-01-01"}, new object[]{"12:01","2001-01-01"}}
                                        };

        
        [Test, TestCaseSource("_sourceLists")]
        public void DataTypeEstimation(string expectedDatatypeInDatabase, object[] rowValues, object[] expectedValuesReadFromDatabase)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();
            
            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("myCol", typeof(string));

            foreach (object rowValue in rowValues)
                dt1.Rows.Add(new[] {rowValue});
            
            dt1.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData(dt1, toConsole, token);

            destination.Dispose(toConsole, null);
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(expectedDatatypeInDatabase, db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("myCol").DataType.SQLType);

            using (var con = db.Server.GetConnection())
            {
                con.Open();
                var r = DatabaseCommandHelper.GetCommand("Select * from DataTableUploadDestinationTests", con).ExecuteReader();
                
                foreach (object e in expectedValuesReadFromDatabase)
                {
                    Assert.IsTrue(r.Read());
                    Assert.AreEqual(e, r["myCol"]);
                }
            }
        }



        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DecimalZeros(bool sendTheZero)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();
            
            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("mynum", typeof(string));
            dt1.Rows.Add(new[] { "0.000742548000424313" });

            if (sendTheZero)
                dt1.Rows.Add(new[] { "0" });

            dt1.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData( dt1, toConsole, token);
            destination.Dispose(toConsole, null);

            //table should exist
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());

            //should have 2 rows
            Assert.AreEqual(sendTheZero?2:1, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
            
            //should be decimal
            Assert.AreEqual("decimal(19,18)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
        }

        [Test]
        public void TestResizing()
        {
            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            var table = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("TestResizing");

            using (var con = server.GetConnection())
            {
                con.Open();

                if (table.Exists())
                    table.Drop();
                
                //create an example table
                var cmdCreateTable = server.GetCommand(
                    @"
CREATE TABLE [dbo].[TestResizing](
	[MyInteger] [int] NULL,
	[MyMaxString] [varchar](max) NULL,
	[Description] [varchar](max) NULL,
	[StringNotNull] [varchar](100) NOT NULL,
    [StringAllowNull] [varchar](100) NULL,
    [StringPk] [varchar](50) NOT NULL,
 CONSTRAINT [PK_MyExcitingPK] PRIMARY KEY CLUSTERED 
(
	[StringPk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]"
                    , con);

                //execute the table
                cmdCreateTable.ExecuteNonQuery();

                //make sure table exists
                Assert.IsTrue(table.Exists());

                //find the columns
                DiscoveredColumn[] discoveredColumns = table.DiscoverColumns();

                //should not allow nulls before
                Assert.AreEqual(false, table.DiscoverColumn("StringNotNull").AllowNulls);
                //do resize
                discoveredColumns.Single(c=>c.GetRuntimeName().Equals("StringNotNull")).DataType.Resize(500);

                //rediscover it to get the new state in database (it should now be 500 and still shouldn't allow nulls)
                Assert.AreEqual("varchar(500)",table.DiscoverColumn("StringNotNull").DataType.SQLType);
                Assert.AreEqual(false, table.DiscoverColumn("StringNotNull").AllowNulls);

                //do the same with the one that allows nulls
                Assert.AreEqual(true, table.DiscoverColumn("StringAllowNull").AllowNulls);
                discoveredColumns.Single(c => c.GetRuntimeName().Equals("StringAllowNull")).DataType.Resize(101);
                discoveredColumns.Single(c => c.GetRuntimeName().Equals("StringAllowNull")).DataType.Resize(103);
                discoveredColumns.Single(c => c.GetRuntimeName().Equals("StringAllowNull")).DataType.Resize(105);
                Assert.AreEqual("varchar(105)", table.DiscoverColumn("StringAllowNull").DataType.SQLType);
                Assert.AreEqual(true, table.DiscoverColumn("StringAllowNull").AllowNulls);

                //we should have correct understanding prior to resize
                Assert.AreEqual("varchar(50)", discoveredColumns.Single(c => c.GetRuntimeName().Equals("StringPk")).DataType.SQLType);
                Assert.AreEqual(true, discoveredColumns.Single(c => c.GetRuntimeName().Equals("StringPk")).IsPrimaryKey);
                Assert.AreEqual(false, discoveredColumns.Single(c => c.GetRuntimeName().Equals("StringPk")).AllowNulls);

                //now we execute the resize
                discoveredColumns.Single(c => c.GetRuntimeName().Equals("StringPk")).DataType.Resize(500);

                Assert.AreEqual("varchar(500)", table.DiscoverColumn("StringPk").DataType.SQLType);
                Assert.AreEqual(true, table.DiscoverColumn("StringPk").IsPrimaryKey);
                Assert.AreEqual(false, table.DiscoverColumn("StringPk").AllowNulls);
                
                con.Close();
            }
        }

        [Test]
        public void DodgyTypes()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("col1", typeof(double));
            dt1.Columns.Add("col2", typeof(double));
            dt1.Columns.Add("col3", typeof(bool));
            dt1.Columns.Add("col4", typeof(byte));
            dt1.Columns.Add("col5", typeof(byte[]));

            dt1.Rows.Add(new object[] { 0.425,0.451,true,(byte)2,new byte[]{0x5,0xA}});


            dt1.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData(dt1, toConsole, token);
            destination.Dispose(toConsole, null);

            //table should exist
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());

            //should have 2 rows
            Assert.AreEqual(1, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

            //should be decimal
            Assert.AreEqual("decimal(4,3)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col1").DataType.SQLType);
            Assert.AreEqual("decimal(4,3)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col2").DataType.SQLType);
            Assert.AreEqual("bit", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col3").DataType.SQLType);
            Assert.AreEqual("tinyint", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col4").DataType.SQLType);
            
            Assert.AreEqual("varbinary(max)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col5").DataType.SQLType);
        }


        [Test]
        public void TypeAlteringlResizing()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = DiscoveredDatabaseICanCreateRandomTablesIn;
            var toConsole = new ThrowImmediatelyDataLoadEventListener();
            var toMemory = new ToMemoryDataLoadEventListener(true);

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("mynum", typeof(string));
            dt1.Rows.Add(new[] { "true" });
            dt1.TableName = "DataTableUploadDestinationTests";

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("mynum", typeof(string));
            dt2.Rows.Add(new[] { "999" });
            dt2.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData(dt1, toConsole, token);
            destination.ProcessPipelineData(dt2, toMemory, token);

            Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column 'mynum' from 'bit' to 'int'")));

            destination.Dispose(toConsole, null);
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(2, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
            Assert.AreEqual("int", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
        }

        [Test]
        public void MySqlTest_Simple()
        {
            var token = new GracefulCancellationToken();
            
            var db = GetCleanedServer(DatabaseType.MYSQLServer,true);

            var toConsole = new ThrowImmediatelyDataLoadEventListener();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;
            
            DataTable dt = new DataTable();
            dt.Columns.Add("mystringcol", typeof(string));
            dt.Columns.Add("mynum", typeof(string));
            dt.Columns.Add("mydate", typeof (string));
            dt.Columns.Add("myLegitDateTime", typeof(DateTime));
            dt.Columns.Add("mynullcol", typeof(string));


            //drop the millisecond part
            var now = DateTime.Now;
            now = new DateTime(now.Year,now.Month,now.Day,now.Hour,now.Minute,now.Second);
            
            dt.Rows.Add(new object[] { "Anhoy there \"mates\"", "999", "2001-01-01", now,null});
            dt.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData(dt, toConsole, token);

            destination.Dispose(toConsole, null);
            var tbl = db.ExpectTable("DataTableUploadDestinationTests");
            Assert.IsTrue(tbl.Exists());
            Assert.AreEqual(1, tbl.GetRowCount());
            Assert.AreEqual("int", tbl.DiscoverColumn("mynum").DataType.SQLType);

            using (var con = db.Server.GetConnection())
            {
                con.Open();
                var r = db.Server.GetCommand(tbl.GetTopXSql(10), con).ExecuteReader();

                Assert.IsTrue(r.Read());
                Assert.AreEqual("Anhoy there \"mates\"", (string)r["mystringcol"]);
                Assert.AreEqual(999,(int)r["mynum"]);
                Assert.AreEqual(new DateTime(2001,1,1),(DateTime)r["mydate"]);
                Assert.AreEqual(now, (DateTime)r["myLegitDateTime"]);
                Assert.AreEqual(DBNull.Value, r["mynullcol"]);
            }

            db.ForceDrop();
        }

        [Test]
        public void MySqlTest_Resize()
        {
            var token = new GracefulCancellationToken();

            var db = GetCleanedServer(DatabaseType.MYSQLServer, true);

            var toConsole = new ThrowImmediatelyDataLoadEventListener();
            var toMemory = new ToMemoryDataLoadEventListener(true);

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("mynum", typeof(string));
            dt1.Rows.Add(new[] { "true" });
            dt1.TableName = "DataTableUploadDestinationTests";

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("mynum", typeof(string));
            dt2.Rows.Add(new[] { "999" });
            dt2.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData(dt1, toConsole, token);
            destination.ProcessPipelineData(dt2, toMemory, token);

            destination.Dispose(toConsole, null);
            var tbl = db.ExpectTable("DataTableUploadDestinationTests");
            Assert.IsTrue(tbl.Exists());
            Assert.AreEqual(2, tbl.GetRowCount());
            Assert.AreEqual("int", tbl.DiscoverColumn("mynum").DataType.SQLType);

            using (var con = db.Server.GetConnection())
            {
                con.Open();
                var r = db.Server.GetCommand(tbl.GetTopXSql(10), con).ExecuteReader();

                //technically these can come out in a random order
                List<int> numbersRead = new List<int>();
                Assert.IsTrue(r.Read());
                numbersRead.Add((int) r["mynum"]);
                Assert.IsTrue(r.Read());
                numbersRead.Add((int)r["mynum"]);

                Assert.IsFalse(r.Read());
                Assert.IsTrue(numbersRead.Contains(1));
                Assert.IsTrue(numbersRead.Contains(999));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestDestinationAlreadyExistingIsOk(bool targetTableIsEmpty)
        {
            //create a table in the scratch database with a single column Name
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("TestDestinationAlreadyExistingIsOk",new[]{new DatabaseColumnRequest("Name","varchar(10)",false)});
            try
            {
                if(!targetTableIsEmpty)
                {
                    //upload a single row
                    var dtAlreadyThereData = new DataTable();
                    dtAlreadyThereData.Columns.Add("Name");
                    dtAlreadyThereData.Rows.Add(new[] {"Bob"});

                    using(var bulk = tbl.BeginBulkInsert())
                        bulk.Upload(dtAlreadyThereData);
                }
            
                //create the destination component (what we want to test)
                var destinationComponent = new DataTableUploadDestination();
                destinationComponent.AllowResizingColumnsAtUploadTime = true;
                destinationComponent.AllowLoadingPopulatedTables = true;
            
                //create the simulated chunk that will be dispatched
                var dt = new DataTable("TestDestinationAlreadyExistingIsOk");
                dt.Columns.Add("Name");
                dt.Rows.Add(new[] {"Bob"});
                dt.Rows.Add(new[] { "Frank" });
                dt.Rows.Add(new[] { "I've got a lovely bunch of coconuts" });

                var listener = new ThrowImmediatelyDataLoadEventListener();

                //pre initialzie with the database (which must be part of any pipeline use case involving a DataTableUploadDestination)
                destinationComponent.PreInitialize(DiscoveredDatabaseICanCreateRandomTablesIn,listener);

                //tell the destination component to process the data
                destinationComponent.ProcessPipelineData(dt, listener,new GracefulCancellationToken());
            
                destinationComponent.Dispose(listener,null);
                Assert.AreEqual(targetTableIsEmpty?3:4, tbl.GetRowCount());
            }
            finally
            {
                tbl.Drop();
            }
        }

        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void DataTableUploadDestinationTests_PrimaryKeyDataTableWithAlterSizeLater(DatabaseType dbtype)
        {
            var db = GetCleanedServer(dbtype, true);

            var destination = new DataTableUploadDestination();
            
            destination.AllowResizingColumnsAtUploadTime = true;
            destination.PreInitialize(db,new ThrowImmediatelyDataLoadEventListener());

            DataTable dt1 = new DataTable();
            dt1.TableName = "MyTable";
            dt1.Columns.Add("Name");
            dt1.Rows.Add("Fish");

            dt1.PrimaryKey = dt1.Columns.Cast<DataColumn>().ToArray();

            destination.ProcessPipelineData(dt1, new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());
            
            DataTable dt2 = new DataTable();
            dt2.TableName = "MyTable";
            dt2.Columns.Add("Name");
            dt2.Rows.Add("Fish Monkey Fish Fish"); //notice that this is longer so the column must be resized

            dt2.PrimaryKey = dt2.Columns.Cast<DataColumn>().ToArray();

            destination.ProcessPipelineData(dt2, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());


            destination.Dispose(new ThrowImmediatelyDataLoadEventListener(),null);

            var tbl = db.ExpectTable("MyTable");
            
            Assert.AreEqual(2,tbl.GetRowCount());
            Assert.IsTrue(tbl.DiscoverColumns().Single().IsPrimaryKey);


        }

        [Test]
        public void TestDestinationAlreadyExisting_ColumnSubset()
        {
            //create a table in the scratch database with a single column Name
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("TestDestinationAlreadyExisting_ColumnSubset", new[]
            {
                new DatabaseColumnRequest("Name", "varchar(10)", false),
                new DatabaseColumnRequest("Age","int"),
                new DatabaseColumnRequest("Address","varchar(1000)")

            });

            try
            {
                
                //upload a single row of already existing data
                var dtAlreadyThereData = new DataTable();
                dtAlreadyThereData.Columns.Add("Name");
                dtAlreadyThereData.Columns.Add("Age");
                dtAlreadyThereData.Rows.Add(new object[] { "Bob",5});

                using (var bulk = tbl.BeginBulkInsert())
                    bulk.Upload(dtAlreadyThereData);
                
                //create the destination component (what we want to test)
                var destinationComponent = new DataTableUploadDestination();
                destinationComponent.AllowResizingColumnsAtUploadTime = true;
                destinationComponent.AllowLoadingPopulatedTables = true;

                //create the simulated chunk that will be dispatched
                var dt = new DataTable("TestDestinationAlreadyExisting_ColumnSubset");
                dt.Columns.Add("Name");
                dt.Rows.Add(new[] { "Bob" });
                dt.Rows.Add(new[] { "Frank" });
                dt.Rows.Add(new[] { "I've got a lovely bunch of coconuts" });

                var listener = new ThrowImmediatelyDataLoadEventListener();

                //pre initialzie with the database (which must be part of any pipeline use case involving a DataTableUploadDestination)
                destinationComponent.PreInitialize(DiscoveredDatabaseICanCreateRandomTablesIn, listener);

                //tell the destination component to process the data
                destinationComponent.ProcessPipelineData(dt, listener, new GracefulCancellationToken());

                destinationComponent.Dispose(listener, null);
                Assert.AreEqual(4, tbl.GetRowCount());
            }
            finally
            {
                tbl.Drop();
            }
        }

        #region Two Batch Tests
        [TestCase(DatabaseType.MYSQLServer, true)]
        [TestCase(DatabaseType.MYSQLServer, false)]
        [TestCase(DatabaseType.MicrosoftSQLServer, true)]
        [TestCase(DatabaseType.MicrosoftSQLServer, false)]
        public void TwoBatch_BooleanResizingTest(DatabaseType dbType, bool giveNullValuesOnly)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = GetCleanedServer(dbType, true);
            var toConsole = new ThrowImmediatelyDataLoadEventListener();
            var toMemory = new ToMemoryDataLoadEventListener(true);

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("TestedCol", typeof(string));
            dt1.Columns.Add("OtherCol", typeof(string));
            dt1.Rows.Add(new[] { giveNullValuesOnly ? null : "true", "1.51" });

            dt1.TableName = "DataTableUploadDestinationTests";

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("TestedCol", typeof(string));
            dt2.Columns.Add("OtherCol", typeof(string));

            dt2.Rows.Add(new[] { "2001-01-01", "999.99" });

            dt2.TableName = "DataTableUploadDestinationTests";

            destination.ProcessPipelineData(dt1, toConsole, token);

            Assert.AreEqual("bit", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("TestedCol").DataType.SQLType);

            destination.ProcessPipelineData(dt2, toMemory, token);

            Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column ")));

            destination.Dispose(toConsole, null);
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(2, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

            var tt = db.Server.GetQuerySyntaxHelper().TypeTranslater;

            Assert.AreEqual(

                //if all we got are nulls we should have a DateTime otherwise we had 1/true so the only usable data type is string 
                giveNullValuesOnly ? typeof(DateTime) : typeof(string),

                tt.GetCSharpTypeForSQLDBType(db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("TestedCol").DataType.SQLType));
        }

        /// <summary>
        /// Tests the systems ability to change live table datatypes during bulk insert to accomodate novel data
        /// 
        /// <para>This test set passes v1 in the first batch which determines the initial Type of the database table.  Then v2 is passed in the next batch
        /// which will (in most cases) require an ALTER of the live table to accomodate the wider datatype.</para>
        /// </summary>
        /// <param name="dbType">The DBMS to test</param>
        /// <param name="v1">The row value to send in batch 1</param>
        /// <param name="v2">The row value to send in batch 2 (after table creation)</param>
        /// <param name="expectedTypeForBatch1">The Type you expect to be used to store the v1</param>
        /// <param name="expectedTypeForBatch2">The Type you expect after ALTER to support all values seen up till now (i.e. v1) AND v2</param>
        [TestCase(DatabaseType.MYSQLServer,null,"235", typeof(bool),typeof(int))]
        [TestCase(DatabaseType.MYSQLServer, "123", "2001-01-01 12:00:00" ,typeof(int), typeof(string))] //123 cannot be converted to date so it becomes string
        [TestCase(DatabaseType.MYSQLServer, "2001-01-01", "2001-01-01 12:00:00" ,  typeof(DateTime), typeof(DateTime) )]
        [TestCase(DatabaseType.MYSQLServer, "2001-01-01", "omg", typeof(DateTime), typeof(string))]

        [TestCase(DatabaseType.MicrosoftSQLServer, null, "235", typeof(bool), typeof(int))]
        [TestCase(DatabaseType.MicrosoftSQLServer, "123", "2001-01-01 12:00:00", typeof(int), typeof(string))] //123 cannot be converted to date so it becomes string
        [TestCase(DatabaseType.MicrosoftSQLServer, "2001-01-01", "2001-01-01 12:00:00", typeof(DateTime), typeof(DateTime))]
        [TestCase(DatabaseType.MicrosoftSQLServer, "2001-01-01", "omg", typeof(DateTime), typeof(string))]
        public void TwoBatch_MiscellaneousTest(DatabaseType dbType, string v1,string v2,Type expectedTypeForBatch1,Type expectedTypeForBatch2)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = GetCleanedServer(dbType, true);
            var toConsole = new ThrowImmediatelyDataLoadEventListener();
            var toMemory = new ToMemoryDataLoadEventListener(true);

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);
            destination.AllowResizingColumnsAtUploadTime = true;

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("TestedCol", typeof(string));
            dt1.Rows.Add(new[] { v1 });

            if (v1 != null && v2 != null)
                dt1.PrimaryKey = dt1.Columns.Cast<DataColumn>().ToArray();

            dt1.TableName = "DataTableUploadDestinationTests";

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("TestedCol", typeof(string));

            dt2.Rows.Add(new[] { v2 });
            dt2.TableName = "DataTableUploadDestinationTests";

            var tt = db.Server.GetQuerySyntaxHelper().TypeTranslater;
            var tbl = db.ExpectTable("DataTableUploadDestinationTests");

            try
            {
                destination.ProcessPipelineData(dt1, toConsole, token);
                Assert.AreEqual(expectedTypeForBatch1,
                    tt.GetCSharpTypeForSQLDBType(tbl.DiscoverColumn("TestedCol").DataType.SQLType));

                destination.ProcessPipelineData(dt2, toMemory, token);
                destination.Dispose(toConsole, null);
            }
            catch(Exception ex)
            {
                destination.Dispose(toConsole, ex);
                throw;
            }
            
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(2, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

            var colAfter = tbl.DiscoverColumn("TestedCol");

            Assert.AreEqual(v1 != null && v2 != null,colAfter.IsPrimaryKey);

            Assert.AreEqual(expectedTypeForBatch2, tt.GetCSharpTypeForSQLDBType(colAfter.DataType.SQLType));
        }
        #endregion
    }
}
