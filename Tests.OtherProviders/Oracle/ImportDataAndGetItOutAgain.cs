using System.IO;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary;
using DataLoadEngine.DataFlowPipeline.Destinations;
using Diagnostics.TestData.Exercises;
using LoadModules.Generic.DataFlowSources;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
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
            generator.GenerateTestDataFile(people, new FileInfo(testFile), 500, new ToMemoryDataLoadEventListener(true));

            Assert.IsTrue(File.Exists(testFile));

            server = DiscoveredOracleServer;

            if(server == null || !server.Exists())
                Assert.Inconclusive();
        }

        [Test]
        public void TestDataUpload()
        {
            var listener = new ThrowImmediatelyDataLoadEventListener();
            var canceller = new GracefulCancellationTokenSource();

            var source = new DelimitedFlatFileDataFlowSource
            {
                Separator = ","
            };

            source.PreInitialize(new FlatFileToLoad(new FileInfo(testFile)), new ThrowImmediatelyDataLoadEventListener());//this is the file we want to load
            source.Check(new ThrowImmediatelyCheckNotifier());

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
            destination.PreInitialize(db, new ThrowImmediatelyDataLoadEventListener());
            destination.ProcessPipelineData( dt, new ThrowImmediatelyDataLoadEventListener(), canceller.Token);

            var finalTable = db.ExpectTable("OraTestBiochem");
            Assert.IsTrue(finalTable.Exists());
            Assert.AreEqual(500, finalTable.GetRowCount());

            //cleanup
            finalTable.Drop();

        }

    }
}
