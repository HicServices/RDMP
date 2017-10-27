using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Database;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.TestsAndSetup.StartupUI;
using CatalogueManager.Tutorials;
using DatabaseCreation;
using Diagnostics;
using Microsoft.Win32;
using RDMPObjectVisualisation;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.TransparentHelpSystem;
using ReusableUIComponents.TransparentHelpSystem.ProgressTracking;

namespace CatalogueManager.LocationsMenu
{
    /// <summary>
    /// All metadata in RDMP is stored in one of two main databases.  The Catalogue database records all the technical, descriptive, governance, data load, filtering logic etc about 
    /// your datasets (including where they are stored etc).  The Data Export Manager database stores all the extraction configurations you have created for releasing to researchers.
    /// 
    /// This window lets you tell the software where your Catalogue / Data Export Manager databases are or create new ones.  These connection strings are recorded in each users Registry.
    /// It is strongly advised that you use Integrated Security (Windows Security) for connecting rather than a username/password as this is the only case where Passwords are not encrypted
    /// (Since the encryption certificate location is stored in the Catalogue! - see PasswordEncryptionKeyLocationUI).
    /// 
    /// Only the Catalogue database is required, if you do not intend to do data extraction at this time then you can skip creating one.  
    /// 
    /// It is a good idea to run Check after configuring your connection string to ensure that the database is accessible and that the tables/columns in the database match the softwares
    /// expectations.  
    /// 
    /// IMPORTANT: if you configure your connection string wrongly it might take up to 30s for windows to timeout the network connection (e.g. if you specify the wrong server name). This is
    /// similar to if you type in a dodgy server name in Microsoft Windows Explorer.
    /// </summary>
    public partial class ChoosePlatformDatabases : Form, IHelpWorkflowUser
    {
        private readonly IActivateItems _activatorIfAny;
        public HelpWorkflow HelpWorkflow { get; private set; }

        public ChoosePlatformDatabases(IActivateItems activator, ICommandExecution command): this(activator.RepositoryLocator, command)
        {
            _activatorIfAny = activator;
        }

        private HelpStage _lookAtChecksToSeeWhyItAllFailed;
        private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

        public ChoosePlatformDatabases(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICommandExecution command)
        {
            _repositoryLocator = repositoryLocator;
            
            InitializeComponent();

            RecentHistoryOfControls.GetInstance().HostControl(tbCatalogueConnectionString);
            RecentHistoryOfControls.GetInstance().HostControl(tbDataExportManagerConnectionString);

            gbSqlServer.Enabled = true;

            BuildHelpWorkflow(command);
        }

        private void BuildHelpWorkflow(ICommandExecution command)
        {
            var tracker = TutorialTracker.GetInstance(_activatorIfAny);

            HelpWorkflow = new HelpWorkflow(this, command, tracker);
            
            //////Normal work flow
            var root = new HelpStage(
                pCreateAllPlatformDatabases,"If this is your first you will need to create the RDMP Platform Databases.  Enter your server name and a prefix for the databases here.",
                new HelpStage(checksUI1,"Watch the progress of creating the databases here"),
                new HelpStage(btnSaveAndClose,"Click To Restart Application"));

            //alternate option
            var setCataStage = new HelpStage(
                pReferenceACatalogue, "Enter the connection string to your Catalogue database and click Check",
                new HelpStage(pReferenceADataExport,"Now enter the connection string to your Data Export database and click Check"),
                new HelpStage(btnSaveAndClose,"Now Save And Close the changes to restart the application")
                );
            
            root.SetOption("I Already Have RDMP Platform Databases",setCataStage);

            HelpWorkflow.RootStage = root;

            _lookAtChecksToSeeWhyItAllFailed = new HelpStage(checksUI1,
                "Something went wrong with the operation, double click the red lines to see what went wrong.  Then close this help dialog.");

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

            HelpWorkflow.Start();
        }

