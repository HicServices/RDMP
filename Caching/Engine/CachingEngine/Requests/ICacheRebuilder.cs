using System.Threading;
using System.Threading.Tasks;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests
{
    public interface ICacheRebuilder
    {
        Task RebuildCacheFromArchiveFiles(string[] filenameList, string destinationPath, IDataLoadEventListener listener, CancellationToken token);
    }
}