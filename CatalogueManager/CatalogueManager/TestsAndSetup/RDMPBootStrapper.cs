using System;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.TestsAndSetup.StartupUI;
using HIC.Logging;
using RDMPStartup;
using ReusableUIComponents;


namespace CatalogueManager.TestsAndSetup
{
    public class RDMPBootStrapper<T> where T : RDMPForm, new()
    {
        public static IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }

        public static void Boostrap(string catalogueConnection, string dataExportConnection, bool requiresDataExportDatabaseToo)
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

                RepositoryLocator = startup.RepositoryLocator;
                
                //launch the main application form T
                var mainForm = new T();
                mainForm.RepositoryLocator = RepositoryLocator;

                InitializeAllAspects();

                Application.Run(mainForm);
            }
            catch (Exception e)
            {
                DiagnosticsScreen.OfferLaunchingDiagnosticsScreenOrEnvironmentExit(RepositoryLocator, null, e);
                MessageBox.Show("Diagnostics has been closed, application will now exit");
            }
        }

        private static void InitializeAllAspects()
        {
            LoggingAspect.DoInit = () => RepositoryLocator.CatalogueRepository.GetDefaultLogManager();
        }
    }
}
