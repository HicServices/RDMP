using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Tests.DataExtraction;
using NUnit.Framework;

namespace DataExportLibrary.Tests.CustomData
{
    public class CustomDataImportingTests : TestsRequiringAnExtractionConfiguration
    {
        [Test]
        public void Extract_ProjectSpecificCatalogue_WholeDataset()
        {
            //make the catalogue a custom catalogue for this project
            CustomExtractableDataSet.Project_ID = _project.ID;
            CustomExtractableDataSet.SaveToDatabase();

            var pipe = SetupPipeline();
            pipe.Name = "Extract_ProjectSpecificCatalogue_WholeDataset Pipe";
            pipe.SaveToDatabase();

            _configuration.AddDatasetToConfiguration(CustomExtractableDataSet);

            try
            {
                _request = new ExtractDatasetCommand(RepositoryLocator,_configuration,new ExtractableDatasetBundle(CustomExtractableDataSet));
                ExtractionPipelineUseCase useCase;
                IExecuteDatasetExtractionDestination results;
                Execute(out useCase, out results);

                var customDataCsv = results.DirectoryPopulated.GetFiles().Single();

                Assert.IsNotNull(customDataCsv);
                Assert.AreEqual("custTable99.csv",customDataCsv.Name);
            
                var lines = File.ReadAllLines(customDataCsv.FullName);

                Assert.AreEqual("SuperSecretThing,ReleaseID",lines[0]);
                Assert.AreEqual("monkeys can all secretly fly,Pub_54321",lines[1]);
                Assert.AreEqual("the wizard of OZ was a man behind a machine,Pub_11ftw",lines[2]);

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
            CustomExtractableDataSet.Project_ID = _project.ID;
            CustomExtractableDataSet.SaveToDatabase();

            var pipe = SetupPipeline();
            pipe.Name = "Extract_ProjectSpecificCatalogue_AppendedColumn Pipe";
            pipe.SaveToDatabase();

            var extraColumn = CustomCatalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific).Single(e => e.GetRuntimeName().Equals("SuperSecretThing"));
            var asExtractable = new ExtractableColumn(DataExportRepository, _extractableDataSet, _configuration, extraColumn, 10,extraColumn.SelectSQL);

            //get rid of any lingering joins
            foreach (JoinInfo j in CatalogueRepository.JoinInfoFinder.GetAllJoinInfos())
                j.DeleteInDatabase();

            //add the ability to join the two tables in the query
            var idCol = _extractableDataSet.Catalogue.GetAllExtractionInformation(ExtractionCategory.Core).Single(c => c.IsExtractionIdentifier).ColumnInfo;
            var otherIdCol = CustomCatalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific).Single(e => e.GetRuntimeName().Equals("PrivateID")).ColumnInfo;
            CatalogueRepository.JoinInfoFinder.AddJoinInfo(idCol, otherIdCol,ExtractionJoinType.Left,null);

            //generate a new request (this will include the newly created column)
            _request = new ExtractDatasetCommand(RepositoryLocator, _configuration, new ExtractableDatasetBundle(_extractableDataSet));

            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("TestTable");
            tbl.Truncate();

            using(var blk = tbl.BeginBulkInsert())
            {
                var dt = new DataTable();
                dt.Columns.Add("PrivateID");
                dt.Columns.Add("Name");
                dt.Columns.Add("DateOfBirth");

                dt.Rows.Add(new object[] {"Priv_12345", "Bob","2001-01-01"});
                dt.Rows.Add(new object[] {"Priv_wtf11", "Frank","2001-10-29"});
                blk.Upload(dt);
            }
            
            ExtractionPipelineUseCase useCase;
            IExecuteDatasetExtractionDestination results;
            Execute(out useCase, out results);

            var mainDataTableCsv = results.DirectoryPopulated.GetFiles().Single();

            Assert.IsNotNull(mainDataTableCsv);
            Assert.AreEqual("TestTable.csv", mainDataTableCsv.Name);
                
            var lines = File.ReadAllLines(mainDataTableCsv.FullName);

            Assert.AreEqual("ReleaseID,Name,DateOfBirth,SuperSecretThing", lines[0]);

