using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.LocationsMenu;
using CatalogueManager.Properties;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.DataTables;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using Diagnostics;
using HIC.Logging;
using MapsDirectlyToDatabaseTable.Versioning;
using RDMPObjectVisualisation;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using MapsDirectlyToDatabaseTableUI;

namespace CatalogueManager.TestsAndSetup
{
    internal enum ConnectionTypes
    {
        Logging,
        ANO,
        IdentifierDump,
        Master
    }

    /// <summary>
    /// This super technical  screen appears when things go irrecoverably wrong or when the user launches it from the CatalogueManager main menu.  It includes a variety of tests that 
    /// can be run on the current platform databases to check the contents/configurations.  It also includes 'Create Test Dataset' which can be used as a user acceptance test to make
    /// sure that their installation is successfully working (See UserManual.docx)
    /// 
    /// <para>The 'Evaluate MEF Exports' button is particularly useful if you are trying to debug a plugin you have written to find out why your class isn't appearing in the relevant part of
    /// the RDMP.</para>
    /// </summary>
    public partial class DiagnosticsScreen : RDMPForm
    {
        private readonly Exception _originalException;

        public bool ShouldReloadCatalogues = false;
        private bool setupComplete = false;

        public DiagnosticsScreen(Exception exception)
        {
            _originalException = exception;

            InitializeComponent();
            
            if (_originalException != null)
                btnViewOriginalException.Enabled = true;
            
        }

        protected override void OnRepositoryLocatorAvailable()
        {
            base.OnRepositoryLocatorAvailable();

            if (RepositoryLocator.DataExportRepository == null)
            {
                btnAddExportFunctionality.Enabled = false;
                btnDataExportManagerFields.Enabled = false;

            }
        }

