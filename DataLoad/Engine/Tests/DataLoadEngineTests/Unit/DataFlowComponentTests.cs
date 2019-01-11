using System;
using System.Data;
using System.Text.RegularExpressions;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using LoadModules.Generic.DataFlowOperations;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace DataLoadEngineTests.Unit
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