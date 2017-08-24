using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface IExtractableDataSet:IMapsDirectlyToDatabaseTable,IRevertable
    {
        int Catalogue_ID { get; set; }
        bool DisableExtraction { get; set; }
        ICatalogue Catalogue { get; }
    }
}