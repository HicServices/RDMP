using System;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// A class which can find the location (connection strings) of the of Catalogue and Data Export databases.  This might come from a user settings file or from a 
    /// config file or whatever (depending on how you implement this interface).
    /// </summary>
    public interface IRDMPPlatformRepositoryServiceLocator: ICatalogueRepositoryServiceLocator,IDataExportRepositoryServiceLocator
    {
        /// <summary>
        /// Cross repository method equivallent to GetObjectByID mostly used in persistence recovery (when you startup RDMP after closing it down before).  It is better
        /// to use the specific repository methods on the CatalogueRepository / DataExportRepository.
        /// </summary>
        /// <param name="repositoryTypeName"></param>
        /// <param name="databaseObjectTypeName"></param>
        /// <param name="objectID"></param>
        /// <returns></returns>
        IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName, string databaseObjectTypeName,int objectID);
        bool ArbitraryDatabaseObjectExists(string repositoryTypeName, string databaseObjectTypeName, int objectID);
    }
}