using System;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories
{
    
    public interface IRDMPPlatformRepositoryServiceLocator: ICatalogueRepositoryServiceLocator,IDataExportRepositoryServiceLocator
    {
        IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName, string databaseObjectTypeName,int objectID);
    }
}