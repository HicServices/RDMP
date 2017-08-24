using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataFlowPipeline.Destinations;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
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
            using (var con = new SqlConnection(DatabaseICanCreateRandomTablesIn.ConnectionString))
            {
                con.Open();
                new SqlCommand(@"if exists (select 1 from sys.tables where name ='DataTableUploadDestinationTests') drop table DataTableUploadDestinationTests", con).ExecuteNonQuery();
            } 
        }

        [Test]
        public void DataTableChangesLengths_NoReAlter()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();

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

                var toConsole = new ToConsoleDataLoadEventReceiver();

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
            destination.PreInitialize(db, new ToConsoleDataLoadEventReceiver());

            //order is inverted where name comes out at the end column (index 2)
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("age", typeof(string));
            dt1.Columns.Add("color", typeof(string));
            dt1.Columns.Add("name", typeof(string));

            dt1.Rows.Add("30", "blue", "Fish");
            dt1.TableName = "DroppedColumnsTable";

            var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, new ToConsoleDataLoadEventReceiver(), token));

            string exceptionMessage = ex.InnerException.Message;
            var interestingBit = exceptionMessage.Substring(exceptionMessage.IndexOf(": <<") + ": ".Length);

            string expectedErrorMessage = "<<color>> which had value <<blue>> destination data type was <<varchar(1)>>";
            Assert.AreEqual(expectedErrorMessage, interestingBit);

            if(tbl.Exists())
                tbl.Drop();
        }

        [Test]
        public void DataTableEmpty_ThrowHelpfulException()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);

            DataTable dt1 = new DataTable();
            dt1.TableName = "MyEmptyTable";
            var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, toConsole, token));

            Assert.AreEqual("DataTable 'MyEmptyTable' had no Columns!", ex.Message);
        }
        [Test]
        public void DataTableNoRows_ThrowHelpfulException()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, toConsole);

            DataTable dt1 = new DataTable();
            dt1.Columns.Add("GoTeamGo");
            dt1.TableName = "MyEmptyTable";
            var ex = Assert.Throws<Exception>(() => destination.ProcessPipelineData(dt1, toConsole, token));

            Assert.AreEqual("DataTable 'MyEmptyTable' had no Rows!", ex.Message);
        }
        [Test]
        public void DataTableChangesLengths_AllowAlter()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();
            var toMemory = new ToMemoryDataLoadEventReceiver(true);

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

            Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column 'name' to size 7 based on the latest batch containing values longer than the first seen batch values! Old size was 4")));

            destination.Dispose(toConsole, null);
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(2,db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());

            
        }

        [Test]
        public void DoubleResizingBetweenIntAndDouble()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();

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

        [Test]
        public void VeryLongStringIsVarcharMax()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();

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
            Assert.AreEqual("text", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("myText").DataType.SQLType);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DecimalResizing(bool negative)
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();
            var toMemory = new ToMemoryDataLoadEventReceiver(true);

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

            Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Resizing column 'mynum' to size (5,2) based on the latest batch containing values longer than the first seen batch values! Old size was decimal(3,2)")));

            destination.Dispose(toConsole, null);
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(7, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
            Assert.AreEqual("decimal(5,2)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
        }


        private object[] _sourceLists = {
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
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();
            
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
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();
            
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
            using (var con = new SqlConnection(DatabaseICanCreateRandomTablesIn.ConnectionString))
            {
                con.Open();

                //create an example table
                SqlCommand cmdCreateTable = new SqlCommand(
                    @"
if exists (select 1 from sys.tables where name='TestResizing')
    drop table TestResizing

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
                DiscoveredTable table = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog).ExpectTable("TestResizing");
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
            }
        }

        [Test]
        public void DodgyTypes()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();

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
            
            //apparently this is varbinary(max) - go figure
            Assert.AreEqual("image(2147483647)", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("col5").DataType.SQLType);
        }


        [Test]
        public void TypeAlteringlResizing()
        {
            var token = new GracefulCancellationToken();
            DiscoveredDatabase db = new DiscoveredServer(DatabaseICanCreateRandomTablesIn).ExpectDatabase(DatabaseICanCreateRandomTablesIn.InitialCatalog);
            var toConsole = new ToConsoleDataLoadEventReceiver();
            var toMemory = new ToMemoryDataLoadEventReceiver(true);

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

            Assert.IsTrue(toMemory.EventsReceivedBySender[destination].Any(msg => msg.Message.Contains("Altering Column 'mynum' to int based on the latest batch containing values non bit/null values")));

            destination.Dispose(toConsole, null);
            Assert.IsTrue(db.ExpectTable("DataTableUploadDestinationTests").Exists());
            Assert.AreEqual(2, db.ExpectTable("DataTableUploadDestinationTests").GetRowCount());
            Assert.AreEqual("int", db.ExpectTable("DataTableUploadDestinationTests").DiscoverColumn("mynum").DataType.SQLType);
        }
    }
}
