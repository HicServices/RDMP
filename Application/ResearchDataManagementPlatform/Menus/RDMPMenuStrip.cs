using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.FindAndReplace;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.LocationsMenu.Ticketing;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.PluginManagement;
using CatalogueManager.PluginManagement.CodeGeneration;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.SimpleDialogs.NavigateTo;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.Theme;
using CatalogueManager.Tutorials;
using CohortManager.CommandExecution.AtomicCommands;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using DataQualityEngine;
using HIC.Logging;
using MapsDirectlyToDatabaseTableUI;
using ResearchDataManagementPlatform.Menus.MenuItems;
using ResearchDataManagementPlatform.WindowManagement;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.Licenses;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Settings;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Settings;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.Menus
{

    /// <summary>
    /// The Top menu of the RDMP lets you do most tasks that do not relate directly to a single object (most single object tasks are accessed by right clicking the object).
    /// 
    /// <para>Locations:
    /// - Change which DataCatalogue database you are pointed at (not usually needed unless you have two different databases e.g. a Test database and a Live database)
    /// - Setup Logging / Anonymisation / Query Caching / Data Quality Engine databases
    /// - Configure a Ticketing system e.g. Jira for tracking time against tickets (you can set a ticket identifier for datasets, project extractions etc)
    /// - Perform bulk renaming operations across your entire catalogue database (useful for when someone remaps your server drives to a new letter! e.g. 'D:\Datasets\Private\' becomes 'E:\')
    /// - Refresh the window by reloading all Catalogues/TableInfos etc </para>
    /// 
    /// <para>View:
    /// - View/Edit dataset loading logic
    /// - View/Edit the governance approvals your datasets have (including attachments, period covered, datasets included in approval etc)
    /// - View the Logging database contents (a relational view of all activities undertaken by all Data Analysts using the RDMP - loading, extractions, dqe runs etc).</para>
    /// 
    /// <para>Reports
    /// - Generate a variety of reports that summarise the state of your datasets / governance etc</para>
    /// 
    /// <para>Help
    /// - View the user manual
    /// - View a technical description of each of the core objects maintained by RDMP (Catalogues, TableInfos etc) and what they mean (intended for programmers)
    /// - Generate user interface document (the document you are currently reading).</para>
    /// </summary>

    public partial class RDMPTopMenuStrip : RDMPUserControl
    {
        private IActivateItems _activator;
        private WindowManager _windowManager;
        
        private SaveMenuItem _saveToolStripMenuItem;
        private AtomicCommandUIFactory _atomicCommandUIFactory;

        public RDMPTopMenuStrip()
        {
            InitializeComponent();
        }
        
        private void configureExternalServersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ExecuteCommandConfigureDefaultServers(_activator).Execute();
        }

        private void setTicketingSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TicketingSystemConfigurationUI ui = new TicketingSystemConfigurationUI();
            ui.SetItemActivator(_activator);
            _activator.ShowWindow(ui,true);
        }


        private void governanceReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var generator = new GovernanceReport(new DatasetTimespanCalculator(), RepositoryLocator.CatalogueRepository);
            generator.GenerateReport();
        }
        private void logViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cmd = new ExecuteCommandViewLoggedData(_activator,LoggingTables.DataLoadTask);
            cmd.Execute();
        }
        
        private void databaseAccessComplexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigureAccessRightsReport dialog = new ConfigureAccessRightsReport();
            dialog.Show();
        }

        private void metadataReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigureMetadataReport dialog = new ConfigureMetadataReport(_activator);
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.Show();
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

        private void openExeDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(new DirectoryInfo(Environment.CurrentDirectory));
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
            report.GenerateReport( _activator.RepositoryLocator.CatalogueRepository.CommentStore,
                new PopupChecksUI("Generating class summaries", false),
                _activator.CoreIconProvider,
                typeof(Catalogue).Assembly,
                typeof(ExtractionConfiguration).Assembly);
        }

        private void generateUserInterfaceDocumentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var generator = new DocumentationReportFormsAndControlsUI(_activator);
            generator.RepositoryLocator = RepositoryLocator;
            generator.Show();
        }
        
        private void showHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var current = _windowManager.Navigation.CurrentTab;

            var t = current as RDMPSingleControlTab;
            if(t == null)
                return;

            t.ShowHelp(_activator);
        }

        public void SetWindowManager(WindowManager windowManager)
        {
            _windowManager = windowManager;
            _activator = _windowManager.ActivateItems;
            _atomicCommandUIFactory = new AtomicCommandUIFactory(_activator);
            

            //top menu strip setup / adjustment
            LocationsMenu.DropDownItems.Add(new DataExportMenu(_activator));
            _saveToolStripMenuItem = new SaveMenuItem
            {
                Enabled = false,
                Name = "saveToolStripMenuItem",
                Size = new System.Drawing.Size(214, 22)
            };
            fileToolStripMenuItem.DropDownItems.Add(_saveToolStripMenuItem);

            _windowManager.TabChanged += WindowFactory_TabChanged;

            var tracker = new TutorialTracker(_activator);
            foreach (Tutorial t in tracker.TutorialsAvailable)
                tutorialsToolStripMenuItem.DropDownItems.Add(new LaunchTutorialMenuItem(tutorialsToolStripMenuItem, _activator, t, tracker));

            tutorialsToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            tutorialsToolStripMenuItem.DropDownItems.Add(new DisableTutorialsMenuItem(tutorialsToolStripMenuItem, tracker));
            tutorialsToolStripMenuItem.DropDownItems.Add(new ResetTutorialsMenuItem(tutorialsToolStripMenuItem, tracker));

            closeToolStripMenuItem.Enabled = false;

            rdmpTaskBar1.SetWindowManager(_windowManager);

            //Catalogue commands
            AddToNew(new ExecuteCommandCreateNewCatalogueByImportingFile(_activator));
            AddToNew(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator, false));
            AddToNew(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator));
            AddToNew(new ExecuteCommandCreateNewLoadMetadata(_activator));
            AddToNew(new ExecuteCommandCreateNewStandardRegex(_activator));

            //Saved cohorts database creation
            newToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            AddToNew(new ExecuteCommandCreateNewCohortDatabaseUsingWizard(_activator));

            //cohort creation
            newToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            AddToNew(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator));
            AddToNew(new ExecuteCommandCreateNewCohortFromFile(_activator));
            AddToNew(new ExecuteCommandCreateNewCohortFromCatalogue(_activator));
            
            //Data export commands
            newToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            AddToNew(new ExecuteCommandCreateNewExtractableDataSetPackage(_activator));
            AddToNew(new ExecuteCommandCreateNewDataExtractionProject(_activator));
            AddToNew(new ExecuteCommandRelease(_activator));

            // Location menu
            LocationsMenu.DropDownItems.Add(_atomicCommandUIFactory.CreateMenuItem(new ExecuteCommandChoosePlatformDatabase(RepositoryLocator)));

            _activator.Theme.ApplyTo(menuStrip1);
        }

        private void AddToNew(IAtomicCommand cmd)
        {
            newToolStripMenuItem.DropDownItems.Add(_atomicCommandUIFactory.CreateMenuItem(cmd));
        }

        void WindowFactory_TabChanged(object sender, IDockContent newTab)
        {
            closeToolStripMenuItem.Enabled = newTab != null && !(newTab is PersistableToolboxDockContent);
            showHelpToolStripMenuItem.Enabled = newTab is RDMPSingleControlTab;

            var singleObjectControlTab = newTab as RDMPSingleControlTab;
            if (singleObjectControlTab == null)
            {
                _saveToolStripMenuItem.Saveable = null;
                return;
            }

            var saveable = singleObjectControlTab.GetControl() as ISaveableUI;
            var singleObject = singleObjectControlTab.GetControl() as IRDMPSingleDatabaseObjectControl;

            //if user wants to emphasise on tab change and theres an object we can emphasise associated with the control
            if (singleObject != null && UserSettings.EmphasiseOnTabChanged && singleObject.DatabaseObject != null)
                _activator.RequestItemEmphasis(this, new EmphasiseRequest(singleObject.DatabaseObject));

            _saveToolStripMenuItem.Saveable = saveable;

            navigateBackwardToolStripMenuItem.Enabled = _windowManager.Navigation.CanBack();
            navigateForwardToolStripMenuItem.Enabled = _windowManager.Navigation.CanForward();
        }
        
        private void managePluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginManagementForm dialogue = new PluginManagementForm(_activator);
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
            var dialog = new RunUI(_windowManager.ActivateItems);
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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openables = _activator.CoreChildProvider.GetAllSearchables().Select(kvp=>kvp.Key).Where(o => _activator.CommandExecutionFactory.CanActivate(o));

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(openables, false, false);

            if (dialog.ShowDialog() == DialogResult.OK)
                _activator.WindowArranger.SetupEditAnything(this,dialog.Selected);
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var visibleCollection = _windowManager.GetFocusedCollection();

            new NavigateToObjectUI(_activator, null, visibleCollection).Show();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _windowManager.CloseCurrentTab();
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _activator.ShowWindow(new FindAndReplaceUI(_activator),true);
        }

        private void navigateBackwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _windowManager.Navigation.Back(true);
        }

        private void navigateForwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _windowManager.Navigation.Forward(true);
        }
    }
}


