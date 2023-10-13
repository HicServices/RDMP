// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataFlowPipeline;
using System;
using System.IO;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System.Data;
using NUnit.Framework.Internal;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Terminal.Gui;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

internal class ExtractionHoldoutTests: TestsRequiringAnExtractionConfiguration
{
    private SimpleFileExtractor _extractor;
    private DirectoryInfo _inDir;
    private DirectoryInfo _outDir;
    private DirectoryInfo _inDirSub1;
    private DirectoryInfo _inDirSub2;

    private ExtractionHoldout _holdout;
    private DirectoryInfo _holdoutDir;
    private DataTable _toProcess;
    private IExtractCommand _ExtractionStub;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();
        _holdout = new ExtractionHoldout();
        _holdoutDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Holdout"));
        Console.WriteLine(Path.Combine(TestContext.CurrentContext.WorkDirectory));
        if(_holdoutDir.Exists)
        {
            _holdoutDir.Delete(true);
        }
        _holdoutDir.Create();
        _toProcess = new DataTable();
        _toProcess.Columns.Add("FAKE_CHI");
        _toProcess.Rows.Add(1);
        _toProcess.Rows.Add(2);
        _toProcess.Rows.Add(3);
        _toProcess.Rows.Add(4);
        _toProcess.Rows.Add(5);
        _toProcess.Rows.Add(6);
    }

    [Test]
    public void NoConfiguration()
    {
        var ex = Assert.Throws<Exception>(() => _holdout.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.IsTrue(ex.Message.Contains("No holdout file location set."));
    }

    [Test]
    public void LocationSet()
    {
        _holdout.holdoutStorageLocation = _holdoutDir.FullName;
        var ex = Assert.Throws<Exception>(() => _holdout.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.IsTrue(ex.Message.Contains("No data holdout count set."));
    }

    [Test]
    public void LocationAndCountSet()
    {
        _holdout.holdoutStorageLocation = _holdoutDir.FullName;
        _holdout.holdoutCount = 1;
        _holdout.Check(ThrowImmediatelyCheckNotifier.Quiet);
        Console.WriteLine(_holdoutDir.FullName);
        FileAssert.DoesNotExist(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv"));//todo use the correct name
    }

    [Test]
    public void ExtractionHoldoutSingle()
    {
        _holdout.holdoutStorageLocation = _holdoutDir.FullName;
        _holdout.holdoutCount = 1;
        _ExtractionStub = new ExtractDatasetCommand(_configuration, new ExtractableDatasetBundle(_extractableDataSet));
        _holdout.PreInitialize(_ExtractionStub, ThrowImmediatelyDataLoadEventListener.Quiet);
        _holdout.ProcessPipelineData(_toProcess, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        _holdout.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        Assert.IsTrue(File.Exists(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv")));
        String expectedOutput = File.ReadAllText(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv"));
        Assert.That(expectedOutput, Does.Match("FAKE_CHI\r\n[1-6]\r\n"));

    }

    [Test]
    public void ExtractionHoldoutPercentage()
    {
        _holdout.holdoutStorageLocation = _holdoutDir.FullName;
        _holdout.holdoutCount = 33;
        _holdout.isPercentage = true;
        _ExtractionStub = new ExtractDatasetCommand(_configuration, new ExtractableDatasetBundle(_extractableDataSet));
        _holdout.PreInitialize(_ExtractionStub, ThrowImmediatelyDataLoadEventListener.Quiet);
        _holdout.ProcessPipelineData(_toProcess, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        _holdout.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        Assert.IsTrue(File.Exists(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv")));
        String expectedOutput = File.ReadAllText(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv"));
        Assert.That(expectedOutput, Does.Match("FAKE_CHI\r\n[1-6]\r\n[1-6]\r\n"));

    }
    [Test]
    public void ExtractionHoldoutPercentageAppend()
    {
        _holdout.holdoutStorageLocation = _holdoutDir.FullName;
        _holdout.holdoutCount = 33;
        _holdout.isPercentage = true;
        _holdout.overrideFile = false;
        _ExtractionStub = new ExtractDatasetCommand(_configuration, new ExtractableDatasetBundle(_extractableDataSet));
        _holdout.PreInitialize(_ExtractionStub, ThrowImmediatelyDataLoadEventListener.Quiet);
        _holdout.ProcessPipelineData(_toProcess, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        _holdout.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        Assert.IsTrue(File.Exists(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv")));
        String expectedOutput = File.ReadAllText(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv"));
        Assert.That(expectedOutput, Does.Match("FAKE_CHI\r\n[1-6]\r\n[1-6]\r\n"));

        _holdout.PreInitialize(_ExtractionStub, ThrowImmediatelyDataLoadEventListener.Quiet);
        _holdout.ProcessPipelineData(_toProcess, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());
        _holdout.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        Assert.IsTrue(File.Exists(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv")));
        expectedOutput = File.ReadAllText(Path.Combine(_holdoutDir.FullName, "holdout_TestTable.csv"));
        Assert.That(expectedOutput, Does.Match("FAKE_CHI\r\n[1-6]\r\n[1-6]\r\n[1-6]\r\n[1-6]\r\n"));

    }

    //[Test]
    //public void OneFile()
    //{
    //    _extractor.Directories = false;
    //    _extractor.Pattern = "blah.*";
    //    _extractor.OutputDirectoryName = _outDir.FullName;
    //    _extractor.Check(ThrowImmediatelyCheckNotifier.Quiet);

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));

    //    _extractor.MoveAll(_outDir, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

    //    FileAssert.Exists(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));
    //    DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName, "Sub1"));
    //    DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName, "Sub2"));
    //}

    //[Test]
    //public void AllDirs()
    //{
    //    _extractor.Directories = true;
    //    _extractor.Pattern = "*";
    //    _extractor.OutputDirectoryName = _outDir.FullName;
    //    _extractor.Check(ThrowImmediatelyCheckNotifier.Quiet);

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));

    //    _extractor.MoveAll(_outDir, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));
    //    DirectoryAssert.Exists(Path.Combine(_outDir.FullName, "Sub1"));
    //    DirectoryAssert.Exists(Path.Combine(_outDir.FullName, "Sub2"));
    //}

    //[Test]
    //public void OneDir()
    //{
    //    _extractor.Directories = true;
    //    _extractor.Pattern = "*1";
    //    _extractor.OutputDirectoryName = _outDir.FullName;
    //    _extractor.Check(ThrowImmediatelyCheckNotifier.Quiet);

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));

    //    _extractor.MoveAll(_outDir, ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));
    //    DirectoryAssert.Exists(Path.Combine(_outDir.FullName, "Sub1"));
    //    DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName, "Sub2"));
    //}

    //[Test]
    //public void PatientFiles()
    //{
    //    _extractor.PerPatient = true;
    //    _extractor.Directories = false;
    //    _extractor.Pattern = "$p.txt";
    //    _extractor.OutputDirectoryName = _outDir.FullName;
    //    _extractor.Check(ThrowImmediatelyCheckNotifier.Quiet);

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));

    //    _extractor.MovePatient("Pat1", "Rel1", _outDir, ThrowImmediatelyDataLoadEventListener.QuietPicky,
    //        new GracefulCancellationToken());

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));
    //    FileAssert.Exists(Path.Combine(_outDir.FullName, "Rel1.txt"));
    //}

    //[Test]
    //public void PatientFileMissingOne()
    //{
    //    _extractor.PerPatient = true;
    //    _extractor.Directories = false;
    //    _extractor.Pattern = "$p.txt";
    //    _extractor.OutputDirectoryName = _outDir.FullName;
    //    _extractor.Check(ThrowImmediatelyCheckNotifier.Quiet);

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));

    //    var mem = new ToMemoryDataLoadEventListener(true);

    //    _extractor.MovePatient("Pat1", "Rel1", _outDir, mem, new GracefulCancellationToken());
    //    _extractor.MovePatient("Pat2", "Rel2", _outDir, mem, new GracefulCancellationToken());

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));
    //    FileAssert.Exists(Path.Combine(_outDir.FullName, "Rel1.txt"));

    //    Assert.AreEqual(ProgressEventType.Warning, mem.GetWorst());

    //    StringAssert.StartsWith("No Files were found matching Pattern Pat2.txt in ",
    //        mem.GetAllMessagesByProgressEventType()[ProgressEventType.Warning].Single().Message);
    //}

    //[Test]
    //public void PatientDirs()
    //{
    //    _extractor.PerPatient = true;
    //    _extractor.Directories = true;
    //    _extractor.Pattern = "$p";
    //    _extractor.OutputDirectoryName = _outDir.FullName;
    //    _extractor.Check(ThrowImmediatelyCheckNotifier.Quiet);

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));

    //    _extractor.MovePatient("Sub1", "Rel1", _outDir, ThrowImmediatelyDataLoadEventListener.QuietPicky,
    //        new GracefulCancellationToken());

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));
    //    DirectoryAssert.Exists(Path.Combine(_outDir.FullName, "Rel1"));
    //    DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName, "Rel2"));
    //    DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName, "Sub1"));
    //    DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName, "Sub2"));
    //}

    //[Test]
    //public void PatientBothDirs()
    //{
    //    _extractor.PerPatient = true;
    //    _extractor.Directories = true;
    //    _extractor.Pattern = "$p";
    //    _extractor.OutputDirectoryName = _outDir.FullName;
    //    _extractor.Check(ThrowImmediatelyCheckNotifier.Quiet);

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));

    //    _extractor.MovePatient("Sub1", "Rel1", _outDir, ThrowImmediatelyDataLoadEventListener.QuietPicky,
    //        new GracefulCancellationToken());
    //    _extractor.MovePatient("Sub2", "Rel2", _outDir, ThrowImmediatelyDataLoadEventListener.QuietPicky,
    //        new GracefulCancellationToken());

    //    // does not exist
    //    var ex = Assert.Throws<Exception>(() => _extractor.MovePatient("Sub3", "Rel3", _outDir,
    //        ThrowImmediatelyDataLoadEventListener.QuietPicky, new GracefulCancellationToken()));
    //    Assert.AreEqual(
    //        $"No Directories were found matching Pattern Sub3 in {_inDir.FullName}.  For private identifier 'Sub3'",
    //        ex.Message);

    //    // if not throwing on warnings then a missing sub just passes through and is ignored
    //    _extractor.MovePatient("Sub3", "Rel3", _outDir, ThrowImmediatelyDataLoadEventListener.Quiet,
    //        new GracefulCancellationToken());

    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah.txt"));
    //    FileAssert.DoesNotExist(Path.Combine(_outDir.FullName, "blah2.txt"));
    //    DirectoryAssert.Exists(Path.Combine(_outDir.FullName, "Rel1"));
    //    DirectoryAssert.Exists(Path.Combine(_outDir.FullName, "Rel2"));
    //    DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName, "Sub1"));
    //    DirectoryAssert.DoesNotExist(Path.Combine(_outDir.FullName, "Sub2"));
    //}
}