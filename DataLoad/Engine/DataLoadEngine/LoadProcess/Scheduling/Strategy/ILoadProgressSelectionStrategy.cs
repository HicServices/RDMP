using System.Collections.Generic;
using CatalogueLibrary.Data;

namespace DataLoadEngine.LoadProcess.Scheduling.Strategy
{
    /// <summary>
    /// Decides which LoadProgress (if any) to advance in a ScheduledDataLoadProcess.
    /// </summary>
    public interface ILoadProgressSelectionStrategy
    {
        /// <summary>
        /// Return a list of the runnable load progresses
        /// </summary>
        /// <returns></returns>
        List<ILoadProgress> GetAllLoadProgresses();
    }
}