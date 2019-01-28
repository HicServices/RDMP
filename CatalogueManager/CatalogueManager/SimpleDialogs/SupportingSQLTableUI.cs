using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Copying;
using ReusableUIComponents;

using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;
using Color = System.Drawing.Color;

namespace CatalogueManager.SimpleDialogs
{
    /// <summary>
    /// The RDMP allows you at attach both documents and auxiliary tables (SupportingSQLTable) to your datasets (Catalogue).  These artifacts are then available to data analysts who
    /// want to understand the dataset better.  Also if you tick IsExtractable then whenever the Catalogue is extracted the table/document is automatically copied and extracted into 
    /// project extraction directory for provision to the researcher.
    /// 
    /// <para>If you have Lookup tables (that you don't want to configure as Lookup objects, see LookupConfiguration) or complex dictionary tables etc which are required/helpful in understanding or
    /// processing the data in your dataset then you should configure it as a SupportingSQLTable.  Make sure to put in an appropriate name and description of what is in the table.  You
    /// must select the server on which the SQL should be run (See ManageExternalServers), if you setup a single reference to your data repository with Database='master' and then ensure
    /// that all your SupportingSQLTables are fully qualified (e.g. [MyDb].dbo.[MyTable]) then you can avoid having to create an ExternalDatabaseServer for each different database.</para>
    /// 
    /// <para>If you tick IsGlobal then the table will be extracted regardless of what dataset is selected in a researchers data request (useful for global lookups that contain cross dataset 
    /// codes).  </para>
    /// 
    /// <para>IMPORTANT: Make sure your SQL query DOES NOT return any identifiable data if it is marked as IsExtractable as this SQL is executed 'as is' and does not undergo any project level
    /// anonymisation.</para>
    /// </summary>
    public partial class SupportingSQLTableUI : SupportingSQLTableUI_Design, ISaveableUI
    {
        private Scintilla QueryPreview;
        private SupportingSQLTable _supportingSQLTable;

        private const string NoExternalServer = "<<NONE>>";
        
        public SupportingSQLTableUI()
        {
            InitializeComponent();

            #region Query Editor setup
            if (VisualStudioDesignMode)
                return;

            QueryPreview = new ScintillaTextEditorFactory().Create(new RDMPCommandFactory());
            QueryPreview.ReadOnly = false;
            QueryPreview.TextChanged += new EventHandler(QueryPreview_TextChanged);

            this.pSQL.Controls.Add(QueryPreview);
            #endregion

            tcTicket.TicketTextChanged += TcTicketOnTicketTextChanged;
            AssociatedCollection = RDMPCollection.Catalogue;
        }


        public override void SetDatabaseObject(IActivateItems activator, SupportingSQLTable databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            SupportingSQLTable = databaseObject;
            RefreshUIFromDatabase();
        }
        
        private void RefreshUIFromDatabase()
        {
            ddExternalServers.Items.Clear();
            ddExternalServers.Items.Add(NoExternalServer);
            ddExternalServers.Items.AddRange(SupportingSQLTable.Repository.GetAllObjects<ExternalDatabaseServer>().ToArray());

            if (_supportingSQLTable != null)
                ddExternalServers.SelectedItem = _supportingSQLTable.ExternalDatabaseServer;
        }

        private bool _bLoading;

        protected SupportingSQLTable SupportingSQLTable
        {
            get { return _supportingSQLTable; }
            private set
            {
                _bLoading = true;

                _supportingSQLTable = value;

                tbDescription.Text = value.Description;
                tbName.Text = value.Name;
                tbID.Text = value.ID.ToString();
                QueryPreview.Text = value.SQL;

                //if it has an external server configured
                if (value.ExternalDatabaseServer_ID != null)
                    ddExternalServers.Text = value.ExternalDatabaseServer.ToString();
                else
                    ddExternalServers.Text = NoExternalServer;

                tcTicket.TicketText = value.Ticket;
                    
                cbExtractable.Checked = value.Extractable;
                cbGlobal.Checked = value.IsGlobal;                 
                
                _bLoading = false;
            }
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                tbName.Text = "No Name";
                tbName.SelectAll();
            }
            
            if (SupportingSQLTable != null)
                SupportingSQLTable.Name = tbName.Text;
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            if (SupportingSQLTable != null)
                SupportingSQLTable.Description = tbDescription.Text;
             
        }

        void QueryPreview_TextChanged(object sender, EventArgs e)
        {
            if (SupportingSQLTable != null)
                SupportingSQLTable.SQL = QueryPreview.Text;
        }

        private void cbExtractable_CheckedChanged(object sender, EventArgs e)
        {
            if (SupportingSQLTable != null)
                SupportingSQLTable.Extractable = cbExtractable.Checked;
        }

        private void cbGlobal_CheckedChanged(object sender, EventArgs e)
        {
            if(_bLoading)
                return;

            if (SupportingSQLTable != null)
            {
                if (cbGlobal.Checked)
                    SupportingSQLTable.IsGlobal = true;
                else
                {
                    if (
                        MessageBox.Show(
                            "Are you sure you want to tie this SQL to this specific Catalogue? and stop it being Globally viewable to all Catalogues?",
                            "Disable Globalness?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        SupportingSQLTable.IsGlobal = false;
                    else
                        cbGlobal.Checked = true;
                }
                
            }
        }
        
        private void tbDescription_KeyPress(object sender, KeyPressEventArgs e)
        {
            //apparently that is S when the control key is held down
            if(e.KeyChar == 19 && ModifierKeys == Keys.Control)
                e.Handled = true;
            
        }
        
        private void btnManageExternalServers_Click(object sender, EventArgs e)
        {
            
        }

        private void ddExternalServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(_supportingSQLTable == null)
                return;

            //user selected NONE
            if (ReferenceEquals(ddExternalServers.SelectedItem, NoExternalServer))
                _supportingSQLTable.ExternalDatabaseServer_ID = null;
            else
                //user selected a good server
                _supportingSQLTable.ExternalDatabaseServer_ID = ((ExternalDatabaseServer)ddExternalServers.SelectedItem).ID;
        }
        
        private void TcTicketOnTicketTextChanged(object sender, EventArgs eventArgs)
        {
            if (_supportingSQLTable != null)
                _supportingSQLTable.Ticket = tcTicket.TicketText;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var cmd = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, null, ServerDefaults.PermissableDefaults.None);
            cmd.Execute();
            RefreshUIFromDatabase();
        }
    }
    
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<SupportingSQLTableUI_Design, UserControl>))]
    public abstract class SupportingSQLTableUI_Design:RDMPSingleDatabaseObjectControl<SupportingSQLTable>
    {
    }
}
