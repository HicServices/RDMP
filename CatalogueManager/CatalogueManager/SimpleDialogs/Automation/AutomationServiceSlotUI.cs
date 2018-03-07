using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using CatalogueManager.AggregationUIs;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReusableLibraryCode.Serialization;
using ReusableUIComponents;
using ScintillaNET;

namespace CatalogueManager.SimpleDialogs.Automation
{
    /// <summary>
    /// This dialog lets you configure how often and how many jobs run on your automation server (if you have one) at once.  The first step is to create a new 'Automation Service Slot', this 
    /// is a 'permission to run' object for the automation executable (RDMPAutomationService.exe - supports running as a Windows Service) which can now be started on your Automation server.  
    /// 
    /// When running the RDMPAutomationService executable will lock it's slot so that any other copies can run at the same time.  It will then process DQE, DLE and caching activities as configured
    /// on this dialog with a maximum number of each running at the same time as configured in the 'Maximum Concurrent Jobs' options.
    /// 
    /// If you do not want automation to perform a given task (e.g. DQE) then set the 'Maximum Concurrent Jobs' to 0.  Changing the Maximum Jobs will not cancel any ongoing jobs but it will 
    /// prevent new jobs starting.
    /// 
    /// Changing the 'Failure Strategy' controls how the automation service reacts to one of the async jobs crashing, if it is TryNext then it will leave the job in a crashed state and start the
    /// next job (if less than the 'Maximum Concurrent Jobs' for that category).  Crashed jobs have to be manually resolved and deleted via AutomationServerMonitorUI in the Dashboard, until this
    /// time they still count towards the number of 'currently executing jobs'.  
    /// 
    /// If the strategy is 'Stop' then no new jobs will be started while there is at least 1 outstanding crashed job in a category (e.g. DQE).
    /// </summary>
    public partial class AutomationServiceSlotUI : AutomationServiceSlotUI_Design, ISaveableUI
    {
        private bool _loadingUI;

        public AutomationServiceSlot AutomationServiceSlot { get; private set; }
        
        public AutomationServiceSlotUI()
        {
            InitializeComponent();
            lockableUI1.Poll = true;
            AssociatedCollection = RDMPCollection.Tables;
        }

        public override void SetDatabaseObject(IActivateItems activator, AutomationServiceSlot databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            AutomationServiceSlot = databaseObject;
            objectSaverButton1.SetupFor(databaseObject, activator.RefreshBus);
            
            RefreshUIFromDatabase();
        }

        private void RefreshUIFromDatabase()
        {
            lockableUI1.Lockable = AutomationServiceSlot;

            _loadingUI = true;

            if (AutomationServiceSlot != null)
            {
                automateablePipelineCollectionUI1.AutomationServiceSlot = AutomationServiceSlot;
                automateablePipelineCollectionUI1.Enabled = true;

                groupBox1.Enabled = true;
                groupBox2.Enabled = true;
                groupBox3.Enabled = true;

                tbID.Text = AutomationServiceSlot.ID.ToString();
                dqeMaxJobs.Value = AutomationServiceSlot.DQEMaxConcurrentJobs;
                dqeDaysBetween.Value = AutomationServiceSlot.DQEDaysBetweenEvaluations;

                ddDQESelectionStrategy.DataSource = Enum.GetValues(typeof(AutomationDQEJobSelectionStrategy));
                ddDQESelectionStrategy.SelectedItem = AutomationServiceSlot.DQESelectionStrategy;

                ddDQEFailureStrategy.DataSource = Enum.GetValues(typeof(AutomationFailureStrategy));
                ddDQEFailureStrategy.SelectedItem = AutomationServiceSlot.DQEFailureStrategy;

                dleMaxJobs.Value = AutomationServiceSlot.DLEMaxConcurrentJobs;

                ddDLEFailureStrategy.DataSource = Enum.GetValues(typeof(AutomationFailureStrategy));
                ddDLEFailureStrategy.SelectedItem = AutomationServiceSlot.DLEFailureStrategy;

                cacheMaxJobs.Value = AutomationServiceSlot.CacheMaxConcurrentJobs;

                ddCacheFailureStrategy.DataSource = Enum.GetValues(typeof(AutomationFailureStrategy));
                ddCacheFailureStrategy.SelectedItem = AutomationServiceSlot.CacheFailureStrategy;

                overrideCommandTimeout.Value = AutomationServiceSlot.GlobalTimeoutPeriod.HasValue
                    ? AutomationServiceSlot.GlobalTimeoutPeriod.Value
                    : 0;
            }
            else
            {
                tbID.Text = "";
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
                groupBox3.Enabled = false;
                overrideCommandTimeout.Value = 0;
                automateablePipelineCollectionUI1.Enabled = false;
            }

            _loadingUI = false;
        }

        private void ddDQESelectionStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            AutomationServiceSlot.DQESelectionStrategy = (AutomationDQEJobSelectionStrategy) ddDQESelectionStrategy.SelectedItem;
        }

        private void ddDQEFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            AutomationServiceSlot.DQEFailureStrategy = (AutomationFailureStrategy)ddDQEFailureStrategy.SelectedItem;
        }

        private void ddDLEFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            AutomationServiceSlot.DLEFailureStrategy = (AutomationFailureStrategy)ddDLEFailureStrategy.SelectedItem;
        }

        private void ddCacheFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingUI)
                return;

            AutomationServiceSlot.CacheFailureStrategy = (AutomationFailureStrategy)ddCacheFailureStrategy.SelectedItem;
        }

        private void dqeDaysBetween_ValueChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DQEDaysBetweenEvaluations = (int) dqeDaysBetween.Value;
        }

        private void dqeMaxJobs_ValueChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DQEMaxConcurrentJobs = (int) dqeMaxJobs.Value;
        }

        private void dleMaxJobs_ValueChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DLEMaxConcurrentJobs = (int) dleMaxJobs.Value;
        }

        private void cacheMaxJobs_ValueChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.CacheMaxConcurrentJobs = (int) cacheMaxJobs.Value;
        }

        private void overrideCommandTimeout_ValueChanged(object sender, EventArgs e)
        {
            if (AutomationServiceSlot != null)
                AutomationServiceSlot.GlobalTimeoutPeriod = ((int) overrideCommandTimeout.Value) > 0
                                                          ? ((int?) overrideCommandTimeout.Value)
                                                          : null;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<AutomationServiceSlotUI_Design, UserControl>))]
    public abstract class AutomationServiceSlotUI_Design : RDMPSingleDatabaseObjectControl<AutomationServiceSlot>
    {
    }
}
