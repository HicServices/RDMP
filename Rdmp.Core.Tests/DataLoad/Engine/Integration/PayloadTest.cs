// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class PayloadTest : DatabaseTests
{
    public static object payload = new();
    public static bool Success;

    [Test]
    public void TestPayloadInjection()
    {
        var b = new BulkTestsData(CatalogueRepository, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer), 10);
        b.SetupTestData();
        b.ImportAsCatalogue();

        var lmd = new LoadMetadata(CatalogueRepository, "Loading");
        var filePath = LoadDirectory
                .CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), "delme", true)
                .RootPath.FullName;
        lmd.LocationOfForLoadingDirectory = filePath + lmd.DefaultForLoadingPath;
        lmd.LocationOfForArchivingDirectory = filePath + lmd.DefaultForArchivingPath;
        lmd.LocationOfExecutablesDirectory = filePath + lmd.DefaultExecutablesPath;
        lmd.LocationOfCacheDirectory = filePath + lmd.DefaultCachePath;
        lmd.SaveToDatabase();

        MEF.AddTypeToCatalogForTesting(typeof(TestPayloadAttacher));

        b.catalogue.LoggingDataTask = "TestPayloadInjection";
        b.catalogue.SaveToDatabase();
        lmd.LinkToCatalogue(b.catalogue);
        var lm = new LogManager(CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        lm.CreateNewLoggingTaskIfNotExists("TestPayloadInjection");

        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.Mounting)
        {
            Path = typeof(TestPayloadAttacher).FullName,
            ProcessTaskType = ProcessTaskType.Attacher
        };
        pt.SaveToDatabase();

        var config = new HICDatabaseConfiguration(GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer).Server);
        var factory = new HICDataLoadFactory(lmd, config, new HICLoadConfigurationFlags(), CatalogueRepository, lm);
        var execution = factory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

        var procedure = new DataLoadProcess(RepositoryLocator, lmd, null, lm,
            ThrowImmediatelyDataLoadEventListener.Quiet, execution, config);

        procedure.Run(new GracefulCancellationToken(), payload);

        Assert.That(Success, "Expected IAttacher to detect Payload and set this property to true");
    }


    public class TestPayloadAttacher : Attacher, IPluginAttacher
    {
        public TestPayloadAttacher() : base(false)
        {
        }

        public override ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, $"Found Payload:{job.Payload}"));
            Success = ReferenceEquals(payload, job.Payload);

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