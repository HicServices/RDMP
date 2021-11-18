// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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

            dt.Dispose();
        }
    }
}
