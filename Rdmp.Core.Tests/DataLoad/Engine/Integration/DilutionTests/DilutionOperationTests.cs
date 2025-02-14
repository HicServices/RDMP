// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using FAnsi;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Mutilators.Dilution;
using Rdmp.Core.DataLoad.Modules.Mutilators.Dilution.Operations;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.DilutionTests;

public class DilutionOperationTests : DatabaseTests
{
    [TestCase("2001-01-03", "2001-02-15")]
    [TestCase("2001-03-31", "2001-02-15")]
    [TestCase("2001-04-01", "2001-05-15")]
    [TestCase("2001-03-31 23:59:59", "2001-02-15")]
    [TestCase("2001-04-01 01:15:00", "2001-05-15")]
    [TestCase(null, null)]
    public void TestRoundDateToMiddleOfQuarter(string input, string expectedDilute)
    {
        var tbl = Substitute.For<ITableInfo>();
        tbl.GetRuntimeName(LoadStage.AdjustStaging, null).Returns("DateRoundingTests");
        var col = Substitute.For<IPreLoadDiscardedColumn>();
        col.TableInfo.Returns(tbl);
        col.GetRuntimeName().Returns("TestField");

        var o = new RoundDateToMiddleOfQuarter
        {
            ColumnToDilute = col
        };
        var sql = o.GetMutilationSql(null);

        var server = GetCleanedServer(DatabaseType.MicrosoftSQLServer).Server;
        using var con = server.BeginNewTransactedConnection();
        try
        {
            var insert = input != null ? $"'{input}'" : "NULL";

            server.GetCommand($@"CREATE TABLE DateRoundingTests(TestField datetime)
INSERT INTO DateRoundingTests VALUES ({insert})", con).ExecuteNonQuery();

            UsefulStuff.ExecuteBatchNonQuery(sql, con.Connection, con.Transaction);

            var result = server.GetCommand("SELECT * from DateRoundingTests", con).ExecuteScalar();

            if (expectedDilute == null)
                Assert.That(result, Is.EqualTo(DBNull.Value));
            else
                Assert.That(result, Is.EqualTo(DateTime.Parse(expectedDilute)));
        }
        finally
        {
            con.ManagedTransaction.AbandonAndCloseConnection();
        }
    }


    [TestCase("DD3 9TA", "DD3")]
    [TestCase("DD03 9TA", "DD03")]
    [TestCase("EC4V 2AU", "EC4V")] //London postcodes have extra digits
    [TestCase("EC4V", "EC4V")] //Already is a prefix
    [TestCase("DD3", "DD3")] //Already is a prefix
    [TestCase("DD3_5L1", "DD3")] //Makey upey suffix
    [TestCase("DD3_XXX", "DD3")] //Makey upey suffix
    [TestCase("!D!D!3!9TA!", "DD3")] //Random garbage
    [TestCase("EC4V_2AU", "EC4V")] //underscore instead of space
    [TestCase("EC4V2AU   ", "EC4V")] //Trailing whitespace
    [TestCase("??",
        "??")] //It's short and it's complete garbage but this is the kind of thing research datasets have :)
    [TestCase("???????",
        "????")] //Return type is varchar(4) so while we reject the original value we still end SetUp truncating it
    [TestCase("I<3Coffee Yay", "I3Co")] //What can you do?!, got to return varchar(4)
    [TestCase("D3 9T",
        "D39T")] //39T isn't a valid suffix and the remainder (D) wouldn't be enough for a postcode prefix anyway so just return the original input minus dodgy characters
    [TestCase("G    9TA",
        "G")] //9TA is the correct suffix pattern (Numeric Alpha Alpha) so can be chopped off and the remainder returned (G)
    [TestCase("DD3 9T",
        "DD")] //Expected to get it wrong because the suffix check sees 39T but the remainder is long enough to make a legit postcode (2).  We are currently deciding not to evaluate spaces/other dodgy characters when attempting to resolve postcodes
    [TestCase(null, null)]
    public void TestExcludeRight3OfUKPostcodes(string input, string expectedDilute)
    {
        var tbl = Substitute.For<ITableInfo>();
        tbl.GetRuntimeName(LoadStage.AdjustStaging, null).Returns("ExcludeRight3OfPostcodes");
        var col = Substitute.For<IPreLoadDiscardedColumn>();
        col.TableInfo.Returns(tbl);
        col.GetRuntimeName().Returns("TestField");

        var o = new ExcludeRight3OfUKPostcodes
        {
            ColumnToDilute = col
        };
        var sql = o.GetMutilationSql(null);

        var server = GetCleanedServer(DatabaseType.MicrosoftSQLServer).Server;
        using var con = server.BeginNewTransactedConnection();
        try
        {
            var insert = input != null ? $"'{input}'" : "NULL";

            server.GetCommand($@"CREATE TABLE ExcludeRight3OfPostcodes(TestField varchar(15))
    INSERT INTO ExcludeRight3OfPostcodes VALUES ({insert})", con).ExecuteNonQuery();

            UsefulStuff.ExecuteBatchNonQuery(sql, con.Connection, con.Transaction);

            var result = server.GetCommand("SELECT * from ExcludeRight3OfPostcodes", con).ExecuteScalar();

            if (expectedDilute == null)
                Assert.That(result, Is.EqualTo(DBNull.Value));
            else
                Assert.That(result, Is.EqualTo(expectedDilute));
        }
        finally
        {
            con.ManagedTransaction.AbandonAndCloseConnection();
        }
    }

    [TestCase("2001-01-03", "datetime", true)]
    [TestCase("2001-01-03", "varchar(50)", true)]
    [TestCase(null, "varchar(50)", false)]
    [TestCase(null, "bit", false)]
    [TestCase("1", "bit", true)]
    [TestCase("0", "bit", true)]
    [TestCase("", "varchar(1)", true)] //This data exists regardless of if it is blank so it still gets the 1
    public void DiluteToBitFlag(string input, string inputDataType, bool expectedDilute)
    {
        var tbl = Substitute.For<ITableInfo>();
        tbl.GetRuntimeName(LoadStage.AdjustStaging, null).Returns("DiluteToBitFlagTests");
        var col = Substitute.For<IPreLoadDiscardedColumn>();
        col.TableInfo.Returns(tbl);
        col.GetRuntimeName().Returns("TestField");

        var o = new CrushToBitFlag
        {
            ColumnToDilute = col
        };
        var sql = o.GetMutilationSql(null);

        var server = GetCleanedServer(DatabaseType.MicrosoftSQLServer).Server;
        using var con = server.BeginNewTransactedConnection();
        try
        {
            var insert = input != null ? $"'{input}'" : "NULL";

            server.GetCommand($@"CREATE TABLE DiluteToBitFlagTests(TestField {inputDataType})
INSERT INTO DiluteToBitFlagTests VALUES ({insert})", con).ExecuteNonQuery();

            UsefulStuff.ExecuteBatchNonQuery(sql, con.Connection, con.Transaction);

            var result = server.GetCommand("SELECT * from DiluteToBitFlagTests", con).ExecuteScalar();

            Assert.That(Convert.ToBoolean(result), Is.EqualTo(expectedDilute));
        }
        finally
        {
            con.ManagedTransaction.AbandonAndCloseConnection();
        }
    }

    [Test]
    public void Dilution_WithNamer_Test()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        using var dt = new DataTable();
        dt.Columns.Add("Bob");
        dt.Rows.Add("Fish");

        var tbl = db.CreateTable("DilutionNamerTest", dt);
        Import(tbl, out var ti, out _);

        tbl.Rename("AAAA");
        var namer = RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "AAAA");

        var discarded = new PreLoadDiscardedColumn(CatalogueRepository, ti, "Bob")
        {
            SqlDataType = "varchar(10)",
            Destination = DiscardedColumnDestination.Dilute
        };
        discarded.SaveToDatabase();


        var dilution = new Dilution
        {
            ColumnToDilute = discarded,
            Operation = typeof(CrushToBitFlag)
        };

        dilution.Initialize(db, LoadStage.AdjustStaging);
        dilution.Check(ThrowImmediatelyCheckNotifier.Quiet);

        var job = new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(db.Server, namer), ti);

        dilution.Mutilate(job);
    }
}