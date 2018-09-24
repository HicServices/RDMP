using System;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See ExternalDatabaseServer
    /// </summary>
    public interface IExternalDatabaseServer : IDataAccessPoint, INamed
    {
        /// <summary>
        /// Determines whether the given database server was created by the specified .Database assembly e.g. (DataQualityEngine.Database.dll).  If it is then the 
        /// schema will match, database objects will be retrievable through the host assembly (e.g. DataQualityEngine.dll) etc.
        /// </summary>
        /// <param name="databaseAssembly"></param>
        /// <returns></returns>
        bool WasCreatedByDatabaseAssembly(Assembly databaseAssembly);


        /// <summary>
        /// Provides a live object for interacting directly with the server referenced by this <see cref="IExternalDatabaseServer"/>.  This will wokr
        /// even if the server is unreachable (See <see cref="IMightNotExist"/>)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        DiscoveredDatabase Discover(DataAccessContext context);
    }
}