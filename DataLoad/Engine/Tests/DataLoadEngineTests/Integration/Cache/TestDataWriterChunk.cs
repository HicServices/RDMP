using System.IO;
using CachingEngine.Requests;

namespace DataLoadEngineTests.Integration.Cache
{
    public class TestDataWriterChunk : ICacheChunk
    {
        public FileInfo[] Files { get; set; }
        public ICacheFetchRequest Request { get; private set; }

        public TestDataWriterChunk(ICacheFetchRequest request, FileInfo[] files)
        {
            Request = request;
            Files = files;
        }

        
    }
}