// Copyright (c) The University of Dundee 2018-2024
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
            new("group by", new DatabaseTypeRequest(typeof(string), 100)) { IsPrimaryKey = true },
            new(",,,,", new DatabaseTypeRequest(typeof(string)))
        });

        CreateFileInForLoading("Troll.csv", new string[]
        {
            "group by,\",,,,\"",
            "fish,fishon"
        });

        var cata = Import(tbl);
        var lmd = new LoadMetadata(CatalogueRepository, nameof(TestDle_DodgyColumnNames));
        lmd.LocationOfForLoadingDirectory = LoadDirectory.RootPath.FullName + ((LoadMetadata)lmd).DefaultForLoadingPath;
        lmd.LocationOfForArchivingDirectory = LoadDirectory.RootPath.FullName + ((LoadMetadata)lmd).DefaultForArchivingPath;
        lmd.LocationOfExecutablesDirectory = LoadDirectory.RootPath.FullName + ((LoadMetadata)lmd).DefaultExecutablesPath;
        lmd.LocationOfCacheDirectory = LoadDirectory.RootPath.FullName + ((LoadMetadata)lmd).DefaultCachePath;
        lmd.SaveToDatabase();

        CreateFlatFileAttacher(lmd, "Troll.csv", cata.GetTableInfoList(false).Single());

        cata.SaveToDatabase();
        lmd.LinkToCatalogue(cata);
        Assert.That(tbl.GetRowCount(), Is.EqualTo(0));

        RunDLE(lmd, 30000, true);

        Assert.Multiple(() =>
        {
            Assert.That(tbl.GetRowCount(), Is.EqualTo(1));
            Assert.That(tbl.GetDataTable().Rows[0][",,,,"], Is.EqualTo("fishon"));
        });
    }
}