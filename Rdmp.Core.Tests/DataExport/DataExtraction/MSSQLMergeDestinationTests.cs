using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using SynthEHR;
using SynthEHR.Datasets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;
using Tests.Common.Scenarios;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataExport.DataExtraction
{

    internal class MSSqlMergeDestination_Test: MSSqlMergeDestination
    {
        public void Execute(DataTable dt) {
            Assert.DoesNotThrow(()=>WriteRows(dt, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken(),new System.Diagnostics.Stopwatch()));

        }
    }

    public class MSSqlMergeDestinationTests: TestsRequiringAnExtractionConfiguration
    {
        //create table first time
        [Test]
        public void MSSQLMerge_Creates_Table()
        {
            var destination = new MSSqlMergeDestination_Test();

            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Creates_Table";
            destination.DeleteMergeTempTable = true;
            destination.PreInitialize(null,new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("description");
            dt.PrimaryKey= new DataColumn[] { dt.Columns["chi"] };
            dt.Rows.Add("10", "one");
            destination.Execute(dt);
            destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet,null);
            var tbl = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(destination.DatabaseNamingPattern).ExpectTable(destination.TableNamingPattern);
            Assert.That(tbl.Exists());
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
        }
        //merge in new data
        [Test]
        public void MSSQLMerge_Merge_Data()
        {
            var destination = new MSSqlMergeDestination_Test();

            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Data";
            destination.DeleteMergeTempTable = true;
            destination.PreInitialize(null,new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("description");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["chi"] };
            dt.Rows.Add("10", "one");
            destination.Execute(dt);
            destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);

            var tbl = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(destination.DatabaseNamingPattern).ExpectTable(destination.TableNamingPattern);
            Assert.That(tbl.Exists());
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
            dt.Rows.Remove(dt.Rows[0]);
            dt.Rows.Add("2", "two");
            destination = new MSSqlMergeDestination_Test();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Data";
            destination.DeleteMergeTempTable = true;
            destination.PreInitialize(null, new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            destination.Execute(dt);
            destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);

            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(2));
            tbl.Drop();
        }
        // merge in data with dupicates
        [Test]
        public void MSSQLMerge_Merge_Update()
        {
            var destination = new MSSqlMergeDestination_Test();

            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Update";
            destination.DeleteMergeTempTable= true;
            destination.PreInitialize(null,new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("description");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["chi"] };
            dt.Rows.Add("10", "one");
            destination.Execute(dt);
            destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet,null);

            var tbl = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(destination.DatabaseNamingPattern).ExpectTable(destination.TableNamingPattern);
            Assert.That(tbl.Exists());
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
            dt.Rows.Remove(dt.Rows[0]);
            dt.Rows.Add("2", "two");
            destination = new MSSqlMergeDestination_Test();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Update";
            destination.DeleteMergeTempTable = true;
            destination.PreInitialize(null, new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            destination.Execute(dt);
            destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet,null);
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(2));
            dt.Rows.Add("10", "thr");
            destination = new MSSqlMergeDestination_Test();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Update";
            destination.DeleteMergeTempTable = true;
            destination.PreInitialize(null, new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            destination.Execute(dt);
            destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(2));
            Assert.That(tbl.GetDataTable().Rows[1].ItemArray, Is.EqualTo(new List<object>() { 10, "thr" }));
            Assert.That(tbl.GetDataTable().Rows[0].ItemArray, Is.EqualTo(new List<object>() { 2, "two" }));
            tbl.Drop();
        }
        //megre in with perform delete
        [Test]
        public void MSSQLMerge_Merge_Delete()
        {
            var destination = new MSSqlMergeDestination_Test();

            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Delete";
            destination.DeleteMergeTempTable = true;
            destination.AllowMergeToPerformDeletes = true;
            destination.PreInitialize(null,new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            var dt = new DataTable();
            dt.Columns.Add("chi");
            dt.Columns.Add("description");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["chi"] };
            dt.Rows.Add("10", "one");
            destination.Execute(dt);
            destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet,null);
            var tbl = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(destination.DatabaseNamingPattern).ExpectTable(destination.TableNamingPattern);
            Assert.That(tbl.Exists());
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
            dt.Rows.Remove(dt.Rows[0]);
            dt.Rows.Add("2", "two");
            destination = new MSSqlMergeDestination_Test();
            destination.TargetDatabaseServer = _extractionServer;
            destination.DatabaseNamingPattern = "MSSQLMerge_Creates_Table";
            destination.TableNamingPattern = "MSSQLMerge_Merge_Delete";
            destination.DeleteMergeTempTable = true;
            destination.AllowMergeToPerformDeletes = true;
            destination.PreInitialize(null, new Project(RepositoryLocator.DataExportRepository, "test project"), ThrowImmediatelyDataLoadEventListener.Quiet);
            destination.Execute(dt);
            destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet,null);
            Assert.That(tbl.GetDataTable().Rows.Count, Is.EqualTo(1));
            Assert.That(tbl.GetDataTable().Rows[0].ItemArray, Is.EqualTo(new List<object>() {2, "two" }));
            tbl.Drop();
        }


        private FileInfo CreateFileInForLoading(string filename, int rows, Random r)
        {
            var fi = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetFileName(filename)));

            var demog = new Demography(r);
            var people = new PersonCollection();
            people.GeneratePeople(500, r);

            demog.GenerateTestDataFile(people, fi, rows);

            return fi;
        }

        //add a column
        [Test]
        public void MSSQLMerge_SQLServerDestinationWithTriggersAddAColumn()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            //create catalogue from file
            var csvFile = CreateFileInForLoading("bob.csv", 1, new Random(5000));
            // Create the 'out of the box' RDMP pipelines (which includes an excel bulk importer pipeline)
            var creator = new CataloguePipelinesAndReferencesCreation(
                RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);

            // find the excel loading pipeline
            var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));

            if (pipe is null)
            {
                creator.CreatePipelines(new PlatformDatabaseCreationOptions { });
                pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
            }

            // run an import of the file using the pipeline
            var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(
                new ThrowImmediatelyActivator(RepositoryLocator),
                csvFile,
                null, db, pipe, null);

            cmd.Execute();
            var catalogue = CatalogueRepository.GetAllObjects<Catalogue>().FirstOrDefault(static c => c.Name == "bob");
            var chiColumnInfo = catalogue.CatalogueItems.First(static ci => ci.Name == "chi");
            var ei = chiColumnInfo.ExtractionInformation;
            ei.IsExtractionIdentifier = true;
            ei.IsPrimaryKey = true;
            ei.SaveToDatabase();
            var project = new Project(DataExportRepository, "MyProject")
            {
                ProjectNumber = 500,
                ExtractionDirectory = Path.GetTempPath()
            };
            project.SaveToDatabase();
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
            cic.CreateRootContainerIfNotExists();
            var agg1 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
            var conf = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
            conf.SaveToDatabase();
            agg1.SaveToDatabase();
            cic.RootCohortAggregateContainer.AddChild(agg1, 0);
            cic.SaveToDatabase();
            var dim = new AggregateDimension(CatalogueRepository, ei, agg1);
            dim.SaveToDatabase();
            agg1.SaveToDatabase();

            var CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
            var cohortTableName = "Cohort";
            var definitionTableName = "CohortDefinition";
            var ExternalCohortTableNameInCatalogue = "CohortTests";
            const string ReleaseIdentifierFieldName = "ReleaseId";
            const string DefinitionTableForeignKeyField = "cohortDefinition_id";
            var _cohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);
            if (_cohortDatabase.Exists())
                DeleteTables(_cohortDatabase);
            else
                _cohortDatabase.Create();

            var definitionTable = _cohortDatabase.CreateTable("CohortDefinition", new[]
               {
                new DatabaseColumnRequest("id", new DatabaseTypeRequest(typeof(int)))
                    { AllowNulls = false, IsAutoIncrement = true, IsPrimaryKey = true },
                new DatabaseColumnRequest("projectNumber", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("version", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), 3000))
                    { AllowNulls = false },
                new DatabaseColumnRequest("dtCreated", new DatabaseTypeRequest(typeof(DateTime)))
                    { AllowNulls = false, Default = MandatoryScalarFunctions.GetTodaysDate }
            });
            var idColumn = definitionTable.DiscoverColumn("id");
            var foreignKey =
                new DatabaseColumnRequest(DefinitionTableForeignKeyField, new DatabaseTypeRequest(typeof(int)), false)
                { IsPrimaryKey = true };

            _cohortDatabase.CreateTable("Cohort", new[]
            {
                    new DatabaseColumnRequest("chi",
                        new DatabaseTypeRequest(typeof(string)), false)
                    {
                        IsPrimaryKey = true,

                        // if there is a single collation amongst private identifier prototype references we must use that collation
                        // when creating the private column so that the DBMS can link them no bother
                        Collation = null
                    },
                    new DatabaseColumnRequest(ReleaseIdentifierFieldName, new DatabaseTypeRequest(typeof(string), 300))
                        { AllowNulls = true },
                    foreignKey
                });

            var newExternal =
                        new ExternalCohortTable(DataExportRepository, "TestExternalCohort", DatabaseType.MicrosoftSQLServer)
                        {
                            Database = CohortDatabaseName,
                            Server = _cohortDatabase.Server.Name,
                            DefinitionTableName = definitionTableName,
                            TableName = cohortTableName,
                            Name = ExternalCohortTableNameInCatalogue,
                            Username = _cohortDatabase.Server.ExplicitUsernameIfAny,
                            Password = _cohortDatabase.Server.ExplicitPasswordIfAny,
                            PrivateIdentifierField = "chi",
                            ReleaseIdentifierField = "ReleaseId",
                            DefinitionTableForeignKeyField = "cohortDefinition_id"
                        };

            newExternal.SaveToDatabase();
            var cohortPipeline = CatalogueRepository.GetAllObjects<Pipeline>().First(static p => p.Name == "CREATE COHORT:By Executing Cohort Identification Configuration");
            var newCohortCmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(
                new ThrowImmediatelyActivator(RepositoryLocator),
                cic,
                newExternal,
                "MyCohort",
                project,
                cohortPipeline
            );
            newCohortCmd.Execute();
            var extractableCohort = new ExtractableCohort(DataExportRepository, newExternal, 1);

            var ec = new ExtractionConfiguration(DataExportRepository, project)
            {
                Name = "ext1",
                Cohort_ID = extractableCohort.ID
            };
            var eds = new ExtractableDataSet(DataExportRepository, catalogue);
            ec.AddDatasetToConfiguration(eds);
            var cols = ec.GetAllExtractableColumnsFor(eds);
            var col = cols.First(c => c.SelectSQL.Contains("current_record"));
            var order = col.Order;
            var selectSQL = col.SelectSQL;
            var cei = col.CatalogueExtractionInformation;
            col.DeleteInDatabase();
            cols = ec.GetAllExtractableColumnsFor(eds);
            ec.SaveToDatabase();
            var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 4");
            var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(MSSqlMergeDestination), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<MSSqlMergeDestination>()
                .ToList();
            var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
            var argumentUseArchiveTrigger = destinationArguments.Single(a => a.Name == "UseArchiveTrigger");
            var DeleteMergeTempTable = destinationArguments.Single(a => a.Name == "DeleteMergeTempTable");
            
            ////var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
            Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();

            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            argumentUseArchiveTrigger.SetValue(true);
            argumentUseArchiveTrigger.SaveToDatabase();
            DeleteMergeTempTable.SetValue(false);
            DeleteMergeTempTable.SaveToDatabase();
            //reExtract.SetValue(true);
            //reExtract.SaveToDatabase();

            var component2 = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
                .ToArray();
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            extractionPipeline.DestinationPipelineComponent_ID = component.ID;
            extractionPipeline.SourcePipelineComponent_ID = component2.ID;
            extractionPipeline.SaveToDatabase();


            var dbname = TestDatabaseNames.GetConsistentName($"{project.Name}_{project.ProjectNumber}");
            var dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();
            dbToExtractTo.Create();
            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            var returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");



            var destinationTable = dbToExtractTo.ExpectTable("ext1_bob");
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));
            var hicLoadID = dt.Rows[0].ItemArray[37];

            var archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            var archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(0));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(42));
            ec.RemoveDatasetFromConfiguration(eds);
            var nec = new ExtractableColumn(DataExportRepository,eds,ec,col.CatalogueExtractionInformation,0,col.SelectSQL);
            nec.SaveToDatabase();
            ec.AddDatasetToConfiguration(eds);
            cols = ec.GetAllExtractableColumnsFor(eds);
            ec.SaveToDatabase();
            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(40));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(1));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
        }
        //remove a column
        [Test]
        public void MSSQLMerge_SQLServerDestinationWithTriggersRemoveAColumn()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            //create catalogue from file
            var csvFile = CreateFileInForLoading("bob.csv", 1, new Random(5000));
            // Create the 'out of the box' RDMP pipelines (which includes an excel bulk importer pipeline)
            var creator = new CataloguePipelinesAndReferencesCreation(
                RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);

            // find the excel loading pipeline
            var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));

            if (pipe is null)
            {
                creator.CreatePipelines(new PlatformDatabaseCreationOptions { });
                pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
            }

            // run an import of the file using the pipeline
            var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(
                new ThrowImmediatelyActivator(RepositoryLocator),
                csvFile,
                null, db, pipe, null);

            cmd.Execute();
            var catalogue = CatalogueRepository.GetAllObjects<Catalogue>().FirstOrDefault(static c => c.Name == "bob");
            var chiColumnInfo = catalogue.CatalogueItems.First(static ci => ci.Name == "chi");
            var ei = chiColumnInfo.ExtractionInformation;
            ei.IsExtractionIdentifier = true;
            ei.IsPrimaryKey = true;
            ei.SaveToDatabase();
            var project = new Project(DataExportRepository, "MyProject")
            {
                ProjectNumber = 500,
                ExtractionDirectory = Path.GetTempPath()
            };
            project.SaveToDatabase();
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
            cic.CreateRootContainerIfNotExists();
            var agg1 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
            var conf = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
            conf.SaveToDatabase();
            agg1.SaveToDatabase();
            cic.RootCohortAggregateContainer.AddChild(agg1, 0);
            cic.SaveToDatabase();
            var dim = new AggregateDimension(CatalogueRepository, ei, agg1);
            dim.SaveToDatabase();
            agg1.SaveToDatabase();

            var CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
            var cohortTableName = "Cohort";
            var definitionTableName = "CohortDefinition";
            var ExternalCohortTableNameInCatalogue = "CohortTests";
            const string ReleaseIdentifierFieldName = "ReleaseId";
            const string DefinitionTableForeignKeyField = "cohortDefinition_id";
            var _cohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);
            if (_cohortDatabase.Exists())
                DeleteTables(_cohortDatabase);
            else
                _cohortDatabase.Create();

            var definitionTable = _cohortDatabase.CreateTable("CohortDefinition", new[]
               {
                new DatabaseColumnRequest("id", new DatabaseTypeRequest(typeof(int)))
                    { AllowNulls = false, IsAutoIncrement = true, IsPrimaryKey = true },
                new DatabaseColumnRequest("projectNumber", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("version", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), 3000))
                    { AllowNulls = false },
                new DatabaseColumnRequest("dtCreated", new DatabaseTypeRequest(typeof(DateTime)))
                    { AllowNulls = false, Default = MandatoryScalarFunctions.GetTodaysDate }
            });
            var idColumn = definitionTable.DiscoverColumn("id");
            var foreignKey =
                new DatabaseColumnRequest(DefinitionTableForeignKeyField, new DatabaseTypeRequest(typeof(int)), false)
                { IsPrimaryKey = true };

            _cohortDatabase.CreateTable("Cohort", new[]
            {
                    new DatabaseColumnRequest("chi",
                        new DatabaseTypeRequest(typeof(string)), false)
                    {
                        IsPrimaryKey = true,

                        // if there is a single collation amongst private identifier prototype references we must use that collation
                        // when creating the private column so that the DBMS can link them no bother
                        Collation = null
                    },
                    new DatabaseColumnRequest(ReleaseIdentifierFieldName, new DatabaseTypeRequest(typeof(string), 300))
                        { AllowNulls = true },
                    foreignKey
                });

            var newExternal =
                        new ExternalCohortTable(DataExportRepository, "TestExternalCohort", DatabaseType.MicrosoftSQLServer)
                        {
                            Database = CohortDatabaseName,
                            Server = _cohortDatabase.Server.Name,
                            DefinitionTableName = definitionTableName,
                            TableName = cohortTableName,
                            Name = ExternalCohortTableNameInCatalogue,
                            Username = _cohortDatabase.Server.ExplicitUsernameIfAny,
                            Password = _cohortDatabase.Server.ExplicitPasswordIfAny,
                            PrivateIdentifierField = "chi",
                            ReleaseIdentifierField = "ReleaseId",
                            DefinitionTableForeignKeyField = "cohortDefinition_id"
                        };

            newExternal.SaveToDatabase();
            var cohortPipeline = CatalogueRepository.GetAllObjects<Pipeline>().First(static p => p.Name == "CREATE COHORT:By Executing Cohort Identification Configuration");
            var newCohortCmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(
                new ThrowImmediatelyActivator(RepositoryLocator),
                cic,
                newExternal,
                "MyCohort",
                project,
                cohortPipeline
            );
            newCohortCmd.Execute();
            var extractableCohort = new ExtractableCohort(DataExportRepository, newExternal, 1);

            var ec = new ExtractionConfiguration(DataExportRepository, project)
            {
                Name = "ext1",
                Cohort_ID = extractableCohort.ID
            };
            var eds = new ExtractableDataSet(DataExportRepository, catalogue);
            ec.AddDatasetToConfiguration(eds);
            ec.SaveToDatabase();
            var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 5");
            var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(MSSqlMergeDestination), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<MSSqlMergeDestination>()
                .ToList();
            var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
            var argumentUseArchiveTrigger = destinationArguments.Single(a => a.Name == "UseArchiveTrigger");
            //var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
            Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();

            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            argumentUseArchiveTrigger.SetValue(true);
            argumentUseArchiveTrigger.SaveToDatabase();
            //reExtract.SetValue(true);
            //reExtract.SaveToDatabase();

            var component2 = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
                .ToArray();
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            extractionPipeline.DestinationPipelineComponent_ID = component.ID;
            extractionPipeline.SourcePipelineComponent_ID = component2.ID;
            extractionPipeline.SaveToDatabase();


            var dbname = TestDatabaseNames.GetConsistentName($"{project.Name}_{project.ProjectNumber}");
            var dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();
            dbToExtractTo.Create();
            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            var returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");



            var destinationTable = dbToExtractTo.ExpectTable("ext1_bob");
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(40));
            var hicLoadID = dt.Rows[0].ItemArray[37];

            var archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            var archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(0));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);


            var cols = ec.GetAllExtractableColumnsFor(eds);
            var col = cols.First(c => c.SelectSQL.Contains("current_record"));
            var order = col.Order;
            var selectSQL = col.SelectSQL;
            var cei = col.CatalogueExtractionInformation;
            col.DeleteInDatabase();

            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(1));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
        }
        //remove a column, add a different column
        [Test]
        public void MSSQLMerge_SQLServerDestinationWithTriggersRemoveAColumnAddAColumn()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            //create catalogue from file
            var csvFile = CreateFileInForLoading("bob.csv", 1, new Random(5000));
            // Create the 'out of the box' RDMP pipelines (which includes an excel bulk importer pipeline)
            var creator = new CataloguePipelinesAndReferencesCreation(
                RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);

            // find the excel loading pipeline
            var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));

            if (pipe is null)
            {
                creator.CreatePipelines(new PlatformDatabaseCreationOptions { });
                pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
            }

            // run an import of the file using the pipeline
            var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(
                new ThrowImmediatelyActivator(RepositoryLocator),
                csvFile,
                null, db, pipe, null);

            cmd.Execute();
            var catalogue = CatalogueRepository.GetAllObjects<Catalogue>().FirstOrDefault(static c => c.Name == "bob");
            var chiColumnInfo = catalogue.CatalogueItems.First(static ci => ci.Name == "chi");
            var ei = chiColumnInfo.ExtractionInformation;
            ei.IsExtractionIdentifier = true;
            ei.IsPrimaryKey = true;
            ei.SaveToDatabase();
            var project = new Project(DataExportRepository, "MyProject")
            {
                ProjectNumber = 500,
                ExtractionDirectory = Path.GetTempPath()
            };
            project.SaveToDatabase();
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
            cic.CreateRootContainerIfNotExists();
            var agg1 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
            var conf = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
            conf.SaveToDatabase();
            agg1.SaveToDatabase();
            cic.RootCohortAggregateContainer.AddChild(agg1, 0);
            cic.SaveToDatabase();
            var dim = new AggregateDimension(CatalogueRepository, ei, agg1);
            dim.SaveToDatabase();
            agg1.SaveToDatabase();

            var CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
            var cohortTableName = "Cohort";
            var definitionTableName = "CohortDefinition";
            var ExternalCohortTableNameInCatalogue = "CohortTests";
            const string ReleaseIdentifierFieldName = "ReleaseId";
            const string DefinitionTableForeignKeyField = "cohortDefinition_id";
            var _cohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);
            if (_cohortDatabase.Exists())
                DeleteTables(_cohortDatabase);
            else
                _cohortDatabase.Create();

            var definitionTable = _cohortDatabase.CreateTable("CohortDefinition", new[]
               {
                new DatabaseColumnRequest("id", new DatabaseTypeRequest(typeof(int)))
                    { AllowNulls = false, IsAutoIncrement = true, IsPrimaryKey = true },
                new DatabaseColumnRequest("projectNumber", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("version", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), 3000))
                    { AllowNulls = false },
                new DatabaseColumnRequest("dtCreated", new DatabaseTypeRequest(typeof(DateTime)))
                    { AllowNulls = false, Default = MandatoryScalarFunctions.GetTodaysDate }
            });
            var idColumn = definitionTable.DiscoverColumn("id");
            var foreignKey =
                new DatabaseColumnRequest(DefinitionTableForeignKeyField, new DatabaseTypeRequest(typeof(int)), false)
                { IsPrimaryKey = true };

            _cohortDatabase.CreateTable("Cohort", new[]
            {
                    new DatabaseColumnRequest("chi",
                        new DatabaseTypeRequest(typeof(string)), false)
                    {
                        IsPrimaryKey = true,

                        // if there is a single collation amongst private identifier prototype references we must use that collation
                        // when creating the private column so that the DBMS can link them no bother
                        Collation = null
                    },
                    new DatabaseColumnRequest(ReleaseIdentifierFieldName, new DatabaseTypeRequest(typeof(string), 300))
                        { AllowNulls = true },
                    foreignKey
                });

            var newExternal =
                        new ExternalCohortTable(DataExportRepository, "TestExternalCohort", DatabaseType.MicrosoftSQLServer)
                        {
                            Database = CohortDatabaseName,
                            Server = _cohortDatabase.Server.Name,
                            DefinitionTableName = definitionTableName,
                            TableName = cohortTableName,
                            Name = ExternalCohortTableNameInCatalogue,
                            Username = _cohortDatabase.Server.ExplicitUsernameIfAny,
                            Password = _cohortDatabase.Server.ExplicitPasswordIfAny,
                            PrivateIdentifierField = "chi",
                            ReleaseIdentifierField = "ReleaseId",
                            DefinitionTableForeignKeyField = "cohortDefinition_id"
                        };

            newExternal.SaveToDatabase();
            var cohortPipeline = CatalogueRepository.GetAllObjects<Pipeline>().First(static p => p.Name == "CREATE COHORT:By Executing Cohort Identification Configuration");
            var newCohortCmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(
                new ThrowImmediatelyActivator(RepositoryLocator),
                cic,
                newExternal,
                "MyCohort",
                project,
                cohortPipeline
            );
            newCohortCmd.Execute();
            var extractableCohort = new ExtractableCohort(DataExportRepository, newExternal, 1);

            var ec = new ExtractionConfiguration(DataExportRepository, project)
            {
                Name = "ext1",
                Cohort_ID = extractableCohort.ID
            };
            var eds = new ExtractableDataSet(DataExportRepository, catalogue);
            ec.AddDatasetToConfiguration(eds);
            ec.SaveToDatabase();
            var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 6");
            var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(MSSqlMergeDestination), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<MSSqlMergeDestination>()
                .ToList();
            var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
            var argumentUseArchiveTrigger = destinationArguments.Single(a => a.Name == "UseArchiveTrigger");
            //var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
            Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();

            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            argumentUseArchiveTrigger.SetValue(true);
            argumentUseArchiveTrigger.SaveToDatabase();
            //reExtract.SetValue(true);
            //reExtract.SaveToDatabase();

            var component2 = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
                .ToArray();
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            extractionPipeline.DestinationPipelineComponent_ID = component.ID;
            extractionPipeline.SourcePipelineComponent_ID = component2.ID;
            extractionPipeline.SaveToDatabase();


            var dbname = TestDatabaseNames.GetConsistentName($"{project.Name}_{project.ProjectNumber}");
            var dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();
            dbToExtractTo.Create();


            var cols_1 = ec.GetAllExtractableColumnsFor(eds);
            var col_1 = cols_1.First(c => c.SelectSQL.Contains("current_address_L3"));
            var order = col_1.Order;
            var selectSQL = col_1.SelectSQL;
            var cei = col_1.CatalogueExtractionInformation;
            col_1.DeleteInDatabase();

            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            var returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");



            var destinationTable = dbToExtractTo.ExpectTable("ext1_bob");
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));
            var hicLoadID = dt.Rows[0].ItemArray[37];

            var archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            var archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(0));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(42));
            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);


            var cols = ec.GetAllExtractableColumnsFor(eds);
            cols.First(c => c.SelectSQL.Contains("current_record")).DeleteInDatabase();
            cols.First(c => c.SelectSQL.Contains("current_address_L3")).DeleteInDatabase();
            //col.DeleteInDatabase();

            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(38));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(1));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(42));

            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);


            cols = ec.GetAllExtractableColumnsFor(eds);
            cols.First(c => c.SelectSQL.Contains("current_record")).DeleteInDatabase();
            //cols.First(c => c.SelectSQL.Contains("current_address_L3")).DeleteInDatabase();
            //col.DeleteInDatabase();
            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(1));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
        }
        //remove a column, add the column back
        [Test]
        public void MSSQLMerge_SQLServerDestinationWithTriggersRemoveAColumnAddItBack()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            //create catalogue from file
            var csvFile = CreateFileInForLoading("bob.csv", 1, new Random(5000));
            // Create the 'out of the box' RDMP pipelines (which includes an excel bulk importer pipeline)
            var creator = new CataloguePipelinesAndReferencesCreation(
                RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);

            // find the excel loading pipeline
            var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));

            if (pipe is null)
            {
                creator.CreatePipelines(new PlatformDatabaseCreationOptions { });
                pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
            }

            // run an import of the file using the pipeline
            var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(
                new ThrowImmediatelyActivator(RepositoryLocator),
                csvFile,
                null, db, pipe, null);

            cmd.Execute();
            var catalogue = CatalogueRepository.GetAllObjects<Catalogue>().FirstOrDefault(static c => c.Name == "bob");
            var chiColumnInfo = catalogue.CatalogueItems.First(static ci => ci.Name == "chi");
            var ei = chiColumnInfo.ExtractionInformation;
            ei.IsExtractionIdentifier = true;
            ei.IsPrimaryKey = true;
            ei.SaveToDatabase();
            var project = new Project(DataExportRepository, "MyProject")
            {
                ProjectNumber = 500,
                ExtractionDirectory = Path.GetTempPath()
            };
            project.SaveToDatabase();
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
            cic.CreateRootContainerIfNotExists();
            var agg1 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
            var conf = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
            conf.SaveToDatabase();
            agg1.SaveToDatabase();
            cic.RootCohortAggregateContainer.AddChild(agg1, 0);
            cic.SaveToDatabase();
            var dim = new AggregateDimension(CatalogueRepository, ei, agg1);
            dim.SaveToDatabase();
            agg1.SaveToDatabase();

            var CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
            var cohortTableName = "Cohort";
            var definitionTableName = "CohortDefinition";
            var ExternalCohortTableNameInCatalogue = "CohortTests";
            const string ReleaseIdentifierFieldName = "ReleaseId";
            const string DefinitionTableForeignKeyField = "cohortDefinition_id";
            var _cohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);
            if (_cohortDatabase.Exists())
                DeleteTables(_cohortDatabase);
            else
                _cohortDatabase.Create();

            var definitionTable = _cohortDatabase.CreateTable("CohortDefinition", new[]
               {
                new DatabaseColumnRequest("id", new DatabaseTypeRequest(typeof(int)))
                    { AllowNulls = false, IsAutoIncrement = true, IsPrimaryKey = true },
                new DatabaseColumnRequest("projectNumber", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("version", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), 3000))
                    { AllowNulls = false },
                new DatabaseColumnRequest("dtCreated", new DatabaseTypeRequest(typeof(DateTime)))
                    { AllowNulls = false, Default = MandatoryScalarFunctions.GetTodaysDate }
            });
            var idColumn = definitionTable.DiscoverColumn("id");
            var foreignKey =
                new DatabaseColumnRequest(DefinitionTableForeignKeyField, new DatabaseTypeRequest(typeof(int)), false)
                { IsPrimaryKey = true };

            _cohortDatabase.CreateTable("Cohort", new[]
            {
                    new DatabaseColumnRequest("chi",
                        new DatabaseTypeRequest(typeof(string)), false)
                    {
                        IsPrimaryKey = true,

                        // if there is a single collation amongst private identifier prototype references we must use that collation
                        // when creating the private column so that the DBMS can link them no bother
                        Collation = null
                    },
                    new DatabaseColumnRequest(ReleaseIdentifierFieldName, new DatabaseTypeRequest(typeof(string), 300))
                        { AllowNulls = true },
                    foreignKey
                });

            var newExternal =
                        new ExternalCohortTable(DataExportRepository, "TestExternalCohort", DatabaseType.MicrosoftSQLServer)
                        {
                            Database = CohortDatabaseName,
                            Server = _cohortDatabase.Server.Name,
                            DefinitionTableName = definitionTableName,
                            TableName = cohortTableName,
                            Name = ExternalCohortTableNameInCatalogue,
                            Username = _cohortDatabase.Server.ExplicitUsernameIfAny,
                            Password = _cohortDatabase.Server.ExplicitPasswordIfAny,
                            PrivateIdentifierField = "chi",
                            ReleaseIdentifierField = "ReleaseId",
                            DefinitionTableForeignKeyField = "cohortDefinition_id"
                        };

            newExternal.SaveToDatabase();
            var cohortPipeline = CatalogueRepository.GetAllObjects<Pipeline>().First(static p => p.Name == "CREATE COHORT:By Executing Cohort Identification Configuration");
            var newCohortCmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(
                new ThrowImmediatelyActivator(RepositoryLocator),
                cic,
                newExternal,
                "MyCohort",
                project,
                cohortPipeline
            );
            newCohortCmd.Execute();
            var extractableCohort = new ExtractableCohort(DataExportRepository, newExternal, 1);

            var ec = new ExtractionConfiguration(DataExportRepository, project)
            {
                Name = "ext1",
                Cohort_ID = extractableCohort.ID
            };
            var eds = new ExtractableDataSet(DataExportRepository, catalogue);
            ec.AddDatasetToConfiguration(eds);
            ec.SaveToDatabase();
            var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 7");
            var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(MSSqlMergeDestination), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<MSSqlMergeDestination>()
                .ToList();
            var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
            var argumentUseArchiveTrigger = destinationArguments.Single(a => a.Name == "UseArchiveTrigger");
            //var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
            Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();

            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            argumentUseArchiveTrigger.SetValue(true);
            argumentUseArchiveTrigger.SaveToDatabase();
            //reExtract.SetValue(true);
            //reExtract.SaveToDatabase();

            var component2 = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
                .ToArray();
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            extractionPipeline.DestinationPipelineComponent_ID = component.ID;
            extractionPipeline.SourcePipelineComponent_ID = component2.ID;
            extractionPipeline.SaveToDatabase();


            var dbname = TestDatabaseNames.GetConsistentName($"{project.Name}_{project.ProjectNumber}");
            var dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();
            dbToExtractTo.Create();


            var cols_1 = ec.GetAllExtractableColumnsFor(eds);
            var col_1 = cols_1.First(c => c.SelectSQL.Contains("current_address_L3"));
            var order = col_1.Order;
            var selectSQL = col_1.SelectSQL;
            var cei = col_1.CatalogueExtractionInformation;
            //col_1.DeleteInDatabase();

            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            var returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");



            var destinationTable = dbToExtractTo.ExpectTable("ext1_bob");
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(40));
            var hicLoadID = dt.Rows[0].ItemArray[37];

            var archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            var archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(0));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);


            var cols = ec.GetAllExtractableColumnsFor(eds);
            cols.First(c => c.SelectSQL.Contains("current_record")).DeleteInDatabase();
            //cols.First(c => c.SelectSQL.Contains("current_address_L3")).DeleteInDatabase();
            //col.DeleteInDatabase();

            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(1));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));

            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);


            cols = ec.GetAllExtractableColumnsFor(eds);
            //cols.First(c => c.SelectSQL.Contains("current_record")).DeleteInDatabase();
            //cols.First(c => c.SelectSQL.Contains("current_address_L3")).DeleteInDatabase();
            //col.DeleteInDatabase();
            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(40));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(2));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
        }
        //add a column, remove a different column 
        [Test]
        public void MSSQLMerge_SQLServerDestinationWithTriggersAddAColumnThenRemoveADifferentOne()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            //create catalogue from file
            var csvFile = CreateFileInForLoading("bob.csv", 1, new Random(5000));
            // Create the 'out of the box' RDMP pipelines (which includes an excel bulk importer pipeline)
            var creator = new CataloguePipelinesAndReferencesCreation(
                RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);

            // find the excel loading pipeline
            var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));

            if (pipe is null)
            {
                creator.CreatePipelines(new PlatformDatabaseCreationOptions { });
                pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
            }

            // run an import of the file using the pipeline
            var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(
                new ThrowImmediatelyActivator(RepositoryLocator),
                csvFile,
                null, db, pipe, null);

            cmd.Execute();
            var catalogue = CatalogueRepository.GetAllObjects<Catalogue>().FirstOrDefault(static c => c.Name == "bob");
            var chiColumnInfo = catalogue.CatalogueItems.First(static ci => ci.Name == "chi");
            var ei = chiColumnInfo.ExtractionInformation;
            ei.IsExtractionIdentifier = true;
            ei.IsPrimaryKey = true;
            ei.SaveToDatabase();
            var project = new Project(DataExportRepository, "MyProject")
            {
                ProjectNumber = 500,
                ExtractionDirectory = Path.GetTempPath()
            };
            project.SaveToDatabase();
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
            cic.CreateRootContainerIfNotExists();
            var agg1 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
            var conf = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
            conf.SaveToDatabase();
            agg1.SaveToDatabase();
            cic.RootCohortAggregateContainer.AddChild(agg1, 0);
            cic.SaveToDatabase();
            var dim = new AggregateDimension(CatalogueRepository, ei, agg1);
            dim.SaveToDatabase();
            agg1.SaveToDatabase();

            var CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
            var cohortTableName = "Cohort";
            var definitionTableName = "CohortDefinition";
            var ExternalCohortTableNameInCatalogue = "CohortTests";
            const string ReleaseIdentifierFieldName = "ReleaseId";
            const string DefinitionTableForeignKeyField = "cohortDefinition_id";
            var _cohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);
            if (_cohortDatabase.Exists())
                DeleteTables(_cohortDatabase);
            else
                _cohortDatabase.Create();

            var definitionTable = _cohortDatabase.CreateTable("CohortDefinition", new[]
               {
                new DatabaseColumnRequest("id", new DatabaseTypeRequest(typeof(int)))
                    { AllowNulls = false, IsAutoIncrement = true, IsPrimaryKey = true },
                new DatabaseColumnRequest("projectNumber", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("version", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), 3000))
                    { AllowNulls = false },
                new DatabaseColumnRequest("dtCreated", new DatabaseTypeRequest(typeof(DateTime)))
                    { AllowNulls = false, Default = MandatoryScalarFunctions.GetTodaysDate }
            });
            var idColumn = definitionTable.DiscoverColumn("id");
            var foreignKey =
                new DatabaseColumnRequest(DefinitionTableForeignKeyField, new DatabaseTypeRequest(typeof(int)), false)
                { IsPrimaryKey = true };

            _cohortDatabase.CreateTable("Cohort", new[]
            {
                    new DatabaseColumnRequest("chi",
                        new DatabaseTypeRequest(typeof(string)), false)
                    {
                        IsPrimaryKey = true,

                        // if there is a single collation amongst private identifier prototype references we must use that collation
                        // when creating the private column so that the DBMS can link them no bother
                        Collation = null
                    },
                    new DatabaseColumnRequest(ReleaseIdentifierFieldName, new DatabaseTypeRequest(typeof(string), 300))
                        { AllowNulls = true },
                    foreignKey
                });

            var newExternal =
                        new ExternalCohortTable(DataExportRepository, "TestExternalCohort", DatabaseType.MicrosoftSQLServer)
                        {
                            Database = CohortDatabaseName,
                            Server = _cohortDatabase.Server.Name,
                            DefinitionTableName = definitionTableName,
                            TableName = cohortTableName,
                            Name = ExternalCohortTableNameInCatalogue,
                            Username = _cohortDatabase.Server.ExplicitUsernameIfAny,
                            Password = _cohortDatabase.Server.ExplicitPasswordIfAny,
                            PrivateIdentifierField = "chi",
                            ReleaseIdentifierField = "ReleaseId",
                            DefinitionTableForeignKeyField = "cohortDefinition_id"
                        };

            newExternal.SaveToDatabase();
            var cohortPipeline = CatalogueRepository.GetAllObjects<Pipeline>().First(static p => p.Name == "CREATE COHORT:By Executing Cohort Identification Configuration");
            var newCohortCmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(
                new ThrowImmediatelyActivator(RepositoryLocator),
                cic,
                newExternal,
                "MyCohort",
                project,
                cohortPipeline
            );
            newCohortCmd.Execute();
            var extractableCohort = new ExtractableCohort(DataExportRepository, newExternal, 1);

            var ec = new ExtractionConfiguration(DataExportRepository, project)
            {
                Name = "ext1",
                Cohort_ID = extractableCohort.ID
            };
            var eds = new ExtractableDataSet(DataExportRepository, catalogue);
            ec.AddDatasetToConfiguration(eds);
            var cols = ec.GetAllExtractableColumnsFor(eds);
            var col = cols.First(c => c.SelectSQL.Contains("current_record"));
            var order = col.Order;
            var selectSQL = col.SelectSQL;
            var cei = col.CatalogueExtractionInformation;
            col.DeleteInDatabase();
            cols = ec.GetAllExtractableColumnsFor(eds);
            ec.SaveToDatabase();
            var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 8");
            var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(MSSqlMergeDestination), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<MSSqlMergeDestination>()
                .ToList();
            var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
            var argumentUseArchiveTrigger = destinationArguments.Single(a => a.Name == "UseArchiveTrigger");
            //var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
            Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();

            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            argumentUseArchiveTrigger.SetValue(true);
            argumentUseArchiveTrigger.SaveToDatabase();
            //reExtract.SetValue(true);
            //reExtract.SaveToDatabase();

            var component2 = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
                .ToArray();
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            extractionPipeline.DestinationPipelineComponent_ID = component.ID;
            extractionPipeline.SourcePipelineComponent_ID = component2.ID;
            extractionPipeline.SaveToDatabase();


            var dbname = TestDatabaseNames.GetConsistentName($"{project.Name}_{project.ProjectNumber}");
            var dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();
            dbToExtractTo.Create();
            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            var returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");



            var destinationTable = dbToExtractTo.ExpectTable("ext1_bob");
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));
            var hicLoadID = dt.Rows[0].ItemArray[37];

            var archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            var archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(0));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(42));
            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);

            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(40));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(1));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));

            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);

            //current_address_L3
            cols = ec.GetAllExtractableColumnsFor(eds);
            cols.First(c => c.SelectSQL.Contains("current_address_L3")).DeleteInDatabase();
            cols = ec.GetAllExtractableColumnsFor(eds);
            ec.SaveToDatabase();
            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(2));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
        }
        //add a column, remove the same column 
        [Test]
        public void MSSQLMerge_SQLServerDestinationWithTriggersAddAColumnThenRemoveTheSameOne()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            //create catalogue from file
            var csvFile = CreateFileInForLoading("bob.csv", 1, new Random(5000));
            // Create the 'out of the box' RDMP pipelines (which includes an excel bulk importer pipeline)
            var creator = new CataloguePipelinesAndReferencesCreation(
                RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);

            // find the excel loading pipeline
            var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));

            if (pipe is null)
            {
                creator.CreatePipelines(new PlatformDatabaseCreationOptions { });
                pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
            }

            // run an import of the file using the pipeline
            var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(
                new ThrowImmediatelyActivator(RepositoryLocator),
                csvFile,
                null, db, pipe, null);

            cmd.Execute();
            var catalogue = CatalogueRepository.GetAllObjects<Catalogue>().FirstOrDefault(static c => c.Name == "bob");
            var chiColumnInfo = catalogue.CatalogueItems.First(static ci => ci.Name == "chi");
            var ei = chiColumnInfo.ExtractionInformation;
            ei.IsExtractionIdentifier = true;
            ei.IsPrimaryKey = true;
            ei.SaveToDatabase();
            var project = new Project(DataExportRepository, "MyProject")
            {
                ProjectNumber = 500,
                ExtractionDirectory = Path.GetTempPath()
            };
            project.SaveToDatabase();
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
            cic.CreateRootContainerIfNotExists();
            var agg1 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
            var conf = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
            conf.SaveToDatabase();
            agg1.SaveToDatabase();
            cic.RootCohortAggregateContainer.AddChild(agg1, 0);
            cic.SaveToDatabase();
            var dim = new AggregateDimension(CatalogueRepository, ei, agg1);
            dim.SaveToDatabase();
            agg1.SaveToDatabase();

            var CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
            var cohortTableName = "Cohort";
            var definitionTableName = "CohortDefinition";
            var ExternalCohortTableNameInCatalogue = "CohortTests";
            const string ReleaseIdentifierFieldName = "ReleaseId";
            const string DefinitionTableForeignKeyField = "cohortDefinition_id";
            var _cohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);
            if (_cohortDatabase.Exists())
                DeleteTables(_cohortDatabase);
            else
                _cohortDatabase.Create();

            var definitionTable = _cohortDatabase.CreateTable("CohortDefinition", new[]
               {
                new DatabaseColumnRequest("id", new DatabaseTypeRequest(typeof(int)))
                    { AllowNulls = false, IsAutoIncrement = true, IsPrimaryKey = true },
                new DatabaseColumnRequest("projectNumber", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("version", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), 3000))
                    { AllowNulls = false },
                new DatabaseColumnRequest("dtCreated", new DatabaseTypeRequest(typeof(DateTime)))
                    { AllowNulls = false, Default = MandatoryScalarFunctions.GetTodaysDate }
            });
            var idColumn = definitionTable.DiscoverColumn("id");
            var foreignKey =
                new DatabaseColumnRequest(DefinitionTableForeignKeyField, new DatabaseTypeRequest(typeof(int)), false)
                { IsPrimaryKey = true };

            _cohortDatabase.CreateTable("Cohort", new[]
            {
                    new DatabaseColumnRequest("chi",
                        new DatabaseTypeRequest(typeof(string)), false)
                    {
                        IsPrimaryKey = true,

                        // if there is a single collation amongst private identifier prototype references we must use that collation
                        // when creating the private column so that the DBMS can link them no bother
                        Collation = null
                    },
                    new DatabaseColumnRequest(ReleaseIdentifierFieldName, new DatabaseTypeRequest(typeof(string), 300))
                        { AllowNulls = true },
                    foreignKey
                });

            var newExternal =
                        new ExternalCohortTable(DataExportRepository, "TestExternalCohort", DatabaseType.MicrosoftSQLServer)
                        {
                            Database = CohortDatabaseName,
                            Server = _cohortDatabase.Server.Name,
                            DefinitionTableName = definitionTableName,
                            TableName = cohortTableName,
                            Name = ExternalCohortTableNameInCatalogue,
                            Username = _cohortDatabase.Server.ExplicitUsernameIfAny,
                            Password = _cohortDatabase.Server.ExplicitPasswordIfAny,
                            PrivateIdentifierField = "chi",
                            ReleaseIdentifierField = "ReleaseId",
                            DefinitionTableForeignKeyField = "cohortDefinition_id"
                        };

            newExternal.SaveToDatabase();
            var cohortPipeline = CatalogueRepository.GetAllObjects<Pipeline>().First(static p => p.Name == "CREATE COHORT:By Executing Cohort Identification Configuration");
            var newCohortCmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(
                new ThrowImmediatelyActivator(RepositoryLocator),
                cic,
                newExternal,
                "MyCohort",
                project,
                cohortPipeline
            );
            newCohortCmd.Execute();
            var extractableCohort = new ExtractableCohort(DataExportRepository, newExternal, 1);

            var ec = new ExtractionConfiguration(DataExportRepository, project)
            {
                Name = "ext1",
                Cohort_ID = extractableCohort.ID
            };
            var eds = new ExtractableDataSet(DataExportRepository, catalogue);
            ec.AddDatasetToConfiguration(eds);
            var cols = ec.GetAllExtractableColumnsFor(eds);
            var col = cols.First(c => c.SelectSQL.Contains("current_record"));
            var order = col.Order;
            var selectSQL = col.SelectSQL;
            var cei = col.CatalogueExtractionInformation;
            col.DeleteInDatabase();
            cols = ec.GetAllExtractableColumnsFor(eds);
            ec.SaveToDatabase();
            var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 9");
            var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(MSSqlMergeDestination), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<MSSqlMergeDestination>()
                .ToList();
            var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
            var argumentUseArchiveTrigger = destinationArguments.Single(a => a.Name == "UseArchiveTrigger");
            //var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
            Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();

            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            argumentUseArchiveTrigger.SetValue(true);
            argumentUseArchiveTrigger.SaveToDatabase();
            //reExtract.SetValue(true);
            //reExtract.SaveToDatabase();

            var component2 = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
                .ToArray();
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            extractionPipeline.DestinationPipelineComponent_ID = component.ID;
            extractionPipeline.SourcePipelineComponent_ID = component2.ID;
            extractionPipeline.SaveToDatabase();


            var dbname = TestDatabaseNames.GetConsistentName($"{project.Name}_{project.ProjectNumber}");
            var dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();
            dbToExtractTo.Create();
            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            var returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");



            var destinationTable = dbToExtractTo.ExpectTable("ext1_bob");
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));
            var hicLoadID = dt.Rows[0].ItemArray[37];

            var archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            var archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(0));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(42));
            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);

            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(40));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(1));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));

            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);

            //current_address_L3
            cols = ec.GetAllExtractableColumnsFor(eds);
            cols.First(c => c.SelectSQL.Contains("current_record")).DeleteInDatabase();

            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(2));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
        }

        //add a column and remove a  column
        [Test]
        public void MSSQLMerge_SQLServerDestinationWithTriggersAddAColumnAndRemoveAtSameTime()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

            //create catalogue from file
            var csvFile = CreateFileInForLoading("bob.csv", 1, new Random(5000));
            // Create the 'out of the box' RDMP pipelines (which includes an excel bulk importer pipeline)
            var creator = new CataloguePipelinesAndReferencesCreation(
                RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);

            // find the excel loading pipeline
            var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));

            if (pipe is null)
            {
                creator.CreatePipelines(new PlatformDatabaseCreationOptions { });
                pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
                .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
            }

            // run an import of the file using the pipeline
            var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(
                new ThrowImmediatelyActivator(RepositoryLocator),
                csvFile,
                null, db, pipe, null);

            cmd.Execute();
            var catalogue = CatalogueRepository.GetAllObjects<Catalogue>().FirstOrDefault(static c => c.Name == "bob");
            var chiColumnInfo = catalogue.CatalogueItems.First(static ci => ci.Name == "chi");
            var ei = chiColumnInfo.ExtractionInformation;
            ei.IsExtractionIdentifier = true;
            ei.IsPrimaryKey = true;
            ei.SaveToDatabase();
            var project = new Project(DataExportRepository, "MyProject")
            {
                ProjectNumber = 500,
                ExtractionDirectory = Path.GetTempPath()
            };
            project.SaveToDatabase();
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
            cic.CreateRootContainerIfNotExists();
            var agg1 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
            var conf = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
            conf.SaveToDatabase();
            agg1.SaveToDatabase();
            cic.RootCohortAggregateContainer.AddChild(agg1, 0);
            cic.SaveToDatabase();
            var dim = new AggregateDimension(CatalogueRepository, ei, agg1);
            dim.SaveToDatabase();
            agg1.SaveToDatabase();

            var CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
            var cohortTableName = "Cohort";
            var definitionTableName = "CohortDefinition";
            var ExternalCohortTableNameInCatalogue = "CohortTests";
            const string ReleaseIdentifierFieldName = "ReleaseId";
            const string DefinitionTableForeignKeyField = "cohortDefinition_id";
            var _cohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);
            if (_cohortDatabase.Exists())
                DeleteTables(_cohortDatabase);
            else
                _cohortDatabase.Create();

            var definitionTable = _cohortDatabase.CreateTable("CohortDefinition", new[]
               {
                new DatabaseColumnRequest("id", new DatabaseTypeRequest(typeof(int)))
                    { AllowNulls = false, IsAutoIncrement = true, IsPrimaryKey = true },
                new DatabaseColumnRequest("projectNumber", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("version", new DatabaseTypeRequest(typeof(int))) { AllowNulls = false },
                new DatabaseColumnRequest("description", new DatabaseTypeRequest(typeof(string), 3000))
                    { AllowNulls = false },
                new DatabaseColumnRequest("dtCreated", new DatabaseTypeRequest(typeof(DateTime)))
                    { AllowNulls = false, Default = MandatoryScalarFunctions.GetTodaysDate }
            });
            var idColumn = definitionTable.DiscoverColumn("id");
            var foreignKey =
                new DatabaseColumnRequest(DefinitionTableForeignKeyField, new DatabaseTypeRequest(typeof(int)), false)
                { IsPrimaryKey = true };

            _cohortDatabase.CreateTable("Cohort", new[]
            {
                    new DatabaseColumnRequest("chi",
                        new DatabaseTypeRequest(typeof(string)), false)
                    {
                        IsPrimaryKey = true,

                        // if there is a single collation amongst private identifier prototype references we must use that collation
                        // when creating the private column so that the DBMS can link them no bother
                        Collation = null
                    },
                    new DatabaseColumnRequest(ReleaseIdentifierFieldName, new DatabaseTypeRequest(typeof(string), 300))
                        { AllowNulls = true },
                    foreignKey
                });

            var newExternal =
                        new ExternalCohortTable(DataExportRepository, "TestExternalCohort", DatabaseType.MicrosoftSQLServer)
                        {
                            Database = CohortDatabaseName,
                            Server = _cohortDatabase.Server.Name,
                            DefinitionTableName = definitionTableName,
                            TableName = cohortTableName,
                            Name = ExternalCohortTableNameInCatalogue,
                            Username = _cohortDatabase.Server.ExplicitUsernameIfAny,
                            Password = _cohortDatabase.Server.ExplicitPasswordIfAny,
                            PrivateIdentifierField = "chi",
                            ReleaseIdentifierField = "ReleaseId",
                            DefinitionTableForeignKeyField = "cohortDefinition_id"
                        };

            newExternal.SaveToDatabase();
            var cohortPipeline = CatalogueRepository.GetAllObjects<Pipeline>().First(static p => p.Name == "CREATE COHORT:By Executing Cohort Identification Configuration");
            var newCohortCmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(
                new ThrowImmediatelyActivator(RepositoryLocator),
                cic,
                newExternal,
                "MyCohort",
                project,
                cohortPipeline
            );
            newCohortCmd.Execute();
            var extractableCohort = new ExtractableCohort(DataExportRepository, newExternal, 1);

            var ec = new ExtractionConfiguration(DataExportRepository, project)
            {
                Name = "ext1",
                Cohort_ID = extractableCohort.ID
            };
            var eds = new ExtractableDataSet(DataExportRepository, catalogue);
            ec.AddDatasetToConfiguration(eds);
            var cols = ec.GetAllExtractableColumnsFor(eds);
            var col = cols.First(c => c.SelectSQL.Contains("current_record"));
            var order = col.Order;
            var selectSQL = col.SelectSQL;
            var cei = col.CatalogueExtractionInformation;
            col.DeleteInDatabase();
            cols = ec.GetAllExtractableColumnsFor(eds);
            ec.SaveToDatabase();
            var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 10");
            var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(MSSqlMergeDestination), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<MSSqlMergeDestination>()
                .ToList();
            var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
            var argumentUseArchiveTrigger = destinationArguments.Single(a => a.Name == "UseArchiveTrigger");
            //var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
            Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
            var _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
            {
                Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
                Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
                Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
            };
            _extractionServer.SaveToDatabase();

            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            argumentUseArchiveTrigger.SetValue(true);
            argumentUseArchiveTrigger.SaveToDatabase();
            //reExtract.SetValue(true);
            //reExtract.SaveToDatabase();

            var component2 = new PipelineComponent(CatalogueRepository, extractionPipeline,
                typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
                .ToArray();
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            extractionPipeline.DestinationPipelineComponent_ID = component.ID;
            extractionPipeline.SourcePipelineComponent_ID = component2.ID;
            extractionPipeline.SaveToDatabase();


            var dbname = TestDatabaseNames.GetConsistentName($"{project.Name}_{project.ProjectNumber}");
            var dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();
            dbToExtractTo.Create();
            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            var returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");



            var destinationTable = dbToExtractTo.ExpectTable("ext1_bob");
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(39));
            var hicLoadID = dt.Rows[0].ItemArray[37];

            var archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            var archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(0));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(42));
            ec.RemoveDatasetFromConfiguration(eds);
            ec.AddDatasetToConfiguration(eds);
            cols.First(c => c.SelectSQL.Contains("current_address_L3")).DeleteInDatabase();

            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = ec.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = extractionPipeline.ID.ToString()
            });

            returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            Assert.That(destinationTable.Exists());

            dt = destinationTable.GetDataTable();
            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.That(dt.Columns, Has.Count.EqualTo(40));

            archiveTable = dbToExtractTo.ExpectTable("ext1_bob_Archive");
            Assert.That(archiveTable.Exists());
            archive_dt = archiveTable.GetDataTable();
            Assert.That(archive_dt.Rows, Has.Count.EqualTo(1));
            Assert.That(archive_dt.Columns, Has.Count.EqualTo(43));
        }

    }
}
