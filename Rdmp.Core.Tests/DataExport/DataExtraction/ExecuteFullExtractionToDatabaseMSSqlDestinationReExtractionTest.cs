// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using SynthEHR;
using SynthEHR.Datasets;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using TypeGuesser;
using Rdmp.Core.DataLoad.Triggers;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class ExecuteFullExtractionToDatabaseMSSqlDestinationReExtractionTest : DatabaseTests
{

    private FileInfo CreateFileInForLoading(string filename, int rows, Random r)
    {
        var fi = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetFileName(filename)));

        var demog = new Demography(r);
        var people = new PersonCollection();
        people.GeneratePeople(500, r);

        demog.GenerateTestDataFile(people, fi, rows);

        return fi;
    }

    [Test]
    public void ReExtractToADatabaseWithNoNewData()
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
        ec.AddDatasetToConfiguration(new ExtractableDataSet(DataExportRepository, catalogue));

        ec.SaveToDatabase();

        var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 1");
        var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
            typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
        var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>()
            .ToList();
        var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
        var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
        var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
        var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
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
        reExtract.SetValue(true);
        reExtract.SaveToDatabase();

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


    }

    [Test]
    public void ReExtractToADatabaseWithNewDataAndNoPKs()
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
        ec.AddDatasetToConfiguration(new ExtractableDataSet(DataExportRepository, catalogue));

        ec.SaveToDatabase();

        var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 2");
        var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
            typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
        var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>()
            .ToList();
        var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
        var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
        var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
        var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
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
        reExtract.SetValue(true);
        reExtract.SaveToDatabase();

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

        //add new entry here
        var tbl = db.DiscoverTables(false).First();
        tbl.Insert(new Dictionary<string, object>
        {
            { "chi","1111111111"},
            {"notes","T"},
            {"dtCreated", new DateTime(2001, 1, 2) },
            {"century",19},
            {"surname","1234"},
            {"forename","yes"},
            {"sex","M"},
        });

        var cic2 = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
        cic2.CreateRootContainerIfNotExists();
        var agg12 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
        _ = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
        conf.SaveToDatabase();
        agg1.SaveToDatabase();
        cic.RootCohortAggregateContainer.AddChild(agg12, 0);
        cic.SaveToDatabase();
        var dim2 = new AggregateDimension(CatalogueRepository, ei, agg12);
        dim2.SaveToDatabase();
        agg12.SaveToDatabase();

        newCohortCmd.Execute();
        var _extractableCohort2 = new ExtractableCohort(DataExportRepository, newExternal, 2);
        ec.Cohort_ID = _extractableCohort2.ID;
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

        Assert.That(dt.Rows, Has.Count.EqualTo(2));
    }

    [Test]
    public void ReExtractToADatabaseWithNewDataAndSinglePK()
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

        var surnameInfo = catalogue.CatalogueItems.First(static ci => ci.Name == "surname");
        surnameInfo.ExtractionInformation.IsPrimaryKey = true;
        surnameInfo.ExtractionInformation.SaveToDatabase();

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
        ec.AddDatasetToConfiguration(new ExtractableDataSet(DataExportRepository, catalogue));

        ec.SaveToDatabase();

        var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline");
        var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
            typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
        var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>()
            .ToList();
        var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
        var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
        var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
        var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
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
        reExtract.SetValue(true);
        reExtract.SaveToDatabase();

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

        //add new entry here
        var tbl = db.DiscoverTables(false).First();
        tbl.Insert(new Dictionary<string, object>
        {
            { "chi","1111111111"},
            {"notes","T"},
            {"dtCreated", new DateTime(2001, 1, 2) },
            {"century",19},
            {"surname","1234"},
            {"forename","yes"},
            {"sex","M"},
        });

        var cic2 = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
        cic2.CreateRootContainerIfNotExists();
        var agg12 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
        _ = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
        conf.SaveToDatabase();
        agg1.SaveToDatabase();
        cic2.RootCohortAggregateContainer.AddChild(agg12, 0);
        cic2.SaveToDatabase();
        var dim2 = new AggregateDimension(CatalogueRepository, ei, agg12);
        dim2.SaveToDatabase();
        agg12.SaveToDatabase();
        newCohortCmd = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(
            new ThrowImmediatelyActivator(RepositoryLocator),
            cic2,
            newExternal,
            "MyCohort",
            project,
            cohortPipeline
        );
        newCohortCmd.Execute();
        var _extractableCohort2 = new ExtractableCohort(DataExportRepository, newExternal, 2);
        ec.Cohort_ID = _extractableCohort2.ID;
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

        Assert.That(dt.Rows, Has.Count.EqualTo(2));
    }

    [Test]
    public void ReExtractToADatabaseWithNewDataAndExtractionIdentifierIsPK()
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
        ei.IsPrimaryKey = true;
        ei.IsExtractionIdentifier = true;
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
        const string cohortTableName = "Cohort";
        const string definitionTableName = "CohortDefinition";
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
        ec.AddDatasetToConfiguration(new ExtractableDataSet(DataExportRepository, catalogue));

        ec.SaveToDatabase();

        var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 3");
        var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
            typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
        var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>()
            .ToList();
        var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
        var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
        var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
        var reExtract = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
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
        reExtract.SetValue(true);
        reExtract.SaveToDatabase();

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

        //add new entry here
        var tbl = db.DiscoverTables(false).First();
        tbl.Insert(new Dictionary<string, object>
        {
            { "chi","1111111111"},
            {"notes","T"},
            {"dtCreated", new DateTime(2001, 1, 2) },
            {"century",19},
            {"surname","1234"},
            {"forename","yes"},
            {"sex","M"},
        });

        var cic2 = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
        cic2.CreateRootContainerIfNotExists();
        var agg12 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
        _ = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
        conf.SaveToDatabase();
        agg1.SaveToDatabase();
        cic.RootCohortAggregateContainer.AddChild(agg12, 0);
        cic.SaveToDatabase();
        var dim2 = new AggregateDimension(CatalogueRepository, ei, agg12);
        dim2.SaveToDatabase();
        agg12.SaveToDatabase();

        newCohortCmd.Execute();
        var _extractableCohort2 = new ExtractableCohort(DataExportRepository, newExternal, 2);
        ec.Cohort_ID = _extractableCohort2.ID;
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

        Assert.That(dt.Rows, Has.Count.EqualTo(2));
    }

    [Test]
    public void ExtractToDatabaseUseTriggers()
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
        ei.IsPrimaryKey = true;
        ei.IsExtractionIdentifier = true;
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

        var ec = DataExportRepository.GetAllObjectsWhere<ExtractionConfiguration>("Cohort_ID", extractableCohort.ID).FirstOrDefault() ?? new ExtractionConfiguration(DataExportRepository, project)
        {
            Name = "ext1",
            Cohort_ID = extractableCohort.ID
        };
        ec.AddDatasetToConfiguration(new ExtractableDataSet(DataExportRepository, catalogue));

        ec.SaveToDatabase();

        var extractionPipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline 4");
        var component = new PipelineComponent(CatalogueRepository, extractionPipeline,
            typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
        var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>()
            .ToList();
        var argumentServer = destinationArguments.Single(static a => a.Name == "TargetDatabaseServer");
        var argumentDbNamePattern = destinationArguments.Single(static a => a.Name == "DatabaseNamingPattern");
        var argumentTblNamePattern = destinationArguments.Single(static a => a.Name == "TableNamingPattern");
        var reExtract = destinationArguments.Single(static a => a.Name == "AppendDataIfTableExists");
        Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
        var extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
        {
            Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
            Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
            Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
        };
        extractionServer.SaveToDatabase();

        argumentServer.SetValue(extractionServer);
        argumentServer.SaveToDatabase();
        argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
        argumentDbNamePattern.SaveToDatabase();
        argumentTblNamePattern.SetValue("$c_$d");
        argumentTblNamePattern.SaveToDatabase();
        reExtract.SetValue(true);
        reExtract.SaveToDatabase();

        var component2 = new PipelineComponent(CatalogueRepository, extractionPipeline,
            typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
        var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
            .ToArray();
        var allowEmpty = arguments2.Single(static a => a.Name.Equals("AllowEmptyExtractions"));
        allowEmpty.SetValue(false);
        allowEmpty.SaveToDatabase();

        //configure the component as the destination
        extractionPipeline.DestinationPipelineComponent_ID = component.ID;
        extractionPipeline.SourcePipelineComponent_ID = component2.ID;
        extractionPipeline.SaveToDatabase();


        var dbname = TestDatabaseNames.GetConsistentName($"{project.Name}_{project.ProjectNumber}");
        var dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
        if (dbToExtractTo.Exists())
            dbToExtractTo.Drop();

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
        var columns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        Assert.That(columns, Does.Contain(SpecialFieldNames.DataLoadRunID));
        Assert.That(columns, Does.Contain(SpecialFieldNames.ValidFrom));
        ////add new entry here
        var tbl = db.DiscoverTables(false).First();
        tbl.Insert(new Dictionary<string, object>
        {
            { "chi","1111111111"},
            {"notes","T"},
            {"dtCreated", new DateTime(2001, 1, 2) },
            {"century",19},
            {"surname","1234"},
            {"forename","yes"},
            {"sex","M"},
        });

        var cic2 = new CohortIdentificationConfiguration(CatalogueRepository, "Cohort1");
        cic2.CreateRootContainerIfNotExists();
        var agg12 = new AggregateConfiguration(CatalogueRepository, catalogue, "agg1");
        _ = new AggregateConfiguration(CatalogueRepository, catalogue, "UnitTestShortcutAggregate");
        conf.SaveToDatabase();
        agg1.SaveToDatabase();
        cic.RootCohortAggregateContainer.AddChild(agg12, 0);
        cic.SaveToDatabase();
        var dim2 = new AggregateDimension(CatalogueRepository, ei, agg12);
        dim2.SaveToDatabase();
        agg12.SaveToDatabase();

        newCohortCmd.Execute();
        var _extractableCohort2 = new ExtractableCohort(DataExportRepository, newExternal, 2);
        ec.Cohort_ID = _extractableCohort2.ID;
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

        Assert.That(dt.Rows, Has.Count.EqualTo(2));
    }
}