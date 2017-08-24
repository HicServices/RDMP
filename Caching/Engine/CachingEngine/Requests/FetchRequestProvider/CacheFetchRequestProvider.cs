using CatalogueLibrary.Data.Cache;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests.FetchRequestProvider
{
    public class CacheFetchRequestProvider : ICacheFetchRequestProvider
    {
        private readonly ICacheFetchRequest _initialRequest;
        public ICacheFetchRequest Current { get; private set; }

        public CacheFetchRequestProvider(ICacheFetchRequest initialRequest)
        {
            Current = null;
            _initialRequest = initialRequest;
        }

        public ICacheFetchRequest GetNext(IDataLoadEventListener listener)
        {
            Current = Current == null ? _initialRequest : CreateNext();
            return Current;
        }

        private ICacheFetchRequest CreateNext()
        {
            var nextStart = Current.Start.Add(Current.ChunkPeriod);
            return new CacheFetchRequest(_initialRequest.Repository,nextStart)
            {
                CacheProgress = Current.CacheProgress,
                PermissionWindow = Current.PermissionWindow,
                ChunkPeriod = Current.ChunkPeriod
            };
        }
    }
}