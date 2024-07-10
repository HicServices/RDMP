// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.TableCreation;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Checks;
using Rdmp.Core.DataLoad.Engine.Checks.Checkers;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.CrossDatabaseTypeTests;

/*
 Test currently requires for LowPrivilegeLoaderAccount (e.g. minion)
 ---------------------------------------------------

    create database DLE_STAGING

    use DLE_STAGING

    CREATE USER [minion] FOR LOGIN [minion]

    ALTER ROLE [db_datareader] ADD MEMBER [minion]
    ALTER ROLE [db_ddladmin] ADD MEMBER [minion]
    ALTER ROLE [db_datawriter] ADD MEMBER [minion]
*/

internal class CrossDatabaseDataLoadTests : DataLoadEngineTestsBase
{
    public enum TestCase
    {
        Normal,
        LowPrivilegeLoaderAccount,
        ForeignKeyOrphans,
        DodgyCollation,
        AllPrimaryKeys,
        NoTrigger,
        WithNonPrimaryKeyIdentityColumn,

        WithCustomTableNamer,

        WithDiffColumnIgnoreRegex //tests ability of the system to skip a given column when doing the DLE diff section
    }

    [TestCase(DatabaseType.Oracle, TestCase.Normal)]
    [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.Normal)]
    [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.NoTrigger)]
    [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.WithCustomTableNamer)]
    [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.WithNonPrimaryKeyIdentityColumn)]
    [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.DodgyCollation)]
    [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.LowPrivilegeLoaderAccount)]
    [TestCase(DatabaseType.MicrosoftSQLServer, TestCase.AllPrimaryKeys)]
    [TestCase(DatabaseType.MySql, TestCase.Normal)]
    //[TestCase(DatabaseType.MySql, TestCase.WithNonPrimaryKeyIdentityColumn)] //Not supported by MySql:Incorrect table definition; there can be only one auto column and it must be defined as a key
    [TestCase(DatabaseType.MySql, TestCase.DodgyCollation)]
    [TestCase(DatabaseType.MySql, TestCase.WithCustomTableNamer)]
    [TestCase(DatabaseType.MySql, TestCase.LowPrivilegeLoaderAccount)]
    [TestCase(DatabaseType.MySql, TestCase.AllPrimaryKeys)]
    [TestCase(DatabaseType.MySql, TestCase.WithDiffColumnIgnoreRegex)]
    public void Load(DatabaseType databaseType, TestCase testCase)
    {
        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);

        var db = GetCleanedServer(databaseType);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        using var dt = new DataTable("MyTable");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");
        dt.Columns.Add("FavouriteColour");
        dt.Rows.Add("Bob", "2001-01-01", "Pink");
        dt.Rows.Add("Frank", "2001-01-01", "Orange");

        var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 20), false)
            { IsPrimaryKey = true };

        if (testCase == TestCase.DodgyCollation)
            nameCol.Collation = databaseType switch
            {
                DatabaseType.MicrosoftSQLServer => "Latin1_General_CS_AS_KS_WS",
                DatabaseType.MySql => "latin1_german1_ci",
                _ => nameCol.Collation
            };

        DiscoveredTable tbl;
        switch (testCase)
        {
            case TestCase.WithNonPrimaryKeyIdentityColumn:
            {
                tbl = db.CreateTable("MyTable", new[]
                {
                    new DatabaseColumnRequest("ID", new DatabaseTypeRequest(typeof(int)), false)
                        { IsPrimaryKey = false, IsAutoIncrement = true },
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = true },
                    new DatabaseColumnRequest("FavouriteColour", new DatabaseTypeRequest(typeof(string)))
                });

                using (var blk = tbl.BeginBulkInsert())
                {
                    blk.Upload(dt);
                }

                    Assert.That(tbl.DiscoverColumns().Count(c =>
                        c.GetRuntimeName().Equals("ID", StringComparison.CurrentCultureIgnoreCase)), Is.EqualTo(1),
                    "Table created did not contain ID column");
                break;
            }
            case TestCase.AllPrimaryKeys:
                dt.PrimaryKey = dt.Columns.Cast<DataColumn>().ToArray();
                tbl = db.CreateTable("MyTable", dt, new[] { nameCol }); //upload the column as is
                Assert.That(tbl.DiscoverColumns().All(c => c.IsPrimaryKey));
                break;
            default:
                tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = true }
                });
                break;
        }

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");
        if (testCase == TestCase.NoTrigger)
        {
            lmd.IgnoreTrigger = true;
            lmd.SaveToDatabase();
        }

        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");

        //create a text file to load where we update Frank's favourite colour (it's a pk field) and we insert a new record (MrMurder)
        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @"Name,DateOfBirth,FavouriteColour
