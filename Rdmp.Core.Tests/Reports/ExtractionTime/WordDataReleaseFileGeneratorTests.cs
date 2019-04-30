using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Reports.ExtractionTime;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.Reports.ExtractionTime
{
    class WordDataReleaseFileGeneratorTests:TestsRequiringAnExtractionConfiguration
    {
        [Test]
        public void Test_WordDataReleaseFileGenerator_Normal()
        {
            var report = new WordDataReleaseFileGenerator(_configuration,_configuration.DataExportRepository);

            var filename = Path.Combine(TestContext.CurrentContext.WorkDirectory,"release.doc");
            report.GenerateWordFile(filename);

            Assert.IsTrue(File.Exists(filename));
        }
    }
}
