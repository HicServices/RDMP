using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CachingEngine.Factories;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;

namespace CatalogueManager.LoadExecutionUIs.CachingDashboard
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

        private void btnShowPermissionWindow_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
