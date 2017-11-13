using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Reports;
using DataLoadEngine.Job;
using DataLoadEngineTests.Integration;
using LoadModules.Generic.Attachers;
using LoadModules.Generic.DataFlowSources;
using LoadModules.Generic.DataProvider.FlatFileManipulation;
using LoadModules.Generic.Exceptions;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;

namespace DataLoadEngineTests.Unit
{
    public class ExcelTests
    {
        public const string TestFile = "Book1.xlsx";
        public const string FreakyTestFile = "FreakyBook1.xlsx";

        private bool officeInstalled = false;
        private Dictionary<string, FileInfo> _fileLocations = new Dictionary<string, FileInfo>();
        public static FileInfo TestFileInfo;
        public static FileInfo FreakyTestFileInfo;

        [TestFixtureSetUp]
        public void SprayToDisk()
        {
            _fileLocations.Add(TestFile, UsefulStuff.SprayFile(typeof(ExcelTests).Assembly,typeof(ExcelTests).Namespace + ".TestFile." + TestFile,TestFile));
            _fileLocations.Add(FreakyTestFile, UsefulStuff.SprayFile(typeof(ExcelTests).Assembly, typeof(ExcelTests).Namespace + ".TestFile." + FreakyTestFile, FreakyTestFile));
        }


        [Test]
        public void TestFilesExists()
        {
            if (!officeInstalled)
                Assert.Inconclusive();

            Assert.IsTrue(_fileLocations[TestFile].Exists);
            Assert.IsTrue(_fileLocations[FreakyTestFile].Exists);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "File Book1.xlsx has a prohibitted file extension .xlsx",MatchType = MessageMatch.Contains)]
        public void DontTryToOpenWithDelimited_ThrowsInvalidFileExtension()
        {
            DelimitedFlatFileDataFlowSource invalid = new DelimitedFlatFileDataFlowSource();
            invalid.Separator = ",";
            invalid.PreInitialize(new FlatFileToLoad(new FileInfo(TestFile)), new ThrowImmediatelyDataLoadEventListener());
            invalid.Check(new ThrowImmediatelyCheckNotifier());
        }

        [Test]
        [TestCase(TestFile)]
        [TestCase(FreakyTestFile)]
        public void NormalBook_FirstRowCorrect(string versionOfTestFile)
        {
            if (!officeInstalled)
                Assert.Inconclusive();

            ExcelDataFlowSource source = new ExcelDataFlowSource();

            source.PreInitialize(new FlatFileToLoad(_fileLocations[versionOfTestFile]), new ThrowImmediatelyDataLoadEventListener());
            DataTable dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

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
        public void ExcelDateTimeDeciphering(string versionOfTestFile)
        {
            /*
            01/01/2001	0.1	01/01/2001
            01/01/2001 10:30	0.51	01/01/2001 10:30
            01/01/2002 11:30	0.22	0.1
            01/01/2003 01:30	0.10	0.51
            */
            if (!officeInstalled)
                Assert.Inconclusive();

            var listener = new ToMemoryDataLoadEventListener(true);

            ExcelDataFlowSource source = new ExcelDataFlowSource();

            source.PreInitialize(new FlatFileToLoad(_fileLocations[versionOfTestFile]), listener);
            DataTable dt = source.GetChunk(listener, new GracefulCancellationToken());

            Assert.AreEqual("01/01/2001 00:00:00", dt.Rows[0][3]);
            Assert.AreEqual("0.1", dt.Rows[0][4]);
            Assert.AreEqual("10:30:00", dt.Rows[0][5]);

            Assert.AreEqual("01/01/2001 10:30:00", dt.Rows[1][3]);
            Assert.AreEqual("0.51", dt.Rows[1][4]);
            Assert.AreEqual("11:30:00", dt.Rows[1][5]);

            Assert.AreEqual("01/01/2002 11:30:00", dt.Rows[2][3]);
            Assert.AreEqual("0.223", dt.Rows[2][4]);
            Assert.AreEqual("0.1", dt.Rows[2][5]);

            Assert.AreEqual("01/01/2003 01:30:00", dt.Rows[3][3]);
            Assert.AreEqual("0.1", dt.Rows[3][4]);
            Assert.AreEqual("0.51", dt.Rows[3][5]);

            Assert.AreEqual("18/09/2015 00:00:00", dt.Rows[4][3]);
            Assert.AreEqual("15:09", dt.Rows[4][4]);
            Assert.AreEqual("00:03:56", dt.Rows[4][5]);
        }

        [Test]
        public void NormalBook_NoEmptyRowsRead()
        {
            if (!officeInstalled)
                Assert.Inconclusive();

            ExcelDataFlowSource source = new ExcelDataFlowSource();

            var listener = new ToMemoryDataLoadEventListener(true);

            source.PreInitialize(new FlatFileToLoad(_fileLocations[TestFile]), listener);
            DataTable dt = source.GetChunk(listener, new GracefulCancellationToken());
            
            Assert.AreEqual(5, dt.Rows.Count);
        }

        [Test]
        public void FreakyTestFile_WarningsCorrect()
        {
            if (!officeInstalled)
                Assert.Inconclusive();

            var messages = new ToMemoryDataLoadEventListener(true);

            ExcelDataFlowSource source = new ExcelDataFlowSource();

            source.PreInitialize(new FlatFileToLoad(_fileLocations[FreakyTestFile]), new ThrowImmediatelyDataLoadEventListener());
            DataTable dt = source.GetChunk(messages, new GracefulCancellationToken());
            
            var args = messages.EventsReceivedBySender[source];

            Console.Write(messages.ToString());


            Assert.IsTrue(args.Any(a => a.Message.Contains("Column 1 did not have a header and so was not loaded") && a.ProgressEventType == ProgressEventType.Warning));
            Assert.IsTrue(args.Any(a => a.Message.Contains("Column 8 did not have a header and so was not loaded") && a.ProgressEventType == ProgressEventType.Warning));

            Assert.IsTrue(args.Any(a => a.Message.Contains("Discarded the following data (that was found in unamed columns):RowCount:5") && a.ProgressEventType == ProgressEventType.Warning));
        }

        [Test]
        public void BlankFirstLineFile()
        {
            if (!officeInstalled)
                Assert.Inconclusive();

            ExcelDataFlowSource source = new ExcelDataFlowSource();

            var fi = new FileInfo(@".\Resources\BlankLineBook.xlsx");
            Assert.IsTrue(fi.Exists);

            source.PreInitialize(new FlatFileToLoad(fi), new ThrowImmediatelyDataLoadEventListener());
            
            
            DataTable dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            
            Assert.AreEqual(3,dt.Rows.Count);
            Assert.AreEqual(2, dt.Columns.Count);
            Assert.AreEqual("Name", dt.Columns[0].ColumnName);
            Assert.AreEqual("Age", dt.Columns[1].ColumnName);
        }


        [Test]
        public void BlankWorkbook()
        {
            if (!officeInstalled)
                Assert.Inconclusive();

            ExcelDataFlowSource source = new ExcelDataFlowSource();

            var fi = new FileInfo(@".\Resources\BlankBook.xlsx");
            Assert.IsTrue(fi.Exists);

            source.PreInitialize(new FlatFileToLoad(fi), new ThrowImmediatelyDataLoadEventListener());


            var ex = Assert.Throws<FlatFileLoadException>(()=>source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
            Assert.AreEqual("The Excel sheet 'Sheet1' in workbook 'BlankBook.xlsx' is empty", ex.Message);

        }
        [Test]
        public void Checks_ValidFileExtension_Pass()
        {
            if (!officeInstalled)
                Assert.Inconclusive();

            ExcelDataFlowSource source = new ExcelDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(new FileInfo("bob.xlsx")),new ThrowImmediatelyDataLoadEventListener() );
            source.Check(new ThrowImmediatelyCheckNotifier(){ThrowOnWarning = true});
        }
        [Test]
        [ExpectedException(ExpectedMessage = "File extension bob.csv has an invalid extension:.csv (this class only accepts:.xlsx,.xls)")]
        public void Checks_ValidFileExtension_InvalidExtensionPass()
        {
            ExcelDataFlowSource source = new ExcelDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(new FileInfo("bob.csv")), new ThrowImmediatelyDataLoadEventListener());
            source.Check(new ThrowImmediatelyCheckNotifier() { ThrowOnWarning = true });
        }

