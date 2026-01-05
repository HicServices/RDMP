// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class SafePrimaryKeyCollisionResolverMutilationTests : DatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MySql, true)]
    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    [TestCase(DatabaseType.MySql, false)]
    public void SafePrimaryKeyCollisionResolverMutilationTests_NoDifference_NoRecordsDeleted(DatabaseType dbType,
        bool bothNull)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable();
        dt.Columns.Add("PK");
        dt.Columns.Add("ResolveOn");
        dt.Columns.Add("AnotherCol");

        dt.Rows.Add(1, bothNull ? null : "fish", "cat");
        dt.Rows.Add(1, bothNull ? null : "fish", "flop");
        dt.Rows.Add(2, "fish", "flop");
        dt.Rows.Add(3, "dave", "franl");

        var tbl = db.CreateTable("MyTable", dt);

        Import(tbl, out var ti, out var cis);

        var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

        var mutilation = new SafePrimaryKeyCollisionResolverMutilation
        {
            ColumnToResolveOn = resolveOn,
            PreferLargerValues = true,
            PreferNulls = false
        };

        mutilation.Initialize(db, LoadStage.AdjustRaw);
        mutilation.Mutilate(new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(db.Server)));

        Assert.That(tbl.GetRowCount(), Is.EqualTo(4));
    }

    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    [TestCase(DatabaseType.MySql, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MySql, true)]
    public void SafePrimaryKeyCollisionResolverMutilationTests_PreferNull_RecordsDeleted(DatabaseType dbType,
        bool preferNulls)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable();
        dt.Columns.Add("PK");
        dt.Columns.Add("ResolveOn");
        dt.Columns.Add("AnotherCol");

        dt.Rows.Add(1, null, "cat");
        dt.Rows.Add(1, "fish", "flop");
        dt.Rows.Add(2, "fish", "flop");
        dt.Rows.Add(3, "dave", "franl");

        var tbl = db.CreateTable("MyTable", dt);

        Import(tbl, out var ti, out var cis);

        var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

        var mutilation = new SafePrimaryKeyCollisionResolverMutilation
        {
            ColumnToResolveOn = resolveOn,
            PreferLargerValues = true,
            PreferNulls = preferNulls
        };

        mutilation.Initialize(db, LoadStage.AdjustRaw);
        mutilation.Mutilate(new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(db.Server)));

        Assert.That(tbl.GetRowCount(), Is.EqualTo(3));
        var result = tbl.GetDataTable();

        Assert.Multiple(() =>
        {
            //if you prefer nulls you shouldn't want this one
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && r["ResolveOn"] as string == "fish" && r["AnotherCol"] as string == "flop"), Is.EqualTo(preferNulls ? 0 : 1));

            //if you prefer nulls you should have this one
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && r["ResolveOn"] == DBNull.Value && r["AnotherCol"] as string == "cat"), Is.EqualTo(preferNulls ? 1 : 0));
        });
    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void SafePrimaryKeyCollisionResolverMutilationTests_WithDatabaseNamer_RecordsDeleted(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable();
        dt.Columns.Add("PK");
        dt.Columns.Add("ResolveOn");
        dt.Columns.Add("AnotherCol");

        dt.Rows.Add(1, null, "cat");
        dt.Rows.Add(1, "fish", "flop");
        dt.Rows.Add(2, "fish", "flop");
        dt.Rows.Add(3, "dave", "franl");

        var tbl = db.CreateTable("MyTable", dt);

        Import(tbl, out var ti, out var cis);

        tbl.Rename("AAAA");
        var namer = RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "AAAA");

        var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

        var mutilation = new SafePrimaryKeyCollisionResolverMutilation
        {
            ColumnToResolveOn = resolveOn,
            PreferLargerValues = true,
            PreferNulls = true
        };

        mutilation.Initialize(db, LoadStage.AdjustRaw);
        mutilation.Mutilate(new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(db.Server, namer), ti));

        Assert.That(tbl.GetRowCount(), Is.EqualTo(3));
        var result = tbl.GetDataTable();

        Assert.Multiple(() =>
        {
            //if you prefer nulls you shouldn't want this one
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && r["ResolveOn"] as string == "fish" && r["AnotherCol"] as string == "flop"), Is.EqualTo(0));

            //if you prefer nulls you should have this one
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && r["ResolveOn"] == DBNull.Value && r["AnotherCol"] as string == "cat"), Is.EqualTo(1));
        });
    }


    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    [TestCase(DatabaseType.MySql, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MySql, true)]
    public void SafePrimaryKeyCollisionResolverMutilationTests_PreferLarger_RecordsDeleted(DatabaseType dbType,
        bool preferLarger)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable();
        dt.Columns.Add("PK");
        dt.Columns.Add("ResolveOn");
        dt.Columns.Add("AnotherCol");

        dt.Rows.Add(1, null, "cat");
        dt.Rows.Add(1, "a", "flop");
        dt.Rows.Add(1, "b", "flop");
        dt.Rows.Add(2, "fish", "flop");
        dt.Rows.Add(3, "dave", "franl");

        var tbl = db.CreateTable("MyTable", dt);

        Import(tbl, out var ti, out var cis);

        var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

        var mutilation = new SafePrimaryKeyCollisionResolverMutilation
        {
            ColumnToResolveOn = resolveOn,
            PreferLargerValues = preferLarger,
            PreferNulls = false
        };

        mutilation.Initialize(db, LoadStage.AdjustRaw);
        mutilation.Mutilate(new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(db.Server)));

        Assert.That(tbl.GetRowCount(), Is.EqualTo(3));
        var result = tbl.GetDataTable();

        Assert.Multiple(() =>
        {
            //if you like larger values (alphabetically) then you want the 'b'
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && r["ResolveOn"] as string == "b" && r["AnotherCol"] as string == "flop"), Is.EqualTo(preferLarger ? 1 : 0));
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && r["ResolveOn"] as string == "a" && r["AnotherCol"] as string == "flop"), Is.EqualTo(preferLarger ? 0 : 1));

            //either way you shouldn't have the null one
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && r["ResolveOn"] == DBNull.Value && r["AnotherCol"] as string == "cat"), Is.EqualTo(0));
        });
    }


    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    [TestCase(DatabaseType.MySql, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MySql, true)]
    public void SafePrimaryKeyCollisionResolverMutilationTests_PreferLarger_Dates_RecordsDeleted(DatabaseType dbType,
        bool preferLarger)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable();
        dt.Columns.Add("PK");
        dt.Columns.Add("ResolveOn");
        dt.Columns.Add("AnotherCol");

        dt.Rows.Add(1, null, "cat");
        dt.Rows.Add(1, new DateTime(2001, 01, 01), "flop");
        dt.Rows.Add(1, new DateTime(2002, 01, 01), "flop");
        dt.Rows.Add(2, null, "flop");
        dt.Rows.Add(3, null, "franl");

        var tbl = db.CreateTable("MyTable", dt);

        Import(tbl, out var ti, out var cis);

        var pk = cis.Single(c => c.GetRuntimeName().Equals("PK"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

        var mutilation = new SafePrimaryKeyCollisionResolverMutilation
        {
            ColumnToResolveOn = resolveOn,
            PreferLargerValues = preferLarger,
            PreferNulls = false
        };

        mutilation.Initialize(db, LoadStage.AdjustRaw);
        mutilation.Mutilate(new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(db.Server)));

        Assert.That(tbl.GetRowCount(), Is.EqualTo(3));
        var result = tbl.GetDataTable();

        Assert.Multiple(() =>
        {
            //if you like larger values then you want 2002 that's larger than 2001
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && Equals(r["ResolveOn"], new DateTime(2002, 01, 01)) &&
                    r["AnotherCol"] as string == "flop"), Is.EqualTo(preferLarger ? 1 : 0));
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && Equals(r["ResolveOn"], new DateTime(2001, 01, 01)) &&
                    r["AnotherCol"] as string == "flop"), Is.EqualTo(preferLarger ? 0 : 1));

            //either way you shouldn't have the null one
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK"] == 1 && r["ResolveOn"] == DBNull.Value && r["AnotherCol"] as string == "cat"), Is.EqualTo(0));
        });
    }

    [TestCase(DatabaseType.MicrosoftSQLServer, false)]
    [TestCase(DatabaseType.MySql, false)]
    [TestCase(DatabaseType.MicrosoftSQLServer, true)]
    [TestCase(DatabaseType.MySql, true)]
    public void SafePrimaryKeyCollisionResolverMutilationTests_PreferLarger_ComboKey_RecordsDeleted(DatabaseType dbType,
        bool preferLarger)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable();
        dt.Columns.Add("PK1");
        dt.Columns.Add("PK2");
        dt.Columns.Add("ResolveOn");
        dt.Columns.Add("AnotherCol");

        dt.Rows.Add(1, 1, null, "cat");
        dt.Rows.Add(1, 1, new DateTime(2001, 01, 01), "flop");
        dt.Rows.Add(1, 1, new DateTime(2002, 01, 01), "flop");

        dt.Rows.Add(1, 2, null, "cat");
        dt.Rows.Add(1, 2, null, "cat");
        dt.Rows.Add(1, 3, new DateTime(2001, 01, 01), "flop");
        dt.Rows.Add(1, 4, new DateTime(2002, 01, 01), "flop");

        dt.Rows.Add(2, 1, null, "flop");
        dt.Rows.Add(3, 1, null, "franl");

        var tbl = db.CreateTable("MyTable", dt);

        Import(tbl, out var ti, out var cis);

        var pk = cis.Single(c => c.GetRuntimeName().Equals("PK1"));
        pk.IsPrimaryKey = true;
        pk.SaveToDatabase();

        var pk2 = cis.Single(c => c.GetRuntimeName().Equals("PK2"));
        pk2.IsPrimaryKey = true;
        pk2.SaveToDatabase();

        var resolveOn = cis.Single(c => c.GetRuntimeName().Equals("ResolveOn"));

        var mutilation = new SafePrimaryKeyCollisionResolverMutilation
        {
            ColumnToResolveOn = resolveOn,
            PreferLargerValues = preferLarger,
            PreferNulls = false
        };

        mutilation.Initialize(db, LoadStage.AdjustRaw);
        mutilation.Mutilate(new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(db.Server)));

        Assert.That(tbl.GetRowCount(), Is.EqualTo(7));
        var result = tbl.GetDataTable();

        Assert.Multiple(() =>
        {
            //if you like larger values then you want 2002 that's larger than 2001
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK1"] == 1 && (int)r["PK2"] == 1 && Equals(r["ResolveOn"], new DateTime(2002, 01, 01)) &&
                    r["AnotherCol"] as string == "flop"), Is.EqualTo(preferLarger ? 1 : 0));
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK1"] == 1 && (int)r["PK2"] == 1 && Equals(r["ResolveOn"], new DateTime(2001, 01, 01)) &&
                    r["AnotherCol"] as string == "flop"), Is.EqualTo(preferLarger ? 0 : 1));

            //either way you shouldn't have the null one
            Assert.That(result.Rows.Cast<DataRow>().Count(r =>
                    (int)r["PK1"] == 1 && (int)r["PK2"] == 1 && r["ResolveOn"] == DBNull.Value &&
                    r["AnotherCol"] as string == "cat"), Is.EqualTo(0));
        });
    }
}