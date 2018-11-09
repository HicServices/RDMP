using System;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using Diagnostics;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup
{
    internal enum ConnectionTypes
    {
        Logging,
        ANO,
        IdentifierDump,
        Master
    }

    /// <summary>
    /// This super technical  screen appears when things go irrecoverably wrong or when the user launches it from the CatalogueManager main menu.  It includes a variety of tests that 
    /// can be run on the current platform databases to check the contents/configurations.  It also includes 'Create Test Dataset' which can be used as a user acceptance test to make
    /// sure that their installation is successfully working (See UserManual.docx)
    /// 
    /// <para>The 'Evaluate MEF Exports' button is particularly useful if you are trying to debug a plugin you have written to find out why your class isn't appearing in the relevant part of
    /// the RDMP.</para>
    /// </summary>
    public partial class DiagnosticsScreen : RDMPForm
    {
        private readonly Exception _originalException;

        public bool ShouldReloadCatalogues = false;
        private bool setupComplete = false;

        public DiagnosticsScreen(Exception exception)
        {
            _originalException = exception;

            InitializeComponent();
            
            if (_originalException != null)
                btnViewOriginalException.Enabled = true;
            
        }

        protected override void OnRepositoryLocatorAvailable()
        {
            base.OnRepositoryLocatorAvailable();

            if (RepositoryLocator.DataExportRepository == null)
            {
                btnDataExportManagerFields.Enabled = false;
            }
        }

        public static void OfferLaunchingDiagnosticsScreenOrEnvironmentExit(IRDMPPlatformRepositoryServiceLocator locator, IWin32Window owner, Exception e)
        {
            if (MessageBox.Show(ExceptionHelper.ExceptionToListOfInnerMessages(e) + Environment.NewLine + Environment.NewLine + " Would you like to launch the Diagnostics Screen? (If you choose No then the application will exit ᕦ(ò_óˇ)ᕤ )", "Launch Diagnostics?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DiagnosticsScreen screen = new DiagnosticsScreen(e);
                screen.RepositoryLocator = locator;
                screen.ShowDialog(owner);
            }
            else
                Environment.Exit(499);
        }

        private void btnCatalogueFields_Click(object sender, EventArgs e)
        {

            StartChecking(new MissingFieldsChecker(MissingFieldsChecker.ThingToCheck.Catalogue, RepositoryLocator.CatalogueRepository, RepositoryLocator.DataExportRepository));
        }
        private void btnDataExportManagerFields_Click(object sender, EventArgs e)
        {
            StartChecking(new MissingFieldsChecker(MissingFieldsChecker.ThingToCheck.DataExportManager, RepositoryLocator.CatalogueRepository, RepositoryLocator.DataExportRepository));
        }

        private void btnCheckANOConfigurations_Click(object sender, EventArgs e)
        {
            StartChecking(new ANOConfigurationChecker(RepositoryLocator.CatalogueRepository));
        }
        private void btnCohortDatabase_Click(object sender, EventArgs e)
        {
            StartChecking(new CohortConfigurationChecker(RepositoryLocator.DataExportRepository));
        }
        private void btnListBadAssemblies_Click(object sender, EventArgs e)
        {
            StartChecking(new BadAssembliesChecker(RepositoryLocator.CatalogueRepository.MEF));
        }

        private void btnCatalogueTableNames_Click(object sender, EventArgs e)
        {
            StartChecking(new DodgyNamedTableAndColumnsChecker(RepositoryLocator.CatalogueRepository));
        }

        private void StartChecking(ICheckable checkable)
        {
            checksUI1.Clear();
            checksUI1.StartChecking(checkable);
        }
        
        private void btnViewOriginalException_Click(object sender, EventArgs e)
        {
            ExceptionViewer.Show(_originalException);
        }
        
        private void btnCatalogueCheck_Click(object sender, EventArgs e)
        {
            checksUI1.Clear();
            try
            {
                foreach (Catalogue c in RepositoryLocator.CatalogueRepository.GetAllCatalogues(true))
                    c.Check(checksUI1);
            }
            catch (Exception exception)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Catalogue checking crashed completely", CheckResult.Fail,exception));
            }
        }
    }
}
