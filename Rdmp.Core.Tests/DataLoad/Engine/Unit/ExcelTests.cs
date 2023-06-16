// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.DataLoad.Modules.DataProvider.FlatFileManipulation;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit;

[TestFixture]
[Category("Unit")]
public class ExcelTests
{
    public const string TestFile = "Book1.xlsx";
    public const string FreakyTestFile = "FreakyBook1.xlsx";
    public const string OddFormatsFile = "OddFormats.xls";

    private Dictionary<string, FileInfo> _fileLocations = new Dictionary<string, FileInfo>();
    public static FileInfo TestFileInfo;
    public static FileInfo FreakyTestFileInfo;
    public static FileInfo OddFormatsFileInfo;

    [OneTimeSetUp]
    public void SprayToDisk()
    {
        _fileLocations.Add(TestFile, UsefulStuff.SprayFile(typeof(ExcelTests).Assembly,
            $"{typeof(ExcelTests).Namespace}.TestFile.{TestFile}",TestFile,TestContext.CurrentContext.TestDirectory));
        _fileLocations.Add(FreakyTestFile, UsefulStuff.SprayFile(typeof(ExcelTests).Assembly,
            $"{typeof(ExcelTests).Namespace}.TestFile.{FreakyTestFile}", FreakyTestFile, TestContext.CurrentContext.TestDirectory));

        _fileLocations.Add(OddFormatsFile, UsefulStuff.SprayFile(typeof(ExcelTests).Assembly,
            $"{typeof(ExcelTests).Namespace}.TestFile.{OddFormatsFile}", OddFormatsFile, TestContext.CurrentContext.TestDirectory));
    }


    [Test]
    public void TestFilesExists()
    {
        Assert.IsTrue(_fileLocations[TestFile].Exists);
        Assert.IsTrue(_fileLocations[FreakyTestFile].Exists);
    }

    [Test]
    public void DontTryToOpenWithDelimited_ThrowsInvalidFileExtension()
    {
        var invalid = new DelimitedFlatFileDataFlowSource
        {
            Separator = ","
        };
        invalid.PreInitialize(new FlatFileToLoad(new FileInfo(TestFile)), ThrowImmediatelyDataLoadEventListener.Quiet);
        var ex = Assert.Throws<Exception>(()=>invalid.Check(new ThrowImmediatelyCheckNotifier()));
        StringAssert.Contains("File Book1.xlsx has a prohibited file extension .xlsx",ex.Message);
    }

    [Test]
    [TestCase(TestFile)]
    [TestCase(FreakyTestFile)]
    public void NormalBook_FirstRowCorrect(string versionOfTestFile)
    {
        var source = new ExcelDataFlowSource();

        source.PreInitialize(new FlatFileToLoad(_fileLocations[versionOfTestFile]), ThrowImmediatelyDataLoadEventListener.Quiet);
        var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.AreEqual(6,dt.Columns.Count);
        Assert.AreEqual("Participant", dt.Columns[0].ColumnName);
        Assert.AreEqual("Score", dt.Columns[1].ColumnName);
        Assert.AreEqual("IsEvil", dt.Columns[2].ColumnName);

        Assert.AreEqual("DateField", dt.Columns[3].ColumnName);
        Assert.AreEqual("DoubleField", dt.Columns[4].ColumnName);
        Assert.AreEqual("MixedField", dt.Columns[5].ColumnName);

        Assert.AreEqual("Bob",dt.Rows[0][0]);
        Assert.AreEqual("3", dt.Rows[0][1]);
        Assert.AreEqual("yes", dt.Rows[0][2]);
    }


    [Test]
    [TestCase(TestFile)]
    [TestCase(FreakyTestFile)]
    public void NormalBook_FirstRowCorrect_AddFilenameColumnNamed(string versionOfTestFile)
    {
        var source = new ExcelDataFlowSource
        {
            AddFilenameColumnNamed = "Path"
        };

        source.PreInitialize(new FlatFileToLoad(_fileLocations[versionOfTestFile]), ThrowImmediatelyDataLoadEventListener.Quiet);
        var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.AreEqual(7, dt.Columns.Count);
        Assert.AreEqual("Participant", dt.Columns[0].ColumnName);
        Assert.AreEqual("Score", dt.Columns[1].ColumnName);
        Assert.AreEqual("IsEvil", dt.Columns[2].ColumnName);

        Assert.AreEqual("DateField", dt.Columns[3].ColumnName);
        Assert.AreEqual("DoubleField", dt.Columns[4].ColumnName);
        Assert.AreEqual("MixedField", dt.Columns[5].ColumnName);
        Assert.AreEqual("Path", dt.Columns[6].ColumnName);

        Assert.AreEqual("Bob", dt.Rows[0][0]);
        Assert.AreEqual("3", dt.Rows[0][1]);
        Assert.AreEqual("yes", dt.Rows[0][2]);

        Assert.AreEqual(_fileLocations[versionOfTestFile].FullName, dt.Rows[0][6]);
    }


