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
    public class AutomationServiceSlot : DatabaseEntity, ILockable, ILifelineable, INamed
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

        public bool LockedBecauseRunning
        {
            get { return _lockedBecauseRunning; }
            set {SetField(ref  _lockedBecauseRunning , value); }
        }

        public string LockHeldBy
        {
            get { return _lockHeldBy; }
            set {SetField(ref  _lockHeldBy , value); }
        }

        public int DQEMaxConcurrentJobs
        {
            get { return _dqeMaxConcurrentJobs; }
            set {SetField(ref  _dqeMaxConcurrentJobs , value); }
        }

        public int DQEDaysBetweenEvaluations
        {
            get { return _dqeDaysBetweenEvaluations; }
            set {SetField(ref  _dqeDaysBetweenEvaluations , value); }
        }

        public AutomationDQEJobSelectionStrategy DQESelectionStrategy
        {
            get { return _dqeSelectionStrategy; }
            set {SetField(ref  _dqeSelectionStrategy , value); }
        }

        public AutomationFailureStrategy DQEFailureStrategy
        {
            get { return _dqeFailureStrategy; }
            set {SetField(ref  _dqeFailureStrategy , value); }
        }

        public int DLEMaxConcurrentJobs
        {
            get { return _dleMaxConcurrentJobs; }
            set {SetField(ref  _dleMaxConcurrentJobs , value); }
        }

        public AutomationFailureStrategy DLEFailureStrategy
        {
            get { return _dleFailureStrategy; }
            set {SetField(ref  _dleFailureStrategy , value); }
        }

        public int CacheMaxConcurrentJobs
        {
            get { return _cacheMaxConcurrentJobs; }
            set {SetField(ref  _cacheMaxConcurrentJobs , value); }
        }

        public AutomationFailureStrategy CacheFailureStrategy
        {
            get { return _cacheFailureStrategy; }
            set {SetField(ref  _cacheFailureStrategy , value); }
        }

        public DateTime? Lifeline
        {
            get { return _lifeline; }
            set {SetField(ref  _lifeline , value); }
        }

        public int? GlobalTimeoutPeriod
        {
            get { return _globalTimeoutPeriod; }
            set {SetField(ref  _globalTimeoutPeriod , value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        #endregion
      
        #region Relationships
        [NoMappingToDatabase]
        public AutomationJob[] AutomationJobs
        {
            get { return Repository.GetAllObjectsWithParent<AutomationJob>(this).ToArray(); }
        }
        [NoMappingToDatabase]
        public AutomateablePipeline[] AutomateablePipelines { get { return Repository.GetAllObjectsWithParent<AutomateablePipeline>(this); } }
        
        #endregion

        public AutomationServiceSlot(ICatalogueRepository repository, DbDataReader r): base(repository, r)
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

        public void Lock()
        {
            LockedBecauseRunning = true;
            LockHeldBy = Environment.UserName + " (" + Environment.MachineName + ")";
            Lifeline = DateTime.Now;
            SaveToDatabase();
        }

        public void Unlock()
        {
            LockedBecauseRunning = false;
            LockHeldBy = null;
            SaveToDatabase();
        }

        public override string ToString()
        {
            return Name;
        }

        public override void DeleteInDatabase()
        {
            if(LockedBecauseRunning)
                throw new NotSupportedException("Cannot delete " + this + " because it is locked by " + LockHeldBy);

            base.DeleteInDatabase();
        }
        
        public void TickLifeline()
        {
            ((CatalogueRepository)Repository).TickLifeline(this);
        }

        public void RefreshLifelinePropertyFromDatabase()
        {
            Lifeline = ((CatalogueRepository) Repository).GetTickLifeline(this);
        }

        public void RefreshLockPropertiesFromDatabase()
        {
            ((CatalogueRepository)Repository).RefreshLockPropertiesFromDatabase(this);
        }

        public bool IsAcceptingNewJobs( AutomationJobType jobType)
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
                    return false;//do not start any new jobs

            //if the number of automation jobs is 
            return jobs.Count(j => j.AutomationJobType == jobType) < maxAcceptable;
        }

        public AutomationJob[] GetAutomationJobs(int maximumNumberToRetrieve)
        {

            return Repository.SelectAll<AutomationJob>("SELECT TOP " + maximumNumberToRetrieve +
                                                " ID FROM AutomationJob WHERE AutomationServiceSlot_ID = " + ID +
                                                " ORDER BY ID ASC","ID").ToArray();
        }

        public AutomationJob AddNewJob(AutomationJobType type, string description)
        {
            return new AutomationJob((ICatalogueRepository) Repository, this, type, description);
        }

        public AutomationJob AddNewJob(CacheProgress cacheProgress)
        {
            return new AutomationJob((ICatalogueRepository)Repository,this,cacheProgress);
        }

        public AutomationJob AddNewJob(PermissionWindow permissionWindow)
        {
            return new AutomationJob((ICatalogueRepository)Repository, this, permissionWindow);
        }

    }

    public enum AutomationFailureStrategy
    {
        TryNext,
        Stop
    }

    public enum AutomationDQEJobSelectionStrategy
    {
        MostRecentlyLoadedDataset,
        DatasetWithMostOutOfDateDQEResults
    }
}
