// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NSubstitute.Routing.Handlers;
using NUnit.Framework;
using System.Data;
using System;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Tests.Common;
using Rdmp.Core.Curation;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataLoad.Modules.Mutilators;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class DistinctAgainstCatalogueMutilationTests: DatabaseTests
{

    [Test]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.PostgreSql)]
    [TestCase(DatabaseType.Oracle)]
    public void TestDistinctAgainstCatalogueMutilation_New(DatabaseType type)
    {
        var db = GetCleanedServer(type, "TestDistinctAgainstCatalogueMutilation");
        const int batchCount = 1000;

        using var dt = new DataTable("TestDistinctAgainstCatalogueMutilation_New");
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

        dt.EndLoadData();
        var tbl = db.CreateTable(dt.TableName, dt);

        var importer = new TableInfoImporter(CatalogueRepository, tbl);
        importer.DoImport(out var tableInfo, out var colInfos);

        //lie about what hte primary key is because this component is designed to run in the RAW environment and we are simulating a LIVE TableInfo (correctly)
        var pk = colInfos.Single(c => c.GetRuntimeName().Equals("pk"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();
        var cmd = new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(new ThrowImmediatelyActivator(RepositoryLocator), tbl, null);
        cmd.Execute();

        //var distincter = new DistinctAgainstCatalogueMutilation();
    }

    public void TestDistinctAgainstCatalogueMutilation_Update(DatabaseType type)
    {

    }

    public void TestDistinctAgainstCatalogueMutilation_NewAndUpdat(DatabaseType type)
    {

    }
}
