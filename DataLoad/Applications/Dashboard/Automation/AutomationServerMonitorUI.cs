using System;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.Collections;
using CatalogueManager.DashboardTabs.Construction;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace Dashboard.Automation
{
    /// <summary>
    /// Allows you to see the state of a remote RDMP Automation Server Slot.  This includes whether there is an Automation executable that has locked the slot, what jobs that executable
    /// is currently running and the state of those jobs and it's state (whether it has crashed etc).  Also includes the last 'check in' time, which is when the Automation Executable last
    /// checked in with the database to remind everyone it is still alive.
    /// 
    /// Also visible as red Flags are a lists of all the errors encountered by the automation service. If there are flags appearing in this control then you should go check lifeline / last checked
    /// in counter on the right to see if the automation process to see if it is still running.  You can view and resolve the flags by clicking on them.  Typing in an Explanation will resolve the 
    /// error such that it no longer appears on the dashboard. If you do this make sure you have fixed the underlying problem and relaunched the automation service (if required).
    /// </summary>
    public partial class AutomationServerMonitorUI : AutomationServerMonitorUI_Design
    {
        private AutomationServerMonitorUIObjectCache _cache;
        
        public AutomationServerMonitorUI()
        {
            InitializeComponent();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public override void SetDatabaseObject(IActivateItems activator, AutomationServiceSlot databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _cache = new AutomationServerMonitorUIObjectCache(databaseObject);
            automationMonitoringRenderAreaUI1.SetupFor(activator, _cache);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<AutomationServerMonitorUI_Design, UserControl>))]
    public abstract class AutomationServerMonitorUI_Design : RDMPSingleDatabaseObjectControl<AutomationServiceSlot>
    {
        
    }
}
