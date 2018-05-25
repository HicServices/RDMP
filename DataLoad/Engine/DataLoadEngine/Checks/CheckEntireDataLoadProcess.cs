using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Checks
{
    /// <summary>
    /// Checks a LoadMetadata it is in a fit state to be executed (does it have primary keys, backup trigger etc).
    /// </summary>
    public class CheckEntireDataLoadProcess :  ICheckable, IDataLoadEventListener
    {
        private readonly HICDatabaseConfiguration _databaseConfiguration;
        private readonly HICLoadConfigurationFlags _loadConfigurationFlags;
        private readonly MEF _mef;
        public ILoadMetadata LoadMetadata { get; set; }

        MetadataLoggingConfigurationChecks _metadataLoggingConfigurationChecks;
        CatalogueLoadChecks _catalogueLoadChecks;
        PreExecutionChecker _preExecutionChecks;
        private ProcessTaskChecks _processTaskChecks;

        private ICheckNotifier _notifier;


        public CheckEntireDataLoadProcess(ILoadMetadata loadMetadata, HICDatabaseConfiguration databaseConfiguration, HICLoadConfigurationFlags loadConfigurationFlags, MEF mef)
        {
            _databaseConfiguration = databaseConfiguration;
            _loadConfigurationFlags = loadConfigurationFlags;
            _mef = mef;
            LoadMetadata = loadMetadata;


            _catalogueLoadChecks = new CatalogueLoadChecks(LoadMetadata, _loadConfigurationFlags, _databaseConfiguration);
            _metadataLoggingConfigurationChecks = new MetadataLoggingConfigurationChecks(loadMetadata);
            _processTaskChecks = new ProcessTaskChecks(LoadMetadata);
        }



        public void Check(ICheckNotifier notifier)
        {
            _notifier = notifier;

            _mef.CheckForVersionMismatches(_notifier);

            try
            {
                _processTaskChecks.Check(_notifier);
                _preExecutionChecks = new PreExecutionChecker(LoadMetadata,_databaseConfiguration);
            }
            catch (Exception e)
            {
                _notifier.OnCheckPerformed(new CheckEventArgs(
                    "MultiStageDataLoadProcess constructor crashed, unable to perform load checks", CheckResult.Fail, e));
                return;
            }

            try
            {
                _metadataLoggingConfigurationChecks.Check(_notifier);
                _catalogueLoadChecks.Check(_notifier);

                _preExecutionChecks.Check(_notifier);

            }
            catch (Exception e)
            {
                _notifier.OnCheckPerformed(new CheckEventArgs("Entire check process crashed in an unexpected way", CheckResult.Fail, e));
            }

        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if (e.ProgressEventType == ProgressEventType.Error)
                _notifier.OnCheckPerformed(new CheckEventArgs(
                    "received OnNotify event that something had gone wrong from " + sender.GetType().Name + " - Message was:" + e.Message,
                    CheckResult.Fail, e.Exception));

            if (e.ProgressEventType == ProgressEventType.Warning)
                _notifier.OnCheckPerformed(new CheckEventArgs(
                    "received OnNotify warning event from " + sender.GetType().Name + " - Message was:" + e.Message,
                    CheckResult.Warning, e.Exception));
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            
        }
    }
}
