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
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using CatalogueManager.TestsAndSetup;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

namespace CatalogueManager.LoadExecutionUIs
{
    public partial class ExecuteCacheProgressUI : CachingEngineUI_Design
    {
        private CachingHost _cachingHost;
        private CacheProgressUI _cacheProgressUI;
        private LoadProgress _selectedLoadProgress;
        private ICacheProgress _cacheProgress;
        
        private CancellationTokenSource _abortTokenSource;
        private CancellationTokenSource _stopTokenSource;
        private Task _currentTask;

        public ExecuteCacheProgressUI()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (VisualStudioDesignMode)
                return;

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            this.Text = "CachingEngine UI - v" + version;

            InitializeCatalogConnection();
            PopulateAvailableLoadProgress();

        }

        private void PopulateAvailableLoadProgress()
        {
            try
            {
                //clear any old values
                ddLoadProgress.Items.Clear();

                //get all load progresses that have associated cache progress
                var loadProgresses = RepositoryLocator.CatalogueRepository.GetAllObjects<CacheProgress>().Select(c => c.LoadProgress).ToArray();
                ddLoadProgress.Items.AddRange(loadProgresses);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Catalogue interrogation error");
                return;
            }

            ddLoadProgress.Enabled = true;
            ddLoadProgress.Sorted = true;
        }

        private void InitializeCatalogConnection()
        {
            //make sure the connection to the Catalogue is configured on the users profile (registry)
            try
            {
                new RegistryRepositoryFinder().CatalogueRepository.TestConnection();
            }
            catch (Exception e)
            {
                //user does not have Catalogue connection configured, make him configure it!
                MessageBox.Show("Problem with Catalogue Connection Settings:" + e.Message);
                
                new ExecuteCommandChoosePlatformDatabase(RepositoryLocator).Execute();
            }
        }

        private void btnStartCaching_Click(object sender, EventArgs e)
        {
            StartCachingJob(RunSingleCacheJob);
        }

        private void btnStartCacheDaemon_Click(object sender, EventArgs e)
        {
            StartCachingJob(RunCacheDaemon);
        }

