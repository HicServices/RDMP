using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.Collections;
using CatalogueManager.DashboardTabs.Construction;
using CatalogueManager.DashboardTabs.Construction.Exceptions;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.DashboardTabs
{
    /// <summary>
    /// Allows you to create an arrangement of IDashboardableControls on a Form that is stored in the Catalogue database and viewable by all RDMP users.  Use the task bar at the top of the 
    /// screen to add new controls.  Then click the spanner to drag and resize them.  Each control may also have some flexibility in how it is configured which is accessible in edit mode.
    /// </summary>
    public partial class DashboardLayoutUI : DashboardLayoutUI_Design
    {
        private DashboardLayout _layout;
        private DashboardControlFactory _controlFactory;
        private readonly DashboardEditModeFunctionality _editModeFunctionality;

        public Dictionary<DashboardControl,DashboardableControlHostPanel> ControlDictionary = new Dictionary<DashboardControl, DashboardableControlHostPanel>();

        public DashboardLayoutUI()
        {
            InitializeComponent();
            _editModeFunctionality = new DashboardEditModeFunctionality(this);

            AssociatedCollection = RDMPCollection.Catalogue;
        }

        private void btnEditMode_CheckedChanged(object sender, EventArgs e)
        {
            _editModeFunctionality.EditMode = btnEditMode.Checked;
        }

        public override void SetDatabaseObject(IActivateItems activator, DashboardLayout databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _controlFactory = new DashboardControlFactory(activator,RepositoryLocator,new Point(5,25));
            btnAddDashboardControl.Image = activator.CoreIconProvider.GetImage(RDMPConcept.DashboardControl, OverlayKind.Add);
            btnDeleteDashboard.Image = activator.CoreIconProvider.GetImage(RDMPConcept.DashboardLayout, OverlayKind.Delete);
            _layout = databaseObject;
            ReLayout();
        }

        private void ReLayout()
        {
            //remove old controls
            foreach (var kvp in ControlDictionary)
                this.Controls.Remove(kvp.Value);

            //restart audit of controls
            ControlDictionary.Clear();
            
            tbDashboardName.Text = _layout.Name;
            cbxAvailableControls.Items.Clear();
            cbxAvailableControls.Items.AddRange(_controlFactory.GetAvailableControlTypes());
            cbxAvailableControls.SelectedItem = cbxAvailableControls.Items.Cast<object>().FirstOrDefault();

            DashboardableControlHostPanel instance;
            foreach (var c in _layout.Controls)
            {
                try
                {
                    instance = _controlFactory.Create(c);
                }
                catch (DashboardControlHydrationException e)
                {
                    if(MessageBox.Show("Error Hydrating Control '" + c+ "', Do you want to delete the control? (No will attempt to clear the control state but leave it on the Dashboard).  Exception was:"+Environment.NewLine + e.Message,"Delete Broken Control?",MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        c.DeleteInDatabase();
                        continue;
                    }
                    
                    c.PersistenceString = "";
                    c.SaveToDatabase();
                    MessageBox.Show("Control state cleared, we will now try to reload it");
                    instance = _controlFactory.Create(c);
                }

                ControlDictionary.Add(c,instance);
                this.Controls.Add(instance);
                
                //let people know what the edit state is
                _editModeFunctionality.EditMode = btnEditMode.Checked;
            }
        }

        private void tbDashboardName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbDashboardName.Text))
            {
                tbDashboardName.Text = "No Name";
                tbDashboardName.SelectAll();
            }
        }

        private void tbDashboardName_LostFocus(object sender, EventArgs e)
        {
            if (_layout.Name == tbDashboardName.Text)
                return;

            _layout.Name = tbDashboardName.Text;
            _layout.SaveToDatabase();

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_layout));
            
        }

        private void btnAddDashboardControl_Click(object sender, EventArgs e)
        {
            var type = cbxAvailableControls.SelectedItem as Type;

            if(type == null)
                return;

            DashboardableControlHostPanel control;
            var db = _controlFactory.Create(_layout, type, out control);
            this.Controls.Add(control);
            ControlDictionary.Add(db,control);
            Controls.Add(control);
            control.BringToFront();
            
            //add the new control and tell it with the initial edit state is (also updates all the other controls)
            _editModeFunctionality.EditMode = btnEditMode.Checked;
        }

        private void btnDeleteDashboard_Click(object sender, EventArgs e)
        {
            _activator.DeleteWithConfirmation(this, _layout);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DashboardLayoutUI_Design, UserControl>))]
    public abstract class DashboardLayoutUI_Design:RDMPSingleDatabaseObjectControl<DashboardLayout>
    {
        
    }

}
