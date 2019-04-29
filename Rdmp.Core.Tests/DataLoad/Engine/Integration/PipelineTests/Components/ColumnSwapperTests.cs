// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases.Exceptions;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Swapping;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Components
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

        [TestCase(true)]
        [TestCase(false)]
        public void TestColumnSwapper_MissingMappings(bool crashIfNoMappingsFound)
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
            swapper.CrashIfNoMappingsFound = crashIfNoMappingsFound;
            swapper.WHERELogic = swapper.MappingToColumn.GetFullyQualifiedName() + " < 2"; //throws out all rows but A

            swapper.Check(new ThrowImmediatelyCheckNotifier());

            var dtToSwap = new DataTable();

            dtToSwap.Columns.Add("In");
            dtToSwap.Columns.Add("Name");
            dtToSwap.Columns.Add("Age");

            dtToSwap.Rows.Add("A", "Dave", 30);
            dtToSwap.Rows.Add("B", "Frank", 50);

            if(crashIfNoMappingsFound)
                Assert.Throws<KeyNotFoundException>(() => swapper.ProcessPipelineData(dtToSwap, new ThrowImmediatelyDataLoadEventListener(), null));
            else
            {
                var resultDt = swapper.ProcessPipelineData(dtToSwap, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

                Assert.AreEqual(1, resultDt.Rows.Count);
                AreBasicallyEquals(1, resultDt.Rows[0]["Out"]);
                Assert.AreEqual("Dave", resultDt.Rows[0]["Name"]);
            }

        }

    }
}
