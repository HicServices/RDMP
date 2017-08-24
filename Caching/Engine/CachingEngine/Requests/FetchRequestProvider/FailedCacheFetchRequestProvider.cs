using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Cache;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests.FetchRequestProvider
{
    public class FailedCacheFetchRequestProvider : ICacheFetchRequestProvider
    {
        public ICacheFetchRequest Current { get; private set; }
        
        private readonly ICacheProgress _cacheProgress;
        private readonly int _batchSize;

        private int _start;
        private Queue<ICacheFetchFailure> _failuresToProvide = new Queue<ICacheFetchFailure>();

        public FailedCacheFetchRequestProvider(ICacheProgress cacheProgress, int batchSize = 50)
        {
            _cacheProgress = cacheProgress;
            _batchSize = batchSize;
            
            Current = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Next CacheFetchRequest or null if there are no further request failures to process</returns>
        public ICacheFetchRequest GetNext(IDataLoadEventListener listener)
        {
            if (!_failuresToProvide.Any())
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Getting next batch of request failures from database."));
                GetNextBatchFromDatabase();
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Fetched " + _failuresToProvide.Count + " failures from database."));

                // If there are still no failures in the queue then we're done
                if (!_failuresToProvide.Any())
                    return null;
            }

            // Create a new CacheFetchRequest from the failure
            var cacheFetchFailure = _failuresToProvide.Dequeue();
            Current = new CacheFetchRequest(cacheFetchFailure, _cacheProgress);
            return Current;
        }

        private void GetNextBatchFromDatabase()
        {
            _failuresToProvide = new Queue<ICacheFetchFailure>(_cacheProgress.FetchPage(_start, _batchSize));
            _start += _batchSize;
        }
    }
}