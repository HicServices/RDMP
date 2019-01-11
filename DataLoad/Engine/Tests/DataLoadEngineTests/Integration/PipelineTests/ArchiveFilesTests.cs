using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Standard;
using DataLoadEngine.LoadProcess;
using HIC.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration.PipelineTests
{
    public class ArchiveFilesTests : DatabaseTests
    {
        [Test]
        public void TestAllFilesAreArchived()
        {
            var directoryHelper = new TestDirectoryHelper(GetType());
            directoryHelper.SetUp();

            var forArchiving = directoryHelper.Directory.CreateSubdirectory("forArchiving");
            var forLoading = directoryHelper.Directory.CreateSubdirectory("forLoading");
            File.WriteAllText(Path.Combine(forLoading.FullName, "test.txt"), "test data");
            var subDir = forLoading.CreateSubdirectory("subdir");
            File.WriteAllText(Path.Combine(subDir.FullName, "subdir.txt"), "test data in subdir");

            // test the hidden dir which the archiver should ignore
            var hiddenDir = forLoading.CreateSubdirectory(ArchiveFiles.HiddenFromArchiver);
            File.WriteAllText(Path.Combine(hiddenDir.FullName, "hidden.txt"), "I should not appear in the archive");

            var archiveComponent = new ArchiveFiles(new HICLoadConfigurationFlags());
            
            var dataLoadInfo = MockRepository.GenerateStub<IDataLoadInfo>();
            dataLoadInfo.Stub(info => info.ID).Return(1);

            var hicProjectDirectory = MockRepository.GenerateStub<IHICProjectDirectory>();
            hicProjectDirectory.Stub(d => d.ForArchiving).Return(forArchiving);
            hicProjectDirectory.Stub(d => d.ForLoading).Return(forLoading);

            var job = MockRepository.GenerateStub<IDataLoadJob>();
            job.Stub(j => j.DataLoadInfo).Return(dataLoadInfo);
            job.HICProjectDirectory = hicProjectDirectory;

            try
            {
                archiveComponent.Run(job, new GracefulCancellationToken());

                // first we expect a file in forArchiving called 1.zip
                var zipFilename = Path.Combine(forArchiving.FullName, "1.zip");
                Assert.True(File.Exists(zipFilename));

                // there should be two entries
                using (var archive = ZipFile.Open(zipFilename, ZipArchiveMode.Read))
                {
                    Assert.AreEqual(2, archive.Entries.Count, "There should be two entries in this archive: one from the root and one from the subdirectory");
                    Assert.IsTrue(archive.Entries.Any(entry => entry.FullName.Equals(@"subdir\subdir.txt")));
                    Assert.IsTrue(archive.Entries.Any(entry => entry.FullName.Equals(@"test.txt")));
                }
            }
            finally
            {
                directoryHelper.TearDown();
            }
        }

        [Test]
        public void HowDoesMEFHandleTypeNames()
        {

            string expected = "CatalogueLibrary.DataFlowPipeline.IDataFlowSource(System.Data.DataTable)";

            Assert.AreEqual(expected, MEF.GetMEFNameForType(typeof(IDataFlowSource<DataTable>)));
        }

        [Test]
        public void CreateArchiveWithNoFiles_ShouldThrow()
        {
            var directoryHelper = new TestDirectoryHelper(GetType());
            directoryHelper.SetUp();
            
            var testDir = directoryHelper.Directory.CreateSubdirectory("CreateArchiveWithNoFiles_ShouldThrow");
            
            var archiveFiles = new ArchiveFiles(new HICLoadConfigurationFlags());
            var hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(testDir, "dataset");
            
            var job = MockRepository.GenerateStub<IDataLoadJob>();
            job.Stub(j => j.DataLoadInfo).Return(MockRepository.GenerateStub<IDataLoadInfo>());
            job.HICProjectDirectory = hicProjectDirectory;

            try
            {
                archiveFiles.Run(job, new GracefulCancellationToken());

                foreach (FileInfo fileInfo in hicProjectDirectory.ForArchiving.GetFiles("*.zip"))
                    Console.WriteLine("About to throw up because of zip file:" + fileInfo.FullName);

                Assert.IsFalse(hicProjectDirectory.ForArchiving.GetFiles("*.zip").Any(),"There should not be any zip files in the archive directory!");
            }
            finally
            {
                directoryHelper.TearDown();
            }
        }
      
    }
}

