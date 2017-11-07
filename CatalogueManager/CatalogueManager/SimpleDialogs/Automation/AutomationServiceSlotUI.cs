using System;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using CatalogueManager.SimpleDialogs.Revertable;
using MapsDirectlyToDatabaseTable;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReusableLibraryCode.Serialization;
using ScintillaNET;

namespace CatalogueManager.SimpleDialogs.Automation
{
    /// <summary>
    /// Part of AutomationServiceSlotManagement which lets you change the settings for the currently selected AutomationServiceSlot (See AutomationServiceSlotManagement for full description of
    /// the effects of changes in this control).
    /// </summary>
    public partial class AutomationServiceSlotUI : UserControl
    {
        private AutomationServiceSlot _automationServiceSlot;

        public AutomationServiceSlot AutomationServiceSlot
        {
            get { return _automationServiceSlot; }
            set
            {
                _automationServiceSlot = value;
                
                lockableUI1.Lockable = value;
                

                if (value != null)
                {
                    automateablePipelineCollectionUI1.AutomationServiceSlot = AutomationServiceSlot;
                    automateablePipelineCollectionUI1.Enabled = true;

                    groupBox1.Enabled = true;
                    groupBox2.Enabled = true;
                    groupBox3.Enabled = true;

                    tbID.Text = value.ID.ToString();
                    dqeMaxJobs.Value = value.DQEMaxConcurrentJobs;
                    dqeDaysBetween.Value = value.DQEDaysBetweenEvaluations;
                    ddDQESelectionStrategy.SelectedItem = value.DQESelectionStrategy;
                    ddDQEFailureStrategy.SelectedItem = value.DQEFailureStrategy;

                    dleMaxJobs.Value = value.DLEMaxConcurrentJobs;
                    ddDLEFailureStrategy.SelectedItem = value.DLEFailureStrategy;

                    cacheMaxJobs.Value = value.CacheMaxConcurrentJobs;
                    ddCacheFailureStrategy.SelectedItem = value.CacheFailureStrategy;
                    
                    overrideCommandTimeout.Value = value.GlobalTimeoutPeriod.HasValue?value.GlobalTimeoutPeriod.Value :0;
                    button1.Enabled = true;
                }
                else
                {
                    tbID.Text = "";
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                    groupBox3.Enabled = false;
                    overrideCommandTimeout.Value = 0;
                    button1.Enabled = false;
                    automateablePipelineCollectionUI1.Enabled = false;
                }
            }
        }

        public AutomationServiceSlotUI()
        {
            InitializeComponent();
            ddDQESelectionStrategy.DataSource = Enum.GetValues(typeof(AutomationDQEJobSelectionStrategy));
            ddDQEFailureStrategy.DataSource = Enum.GetValues(typeof(AutomationFailureStrategy));
            ddDLEFailureStrategy.DataSource = Enum.GetValues(typeof (AutomationFailureStrategy));
            ddCacheFailureStrategy.DataSource = Enum.GetValues(typeof (AutomationFailureStrategy));
            lockableUI1.Poll = true;
        }

        private void ddDQESelectionStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DQESelectionStrategy = (AutomationDQEJobSelectionStrategy) ddDQESelectionStrategy.SelectedItem;
            AutomationServiceSlot.SaveToDatabase();
        }

        private void ddDQEFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DQEFailureStrategy = (AutomationFailureStrategy) ddDQEFailureStrategy.SelectedItem;
            AutomationServiceSlot.SaveToDatabase();
        }

        private void ddDLEFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DLEFailureStrategy = (AutomationFailureStrategy) ddDLEFailureStrategy.SelectedItem;
            AutomationServiceSlot.SaveToDatabase();
        }

        private void ddCacheFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.CacheFailureStrategy = (AutomationFailureStrategy)ddCacheFailureStrategy.SelectedItem;
            AutomationServiceSlot.SaveToDatabase();
        }

        private void dqeDaysBetween_ValueChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DQEDaysBetweenEvaluations = (int) dqeDaysBetween.Value;
            AutomationServiceSlot.SaveToDatabase();
        }

        private void dqeMaxJobs_ValueChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DQEMaxConcurrentJobs = (int) dqeMaxJobs.Value;
            AutomationServiceSlot.SaveToDatabase();
        }

        private void dleMaxJobs_ValueChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DLEMaxConcurrentJobs = (int) dleMaxJobs.Value;
            AutomationServiceSlot.SaveToDatabase();
        }

        private void cacheMaxJobs_ValueChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.CacheMaxConcurrentJobs = (int) cacheMaxJobs.Value;
            AutomationServiceSlot.SaveToDatabase();
        }

        private void overrideCommandTimeout_ValueChanged(object sender, EventArgs e)
        {
            if(AutomationServiceSlot != null)
            {
                AutomationServiceSlot.GlobalTimeoutPeriod = ((int)overrideCommandTimeout.Value)>0? ((int?)overrideCommandTimeout.Value):null;
                AutomationServiceSlot.SaveToDatabase();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (AutomationServiceSlot != null)
            {
                var ignoreRepoResolver = new IgnorableSerializerContractResolver();
                ignoreRepoResolver.Ignore(typeof(DatabaseEntity), new []{ "Repository" });

                var settings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = ignoreRepoResolver,
                };
                var json = JsonConvert.SerializeObject(AutomationServiceSlot, Formatting.Indented, settings);
                //MessageBox.Show(this, json);
            }
        }
    }
}
