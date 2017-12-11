using System.Data.SqlClient;
using CatalogueLibrary.Data;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
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
        public void ServerMissingDatabaseName()
        {
            var server = new ExternalDatabaseServer(CatalogueRepository, "Fiction");
            server.Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name;
            try
            {
                var destination = new ExecuteFullExtractionToDatabaseMSSql();
                destination.TargetDatabaseServer = server;

                var tomemory = new ToMemoryCheckNotifier();
                destination.Check(tomemory);

                Assert.AreEqual(CheckResult.Fail, tomemory.Messages[0].Result);
                Assert.IsTrue(tomemory.Messages[0].Message.StartsWith("TargetDatabaseServer does not have a .Database specified"));

            }
            finally
            {
                server.DeleteInDatabase();
            }
        }

        [Test]
        public void ServerDatabaseIsAbsent()
        {
            var server = new ExternalDatabaseServer(CatalogueRepository, "Fiction");
            server.Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name;
            server.Database = "FictionalDatabase";
            try
            {
                var destination = new ExecuteFullExtractionToDatabaseMSSql();
                destination.TargetDatabaseServer = server;
                destination.TableNamingPattern = "$d";

                var tomemory = new ToMemoryCheckNotifier();
                destination.Check(tomemory);

                Assert.AreEqual(CheckResult.Fail, tomemory.Messages[0].Result);
                Assert.IsTrue(tomemory.Messages[0].Message.StartsWith("Could not connect to TargetDatabaseServer 'Fiction'"));
                Assert.NotNull(tomemory.Messages[0].Ex);

                SqlConnection.ClearAllPools();//Required because the stale exception breaks future attempts to connect
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
            server.Database = "FictionalDatabase";
            

            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.CreateDatabase("FictionalDatabase");
            Assert.IsTrue(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("FictionalDatabase").Exists());

            try
            {
                var destination = new ExecuteFullExtractionToDatabaseMSSql();
                destination.TargetDatabaseServer = server;
                destination.TableNamingPattern = "$d";

                var tomemory = new ToMemoryCheckNotifier(new ThrowImmediatelyCheckNotifier());
                destination.Check(tomemory);

                Assert.AreEqual(CheckResult.Success,tomemory.GetWorst());

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
            server.Database = "FictionalDatabase";

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
                destination.TargetDatabaseServer = server;
                destination.TableNamingPattern = "$d";

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