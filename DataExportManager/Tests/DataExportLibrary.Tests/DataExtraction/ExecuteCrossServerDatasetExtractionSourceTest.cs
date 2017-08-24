using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data.Pipelines;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using NUnit.Framework;

namespace DataExportLibrary.Tests.DataExtraction
{
    public class ExecuteCrossServerDatasetExtractionSourceTest : TestsRequiringAnExtractionConfiguration
    {
        [Test]
        public void CrossServerExtraction()
        {
            ExtractionPipelineHost execute;
            IExecuteDatasetExtractionDestination result;

            base.Execute(out execute, out result);

            var r = (ExecuteDatasetExtractionFlatFileDestination)result;

            //this should be what is in the file, the private identifier and the 1 that was put into the table in the first place (see parent class for the test data setup)
            Assert.AreEqual(@"ReleaseID,Result
" + _cohortKeysGenerated[_cohortKeysGenerated.Keys.First()] + @",1", File.ReadAllText(r.OutputFile).Trim());

            File.Delete(r.OutputFile);
        }

        protected override Pipeline SetupPipeline()
        {
            var pipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline");
            var component = new PipelineComponent(CatalogueRepository, pipeline, typeof(ExecuteDatasetExtractionFlatFileDestination), 0, "Destination");
            var arguments = component.CreateArgumentsForClassIfNotExists<ExecuteDatasetExtractionFlatFileDestination>().ToArray();
            
            if (arguments.Length != 2)
                throw new Exception("Expected only 2 arguments for type ExecuteDatasetExtractionFlatFileDestination, did somebody add another [DemandsInitialization]? if so handle it below");

            arguments.Single(a => a.Name.Equals("DateFormat")).SetValue("yyyy-MM-dd");
            arguments.Single(a => a.Name.Equals("DateFormat")).SaveToDatabase();

            arguments.Single(a=>a.Name.Equals("FlatFileType")).SetValue(ExecuteExtractionToFlatFileType.CSV);
            arguments.Single(a=>a.Name.Equals("FlatFileType")).SaveToDatabase();
            
            
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

        [Test]
        public void HackSQLTest_Normal()
        {
            string input = @"
 DECLARE @CohortDefinitionID AS int;
SET @CohortDefinitionID=69;
--The project number of project Project - 2387 GO Share
DECLARE @ProjectNumber as int;
SET @ProjectNumber=2387;

SELECT DISTINCT 
Cohort.dbo.Cohort.PROCHI AS PROCHI,
[Aggregation]..[PatientEvents].[Date],
[Aggregation]..[PatientEvents].[Source_ID],
lookup_1.[Name] AS Source_ID_Desc,
[Aggregation]..[PatientEvents].[EventType],
[Aggregation]..[PatientEvents].[Value]
FROM 
[Aggregation]..[PatientEvents] Left JOIN [Aggregation]..[Source] AS lookup_1 ON [Aggregation]..[PatientEvents].[Source_ID] = lookup_1.[ID]
 INNER JOIN [Cohort]..[Cohort] ON [Aggregation]..[PatientEvents].[CHI]=[Cohort]..[Cohort].[CHI] collate Latin1_General_BIN

WHERE
(
--2001 data only
YEAR([Aggregation]..[PatientEvents].[Date]) = 2001
)
AND
[Cohort]..[Cohort].[cohortDefinition_id]=69
";

            string expectedOutput = @"DECLARE @CohortDefinitionID AS int;
SET @CohortDefinitionID=69;
--The project number of project Project - 2387 GO Share
DECLARE @ProjectNumber as int;
SET @ProjectNumber=2387;

SELECT DISTINCT 
tempdb..Cohort.PROCHI AS PROCHI,
[Aggregation]..[PatientEvents].[Date],
[Aggregation]..[PatientEvents].[Source_ID],
lookup_1.[Name] AS Source_ID_Desc,
[Aggregation]..[PatientEvents].[EventType],
[Aggregation]..[PatientEvents].[Value]
FROM 
[Aggregation]..[PatientEvents] Left JOIN [Aggregation]..[Source] AS lookup_1 ON [Aggregation]..[PatientEvents].[Source_ID] = lookup_1.[ID]
 INNER JOIN tempdb..[Cohort] ON [Aggregation]..[PatientEvents].[CHI]=tempdb..[Cohort].[CHI] collate Latin1_General_BIN

WHERE
(
--2001 data only
YEAR([Aggregation]..[PatientEvents].[Date]) = 2001
)
AND
tempdb..[Cohort].[cohortDefinition_id]=69
";
            var e = DataExportRepository.GetObjectByID<ExternalCohortTable>(_request.ExtractableCohort.ExternalCohortTable_ID);
            string origValue = e.Database;

            e.Database = "Cohort";
            e.SaveToDatabase();
            try
            {
                ExecuteCrossServerDatasetExtractionSource s = new ExecuteCrossServerDatasetExtractionSource();
                s.PreInitialize(_request,new ThrowImmediatelyEventsListener());
                string hacked = s.HackExtractionSQL(input, new ThrowImmediatelyEventsListener());

                Assert.AreEqual(expectedOutput.Trim(),hacked.Trim());
            }
            finally
            {
                e.Database = origValue;
                e.SaveToDatabase();
            }
        }





     }
}
