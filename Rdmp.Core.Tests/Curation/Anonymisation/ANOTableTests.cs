// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.Curation.Anonymisation;

public class ANOTableTests : TestsRequiringANOStore
{
    private Regex _anochiPattern = new(@"\d{10}_A");

    #region Create New ANOTables

    [Test]
    [TestCase("varchar(1)")]
    [TestCase("int")]
    [TestCase("tinyint")]
    [TestCase("bit")]
    public void CreateAnANOTable_PushAs(string datatypeForPush)
    {
        var anoTable = GetANOTable();
        Assert.AreEqual("ANOMyTable", anoTable.TableName);
        anoTable.NumberOfCharactersToUseInAnonymousRepresentation = 20;
        anoTable.NumberOfIntegersToUseInAnonymousRepresentation = 20;
        anoTable.PushToANOServerAsNewTable(datatypeForPush, ThrowImmediatelyCheckNotifier.Quiet());

        var discoveredTable = ANOStore_Database.DiscoverTables(false)
            .SingleOrDefault(t => t.GetRuntimeName().Equals("ANOMyTable"));

        //server should have 
        Assert.NotNull(discoveredTable);
        Assert.IsTrue(discoveredTable.Exists());

        //yes that's right hte table name and column name are the same here \|/
        Assert.AreEqual(datatypeForPush, discoveredTable.DiscoverColumn("MyTable").DataType.SQLType);

        //20 + 20 + _ + A
        Assert.AreEqual("varchar(42)", discoveredTable.DiscoverColumn("ANOMyTable").DataType.SQLType);

        anoTable.DeleteInDatabase();
    }

    [Test]
    public void CreateAnANOTable_Revertable()
    {
        var anoTable = GetANOTable();

        anoTable.NumberOfCharactersToUseInAnonymousRepresentation = 63;
        anoTable.RevertToDatabaseState();
        Assert.AreEqual(1, anoTable.NumberOfCharactersToUseInAnonymousRepresentation);
        anoTable.DeleteInDatabase();
    }

    [Test]
    public void CreateAnANOTable_Check()
    {
        var anoTable = GetANOTable();
        Assert.AreEqual("ANOMyTable", anoTable.TableName);
        anoTable.Check(new AcceptAllCheckNotifier());
        anoTable.DeleteInDatabase();
    }

    [Test]
    public void DuplicateSuffix_Throws()
    {
        var anoTable = GetANOTable();
        try
        {
            var ex = Assert.Throws<Exception>(() =>
                new ANOTable(CatalogueRepository, anoTable.Server, "DuplicateSuffix", anoTable.Suffix));
            Assert.AreEqual("There is already another ANOTable with the suffix 'A'", ex.Message);
        }
        finally
        {
            anoTable.DeleteInDatabase();
        }
    }

    [Test]
    public void CreateAnANOTable_CharCountNegative()
    {
        var anoTable = GetANOTable();
        try
        {
            anoTable.NumberOfCharactersToUseInAnonymousRepresentation = -500;
            var ex = Assert.Throws<Exception>(anoTable.SaveToDatabase);
            Assert.AreEqual("NumberOfCharactersToUseInAnonymousRepresentation cannot be negative", ex.Message);
        }
        finally
        {
            anoTable.DeleteInDatabase();
        }
    }

    [Test]
    public void CreateAnANOTable_IntCountNegative()
    {
        var anoTable = GetANOTable();

        try
        {
            anoTable.NumberOfIntegersToUseInAnonymousRepresentation = -500;
            var ex = Assert.Throws<Exception>(anoTable.SaveToDatabase);
            Assert.AreEqual("NumberOfIntegersToUseInAnonymousRepresentation cannot be negative", ex.Message);
        }
        finally
        {
            anoTable.DeleteInDatabase();
        }
    }

