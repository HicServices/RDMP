using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AnonymisationTests;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataHelper;
using LoadModules.Generic.Attachers;
using DataLoadEngine.LoadExecution;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Column = Microsoft.SqlServer.Management.Smo.Column;

namespace DataLoadEngineTests.Integration
{
    class MultiStageDataLoadProcessTests : TestsRequiringANOStore
    {
        private void AddStandardColumns(Table table)
        {
            table.Columns.Add(new Column(table, "ValidFrom", DataType.DateTime));
            table.Columns.Add(new Column(table, "ValidTo", DataType.DateTime));
            table.Columns.Add(new Column(table, "DataLoadRunId", DataType.Int));
        }

        private void SetupDatasetRoot(string datasetRoot)
        {
            const string templateFolderZip = @"Resources\z_TEMPLATE_data_folder.zip";
            if (!Directory.Exists(datasetRoot))
                Directory.CreateDirectory(datasetRoot);

            ZipFile.ExtractToDirectory(templateFolderZip, datasetRoot);
        }

        [Test]
        [Ignore("Fix to get logging config from DB, see todo around line 402")]
        public void TestRunWithAttacher()
        {
            var defaultTestDbServer = Environment.MachineName;
            
            Database liveDb = null;
            Database stagingDb = null;
            Server server = null;
            string dbName = "";
            string datasetId = "";
            string datasetRoot = "";
            int dataLoadTaskID = 0;
            Catalogue catalogue = null;
            ProcessTask attacherProcess = null;
            ProcessTaskArgument filenameArg = null;
            ProcessTaskArgument databaseNameArg = null;
            ANOTable anoTable = null;
            
            const string mdfPath = @"Resources\Test_DB.mdf";
            const string ldfPath = @"Resources\Test_DB_log.ldf";

            try
            {
                // Set up test catalogue
                catalogue = new Catalogue(CatalogueRepository, "TestRunWithAttacher");

                // set up dataset directory
                datasetRoot = @"datasets\" + catalogue.Name;
                SetupDatasetRoot(datasetRoot);
                var hicProjectDirectory = new HICProjectDirectory(datasetRoot, false);

                // Set up LoadMetadata
                var loadMetadata = new LoadMetadata(CatalogueRepository)
                {
                    LocationOfFlatFiles = new FileInfo(datasetRoot).FullName,
                    AnonymisationEngineClass = "DataLoadEngine.PipelineComponent.Anonymisation.BasicAnonymisationEngine"
                };
                loadMetadata.SaveToDatabase();

                catalogue.LoadMetadata_ID = loadMetadata.ID;//associate it with the catalogue
                catalogue.SaveToDatabase();//save this assocation
                
                // Add attacher process
                attacherProcess = new ProcessTask(CatalogueRepository, loadMetadata, LoadStage.Mounting)
                {
                    Path = typeof (MDFAttacher).FullName,
                    Order = 0,
                    ProcessTaskType = ProcessTaskType.Attacher
                };
                attacherProcess.SaveToDatabase();

                filenameArg = new ProcessTaskArgument(CatalogueRepository, attacherProcess);
                filenameArg.SetType(typeof(string));
                filenameArg.Name = "Filepath";
                filenameArg.Value = Path.Combine(hicProjectDirectory.ForLoading.FullName, catalogue.Name + ".mdf");
                filenameArg.SaveToDatabase();

                databaseNameArg = new ProcessTaskArgument(CatalogueRepository, attacherProcess);
                databaseNameArg.Name = "DatabaseName";
                databaseNameArg.Value = catalogue.Name;
                databaseNameArg.SaveToDatabase();

                dbName = catalogue.Name;
                datasetId = dbName.Substring(0, 16);

                server = new Server(new ServerConnection(CatalogueRepository.ConnectionString));

                // make sure the database doesn't exist
                if (server.Databases.Contains(dbName))
                    throw new Exception("Database " + dbName + " already exists");

                if (server.Databases["ANO"] == null)
                    throw new Exception("You need an anonymisation database called ANO to run this test");

                // Check the ANO configuration table exists
                var anoDb = server.Databases["ANO"];
                var anoConfigTable = anoDb.Tables["Configuration"];
                if (anoConfigTable == null) throw new Exception("The ANO database does not have a Configuration table");
                    
                // Create the live database and table
                liveDb = new Database(server, dbName);
                liveDb.Create();

                var table = new Table(liveDb, "Data");
                table.Columns.Add(new Column(table, "ANOCHI", DataType.VarCharMax));
                table.Columns.Add(new Column(table, "Description", DataType.VarCharMax));
                AddStandardColumns(table);
                table.Create();

                // Create the staging database and table
                stagingDb = new Database(server, dbName + "_STAGING");
                stagingDb.Create();

                var stagingTable = new Table(stagingDb, "Data");
                stagingTable.Columns.Add(new Column(stagingTable, "ANOCHI", DataType.VarCharMax));
                stagingTable.Columns.Add(new Column(stagingTable, "Description", DataType.VarCharMax));
                stagingTable.Create();

                // import table info
                var columnInfos = ImportTableInfo(dbName);

                CreateCatalogueItems(columnInfos, catalogue);
                ConfigureColumnInfoPrimaryKeys(columnInfos);

                // Set up ANO config
                anoTable = new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, "ANOCHI","C");
                anoTable.NumberOfIntegersToUseInAnonymousRepresentation = 10;
                anoTable.PushToANOServerAsNewTable("varchar(10)",new ThrowImmediatelyCheckNotifier());
                
                // configure logging task
                dataLoadTaskID = CreateLoggingRecords(datasetId);

                catalogue.LoggingDataTask = datasetId;
                catalogue.SaveToDatabase();

                // Set up and run load process
                var info = catalogue.GetLoggingServer(true);

                var loadPipeline = MockRepository.GenerateStub<IDataLoadExecution>();
                var checker = MockRepository.GenerateStub<ICheckable>();

                // copy db files to preloading
                File.Copy(mdfPath, Path.Combine(hicProjectDirectory.ForLoading.FullName, catalogue.Name + ".mdf"));
                File.Copy(ldfPath, Path.Combine(hicProjectDirectory.ForLoading.FullName, catalogue.Name + "_log.ldf"));

                //loadProcess.Run();
            }
            finally
            {
                if (server != null)
                {

                    if (server.Name.Equals("CONSUS"))
                        throw new Exception("Never drop databases on CONSUS please");

                    server.KillDatabase(dbName);
                    
                    if (stagingDb != null)
                        server.KillDatabase(stagingDb.Name);

                    server.Databases["ANO"].Tables["ANOCHI"].Drop();
                }

                // clean up the catalogue (no delete cascades)
                if (catalogue != null)
                {
                    foreach (var ti in catalogue.GetTableInfoList(false))
                    {
                        foreach (var colInfo in ti.ColumnInfos)
                            colInfo.DeleteInDatabase();

                        ti.DeleteInDatabase();
                    }

                    foreach (var catItem in catalogue.CatalogueItems)
                        catItem.DeleteInDatabase();

                    if (filenameArg != null)
                        filenameArg.DeleteInDatabase();

                    if (databaseNameArg != null)
                        databaseNameArg.DeleteInDatabase();

                    if (attacherProcess != null)
                        attacherProcess.DeleteInDatabase();

                    if (anoTable != null)
                        anoTable.DeleteInDatabase();

                    catalogue.DeleteInDatabase();
                }

                CleanUpLogging(datasetId, dataLoadTaskID);

                Directory.Delete(datasetRoot, true);
            }
        }
        
