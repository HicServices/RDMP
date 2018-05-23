using System;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataHelper;
using DataLoadEngine.Checks;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using HIC.Logging;
using LoadModules.Generic.Attachers;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Integration.CrossDatabaseTypeTests
{
    public class CrossDatabaseDataLoadTests :DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void Load(DatabaseType databaseType)
        {
            var defaults = new ServerDefaults(CatalogueRepository);
            defaults.ClearDefault(ServerDefaults.PermissableDefaults.RAWDataLoadServer);
            
            var db = GetCleanedServer(databaseType, "CrossDatabaseLoadTest");
            
            var dt = new DataTable("MyTable");
            dt.Columns.Add("Name");
            dt.Columns.Add("DateOfBirth");
            dt.Columns.Add("FavouriteColour");
            dt.Rows.Add("Bob", "2001-01-01","Pink");
            dt.Rows.Add("Frank", "2001-01-01","Orange");
            dt.Rows.Add("Frank", "2001-01-01","Orange");
            
            var tbl = db.CreateTable("MyTable",dt,new []
            {
                new DatabaseColumnRequest("Name","varchar(20)",false), 
                new DatabaseColumnRequest("DateOfBirth",new DatabaseTypeRequest(typeof(DateTime)),false)
            });
            Assert.AreEqual(3,tbl.GetRowCount());
            
            tbl.MakeDistinct();

            Assert.AreEqual(2, tbl.GetRowCount());

            tbl.CreatePrimaryKey(tbl.DiscoverColumn("Name"), tbl.DiscoverColumn("DateOfBirth"));

            var importer = new TableInfoImporter(CatalogueRepository,tbl);
            TableInfo ti;
            ColumnInfo[] cis;
            importer.DoImport(out ti,out cis);

            var forwardEngineer = new ForwardEngineerCatalogue(ti, cis, true);
            
            Catalogue cata;
            CatalogueItem[] cataItems;
            ExtractionInformation[] eis;
            forwardEngineer.ExecuteForwardEngineering(out cata,out cataItems,out eis);

            //define a new load configuration
            var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

            //make the catalogue use the load configuration
            cata.LoadMetadata_ID = lmd.ID;
            cata.LoggingDataTask = "MyLoading";
            cata.SaveToDatabase();

            var projectDirectory = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo(Environment.CurrentDirectory), "MyLoadDir",true);
            lmd.LocationOfFlatFiles = projectDirectory.RootPath.FullName;
            lmd.SaveToDatabase();

            var pt = new ProcessTask(CatalogueRepository, lmd,LoadStage.Mounting);
            pt.Path = typeof (AnySeparatorFileAttacher).FullName;
            pt.ProcessTaskType = ProcessTaskType.Attacher;
            pt.Name = "Load CSV";
            pt.SaveToDatabase();

            pt.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>();
            pt.SetArgumentValue("FilePattern", "*.csv");
            pt.SetArgumentValue("Separator", ",");
            pt.SetArgumentValue("TableToLoad", ti);
            
            pt.Check(new ThrowImmediatelyCheckNotifier());
            
            //create a text file to load where we update Frank's favourite colour (it's a pk field) and we insert a new record (MrMurder)
            File.WriteAllText(
                Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
@"Name,DateOfBirth,FavouriteColour
Frank,2001-01-01,Neon
MrMurder,2001-01-01,Yella");

            //clean up RAW / STAGING etc and generally accept proposed cleanup operations
            var checker = new CheckEntireDataLoadProcess(lmd, new HICDatabaseConfiguration(lmd), new HICLoadConfigurationFlags(),CatalogueRepository.MEF);
            checker.Check(new AcceptAllCheckNotifier());

            var logManager = new LogManager(lmd.GetDistinctLoggingDatabaseSettings());
            logManager.CreateNewLoggingTaskIfNotExists("MyLoading");

            var loadFactory = new HICDataLoadFactory(
                lmd,
                new HICDatabaseConfiguration(lmd),
                new HICLoadConfigurationFlags(),
                CatalogueRepository,
                logManager
                );
            try
            {
                var exe = loadFactory.Create(new ThrowImmediatelyDataLoadEventListener());
            
                var exitCode = exe.Run(
                    new DataLoadJob("Go go go!", logManager, lmd, projectDirectory,new ThrowImmediatelyDataLoadEventListener()),
                    new GracefulCancellationToken());

                Assert.AreEqual(ExitCodeType.Success,exitCode);

                Assert.AreEqual(3,tbl.GetRowCount());
                var result = tbl.GetDataTable();
                var frank = result.Rows.Cast<DataRow>().Single(r => (string) r["Name"] == "Frank");
                Assert.AreEqual("Neon",frank["FavouriteColour"]);

                var mrmurder = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "MrMurder");
                Assert.AreEqual("Yella", mrmurder["FavouriteColour"]);
                Assert.AreEqual(new DateTime(2001,01,01), mrmurder["DateOfBirth"]);

            }
            finally
            {
                cata.DeleteInDatabase();
                lmd.DeleteInDatabase();
                ti.DeleteInDatabase();
            }
        }

    }
}
