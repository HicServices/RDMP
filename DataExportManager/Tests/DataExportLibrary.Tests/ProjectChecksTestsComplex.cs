using System;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.Tests.DataExtraction;
using DataExportLibrary.Checks;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using NUnit.Framework;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Tests
{
    public class ProjectChecksTestsComplex:TestsRequiringAnExtractionConfiguration
    {
        [Test]
        public void CheckBasicConfiguration()
        {
            new ProjectChecker(RepositoryLocator,_project).Check(new ThrowImmediatelyCheckNotifier { ThrowOnWarning = true });
        }

        [Test]
        public void DatasetIsDisabled()
        {
            _extractableDataSet.DisableExtraction = true;
            _extractableDataSet.SaveToDatabase();

            //checking should fail
            var exception = Assert.Throws<Exception>(() => new ProjectChecker(RepositoryLocator, _project).Check(new ThrowImmediatelyCheckNotifier { ThrowOnWarning = true }));
            Assert.AreEqual("Dataset TestTable is set to DisableExtraction=true, probably someone doesn't want you extracting this dataset at the moment", exception.Message);

            //but if the user goes ahead and executes the extraction that should fail too
            var source = new ExecuteDatasetExtractionSource();
            source.PreInitialize(_request,new ThrowImmediatelyEventsListener());
            var exception2 = Assert.Throws<Exception>(() => source.GetChunk(new ThrowImmediatelyEventsListener(), new GracefulCancellationToken()));

            Assert.AreEqual("Cannot extract TestTable because DisableExtraction is set to true", exception2.Message);
        }
    }
}
