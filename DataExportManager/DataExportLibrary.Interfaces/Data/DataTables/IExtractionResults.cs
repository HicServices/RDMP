using System;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface IExtractionResults : IMapsDirectlyToDatabaseTable, ISaveable
    {
        string DestinationDescription { get; }
        int RecordsExtracted { get; }
        string Exception { get; set; }
        string SQLExecuted { get; }
    }
}