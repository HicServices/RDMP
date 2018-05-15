using System.Collections.Generic;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// See DataExportRepository
    /// </summary>
    public interface IDataExportRepository : ITableRepository
    {
        CatalogueRepository CatalogueRepository { get; }
        CatalogueExtractabilityStatus GetExtractabilityStatus(ICatalogue c);
    }
}
