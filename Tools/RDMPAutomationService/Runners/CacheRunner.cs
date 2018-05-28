using System;
using System.Linq;
using CachingEngine;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using HIC.Logging;
using HIC.Logging.Listeners;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Runners
{
    /// <summary>
    /// Automation task that runs a single CacheProgress until it is up-to-date (all available data read) or crashes.
    /// </summary>
    public class CacheRunner : IRunner
    {
        private readonly CacheOptions _options;
        
        public CacheRunner(CacheOptions options)
        {
            _options = options;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            const string dataLoadTask = "caching";

            CacheProgress cp = repositoryLocator.CatalogueRepository.GetObjectByID<CacheProgress>(_options.CacheProgress);
            
            var defaults = new ServerDefaults(repositoryLocator.CatalogueRepository);
            var loggingServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

            if (loggingServer == null)
                throw new NotSupportedException("No default logging server specified, you must specify one in ");

            var logManager = new LogManager(loggingServer);

            logManager.CreateNewLoggingTaskIfNotExists(dataLoadTask);

            switch (_options.Command)
            {
                case CommandLineActivity.run:

                    //Setup dual listeners for the Cache process, one ticks the lifeline one very message and one logs to the logging db
                    var toLog = new ToLoggingDatabaseDataLoadEventListener(this, logManager, dataLoadTask, "Caching " + cp);
                    var forkListener = new ForkDataLoadEventListener(toLog, listener);
                    try
                    {
                        var cachingHost = new CachingHost(repositoryLocator.CatalogueRepository);
                        cachingHost.RetryMode = _options.RetryMode;
                        cachingHost.CacheProgressList = new ICacheProgress[] { cp }.ToList(); //run the cp

                        //if it has a permission window
                        if (cp.PermissionWindow_ID != null)
                            cachingHost.PermissionWindows = new[] { cp.PermissionWindow }.ToList();

                        //By default caching host will block 
                        cachingHost.TerminateIfOutsidePermissionWindow = true;

                        cachingHost.Start(forkListener, token);
                    }
                    finally
                    {
                        //finish everything
                        toLog.FinalizeTableLoadInfos();
                    }

                    break;
                case CommandLineActivity.check:
                    var checkable = new CachingPreExecutionChecker(cp);
                    checkable.Check(checkNotifier);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return 0;
        }
    }
}
