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


        public int CacheProgress_ID
        {
            get { return _cacheProgressID; }
            set { SetField(ref  _cacheProgressID, value); }
        }

        public DateTime FetchRequestStart
        {
            get { return _fetchRequestStart; }
            set { SetField(ref  _fetchRequestStart, value); }
        }

        public DateTime FetchRequestEnd
        {
            get { return _fetchRequestEnd; }
            set { SetField(ref  _fetchRequestEnd, value); }
        }

        public string ExceptionText
        {
            get { return _exceptionText; }
            set { SetField(ref  _exceptionText, value); }
        }

        public DateTime LastAttempt
        {
            get { return _lastAttempt; }
            set { SetField(ref  _lastAttempt, value); }
        }

        public DateTime? ResolvedOn
        {
            get { return _resolvedOn; }
            set { SetField(ref  _resolvedOn, value); }
        }

        #endregion
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


        public void Resolve()
        {
            ResolvedOn = DateTime.Now;
            SaveToDatabase();
        }
    }
}