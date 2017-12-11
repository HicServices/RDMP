using System;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See ExternalDatabaseServer
    /// </summary>
    public interface IExternalDatabaseServer : IDataAccessPoint
    {
        int ID { get; }
        string Name { get; }

        bool IsSameDatabase(string server, string database);
        bool RespondsWithinTime(int timeoutInSeconds, DataAccessContext context,out Exception exception);
    }
}