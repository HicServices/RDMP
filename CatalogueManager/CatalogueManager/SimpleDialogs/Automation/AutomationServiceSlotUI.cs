using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using CatalogueManager.AggregationUIs;
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
    /// Part of AutomationServiceSlotManagement which lets you change the settings for the currently selected AutomationServiceSlot (See AutomationServiceSlotManagement for full description of
    /// the effects of changes in this control).
    /// </summary>
    public partial class AutomationServiceSlotUI : AutomationServiceSlotUI_Design, ISaveableUI
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

                    overrideCommandTimeout.Value = value.GlobalTimeoutPeriod.HasValue
                        ? value.GlobalTimeoutPeriod.Value
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
            }
        }

        public AutomationServiceSlotUI()
        {
            InitializeComponent();
            lockableUI1.Poll = true;
        }

        public override void SetDatabaseObject(IActivateItems activator, AutomationServiceSlot databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            AutomationServiceSlot = databaseObject;
            objectSaverButton1.SetupFor(databaseObject, activator.RefreshBus);
            
            ddDQESelectionStrategy.DataSource = Enum.GetValues(typeof(AutomationDQEJobSelectionStrategy));
            ddDQEFailureStrategy.DataSource = Enum.GetValues(typeof(AutomationFailureStrategy));
            ddDLEFailureStrategy.DataSource = Enum.GetValues(typeof(AutomationFailureStrategy));
            ddCacheFailureStrategy.DataSource = Enum.GetValues(typeof(AutomationFailureStrategy));
        }

        private void ddDQESelectionStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DQESelectionStrategy = (AutomationDQEJobSelectionStrategy) ddDQESelectionStrategy.SelectedItem;
        }

        private void ddDQEFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DQEFailureStrategy = (AutomationFailureStrategy)ddDQEFailureStrategy.SelectedItem;
        }

        private void ddDLEFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutomationServiceSlot.DLEFailureStrategy = (AutomationFailureStrategy)ddDLEFailureStrategy.SelectedItem;
        }

        private void ddCacheFailureStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
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
