using System;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data
{
    public interface ITableInfo : IComparable, IHasRuntimeName, IDataAccessPoint, IHasDependencies, ICollectSqlParameters
    {
        string GetRuntimeName(LoadStage loadStage, INameDatabasesAndTablesDuringLoads tableNamingScheme = null);
        string GetRuntimeNameFor(INameDatabasesAndTablesDuringLoads namer, LoadBubble namingConvention);
    }
}