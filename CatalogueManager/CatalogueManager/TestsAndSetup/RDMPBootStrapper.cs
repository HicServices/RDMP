using System;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.TestsAndSetup.StartupUI;
using RDMPStartup;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;


namespace CatalogueManager.TestsAndSetup
{
    public class RDMPBootStrapper<T> where T : RDMPForm, new()
    {
        private readonly string catalogueConnection;
        private readonly string dataExportConnection;
        private T _mainForm;

        private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

        public RDMPBootStrapper(string catalogueConnection, string dataExportConnection)
        {
            this.catalogueConnection = catalogueConnection;
            this.dataExportConnection = dataExportConnection;
        }

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
                if (!String.IsNullOrWhiteSpace(catalogueConnection) && !String.IsNullOrWhiteSpace(dataExportConnection))
                {
                    startup.RepositoryLocator = new LinkedRepositoryProvider(catalogueConnection, dataExportConnection);
                    startup.RepositoryLocator.CatalogueRepository.TestConnection();
                    startup.RepositoryLocator.DataExportRepository.TestConnection();
                }
                var startupUI = new StartupUIMainForm(startup);
                startupUI.ShowDialog();

                _repositoryLocator = startup.RepositoryLocator;
                
                //launch the main application form T
                _mainForm = new T();
                _mainForm.RepositoryLocator = _repositoryLocator;
                Application.Run(_mainForm);
            }
            catch (Exception e)
            {
                DiagnosticsScreen.OfferLaunchingDiagnosticsScreenOrEnvironmentExit(_repositoryLocator, null, e);
                MessageBox.Show("Diagnostics has been closed, application will now exit");
            }
        }
    }
}
