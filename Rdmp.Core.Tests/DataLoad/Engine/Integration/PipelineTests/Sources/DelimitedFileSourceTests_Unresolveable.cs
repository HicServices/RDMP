// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.DataLoad.Modules.DataFlowSources;
using Rdmp.Core.DataLoad.Modules.Exceptions;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration.PipelineTests.Sources
{
    class DelimitedFileSourceTests_Unresolveable: DelimitedFileSourceTestsBase
    {

        [TestCase(BadDataHandlingStrategy.DivertRows)]
        [TestCase(BadDataHandlingStrategy.ThrowException)]
        [TestCase(BadDataHandlingStrategy.IgnoreRows)]
        public void BadCSV_UnclosedQuote(BadDataHandlingStrategy strategy)
        {
            var file = CreateTestFile(
                "Name,Description,Age",
                "Frank,\"Is, the greatest\",100", //<---- how you should be doing it
                "Frank,Is the greatest,100",
                "Frank,\"Is the greatest,100", //<----- no closing quote! i.e. read the rest of the file!
                "Frank,Is the greatest,100",
                "Frank,Is the greatest,100",
                "Frank,Is the greatest,100",
                "Frank,Is the greatest,100");

            Action<DelimitedFlatFileDataFlowSource> adjust = (a) =>
            {
                a.BadDataHandlingStrategy = strategy;
                a.ThrowOnEmptyFiles = true;
                a.IgnoreQuotes = false;
            };

            switch (strategy)
            {
                case BadDataHandlingStrategy.ThrowException:
                    var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, adjust));
                    Assert.AreEqual("Bad data found on line 9", ex.Message);
                    break;
                case BadDataHandlingStrategy.IgnoreRows:
                    var dt = RunGetChunk(file, adjust);
                    Assert.AreEqual(2, dt.Rows.Count);  //reads first 2 rows and chucks the rest!
                    break;
                case BadDataHandlingStrategy.DivertRows:

                    //read 2 rows and rejected the rest
                    var dt2 = RunGetChunk(file, adjust);
                    Assert.AreEqual(2, dt2.Rows.Count);
                    AssertDivertFileIsExactly($"Frank,\"Is the greatest,100{Environment.NewLine}Frank,Is the greatest,100{Environment.NewLine}Frank,Is the greatest,100{Environment.NewLine}Frank,Is the greatest,100{Environment.NewLine}Frank,Is the greatest,100{Environment.NewLine}");

                    break;
                default:
                    throw new ArgumentOutOfRangeException("strategy");
            }
        }
        
        [Test]
        public void BadCSV_UnclosedQuote_IgnoreQuotes()
        {
            var file = CreateTestFile(
                "Name,Description,Age",
                "Frank,Is the greatest,100",
                "Frank,\"Is the greatest,100",
                "Frank,Is the greatest,100",
                "Frank,Is the greatest,100",
                "Frank,Is the greatest,100");

            Action<DelimitedFlatFileDataFlowSource> adjust = (a) =>
            {
                a.BadDataHandlingStrategy = BadDataHandlingStrategy.ThrowException;
                a.ThrowOnEmptyFiles = true;
                a.IgnoreQuotes = true;
            };

            var dt2 = RunGetChunk(file, adjust);
            Assert.AreEqual(5, dt2.Rows.Count);
            Assert.AreEqual("\"Is the greatest", dt2.Rows[1]["Description"]);
        }
    }
}
