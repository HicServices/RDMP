using System.IO;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.Reports.ExtractionTime;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.Reports.ExtractionTime
{
    internal class DatasetVariableReportGeneratorTests: TestsRequiringAnExtractionConfiguration
    {
        [Test]
        public void Test_DatasetVariableReportGenerator_Creation()
        {
            this.Execute(out ExtractionPipelineUseCase pipelineUseCase,out var results);
            var report = new DatasetVariableReportGenerator(pipelineUseCase);
            report.GenerateDatasetVariableReport();
            var filename = Path.Join(
                pipelineUseCase.Destination.DirectoryPopulated.FullName,
                $"{pipelineUseCase.Destination.GetFilename()}Variables.csv"
            );
            Assert.That(File.Exists(filename));
        }
    }
}
