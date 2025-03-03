// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class CoalescerTests : DatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    [TestCase(DatabaseType.MySql, true)]
    [TestCase(DatabaseType.MySql, false)]
    public void TestCoalescer_RampantNullness(DatabaseType type, bool useCustomNamer)
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

        var r = new Random();

        for (var i = 0; i < batchCount; i++)
        {
            var randInt = r.Next(250);
            var randCompleteness = r.Next(4);

            dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt });
            dt.Rows.Add(new object[] { randInt, DBNull.Value, DBNull.Value, DBNull.Value, randInt });
            dt.Rows.Add(new object[] { randInt, DBNull.Value, DBNull.Value, randInt, DBNull.Value });
            dt.Rows.Add(new object[] { randInt, DBNull.Value, DBNull.Value, randInt, randInt });

            if (randCompleteness >= 1)
            {
                dt.Rows.Add(new object[] { randInt, DBNull.Value, randInt, DBNull.Value, DBNull.Value });
                dt.Rows.Add(new object[] { randInt, DBNull.Value, randInt, DBNull.Value, randInt });
                dt.Rows.Add(new object[] { randInt, DBNull.Value, randInt, randInt, DBNull.Value });
                dt.Rows.Add(new object[] { randInt, DBNull.Value, randInt, randInt, randInt });
            }

            if (randCompleteness >= 2)
            {
                dt.Rows.Add(new object[] { randInt, randInt, DBNull.Value, DBNull.Value, DBNull.Value });
                dt.Rows.Add(new object[] { randInt, randInt, DBNull.Value, DBNull.Value, randInt });
                dt.Rows.Add(new object[] { randInt, randInt, DBNull.Value, randInt, DBNull.Value });
                dt.Rows.Add(new object[] { randInt, randInt, DBNull.Value, randInt, randInt });
            }


            if (randCompleteness >= 3)
            {
                dt.Rows.Add(new object[] { randInt, randInt, randInt, DBNull.Value, DBNull.Value });
                dt.Rows.Add(new object[] { randInt, randInt, randInt, DBNull.Value, randInt });
                dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, DBNull.Value });
                dt.Rows.Add(new object[] { randInt, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value });
            }
        }

        dt.EndLoadData();
        var tbl = db.CreateTable(dt.TableName, dt);

        var importer = new TableInfoImporter(CatalogueRepository, tbl);
        importer.DoImport(out var tableInfo, out var colInfos);

        //lie about what the primary key is because this component is designed to run in the RAW environment and we are simulating a LIVE TableInfo (correctly)
        var pk = colInfos.Single(c => c.GetRuntimeName().Equals("pk"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        INameDatabasesAndTablesDuringLoads namer = null;

        if (useCustomNamer)
        {
            tbl.Rename("AAAA");
            namer = RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "AAAA");
        }

        var configuration = new HICDatabaseConfiguration(db.Server, namer);

        var coalescer = new Coalescer
        {
            TableRegexPattern = new Regex(".*"),
            CreateIndex = true
        };
        coalescer.Initialize(db, LoadStage.AdjustRaw);


        var job = new ThrowImmediatelyDataLoadJob(configuration, tableInfo);
        coalescer.Mutilate(job);

        var dt2 = tbl.GetDataTable();

        foreach (DataRow row in dt2.Rows)
        {
            Assert.Multiple(() =>
            {
                Assert.That(row["f1"], Is.Not.EqualTo(DBNull.Value));
                Assert.That(row["f2"], Is.Not.EqualTo(DBNull.Value));
                Assert.That(row["f3"], Is.Not.EqualTo(DBNull.Value));
                Assert.That(row["f4"], Is.Not.EqualTo(DBNull.Value));
            });
        }

        db.Drop();
    }
}