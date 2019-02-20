using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using FAnsi;
using LoadModules.Generic.DataFlowOperations.Aliases;
using LoadModules.Generic.DataFlowOperations.Aliases.Exceptions;
using LoadModules.Generic.DataFlowOperations.Swapping;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Integration.PipelineTests.Components
{
    class ColumnSwapperTests:DatabaseTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void TestColumnSwapper_NormalUseCase(bool keepInputColumnToo)
        {
            var dt = new DataTable();
            dt.Columns.Add("In");
            dt.Columns.Add("Out");

            dt.Rows.Add("A", 1);
            dt.Rows.Add("B", 2);
            dt.Rows.Add("C", 3);
            dt.Rows.Add("D", 4);
            dt.Rows.Add("D", 5); //oh dear D maps to 2 out values that's a violation! but if we don't see a D it doesn't matter

            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            TableInfo map;
            ColumnInfo[] mapCols;

            Import(db.CreateTable("Map", dt),out map,out mapCols);

            var swapper = new ColumnSwapper();
            swapper.MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In"));
            swapper.MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out"));
            swapper.KeepInputColumnToo = keepInputColumnToo;

            swapper.Check(new ThrowImmediatelyCheckNotifier());
            
            var dtToSwap = new DataTable();

            dtToSwap.Columns.Add("In");
            dtToSwap.Columns.Add("Name");
            dtToSwap.Columns.Add("Age");

            dtToSwap.Rows.Add("A", "Dave", 30);
            dtToSwap.Rows.Add("A", "Dave", 30);
            dtToSwap.Rows.Add("B", "Frank", 50);

            var resultDt = swapper.ProcessPipelineData(dtToSwap, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            //in should be there or not depending on the setting KeepInputColumnToo
            Assert.AreEqual(keepInputColumnToo, resultDt.Columns.Contains("In"));

            AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
            Assert.AreEqual("Dave", resultDt.Rows[0]["Name"]);

            AreBasicallyEquals(1, resultDt.Rows[1]["Out"]);
            Assert.AreEqual("Dave", resultDt.Rows[1]["Name"]);

            AreBasicallyEquals(2, resultDt.Rows[2]["Out"]);
            Assert.AreEqual("Frank", resultDt.Rows[2]["Name"]);

            if (keepInputColumnToo)
            {
                Assert.AreEqual("A", resultDt.Rows[0]["In"]);
                Assert.AreEqual("A", resultDt.Rows[1]["In"]);
                Assert.AreEqual("B", resultDt.Rows[2]["In"]);
            }
        }

        [TestCase(AliasResolutionStrategy.CrashIfAliasesFound)]
        [TestCase(AliasResolutionStrategy.MultiplyInputDataRowsByAliases)]
        public void TestColumnSwapper_Aliases(AliasResolutionStrategy strategy)
        {
            var dt = new DataTable();
            dt.Columns.Add("In");
            dt.Columns.Add("Out");

            dt.Rows.Add("A", 1);
            dt.Rows.Add("B", 2);
            dt.Rows.Add("C", 3);
            dt.Rows.Add("D", 4);
            dt.Rows.Add("D", 5); //oh dear D maps to 2 out values that's a violation! but if we don't see a D it doesn't matter

            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            TableInfo map;
            ColumnInfo[] mapCols;

            Import(db.CreateTable("Map", dt), out map, out mapCols);

            var swapper = new ColumnSwapper();
            swapper.MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In"));
            swapper.MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out"));
            swapper.AliasResolutionStrategy = strategy;

            swapper.Check(new ThrowImmediatelyCheckNotifier());

            var dtToSwap = new DataTable();

            dtToSwap.Columns.Add("In");
            dtToSwap.Columns.Add("Name");
            dtToSwap.Columns.Add("Age");

            dtToSwap.Rows.Add("A", "Dave", 30);
            dtToSwap.Rows.Add("D", "Dandy", 60);

            switch (strategy)
            {
                case AliasResolutionStrategy.CrashIfAliasesFound:
                    Assert.Throws<AliasException>(()=>swapper.ProcessPipelineData(dtToSwap, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken()));
                    break;
                case AliasResolutionStrategy.MultiplyInputDataRowsByAliases:

                    var resultDt = swapper.ProcessPipelineData(dtToSwap, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

                    AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
                    Assert.AreEqual("Dave", resultDt.Rows[0]["Name"]);
                    
                    //we get the first alias (4)
                    AreBasicallyEquals(4, resultDt.Rows[1]["Out"]);
                    Assert.AreEqual("Dandy", resultDt.Rows[1]["Name"]);
                    AreBasicallyEquals(60, resultDt.Rows[1]["Age"]);

                    //and the second alias (5)
                    AreBasicallyEquals(5, resultDt.Rows[2]["Out"]);
                    Assert.AreEqual("Dandy", resultDt.Rows[2]["Name"]);
                    AreBasicallyEquals(60, resultDt.Rows[1]["Age"]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("strategy");
            }
            
        }
    }
}
