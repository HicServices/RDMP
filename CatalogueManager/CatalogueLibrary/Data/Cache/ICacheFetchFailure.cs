using System;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Cache
{
    /// <summary>
    /// See CacheFetchFailure
    /// </summary>
    public interface ICacheFetchFailure : ISaveable, IDeleteable,IMapsDirectlyToDatabaseTable
    {
        int CacheProgress_ID { get; set; }
        DateTime FetchRequestStart { get; set; }
        DateTime FetchRequestEnd { get; set; }
        string ExceptionText { get; set; }
        DateTime LastAttempt { get; set; }
        DateTime? ResolvedOn { get; set; }
        void Resolve();
    }
}