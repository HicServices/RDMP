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
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Checks.Checkers;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.CheckingTests;

public class ProcessTaskCheckingTests : DatabaseTests
{
    private LoadMetadata _lmd;
    private ProcessTask _task;
    private ProcessTaskChecks _checker;
    private DirectoryInfo _dir;

    [SetUp]
    public void CreateTask()
    {
        _lmd = new LoadMetadata(CatalogueRepository);

        _dir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "ProcessTaskCheckingTests"));
        _dir.Create();

        var hicdir = LoadDirectory.CreateDirectoryStructure(_dir, "ProjDir", true, _lmd);
        _lmd.SaveToDatabase();

        var c = new Catalogue(CatalogueRepository, "c");
        var ci = new CatalogueItem(CatalogueRepository, c, "ci");
        var t = new TableInfo(CatalogueRepository, "t")
        {
            Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
            Database = "mydb"
        };
        t.SaveToDatabase();
        var col = new ColumnInfo(CatalogueRepository, "col", "bit", t);
        ci.SetColumnInfo(col);
        c.SaveToDatabase();
        _lmd.LinkToCatalogue(c);
        _task = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles);
        _checker = new ProcessTaskChecks(_lmd);
    }


    [Test]
    [TestCase(null, ProcessTaskType.Executable)]
    [TestCase("", ProcessTaskType.Executable)]
    [TestCase("     ", ProcessTaskType.Executable)]
    [TestCase(null, ProcessTaskType.SQLFile)]
    [TestCase("", ProcessTaskType.SQLFile)]
    [TestCase("     ", ProcessTaskType.SQLFile)]
    public void EmptyFilePath(string path, ProcessTaskType typeThatRequiresFiles)
    {
        _task.ProcessTaskType = typeThatRequiresFiles;
        _task.Path = path;
        _task.SaveToDatabase();
        var ex = Assert.Throws<Exception>(() => _checker.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Does.Contain("does not have a path specified"));
    }

    [Test]
    [TestCase(null, ProcessTaskType.MutilateDataTable, LoadStage.AdjustStaging)]
    [TestCase("", ProcessTaskType.MutilateDataTable, LoadStage.AdjustStaging)]
    [TestCase("     ", ProcessTaskType.MutilateDataTable, LoadStage.AdjustRaw)]
    [TestCase(null, ProcessTaskType.Attacher, LoadStage.Mounting)]
    [TestCase(null, ProcessTaskType.DataProvider, LoadStage.GetFiles)]
    public void EmptyClassPath(string path, ProcessTaskType typeThatRequiresMEF, LoadStage stage)
    {
        _task.ProcessTaskType = typeThatRequiresMEF;
        _task.Path = path;
        _task.LoadStage = stage;
        _task.SaveToDatabase();
        var ex = Assert.Throws<ArgumentException>(() => _checker.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(Regex.IsMatch(ex.Message,
            "Path is blank for ProcessTask 'New Process.*' - it should be a class name of type"));
    }

    [Test]
    public void MEFIncompatibleType()
    {
        _task.LoadStage = LoadStage.AdjustStaging;
        _task.ProcessTaskType = ProcessTaskType.MutilateDataTable;
        _task.Path = typeof(object).ToString();
        _task.SaveToDatabase();
        var ex = Assert.Throws<Exception>(() => _checker.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(
            ex.Message, Is.EqualTo("Requested typeToCreate 'System.Object' was not assignable to the required Type 'IMutilateDataTables'"));
    }

    [Test]
    public void MEFCompatibleType_NoProjectDirectory()
    {
        _lmd.LocationOfForLoadingDirectory = null;
        _lmd.LocationOfForArchivingDirectory = null;
        _lmd.LocationOfExecutablesDirectory = null;
        _lmd.LocationOfCacheDirectory = null;
        _lmd.SaveToDatabase();

        _task.ProcessTaskType = ProcessTaskType.Attacher;
        _task.LoadStage = LoadStage.Mounting;
        _task.Path = typeof(AnySeparatorFileAttacher).FullName;
        _task.SaveToDatabase();
        _task.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>();

        var ex = Assert.Throws<Exception>(() => _checker.Check(ThrowImmediatelyCheckNotifier.QuietPicky));
        Assert.That(ex.InnerException.Message, Is.EqualTo($@"No Project Directory has been configured on LoadMetadata {_lmd.Name}"));
    }

    [Test]
    public void MEFCompatibleType_NoArgs()
    {
        var projDir =
            LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                "DelMeProjDir", true, _lmd);
        try
        {
            _task.ProcessTaskType = ProcessTaskType.Attacher;
            _task.LoadStage = LoadStage.Mounting;
            _task.Path = typeof(AnySeparatorFileAttacher).FullName;
            _task.SaveToDatabase();


            var ex = Assert.Throws<ArgumentException>(() => _checker.Check(ThrowImmediatelyCheckNotifier.QuietPicky));

            Assert.That(
                ex.Message, Is.EqualTo(@"Class AnySeparatorFileAttacher has a Mandatory property 'Separator' marked with DemandsInitialization but no corresponding argument was provided in ArgumentCollection"));
        }
        finally
        {
            //delete everything for real
            projDir.RootPath.Delete(true);
        }
    }

    [Test]
    public void MEFCompatibleType_Passes()
    {
        var projDir =
            LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                "DelMeProjDir", true, _lmd);
        try
        {
            _task.ProcessTaskType = ProcessTaskType.Attacher;
            _task.LoadStage = LoadStage.Mounting;
            _task.Path = typeof(AnySeparatorFileAttacher).FullName;
            _task.SaveToDatabase();

            //create the arguments
            var args = ProcessTaskArgument.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>(_task);

            var tblName = (ProcessTaskArgument)args.Single(a => a.Name.Equals("TableName"));
            tblName.Value = "MyExcitingTable";
            tblName.SaveToDatabase();

            var filePattern = (ProcessTaskArgument)args.Single(a => a.Name.Equals("FilePattern"));
            filePattern.Value = "*.csv";
            filePattern.SaveToDatabase();

            var separator = (ProcessTaskArgument)args.Single(a => a.Name.Equals("Separator"));
            separator.Value = ",";
            separator.SaveToDatabase();

            var results = new ToMemoryCheckNotifier();
            _checker.Check(results);

            foreach (var msg in results.Messages)
            {
                Console.WriteLine($"({msg.Result}){msg.Message}");

                if (msg.Ex != null)
                    Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(msg.Ex));
            }

            Assert.That(results.GetWorst(), Is.EqualTo(CheckResult.Success));
        }
        finally
        {
            //delete everything for real
            projDir.RootPath.Delete(true);
        }
    }

    [Test]
    [TestCase("bob.exe")]
    [TestCase(@"""C:\ProgramFiles\My Software With Spaces\bob.exe""")]
    [TestCase(@"""C:\ProgramFiles\My Software With Spaces\bob.exe"" arg1 arg2 -f ""c:\my folder\arg3.exe""")]
    public void ImaginaryFile(string path)
    {
        _task.ProcessTaskType = ProcessTaskType.Executable;
        _task.Path = path;
        _task.SaveToDatabase();
        var ex = Assert.Throws<Exception>(() => _checker.Check(ThrowImmediatelyCheckNotifier.QuietPicky));
        Assert.That(ex?.Message, Does.Contain("bob.exe which does not exist at this time."));
    }
}