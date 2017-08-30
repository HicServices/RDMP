using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using LoadModules.Generic.FileOperations;
using DataLoadEngineTests.Resources;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace DataLoadEngineTests.Integration
{
    [Category("Integration")]
    [Ignore("These require Microsoft Office to be installed on the test machine (the provided interop assemblies only wrap the COM functionality)")]
    public class ExcelConversionTest
    {
        private readonly Stack<DirectoryInfo> _dirsToCleanUp = new Stack<DirectoryInfo>();
        private DirectoryInfo _parentDir;
      
        [TestFixtureSetUp]
        public void SetUp()
        {
            var testDir = new DirectoryInfo(".");
            _parentDir = testDir.CreateSubdirectory("ExcelConversionTest");
            _dirsToCleanUp.Push(_parentDir);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            while (_dirsToCleanUp.Any())
                _dirsToCleanUp.Pop().Delete(true);
        }

        private HICProjectDirectory CreateHICProjectDirectoryForTest(string directoryName)
        {
            var hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(_parentDir, directoryName);
            _dirsToCleanUp.Push(hicProjectDirectory.RootPath);
            return hicProjectDirectory;
        }

        [Test]
        public void TestExcelFunctionality_OnSimpleXlsx()
        {
            var hicProjectDirectory = CreateHICProjectDirectoryForTest("TestExcelFunctionality_OnSimpleXlsx");

            //clean up anything in the test project folders forloading directory
            foreach (FileInfo fileInfo in hicProjectDirectory.ForLoading.GetFiles())
                fileInfo.Delete();

            string targetFile = Path.Combine(hicProjectDirectory.ForLoading.FullName, "Test.xlsx");
            File.WriteAllBytes(targetFile, Resource1.TestExcelFile1);

            TestConversionFor(targetFile, "*.xlsx", 5, hicProjectDirectory);
        }

        [Test]
        [ExpectedException(ExpectedMessage ="Could not find any files matching extension *.fish",MatchType = MessageMatch.Exact )]
        public void TestExcelFunctionality_DodgyFileExtension()
        {
            var hicProjectDirectory = CreateHICProjectDirectoryForTest("TestExcelFunctionality_DodgyFileExtension");

            //clean up anything in the test project folders forloading directory
            foreach (FileInfo fileInfo in hicProjectDirectory.ForLoading.GetFiles())
                fileInfo.Delete();


            string targetFile = Path.Combine(hicProjectDirectory.ForLoading.FullName, "Test.xml");
            File.WriteAllText(targetFile, Resource1.TestExcelFile2);

            TestConversionFor(targetFile, "*.fish", 1, hicProjectDirectory);
        }


        [Test]
        public void TestExcelFunctionality_OnExcelXml()
        {
            var hicProjectDirectory = CreateHICProjectDirectoryForTest("TestExcelFunctionality_OnExcelXml");

            //clean up anything in the test project folders forloading directory
            foreach (FileInfo fileInfo in hicProjectDirectory.ForLoading.GetFiles())
                fileInfo.Delete();


            string targetFile = Path.Combine(hicProjectDirectory.ForLoading.FullName, "Test.xml");
            File.WriteAllText(targetFile, Resource1.TestExcelFile2);

            TestConversionFor(targetFile, "*.xml", 1, hicProjectDirectory);

        }

        private void TestConversionFor(string targetFile,string fileExtensionToConvert, int expectedNumberOfSheets, HICProjectDirectory hicProjectDirectory)
        {
            FileInfo f = new FileInfo(targetFile);

            try
            {
                Assert.IsTrue(f.Exists);
                Assert.IsTrue(f.Length > 100);

                ExcelToCsvConverter converter = new ExcelToCsvConverter();

                converter.FilePatternToConvert = fileExtensionToConvert;
                converter.Fetch(hicProjectDirectory, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

                FileInfo[] filesCreated = hicProjectDirectory.ForLoading.GetFiles("*.csv");

                Assert.AreEqual(filesCreated.Length, expectedNumberOfSheets);

                foreach (FileInfo fileCreated in filesCreated)
                {
                    Assert.IsTrue(Regex.IsMatch(fileCreated.Name, "_Sheet[0-9].csv"));
                    Assert.GreaterOrEqual(fileCreated.Length, 100);
                    fileCreated.Delete();
                }
            }
            finally
            {
                f.Delete();
            }
        }
    }
}
