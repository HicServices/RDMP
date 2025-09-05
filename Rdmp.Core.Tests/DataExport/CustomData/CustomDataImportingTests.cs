// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.CustomData;

public class CustomDataImportingTests : TestsRequiringAnExtractionConfiguration
{
    [Test]
    public void Extract_ProjectSpecificCatalogue_WholeDataset()
    {
        //make the catalogue a custom catalogue for this project
        CustomExtractableDataSet.Projects.Add(_project);
        CustomExtractableDataSet.SaveToDatabase();

        var pipe = SetupPipeline();
        pipe.Name = "Extract_ProjectSpecificCatalogue_WholeDataset Pipe";
        pipe.SaveToDatabase();

        _configuration.AddDatasetToConfiguration(CustomExtractableDataSet);

        try
        {
            _request = new ExtractDatasetCommand(_configuration,
                new ExtractableDatasetBundle(CustomExtractableDataSet));
            Execute(out _, out var results);

            var customDataCsv = results.DirectoryPopulated.GetFiles().Single(f => f.Name.Equals("custTable99.csv"));

            Assert.That(customDataCsv, Is.Not.Null);

            var lines = File.ReadAllLines(customDataCsv.FullName);

            Assert.Multiple(() =>
            {
                Assert.That(lines[0], Is.EqualTo("SuperSecretThing,ReleaseID"));
                Assert.That(lines[1], Is.EqualTo("monkeys can all secretly fly,Pub_54321"));
                Assert.That(lines[2], Is.EqualTo("the wizard of OZ was a man behind a machine,Pub_11ftw"));
            });
        }
        finally
        {
            _configuration.RemoveDatasetFromConfiguration(CustomExtractableDataSet);
        }
    }


    /// <summary>
    /// Tests that you can add a custom cohort column on the end of an existing dataset as an append.  Requires you configure a JoinInfo
    /// </summary>
    [Test]
    public void Extract_ProjectSpecificCatalogue_AppendedColumn()
    {
        //make the catalogue a custom catalogue for this project
        CustomExtractableDataSet.Projects.Add(_project);
        CustomExtractableDataSet.SaveToDatabase();

        var pipe = SetupPipeline();
        pipe.Name = "Extract_ProjectSpecificCatalogue_AppendedColumn Pipe";
        pipe.SaveToDatabase();

        var extraColumn = CustomCatalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific)
            .Single(e => e.GetRuntimeName().Equals("SuperSecretThing"));
        var asExtractable = new ExtractableColumn(DataExportRepository, _extractableDataSet, _configuration,
            extraColumn, 10, extraColumn.SelectSQL);

        //get rid of any lingering joins
        foreach (var j in CatalogueRepository.GetAllObjects<JoinInfo>())
            j.DeleteInDatabase();

