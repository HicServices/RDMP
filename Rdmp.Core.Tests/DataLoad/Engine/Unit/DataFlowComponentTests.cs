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
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit
{
    class DataFlowComponentTests
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

            var ex = Assert.Throws<InvalidOperationException>(() => renamer.ProcessPipelineData( toProcess, new ThrowImmediatelyDataLoadEventListener(), cts.Token));
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

            var processed = renamer.ProcessPipelineData( toProcess, new ThrowImmediatelyDataLoadEventListener(), cts.Token);

            Assert.AreEqual(1, processed.Columns.Count);
            Assert.AreEqual("ReplacementName", processed.Columns[0].ColumnName);
        }
    }

    class ColumnBlacklistTests
    {
        [Test]
        public void ColumnBlacklisterTest_MatchingColumn()
        {
            var blacklister = new ColumnBlacklister();
            blacklister.CrashIfAnyColumnMatches = new Regex("^fish$");

            var toProcess = new DataTable();
            toProcess.Columns.Add("ToFind");

            Assert.DoesNotThrow(() => blacklister.ProcessPipelineData(toProcess, new ThrowImmediatelyDataLoadJob(), null));
            toProcess.Columns.Add("fish");

            Assert.Throws<Exception>(()=>blacklister.ProcessPipelineData(toProcess, new ThrowImmediatelyDataLoadJob(), null));

            blacklister.Rationale = "kaleidoscope engage";
            var ex = Assert.Throws<Exception>(() => blacklister.ProcessPipelineData(toProcess, new ThrowImmediatelyDataLoadJob(), null));
            Assert.IsTrue(ex.Message.Contains("kaleidoscope engage"));


        }
    }
}