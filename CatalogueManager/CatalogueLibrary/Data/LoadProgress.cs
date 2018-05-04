using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes the progress of a long term epic data load operation which cannot be completed in a single Data load bubble (execution of LoadMetadata through the data load engine).
    /// This entity includes start and end dates for what is trying to be loaded as well as how far through that process progess has been made up to.
    /// </summary>
    public class LoadProgress : VersionedDatabaseEntity, ILoadProgress,INamed
    {
        #region Database Properties
        private bool _isDisabled;
        private string _name;
        private DateTime? _originDate;
        private string _loadPeriodicity;
        private DateTime? _dataLoadProgress;
        private bool _lockedBecauseRunning;
        private string _lockHeldBy;
        private int _loadMetadata_ID;
        private bool _allowAutomation;
        private int _defaultNumberOfDaysToLoadEachTime;

        public bool IsDisabled
        {
            get { return _isDisabled; }
            set { SetField(ref _isDisabled, value); }
        }
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        public DateTime? OriginDate
        {
            get { return _originDate; }
            set { SetField(ref _originDate, value); }
        }
        public string LoadPeriodicity
        {
            get { return _loadPeriodicity; }
            set { SetField(ref _loadPeriodicity, value); }
        }
        public DateTime? DataLoadProgress
        {
            get { return _dataLoadProgress; }
            set { SetField(ref _dataLoadProgress, value); }
        }
        public bool LockedBecauseRunning
        {
            get { return _lockedBecauseRunning; }
            set { SetField(ref _lockedBecauseRunning, value); }
        }
        public string LockHeldBy
        {
            get { return _lockHeldBy; }
            set { SetField(ref _lockHeldBy, value); }
        }
        public int LoadMetadata_ID
        {
            get { return _loadMetadata_ID; }
            set { SetField(ref _loadMetadata_ID, value); }
        }
        public bool AllowAutomation
        {
            get { return _allowAutomation; }
            set { SetField(ref _allowAutomation, value); }
        }
        public int DefaultNumberOfDaysToLoadEachTime
        {
            get { return _defaultNumberOfDaysToLoadEachTime; }
            set { SetField(ref _defaultNumberOfDaysToLoadEachTime, value); }
        }

        #endregion
        #region Relationships
        [NoMappingToDatabase]
        public ILoadMetadata LoadMetadata { get { return Repository.GetObjectByID<LoadMetadata>(LoadMetadata_ID); }}

        [NoMappingToDatabase]
        public ICacheProgress CacheProgress
        {
            get
            {
                return Repository.GetAllObjectsWithParent<CacheProgress>(this).SingleOrDefault();
            }
        }
#endregion

        public LoadProgress(ICatalogueRepository repository, LoadMetadata parent)
        {
            repository.InsertAndHydrate(this,  
            new Dictionary<string, object>()
            {
                {"Name", Guid.NewGuid().ToString()},
                {"LoadMetadata_ID", parent.ID}
            });
        }

        internal LoadProgress(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Name = r["Name"] as string;
            OriginDate = ObjectToNullableDateTime(r["OriginDate"]);
            DataLoadProgress = ObjectToNullableDateTime(r["DataLoadProgress"]);
            LoadMetadata_ID = int.Parse(r["LoadMetaData_ID"].ToString());
            LockedBecauseRunning = (bool)r["LockedBecauseRunning"];
            LockHeldBy = r["LockHeldBy"] as string;
            LoadPeriodicity = r["LoadPeriodicity"].ToString();
            IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
            DefaultNumberOfDaysToLoadEachTime = Convert.ToInt32(r["DefaultNumberOfDaysToLoadEachTime"]);
            AllowAutomation = Convert.ToBoolean(r["AllowAutomation"]);
        }
        
        public TimeSpan GetLoadPeriodicity()
        {
            return TimeSpan.Parse(LoadPeriodicity);
        }

        public void SetLoadPeriodicity(TimeSpan period)
        {
            LoadPeriodicity = period.ToString();
        }

        public override string ToString()
        {
            return Name + " ID=" + ID;
        }

        public void Lock()
        {
            LockedBecauseRunning = true;
            LockHeldBy = Environment.UserName + " (" + Environment.MachineName + ")";
            SaveToDatabase();
        }
        
        public void Unlock()
        {
            LockedBecauseRunning = false;
            LockHeldBy = null;
            SaveToDatabase();
        }

        public void RefreshLockPropertiesFromDatabase()
        {
            ((CatalogueRepository) Repository).RefreshLockPropertiesFromDatabase(this);
        }
    }
}
