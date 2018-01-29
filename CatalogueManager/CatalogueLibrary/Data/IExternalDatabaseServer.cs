using System;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See ExternalDatabaseServer
    /// </summary>
    public interface IExternalDatabaseServer : IDataAccessPoint, IMapsDirectlyToDatabaseTable
    {
        string Name { get; }

        bool IsSameDatabase(string server, string database);
        bool RespondsWithinTime(int timeoutInSeconds, DataAccessContext context,out Exception exception);

        /// <summary>
        /// Determines whether the given database server was created by the specified .Database assembly e.g. (DataQualityEngine.Database.dll).  If it is then the 
        /// schema will match, database objects will be retrievable through the host assembly (e.g. DataQualityEngine.dll) etc.
        /// </summary>
        /// <param name="databaseAssembly"></param>
        /// <returns></returns>
        bool WasCreatedByDatabaseAssembly(Assembly databaseAssembly);
    }
}