    [Test]
    public void CreateAnANOTable_TotalCountZero()
    {
        var anoTable = GetANOTable();
        try
        {
            anoTable.NumberOfIntegersToUseInAnonymousRepresentation = 0;
            anoTable.NumberOfCharactersToUseInAnonymousRepresentation = 0;
            var ex = Assert.Throws<Exception>(anoTable.SaveToDatabase);
            Assert.AreEqual("Anonymous representations must have at least 1 integer or character", ex.Message);
        }
        finally
        {
            anoTable.DeleteInDatabase();
        }
    }

    #endregion

    [Test]
    public void SubstituteANOIdentifiers_2CHINumbers()
    {
        var anoTable = GetANOTable();
        anoTable.NumberOfCharactersToUseInAnonymousRepresentation = 0;
        anoTable.NumberOfIntegersToUseInAnonymousRepresentation = 10;
        anoTable.PushToANOServerAsNewTable("varchar(10)",ThrowImmediatelyCheckNotifier.Quiet());


        var dt = new DataTable();
        dt.Columns.Add("CHI");
        dt.Columns.Add("ANOCHI");

        dt.Rows.Add("0101010101", DBNull.Value); //duplicates
        dt.Rows.Add("0101010102", DBNull.Value);
        dt.Rows.Add("0101010101", DBNull.Value); //duplicates

        var transformer = new ANOTransformer(anoTable, ThrowImmediatelyDataLoadEventListener.Quiet);
        transformer.Transform(dt,dt.Columns["CHI"],dt.Columns["ANOCHI"]);

        Assert.IsTrue((string)dt.Rows[0][0] == "0101010101");
        Assert.IsTrue(_anochiPattern.IsMatch((string)dt.Rows[0][1])); //should be 10 digits and then _A
        Assert.AreEqual(dt.Rows[0][1], dt.Rows[2][1]); //because of duplication these should both be the same

        Console.WriteLine($"ANO identifiers created were:{dt.Rows[0][1]},{dt.Rows[1][1]}");

        TruncateANOTable(anoTable);

        //now test previews
        transformer.Transform(dt, dt.Columns["CHI"], dt.Columns["ANOCHI"], true);
        var val1 = dt.Rows[0][1];

        transformer.Transform(dt, dt.Columns["CHI"], dt.Columns["ANOCHI"], true);
        var val2 = dt.Rows[0][1];

        transformer.Transform(dt, dt.Columns["CHI"], dt.Columns["ANOCHI"], true);
        var val3 = dt.Rows[0][1];

        //should always be different
        Assert.AreNotEqual(val1, val2);
        Assert.AreNotEqual(val1, val3);

        //now test repeatability
        transformer.Transform(dt, dt.Columns["CHI"], dt.Columns["ANOCHI"], false);
        var val4 = dt.Rows[0][1];

        transformer.Transform(dt, dt.Columns["CHI"], dt.Columns["ANOCHI"], false);
        var val5 = dt.Rows[0][1];

        transformer.Transform(dt, dt.Columns["CHI"], dt.Columns["ANOCHI"], false);
        var val6 = dt.Rows[0][1];
        Assert.AreEqual(val4, val5);
        Assert.AreEqual(val4, val6);

        TruncateANOTable(anoTable);

        anoTable.DeleteInDatabase();
    }

    [Test]
    public void SubstituteANOIdentifiers_PreviewWithoutPush()
    {
        var anoTable = GetANOTable();
        anoTable.NumberOfCharactersToUseInAnonymousRepresentation = 0;
        anoTable.NumberOfIntegersToUseInAnonymousRepresentation = 10;

        var ANOtable = ANOStore_Database.ExpectTable(anoTable.TableName);

        //should not exist yet
        Assert.False(ANOtable.Exists());
            
        using var dt = new DataTable();
        dt.Columns.Add("CHI");
        dt.Columns.Add("ANOCHI");
        dt.Rows.Add("0101010101", DBNull.Value);
        var transformer = new ANOTransformer(anoTable, ThrowImmediatelyDataLoadEventListener.Quiet);
        transformer.Transform(dt, dt.Columns["CHI"], dt.Columns["ANOCHI"], true);

        Assert.IsTrue(_anochiPattern.IsMatch((string)dt.Rows[0][1])); //should be 10 digits and then _A

        //still not exist yet
        Assert.False(ANOtable.Exists());

        anoTable.DeleteInDatabase();
    }


