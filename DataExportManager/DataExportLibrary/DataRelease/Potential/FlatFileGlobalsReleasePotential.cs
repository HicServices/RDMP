using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.DataRelease.Potential
{
    public class FlatFileGlobalsReleasePotential : GlobalReleasePotential
    {
        public FlatFileGlobalsReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
            : base(repositoryLocator, globalResult, globalToCheck)
        {
        }

        protected override void CheckDestination(ICheckNotifier notifier, ISupplementalExtractionResults globalResult)
        {
            CheckFileExists(notifier, globalResult.DestinationDescription);
        }
    }
}