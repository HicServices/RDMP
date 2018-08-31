using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <inheritdoc cref="ILoadProgress"/>
    public class LoadProgress : VersionedDatabaseEntity, ILoadProgress
    {
        #region Database Properties
        private bool _isDisabled;
        private string _name;
        private DateTime? _originDate;
        private string _loadPeriodicity;
        private DateTime? _dataLoadProgress;
        private int _loadMetadata_ID;
        private int _defaultNumberOfDaysToLoadEachTime;

        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public DateTime? DataLoadProgress
        {
            get { return _dataLoadProgress; }
            set { SetField(ref _dataLoadProgress, value); }
        }
        /// <inheritdoc/>
        public int LoadMetadata_ID
        {
            get { return _loadMetadata_ID; }
            set { SetField(ref _loadMetadata_ID, value); }
        }

        /// <inheritdoc/>
        public int DefaultNumberOfDaysToLoadEachTime
        {
            get { return _defaultNumberOfDaysToLoadEachTime; }
            set { SetField(ref _defaultNumberOfDaysToLoadEachTime, value); }
        }

        #endregion
        #region Relationships
        /// <inheritdoc/>
        [NoMappingToDatabase]
        public ILoadMetadata LoadMetadata { get { return Repository.GetObjectByID<LoadMetadata>(LoadMetadata_ID); }}

        /// <inheritdoc/>
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
            LoadPeriodicity = r["LoadPeriodicity"].ToString();
            IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
            DefaultNumberOfDaysToLoadEachTime = Convert.ToInt32(r["DefaultNumberOfDaysToLoadEachTime"]);
        }
        
        public override string ToString()
        {
            return Name + " ID=" + ID;
        }
    }
}
