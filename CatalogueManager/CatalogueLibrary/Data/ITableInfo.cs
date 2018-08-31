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
        /// <summary>
        /// Gets the name of the table in the given RAW=>STAGING=>LIVE section of a DLE run using the provided <see cref="tableNamingScheme"/>
        /// </summary>
        /// <param name="bubble"></param>
        /// <param name="tableNamingScheme"></param>
        /// <returns></returns>
        string GetRuntimeName(LoadBubble bubble, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);

        /// <inheritdoc cref="GetRuntimeName(LoadBubble,INameDatabasesAndTablesDuringLoads)"/>
        string GetRuntimeName(LoadStage stage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);
    }
}