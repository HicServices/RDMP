using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CachingEngine;
using CachingEngine.Requests;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.TestsAndSetup;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

namespace CatalogueManager.LoadExecutionUIs
{
    /// <summary>
    /// Allows you to execute a Caching pipeline for a series of days.  For example this might download files from a web service by date and store them in a cache directory
    /// for later loading.  Caching is independent of data loading and only required if you have a long running fetch process which is time based and not suitable for
    /// execution as part of the load (due to the length of time it takes or the volatility of the load or just because you want to decouple the two processes).
    /// </summary>
    public partial class ExecuteCacheProgressUI : CachingEngineUI_Design
    {
        private CachingHost _cachingHost;
        private ILoadProgress _loadProgress;
        private ICacheProgress _cacheProgress;
        
        private CancellationTokenSource _abortTokenSource;
        private CancellationTokenSource _stopTokenSource;
        private bool _checksPassed;
        private bool _dataLoadAttempted;
        private bool _dataLoadRunning;

        public ExecuteCacheProgressUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.DataLoad;
        }

        public override void SetDatabaseObject(IActivateItems activator, CacheProgress databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _loadProgress = databaseObject.LoadProgress;
            _cacheProgress = databaseObject;

            rdmpObjectsRibbonUI1.Clear();
            rdmpObjectsRibbonUI1.SetIconProvider(activator.CoreIconProvider);
            rdmpObjectsRibbonUI1.Add((DatabaseEntity) _loadProgress);
            rdmpObjectsRibbonUI1.Add((DatabaseEntity) _cacheProgress);

            SetButtonStates();
        }

        private void SetButtonStates()
        {
            //if there is a task underway (caching)
            if (_dataLoadAttempted)
            {
                //there is a task underway so promote the UI
                progressUI.Visible = true;
                checksUI.Visible = false;

                btnRunChecks.Enabled = !_dataLoadRunning;
                btnExecute.Enabled = false;
                btnAbortLoad.Enabled = _dataLoadRunning;
            }
            else if (!_checksPassed) 
            {
                //checks failed to, do not let them run a cache job
                checksUI.Visible = true;
                progressUI.Visible = false;

                btnRunChecks.Enabled = true;
                btnAbortLoad.Enabled = false;
                btnExecute.Enabled = false;
            }
            else
            {
                //checks have passed, user can execute
                checksUI.Visible = true;
                progressUI.Visible = false;

                btnRunChecks.Enabled = true;
                btnExecute.Enabled = true;
                btnAbortLoad.Enabled = false;
            }

        }

        private void StartCachingJob(Action<GracefulCancellationToken> action)
        {
            progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Creating caching host for CacheProgress: " + _cacheProgress.LoadProgress.Name));
            _cachingHost = new CachingHost( RepositoryLocator.CatalogueRepository);
            _cachingHost.CacheProgressList = new List<ICacheProgress> { _cacheProgress };

            _abortTokenSource = new CancellationTokenSource();
            _stopTokenSource = new CancellationTokenSource();
            var cancellationToken = new GracefulCancellationToken(_stopTokenSource.Token, _abortTokenSource.Token);
            
            //checks are now out of date because cache load attempt has been made
            _checksPassed = false;

            progressUI.Clear();
            
            _dataLoadAttempted = true;
            _dataLoadRunning = true;

            Task.Run(() => {
                                              try
                                              {
                                                  progressUI.ShowRunning(true);
                                                  action(cancellationToken);
                                              }
                                              catch (Exception e)
                                              {
                                                  progressUI.OnNotify(this,
                                                      new NotifyEventArgs(ProgressEventType.Error, e.Message, e));
                                                  _stopTokenSource.Cancel();
                                                  _abortTokenSource.Cancel();
                                              }
                                              finally
                                              {
                                                  _dataLoadRunning = false;
                                                  progressUI.ShowRunning(false);
                                              }
            }).ContinueWith(s => SetButtonStates(), TaskScheduler.FromCurrentSynchronizationContext());

            // update UI
            SetButtonStates();
        }

