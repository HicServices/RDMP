using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadExecution.Delegates;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadProcess
{
    public class DataLoadProcess : IDataLoadProcess, IDataLoadOperation
    {
        /// <summary>
        /// Provides jobs for the data load process, allows different strategies for what jobs will be loaded e.g. single job, scheduled
        /// </summary>
        public IJobFactory JobProvider { get; set; }

        /// <summary>
        /// The load execution that will be used to load the jobs provided by the JobProvider
        /// </summary>
        public IDataLoadExecution LoadExecution { get; private set; }

        public ExitCodeType? ExitCode { get; private set; }
        public Exception Exception { get; private set; }

        protected readonly ILoadMetadata LoadMetadata;
        protected readonly IDataLoadEventListener DataLoadEventListener;
        protected readonly ILogManager LogManager;

        private readonly ICheckable _preExecutionChecker;
        
        public DataLoadProcess(ILoadMetadata loadMetadata, ICheckable preExecutionChecker, ILogManager logManager, IDataLoadEventListener dataLoadEventListener, IDataLoadExecution loadExecution)
        {
            LoadMetadata = loadMetadata;
            DataLoadEventListener = dataLoadEventListener;
            LoadExecution = loadExecution;
            _preExecutionChecker = preExecutionChecker;
            LogManager = logManager;
            ExitCode = ExitCodeType.Success;

            JobProvider = new SingleJobProvider(new JobFactory(loadMetadata,logManager));
        }

        public virtual ExitCodeType Run(GracefulCancellationToken loadCancellationToken)
        {
            PerformPreExecutionChecks();

            // create job
            var job = JobProvider.Create(DataLoadEventListener);

            // if job is null, there are no more jobs to submit
            if (job == null)
                return ExitCodeType.OperationNotRequired;

            return LoadExecution.Run(job, loadCancellationToken);
        }

       private void PerformPreExecutionChecks()
        {
            try
            {
                DataLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Performing pre-execution checks"));
                var thrower = new ThrowImmediatelyCheckNotifier();
                _preExecutionChecker.Check(thrower);
            }
            catch (Exception e)
            {
                Exception = e;
                ExitCode = ExitCodeType.Error;
            }
        }
    }
}