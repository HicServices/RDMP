using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Tests.DataExtraction
{
    public class EmptyDataExtractionTests:TestsRequiringAnExtractionConfiguration
    {

        private void TruncateDataTable()
        {
            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = server.GetConnection())
            {
                con.Open();

                var cmdTruncate = server.GetCommand("TRUNCATE TABLE TestTable",con);
                cmdTruncate.ExecuteNonQuery();

                con.Close();
            }

        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void TestAllowingEmptyDatasets(bool allowEmptyDatasetExtractions)
        {
            Pipeline p = SetupPipeline();
            
            TruncateDataTable();

            var host = new ExtractionPipelineHost();

            var engine = host.GetEngine(p, new ThrowImmediatelyDataLoadEventListener());
            host.Source.AllowEmptyExtractions = allowEmptyDatasetExtractions;

            var token = new GracefulCancellationToken();
            
            if(allowEmptyDatasetExtractions)
            {

                var dt = host.Source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), token);
                Assert.IsNull(host.Source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), token));

                Assert.AreEqual(0,dt.Rows.Count);
                Assert.AreEqual(2, dt.Columns.Count);
            }
            else
            {
                var exception = Assert.Throws<Exception>(() => host.Source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), token));

                Assert.IsTrue(exception.Message.StartsWith("There is no data to load, query returned no rows, query was"));
            }

            p.DeleteInDatabase();
        }

        [Test]
        public void ProducesEmptyCSV()
        {
            TruncateDataTable();
            AllowEmptyExtractions = true;

            ExtractionPipelineHost execute;
            IExecuteDatasetExtractionDestination result;

            Assert.AreEqual(1, _request.ColumnsToExtract.Count(c => c.IsExtractionIdentifier));

            base.Execute(out execute, out result);

            var r = (ExecuteDatasetExtractionFlatFileDestination)result;

            //this should be what is in the file, the private identifier and the 1 that was put into the table in the first place (see parent class for the test data setup)
            Assert.AreEqual(@"ReleaseID,Result", File.ReadAllText(r.OutputFile).Trim());

            Assert.AreEqual(1, _request.QueryBuilder.SelectColumns.Count(c => c.IColumn is ReleaseIdentifierSubstitution));
            File.Delete(r.OutputFile);
        }
    }
}