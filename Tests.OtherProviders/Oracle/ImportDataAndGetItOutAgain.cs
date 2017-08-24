using System.IO;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary;
using DataLoadEngine.DataFlowPipeline.Destinations;
using Diagnostics.TestData.Exercises;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace Tests.OtherProviders.Oracle
{
    [TestFixture]
    [Ignore("Doesnt work currently")]
    public class ImportDataAndGetItOutAgain : DatabaseTests
    {
        private string testFile = "OraTestBiochem.csv";
        private string dbName = "INTEGRATION_SCRATCH";

        DiscoveredServer server;

        [TestFixtureSetUp]
        public void DoSomething()
        {
            ExerciseTestIdentifiers people = new ExerciseTestIdentifiers();
            people.GeneratePeople(100);

            ExerciseTestDataGenerator generator = new BiochemistryExerciseTestData();
            generator.GenerateTestDataFile(people, new FileInfo(testFile), 500, new ToMemoryDataLoadEventReceiver(true));

            Assert.IsTrue(File.Exists(testFile));
            
            server = new DiscoveredServer(OracleServer);

            if(!server.Exists())
                Assert.Inconclusive();
        }

        [Test]
        public void TestDataUpload()
        {
            var listener = new ToConsoleDataLoadEventReceiver();
            var canceller = new GracefulCancellationTokenSource();

            CsvDataTableHelper source = new CsvDataTableHelper(testFile,0);
            DiscoveredDatabase db = server.ExpectDatabase(dbName);

            //cleanup
            if(db.Exists())
            {
                foreach (DiscoveredTable table in server.ExpectDatabase(dbName).DiscoverTables(false))
                    table.Drop();

                db.Drop();
            }

            //recreate it
            server.CreateDatabase(dbName);
            Assert.IsTrue(db.Exists());
            
            var dt = source.GetChunk(listener, canceller.Token);
            Assert.AreEqual(500,dt.Rows.Count);

            DataTableUploadDestination destination = new DataTableUploadDestination();
            destination.PreInitialize(db, new ToConsoleDataLoadEventReceiver());
            destination.ProcessPipelineData( dt, new ToConsoleDataLoadEventReceiver(), canceller.Token);

            var finalTable = db.ExpectTable("OraTestBiochem");
            Assert.IsTrue(finalTable.Exists());
            Assert.AreEqual(500, finalTable.GetRowCount());

            //cleanup
            finalTable.Drop();

        }

    }
}
