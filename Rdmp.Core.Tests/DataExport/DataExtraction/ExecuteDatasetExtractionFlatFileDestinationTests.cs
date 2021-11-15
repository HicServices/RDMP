using FAnsi;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging;
using ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction
{
    class ExecuteDatasetExtractionFlatFileDestinationTests : TestsRequiringAnExtractionConfiguration
    {
        [TestCase(true)]
        [TestCase(false)]
        public void ExtractionDestination_FloatRounding(bool lotsOfDecimalPlaces)
        {
            var dest = new ExecuteDatasetExtractionFlatFileDestination();

            var dt = new DataTable();
            dt.Columns.Add("Floats", typeof(decimal));

            dt.Rows.Add(Math.PI);

            var lm = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));
            lm.CreateNewLoggingTaskIfNotExists("ExtractionDestination_FloatRounding");

            var dli = lm.CreateDataLoadInfo("ExtractionDestination_FloatRounding", nameof(ExecuteDatasetExtractionFlatFileDestinationTests), "test", "", true);
            
            if(_request.QueryBuilder == null)
            {
                _request.GenerateQueryBuilder();
            }
            dest.RoundFloatsTo = lotsOfDecimalPlaces ? 10 : 2;

            dest.PreInitialize(_request, new ThrowImmediatelyDataLoadEventListener());
            dest.PreInitialize(_project, new ThrowImmediatelyDataLoadEventListener());
            dest.PreInitialize((DataLoadInfo)dli, new ThrowImmediatelyDataLoadEventListener());

            dest.ProcessPipelineData(dt, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            dest.Dispose(new ThrowImmediatelyDataLoadEventListener(),null);

            Assert.IsNotNull(dest.OutputFile);
            FileAssert.Exists(dest.OutputFile);

            if (lotsOfDecimalPlaces)
            {
                Assert.AreEqual($"Floats{Environment.NewLine}3.1415926536{Environment.NewLine}", File.ReadAllText(dest.OutputFile));
            }
            else
            {
                Assert.AreEqual($"Floats{Environment.NewLine}3.14{Environment.NewLine}", File.ReadAllText(dest.OutputFile));
            }
            
        }
    }
}
