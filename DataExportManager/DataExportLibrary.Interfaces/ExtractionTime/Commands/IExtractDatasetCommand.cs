using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;

namespace DataExportLibrary.Interfaces.ExtractionTime.Commands
{
    /// <summary>
    /// See ExtractDatasetCommand
    /// </summary>
    public interface IExtractDatasetCommand : IExtractCommand
    {
        ISelectedDataSets SelectedDataSets { get; }

        IExtractableCohort ExtractableCohort { get; set; }
        ICatalogue Catalogue { get; }
        IExtractionDirectory Directory { get; set; }
        IExtractableDatasetBundle DatasetBundle { get; }
        List<IColumn> ColumnsToExtract { get; set; }

        void GenerateQueryBuilder();
        ISqlQueryBuilder QueryBuilder { get; set; }

        ICumulativeExtractionResults CumulativeExtractionResults { get; }
        int TopX { get; set; }
    }
}