Frank,2001-01-01,Neon
MrMurder,2001-01-01,Yella");


        //the checks will probably need to be run as ddl admin because it involves creating _Archive table and trigger the first time

        //clean SetUp RAW / STAGING etc and generally accept proposed cleanup operations
        var checker =
            new CheckEntireDataLoadProcess(lmd, new HICDatabaseConfiguration(lmd), new HICLoadConfigurationFlags());
        checker.Check(new AcceptAllCheckNotifier());

        //create a reader
        if (testCase == TestCase.LowPrivilegeLoaderAccount)
        {
            SetupLowPrivilegeUserRightsFor(ti, TestLowPrivilegePermissions.Reader | TestLowPrivilegePermissions.Writer);
            SetupLowPrivilegeUserRightsFor(db.Server.ExpectDatabase("DLE_STAGING"), TestLowPrivilegePermissions.All);
        }

        Assert.Multiple(() =>
        {
            Assert.That(tbl.DiscoverColumns().Select(c => c.GetRuntimeName()).Contains(SpecialFieldNames.DataLoadRunID), Is.EqualTo(testCase != TestCase.NoTrigger),
                    $"When running with NoTrigger there shouldn't be any additional columns added to table. Test case was {testCase}");
            Assert.That(tbl.DiscoverColumns().Select(c => c.GetRuntimeName()).Contains(SpecialFieldNames.ValidFrom), Is.EqualTo(testCase != TestCase.NoTrigger),
                $"When running with NoTrigger there shouldn't be any additional columns added to table. Test case was {testCase}");
        });

        var dbConfig = new HICDatabaseConfiguration(lmd,
            testCase == TestCase.WithCustomTableNamer ? new CustomINameDatabasesAndTablesDuringLoads() : null);

        if (testCase == TestCase.WithCustomTableNamer)
            new PreExecutionChecker(lmd, dbConfig).Check(
                new AcceptAllCheckNotifier()); //handles staging database creation etc

        if (testCase == TestCase.WithDiffColumnIgnoreRegex)
            dbConfig.UpdateButDoNotDiff = new Regex("^FavouriteColour"); //do not diff FavouriteColour


        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        try
        {
            var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

            var exitCode = exe.Run(
                new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                    ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig),
                new GracefulCancellationToken());

            Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));

            if (testCase == TestCase.AllPrimaryKeys)
            {
                Assert.That(tbl.GetRowCount(), Is.EqualTo(4)); //Bob, Frank, Frank (with also pk Neon) & MrMurder
                Assert.Pass();
            }

            if (testCase == TestCase.WithDiffColumnIgnoreRegex)
            {
                Assert.That(tbl.GetRowCount(), Is.EqualTo(3)); //Bob, Frank (original since the diff was skipped), & MrMurder

                //frank should be updated to like Neon instead of Orange
                Assert.That(tbl.GetRowCount(), Is.EqualTo(3));
                var frankOld = tbl.GetDataTable().Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "Frank");
                Assert.That(frankOld["FavouriteColour"], Is.EqualTo("Orange"));
                Assert.Pass();
            }

            //frank should be updated to like Neon instead of Orange
            Assert.That(tbl.GetRowCount(), Is.EqualTo(3));
            var result = tbl.GetDataTable();
            var frank = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "Frank");
            Assert.That(frank["FavouriteColour"], Is.EqualTo("Neon"));

            if (testCase != TestCase.NoTrigger)
                AssertHasDataLoadRunId(frank);

            //MrMurder is a new person who likes Yella
            var mrmurder = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "MrMurder");
            Assert.Multiple(() =>
            {
                Assert.That(mrmurder["FavouriteColour"], Is.EqualTo("Yella"));
                Assert.That(mrmurder["DateOfBirth"], Is.EqualTo(new DateTime(2001, 01, 01)));
            });

            if (testCase != TestCase.NoTrigger)
                AssertHasDataLoadRunId(mrmurder);

            //bob should be untouched (same values as before and no dataloadrunID)
            var bob = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "Bob");
            Assert.Multiple(() =>
            {
                Assert.That(bob["FavouriteColour"], Is.EqualTo("Pink"));
                Assert.That(bob["DateOfBirth"], Is.EqualTo(new DateTime(2001, 01, 01)));
            });

            if (testCase != TestCase.NoTrigger)
            {
                Assert.That(bob[SpecialFieldNames.DataLoadRunID], Is.EqualTo(DBNull.Value));

                //MySql add default of now() on a table will auto populate all the column values with the the now() date while Sql Server will leave them as nulls
                if (databaseType == DatabaseType.MicrosoftSQLServer)
                    Assert.That(bob[SpecialFieldNames.ValidFrom], Is.EqualTo(DBNull.Value));
            }

            Assert.Multiple(() =>
            {
                Assert.That(tbl.DiscoverColumns().Select(c => c.GetRuntimeName()).Contains(SpecialFieldNames.DataLoadRunID), Is.EqualTo(testCase != TestCase.NoTrigger),
                            $"When running with NoTrigger there shouldn't be any additional columns added to table. Test case was {testCase}");
                Assert.That(tbl.DiscoverColumns().Select(c => c.GetRuntimeName()).Contains(SpecialFieldNames.ValidFrom), Is.EqualTo(testCase != TestCase.NoTrigger),
                    $"When running with NoTrigger there shouldn't be any additional columns added to table. Test case was {testCase}");
            });
        }
        finally
        {
            Directory.Delete(lmd.LocationOfForLoadingDirectory, true);
            Directory.Delete(lmd.LocationOfForArchivingDirectory, true);
            Directory.Delete(lmd.LocationOfExecutablesDirectory, true);
            Directory.Delete(lmd.LocationOfCacheDirectory, true);

            foreach (var c in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
                c.DeleteInDatabase();

            foreach (var t in RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>())
                t.DeleteInDatabase();

            foreach (var l in RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                l.DeleteInDatabase();
        }

        if (testCase == TestCase.WithCustomTableNamer)
        {
            var db2 = db.Server.ExpectDatabase("BB_STAGING");
            if (db.Exists())
                db2.Drop();
        }
    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void DLELoadTwoTables(DatabaseType databaseType)
    {
        //setup the data tables
        var logServer = CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);

        var db = GetCleanedServer(databaseType);

        var dtParent = new DataTable();
        dtParent.Columns.Add("ID", typeof(int));
        dtParent.Columns.Add("Name");
        dtParent.Columns.Add("Height");
        dtParent.PrimaryKey = new[] { dtParent.Columns[0] };

        dtParent.Rows.Add("1", "Dave", "3.5");

        var dtChild = new DataTable();
        dtChild.Columns.Add("Parent_ID");
        dtChild.Columns.Add("ChildNumber");
        dtChild.Columns.Add("Name");
        dtChild.Columns.Add("DateOfBirth");
        dtChild.Columns.Add("Age");
        dtChild.Columns.Add("Height");

        dtChild.Rows.Add("1", "1", "Child1", "2001-01-01", "20", "3.5");
        dtChild.Rows.Add("1", "2", "Child2", "2002-01-01", "19", "3.4");

        dtChild.PrimaryKey = new[] { dtChild.Columns[0], dtChild.Columns[1] };

        //create the parent table based on the DataTable
        var parentTbl = db.CreateTable("Parent", dtParent);

        //go find the primary key column created
        var pkParentID = parentTbl.DiscoverColumn("ID");

        //forward declare this column as part of pk (will be used to specify foreign key
        var fkParentID = new DatabaseColumnRequest("Parent_ID", "int") { IsPrimaryKey = true };

        var args = new CreateTableArgs(
            db,
            "Child",
            null,
            dtChild,
            false,
            new Dictionary<DatabaseColumnRequest, DiscoveredColumn>
            {
                { fkParentID, pkParentID }
            },
            true)
        {
            ExplicitColumnDefinitions = new[]
            {
                fkParentID
            }
        };

        var childTbl = db.CreateTable(args);

        Assert.Multiple(() =>
        {
            Assert.That(parentTbl.GetRowCount(), Is.EqualTo(1));
            Assert.That(childTbl.GetRowCount(), Is.EqualTo(2));
        });

        //create a new load
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoading2");

        var childTableInfo = Import(childTbl, lmd, logManager);
        var parentTableInfo = Import(parentTbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, parentTableInfo, "parent.csv");
        CreateCSVProcessTask(lmd, childTableInfo, "child.csv");

        //create a text file to load where we update Frank's favourite colour (it's a pk field) and we insert a new record (MrMurder)
        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "parent.csv"),
            @"ID,Name,Height
2,Man2,3.1
1,Dave,3.2");

        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "child.csv"),
            @"Parent_ID,ChildNumber,Name,DateOfBirth,Age,Height
