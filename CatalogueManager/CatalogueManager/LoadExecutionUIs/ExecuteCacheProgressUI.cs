using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPAutomationService.Options;
using ReusableLibraryCode;
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
        private ICacheProgress _cacheProgress;
        
        public ExecuteCacheProgressUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.DataLoad;
            checkAndExecuteUI1.CommandGetter += CommandGetter;
        }

        private RDMPCommandLineOptions CommandGetter(CommandLineActivity commandLineActivity)
        {
            return new CacheOptions()
            {
                CacheProgress = _cacheProgress.ID,
                Command = commandLineActivity,
                RetryMode = cbFailures.Checked
            };
        }

        public override void SetDatabaseObject(IActivateItems activator, CacheProgress databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _cacheProgress = databaseObject;

            rdmpObjectsRibbonUI1.Clear();
            rdmpObjectsRibbonUI1.SetIconProvider(activator.CoreIconProvider);
            rdmpObjectsRibbonUI1.Add((DatabaseEntity) _cacheProgress.LoadProgress);
            rdmpObjectsRibbonUI1.Add((DatabaseEntity) _cacheProgress);

            bool failures = _cacheProgress.CacheFetchFailures.Any(f => f.ResolvedOn == null);
            btnViewFailures.Enabled = failures;
            cbFailures.Enabled = failures;

            checkAndExecuteUI1.SetItemActivator(activator);
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
            var form = new Form {Text = "Cache Fetch Failures for " + _cacheProgress.LoadProgress.Name};
            form.Controls.Add(dgv);
            form.Show();
        }
    }
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CachingEngineUI_Design, UserControl>))]
    public abstract class CachingEngineUI_Design : RDMPSingleDatabaseObjectControl<CacheProgress>
    {
        
    }
}

