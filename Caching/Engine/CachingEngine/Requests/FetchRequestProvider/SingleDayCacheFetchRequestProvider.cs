using CatalogueLibrary.Data.Cache;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests.FetchRequestProvider
{
    public class SingleDayCacheFetchRequestProvider : ICacheFetchRequestProvider
    {
        private readonly ICacheFetchRequest _initialRequest;
        public ICacheFetchRequest Current { get; private set; }

        public SingleDayCacheFetchRequestProvider(ICacheFetchRequest initialRequest)
        {
            Current = null;
            _initialRequest = initialRequest;
        }

        public ICacheFetchRequest GetNext(IDataLoadEventListener listener)
        {
            // If we haven't provided one, give out _initialRequest
            if (Current == null)
            {
                Current = _initialRequest;
            }
            else
            {
                Current = Current.GetNext();
                // We have provided requests for more than one day
                if (Current.Start >= _initialRequest.Start.AddDays(1))
                    return null;
            }

            // Otherwise we have provided our request so signal there is none left
            return Current;
        }
    }
}