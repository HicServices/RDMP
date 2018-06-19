using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataExportLibrary.Tests.DataExtraction
{
    public class ExtractSourceWithPkSynthesizerTests : TestsRequiringAnExtractionConfiguration
    {
        [Test]
        public void Test_CatalogueItems_ExtractionInformationPrimaryKey_IsRespected()
        {
            var request = SetupExtractDatasetCommand("ExtractionInformationPrimaryKey_IsRespected", new[] { "DateOfBirth" });

            var source = new ExecutePkSynthesizerDatasetExtractionSource();
            source.PreInitialize(request, new ThrowImmediatelyDataLoadEventListener());
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(), Has.Count.EqualTo(_columnInfos.Count() + 1));
            Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
            Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("SynthesizedPk"));

            var firstvalue = chunk.Rows[0]["SynthesizedPk"].ToString();
            Assert.That(firstvalue, Is.EqualTo("HASHED: 2001-01-01 00:00:00.0000000"));
        }

        [Test]
        public void Test_CatalogueItems_ExtractionInformationMultiPrimaryKey_IsRespected()
        {
            var request = SetupExtractDatasetCommand("ExtractionInformationMultiPrimaryKey_IsRespected", new[] { "PrivateID", "DateOfBirth" });

            var source = new ExecutePkSynthesizerDatasetExtractionSource();
            source.PreInitialize(request, new ThrowImmediatelyDataLoadEventListener());
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(), Has.Count.EqualTo(_columnInfos.Count() + 1));
            Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
            Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("SynthesizedPk"));

            var firstvalue = chunk.Rows[0]["SynthesizedPk"].ToString();
            Assert.That(firstvalue, Is.EqualTo("HASHED: 2001-01-01 00:00:00.0000000_" + _cohortKeysGenerated.Values.First()));
        }

        [Test]
        public void Test_CatalogueItems_NonExtractedPrimaryKey_AreRespected()
        {
            var request = SetupExtractDatasetCommand("NonExtractedPrimaryKey_AreRespected", new string[] { }, new [] { "DateOfBirth" });

            var source = new ExecutePkSynthesizerDatasetExtractionSource();
            source.PreInitialize(request, new ThrowImmediatelyDataLoadEventListener());
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(), Has.Count.EqualTo(_columnInfos.Count() + 1));
            Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
            Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("SynthesizedPk"));

            var firstvalue = chunk.Rows[0]["SynthesizedPk"].ToString();
            Assert.That(firstvalue, Is.EqualTo("HASHED: 2001-01-01 00:00:00.0000000"));
        }

        private ExtractDatasetCommand SetupExtractDatasetCommand(string testTableName, string[] pkExtractionColumns, string[] pkColumnInfos = null)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("PrivateID");
            dt.Columns.Add("Name");
            dt.Columns.Add("DateOfBirth");

            if (pkColumnInfos != null)
                dt.PrimaryKey =
                    dt.Columns.Cast<DataColumn>().Where(col => pkColumnInfos.Contains(col.ColumnName)).ToArray();

            dt.Rows.Add(new object[] { _cohortKeysGenerated.Keys.First(), "Dave", "2001-01-01" });

            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable(testTableName, dt, new[] { new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 50)) });

            TableInfo tableInfo;
            ColumnInfo[] columnInfos;
            CatalogueItem[] cataItems;
            ExtractionInformation[] extractionInformations;
            var catalogue = Import(tbl, out tableInfo, out columnInfos, out cataItems, out extractionInformations);

            ExtractionInformation privateID = extractionInformations.First(e => e.GetRuntimeName().Equals("PrivateID"));
            privateID.IsExtractionIdentifier = true;
            privateID.SaveToDatabase();

            foreach (var pkExtractionColumn in pkExtractionColumns)
            {
                ExtractionInformation column = extractionInformations.First(e => e.GetRuntimeName().Equals(pkExtractionColumn));
                column.IsPrimaryKey = true;
                column.SaveToDatabase();
            }

            ExtractionConfiguration configuration;
            var extractableColumns = new List<IColumn>();
            IExtractableDataSet extractableDataSet;
            Project project;

            SetupDataExport(testTableName, catalogue, extractionInformations, extractableColumns, 
                            out configuration, out extractableDataSet, out project);

            configuration.Cohort_ID = _extractableCohort.ID;
            configuration.SaveToDatabase();

            return new ExtractDatasetCommand(RepositoryLocator, configuration, _extractableCohort, new ExtractableDatasetBundle(extractableDataSet),
                                             extractableColumns, new HICProjectSalt(project), "",
                                             new ExtractionDirectory(@"C:\temp\", configuration));
        }

        private void SetupDataExport(string testDbName, Catalogue catalogue, IEnumerable<ExtractionInformation> extractionInformations, List<IColumn> extractableColumns, out ExtractionConfiguration extractionConfiguration, out IExtractableDataSet extractableDataSet, out Project project)
        {
            extractableDataSet = new ExtractableDataSet(DataExportRepository, catalogue);

            project = new Project(DataExportRepository, testDbName);
            project.ProjectNumber = 1;

            Directory.CreateDirectory(@"C:\temp\");
            project.ExtractionDirectory = @"C:\temp\";

            project.SaveToDatabase();

            extractionConfiguration = new ExtractionConfiguration(DataExportRepository, project);

            //select the dataset for extraction under this configuration
            var selectedDataSet = new SelectedDataSets(RepositoryLocator.DataExportRepository, extractionConfiguration, extractableDataSet, null);

            //select all the columns for extraction
            foreach (var toSelect in extractionInformations)
            {
                var col = new ExtractableColumn(DataExportRepository, extractableDataSet, extractionConfiguration, toSelect, toSelect.Order, toSelect.SelectSQL);

                col.IsExtractionIdentifier = toSelect.IsExtractionIdentifier;
                col.IsPrimaryKey = toSelect.IsPrimaryKey;
                col.SaveToDatabase();

                extractableColumns.Add(col);
            }
        }

    }
}