            var bobLine = lines.Single(l => l.StartsWith("Pub_54321,Bob"));
            var frankLine = lines.Single(l => l.StartsWith("Pub_11ftw,Frank"));

            Assert.AreEqual("Pub_54321,Bob,2001-01-01,monkeys can all secretly fly", bobLine);
            Assert.AreEqual("Pub_11ftw,Frank,2001-10-29,the wizard of OZ was a man behind a machine", frankLine);

            asExtractable.DeleteInDatabase();
        }

        /// <summary>
        /// Tests that you can reference a custom cohort column in the WHERE Sql of a core dataset in extraction.  Requires you configure a <see cref="JoinInfo"/> and specify a <see cref="SelectedDataSetsForcedJoin"/>
        /// </summary>
        [Test]
        public void Extract_ProjectSpecificCatalogue_FilterReference()
        {
            //make the catalogue a custom catalogue for this project
            CustomExtractableDataSet.Project_ID = _project.ID;
            CustomExtractableDataSet.SaveToDatabase();

            var pipe = SetupPipeline();
            pipe.Name = "Extract_ProjectSpecificCatalogue_FilterReference Pipe";
            pipe.SaveToDatabase();

            var rootContainer = new FilterContainer(DataExportRepository);
            _selectedDataSet.RootFilterContainer_ID = rootContainer.ID;
            _selectedDataSet.SaveToDatabase();

            var filter = new DeployedExtractionFilter(DataExportRepository, "monkeys only", rootContainer);
            filter.WhereSQL = "SuperSecretThing = 'monkeys can all secretly fly'";
            filter.SaveToDatabase();
            rootContainer.AddChild(filter);

            //get rid of any lingering joins
            foreach (JoinInfo j in CatalogueRepository.JoinInfoFinder.GetAllJoinInfos())
                j.DeleteInDatabase();

            //add the ability to join the two tables in the query
            var idCol = _extractableDataSet.Catalogue.GetAllExtractionInformation(ExtractionCategory.Core).Single(c => c.IsExtractionIdentifier).ColumnInfo;
            var otherIdCol = CustomCatalogue.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific).Single(e => e.GetRuntimeName().Equals("PrivateID")).ColumnInfo;
            CatalogueRepository.JoinInfoFinder.AddJoinInfo(idCol, otherIdCol, ExtractionJoinType.Left, null);

            new SelectedDataSetsForcedJoin(DataExportRepository, _selectedDataSet, CustomTableInfo);

            //generate a new request (this will include the newly created column)
            _request = new ExtractDatasetCommand(RepositoryLocator, _configuration, new ExtractableDatasetBundle(_extractableDataSet));
            
            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("TestTable");
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

            ExtractionPipelineUseCase useCase;
            IExecuteDatasetExtractionDestination results;
            Execute(out useCase, out results);

            var mainDataTableCsv = results.DirectoryPopulated.GetFiles().Single();

            Assert.IsNotNull(mainDataTableCsv);
            Assert.AreEqual("TestTable.csv", mainDataTableCsv.Name);

            var lines = File.ReadAllLines(mainDataTableCsv.FullName);

            Assert.AreEqual("ReleaseID,Name,DateOfBirth", lines[0]);
            Assert.AreEqual("Pub_54321,Bob,2001-01-01", lines[1]);
            Assert.AreEqual(2,lines.Length);

            
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

            engine.Source.Dispose(new ThrowImmediatelyDataLoadEventListener(),null );
            engine.Destination.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);

            //batches are 1 record each so 
            Assert.AreEqual(numberOfBatches, listener.LastProgressRecieivedByTaskName["Comitting rows to cohort 99_unitTestDataForCohort_V1fish"].Progress.Value);
            
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
                engine.Source.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
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
                engine.Source.Dispose(new ThrowImmediatelyDataLoadEventListener(), ex);
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

            DataFlowPipelineEngine<DataTable> engine = new DataFlowPipelineEngine<DataTable>(context, source, destination, new ThrowImmediatelyDataLoadEventListener());

            engine.Initialize(_extractableCohort,new FlatFileToLoad(new FileInfo(filename)));
            source.Check(new ThrowImmediatelyCheckNotifier());

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
}
