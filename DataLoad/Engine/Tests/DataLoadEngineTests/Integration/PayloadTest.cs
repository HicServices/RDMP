// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Defaults;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Attachers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using Diagnostics.TestData;
using HIC.Logging;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
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
            lmd.LocationOfFlatFiles = LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),"delme", true).RootPath.FullName;
            lmd.SaveToDatabase();

            CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestPayloadAttacher));

            b.catalogue.LoadMetadata_ID = lmd.ID;
            b.catalogue.LoggingDataTask = "TestPayloadInjection";
            b.catalogue.SaveToDatabase();

            var lm = new LogManager(new ServerDefaults(CatalogueRepository).GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
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
