// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using FAnsi;
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

        Assert.AreEqual(1, tbl.GetRowCount());
        Import(tbl, out var ti, out var cols);

        //Create a virtual RAW column
        if (scenario == Scenario.MissingPreLoadDiscardedColumn ||
            scenario == Scenario.MissingPreLoadDiscardedColumnButSelectStar)
            new PreLoadDiscardedColumn(CatalogueRepository, ti, "MyMissingCol");

        var externalServer = new ExternalDatabaseServer(CatalogueRepository, "MyFictionalRemote", null);
        externalServer.SetProperties(db);

        var attacher = new RemoteDatabaseAttacher();
        attacher.Initialize(null, db);

        attacher.LoadRawColumnsOnly =
            scenario == Scenario.AllRawColumns || scenario == Scenario.MissingPreLoadDiscardedColumn;
        attacher.RemoteSource = externalServer;

        var lm = new LogManager(CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        lm.CreateNewLoggingTaskIfNotExists("amagad");
        var dli = lm.CreateDataLoadInfo("amagad", "p", "a", "", true);

        var job = Substitute.For<IDataLoadJob>();
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

                Assert.AreEqual("Invalid column name 'MyMissingCol'.", ex.InnerException.InnerException.InnerException.Message);
                return;
            case Scenario.MissingPreLoadDiscardedColumnButSelectStar:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        attacher.Attach(job, new GracefulCancellationToken());

        Assert.AreEqual(2, tbl.GetRowCount());

        dt = tbl.GetDataTable();

        VerifyRowExist(dt, 123, 11);

        if (scenario == Scenario.AllRawColumns)
            VerifyRowExist(dt, 123, DBNull.Value);

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