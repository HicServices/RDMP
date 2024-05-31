// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class HICPipelineTests : DatabaseTests
{
    internal class CatalogueEntities : IDisposable
    {
        public Catalogue Catalogue { get; private set; }
        public LoadMetadata LoadMetadata { get; private set; }
        public ColumnInfo ColumnInfo { get; private set; }
        public TableInfo TableInfo { get; private set; }

        public DataAccessCredentials Credentials { get; private set; }

        public CatalogueEntities()
        {
            Catalogue = null;
            LoadMetadata = null;
            ColumnInfo = null;
            TableInfo = null;
        }

        public void Create(CatalogueRepository repository, DiscoveredDatabase database,
            ILoadDirectory directory)
        {
            TableInfo = new TableInfo(repository, "TestData")
            {
                Server = database.Server.Name,
                Database = database.GetRuntimeName()
            };
            TableInfo.SaveToDatabase();

            if (!string.IsNullOrWhiteSpace(database.Server.ExplicitUsernameIfAny))
                Credentials = new DataAccessCredentialsFactory(repository).Create(TableInfo,
                    database.Server.ExplicitUsernameIfAny, database.Server.ExplicitPasswordIfAny,
                    DataAccessContext.Any);


            ColumnInfo = new ColumnInfo(repository, "Col1", "int", TableInfo)
            {
                IsPrimaryKey = true
            };
            ColumnInfo.SaveToDatabase();

            LoadMetadata = new LoadMetadata(repository, "HICLoadPipelineTests")
            {
                LocationOfFlatFiles = directory.RootPath.FullName
            };
            LoadMetadata.SaveToDatabase();

            Catalogue = new Catalogue(repository, "HICLoadPipelineTests")
            {
                LoggingDataTask = "Test",
            };
            Catalogue.SaveToDatabase();
            LoadMetadata.LinkToCatalogue(Catalogue);
            var catalogueItem = new CatalogueItem(repository, Catalogue, "Test");
            catalogueItem.SetColumnInfo(ColumnInfo);

            SetupLoadProcessTasks(repository);
        }

        public void Dispose()
        {
            Catalogue?.DeleteInDatabase();

            LoadMetadata?.DeleteInDatabase();

            ColumnInfo?.DeleteInDatabase();

            TableInfo?.DeleteInDatabase();

            Credentials?.DeleteInDatabase();
        }

        private void SetupLoadProcessTasks(ICatalogueRepository catalogueRepository)
        {
            var attacherTask = new ProcessTask(catalogueRepository, LoadMetadata, LoadStage.Mounting)
            {
                Name = "Attach CSV file",
                Order = 1,
                Path = "Rdmp.Core.DataLoad.Modules.Attachers.AnySeparatorFileAttacher",
                ProcessTaskType = ProcessTaskType.Attacher
            };
            attacherTask.SaveToDatabase();

            // Not assigned to a variable as they will be magically available through the repository
            var processTaskArgs = new List<Tuple<string, string, Type>>
            {
                new("FilePattern", "1.csv", typeof(string)),
                new("TableName", "TestData", typeof(string)),
                new("ForceHeaders", null, typeof(string)),
                new("IgnoreQuotes", null, typeof(bool)),
                new("IgnoreBlankLines", null, typeof(bool)),
                new("ForceHeadersReplacesFirstLineInFile", null, typeof(bool)),
                new("SendLoadNotRequiredIfFileNotFound", "false", typeof(bool)),
                new("Separator", ",", typeof(string)),
                new("TableToLoad", null, typeof(TableInfo)),
                new("BadDataHandlingStrategy", BadDataHandlingStrategy.ThrowException.ToString(),
                    typeof(BadDataHandlingStrategy)),
                new("ThrowOnEmptyFiles", "true", typeof(bool)),
                new("AttemptToResolveNewLinesInRecords", "true", typeof(bool)),
                new("MaximumErrorsToReport", "0", typeof(int)),
                new("IgnoreColumns", null, typeof(string)),
                new("IgnoreBadReads", "false", typeof(bool)),
                new("AddFilenameColumnNamed", null, typeof(string))
            };


            foreach (var tuple in processTaskArgs)
            {
                var pta = new ProcessTaskArgument(catalogueRepository, attacherTask)
                {
                    Name = tuple.Item1,
                    Value = tuple.Item2
                };
                pta.SetType(tuple.Item3);
                pta.SaveToDatabase();
            }
        }
    }

    internal class DatabaseHelper : IDisposable
    {
        private DiscoveredServer _server;


        public DiscoveredDatabase DatabaseToLoad { get; private set; }

        public void SetUp(DiscoveredServer server)
        {
            _server = server;

            var databaseToLoadName = "HICPipelineTests";

            // Create the databases
            server.ExpectDatabase(databaseToLoadName).Create(true);
            server.ChangeDatabase(databaseToLoadName);

            // Create the dataset table
            DatabaseToLoad = server.ExpectDatabase(databaseToLoadName);
            using (var con = DatabaseToLoad.Server.GetConnection())
            {
                con.Open();
                const string createDatasetTableQuery =
                    "CREATE TABLE TestData ([Col1] [int], [hic_dataLoadRunID] [int] NULL, [hic_validFrom] [datetime] NULL, CONSTRAINT [PK_TestData] PRIMARY KEY CLUSTERED ([Col1] ASC))";
                const string addValidFromDefault =
                    "ALTER TABLE TestData ADD CONSTRAINT [DF_TestData__hic_validFrom]  DEFAULT (getdate()) FOR [hic_validFrom]";
                using (var cmd = DatabaseCommandHelper.GetCommand(createDatasetTableQuery, con))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = DatabaseCommandHelper.GetCommand(addValidFromDefault, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // Ensure the dataset table has been created
            var datasetTable = DatabaseToLoad.ExpectTable("TestData");
            Assert.That(datasetTable.Exists());
        }

        public void Dispose()
        {
            if (DatabaseToLoad == null)
                return;

            if (DatabaseToLoad.Exists())
                DatabaseToLoad.Drop();

            // check if RAW has been created and remove it
            var raw = _server.ExpectDatabase($"{DatabaseToLoad.GetRuntimeName()}_RAW");
            if (raw.Exists())
                raw.Drop();
        }
    }

    [Test]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void TestSingleJob(bool overrideRAW, bool sendDodgyCredentials)
    {
        if (sendDodgyCredentials && !overrideRAW)
            throw new NotSupportedException("Cannot send dodgy credentials if you aren't overriding RAW");

        IServerDefaults defaults = CatalogueRepository;
        var oldDefault = defaults.GetDefaultFor(PermissableDefaults.RAWDataLoadServer);

        var testDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var testDir = Directory.CreateDirectory(testDirPath);
        var server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn;

        using (var catalogueEntities = new CatalogueEntities())
        {
            using (var databaseHelper = new DatabaseHelper())
            {
                ExternalDatabaseServer external = null;

                try
                {
                    // Set SetUp the dataset's project directory and add the CSV file to ForLoading
                    var loadDirectory = LoadDirectory.CreateDirectoryStructure(testDir, "TestDataset");
                    File.WriteAllText(Path.Combine(loadDirectory.ForLoading.FullName, "1.csv"),
                        "Col1\r\n1\r\n2\r\n3\r\n4");

                    databaseHelper.SetUp(server);

                    // Create the Catalogue entities for the dataset
                    catalogueEntities.Create(CatalogueTableRepository, databaseHelper.DatabaseToLoad, loadDirectory);

                    if (overrideRAW)
                    {
                        external = new ExternalDatabaseServer(CatalogueRepository, "RAW Server", null);
                        external.SetProperties(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("master"));

                        if (sendDodgyCredentials)
                        {
                            external.Username = "IveGotaLovely";
                            external.Password = "BunchOfCoconuts";
                        }

                        external.SaveToDatabase();

                        defaults.SetDefault(PermissableDefaults.RAWDataLoadServer, external);
                    }

                    var options = new DleOptions
                    {
                        LoadMetadata = catalogueEntities.LoadMetadata.ID.ToString(),
                        Command = CommandLineActivity.check
                    };

                    //run checks (with ignore errors if we are sending dodgy credentials)
                    RunnerFactory.CreateRunner(new ThrowImmediatelyActivator(RepositoryLocator), options).Run(RepositoryLocator,
                        ThrowImmediatelyDataLoadEventListener.Quiet,
                        sendDodgyCredentials
                            ? (ICheckNotifier)IgnoreAllErrorsCheckNotifier.Instance
                            : new AcceptAllCheckNotifier(), new GracefulCancellationToken());

                    //run load
                    options.Command = CommandLineActivity.run;
                    var runner = RunnerFactory.CreateRunner(new ThrowImmediatelyActivator(RepositoryLocator), options);


                    if (sendDodgyCredentials)
                    {
                        var ex = Assert.Throws<Exception>(() => runner.Run(RepositoryLocator,
                            ThrowImmediatelyDataLoadEventListener.Quiet, new AcceptAllCheckNotifier(),
                            new GracefulCancellationToken()));
                        Assert.That(ex.InnerException.Message, Does.Contain("Login failed for user 'IveGotaLovely'"),
                            "Error message did not contain expected text");
                        return;
                    }
                    else
                    {
                        runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, new AcceptAllCheckNotifier(),
                            new GracefulCancellationToken());
                    }


                    var archiveFile = loadDirectory.ForArchiving.EnumerateFiles("*.zip").MaxBy(f => f.FullName);
                    Assert.Multiple(() =>
                    {
                        Assert.That(archiveFile, Is.Not.Null, "Archive file has not been created by the load.");
                        Assert.That(loadDirectory.ForLoading.EnumerateFileSystemInfos().Any(), Is.False);
                    });
                }
                finally
                {
                    //reset the original RAW server
                    defaults.SetDefault(PermissableDefaults.RAWDataLoadServer, oldDefault);

                    external?.DeleteInDatabase();

                    testDir.Delete(true);
                }
            }
        }
    }
}

public class TestCacheFileRetriever : CachedFileRetriever
{
    public override void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
    }

    public override ExitCodeType Fetch(IDataLoadJob dataLoadJob, GracefulCancellationToken cancellationToken)
    {
        var LoadDirectory = dataLoadJob.LoadDirectory;
        var fileToMove = LoadDirectory.Cache.EnumerateFiles("*.csv").FirstOrDefault();
        if (fileToMove == null)
            return ExitCodeType.OperationNotRequired;

        File.Move(fileToMove.FullName, Path.Combine(LoadDirectory.ForLoading.FullName, "1.csv"));
        return ExitCodeType.Success;
    }
}