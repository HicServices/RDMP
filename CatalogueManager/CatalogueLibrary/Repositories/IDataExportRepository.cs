using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// See DataExportRepository
    /// </summary>
    public interface IDataExportRepository : ITableRepository
    {
        CatalogueRepository CatalogueRepository { get; }
        
    }
}
