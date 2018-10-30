using System;
using LoadModules.Generic.DataFlowSources;
using LoadModules.Generic.Exceptions;
using NUnit.Framework;

namespace DataLoadEngineTests.Integration.PipelineTests.Sources
{
    public class DelimitedFileSourceTests_ResolvedAccordingToStrategy : DelimitedFileSourceTestsBase
    {
        [TestCase(true)]
        [TestCase(false)]
        public void EmptyFile_TotallyEmpty(bool throwOnEmpty)
        {
            var file = CreateTestFile(); //create completely empty file

            if (throwOnEmpty)
            {
                var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, BadDataHandlingStrategy.ThrowException, throwOnEmpty));
                Assert.AreEqual("File DelimitedFileSourceTests.txt is empty", ex.Message);
            }
            else
            {
                Assert.IsNull(RunGetChunk(file, BadDataHandlingStrategy.ThrowException, throwOnEmpty));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EmptyFile_AllWhitespace(bool throwOnEmpty)
        {
            var file = CreateTestFile(@" 
     
    ");

            if(throwOnEmpty)
            {
                var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, BadDataHandlingStrategy.ThrowException, throwOnEmpty));
                Assert.AreEqual("File DelimitedFileSourceTests.txt is empty", ex.Message);
            }
            else
            {
                Assert.IsNull(RunGetChunk(file, BadDataHandlingStrategy.ThrowException,throwOnEmpty));
            }
        }


        [TestCase(true)]
        [TestCase(false)]
        public void EmptyFile_HeaderOnly(bool throwOnEmpty)
        {
            var file = CreateTestFile(@"Name,Address

 
     
    ");

            if (throwOnEmpty)
            {
                var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file, s=>s.ThrowOnEmptyFiles = true));
                Assert.AreEqual("File DelimitedFileSourceTests.txt is empty", ex.Message);
            }
            else
            {
                Assert.IsNull(RunGetChunk(file,s => s.ThrowOnEmptyFiles = false));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EmptyFile_ForceHeader(bool throwOnEmpty)
        {
            var file = CreateTestFile(@"Name,Address

 
     
    ");
            
            if (throwOnEmpty)
            {
                var ex = Assert.Throws<FlatFileLoadException>(() => RunGetChunk(file,  
                    s =>{ s.ThrowOnEmptyFiles = true; s.ForceHeaders="Name,Address"; s.ForceHeadersReplacesFirstLineInFile = true;}));
                Assert.AreEqual("File DelimitedFileSourceTests.txt is empty", ex.Message);
            }
            else
            {
                Assert.IsNull(RunGetChunk(file,
                    s =>{ s.ThrowOnEmptyFiles = false; s.ForceHeaders="Name,Address"; s.ForceHeadersReplacesFirstLineInFile = true;}));
            }
        }
    }
}