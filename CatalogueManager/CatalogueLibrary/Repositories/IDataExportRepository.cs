using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories
{
    public interface IDataExportRepository : ITableRepository
    {
        CatalogueRepository CatalogueRepository { get; }
        
    }
}
