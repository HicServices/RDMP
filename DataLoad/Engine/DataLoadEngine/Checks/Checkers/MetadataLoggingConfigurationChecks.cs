using System;
using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using LogManager = HIC.Logging.LogManager;

namespace DataLoadEngine.Checks.Checkers
{
    class MetadataLoggingConfigurationChecks : ICheckable
    {
        private readonly LoadMetadata _loadMetadata;


        public MetadataLoggingConfigurationChecks(LoadMetadata loadMetadata)
        {
            _loadMetadata = loadMetadata;
        }

        

        public void Check(ICheckNotifier notifier)
        {

            string distinctLoggingTask = null; 
            try
            {
                distinctLoggingTask = _loadMetadata.GetDistinctLoggingTask();
                notifier.OnCheckPerformed(new CheckEventArgs("All Catalogues agreed on a single Logging Task:" + distinctLoggingTask, CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Catalogues could not agreed on a single Logging Task", CheckResult.Fail, e));
            }

            try
            {
                var settings = _loadMetadata.GetDistinctLoggingDatabaseSettings();
                settings.TestConnection();
                notifier.OnCheckPerformed(new CheckEventArgs("Connected to logging architecture successfully", CheckResult.Success, null));


                if(distinctLoggingTask != null)
                {
                    LogManager lm = new LogManager(settings);
                    string[] dataTasks = lm.ListDataTasks();

                    if (dataTasks.Contains(distinctLoggingTask))
                        notifier.OnCheckPerformed(new CheckEventArgs("Found Logging Task " + distinctLoggingTask + " in Logging database",CheckResult.Success, null));
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs("Could not find Logging Task " + distinctLoggingTask + " in Logging database, you must enter the CatalogueManager and choose a new Logging Task by right clicking the Catalogue and selecting 'Configure Logging'", CheckResult.Fail, null));
                }

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could reach default logging server", CheckResult.Fail, e));
            }
        
        }
    }
}
