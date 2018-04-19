using System.Collections.Generic;
using System.IO;
using ReusableLibraryCode.Progress;

namespace CachingEngineTests.Unit
{
    public interface IFileDataFlowSource
    {
        IEnumerable<FileInfo> GetChunk(IDataLoadEventListener listener);
        void Dispose(IDataLoadEventListener listener);
        IEnumerable<FileInfo> TryGetPreview();
    }
}