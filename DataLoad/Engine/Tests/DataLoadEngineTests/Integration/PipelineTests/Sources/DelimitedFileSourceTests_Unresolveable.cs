using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoadModules.Generic.DataFlowSources;
using LoadModules.Generic.Exceptions;
using NUnit.Framework;

namespace DataLoadEngineTests.Integration.PipelineTests.Sources
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
                    AssertDivertFileIsExactly("Frank,\"Is the greatest,100\r\nFrank,Is the greatest,100\r\nFrank,Is the greatest,100\r\nFrank,Is the greatest,100\r\n");

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
