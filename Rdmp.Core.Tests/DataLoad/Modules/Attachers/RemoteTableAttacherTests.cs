// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using FAnsi;
using FAnsi.Discovery;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.LoadProgressUpdating;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataLoad.Modules.Attachers;

internal class RemoteTableAttacherTests : DatabaseTests
{
    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void TestRemoteTableAttacher_Normal(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        var attacher = new RemoteTableAttacher
        {
            //where to go for data
            RemoteServer = db.Server.Name,
            RemoteDatabaseName = db.GetRuntimeName(),
            DatabaseType = db.Server.DatabaseType
        };

        if (db.Server.ExplicitUsernameIfAny != null)
        {
            var creds = new DataAccessCredentials(CatalogueRepository)
            {
                Username = db.Server.ExplicitUsernameIfAny,
                Password = db.Server.ExplicitPasswordIfAny
            };
            creds.SaveToDatabase();
            attacher.RemoteTableAccessCredentials = creds;
        }

        RunAttachStageWithNormalJob(attacher, db);
    }


    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void TestRemoteTableAttacher_AsReference(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        var attacher = new RemoteTableAttacher();
        //where to go for data
        var eds = new ExternalDatabaseServer(CatalogueRepository, "ref", null);
        eds.SetProperties(db);
        eds.SaveToDatabase();

        attacher.RemoteServerReference = eds;

        RunAttachStageWithNormalJob(attacher, db);
    }

    [TestCaseSource(typeof(All),nameof(All.DatabaseTypesWithBoolFlags))]
    public void TestRemoteTableAttacher_WithLoadProgress(DatabaseType dbType, bool mismatchProgress)
    {
        var db = GetCleanedServer(dbType);

        var attacher = new RemoteTableAttacher();
        //where to go for data
        var eds = new ExternalDatabaseServer(CatalogueRepository, "ref", null);
        eds.SetProperties(db);
        eds.SaveToDatabase();

        attacher.RemoteServerReference = eds;

        RunAttachStageWithLoadProgressJob(attacher, db, mismatchProgress);
    }

    private void RunAttachStageWithNormalJob(RemoteTableAttacher attacher, DiscoveredDatabase db)
    {
        //the table to get data from
        attacher.RemoteTableName = "table1";
        attacher.RAWTableName = "table2";

        attacher.Check(ThrowImmediatelyCheckNotifier.Quiet);

        attacher.Initialize(null, db);

        using var dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Rows.Add("fff");

        var tbl1 = db.CreateTable("table1", dt);
        var tbl2 = db.CreateTable("table2", new []{new DatabaseColumnRequest("Col1",new DatabaseTypeRequest(typeof(string),5))});

        Assert.AreEqual(1,tbl1.GetRowCount());
        Assert.AreEqual(0,tbl2.GetRowCount());

        var logManager = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(tbl2);
        Mock.Get(lmd).Setup(p=>p.CatalogueRepository).Returns(CatalogueRepository);
        logManager.CreateNewLoggingTaskIfNotExists(lmd.GetDistinctLoggingTask());

        var dbConfiguration = new HICDatabaseConfiguration(lmd, RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "table2"));

        var job = new DataLoadJob(RepositoryLocator,"test job",logManager,lmd,new TestLoadDirectory(),ThrowImmediatelyDataLoadEventListener.Quiet,dbConfiguration);
        job.StartLogging();
        attacher.Attach(job, new GracefulCancellationToken());

        Assert.AreEqual(1,tbl1.GetRowCount());
        Assert.AreEqual(1,tbl2.GetRowCount());
    }

    private void RunAttachStageWithLoadProgressJob(RemoteTableAttacher attacher, DiscoveredDatabase db,
        bool mismatchProgress)
    {
        var syntax = db.Server.GetQuerySyntaxHelper();

        //the table to get data from
        attacher.RemoteSelectSQL =
            $"SELECT * FROM table1 WHERE {syntax.EnsureWrapped("DateCol")} >= @startDate AND {syntax.EnsureWrapped("DateCol")} <= @endDate";
        attacher.RAWTableName = "table2";

        attacher.Check(ThrowImmediatelyCheckNotifier.Quiet);

        attacher.Initialize(null, db);

        using var dt = new DataTable();
        dt.Columns.Add("Col1");
        dt.Columns.Add("DateCol");

        dt.Rows.Add("fff",new DateTime(2000,1,1));
        dt.Rows.Add("fff",new DateTime(2001,1,1));
        dt.Rows.Add("fff",new DateTime(2002,1,1));
            

        var tbl1 = db.CreateTable("table1", dt);
        var tbl2 = db.CreateTable("table2", new []
        {
            new DatabaseColumnRequest("Col1",new DatabaseTypeRequest(typeof(string),5)),
            new DatabaseColumnRequest("DateCol",new DatabaseTypeRequest(typeof(DateTime)))
        });

        Assert.AreEqual(3,tbl1.GetRowCount());
        Assert.AreEqual(0,tbl2.GetRowCount());

        var logManager = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(tbl2);
        Mock.Get(lmd).Setup(p=>p.CatalogueRepository).Returns(CatalogueRepository);
        logManager.CreateNewLoggingTaskIfNotExists(lmd.GetDistinctLoggingTask());

        var lp = new LoadProgress(CatalogueRepository, new LoadMetadata(CatalogueRepository, "ffffff"))
        {
            OriginDate = new DateTime(2001,1,1)
        };
        attacher.Progress = lp;
        attacher.ProgressUpdateStrategy = new DataLoadProgressUpdateInfo {Strategy = DataLoadProgressUpdateStrategy.DoNothing};
            
        var dbConfiguration = new HICDatabaseConfiguration(lmd, RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "table2"));

        var job = new ScheduledDataLoadJob(RepositoryLocator,"test job",logManager,lmd,new TestLoadDirectory(),ThrowImmediatelyDataLoadEventListener.Quiet,dbConfiguration)
        {
            LoadProgress = mismatchProgress
                ? new LoadProgress(CatalogueRepository, new LoadMetadata(CatalogueRepository, "ffsdf"))
                : lp,
            DatesToRetrieve = new List<DateTime>{new(2001,01,01)}
        };

        job.StartLogging();
        attacher.Attach(job, new GracefulCancellationToken());

        Assert.AreEqual(3,tbl1.GetRowCount());
        Assert.AreEqual(mismatchProgress ? 0 : 1,tbl2.GetRowCount());
    }
}