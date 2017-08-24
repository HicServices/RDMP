using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface ISelectedDataSets:IDeleteable,IRevertable
    {
        int ExtractionConfiguration_ID { get; set; }
        int ExtractableDataSet_ID { get; set; }
        int? RootFilterContainer_ID { get; set; }
    }
}