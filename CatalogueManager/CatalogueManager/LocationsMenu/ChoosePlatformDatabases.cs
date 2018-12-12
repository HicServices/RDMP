using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using DatabaseCreation;
using Diagnostics;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Settings;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;

namespace CatalogueManager.LocationsMenu
{
    /// <summary>
    /// All metadata in RDMP is stored in one of two main databases.  The Catalogue database records all the technical, descriptive, governance, data load, filtering logic etc about 
    /// your datasets (including where they are stored etc).  The Data Export Manager database stores all the extraction configurations you have created for releasing to researchers.
    /// 
    /// <para>This window lets you tell the software where your Catalogue / Data Export Manager databases are or create new ones.  These connection strings are recorded in each users settings file.
    /// It is strongly advised that you use Integrated Security (Windows Security) for connecting rather than a username/password as this is the only case where Passwords are not encrypted
    /// (Since the encryption certificate location is stored in the Catalogue! - see PasswordEncryptionKeyLocationUI).</para>
    /// 
    /// <para>Only the Catalogue database is required, if you do not intend to do data extraction at this time then you can skip creating one.  </para>
    /// 
    /// <para>It is a good idea to run Check after configuring your connection string to ensure that the database is accessible and that the tables/columns in the database match the softwares
    /// expectations.  </para>
    /// 
    /// <para>IMPORTANT: if you configure your connection string wrongly it might take up to 30s for windows to timeout the network connection (e.g. if you specify the wrong server name). This is
    /// similar to if you type in a dodgy server name in Microsoft Windows Explorer.</para>
    /// </summary>
    public partial class ChoosePlatformDatabases : Form
    {
        private readonly IActivateItems _activatorIfAny;
        private UserSettingsRepositoryFinder _repositoryLocator;

        public ChoosePlatformDatabases(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = (UserSettingsRepositoryFinder)repositoryLocator;
            
            InitializeComponent();

            new RecentHistoryOfControls(tbCatalogueConnectionString, new Guid("75e6b0a3-03f2-49fc-9446-ebc1dae9f123"));
            new RecentHistoryOfControls(tbDataExportManagerConnectionString, new Guid("9ce952d8-d629-454a-ab9b-a1af97548be6"));

            SetState(State.PickNewOrExisting);
        }

        private void SetState(State newState)
        {
            switch (newState)
            {
                case State.PickNewOrExisting:
                    pChooseOption.Dock = DockStyle.Top;
                    
                    pResults.Visible = false;
                    gbCreateNew.Visible = false;
                    gbUseExisting.Visible = false;

                    pChooseOption.Visible = true;
                    pChooseOption.BringToFront();
                    break;
                case State.CreateNew:

                    pResults.Dock = DockStyle.Fill;
                    gbCreateNew.Dock = DockStyle.Top;
                    
                    
                    pResults.Visible = true;
                    pChooseOption.Visible = false;
                    gbUseExisting.Visible = false;

                    gbCreateNew.Visible = true;
                    pResults.BringToFront();

                    
                    break;
                case State.ConnectToExisting:
                    pResults.Dock = DockStyle.Fill;
                    gbUseExisting.Dock = DockStyle.Top;
                    
                    pChooseOption.Visible = false;
                    gbCreateNew.Visible = false;

                    pResults.Visible = true;
                    gbUseExisting.Visible = true;
                    pResults.BringToFront();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("newState");
            }
        }

        private enum State
        {
            PickNewOrExisting,
            CreateNew,
            ConnectToExisting
        }

        protected override void OnLoad(EventArgs e)
        {
            if (_repositoryLocator == null)
                return;

            if (_repositoryLocator.CatalogueRepository != null)
                tbCatalogueConnectionString.Text = _repositoryLocator.CatalogueRepository.ConnectionString;

            if (_repositoryLocator.DataExportRepository != null)
                tbDataExportManagerConnectionString.Text = _repositoryLocator.DataExportRepository.ConnectionString;
            
            base.OnLoad(e);
        }

        private bool SaveConnectionStrings()
        {
            try
            {
                // save all the settings
                UserSettings.CatalogueConnectionString = tbCatalogueConnectionString.Text;
                UserSettings.DataExportConnectionString = tbDataExportManagerConnectionString.Text;
                _repositoryLocator.RefreshRepositoriesFromUserSettings();
                return true;
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Failed to save connection settings",CheckResult.Fail,exception));
                return false;
            }
            
        }

        private void ChooseDatabase_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
                btnSaveAndClose_Click(null,null);

