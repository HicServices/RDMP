using CatalogueLibrary.Data.Cache;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests.FetchRequestProvider
{
    /// <summary>
    /// Interface for classes that make descisions about which time periods to request and in what order when caching.
    /// </summary>
    public interface ICacheFetchRequestProvider
    {
        ICacheFetchRequest Current { get; }
        ICacheFetchRequest GetNext(IDataLoadEventListener listener);
    }
}