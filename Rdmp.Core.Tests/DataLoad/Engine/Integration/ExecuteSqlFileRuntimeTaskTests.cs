// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using FAnsi;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

internal class ExecuteSqlFileRuntimeTaskTests:DatabaseTests
{
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void ExecuteSqlFileRuntimeTask_BasicScript(DatabaseType dbType)
    {
        var dt = new DataTable();
        dt.Columns.Add("Lawl");
        dt.Rows.Add(new object []{2});

        var db = GetCleanedServer(dbType);

        var tbl = db.CreateTable("Fish",dt);

        var f = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,"Bob.sql"));

        File.WriteAllText(f.FullName,@"UPDATE Fish Set Lawl = 1");

        var pt = Mock.Of<IProcessTask>(x => x.Path==f.FullName);

        var dir = LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),"ExecuteSqlFileRuntimeTaskTests", true);

        var task = new ExecuteSqlFileRuntimeTask(pt, new RuntimeArgumentCollection(Array.Empty<IArgument>(), new StageArgs(LoadStage.AdjustRaw, db, dir)));

        task.Check(new ThrowImmediatelyCheckNotifier());

        var job = Mock.Of<IDataLoadJob>();

        task.Run(job, new GracefulCancellationToken());

        Assert.AreEqual(1,tbl.GetDataTable().Rows[0][0]);

        tbl.Drop();
    }

    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void ExecuteSqlFileRuntimeTask_InvalidID(DatabaseType dbType)
    {
        var dt = new DataTable();
        dt.Columns.Add("Lawl");
        dt.Rows.Add(new object[] { 2 });

        var db = GetCleanedServer(dbType);

        var tbl = db.CreateTable("Fish", dt);

        Import(tbl,out var ti, out var cols);

        var f = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Bob.sql"));
            
        File.WriteAllText(f.FullName, @"UPDATE {T:0} Set {C:0} = 1");

        var pt = Mock.Of<IProcessTask>(x => x.Path==f.FullName);

        var dir = LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),"ExecuteSqlFileRuntimeTaskTests", true);



        var task = new ExecuteSqlFileRuntimeTask(pt, new RuntimeArgumentCollection(Array.Empty<IArgument>(), new StageArgs(LoadStage.AdjustRaw, db, dir)));

        task.Check(new ThrowImmediatelyCheckNotifier());
        var configuration = new HICDatabaseConfiguration(db.Server);

        var job = Mock.Of<IDataLoadJob>(x =>
            x.RegularTablesToLoad == new List<ITableInfo> {ti} &&
            x.LookupTablesToLoad == new List<ITableInfo>() &&
            x.Configuration == configuration);

        var ex = Assert.Throws<ExecuteSqlFileRuntimeTaskException>(()=>task.Run(job, new GracefulCancellationToken()));
        StringAssert.Contains("Failed to find a TableInfo in the load with ID 0",ex.Message);

        task.LoadCompletedSoDispose(Core.DataLoad.ExitCodeType.Success,new ThrowImmediatelyDataLoadEventListener());
    }

    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void ExecuteSqlRuntimeTask_InvalidID(DatabaseType dbType)
    {
        var dt = new DataTable();
        dt.Columns.Add("Lawl");
        dt.Rows.Add(new object[] { 2 });

        var db = GetCleanedServer(dbType);

        var tbl = db.CreateTable("Fish", dt);

        Import(tbl,out var ti, out var cols);

        var sql = @"UPDATE {T:0} Set {C:0} = 1";

        var dir = LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),"ExecuteSqlFileRuntimeTaskTests", true);

#pragma warning disable CS0252, CS0253 // Spurious warning 'Possible unintended reference comparison; left hand side needs cast' since VS doesn't grok Moq fully
        var sqlArg = new IArgument[]{Mock.Of<IArgument>(x =>
            x.Name == "Sql" &&
            x.Value == sql &&
            x.GetValueAsSystemType() == sql) };
#pragma warning restore CS0252, CS0253

        var args = new RuntimeArgumentCollection(sqlArg, new StageArgs(LoadStage.AdjustRaw, db, dir));

        var pt = Mock.Of<IProcessTask>(x =>
            x.Path == typeof(ExecuteSqlMutilation).FullName &&
            x.GetAllArguments() == sqlArg
        );

        IRuntimeTask task = new MutilateDataTablesRuntimeTask(pt,args,CatalogueRepository.MEF);

        task.Check(new ThrowImmediatelyCheckNotifier());
        var configuration = new HICDatabaseConfiguration(db.Server);

        var job = new ThrowImmediatelyDataLoadJob
        {
            RegularTablesToLoad = new List<ITableInfo> {ti},
            LookupTablesToLoad = new List<ITableInfo>(),
            Configuration = configuration
        };

        var ex = Assert.Throws<Exception>(()=>task.Run(job, new GracefulCancellationToken()));

        StringAssert.Contains("Mutilate failed",ex.Message);
        StringAssert.Contains("Failed to find a TableInfo in the load with ID 0",ex.InnerException.Message);

        task.LoadCompletedSoDispose(Core.DataLoad.ExitCodeType.Success,new ThrowImmediatelyDataLoadEventListener());
    }

    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void ExecuteSqlFileRuntimeTask_ValidID_CustomNamer(DatabaseType dbType)
    {
        var dt = new DataTable();
        dt.Columns.Add("Lawl");
        dt.Rows.Add(new object[] { 2 });

        var db = GetCleanedServer(dbType);

        var tbl = db.CreateTable("Fish", dt);

        var tableName = "AAAAAAA";

        Import(tbl, out var ti, out var cols);

        var f = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Bob.sql"));

        File.WriteAllText(f.FullName, $@"UPDATE {{T:{ti.ID}}} Set {{C:{cols[0].ID}}} = 1");

        tbl.Rename(tableName);

        //we renamed the table to simulate RAW, confirm TableInfo doesn't think it exists
        Assert.IsFalse(ti.Discover(DataAccessContext.InternalDataProcessing).Exists());

        var pt = Mock.Of<IProcessTask>(x => x.Path==f.FullName);

        var dir = LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),"ExecuteSqlFileRuntimeTaskTests", true);

        var task = new ExecuteSqlFileRuntimeTask(pt, new RuntimeArgumentCollection(Array.Empty<IArgument>(), new StageArgs(LoadStage.AdjustRaw, db, dir)));

        task.Check(new ThrowImmediatelyCheckNotifier());


        //create a namer that tells the user
        var namer = RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, tableName);
        var configuration = new HICDatabaseConfiguration(db.Server,namer);

        var job = Mock.Of<IDataLoadJob>(x =>
            x.RegularTablesToLoad == new List<ITableInfo> { ti }&&
            x.LookupTablesToLoad == new List<ITableInfo>() &&
            x.Configuration == configuration);

        task.Run(job, new GracefulCancellationToken());

        Assert.AreEqual(1, tbl.GetDataTable().Rows[0][0]);

        tbl.Drop();
    }
}