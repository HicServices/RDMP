using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Tests.DataExtraction;
using DataExportLibrary;
using DataExportLibrary.CohortCreationPipeline.Destinations;
using LoadModules.Generic.DataFlowSources;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;


namespace DataExportLibrary.Tests.Cohort
{
    public class CustomDataImportingTests:TestsRequiringACohort
    {

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

            Assert.IsTrue(_extractableCohort.GetCustomTableNames().Count(t=> SqlSyntaxHelper.GetRuntimeName(t).Equals(Path.GetFileNameWithoutExtension(filename)))==1);
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

            Assert.IsTrue(_extractableCohort.GetCustomTableNames().Count(t => SqlSyntaxHelper.GetRuntimeName(t).Equals("fish")) == 1);
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

        #endregion
    }
}