    [Test]
    [TestCase(TestFile)]
    [TestCase(FreakyTestFile)]
    public void ExcelDateTimeDeciphering(string versionOfTestFile)
    {
        /*
        01/01/2001	0.1	01/01/2001
        01/01/2001 10:30	0.51	01/01/2001 10:30
        01/01/2002 11:30	0.22	0.1
        01/01/2003 01:30	0.10	0.51
        */
        var listener = new ToMemoryDataLoadEventListener(true);

        var source = new ExcelDataFlowSource();

        source.PreInitialize(new FlatFileToLoad(_fileLocations[versionOfTestFile]), listener);
        var dt = source.GetChunk(listener, new GracefulCancellationToken());

        Assert.AreEqual(5,dt.Rows.Count);

        Assert.AreEqual("2001-01-01", dt.Rows[0][3]);
        Assert.AreEqual("0.1", dt.Rows[0][4]);
        Assert.AreEqual("10:30:00", dt.Rows[0][5]);

        Assert.AreEqual("2001-01-01 10:30:00", dt.Rows[1][3]);
        Assert.AreEqual("0.51", dt.Rows[1][4]);
        Assert.AreEqual("11:30:00", dt.Rows[1][5]);

        Assert.AreEqual("2002-01-01 11:30:00", dt.Rows[2][3]);
        Assert.AreEqual("0.22", dt.Rows[2][4]);
        Assert.AreEqual("0.1", dt.Rows[2][5]);

        Assert.AreEqual("2003-01-01 01:30:00", dt.Rows[3][3]);
        Assert.AreEqual("0.10", dt.Rows[3][4]);
        Assert.AreEqual("0.51", dt.Rows[3][5]);

        Assert.AreEqual("2015-09-18", dt.Rows[4][3]);
        Assert.AreEqual("15:09:00", dt.Rows[4][4]);
        Assert.AreEqual("00:03:56", dt.Rows[4][5]);
    }

    [Test]
    public void TestOddFormats()
    {
        var listener = new ToMemoryDataLoadEventListener(true);

        var source = new ExcelDataFlowSource
        {
            WorkSheetName = "MySheet"
        };

        source.PreInitialize(new FlatFileToLoad(_fileLocations[OddFormatsFile]), listener);
        var dt = source.GetChunk(listener, new GracefulCancellationToken());

        Assert.AreEqual(2,dt.Rows.Count);
        Assert.AreEqual(5, dt.Columns.Count);
            
        Assert.AreEqual("Name", dt.Columns[0].ColumnName);
        Assert.AreEqual("Category", dt.Columns[1].ColumnName);
        Assert.AreEqual("Age", dt.Columns[2].ColumnName);
        Assert.AreEqual("Wage", dt.Columns[3].ColumnName);
        Assert.AreEqual("Invisibre", dt.Columns[4].ColumnName); //this column is hidden in the spreadsheet but we still load it

        Assert.AreEqual("Frank", dt.Rows[0][0]);
        Assert.AreEqual("Upper, Left", dt.Rows[0][1]);
        Assert.AreEqual("30", dt.Rows[0][2]);
            
        //its a pound symbol alright! but since there is 2 encodings for pound symbol let's just make everyones life easier
        StringAssert.IsMatch(@"^\W11.00$", dt.Rows[0][3].ToString());
            
        Assert.AreEqual("0.1", dt.Rows[0][4]);

        Assert.AreEqual("Castello", dt.Rows[1][0]);
        Assert.AreEqual("Lower, Back", dt.Rows[1][1]);
        Assert.AreEqual("31", dt.Rows[1][2]);
        Assert.AreEqual("50.00%", dt.Rows[1][3]);
        Assert.AreEqual("0.2", dt.Rows[1][4]);
    }


    [Test]
    public void NormalBook_NoEmptyRowsRead()
    {
        var source = new ExcelDataFlowSource();

        var listener = new ToMemoryDataLoadEventListener(true);

        source.PreInitialize(new FlatFileToLoad(_fileLocations[TestFile]), listener);
        var dt = source.GetChunk(listener, new GracefulCancellationToken());
            
        Assert.AreEqual(5, dt.Rows.Count);
    }