        //add the ability to join the two tables in the query
        var idCol = _extractableDataSet.Catalogue.GetAllExtractionInformation(ExtractionCategory.Core)
            .Single(c => c.IsExtractionIdentifier).ColumnInfo;
        var otherIdCol = CustomCatalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific)
            .Single(e => e.GetRuntimeName().Equals("PrivateID")).ColumnInfo;
        new JoinInfo(CatalogueRepository, idCol, otherIdCol, ExtractionJoinType.Left, null);

        //generate a new request (this will include the newly created column)
        _request = new ExtractDatasetCommand(_configuration, new ExtractableDatasetBundle(_extractableDataSet));

        var tbl = Database.ExpectTable("TestTable");
        tbl.Truncate();

        using (var blk = tbl.BeginBulkInsert())
        {
            var dt = new DataTable();
            dt.Columns.Add("PrivateID");
            dt.Columns.Add("Name");
            dt.Columns.Add("DateOfBirth");

            dt.Rows.Add(new object[] { "Priv_12345", "Bob", "2001-01-01" });
            dt.Rows.Add(new object[] { "Priv_wtf11", "Frank", "2001-10-29" });
            blk.Upload(dt);
        }

        Execute(out _, out var results);

        var mainDataTableCsv = results.DirectoryPopulated.GetFiles().Single(f => f.Name.Equals("TestTable.csv"));

        Assert.That(mainDataTableCsv, Is.Not.Null);
        Assert.That(mainDataTableCsv.Name, Is.EqualTo("TestTable.csv"));

        var lines = File.ReadAllLines(mainDataTableCsv.FullName);

        Assert.That(lines[0], Is.EqualTo("ReleaseID,Name,DateOfBirth,SuperSecretThing"));

        var bobLine = lines.Single(l => l.StartsWith("Pub_54321,Bob"));
        var frankLine = lines.Single(l => l.StartsWith("Pub_11ftw,Frank"));

        Assert.Multiple(() =>
        {
            Assert.That(bobLine, Is.EqualTo("Pub_54321,Bob,2001-01-01,monkeys can all secretly fly"));
            Assert.That(frankLine, Is.EqualTo("Pub_11ftw,Frank,2001-10-29,the wizard of OZ was a man behind a machine"));
        });

        asExtractable.DeleteInDatabase();
    }

    /// <summary>
    /// Tests that you can reference a custom cohort column in the WHERE Sql of a core dataset in extraction.  Requires you configure a <see cref="JoinInfo"/> and specify a <see cref="SelectedDataSetsForcedJoin"/>
    /// </summary>
    [Test]
    public void Extract_ProjectSpecificCatalogue_FilterReference()
    {
        //make the catalogue a custom catalogue for this project
        CustomExtractableDataSet.Projects.Add(_project);
        CustomExtractableDataSet.SaveToDatabase();

        var pipe = SetupPipeline();
        pipe.Name = "Extract_ProjectSpecificCatalogue_FilterReference Pipe";
        pipe.SaveToDatabase();

        var rootContainer = new FilterContainer(DataExportRepository);
        _selectedDataSet.RootFilterContainer_ID = rootContainer.ID;
        _selectedDataSet.SaveToDatabase();

        var filter = new DeployedExtractionFilter(DataExportRepository, "monkeys only", rootContainer)
        {
            WhereSQL = "SuperSecretThing = 'monkeys can all secretly fly'"
        };
        filter.SaveToDatabase();
        rootContainer.AddChild(filter);

        //get rid of any lingering joins
        foreach (var j in CatalogueRepository.GetAllObjects<JoinInfo>())
            j.DeleteInDatabase();

        //add the ability to join the two tables in the query
        var idCol = _extractableDataSet.Catalogue.GetAllExtractionInformation(ExtractionCategory.Core)
            .Single(c => c.IsExtractionIdentifier).ColumnInfo;
        var otherIdCol = CustomCatalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific)
            .Single(e => e.GetRuntimeName().Equals("PrivateID")).ColumnInfo;
        new JoinInfo(CatalogueRepository, idCol, otherIdCol, ExtractionJoinType.Left, null);

        new SelectedDataSetsForcedJoin(DataExportRepository, _selectedDataSet, CustomTableInfo);

        //generate a new request (this will include the newly created column)
        _request = new ExtractDatasetCommand(_configuration, new ExtractableDatasetBundle(_extractableDataSet));

        var tbl = Database.ExpectTable("TestTable");
        tbl.Truncate();

        using (var blk = tbl.BeginBulkInsert())
        {
            var dt = new DataTable();
            dt.Columns.Add("PrivateID");
            dt.Columns.Add("Name");
            dt.Columns.Add("DateOfBirth");

            dt.Rows.Add(new object[] { "Priv_12345", "Bob", "2001-01-01" });
            dt.Rows.Add(new object[] { "Priv_wtf11", "Frank", "2001-10-29" });
            blk.Upload(dt);
        }

        Execute(out _, out var results);

        var mainDataTableCsv = results.DirectoryPopulated.GetFiles().Single(f => f.Name.Equals("TestTable.csv"));

        Assert.That(mainDataTableCsv, Is.Not.Null);

        var lines = File.ReadAllLines(mainDataTableCsv.FullName);

        Assert.Multiple(() =>
        {
            Assert.That(lines[0], Is.EqualTo("ReleaseID,Name,DateOfBirth"));
            Assert.That(lines[1], Is.EqualTo("Pub_54321,Bob,2001-01-01"));
            Assert.That(lines, Has.Length.EqualTo(2));
        });
    }


    /*
    private List<string> _customTablesToCleanup = new List<string>();

    [Test]
    public void CSVImportPipeline()
    {
        var customData = GetCustomData();
        string filename = "CustomDataImportingTests.csv";
        File.WriteAllText(filename, customData);

        var engine = GetEnginePointedAtFile(filename);
        engine.ExecutePipeline(new GracefulCancellationToken());

        var customTableNames = _extractableCohort.GetCustomTableNames().ToArray();

        Console.WriteLine("Found the following custom tables:");
        foreach (string tableName in customTableNames)
            Console.WriteLine(tableName);

        var syntax = _extractableCohort.GetQuerySyntaxHelper();

        Assert.IsTrue(_extractableCohort.GetCustomTableNames().Count(t => syntax.GetRuntimeName(t).Equals(Path.GetFileNameWithoutExtension(filename))) == 1);
        _extractableCohort.DeleteCustomData(Path.GetFileNameWithoutExtension(filename));

        File.Delete(filename);
    }



    [Test]
    [TestCase(1)]
    [TestCase(10)]
    public void IterativeBatchLoadingTest(int numberOfBatches)
    {

        //will actually be ignored in place of us manually firing batches into the destination
        var customData = GetCustomData();
        string filename = "fish.txt";
        File.WriteAllText(filename, customData);

        var engine = GetEnginePointedAtFile("fish.txt");

        ToMemoryDataLoadEventListener listener = new ToMemoryDataLoadEventListener(true);

        Random r = new Random();
        var token = new GracefulCancellationTokenSource();

        for (int i = 0; i < numberOfBatches; i++)
        {
            DataTable dt = new DataTable();
            dt.TableName = "fish";
            dt.Columns.Add("PrivateID");
            dt.Columns.Add("Age");

            dt.Rows.Add(_cohortKeysGenerated.Keys.First(),r.Next(100));
            engine.Destination.ProcessPipelineData( dt,listener,token.Token);
        }

        //then give them the null
        engine.Destination.ProcessPipelineData( null,listener, token.Token);

        engine.Source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet,null );
        engine.Destination.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);

        //batches are 1 record each so
        Assert.AreEqual(numberOfBatches, listener.LastProgressRecieivedByTaskName["Committing rows to cohort 99_unitTestDataForCohort_V1fish"].Progress.Value);

        var customTableNames = _extractableCohort.GetCustomTableNames().ToArray();
        Console.WriteLine("Found the following custom tables:");
        foreach (string tableName in customTableNames)
            Console.WriteLine(tableName);

        var syntax = _extractableCohort.GetQuerySyntaxHelper();

        Assert.IsTrue(_extractableCohort.GetCustomTableNames().Count(t => syntax.GetRuntimeName(t).Equals("fish")) == 1);
        _extractableCohort.DeleteCustomData("fish");

        File.Delete("fish.txt");
    }

    [Test]
    [ExpectedException(ExpectedMessage = "Cohort Private Identifier PrivateID not found in DataTable" )]
    public void CSVImportPipeline_MissingPrivateIdentifier()
    {
        Exception ex = null;
        string filename = "CSVImportPipeline_MissingPrivateIdentifier.csv";

        File.WriteAllText(filename, GetCustomData().Replace("PrivateID", "NHSNumber"));

        var engine = GetEnginePointedAtFile(filename);

        try
        {
            try
            {
                engine.ExecutePipeline(new GracefulCancellationToken());
            }
            catch (Exception e)
            {
                ex = e;
                Console.WriteLine(e.ToString());
                Assert.IsTrue(e.InnerException.Message.StartsWith("Last minute checks (just before committing to the database) f"));
                Assert.NotNull(e.InnerException);
                throw e.InnerException.InnerException;
            }
        }
        finally
        {
            engine.Source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, ex);
            File.Delete(filename);
        }
    }
    [Test]
    public void CSVImportPipeline_ReleaseIdentifiersButNoPrivateIdentifier()
    {
        Exception ex = null;
        string filename = "CSVImportPipeline_MissingPrivateIdentifier.csv";

        File.WriteAllText(filename, GetCustomData_ButWithReleaseIdentifiers());

        var engine = GetEnginePointedAtFile(filename);

        try
        {
            engine.ExecutePipeline(new GracefulCancellationToken());
        }
        catch (Exception e)
        {
            ex = e;
        }
        finally
        {
            engine.Source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, ex);
            File.Delete(filename);
        }
    }

    #region Helper methods
    private DataFlowPipelineEngine<DataTable> GetEnginePointedAtFile(string filename)
    {
        var source = new DelimitedFlatFileDataFlowSource
        {
            Separator = ",",
            IgnoreBlankLines = true,
            UnderReadBehaviour = BehaviourOnUnderReadType.AppendNextLineToCurrentRow,
            MakeHeaderNamesSane = true,
            StronglyTypeInputBatchSize = -1,
            StronglyTypeInput = true
        };

        CustomCohortDataDestination destination = new CustomCohortDataDestination();

        var context = new DataFlowPipelineContextFactory<DataTable>().Create(
            PipelineUsage.FixedDestination |
            PipelineUsage.LogsToTableLoadInfo |
            PipelineUsage.LoadsSingleTableInfo |
            PipelineUsage.LoadsSingleFlatFile);

        DataFlowPipelineEngine<DataTable> engine = new DataFlowPipelineEngine<DataTable>(context, source, destination, ThrowImmediatelyDataLoadEventListener.Quiet);

        engine.Initialize(_extractableCohort,new FlatFileToLoad(new FileInfo(filename)));
        source.Check(ThrowImmediatelyCheckNotifier.Quiet);

        return engine;
    }

    private string GetCustomData()
    {
        string customData = "PrivateID,Age" + Environment.NewLine;

        int[] ages = {30, 35, 40};

        var privateIdentifiers = _cohortKeysGenerated.Keys.Take(3).ToArray();//keys = privateIDs

        for (int i = 0; i < privateIdentifiers.Length; i++)
            customData += privateIdentifiers[i] + "," + ages[i] + Environment.NewLine;

        return customData;
    }
    private string GetCustomData_ButWithReleaseIdentifiers()
    {
        string customData = "ReleaseID,Age" + Environment.NewLine;

        int[] ages = { 30, 35, 40 };

        var privateIdentifiers = _cohortKeysGenerated.Values.Take(3).ToArray();//note that in this like we take values not keys because values of this dictionary are ReleaseIDs while keys are PrivateIDs

        for (int i = 0; i < privateIdentifiers.Length; i++)
            customData += privateIdentifiers[i] + "," + ages[i] + Environment.NewLine;

        return customData;
    }

    #endregion*/
}