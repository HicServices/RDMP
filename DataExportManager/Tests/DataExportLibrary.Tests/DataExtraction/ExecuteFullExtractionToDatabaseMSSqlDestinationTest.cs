using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.Operations;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace DataExportLibrary.Tests.DataExtraction
{
    public class ExecuteFullExtractionToDatabaseMSSqlDestinationTest :TestsRequiringAnExtractionConfiguration
    {
        private ExternalDatabaseServer _extractionServer;
        
        private readonly string _expectedTableName = TestDatabaseNames.GetConsistentName("ExtractionConfiguration") + "_1_ExecuteFullExtractionToDatabaseMSSqlDestinationTest_TestTable";

        //The database that the extracted records will appear in
        DiscoveredDatabase dbToExtractTo;

        [Test]
        public void SQLServerDestination()
        {
            try
            {
                _configuration.Name = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest";
                _configuration.SaveToDatabase();
                
                ExtractionPipelineHost execute;
                IExecuteDatasetExtractionDestination result;

                base.Execute(out execute, out result);

                var extractionDatabase = DataAccessPortal.GetInstance().ExpectDatabase(_extractionServer, DataAccessContext.DataExport);
                
                Assert.IsTrue( extractionDatabase.ExpectTable(_expectedTableName).Exists());

                var server = extractionDatabase.Server;
                using (var con = server.GetConnection())
                {
                    con.Open();
                    var cmd = server.GetCommand("SELECT * FROM " + _expectedTableName, con);

                    DataTable dt = new DataTable();

                    var da = server.GetDataAdapter(cmd);
                    da.Fill(dt);

                    Assert.AreEqual(1,dt.Rows.Count);

                    Assert.AreEqual(_cohortKeysGenerated[_cohortKeysGenerated.Keys.First()].Trim(), ((string)dt.Rows[0]["ReleaseID"]).Trim());
                    Assert.AreEqual(1, dt.Rows[0]["Result"]);

                }

                foreach (var table in extractionDatabase.DiscoverTables(true))
                {
                    Console.WriteLine("Dropping table " + table.GetFullyQualifiedName());
                    table.Drop();
                }

                Console.WriteLine("Dropping database " + extractionDatabase.GetRuntimeName());
                extractionDatabase.Drop();
            }
            finally
            {
                if(_extractionServer != null)
                    _extractionServer.DeleteInDatabase();

                if(dbToExtractTo != null)
                    dbToExtractTo.ForceDrop();
            }
        }
        
        
        protected override Pipeline SetupPipeline()
        {
            var dbname = TestDatabaseNames.GetConsistentName("ExtractionTest");
            dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);

            //create a target server pointer
            _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "ExtractionTest");
            _extractionServer.SetProperties(dbToExtractTo);
            
            dbToExtractTo.Create(true);

            //create a pipeline
            var pipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline");
            
            //set the destination pipeline
            var component = new PipelineComponent(CatalogueRepository, pipeline, typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
            PipelineComponentArgument argument = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>().Single();

            Assert.AreEqual("TargetDatabaseServer", argument.Name);
            argument.SetValue(_extractionServer);
            argument.SaveToDatabase();
            
            var component2 = new PipelineComponent(CatalogueRepository, pipeline, typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>().ToArray();
            arguments2.Single(a=>a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            pipeline.DestinationPipelineComponent_ID = component.ID;
            pipeline.SourcePipelineComponent_ID = component2.ID;
            pipeline.SaveToDatabase();

            return pipeline;
        }
    }
}