        public static void OfferLaunchingDiagnosticsScreenOrEnvironmentExit(IRDMPPlatformRepositoryServiceLocator locator, IWin32Window owner, Exception e)
        {
            if (MessageBox.Show(ExceptionHelper.ExceptionToListOfInnerMessages(e) + Environment.NewLine + Environment.NewLine + " Would you like to launch the Diagnostics Screen? (If you choose No then the application will exit ᕦ(ò_óˇ)ᕤ )", "Launch Diagnostics?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DiagnosticsScreen screen = new DiagnosticsScreen(e);
                screen.RepositoryLocator = locator;
                screen.ShowDialog(owner);
            }
            else
                Environment.Exit(499);
        }

        private void btnCatalogueFields_Click(object sender, EventArgs e)
        {

            StartChecking(new MissingFieldsChecker(MissingFieldsChecker.ThingToCheck.Catalogue, RepositoryLocator.CatalogueRepository, RepositoryLocator.DataExportRepository));
        }
        private void btnDataExportManagerFields_Click(object sender, EventArgs e)
        {
            StartChecking(new MissingFieldsChecker(MissingFieldsChecker.ThingToCheck.DataExportManager, RepositoryLocator.CatalogueRepository, RepositoryLocator.DataExportRepository));
        }

        private void btnCheckANOConfigurations_Click(object sender, EventArgs e)
        {
            StartChecking(new ANOConfigurationChecker(RepositoryLocator.CatalogueRepository));
        }
        private void btnCohortDatabase_Click(object sender, EventArgs e)
        {
            StartChecking(new CohortConfigurationChecker(RepositoryLocator.DataExportRepository));
        }
        private void btnListBadAssemblies_Click(object sender, EventArgs e)
        {
            StartChecking(new BadAssembliesChecker(RepositoryLocator.CatalogueRepository.MEF));
        }

        private void btnCatalogueTableNames_Click(object sender, EventArgs e)
        {
            StartChecking(new DodgyNamedTableAndColumnsChecker(RepositoryLocator.CatalogueRepository));
        }

        private void StartChecking(ICheckable checkable)
        {
            tabControl1.SelectTab(tpProgress);
            checksUI1.Clear();
            checksUI1.StartChecking(checkable);
        }

        #region setup test environment
        private void btnCreateTestDataset_Click(object sender, EventArgs e)
        {
            
            var loggingBuilder = new SqlConnectionStringBuilder(tbLoggingConnectionString.Text);
            var testDataBuilder = new SqlConnectionStringBuilder(tbTestDatasetServer.Text);
            SqlConnectionStringBuilder dumpBuilder = null;
            SqlConnectionStringBuilder anoBuilder = null;

            if (cbIncludeAnonymisation.Checked)
            {
                dumpBuilder = new SqlConnectionStringBuilder(tbDumpConnectionString.Text);
                anoBuilder = new SqlConnectionStringBuilder(tbANOConnectionString.Text);
            }

            if (string.IsNullOrWhiteSpace(tbLoggingConnectionString.Text))
            {
                MessageBox.Show("Please enter a connection string for the Logging server", "Create Test Dataset error");
                return;
            }
            
            //switch to the progress tab
            tabControl1.SelectTab(tpProgress);

            checksUI1.StartChecking(new UserAcceptanceTestEnvironment(testDataBuilder, cbxDataSetFolder.Text, loggingBuilder, ddLoggingTask.Text, anoBuilder, dumpBuilder,RepositoryLocator));
            ShouldReloadCatalogues = true;

            gbAddDataExportFunctionality.Enabled = true;
            btnImportHospitalAdmissions.Enabled = true;
        }

        private void PerformLoggingDatabaseChecks()
        {
            checksUI1.Clear();
            
            var builder = new SqlConnectionStringBuilder(tbLoggingConnectionString.Text);
            
            var loggingChecker = new LoggingDatabaseChecker(new DiscoveredServer(builder));

            loggingChecker.Check(new ThrowImmediatelyCheckNotifier());
        }

        private void PopulateLoggingTasks()
        {
            string before = ddLoggingTask.Text;

            var builder = new SqlConnectionStringBuilder(tbLoggingConnectionString.Text);

            LogManager lm = new LogManager(new DiscoveredServer(builder));
            ddLoggingTask.Items.Clear();
            ddLoggingTask.Items.AddRange(lm.ListDataTasks());
            lblLoggingDatabaseState.Text = "Success";
            lblLoggingDatabaseState.ForeColor = Color.Black;

            if (ddLoggingTask.Items.Contains(before))
                ddLoggingTask.Text = before;
        }

        private void cbIncludeAnonymisation_CheckedChanged(object sender, EventArgs e)
        {
            gbAnonymisation.Enabled = cbIncludeAnonymisation.Checked;
        }

        private void cbxDataSetFolder_Leave(object sender, EventArgs e)
        {
            if (!setupComplete)
                return;
            try
            {
                DirectoryInfo d = new DirectoryInfo(cbxDataSetFolder.Text);

                if (!d.Exists)
                    lblDatasetFolder.Text = "Directory does not exist";
                else
                    lblDatasetFolder.Text = "Ok";

                try
                {
                    // Attempt to get a list of security permissions from the folder. 
                    // This will raise an exception if the path is read only or do not have access to view the permissions. 
                    DirectorySecurity ds = d.GetAccessControl();

                }
                catch (UnauthorizedAccessException exception)
                {
                    lblDatasetFolder.Text = "Access Rights Problem:" + exception.Message;
                }
            }
            catch (Exception exception)
            {
                lblDatasetFolder.Text = exception.Message;
            }

            EnableGoButtonIfReadyToGo();

        }

        private void EnableGoButtonIfReadyToGo()
        {
            Control offender = null;


            //Do this in reverse order of importance (Bottom up so that the assignment of offender at the end is the first and highest priority thing user has to fix)

            if (cbIncludeAnonymisation.Checked)
            {
                if (string.IsNullOrWhiteSpace(tbANOConnectionString.Text))
                    offender = tbANOConnectionString;
                else
                    tbANOConnectionString.BackColor = Color.White;

                if (string.IsNullOrWhiteSpace(tbDumpConnectionString.Text))
                    offender = tbDumpConnectionString;
                else
                    tbDumpConnectionString.BackColor = Color.White;
            }

            if (ddLoggingTask.SelectedItem == null)
                offender = lbLoggingTask;
            else
                lbLoggingTask.ForeColor = Color.Black;
            
            if (string.IsNullOrWhiteSpace(tbLoggingConnectionString.Text))
                offender = tbLoggingConnectionString;
            else
                tbLoggingConnectionString.BackColor = Color.White;

            if (string.IsNullOrWhiteSpace(cbxDataSetFolder.Text))
                offender = cbxDataSetFolder;
            else
                cbxDataSetFolder.BackColor = Color.White;

            if(offender != null)
            {
                if (offender is Label)
                    offender.ForeColor = Color.Pink;
                else
                    offender.BackColor = Color.Pink;

                btnCreateTestDataset.Enabled = false;
            }
            else
                btnCreateTestDataset.Enabled = true;

        }

        #endregion


        private void btnViewOriginalException_Click(object sender, EventArgs e)
        {
            ExceptionViewer.Show(_originalException);
        }


        private void btnAddExportFunctionality_Click(object sender, EventArgs e)
        {
            ShouldReloadCatalogues = true;
            checksUI1.Clear();
            var setup = new UserAcceptanceTestDataExportFunctionality(cbxProjectExtractionDirectory.Text, RepositoryLocator);
            
            //switch to the progress tab
            tabControl1.SelectTab(tpProgress);

            setup.Check(checksUI1);
        }
        
        private void btnImportHospitalAdmissions_Click(object sender, EventArgs e)
        {
            ShouldReloadCatalogues = true;
            checksUI1.Clear();
            
            //do not need to provide server because this test will pick up the server settings off of the DMPTestCatalogue and use the same target data server to load to
            var setup = new UserAcceptanceTestImportHospitalAdmissions(RepositoryLocator);

            //switch to the progress tab
            tabControl1.SelectTab(tpProgress);

            setup.Check(checksUI1);

        }
        
        private void btnCreateNewLoggingDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                CreatePlatformDatabase dialog = new CreatePlatformDatabase( typeof(HIC.Logging.Database.Class1).Assembly);
                dialog.ShowDialog();

                if (dialog.DatabaseConnectionString!= null)
                {
                    tbLoggingConnectionString.Text = dialog.DatabaseConnectionString;
                    tbLoggingConnectionString.Focus();
                    AfterUpdateConnectionString(ConnectionTypes.Logging, tbLoggingConnectionString);//run logging checks
                }

                if (ddLoggingTask.SelectedItem == null)
                    ddLoggingTask.Text = "Internal";
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void AfterUpdateConnectionString(ConnectionTypes connectionType, TextBox tbConnectionString)
        {
            DiscoveredServer server=null;

            try
            {
                server = new DiscoveredServer(new SqlConnectionStringBuilder(tbConnectionString.Text));
                server.TestConnection();
                
                switch (connectionType)
                {
                    case ConnectionTypes.Master:
                        break;
                    case ConnectionTypes.Logging:
                        PerformLoggingDatabaseChecks();
                        PopulateLoggingTasks();
                        break;
                    case ConnectionTypes.ANO:

                        lblANODatabaseState.Text = "State:Broken";
                        lblANODatabaseState.ForeColor = Color.Red;
                        ANOTransformer.ConfirmDependencies(server.GetCurrentDatabase(), new ThrowImmediatelyCheckNotifier());
                        lblANODatabaseState.Text = "State:Ok";
                        lblANODatabaseState.ForeColor = Color.Green;

                        break;
                    case ConnectionTypes.IdentifierDump:
                        lblIdentifierDumpDatabaseState.Text = "State:Broken";
                        lblIdentifierDumpDatabaseState.ForeColor = Color.Red;
                        IdentifierDumper.ConfirmDependencies(server.GetCurrentDatabase(), new ThrowImmediatelyCheckNotifier());
                        lblIdentifierDumpDatabaseState.Text = "State:Ok";
                        lblIdentifierDumpDatabaseState.ForeColor = Color.Green;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("connectionType", connectionType, null);
                }

                EnableGoButtonIfReadyToGo();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Failed to check " + connectionType + "(" + server+")",exception);
            }
        }
        
        private void tbLoggingConnectionString_Leave(object sender, EventArgs e)
        {
            AfterUpdateConnectionString(ConnectionTypes.Logging, tbLoggingConnectionString);
        }

        private void tbANOConnectionString_Leave(object sender, EventArgs e)
        {
            AfterUpdateConnectionString(ConnectionTypes.ANO, tbANOConnectionString);
        }

        private void tbDumpConnectionString_Leave(object sender, EventArgs e)
        {
            AfterUpdateConnectionString(ConnectionTypes.IdentifierDump, tbDumpConnectionString);
        }

        private void tbTestDatasetServer_Leave(object sender, EventArgs e)
        {
            AfterUpdateConnectionString(ConnectionTypes.Master, tbTestDatasetServer);
        }

        private void btnRefreshDataLoadTasks_Click(object sender, EventArgs e)
        {
            AfterUpdateConnectionString(ConnectionTypes.Logging, tbLoggingConnectionString);
        }

        private void btnCreateNewANOStore_Click(object sender, EventArgs e)
        {
            CreatePlatformDatabase dialog = new CreatePlatformDatabase(typeof(ANOStore.Database.Class1).Assembly);
            dialog.ShowDialog(this);

            if(!string.IsNullOrWhiteSpace(dialog.DatabaseConnectionString))
                tbANOConnectionString.Text = dialog.DatabaseConnectionString;
        }

        private void btnCreateNewIdentifierDump_Click(object sender, EventArgs e)
        {
            CreatePlatformDatabase dialog = new CreatePlatformDatabase(typeof(IdentifierDump.Database.Class1).Assembly);
            dialog.ShowDialog(this);

            if (!string.IsNullOrWhiteSpace(dialog.DatabaseConnectionString))
                tbDumpConnectionString.Text = dialog.DatabaseConnectionString;

        }

        private void btnCatalogueCheck_Click(object sender, EventArgs e)
        {
            checksUI1.Clear();
            try
            {
                foreach (Catalogue c in RepositoryLocator.CatalogueRepository.GetAllCatalogues(true))
                    c.Check(checksUI1);
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Catalogue checking crashed completely", CheckResult.Fail,exception));
            }
        }

        private void btnCheckANOStore_Click(object sender, EventArgs e)
        {
            AfterUpdateConnectionString(ConnectionTypes.ANO, tbANOConnectionString);
        }
        
        private void btnCheckIdentifierDump_Click(object sender, EventArgs e)
        {
            AfterUpdateConnectionString(ConnectionTypes.IdentifierDump, tbDumpConnectionString);
        }

        private void DiagnosticsScreen_Load(object sender, EventArgs e)
        {
            SqlConnectionStringBuilder defaultBuilder;

            //if there is a Catalogue Server default to it
            if (RepositoryLocator != null && RepositoryLocator.CatalogueRepository != null && !string.IsNullOrWhiteSpace(RepositoryLocator.CatalogueRepository.ConnectionString))
            {
                var builder = (SqlConnectionStringBuilder)RepositoryLocator.CatalogueRepository.ConnectionStringBuilder;
                defaultBuilder = new SqlConnectionStringBuilder()
                {
                    DataSource = builder.DataSource,
                    IntegratedSecurity = builder.IntegratedSecurity
                };
            }
            else
                defaultBuilder = new SqlConnectionStringBuilder()
                {
                    DataSource = Environment.MachineName,
                    IntegratedSecurity = true
                };

            string defaultConnectionString = defaultBuilder.ConnectionString;
            
            tbLoggingConnectionString.Text = defaultConnectionString;
            tbANOConnectionString.Text = defaultConnectionString;
            tbDumpConnectionString.Text = defaultConnectionString;
            tbTestDatasetServer.Text = defaultConnectionString;
            
            setupComplete = true;

            //only allow them to add data export if they have imported the dataset
            if (RepositoryLocator != null && RepositoryLocator.CatalogueRepository != null && !string.IsNullOrWhiteSpace(RepositoryLocator.CatalogueRepository.ConnectionString))
            {
                var importAlreadyHappened = RepositoryLocator.CatalogueRepository.GetAllCatalogues().Any(c => c.Name.Equals(UserAcceptanceTestEnvironment.CatalogueName));
                btnImportHospitalAdmissions.Enabled = importAlreadyHappened;
                gbAddDataExportFunctionality.Enabled = importAlreadyHappened;
            }
        }

        private void ddLoggingTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableGoButtonIfReadyToGo();
        }

    }
}
