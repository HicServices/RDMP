// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class DistincterTests : DatabaseTests
{
    [Test]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void TestDistincter_Duplicates(DatabaseType type)
    {
        var db = GetCleanedServer(type, "TestCoalescer");

        const int batchCount = 1000;

        using var dt = new DataTable("TestCoalescer_RampantNullness");
        dt.BeginLoadData();
        dt.Columns.Add("pk");
        dt.Columns.Add("f1");
        dt.Columns.Add("f2");
        dt.Columns.Add("f3");
        dt.Columns.Add("f4");

        var r = new Random(123);

        for (var i = 0; i < batchCount; i++)
        {
            var randInt = r.Next(int.MaxValue);

            dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt });
            dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt });
        }

        dt.EndLoadData();
        var tbl = db.CreateTable(dt.TableName, dt);

        var importer = new TableInfoImporter(CatalogueRepository, tbl);
        importer.DoImport(out var tableInfo, out var colInfos);

        //lie about what the primary key is because this component is designed to run in the RAW environment and we are simulating a LIVE TableInfo (correctly)
        var pk = colInfos.Single(c => c.GetRuntimeName().Equals("pk"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        var rowsBefore = tbl.GetRowCount();

        var distincter = new Distincter
        {
            TableRegexPattern = new Regex(".*")
        };
        distincter.Initialize(db, LoadStage.AdjustRaw);

        var job = Substitute.For<IDataLoadJob>();
        job.RegularTablesToLoad.Returns(new List<ITableInfo>(new[] { tableInfo }));
        job.Configuration.Returns(new HICDatabaseConfiguration(db.Server, null, null, null));

        distincter.Mutilate(job);

        var rowsAfter = tbl.GetRowCount();

        Assert.That(rowsAfter, Is.EqualTo(rowsBefore / 2));

        db.Drop();
    }

    [Test]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void TestDistincter_NoDuplicates(DatabaseType type)
    {
        var db = GetCleanedServer(type, "TestCoalescer");

        const int batchCount = 1000;

        using var dt = new DataTable("TestCoalescer_RampantNullness");
        dt.BeginLoadData();
        dt.Columns.Add("pk");
        dt.Columns.Add("f1");
        dt.Columns.Add("f2");
        dt.Columns.Add("f3");
        dt.Columns.Add("f4");

        var r = new Random(123);

        for (var i = 0; i < batchCount; i++)
        {
            var randInt = r.Next(int.MaxValue);

            dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt });
            dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt + 1 });
        }

        dt.EndLoadData();
        var tbl = db.CreateTable(dt.TableName, dt);

        var importer = new TableInfoImporter(CatalogueRepository, tbl);
        importer.DoImport(out var tableInfo, out var colInfos);

        //lie about what the primary key is because this component is designed to run in the RAW environment and we are simulating a LIVE TableInfo (correctly)
        var pk = colInfos.Single(c => c.GetRuntimeName().Equals("pk"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        var rowsBefore = tbl.GetRowCount();

        var distincter = new Distincter
        {
            TableRegexPattern = new Regex(".*")
        };
        distincter.Initialize(db, LoadStage.AdjustRaw);

        var job = Substitute.For<IDataLoadJob>();
        job.RegularTablesToLoad.Returns(new List<ITableInfo>(new[] { tableInfo }));
        job.Configuration.Returns(new HICDatabaseConfiguration(db.Server, null, null, null));

        distincter.Mutilate(job);

        var rowsAfter = tbl.GetRowCount();

        Assert.That(rowsAfter, Is.EqualTo(rowsBefore));

        db.Drop();
    }
}