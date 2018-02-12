using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Automation
{
    /// <summary>
    /// Records realtime information about an executing task running in the RDMPAutomationService.  This could be a Data Quality Evaluation run or a Data Load Engine run or caching etc.  During normal
    /// execution an AutomationJob record will only exist so long as the task is still running in the Automation executable.  Once the task is completed the AutomationJob record is deleted.  However
    /// if an AutomationJob crashes or the user issues a remote cancel request (see CancelRequested field) then the AutomationJob record will remain (which will prevent it executing again and depending
    /// on the AutomationFailureStrategy may stop other new loads / dqe evaluations from starting).
    /// </summary>
    public class AutomationJob : DatabaseEntity, ILifelineable
    {
        #region Database Properties

        private int _automationServiceSlotID;
        private AutomationJobType _automationJobType;
        private AutomationJobStatus _lastKnownStatus;
        private DateTime? _lifeline;
        private int? _dataLoadRunID;
        private int? _loggingServerID;
        private bool _cancelRequested;
        private string _description;


        public int AutomationServiceSlot_ID
        {
            get { return _automationServiceSlotID; }
            set { SetField(ref _automationServiceSlotID , value); }
        }

        public AutomationJobType AutomationJobType
        {
            get { return _automationJobType; }
            set { SetField(ref _automationJobType , value); }
        }

        public AutomationJobStatus LastKnownStatus
        {
            get { return _lastKnownStatus; }
            set { SetField(ref _lastKnownStatus, value); }
        }

        public DateTime? Lifeline
        {
            get { return _lifeline; }
            set { SetField(ref _lifeline, value); }
        }

        public int? DataLoadRunID
        {
            get { return _dataLoadRunID; }
            set { SetField(ref _dataLoadRunID, value); }
        }

        public int? LoggingServer_ID
        {
            get { return _loggingServerID; }
            set { SetField(ref _loggingServerID , value); }
        }

        public bool CancelRequested
        {
            get { return _cancelRequested; }
            set { SetField(ref _cancelRequested, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref _description , value); }
        }

        #endregion

        public AutomationJob(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            AutomationServiceSlot_ID = Convert.ToInt32(r["AutomationServiceSlot_ID"]);

            AutomationJobType type;
            AutomationJobType.TryParse(r["AutomationJobType"].ToString(),out type);
            AutomationJobType = type;

            AutomationJobStatus jobStatus;
            AutomationJobStatus.TryParse(r["LastKnownStatus"].ToString(),out jobStatus);
            LastKnownStatus = jobStatus;

            Lifeline = ObjectToNullableDateTime(r["Lifeline"]);
            DataLoadRunID = ObjectToNullableInt(r["DataLoadRunID"]);
            LoggingServer_ID = ObjectToNullableInt(r["LoggingServer_ID"]);

            CancelRequested = Convert.ToBoolean(r["CancelRequested"]);

            Description = r["Description"].ToString();
        }

        public AutomationJob(ICatalogueRepository repository, AutomationServiceSlot parent, AutomationJobType jobType, string description)
        {
            Repository = repository;
            
            Repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"AutomationServiceSlot_ID", parent.ID},
                {"AutomationJobType", jobType.ToString()},
                {"Description",description}
            });
        }

        public AutomationJob(ICatalogueRepository repository, AutomationServiceSlot parent, CacheProgress cacheProgress)
            : this(repository, parent, AutomationJobType.Cache, GetCacheJobNameFor(cacheProgress))
        {
            
        }
        public AutomationJob(ICatalogueRepository repository, AutomationServiceSlot parent, PermissionWindow cachePermissionWindow)
            : this(repository, parent, AutomationJobType.Cache, GetCacheJobNameFor(cachePermissionWindow))
        {

        }

        public void TickLifeline()
        {
            ((CatalogueRepository)Repository).TickLifeline(this);
        }

        public void RefreshLifelinePropertyFromDatabase()
        {
            Lifeline = ((CatalogueRepository) Repository).GetTickLifeline(this);
        }

        public void LockCatalogues(Catalogue[] cataloguesToLock)
        {
            foreach (Catalogue c in cataloguesToLock)
                ((CatalogueRepository) Repository).Insert(
                    "INSERT INTO AutomationLockedCatalogues (AutomationJob_ID,Catalogue_ID) VALUES (" + ID + "," + c.ID +
                    ")", new Dictionary<string, object>());
        }

        public Catalogue[] GetLockedCatalogues()
        {
            return ((CatalogueRepository) Repository).SelectAll<Catalogue>("SELECT * FROM AutomationLockedCatalogues WHERE AutomationJob_ID=" + ID, "Catalogue_ID").ToArray();
        }

        public void SetLastKnownStatus(AutomationJobStatus newStatus)
        {
            ((CatalogueRepository)Repository).SaveSpecificPropertyOnlyToDatabase(this,"LastKnownStatus",newStatus);
        }

        public void IssueCancelRequest()
        {
            ((CatalogueRepository)Repository).SaveSpecificPropertyOnlyToDatabase(this, "CancelRequested", true);
        }

        public override string ToString()
        {
            return Description;
        }

        public void SetLoggingInfo(IExternalDatabaseServer serverChosen, int dataLoadRunID)
        {
            ((CatalogueRepository) Repository).SaveSpecificPropertyOnlyToDatabase(this, "LoggingServer_ID",serverChosen.ID);
            ((CatalogueRepository) Repository).SaveSpecificPropertyOnlyToDatabase(this, "DataLoadRunID",dataLoadRunID);
        }


        public const string CacheProgressJobDescriptionPrefix = "Cache Progress ";
        public const string CachePermissionWindowJobDescriptionPrefix = "Cache PermissionWindow ";

        private static string GetCacheJobNameFor(CacheProgress cacheProgress)
        {
            return CacheProgressJobDescriptionPrefix + cacheProgress.ID;
        }

        private static string GetCacheJobNameFor(PermissionWindow permissionWindow)
        {
            return CachePermissionWindowJobDescriptionPrefix + permissionWindow.ID;
        }

        public CacheProgress GetCachingJobsProgressObjectIfAny()
        {
            if(AutomationJobType != AutomationJobType.Cache)
                throw new NotSupportedException("You can only call this method on AutomationJobType.Cache, this job is a " + AutomationJobType);

            if (Description.StartsWith(CacheProgressJobDescriptionPrefix))
                return
                    Repository.GetObjectByID<CacheProgress>(
                        int.Parse(Description.Substring(CacheProgressJobDescriptionPrefix.Length)));

            return null;

        }
        public PermissionWindow GetCachingJobsPermissionWindowObjectIfAny()
        {
            if (AutomationJobType != AutomationJobType.Cache)
                throw new NotSupportedException("You can only call this method on AutomationJobType.Cache, this job is a " + AutomationJobType);

            if (Description.StartsWith(CachePermissionWindowJobDescriptionPrefix))
                return
                    Repository.GetObjectByID<PermissionWindow>(
                        int.Parse(Description.Substring(CachePermissionWindowJobDescriptionPrefix.Length)));

            return null;
        }
    }

    /// <summary>
    /// The category of Pipeline responsible for creating this AutomationJob.  Either one of the 3 core automation RDMP activities or a custom plugin pipleine.
    /// </summary>
    public enum AutomationJobType
    {
        DQE,
        DLE,
        Cache,
        UserCustomPipeline
    }


    /// <summary>
    /// The last known state of the AutomationJob on an AutomationServiceSlot run by an automation server.
    /// </summary>
    public enum AutomationJobStatus
    {
        NotYetStarted,
        Running,
        Cancelled,
        Crashed,
        Finished
    }
}
