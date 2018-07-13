using System.Data.SqlClient;
using CatalogueLibrary.Data;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.Interfaces.Data.DataTables;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataExportLibrary.Tests.DataExtraction
{
    public class ExecuteFullExtractionToDatabaseMSSqlChecksTests:DatabaseTests
    {
        [SetUp]
        public void CleanupOnStart()
        {
            var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("FictionalDatabase");

            if(db.Exists())
                db.ForceDrop();
        }

        [TearDown]
        public void TearDown()
        {
            var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("FictionalDatabase");

            if (db.Exists())
                db.ForceDrop();
        }
    
        [Test]
        public void NoServer()
        {
            var destination = new ExecuteFullExtractionToDatabaseMSSql();

            var tomemory = new ToMemoryCheckNotifier();
            destination.Check(tomemory);

            Assert.AreEqual(CheckResult.Fail,tomemory.Messages[0].Result);
            Assert.IsTrue(tomemory.Messages[0].Message.StartsWith("Target database server property has not been set"));
        }
        [Test]
        public void ServerMissingServerName()
        {
            var server = new ExternalDatabaseServer(CatalogueRepository, "Fiction");
            try
            {
                var destination = new ExecuteFullExtractionToDatabaseMSSql();
                destination.TargetDatabaseServer = server;

                var tomemory = new ToMemoryCheckNotifier();
                destination.Check(tomemory);

                Assert.AreEqual(CheckResult.Fail, tomemory.Messages[0].Result);
                Assert.IsTrue(tomemory.Messages[0].Message.StartsWith("TargetDatabaseServer does not have a .Server specified"));
            }
            finally
            {
                server.DeleteInDatabase();
            }
        }

        [Test]
        public void ServerDatabaseIsPresentAndCorrect()
        {
            var server = new ExternalDatabaseServer(CatalogueRepository, "Fiction");
            server.Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name;
            //server.Database = "FictionalDatabase"; Ignored by the extractor!

            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.CreateDatabase("FictionalDatabase");
            Assert.IsTrue(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("FictionalDatabase").Exists());

            try
            {
                var destination = new ExecuteFullExtractionToDatabaseMSSql();
                destination.PreInitialize(MockRepository.GenerateStub<IProject>(), new ThrowImmediatelyDataLoadEventListener());
                destination.TargetDatabaseServer = server;
                destination.TableNamingPattern = "$d";
                destination.DatabaseNamingPattern = "Fictional$nDatabase";

                var tomemory = new ToMemoryCheckNotifier(new ThrowImmediatelyCheckNotifier());
                destination.Check(tomemory);

                Assert.AreEqual(CheckResult.Warning,tomemory.GetWorst());

            }
            finally
            {
                server.DeleteInDatabase();
            }
        }

        [Test]
        public void ServerDatabaseIsPresentAndCorrectButHasTablesInIt()
        {
            var server = new ExternalDatabaseServer(CatalogueRepository, "Fiction");
            server.Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name;
            //server.Database = "FictionalDatabase"; Ignored by the extractor!

            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.CreateDatabase("FictionalDatabase");
            
            var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("FictionalDatabase");

            using (var con = db.Server.GetConnection())
            {
                con.Open();

                db.Server.GetCommand("CREATE TABLE Bob(name varchar(10))", con).ExecuteNonQuery();
            }
            
            try
            {
                var destination = new ExecuteFullExtractionToDatabaseMSSql();
                destination.PreInitialize(MockRepository.GenerateStub<IProject>(), new ThrowImmediatelyDataLoadEventListener());
                destination.TargetDatabaseServer = server;
                destination.TableNamingPattern = "$d";
                destination.DatabaseNamingPattern = "Fictional$nDatabase";

                var tomemory = new ToMemoryCheckNotifier(new ThrowImmediatelyCheckNotifier());
                destination.Check(tomemory);

                Assert.AreEqual(CheckResult.Warning, tomemory.GetWorst());

                db.ExpectTable("Bob").Drop();
            }
            finally
            {
                server.DeleteInDatabase();
            }
        }
    }
}