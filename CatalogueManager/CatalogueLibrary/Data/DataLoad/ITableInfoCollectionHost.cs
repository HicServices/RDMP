using System.Collections.Generic;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface for defining that a given class is dependent or operates on one or more TableInfos.  This is used when you declare a [DemandsInitialization] property on
    /// a plugin component of type ITableInfo to determine which instances to offer at design time (i.e. only show TableInfos that are associated with the load you are
    /// editing).
    /// </summary>
    public interface ITableInfoCollectionHost
    {
        IEnumerable<TableInfo> GetTableInfos();
    }
}