using System;
using System.Windows.Forms;
using CatalogueLibrary.Data.Automation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ReusableUIComponents;

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
    /// next job (if less than the 'Maximum Concurrent Jobs' for that category).  Crashed jobs have to be manually resolved and deleted via AutomationJobUI in the Dashboard, until this time they
    /// still count towards the number of 'currently executing jobs'.  
    /// 
    /// If the strategy is 'Stop' then no new jobs will be started while there is at least 1 outstanding crashed job in a category (e.g. DQE).
    /// 
    /// </summary>
    public partial class AutomationServiceSlotManagement : RDMPForm
    {
        public AutomationServiceSlotManagement()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (VisualStudioDesignMode)
                return;

            RefreshUIFromDatabase();
        }

        private void RefreshUIFromDatabase()
        {
            automationServiceSlots.ClearObjects();
            automationServiceSlots.AddObjects(RepositoryLocator.CatalogueRepository.GetAllObjects<AutomationServiceSlot>());
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            var newSlot = new AutomationServiceSlot(RepositoryLocator.CatalogueRepository);
            automationServiceSlots.AddObject(newSlot);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedObject();
        }

        private void DeleteSelectedObject()
        {
            var deletable = automationServiceSlots.SelectedObject as IDeleteable;

            if(deletable != null)
                if (
                    MessageBox.Show("Are you sure you want to delete slot " + deletable + "?", "Confirm delete",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        deletable.DeleteInDatabase();
                        automationServiceSlotUI1.AutomationServiceSlot = null;
                    }
                    catch (Exception e)
                    {
                        ExceptionViewer.Show(e);
                    }
                }
            
            RefreshUIFromDatabase();
        }

      

        private void automationServiceSlots_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
                DeleteSelectedObject();
        }

        private void automationServiceSlots_SelectedIndexChanged(object sender, EventArgs e)
        {
            var slot = automationServiceSlots.SelectedObject as AutomationServiceSlot;
            automationServiceSlotUI1.AutomationServiceSlot = slot;
        }
    }
}
