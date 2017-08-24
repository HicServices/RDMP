using System;
using System.IO;
using CachingEngine.Requests;

namespace RDMPAutomationServiceTests.AutomationLoopTests.FictionalCache
{
    public class TestDataWritterChunk : ICacheChunk
    {
        public FileInfo[] Files { get; set; }
        public ICacheFetchRequest Request { get; private set; }

        public TestDataWritterChunk(ICacheFetchRequest request, FileInfo[] files)
        {
            Request = request;
            Files = files;
        }

        
    }
}