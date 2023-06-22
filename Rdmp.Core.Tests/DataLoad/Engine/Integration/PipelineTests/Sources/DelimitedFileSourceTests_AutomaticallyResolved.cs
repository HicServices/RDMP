// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using NUnit.Framework;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Sources;

public class DelimitedFileSourceTests_AutomaticallyResolved : DelimitedFileSourceTestsBase
{
    [Test]
    public void Test_ValuesSpradAcrossMultipleRows()
    {

        var file = CreateTestFile(
            "A,B,C,D",
            "1,17,Beginning of a long,37.00",
            ",,description of something,",
            ",,really boring,"
        );

        var dt = RunGetChunk(file);
        Assert.AreEqual(3,dt.Rows.Count);
        Assert.AreEqual(true, dt.Rows[0]["A"]);
        Assert.AreEqual(17, dt.Rows[0]["B"]);
        Assert.AreEqual("Beginning of a long", dt.Rows[0]["C"]);
        Assert.AreEqual(37, dt.Rows[0]["D"]);


        Assert.AreEqual(DBNull.Value, dt.Rows[1]["A"]);
        Assert.AreEqual(DBNull.Value, dt.Rows[1]["B"]);
        Assert.AreEqual("description of something", dt.Rows[1]["C"]);
        Assert.AreEqual(DBNull.Value, dt.Rows[1]["D"]);


        Assert.AreEqual(DBNull.Value, dt.Rows[2]["A"]);
        Assert.AreEqual(DBNull.Value, dt.Rows[2]["B"]);
        Assert.AreEqual("really boring", dt.Rows[2]["C"]);
        Assert.AreEqual(DBNull.Value, dt.Rows[2]["D"]);
    }

    [Test]
    public void NewLineInFile_Ignored()
    {
        var file = CreateTestFile(
            "Name,Dob",
            "Frank,2001-01-01",
            "",
            "Herbert,2002-01-01"
        );

        var dt = RunGetChunk(file);
        Assert.AreEqual(2,dt.Rows.Count);
        Assert.AreEqual("Frank", dt.Rows[0]["Name"]);
        Assert.AreEqual("Herbert", dt.Rows[1]["Name"]);
    }

    [Test]
    public void NewLineInFile_RespectedWhenQuoted()
    {
        var file = CreateTestFile(
            @"Name,Dob,Description
Frank,2001-01-01,""Frank is

the best ever""
Herbert,2002-01-01,Hey"
        );

        var dt = RunGetChunk(file);
        Assert.AreEqual(2,dt.Rows.Count);
        Assert.AreEqual("Frank", dt.Rows[0]["Name"]);
        Assert.AreEqual(@"Frank is

the best ever", dt.Rows[0]["Description"]);
        Assert.AreEqual("Herbert", dt.Rows[1]["Name"]);
        
    }

    [TestCase("")]
    [TestCase("     ")]
    [TestCase("\"  \"")]
    [TestCase("null")]
    [TestCase("NULL ")]
    public void NullCellValues_ToDbNull(string nullstring)
    {
        var file = CreateTestFile(
            "Name,Dob",
            $"{nullstring},2001-01-01",
            "",
            $"Herbert ,{nullstring}"
        );

        var dt = RunGetChunk(file);
        Assert.AreEqual(2, dt.Rows.Count);
        Assert.AreEqual(DBNull.Value, dt.Rows[0]["Name"]);
        Assert.AreEqual("Herbert", dt.Rows[1]["Name"]);
        Assert.AreEqual(DBNull.Value, dt.Rows[1]["Dob"]);
    }


    [TestCase(",,")]
    [TestCase("NULL,NULL,NULL")]
    [TestCase("NULL,,null")]
    [TestCase("NULL , null ,")]
    public void TrailingNulls_InRows(string nullSuffix)
    {
        var file = CreateTestFile(
            "CHI,StudyID,Date",
            "0101010101,5,2001-01-05",
            "0101010101,5,2001-01-05",
            $"0101010101,5,2001-01-05{nullSuffix}", //Row has trailing nulls in it which get ignored
            "0101010101,5,2001-01-05");

        var dt = RunGetChunk(file);
        Assert.AreEqual(4, dt.Rows.Count);
        Assert.AreEqual(3, dt.Columns.Count);
    }
    [Test]
    public void TrailingNulls_InHeader()
    {
        var file = CreateTestFile(
            "CHI ,StudyID,Date,,",
            //Row has trailing null headers, these get ignored
            "0101010101,5,2001-01-05",
            "0101010101,5,2001-01-05",
            "0101010101,5,2001-01-05",
            "0101010101,5,2001-01-05");
            
        var dt = RunGetChunk(file);
        Assert.IsNotNull(dt);
        Assert.AreEqual(4, dt.Rows.Count);
        Assert.AreEqual(3, dt.Columns.Count); //and therefore do not appear in the output table
        Assert.AreEqual("CHI", dt.Columns[0].ColumnName);
        Assert.AreEqual("StudyID", dt.Columns[1].ColumnName);
        Assert.AreEqual("Date", dt.Columns[2].ColumnName);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void NullHeader_InMiddleOfColumns(bool forceHeaders)
    {
        var file = CreateTestFile(
            "CHI ,,StudyID,Date,,",
            //Row has trailing null headers, these get ignored but the one in the middle must be maintained to prevent cell read errors/mismatch

            "0101010101,,5,2001-01-05",
            "0101010101,,5,2001-01-05",
            "0101010101,,5,2001-01-05",
            "0101010101,,5,2001-01-05"); //note that if you put any values in these empty column it is BadData

            
        DataTable dt;
        if (forceHeaders)
            dt = RunGetChunk(file,s=> { s.ForceHeaders = "CHI ,,StudyID,Date,,";
                s.ForceHeadersReplacesFirstLineInFile = true;
            });
        else
            dt = RunGetChunk(file);

        Assert.IsNotNull(dt);
        Assert.AreEqual(4, dt.Rows.Count);
        Assert.AreEqual(3, dt.Columns.Count);
        Assert.AreEqual("CHI", dt.Columns[0].ColumnName);
        Assert.AreEqual("StudyID", dt.Columns[1].ColumnName);
        Assert.AreEqual("Date", dt.Columns[2].ColumnName);
    }
}