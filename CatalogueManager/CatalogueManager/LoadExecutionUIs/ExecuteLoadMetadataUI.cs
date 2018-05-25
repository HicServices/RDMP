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
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CommandLine;
using CommandLine.Text;
using DataLoadEngine.Checks;
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
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.LoadExecutionUIs
{
    /// <summary>
    /// Runs the Data Load Engine on a single LoadMetadata.  This user interface is intended for manually executing and debugging loads.  If you have a stable load and you want
    /// to execute it automatically you can set up a periodic execution (See LoadPeriodicallyUI / AutomationServiceSlotUI).
    /// 
    /// <para>You can only attempt to launch a data load if the checks are all passing (or giving Warnings that you understand and are not concerned about).  </para>
    /// 
    /// <para>Once started the load progress will appear and show as data is loaded into RAW, migrated to STAGING and committed to LIVE (See  'RAW Bubble, STAGING Bubble, LIVE Model'
    /// in UserManual.docx for full implementation details).</para>
    /// 
    /// <para>There are various options for debugging for example you can override and stop the data load after RAW is populated (in which case the load will crash out early allowing
    /// you to evaluated the RAW data in a database environment conducive with debugging dataset issues). </para>
    /// </summary>
    public partial class ExecuteLoadMetadataUI : DatasetLoadControl_Design
    {
        private HICDatabaseConfiguration _databaseLoadConfiguration;
        
        private Task _runningLoadProcessTask;
        
        private LoadMetadata _loadMetadata;
        private ILoadProgress[] _allLoadProgresses;

        public ExecuteLoadMetadataUI()
        {
            InitializeComponent();

            helpIconRunRepeatedly.SetHelpText("Run Repeatedly", "By default running a scheduled load will run the number of days specified as a single load (e.g. 5 days of data).  Ticking this box means that if the load is succesful a further 5 days will be executed and again until either a data load fails or the load is up to date.");
            helpIconAbortShouldCancel.SetHelpText("Abort Behaviour", "By default clicking the Abort button (in Controls) will issue an Abort flag on a run which usually results in it completing the current stage (e.g. Migrate RAW to STAGING) then stop.  Ticking this button in a Load Progress based load will make the button instead issue a Cancel notification which means the data load will complete the current LoadProgress and then not start a new one.  This is only an option when you have ticked 'Run Repeatedly' (left)");

            AssociatedCollection = RDMPCollection.DataLoad;

            checkAndExecuteUI1.CommandGetter = AutomationCommandGetter;
            checkAndExecuteUI1.StateChanged += SetButtonStates;
        }
        
        public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _loadMetadata = databaseObject;

            checkAndExecuteUI1.SetItemActivator(activator);

            SetButtonStates(null,null);

            SetLoadProgressGroupBoxState();

        }


        private void SetLoadProgressGroupBoxState()
        {
            _allLoadProgresses = _loadMetadata.LoadProgresses;

            if (_allLoadProgresses.Any())
            {
                //there are some load progresses
                gbLoadProgresses.Visible = true;
                gbDebugOptions.Left = gbLoadProgresses.Right + 3;
                
                //give the user the dropdown options for which load progress he wants to run
                var loadProgressData = new Dictionary<int, string> { { 0, "All available" } };

                foreach (var loadProgress in _allLoadProgresses)
                    loadProgressData.Add(loadProgress.ID, loadProgress.Name);

                ddLoadProgress.DataSource = new BindingSource(loadProgressData, null);
                ddLoadProgress.DisplayMember = "Value";
                ddLoadProgress.ValueMember = "Key";
            }
            else
            {
                gbLoadProgresses.Visible = false;
                gbDebugOptions.Left = 110;
            }
        }

        private void SetButtonStates(object sender, EventArgs e)
        {
            gbLoadProgresses.Enabled = checkAndExecuteUI1.ChecksPassed;
            gbDebugOptions.Enabled = checkAndExecuteUI1.ChecksPassed;
        }

        private void ExecuteHandler(IDataLoadEventListener listener,GracefulCancellationToken token)
        {
            IDataLoadProcess _dataLoadProcess;

            if (_loadMetadata.LoadProgresses.Any())
                if (cbRunIteratively.Checked)
                    _dataLoadProcess = CreateIterativeScheduledDataLoadProcess(listener);
                else
                    _dataLoadProcess = CreateSingleScheduledJobLoadProcess(listener);
            else
                _dataLoadProcess = CreateOnDemandDataLoadProcess(listener);

            RunDataLoadProcess(_dataLoadProcess, listener, token);
        }

        private RDMPCommandLineOptions AutomationCommandGetter(CommandLineActivity activityRequested)
        {
            var lp = GetLoadProgressIfAny();

            var options = new DleOptions
            {
                Command = activityRequested,
                LoadMetadata = _loadMetadata.ID,
                Iterative = cbRunIteratively.Checked
            };

            if (lp != null)
                options.LoadProgress = lp.ID;
            
            return options;
        }


        private ILogManager CreateLogManager(ILoadMetadata loadMetadata)
        {
            return new LogManager(loadMetadata.GetDistinctLoggingDatabaseSettings());
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

        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            if(checkAndExecuteUI1.IsExecuting)
            {
                e.Cancel = true;
                MessageBox.Show("Load is running so you cannot close this window, try clicking Abort first");
            }
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

        #region LoadProcess creation

        private DataLoadProcess CreateOnDemandDataLoadProcess(IDataLoadEventListener listener)
        {
            // Create the 'on demand' data load process, overriding the data provider if one is specified
            var logManager = CreateLogManager(_loadMetadata);
            var pipeline = CreateLoadPipeline(listener,logManager);
            var preExecutionChecker = new PreExecutionChecker(_loadMetadata, _databaseLoadConfiguration);
            return new DataLoadProcess(_loadMetadata, preExecutionChecker, logManager, listener, pipeline);
        }

        private SingleJobScheduledDataLoadProcess CreateSingleScheduledJobLoadProcess(IDataLoadEventListener listener)
        {
            var toAttempt = CreateLoadProgressSelectionStrategy();
            var jobDateGenerationStrategyFactory = new JobDateGenerationStrategyFactory(toAttempt);

            var logManager = CreateLogManager(_loadMetadata);
            var preExecutionChecker = new PreExecutionChecker(_loadMetadata, _databaseLoadConfiguration);
            var pipeline = CreateLoadPipeline(listener, logManager);

            return new SingleJobScheduledDataLoadProcess(_loadMetadata, preExecutionChecker, pipeline,
                jobDateGenerationStrategyFactory,
                CreateLoadProgressSelectionStrategy(), GetNumDaysPerJob(), logManager, listener);
        }

        private int GetNumDaysPerJob()
        {
            return Convert.ToInt32(udDaysPerJob.Value);
        }

        private IterativeScheduledDataLoadProcess CreateIterativeScheduledDataLoadProcess(IDataLoadEventListener listener)
        {
            var toAttempt = CreateLoadProgressSelectionStrategy();
            var jobDateGenerationStrategyFactory = new JobDateGenerationStrategyFactory(toAttempt);
            var logManager = CreateLogManager(_loadMetadata);
            var preExecutionChecker = new PreExecutionChecker(_loadMetadata, _databaseLoadConfiguration);
            var pipeline = CreateLoadPipeline(listener,logManager);
            return new IterativeScheduledDataLoadProcess(_loadMetadata, preExecutionChecker, pipeline, jobDateGenerationStrategyFactory, toAttempt, GetNumDaysPerJob(), logManager, listener);
        }

        #endregion

        private void RunDataLoadProcess(IDataLoadProcess dataLoadProcess,IDataLoadEventListener listener, GracefulCancellationToken token)
        {
            try
            {
                var exitCode = dataLoadProcess.Run(token);
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, TranslateExitCode(exitCode)));
            }
            catch (OperationCanceledException ex)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Data load operation has been cancelled.", ex));
            }
            catch (AggregateException ex)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Entire data load process crashed", ex));
                ex.Handle(e =>
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, e.Message, e));
                    return true;
                });
            }
            catch (Exception e)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Entire data load process crashed", e));
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, e.Message, e));
            }
        }

        private ILoadProgressSelectionStrategy CreateLoadProgressSelectionStrategy()
        {
            var loadProgress = GetLoadProgressIfAny();
             
            if(loadProgress == null)
                return new AnyAvailableLoadProgressSelectionStrategy(_loadMetadata);
            
            return new SingleLoadProgressSelectionStrategy(loadProgress);
        }

        private LoadProgress GetLoadProgressIfAny()
        {
            if (ddLoadProgress.SelectedIndex == 0)
                return null;

            var scheduleItem = (KeyValuePair<int, string>)ddLoadProgress.SelectedItem;
            if (scheduleItem.Key == 0)
                return null;

            return RepositoryLocator.CatalogueRepository.GetObjectByID<LoadProgress>(scheduleItem.Key);
        }

        private IDataLoadExecution CreateLoadPipeline(IDataLoadEventListener listener, ILogManager logManager)
        {
            try
            {
                var repository = (CatalogueRepository)_loadMetadata.Repository;

                // Create the pipeline
                var factory = new HICDataLoadFactory(_loadMetadata, _databaseLoadConfiguration, GetLoadConfigurationFlags(), repository, logManager);
                var loadPipeline = factory.Create(listener);
                return loadPipeline;
            }
            catch (InvalidOperationException e)
            {
                throw new Exception("Error when building load pipeline: " + e);
            }
        }
        
        private void ddLoadProgress_SelectedIndexChanged(object sender, EventArgs e)
        {
            var loadprogress = GetLoadProgressIfAny();

            if (loadprogress == null)
            {
                var progresses = _loadMetadata.LoadProgresses.ToArray();
                if (progresses.Length == 1)
                    udDaysPerJob.Value = progresses[0].DefaultNumberOfDaysToLoadEachTime;
            }
            else
                udDaysPerJob.Value = loadprogress.DefaultNumberOfDaysToLoadEachTime;
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

        private void btnRefreshLoadProgresses_Click(object sender, EventArgs e)
        {
            SetLoadProgressGroupBoxState();
        }

    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DatasetLoadControl_Design, UserControl>))]
    public abstract class DatasetLoadControl_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
    {
    }
}
