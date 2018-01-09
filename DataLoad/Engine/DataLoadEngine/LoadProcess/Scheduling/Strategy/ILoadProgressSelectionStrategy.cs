using System.Collections.Generic;
using CatalogueLibrary.Data;

namespace DataLoadEngine.LoadProcess.Scheduling.Strategy
{
    /// <summary>
    /// Decides which LoadProgress (if any) to advance in a ScheduledDataLoadProcess.  This should respect locks (where the LoadProgress is already executing 
    /// elsewhere) and current progress date (if LoadProgress is already up-to-date it shouldn't be run).
    /// </summary>
    public interface ILoadProgressSelectionStrategy
    {
        /// <summary>
        /// Return a list of the runnable load progresses, if respectAndAcquire is true you should Lock all the LoadProgresses that you suggest so nobody
        /// else can use them (e.g. if another user tries to run the same load).
        /// </summary>
        /// <param name="respectAndAcquire"></param>
        /// <returns></returns>
        List<ILoadProgress> GetAllLoadProgresses(bool respectAndAcquire = true);
    }
}