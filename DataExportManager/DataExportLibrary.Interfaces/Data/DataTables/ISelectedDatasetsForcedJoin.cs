using CatalogueLibrary.Data;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface ISelectedDatasetsForcedJoin
    {
        int TableInfo_ID { get; }
        TableInfo TableInfo { get;}
    }
}