        [Test]
        public void Checks_ExcelInstalled()
        {
            ExcelDataFlowSource source = new ExcelDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(new FileInfo("bob.xlsx")),new ThrowImmediatelyDataLoadEventListener() );
            try
            {
                //check it
                source.Check(new ThrowImmediatelyCheckNotifier(){ThrowOnWarning = true});
                
                //checking did not throw fail so it must be the case that office is installed
                Assert.IsTrue(officeInstalled);
            }
            catch (Exception e)//it threw on checking
            {
                if (!officeInstalled)
                    Assert.IsTrue(
                        e.Message.Contains("Microsoft Office was not detected on the PC")
                        ||
                        e.Message.Contains("Could not find installed Microsoft Excel application")
                        ,"Expected the error message to be about office not being installed");
                else
                    throw;
            }
        }

        [Test]
        public void TestToCSVConverter()
        {
            if (!officeInstalled)
                Assert.Inconclusive();

            var loc = _fileLocations[TestFile];

            ExcelToCSVFilesConverter converter = new ExcelToCSVFilesConverter();
            converter.ExcelFilePattern = loc.Name;
            
            var mockProjDir = MockRepository.GenerateMock<IHICProjectDirectory>();
            mockProjDir.Expect(p => p.ForLoading).Return(loc.Directory);
          
            var j= new ThrowImmediatelyDataLoadJob();
            j.HICProjectDirectory = mockProjDir;

            converter.Fetch(j, new GracefulCancellationToken());

            var file = loc.Directory.GetFiles("Sheet1.csv").Single();

            Assert.IsTrue(file.Exists);
            
            var contents = File.ReadAllText(file.FullName);

            Assert.AreEqual(
            @"Participant,Score,IsEvil,DateField,DoubleField,MixedField
Bob,3,yes,1/1/2001,0.1,10:30:00
Frank,1.1,no,1/1/2001 10:30,0.51,11:30:00
Hank,2.1,no,1/1/2002 11:30,0.22,0.1
Shanker,2,yes,1/1/2003 1:30,0.10,0.51
,,,,,
Bobboy,2,maybe,9/18/2015,15:09,00:03:56", contents.Trim(new[] { ',', '\r', '\n', ' ', '\t' }));

            file.Delete();

        }
    }
}