        private void StartCachingJob(Action<GracefulCancellationToken> action)
        {
            progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Creating caching host for CacheProgress: " + _cacheProgress.GetLoadProgress().Name));
            _cachingHost = new CachingHost( RepositoryLocator.CatalogueRepository);
            _cachingHost.CacheProgressList = new List<ICacheProgress> { _cacheProgress };

            _abortTokenSource = new CancellationTokenSource();
            _stopTokenSource = new CancellationTokenSource();
            var cancellationToken = new GracefulCancellationToken(_stopTokenSource.Token, _abortTokenSource.Token);

            // update UI
            btnStopCaching.Enabled = true;
            btnAbort.Enabled = true;
            btnStartCaching.Enabled = false;

            _currentTask = Task.Run(() => {
                try
                {
                    action(cancellationToken);
                }
                catch (Exception e)
                {
                    progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, e.Message, e));
                }
            });
        }

        private async void StopCachingJob()
        {
            if (_stopTokenSource == null)
                return;

            _stopTokenSource.Cancel();
            progressUI.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "Stopping...this may take some time depending on what the pipeline is up to (and where the next cancellation check is)"));

            await _currentTask;

            progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Caching stopped"));

            btnStopCaching.Enabled = false;
            btnAbort.Enabled = false;
            btnStartCaching.Enabled = true;
        }

        private async void AbortCachingJob()
        {
            if (_abortTokenSource == null)
                return;

            _abortTokenSource.Cancel();
            progressUI.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "Aborting...this may take some time depending on what the pipeline is up to (and where the next cancellation check is)"));

            await _currentTask;

            progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Caching aborted"));

            btnStopCaching.Enabled = false;
            btnAbort.Enabled = false;
            btnStartCaching.Enabled = true;
        }

        private void RunSingleCacheJob(GracefulCancellationToken cancellationToken)
        {
            _cachingHost.RetryMode = false;
            _cachingHost.Start(progressUI, cancellationToken);
        }

        private void RunCacheDaemon(GracefulCancellationToken cancellationToken)
        {
            _cachingHost.RetryMode = false;

            try
            {
                _cachingHost.StartDaemon(progressUI, cancellationToken);
            }
            catch (Exception e)
            {
                progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, e.Message, e));
            }
        }

        private void LoadCacheProgress()
        {
            _cacheProgress = _selectedLoadProgress.CacheProgress;
            if (_cacheProgress == null)
                progressUI.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Error,
                        "The load schedule does not have a CacheProgress (so shouldn't have appeared in the drop-down in the first place...)"));

            ProcessCacheProgressFailures();
        }

        private void btnStopCaching_Click(object sender, EventArgs e)
        {
            StopCachingJob();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            AbortCachingJob();
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            var selectedLoadProgress = (LoadProgress)ddLoadProgress.SelectedItem;
            if (selectedLoadProgress == null)
                return;

            var loadMetadata = selectedLoadProgress.LoadMetadata;
            Process.Start(loadMetadata.LocationOfFlatFiles);
        }

        private void btnRunChecks_Click(object sender, EventArgs e)
        {
            LoadCacheProgress();
            RunPipelineChecks();
        }
        
        private void RunPipelineChecks()
        {
            progressUI.Clear();
            checksUI.Clear();
            btnRunChecks.Enabled = false;
            ddLoadProgress.Enabled = false;

            progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Running Cache Pipeline checks, disabling job control"));
            groupBox1.Enabled = false;

            var checkable = new CachingPreExecutionChecker(_selectedLoadProgress.CacheProgress);
            checksUI.StartChecking(checkable);

            PostPipelineCheck();
        }

        private void PostPipelineCheck()
        {
            progressUI.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Cache Pipeline checks complete, re-enabling job control"));
            groupBox1.Enabled = true;
            btnRunChecks.Enabled = true;
            ddLoadProgress.Enabled = true;
        }

        private void btnStartSingleDateRetrieve_Click(object sender, EventArgs e)
        {
            _abortTokenSource = new CancellationTokenSource();
            _stopTokenSource = new CancellationTokenSource();
            var token = new GracefulCancellationToken(_stopTokenSource.Token, _abortTokenSource.Token);

            var loadMetadata = _cacheProgress.GetLoadProgress().GetLoadMetadata();
            var hicProjectDirectory = new HICProjectDirectory(loadMetadata.LocationOfFlatFiles, false);
            var customDateCaching = new CustomDateCaching(_cacheProgress, RepositoryLocator.CatalogueRepository, hicProjectDirectory);
            
            // If the user has asked to ignore the permission window, use a blank one (means 'can download at any time') otherwise set to null (uses the window belonging to the CacheProgress)
            var permissionWindowOverride = cbIgnorePermissionWindow.Checked ? new PermissionWindow() : null;

            _currentTask = customDateCaching.Fetch(dtpStartDateToRetrieve.Value, dtpEndDateToRetrieve.Value, token, progressUI, permissionWindowOverride);
        }

        private void CachingEngineUI_Load(object sender, EventArgs e)
        {

        }

        private void ddLoadProgress_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedLoadProgress = (LoadProgress)ddLoadProgress.SelectedItem;
            LoadCacheProgress();
            RunPipelineChecks();
        }

        private void ProcessCacheProgressFailures()
        {
            // Check if there are any failures
            var fetchFailures = _cacheProgress.GetAllFetchFailures().Where(f => f.ResolvedOn == null).ToList();
            if (fetchFailures.Any())
            {
                tabFailures.Enabled = true;
                tabFailures.ToolTipText = fetchFailures.Count + " cache fetch failures";
            }
            else
            {
                tabFailures.Enabled = false;
                tabFailures.ToolTipText = "No failed cache fetch requests";
            }
        }

        private void btnShowCachingPipeline_Click(object sender, EventArgs e)
        {
            var mef = RepositoryLocator.CatalogueRepository.MEF;

            var context = CachingPipelineEngineFactory.Context;
            var loadMetadata = _cacheProgress.GetLoadProgress().GetLoadMetadata();

            var uiFactory = new ConfigurePipelineUIFactory(mef, RepositoryLocator.CatalogueRepository);
            var permissionWindow = _cacheProgress.GetPermissionWindow() ?? new PermissionWindow();
            var pipelineForm = uiFactory.Create(context.GetType().GenericTypeArguments[0].FullName,
                _cacheProgress.Pipeline, null, null, context,
                new List<object>
                {
                    new CacheFetchRequestProvider(new CacheFetchRequest(RepositoryLocator.CatalogueRepository)),
                    permissionWindow,
                    new HICProjectDirectory(loadMetadata.LocationOfFlatFiles, false),
                    RepositoryLocator.CatalogueRepository.MEF
                });

            pipelineForm.ShowDialog();
        }

        private void btnShowConfiguration_Click(object sender, EventArgs e)
        {
            _cacheProgressUI = new CacheProgressUI
            {
                Dock = DockStyle.Fill,
                RepositoryLocator = RepositoryLocator
            };

            var form = new Form();
            form.Controls.Add(_cacheProgressUI);
            form.Text = "Cache Configuration";
            form.Size = new Size(_cacheProgressUI.Size.Width + 80, _cacheProgressUI.Size.Height + 80);
            form.MinimumSize = new Size(_cacheProgressUI.MinimumSize.Width + 80, _cacheProgressUI.MinimumSize.Height + 80);

            var saveButton = new Button
            {
                Text = "Save"
            };
            saveButton.Location = new Point(form.Width/2 - saveButton.Width/2, form.Height - saveButton.Height);
            saveButton.Click += OnCacheConfigurationSave;
            form.Controls.Add(saveButton);

            _cacheProgressUI.CacheProgress = (CacheProgress) _cacheProgress;
            
            form.ShowDialog();
        }

        private void OnCacheConfigurationSave(object sender, EventArgs e)
        {
            _cacheProgressUI.CacheProgress.SaveToDatabase();
        }

        private void changeCatalogueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ExecuteCommandChoosePlatformDatabase(RepositoryLocator).Execute();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopulateAvailableLoadProgress();
        }

        private void launchDiagnosticsScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new DiagnosticsScreen(null);
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.ShowDialog();
        }

        private void showPerformanceCounterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new PerformanceCounterUI().Show();
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
            var form = new Form {Text = "Cache Fetch Failures for " + _selectedLoadProgress.Name};
            form.Controls.Add(dgv);
            form.Show();
        }

        private void btnRetryFailures_Click(object sender, EventArgs e)
        {
            StartCachingJob(RetryFailures);
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

        
    }
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CachingEngineUI_Design, UserControl>))]
    public abstract class CachingEngineUI_Design : RDMPSingleDatabaseObjectControl<CacheProgress>
    {
        
    }
}
