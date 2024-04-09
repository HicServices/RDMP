// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Databases;
using Rdmp.Core.DataQualityEngine;
using Rdmp.Core.Logging;
using Rdmp.Core.Reports;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.LocationsMenu.Ticketing;
using Rdmp.UI.MainFormUITabs;
using Rdmp.UI.Menus.MenuItems;
using Rdmp.UI.Performance;
using Rdmp.UI.PluginManagement.CodeGeneration;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SimpleDialogs.NavigateTo;
using Rdmp.UI.TestsAndSetup;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Tutorials;
using ResearchDataManagementPlatform.Menus.MenuItems;
using ResearchDataManagementPlatform.WindowManagement;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.Licenses;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.Menus;

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
/// - View a technical description of each of the core objects maintained by RDMP (Catalogues, TableInfos etc) and what they mean (intended for programmers)</para>
/// </summary>
public partial class RDMPTopMenuStripUI : RDMPUserControl
{
    private WindowManager _windowManager;

    private SaveMenuItem _saveToolStripMenuItem;
    private AtomicCommandUIFactory _atomicCommandUIFactory;

    public RDMPTopMenuStripUI()
    {
        InitializeComponent();
    }

    private void BuildSwitchInstanceMenuItems()
    {
        var args = RDMPBootStrapper.ApplicationArguments;

        // somehow app was launched without populating the load args
        if (args == null) return;

        var origYamlFile = args.ConnectionStringsFileLoaded;

        //default settings were used if no yaml file was specified or the file specified did not exist
        var defaultsUsed = origYamlFile == null;

        // if defaults were not used then it is valid to switch to them
        switchToDefaultSettings.Enabled = !defaultsUsed;

        switchToDefaultSettings.Checked = defaultsUsed;
        launchNewWithDefaultSettings.Checked = defaultsUsed;

        // load the yaml files in the RDMP binary directory
        var exeDir = UsefulStuff.GetExecutableDirectory();
        AddMenuItemsForSwitchingToInstancesInYamlFilesOf(origYamlFile, exeDir);

        // also add yaml files from wherever they got their original yaml file
        if (origYamlFile?.FileLoaded != null && !exeDir.FullName.Equals(origYamlFile.FileLoaded.Directory?.FullName))
            AddMenuItemsForSwitchingToInstancesInYamlFilesOf(origYamlFile, origYamlFile.FileLoaded.Directory);
    }

    private void AddMenuItemsForSwitchingToInstancesInYamlFilesOf(ConnectionStringsYamlFile origYamlFile,
        DirectoryInfo dir)
    {
        foreach (var yaml in dir.GetFiles("*.yaml"))
        {
            // if the yaml file is invalid bail out early
            if (!ConnectionStringsYamlFile.TryLoadFrom(yaml, out var connectionStrings))
                continue;

            var isSameAsCurrent = origYamlFile?.FileLoaded != null &&
                                  yaml.FullName.Equals(origYamlFile.FileLoaded.FullName);

            var launchNew = new ToolStripMenuItem(connectionStrings.Name ?? yaml.Name, null,
                (_, _) => { LaunchNew(connectionStrings); })
            {
                Checked = isSameAsCurrent,
                ToolTipText = connectionStrings.Description ?? yaml.FullName
            };

            var switchTo = new ToolStripMenuItem(connectionStrings.Name ?? yaml.Name, null,
                (_, _) => { SwitchTo(connectionStrings); })
            {
                Enabled = !isSameAsCurrent,
                Checked = isSameAsCurrent,
                ToolTipText = connectionStrings.Description ?? yaml.FullName
            };

            launchAnotherInstanceToolStripMenuItem.DropDownItems.Add(launchNew);
            switchToInstanceToolStripMenuItem.DropDownItems.Add(switchTo);
        }
    }

    private static void SwitchTo(ConnectionStringsYamlFile yaml)
    {
        LaunchNew(yaml);

        Application.Exit();
    }

    private static void LaunchNew(ConnectionStringsYamlFile yaml)
    {
        var exeName = Path.Combine(UsefulStuff.GetExecutableDirectory().FullName,
            Process.GetCurrentProcess().ProcessName);
        if (yaml == null)
            Process.Start(exeName);
        else
            Process.Start(exeName,
                $"--{nameof(RDMPCommandLineOptions.ConnectionStringsFile)} \"{yaml.FileLoaded.FullName}\"");
    }

