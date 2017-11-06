using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueManager.AggregationUIs.Advanced.Options;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.SimpleDialogs.ForwardEngineering;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;

namespace CatalogueManager.MainFormUITabs.SubComponents
{
    /// <summary>
    /// This control offers the preferred method of telling RDMP about your existing datasets.  It lets you select a table on your server and then forward engineer an RDMP Catalogue
    /// which lets you build a data load for the table, document it's columns, configure extraction logic etc.  
    /// 
    /// Start by entering the details of your table (server, database, table etc).  If you specify username/password then SQL Authentication will be used and the credentials will be
    /// stored along with the table (See PasswordEncryptionKeyLocationUI for details), if you do not enter username/password then Windows Authentication will be used (preferred).  
    /// 
    /// Clicking Import will create TableInfo / ColumnInfo objects in your Data Catalogue database and then ForwardEngineerCatalogueUI will be launched which lets you pick which 
    /// columns are extractable and which contains the Patient Identifier (e.g. CHI number / NHS number etc).  See ForwardEngineerCatalogueUI for full details. 
    /// </summary>
    public partial class ImportSQLTable : Form
    {
        private readonly IActivateItems _activator;
        private readonly bool _autoCreateCatalogue;
        public ITableInfoImporter Importer { get; private set; }
        public TableInfo TableInfoCreatedIfAny { get; private set; }

        public ImportSQLTable(IActivateItems activator,bool autoCreateCatalogue)
        {
            _activator = activator;
            _autoCreateCatalogue = autoCreateCatalogue;
            InitializeComponent();

            serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = true;
            serverDatabaseTableSelector1.SelectionChanged += serverDatabaseTableSelector1_SelectionChanged;

            ddContext.DataSource = Enum.GetValues(typeof (DataAccessContext));
            ddContext.SelectedItem = DataAccessContext.Any;//default to any!

            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RecentHistoryOfControls.GetInstance().HostControl(serverDatabaseTableSelector1.cbxServer);
            RecentHistoryOfControls.GetInstance().AddHistoryAsItemsToComboBox(serverDatabaseTableSelector1.cbxServer);

        }

        void serverDatabaseTableSelector1_SelectionChanged()
        {
            AdjustImporter();
        }

        private void AdjustImporter()
        {
            var cataRepo = _activator.RepositoryLocator.CatalogueRepository;
            try
            {
                if (!string.IsNullOrWhiteSpace(serverDatabaseTableSelector1.Table))
                {
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    builder.DataSource = serverDatabaseTableSelector1.Server;
                    builder.InitialCatalog = serverDatabaseTableSelector1.Database;

                    //if there is no username/password then use integrated security
                    if (string.IsNullOrWhiteSpace(serverDatabaseTableSelector1.Username))
                    {
                        builder.IntegratedSecurity = true;
                        Importer = new TableInfoImporter(cataRepo, serverDatabaseTableSelector1.Server, serverDatabaseTableSelector1.Database, serverDatabaseTableSelector1.Table, serverDatabaseTableSelector1.DatabaseType);
                    }
                    else
                    {

                        builder.IntegratedSecurity = false;
                        builder.UserID = serverDatabaseTableSelector1.Username;
                        builder.Password = serverDatabaseTableSelector1.Password;

                        Importer = new TableInfoImporter(cataRepo, serverDatabaseTableSelector1.Server, serverDatabaseTableSelector1.Database, serverDatabaseTableSelector1.Table, serverDatabaseTableSelector1.DatabaseType, username: serverDatabaseTableSelector1.Username, password: serverDatabaseTableSelector1.Password, usageContext: (DataAccessContext)ddContext.SelectedValue);
                    }

                    btnImport.Enabled = true;
                }
                else
                    if (!string.IsNullOrWhiteSpace(serverDatabaseTableSelector1.TableValuedFunction))
                    {
                        var table = serverDatabaseTableSelector1.GetDiscoveredDatabase()
                            .ExpectTableValuedFunction(serverDatabaseTableSelector1.TableValuedFunction);
                        Importer = new TableValuedFunctionImporter(cataRepo, table, (DataAccessContext)ddContext.SelectedValue);
                        btnImport.Enabled = true;
                    }
                    else
                        btnImport.Enabled = false;
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            TableInfo parent;
            ColumnInfo[] newCols;
            try
            {
                Importer.DoImport(out parent, out newCols);
                DialogResult = DialogResult.OK;

                TableInfoCreatedIfAny = parent;
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(parent));


                if (_autoCreateCatalogue)
                {
                    ForwardEngineerCatalogue engineer = new ForwardEngineerCatalogue(parent,newCols.ToArray(),true);
                    Catalogue catalogue;
                    CatalogueItem[] cis;
                    ExtractionInformation[] eis;
                    engineer.ExecuteForwardEngineering(out catalogue,out cis,out eis);
                    _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(catalogue));
                }
                else
                {
                    // logic to add credentials 
                    // parent.SetCredentials(); 
                    ForwardEngineerCatalogueUI f = new ForwardEngineerCatalogueUI(_activator,parent, newCols.ToArray());
                    f.ShowDialog();

                    if(f.CatalogueCreatedIfAny != null)
                        _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(f.CatalogueCreatedIfAny));
                }

                if(parent.IsTableValuedFunction && parent.GetAllParameters().Any())
                {
                    var options = new ParameterCollectionUIOptionsFactory().Create(parent);
                    ParameterCollectionUI.ShowAsDialog(options,true);
                }

                MessageBox.Show("Successfully imported table '" + parent + "'");
                Close();
            }
            catch (SqlException exception)
            {
                MessageBox.Show("Problem importing table :" + exception.Message);
            }
        }

        private void serverDatabaseTableSelector1_IntegratedSecurityUseChanged(bool use)
        {
            lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Visible = !use;
            ddContext.Enabled = !use;
        }

        private void ddContext_SelectedIndexChanged(object sender, EventArgs e)
        {
            AdjustImporter();
        }    
    }
}
