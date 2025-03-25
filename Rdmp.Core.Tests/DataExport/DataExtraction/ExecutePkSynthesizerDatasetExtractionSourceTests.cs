// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class ExecutePkSynthesizerDatasetExtractionSourceTests : TestsRequiringAnExtractionConfiguration
{
    //C24D365B7C271E2C1BC884B5801C2961
    private readonly Regex reghex = new(@"^HASHED: [A-F\d]{32}");

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        DataExportRepository.DataExportPropertyManager.SetValue(DataExportProperty.HashingAlgorithmPattern,
            "CONCAT('HASHED: ',{0})");
    }

    [Test]
    public void Test_CatalogueItems_ExtractionInformationPrimaryKey_IsRespected()
    {
        var request =
            SetupExtractDatasetCommand("ExtractionInformationPrimaryKey_IsRespected", new[] { "DateOfBirth" });

        var source = new ExecutePkSynthesizerDatasetExtractionSource();
        source.PreInitialize(request, ThrowImmediatelyDataLoadEventListener.Quiet);
        var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(),
                Has.Count.EqualTo(_columnInfos.Length)); // NO new column added
        });
        Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
        Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("DateOfBirth"));
    }

    [Test]
    public void Test_CatalogueItems_ExtractionInformationMultiPrimaryKey_IsRespected()
    {
        var request = SetupExtractDatasetCommand("ExtractionInformationMultiPrimaryKey_IsRespected",
            new[] { "PrivateID", "DateOfBirth" });

        var source = new ExecutePkSynthesizerDatasetExtractionSource();
        source.PreInitialize(request, ThrowImmediatelyDataLoadEventListener.Quiet);
        var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(), Has.Count.EqualTo(_columnInfos.Length));
        });
        Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(2));
        Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("ReleaseID"));
    }

    [Test]
    public void Test_CatalogueItems_NonExtractedPrimaryKey_AreRespected()
    {
        var request = SetupExtractDatasetCommand("NonExtractedPrimaryKey_AreRespected", Array.Empty<string>(),
            new[] { "DateOfBirth" });

        var source = new ExecutePkSynthesizerDatasetExtractionSource();
        source.PreInitialize(request, ThrowImmediatelyDataLoadEventListener.Quiet);
        var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(),
                Has.Count.EqualTo(_columnInfos.Length + 1)); // synth PK is added
        });
        Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
        Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("SynthesizedPk"));

        var firstvalue = chunk.Rows[0]["SynthesizedPk"].ToString();
        Assert.That(reghex.IsMatch(firstvalue));
    }

    [Test]
    public void Test_CatalogueItems_NonExtractedPrimaryKey_MultiTable_PksAreMerged()
    {
        var request = SetupExtractDatasetCommand("MultiTable_PksAreMerged", Array.Empty<string>(),
            new[] { "DateOfBirth" }, true, true);

        var source = new ExecutePkSynthesizerDatasetExtractionSource();
        source.PreInitialize(request, ThrowImmediatelyDataLoadEventListener.Quiet);
        var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(),
                Has.Count.EqualTo(_columnInfos.Length + 3)); // the "desc" column is added to the existing ones
        });
        Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
        Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("SynthesizedPk"));

        var firstvalue = chunk.Rows[0]["SynthesizedPk"].ToString();
        Assert.That(reghex.IsMatch(firstvalue));

        Database.ExpectTable("SimpleLookup").Drop();
        Database.ExpectTable("SimpleJoin").Drop();
    }

    [Test]
    public void Test_CatalogueItems_NonExtractedPrimaryKey_LookupsOnly_IsRespected()
    {
        var request = SetupExtractDatasetCommand("LookupsOnly_IsRespected", Array.Empty<string>(),
            new[] { "DateOfBirth" }, true);

        var source = new ExecutePkSynthesizerDatasetExtractionSource();
        source.PreInitialize(request, ThrowImmediatelyDataLoadEventListener.Quiet);
        var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(),
                Has.Count.EqualTo(_columnInfos.Length +
                                  2)); // the "desc" column is added to the existing ones + the SynthPk
        });
        Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
        Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("SynthesizedPk"));

        var firstvalue = chunk.Rows[0]["SynthesizedPk"].ToString();
        Assert.That(reghex.IsMatch(firstvalue));

        Database.ExpectTable("SimpleLookup").Drop();
    }

    private void SetupJoin()
    {
        var dt = new DataTable();

        dt.Columns.Add("Name");
        dt.Columns.Add("Description");

        dt.Rows.Add("Dave", "Is a maniac");

        var tbl = Database.CreateTable("SimpleJoin", dt,
            new[]
            {
                new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 50)) { IsPrimaryKey = true }
            });

        var lookupCata = Import(tbl);

        var fkEi = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(n => n.GetRuntimeName() == "Name");
        var pk = lookupCata.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "Name");

        new JoinInfo(CatalogueRepository, fkEi.ColumnInfo, pk, ExtractionJoinType.Left, null);

        var ci = new CatalogueItem(CatalogueRepository, _catalogue, "Name_2");
        var ei = new ExtractionInformation(CatalogueRepository, ci, pk, pk.Name)
        {
            Alias = "Name_2"
        };
        ei.SaveToDatabase();
    }

    private void SetupLookupTable()
    {
        var dt = new DataTable();

        dt.Columns.Add("Name");
        dt.Columns.Add("Description");

        dt.Rows.Add("Dave", "Is a maniac");

        var tbl = Database.CreateTable("SimpleLookup", dt,
            new[] { new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 50)) });

        var lookupCata = Import(tbl);

        var fkEi = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(n => n.GetRuntimeName() == "Name");
        var pk = lookupCata.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "Name");

        var descLine1 = lookupCata.GetTableInfoList(false).Single().ColumnInfos
            .Single(n => n.GetRuntimeName() == "Description");

        var cmd = new ExecuteCommandCreateLookup(CatalogueRepository, fkEi, descLine1, pk, null, true);
        cmd.Execute();
    }

    private ExtractDatasetCommand SetupExtractDatasetCommand(string testTableName, string[] pkExtractionColumns,
        string[] pkColumnInfos = null, bool withLookup = false, bool withJoin = false)
    {
        var dt = new DataTable();

        dt.Columns.Add("PrivateID");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");

        if (pkColumnInfos != null)
            dt.PrimaryKey =
                dt.Columns.Cast<DataColumn>().Where(col => pkColumnInfos.Contains(col.ColumnName)).ToArray();

        dt.Rows.Add(_cohortKeysGenerated.Keys.First(), "Dave", "2001-01-01");

        var tbl = Database.CreateTable(testTableName,
            dt,
            new[] { new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 50)) });

        _catalogue = Import(tbl, out _, out _, out _,
            out var extractionInformations);

        var privateID = extractionInformations.First(e => e.GetRuntimeName().Equals("PrivateID"));
        privateID.IsExtractionIdentifier = true;
        privateID.SaveToDatabase();

        if (withLookup)
            SetupLookupTable();

        if (withJoin)
            SetupJoin();

        _catalogue.ClearAllInjections();
        extractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

        foreach (var pkExtractionColumn in pkExtractionColumns)
        {
            var column = extractionInformations.First(e => e.GetRuntimeName().Equals(pkExtractionColumn));
            column.IsPrimaryKey = true;
            column.SaveToDatabase();
        }

        SetupDataExport(testTableName, _catalogue,
            out var configuration, out var extractableDataSet, out _);

        configuration.Cohort_ID = _extractableCohort.ID;
        configuration.SaveToDatabase();

        return new ExtractDatasetCommand(configuration, new ExtractableDatasetBundle(extractableDataSet));
    }

    private void SetupDataExport(string testDbName, ICatalogue catalogue,
        out ExtractionConfiguration extractionConfiguration, out IExtractableDataSet extractableDataSet,
        out IProject project)
    {
        extractableDataSet = new ExtractableDataSet(DataExportRepository, catalogue);

        project = new Project(DataExportRepository, testDbName)
        {
            ProjectNumber = 1
        };

        Directory.CreateDirectory(ProjectDirectory);
        project.ExtractionDirectory = ProjectDirectory;

        project.SaveToDatabase();

        extractionConfiguration = new ExtractionConfiguration(DataExportRepository, project);
        extractionConfiguration.AddDatasetToConfiguration(extractableDataSet);

        foreach (var ei in _catalogue.GetAllExtractionInformation(ExtractionCategory.Supplemental))
            extractionConfiguration.AddColumnToExtraction(extractableDataSet, ei);
    }
}