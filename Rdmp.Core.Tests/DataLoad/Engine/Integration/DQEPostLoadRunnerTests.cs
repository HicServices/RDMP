// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Databases;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.DataQualityEngine.Reports;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Tests.Common;
using Tests.Common.Scenarios;
using YamlDotNet.Core.Tokens;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class DQEPostLoadRunnerTests : TestsRequiringAnExtractionConfiguration
{

    [SetUp]
    protected override void SetUp()
    {
    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.PostgreSql)]
    [TestCase(DatabaseType.Oracle)]
    public void TestDQEPostLoad_WrongStage(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType, "TestDQE");

        var dqePostLoad = new DQEPostLoadRunner();
        var ex = Assert.Throws<Exception>(() => dqePostLoad.Initialize(db, LoadStage.AdjustRaw));
        Assert.That(ex.Message, Is.EqualTo("DQL Runner can only be done in the PostLoad stage."));

    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.PostgreSql)]
    [TestCase(DatabaseType.Oracle)]
    public void TestDEQPostLoad_NoPreviousDQE(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType, "DQETempTestDb");
        var patcher = new DataQualityEnginePatcher();

        var mds = new MasterDatabaseScriptExecutor(db);
        mds.CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());
        var dqeRepository = new DQERepository(CatalogueRepository, db);
        var numberOfRecordsToGenerate = 1000;
        var startTime = DateTime.Now;

        var testData = new BulkTestsData(CatalogueRepository, GetCleanedServer(DatabaseType.MicrosoftSQLServer),
            numberOfRecordsToGenerate);
        testData.SetupTestData();
        testData.ImportAsCatalogue();
        Assert.That(dqeRepository.GetMostRecentEvaluationFor(_catalogue), Is.Null);
        var lmd = new LoadMetadata(CatalogueRepository, "test");
        lmd.LinkToCatalogue(_catalogue);
        lmd.SaveToDatabase();
        var dqePostLoad = new DQEPostLoadRunner();
        Assert.DoesNotThrow(() => dqePostLoad.Initialize(db, LoadStage.PostLoad));
        var job = Substitute.For<IDataLoadJob>();
        job.RepositoryLocator.Returns(RepositoryLocator);
        job.DataLoadInfo.ID.Returns(1);
        job.LoadMetadata.Returns(lmd);
        Assert.DoesNotThrow(() => dqePostLoad.Mutilate(job));
        //todo test that no dqe was tgenerated
        Assert.That(dqeRepository.GetAllEvaluationsFor(_catalogue), Is.Empty);

    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.PostgreSql)]
    [TestCase(DatabaseType.Oracle)]
    public void TestDEQPostLoad_AddRow(DatabaseType dbType)
    {
        SingleTableSetup();
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(
                $"INSERT INTO [Samples] (ID, SampleDate, Description, {SpecialFieldNames.ValidFrom}, {SpecialFieldNames.DataLoadRunID}) VALUES (1, '2016-01-10T12:00:00', 'A', NULL, NULL)",
                connection);
            cmd.ExecuteNonQuery();
        }
        var dqeRepository = new DQERepository(_catalogue.CatalogueRepository);
        Assert.That(dqeRepository.GetAllEvaluationsFor(_catalogue), Is.Empty);
        _catalogue.ValidatorXML = validatorXML;
        var toBeTimePeriodicityCol = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
           .Single(e => e.GetRuntimeName().Equals("SampleDate"));
        _catalogue.TimeCoverage_ExtractionInformation_ID = toBeTimePeriodicityCol.ID;
        _catalogue.SaveToDatabase();
        var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID)
        {
            ExplicitDQERepository = dqeRepository
        };

        report.Check(ThrowImmediatelyCheckNotifier.Quiet);

        var source = new CancellationTokenSource();
        var listener = new ToMemoryDataLoadEventListener(false);
        report.GenerateReport(_catalogue, listener, source.Token);
        var results = dqeRepository.GetMostRecentEvaluationFor(_catalogue);
        var columnStates = dqeRepository.GetMostRecentEvaluationFor(_catalogue).ColumnStates;
        Assert.That(columnStates.Length, Is.EqualTo(5));
        Assert.That(columnStates.Where(c => c.TargetProperty == "Description").First().CountCorrect, Is.EqualTo(1));
        Assert.That(results, Is.Not.Null);
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(
                $"INSERT INTO [Samples] (ID, SampleDate, Description, {SpecialFieldNames.ValidFrom}, {SpecialFieldNames.DataLoadRunID}) VALUES (2, '2017-01-10T12:00:00', 'B', '2016-01-10T12:00:00', 1)",
                connection);
            cmd.ExecuteNonQuery();
        }
        var job = Substitute.For<IDataLoadJob>();
        job.RepositoryLocator.Returns(RepositoryLocator);
        job.DataLoadInfo.ID.Returns(1);
        job.LoadMetadata.Returns(_lmd);
        Mutilate(job);
        Assert.That(dqeRepository.GetAllEvaluationsFor(_catalogue).Count, Is.EqualTo(2));
        columnStates = dqeRepository.GetMostRecentEvaluationFor(_catalogue).ColumnStates;
        Assert.That(columnStates.Length, Is.EqualTo(5));
        Assert.That(columnStates.Where(c => c.TargetProperty == "Description").First().CountCorrect, Is.EqualTo(2));
    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.PostgreSql)]
    [TestCase(DatabaseType.Oracle)]
    public void TestDEQPostLoad_AddRowAndReplaceRow(DatabaseType dbType)
    {
        SingleTableSetup();
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(
                $"INSERT INTO [Samples] (ID, SampleDate, Description, {SpecialFieldNames.ValidFrom}, {SpecialFieldNames.DataLoadRunID}) VALUES (1, '2016-01-10T12:00:00', 'A', NULL, NULL)",
                connection);
            cmd.ExecuteNonQuery();
        }
        var dqeRepository = new DQERepository(_catalogue.CatalogueRepository);
        Assert.That(dqeRepository.GetAllEvaluationsFor(_catalogue), Is.Empty);
        _catalogue.ValidatorXML = validatorXML;
        var toBeTimePeriodicityCol = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
           .Single(e => e.GetRuntimeName().Equals("SampleDate"));
        _catalogue.TimeCoverage_ExtractionInformation_ID = toBeTimePeriodicityCol.ID;
        _catalogue.SaveToDatabase();
        var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID)
        {
            ExplicitDQERepository = dqeRepository
        };

        report.Check(ThrowImmediatelyCheckNotifier.Quiet);

        var source = new CancellationTokenSource();
        var listener = new ToMemoryDataLoadEventListener(false);
        report.GenerateReport(_catalogue, listener, source.Token);
        var results = dqeRepository.GetMostRecentEvaluationFor(_catalogue);
        var columnStates = dqeRepository.GetMostRecentEvaluationFor(_catalogue).ColumnStates;
        Assert.That(columnStates.Length, Is.EqualTo(5));
        Assert.That(columnStates.Where(c => c.TargetProperty == "Description").First().CountCorrect, Is.EqualTo(1));
        Assert.That(results, Is.Not.Null);
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(
                $"INSERT INTO [Samples] (ID, SampleDate, Description, {SpecialFieldNames.ValidFrom}, {SpecialFieldNames.DataLoadRunID}) VALUES (2, '2017-01-10T12:00:00', 'B', '2016-01-10T12:00:00', 1)",
                connection);
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand(
               $"UPDATE[Samples] SET Description='c', {SpecialFieldNames.ValidFrom}= '2016-01-10T12:00:00', {SpecialFieldNames.DataLoadRunID}=1 WHERE ID=1",
               connection);
            cmd.ExecuteNonQuery();
        }
        var job = Substitute.For<IDataLoadJob>();
        job.RepositoryLocator.Returns(RepositoryLocator);
        job.DataLoadInfo.ID.Returns(1);
        job.LoadMetadata.Returns(_lmd);
        Mutilate(job);
        Assert.That(dqeRepository.GetAllEvaluationsFor(_catalogue).Count, Is.EqualTo(2));
        columnStates = dqeRepository.GetMostRecentEvaluationFor(_catalogue).ColumnStates;
        Assert.That(columnStates.Length, Is.EqualTo(5));
        Assert.That(columnStates.Where(c => c.TargetProperty == "Description").First().CountCorrect, Is.EqualTo(2));
    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.PostgreSql)]
    [TestCase(DatabaseType.Oracle)]
    public void TestDEQPostLoad_AddRowAndReplaceRowCheckPeriodicity(DatabaseType dbType)
    {
        SingleTableSetup();
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(
                $"INSERT INTO [Samples] (ID, SampleDate, Description, {SpecialFieldNames.ValidFrom}, {SpecialFieldNames.DataLoadRunID}) VALUES (1, '2016-01-10T12:00:00', 'A', NULL, NULL)",
                connection);
            cmd.ExecuteNonQuery();
        }
        var dqeRepository = new DQERepository(_catalogue.CatalogueRepository);
        Assert.That(dqeRepository.GetAllEvaluationsFor(_catalogue), Is.Empty);
        _catalogue.ValidatorXML = validatorXML;
        var toBeTimePeriodicityCol = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
           .Single(e => e.GetRuntimeName().Equals("SampleDate"));
        _catalogue.TimeCoverage_ExtractionInformation_ID = toBeTimePeriodicityCol.ID;
        _catalogue.SaveToDatabase();
        var report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID)
        {
            ExplicitDQERepository = dqeRepository
        };

        report.Check(ThrowImmediatelyCheckNotifier.Quiet);

        var source = new CancellationTokenSource();
        var listener = new ToMemoryDataLoadEventListener(false);
        report.GenerateReport(_catalogue, listener, source.Token);
        var results = dqeRepository.GetMostRecentEvaluationFor(_catalogue);
        var columnStates = dqeRepository.GetMostRecentEvaluationFor(_catalogue).ColumnStates;
        Assert.That(columnStates.Length, Is.EqualTo(5));
        Assert.That(columnStates.Where(c => c.TargetProperty == "Description").First().CountCorrect, Is.EqualTo(1));
        Assert.That(results, Is.Not.Null);
        using (var connection = (SqlConnection)To.Server.GetConnection())
        {
            connection.Open();

            var cmd = new SqlCommand(
                $"INSERT INTO [Samples] (ID, SampleDate, Description, {SpecialFieldNames.ValidFrom}, {SpecialFieldNames.DataLoadRunID}) VALUES (2, '2025-01-10T12:00:00', 'B', '2016-01-10T12:00:00', 1)",
                connection);
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand(
               $"UPDATE[Samples] SET Description='c', {SpecialFieldNames.ValidFrom}= '2016-01-10T12:00:00', {SpecialFieldNames.DataLoadRunID}=1 WHERE ID=1",
               connection);
            cmd.ExecuteNonQuery();
        }
        var job = Substitute.For<IDataLoadJob>();
        job.RepositoryLocator.Returns(RepositoryLocator);
        job.DataLoadInfo.ID.Returns(1);
        job.LoadMetadata.Returns(_lmd);
        Mutilate(job);
        Assert.That(dqeRepository.GetAllEvaluationsFor(_catalogue).Count, Is.EqualTo(2));
        var periodicityState = PeriodicityState.GetPeriodicityCountsForEvaluation(dqeRepository.GetMostRecentEvaluationFor(_catalogue),false);
        Assert.That(periodicityState.Count, Is.EqualTo(2));
    }

    private void Mutilate(IDataLoadJob job)
    {
        var mutilator = new DQEPostLoadRunner();

        mutilator.Initialize(To, LoadStage.PostLoad);
        mutilator.Check(ThrowImmediatelyCheckNotifier.Quiet);
        mutilator.Mutilate(job);
    }

    private LoadMetadata _lmd;

    private void SingleTableSetup()
    {
        CreateTables("Samples", "ID int NOT NULL, SampleDate DATETIME, Description varchar(1024)", "ID");

        // Set SetUp catalogue entities
        AddTableToCatalogue(DatabaseName, "Samples", "ID", out _, true);

        Assert.That(_catalogue.CatalogueItems, Has.Length.EqualTo(5), "Unexpected number of items in catalogue");
        _lmd = new LoadMetadata(CatalogueRepository, "test");
        _lmd.LinkToCatalogue(_catalogue);
        _lmd.SaveToDatabase();
    }

    private ITableInfo AddTableToCatalogue(string databaseName, string tableName, string pkName,
       out ColumnInfo[] ciList, bool createCatalogue = false)
    {
        var table = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(databaseName)
            .ExpectTable(tableName);
        var resultsImporter = new TableInfoImporter(CatalogueRepository, table);

        resultsImporter.DoImport(out var ti, out ciList);

        var pkResult = ciList.Single(info => info.GetRuntimeName().Equals(pkName));
        pkResult.IsPrimaryKey = true;
        pkResult.SaveToDatabase();

        var forwardEngineer = new ForwardEngineerCatalogue(ti, ciList);
        if (createCatalogue)
            forwardEngineer.ExecuteForwardEngineering(out _catalogue, out _, out _);
        else
            forwardEngineer.ExecuteForwardEngineering(_catalogue);

        return ti;
    }

    private void CreateTables(string tableName, string columnDefinitions, string pkColumn,
      string fkConstraintString = null)
    {
        // todo: doesn't do combo primary keys yet

        if (pkColumn == null || string.IsNullOrWhiteSpace(pkColumn))
            throw new InvalidOperationException("Primary Key column is required.");

        var pkConstraint = $"CONSTRAINT PK_{tableName} PRIMARY KEY ({pkColumn})";
        var stagingTableDefinition = $"{columnDefinitions}, {pkConstraint}";
        var liveTableDefinition =
            $"{columnDefinitions}, hic_validFrom DATETIME, hic_dataLoadRunID int, {pkConstraint}";

        if (fkConstraintString != null)
        {
            stagingTableDefinition += $", {fkConstraintString}";
            liveTableDefinition += $", {fkConstraintString}";
        }


        using (var con = (SqlConnection)From.Server.GetConnection())
        {
            con.Open();
            new SqlCommand($"CREATE TABLE {tableName} ({stagingTableDefinition})", con).ExecuteNonQuery();
        }

        using (var con = (SqlConnection)To.Server.GetConnection())
        {
            con.Open();
            new SqlCommand($"CREATE TABLE {tableName} ({liveTableDefinition})", con).ExecuteNonQuery();
        }
    }

    private string validatorXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Validator xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <ItemValidators>
    <ItemValidator>
      <TargetProperty>SampleDate</TargetProperty>
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>Description</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
  </ItemValidators>
</Validator>";


}
