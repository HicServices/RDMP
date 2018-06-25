using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using ReusableUIComponents;

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

            rdmpObjectsRibbonUI1.SetIconProvider(activator.CoreIconProvider);
            rdmpObjectsRibbonUI1.Clear();
            rdmpObjectsRibbonUI1.Add(_loadMetadata);

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
        
        private RDMPCommandLineOptions AutomationCommandGetter(CommandLineActivity activityRequested)
        {
            var lp = GetLoadProgressIfAny();

            var options = new DleOptions
            {
                Command = activityRequested,
                LoadMetadata = _loadMetadata.ID,
                Iterative = cbRunIteratively.Checked,
                DaysToLoad = Convert.ToInt32(udDaysPerJob.Value),
                DoNotArchiveData = cbSkipArchiving.Checked,
                StopAfterRAW = !cbMigrateRAWToStaging.Checked,
                StopAfterSTAGING = !cbMigrateStagingToLive.Checked,
            };

            if (lp != null)
                options.LoadProgress = lp.ID;
            
            return options;
        }

        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            if(checkAndExecuteUI1.IsExecuting)
            {
                e.Cancel = true;
                MessageBox.Show("Load is running so you cannot close this window, try clicking Abort first");
            }
        }
        
        private LoadProgress GetLoadProgressIfAny()
        {
            if (ddLoadProgress.SelectedIndex == 0 || ddLoadProgress.SelectedIndex == -1)
                return null;

            var scheduleItem = (KeyValuePair<int, string>)ddLoadProgress.SelectedItem;
            if (scheduleItem.Key == 0)
                return null;

            return RepositoryLocator.CatalogueRepository.GetObjectByID<LoadProgress>(scheduleItem.Key);
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

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            _activator.ActivateViewLog(_loadMetadata);
        }

        private void btnLoadDiagram_Click(object sender, EventArgs e)
        {
            _activator.ActivateViewLoadMetadataDiagram(this, _loadMetadata);
        }

    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DatasetLoadControl_Design, UserControl>))]
    public abstract class DatasetLoadControl_Design : RDMPSingleDatabaseObjectControl<LoadMetadata>
    {
    }
}
