using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ResearchDataManagementPlatform.WindowManagement;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.Licenses;
using ResearchDataManagementPlatform.WindowManagement.UserSettings;
using ReusableUIComponents;
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
        private PersistenceDecisionFactory _persistenceFactory = new PersistenceDecisionFactory();

        public RDMPMainForm()
        {
            InitializeComponent();

            dockPanel1.DocumentStyle = DocumentStyle.DockingWindow;
            WindowState = FormWindowState.Maximized;

            if(!UserSettingsFile.GetInstance().LicenseAccepted)
                new LicenseUI().ShowDialog();
        }

        ToolboxWindowManager _windowManager;
        RefreshBus _refreshBus = new RefreshBus();
        private FileInfo _persistenceFile;

        private void RDMPMainForm_Load(object sender, EventArgs e)
        {
            if (RepositoryLocator == null)
                return;

            _windowManager = new ToolboxWindowManager(_refreshBus, dockPanel1, RepositoryLocator);
            rdmpMenuStrip1.SetWindowManager(_windowManager);
            
            //put the version of the software into the window title
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            this.Text = "Research Data Management Platform - v" + version;

            var rdmpDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RDMP"));
            if(!rdmpDir.Exists)
                rdmpDir.Create();

            _persistenceFile = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"RDMP", "RDMPDockPanelPersist.xml"));

            //if there is no persist file or user wants to show the home screen always on startup
            if (!_persistenceFile.Exists || UserSettingsFile.GetInstance().ShowHomeScreenInsteadOfPersistence)
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
                        dockPanel1.LoadFromXml(new FileStream(_persistenceFile.FullName, FileMode.Open), DeserializeContent);
                    //load the stateusing the method
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show("Could not load window persistence due to error in persistence file", ex);
                }
            }
         
            FormClosing += CloseForm;
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {

            if(e.CloseReason == CloseReason.UserClosing)
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
                    return _windowManager.Create(toolbox.Value);

                var instruction = _persistenceFactory.ShouldCreateSingleObjectControl(persiststring,RepositoryLocator) ??
                                  _persistenceFactory.ShouldCreateObjectCollection(persiststring, RepositoryLocator);

                if (instruction != null)
                    return _windowManager.ContentManager.Activate(instruction);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show("Could not work out what window to show for persistence string '" + persiststring + "'",e);
            }

            return null;
        }

    }
}