        private bool SaveConnectionStringsToRegistry()
        {
            try
            {
                var regRepo = (RegistryRepositoryFinder)_repositoryLocator;
                // save all the settings
                regRepo.SetRegistryValue(RegistrySetting.Catalogue, tbCatalogueConnectionString.Text);
                regRepo.SetRegistryValue(RegistrySetting.DataExportManager, tbDataExportManagerConnectionString.Text);
                return true;
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Failed to save connection settings into registry",CheckResult.Fail,exception));
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
                        return;
                    }
                }
            }
        }

        private void btnCreateNewCatalogue_Click(object sender, EventArgs e)
        {
            try
            {
                CreatePlatformDatabase dialog = new CreatePlatformDatabase(typeof(Class1).Assembly);
                dialog.ShowDialog();
                if (dialog.DatabaseConnectionString != null)
                    tbCatalogueConnectionString.Text = dialog.DatabaseConnectionString;
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }


        private void btnCreateNewDataExportManagerDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                CreatePlatformDatabase dialog = new CreatePlatformDatabase(typeof(DataExportLibrary.Database.Class1).Assembly);
                dialog.ShowDialog();

                if (dialog.DatabaseConnectionString != null)
                    tbDataExportManagerConnectionString.Text = dialog.DatabaseConnectionString;
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private MissingFieldsChecker CreateMissingFieldsChecker(MissingFieldsChecker.ThingToCheck thingToCheck)
        {
            return new MissingFieldsChecker(thingToCheck, _repositoryLocator.CatalogueRepository, _repositoryLocator.DataExportRepository);
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            //if save is successful
            if(SaveConnectionStringsToRegistry())
            {
                try
                {
                    CreateMissingFieldsChecker(MissingFieldsChecker.ThingToCheck.Catalogue)
                        .Check(new ThrowImmediatelyCheckNotifier());
                }
                catch (Exception exception)
                {
                    bool launchDiagnostics = checksUI1.OnCheckPerformed(new CheckEventArgs("Catalogue database did not pass silent integrity checks, press the Check button to see the full check output",CheckResult.Fail,exception,"Launch diagnostics screen?"));
                    
                    if(launchDiagnostics)
                    {
                        var dialog = new DiagnosticsScreen(exception);
                        dialog.RepositoryLocator = _repositoryLocator;
                        dialog.ShowDialog(this);
                    }

                    return;
                }

                if(!string.IsNullOrWhiteSpace(tbDataExportManagerConnectionString.Text))
                    try
                    {
                        CreateMissingFieldsChecker(MissingFieldsChecker.ThingToCheck.DataExportManager).Check(new ThrowImmediatelyCheckNotifier());
                    }
                    catch (Exception exception)
                    {
                        bool launchDiagnostics = checksUI1.OnCheckPerformed(new CheckEventArgs("Data Export Manager database did not pass silent integrity checks, press the Check button to see the full check output", CheckResult.Fail, exception,"Launch diagnostics screen"));
                        
                        if (launchDiagnostics)
                        {
                            var dialog = new DiagnosticsScreen(exception);
                            dialog.RepositoryLocator = _repositoryLocator;
                            dialog.ShowDialog(this);
                        }

                        return;
                    }

                //integrity checks passed
                RestartApplication();
            }
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
                SaveConnectionStringsToRegistry();

                checksUI1.StartChecking(CreateMissingFieldsChecker(repositoryToCheck));
                checksUI1.AllChecksComplete += ShowNextStageOnChecksComplete;
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Checking of " + repositoryToCheck + " Database failed", CheckResult.Fail,exception));
                HelpWorkflow.ShowStage(_lookAtChecksToSeeWhyItAllFailed);
            }
        }

        private void ShowNextStageOnChecksComplete(object sender, AllChecksCompleteHandlerArgs args)
        {
            if (args.CheckResults.GetWorst() == CheckResult.Fail)
                HelpWorkflow.ShowStage(_lookAtChecksToSeeWhyItAllFailed);
            else
                HelpWorkflow.ShowNextStageOrClose();

            ((ChecksUI) sender).AllChecksComplete -= ShowNextStageOnChecksComplete;
        }

        private void btnShowRegistry_Click(object sender, EventArgs e)
        {
            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", "Lastkey", @"Computer\" + RegistryRepositoryFinder.RDMPRegistryRoot);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Could not set LastKey (makes regedit open at the correct registry key)",exception);
            }

            //give it time to take effect (windows be slow)
            Thread.Sleep(500);

            try
            {
                Process.Start("regedit.exe");
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Regedit.exe crashed or could not be launched",exception);
            }
        }

        private void btnCreateSuite_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            try
            {
                Cursor = Cursors.WaitCursor;
                HelpWorkflow.ShowNextStageOrClose();

                Console.SetOut(new StringWriter(sb));

                var task = new Task(() =>
                {
                    try
                    {
                        DatabaseCreationProgram.Main(new string[] {tbSuiteServer.Text, tbDatabasePrefix.Text});
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

                HelpWorkflow.ShowNextStageOrClose();
                checksUI1.OnCheckPerformed(new CheckEventArgs("Finished Creating Platform Databases", CheckResult.Success));

                var cata = DatabaseCreationProgram.GetBuilder(tbSuiteServer.Text, tbDatabasePrefix.Text,
                    DatabaseCreationProgram.DefaultCatalogueDatabaseName);
                var export = DatabaseCreationProgram.GetBuilder(tbSuiteServer.Text, tbDatabasePrefix.Text,
                    DatabaseCreationProgram.DefaultDataExportDatabaseName);

                tbCatalogueConnectionString.Text = cata.ConnectionString;
                tbDataExportManagerConnectionString.Text = export.ConnectionString;


            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Database creation failed, check exception for details",CheckResult.Fail, exception));
                HelpWorkflow.ShowStage(_lookAtChecksToSeeWhyItAllFailed);
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

        
    }
}