        private void StopCachingJob()
        {
            if (_stopTokenSource == null)
                return;

            _stopTokenSource.Cancel();
            progressUI.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "Stopping...this may take some time depending on what the pipeline is up to (and where the next cancellation check is)"));
        }

        private void AbortCachingJob()
        {
            if (_abortTokenSource == null)
                return;

            _abortTokenSource.Cancel();
            progressUI.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "Aborting...this may take some time depending on what the pipeline is up to (and where the next cancellation check is)"));
        }

        private void RunSingleCacheJob(GracefulCancellationToken cancellationToken)
        {
            _cachingHost.RetryMode = false;
            _cachingHost.Start(progressUI, cancellationToken);
        }

        private void btnRunChecks_Click(object sender, EventArgs e)
        {
            ProcessCacheProgressFailures();
            RunPipelineChecks();
        }
        
        private void RunPipelineChecks()
        {
            ragChecks.Reset();
            checksUI.Clear();

            _checksPassed = false;
            _dataLoadAttempted = false;
            SetButtonStates();

            btnRunChecks.Enabled = false;
            
            //create a to memory that passes the events to checksui since that's the only one that can respond to proposed fixes
            var toMemory = new ToMemoryCheckNotifier(checksUI);

            Task.Factory.StartNew(() =>
            {
                //run the checks into toMemory / checksUI in a Thread
                var checkable = new CachingPreExecutionChecker(_loadProgress.CacheProgress);
                checkable.Check(toMemory);

            }).ContinueWith(
                t =>
                {
                    //once Thread completes do this on the main UI Thread

                    //find the worst check state
                    var worst = toMemory.GetWorst();
                    //update the rag smiley to reflect whether it has passed
                    ragChecks.OnCheckPerformed(new CheckEventArgs("Checks resulted in " + worst, worst));
                    
                    //update the bit flag
                    _checksPassed = worst <= CheckResult.Warning;

                    //enable other buttons now based on the new state
                    SetButtonStates();

                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        
        private void ProcessCacheProgressFailures()
        {
            // Check if there are any failures
            var anyFailures = _cacheProgress.CacheFetchFailures.Any(f => f.ResolvedOn == null);

            rbRetryFailures.Enabled = anyFailures;
            btnViewFailures.Enabled = anyFailures;
        }
        
        private void btnViewFailures_Click(object sender, EventArgs e)
        {
            // for now just show a modal dialog with a data grid view of all the failure rows
            var dt = new DataTable("CacheFetchFailure");
            
            using (var con = RepositoryLocator.CatalogueRepository.GetConnection())
            {
                var cmd = (SqlCommand)DatabaseCommandHelper.GetCommand("SELECT * FROM CacheFetchFailure WHERE CacheProgress_ID=@CacheProgressID AND ResolvedOn IS NULL", con.Connection);
                cmd.Parameters.AddWithValue("@CacheProgressID", _cacheProgress.ID);
                var reader = cmd.ExecuteReader();
                dt.Load(reader);
            }

            var dgv = new DataGridView {DataSource = dt, Dock = DockStyle.Fill};
            var form = new Form {Text = "Cache Fetch Failures for " + _loadProgress.Name};
            form.Controls.Add(dgv);
            form.Show();
        }
        
        private void RetryFailures(GracefulCancellationToken cancellationToken)
        {
            progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Setting cache retry mode to true."));
            _cachingHost.RetryMode = true;
            _cachingHost.Start(progressUI, cancellationToken);
        }

        private void dtpStartDateToRetrieve_ValueChanged(object sender, EventArgs e)
        {
            if (dtpStartDateToRetrieve.Value > dtpEndDateToRetrieve.Value)
                dtpEndDateToRetrieve.Value = dtpStartDateToRetrieve.Value;
        }

        private void dtpEndDateToRetrieve_ValueChanged(object sender, EventArgs e)
        {
            if (dtpEndDateToRetrieve.Value < dtpStartDateToRetrieve.Value)
                dtpStartDateToRetrieve.Value = dtpEndDateToRetrieve.Value;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if(rbRetryFailures.Checked)
                StartCachingJob(RetryFailures);

            if (rbSpecificDates.Checked)
                StartCachingJob(SpecificDates);

            if (rbNextDates.Checked)
                StartCachingJob(RunSingleCacheJob);
        }

        private void SpecificDates(GracefulCancellationToken token)
        {
            var customDateCaching = new CustomDateCaching(_cacheProgress, RepositoryLocator.CatalogueRepository);

            // If the user has asked to ignore the permission window, use a blank one (means 'can download at any time') otherwise set to null (uses the window belonging to the CacheProgress)
            customDateCaching.Fetch(dtpStartDateToRetrieve.Value, dtpEndDateToRetrieve.Value, token, progressUI, cbIgnorePermissionWindow.Checked);
        }
        
        private void btnAbortLoad_Click(object sender, EventArgs e)
        {
            StopCachingJob();
        }

    }
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CachingEngineUI_Design, UserControl>))]
    public abstract class CachingEngineUI_Design : RDMPSingleDatabaseObjectControl<CacheProgress>
    {
        
    }
}

