// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using FAnsi;
using MongoDB.Driver.Linq;
using NPOI.SS.Formula.Functions;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class RemoteDatabaseAttacherTests : DatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.AllRawColumns)]
    [TestCase(DatabaseType.MySql, Scenario.AllRawColumns)]
    [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.AllColumns)]
    [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.MissingPreLoadDiscardedColumn)]
    [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.MissingPreLoadDiscardedColumnButSelectStar)]
    public void TestRemoteDatabaseAttach(DatabaseType dbType, Scenario scenario)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable();

        dt.Columns.Add("Fish");
        dt.Columns.Add("hic_Heroism");

        dt.Rows.Add("123", 11);

        var tbl = db.CreateTable("MyTable", dt);

        Assert.That(tbl.GetRowCount(), Is.EqualTo(1));
        Import(tbl, out var ti, out _);

        //Create a virtual RAW column
        if (scenario is Scenario.MissingPreLoadDiscardedColumn or Scenario.MissingPreLoadDiscardedColumnButSelectStar)
            new PreLoadDiscardedColumn(CatalogueRepository, ti, "MyMissingCol");

        var externalServer = new ExternalDatabaseServer(CatalogueRepository, "MyFictionalRemote", null);
        externalServer.SetProperties(db);

        var attacher = new RemoteDatabaseAttacher();
        attacher.Initialize(null, db);

        attacher.LoadRawColumnsOnly = scenario is Scenario.AllRawColumns or Scenario.MissingPreLoadDiscardedColumn;
        attacher.RemoteSource = externalServer;

        var lm = new LogManager(CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        lm.CreateNewLoggingTaskIfNotExists("amagad");
        var dli = lm.CreateDataLoadInfo("amagad", "p", "a", "", true);

        var job = NSubstitute.Substitute.For<IDataLoadJob>();
        job.RegularTablesToLoad.Returns(new List<ITableInfo> { ti });
        job.LookupTablesToLoad.Returns(new List<ITableInfo>());
        job.DataLoadInfo.Returns(dli);

        switch (scenario)
        {
            case Scenario.AllRawColumns:
                break;
            case Scenario.AllColumns:
                break;
            case Scenario.MissingPreLoadDiscardedColumn:
                var ex = Assert.Throws<PipelineCrashedException>(() =>
                    attacher.Attach(job, new GracefulCancellationToken()));

                Assert.That(ex.InnerException.InnerException.InnerException.Message, Is.EqualTo("Invalid column name 'MyMissingCol'."));
                return;
            case Scenario.MissingPreLoadDiscardedColumnButSelectStar:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        attacher.Attach(job, new GracefulCancellationToken());

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        dt = tbl.GetDataTable();

        VerifyRowExist(dt, 123, 11);

        if (scenario == Scenario.AllRawColumns)
            VerifyRowExist(dt, 123, DBNull.Value);

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

        externalServer.DeleteInDatabase();
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

    private static Scenario[] Scenarios = {
        Scenario.AllRawColumns,
        Scenario.AllColumns,
        Scenario.MissingPreLoadDiscardedColumn,
        Scenario.MissingPreLoadDiscardedColumnButSelectStar
    };
    private static AttacherHistoricalDurations[] Durations = {
        AttacherHistoricalDurations.Past24Hours,
        AttacherHistoricalDurations.Past7Days,
        AttacherHistoricalDurations.PastMonth,
        AttacherHistoricalDurations.PastYear,
        AttacherHistoricalDurations.Custom,
        AttacherHistoricalDurations.DeltaReading
    };

    public static IEnumerable<object[]> GetAttacherCombinations()
    {
        foreach (var db in All.DatabaseTypes)
        {
            foreach (var sc in Scenarios)
            {
                foreach (var dur in Durations)
                {
                    yield return new object[] { db, sc, dur };
                }
            }
        }
    }

    [TestCaseSource(nameof(GetAttacherCombinations))]
    public void TestRemoteDatabaseAttacherWithDateFilter(DatabaseType dbType, Scenario scenario, AttacherHistoricalDurations duration)
    {
        var db = GetCleanedServer(dbType);

        using var dt = new DataTable();
        dt.Columns.Add("animal");
        dt.Columns.Add("date_seen");
        var withinDate = Within(duration);
        dt.Rows.Add("Cow", withinDate);
        dt.Rows.Add("Crow", Outwith(duration));

        var tbl = db.CreateTable("MyTable", dt);

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));
        Import(tbl, out var ti, out _);
        //Create a virtual RAW column
        if (scenario is Scenario.MissingPreLoadDiscardedColumn or Scenario.MissingPreLoadDiscardedColumnButSelectStar)
            new PreLoadDiscardedColumn(CatalogueRepository, ti, "MyMissingCol");

        var externalServer = new ExternalDatabaseServer(CatalogueRepository, "MyFictionalRemote", null);
        externalServer.SetProperties(db);

        var attacher = new RemoteDatabaseAttacher();
        attacher.Initialize(null, db);
        attacher.HistoricalFetchDuration = duration;
        attacher.RemoteTableDateColumn = "date_seen";
        attacher.LoadRawColumnsOnly = scenario is Scenario.AllRawColumns or Scenario.MissingPreLoadDiscardedColumn;
        attacher.RemoteSource = externalServer;

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


        var lm = new LogManager(CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        lm.CreateNewLoggingTaskIfNotExists("amagad");
        var dli = lm.CreateDataLoadInfo("amagad", "p", "a", "", true);

        var job = NSubstitute.Substitute.For<IDataLoadJob>();
        job.RegularTablesToLoad.Returns(new List<ITableInfo> { ti });
        job.LookupTablesToLoad.Returns(new List<ITableInfo>());
        job.DataLoadInfo.Returns(dli);

        if (duration == AttacherHistoricalDurations.SinceLastUse)
        {
            job.LoadMetadata.LastLoadTime = DateTime.Now.AddDays(-1);// last used yesterday
            job.LoadMetadata.SaveToDatabase();
        }
        if (scenario == Scenario.MissingPreLoadDiscardedColumn)
        {
            var ex = Assert.Throws<PipelineCrashedException>(() =>
                   attacher.Attach(job, new GracefulCancellationToken()));

            Assert.That(ex.InnerException.InnerException.InnerException.Message.Contains("MyMissingCol"), Is.EqualTo(true));
            return;
        }

        attacher.Attach(job, new GracefulCancellationToken());

        Assert.That(tbl.GetRowCount(), Is.EqualTo(3));

        using var dt2 = tbl.GetDataTable();
        VerifyRowExist(dt2, "Cow", withinDate);

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);
        externalServer.DeleteInDatabase();
    }


    public enum Scenario
    {
        /// <summary>
        /// Tests the ability of the DLE to load RAW columns from a remote database by identifying tables matching
        /// by name and fetching all columns which are expected to be in RAW.
        /// </summary>
        AllRawColumns,

        /// <summary>
        /// Tests the ability of the DLE to load RAW columns from a remote database by identifying tables matching
        /// by name and fetching all columns using SELECT *.
        /// </summary>
        AllColumns,

        /// <summary>
        /// Tests the behaviour of the system when there is a RAW only column which does not appear in the remote
        /// database when using the <see cref="RemoteDatabaseAttacher.LoadRawColumnsOnly"/> option.
        /// </summary>
        MissingPreLoadDiscardedColumn,

        /// <summary>
        /// Tests the behaviour of the system when there is a RAW only column which does not appear in the remote
        /// database but the mode fetch mode is SELECT *
        /// </summary>
        MissingPreLoadDiscardedColumnButSelectStar
    }
}