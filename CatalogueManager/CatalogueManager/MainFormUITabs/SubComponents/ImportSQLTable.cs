using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.ForwardEngineering;
using FAnsi.Discovery;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;

namespace CatalogueManager.MainFormUITabs.SubComponents
{
    /// <summary>
    /// This control offers the preferred method of telling RDMP about your existing datasets.  It lets you select a table on your server and then forward engineer an RDMP Catalogue
    /// which lets you build a data load for the table, document it's columns, configure extraction logic etc.  
    /// 
    /// <para>Start by entering the details of your table (server, database, table etc).  If you specify username/password then SQL Authentication will be used and the credentials will be
    /// stored along with the table (See PasswordEncryptionKeyLocationUI for details), if you do not enter username/password then Windows Authentication will be used (preferred).  </para>
    /// 
    /// <para>Clicking Import will create TableInfo / ColumnInfo objects in your Data Catalogue database and then ConfigureCatalogueExtractabilityUI will be launched which lets you pick which 
    /// columns are extractable and which contains the Patient Identifier (e.g. CHI number / NHS number etc).  See ConfigureCatalogueExtractabilityUI for full details. </para>
    /// </summary>
    public partial class ImportSQLTable : Form
    {
        private readonly IActivateItems _activator;
        private readonly bool _allowImportAsCatalogue;
        public ITableInfoImporter Importer { get; private set; }
        public TableInfo TableInfoCreatedIfAny { get; private set; }

        public ImportSQLTable(IActivateItems activator,bool allowImportAsCatalogue)
        {
            _activator = activator;
            _allowImportAsCatalogue = allowImportAsCatalogue;
            InitializeComponent();

            serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = true;
            serverDatabaseTableSelector1.SelectionChanged += serverDatabaseTableSelector1_SelectionChanged;

            ddContext.DataSource = Enum.GetValues(typeof (DataAccessContext));
            ddContext.SelectedItem = DataAccessContext.Any;//default to any!
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
                DiscoveredTable tbl = serverDatabaseTableSelector1.GetDiscoveredTable();

                if (tbl == null)
                {
                    btnImport.Enabled = false;
                    return;
                }
                
                //if it isn't a table valued function
                if (tbl is DiscoveredTableValuedFunction)
                    Importer = new TableValuedFunctionImporter(cataRepo, (DiscoveredTableValuedFunction) tbl,(DataAccessContext) ddContext.SelectedValue);
                else
                    Importer = new TableInfoImporter(cataRepo, tbl, (DataAccessContext) ddContext.SelectedValue);
                    
                btnImport.Enabled = true;
                    
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {

            if(_allowImportAsCatalogue)
            {
                var ui = new ConfigureCatalogueExtractabilityUI(_activator, Importer, "Existing Table", null);
                ui.ShowDialog();
                TableInfoCreatedIfAny = ui.TableInfoCreated;
            }
            else
            {
                // logic to add credentials 
                    // parent.SetCredentials(); 
                TableInfo ti;
                ColumnInfo[] cols;
                Importer.DoImport(out ti,out cols);
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(ti));
                TableInfoCreatedIfAny = ti;
            }

            try
            {
                DialogResult = DialogResult.OK;

                var ti = TableInfoCreatedIfAny;

                if(ti.IsTableValuedFunction && ti.GetAllParameters().Any())
                {
                    var options = new ParameterCollectionUIOptionsFactory().Create(ti);
                    ParameterCollectionUI.ShowAsDialog(options,true);
                }

                MessageBox.Show("Successfully imported table '" + ti + "'");
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
