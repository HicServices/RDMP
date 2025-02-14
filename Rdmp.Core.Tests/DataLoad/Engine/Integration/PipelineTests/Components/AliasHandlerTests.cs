// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases.Exceptions;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Components;

public class AliasHandlerTests : DatabaseTests
{
    private ExternalDatabaseServer _server;
    private AliasHandler _handler;

    private DiscoveredDatabase _database;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _server = new ExternalDatabaseServer(CatalogueRepository, "AliasHandlerTestsServer", null);
        _server.SetProperties(GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer));

        _database = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var s = _database.Server;
        using (var con = s.GetConnection())
        {
            con.Open();

            s.GetCommand("CREATE TABLE AliasHandlerTests (input varchar(50), alias varchar(50))", con)
                .ExecuteNonQuery();

            //Two names which are aliases of the same person
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('freddie','craig')", con).ExecuteNonQuery();
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('craig','freddie')", con).ExecuteNonQuery();

            //Three names which are all aliases of the same person
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('peter','paul')", con).ExecuteNonQuery();
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('peter','pepey')", con).ExecuteNonQuery();
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('pepey','paul')", con).ExecuteNonQuery();
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('pepey','peter')", con).ExecuteNonQuery();
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('paul','pepey')", con).ExecuteNonQuery();
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('paul','peter')", con).ExecuteNonQuery();
        }

        _handler = new AliasHandler
        {
            AliasColumnInInputDataTables = "input",
            AliasTableSQL = "select * from AliasHandlerTests",
            DataAccessContext = DataAccessContext.DataLoad,
            ResolutionStrategy = AliasResolutionStrategy.CrashIfAliasesFound,
            TimeoutForAssemblingAliasTable = 10,
            ServerToExecuteQueryOn = _server
        };
    }


    [Test]
    public void ThrowBecause_ColumnNotInInputDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("cannonballer"); //not the same as the expected input column name
        dt.Rows.Add("yes");

        var ex = Assert.Throws<KeyNotFoundException>(() =>
            _handler.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
                new GracefulCancellationToken()));

        Assert.That(
            ex.Message, Is.EqualTo("You asked to resolve aliases on a column called 'input' but no column by that name appeared in the DataTable being processed.  Columns in that table were:cannonballer"));
    }

    [Test]
    public void ThrowBecause_NameAndAliasSameValue()
    {
        var s = _database.Server;
        using (var con = s.GetConnection())
        {
            con.Open();
            s.GetCommand("INSERT INTO  AliasHandlerTests VALUES ('dave','dave')", con).ExecuteNonQuery();
        }

        var dt = new DataTable();
        dt.Columns.Add("input");
        dt.Rows.Add("candle");

        var ex = Assert.Throws<AliasTableFetchException>(() =>
            _handler.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
                new GracefulCancellationToken()));
        Assert.That(ex.Message, Does.StartWith("Alias table SQL should only return aliases not exact matches"));
    }

    [Test]
    public void ThrowBecause_ThreeColumnAliasTable()
    {
        var s = _database.Server;
        using (var con = s.GetConnection())
        {
            con.Open();
            s.GetCommand("ALTER TABLE AliasHandlerTests ADD anotherAliascol varchar(50)", con).ExecuteNonQuery();
        }

        var dt = new DataTable();
        dt.Columns.Add("input");
        dt.Columns.Add("value");

        dt.Rows.Add("dave", 100);
        dt.Rows.Add("frank", 100);

        var ex = Assert.Throws<AliasTableFetchException>(() =>
            _handler.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
                new GracefulCancellationToken()));

        Assert.That(ex.Message, Does.Contain("Alias table SQL resulted in 3 fields being returned"));
    }

    [Test]
    public void NoAliases()
    {
        var dt = new DataTable();
        dt.Columns.Add("input");
        dt.Columns.Add("value");

        dt.Rows.Add("dave", 100);
        dt.Rows.Add("frank", 100);

        var result = _handler.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        Assert.That(result.Rows, Has.Count.EqualTo(2));
    }


    [Test]
    public void CrashStrategy()
    {
        var dt = new DataTable();
        dt.Columns.Add("input");

        dt.Rows.Add("paul");
        Assert.Throws<AliasException>(() =>
            _handler.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
                new GracefulCancellationToken()));
    }


    [Test]
    public void ResolveTwoNameAlias()
    {
        _handler.ResolutionStrategy = AliasResolutionStrategy.MultiplyInputDataRowsByAliases;

        var dt = new DataTable();
        dt.Columns.Add("value1", typeof(int));
        dt.Columns.Add("input");
        dt.Columns.Add("value2", typeof(int));

        dt.Rows.Add(99, "dave", 100);
        dt.Rows.Add(199, "frank", 200);
        dt.Rows.Add(299, "freddie", 300); //has a two name alias

        var result = _handler.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        Assert.That(result.Rows, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(result.Rows[2][0], Is.EqualTo(299));
            Assert.That(result.Rows[2][1], Is.EqualTo("freddie")); //the original input row which had an alias on it
            Assert.That(result.Rows[2][2], Is.EqualTo(300));

            Assert.That(result.Rows[3][0], Is.EqualTo(299));
            Assert.That(result.Rows[3][1], Is.EqualTo("craig")); //The new row that should have appeared to resolve the freddie=craig alias
            Assert.That(result.Rows[3][2], Is.EqualTo(300)); //value should match the input array
        });
    }

    [Test]
    public void ResolveThreeNameAlias()
    {
        _handler.ResolutionStrategy = AliasResolutionStrategy.MultiplyInputDataRowsByAliases;

        var dt = new DataTable();
        dt.Columns.Add("value1", typeof(int));
        dt.Columns.Add("input");
        dt.Columns.Add("value2", typeof(int));

        dt.Rows.Add(99, "pepey", 100); //has a three name alias
        dt.Rows.Add(199, "frank", 200);
        dt.Rows.Add(299, "anderson", 300);

        var result = _handler.ProcessPipelineData(dt, ThrowImmediatelyDataLoadEventListener.Quiet,
            new GracefulCancellationToken());

        Assert.That(result.Rows, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            Assert.That(result.Rows[0][0], Is.EqualTo(99));
            Assert.That(result.Rows[0][1], Is.EqualTo("pepey")); //the original input row which had an alias on it
            Assert.That(result.Rows[0][2], Is.EqualTo(100));


            //new rows are added at the end of the DataTable
            Assert.That(result.Rows[3][0], Is.EqualTo(99));
            Assert.That(result.Rows[3][1], Is.EqualTo("paul")); //The new row that should have appeared to resolve the pepey=paul=peter alias
            Assert.That(result.Rows[3][2], Is.EqualTo(100)); //value should match the input array

            Assert.That(result.Rows[4][0], Is.EqualTo(99));
            Assert.That(result.Rows[4][1], Is.EqualTo("peter")); //The new row that should have appeared to resolve the  pepey=paul=peter alias
            Assert.That(result.Rows[4][2], Is.EqualTo(100)); //value should match the input array
        });
    }
}