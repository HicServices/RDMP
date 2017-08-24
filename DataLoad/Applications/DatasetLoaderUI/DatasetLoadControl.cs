using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.Checks;
using DataLoadEngine.Checks.Checkers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job.Scheduling;
using DataLoadEngine.LoadExecution;
using DataLoadEngine.LoadProcess;
using DataLoadEngine.LoadProcess.Scheduling;
using DataLoadEngine.LoadProcess.Scheduling.Strategy;
using HIC.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

using ReusableUIComponents.SingleControlForms;

namespace DatasetLoaderUI
{
    /// <summary>
    /// Main form for the Data Export Manager.  At the top of the screen is a list of all the loads you have configured with the RDMP (See LoadMetadataUI).  Selecting a load will start
    /// 'pre execution checks' which will tick away.  You can edit a load by selecting 'View Load Metadata' (Launches LoadMetadataUI).
    /// 
    /// This user interface is intended for building and executing loads until they are stable after which you should set up periodic execution (See LoadPeriodicallyUI).
    /// 
    /// Only attempt to launch a data load if the checks are all passing (or giving Warnings that you understand and are not concerned about).  
    /// 
    /// Once started the load progress will appear on the right as data is loaded into RAW, migrated to STAGING and committed to LIVE (See  'RAW Bubble, STAGING Bubble, LIVE Model' in
    /// UserManual.docx for full implementation details).
    /// 
    /// There are various options for debugging for example you can override the location of the RAW bubble (e.g. to a test server), stop the data load after RAW is populated (in which
    /// case the load will crash out early allowing you to evaluated the RAW data in a database environment conducive with debugging dataset issues). 
    /// </summary>
    public partial class DatasetLoadControl : DatasetLoadControl_Design, IConsultableBeforeClosing
    {
        private IDataLoadProcess _dataLoadProcess;
        private HICDatabaseConfiguration _databaseLoadConfiguration;
        
        private Task _runningLoadProcessTask;
        private GracefulCancellationTokenSource _cancellationTokenSource;
        
        private bool _checksPassed = false;

        private LoadMetadata _loadMetadata;

        public DatasetLoadControl()
        {
            _cancellationTokenSource = null;
            InitializeComponent();

            helpIconRunRepeatedly.SetHelpText("Run Repeatedly", "By default running a scheduled load will run the number of days specified as a single load (e.g. 5 days of data).  Ticking this box means that if the load is succesful a further 5 days will be executed and again until either a data load fails or the load is up to date.");
            helpIconAbortShouldCancel.SetHelpText("Abort Behaviour", "By default clicking the Abort button (in Controls) will issue an Abort flag on a run which usually results in it completing the current stage (e.g. Migrate RAW to STAGING) then stop.  Ticking this button in a Load Progress based load will make the button instead issue a Cancel notification which means the data load will complete the current LoadProgress and then not start a new one.  This is only an option when you have ticked 'Run Repeatedly' (left)");
        }

        public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _loadMetadata = databaseObject;

            SetButtonStates();

            SetLoadProgressGroupBoxState();
        }

        private void SetLoadProgressGroupBoxState()
        {
            var loadProgresses = _loadMetadata.LoadProgresses;
            if (loadProgresses.Any())
            {
                //there are some load progresses
                gbLoadProgresses.Visible = true;
                gbDebugOptions.Left = gbLoadProgresses.Right + 3;
                
                //get the unlocked ones
                var unlockedLoadProgresses = loadProgresses.Where(schedule => !schedule.LockedBecauseRunning).ToArray();

                //they are all locked!!!
                if(!unlockedLoadProgresses.Any())
                {
                    ragChecks.Fatal(new Exception("All LoadProgresses for '" + _loadMetadata + "' are Locked, is someone else running this load?"));
                    ddLoadProgress.DataSource = null;
                    return;
                }

                //give the user the dropdown options for which load progress he wants to run
                var loadProgressData = new Dictionary<int, string> { { 0, "All available" } };

                foreach (var loadProgress in unlockedLoadProgresses)
                    loadProgressData.Add(loadProgress.ID, loadProgress.Name);

                ddLoadProgress.DataSource = new BindingSource(loadProgressData, null);
                ddLoadProgress.DisplayMember = "Value";
                ddLoadProgress.ValueMember = "Key";
            }
            else
            {
                gbLoadProgresses.Visible = false;
                gbDebugOptions.Left = gbControls.Right + 3;
            }
        }

