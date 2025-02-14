// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using FAnsi;
using NUnit.Framework;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.CrossDatabaseTypeTests;

internal class HowDoWeAchieveMd5Test : DatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void TestMd5String(DatabaseType type)
    {
        var dt = new DataTable();
        dt.Columns.Add("F");
        dt.Rows.Add("Fish");

        var db = GetCleanedServer(type);
        var tbl = db.CreateTable("MD5Test", dt);

        var col = tbl.DiscoverColumn("F");

        var sql =
            $"SELECT {tbl.GetQuerySyntaxHelper().HowDoWeAchieveMd5(col.GetFullyQualifiedName())} FROM {tbl.GetFullyQualifiedName()}";


        using var con = db.Server.GetConnection();
        con.Open();
        var cmd = db.Server.GetCommand(sql, con);
        var value = cmd.ExecuteScalar();


        Console.WriteLine($"Value was:{value}");

        Assert.That(value, Is.Not.Null);
        Assert.That(value, Is.Not.EqualTo("Fish"));
        Assert.That(value.ToString(), Has.Length.GreaterThanOrEqualTo(32));
    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void TestMd5Date(DatabaseType type)
    {
        var dt = new DataTable();
        dt.Columns.Add("F");
        dt.Rows.Add("2001-01-01");

        var db = GetCleanedServer(type);
        var tbl = db.CreateTable("MD5Test", dt);

        var col = tbl.DiscoverColumn("F");


        Assert.That(tbl.GetQuerySyntaxHelper().TypeTranslater.GetCSharpTypeForSQLDBType(col.DataType.SQLType), Is.EqualTo(typeof(DateTime)));


        var sql =
            $"SELECT {tbl.GetQuerySyntaxHelper().HowDoWeAchieveMd5(col.GetFullyQualifiedName())} FROM {tbl.GetFullyQualifiedName()}";


        using var con = db.Server.GetConnection();
        con.Open();
        var cmd = db.Server.GetCommand(sql, con);
        var value = cmd.ExecuteScalar();


        Console.WriteLine($"Value was:{value}");

        Assert.That(value, Is.Not.Null);
        Assert.That(value.ToString(), Has.Length.GreaterThanOrEqualTo(32));
    }
}