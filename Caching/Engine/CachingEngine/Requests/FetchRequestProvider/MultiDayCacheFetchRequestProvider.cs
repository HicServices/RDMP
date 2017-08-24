using System;
using CatalogueLibrary.Data.Cache;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests.FetchRequestProvider
{
    public class MultiDayCacheFetchRequestProvider : ICacheFetchRequestProvider
    {
        private readonly ICacheFetchRequest _initialRequest;
        private readonly DateTime _endDateInclusive;
        public ICacheFetchRequest Current { get; private set; }

        public MultiDayCacheFetchRequestProvider(ICacheFetchRequest initialRequest, DateTime endDateInclusive)
        {
            Current = null;
            _initialRequest = initialRequest;
            _endDateInclusive = endDateInclusive;
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
                
                // We have provided requests for the whole time period
                if (Current.Start > _endDateInclusive)
                    return null;
            }

            return Current;
        }
    }
}