    private void configureExternalServersToolStripMenuItem_Click(object sender, EventArgs e)
    {
        new ExecuteCommandConfigureDefaultServers(Activator).Execute();
    }

    private void setTicketingSystemToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var ui = new TicketingSystemConfigurationUI();
        Activator.ShowWindow(ui, true);
    }


    private void governanceReportToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var generator = new GovernanceReport(new DatasetTimespanCalculator(),
            Activator.RepositoryLocator.CatalogueRepository);
        generator.GenerateReport();
    }

    private void logViewerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandViewLogs(Activator, new LogViewerFilter(LoggingTables.DataLoadTask));
        cmd.Execute();
    }


    private void metadataReportToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandGenerateMetadataReport(Activator);
        cmd.Execute();
    }


    private void dITAExtractionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var f = new Form
        {
            Text = "DITA Extraction of Catalogue Metadata"
        };
        var d = new DitaExtractorUI();
        d.SetItemActivator(Activator);
        f.Width = d.Width + 10;
        f.Height = d.Height + 50;
        f.Controls.Add(d);
        f.Show();
    }

    private void generateTestDataToolStripMenuItem_Click(object sender, EventArgs e)
    {
        new ExecuteCommandGenerateTestDataUI(Activator).Execute();
    }

    private void showPerformanceCounterToolStripMenuItem_Click(object sender, EventArgs e)
    {
        new PerformanceCounterUI().Show();
    }

    private void openExeDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            UsefulStuff.ShowPathInWindowsExplorer(UsefulStuff.GetExecutableDirectory());
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
            UsefulStuff.OpenUrl("https://github.com/HicServices/RDMP#research-data-management-platform");
        }
        catch (Exception exception)
        {
            ExceptionViewer.Show(exception);
        }
    }

    private void generateClassTableSummaryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var report = new DocumentationReportDatabaseEntities();
        report.GenerateReport(Activator.RepositoryLocator.CatalogueRepository.CommentStore,
            new PopupChecksUI("Generating class summaries", false),
            Activator.CoreIconProvider,
            true);
    }

    private void showHelpToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (_windowManager.Navigation.Current is RDMPSingleControlTab t)
            t.ShowHelp(Activator);
    }

    public void SetWindowManager(WindowManager windowManager)
    {
        SetItemActivator(windowManager.ActivateItems.Value);

        _windowManager = windowManager;
        _atomicCommandUIFactory = new AtomicCommandUIFactory(Activator);


        //top menu strip setup / adjustment
        LocationsMenu.DropDownItems.Add(new DataExportMenu(Activator));
        _saveToolStripMenuItem = new SaveMenuItem
        {
            Enabled = false,
            Name = "saveToolStripMenuItem",
            Size = new System.Drawing.Size(214, 22)
        };
        fileToolStripMenuItem.DropDownItems.Insert(3, _saveToolStripMenuItem);

        _windowManager.TabChanged += WindowFactory_TabChanged;
        _windowManager.Navigation.Changed += (s, e) => UpdateForwardBackEnabled();

        var tracker = new TutorialTracker(Activator);
        foreach (var t in tracker.TutorialsAvailable)
            tutorialsToolStripMenuItem.DropDownItems.Add(new LaunchTutorialMenuItem(tutorialsToolStripMenuItem,
                Activator, t, tracker));

        tutorialsToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

        tutorialsToolStripMenuItem.DropDownItems.Add(new DisableTutorialsMenuItem(tutorialsToolStripMenuItem, tracker));
        tutorialsToolStripMenuItem.DropDownItems.Add(new ResetTutorialsMenuItem(tutorialsToolStripMenuItem, tracker));

        closeToolStripMenuItem.Enabled = false;

        rdmpTaskBar1.SetWindowManager(_windowManager);

        // Location menu
        instancesToolStripMenuItem.DropDownItems.Add(_atomicCommandUIFactory.CreateMenuItem(
            new ExecuteCommandChoosePlatformDatabase(Activator.RepositoryLocator)
                { OverrideCommandName = "Change Default Instance" }));

        Activator.Theme.ApplyTo(menuStrip1);

        try
        {
            BuildSwitchInstanceMenuItems();
        }
        catch (Exception ex)
        {
            Activator.GlobalErrorCheckNotifier.OnCheckPerformed(
                new CheckEventArgs("Failed to BuildSwitchInstanceMenuItems", CheckResult.Fail, ex));
        }

        launchAnotherInstanceToolStripMenuItem.ToolTipText =
            "Start another copy of the RDMP process targetting the same (or another) RDMP platform database";

        if (switchToInstanceToolStripMenuItem.DropDownItems.Count > 1)
        {
            switchToInstanceToolStripMenuItem.Enabled = true;
            switchToInstanceToolStripMenuItem.ToolTipText =
                "Close the application and start another copy of the RDMP process targetting another RDMP platform database";
        }
        else
        {
            switchToInstanceToolStripMenuItem.Enabled = false;
            switchToInstanceToolStripMenuItem.ToolTipText =
                "There are no other RDMP platform databases configured, create a .yaml file with connection strings to enable this feature";
        }
    }


    private IAtomicCommand[] GetNewCommands()
    {
        //Catalogue commands
        return new IAtomicCommand[]
        {
            new ExecuteCommandCreateNewCatalogueByImportingFileUI(Activator),
            new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(Activator),
            new ExecuteCommandImportTableInfo(Activator, null, false),
            new ExecuteCommandCreateNewCohortIdentificationConfiguration(Activator),
            new ExecuteCommandCreateNewLoadMetadata(Activator),
            new ExecuteCommandCreateNewStandardRegex(Activator),
            new ExecuteCommandCreateNewCohortDatabaseUsingWizard(Activator),
            new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(Activator, null),
            new ExecuteCommandCreateNewCohortFromFile(Activator, null),
            new ExecuteCommandCreateNewCohortFromCatalogue(Activator),
            new ExecuteCommandCreateNewCohortFromTable(Activator, null),
            new ExecuteCommandCreateNewExtractableDataSetPackage(Activator),
            new ExecuteCommandCreateNewDataExtractionProject(Activator),
            new ExecuteCommandRelease(Activator) { OverrideCommandName = "New Release..." },
            new ExecuteCommandCreateANOVersion(Activator)
        };
    }

    private void WindowFactory_TabChanged(object sender, IDockContent newTab)
    {
        closeToolStripMenuItem.Enabled = newTab is not null and not PersistableToolboxDockContent;
        showHelpToolStripMenuItem.Enabled = newTab is RDMPSingleControlTab;

        if (newTab is not RDMPSingleControlTab singleObjectControlTab)
        {
            _saveToolStripMenuItem.Saveable = null;
            return;
        }

        var saveable = singleObjectControlTab.Control as ISaveableUI;

        //if user wants to emphasise on tab change and there's an object we can emphasise associated with the control
        if (singleObjectControlTab.Control is IRDMPSingleDatabaseObjectControl singleObject &&
            UserSettings.EmphasiseOnTabChanged && singleObject.DatabaseObject != null)
        {
            var isCicChild = Activator.CoreChildProvider.GetDescendancyListIfAnyFor(singleObject.DatabaseObject)
                ?.Parents?.Any(p => p is CohortIdentificationConfiguration);

            //don't emphasise things that live under cics because it doesn't result in a collection being opened but instead opens the cic Tab (which could result in you being unable to get to your original tab!)
            if (isCicChild == false)
            {
                _windowManager.Navigation.Suspend();
                Activator.RequestItemEmphasis(this, new EmphasiseRequest(singleObject.DatabaseObject));
                _windowManager.Navigation.Resume();
            }
        }


        _saveToolStripMenuItem.Saveable = saveable;
    }

    /// <summary>
    /// Updates the enabled status (greyed out) of the Forward/Back menu items (includes the use of keyobard shortcuts)
    /// </summary>
    private void UpdateForwardBackEnabled()
    {
        navigateBackwardToolStripMenuItem.Enabled = _windowManager.Navigation.CanBack();
        navigateForwardToolStripMenuItem.Enabled = _windowManager.Navigation.CanForward();
    }


    private void codeGenerationToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var ui = new GenerateClassCodeFromTableUI();
        ui.Show();
    }

    private void runToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var dialog = new RunUI(_windowManager.ActivateItems.Value);
        dialog.Show();
    }

    private void userSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var settings = new UserSettingsFileUI(Activator);
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
        Activator.SelectAnythingThen(new DialogArgs
        {
            WindowTitle = "Open"
        }, o => Activator.WindowArranger.SetupEditAnything(this, o));
    }

    private void findToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Activator.SelectAnythingThen(new DialogArgs
        {
            WindowTitle = "Find",
            InitialSearchTextGuid = new Guid("00a0733b-848f-4bf3-bcde-7028fe159050"),
            IsFind = true,
            TaskDescription = "Enter the name of an object or part of the name or the dataset/project it is in."
        }, o => Activator.RequestItemEmphasis(this, new EmphasiseRequest(o)));
    }

    private void closeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _windowManager.CloseCurrentTab();
    }

    private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Activator.ShowWindow(new FindAndReplaceUI(Activator), true);
    }

    private void navigateBackwardToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _windowManager.Navigation.Back(true);
    }

    private void navigateForwardToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _windowManager.Navigation.Forward(true);
    }

    private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // AutoUpdater.NET is Windows-only for now:
        if (!OperatingSystem.IsWindowsVersionAtLeast(7))
            return;

        var url = "https://raw.githubusercontent.com/HicServices/RDMP/main/rdmp-client.xml";

        // Give user a chance to change the URL that is updating from
        if (!Activator.TypeText("Update Location", "Url:", int.MaxValue, url, out url, false))
            return;

        try
        {
            AutoUpdater.ReportErrors = true;
            AutoUpdater.Start(url);
        }
        catch (Exception ex)
        {
            Activator.ShowException($"Failed to update from {url}", ex);
        }
    }

    private void ListAllTypesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var file = new FileInfo(Path.GetTempFileName());
        File.WriteAllLines(file.FullName, Rdmp.Core.Repositories.MEF.GetAllTypes().Select(t => t.FullName));
        UsefulStuff.ShowPathInWindowsExplorer(file);
    }

    private void NewToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var dlg = new SelectDialog<IAtomicCommand>(new DialogArgs
        {
            WindowTitle = "Create New",
            TaskDescription = "What do you want to create?"
        }, Activator, GetNewCommands(), false);

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            var picked = dlg.Selected;
            picked.Execute();
        }
    }

    private void quitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void newSessionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandStartSession(Activator, null, null);
        cmd.Execute();
    }


    private void queryDataExport_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandQueryPlatformDatabase(Activator, nameof(DataExportPatcher));

        if (cmd.IsImpossible)
        {
            Activator.Show("Cannot Query Database", cmd.ReasonCommandImpossible);
            return;
        }

        cmd.Execute();
    }

    private void queryCatalogue_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandQueryPlatformDatabase(Activator, nameof(CataloguePatcher));

        if (cmd.IsImpossible)
        {
            Activator.Show("Cannot Query Database", cmd.ReasonCommandImpossible);
            return;
        }

        cmd.Execute();
    }

    private void restartApplicationToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (UserSettings.ConfirmApplicationExiting &&
            Activator.Confirm("Restart Application?", "Confirm Restart") == false)
            return;


        ApplicationRestarter.Restart();
    }

    private void lastCommandMonitorToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var lastCommand = new LastCommandUI();
        lastCommand.Show();
    }

    private void switchToUsingUserSettings_Click(object sender, EventArgs e)
    {
        SwitchTo(null);
    }

    private void launchNewInstanceWithUserSettings_Click(object sender, EventArgs e)
    {
        LaunchNew(null);
    }

    private void terminateProcessToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (Activator.YesNo("Terminate the process without saving?", "Terminate")) Process.GetCurrentProcess().Kill();
    }

    private void findMultipleToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var cmd = new ExecuteCommandStartSession(Activator, null, ExecuteCommandStartSession.FindResultsTitle);
        cmd.Execute();
    }

    private void viewHistoryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Activator.ShowWindow(new CommitsUI(Activator), true);
    }
}