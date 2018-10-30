using NUnit.Framework;

namespace DataLoadEngineTests.Integration.PipelineTests.Sources
{
    public class DelimitedFileSourceTests_AutomaticallyResolved : DelimitedFileSourceTestsBase
    {
        [Test]
        public void NewLineInFile_Ignored()
        {
            var file = CreateTestFile(
                "Name,Dob",
                "Frank,2001-01-01",
                "",
                "Herbert,2002-01-01"
                );

            var dt = RunGetChunk(file);
            Assert.AreEqual(2,dt.Rows.Count);
            Assert.AreEqual("Frank", dt.Rows[0]["Name"]);
            Assert.AreEqual("Herbert", dt.Rows[1]["Name"]);
        }

        [Test]
        public void NewLineInFile_RespectedWhenQuoted()
        {
            var file = CreateTestFile(
                @"Name,Dob,Description
Frank,2001-01-01,""Frank is

the best ever""
Herbert,2002-01-01,Hey"
                );

            var dt = RunGetChunk(file);
            Assert.AreEqual(2,dt.Rows.Count);
            Assert.AreEqual("Frank", dt.Rows[0]["Name"]);
            Assert.AreEqual(@"Frank is

the best ever", dt.Rows[0]["Description"]);
            Assert.AreEqual("Herbert", dt.Rows[1]["Name"]);
        
        }

    }
}