using System.Data.Common;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.DataAccess;

namespace DataExportLibrary.DataRelease
{
    public class MsSqlExtractionReleasePotential : ReleasePotential
    {
        public MsSqlExtractionReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ExtractionConfiguration configuration, ExtractableDataSet dataSet) : base(repositoryLocator, configuration, dataSet)
        {
            
        }

        protected override Releaseability GetSpecificAssessment()
        {
            var externalServerId = int.Parse(ExtractionResults.DestinationDescription.Split('|')[0]);
            var externalServer = _repositoryLocator.CatalogueRepository.GetObjectByID<ExternalDatabaseServer>(externalServerId);
            var tblName = ExtractionResults.DestinationDescription.Split('|')[1];
            var server = DataAccessPortal.GetInstance().ExpectServer(externalServer, DataAccessContext.DataExport);
            using (DbConnection con = server.GetConnection())
            {
                con.Open();
                var database = server.ExpectDatabase(externalServer.Database);
                if (!database.Exists())
                {
                    return Releaseability.ExtractFilesMissing;
                }

                var foundTable = database.ExpectTable(tblName);
                if (!foundTable.Exists())
                {
                    return Releaseability.ExtractFilesMissing;
                }
            }

            // TODO: Table can be polluted, how to check?? CHECK FOR SPURIOUS TABLES
            
            return Releaseability.Undefined;
        }
    }
}