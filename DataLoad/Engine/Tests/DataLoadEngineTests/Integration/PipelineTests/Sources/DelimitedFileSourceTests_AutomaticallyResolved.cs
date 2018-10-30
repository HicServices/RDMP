using System;
using System.IO;
using System.Text;
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

        [TestCase("")]
        [TestCase("     ")]
        [TestCase("\"  \"")]
        [TestCase("null")]
        [TestCase("NULL ")]
        public void NullCellValues_ToDbNull(string nullstring)
        {
            var file = CreateTestFile(
                "Name,Dob",
                string.Format("{0},2001-01-01",nullstring),
                "",
                string.Format("Herbert ,{0}",nullstring)
                );

            var dt = RunGetChunk(file);
            Assert.AreEqual(2, dt.Rows.Count);
            Assert.AreEqual(DBNull.Value, dt.Rows[0]["Name"]);
            Assert.AreEqual("Herbert", dt.Rows[1]["Name"]);
            Assert.AreEqual(DBNull.Value, dt.Rows[1]["Dob"]);
        }


        [TestCase(",,")]
        [TestCase("NULL,NULL,NULL")]
        [TestCase("NULL,,null")]
        [TestCase("NULL , null ,")]
        public void TrailingNulls_InRows(string nullSuffix)
        {
            var file = CreateTestFile(
            "CHI,StudyID,Date",
            "0101010101,5,2001-01-05",
            "0101010101,5,2001-01-05",
            "0101010101,5,2001-01-05" + nullSuffix, //Row has trailing nulls in it which get ignored
            "0101010101,5,2001-01-05");

            var dt = RunGetChunk(file);
            Assert.AreEqual(4, dt.Rows.Count);
            Assert.AreEqual(3, dt.Columns.Count);
        }
        [Test]
        public void TrailingNulls_InHeader()
        {
            var file = CreateTestFile(
                "CHI ,StudyID,Date,,",
                //Row has trailing null headers, these get given values Column1, Column2 etc incase later in the file we find values for them
                "0101010101,5,2001-01-05",
                "0101010101,5,2001-01-05",
                "0101010101,5,2001-01-05",
                "0101010101,5,2001-01-05");
            
            var dt = RunGetChunk(file);
            Assert.IsNotNull(dt);
            Assert.AreEqual(4, dt.Rows.Count);
            Assert.AreEqual(3, dt.Columns.Count);
            Assert.AreEqual("CHI", dt.Columns[0].ColumnName);
            Assert.AreEqual("StudyID", dt.Columns[1].ColumnName);
            Assert.AreEqual("Date", dt.Columns[2].ColumnName);
        }
    }
}