// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using static Rdmp.Core.Tests.DataLoad.Engine.Integration.RemoteDatabaseAttacherTests;

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

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypesWithBoolFlags))]
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
        var tbl2 = db.CreateTable("table2",
            new[] { new DatabaseColumnRequest("Col1", new DatabaseTypeRequest(typeof(string), 5)) });

        Assert.Multiple(() =>
        {
            Assert.That(tbl1.GetRowCount(), Is.EqualTo(1));
            Assert.That(tbl2.GetRowCount(), Is.EqualTo(0));
        });

        var logManager = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(tbl2);
        lmd.CatalogueRepository.Returns(CatalogueRepository);
        logManager.CreateNewLoggingTaskIfNotExists(lmd.GetDistinctLoggingTask());

        var dbConfiguration = new HICDatabaseConfiguration(lmd,
            RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "table2"));

        var job = new DataLoadJob(RepositoryLocator, "test job", logManager, lmd, new TestLoadDirectory(),
            ThrowImmediatelyDataLoadEventListener.Quiet, dbConfiguration);
        job.StartLogging();
        attacher.Attach(job, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(tbl1.GetRowCount(), Is.EqualTo(1));
            Assert.That(tbl2.GetRowCount(), Is.EqualTo(1));
        });
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

        dt.Rows.Add("fff", new DateTime(2000, 1, 1));
        dt.Rows.Add("fff", new DateTime(2001, 1, 1));
        dt.Rows.Add("fff", new DateTime(2002, 1, 1));


        var tbl1 = db.CreateTable("table1", dt);
        var tbl2 = db.CreateTable("table2", new[]
        {
            new DatabaseColumnRequest("Col1", new DatabaseTypeRequest(typeof(string), 5)),
            new DatabaseColumnRequest("DateCol", new DatabaseTypeRequest(typeof(DateTime)))
        });

        Assert.Multiple(() =>
        {
            Assert.That(tbl1.GetRowCount(), Is.EqualTo(3));
            Assert.That(tbl2.GetRowCount(), Is.EqualTo(0));
        });

        var logManager = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));


        var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(tbl2);
        lmd.CatalogueRepository.Returns(CatalogueRepository);
        logManager.CreateNewLoggingTaskIfNotExists(lmd.GetDistinctLoggingTask());

        var lp = new LoadProgress(CatalogueRepository, new LoadMetadata(CatalogueRepository, "ffffff"))
        {
            OriginDate = new DateTime(2001, 1, 1)
        };
        attacher.Progress = lp;
        attacher.ProgressUpdateStrategy = new DataLoadProgressUpdateInfo
        { Strategy = DataLoadProgressUpdateStrategy.DoNothing };

        var dbConfiguration = new HICDatabaseConfiguration(lmd,
            RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "table2"));

        var job = new ScheduledDataLoadJob(RepositoryLocator, "test job", logManager, lmd, new TestLoadDirectory(),
            ThrowImmediatelyDataLoadEventListener.Quiet, dbConfiguration)
        {
            LoadProgress = mismatchProgress
                ? new LoadProgress(CatalogueRepository, new LoadMetadata(CatalogueRepository, "ffsdf"))
                : lp,
            DatesToRetrieve = new List<DateTime> { new(2001, 01, 01) }
        };

        job.StartLogging();
        attacher.Attach(job, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(tbl1.GetRowCount(), Is.EqualTo(3));
            Assert.That(tbl2.GetRowCount(), Is.EqualTo(mismatchProgress ? 0 : 1));
        });
    }

    [TestCase(DatabaseType.MicrosoftSQLServer, AttacherHistoricalDurations.Past24Hours)]
    [TestCase(DatabaseType.MySql, AttacherHistoricalDurations.Past24Hours)]
    [TestCase(DatabaseType.Oracle, AttacherHistoricalDurations.Past24Hours)]
    [TestCase(DatabaseType.PostgreSql, AttacherHistoricalDurations.Past24Hours)]
    [TestCase(DatabaseType.MicrosoftSQLServer, AttacherHistoricalDurations.Past7Days)]
    [TestCase(DatabaseType.MySql, AttacherHistoricalDurations.Past7Days)]
    [TestCase(DatabaseType.Oracle, AttacherHistoricalDurations.Past7Days)]
    [TestCase(DatabaseType.PostgreSql, AttacherHistoricalDurations.Past7Days)]
    [TestCase(DatabaseType.MicrosoftSQLServer, AttacherHistoricalDurations.PastMonth)]
    [TestCase(DatabaseType.MySql, AttacherHistoricalDurations.PastMonth)]
    [TestCase(DatabaseType.Oracle, AttacherHistoricalDurations.PastMonth)]
    [TestCase(DatabaseType.PostgreSql, AttacherHistoricalDurations.PastMonth)]
    [TestCase(DatabaseType.MySql, AttacherHistoricalDurations.PastYear)]
    [TestCase(DatabaseType.Oracle, AttacherHistoricalDurations.PastYear)]
    [TestCase(DatabaseType.PostgreSql, AttacherHistoricalDurations.PastYear)]
    [TestCase(DatabaseType.MicrosoftSQLServer, AttacherHistoricalDurations.SinceLastUse)]
    [TestCase(DatabaseType.MySql, AttacherHistoricalDurations.SinceLastUse)]
    [TestCase(DatabaseType.Oracle, AttacherHistoricalDurations.SinceLastUse)]
    [TestCase(DatabaseType.PostgreSql, AttacherHistoricalDurations.SinceLastUse)]
    [TestCase(DatabaseType.MicrosoftSQLServer, AttacherHistoricalDurations.Custom)]
    [TestCase(DatabaseType.MySql, AttacherHistoricalDurations.Custom)]
    [TestCase(DatabaseType.Oracle, AttacherHistoricalDurations.Custom)]
    [TestCase(DatabaseType.PostgreSql, AttacherHistoricalDurations.Custom)]
    [TestCase(DatabaseType.MicrosoftSQLServer, AttacherHistoricalDurations.DeltaReading)]
    [TestCase(DatabaseType.MySql, AttacherHistoricalDurations.DeltaReading)]
    [TestCase(DatabaseType.Oracle, AttacherHistoricalDurations.DeltaReading)]
    [TestCase(DatabaseType.PostgreSql, AttacherHistoricalDurations.DeltaReading)]
    public void TestRemoteTableAttacher_DateFilters(DatabaseType dbType, AttacherHistoricalDurations duration)
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
        attacher.HistoricalFetchDuration = duration;

        RunAttachStageWithFilterJob(attacher, db, duration);
    }


    private static string Within(AttacherHistoricalDurations duration)
    {
        switch (duration)
        {
            case AttacherHistoricalDurations.Past24Hours:
                return DateTime.Now.AddHours(-1).ToString();
            case AttacherHistoricalDurations.Past7Days:
                return DateTime.Now.AddHours(-1).ToString();
            case AttacherHistoricalDurations.PastMonth:
                return DateTime.Now.AddHours(-1).ToString();
            case AttacherHistoricalDurations.PastYear:
                return DateTime.Now.AddHours(-1).ToString();
            case AttacherHistoricalDurations.SinceLastUse:
                return DateTime.Now.AddHours(-1).ToString();
            case AttacherHistoricalDurations.Custom:
                return DateTime.Now.AddDays(-1).ToString();
            case AttacherHistoricalDurations.DeltaReading:
                return DateTime.Now.AddDays(-4).ToString();
            default:
                return "fail";
        }
    }

    private static string Outwith(AttacherHistoricalDurations duration)
    {
        switch (duration)
        {
            case AttacherHistoricalDurations.Past24Hours:
                return DateTime.Now.AddDays(-2).ToString();
            case AttacherHistoricalDurations.Past7Days:
                return DateTime.Now.AddDays(-8).ToString();
            case AttacherHistoricalDurations.PastMonth:
                return DateTime.Now.AddMonths(-2).ToString();
            case AttacherHistoricalDurations.PastYear:
                return DateTime.Now.AddYears(-2).ToString();
            case AttacherHistoricalDurations.SinceLastUse:
                return DateTime.Now.AddDays(-2).ToString();
            case AttacherHistoricalDurations.Custom:
                return DateTime.Now.AddDays(-14).ToString();
            case AttacherHistoricalDurations.DeltaReading:
                return DateTime.Now.AddDays(-10).ToString();
            default:
                return "fail";
        }
    }
    private void RunAttachStageWithFilterJob(RemoteTableAttacher attacher, DiscoveredDatabase db, AttacherHistoricalDurations duration)
    {
        //the table to get data from
        attacher.RemoteTableName = "table1";
        attacher.RAWTableName = "table2";
        attacher.RemoteTableDateColumn = "dateColumn";
        attacher.Check(ThrowImmediatelyCheckNotifier.Quiet);

        attacher.Initialize(null, db);

        using var dt = new DataTable();
        var within = Within(duration);
        var outwith = Outwith(duration);
        dt.Columns.Add("Col1");
        dt.Columns.Add("dateColumn");
        dt.Rows.Add("fff", within);
        dt.Rows.Add("rrr", outwith);

        var tbl1 = db.CreateTable("table1", dt);
        var tbl2 = db.CreateTable("table2",
            new[] { new DatabaseColumnRequest("Col1", new DatabaseTypeRequest(typeof(string), 5)), new DatabaseColumnRequest("dateColumn", new DatabaseTypeRequest(typeof(string), 100)) });

        Assert.Multiple(() =>
        {
            Assert.That(tbl1.GetRowCount(), Is.EqualTo(2));
            Assert.That(tbl2.GetRowCount(), Is.EqualTo(0));
        });

        var logManager = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(tbl2);
        lmd.CatalogueRepository.Returns(CatalogueRepository);
        logManager.CreateNewLoggingTaskIfNotExists(lmd.GetDistinctLoggingTask());

        var dbConfiguration = new HICDatabaseConfiguration(lmd,
            RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "table2"));

        var job = new DataLoadJob(RepositoryLocator, "test job", logManager, lmd, new TestLoadDirectory(),
            ThrowImmediatelyDataLoadEventListener.Quiet, dbConfiguration);
        job.StartLogging();
        if (duration == AttacherHistoricalDurations.SinceLastUse)
        {
            job.LoadMetadata.LastLoadTime = DateTime.Now.AddDays(-1);// last used yesterday
            job.LoadMetadata.SaveToDatabase();
        }
        if (duration == AttacherHistoricalDurations.Custom)
        {
            attacher.CustomFetchDurationStartDate = DateTime.Now.AddDays(-7);
            attacher.CustomFetchDurationEndDate = DateTime.Now;
        }
        if (duration == AttacherHistoricalDurations.DeltaReading)
        {
            attacher.DeltaReadingStartDate = DateTime.Now.AddDays(-7);
            attacher.DeltaReadingLookBackDays = 0;
            attacher.DeltaReadingLookForwardDays = 5;
        }
        attacher.Attach(job, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(tbl1.GetRowCount(), Is.EqualTo(2));
            Assert.That(tbl2.GetRowCount(), Is.EqualTo(1));
        });
    }
}
