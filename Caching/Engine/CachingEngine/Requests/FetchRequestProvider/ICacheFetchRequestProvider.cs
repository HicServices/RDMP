using CatalogueLibrary.Data.Cache;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests.FetchRequestProvider
{
    public interface ICacheFetchRequestProvider
    {
        ICacheFetchRequest Current { get; }
        ICacheFetchRequest GetNext(IDataLoadEventListener listener);
    }
}