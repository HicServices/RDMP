using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CatalogueLibrary.Data;

namespace DataLoadEngine.LoadProcess.Scheduling.Strategy
{
    /// <summary>
    /// Hacky ILoadProgressSelectionStrategy in which only the specific LoadProgress in the constructor to this class is ever suggested.
    /// </summary>
    public class SingleLoadProgressSelectionStrategy : ILoadProgressSelectionStrategy
    {
        private readonly ILoadProgress _loadProgress;

        public SingleLoadProgressSelectionStrategy(ILoadProgress loadProgress)
        {
            
            _loadProgress = loadProgress;
        }

        public List<ILoadProgress> GetAllLoadProgresses()
        {
            //here are the load progresses that exist
            return new List<ILoadProgress> { _loadProgress };
        }
    }
}