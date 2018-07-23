using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.DataRelease.Potential
{
    /// <summary>
    /// Release Potential class to be used when nothing has ever been extracted
    /// </summary>
    public class NoReleasePotential : ReleasePotential
    {
        public NoReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISelectedDataSets selectedDataSet): base(repositoryLocator, selectedDataSet)
        {
        }

        protected override Releaseability GetSupplementalSpecificAssessment(IExtractionResults supplementalExtractionResults)
        {
            return Releaseability.NeverBeenSuccessfullyExecuted;
        }

        protected override Releaseability GetSpecificAssessment(IExtractionResults extractionResults)
        {
            return Releaseability.NeverBeenSuccessfullyExecuted;
        }

        public override void Check(ICheckNotifier notifier)
        {
            base.Check(notifier);

            notifier.OnCheckPerformed(new CheckEventArgs(SelectedDataSet + " is " + Releaseability.NeverBeenSuccessfullyExecuted, CheckResult.Fail));
        }
    }
}