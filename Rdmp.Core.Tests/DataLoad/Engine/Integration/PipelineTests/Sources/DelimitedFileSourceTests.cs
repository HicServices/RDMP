// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Sources;

[Category("Unit")]
public class DelimitedFileSourceTests
{
    private string filename;

    [SetUp]
    public void SetUp()
    {
        filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "DelimitedFileSourceTests.txt");

        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();

        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine("0101010101,5,2001-01-05");

        File.WriteAllText(filename, sb.ToString());
    }

    [Test]
    public void FileToLoadNotSet_Throws()
    {
        var source = new DelimitedFlatFileDataFlowSource();
        var ex = Assert.Throws<Exception>(() =>
            source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
        Assert.That(ex.Message, Does.Contain("_fileToLoad was not set"));
    }

    [Test]
    public void SeparatorNotSet_Throws()
    {
        var testFile = new FileInfo(filename);
        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        var ex = Assert.Throws<Exception>(() =>
            source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
        Assert.That(ex.Message, Does.Contain("Separator has not been set"));
    }

    [Test]
    public void LoadCSVWithCorrectDatatypes_ForceHeadersWhitespace()
    {
        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.ForceHeaders = "chi  ,Study ID\t ,Date";
        source.ForceHeadersReplacesFirstLineInFile = true;
        source.StronglyTypeInput = true; //makes the source interpret the file types properly

        var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Console.WriteLine(
            $"Resulting columns were:{string.Join(",", chunk.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}");

        Assert.That(chunk.Columns.Contains("chi")); //notice the lack of whitespace!
        Assert.That(
            chunk.Columns
                .Contains("study ID")); //whitespace is allowed in the middle though... because we like a challenge!

        Assert.That(chunk.Columns, Has.Count.EqualTo(3));
        Assert.That(chunk.Rows, Has.Count.EqualTo(1));
        Assert.That(chunk.Rows[0][0], Is.EqualTo("0101010101"));
        Assert.That(chunk.Rows[0][1], Is.EqualTo(5));
        Assert.That(chunk.Rows[0][2], Is.EqualTo(new DateTime(2001, 1, 5))); //notice the strong typing (we are not looking for strings here)

        source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
    }

    [Test]
    public void LoadCSVWithCorrectDatatypes_DatatypesAreCorrect()
    {
        var testFile = new FileInfo(filename);
        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.StronglyTypeInput = true; //makes the source interpret the file types properly

        var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.That(chunk.Columns, Has.Count.EqualTo(3));
        Assert.That(chunk.Rows, Has.Count.EqualTo(1));
        Assert.That(chunk.Rows[0][0], Is.EqualTo("0101010101"));
        Assert.That(chunk.Rows[0][1], Is.EqualTo(5));
        Assert.That(chunk.Rows[0][2], Is.EqualTo(new DateTime(2001, 1, 5))); //notice the strong typing (we are not looking for strings here)

        source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
    }

    [Test]
    public void OverrideDatatypes_ForcedFreakyTypesCorrect()
    {
        var testFile = new FileInfo(filename);
        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.StronglyTypeInput = true; //makes the source interpret the file types properly

        source.ExplicitlyTypedColumns = new ExplicitTypingCollection();
        source.ExplicitlyTypedColumns.ExplicitTypesCSharp.Add("StudyID", typeof(string));

        //preview should be correct
        var preview = source.TryGetPreview();
        Assert.That(preview.Columns["StudyID"].DataType, Is.EqualTo(typeof(string)));
        Assert.That(preview.Rows[0]["StudyID"], Is.EqualTo("5"));

        //as should live run
        var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        Assert.That(chunk.Columns["StudyID"].DataType, Is.EqualTo(typeof(string)));
        Assert.That(chunk.Rows[0]["StudyID"], Is.EqualTo("5"));

        source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
    }

    [Test]
    public void TestIgnoreQuotes()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();

        sb.AppendLine("Number,Field");
        sb.AppendLine("1,\"Sick\" headaches");
        sb.AppendLine("2,2\" length of wood");
        sb.AppendLine("3,\"\"The bends\"\"");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.IgnoreQuotes = true;
        source.MaxBatchSize = 10000;
        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        Assert.That(dt.Rows, Has.Count.EqualTo(3));
        Assert.That(dt.Rows[0][1], Is.EqualTo("\"Sick\" headaches"));
        Assert.That(dt.Rows[1][1], Is.EqualTo("2\" length of wood"));
        Assert.That(dt.Rows[2][1], Is.EqualTo("\"\"The bends\"\""));

        source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
    }


    [TestCase(BadDataHandlingStrategy.DivertRows)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows)]
    [TestCase(BadDataHandlingStrategy.ThrowException)]
    public void BadDataTestExtraColumns(BadDataHandlingStrategy strategy)
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05,fish,watafak");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";

        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = strategy;
        try
        {
            switch (strategy)
            {
                case BadDataHandlingStrategy.ThrowException:
                    var ex = Assert.Throws<FlatFileLoadException>(() =>
                        source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
                    Assert.That(ex.Message, Does.Contain("line 4"));
                    break;
                case BadDataHandlingStrategy.IgnoreRows:
                    var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet,
                        new GracefulCancellationToken());
                    Assert.That(dt, Is.Not.Null);

                    Assert.That(dt.Rows, Has.Count.EqualTo(4));
                    break;
                case BadDataHandlingStrategy.DivertRows:
                    var dt2 = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet,
                        new GracefulCancellationToken());
                    Assert.That(dt2.Rows, Has.Count.EqualTo(4));

                    Assert.That(source.EventHandlers.DivertErrorsFile, Is.Not.Null);

                    Assert.That(File.ReadAllText(source.EventHandlers.DivertErrorsFile.FullName), Is.EqualTo($"0101010101,5,2001-01-05,fish,watafak{Environment.NewLine}"));

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy));
            }
        }
        finally
        {
            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        }
    }

    [Test]
    public void DelimitedFlatFileDataFlowSource_ProperQuoteEscaping()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("CHI,Name,SomeInterestingFacts,Date");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");
        sb.AppendLine(
            "0101010101,Dave,\"Dave is \"\"over\"\" 1000 years old\",2001-01-05"); //https://tools.ietf.org/html/rfc4180 (to properly include quotes in escaped text you need to use "")

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
        source.IgnoreBadReads = false;

        try
        {
            var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
            Assert.That(chunk.Rows, Has.Count.EqualTo(2));
            Assert.That(chunk.Rows[1][2], Is.EqualTo("Dave is \"over\" 1000 years old"));
        }
        finally
        {
            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        }
    }

    /// <summary>
    /// Test checks that IgnoreBadReads lets you load quotes in the middle of free text without having to set IgnoreQuotes to true:
    /// 1. There is a row (2) with quotes in the middle which should get loaded correctly
    /// 2. there's a row (4) with quotes in the middle of the text and the cell itself is quoted.  This loads but drops some quotes.
    /// 
    /// <para>The proper way to express row 4 is by escaping the quote with another quote i.e. "" (See test DelimitedFlatFileDataFlowSource_ProperQuoteEscaping) </para>
    /// </summary>
    [Test]
    public void DelimitedFlatFileDataFlowSource_LoadDataWithQuotesInMiddle_IgnoreBadReads()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("CHI,Name,SomeInterestingFacts,Date");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");
        sb.AppendLine("0101010101,Dave,Dave is \"over\" 1000 years old,2001-01-05");
        sb.AppendLine($"0101010101,Dave,\"Dave is {Environment.NewLine}over 1000 years old\",2001-01-05");
        sb.AppendLine("0101010101,Dave,\"Dave is \"over\" 1000 years old\",2001-01-05");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
        source.IgnoreBadReads = true;

        try
        {
            var chunk = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
            Assert.That(chunk.Rows, Has.Count.EqualTo(5));
            Assert.That(chunk.Rows[1][2], Is.EqualTo("Dave is \"over\" 1000 years old"));
            Assert.That(chunk.Rows[2][2], Is.EqualTo($"Dave is {Environment.NewLine}over 1000 years old"));
            Assert.That(chunk.Rows[3][2], Is.EqualTo("Dave is over\" 1000 years old\"")); //notice this line drops some of the quotes, we just have to live with that
        }
        finally
        {
            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        }
    }


    /// <summary>
    /// Test checks that IgnoreBadReads doesn't cause serious errors (too many cells in row) to be ignored/swallowed
    /// </summary>
    [Test]
    public void DelimitedFlatFileDataFlowSource_TrashFile_IgnoreBadReads()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("CHI,Name,SomeInterestingFacts,Date");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");
        sb.AppendLine("0101010101,Dave,Da,,ve is \"over\" 1000 years old,2001-01-05");
        sb.AppendLine("0101010101\"Dave is \"over\" 1000 years old\",2001-01-05");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
        source.IgnoreBadReads = true;

        try
        {
            var ex = Assert.Throws<FlatFileLoadException>(() =>
                source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
            Assert.That(ex.Message, Is.EqualTo("Bad data found on line 3"));
        }
        finally
        {
            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        }
    }

    [Test]
    public void DelimitedFlatFileDataFlowSource_LoadDataWithQuotesInMiddle_WithMultiLineRecords()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("CHI,Name,SomeInterestingFacts,Date");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");
        sb.AppendLine("0101010101,Dave,Dave is \"over\" 1000 years old,2001-01-05");
        sb.AppendLine(@"0101010101,Dave,""Dave is
""over"" 1000 years 

old"",2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.MaxBatchSize = 10000;
        source.AttemptToResolveNewLinesInRecords = true;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
        try
        {
            var ex = Assert.Throws<FlatFileLoadException>(() =>
                source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
            Assert.That(ex.Message, Is.EqualTo("Bad data found on line 3"));
        }
        finally
        {
            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        }
    }


    [TestCase(BadDataHandlingStrategy.DivertRows)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows)]
    [TestCase(BadDataHandlingStrategy.ThrowException)]
    public void BadDataTestExtraColumns_ErrorIsOnLastLine(BadDataHandlingStrategy strategy)
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();

        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05,fish,watafak");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";

        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = strategy;
        try
        {
            switch (strategy)
            {
                case BadDataHandlingStrategy.ThrowException:
                    var ex = Assert.Throws<FlatFileLoadException>(() =>
                        source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
                    Assert.That(ex.Message, Does.Contain("line 6"));
                    break;
                case BadDataHandlingStrategy.IgnoreRows:
                    var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet,
                        new GracefulCancellationToken());
                    Assert.That(dt, Is.Not.Null);

                    Assert.That(dt.Rows, Has.Count.EqualTo(4));
                    break;
                case BadDataHandlingStrategy.DivertRows:
                    var dt2 = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet,
                        new GracefulCancellationToken());
                    Assert.That(dt2.Rows, Has.Count.EqualTo(4));

                    Assert.That(source.EventHandlers.DivertErrorsFile, Is.Not.Null);

                    Assert.That(File.ReadAllText(source.EventHandlers.DivertErrorsFile.FullName), Is.EqualTo($"0101010101,5,2001-01-05,fish,watafak{Environment.NewLine}"));

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy));
            }
        }
        finally
        {
            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        }
    }

    [Test]
    public void NewLinesInConstantString_EscapedCorrectly()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine(@"0101010101,""5
    The first"",2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";

        source.MaxBatchSize = 10000;
        source.StronglyTypeInput = true; //makes the source interpret the file types properly

        try
        {
            var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
            Assert.That(dt, Is.Not.Null);
            Assert.That(dt.Rows, Has.Count.EqualTo(5));
            Assert.That(dt.Rows[0][1], Is.EqualTo(@"5
    The first"));
        }
        finally
        {
            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        }
    }

    [TestCase(BadDataHandlingStrategy.ThrowException)]
    [TestCase(BadDataHandlingStrategy.DivertRows)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows)]
    public void NewLinesInConstantString_NotEscaped(BadDataHandlingStrategy strategy)
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine(@"0101010101,5
    The first,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";

        source.MaxBatchSize = 10000;
        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = strategy;
        try
        {
            switch (strategy)
            {
                case BadDataHandlingStrategy.ThrowException:
                    var ex = Assert.Throws<FlatFileLoadException>(() =>
                        source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
                    Assert.That(ex.Message, Does.Contain("line 2"));
                    break;
                case BadDataHandlingStrategy.IgnoreRows:
                    var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet,
                        new GracefulCancellationToken());
                    Assert.That(dt, Is.Not.Null);

                    Assert.That(dt.Rows, Has.Count.EqualTo(4));
                    break;
                case BadDataHandlingStrategy.DivertRows:
                    var dt2 = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet,
                        new GracefulCancellationToken());
                    Assert.That(dt2.Rows, Has.Count.EqualTo(4));

                    Assert.That(source.EventHandlers.DivertErrorsFile, Is.Not.Null);

                    Assert.That(File.ReadAllText(source.EventHandlers.DivertErrorsFile.FullName), Is.EqualTo(@"0101010101,5
    The first,2001-01-05
"));

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy));
            }
        }
        finally
        {
            source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        }
    }


    [Test]
    public void OverrideHeadersAndTab()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("0101010101\t5\t2001-01-05");
        sb.AppendLine("0101010101\t5\t2001-01-05");
        File.WriteAllText(filename, sb.ToString());


        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator =
            "\\t"; //<-- Important this is the string value SLASH T not an actual escaped tab as C# understands it.  This reflects the user pressing slash and t on his keyboard for the Separator argument in the UI
        source.ForceHeaders = "CHI\tStudyID\tDate";
        source.MaxBatchSize = 10000;

        var dt = source.GetChunk(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

        Assert.That(dt, Is.Not.Null);

        Assert.That(dt.Columns, Has.Count.EqualTo(3));

        Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("CHI"));
        Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("StudyID"));
        Assert.That(dt.Columns[2].ColumnName, Is.EqualTo("Date"));

        Assert.That(dt.Rows, Has.Count.EqualTo(2));

        source.Dispose(new ThrowImmediatelyDataLoadJob(), null);

        File.Delete(filename);
    }

    [Test]
    public void Test_IgnoreColumns()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        var sb = new StringBuilder();
        sb.AppendLine("0101010101\t5\t2001-01-05\tomg\t");
        sb.AppendLine("0101010101\t5\t2001-01-05\tomg2\t");
        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator =
            "\\t"; //<-- Important this is the string value SLASH T not an actual escaped tab as C# understands it.  This reflects the user pressing slash and t on his keyboard for the Separator argument in the UI
        source.ForceHeaders = "CHI\tStudyID\tDate\tSomeText";
        source.MaxBatchSize = 10000;
        source.IgnoreColumns = "StudyID\tDate\t";

        var dt = source.GetChunk(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

        Assert.That(dt, Is.Not.Null);

        //should only be one column (chi since we ignore study and date)
        Assert.That(dt.Columns, Has.Count.EqualTo(2));
        Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("CHI"));
        Assert.That(dt.Columns[1].ColumnName, Is.EqualTo("SomeText"));

        Assert.That(dt.Rows, Has.Count.EqualTo(2));

        source.Dispose(new ThrowImmediatelyDataLoadJob(), null);

        File.Delete(filename);
    }

    [TestCase("Fish In Barrel", "FishInBarrel")]
    [TestCase("32 Fish In Barrel",
        "_32FishInBarrel")] //Column names can't start with numbers so underscore prefix applies
    [TestCase("once upon a time",
        "onceUponATime")] //where spaces are removed cammel case the next symbol if it's a character
    [TestCase("once _  upon a time",
        "once_UponATime")] //where spaces are removed cammel case the next symbol if it's a character
    [TestCase("once#upon a", "onceuponA")]
    [TestCase("once #upon",
        "onceUpon")] //Dodgy characters are stripped before cammel casing after spaces so 'u' gets cammeled even though it has a symbol before it.
    public void TestMakingHeaderNamesSane(string bad, string expectedGood)
    {
        Assert.That(QuerySyntaxHelper.MakeHeaderNameSensible(bad), Is.EqualTo(expectedGood));
    }


    [Test]
    public void Test_ScientificNotation_StronglyTyped()
    {
        var f = Path.Combine(TestContext.CurrentContext.WorkDirectory, "meee.csv");

        var sb = new StringBuilder();

        sb.AppendLine("test");

        //1 scientific notation on first row (test is the header)
        sb.AppendLine("-4.10235746055587E-05");

        //500 lines of random stuff to force 2 batches
        for (var i = 0; i < DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize; i++)
            sb.AppendLine("5");

        //a scientific notation in batch 2
        sb.AppendLine("-4.10235746055587E-05");

        File.WriteAllText(f, sb.ToString());

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(new FileInfo(f)), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.MaxBatchSize = DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize;
        source.StronglyTypeInputBatchSize = DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize;
        source.StronglyTypeInput = true;

        var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        Assert.That(dt.Columns.Cast<DataColumn>().Single().DataType, Is.EqualTo(typeof(decimal)));
        Assert.That(dt.Rows, Has.Count.EqualTo(DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize));

        dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        Assert.That(dt.Columns.Cast<DataColumn>().Single().DataType, Is.EqualTo(typeof(decimal)));
        Assert.That(dt.Rows, Has.Count.EqualTo(2));


        dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        Assert.That(dt, Is.Null);
    }

    /// <summary>
    /// Depicts a case where quotes appear at the start of a string field
    /// </summary>
    [Test]
    public void Test_IgnoreQuotes()
    {
        var f = Path.Combine(TestContext.CurrentContext.WorkDirectory, "talk.csv");

        File.WriteAllText(f, @"Field1,Field2
1,Watch out guys its Billie ""The Killer"" Cole
2,""The Killer""? I've heard of him hes a bad un");

        var source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(new FileInfo(f)), ThrowImmediatelyDataLoadEventListener.Quiet);
        source.Separator = ",";
        source.MaxBatchSize = DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize;
        source.StronglyTypeInputBatchSize = DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize;
        source.StronglyTypeInput = true;

        var toMem = new ToMemoryDataLoadEventListener(true);
        var ex = Assert.Throws<FlatFileLoadException>(() => source.GetChunk(toMem, new GracefulCancellationToken()));
        Assert.That(ex.Message, Is.EqualTo("Bad data found on line 2"));
        source.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
    }
}