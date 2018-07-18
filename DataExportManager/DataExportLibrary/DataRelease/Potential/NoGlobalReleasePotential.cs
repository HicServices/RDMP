using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.DataRelease.Potential
{
    public class NoGlobalReleasePotential : GlobalReleasePotential
    {
        public NoGlobalReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
            : base(repositoryLocator, globalResult, globalToCheck)
        {
        }

        public override void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(RelatedGlobal + " is " + Releaseability.NeverBeenSuccessfullyExecuted, CheckResult.Fail));
            Releasability = Releaseability.NeverBeenSuccessfullyExecuted;
        }

        protected override void CheckDestination(ICheckNotifier notifier, ISupplementalExtractionResults globalResult)
        {
            Releasability = Releaseability.NeverBeenSuccessfullyExecuted;
        }
    }
}