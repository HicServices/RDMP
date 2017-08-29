using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using LoadModules.Generic.DataFlowOperations.Aliases;
using LoadModules.Generic.DataFlowOperations.Aliases.Exceptions;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration.PipelineTests.Components
{
    public class AliasHandlerTests  : DatabaseTests
    {
        private ExternalDatabaseServer _server;
        private AliasHandler _handler;

        [SetUp]
        public void SetupServer()
        {
            _server = new ExternalDatabaseServer(CatalogueRepository, "AliasHandlerTestsServer");
            _server.SetProperties(DiscoveredDatabaseICanCreateRandomTablesIn);

            var s = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = s.GetConnection())
            {
                con.Open();

                s.GetCommand("CREATE TABLE AliasHandlerTests (input varchar(50), alias varchar(50))", con).ExecuteNonQuery();

                //Two names which are aliases of the same person
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('freddie','craig')", con).ExecuteNonQuery();
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('craig','freddie')", con).ExecuteNonQuery();

                //Three names which are all aliases of the same person
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('peter','paul')", con).ExecuteNonQuery();
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('peter','pepey')", con).ExecuteNonQuery();
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('pepey','paul')", con).ExecuteNonQuery();
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('pepey','peter')", con).ExecuteNonQuery();
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('paul','pepey')", con).ExecuteNonQuery();
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('paul','peter')", con).ExecuteNonQuery();
            }

            _handler = new AliasHandler();

            _handler.AliasColumnInInputDataTables = "input";
            _handler.AliasTableSQL = "select * from AliasHandlerTests";
            _handler.DataAccessContext = DataAccessContext.DataLoad;
            _handler.ResolutionStrategy = AliasResolutionStrategy.CrashIfAliasesFound;
            _handler.TimeoutForAssemblingAliasTable = 10;
            _handler.ServerToExecuteQueryOn = _server;

        }

        [Test]
        public void ThrowBecause_ColumnNotInInputDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("cannonballer");//not the same as the expected input column name
            dt.Rows.Add(new object[] { "yes"});

            var ex = Assert.Throws<KeyNotFoundException>(()=>_handler.ProcessPipelineData(dt, new ToConsoleDataLoadEventReceiver(), new GracefulCancellationToken()));

            Assert.AreEqual("You asked to resolve aliases on a column called 'input' but no column by that name appeared in the DataTable being processed.  Columns in that table were:cannonballer",
                ex.Message);
        }

        [Test]
        public void ThrowBecause_NameAndAliasSameValue()
        {            
            var s = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = s.GetConnection())
            {
                con.Open();
                s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('dave','dave')", con).ExecuteNonQuery();
            }

            var dt = new DataTable();
            dt.Columns.Add("input");
            dt.Rows.Add(new object[] { "candle" });

            var ex =  Assert.Throws<AliasTableFetchException>(()=>_handler.ProcessPipelineData(dt, new ToConsoleDataLoadEventReceiver(), new GracefulCancellationToken()));
            Assert.IsTrue(ex.Message.StartsWith("Alias table SQL should only return aliases not exact matches"));

        }

        [Test]
        public void ThrowBecause_ThreeColumnAliasTable()
        {
            var s = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = s.GetConnection())
            {
                con.Open();
                s.GetCommand("ALTER TABLE AliasHandlerTests ADD anotherAliascol varchar(50)", con).ExecuteNonQuery();
            }

            var dt = new DataTable();
            dt.Columns.Add("input");
            dt.Columns.Add("value");

            dt.Rows.Add(new object[] { "dave", 100 });
            dt.Rows.Add(new object[] { "frank", 100 });

            var ex = Assert.Throws<AliasTableFetchException>(() => _handler.ProcessPipelineData(dt, new ToConsoleDataLoadEventReceiver(), new GracefulCancellationToken()));

            Assert.IsTrue(ex.Message.Contains("Alias table SQL resulted in 3 fields being returned"));
        }

        [Test]
        public void NoAliases()
        {
            var dt = new DataTable();
            dt.Columns.Add("input");
            dt.Columns.Add("value");

            dt.Rows.Add(new object[] {"dave", 100});
            dt.Rows.Add(new object[] {"frank", 100});

            var result = _handler.ProcessPipelineData(dt, new ToConsoleDataLoadEventReceiver(), new GracefulCancellationToken());

            Assert.AreEqual(2,result.Rows.Count);
        }


        [Test]
        public void CrashStrategy()
        {
            var dt = new DataTable();
            dt.Columns.Add("input");

            dt.Rows.Add(new object[] { "paul"});
            Assert.Throws<AliasException>(()=> _handler.ProcessPipelineData(dt, new ToConsoleDataLoadEventReceiver(), new GracefulCancellationToken()));
        }


        [Test]
        public void ResolveTwoNameAlias()
        {
            _handler.ResolutionStrategy = AliasResolutionStrategy.MultiplyInputDataRowsByAliases;

            var dt = new DataTable();
            dt.Columns.Add("value1",typeof(int));
            dt.Columns.Add("input");
            dt.Columns.Add("value2", typeof(int));

            dt.Rows.Add(new object[] { 99,"dave", 100 });
            dt.Rows.Add(new object[] { 199,"frank", 200 });
            dt.Rows.Add(new object[] { 299,"freddie", 300 }); //has a two name alias

            var result = _handler.ProcessPipelineData(dt, new ToConsoleDataLoadEventReceiver(), new GracefulCancellationToken());

            Assert.AreEqual(4, result.Rows.Count);
            
            Assert.AreEqual(299, result.Rows[2][0]);
            Assert.AreEqual("freddie", result.Rows[2][1]);//the original input row which had an alias on it
            Assert.AreEqual(300, result.Rows[2][2]);

            Assert.AreEqual(299, result.Rows[3][0]);
            Assert.AreEqual("craig", result.Rows[3][1]);//The new row that should have appeared to resolve the freddie=craig alias
            Assert.AreEqual(300, result.Rows[3][2]);//value should match the input array
        }

        [Test]
        public void ResolveThreeNameAlias()
        {
            _handler.ResolutionStrategy = AliasResolutionStrategy.MultiplyInputDataRowsByAliases;

            var dt = new DataTable();
            dt.Columns.Add("value1", typeof(int));
            dt.Columns.Add("input");
            dt.Columns.Add("value2", typeof(int));

            dt.Rows.Add(new object[] { 99, "pepey", 100 });//has a three name alias
            dt.Rows.Add(new object[] { 199, "frank", 200 });
            dt.Rows.Add(new object[] { 299, "anderson", 300 }); 

            var result = _handler.ProcessPipelineData(dt, new ToConsoleDataLoadEventReceiver(), new GracefulCancellationToken());

            Assert.AreEqual(5, result.Rows.Count);

            Assert.AreEqual(99, result.Rows[0][0]);
            Assert.AreEqual("pepey", result.Rows[0][1]);//the original input row which had an alias on it
            Assert.AreEqual(100, result.Rows[0][2]);


            //new rows are added at the end of the DataTable
            Assert.AreEqual(99, result.Rows[3][0]);
            Assert.AreEqual("paul", result.Rows[3][1]);//The new row that should have appeared to resolve the pepey=paul=peter alias
            Assert.AreEqual(100, result.Rows[3][2]);//value should match the input array

            Assert.AreEqual(99, result.Rows[4][0]);
            Assert.AreEqual("peter", result.Rows[4][1]);//The new row that should have appeared to resolve the  pepey=paul=peter alias
            Assert.AreEqual(100, result.Rows[4][2]);//value should match the input array
        }


        [TearDown]
        public void TearDown()
        {
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("AliasHandlerTests");
            
            if(tbl.Exists())
                tbl.Drop();
            
            _server.DeleteInDatabase();
        }

    }
}
