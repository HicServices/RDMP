using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ResearchDataManagementPlatform.WindowManagement;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;
using ResearchDataManagementPlatform.WindowManagement.Licenses;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Settings;
using ReusableUIComponents;
using ReusableUIComponents.Settings;
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

        public RDMPMainForm()
        {
            InitializeComponent();

            PatchController.EnableAll = true;
            dockPanel1.Theme = new VS2015LightTheme();
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

        private void RDMPMainForm_Load(object sender, EventArgs e)
        {
            if (RepositoryLocator == null)
                return;

            var exceptionCounter = new ExceptionCounterUI();
            _globalErrorCheckNotifier = exceptionCounter;
            _rdmpTopMenuStrip1.InjectButton(exceptionCounter);

            _windowManager = new WindowManager(this,_refreshBus, dockPanel1, RepositoryLocator, exceptionCounter);
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

        private void RDMPMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SqlDependencyTableMonitor.Stop();
        }
    }
}
