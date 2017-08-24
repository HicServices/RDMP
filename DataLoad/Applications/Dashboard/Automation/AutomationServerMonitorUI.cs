using System;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.Collections;
using CatalogueManager.DashboardTabs.Construction;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTableUI;

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
    public partial class AutomationServerMonitorUI : UserControl, IDashboardableControl
    {
        private AutomationServerMonitorUIObjectCollection _collection;
        private IActivateItems _activator;
        private DashboardControl _databaseRecord;

        public AutomationServerMonitorUI()
        {
            InitializeComponent();
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            btnPickServerSlot.Image = activator.CoreIconProvider.GetImage(RDMPConcept.AutomationServiceSlot,OverlayKind.Link);
            _activator = activator;
            _collection = (AutomationServerMonitorUIObjectCollection)collection;
            automationMonitoringRenderAreaUI1.SetupFor(activator, _collection);
        }

        public IPersistableObjectCollection GetCollection()
        {
            return _collection;
        }

        public string GetTabName()
        {
            return Name;
        }

        public void NotifyEditModeChange(bool isEditModeOn)
        {
            if(isEditModeOn)
            {
                this.Controls.Add(toolStrip1);
                automationMonitoringRenderAreaUI1.Bounds = new Rectangle(0,toolStrip1.Bottom,Width,Height - toolStrip1.Bottom);
            }
            else
            {
                this.Controls.Remove(toolStrip1);

                //set it to fill our whole control
                automationMonitoringRenderAreaUI1.Location = new Point(0,0);
                automationMonitoringRenderAreaUI1.Width = Width;
                automationMonitoringRenderAreaUI1.Height = Height;

            }
        }

        public IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord)
        {
            _databaseRecord = databaseRecord;
            return new AutomationServerMonitorUIObjectCollection();
        }

        private void btnPickServerSlot_Click(object sender, EventArgs e)
        {
            var servers = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<AutomationServiceSlot>().ToArray();

            if (!servers.Any())
            {
                MessageBox.Show("You do not currently have any servers, select 'Automation Management' to create one");
                return;
            }

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(servers,true,false);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var server = (AutomationServiceSlot) dialog.Selected;
                _collection.SetServer(server);
                _databaseRecord.SaveCollectionState(_collection);
                automationMonitoringRenderAreaUI1.Invalidate();
                
            }
        }
    }
}
