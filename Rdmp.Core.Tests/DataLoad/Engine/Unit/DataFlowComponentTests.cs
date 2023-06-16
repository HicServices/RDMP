// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit;

[Category("Unit")]
internal class DataFlowComponentTests
{
    [Test]
    public void ColumnRenamer_NoMatchingColumnAtRuntime()
    {
        var renamer = new ColumnRenamer
        {
            ColumnNameToFind = "DoesNotExist",
            ReplacementName = "ReplacementName"
        };

        var cts = new GracefulCancellationTokenSource();
        var toProcess = new DataTable();
        toProcess.Columns.Add("Column1");

        var ex = Assert.Throws<InvalidOperationException>(() => renamer.ProcessPipelineData( toProcess, ThrowImmediatelyDataLoadEventListener.Quiet, cts.Token));
        Assert.IsTrue(ex.Message.Contains("does not exist in the supplied data table"));
    }

    [Test]
    public void ColumnRenamer_Successful()
    {
        var renamer = new ColumnRenamer
        {
            ColumnNameToFind = "ToFind",
            ReplacementName = "ReplacementName"
        };

        var cts = new GracefulCancellationTokenSource();
        var toProcess = new DataTable();
        toProcess.Columns.Add("ToFind");

        var processed = renamer.ProcessPipelineData( toProcess, ThrowImmediatelyDataLoadEventListener.Quiet, cts.Token);

        Assert.AreEqual(1, processed.Columns.Count);
        Assert.AreEqual("ReplacementName", processed.Columns[0].ColumnName);
    }

    [Test]
    public void ColumnDropper_NoMatchingColumnAtRuntime()
    {
        var dropper = new ColumnDropper
        {
            ColumnNameToDrop = "DoesNotExist"
        };

        var cts = new GracefulCancellationTokenSource();
        var toProcess = new DataTable();
        toProcess.Columns.Add("Column1");

        var ex = Assert.Throws<InvalidOperationException>(() => dropper.ProcessPipelineData( toProcess, ThrowImmediatelyDataLoadEventListener.Quiet, cts.Token));
        Assert.IsTrue(ex.Message.Contains("does not exist in the supplied data table"));
    }

    [Test]
    public void ColumnDropper_Successful()
    {
        var dropper = new ColumnDropper
        {
            ColumnNameToDrop = "ToDrop"
        };

        var cts = new GracefulCancellationTokenSource();
        var toProcess = new DataTable();
        toProcess.Columns.Add("ToDrop");

        var processed = dropper.ProcessPipelineData( toProcess, ThrowImmediatelyDataLoadEventListener.Quiet, cts.Token);

        Assert.AreEqual(0, processed.Columns.Count);
        Assert.AreEqual(false, processed.Columns.Contains("ToDrop"));
    }
}

[Category("Unit")]
internal class ColumnforbidlistTests
{
    [Test]
    public void ColumnForbidderTest_MatchingColumn()
    {
        var forbidlister = new ColumnForbidder
        {
            CrashIfAnyColumnMatches = new Regex("^fish$")
        };

        var toProcess = new DataTable();
        toProcess.Columns.Add("ToFind");

        Assert.DoesNotThrow(() => forbidlister.ProcessPipelineData(toProcess, new ThrowImmediatelyDataLoadJob(), null));
        toProcess.Columns.Add("fish");

        Assert.Throws<Exception>(() =>
            forbidlister.ProcessPipelineData(toProcess, new ThrowImmediatelyDataLoadJob(), null));

        forbidlister.Rationale = "kaleidoscope engage";
        var ex = Assert.Throws<Exception>(() =>
            forbidlister.ProcessPipelineData(toProcess, new ThrowImmediatelyDataLoadJob(), null));
        Assert.IsTrue(ex.Message.Contains("kaleidoscope engage"));
    }
}