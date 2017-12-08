using System;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A persistent reference to an existing Database Table (See TableInfo).
    /// </summary>
    public interface ITableInfo : IComparable, IHasRuntimeName, IDataAccessPoint, IHasDependencies, ICollectSqlParameters
    {
        string GetRuntimeName(LoadStage loadStage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);
        string GetRuntimeNameFor(INameDatabasesAndTablesDuringLoads namer, LoadBubble namingConvention);
    }
}