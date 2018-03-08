using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.ExternalDatabaseServerPatching;
using CatalogueLibrary.Reports;
using CatalogueLibrary.Repositories;
using CatalogueManager.LocationsMenu;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.TestsAndSetup.StartupUI;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ScintillaNET;


namespace CatalogueManager.TestsAndSetup
{
    public class RDMPBootStrapper<T> where T : RDMPForm, new()
    {
        private T _mainForm;

        private IRDMPPlatformRepositoryServiceLocator _registry;


        public void Show(bool requiresDataExportDatabaseToo)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //tell me when you blow up somewhere in the windows API instead of somewhere sensible
            Application.ThreadException += (sender, args) => ExceptionViewer.Show(args.Exception,false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => ExceptionViewer.Show((Exception)args.ExceptionObject,false);

            try
            {
                //show the startup dialog
                Startup startup = new Startup();
                var startupUI = new StartupUIMainForm(startup);
                startupUI.ShowDialog();

                _registry = startup.RepositoryLocator;
                
                //launch the main application form T
                _mainForm = new T();
                _mainForm.RepositoryLocator = _registry;
                Application.Run(_mainForm);
            }
            catch (Exception e)
            {
                DiagnosticsScreen.OfferLaunchingDiagnosticsScreenOrEnvironmentExit(_registry,null, e);
                MessageBox.Show("Diagnostics has been closed, application will now exit");
            }
        }
    }
}
