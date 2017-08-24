using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs
{
    /// <summary>
    /// Caching is method by which long term transfer tasks take place in the RDMP.  These are usually of files and are expected to run all the time (up to 24/7).  Cached data must always be 
    /// temporal (i.e. a given set of files must correspond to a specific time) such that cache requests for a specific date/time do not vary in real time.  The exact implementation of any
    /// caching task is done through a Pipeline.  Since caching is super bespoke, it is anticipated that you will have written your own caching data classes for use in your pipeline.
    /// 
    /// Clicking 'Configure Caching Pipeline' will let you setup what happens during the caching activity.
    /// 
    /// Changing the 'Lag Period' to a positive number will indicate a period of time in which to NOT cache data (e.g. if you set it to 30 days then caching will always be suspended when it 
    /// has cached up to 1 month ago).
    /// 
    /// Setting a 'Permission Window' will create a restriction on the times of day in which caching can take place (e.g. between midnight and 4am only).
    /// </summary>
    public partial class CacheProgressUI : RDMPUserControl
    {
        private CacheProgress _cacheProgress;

        public event Action CacheProgressDeleted = delegate { };

        public CacheProgress CacheProgress
        {
            get
            {
                return _cacheProgress;
            }
            set
            {
                _cacheProgress = value;
                RefreshUIFromDatabase();
            }
        }
        
        public CacheProgressUI()
        {
            InitializeComponent();
        }

        private bool _bLoading = false;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;
            
            
            RefreshUIFromDatabase();
        }

        private void RefreshUIFromDatabase()
        {
            _bLoading = true;
            ddCacheLagDurationType.DataSource = Enum.GetValues(typeof(CacheLagPeriod.PeriodType));
            ddCacheLagDelayDurationType.DataSource = Enum.GetValues(typeof(CacheLagPeriod.PeriodType));

            if (RepositoryLocator == null)
                return;
            
            if (CacheProgress == null)
                ClearCacheProgressPanel();
            else
                PopulateCacheProgressPanel(CacheProgress);
            
            _bLoading = false;
        }

        private void PopulateCacheProgressPanel(ICacheProgress cacheProgress)
        {
            gbCacheProgress.Enabled = true; 
            tbCacheProgressID.Text = cacheProgress.ID.ToString();
            tbCacheProgress.Text = cacheProgress.CacheFillProgress.HasValue ? cacheProgress.CacheFillProgress.ToString() : "Caching not started";
            
            var cacheLagPeriod = cacheProgress.GetCacheLagPeriod();
            if (cacheLagPeriod != null)
            {
                udCacheLagDuration.Value = cacheLagPeriod.Duration;
                ddCacheLagDurationType.SelectedItem = cacheLagPeriod.Type;
            }

            tbChunkPeriod.Text = cacheProgress.ChunkPeriod.ToString();

            cbPermissionWindowRequired.Checked = cacheProgress.PermissionWindow_ID != null;
            EnablePermissionWindowUI(cacheProgress.PermissionWindow_ID != null);

            UpdateCacheLagPeriodControl();
        }

        private void ClearCacheProgressPanel()
        {
            tbCacheProgressID.Text = "";
            gbCacheProgress.Enabled = false;
        }

        private void UpdateCacheLagPeriodControl()
        {
            if (CacheProgress == null)
                return;

            var cacheLagPeriod = CacheProgress.GetCacheLagPeriod();
            if (cacheLagPeriod == null)
            {
                udCacheLagDuration.Value = 0;
                ddCacheLagDurationType.SelectedItem = CacheLagPeriod.PeriodType.Day;
            }
            else
            {
                udCacheLagDuration.Value = cacheLagPeriod.Duration;
                ddCacheLagDurationType.SelectedItem = cacheLagPeriod.Type;
            }

            var cacheLoadDelayPeriod = CacheProgress.GetCacheLagPeriodLoadDelay();
            udCacheLagDelayPeriodDuration.Value = cacheLoadDelayPeriod.Duration;
            ddCacheLagDelayDurationType.SelectedItem = cacheLoadDelayPeriod.Type;

        }

        private void btnConfigureCachingPipeline_Click(object sender, EventArgs e)
        {
            // Associate a new caching pipeline on-the-fly
            IPipeline pipeline;
            if (CacheProgress.Pipeline_ID == null)
            {
                pipeline = new Pipeline(RepositoryLocator.CatalogueRepository, "CachingPipeline_" + Guid.NewGuid());
                CacheProgress.Pipeline_ID = pipeline.ID;
                CacheProgress.SaveToDatabase();
            }
            else
                pipeline = CacheProgress.Pipeline;

            HICProjectDirectory projectDirectory;
            var lmd = CacheProgress.GetLoadProgress().GetLoadMetadata();

            try
            {   
                projectDirectory = new HICProjectDirectory(lmd.LocationOfFlatFiles, false);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Could not generate/edit Cache Pipeline because of a problem with the LoadMetadata '"+lmd+"' project directory.  See Inner Exception for specifics" ,exception);
                return;
            }
            //var engine = engineFactory.Create(cacheProgress, new ToConsoleDataLoadEventreceiver());
            var context = CachingPipelineEngineFactory.Context;

            var uiFactory = new ConfigurePipelineUIFactory(RepositoryLocator.CatalogueRepository.MEF, RepositoryLocator.CatalogueRepository);
            var form = uiFactory.Create(context.GetType().GenericTypeArguments[0].FullName, pipeline, null, null, context, 
                new List<object>
                {
                    new CacheFetchRequestProvider(new CacheFetchRequest(pipeline.Repository,DateTime.Now)),
                    CacheProgress.GetPermissionWindow() ?? new PermissionWindow(){Repository = pipeline.Repository},
                          projectDirectory,
                        RepositoryLocator.CatalogueRepository.MEF
                
                });

            form.ShowDialog();
        }

        private void btnAddNewPermissionWindow_Click(object sender, EventArgs e)
        {
            var permissionWindow = new PermissionWindow(RepositoryLocator.CatalogueRepository);
            var form = new PermissionWindowUI();
            form.SetPermissionWindow(permissionWindow);

            if (form.ShowDialog() != DialogResult.OK)
                permissionWindow.DeleteInDatabase();
            else
            {
                CacheProgress.PermissionWindow_ID = permissionWindow.ID;
                CacheProgress.SaveToDatabase();

                RefreshUIFromDatabase();
            }
        }

        private void btnEditPermissionWindow_Click(object sender, EventArgs e)
        {
            var permissionWindow = ddPermissionWindow.SelectedItem as IPermissionWindow;

            if(permissionWindow == null)
                return;

            var form = new PermissionWindowUI();
            form.SetPermissionWindow(permissionWindow);
            form.ShowDialog();
            RefreshUIFromDatabase();
        }

        private void cbPermissionWindowRequired_CheckedChanged(object sender, EventArgs e)
        {
            if(_bLoading)
                return;

            EnablePermissionWindowUI(cbPermissionWindowRequired.Checked);

            if (cbPermissionWindowRequired.Checked)
            {
                var permissionWindow = ddPermissionWindow.SelectedItem as IPermissionWindow;
                if (permissionWindow != null)
                    CacheProgress.PermissionWindow_ID = permissionWindow.ID;
            }
            else
                CacheProgress.PermissionWindow_ID = null;

            CacheProgress.SaveToDatabase();
        }

        private void EnablePermissionWindowUI(bool enabled)
        {
            ddPermissionWindow.Enabled = enabled;
            btnAddNewPermissionWindow.Enabled = enabled;
            btnEditPermissionWindow.Enabled = enabled;
            btnDeletePermissionWindow.Enabled = enabled;

            if (CacheProgress == null)
                return;
            
            ddPermissionWindow.Items.Clear();
            ddPermissionWindow.Items.AddRange(CacheProgress.Repository.GetAllObjects<PermissionWindow>());

            if (CacheProgress.PermissionWindow_ID != null)
                ddPermissionWindow.SelectedItem = ddPermissionWindow.Items.Cast<PermissionWindow>().First(window => window.ID == CacheProgress.PermissionWindow_ID);
        }

        private void ddPermissionWindow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoading)
                return;
            
            _bLoading = true;
            var permissionWindow = ddPermissionWindow.SelectedItem as IPermissionWindow;
            if (permissionWindow == null)
            {
                CacheProgress.PermissionWindow_ID = null;
                EnablePermissionWindowUI(false);
            }
            else
            {
                CacheProgress.PermissionWindow_ID = permissionWindow.ID;
                EnablePermissionWindowUI(true);
            }
            CacheProgress.SaveToDatabase();
            _bLoading = false;
        }

        private void udCacheLagDuration_ValueChanged(object sender, EventArgs e)
        {
            if (_bLoading)
                return;

            CacheProgress.SetCacheLagPeriod(CreateNewCacheLagPeriod(udCacheLagDuration.Value, ddCacheLagDurationType.SelectedItem));
            CacheProgress.SaveToDatabase();
        }

        private void ddCacheLagDurationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoading)
                return;

            CacheProgress.SetCacheLagPeriod(CreateNewCacheLagPeriod(udCacheLagDuration.Value, ddCacheLagDurationType.SelectedItem));
            CacheProgress.SaveToDatabase();
        }

        private void ddCacheLagDelayDurationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoading)
                return;

            CacheProgress.SetCacheLagPeriodLoadDelay(CreateNewCacheLagPeriod(udCacheLagDelayPeriodDuration.Value, ddCacheLagDelayDurationType.SelectedItem));
            CacheProgress.SaveToDatabase();
        }
        private void udCacheLagDelayPeriodDuration_ValueChanged(object sender, EventArgs e)
        {
            if (_bLoading)
                return;

            CacheProgress.SetCacheLagPeriodLoadDelay(CreateNewCacheLagPeriod(udCacheLagDelayPeriodDuration.Value, ddCacheLagDelayDurationType.SelectedItem));
            CacheProgress.SaveToDatabase();
        }

        // Returns null if there is no duration, otherwise picks up the current state of both Duration and Type UI elements
        private CacheLagPeriod CreateNewCacheLagPeriod(decimal value, object selectedItem)
        {
            var duration = Convert.ToInt32(value);
            CacheLagPeriod cacheLagPeriod = null;

            if (selectedItem != null)
                cacheLagPeriod = new CacheLagPeriod(duration, (CacheLagPeriod.PeriodType)selectedItem);

            return cacheLagPeriod;
        }
        
        private void btnDeletePermissionWindow_Click(object sender, EventArgs e)
        {
            try
            {
                var window = ddPermissionWindow.SelectedItem as PermissionWindow;
                if(window == null)
                    return;

                var otherUsers = RepositoryLocator.CatalogueRepository.GetAllObjects<CacheProgress>().Where(c => c.PermissionWindow_ID == window.ID).Except(new[] { CacheProgress }).ToArray();

                if (otherUsers.Any())
                {
                    MessageBox.Show(
                        "Cannot delete permission window because it is used by another cache progress(es) (" +
                        string.Join(",", otherUsers.Select(u => u.ToString())));
                    return;
                }

                if (
                        MessageBox.Show(
                            "Are you SURE you want to delete the permission window " + window,
                            "Confirm deleting permission window?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    CacheProgress.PermissionWindow_ID = null;
                    CacheProgress.SaveToDatabase();

                    window.DeleteInDatabase();

                    ddPermissionWindow.SelectedItem = null;
                    cbPermissionWindowRequired.Checked = false;

                    ddPermissionWindow.Items.Clear();
                    ddPermissionWindow.Items.AddRange(RepositoryLocator.CatalogueRepository.GetAllObjects<PermissionWindow>());
                }
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        private void btnDeleteCaching_Click(object sender, EventArgs e)
        {
            try
            {
                if (CacheProgress != null)
                {
                    if (
                        MessageBox.Show(
                            "Are you SURE you want to delete the caching configuration? this may result in loosing important information about what data you have cached and what data cache fetches failed (holes in your data)?",
                            "Confirm deleting cache configuration?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {

                        CacheProgress.DeleteInDatabase();
                        CacheProgress = null;
                        CacheProgressDeleted();
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}