        private void SetButtonStates()
        {
            //user must run checks
            if (!_checksPassed)
            {
                //tell user he must run checks
                if (ragChecks.IsGreen())
                    ragChecks.Warning(new Exception("Checks have not been run yet"));

                btnRunChecks.Enabled = true;

                //and disable everything else
                gbLoadProgresses.Enabled = false;
                btnExecute.Enabled = false;
                btnAbortLoad.Enabled = false;
                gbDebugOptions.Enabled = false;

                return;
            }
            
            //checks have passed is there a load underway already?
            if (_runningLoadProcessTask == null || _runningLoadProcessTask.IsCompleted)
            {
                //no load underway!

                //leave checks enabled and enable execute
                btnRunChecks.Enabled = true;
                btnExecute.Enabled = true;

                //also enable load progress selection if it's a LoadProgress based load
                gbLoadProgresses.Enabled = true;
                gbDebugOptions.Enabled = true;
            }
            else
            {
                //load is underway!
                btnExecute.Enabled = false;
                btnRunChecks.Enabled = false;

                //only thing we can do is abort
                btnAbortLoad.Enabled = true;
                gbLoadProgresses.Enabled = false;
                gbDebugOptions.Enabled = false;
            }
        }

        private void btnRunChecks_Click(object sender, EventArgs e)
        {
            _checksPassed = false;
            btnRunChecks.Enabled = false;
            
            //reset the visualisations
            ragChecks.Reset();
            checksUI1.Clear();

            //ensure the checks are visible over the load
            loadProgressUI1.Visible = false;
            checksUI1.Visible = true;


            try
            {
                TryToCreateHICProjectDirectory();
            }
            catch (Exception ex)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("There was a problem with the LoadMetadata's Project Directory", CheckResult.Fail, ex));
                ragChecks.Fatal(ex);
                return;
            }
            try
            {
                _databaseLoadConfiguration = new HICDatabaseConfiguration(_loadMetadata);
            }
            catch (Exception ex)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Could not resolve LoadMetadata into a valid HICDatabaseConfiguration", CheckResult.Fail, ex));
                ragChecks.Fatal(ex);
                return;
            }

            //create a to memory that passes the events to checksui since that's the only one that can respond to proposed fixes
            var toMemory = new ToMemoryCheckNotifier(checksUI1);


            Task.Factory.StartNew(() =>
            {
                //run the checks into toMemory / checksUI in a Thread
                var checker = new CheckEntireDataLoadProcess(_loadMetadata, _databaseLoadConfiguration, GetLoadConfigurationFlags(), RepositoryLocator.CatalogueRepository.MEF);
                checker.Check(toMemory);
                
            }).ContinueWith(
                t=>
                {
                    //once Thread completes do this on the main UI Thread

                    //find the worst check state
                    var worst = toMemory.GetWorst();
                    //update the rag smiley to reflect whether it has passed
                    ragChecks.OnCheckPerformed(new CheckEventArgs("Checks resulted in " + worst ,worst));
                    //update the bit flag
                    _checksPassed = worst <= CheckResult.Warning;
                
                    //enable other buttons now based on the new state
                    SetButtonStates();


                }, TaskScheduler.FromCurrentSynchronizationContext());


            _checksPassed = true;

        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            checksUI1.Visible = false;
            loadProgressUI1.Visible = true;
            loadProgressUI1.Clear();

            if (_loadMetadata.LoadProgresses.Any())
                if (cbRunIteratively.Checked)
                    _dataLoadProcess = CreateIterativeScheduledDataLoadProcess();
                else
                    _dataLoadProcess = CreateSingleScheduledJobLoadProcess();
            else
                _dataLoadProcess = CreateOnDemandDataLoadProcess();

            //create cancellation and start the run async
            _cancellationTokenSource = new GracefulCancellationTokenSource();
            _runningLoadProcessTask = 
                //run the data load in a Thread
                Task.Factory.StartNew(RunDataLoadProcess)
                //then on the main UI thread (after load completes with success/error
                .ContinueWith((t) =>
                {
                    //reset the system state because the execution has completed
                    _checksPassed = false;
                    //adjust the buttons accordingly
                    SetButtonStates();
                }
                    
                    
                    , TaskScheduler.FromCurrentSynchronizationContext());

            SetButtonStates();
        }

        private ILogManager CreateLogManager(ILoadMetadata loadMetadata)
        {
            return new LogManager(loadMetadata.GetDistinctLoggingDatabaseSettings(false));
        }

        private static string TranslateExitCode(ExitCodeType exitCode)
        {
            string message;
            switch (exitCode)
            {
                case ExitCodeType.Success:
                    message = "Data load has ended successfully";
                    break;
                case ExitCodeType.Error:
                    message = "Data load encountered an error, please see previous exceptions/errors.";
                    break;
                case ExitCodeType.Abort:
                    message = "Data load has been aborted/cancelled successfully";
                    break;
                case ExitCodeType.OperationNotRequired:
                    message = "Data load is not required, there is no data to load.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exitCode");
            }
            return message;
        }

        private void TryToCreateHICProjectDirectory()
        {
            try
            {
                new HICProjectDirectory(_loadMetadata.LocationOfFlatFiles, false);
            }
            catch (DirectoryNotFoundException e)
            {
                ExceptionViewer.Show("Couldn't create HICProjectDirectory for " + _loadMetadata.Name +
                                     ". It either doesn't exist or you don't have access rights to the root directory (DirectoryNotFoundException could mean this too) " +
                                     _loadMetadata.LocationOfFlatFiles  ,e);
            }
        }

        public void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            if (_cancellationTokenSource == null) return;

            e.Cancel = true;

            MessageBox.Show("Load is running so you cannot close this window, try clicking Abort first");
        }

        private void cbMigrateRAWToStaging_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMigrateRAWToStaging.Checked)
                cbMigrateStagingToLive.Enabled = true;
            else
            {
                cbMigrateStagingToLive.Enabled = false;
                cbMigrateStagingToLive.Checked = false;
            }
        }

        private HICLoadConfigurationFlags GetLoadConfigurationFlags()
        {
              return new HICLoadConfigurationFlags
            {
                ArchiveData = !cbSkipArchiving.Checked,
                DoLoadToStaging = cbMigrateRAWToStaging.Checked,
                DoMigrateFromStagingToLive = cbMigrateStagingToLive.Checked,
            };

        }

        private void btnAbortLoad_Click(object sender, EventArgs e)
        {
            if (cbAbortShouldActuallyCancelInstead.Checked)
                _cancellationTokenSource.Stop();
            else
                _cancellationTokenSource.Abort();

            loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,"Abort request issued, cross your fingers, eventually the DLE will reach a stopping point :)"));
        }
        #region LoadProcess creation

        private DataLoadProcess CreateOnDemandDataLoadProcess()
        {
            // Create the 'on demand' data load process, overriding the data provider if one is specified
            var logManager = CreateLogManager(_loadMetadata);
            var pipeline = CreateLoadPipeline(logManager);
            var preExecutionChecker = new PreExecutionChecker(_loadMetadata, _databaseLoadConfiguration);
            return new DataLoadProcess(_loadMetadata, preExecutionChecker, logManager, loadProgressUI1, pipeline);
        }

        private SingleJobScheduledDataLoadProcess CreateSingleScheduledJobLoadProcess()
        {
            var toAttempt = CreateLoadProgressSelectionStrategy();
            var jobDateGenerationStrategyFactory = new JobDateGenerationStrategyFactory(toAttempt, _databaseLoadConfiguration);

            var logManager = CreateLogManager(_loadMetadata);
            var preExecutionChecker = new PreExecutionChecker(_loadMetadata, _databaseLoadConfiguration);
            var pipeline = CreateLoadPipeline(logManager);

            return new SingleJobScheduledDataLoadProcess(_loadMetadata, preExecutionChecker, pipeline,
                jobDateGenerationStrategyFactory,
                CreateLoadProgressSelectionStrategy(), GetNumDaysPerJob(), logManager, loadProgressUI1);
        }

        private int GetNumDaysPerJob()
        {
            return Convert.ToInt32(udDaysPerJob.Value);
        }

        private IterativeScheduledDataLoadProcess CreateIterativeScheduledDataLoadProcess()
        {
            var toAttempt = CreateLoadProgressSelectionStrategy();
            var jobDateGenerationStrategyFactory = new JobDateGenerationStrategyFactory(toAttempt, _databaseLoadConfiguration);
            var logManager = CreateLogManager(_loadMetadata);
            var preExecutionChecker = new PreExecutionChecker(_loadMetadata, _databaseLoadConfiguration);
            var pipeline = CreateLoadPipeline(logManager);
            return new IterativeScheduledDataLoadProcess(_loadMetadata, preExecutionChecker, pipeline, jobDateGenerationStrategyFactory, toAttempt, GetNumDaysPerJob(), logManager, loadProgressUI1);
        }

        #endregion

        private void RunDataLoadProcess()
        {
            try
            {
                var exitCode = _dataLoadProcess.Run(_cancellationTokenSource.Token);
                loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, TranslateExitCode(exitCode)));
            }
            catch (OperationCanceledException ex)
            {
                loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Data load operation has been cancelled.", ex));
            }
            catch (AggregateException ex)
            {
                loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Entire data load process crashed", ex));
                ex.Handle(e =>
                {
                    loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, e.Message, e));
                    return true;
                });
            }
            catch (Exception e)
            {
                loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Entire data load process crashed", e)); 
                loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, e.Message, e));
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }

        private ILoadProgressSelectionStrategy CreateLoadProgressSelectionStrategy()
        {
            var scheduleItem = (KeyValuePair<int, string>)ddLoadProgress.SelectedItem;
            if (scheduleItem.Key == 0)
                return new AnyAvailableLoadProgressSelectionStrategy(_loadMetadata);

            var loadProgress = RepositoryLocator.CatalogueRepository.GetObjectByID<LoadProgress>(scheduleItem.Key);

            return new SingleLoadProgressSelectionStrategy(loadProgress);
        }

        private IDataLoadExecution CreateLoadPipeline(ILogManager logManager)
        {
            try
            {
                var repository = (CatalogueRepository)_loadMetadata.Repository;

                // Create the pipeline
                var factory = new HICDataLoadFactory(_loadMetadata, _databaseLoadConfiguration, GetLoadConfigurationFlags(), repository, logManager);
                var loadPipeline = factory.Create(loadProgressUI1);
                return loadPipeline;
            }
            catch (InvalidOperationException e)
            {
                throw new Exception("Error when building load pipeline: " + e);
            }
        }
        
        private void ddLoadProgress_SelectedIndexChanged(object sender, EventArgs e)
        {
            var scheduleItem = (KeyValuePair<int, string>)ddLoadProgress.SelectedItem;
            if (scheduleItem.Key == 0)
            {
                var progresses = _loadMetadata.LoadProgresses.ToArray();
                if (progresses.Length == 1)
                    udDaysPerJob.Value = progresses[0].DefaultNumberOfDaysToLoadEachTime;
            }
            else
                udDaysPerJob.Value =
                    RepositoryLocator.CatalogueRepository.GetObjectByID<LoadProgress>(scheduleItem.Key)
                        .DefaultNumberOfDaysToLoadEachTime;
        }

        public override string GetTabName()
        {
            return "Execution:"+ base.GetTabName();
        }

        private void cbRunIteratively_CheckedChanged(object sender, EventArgs e)
        {
            //can only cancel between runs if we are running multiple runs
            cbAbortShouldActuallyCancelInstead.Enabled = cbRunIteratively.Checked;
        }

    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DatasetLoadControl_Design, UserControl>))]
    public abstract class DatasetLoadControl_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
    {
    }
}