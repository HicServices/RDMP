using System.Collections.Generic;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface for defining that a given class is dependent or operates on one or more LoadProgress.  This is used when you declare a [DemandsInitialization] property on
    /// a plugin component of type ILoadProgress to determine which instances to offer at design time (i.e. only show LoadProgresses that are associated with the load you are
    /// editing).
    /// </summary>
    public interface ILoadProgressHost
    {
        IEnumerable<ILoadProgress> GetLoadProgresses();
    }
}