    [Test]
    public void SubstituteANOIdentifiers_BulkTest()
    {
        const int batchSize = 10000;

        var anoTable = GetANOTable();
        anoTable.NumberOfCharactersToUseInAnonymousRepresentation = 0;
        anoTable.NumberOfIntegersToUseInAnonymousRepresentation = 10;
        anoTable.PushToANOServerAsNewTable("varchar(10)", ThrowImmediatelyCheckNotifier.Quiet());


        var sw = new Stopwatch();
        sw.Start();

        using var dt = new DataTable();
        dt.BeginLoadData();
        dt.Columns.Add("CHI");
        dt.Columns.Add("ANOCHI");

        var r = new Random();

        var uniqueSourceSet = new HashSet<string>();


        for (var i = 0; i < batchSize; i++)
        {
            var val = r.NextDouble() * 9999999999;
            val = Math.Round(val);
            var valAsString = val.ToString();

            while (valAsString.Length < 10)
                valAsString = $"0{valAsString}";

            uniqueSourceSet.Add(valAsString);

            dt.Rows.Add(valAsString, DBNull.Value); //duplicates    
        }

        Console.WriteLine($"Time to allocate in C# memory:{sw.Elapsed}");
        Console.WriteLine($"Allocated {dt.Rows.Count} identifiers ({uniqueSourceSet.Count} unique ones)");

        sw.Reset();
        sw.Start();

        var transformer = new ANOTransformer(anoTable, ThrowImmediatelyDataLoadEventListener.Quiet);
        transformer.Transform(dt, dt.Columns["CHI"], dt.Columns["ANOCHI"]);
        Console.WriteLine($"Time to perform SQL transform and allocation:{sw.Elapsed}");

        sw.Reset();
        sw.Start();
        var uniqueSet = new HashSet<string>();

        foreach (DataRow row in dt.Rows)
        {
            var ANOid= row["ANOCHI"].ToString();
            uniqueSet.Add(ANOid);

            Assert.IsTrue(_anochiPattern.IsMatch(ANOid));
        }

        Console.WriteLine($"Allocated {uniqueSet.Count} anonymous identifiers");


        var server = ANOStore_Database.Server;
        using (var con = server.GetConnection())
        {
            con.Open();

            var cmd = server.GetCommand("Select count(*) from ANOMyTable", con);
            var numberOfRows = Convert.ToInt32(cmd.ExecuteScalar());

            //should be the same number of unique identifiers in memory as in the database
            Assert.AreEqual(uniqueSet.Count, numberOfRows);
            Console.WriteLine($"Found {numberOfRows} unique ones");

            var cmdNulls = server.GetCommand("select count(*) from ANOMyTable where ANOMyTable is null", con);
            var nulls = Convert.ToInt32(cmdNulls.ExecuteScalar());
            Assert.AreEqual(0, nulls);
            Console.WriteLine($"Found {nulls} null ANO identifiers");

            con.Close();
        }

        sw.Stop();
        Console.WriteLine($"Time to evaluate results:{sw.Elapsed}");
        TruncateANOTable(anoTable);

        anoTable.DeleteInDatabase();
    }

    /// <summary>
    /// Creates a new ANOTable called ANOMyTable in the Data Catalogue (and cleans SetUp any old copy kicking around), you will need to set its properties and
    /// call PushToANOServerAsNewTable if you want to use it with an ANOTransformer
    /// </summary>
    /// <returns></returns>
    protected ANOTable GetANOTable()
    {
        const string name = "ANOMyTable";

        var toCleanup = CatalogueRepository.GetAllObjects<ANOTable>().SingleOrDefault(a => a.TableName.Equals(name));

        toCleanup?.DeleteInDatabase();

        return new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, name, "A");
    }
}