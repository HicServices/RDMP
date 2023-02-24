// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Standard;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.Logging;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests;

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
            
        var dataLoadInfo = Mock.Of<IDataLoadInfo>(info => info.ID==1);

        var LoadDirectory = Mock.Of<ILoadDirectory>(d => d.ForArchiving==forArchiving && d.ForLoading==forLoading);

        var job = Mock.Of<IDataLoadJob>(j => j.DataLoadInfo==dataLoadInfo);
        job.LoadDirectory = LoadDirectory;

        try
        {
            archiveComponent.Run(job, new GracefulCancellationToken());

            // first we expect a file in forArchiving called 1.zip
            var zipFilename = Path.Combine(forArchiving.FullName, "1.zip");
            Assert.True(File.Exists(zipFilename));

            // there should be two entries
            using var archive = ZipFile.Open(zipFilename, ZipArchiveMode.Read);
            Assert.AreEqual(2, archive.Entries.Count, "There should be two entries in this archive: one from the root and one from the subdirectory");
            Assert.IsTrue(archive.Entries.Any(entry => entry.FullName.Equals(@"subdir/subdir.txt")));
            Assert.IsTrue(archive.Entries.Any(entry => entry.FullName.Equals(@"test.txt")));
        }
        finally
        {
            directoryHelper.TearDown();
        }
    }
        
    [Test]
    public void CreateArchiveWithNoFiles_ShouldThrow()
    {
        var directoryHelper = new TestDirectoryHelper(GetType());
        directoryHelper.SetUp();
            
        var testDir = directoryHelper.Directory.CreateSubdirectory("CreateArchiveWithNoFiles_ShouldThrow");
            
        var archiveFiles = new ArchiveFiles(new HICLoadConfigurationFlags());
        var loadDirectory = LoadDirectory.CreateDirectoryStructure(testDir, "dataset");
            
        var job = Mock.Of<IDataLoadJob>(j => j.DataLoadInfo==Mock.Of<IDataLoadInfo>());
        job.LoadDirectory = loadDirectory;

        try
        {
            archiveFiles.Run(job, new GracefulCancellationToken());

            foreach (var fileInfo in loadDirectory.ForArchiving.GetFiles("*.zip"))
                Console.WriteLine($"About to throw SetUp because of zip file:{fileInfo.FullName}");

            Assert.IsFalse(loadDirectory.ForArchiving.GetFiles("*.zip").Any(),"There should not be any zip files in the archive directory!");
        }
        finally
        {
            directoryHelper.TearDown();
        }
    }
      
}