1,1,UpdC1,2001-01-01,20,3.5
2,1,NewC1,2000-01-01,19,null");


        //clean SetUp RAW / STAGING etc and generally accept proposed cleanup operations
        var checker =
            new CheckEntireDataLoadProcess(lmd, new HICDatabaseConfiguration(lmd), new HICLoadConfigurationFlags());
        checker.Check(new AcceptAllCheckNotifier());

        var config = new HICDatabaseConfiguration(lmd);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            config,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );
        try
        {
            var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

            var exitCode = exe.Run(
                new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                    ThrowImmediatelyDataLoadEventListener.Quiet, config),
                new GracefulCancellationToken());

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));

                //should now be 2 parents (the original - who was updated) + 1 new one (Man2)
                Assert.That(parentTbl.GetRowCount(), Is.EqualTo(2));
            });
            var result = parentTbl.GetDataTable();
            var dave = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "Dave");
            Assert.That(dave["Height"], Is.EqualTo(3.2f)); //should now be only 3.2 inches high
            AssertHasDataLoadRunId(dave);

            //should be 3 children (Child1 who gets updated to be called UpdC1) and NewC1
            Assert.That(childTbl.GetRowCount(), Is.EqualTo(3));
            result = childTbl.GetDataTable();

            var updC1 = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "UpdC1");
            Assert.Multiple(() =>
            {
                Assert.That(updC1["Parent_ID"], Is.EqualTo(1));
                Assert.That(updC1["ChildNumber"], Is.EqualTo(1));
            });
            AssertHasDataLoadRunId(updC1);

            var newC1 = result.Rows.Cast<DataRow>().Single(r => (string)r["Name"] == "NewC1");
            Assert.Multiple(() =>
            {
                Assert.That(newC1["Parent_ID"], Is.EqualTo(2));
                Assert.That(newC1["ChildNumber"], Is.EqualTo(1));
                Assert.That(newC1["Height"], Is.EqualTo(DBNull.Value)); //the "null" in the input file should be DBNull.Value in the final database
            });
            AssertHasDataLoadRunId(newC1);
        }
        finally
        {
            Directory.Delete(lmd.LocationOfForLoadingDirectory, true);
            Directory.Delete(lmd.LocationOfForArchivingDirectory, true);
            Directory.Delete(lmd.LocationOfExecutablesDirectory, true);
            Directory.Delete(lmd.LocationOfCacheDirectory, true);

            foreach (var c in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
                c.DeleteInDatabase();

            foreach (var t in RepositoryLocator.CatalogueRepository.GetAllObjects<TableInfo>())
                t.DeleteInDatabase();

            foreach (var l in RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                l.DeleteInDatabase();
        }
    }
}

internal class CustomINameDatabasesAndTablesDuringLoads : INameDatabasesAndTablesDuringLoads
{
    public string GetDatabaseName(string rootDatabaseName, LoadBubble convention)
    {
        //RAW is AA, Staging is BB
        return convention switch
        {
            LoadBubble.Raw => "AA_RAW",
            LoadBubble.Staging => "BB_STAGING",
            LoadBubble.Live => rootDatabaseName,
            LoadBubble.Archive => rootDatabaseName,
            _ => throw new ArgumentOutOfRangeException(nameof(convention))
        };
    }

    public string GetName(string tableName, LoadBubble convention) =>
        //all tables get called CC
        convention < LoadBubble.Live ? "CC" : tableName;
}