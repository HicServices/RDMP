using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;

namespace DataLoadEngine.LoadProcess.Scheduling.Strategy
{
    /// <summary>
    /// ILoadProgressSelectionStrategy in which all LoadProgresses are suggested and all are locked.
    /// </summary>
    public class AnyAvailableLoadProgressSelectionStrategy : ILoadProgressSelectionStrategy
    {
        private readonly ILoadMetadata _loadMetadata;

        public AnyAvailableLoadProgressSelectionStrategy(ILoadMetadata loadMetadata)
        {
            _loadMetadata = loadMetadata;
        }

        public List<ILoadProgress> GetAllLoadProgresses(bool respectAndAcquire = true)
        {
            var loadProgresses = _loadMetadata.LoadProgresses.Where(schedule => 
                !schedule.LockedBecauseRunning  //it is available to run
                || 
                !respectAndAcquire//we don't care just give us it
                ).ToList();

            //if we want lock
            if(respectAndAcquire)
                loadProgresses.ForEach(schedule => schedule.Lock());//lock everything

            return loadProgresses;
        }
    }
}