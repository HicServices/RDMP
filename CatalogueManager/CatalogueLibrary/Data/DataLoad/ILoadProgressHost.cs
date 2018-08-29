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
        /// <summary>
        /// Data loads can be either one offs (e.g. load all csv files in ForLoading) or iterative (load all data from the cache between 2001-01-01 and 2002-01-01).
        /// If a data load is iterative then it will have one or more <see cref="ILoadProgress"/> which describe how far through the loading process it is.
        /// </summary>
        ILoadProgress[] LoadProgresses { get; }
    }
}