using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CachingEngine.Factories;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using CatalogueManager.DataLoadUIs;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using CatalogueManager.TestsAndSetup;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;
using ReusableUIComponents;

namespace CachingDashboard
{
    public partial class CachingDashboardForm : RDMPForm
    {
        private ICacheProgress _cacheProgress;
        private ILoadProgress _loadProgress;
        private CancellationTokenSource _cts;
        private Form _pipelineForm;

        public CachingDashboardForm()
        {
            InitializeComponent();

            tvCacheItems.NodeMouseClick += TvCacheItemsOnNodeMouseClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            this.Text = "Caching Dashboard - v" + version;

            PopulateTree();
        }

        private void PopulateTree()
        {
            tvCacheItems.Nodes.Clear();

            tvCacheItems.ImageList = imageListForCacheTree;
            foreach (
                var window in
                    RepositoryLocator.CatalogueRepository.GetAllObjects<PermissionWindow>().Where(window => !string.IsNullOrWhiteSpace(window.Name)))
            {
                var parentNode = new PermissionWindowTreeNode(window);
                if (window.LockedBecauseRunning)
                {
                    parentNode.ImageIndex = 1;
                    parentNode.SelectedImageIndex = 1;
                }

                foreach (var cacheProgress in window.GetAllCacheProgresses())
                    parentNode.Nodes.Add(new CacheProgressTreeNode(cacheProgress));

                tvCacheItems.Nodes.Add(parentNode);
            }

            var unassigned = new PermissionWindow {Name = "Unassigned"};
            var unassignedNode = new PermissionWindowTreeNode(unassigned);
            foreach (var cacheProgress in RepositoryLocator.CatalogueRepository.GetAllCacheProgressWithoutAPermissionWindow())
                unassignedNode.Nodes.Add(new CacheProgressTreeNode(cacheProgress));
            
            tvCacheItems.Nodes.Add(unassignedNode);
            tvCacheItems.ExpandAll();
        }

        private void TvCacheItemsOnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs treeNodeMouseClickEventArgs)
        {
            var parent = treeNodeMouseClickEventArgs.Node.Parent;
            if (parent == null)
            {
                var node = (PermissionWindowTreeNode)treeNodeMouseClickEventArgs.Node;
            }
            else
            {
                var node = (CacheProgressTreeNode)treeNodeMouseClickEventArgs.Node;
                SetCacheProgress(node.CacheProgress);
            }
        }
        private void CachingDashboardForm_Load(object sender, EventArgs e)
        {

        }

        private void SetCacheProgress(ICacheProgress cacheProgress)
        {
            var loadProgress = cacheProgress.GetLoadProgress();

            _cacheProgress = cacheProgress;
            _loadProgress = loadProgress;

            lblResourceIdentifier.Text = loadProgress.Name;
            lblCacheFillProgress.Text = cacheProgress.CacheFillProgress == null ? "None" : cacheProgress.CacheFillProgress.ToString();
            lblCacheLagPeriod.Text = cacheProgress.GetCacheLagPeriod() == null ? "No lag period" : cacheProgress.GetCacheLagPeriod().ToString();
            lblChunkPeriod.Text = cacheProgress.ChunkPeriod.ToString();

            UpdateLockInfo(loadProgress);

            try
            {
                var permissionWindow = cacheProgress.GetPermissionWindow();
                btnShowPermissionWindow.Enabled = (permissionWindow != null);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
                btnShowPermissionWindow.Enabled = false;
            }

            try
            {
                var pipeline = cacheProgress.Pipeline_ID == null ? null : cacheProgress.Pipeline;
                btnShowPipeline.Enabled = (pipeline != null);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
                btnShowPipeline.Enabled = false;
            }

            PopulateCacheFetchErrors(_cacheProgress, _loadProgress);
            CalculateLagShortfall(_cacheProgress);
        }

        private void UpdateLockInfo(ILoadProgress loadProgress)
        {
            lblLockInfo.Text = loadProgress.LockedBecauseRunning ? "Locked by: " + loadProgress.LockHeldBy : "Unlocked";
            btnForceUnlock.Enabled = loadProgress.LockedBecauseRunning;
        }

        private void CalculateLagShortfall(ICacheProgress cacheProgress)
        {
            var shortfall = cacheProgress.GetShortfall();
            lblShortfall.Text = shortfall.Days + " days";
        }

        private void PopulateCacheFetchErrors(ICacheProgress cacheProgress, ILoadProgress loadProgress)
        {
            dgvCacheFetchErrors.Rows.Clear();
            
            var failures = cacheProgress.GetAllFetchFailures();
            foreach (var failure in failures)
            {
                dgvCacheFetchErrors.Rows.Add(new[]
                {
                    failure.LastAttempt.ToString(),
                    failure.FetchRequestStart.ToString(),
                    failure.FetchRequestEnd.ToString(),
                    failure.ExceptionText
                });
            }
        }

        private async void btnShowPipeline_Click(object sender, EventArgs e)
        {
            if (_cts != null)
                _cts.Cancel();

            var newCTS = new CancellationTokenSource();
            _cts = newCTS;

            await ShowPipelineDialog(_cts.Token);

            statusLabel.Text = "Showing PipelineUI";

            // If this call isn't from the latest click of 'Show Pipeline' then let it go
            if (_cts != newCTS) return;

            _cts = null;

            if (_pipelineForm != null)
                _pipelineForm.ShowDialog();

            statusLabel.Text = "";
        }

        private async Task ShowPipelineDialog(CancellationToken cancellationToken)
        {
            _pipelineForm = null;
            statusLabel.Text = "Loading pipeline...";

            try
            {
                await Task.Run(() =>
                {
                    AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => statusLabel.Text = "Loading assembly: " + args.LoadedAssembly.FullName;

                    var context = CachingPipelineEngineFactory.Context;

                    var uiFactory = new ConfigurePipelineUIFactory(RepositoryLocator.CatalogueRepository.MEF, RepositoryLocator.CatalogueRepository);
                    _pipelineForm = uiFactory.Create(context.GetType().GenericTypeArguments[0].FullName,
                        _cacheProgress.Pipeline, null, null, context,
                        new List<object>());

                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }

        private void btnShowPermissionWindow_Click(object sender, EventArgs e)
        {
            var permissionWindow = _cacheProgress.PermissionWindow;
            var form = new PermissionWindowUI();
            form.SetPermissionWindow(permissionWindow);
            form.ShowDialog();
        }

        private void dgvCacheFetchErrors_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var column = dgvCacheFetchErrors.Columns[e.ColumnIndex];
            if (column.Name.Equals("ExceptionText"))
            {
                var cell = dgvCacheFetchErrors.Rows[e.RowIndex].Cells["ExceptionText"];
                MessageBox.Show(cell.Value as string, "Exception Text");
            }
        }

        private void btnForceUnlock_Click(object sender, EventArgs e)
        {
            _loadProgress.Unlock();
            UpdateLockInfo(_loadProgress);
        }

        private void btnRefreshList_Click(object sender, EventArgs e)
        {
            PopulateTree();
        }

        public void Refresh(IMapsDirectlyToDatabaseTable triggeringObject)
        {
            PopulateTree();

        }

        public void RefreshAll()
        {
            PopulateTree();
        }
    }
}
