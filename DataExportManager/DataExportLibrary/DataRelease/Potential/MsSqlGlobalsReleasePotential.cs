using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace DataExportLibrary.DataRelease.Potential
{
    public class MsSqlGlobalsReleasePotential : GlobalReleasePotential
    {
        public MsSqlGlobalsReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
            : base(repositoryLocator, globalResult, globalToCheck)
        {
        }

        protected override void CheckDestination(ICheckNotifier notifier, ISupplementalExtractionResults globalResult)
        {
            var externalServerId = int.Parse(globalResult.DestinationDescription.Split('|')[0]);
            var externalServer = RepositoryLocator.CatalogueRepository.GetObjectByID<ExternalDatabaseServer>(externalServerId);
            var dbName = globalResult.DestinationDescription.Split('|')[1];
            var tblName = globalResult.DestinationDescription.Split('|')[2];
            var server = DataAccessPortal.GetInstance().ExpectServer(externalServer, DataAccessContext.DataExport, setInitialDatabase: false);
            var database = server.ExpectDatabase(dbName);
            if (!database.Exists())
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Database: " + database + " was not found", CheckResult.Fail));
                Releasability = Releaseability.ExtractFilesMissing;
            }
            var foundTable = database.ExpectTable(tblName);
            if (!foundTable.Exists())
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Table: " + foundTable + " was not found", CheckResult.Fail));
                Releasability = Releaseability.ExtractFilesMissing;
            }
        }
    }
}