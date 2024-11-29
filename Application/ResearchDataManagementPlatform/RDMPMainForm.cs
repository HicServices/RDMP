// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Theme;
using ResearchDataManagementPlatform.Theme;
using ResearchDataManagementPlatform.WindowManagement;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;
using ResearchDataManagementPlatform.WindowManagement.Licenses;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform;

/// <summary>
/// Main entry point into the RDMP software.  Hosts all tab collections and document windows for all RDMP tasks.  See CatalogueCollectionUI , DataExportCollectionUI ,
///  TableInfoCollectionUI , LoadMetadataCollectionUI and CohortIdentificationCollectionUI
/// See
/// </summary>
public partial class RDMPMainForm : RDMPForm
{
    private readonly PersistenceDecisionFactory _persistenceFactory = new();
    private readonly ITheme _theme;
    private IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }

    /// <summary>
    /// True while the main form is loading (e.g. from a persistence file)
    /// </summary>
    public static bool Loading = true;

    public RDMPMainForm()
    {
        InitializeComponent();

        PatchController.EnableAll = true;

        try
        {
            var t = UserSettings.Theme;
            if (!string.IsNullOrWhiteSpace(t))
            {
                var type = Type.GetType(t);
                _theme = type == null ? new MyVS2015BlueTheme() : (ITheme)System.Activator.CreateInstance(type);
            }
            else
            {
                _theme = new MyVS2015BlueTheme();
            }
        }
        catch (Exception)
        {
            _theme = new MyVS2015BlueTheme();
        }

        _theme.ApplyThemeToMenus = UserSettings.ApplyThemeToMenus;

        dockPanel1.Theme = (ThemeBase)_theme;
        dockPanel1.Theme.Extender.FloatWindowFactory = new CustomFloatWindowFactory();
        dockPanel1.DefaultFloatWindowSize = new Size(640, 520);
        dockPanel1.ShowDocumentIcon = true;
        dockPanel1.DocumentStyle = DocumentStyle.DockingWindow;

        WindowState = FormWindowState.Maximized;
        CloseOnEscape = false;

        if (UserSettings.LicenseAccepted != new WindowManagement.Licenses.License("LIBRARYLICENSES").GetHashOfLicense())
            new LicenseUI().ShowDialog();
    }

    private WindowManager _windowManager;
    private readonly RefreshBus _refreshBus = new();
    private FileInfo _persistenceFile;
    private ICheckNotifier _globalErrorCheckNotifier;
    private string _version;
    private string _connectedTo;

    public void SetRepositoryLocator(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        RepositoryLocator = repositoryLocator;
    }

    private void RDMPMainForm_Load(object sender, EventArgs e)
    {
        var exceptionCounter = new ExceptionCounterUI();
        _globalErrorCheckNotifier = exceptionCounter;
        _rdmpTopMenuStrip1.InjectButton(exceptionCounter);

        _windowManager = new WindowManager(_theme, this, _refreshBus, dockPanel1, RepositoryLocator, exceptionCounter);
        SetItemActivator(_windowManager.ActivateItems);

        _rdmpTopMenuStrip1.SetWindowManager(_windowManager);

        //put the version of the software into the window title

        _version = StartupUI.GetVersion();

        //put the current platform database into the window title too
        if (Activator?.RepositoryLocator?.CatalogueRepository is TableRepository connectedTo)
        {
            var database = connectedTo.DiscoveredServer?.GetCurrentDatabase();
            var instanceDescription = "";

            var connectionStringsFileLoaded = RDMPBootStrapper.ApplicationArguments?.ConnectionStringsFileLoaded;
            if (connectionStringsFileLoaded != null)
                instanceDescription =
                    $" - {connectionStringsFileLoaded.Name ?? connectionStringsFileLoaded.FileLoaded.Name}";
            if (database != null)
                _connectedTo = $"({database.GetRuntimeName()} on {database.Server.Name}){instanceDescription}";
        }

        Text = "Research Data Management Platform";

        var rdmpDir =
            new DirectoryInfo(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RDMP"));
        if (!rdmpDir.Exists)
            rdmpDir.Create();

        _persistenceFile =
            new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RDMP",
                "RDMPDockPanelPersist.xml"));

        //if there is no persist file or user wants to show the home screen always on startup
        if (!_persistenceFile.Exists || UserSettings.ShowHomeOnStartup)
            _windowManager.PopHome();
        else if (_persistenceFile.Exists)
            try
            {
                //load the state using the method
                LoadFromXml(new FileStream(_persistenceFile.FullName, FileMode.Open));
            }
            catch (Exception ex)
            {
                _globalErrorCheckNotifier.OnCheckPerformed(
                    new CheckEventArgs("Could not load window persistence due to error in persistence file",
                        CheckResult.Fail, ex));

                //delete the persistence file and try again
                MessageBox.Show("Persistence file corrupt, application will restart without persistence");
                _persistenceFile.Delete();
                ApplicationRestarter.Restart();
            }

        FormClosing += CloseForm;
        Loading = false;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
        get => base.Text;
        set => base.Text = $"{value} v{_version} {_connectedTo}".Trim();
    }

    private void LoadFromXml(Stream stream)
    {
        if (dockPanel1.DocumentStyle == DocumentStyle.SystemMdi)
            foreach (var form in MdiChildren)
                form.Close();
        else
            foreach (var document in dockPanel1.DocumentsToArray())
            {
                // IMPORTANT: dispose all panes.
                document.DockHandler.DockPanel = null;
                document.DockHandler.Close();
            }

        foreach (var pane in dockPanel1.Panes.ToList())
        {
            pane.CloseActiveContent();
            pane.Dispose();
        }

        // IMPORTANT: dispose all float windows.
        foreach (var window in dockPanel1.FloatWindows.ToList())
            window.Dispose();

        System.Diagnostics.Debug.Assert(dockPanel1.Panes.Count == 0);
        System.Diagnostics.Debug.Assert(dockPanel1.Contents.Count == 0);
        System.Diagnostics.Debug.Assert(dockPanel1.FloatWindows.Count == 0);

        dockPanel1.LoadFromXml(stream, DeserializeContent);
    }

    public void LoadFromXml(WindowLayout target)
    {
        var uniEncoding = new UnicodeEncoding();

        // You might not want to use the outer using statement that I have
        // I wasn't sure how long you would need the MemoryStream object
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms, uniEncoding);
        sw.Write(target.LayoutData);
        sw.Flush(); //otherwise you are risking empty stream
        ms.Seek(0, SeekOrigin.Begin);
        LoadFromXml(ms);
    }


    public string GetCurrentLayoutXml()
    {
        var uniEncoding = new UnicodeEncoding();

        using var ms = new MemoryStream();
        dockPanel1.SaveAsXml(ms, uniEncoding);
        ms.Seek(0, SeekOrigin.Begin);
        return new StreamReader(ms).ReadToEnd();
    }

    private void CloseForm(object sender, FormClosingEventArgs e)
    {
        // give the window manager a chance to cancel closing
        _windowManager.OnFormClosing(e);

        if (e.Cancel) return;

        if (e.CloseReason == CloseReason.UserClosing && UserSettings.ConfirmApplicationExiting)
            if (!Activator.YesNo("Are you sure you want to Exit?", "Confirm Exit"))
            {
                e.Cancel = true;
                return;
            }

        try
        {
            if (_persistenceFile == null) return;
            if (_persistenceFile.Directory?.Exists == false)
                _persistenceFile.Directory.Create();
            dockPanel1.SaveAsXml(_persistenceFile.FullName); //save when Form closes
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show("Could not write persistence file", ex);
        }
    }

    private IDockContent DeserializeContent(string persiststring)
    {
        try
        {
            var toolbox = PersistenceDecisionFactory.ShouldCreateCollection(persiststring);
            if (toolbox.HasValue)
            {
                var toolboxInstance = _windowManager.Create(toolbox.Value);
                return toolboxInstance;
            }

            var instruction = PersistenceDecisionFactory.ShouldCreateBasicControl(persiststring, RepositoryLocator) ??
                              PersistenceDecisionFactory.ShouldCreateSingleObjectControl(persiststring,
                                  RepositoryLocator) ??
                              PersistenceDecisionFactory.ShouldCreateObjectCollection(persiststring, RepositoryLocator);

            if (instruction != null)
                return _windowManager.ActivateItems.Activate(instruction, _windowManager.ActivateItems);
        }
        catch (Exception e)
        {
            _globalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs(
                $"Could not work out what window to show for persistence string '{persiststring}'", CheckResult.Fail,
                e));
        }

        return null;
    }
}