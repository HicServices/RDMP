using System;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;

namespace DatabaseCreation
{
    /// <summary>
    /// IRDMPPlatformRepositoryServiceLocator which identifies the location of Catalogue and Data Export databases during the runtime of DatabaseCreation.exe
    /// 
    /// Since these connection strings are part of the command line arguments to DatabaseCreation.exe it's a pretty simple class!
    /// </summary>
    public class DatabaseCreationRepositoryFinder : IRDMPPlatformRepositoryServiceLocator
    {
        private readonly LinkedRepositoryProvider _linkedRepositoryProvider;

        public CatalogueRepository CatalogueRepository
        {
            get { return _linkedRepositoryProvider.CatalogueRepository; }
        }

        public IDataExportRepository DataExportRepository
        {
            get { return _linkedRepositoryProvider.DataExportRepository; }
        }

        public IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName, string databaseObjectTypeName, int objectID)
        {
            throw new NotImplementedException();
        }

        public DatabaseCreationRepositoryFinder(string servername,string prefix)
        {
            var cata = DatabaseCreationProgram.GetBuilder(servername, prefix,DatabaseCreationProgram.DefaultCatalogueDatabaseName);
            var export = DatabaseCreationProgram.GetBuilder(servername,prefix, DatabaseCreationProgram.DefaultDataExportDatabaseName);

            Exception exCatalogue;
            Exception exDataExport;

            _linkedRepositoryProvider = new LinkedRepositoryProvider(cata.ConnectionString, export.ConnectionString, out exCatalogue, out exDataExport);

            if(exCatalogue != null)
                throw new Exception("There was a problem with the Catalogue connection string",exCatalogue);

            if (exDataExport != null)
                throw new Exception("There was a problem with the Data Export connection string", exDataExport);
        }
    }
}