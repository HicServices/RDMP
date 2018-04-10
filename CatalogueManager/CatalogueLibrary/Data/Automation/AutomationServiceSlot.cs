using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data.Automation
{
    /// <summary>
    /// Determines how many instances of the RDMPAutomationService executable can be run at once and stores the settings that they will use to guide their behaviour (e.g. how many DQE runs to 
    /// execute simultaneously, how many data load jobs to run at once etc).  Each record in this table corresponds to one allowable instance of RDMPAutomationService.exe.  The first thing the
    /// automation service does is to look for an unlocked slot (LockHeldBy is null) which it will then lock.  It will then use the settings to decide what runs to launch.  All runs (e.g. DQE/DLE)
    /// are audited at runtime as an AutomationJob (as well as any logging that takes place in the logging database). 
    /// </summary>
    public class AutomationServiceSlot : DatabaseEntity, ILockable, ILifelineable, INamed,IHasDependencies
    {
       
        #region Database Properties
        private bool _lockedBecauseRunning;
        private string _lockHeldBy;
        private int _dqeMaxConcurrentJobs;
        private int _dqeDaysBetweenEvaluations;
        private AutomationDQEJobSelectionStrategy _dqeSelectionStrategy;
        private AutomationFailureStrategy _dqeFailureStrategy;
        private int _dleMaxConcurrentJobs;
        private AutomationFailureStrategy _dleFailureStrategy;
        private int _cacheMaxConcurrentJobs;
        private AutomationFailureStrategy _cacheFailureStrategy;
        private DateTime? _lifeline;
        private int? _globalTimeoutPeriod;
        private string _name;

        /// <inheritdoc/>
        public bool LockedBecauseRunning
        {
            get { return _lockedBecauseRunning; }
            set {SetField(ref  _lockedBecauseRunning , value); }
        }

        /// <inheritdoc/>
        public string LockHeldBy
        {
            get { return _lockHeldBy; }
            set {SetField(ref  _lockHeldBy , value); }
        }

        /// <summary>
        /// The maximum number of AutomationJobs of type Data Quality Engine that should be allowed to run simultaneously
        /// </summary>
        public int DQEMaxConcurrentJobs
        {
            get { return _dqeMaxConcurrentJobs; }
            set {SetField(ref  _dqeMaxConcurrentJobs , value); }
        }

        /// <summary>
        /// The number of days to wait before triggering an automated DQE evaluation of a dataset
        /// </summary>
        public int DQEDaysBetweenEvaluations
        {
            get { return _dqeDaysBetweenEvaluations; }
            set {SetField(ref  _dqeDaysBetweenEvaluations , value); }
        }

        /// <summary>
        /// Determines which Catalogue to run the automted DQE on when multiple Catalogues are due
        /// to have DQE run
        /// </summary>
        public AutomationDQEJobSelectionStrategy DQESelectionStrategy
        {
            get { return _dqeSelectionStrategy; }
            set {SetField(ref  _dqeSelectionStrategy , value); }
        }

        /// <summary>
        /// Determines what to do when a DQE evaluation crashes on a Catalogue
        /// </summary>
        public AutomationFailureStrategy DQEFailureStrategy
        {
            get { return _dqeFailureStrategy; }
            set {SetField(ref  _dqeFailureStrategy , value); }
        }

        /// <summary>
        /// The maximum number of AutomationJobs of type Data Load Engine that should be allowed to run simultaneously
        /// </summary>
        public int DLEMaxConcurrentJobs
        {
            get { return _dleMaxConcurrentJobs; }
            set {SetField(ref  _dleMaxConcurrentJobs , value); }
        }
        
        /// <summary>
        /// Determines what to do when an automated Data Load crashes
        /// </summary>
        public AutomationFailureStrategy DLEFailureStrategy
        {
            get { return _dleFailureStrategy; }
            set {SetField(ref  _dleFailureStrategy , value); }
        }

        /// <summary>
        /// The maximum number of AutomationJobs of type Caching (long running tasks that primarily involve fetching files and storing to disk) that should be
        ///  allowed to run simultaneously
        /// </summary>
        public int CacheMaxConcurrentJobs
        {
            get { return _cacheMaxConcurrentJobs; }
            set {SetField(ref  _cacheMaxConcurrentJobs , value); }
        }
        
        /// <summary>
        /// Determines what to do when an automated Data caching crashes
        /// </summary>
        public AutomationFailureStrategy CacheFailureStrategy
        {
            get { return _cacheFailureStrategy; }
            set {SetField(ref  _cacheFailureStrategy , value); }
        }

        /// <inheritdoc/>
        public DateTime? Lifeline
        {
            get { return _lifeline; }
            set {SetField(ref  _lifeline , value); }
        }

        /// <summary>
        /// Overrides the application wide default timeout setting for database commands.  This is usually 30 seconds (<see cref="DatabaseCommandHelper.GlobalTimeout"/>).  This
        /// will only affect commands where an explicit (usually longer) timeout has been set on the command e.g. for MERGE operations etc that are expected to take a long time. 
        /// </summary>
        public int? GlobalTimeoutPeriod
        {
            get { return _globalTimeoutPeriod; }
            set {SetField(ref  _globalTimeoutPeriod , value); }
        }

        /// <inheritdoc/>
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        #endregion
      
        #region Relationships
        /// <summary>
        /// Fetches all the AutomationJobs currently underway/crashed on the AutomationServiceSlot (This is refreshed every time you call this property)
        /// </summary>
        [NoMappingToDatabase]
        public AutomationJob[] AutomationJobs
        {
            get { return Repository.GetAllObjectsWithParent<AutomationJob>(this).ToArray(); }
        }

        /// <summary>
        /// Fetches all the AutomateablePipelines that are associated with this AutomationServiceSlot
        /// </summary>
        [NoMappingToDatabase]
        public AutomateablePipeline[] AutomateablePipelines { get { return Repository.GetAllObjectsWithParent<AutomateablePipeline>(this); } }
        
        #endregion

        internal AutomationServiceSlot(ICatalogueRepository repository, DbDataReader r): base(repository, r)
        {
            Name = r["Name"].ToString();
            LockedBecauseRunning = Convert.ToBoolean(r["LockedBecauseRunning"]);
            LockHeldBy = r["LockHeldBy"].ToString();
            DQEMaxConcurrentJobs = Convert.ToInt32(r["DQEMaxConcurrentJobs"]);
            DQEDaysBetweenEvaluations = Convert.ToInt32(r["DQEDaysBetweenEvaluations"]);
            DLEMaxConcurrentJobs = Convert.ToInt32(r["DLEMaxConcurrentJobs"]);
            CacheMaxConcurrentJobs = Convert.ToInt32(r["CacheMaxConcurrentJobs"]);

            DLEFailureStrategy = ToFailureStrategy(r["DLEFailureStrategy"]);
            DQEFailureStrategy = ToFailureStrategy(r["DQEFailureStrategy"]);
            CacheFailureStrategy = ToFailureStrategy(r["CacheFailureStrategy"]);
            
            DQESelectionStrategy = ToJobSelectionStrategy(r["DQESelectionStrategy"]);

            GlobalTimeoutPeriod = ObjectToNullableInt(r["GlobalTimeoutPeriod"]);
            Lifeline = ObjectToNullableDateTime(r["Lifeline"]);
        }

        private AutomationFailureStrategy ToFailureStrategy(object o)
        {
            AutomationFailureStrategy outVar;
            AutomationFailureStrategy.TryParse(o.ToString(), out outVar);

            return outVar;
        }

        private AutomationDQEJobSelectionStrategy ToJobSelectionStrategy(object o)
        {
            AutomationDQEJobSelectionStrategy outVar;
            AutomationDQEJobSelectionStrategy.TryParse(o.ToString(), out outVar);

            return outVar;
        }

        /// <summary>
        /// Defines a persistent slot for the automation service to run the jobs specified according to the strategies you pick.  You will still
        /// need to run the RDMPAutomationService, this class only outlines the users desired behaviour. 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="dqeFailureStrategy"></param>
        /// <param name="dleFailureStrategy"></param>
        /// <param name="cacheFailureStrategy"></param>
        /// <param name="dqeSelectionStrategy"></param>
        public AutomationServiceSlot(ICatalogueRepository repository,
            AutomationFailureStrategy dqeFailureStrategy = AutomationFailureStrategy.Stop,
            AutomationFailureStrategy dleFailureStrategy = AutomationFailureStrategy.Stop,
            AutomationFailureStrategy cacheFailureStrategy = AutomationFailureStrategy.Stop,
            AutomationDQEJobSelectionStrategy dqeSelectionStrategy = AutomationDQEJobSelectionStrategy.DatasetWithMostOutOfDateDQEResults)
        {
            Repository = repository;
            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"Name","Unnamed Slot"},
                {"DQEFailureStrategy",dqeFailureStrategy.ToString()},
                {"DLEFailureStrategy",dleFailureStrategy.ToString()},
                {"CacheFailureStrategy",cacheFailureStrategy.ToString()},
                {"DQESelectionStrategy",dqeSelectionStrategy.ToString()}
                
            });
        }

        /// <inheritdoc/>
        public void Lock()
        {
            LockedBecauseRunning = true;
            LockHeldBy = Environment.UserName + " (" + Environment.MachineName + ")";
            Lifeline = DateTime.Now;
            SaveToDatabase();
        }

        /// <inheritdoc/>
        public void Unlock()
        {
            LockedBecauseRunning = false;
            LockHeldBy = null;
            SaveToDatabase();
        }

        /// <summary>
        /// Returns the Name property
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public override void DeleteInDatabase()
        {
            if(LockedBecauseRunning)
                throw new NotSupportedException("Cannot delete " + this + " because it is locked by " + LockHeldBy);

            base.DeleteInDatabase();
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

        /// <inheritdoc/>
        public void RefreshLockPropertiesFromDatabase()
        {
            ((CatalogueRepository)Repository).RefreshLockPropertiesFromDatabase(this);
        }

        /// <summary>
        /// Returns true if the maximum number of jobs permitted on the slot has not yet been reached for the given AutomationJobType.  Also considers the AutomationFailureStrategy
        /// if there are outstanding crashed jobs of the corresponding jobType.
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public bool IsAcceptingNewJobs(AutomationJobType jobType)
        {
            AutomationFailureStrategy strategy;

            int maxAcceptable;
            switch (jobType)
            {
                case AutomationJobType.DQE:
                    maxAcceptable = DQEMaxConcurrentJobs;
                    strategy = DQEFailureStrategy;
                    break;
                case AutomationJobType.DLE:
                    maxAcceptable = DLEMaxConcurrentJobs;
                    strategy = DLEFailureStrategy;
                    break;
                case AutomationJobType.Cache:
                    maxAcceptable = CacheMaxConcurrentJobs;
                    strategy = CacheFailureStrategy;
                    break;
                case AutomationJobType.UserCustomPipeline:
                    throw new Exception("Do not call this method with UserCustomPipeline, instead your custom source component should look at the ongoing jobs and wherver it gets it's settings from and decide whether it should issue a new job or not at any given time");
                default:
                    throw new ArgumentOutOfRangeException("jobType");
            }

            //cache it so that we don't refetch from database twice
            var jobs = AutomationJobs;

            //if Failure strategy is to stop and there are failed/crashed jobs of the type proposed
            if (strategy == AutomationFailureStrategy.Stop && jobs.Any(j =>
                    //job type matches
                    j.AutomationJobType == jobType &&
                    //AND job was crashed or cancelled
                    (j.LastKnownStatus == AutomationJobStatus.Cancelled ||j.LastKnownStatus == AutomationJobStatus.Crashed)
                ))
            {
                return false;//do not start any new jobs
            }

            //if the number of automation jobs is less than the max allowed then go ahead!
            return jobs.Count(j => j.AutomationJobType == jobType) < maxAcceptable;
        }

        /// <inheritdoc cref="AutomationJobs/>
        public AutomationJob[] GetAutomationJobs(int maximumNumberToRetrieve)
        {
            return Repository.SelectAll<AutomationJob>("SELECT TOP " + maximumNumberToRetrieve +
                                                " ID FROM AutomationJob WHERE AutomationServiceSlot_ID = " + ID +
                                                " ORDER BY ID ASC","ID").ToArray();
        }

        /// <summary>
        /// Documents that a new job has begun on the Automation Server that occupies the AutomationServiceSlot (has it locked).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public AutomationJob AddNewJob(AutomationJobType type, string description)
        {
            return new AutomationJob((ICatalogueRepository) Repository, this, type, description);
        }

        /// <summary>
        /// Documents that a new caching job has begun on the Automation Server that occupies the AutomationServiceSlot (has it locked).
        /// </summary>
        public AutomationJob AddNewJob(CacheProgress cacheProgress)
        {
            return new AutomationJob((ICatalogueRepository)Repository,this,cacheProgress);
        }

        /// <summary>
        /// Documents that a new caching job using the PermissionWindow has begun on the Automation Server that occupies the AutomationServiceSlot (has it locked).
        /// </summary>
        public AutomationJob AddNewJob(IPermissionWindow permissionWindow)
        {
            return new AutomationJob((ICatalogueRepository)Repository, this, permissionWindow);
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            return new IHasDependencies[0];
        }

        /// <inheritdoc/>
        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            List<IHasDependencies> dependencies = new List<IHasDependencies>();
            dependencies.AddRange(AutomateablePipelines);

            return dependencies.ToArray();
        }
    }

    /// <summary>
    /// What to do when an AutomationJob completes with Crashed state
    /// </summary>
    public enum AutomationFailureStrategy
    {
        /// <summary>
        /// Attempt to do the next novel Job - e.g. if DQE run on Prescribing crashed carry on and try to DQE Biochemistry
        /// </summary>
        TryNext,

        /// <summary>
        /// Once a job of a given the category has crashed do not start any new ones - e.g. if DQE run on Prescribing crashed do not start any more DQE runs on other datasets
        /// </summary>
        Stop
    }

    /// <summary>
    /// How to Prioritise which dataset to run the data quality engine on next.
    /// </summary>
    public enum AutomationDQEJobSelectionStrategy
    {
        /// <summary>
        /// Run the DQE prioritising datasets which have recently been loaded by the DLE
        /// </summary>
        MostRecentlyLoadedDataset,

        /// <summary>
        /// Run the DQE prioritising datasets which have not been DQEd for a long time.
        /// </summary>
        DatasetWithMostOutOfDateDQEResults
    }
}
