// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using System;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.DataLoad;
using Tests.Common;
using Tests.Common.Scenarios;
using TypeGuesser;

namespace Rdmp.Core.Tests.CommandLine.AutomationLoopTests;

public class EndToEndDLETest : TestsRequiringADle
{
    [Test]
    public void RunEndToEndDLETest()
    {
        const int timeoutInMilliseconds = 120000;
        CreateFileInForLoading("loadmeee.csv", 500, new Random(500));
        RunDLE(timeoutInMilliseconds);
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void TestDle_DodgyColumnNames(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        var tbl = db.CreateTable("Troll Select * Loll", new DatabaseColumnRequest[]
        {
            new("group by",new DatabaseTypeRequest(typeof(string),100)){IsPrimaryKey = true},
            new(",,,,",new DatabaseTypeRequest(typeof(string)))
        });

        CreateFileInForLoading("Troll.csv", new string[]
        {
            "group by,\",,,,\"",
            "fish,fishon"
        });

        var cata = Import(tbl);
        var lmd = new LoadMetadata(CatalogueRepository, nameof(TestDle_DodgyColumnNames))
        {
            LocationOfFlatFiles = LoadDirectory.RootPath.FullName
        };
        lmd.SaveToDatabase();

        CreateFlatFileAttacher(lmd, "Troll.csv", cata.GetTableInfoList(false).Single());

        cata.LoadMetadata_ID = lmd.ID;
        cata.SaveToDatabase();

        Assert.AreEqual(0, tbl.GetRowCount());

        RunDLE(lmd, 30000, true);

        Assert.AreEqual(1, tbl.GetRowCount());
        Assert.AreEqual("fishon", tbl.GetDataTable().Rows[0][",,,,"]);
    }
}