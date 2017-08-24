using System.Collections.Generic;

namespace CatalogueLibrary.Data.DataLoad
{
    public interface ITableInfoCollectionHost
    {
        IEnumerable<TableInfo> GetTableInfos();
    }
}