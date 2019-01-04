using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Factories;
using DataLoadEngine.LoadProcess;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.Checks
{
    /// <summary>
    /// Checks a LoadMetadata it is in a fit state to be executed (does it have primary keys, backup trigger etc).
    /// </summary>
    public class CheckEntireDataLoadProcess :  ICheckable
    {
        private readonly HICDatabaseConfiguration _databaseConfiguration;
        private readonly HICLoadConfigurationFlags _loadConfigurationFlags;
        private readonly MEF _mef;
        public ILoadMetadata LoadMetadata { get; set; }

        public CheckEntireDataLoadProcess(ILoadMetadata loadMetadata, HICDatabaseConfiguration databaseConfiguration, HICLoadConfigurationFlags loadConfigurationFlags, MEF mef)
        {
            _databaseConfiguration = databaseConfiguration;
            _loadConfigurationFlags = loadConfigurationFlags;
            _mef = mef;
            LoadMetadata = loadMetadata;

        }
        
        public void Check(ICheckNotifier notifier)
        {
            var catalogueLoadChecks = new CatalogueLoadChecks(LoadMetadata, _loadConfigurationFlags, _databaseConfiguration);
            var metadataLoggingConfigurationChecks = new MetadataLoggingConfigurationChecks(LoadMetadata);
            var processTaskChecks = new ProcessTaskChecks(LoadMetadata);
            var preExecutionChecks = new PreExecutionChecker(LoadMetadata, _databaseConfiguration);

            _mef.CheckForVersionMismatches(notifier);

            //If the load is a progressable (loaded over time) then make sure any associated caches are compatible with the load ProcessTasks
            foreach (ILoadProgress loadProgress in LoadMetadata.LoadProgresses)
            {
                var cp = loadProgress.CacheProgress;
                if(cp != null)
                {
                    try
                    {
                        var f = new CacheLayoutFactory();
                        f.CreateCacheLayout(loadProgress,LoadMetadata);
                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Load contains a CacheProgress '" + cp + "' but we were unable to generate an ICacheLayout, see Inner Exception for details",CheckResult.Fail,e));
                    }
                }
            }

            //Make sure theres some load tasks and they are valid
            processTaskChecks.Check(notifier);
            
            
            try
            {
                metadataLoggingConfigurationChecks.Check(notifier);

                preExecutionChecks.Check(notifier);
                
                if(!preExecutionChecks.HardFail)
                    catalogueLoadChecks.Check(notifier);

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Entire check process crashed in an unexpected way", CheckResult.Fail, e));
            }

        }
    }
}
