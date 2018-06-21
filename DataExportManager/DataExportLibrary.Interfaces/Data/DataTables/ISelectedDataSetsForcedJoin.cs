using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See SelectedDataSetsForcedJoin
    /// </summary>
    public interface ISelectedDataSetsForcedJoin:IMapsDirectlyToDatabaseTable
    {
        int TableInfo_ID { get; }
        TableInfo TableInfo { get;}
    }
}