// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using System;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataProvider;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class ImportFilesDataProviderTests : DatabaseTests
{
    [Test]
    public void CopyFiles()
    {
        var sourceDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory).CreateSubdirectory("subdir");
        var targetDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory).CreateSubdirectory("loaddir");

        //make sure target is empty
        foreach (var f in targetDir.GetFiles())
            f.Delete();

        var originpath = Path.Combine(sourceDir.FullName, "myFile.txt");

        File.WriteAllText(originpath, "fish");

        var job = new ThrowImmediatelyDataLoadJob();
        var mockProjectDirectory = Substitute.For<ILoadDirectory>();
        mockProjectDirectory.ForLoading.Returns(targetDir);
        job.LoadDirectory = mockProjectDirectory;


        //Create the provider
        var provider = new ImportFilesDataProvider();

        //it doesn't know what to load yet
        Assert.Throws<Exception>(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));

        //now it does
        provider.DirectoryPath = sourceDir.FullName;

        //but it doesn't have a file pattern
        Assert.Throws<Exception>(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));

        //now it does but its not a matching one
        provider.FilePattern = "cannonballs.bat";

        //either way it passes checking
        Assert.DoesNotThrow(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));

        //execute the provider
        provider.Fetch(job, new GracefulCancellationToken());

        //destination is empty because nothing matched
        Assert.That(targetDir.GetFiles(), Is.Empty);

        //give it correct pattern
        provider.FilePattern = "*.txt";

        //execute the provider
        provider.Fetch(job, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            //both files should exist
            Assert.That(targetDir.GetFiles(), Has.Length.EqualTo(1));
            Assert.That(sourceDir.GetFiles(), Has.Length.EqualTo(1));
        });

        //simulate load failure
        provider.LoadCompletedSoDispose(ExitCodeType.Abort, new ThrowImmediatelyDataLoadJob());

        Assert.Multiple(() =>
        {
            //both files should exist
            Assert.That(targetDir.GetFiles(), Has.Length.EqualTo(1));
            Assert.That(sourceDir.GetFiles(), Has.Length.EqualTo(1));
        });

        //simulate load success
        provider.LoadCompletedSoDispose(ExitCodeType.Success, new ThrowImmediatelyDataLoadJob());

        Assert.Multiple(() =>
        {
            //both files should exist because Delete on success is false
            Assert.That(targetDir.GetFiles(), Has.Length.EqualTo(1));
            Assert.That(sourceDir.GetFiles(), Has.Length.EqualTo(1));
        });

        //change behaviour to delete on successful data loads
        provider.DeleteFilesOnsuccessfulLoad = true;

        //simulate load failure
        provider.LoadCompletedSoDispose(ExitCodeType.Error, new ThrowImmediatelyDataLoadJob());

        Assert.Multiple(() =>
        {
            //both files should exist
            Assert.That(targetDir.GetFiles(), Has.Length.EqualTo(1));
            Assert.That(sourceDir.GetFiles(), Has.Length.EqualTo(1));
        });

        //simulate load success
        provider.LoadCompletedSoDispose(ExitCodeType.Success, new ThrowImmediatelyDataLoadJob());

        Assert.Multiple(() =>
        {
            //only forLoading file should exist (in real life that one would be handled by archivng already)
            Assert.That(targetDir.GetFiles(), Has.Length.EqualTo(1));
            Assert.That(sourceDir.GetFiles(), Is.Empty);
        });
    }
}