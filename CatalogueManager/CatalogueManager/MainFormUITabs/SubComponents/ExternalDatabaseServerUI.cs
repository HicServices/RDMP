using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;

namespace CatalogueManager.MainFormUITabs.SubComponents
{
    /// <summary>
    /// Allows you to change the connection strings of a known ExternalDatabaseServer.
    /// 
    /// <para>ExternalDatabaseServers are references to existing servers.  They have a logistical name (what you want to call it) and servername.  Optionally you can
    /// specify a database (required in the case of references to specific databases e.g. Logging Database), if you omit it then the 'master' database will be used.
    /// If you do not specify a username/password then Integrated Security will be used when connecting (the preferred method).  Usernames and passwords are stored
    /// in encrypted form (See PasswordEncryptionKeyLocationUI).</para>
    /// </summary>
    public partial class ExternalDatabaseServerUI : ExternalDatabaseServerUI_Design, ISaveableUI
    {
        private ExternalDatabaseServer _server;
        private bool bloading;

        public ExternalDatabaseServerUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.Tables;

            ddDatabaseType.DataSource = Enum.GetValues(typeof(DatabaseType));
        }

        public override void SetDatabaseObject(IActivateItems activator, ExternalDatabaseServer databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _server = databaseObject;
            objectSaverButton1.SetupFor(_server, activator.RefreshBus);

            bloading = true;
            
            try
            {
                SetupDropdownItems();

                tbID.Text = _server.ID.ToString();
                tbName.Text = _server.Name;
                tbServerName.Text = _server.Server;
                tbMappedDataPath.Text = _server.MappedDataPath;
                tbDatabaseName.Text = _server.Database;
                tbUsername.Text = _server.Username;
                tbPassword.Text = _server.GetDecryptedPassword();
                ddSetKnownType.Text = _server.CreatedByAssembly;

                ddDatabaseType.SelectedItem = _server.DatabaseType;
                pbDatabaseProvider.Image = _activator.CoreIconProvider.GetImage(_server.DatabaseType);

                pbServer.Image = _activator.CoreIconProvider.GetImage(_server);
            }
            finally
            {
                bloading = false;
            }
        }

        private void SetupDropdownItems()
        {
            ddSetKnownType.Items.Clear();
            ddSetKnownType.Items.AddRange(
                AppDomain.CurrentDomain.GetAssemblies() //get all current assemblies that are loaded
                .Select(n => n.GetName().Name)//get the name of the assembly
                .Where(s => s.EndsWith(".Database") && //if it is a .Database assembly advertise it to the user as a known type of database
                    !(s.EndsWith("CatalogueLibrary.Database") || s.EndsWith("DataExportManager.Database"))).ToArray()); //unless it's one of the core ones (catalogue/data export)
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            _server.Name = tbName.Text;
        }

        private void tbServerName_TextChanged(object sender, EventArgs e)
        {
            _server.Server = tbServerName.Text;
        }

        private void tbMappedDataPath_TextChanged(object sender, EventArgs e)
        {
            _server.MappedDataPath = tbMappedDataPath.Text;
        }

        private void tbDatabaseName_TextChanged(object sender, EventArgs e)
        {
            _server.Database = tbDatabaseName.Text;
        }

        private void tbUsername_TextChanged(object sender, EventArgs e)
        {
            _server.Username = tbUsername.Text;
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            _server.Password = tbPassword.Text;
        }

        private void ddSetKnownType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(bloading)
                return;
            
            _server.CreatedByAssembly = ddSetKnownType.SelectedItem as string;
        }

        private void btnCheckState_Click(object sender, EventArgs e)
        {
            try
            {
                ragSmiley1.Reset();
                ragSmiley1.Visible = true;

                DataAccessPortal.GetInstance().ExpectServer(_server, DataAccessContext.InternalDataProcessing).TestConnection();
                lblState.Text = "State:OK";
                lblState.ForeColor = Color.Green;
            }
            catch (Exception exception)
            {
                lblState.Text = "State" + exception.Message;
                lblState.ForeColor = Color.Red;
                ragSmiley1.Fatal(exception);
            }
        }

        private void btnClearKnownType_Click(object sender, EventArgs e)
        {
            _server.CreatedByAssembly = null;
            ddSetKnownType.SelectedItem = null;
            ddSetKnownType.Text = null;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void ddDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_server == null)
                return;

            var type = (DatabaseType)ddDatabaseType.SelectedValue;
            _server.DatabaseType = type;
            pbDatabaseProvider.Image = _activator.CoreIconProvider.GetImage(type);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExternalDatabaseServerUI_Design, UserControl>))]
    public abstract class ExternalDatabaseServerUI_Design:RDMPSingleDatabaseObjectControl<ExternalDatabaseServer>
    {
    }
}
