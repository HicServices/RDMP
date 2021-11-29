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
using FAnsi.Discovery;
using FAnsi.Extensions;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Aliases.Exceptions;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations.Swapping;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Components
{
    class ExtractionColumnSwapperTests:DatabaseTests
    {
        private IExtractDatasetCommand GetMockExtractDatasetCommand()
        {
            var mockPj = Mock.Of<IProject>(p =>
                p.Name == "Project Name" &&
                p.ProjectNumber == 1
            );

            var mockConfig = Mock.Of<IExtractionConfiguration>();

            var mockSelectedDatasets = Mock.Of<ISelectedDataSets>(sds =>
                sds.ExtractionConfiguration == mockConfig
            );

            var mockExtractDsCmd = Mock.Of<IExtractDatasetCommand>(d =>
                d.Project == mockPj &&
                d.SelectedDataSets == mockSelectedDatasets
            );

            return mockExtractDsCmd;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestExtractionColumnSwapper_NormalUseCase(bool keepInputColumnToo)
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
            
            Import(db.CreateTable("Map", dt),out var map,out var mapCols);

            var swapper = new ExtractionColumnSwapper();
            swapper.MappingFromColumn = mapCols.Single(c => c.GetRuntimeName().Equals("In"));
            swapper.MappingToColumn = mapCols.Single(c => c.GetRuntimeName().Equals("Out"));
            swapper.KeepInputColumnToo = keepInputColumnToo;

            swapper.PreInitialize(GetMockExtractDatasetCommand(), new ThrowImmediatelyDataLoadEventListener());
            
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

      
    }
}
