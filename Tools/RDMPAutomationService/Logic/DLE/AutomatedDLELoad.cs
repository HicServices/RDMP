using System;
using System.Linq;
using System.Threading;
using CatalogueLibrary;
using CatalogueLibrary.Data;

using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using DataLoadEngine.LoadProcess.Scheduling;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using HIC.Logging;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Logic.DLE
{
    /// <summary>
    /// Automation task that runs a single data load (LoadMetadata)
    /// </summary>
    internal class AutomatedDLELoad
    {
        private readonly ILoadMetadata _loadMetadata;
        private ILoadProgress _loadProgress;
        private readonly bool _iterative;

        private CatalogueRepository _repository;
        private CancellationToken _cancellationToken;
        
        /// <summary>
        /// Starts a new one off load of the specified load metadata
        /// </summary>
        public AutomatedDLELoad(DleOptions options)
        {
            _repository = options.GetRepositoryLocator().CatalogueRepository;

            if(options.LoadProgress != 0)
            _loadMetadata = _repository.GetObjectByID<LoadMetadata>(options.LoadMetadata);

            if(options.LoadProgress != 0)
            {
                _loadProgress = _repository.GetObjectByID<LoadProgress>(options.LoadProgress);
                if (_loadMetadata == null)
                    _loadMetadata = _loadProgress.LoadMetadata;
                else
                    if(_loadProgress.LoadMetadata_ID != _loadMetadata.ID)
                        throw new ArgumentException("The supplied LoadProgress does not belong to the supplied LoadMetadata load");
            }

            _iterative = options.Iterative;
        }

        public int RunTask(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repository = repositoryLocator.CatalogueRepository;
            _cancellationToken = new CancellationToken();
      
            var p = new PreExecutionChecker(_loadMetadata, null);
            p.Check(new IgnoreAllErrorsCheckNotifier());

            var loggingServer = _loadMetadata.GetDistinctLoggingDatabaseSettings();
            var logManager = new LogManager(loggingServer);
                
            var databaseConfiguration = new HICDatabaseConfiguration(_loadMetadata);

            // Create the pipeline to pass into the DataLoadProcess object
            var dataLoadFactory = new HICDataLoadFactory(_loadMetadata, databaseConfiguration,new HICLoadConfigurationFlags(),_repository, logManager);
                
            var listener = new ThrowImmediatelyDataLoadEventListener(){WriteToConsole = false};

            IDataLoadExecution execution = dataLoadFactory.Create(listener);

            IDataLoadProcess dataLoadProcess;

                
            if (_loadMetadata.LoadProgresses.Any())
            {
                //Then the load is designed to run X days of source data at a time
                //Load Progress
                ILoadProgressSelectionStrategy whichLoadProgress = _loadProgress != null ?
                    (ILoadProgressSelectionStrategy) new SingleLoadProgressSelectionStrategy(_loadProgress) :
                    new AnyAvailableLoadProgressSelectionStrategy(_loadMetadata);

                var jobDateFactory = new JobDateGenerationStrategyFactory(whichLoadProgress);
                    
                dataLoadProcess = _iterative
                    ? (IDataLoadProcess) new IterativeScheduledDataLoadProcess(_loadMetadata, p, execution, jobDateFactory,whichLoadProgress, null, logManager, listener):
                        new SingleJobScheduledDataLoadProcess(_loadMetadata, p, execution, jobDateFactory,whichLoadProgress, null, logManager, listener) ;
            }
            else
                //OnDemand
                dataLoadProcess = new DataLoadProcess(_loadMetadata, p, logManager, listener, execution);

            var exitCode =
                dataLoadProcess.Run(new GracefulCancellationToken(_cancellationToken,_cancellationToken));

            return exitCode == ExitCodeType.OperationNotRequired || exitCode == ExitCodeType.Success ? 1 : 0;
                
        }
    }
}
