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
        
        private readonly string _expectedTableName = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest_TestTable";
        
        [Test]
        public void SQLServerDestination()
        {
            DiscoveredDatabase dbToExtractTo = null;

            try
            {
                _configuration.Name = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest";
                _configuration.SaveToDatabase();
                
                ExtractionPipelineUseCase execute;
                IExecuteDatasetExtractionDestination result;

                var dbname = TestDatabaseNames.GetConsistentName(_project.Name + "_" + _project.ProjectNumber);
                dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
                if (dbToExtractTo.Exists())
                    dbToExtractTo.ForceDrop();

                base.Execute(out execute, out result);

                Assert.IsTrue(dbToExtractTo.ExpectTable(_expectedTableName).Exists());

                var server = dbToExtractTo.Server;
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
            //create a target server pointer
            _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver");
            _extractionServer.Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name;
            _extractionServer.SaveToDatabase();

            //create a pipeline
            var pipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline");
            
            //set the destination pipeline
            var component = new PipelineComponent(CatalogueRepository, pipeline, typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>().ToList();
            PipelineComponentArgument argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            PipelineComponentArgument argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            PipelineComponentArgument argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");

            Assert.AreEqual("TargetDatabaseServer", argumentServer.Name);
            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue(TestDatabaseNames.Prefix + "$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            
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
