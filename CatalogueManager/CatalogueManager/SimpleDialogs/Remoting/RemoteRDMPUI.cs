using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data.Remoting;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.Remoting
{
    /// <summary>
    /// Part of AutomationServiceSlotManagement which lets you change the settings for the currently selected AutomationServiceSlot (See AutomationServiceSlotManagement for full description of
    /// the effects of changes in this control).
    /// </summary>
    public partial class RemoteRDMPUI : RemoteRDMPUI_Design, ISaveableUI
    {
        private RemoteRDMP _remote;

        public RemoteRDMP Remote
        {
            get { return _remote; }
            set
            {
                _remote = value;
            }
        }

        public RemoteRDMPUI()
        {
            InitializeComponent();
        }

        public override void SetDatabaseObject(IActivateItems activator, RemoteRDMP databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            objectSaverButton1.SetupFor(databaseObject, activator.RefreshBus);

            Remote = databaseObject;

            txtName.Text = Remote.Name;
            txtBaseUrl.Text = Remote.URL;
            txtUsername.Text = Remote.Username;
            txtPassword.Text = Remote.Password;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            Remote.Name = txtName.Text;
        }

        private void txtBaseUrl_TextChanged(object sender, EventArgs e)
        {
            Remote.URL = txtBaseUrl.Text;
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            Remote.Username = txtUsername.Text;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            Remote.Password = txtPassword.Text;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RemoteRDMPUI_Design, UserControl>))]
    public abstract class RemoteRDMPUI_Design : RDMPSingleDatabaseObjectControl<RemoteRDMP>
    {
    }
}
