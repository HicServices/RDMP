using CatalogueLibrary.Data.Cohort;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.Tests.DataExtraction
{
    public class TestCohortRefreshing : TestsRequiringAnExtractionConfiguration
    {

        [Test]
        public void RefreshCohort()
        {
            ExtractionPipelineUseCase useCase;
            IExecuteDatasetExtractionDestination results;

            var pipe = SetupPipeline();
            pipe.Name = "RefreshPipe";
            pipe.SaveToDatabase();

            Execute(out useCase,out results);

            var oldcohort = _configuration.Cohort;


            _configuration.CohortIdentificationConfiguration_ID =new CohortIdentificationConfiguration(RepositoryLocator.CatalogueRepository, "RefreshCohort.cs").ID;
            _configuration.CohortRefreshPipeline_ID = pipe.ID;
            _configuration.SaveToDatabase();

            var engine = new CohortRefreshEngine(new ThrowImmediatelyDataLoadEventListener(), _configuration);
            
            Assert.NotNull(engine.Request.NewCohortDefinition);
            
            var oldData = oldcohort.GetExternalData();

            Assert.AreEqual(oldData.ExternalDescription, engine.Request.NewCohortDefinition.Description);
            Assert.AreEqual(oldData.ExternalVersion + 1, engine.Request.NewCohortDefinition.Version);
        }
    }
}