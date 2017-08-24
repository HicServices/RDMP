using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs
{
    /// <summary>
    /// Part of LoadProgressManagement, let's you configure the settings of a LoadProgress (see LoadProgressManagement for a description of what a LoadProgress is).
    /// 
    /// Each LoadProgress can be tied to a Cache progress.  If you are using a LoadProgress without a cache then it is up to your load implementation to respect the time period being loaded 
    /// (e.g. when using RemoteSQLTableAttacher you should make use of the @startDate and @endDate parameters in your fetch query).  See CacheProgressUI for a description of caching and 
    /// permission windows.
    /// </summary>
    public partial class LoadProgressUI : LoadProgressUI_Design
    {
        private LoadProgress _loadProgress;
        private bool bLoading;
        
        public event EventHandler Saved;

        public LoadProgressUI()
        {
            InitializeComponent();
            cacheProgressUI1.CacheProgressDeleted += ReloadUIFromDatabase;
            loadProgressDiagram1.LoadProgressChanged += ReloadUIFromDatabase;
        }
        
        public LoadProgress LoadProgress
        {
            get { return _loadProgress; }
            set
            {
                cacheProgressUI1.Enabled = value != null;
                bLoading = true;
                
                AllowUserToSaveChanges();

                _loadProgress = value;

                ReloadUIFromDatabase();

                
                bLoading = false;
            }
        }

        private void ReloadUIFromDatabase()
        {
            if (_loadProgress != null)
            {
                loadProgressDiagram1.LoadProgress = _loadProgress;
                loadProgressDiagram1.Visible = true;
            }
            else
                loadProgressDiagram1.Visible = false;

            tbDataLoadProgress.ReadOnly = true;

            if (_loadProgress == null)
            {
                btnConfigureCaching.Visible = true;

                tbID.Text = "";
                //dtpOriginDate.Value = DateTime.MinValue;
                tbOriginDate.Text = null;
                tbName.Text = null;

                groupBox1.Visible = false;
                cacheProgressUI1.Visible = false;

                btnSave.Visible = false;
            }
            else
            {
                cacheProgressUI1.Visible = true;
                
                groupBox1.Visible = true;
                
                if(_loadProgress.OriginDate != null)
                    tbOriginDate.Text = _loadProgress.OriginDate.ToString();
                
                tbID.Text = LoadProgress.ID.ToString();
                tbName.Text = LoadProgress.Name;

                tbDataLoadProgress.Text = LoadProgress.DataLoadProgress != null ? LoadProgress.DataLoadProgress.ToString() : "";

                //If there is no caching yet then the button is enabled otherwise it isn't
                btnConfigureCaching.Enabled = _loadProgress.CacheProgress == null;

                cacheProgressUI1.CacheProgress = _loadProgress.CacheProgress;

                nDefaultNumberOfDaysToLoadEachTime.Value = LoadProgress.DefaultNumberOfDaysToLoadEachTime;
                cbEnableAutomation.Checked = _loadProgress.AllowAutomation;

                btnSave.Visible = true;
            }
        }

        private void AllowUserToSaveChanges()
        {
            if (btnSave.Enabled && _loadProgress != null && _loadProgress.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent)
                if (MessageBox.Show("Save changes to " + _loadProgress + "?", "Save changes?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    _loadProgress.SaveToDatabase();
            
        }

        protected override bool ProcessKeyPreview(ref Message m)
        {
            PreviewKey p = new PreviewKey(ref m, ModifierKeys);

            if (p.IsKeyDownMessage && p.e.KeyCode == Keys.S && p.e.Control)
            {
                btnSave_Click(null,null);
                p.Trap(this);
            }

            return base.ProcessKeyPreview(ref m);
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if(bLoading)
                return;

            LoadProgress.Name = tbName.Text;
            
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(LoadProgress!= null)
            {
                LoadProgress.SaveToDatabase();

                if(cacheProgressUI1.CacheProgress != null)
                    cacheProgressUI1.CacheProgress.SaveToDatabase();
                
                if (Saved != null)
                    Saved(this, new EventArgs());

                btnSave.Enabled = false;

                ReloadUIFromDatabase();

                Task.Delay(1000).ContinueWith(delegate { Invoke(new MethodInvoker(() => { btnSave.Enabled = true; })); });



            }
        }
        private void btnConfigureCaching_Click(object sender, EventArgs e)
        {
            var cacheProgress = _loadProgress.CacheProgress;

            // If the LoadProgress doesn't have a corresponding CacheProgress, create it
            if (cacheProgress == null)
            {
                cacheProgress = new CacheProgress(RepositoryLocator.CatalogueRepository, _loadProgress);
                _loadProgress.SaveToDatabase();
            }
            cacheProgressUI1.CacheProgress = (CacheProgress)cacheProgress;
            cacheProgressUI1.Visible = true;

            btnConfigureCaching.Enabled = false;
        }
        
        private void nDefaultNumberOfDaysToLoadEachTime_ValueChanged(object sender, EventArgs e)
        {

            _loadProgress.DefaultNumberOfDaysToLoadEachTime = Convert.ToInt32(nDefaultNumberOfDaysToLoadEachTime.Value);
        }

        private void btnEditLoadProgress_Click(object sender, EventArgs e)
        {
            tbDataLoadProgress.ReadOnly = false;
        }

        private void tbDataLoadProgress_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbDataLoadProgress.Text))
                    _loadProgress.DataLoadProgress = null;
                else
                    _loadProgress.DataLoadProgress = DateTime.Parse(tbDataLoadProgress.Text);

                tbDataLoadProgress.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbDataLoadProgress.ForeColor = Color.Red;
            }
        }

        private void tbOriginDate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(tbOriginDate.Text))
                    _loadProgress.OriginDate = null;
                else
                    _loadProgress.OriginDate = DateTime.Parse(tbOriginDate.Text);

                tbOriginDate.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbOriginDate.ForeColor = Color.Red;
            }
        }

        private void cbEnableAutomation_CheckedChanged(object sender, EventArgs e)
        {
            _loadProgress.AllowAutomation = cbEnableAutomation.Enabled;
            _loadProgress.SaveToDatabase();
        }

        public override void SetDatabaseObject(IActivateItems activator, LoadProgress databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            LoadProgress = databaseObject;

        }
    }

    public abstract class LoadProgressUI_Design:RDMPSingleDatabaseObjectControl<LoadProgress>
    {
    }
}
