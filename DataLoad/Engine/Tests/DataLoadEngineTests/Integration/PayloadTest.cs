using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnonymisationTests;
using CatalogueLibrary;
using CatalogueLibrary.CommandExecution.AtomicCommands.PluginCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Attachers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using Diagnostics;
using Diagnostics.TestData;
using HIC.Logging;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class PayloadTest:DatabaseTests
    {
        public static object payload = new object();
        public static bool Success = false;

        [Test]
        public void TestPayloadInjection()
        {
            BulkTestsData b = new BulkTestsData(CatalogueRepository,DiscoveredDatabaseICanCreateRandomTablesIn,10);
            b.SetupTestData();
            b.ImportAsCatalogue();

            var lmd = new LoadMetadata(CatalogueRepository, "Loading");
            lmd.LocationOfFlatFiles = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo("delme"), true).RootPath.FullName;
            lmd.SaveToDatabase();

            CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestPayloadAttacher));

            b.catalogue.LoadMetadata_ID = lmd.ID;
            b.catalogue.LoggingDataTask = "TestPayloadInjection";
            b.catalogue.SaveToDatabase();

            var lm = new LogManager(new ServerDefaults(CatalogueRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID));
            lm.CreateNewLoggingTaskIfNotExists("TestPayloadInjection");

            var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.Mounting);
            pt.Path = typeof (TestPayloadAttacher).FullName;
            pt.ProcessTaskType = ProcessTaskType.Attacher;
            pt.SaveToDatabase();

            var config = new HICDatabaseConfiguration(DiscoveredDatabaseICanCreateRandomTablesIn.Server);
            var factory = new HICDataLoadFactory(lmd, config, new HICLoadConfigurationFlags(), CatalogueRepository, lm);
            IDataLoadExecution execution = factory.Create(new ThrowImmediatelyDataLoadEventListener());

            var proceedure = new DataLoadProcess(RepositoryLocator, lmd, null, lm, new ThrowImmediatelyDataLoadEventListener(), execution, config);

            proceedure.Run(new GracefulCancellationToken(), payload);

            Assert.IsTrue(PayloadTest.Success, "Expected IAttacher to detect Payload and set this property to true");
        }


        public class TestPayloadAttacher : Attacher,IPluginAttacher
        {
            public TestPayloadAttacher() : base(false)
            {
            }

            public override ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
            {
                base.Attach(job, cancellationToken);

                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Found Payload:" + job.Payload));
                PayloadTest.Success = ReferenceEquals(payload, job.Payload);

                return ExitCodeType.OperationNotRequired;
            }

            public override void Check(ICheckNotifier notifier)
            {
                
            }

            public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
            {
                
            }
        }
    }
}
