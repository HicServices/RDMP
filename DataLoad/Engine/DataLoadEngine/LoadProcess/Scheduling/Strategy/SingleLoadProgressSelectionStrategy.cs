using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CatalogueLibrary.Data;

namespace DataLoadEngine.LoadProcess.Scheduling.Strategy
{
    public class SingleLoadProgressSelectionStrategy : ILoadProgressSelectionStrategy
    {
        private readonly ILoadProgress _loadProgress;

        public SingleLoadProgressSelectionStrategy(ILoadProgress loadProgress)
        {
            
            _loadProgress = loadProgress;
        }

        public List<ILoadProgress> GetAllLoadProgresses(bool respectAndAcquire=true)
        {
            if (respectAndAcquire)
            {
                // todo: refresh the object?

                //if we respect your lock and it is locked already
                if (_loadProgress.LockedBecauseRunning)
                    throw new Exception("Load Progress '" + _loadProgress.Name + "' (" + _loadProgress.ID + ") has been locked by another process in the time between construction and use");//complain

                // acquire lock
                _loadProgress.Lock();
            }

            //here are the load progresses that exist
            return new List<ILoadProgress> { _loadProgress };
        }
    }
}