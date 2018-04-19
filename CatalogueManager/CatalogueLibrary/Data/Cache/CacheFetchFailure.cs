using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Cache
{
    /// <summary>
    /// Describes a failed attempt to contact a caching service including the time it occurred and any associated Exception as well as whether it has been 
    /// resolved.  Any object of type ICacheFetchRequest (with paired Exception) can be used to create a failure record.
    /// </summary>
    public class CacheFetchFailure : DatabaseEntity, ICacheFetchFailure
    {
        #region Database Properties

        private int _cacheProgressID;
        private DateTime _fetchRequestStart;
        private DateTime _fetchRequestEnd;
        private string _exceptionText;
        private DateTime _lastAttempt;
        private DateTime? _resolvedOn;


        /// <summary>
        /// The ID of the <see cref="CacheProgress"/> that was being executed when the fetch error occured.
        /// </summary>
        public int CacheProgress_ID
        {
            get { return _cacheProgressID; }
            set { SetField(ref  _cacheProgressID, value); }
        }

        /// <summary>
        /// The time in 'dataset time' for which the request errored.  For example if the cache fetch request was for 10:00am - 11:00am on 2001-01-01 then the <see cref="FetchRequestStart"/>
        /// would be 10:00 2001-01-01 and the <see cref="FetchRequestEnd"/> would be 11:00 2001-01-01.  This has no bearing on the time the process was running at or the time
        /// it errored, it is the period of dataset time that we were attempting to fetch from the remote endpoint
        /// </summary>
        public DateTime FetchRequestStart
        {
            get { return _fetchRequestStart; }
            set { SetField(ref  _fetchRequestStart, value); }
        }

        /// <inheritdoc cref="FetchRequestStart"/>
        public DateTime FetchRequestEnd
        {
            get { return _fetchRequestEnd; }
            set { SetField(ref  _fetchRequestEnd, value); }
        }

        /// <summary>
        /// The Exception resulted from the cache fetch request which documents what went wrong (e.g. 404 file not found, invalid credentials etc)
        /// </summary>
        public string ExceptionText
        {
            get { return _exceptionText; }
            set { SetField(ref  _exceptionText, value); }
        }

        /// <summary>
        /// The realtime date that this request was last attempted
        /// </summary>
        public DateTime LastAttempt
        {
            get { return _lastAttempt; }
            set { SetField(ref  _lastAttempt, value); }
        }

        /// <summary>
        /// The date at which we were able to make a succesful request for the time period defined by <see cref="FetchRequestStart"/> and <see cref="FetchRequestEnd"/>.  If this
        /// date is populated then it means that although we were unable to fetch the period when we first requested it we were subsequently able to rerun that period and the 
        /// remote endpoint was succesfully able to return to us the results
        /// </summary>
        public DateTime? ResolvedOn
        {
            get { return _resolvedOn; }
            set { SetField(ref  _resolvedOn, value); }
        }

        #endregion

        /// <summary>
        /// Documents that a given cache fetch request was not succesfully executed e.g. the remote endpoint returned an error for that date range.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="cacheProgress"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="e"></param>
        public CacheFetchFailure(ICatalogueRepository repository, ICacheProgress cacheProgress, DateTime start, DateTime end, Exception e)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"CacheProgress_ID", cacheProgress.ID},
                {"FetchRequestStart", start},
                {"FetchRequestEnd", end},
                {"ExceptionText", ExceptionHelper.ExceptionToListOfInnerMessages(e,true)},
                {"LastAttempt", DateTime.Now},
                {"ResolvedOn", DBNull.Value}
            });
        }

        internal CacheFetchFailure(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            CacheProgress_ID = int.Parse(r["CacheProgress_ID"].ToString());
            FetchRequestStart = (DateTime)r["FetchRequestStart"];
            FetchRequestEnd = (DateTime)r["FetchRequestEnd"];
            ExceptionText = r["ExceptionText"].ToString();
            LastAttempt = (DateTime)r["LastAttempt"];
            ResolvedOn = ObjectToNullableDateTime(r["ResolvedOn"]);
        }

        /// <summary>
        /// Marks that we were able to succesfully rerun this request window
        /// </summary>
        /// <seealso cref="ResolvedOn"/>
        public void Resolve()
        {
            ResolvedOn = DateTime.Now;
            SaveToDatabase();
        }
    }
}