// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.Checks;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport;

public class ProjectChecksTestsSimple : DatabaseTests
{
    [Test]
    public void Project_NoConfigurations()
    {
        var p = new Project(DataExportRepository, "Fish");

        try
        {
            var ex = Assert.Throws<Exception>(() =>
                new ProjectChecker(new ThrowImmediatelyActivator(RepositoryLocator), p).Check(
                    ThrowImmediatelyCheckNotifier.Quiet));
            Assert.That(ex?.Message, Is.EqualTo("Project does not have any ExtractionConfigurations yet"));
        }
        finally
        {
            p.DeleteInDatabase();
        }
    }

    [Test]
    public void Project_NoDirectory()
    {
        var p = GetProjectWithConfig(out var config);
        var ex = Assert.Throws<Exception>(() => RunTestWithCleanup(p, config));
        Assert.That(ex?.Message, Is.EqualTo("Project does not have an ExtractionDirectory"));
    }

    [Test]
    [TestCase(@"C:\asdlfasdjlfhasjldhfljh")]
    [TestCase(@"\\MyMakeyUpeyServer\Where")]
    [TestCase(@"Z:\WizardOfOz")]
    public void Project_NonExistentDirectory(string dir)
    {
        var p = GetProjectWithConfig(out var config);

        p.ExtractionDirectory = dir;
        var ex = Assert.Throws<Exception>(() => RunTestWithCleanup(p, config));
        Assert.That(Regex.IsMatch(ex.Message, @"Project ExtractionDirectory .* Does Not Exist"));
    }

    [Test]
    public void Project_DodgyCharactersInExtractionDirectoryName()
    {
        var p = GetProjectWithConfig(out var config);
        p.ExtractionDirectory = @"C:\|||";

        var ex = Assert.Throws<Exception>(() => RunTestWithCleanup(p, config));
        Assert.That(ex.Message, Is.EqualTo(@"Project ExtractionDirectory ('C:\|||') Does Not Exist"));
    }

    [Test]
    public void ConfigurationFrozen_Remnants()
    {
        var p = GetProjectWithConfigDirectory(out var config, out var dir);

        //create remnant directory (empty)
        var remnantDir = dir.CreateSubdirectory($"Extr_{config.ID}20011225");

        //with empty subdirectories
        remnantDir.CreateSubdirectory("DMPTestCatalogue").CreateSubdirectory("Lookups");

        config.IsReleased = true; //make environment think config is released
        config.SaveToDatabase();

        try
        {
            Assert.Multiple(() =>
            {
                //remnant exists
                Assert.That(dir.Exists);
                Assert.That(remnantDir.Exists);
            });

            //resolve accepting deletion
            new ProjectChecker(new ThrowImmediatelyActivator(RepositoryLocator), p).Check(new AcceptAllCheckNotifier());

            Assert.Multiple(() =>
            {
                //boom remnant doesn't exist anymore (but parent does obviously)
                Assert.That(dir.Exists);
                Assert.That(Directory.Exists(remnantDir.FullName), Is.False); //can't use .Exists for some reason, c# caches answer?
            });
        }
        finally
        {
            config.DeleteInDatabase();
            p.DeleteInDatabase();
        }
    }


    [Test]
    public void ConfigurationFrozen_RemnantsWithFiles()
    {
        var p = GetProjectWithConfigDirectory(out var config, out var dir);

        //create remnant directory (empty)
        var remnantDir = dir.CreateSubdirectory($"Extr_{config.ID}20011225");

        //with empty subdirectories
        var lookupDir = remnantDir.CreateSubdirectory("DMPTestCatalogue").CreateSubdirectory("Lookups");

        //this time put a file in
        File.AppendAllLines(Path.Combine(lookupDir.FullName, "Text.txt"), new string[] { "Amagad" });

        config.IsReleased = true; //make environment think config is released
        config.SaveToDatabase();
        try
        {
            var notifier = new ToMemoryCheckNotifier();
            RunTestWithCleanup(p, config, notifier);

            Assert.That(notifier.Messages.Any(
                m => m.Result == CheckResult.Fail &&
                     Regex.IsMatch(m.Message,
                         @"Found non-empty folder .* which is left over extracted folder after data release \(First file found was '.*[/\\]DMPTestCatalogue[/\\]Lookups[/\\]Text.txt' but there may be others\)")));
        }
        finally
        {
            remnantDir.Delete(true);
        }
    }

    [Test]
    public void Configuration_NoDatasets()
    {
        var p = GetProjectWithConfigDirectory(out var config, out _);
        var ex = Assert.Throws<Exception>(() => RunTestWithCleanup(p, config));
        Assert.That(ex.Message, Does.StartWith("There are no datasets selected for open configuration 'New ExtractionConfiguration"));
    }


    [Test]
    public void Configuration_NoProjectNumber()
    {
        var p = GetProjectWithConfigDirectory(out var config, out _);
        p.ProjectNumber = null;
        var ex = Assert.Throws<Exception>(() => RunTestWithCleanup(p, config));
        Assert.That(
            ex.Message, Does.Contain("Project does not have a Project Number, this is a number which is meaningful to you (as opposed to ID which is the "));
    }

    private void RunTestWithCleanup(Project p, ExtractionConfiguration config, ICheckNotifier notifier = null)
    {
        try
        {
            new ProjectChecker(new ThrowImmediatelyActivator(RepositoryLocator), p).Check(notifier ??
                ThrowImmediatelyCheckNotifier.QuietPicky);
        }
        finally
        {
            config.DeleteInDatabase();
            p.DeleteInDatabase();
        }
    }

    private Project GetProjectWithConfig(out ExtractionConfiguration config)
    {
        var p = new Project(DataExportRepository, "Fish")
        {
            ProjectNumber = -5000
        };
        config = new ExtractionConfiguration(DataExportRepository, p);
        return p;
    }

    private Project GetProjectWithConfigDirectory(out ExtractionConfiguration config, out DirectoryInfo dir)
    {
        var p = new Project(DataExportRepository, "Fish");
        config = new ExtractionConfiguration(DataExportRepository, p);

        var projectFolder = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ProjectCheckerTestDir");

        dir = new DirectoryInfo(projectFolder);
        dir.Create();

        p.ExtractionDirectory = projectFolder;
        p.ProjectNumber = -5000;
        p.SaveToDatabase();

        return p;
    }
}