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

        if(File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine("0101010101,5,2001-01-05");

        File.WriteAllText(filename, sb.ToString());
    }

    [Test]
    public void FileToLoadNotSet_Throws()
    {
        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        var ex = Assert.Throws<Exception>(()=>source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
        StringAssert.Contains("_fileToLoad was not set",ex.Message);
    }
    [Test]
    public void SeparatorNotSet_Throws()
    {
        var testFile = new FileInfo(filename);
        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile),new ThrowImmediatelyDataLoadEventListener() );
        var ex = Assert.Throws<Exception>(()=>source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
        StringAssert.Contains("Separator has not been set", ex.Message);
    }
    [Test]
    public void LoadCSVWithCorrectDatatypes_ForceHeadersWhitespace()
    {
        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.ForceHeaders = "chi  ,Study ID\t ,Date";
        source.ForceHeadersReplacesFirstLineInFile = true;
        source.StronglyTypeInput = true;//makes the source interpret the file types properly

        var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

        Console.WriteLine("Resulting columns were:" + string.Join("," , chunk.Columns.Cast<DataColumn>().Select(c=>c.ColumnName)));

        Assert.IsTrue(chunk.Columns.Contains("chi")); //notice the lack of whitespace!
        Assert.IsTrue(chunk.Columns.Contains("study ID")); //whitespace is allowed in the middle though... because we like a challenge!

        Assert.AreEqual(3,chunk.Columns.Count);
        Assert.AreEqual(1, chunk.Rows.Count);
        Assert.AreEqual("0101010101", chunk.Rows[0][0]);
        Assert.AreEqual(5, chunk.Rows[0][1]);
        Assert.AreEqual(new DateTime(2001 , 1 , 5), chunk.Rows[0][2]);//notice the strong typing (we are not looking for strings here)
            
        source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
    }

    [Test]
    public void LoadCSVWithCorrectDatatypes_DatatypesAreCorrect()
    {

        var testFile = new FileInfo(filename);
        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.StronglyTypeInput = true;//makes the source interpret the file types properly

        var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

        Assert.AreEqual(3, chunk.Columns.Count);
        Assert.AreEqual(1, chunk.Rows.Count);
        Assert.AreEqual("0101010101", chunk.Rows[0][0]);
        Assert.AreEqual(5, chunk.Rows[0][1]);
        Assert.AreEqual(new DateTime(2001, 1, 5), chunk.Rows[0][2]);//notice the strong typing (we are not looking for strings here)

        source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
    }
    [Test]
    public void OverrideDatatypes_ForcedFreakyTypesCorrect()
    {

        var testFile = new FileInfo(filename);
        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.StronglyTypeInput = true;//makes the source interpret the file types properly
            
        source.ExplicitlyTypedColumns = new ExplicitTypingCollection();
        source.ExplicitlyTypedColumns.ExplicitTypesCSharp.Add("StudyID",typeof(string));

        //preview should be correct
        DataTable preview = source.TryGetPreview();
        Assert.AreEqual(typeof(string), preview.Columns["StudyID"].DataType);
        Assert.AreEqual("5", preview.Rows[0]["StudyID"]);

        //as should live run
        var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        Assert.AreEqual(typeof(string), chunk.Columns["StudyID"].DataType);
        Assert.AreEqual("5", chunk.Rows[0]["StudyID"]);

        source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
    }

    [Test]
    public void TestIgnoreQuotes()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Number,Field");
        sb.AppendLine("1,\"Sick\" headaches");
        sb.AppendLine("2,2\" length of wood");
        sb.AppendLine("3,\"\"The bends\"\"");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.IgnoreQuotes = true;
        source.MaxBatchSize = 10000;
        source.StronglyTypeInput = true;//makes the source interpret the file types properly
        var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        Assert.AreEqual(3, dt.Rows.Count);
        Assert.AreEqual("\"Sick\" headaches", dt.Rows[0][1]);
        Assert.AreEqual("2\" length of wood", dt.Rows[1][1]);
        Assert.AreEqual("\"\"The bends\"\"", dt.Rows[2][1]);

        source.Dispose(new ThrowImmediatelyDataLoadEventListener(),null);
    }


    [TestCase(BadDataHandlingStrategy.DivertRows)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows)]
    [TestCase(BadDataHandlingStrategy.ThrowException)]
    public void BadDataTestExtraColumns(BadDataHandlingStrategy strategy)
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05,fish,watafak");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";

        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true;//makes the source interpret the file types properly
        source.BadDataHandlingStrategy = strategy;
        try
        {
            switch (strategy)
            {
                case BadDataHandlingStrategy.ThrowException:
                    var ex = Assert.Throws<FlatFileLoadException>(() => source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
                    StringAssert.Contains("line 4",ex.Message);
                    break;
                case BadDataHandlingStrategy.IgnoreRows:
                    var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());
                    Assert.IsNotNull(dt);

                    Assert.AreEqual(4,dt.Rows.Count);
                    break;
                case BadDataHandlingStrategy.DivertRows:
                    var dt2 = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
                    Assert.AreEqual(4, dt2.Rows.Count);

                    Assert.IsNotNull(source.EventHandlers.DivertErrorsFile);

                    Assert.AreEqual("0101010101,5,2001-01-05,fish,watafak"+Environment.NewLine, File.ReadAllText(source.EventHandlers.DivertErrorsFile.FullName));

                    break;
                default:
                    throw new ArgumentOutOfRangeException("strategy");
            }
        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);    
        }
            
    }

    [Test]
    public void DelimitedFlatFileDataFlowSource_ProperQuoteEscaping()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CHI,Name,SomeInterestingFacts,Date");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");
        sb.AppendLine("0101010101,Dave,\"Dave is \"\"over\"\" 1000 years old\",2001-01-05"); //https://tools.ietf.org/html/rfc4180 (to properly include quotes in escaped text you need to use "")

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
        source.IgnoreBadReads = false;

        try
        {
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            Assert.AreEqual(2, chunk.Rows.Count);
            Assert.AreEqual("Dave is \"over\" 1000 years old", chunk.Rows[1][2]);
        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
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

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CHI,Name,SomeInterestingFacts,Date");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");
        sb.AppendLine("0101010101,Dave,Dave is \"over\" 1000 years old,2001-01-05");
        sb.AppendLine($"0101010101,Dave,\"Dave is {Environment.NewLine}over 1000 years old\",2001-01-05");
        sb.AppendLine("0101010101,Dave,\"Dave is \"over\" 1000 years old\",2001-01-05");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
        source.IgnoreBadReads = true;

        try
        {
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            Assert.AreEqual(5,chunk.Rows.Count);
            Assert.AreEqual("Dave is \"over\" 1000 years old", chunk.Rows[1][2]);
            Assert.AreEqual($"Dave is {Environment.NewLine}over 1000 years old", chunk.Rows[2][2]);
            Assert.AreEqual("Dave is over\" 1000 years old\"", chunk.Rows[3][2]); //notice this line drops some of the quotes, we just have to live with that
        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
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

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CHI,Name,SomeInterestingFacts,Date");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");
        sb.AppendLine("0101010101,Dave,Da,,ve is \"over\" 1000 years old,2001-01-05");
        sb.AppendLine("0101010101\"Dave is \"over\" 1000 years old\",2001-01-05");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
        source.IgnoreBadReads = true;

        try
        {
            var ex = Assert.Throws<FlatFileLoadException>(()=>source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
            Assert.AreEqual("Bad data found on line 3", ex.Message);

        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }
    }

    [Test]
    public void DelimitedFlatFileDataFlowSource_LoadDataWithQuotesInMiddle_WithMultiLineRecords()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CHI,Name,SomeInterestingFacts,Date");
        sb.AppendLine("0101010101,Dave,Dave is over 1000 years old,2001-01-05");
        sb.AppendLine("0101010101,Dave,Dave is \"over\" 1000 years old,2001-01-05");
        sb.AppendLine(@"0101010101,Dave,""Dave is
""over"" 1000 years 

old"",2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.MaxBatchSize = 10000;
        source.AttemptToResolveNewLinesInRecords = true;

        source.StronglyTypeInput = true; //makes the source interpret the file types properly
        source.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
        try
        {
            var ex = Assert.Throws<FlatFileLoadException>(() => source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
            Assert.AreEqual("Bad data found on line 3", ex.Message);
        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }
    }


    [TestCase(BadDataHandlingStrategy.DivertRows)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows)]
    [TestCase(BadDataHandlingStrategy.ThrowException)]
    public void BadDataTestExtraColumns_ErrorIsOnLastLine(BadDataHandlingStrategy strategy)
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05,fish,watafak");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";

        source.MaxBatchSize = 10000;

        source.StronglyTypeInput = true;//makes the source interpret the file types properly
        source.BadDataHandlingStrategy = strategy;
        try
        {
            switch (strategy)
            {
                case BadDataHandlingStrategy.ThrowException:
                    var ex = Assert.Throws<FlatFileLoadException>(() => source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
                    StringAssert.Contains("line 6", ex.Message);
                    break;
                case BadDataHandlingStrategy.IgnoreRows:
                    var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
                    Assert.IsNotNull(dt);

                    Assert.AreEqual(4, dt.Rows.Count);
                    break;
                case BadDataHandlingStrategy.DivertRows:
                    var dt2 = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
                    Assert.AreEqual(4, dt2.Rows.Count);

                    Assert.IsNotNull(source.EventHandlers.DivertErrorsFile);

                    Assert.AreEqual("0101010101,5,2001-01-05,fish,watafak" + Environment.NewLine, File.ReadAllText(source.EventHandlers.DivertErrorsFile.FullName));

                    break;
                default:
                    throw new ArgumentOutOfRangeException("strategy");
            }
        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }   
    }

    [Test]
    public void NewLinesInConstantString_EscapedCorrectly()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine(@"0101010101,""5
    The first"",2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";

        source.MaxBatchSize = 10000;
        source.StronglyTypeInput = true;//makes the source interpret the file types properly
            
        try
        {
            var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            Assert.IsNotNull(dt);
            Assert.AreEqual(5, dt.Rows.Count);
            Assert.AreEqual(@"5
    The first",dt.Rows[0][1]);
                      
        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }
    }

    [TestCase(BadDataHandlingStrategy.ThrowException)]
    [TestCase(BadDataHandlingStrategy.DivertRows)]
    [TestCase(BadDataHandlingStrategy.IgnoreRows)]
    public void NewLinesInConstantString_NotEscaped(BadDataHandlingStrategy strategy)
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("CHI,StudyID,Date");
        sb.AppendLine(@"0101010101,5
    The first,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");
        sb.AppendLine("0101010101,5,2001-01-05");

        File.WriteAllText(filename, sb.ToString());

        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";

        source.MaxBatchSize = 10000;
        source.StronglyTypeInput = true;//makes the source interpret the file types properly
        source.BadDataHandlingStrategy = strategy;
        try
        {
            switch (strategy)
            {
                case BadDataHandlingStrategy.ThrowException:
                    var ex = Assert.Throws<FlatFileLoadException>(() => source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
                    StringAssert.Contains("line 2", ex.Message);
                    break;
                case BadDataHandlingStrategy.IgnoreRows:
                    var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
                    Assert.IsNotNull(dt);

                    Assert.AreEqual(4, dt.Rows.Count);
                    break;
                case BadDataHandlingStrategy.DivertRows:
                    var dt2 = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
                    Assert.AreEqual(4, dt2.Rows.Count);

                    Assert.IsNotNull(source.EventHandlers.DivertErrorsFile);

                    Assert.AreEqual(@"0101010101,5
    The first,2001-01-05
", File.ReadAllText(source.EventHandlers.DivertErrorsFile.FullName));

                    break;
                default:
                    throw new ArgumentOutOfRangeException("strategy");
            }
        }
        finally
        {
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }   
    }
        

    [Test]
    public void OverrideHeadersAndTab()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("0101010101\t5\t2001-01-05");
        sb.AppendLine("0101010101\t5\t2001-01-05");
        File.WriteAllText(filename,sb.ToString());


        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = "\\t"; //<-- Important this is the string value SLASH T not an actual escaped tab as C# understands it.  This reflects the user pressing slash and t on his keyboard for the Separator argument in the UI
        source.ForceHeaders = "CHI\tStudyID\tDate";
        source.MaxBatchSize = 10000;

        var dt = source.GetChunk(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

        Assert.NotNull(dt);

        Assert.AreEqual(3,dt.Columns.Count);

        Assert.AreEqual("CHI", dt.Columns[0].ColumnName);
        Assert.AreEqual("StudyID", dt.Columns[1].ColumnName);
        Assert.AreEqual("Date", dt.Columns[2].ColumnName);

        Assert.AreEqual(2,dt.Rows.Count);

        source.Dispose(new ThrowImmediatelyDataLoadJob(), null);

        File.Delete(filename);
    }

    [Test]
    public void Test_IgnoreColumns()
    {
        if (File.Exists(filename))
            File.Delete(filename);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("0101010101\t5\t2001-01-05\tomg\t");
        sb.AppendLine("0101010101\t5\t2001-01-05\tomg2\t");
        File.WriteAllText(filename, sb.ToString());
            
        var testFile = new FileInfo(filename);

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = "\\t"; //<-- Important this is the string value SLASH T not an actual escaped tab as C# understands it.  This reflects the user pressing slash and t on his keyboard for the Separator argument in the UI
        source.ForceHeaders = "CHI\tStudyID\tDate\tSomeText";
        source.MaxBatchSize = 10000;
        source.IgnoreColumns = "StudyID\tDate\t";

        var dt = source.GetChunk(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

        Assert.NotNull(dt);

        //should only be one column (chi since we ignore study and date)
        Assert.AreEqual(2, dt.Columns.Count);
        Assert.AreEqual("CHI", dt.Columns[0].ColumnName);
        Assert.AreEqual("SomeText", dt.Columns[1].ColumnName);

        Assert.AreEqual(2, dt.Rows.Count);

        source.Dispose(new ThrowImmediatelyDataLoadJob(), null);

        File.Delete(filename);
            
    }

    [TestCase("Fish In Barrel", "FishInBarrel")]
    [TestCase("32 Fish In Barrel","_32FishInBarrel")]//Column names can't start with numbers so underscore prefix applies
    [TestCase("once upon a time","onceUponATime")]//where spaces are removed cammel case the next symbol if it's a character
    [TestCase("once _  upon a time", "once_UponATime")]//where spaces are removed cammel case the next symbol if it's a character
    [TestCase("once#upon a", "onceuponA")]
    [TestCase("once #upon", "onceUpon")] //Dodgy characters are stripped before cammel casing after spaces so 'u' gets cammeled even though it has a symbol before it.
    public void TestMakingHeaderNamesSane(string bad, string expectedGood)
    {
        Assert.AreEqual(expectedGood,QuerySyntaxHelper.MakeHeaderNameSensible(bad));
    }


    [Test]
    public void Test_ScientificNotation_StronglyTyped()
    {
        var f = Path.Combine(TestContext.CurrentContext.WorkDirectory,"meee.csv");
            
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("test");

        //1 scientific notation on first row (test is the header)
        sb.AppendLine("-4.10235746055587E-05");

        //500 lines of random stuff to force 2 batches
        for (int i=0;i< DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize; i++)
            sb.AppendLine("5");

        //a scientific notation in batch 2
        sb.AppendLine("-4.10235746055587E-05");

        File.WriteAllText(f,sb.ToString());

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(new FileInfo(f)), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.MaxBatchSize = DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize;
        source.StronglyTypeInputBatchSize = DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize;
        source.StronglyTypeInput = true;

        var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());
        Assert.AreEqual(typeof(Decimal), dt.Columns.Cast<DataColumn>().Single().DataType);
        Assert.AreEqual(DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize, dt.Rows.Count);
            
        dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        Assert.AreEqual(typeof(Decimal), dt.Columns.Cast<DataColumn>().Single().DataType);
        Assert.AreEqual(2, dt.Rows.Count);
            

        dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        Assert.IsNull(dt);
    }

    /// <summary>
    /// Depicts a case where quotes appear at the start of a string field
    /// </summary>
    [Test]
    public void Test_IgnoreQuotes()
    {
        var f = Path.Combine(TestContext.CurrentContext.WorkDirectory,"talk.csv");
            
        File.WriteAllText(f,@"Field1,Field2
1,Watch out guys its Billie ""The Killer"" Cole
2,""The Killer""? I've heard of him hes a bad un");

        DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(new FileInfo(f)), new ThrowImmediatelyDataLoadEventListener());
        source.Separator = ",";
        source.MaxBatchSize = DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize;
        source.StronglyTypeInputBatchSize = DelimitedFlatFileDataFlowSource.MinimumStronglyTypeInputBatchSize;
        source.StronglyTypeInput = true;

        var toMem = new ToMemoryDataLoadEventListener(true);
        var ex = Assert.Throws<FlatFileLoadException>(()=>source.GetChunk(toMem,new GracefulCancellationToken()));
        Assert.AreEqual("Bad data found on line 2", ex.Message);
        source.Dispose(new ThrowImmediatelyDataLoadEventListener(),null);
    }
}