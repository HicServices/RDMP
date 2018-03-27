using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Cache
{
    /// <summary>
    /// Records the progress of fetching and caching data from a remote source e.g. a Webservice or Imaging file host.  Each CacheProgress
    /// is tied to a LoadProgress (which itself is tied to a LoadMetadata).  
    /// </summary>
    public class CacheProgress : VersionedDatabaseEntity, ICacheProgress
    {
        #region Database Properties

        private int _loadProgressID;
        private int? _permissionWindowID;
        private DateTime? _cacheFillProgress;
        private string _cacheLagPeriod;
        private int? _pipelineID;
        private TimeSpan _chunkPeriod;
        private string _cacheLagPeriodLoadDelay;

        public int LoadProgress_ID
        {
            get { return _loadProgressID; }
            set { SetField(ref  _loadProgressID, value); }
        }

        /// <summary>
        /// When we can run the cache (e.g. evenings only).  Also serves as the locking object for preventing multiple caching engines/automation servers from executing
        /// caching activities belonging to the same window.
        /// </summary>
        public int? PermissionWindow_ID
        {
            get { return _permissionWindowID; }
            set { SetField(ref  _permissionWindowID, value); }
        }

        /// <summary>
        /// How far through the caching activity are we? files up to this date should be on disk in archives
        /// </summary>
        public DateTime? CacheFillProgress
        {
            get { return _cacheFillProgress; }
            set { SetField(ref  _cacheFillProgress, value); }
        }

        /// <summary>
        /// The period of time e.g. 1m that is never fetched expressed as an offset from DateTime.Now.  This handles the case where the cache source
        /// is not real time i.e. we expect it to take at least 1 month for data to appear on the cache endpoint so don't make requests for data that would
        /// originate very recently.  
        /// </summary>
        public string CacheLagPeriod
        {
            get { return _cacheLagPeriod; }
            set { SetField(ref  _cacheLagPeriod, value); }
        } // stored as string in DB, use GetCacheLagPeriod() to get as CacheLagPeriod

        public int? Pipeline_ID
        {
            get { return _pipelineID; }
            set { SetField(ref  _pipelineID, value); }
        }

        /// <summary>
        /// The amount of time to request at a time from the cache source e.g. (fetch 6 hours of data at a time).
        /// </summary>
        public TimeSpan ChunkPeriod
        {
            get { return _chunkPeriod; }
            set { SetField(ref  _chunkPeriod, value); }
        }

        /// <summary>
        /// Used by the automation server to decide whether to execute a CacheProgress.  This period is the minimum amount of available to fetch data that must
        /// exist before starting a caching activity.  E.g. if the cache is up to date (including the lag period offset) and 1 second passes then we don't want to 
        /// make a request for 1 second of data.  Similarly we don't want to just use the ChunkPeriod otherwise we would be executing arbitrary manner.  
        /// 
        /// This lets you say 'only execute the cache if theres at least 1 month of data (assumed) to exist on the cache source' (CacheLagPeriodLoadDelay) and don't
        /// request any data generated within the last month (CacheLagPeriod).  This means the cache will execute once a month and fetch a month of data at a time but
        /// the CacheFillProgress will always be 1-2 months behind DateTime.Now (depending on how recently the execution occured).
        /// </summary>
        public string CacheLagPeriodLoadDelay
        {
            get { return _cacheLagPeriodLoadDelay; }
            set { SetField(ref  _cacheLagPeriodLoadDelay, value); }
        } // stored as string in DB, use GetCacheLagPeriod() to get as CacheLagPeriod

        #endregion

        #region Relationships

        [NoMappingToDatabase]
        public IEnumerable<CacheFetchFailure> CacheFetchFailures {
            get { return Repository.GetAllObjectsWithParent<CacheFetchFailure>(this); }
        }

        [NoMappingToDatabase]
        public LoadProgress LoadProgress
        {
            get { return Repository.GetObjectByID<LoadProgress>(LoadProgress_ID); }
        }

        [NoMappingToDatabase]
        public IPipeline Pipeline
        {
            get { return Pipeline_ID == null ? null : Repository.GetObjectByID<Pipeline>((int)Pipeline_ID); }
        }

        [NoMappingToDatabase]
        public PermissionWindow PermissionWindow
        {
            get
            {
                return PermissionWindow_ID == null
                    ? null
                    : Repository.GetObjectByID<PermissionWindow>((int) PermissionWindow_ID);
            }
        }

        #endregion


        public CacheLagPeriod GetCacheLagPeriod()
        {
            if (string.IsNullOrWhiteSpace(CacheLagPeriod))
                return null;

            return new CacheLagPeriod(CacheLagPeriod);
        }

        public CacheLagPeriod GetCacheLagPeriodLoadDelay()
        {
            if (string.IsNullOrWhiteSpace(CacheLagPeriodLoadDelay))
                return Cache.CacheLagPeriod.Zero;

            return new CacheLagPeriod(CacheLagPeriodLoadDelay);
        }

        public void SetCacheLagPeriod(CacheLagPeriod cacheLagPeriod)
        {
            CacheLagPeriod = (cacheLagPeriod == null) ? "" : cacheLagPeriod.ToString();
        }
        public void SetCacheLagPeriodLoadDelay(CacheLagPeriod cacheLagLoadDelayPeriod)
        {
            CacheLagPeriodLoadDelay = (cacheLagLoadDelayPeriod == null) ? "" : cacheLagLoadDelayPeriod.ToString();
        }

        public ILoadProgress GetLoadProgress()
        {
            return LoadProgress;
        }

        public IPermissionWindow GetPermissionWindow()
        {
            return PermissionWindow;
        }

        public IEnumerable<ICacheFetchFailure> GetAllFetchFailures()
        {
            return CacheFetchFailures;
        }

        public CacheProgress(ICatalogueRepository repository, ILoadProgress loadProgress)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"LoadProgress_ID", loadProgress.ID}
            });
        }

        internal CacheProgress(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            LoadProgress_ID = int.Parse(r["LoadProgress_ID"].ToString());
            PermissionWindow_ID = ObjectToNullableInt(r["PermissionWindow_ID"]);
            CacheFillProgress = ObjectToNullableDateTime(r["CacheFillProgress"]);
            CacheLagPeriod = r["CacheLagPeriod"] as string;
            CacheLagPeriodLoadDelay = r["CacheLagPeriodLoadDelay"] as string;
            Pipeline_ID = ObjectToNullableInt(r["Pipeline_ID"]);
            ChunkPeriod = (TimeSpan)r["ChunkPeriod"];
        }

        public IEnumerable<ICacheFetchFailure> FetchPage(int start, int batchSize)
        {
            List<int> toReturnIds = new List<int>();

            using (var conn = ((CatalogueRepository)Repository).GetConnection())
            {
                var cmd =
                    DatabaseCommandHelper.GetCommand(@"SELECT ID FROM CacheFetchFailure 
WHERE CacheProgress_ID = @CacheProgressID AND ResolvedOn IS NULL
ORDER BY FetchRequestStart
OFFSET " + start + @" ROWS
FETCH NEXT " + batchSize + @" ROWS ONLY", conn.Connection,conn.Transaction);

                DatabaseCommandHelper.AddParameterWithValueToCommand("@CacheProgressID",cmd, ID);

                

                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        toReturnIds.Add(Convert.ToInt32(reader["ID"]));

                cmd.Dispose();
            }

            return Repository.GetAllObjectsInIDList<CacheFetchFailure>(toReturnIds); 
        }

        /// <summary>
        /// Returns the maximum amount of time that could be loaded from this cache (based on the origin date of the dataset or the max date of current data cached) and todays date (offset by lag period)
        /// e.g. if the data has been cached to 2016-01-01 and todays date is 2016-01-05 then the TimeSpan will be +4 days.  A Negative shortfall indicates that it has overcached ahead of the lag period or
        /// it has cached future data (as in data from the future!).
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetShortfall()
        {
            var lag = GetCacheLagPeriod();
            var lastExpectedCacheDate = (lag == null) ? DateTime.Today : lag.CalculateStartOfLagPeriodFrom(DateTime.Today);

            TimeSpan shortfall;
            if (CacheFillProgress != null)
                shortfall = lastExpectedCacheDate.Subtract(CacheFillProgress.Value);
            else
            {
                var load = GetLoadProgress();
                if (load.DataLoadProgress.HasValue)
                    shortfall = lastExpectedCacheDate.Subtract(load.DataLoadProgress.Value);
                else if (load.OriginDate.HasValue)
                    shortfall = lastExpectedCacheDate.Subtract(load.OriginDate.Value);
                else
                    throw new NotSupportedException("Unknown (no cache progress or load origin date)");
            }

            return shortfall;
        }

        public override string ToString()
        {
            return "Cache Progress " + ID;
        }

        /// <summary>
        /// Returns all the Catalogues in this caches LoadMetadata OR if it has a PermissionWindow then ALL the unique catalogues across ALL the LoadMetadatas of ANY cache that uses the same permission window
        /// </summary>
        /// <returns></returns>
        public Catalogue[] GetAllCataloguesMaximisingOnPermissionWindow()
        {
            if(PermissionWindow_ID == null)
                return LoadProgress.GetLoadMetadata().GetAllCatalogues().Cast<Catalogue>().Distinct().ToArray();
            
            return PermissionWindow.GetAllCacheProgresses().SelectMany(p => p.GetLoadProgress().GetLoadMetadata().GetAllCatalogues()).Cast<Catalogue>().Distinct().ToArray();
        }
    }
}