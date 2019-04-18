// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ResearchDataManagementPlatform.Theme;
using ResearchDataManagementPlatform.WindowManagement;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;
using ResearchDataManagementPlatform.WindowManagement.Licenses;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Settings;
using ReusableUIComponents;
using ReusableUIComponents.Theme;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform
{
    /// <summary>
    /// Main entry point into the RDMP software.  Hosts all tab collections and document windows for all RDMP tasks.  See CatalogueCollectionUI , DataExportCollectionUI ,
    ///  TableInfoCollectionUI , LoadMetadataCollectionUI and CohortIdentificationCollectionUI
    /// See 
    /// </summary>
    public partial class RDMPMainForm : RDMPForm
    {
        private readonly PersistenceDecisionFactory _persistenceFactory = new PersistenceDecisionFactory();
        private ITheme _theme;
        IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }

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
                    _theme = type == null ? new MyVS2015BlueTheme() : (ITheme) System.Activator.CreateInstance(type);
                }
                else
                    _theme = new MyVS2015BlueTheme();
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

            if (UserSettings.LicenseAccepted != new License("LIBRARYLICENSES").GetMd5OfLicense())
                new LicenseUI().ShowDialog();
        }

        WindowManager _windowManager;
        readonly RefreshBus _refreshBus = new RefreshBus();
        private FileInfo _persistenceFile;
        private ICheckNotifier _globalErrorCheckNotifier;

        public void SetRepositoryLocator(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            RepositoryLocator = repositoryLocator;
        }

        private void RDMPMainForm_Load(object sender, EventArgs e)
        {
            var exceptionCounter = new ExceptionCounterUI();
            _globalErrorCheckNotifier = exceptionCounter;
            _rdmpTopMenuStrip1.InjectButton(exceptionCounter);

            _windowManager = new WindowManager(_theme,this,_refreshBus, dockPanel1, RepositoryLocator, exceptionCounter);
            _rdmpTopMenuStrip1.SetWindowManager(_windowManager);
            
            //put the version of the software into the window title
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            this.Text = "Research Data Management Platform - v" + version;

            var rdmpDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RDMP"));
            if(!rdmpDir.Exists)
                rdmpDir.Create();

            _persistenceFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"RDMP", "RDMPDockPanelPersist.xml"));

            //if there is no persist file or user wants to show the home screen always on startup
            if (!_persistenceFile.Exists || UserSettings.ShowHomeOnStartup)
            {
                _windowManager.CloseAllToolboxes();
                _windowManager.CloseAllWindows();
                _windowManager.PopHome();
            }
            else
            {
                try
                {
                    if (_persistenceFile.Exists)
                        LoadFromXml(new FileStream(_persistenceFile.FullName, FileMode.Open));

                    //load the stateusing the method
                }
                catch (Exception ex)
                {
                    _globalErrorCheckNotifier.OnCheckPerformed(
                        new CheckEventArgs("Could not load window persistence due to error in persistence file",
                            CheckResult.Fail, ex));

                    //delete the persistence file and try again
                    MessageBox.Show("Persistence file corrupt, application will restart without persistence");
                    _persistenceFile.Delete();
                    Application.Restart();
                }
            }
         
            FormClosing += CloseForm;
        }

        public void LoadFromXml(Stream stream)
        {
            if (dockPanel1.DocumentStyle == DocumentStyle.SystemMdi)
            {
                foreach (Form form in MdiChildren)
                    form.Close();
            }
            else
            {
                foreach (IDockContent document in dockPanel1.DocumentsToArray())
                {
                    // IMPORANT: dispose all panes.
                    document.DockHandler.DockPanel = null;
                    document.DockHandler.Close();
                }
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
            UnicodeEncoding uniEncoding = new UnicodeEncoding();
            
            // You might not want to use the outer using statement that I have
            // I wasn't sure how long you would need the MemoryStream object    
            using (MemoryStream ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms, uniEncoding);
                try
                {
                    sw.Write(target.LayoutData);
                    sw.Flush();//otherwise you are risking empty stream
                    ms.Seek(0, SeekOrigin.Begin);

                    LoadFromXml(ms);
                }
                finally
                {
                    sw.Dispose();
                }
            }
        }


        public string GetCurrentLayoutXml()
        {
            UnicodeEncoding uniEncoding = new UnicodeEncoding();

            using (MemoryStream ms = new MemoryStream())
            {
                dockPanel1.SaveAsXml(ms, uniEncoding);

                ms.Seek(0, SeekOrigin.Begin);

                try
                {
                    return new StreamReader(ms).ReadToEnd();
                }
                finally
                {
                    ms.Dispose();
                }
            }
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {

            if (e.CloseReason == CloseReason.UserClosing && UserSettings.ConfirmApplicationExiting)
                if (MessageBox.Show("Are you sure you want to Exit?", "Confirm Exit", MessageBoxButtons.YesNo) !=
                    DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

            if (_persistenceFile != null)
                dockPanel1.SaveAsXml(_persistenceFile.FullName); //save when Form closes
        }

        private IDockContent DeserializeContent(string persiststring)
        {
            try
            {
                var toolbox = _persistenceFactory.ShouldCreateCollection(persiststring);
                if (toolbox.HasValue)
                {
                    var toolboxInstance = _windowManager.Create(toolbox.Value);
                    toolboxInstance.LoadPersistString(_windowManager.ActivateItems,persiststring);
                    return toolboxInstance;
                }

                var instruction = _persistenceFactory.ShouldCreateSingleObjectControl(persiststring,RepositoryLocator) ??
                                  _persistenceFactory.ShouldCreateObjectCollection(persiststring, RepositoryLocator);

                if (instruction != null)
                    return _windowManager.ActivateItems.Activate(instruction);
            }
            catch (Exception e)
            {
                _globalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Could not work out what window to show for persistence string '" + persiststring + "'",CheckResult.Fail, e));
            }

            return null;
        }
    }
}
