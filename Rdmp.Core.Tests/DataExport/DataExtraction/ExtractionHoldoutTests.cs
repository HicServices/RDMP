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
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System.Data;
using NUnit.Framework.Internal;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

internal class ExtractionHoldoutTests: TestsRequiringAnExtractionConfiguration
{
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

}