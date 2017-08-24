using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs.SubComponents;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using Ticketing;

namespace CatalogueManager.LocationsMenu.Ticketing
{
    /// <summary>
    /// The RDMP recognises that there are a wide array of software systems for tracking time worked, issues,project requests, bug reports etc.  The RDMP is designed to support gated 
    /// interactions with ticketing systems (which can be skipped entirely if you do not want the functionality).  This window lets you configure which ticketing system you have, the
    /// credentials needed to access it and where it is located.  You will need to make sure you select the appropriate Type of ticketing system you have.
    /// 
    /// Because there are many different ticketing systems and they can often be configured in diverse ways, the RDMP uses a 'plugin' approach to interacting with ticketing systems.
    /// The scope of functionality includes: 
    /// 
    /// 1. Validating whether a ticket is valid
    /// 2. Navigating to the ticket when the user clicks 'Show' in a TicketingControl (See TicketingControl)
    /// 3. Determining whether a given project extraction can go ahead (This lets you drive ethics/approvals process through your normal ticketing system but have RDMP prevent 
    /// releases of data until the ticketing system says its ok). 
    /// 
    /// Ticketing systems are entirely optional and you can ignore them if you don't have one or don't want to configure it.  If you do not see a Type that corresponds with your 
    /// ticketing system you might need to write your own Ticketing dll (See ITicketingSystem interface) and upload it as a plugin to the Data Catalogue.
    /// </summary>
    public partial class TicketingSystemConfigurationUI : RDMPUserControl
    {
        private TicketingSystemConfiguration _ticketingSystemConfiguration;
        private const string NoneText = "<<NONE>>";
        public TicketingSystemConfigurationUI()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;

            RefreshUIFromDatabase();
        }

        bool _bLoading = true;
        private IActivateItems _activator;

        private void RefreshUIFromDatabase()
        {
            _bLoading = true;
            _ticketingSystemConfiguration = RepositoryLocator.CatalogueRepository.GetTicketingSystem();
            var mef = RepositoryLocator.CatalogueRepository.MEF;
            
            cbxType.Items.Clear();
            cbxType.Items.AddRange(mef.GetTypes<ITicketingSystem>().Select(t=>t.FullName).ToArray());

            ddCredentials.Items.Clear();
            ddCredentials.Items.Add(NoneText);
            ddCredentials.Items.AddRange(RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>().ToArray());

            if (_ticketingSystemConfiguration == null)
            {
                gbTicketingSystem.Enabled = false;
                tbID.Text = "";
                tbName.Text = "";
                tbUrl.Text = "";
                cbxType.Text = "";

                btnCreate.Enabled = true;
                btnDelete.Enabled = false;
            }
            else
            {
                gbTicketingSystem.Enabled = true;

                tbID.Text = _ticketingSystemConfiguration.ID.ToString();
                tbName.Text = _ticketingSystemConfiguration.Name;
                tbUrl.Text = _ticketingSystemConfiguration.Url;
                cbxType.Text = _ticketingSystemConfiguration.Type;

                if (_ticketingSystemConfiguration.DataAccessCredentials_ID != null)
                    ddCredentials.Text =
                        _ticketingSystemConfiguration.DataAccessCredentials.ToString();
                else
                    ddCredentials.Text = NoneText;

                btnCreate.Enabled = false;
                btnDelete.Enabled = true;
                btnSave.Enabled = false;
            }
            _bLoading = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _ticketingSystemConfiguration.SaveToDatabase();
            btnSave.Enabled = false;
            RefreshUIFromDatabase();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            new TicketingSystemConfiguration(RepositoryLocator.CatalogueRepository,"New Ticketing System");
            RefreshUIFromDatabase();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the Ticketing system from this Catalogue database? there can be only one so be sure before you delete it.","Confirm deleting Ticketing system",MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                _ticketingSystemConfiguration.DeleteInDatabase();
                RefreshUIFromDatabase();
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            if(btnSave.Enabled)
                btnSave_Click(null,null);

            ITicketingSystem instance;
            try
            {
                TicketingSystemFactory factory = new TicketingSystemFactory(RepositoryLocator.CatalogueRepository);
                instance = factory.CreateIfExists(_ticketingSystemConfiguration);

                checksUI1.OnCheckPerformed(
                    new CheckEventArgs("successfully created a instance of " + instance.GetType().FullName,
                        CheckResult.Success));
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(
                    new CheckEventArgs("Could not create ticketing system from your current configuration",
                        CheckResult.Fail, exception));
                return;
            }
            checksUI1.StartChecking(instance);
        }

        private void btnEditCredentials_Click(object sender, EventArgs e)
        {
            var creds = ddCredentials.SelectedItem as DataAccessCredentials;

            if(creds != null)
                _activator.ActivateDataAccessCredentials(this,creds);
        }

        private void btnAddCredentials_Click(object sender, EventArgs e)
        {
            new DataAccessCredentials(RepositoryLocator.CatalogueRepository,"New Data Access Credentials");
            RefreshUIFromDatabase();
        }

        private void btnDeleteCredentials_Click(object sender, EventArgs e)
        {
            try
            {
                if(_ticketingSystemConfiguration.DataAccessCredentials_ID != null)
                {
                    var toDelete = _ticketingSystemConfiguration.DataAccessCredentials;

                    if (
                        MessageBox.Show("Confirm deleting Encrypted Credentials " + toDelete.Name + "?",
                            "Confirm delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        toDelete.DeleteInDatabase();
                }
            }
            catch (Exception ex)
            {  
                ExceptionViewer.Show(ex);
            }
            RefreshUIFromDatabase();
        }

        private void tb_TextChanged(object sender, EventArgs e)
        {
            if (_bLoading)
                return;
            
            _ticketingSystemConfiguration.Name = tbName.Text;
            _ticketingSystemConfiguration.Url = tbUrl.Text;
            _ticketingSystemConfiguration.Type = cbxType.Text;
            btnSave.Enabled = true;
        }

        private void ddCredentials_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_bLoading)
                return;


            var creds = ddCredentials.SelectedItem as DataAccessCredentials;

            if (creds == null)
            {
                _ticketingSystemConfiguration.DataAccessCredentials_ID = null;
                _ticketingSystemConfiguration.SaveToDatabase();
            }
            else
            {
                _ticketingSystemConfiguration.DataAccessCredentials_ID = creds.ID;
                _ticketingSystemConfiguration.SaveToDatabase();
            }
        }

        public void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
        }
    }
}
