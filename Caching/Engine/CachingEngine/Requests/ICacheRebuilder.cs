using System;
using System.Threading;
using System.Threading.Tasks;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests
{
    /// <summary>
    /// Interface for attempting to rebuild the .\Data\Cache (or alternative) Cache directory based on files currently in the ForArchiving directory.
    /// </summary>
    [Obsolete("Not tied into any UI or engine classes.  Plugins may still implement this but it will never be called")]
    public interface ICacheRebuilder
    {
        Task RebuildCacheFromArchiveFiles(string[] filenameList, string destinationPath, IDataLoadEventListener listener, CancellationToken token);
    }
}