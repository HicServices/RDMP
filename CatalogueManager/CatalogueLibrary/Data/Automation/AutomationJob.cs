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


        /// <summary>
        /// The <see cref="AutomationServiceSlot"/> that this job is currently running on (or crashed on).
        /// </summary>
        public int AutomationServiceSlot_ID
        {
            get { return _automationServiceSlotID; }
            set { SetField(ref _automationServiceSlotID , value); }
        }

        /// <summary>
        /// The type of activity that this job is auditing the progress of (e.g. data loading, dqe, custom etc)
        /// </summary>
        public AutomationJobType AutomationJobType
        {
            get { return _automationJobType; }
            set { SetField(ref _automationJobType , value); }
        }

        /// <summary>
        /// The last reported status of the current job
        /// </summary>
        public AutomationJobStatus LastKnownStatus
        {
            get { return _lastKnownStatus; }
            set { SetField(ref _lastKnownStatus, value); }
        }

        /// <inheritdoc/>
        public DateTime? Lifeline
        {
            get { return _lifeline; }
            set { SetField(ref _lifeline, value); }
        }

        /// <summary>
        /// The ID of the logging task (if any) that is being used to audit the activities of this job 
        /// <seealso cref="LoggingServer_ID"/>
        /// </summary>
        public int? DataLoadRunID
        {
            get { return _dataLoadRunID; }
            set { SetField(ref _dataLoadRunID, value); }
        }


        /// <summary>
        /// The ID of the logging server(if any) that is being used to audit the activities of this job 
        /// <seealso cref="DataLoadRunID"/>
        /// </summary>
        public int? LoggingServer_ID
        {
            get { return _loggingServerID; }
            set { SetField(ref _loggingServerID , value); }
        }

        /// <summary>
        /// A flag indicating if the system user has set a cancellation request on the job.  This is typically done from
        /// a remote machine and therefore is a setting in this database object.
        /// 
        /// <para>If you want to check whether someone has remotely changed this property in the database you can call  <see cref="AutomationJob.RevertToDatabaseState"/></para>
        /// </summary>
        public bool CancelRequested
        {
            get { return _cancelRequested; }
            set { SetField(ref _cancelRequested, value); }
        }

        /// <summary>
        /// Description of the job which is currently executing, this should include useful information e.g. the name of the load that is being running if the
        /// job is a DLE job etc.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { SetField(ref _description , value); }
        }

        #endregion

        internal AutomationJob(ICatalogueRepository repository, DbDataReader r): base(repository, r)
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

        /// <summary>
        /// Defines that a new job has been started by the process with the lock on the specified <see cref="AutomationServiceSlot"/>
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="parent"></param>
        /// <param name="jobType"></param>
        /// <param name="description"></param>
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

        /// <summary>
        /// Defines that a new caching job has been started by the process with the lock on the specified <see cref="AutomationServiceSlot"/>
        /// </summary>
        public AutomationJob(ICatalogueRepository repository, AutomationServiceSlot parent, CacheProgress cacheProgress)
            : this(repository, parent, AutomationJobType.Cache, GetCacheJobNameFor(cacheProgress))
        {
            
        }

        /// <summary>
        /// Defines that a new caching job associated with the specified <see cref="PermissionWindow"/> has been started by the process with the lock on the specified <see cref="AutomationServiceSlot"/>
        /// </summary>
        public AutomationJob(ICatalogueRepository repository, AutomationServiceSlot parent, PermissionWindow cachePermissionWindow)
            : this(repository, parent, AutomationJobType.Cache, GetCacheJobNameFor(cachePermissionWindow))
        {

        }

        /// <inheritdoc/>
        public void TickLifeline()
        {
            ((CatalogueRepository)Repository).TickLifeline(this);
        }

        /// <inheritdoc/>
        public void RefreshLifelinePropertyFromDatabase()
        {
            Lifeline = ((CatalogueRepository) Repository).GetTickLifeline(this);
        }

        /// <summary>
        /// Specifies that the current job requires exclusive use of the specified Catalogues (in addition to any others it might have already locked).  This 
        /// method will throw an Exception if the <see cref="Catalogue"/> is already declared as an <see cref="AutomationLockedCatalogues"/>.  Exclusive access
        /// applies only to automation jobs and will not be respected system wide by other activities e.g. users running manual loads / extractions etc.
        /// </summary>
        /// <param name="cataloguesToLock"></param>
        public void LockCatalogues(Catalogue[] cataloguesToLock)
        {
            foreach (Catalogue c in cataloguesToLock)
                ((CatalogueRepository) Repository).Insert(
                    "INSERT INTO AutomationLockedCatalogues (AutomationJob_ID,Catalogue_ID) VALUES (" + ID + "," + c.ID +
                    ")", new Dictionary<string, object>());
        }

        /// <summary>
        /// Returns all Catalogues which this job has an exclusive lock on 
        /// <seealso cref="LockCatalogues"/>
        /// </summary>
        /// <returns></returns>
        public Catalogue[] GetLockedCatalogues()
        {
            return ((CatalogueRepository) Repository).SelectAll<Catalogue>("SELECT * FROM AutomationLockedCatalogues WHERE AutomationJob_ID=" + ID, "Catalogue_ID").ToArray();
        }

        /// <summary>
        /// Updates the <seealso cref="LastKnownStatus"/> to the newStatus and saves the property to the database immediately
        /// </summary>
        /// <param name="newStatus"></param>
        public void SetLastKnownStatus(AutomationJobStatus newStatus)
        {
            ((CatalogueRepository)Repository).SaveSpecificPropertyOnlyToDatabase(this,"LastKnownStatus",newStatus);
        }

        /// <summary>
        /// Updates the <seealso cref="CancelRequested"/> flag to true and saves the property to the database immediately
        /// </summary>
        public void IssueCancelRequest()
        {
            ((CatalogueRepository)Repository).SaveSpecificPropertyOnlyToDatabase(this, "CancelRequested", true);
        }
         
        /// <inheritdoc/>
        public override string ToString()
        {
            return Description;
        }

        /// <summary>
        /// Records that audit logging has begun on the specified logging server under the specified ID <see cref="HIC.Logging.DataLoadInfo.ID"/>
        /// </summary> 
        /// <example><code>
        /// var lm = repository.GetDefaultLogManager();
        /// var dli = lm.CreateDataLoadInfo("LoadingData", "myexe.exe", "routine load 123", null, false);
        /// job.SetLoggingInfo((ExternalDatabaseServer)lm.DataAccessPointIfAny,dli.ID);
        /// </code></example>
        /// <param name="serverChosen"></param>
        /// <param name="dataLoadRunID"></param>
        public void SetLoggingInfo(IExternalDatabaseServer serverChosen, int dataLoadRunID)
        {
            ((CatalogueRepository) Repository).SaveSpecificPropertyOnlyToDatabase(this, "LoggingServer_ID",serverChosen.ID);
            ((CatalogueRepository) Repository).SaveSpecificPropertyOnlyToDatabase(this, "DataLoadRunID",dataLoadRunID);
        }
        
        private const string CacheProgressJobDescriptionPrefix = "Cache Progress ";
        private const string CachePermissionWindowJobDescriptionPrefix = "Cache PermissionWindow ";

        private static string GetCacheJobNameFor(CacheProgress cacheProgress)
        {
            return CacheProgressJobDescriptionPrefix + cacheProgress.ID;
        }

        private static string GetCacheJobNameFor(PermissionWindow permissionWindow)
        {
            return CachePermissionWindowJobDescriptionPrefix + permissionWindow.ID;
        }

        /// <summary>
        /// Returns the CacheProgress that is being executed by the AutomationJob if the job is a Cache <see cref="AutomationJobType"/> 
        /// <remarks>This relies on the ID of the CacheProgress being encoded in the name of the job</remarks>
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the CacheProgress PermissionWindow that is being executed by the AutomationJob if the job is a Cache
        /// <remarks>This relies on the ID of the PermissionWindow being encoded in the name of the job</remarks>
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Data Quality Engine automated execution
        /// </summary>
        DQE,

        /// <summary>
        /// Data Load Engine automated execution
        /// </summary>
        DLE,

        /// <summary>
        /// Caching Engine automated execution
        /// </summary>
        Cache,

        /// <summary>
        /// User defined <see cref="AutomateablePipeline"/>
        /// </summary>
        UserCustomPipeline
    }


    /// <summary>
    /// The last known state of the AutomationJob on an AutomationServiceSlot run by an automation server.
    /// </summary>
    public enum AutomationJobStatus
    {
        /// <summary>
        /// The job has been dispatched to the AutomationDestination but has not begun yet
        /// </summary>
        NotYetStarted,

        /// <summary>
        /// The job has begun execution
        /// </summary>
        Running,

        /// <summary>
        /// The job has succesfully acknowledged a cancellation request <see cref="AutomationJob.CancelRequested"/> and has cancelled itself
        /// </summary>
        Cancelled,

        /// <summary>
        /// The job has crashed unexpectedly.  Typically the <see cref="AutomationJob"/> will remain defined in a crashed state until a user notices and 
        /// marks it as resolved by deleting it
        /// </summary>
        Crashed,

        /// <summary>
        /// The job has finished executing.  Typically this should be immediately followed by deleting the <see cref="AutomationJob"/> so that it frees the 
        /// space for another job to start.
        /// </summary>
        Finished
    }
}
