using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Checks;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.LoadExecution;
using NUnit.Framework;
using RDMPAutomationService.Logic.DLE;
using RDMPAutomationService.Options;
using RDMPAutomationService.Runners;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace RDMPAutomationServiceTests.AutomationLoopTests
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
