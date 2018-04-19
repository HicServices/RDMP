using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Linq.SqlClient;
using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// This class is used by the DLE windows service and describes how often to run a given LoadMetadata configuration.  For example you might load 'Hospital Admissions Data' once
    /// every month on the 1st day of each month.  
    /// 
    /// <para>The class records when it was last loaded and can also have a 'follow on' LoadMetadata which should be launched if the main load is successful. </para>
    /// </summary>
    public class LoadPeriodically : DatabaseEntity
    {
        #region Database Properties

        private int _daysToWaitBetweenLoads;
        private DateTime? _lastLoaded;
        private int _loadMetadataID;
        private int? _onSuccessLaunchLoadMetadataID;

        public DateTime? LastLoaded
        {
            get { return _lastLoaded; }
            set { SetField(ref  _lastLoaded, value); }
        }
        
        public int DaysToWaitBetweenLoads
        {
            get { return _daysToWaitBetweenLoads; }
            set { SetField(ref _daysToWaitBetweenLoads, Math.Max(value, 1)); }
        }
        
        public int LoadMetadata_ID
        {
            get { return _loadMetadataID; }
            set { SetField(ref  _loadMetadataID, value); }
        }
        
        public int? OnSuccessLaunchLoadMetadata_ID
        {
            get { return _onSuccessLaunchLoadMetadataID; }
            set { SetField(ref  _onSuccessLaunchLoadMetadataID, value); }
        }

        #endregion

        #region Relationships

        /// <inheritdoc cref="LoadMetadata_ID"/>
        [NoMappingToDatabase]
        public LoadMetadata LoadMetadata {
            get
            {
                return Repository.GetObjectByID<LoadMetadata>(LoadMetadata_ID);
            }
        }

        [NoMappingToDatabase]
        public LoadMetadata OnSuccessLaunchLoadMetadata { get
        {
            return OnSuccessLaunchLoadMetadata_ID == null
                ? null
                : Repository.GetObjectByID<LoadMetadata>(OnSuccessLaunchLoadMetadata_ID.Value);
        }}

        #endregion

        public LoadPeriodically(ICatalogueRepository repository, LoadMetadata parent, int daysToWaitBetweenLoads)
        {
            if(daysToWaitBetweenLoads <=0)
                throw new ArgumentOutOfRangeException("daysToWaitBetweenLoads must be >= 1");

            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"LoadMetadata_ID", parent.ID},
                {"DaysToWaitBetweenLoads", daysToWaitBetweenLoads}
            });
        }

        internal LoadPeriodically(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            LoadMetadata_ID = Convert.ToInt32(r["LoadMetadata_ID"]);

            object o = r["LastLoaded"];
            if (o == DBNull.Value)
                LastLoaded = null;
            else
                LastLoaded = Convert.ToDateTime(o);

            DaysToWaitBetweenLoads = Convert.ToInt32(r["DaysToWaitBetweenLoads"]);

            o = r["OnSuccessLaunchLoadMetadata_ID"];
            if (o == DBNull.Value)
                OnSuccessLaunchLoadMetadata_ID = null;
            else
                OnSuccessLaunchLoadMetadata_ID = Convert.ToInt32(o);
        }

        public DateTime WhenIsNextLoadDue()
        {
            if (LastLoaded == null)
                return DateTime.Now;

            return LastLoaded.Value.AddDays(DaysToWaitBetweenLoads);
        }


        public bool IsLoadDue(Catalogue[] lockedCatalogues)
        {
            var allCataloguesInLoadMetadata = LoadMetadata.GetAllCatalogues().ToArray();

            if (!allCataloguesInLoadMetadata.Any())
                return false;

            if (lockedCatalogues != null && allCataloguesInLoadMetadata.Any(lockedCatalogues.Contains))
                return false;

            CheckForCircularReferences();

            //are we a link in a chain?
            if (Repository.GetAllObjects<LoadPeriodically>("WHERE OnSuccessLaunchLoadMetadata_ID=" + LoadMetadata_ID).Any()) //we are a link if there are any other load periodicals that target us on successful load
                return false;

            //There must be catalogues associated with the dataset or it cannot be run and therefore is not due
            return WhenIsNextLoadDue() <= DateTime.Now;
        }
        
        public void CheckForCircularReferences()
        {
            List<int> idsEncountered = new List<int>();
            idsEncountered.Add(LoadMetadata_ID);

            var lp = this;
            while(lp.OnSuccessLaunchLoadMetadata_ID != null)
            {
                int next = (int) lp.OnSuccessLaunchLoadMetadata_ID;
                var lmd = Repository.GetObjectByID<LoadMetadata>(next);

                if (idsEncountered.Contains(next))
                    throw new Exception("Circular reference encountered in IDs of LoadPeriodically.OnSuccessLaunchLoadMetadata_ID id's chain:" + idsEncountered.Aggregate("",(s, n) => s +","+ n));

                lp = lmd.LoadPeriodically;
                if (lp == null)
                    break; //there is no chain
            }
        }

        public override void SaveToDatabase()
        {
            CheckForCircularReferences();
            base.SaveToDatabase();
        }
    }
}
