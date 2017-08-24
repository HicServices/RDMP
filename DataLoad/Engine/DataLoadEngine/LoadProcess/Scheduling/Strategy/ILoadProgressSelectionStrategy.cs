using System.Collections.Generic;
using CatalogueLibrary.Data;

namespace DataLoadEngine.LoadProcess.Scheduling.Strategy
{
    public interface ILoadProgressSelectionStrategy
    {
        List<ILoadProgress> GetAllLoadProgresses(bool respectAndAcquire = true);
    }
}