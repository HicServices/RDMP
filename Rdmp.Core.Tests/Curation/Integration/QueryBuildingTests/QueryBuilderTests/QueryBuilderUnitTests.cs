// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.QueryBuilding;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.QueryBuildingTests.QueryBuilderTests;

internal class QueryBuilderUnitTests : UnitTests
{
    [Test]
    public void Test_IsPrimaryExtractionTable_TwoTables()
    {
        var c1 = WhenIHaveA<ColumnInfo>();
        var c2 = WhenIHaveA<ColumnInfo>();

        c1.TableInfo.IsPrimaryExtractionTable = true;
        c1.TableInfo.SaveToDatabase();

        c2.TableInfo.IsPrimaryExtractionTable = true;
        c2.TableInfo.SaveToDatabase();

        var builder = new QueryBuilder(null, null);
        builder.AddColumn(new ColumnInfoToIColumn(Repository, c1));
        builder.AddColumn(new ColumnInfoToIColumn(Repository, c2));

        var ex = Assert.Throws<QueryBuildingException>(() => { });

        Assert.That(ex.Message, Does.Contain("There are multiple tables marked as IsPrimaryExtractionTable"));
    }

    [Test]
    public void Test_TwoTables_JoinFound()
    {
        //4 tables
        var c1 = WhenIHaveA<ColumnInfo>();
        var c2 = WhenIHaveA<ColumnInfo>();

        //1 is primary
        c1.TableInfo.IsPrimaryExtractionTable = true;
        c1.TableInfo.SaveToDatabase();

        var j1 = new JoinInfo(Repository, c2, c1, ExtractionJoinType.Inner, null);

        var builder = new QueryBuilder(null, null);
        builder.AddColumn(new ColumnInfoToIColumn(Repository, c1));
        builder.AddColumn(new ColumnInfoToIColumn(Repository, c2));

        Assert.Multiple(() =>
        {
            Assert.That(builder.SQL, Does.Contain("JOIN"));

            //we have 1 legit join go go team!
            Assert.That(builder.JoinsUsedInQuery, Has.Count.EqualTo(1));
        });
        Assert.That(builder.JoinsUsedInQuery[0], Is.EqualTo(j1));
    }

    [Test]
    public void Test_FourTables_MultipleRoutes()
    {
        //4 tables
        var c1 = WhenIHaveA<ColumnInfo>();
        c1.Name = "c1";
        c1.SaveToDatabase();
        c1.TableInfo.Name = "t1";
        c1.TableInfo.IsPrimaryExtractionTable = true; //t1 is primary
        c1.TableInfo.SaveToDatabase();

        var c2 = WhenIHaveA<ColumnInfo>();
        c2.Name = "c2";
        c2.SaveToDatabase();
        c2.TableInfo.Name = "t2";
        c2.TableInfo.SaveToDatabase();

        var c3 = WhenIHaveA<ColumnInfo>();
        c3.Name = "c3";
        c3.SaveToDatabase();
        c3.TableInfo.Name = "t3";
        c3.TableInfo.SaveToDatabase();

        var c4 = WhenIHaveA<ColumnInfo>();
        c4.Name = "c4";
        c4.SaveToDatabase();
        c4.TableInfo.Name = "t4";
        c4.TableInfo.SaveToDatabase();


        /*       c2
         *     /    \
         *   c1      c4
         *     \   /
         *       c3
         *
         * */

        var j1 = new JoinInfo(Repository, c2, c1, ExtractionJoinType.Inner, null);
        var j2 = new JoinInfo(Repository, c3, c1, ExtractionJoinType.Inner, null);
        var j3 = new JoinInfo(Repository, c4, c2, ExtractionJoinType.Inner, null);


        var builder = new QueryBuilder(null, null);
        builder.AddColumn(new ColumnInfoToIColumn(Repository, c1));
        builder.AddColumn(new ColumnInfoToIColumn(Repository, c2));
        builder.AddColumn(new ColumnInfoToIColumn(Repository, c3));
        builder.AddColumn(new ColumnInfoToIColumn(Repository, c4));

        Console.WriteLine(builder.SQL);

        //should be using only 3 of the 4 joins because we already have a route to c4 without a fourth join
        Assert.That(builder.JoinsUsedInQuery, Has.Count.EqualTo(3));
        Assert.That(builder.JoinsUsedInQuery, Does.Contain(j1));
        Assert.That(builder.JoinsUsedInQuery, Does.Contain(j2));
        Assert.That(builder.JoinsUsedInQuery, Does.Contain(j3));
    }
}