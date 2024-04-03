// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Amazon.Runtime.Internal.Transform;
using BadMedicine.Datasets;
using FAnsi.Discovery;
using NPOI.SS.Formula.Functions;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
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
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class ExecuteFullExtractionToDatabaseMSSqlDestinationReExtractionTest : TestsRequiringAnExtractionConfiguration
{
    private ExternalDatabaseServer _extractionServer;

    private const string _expectedTableName = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest_TestTable";
    private ColumnInfo _columnToTransform;
    private Pipeline _pipeline;
    DiscoveredTable tbl;
    SupportingSQLTable sql;

    [Test]
    public void SQLServerDestinationReExtraction()
    {
        DiscoveredDatabase dbToExtractTo = null;

        var ci = new CatalogueItem(CatalogueRepository, _catalogue, "YearOfBirth");
        _columnToTransform = _columnInfos.Single(c =>
            c.GetRuntimeName().Equals("DateOfBirth", StringComparison.CurrentCultureIgnoreCase));

        var transform = $"YEAR({_columnToTransform.Name})";


        if (_catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .All(ei => ei.GetRuntimeName() != "YearOfBirth"))
        {
            var ei = new ExtractionInformation(CatalogueRepository, ci, _columnToTransform, transform)
            {
                Alias = "YearOfBirth",
                ExtractionCategory = ExtractionCategory.Core
            };
            ei.SaveToDatabase();

            //make it part of the ExtractionConfiguration
            var newColumn = new ExtractableColumn(DataExportRepository, _selectedDataSet.ExtractableDataSet,
                (ExtractionConfiguration)_selectedDataSet.ExtractionConfiguration, ei, 0, ei.SelectSQL)
            {
                Alias = ei.Alias
            };
            newColumn.SaveToDatabase();

            _extractableColumns.Add(newColumn);
        }

        CreateLookupsEtc();

        try
        {
            _configuration.Name = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest";
            _configuration.SaveToDatabase();

            var dbname = TestDatabaseNames.GetConsistentName($"{_project.Name}_{_project.ProjectNumber}");
            dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();

            //ExecuteRunner();
            var pipeline = SetupPipeline();

            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
            {
                Command = CommandLineActivity.run,
                ExtractionConfiguration = _configuration.ID.ToString(),
                ExtractGlobals = true,
                Pipeline = pipeline.ID.ToString()
            });

            var returnCode = runner.Run(
                RepositoryLocator,
                ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet,
                new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            returnCode = runner.Run(
               RepositoryLocator,
               ThrowImmediatelyDataLoadEventListener.Quiet,
               ThrowImmediatelyCheckNotifier.Quiet,
               new GracefulCancellationToken());

            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

            var destinationTable = dbToExtractTo.ExpectTable(_expectedTableName);
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(dt.Rows[0]["ReleaseID"], Is.EqualTo(_cohortKeysGenerated[_cohortKeysGenerated.Keys.First()].Trim()));
                Assert.That(dt.Rows[0]["DateOfBirth"], Is.EqualTo(new DateTime(2001, 1, 1)));
                Assert.That(dt.Rows[0]["YearOfBirth"], Is.EqualTo(2001));

                Assert.That(destinationTable.DiscoverColumn("DateOfBirth").DataType.SQLType, Is.EqualTo(_columnToTransform.Data_type));
                Assert.That(destinationTable.DiscoverColumn("YearOfBirth").DataType.SQLType, Is.EqualTo("int"));
            });

            AssertLookupsEtcExist(dbToExtractTo);
        }
        finally
        {
            if (dbToExtractTo?.Exists() == true)
                dbToExtractTo.Drop();

            _pipeline?.DeleteInDatabase();
        }
    }

    //[Test]
    //    public void SQLServerDestinationReExtractionwithNewData()
    //    {
    //        DiscoveredDatabase dbToExtractTo = null;

    //        var ci = new CatalogueItem(CatalogueRepository, _catalogue, "YearOfBirth");
    //        _columnToTransform = _columnInfos.Single(c =>
    //            c.GetRuntimeName().Equals("DateOfBirth", StringComparison.CurrentCultureIgnoreCase));

    //        var transform = $"YEAR({_columnToTransform.Name})";


    //        if (_catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
    //            .All(ei => ei.GetRuntimeName() != "YearOfBirth"))
    //        {
    //            var ei = new ExtractionInformation(CatalogueRepository, ci, _columnToTransform, transform)
    //            {
    //                Alias = "YearOfBirth",
    //                ExtractionCategory = ExtractionCategory.Core
    //            };
    //            ei.SaveToDatabase();

    //            //make it part of the ExtractionConfiguration
    //            var newColumn = new ExtractableColumn(DataExportRepository, _selectedDataSet.ExtractableDataSet,
    //                (ExtractionConfiguration)_selectedDataSet.ExtractionConfiguration, ei, 0, ei.SelectSQL)
    //            {
    //                Alias = ei.Alias
    //            };
    //            newColumn.SaveToDatabase();

    //            _extractableColumns.Add(newColumn);
    //        }

    //        CreateLookupsEtc();

    //        try
    //        {
    //            _configuration.Name = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest";
    //            _configuration.SaveToDatabase();

    //            var dbname = TestDatabaseNames.GetConsistentName($"{_project.Name}_{_project.ProjectNumber}");
    //            dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
    //            if (dbToExtractTo.Exists())
    //                dbToExtractTo.Drop();

    //            //ExecuteRunner();
    //            var pipeline = SetupPipeline();

    //            var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
    //            {
    //                Command = CommandLineActivity.run,
    //                ExtractionConfiguration = _configuration.ID.ToString(),
    //                ExtractGlobals = true,
    //                Pipeline = pipeline.ID.ToString()
    //            });

    //            var returnCode = runner.Run(
    //                RepositoryLocator,
    //                ThrowImmediatelyDataLoadEventListener.Quiet,
    //                ThrowImmediatelyCheckNotifier.Quiet,
    //                new GracefulCancellationToken());

    //            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");
    //            tbl.Insert(new Dictionary<string, object>
    //            {
    //                { "chi","1111111111"},
    //                {"Healthboard","T"},
    //                {"SampleDate", new DateTime(2001, 1, 2) },
    //                {"SampleType","Blood"},
    //                {"TestCode","1234"},
    //                {"Result",1},
    //                {"Labnumber","1"},
    //                {"QuantityUnit","ml"},
    //                {"ReadCodeValue","1"},
    //                {"ArithmeticComparator","="},
    //                {"Interpretation","!"},
    //                {"RangeHighValue",1},
    //                {"RangeLowValue",1}
    //            });
    //            sql.SQL = $"SELECT * FROM {tbl.GetFullyQualifiedName()}";
    //            sql.SaveToDatabase();
    //            runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
    //            {
    //                Command = CommandLineActivity.run,
    //                ExtractionConfiguration = _configuration.ID.ToString(),
    //                ExtractGlobals = true,
    //                Pipeline = pipeline.ID.ToString()
    //            });

    //            returnCode = runner.Run(
    //               RepositoryLocator,
    //               ThrowImmediatelyDataLoadEventListener.Quiet,
    //               ThrowImmediatelyCheckNotifier.Quiet,
    //               new GracefulCancellationToken());

    //            Assert.That(returnCode, Is.EqualTo(0), "Return code from runner was non zero");

    //            var destinationTable = dbToExtractTo.ExpectTable(_expectedTableName);
    //            Assert.That(destinationTable.Exists());

    //            var dt = destinationTable.GetDataTable();

    //            Assert.That(dt.Rows, Has.Count.EqualTo(2));
    //            Assert.Multiple(() =>
    //            {
    //                Assert.That(dt.Rows[0]["ReleaseID"], Is.EqualTo(_cohortKeysGenerated[_cohortKeysGenerated.Keys.First()].Trim()));
    //                Assert.That(dt.Rows[0]["DateOfBirth"], Is.EqualTo(new DateTime(2001, 1, 1)));
    //                Assert.That(dt.Rows[0]["YearOfBirth"], Is.EqualTo(2001));

    //                Assert.That(destinationTable.DiscoverColumn("DateOfBirth").DataType.SQLType, Is.EqualTo(_columnToTransform.Data_type));
    //                Assert.That(destinationTable.DiscoverColumn("YearOfBirth").DataType.SQLType, Is.EqualTo("int"));
    //            });

    //            AssertLookupsEtcExist(dbToExtractTo);
    //        }
    //        finally
    //        {
    //            if (dbToExtractTo?.Exists() == true)
    //                dbToExtractTo.Drop();

    //            _pipeline?.DeleteInDatabase();
    //        }
    //    }

    private static void AssertLookupsEtcExist(DiscoveredDatabase dbToExtractTo)
    {
        Assert.That(dbToExtractTo.ExpectTable("ExecuteFullExtractionToDatabaseMSSqlDestinationTest_TestTable_Biochem")
                .Exists());
    }

    private void CreateLookupsEtc()
    {
        //an extractable file
        var filename = Path.Combine(TestContext.CurrentContext.WorkDirectory, "bob.txt");

        File.WriteAllText(filename, "fishfishfish");
        var doc = new SupportingDocument(CatalogueRepository, _catalogue, "bob")
        {
            URL = new Uri($"file://{filename}"),
            Extractable = true
        };
        doc.SaveToDatabase();

        //an extractable global file (comes out regardless of datasets)
        var filename2 = Path.Combine(TestContext.CurrentContext.WorkDirectory, "bob2.txt");

        File.WriteAllText(filename2, "fishfishfish2");
        var doc2 = new SupportingDocument(CatalogueRepository, _catalogue, "bob2")
        {
            URL = new Uri($"file://{filename2}"),
            Extractable = true,
            IsGlobal = true
        };
        doc2.SaveToDatabase();

        //an supplemental table in the database (not linked against cohort)
        tbl = CreateDataset<Biochemistry>(Database, 1, 1, new Random(50));

        sql = new SupportingSQLTable(CatalogueRepository, _catalogue, "Biochem");
        var server = new ExternalDatabaseServer(CatalogueRepository, "myserver", null);
        server.SetProperties(tbl.Database);
        sql.ExternalDatabaseServer_ID = server.ID;
        sql.SQL = $"SELECT * FROM {tbl.GetFullyQualifiedName()}";
        sql.Extractable = true;
        sql.SaveToDatabase();

    }


    protected override Pipeline SetupPipeline()
    {
        //create a target server pointer
        _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
        {
            Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
            Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
            Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
        };
        _extractionServer.SaveToDatabase();
        Random rnd = new Random();

        //create a pipeline
        _pipeline = new Pipeline(CatalogueRepository, $"Empty extraction pipeline{rnd.Next()}");

        //set the destination pipeline
        var component = new PipelineComponent(CatalogueRepository, _pipeline,
            typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
        var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>()
            .ToList();

        var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
        var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
        var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");
        var argumentAppendDataIfTableExists = destinationArguments.Single(a => a.Name == "AppendDataIfTableExists");
        Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
        argumentServer.SetValue(_extractionServer);
        argumentServer.SaveToDatabase();
        argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
        argumentDbNamePattern.SaveToDatabase();
        argumentTblNamePattern.SetValue("$c_$d");
        argumentTblNamePattern.SaveToDatabase();
        argumentAppendDataIfTableExists.SetValue(true);
        argumentAppendDataIfTableExists.SaveToDatabase();
        AdjustPipelineComponentDelegate?.Invoke(component);

        var component2 = new PipelineComponent(CatalogueRepository, _pipeline,
            typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
        var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
            .ToArray();
        arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
        arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();
        AdjustPipelineComponentDelegate?.Invoke(component2);

        //configure the component as the destination
        _pipeline.DestinationPipelineComponent_ID = component.ID;
        _pipeline.SourcePipelineComponent_ID = component2.ID;
        _pipeline.SaveToDatabase();

        return _pipeline;
    }
}