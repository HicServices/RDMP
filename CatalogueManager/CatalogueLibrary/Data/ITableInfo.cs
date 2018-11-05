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
    public interface ITableInfo : IComparable, IHasRuntimeName, IDataAccessPoint, IHasDependencies,
        ICollectSqlParameters, IMapsDirectlyToDatabaseTable
    {
        /// <summary>
        /// Gets the name of the table in the given RAW=>STAGING=>LIVE section of a DLE run using the provided <paramref name="tableNamingScheme"/>
        /// </summary>
        /// <param name="bubble"></param>
        /// <param name="tableNamingScheme"></param>
        /// <returns></returns>
        string GetRuntimeName(LoadBubble bubble, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);

        /// <inheritdoc cref="GetRuntimeName(LoadBubble,INameDatabasesAndTablesDuringLoads)"/>
        string GetRuntimeName(LoadStage stage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);

        /// <summary>
        /// Fetches all the ColumnInfos associated with this TableInfo (This is refreshed every time you call this property)
        /// </summary>
        [NoMappingToDatabase]
        ColumnInfo[] ColumnInfos { get; }

        /// <summary>
        /// Gets all the <see cref="PreLoadDiscardedColumn"/> declared against this table reference.  These are virtual columns which 
        /// do not exist in the LIVE table schema (Unless <see cref="DiscardedColumnDestination.Dilute"/>) but which appear in the RAW 
        /// stage of the data load.  
        /// 
        /// <para>See <see cref="PreLoadDiscardedColumn"/> for more information</para>
        /// </summary>
        [NoMappingToDatabase]
        PreLoadDiscardedColumn[] PreLoadDiscardedColumns { get; }
    }
}