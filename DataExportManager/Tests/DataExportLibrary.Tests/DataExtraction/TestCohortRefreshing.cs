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
            ExtractionPipelineHost host;
            IExecuteDatasetExtractionDestination results;

            Execute(out host,out results);

            var oldcohort = _configuration.Cohort;
            
            var engine = new CohortRefreshEngine(new ToConsoleDataLoadEventReceiver(), _configuration, CatalogueRepository.MEF);
            
            Assert.NotNull(engine.Request.NewCohortDefinition);
            
            var oldData = oldcohort.GetExternalData();

            Assert.AreEqual(oldData.ExternalDescription, engine.Request.NewCohortDefinition.Description);
            Assert.AreEqual(oldData.ExternalVersion + 1, engine.Request.NewCohortDefinition.Version);
        }
    }
}