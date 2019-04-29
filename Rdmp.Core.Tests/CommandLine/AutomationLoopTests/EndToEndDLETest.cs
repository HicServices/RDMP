// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.CommandLine.Options.Abstracts;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine.AutomationLoopTests
{
    public class EndToEndDLETest : DatabaseTests
    {
        [Test]
        public void RunEndToEndDLETest()
        {
            const int timeoutInMilliseconds = 120000;

            var setup = new DLEEndToEndTestSetup(
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn, 
                UnitTestLoggingConnectionString,
                RepositoryLocator,
                DiscoveredServerICanCreateRandomDatabasesAndTablesOn);

            LoadMetadata lmd;
            setup.SetUp(timeoutInMilliseconds,out lmd);

            var auto = new DleRunner(new DleOptions() { LoadMetadata = lmd.ID,Command = CommandLineActivity.run });
            auto.Run(RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(), new ThrowImmediatelyCheckNotifier(), new GracefulCancellationToken());

            setup.VerifyNoErrorsAfterExecutionThenCleanup(timeoutInMilliseconds);
        }
    }
}
