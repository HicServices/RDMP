// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging;
using System;
using System.Data;
using System.IO;
using NUnit.Framework.Legacy;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

internal class ExecuteDatasetExtractionFlatFileDestinationTests : TestsRequiringAnExtractionConfiguration
{
    [TestCase(true)]
    [TestCase(false)]
    public void ExtractionDestination_FloatRounding(bool lotsOfDecimalPlaces)
    {
        var dest = new ExecuteDatasetExtractionFlatFileDestination();

        var dt = new DataTable();
        try
        {
            dt.Columns.Add("Floats", typeof(decimal));

            dt.Rows.Add(Math.PI);

            var lm = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));
            lm.CreateNewLoggingTaskIfNotExists("ExtractionDestination_FloatRounding");

            var dli = lm.CreateDataLoadInfo("ExtractionDestination_FloatRounding",
                nameof(ExecuteDatasetExtractionFlatFileDestinationTests), "test", "", true);

            if (_request.QueryBuilder == null) _request.GenerateQueryBuilder();
            dest.RoundFloatsTo = lotsOfDecimalPlaces ? 10 : 2;

            dest.PreInitialize(_request, ThrowImmediatelyDataLoadEventListener.Quiet);
            dest.PreInitialize(_project, ThrowImmediatelyDataLoadEventListener.Quiet);
            dest.PreInitialize((DataLoadInfo)dli, ThrowImmediatelyDataLoadEventListener.Quiet);

            dest.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
            dest.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);

            Assert.That(dest.OutputFile, Is.Not.Null);
            FileAssert.Exists(dest.OutputFile);

            Assert.That(
    File.ReadAllText(dest.OutputFile), Is.EqualTo(lotsOfDecimalPlaces
                    ? $"Floats{Environment.NewLine}3.1415926536{Environment.NewLine}"
                    : $"Floats{Environment.NewLine}3.14{Environment.NewLine}"));
        }
        finally { dt.Dispose(); }
    }
}