    [Test]
    public void FreakyTestFile_WarningsCorrect()
    {
        var messages = new ToMemoryDataLoadEventListener(true);

        var source = new ExcelDataFlowSource();

        source.PreInitialize(new FlatFileToLoad(_fileLocations[FreakyTestFile]), ThrowImmediatelyDataLoadEventListener.Quiet);
        var dt = source.GetChunk(messages, new GracefulCancellationToken());
            
        var args = messages.EventsReceivedBySender[source];

        Console.Write(messages.ToString());

        Assert.IsTrue(args.Any(a => a.Message.Contains("Discarded the following data (that was found in unnamed columns):RowCount:5") && a.ProgressEventType == ProgressEventType.Warning));
    }

    [Test]
    public void BlankFirstLineFile()
    {
        var source = new ExcelDataFlowSource();

        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,
            "DataLoad","Engine","Resources","BlankLineBook.xlsx"));
        Assert.IsTrue(fi.Exists);

        source.PreInitialize(new FlatFileToLoad(fi), ThrowImmediatelyDataLoadEventListener.Quiet);
            
            
        var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

            
        Assert.AreEqual(3,dt.Rows.Count);
        Assert.AreEqual(2, dt.Columns.Count);
        Assert.AreEqual("Name", dt.Columns[0].ColumnName);
        Assert.AreEqual("Age", dt.Columns[1].ColumnName);
    }


    [Test]
    public void BlankWorkbook()
    {
        var source = new ExcelDataFlowSource();

            
        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,"DataLoad","Engine","Resources","BlankBook.xlsx"));
        Assert.IsTrue(fi.Exists);

        source.PreInitialize(new FlatFileToLoad(fi), ThrowImmediatelyDataLoadEventListener.Quiet);


        var ex = Assert.Throws<FlatFileLoadException>(()=>source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));
        Assert.AreEqual("The Excel sheet 'Sheet1' in workbook 'BlankBook.xlsx' is empty", ex.Message);

    }
    [Test]
    public void Checks_ValidFileExtension_Pass()
    {
        var source = new ExcelDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(new FileInfo("bob.xlsx")),ThrowImmediatelyDataLoadEventListener.Quiet );
        source.Check(new ThrowImmediatelyCheckNotifier {ThrowOnWarning = true});
    }
    [Test]
    public void Checks_ValidFileExtension_InvalidExtensionPass()
    {
        var source = new ExcelDataFlowSource();
        source.PreInitialize(new FlatFileToLoad(new FileInfo("bob.csv")), ThrowImmediatelyDataLoadEventListener.Quiet);
        var ex = Assert.Throws<Exception>(()=>source.Check(new ThrowImmediatelyCheckNotifier { ThrowOnWarning = true }));
        Assert.AreEqual("File extension bob.csv has an invalid extension:.csv (this class only accepts:.xlsx,.xls)",ex.Message);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestToCSVConverter(bool prefixWithWorkbookName)
    {
        var loc = _fileLocations[TestFile];

        var converter = new ExcelToCSVFilesConverter
        {
            ExcelFilePattern = loc.Name,
            PrefixWithWorkbookName = prefixWithWorkbookName
        };

        var mockProjDir = Mock.Of<ILoadDirectory>(p => p.ForLoading==loc.Directory);
          
        var j= new ThrowImmediatelyDataLoadJob
        {
            LoadDirectory = mockProjDir
        };

        converter.Fetch(j, new GracefulCancellationToken());

        var file = prefixWithWorkbookName ?  loc.Directory.GetFiles("Book1_Sheet1.csv").Single(): loc.Directory.GetFiles("Sheet1.csv").Single();

        Assert.IsTrue(file.Exists);
            
        var contents = File.ReadAllText(file.FullName);

        Assert.AreEqual(
            @"Participant,Score,IsEvil,DateField,DoubleField,MixedField
Bob,3,yes,2001-01-01,0.1,10:30:00
Frank,1.1,no,2001-01-01 10:30:00,0.51,11:30:00
Hank,2.1,no,2002-01-01 11:30:00,0.22,0.1
Shanker,2,yes,2003-01-01 01:30:00,0.10,0.51
Bobboy,2,maybe,2015-09-18,15:09:00,00:03:56", contents.Trim(new[] { ',', '\r', '\n', ' ', '\t' }));

        file.Delete();

    }
}