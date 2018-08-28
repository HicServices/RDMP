using System;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Cache
{
    /// <summary>
    /// See CacheFetchFailure
    /// </summary>
    public interface ICacheFetchFailure : ISaveable, IDeleteable,IMapsDirectlyToDatabaseTable
    {
        /// <summary>
        /// The ID of the <see cref="CacheProgress"/> that was being executed when the fetch error occured.
        /// </summary>
        int CacheProgress_ID { get; set; }

        /// <summary>
        /// The time in 'dataset time' for which the request errored.  For example if the cache fetch request was for 10:00am - 11:00am on 2001-01-01 then the <see cref="FetchRequestStart"/>
        /// would be 10:00 2001-01-01 and the <see cref="FetchRequestEnd"/> would be 11:00 2001-01-01.  This has no bearing on the time the process was running at or the time
        /// it errored, it is the period of dataset time that we were attempting to fetch from the remote endpoint
        /// </summary>
        DateTime FetchRequestStart { get; set; }

        /// <inheritdoc cref="FetchRequestStart"/>
        DateTime FetchRequestEnd { get; set; }

        /// <summary>
        /// The Exception resulted from the cache fetch request which documents what went wrong (e.g. 404 file not found, invalid credentials etc)
        /// </summary>
        string ExceptionText { get; set; }

        /// <summary>
        /// The realtime date that this request was last attempted
        /// </summary>
        DateTime LastAttempt { get; set; }

        /// <summary>
        /// The date at which we were able to make a succesful request for the time period defined by <see cref="FetchRequestStart"/> and <see cref="FetchRequestEnd"/>.  If this
        /// date is populated then it means that although we were unable to fetch the period when we first requested it we were subsequently able to rerun that period and the 
        /// remote endpoint was succesfully able to return to us the results
        /// </summary>
        DateTime? ResolvedOn { get; set; }

        /// <summary>
        /// Marks that we were able to succesfully rerun this request window
        /// </summary>
        /// <seealso cref="ResolvedOn"/>
        void Resolve();
    }
}