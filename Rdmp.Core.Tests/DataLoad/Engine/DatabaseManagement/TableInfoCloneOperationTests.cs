// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.Operations;
using System.Data;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.DatabaseManagement;

internal class TableInfoCloneOperationTests : DatabaseTests
{
    [Test]
    public void Test_CloneTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("FF");

        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var tbl = db.CreateTable("MyTable", dt);

        Import(tbl, out var ti, out _);

        var config = new HICDatabaseConfiguration(tbl.Database.Server);

        //create a RAW table schema called TableName_Isolation
        var cloner = new TableInfoCloneOperation(config, (TableInfo)ti, LoadBubble.Live,
            ThrowImmediatelyDataLoadEventListener.Quiet);
        cloner.CloneTable(tbl.Database, tbl.Database, tbl, $"{tbl.GetRuntimeName()}_copy", true, true, true,
            ti.PreLoadDiscardedColumns);

        var tbl2 = tbl.Database.ExpectTable($"{tbl.GetRuntimeName()}_copy");

        Assert.IsTrue(tbl2.Exists());
    }
}