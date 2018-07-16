using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary.CommandExecution;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataHelper;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Interfaces.Data.DataTables;
using NUnit.Framework;
using ReusableLibraryCode;
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
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(), Has.Count.EqualTo(_columnInfos.Count())); // NO new column added
            Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
            Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("DateOfBirth"));
        }

        [Test]
        public void Test_CatalogueItems_ExtractionInformationMultiPrimaryKey_IsRespected()
        {
            var request = SetupExtractDatasetCommand("ExtractionInformationMultiPrimaryKey_IsRespected", new[] { "PrivateID", "DateOfBirth" });

            var source = new ExecutePkSynthesizerDatasetExtractionSource();
            source.PreInitialize(request, new ThrowImmediatelyDataLoadEventListener());
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(), Has.Count.EqualTo(_columnInfos.Count()));
            Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(2));
            Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("ReleaseID"));
        }

        [Test]
        public void Test_CatalogueItems_NonExtractedPrimaryKey_AreRespected()
        {
            var request = SetupExtractDatasetCommand("NonExtractedPrimaryKey_AreRespected", new string[] { }, new [] { "DateOfBirth" });

            var source = new ExecutePkSynthesizerDatasetExtractionSource();
            source.PreInitialize(request, new ThrowImmediatelyDataLoadEventListener());
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(), Has.Count.EqualTo(_columnInfos.Count() + 1)); // synth PK is added
            Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(1));
            Assert.That(chunk.PrimaryKey.First().ColumnName, Is.EqualTo("SynthesizedPk"));

            var firstvalue = chunk.Rows[0]["SynthesizedPk"].ToString();
            Assert.That(firstvalue, Is.EqualTo("HASHED: 2001-01-01 00:00:00.0000000"));
        }

        [Test]
        public void Test_CatalogueItems_NonExtractedPrimaryKey_MultiTable_IsImpossible()
        {
            var request = SetupExtractDatasetCommand("MultiTable_IsImpossible", new string[] { }, new[] { "DateOfBirth" }, true);

            var source = new ExecutePkSynthesizerDatasetExtractionSource();
            source.PreInitialize(request, new ThrowImmediatelyDataLoadEventListener());
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.That(chunk.PrimaryKey, Is.Not.Null);
            Assert.That(chunk.Columns.Cast<DataColumn>().ToList(), Has.Count.EqualTo(_columnInfos.Count() + 1)); // the "desc" column is added to the existing ones
            Assert.That(chunk.PrimaryKey, Has.Length.EqualTo(0)); // MultiTable does not synthetise a new PK
        }

        private void SetupLookupTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Name");
            dt.Columns.Add("Description");

            dt.Rows.Add(new object[] { "Dave", "Is a maniac" });
            
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("SimpleLookup", dt, new[] { new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 50)) });

            var lookupCata = Import(tbl);

            ExtractionInformation fkEi = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(n => n.GetRuntimeName() == "Name");
            ColumnInfo fk = _catalogue.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "Name");
            ColumnInfo pk = lookupCata.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "Name");

            ColumnInfo descLine1 = lookupCata.GetTableInfoList(false).Single().ColumnInfos.Single(n => n.GetRuntimeName() == "Description");

            var cmd = new ExecuteCommandCreateLookup(CatalogueRepository, fkEi, descLine1, pk, null, true); 
            cmd.Execute();
        }

        private ExtractDatasetCommand SetupExtractDatasetCommand(string testTableName, string[] pkExtractionColumns, string[] pkColumnInfos = null, bool withLookup = false)
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
            _catalogue = Import(tbl, out tableInfo, out columnInfos, out cataItems, out extractionInformations);

            ExtractionInformation privateID = extractionInformations.First(e => e.GetRuntimeName().Equals("PrivateID"));
            privateID.IsExtractionIdentifier = true;
            privateID.SaveToDatabase();

            if (withLookup)
                SetupLookupTable();

            _catalogue.ClearAllInjections();
            extractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

            foreach (var pkExtractionColumn in pkExtractionColumns)
            {
                ExtractionInformation column = extractionInformations.First(e => e.GetRuntimeName().Equals(pkExtractionColumn));
                column.IsPrimaryKey = true;
                column.SaveToDatabase();
            }

            ExtractionConfiguration configuration;
            IExtractableDataSet extractableDataSet;
            Project project;

            SetupDataExport(testTableName, _catalogue,
                            out configuration, out extractableDataSet, out project);

            configuration.Cohort_ID = _extractableCohort.ID;
            configuration.SaveToDatabase();

            return new ExtractDatasetCommand(RepositoryLocator, configuration, new ExtractableDatasetBundle(extractableDataSet));
        }

        private void SetupDataExport(string testDbName, Catalogue catalogue, out ExtractionConfiguration extractionConfiguration, out IExtractableDataSet extractableDataSet, out Project project)
        {
            extractableDataSet = new ExtractableDataSet(DataExportRepository, catalogue);

            project = new Project(DataExportRepository, testDbName);
            project.ProjectNumber = 1;

            Directory.CreateDirectory(@"C:\temp\");
            project.ExtractionDirectory = @"C:\temp\";

            project.SaveToDatabase();

            extractionConfiguration = new ExtractionConfiguration(DataExportRepository, project);
            extractionConfiguration.AddDatasetToConfiguration(extractableDataSet);

            foreach (var ei in _catalogue.GetAllExtractionInformation(ExtractionCategory.Supplemental))
            {
                extractionConfiguration.AddColumnToExtraction(extractableDataSet, ei);   
            }
        }
    }
}