        private void CleanUpLogging(string dataSetID, int dataLoadTaskID)
        {
            var loggingBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["LoggingConnection"].ConnectionString);

            using (var logConn = new SqlConnection(loggingBuilder.ConnectionString))
            {
                logConn.Open();

                var cmd = DatabaseCommandHelper.GetCommand(@"DELETE FROM DataLoadRun WHERE dataLoadTaskID = @dataLoadTaskID", logConn);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@dataLoadTaskID", cmd));
                cmd.Parameters["@dataLoadTaskID"].Value = dataLoadTaskID;
                cmd.ExecuteNonQuery();
                
                cmd = DatabaseCommandHelper.GetCommand(@"DELETE FROM DataLoadTask WHERE dataSetID = @dataSetID", logConn);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@dataSetID", cmd));
                cmd.Parameters["@dataSetID"].Value = dataSetID;
                cmd.ExecuteNonQuery();
                
                cmd = DatabaseCommandHelper.GetCommand(@"DELETE FROM DataSet WHERE dataSetID = @dataSetID", logConn);
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@dataSetID", cmd));
                cmd.Parameters["@dataSetID"].Value = dataSetID;
                cmd.ExecuteNonQuery();
            }
        }

        private void CreateCatalogueItems(ColumnInfo[] columnInfos, Catalogue catalogue)
        {
            // Create catalogue items from column infos
            foreach (ColumnInfo col in columnInfos)
            {
                var cataItem = new CatalogueItem(CatalogueRepository, catalogue, col.Name.Substring(col.Name.LastIndexOf(".") + 1).Trim('[', ']', '`'));
                //create it with the same name
                cataItem.SetColumnInfo(col);
            }
        }

        private void ConfigureColumnInfoPrimaryKeys(ColumnInfo[] columnInfos)
        {
            var pkColumn = columnInfos.First(info => info.GetRuntimeName() == "ANOCHI");
            if (pkColumn == null)
                throw new Exception("Primary key column not found in column infos");
            pkColumn.IsPrimaryKey = true;
            pkColumn.SaveToDatabase();
        }

        private ColumnInfo[] ImportTableInfo(string dbName)
        {
            var importer = new TableInfoImporter(CatalogueRepository, CatalogueRepository.DiscoveredServer.Name, dbName, "Data", DatabaseType.MicrosoftSQLServer);

            TableInfo tableInfo;
            ColumnInfo[] columnInfos;
            importer.DoImport(out tableInfo, out columnInfos);
            return columnInfos;
        }

        private int CreateLoggingRecords(string datasetId)
        {
            var loggingBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["LoggingConnection"].ConnectionString);

            using (var logConn = new SqlConnection(loggingBuilder.ConnectionString))
            {
                logConn.Open();

                var cmd =
                    DatabaseCommandHelper.GetCommand(
                        @"INSERT INTO DataSet (dataSetID, name, description) VALUES (@dataSetID, @name, @description)", logConn);

                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@dataSetID", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@name", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@description", cmd));

                cmd.Parameters["@dataSetID"].Value = datasetId;
                cmd.Parameters["@name"].Value = datasetId;
                cmd.Parameters["@description"].Value = "Dataset created by MultStageDataLoadProcessTests.Run";

                cmd.ExecuteNonQuery();

                cmd =
                    DatabaseCommandHelper.GetCommand(
                        @"INSERT INTO DataLoadTask (description, name, createTime, userAccount, statusID, isTest, dataSetID) VALUES (@description, @name, @createTime, @userAccount, @statusID, @isTest, @dataSetID); SELECT @@IDENTITY;",
                        logConn);

                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@description", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@name", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@createTime", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@userAccount", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@statusID", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@isTest", cmd));
                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@dataSetID", cmd));

                cmd.Parameters["@description"].Value = datasetId;
                cmd.Parameters["@name"].Value = datasetId;
                cmd.Parameters["@createTime"].Value = DateTime.Now;
                var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
                cmd.Parameters["@userAccount"].Value = windowsIdentity != null ? windowsIdentity.Name : "Unknown";
                cmd.Parameters["@statusID"].Value = 1;
                cmd.Parameters["@isTest"].Value = 1;
                cmd.Parameters["@dataSetID"].Value = datasetId;

                return int.Parse(cmd.ExecuteScalar().ToString());
            }
        }

        [Test]
        public void LoadProcessEndsCorrectlyWhenPipelineCrashes_Success()
        {
            /*
            var loadMetadata = MockRepository.GenerateStub<ILoadMetadata>();
            var listener = MockRepository.GenerateMock<IDataLoadEventListener>();
            var loadPipeline = MockRepository.GenerateStub<IDataLoadPipeline>();
            var checker = MockRepository.GenerateStub<ICheckable>();

            var loadProcess = new MultiStageDataLoadProcess(loadMetadata, listener, loadPipeline, checker);
            var token = new GracefulCancellationToken();

            // pre-crash the pipeline
            loadPipeline.HasCrashed = true;

            // keep track of the number of times where the listener has been notified with the crash message
            var numIterations = 0;
            listener.Expect(
                l => l.OnNotify(Arg<object>.Is.Anything,
                    Arg<NotifyEventArgs>.Matches(c => c.Message.Equals("Load pipeline reports that it has crashed"))))
                .WhenCalled(invocation =>
                {
                    numIterations++;
                });

            // run the process async so we can keep an eye on numIterations
            var task = Task.Run(() =>
            {
                RunDataLoad(token);
            });

            // check numIterations periodically while RunDataLoad is churning away
            while (!task.IsCompleted)
            {
                Task.Delay(1000);
                Assert.LessOrEqual(numIterations, 1, "Appears that RunDataLoad has gone into an infinite loop, crash has been reported " + numIterations + " times");
            }

            Assert.AreEqual(1, numIterations);
            */
        }
    }
}