            if(e.KeyCode == Keys.Escape)
               this.Close();

        }
        private void tbCatalogueConnectionString_KeyUp(object sender, KeyEventArgs e)
        {
            //if user is doing a paste
            if (e.KeyCode == Keys.V && e.Control)
            {
                //check to see what he is pasting
                string toPaste = Clipboard.GetText();

                //he is pasting something with newlines
                if (toPaste.Contains(Environment.NewLine))
                {
                    //see if he is trying to paste two lines at once, in whichcase surpress Windows and paste it across the two text boxes
                    string[] toPasteArray = toPaste.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (toPasteArray.Length == 2)
                    {
                        tbCatalogueConnectionString.Text = toPasteArray[0];
                        tbDataExportManagerConnectionString.Text = toPasteArray[1];
                        e.SuppressKeyPress = true;
                    }
                }
            }
        }
        
        private MissingFieldsChecker CreateMissingFieldsChecker(MissingFieldsChecker.ThingToCheck thingToCheck)
        {
            return new MissingFieldsChecker(thingToCheck, _repositoryLocator.CatalogueRepository, _repositoryLocator.DataExportRepository);
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            //if save is successful
            if (SaveConnectionStrings())
                //integrity checks passed
                RestartApplication();
        }

        private void btnCheckDataExportManager_Click(object sender, EventArgs e)
        {
            CheckRepository(MissingFieldsChecker.ThingToCheck.DataExportManager);
        }

        private void btnCheckCatalogue_Click(object sender, EventArgs e)
        {
            CheckRepository(MissingFieldsChecker.ThingToCheck.Catalogue);
        }

        private void CheckRepository(MissingFieldsChecker.ThingToCheck repositoryToCheck)
        {
            try
            {
                //save the settings
                SaveConnectionStrings();

                checksUI1.StartChecking(CreateMissingFieldsChecker(repositoryToCheck));
                checksUI1.AllChecksComplete += ShowNextStageOnChecksComplete;
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Checking of " + repositoryToCheck + " Database failed", CheckResult.Fail,exception));
            }
        }

        private void ShowNextStageOnChecksComplete(object sender, AllChecksCompleteHandlerArgs args)
        {
            ((ChecksUI) sender).AllChecksComplete -= ShowNextStageOnChecksComplete;
        }

        private void btnCreateSuite_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            try
            {
                Cursor = Cursors.WaitCursor;

                Console.SetOut(new StringWriter(sb));

                var opts = new DatabaseCreationProgramOptions();
                opts.ServerName = tbSuiteServer.Text;
                opts.Prefix = tbDatabasePrefix.Text;
                opts.Username = tbUsername.Text;
                opts.Password = tbPassword.Text;

                var task = new Task(() =>
                {
                    try
                    {
                        DatabaseCreationProgram.RunOptionsAndReturnExitCode(opts);
                    }
                    catch (Exception ex)
                    {
                        checksUI1.OnCheckPerformed(
                            new CheckEventArgs("Database creation failed, check exception for details", CheckResult.Fail,
                                ex));
                    }
                });
                task.Start();

                while (!task.IsCompleted)
                {
                    task.Wait(100);
                    Application.DoEvents();

                    var result = sb.ToString();

                    if (string.IsNullOrEmpty(result))
                        continue;

                    sb.Clear();

                    if (result.Contains("Exception"))
                        throw new Exception(result);

                    checksUI1.OnCheckPerformed(new CheckEventArgs(result, CheckResult.Success));
                }

                checksUI1.OnCheckPerformed(new CheckEventArgs("Finished Creating Platform Databases", CheckResult.Success));

                var cata = opts.GetBuilder(DatabaseCreationProgram.DefaultCatalogueDatabaseName);
                var export = opts.GetBuilder(DatabaseCreationProgram.DefaultDataExportDatabaseName);
                
                UserSettings.CatalogueConnectionString = cata.ConnectionString;
                UserSettings.DataExportConnectionString = export.ConnectionString;
                RestartApplication();

            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Database creation failed, check exception for details",CheckResult.Fail, exception));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RestartApplication()
        {
            MessageBox.Show("Connection Strings Changed, the application will now restart");
            Application.Restart();
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            SetState(State.CreateNew);
        }

        private void btnUseExisting_Click(object sender, EventArgs e)
        {
            SetState(State.ConnectToExisting);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            SetState(State.PickNewOrExisting);
        }

        private void btnBrowseForCatalogue_Click(object sender, EventArgs e)
        {
            var dialog = new ServerDatabaseTableSelectorDialog("Catalogue Database",false,false);
            dialog.LockDatabaseType(DatabaseType.MicrosoftSQLServer);
            if (dialog.ShowDialog() == DialogResult.OK)
                tbCatalogueConnectionString.Text = dialog.SelectedDatabase.Server.Builder.ConnectionString;
        }

        private void btnBrowseForDataExport_Click(object sender, EventArgs e)
        {
            var dialog = new ServerDatabaseTableSelectorDialog("Data Export Database", false, false);
            dialog.LockDatabaseType(DatabaseType.MicrosoftSQLServer);
            if (dialog.ShowDialog() == DialogResult.OK)
                tbDataExportManagerConnectionString.Text = dialog.SelectedDatabase.Server.Builder.ConnectionString;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
