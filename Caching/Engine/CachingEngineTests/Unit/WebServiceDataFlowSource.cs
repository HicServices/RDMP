using System;
using System.Collections.Generic;
using System.IO;
using ReusableLibraryCode.Progress;

namespace CachingEngineTests.Unit
{
    public class WebServiceDataFlowSource : IFileDataFlowSource
    {
        public IEnumerable<FileInfo> GetChunk(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public void Dispose(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileInfo> TryGetPreview()
        {
            throw new NotImplementedException();
        }
    }
}