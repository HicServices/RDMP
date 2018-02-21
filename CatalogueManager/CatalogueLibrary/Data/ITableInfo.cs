using System;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A persistent reference to an existing Database Table (See TableInfo).
    /// </summary>
    public interface ITableInfo : IComparable, IHasRuntimeName, IDataAccessPoint, IHasDependencies, ICollectSqlParameters, IMapsDirectlyToDatabaseTable
    {
        string GetRuntimeName(LoadBubble bubble, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);
        string GetRuntimeName(LoadStage stage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);
    }
}