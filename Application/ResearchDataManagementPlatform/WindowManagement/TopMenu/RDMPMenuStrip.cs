using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Reports;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.LocationsMenu;
using CatalogueManager.LocationsMenu.LocationAdjustment;
using CatalogueManager.LocationsMenu.Ticketing;
using CatalogueManager.LogViewer;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.PluginManagement;
using CatalogueManager.PluginManagement.CodeGeneration;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.SimpleDialogs.Automation;
using CatalogueManager.SimpleDialogs.Governance;
using CatalogueManager.SimpleDialogs.NavigateTo;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Tutorials;
using DataQualityEngine;
using MapsDirectlyToDatabaseTableUI;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.Licenses;
using ResearchDataManagementPlatform.WindowManagement.TopMenu.MenuItems;
using ResearchDataManagementPlatform.WindowManagement.UserSettings;
using ReusableLibraryCode;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.TopMenu
{

    /// <summary>
    /// The Top menu of the RDMP lets you do most tasks that do not relate directly to a single object (most single object tasks are accessed by right clicking the object).
    /// 
    /// Locations:
    /// - Change which DataCatalogue database you are pointed at (not usually needed unless you have two different databases e.g. a Test database and a Live database)
    /// - Setup Logging / Anonymisation / Query Caching / Data Quality Engine databases
    /// - Configure a Ticketing system e.g. Jira for tracking time against tickets (you can set a ticket identifier for datasets, project extractions etc)
    /// - Perform bulk renaming operations across your entire catalogue database (useful for when someone remaps your server drives to a new letter! e.g. 'D:\Datasets\Private\' becomes 'E:\')
    /// - Refresh the window by reloading all Catalogues/TableInfos etc 
    /// 
    /// View:
    /// - View/Edit dataset loading logic
    /// - View/Edit the governance approvals your datasets have (including attachments, period covered, datasets included in approval etc)
    /// - View the Logging database contents (a relational view of all activities undertaken by all Data Analysts using the RDMP - loading, extractions, dqe runs etc).
    /// 
    /// Reports
    /// - Generate a variety of reports that summarise the state of your datasets / governance etc
    /// 
    /// Help
    /// - View the user manual
    /// - View a technical description of each of the core objects maintained by RDMP (Catalogues, TableInfos etc) and what they mean (intended for programmers)
    /// - Generate user interface document (the document you are currently reading).
    /// </summary>

    public partial class RDMPMenuStrip : RDMPUserControl
    {
        private IActivateItems _activator;
        private ToolboxWindowManager _windowManager;
        private DockContent currentTab;
        private ISaveableUI _saveable;

        public RDMPMenuStrip()
        {
            InitializeComponent();
        }

        private void changeCatalogueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchDatabaseSettingsChangeDialog();
        }

        private void LaunchDatabaseSettingsChangeDialog()
        {
            new ExecuteCommandChoosePlatformDatabase(RepositoryLocator).Execute();
        }

        private void configureExternalServersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var manageExternalServers = new ManageExternalServers(_activator.CoreIconProvider);
            manageExternalServers.RepositoryLocator = RepositoryLocator;
            manageExternalServers.ShowDialog(this);
        }

        private void setTicketingSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TicketingSystemConfigurationUI ui = new TicketingSystemConfigurationUI();
            ui.SetItemActivator(_activator);
            _activator.ShowWindow(ui,true);
        }

        private void adjustFileLocationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LocationsAdjuster adjuster = new LocationsAdjuster();
            adjuster.RepositoryLocator = RepositoryLocator;
            adjuster.ShowDialog(this);
        }
        
        private void governanceManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GovernanceUI dialog = new GovernanceUI();
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.ShowDialog();
        }

        private void governanceReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var generator = new GovernanceReport(new DatasetTimespanCalculator(), RepositoryLocator.CatalogueRepository);

            if (generator.RequirementsMet())
                generator.GenerateReport();
            else
                MessageBox.Show(generator.RequirementsDescription());

        }
        private void logViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var servers = RepositoryLocator.CatalogueRepository.GetAllTier2Databases(Tier2DatabaseType.Logging);

            LogViewerForm form = null;

            if (!servers.Any())
                MessageBox.Show("None of your ExternalDatabaseServers are Logging servers, maybe you have yet to create a Logging server? go to Locations=>Manage External Servers and configure/create a Logging server", "No Logging Servers Found");
            else
                if (servers.Count() == 1)
                    form = new LogViewerForm(servers.Single());
                else
                {
                    SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(servers, false, false);
                    if (dialog.ShowDialog() == DialogResult.OK)
                        form = new LogViewerForm((ExternalDatabaseServer)dialog.Selected);
                }

            if (form != null)
                form.Show();
        }

        private void automationManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new AutomationServiceSlotManagement();
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.Show();
        }

        private void issueReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var generator = new CatalogueItemIssueReportGenerator(RepositoryLocator.CatalogueRepository);
            if (generator.RequirementsMet())
                generator.GenerateReport();
            else
                MessageBox.Show(generator.RequirementsDescription());
        }

        private void databaseAccessComplexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigureAccessRightsReport dialog = new ConfigureAccessRightsReport();
            dialog.Show();
        }

        private void metadataReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RequiresMicrosoftOffice requirement = new RequiresMicrosoftOffice();

            if (requirement.RequirementsMet())
            {
                ConfigureMetadataReport dialog = new ConfigureMetadataReport(_activator);
                dialog.RepositoryLocator = RepositoryLocator;
                dialog.Show();
            }
            else
                MessageBox.Show(requirement.RequirementsDescription());

        }


        private void serverSpecReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new DatabaseSizeReportUI();
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.Show();
        }

        private void dITAExtractionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            f.Text = "DITA Extraction of Catalogue Metadata";
            DitaExtractorUI d = new DitaExtractorUI();
            d.RepositoryLocator = RepositoryLocator;
            f.Width = d.Width + 10;
            f.Height = d.Height + 50;
            f.Controls.Add(d);
            f.Show();
        }

        private void launchDiagnosticsScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiagnosticsScreen dialog = new DiagnosticsScreen(null);
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.ShowDialog();
        }

        private void generateTestDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ExecuteCommandGenerateTestData(_activator).Execute();
        }
        
        private void showPerformanceCounterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new PerformanceCounterUI().Show();
        }

        private void clearAllAutocompleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecentHistoryOfControls.GetInstance().Clear();
        }
        
        private void openExeDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void userManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var req = new RequiresMicrosoftOffice();

                if (!req.RequirementsMet())
                {
                    MessageBox.Show(req.RequirementsDescription());
                    return;
                }

                FileInfo f = UsefulStuff.SprayFile(typeof(CatalogueCollectionUI).Assembly, "CatalogueManager.UserManual.docx", "UserManual.docx");
                Process.Start(f.FullName);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void generateClassTableSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var report = new DocumentationReportMapsDirectlyToDatabaseOfficeBit();

            if (!report.RequirementsMet())
            {
                MessageBox.Show(report.RequirementsDescription());
                return;
            }

            var imagesDictionary = new EnumImageCollection<RDMPConcept>(CatalogueIcons.ResourceManager).ToStringDictionary();
            report.GenerateReport(new PopupChecksUI("Generating class summaries", false), imagesDictionary);
        }

        private void generateUserInterfaceDocumentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var generator = new DocumentationReportFormsAndControlsUI(_activator);
            generator.RepositoryLocator = RepositoryLocator;
            generator.Show();
        }

        private void miIndexRebuild_Click(object sender, EventArgs e)
        {
            SqlConnection con;
            if (sender == mi_CatalogueDatabaseIndexRebuild)
                con = (SqlConnection)RepositoryLocator.CatalogueRepository.GetConnection().Connection;
            else if (sender == mi_DataExportDatabaseIndexRebuild)
                con = (SqlConnection)RepositoryLocator.DataExportRepository.GetConnection().Connection;
            else
                throw new ArgumentException();

            SMOIndexRebuilder rebuilder = new SMOIndexRebuilder();

            using (con)
            {
                con.Open();
                rebuilder.RebuildIndexes(con, new PopupChecksUI("Rebuild Indexes", false));
            }
        }
        
        private void showHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(currentTab == null)
                return;
            
            var typeDocs = _windowManager.ContentManager.DocumentationStore.TypeDocumentation;

            StringBuilder sb = new StringBuilder();

            string firstMatch = null;

            foreach (var c in currentTab.Controls)
                if (typeDocs.ContainsKey(c.GetType()))
                {
                    if (firstMatch == null)
                        firstMatch = c.GetType().Name;

                    sb.AppendLine(c.GetType().Name);
                    sb.AppendLine(typeDocs[c.GetType()]);
                    sb.AppendLine();
                }
            
            if(sb.Length >0)
                WideMessageBox.Show(sb.ToString(), environmentDotStackTrace: null, isModalDialog: true, keywordNotToAdd: firstMatch, title: "Help");
        
        }

        public void SetWindowManager(ToolboxWindowManager windowManager)
        {
            _windowManager = windowManager;
            _activator = _windowManager.ContentManager;

            //top menu strip setup / adjustment
            LocationsMenu.DropDownItems.Add(new DataExportMenu(RepositoryLocator));
            
            _windowManager.ContentManager.WindowFactory.TabChanged += WindowFactory_TabChanged;

            var tracker = TutorialTracker.GetInstance(_activator);
            foreach (Tutorial t in tracker.TutorialsAvailable)
                tutorialsToolStripMenuItem.DropDownItems.Add(new LaunchTutorialMenuItem(tutorialsToolStripMenuItem,_activator, t,tracker));

            tutorialsToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            tutorialsToolStripMenuItem.DropDownItems.Add(new DisableTutorialsMenuItem(tutorialsToolStripMenuItem,tracker));
            tutorialsToolStripMenuItem.DropDownItems.Add(new ResetTutorialsMenuItem(tutorialsToolStripMenuItem, tracker));

            rdmpTaskBar1.SetWindowManager(_windowManager);
        }

        void WindowFactory_TabChanged(object sender, DockContent newTab)
        {
            currentTab = newTab;

            var singleObjectControlTab = newTab as RDMPSingleControlTab;

            if (singleObjectControlTab == null)
            {
                _saveable = null;
                saveToolStripMenuItem.Enabled = false;
                return;
            }

            var saveable = singleObjectControlTab.GetControl() as ISaveableUI;
            var singleObject = singleObjectControlTab.GetControl() as IRDMPSingleDatabaseObjectControl;

            //if user wants to emphasise on tab change and theres an object we can emphasise associated with the control
            if (singleObject != null && UserSettingsFile.GetInstance().EmphasiseOnTabChanged && singleObject.DatabaseObject != null)
                _activator.RequestItemEmphasis(this,new EmphasiseRequest(singleObject.DatabaseObject));

            if (saveable != null)
            {
                saveToolStripMenuItem.Enabled = true;
                _saveable = saveable;
            }
            else
                saveToolStripMenuItem.Enabled = false;
        
            
        }

        private void navigateToObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new NavigateToObjectUI(_windowManager.ContentManager).Show();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _saveable.GetObjectSaverButton().Save();
        }

        private void managePluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginManagementForm dialogue = new PluginManagementForm();
            dialogue.RepositoryLocator = RepositoryLocator;
            dialogue.Show();
        }

        private void codeGenerationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ui = new PluginCodeGeneration();
            ui.Show();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new LaunchAnyDialogUI(_windowManager.ContentManager.DocumentationStore.TypeDocumentation,_activator);
            dialog.Show();
        }

        private void userSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = new UserSettingsFileUI();
            settings.Show();

        }

        private void licenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = new LicenseUI();
            l.ShowDialog();
        }

        public void InjectButton(ToolStripButton button)
        {
            rdmpTaskBar1.InjectButton(button);
        }
    }
}


