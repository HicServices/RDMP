// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.DataLoad.Modules.Exceptions;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Sources;

public class DelimitedFileSourceTests_ResolvedAccordingToStrategy : DelimitedFileSourceTestsBase
{
    [TestCase(true)]
    [TestCase(false)]
    public void EmptyFile_TotallyEmpty(bool throwOnEmpty)
    {
        var file = CreateTestFile(); //create completely empty file

        if (throwOnEmpty)
        {
            var ex = Assert.Throws<FlatFileLoadException>(() =>
                RunGetChunk(file, BadDataHandlingStrategy.ThrowException, true));
            Assert.That(ex?.Message, Is.EqualTo("File DelimitedFileSourceTests.txt is empty"));
        }
        else
        {
            Assert.That(RunGetChunk(file, BadDataHandlingStrategy.ThrowException, false), Is.Null);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void EmptyFile_AllWhitespace(bool throwOnEmpty)
    {
        var file = CreateTestFile(@" 
     
    ");

        if (throwOnEmpty)
        {
            var ex = Assert.Throws<FlatFileLoadException>(() =>
                RunGetChunk(file, BadDataHandlingStrategy.ThrowException, true));
            Assert.That(ex?.Message, Does.StartWith("File DelimitedFileSourceTests.txt is empty"));
        }
        else
        {
            Assert.That(RunGetChunk(file, BadDataHandlingStrategy.ThrowException, false), Is.Null);
        }
    }


    [TestCase(true)]
    [TestCase(false)]
    public void EmptyFile_HeaderOnly(bool throwOnEmpty)
    {
        var file = CreateTestFile(@"Name,Address

 
     
    ");

        if (throwOnEmpty)
        {
            var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, s => s.ThrowOnEmptyFiles = true));
            Assert.That(ex.Message, Is.EqualTo("File DelimitedFileSourceTests.txt is empty"));
        }
        else
        {
            Assert.That(RunGetChunk(file, s => s.ThrowOnEmptyFiles = false), Is.Null);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void EmptyFile_ForceHeader(bool throwOnEmpty)
    {
        var file = CreateTestFile(@"Name,Address

 
     
    ");

        if (throwOnEmpty)
        {
            var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file,
                s =>
                {
                    s.ThrowOnEmptyFiles = true;
                    s.ForceHeaders = "Name,Address";
                    s.ForceHeadersReplacesFirstLineInFile = true;
                }));
            Assert.That(ex.Message, Is.EqualTo("File DelimitedFileSourceTests.txt is empty"));
        }
        else
        {
            Assert.That(RunGetChunk(file,
                s =>
                {
                    s.ThrowOnEmptyFiles = false;
                    s.ForceHeaders = "Name,Address";
                    s.ForceHeadersReplacesFirstLineInFile = true;
                }), Is.Null);
        }
    }

    [TestCase(BadDataHandlingStrategy.DivertRows)]
    [TestCase(BadDataHandlingStrategy.ThrowException)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows)]
    public void BadCSV_TooManyCellsInRow(BadDataHandlingStrategy strategy)
    {
        var file = CreateTestFile(
            "Name,Description,Age",
            "Frank,Is the greatest,100",
            "Bob,He's also dynamite, seen him do a lot of good work,30",
            "Dennis,Hes ok,35");

        switch (strategy)
        {
            case BadDataHandlingStrategy.ThrowException:
                var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, strategy, true));
                Assert.That(ex.Message, Does.StartWith("Bad data found on line 3"));
                break;
            case BadDataHandlingStrategy.IgnoreRows:
                var dt = RunGetChunk(file, strategy, true);
                Assert.That(dt.Rows, Has.Count.EqualTo(2));
                break;
            case BadDataHandlingStrategy.DivertRows:
                var dt2 = RunGetChunk(file, strategy, true);
                Assert.That(dt2.Rows, Has.Count.EqualTo(2));

                AssertDivertFileIsExactly(
                    $"Bob,He's also dynamite, seen him do a lot of good work,30{Environment.NewLine}");

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy));
        }
    }

    [TestCase(BadDataHandlingStrategy.DivertRows)]
    [TestCase(BadDataHandlingStrategy.ThrowException)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows)]
    public void BadCSV_TooManyCellsInRow_TwoBadRows(BadDataHandlingStrategy strategy)
    {
        var file = CreateTestFile(
            "Name,Description,Age",
            "Frank,Is the greatest,100",
            "Frank,Is the greatest,100,Frank,Is the greatest,100", //input file has 2 lines stuck together, these should appear in divert file exactly as the input file
            "Bob,He's also dynamite, seen him do a lot of good work,30", // has too many cells, should appear
            "Bob2,He's also dynamite2, seen him do a lot of good work2,30", // aso has too many cells, should appear
            "Dennis,Hes ok,35");

        switch (strategy)
        {
            case BadDataHandlingStrategy.ThrowException:
                var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, strategy, true));
                Assert.That(ex.Message, Does.StartWith("Bad data found on line 3"));
                break;
            case BadDataHandlingStrategy.IgnoreRows:
                var dt = RunGetChunk(file, strategy, true);
                Assert.That(dt.Rows, Has.Count.EqualTo(2));
                break;
            case BadDataHandlingStrategy.DivertRows:
                var dt2 = RunGetChunk(file, strategy, true);
                Assert.That(dt2.Rows, Has.Count.EqualTo(2));

                AssertDivertFileIsExactly(
                    $"Frank,Is the greatest,100,Frank,Is the greatest,100{Environment.NewLine}Bob,He's also dynamite, seen him do a lot of good work,30{Environment.NewLine}Bob2,He's also dynamite2, seen him do a lot of good work2,30{Environment.NewLine}");

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy));
        }
    }

    [TestCase(BadDataHandlingStrategy.DivertRows, true)]
    [TestCase(BadDataHandlingStrategy.ThrowException, false)]
    [TestCase(BadDataHandlingStrategy.ThrowException, true)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows, false)]
    public void BadCSV_TooFewCellsInRow(BadDataHandlingStrategy strategy, bool tryToResolve)
    {
        var file = CreateTestFile(
            "Name,Description,Age",
            "Frank,Is the greatest,100",
            "",
            "Other People To Investigate",
            "Dennis,Hes ok,35");

        switch (strategy)
        {
            case BadDataHandlingStrategy.ThrowException:
                var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, Adjust));
                Assert.That(ex.Message, Does.StartWith("Bad data found on line 4"));
                break;
            case BadDataHandlingStrategy.IgnoreRows:
                var dt = RunGetChunk(file, Adjust);
                Assert.That(dt.Rows, Has.Count.EqualTo(2));
                break;
            case BadDataHandlingStrategy.DivertRows:
                var dt2 = RunGetChunk(file, Adjust);
                Assert.That(dt2.Rows, Has.Count.EqualTo(2));

                AssertDivertFileIsExactly($"Other People To Investigate{Environment.NewLine}");

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy));
        }

        return;

        void Adjust(DelimitedFlatFileDataFlowSource a)
        {
            a.BadDataHandlingStrategy = strategy;
            a.AttemptToResolveNewLinesInRecords = tryToResolve;
            a.ThrowOnEmptyFiles = true;
        }
    }

    [TestCase(BadDataHandlingStrategy.DivertRows, true)]
    [TestCase(BadDataHandlingStrategy.ThrowException, false)]
    [TestCase(BadDataHandlingStrategy.ThrowException, true)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows, false)]
    public void BadCSV_TooFewColumnsOnLastLine(BadDataHandlingStrategy strategy, bool tryToResolve)
    {
        var file = CreateTestFile(
            "Name,Description,Age",
            "Frank,Is the greatest,100",
            "Bob");

        switch (strategy)
        {
            case BadDataHandlingStrategy.ThrowException:
                var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, Adjust));
                Assert.That(ex.Message, Does.StartWith("Bad data found on line 3"));
                break;
            case BadDataHandlingStrategy.IgnoreRows:
                var dt = RunGetChunk(file, Adjust);
                Assert.That(dt.Rows, Has.Count.EqualTo(1));
                break;
            case BadDataHandlingStrategy.DivertRows:
                var dt2 = RunGetChunk(file, Adjust);
                Assert.That(dt2.Rows, Has.Count.EqualTo(1));

                AssertDivertFileIsExactly($"Bob{Environment.NewLine}");

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(strategy));
        }

        return;

        void Adjust(DelimitedFlatFileDataFlowSource a)
        {
            a.BadDataHandlingStrategy = strategy;
            a.AttemptToResolveNewLinesInRecords = tryToResolve;
            a.ThrowOnEmptyFiles = true;
        }
    }

    [Test]
    public void BadCSV_FreeTextMiddleColumn()
    {
        //This is recoverable
        var file = CreateTestFile(
            "Name,Description,Age",
            "Frank,Is the greatest,100",
            @"Bob,He's
not too bad
to be honest,20",
            "Dennis,Hes ok,35");

        var dt = RunGetChunk(file, s => { s.AttemptToResolveNewLinesInRecords = true; });
        Assert.That(dt.Rows, Has.Count.EqualTo(3));
        Assert.That(dt.Rows[1][1], Is.EqualTo($"He's{Environment.NewLine}not too bad{Environment.NewLine}to be honest"));
    }

    [Test]
    public void BadCSV_FreeTextFirstColumn()
    {
        var file = CreateTestFile(
            "Description,Name,Age",
            "Is the greatest,Frank,100",
            @"He's
not too bad
to be honest,Bob,20",
            "Hes ok,Dennis,35");

        var ex = Assert.Throws<FlatFileLoadException>(() =>
            RunGetChunk(file, s => { s.AttemptToResolveNewLinesInRecords = true; }));
        Assert.That(ex.Message, Is.EqualTo("Bad data found on line 3"));

        //looks like a good record followed by 2 bad records
        var dt = RunGetChunk(file, s =>
        {
            s.AttemptToResolveNewLinesInRecords = true;
            s.BadDataHandlingStrategy = BadDataHandlingStrategy.IgnoreRows;
        });
        Assert.That(dt.Rows, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows[1][0], Is.EqualTo("to be honest"));
            Assert.That(dt.Rows[1][1], Is.EqualTo("Bob"));
            Assert.That(dt.Rows[1][2], Is.EqualTo(20));
        });
    }

    [Test]
    public void BadCSV_FreeTextLastColumn()
    {
        var file = CreateTestFile(
            "Name,Age,Description",
            "Frank,100,Is the greatest",
            @"Bob,20,He's
not too bad
to be honest",
            "Dennis,35,Hes ok");

        var ex = Assert.Throws<FlatFileLoadException>(() =>
            RunGetChunk(file, s => { s.AttemptToResolveNewLinesInRecords = true; }));
        Assert.That(ex.Message, Is.EqualTo("Bad data found on line 4"));

        //looks like a good record followed by 2 bad records
        var dt = RunGetChunk(file, s =>
        {
            s.AttemptToResolveNewLinesInRecords = true;
            s.BadDataHandlingStrategy = BadDataHandlingStrategy.IgnoreRows;
        });
        Assert.That(dt.Rows, Has.Count.EqualTo(3));
        Assert.That(dt.Rows[1][2], Is.EqualTo("He's"));
    }

    [Test]
    public void BadCSV_ForceHeaders()
    {
        var file = CreateTestFile(
            "Patient's first name, Patients blood glucose, measured in mg",
            "Thomas,100",
            "Frank,300");

        var ex = Assert.Throws<FlatFileLoadException>(() =>
            RunGetChunk(file, s => { s.AttemptToResolveNewLinesInRecords = false; }));
        Assert.That(ex.Message, Is.EqualTo("Bad data found on line 2"));


        var dt = RunGetChunk(file, s =>
        {
            s.AttemptToResolveNewLinesInRecords = false;
            s.ForceHeaders = "Name,BloodGlucose";
            s.ForceHeadersReplacesFirstLineInFile = true;
        });

        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows, Has.Count.EqualTo(2));
            Assert.That(dt.Columns, Has.Count.EqualTo(2));
        });
        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows[0]["Name"], Is.EqualTo("Thomas"));
            Assert.That(dt.Rows[0]["BloodGlucose"], Is.EqualTo(100));
        });
    }

    [Test]
    public void BadCSV_ForceHeaders_NoReplace()
    {
        var file = CreateTestFile(
            "Thomas,100",
            "Frank,300");

        var dt = RunGetChunk(file, s =>
        {
            s.AttemptToResolveNewLinesInRecords = false;
            s.ForceHeaders = "Name,BloodGlucose";
        });

        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows, Has.Count.EqualTo(2));
            Assert.That(dt.Columns, Has.Count.EqualTo(2));
        });
        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows[0]["Name"], Is.EqualTo("Thomas"));
            Assert.That(dt.Rows[0]["BloodGlucose"], Is.EqualTo(100));
        });
    }
}