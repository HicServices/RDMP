using System;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface ICumulativeExtractionResults : IDeleteable, IRevertable
    {
        int ExtractionConfiguration_ID { get; set; }
        int ExtractableDataSet_ID { get; set; }

        DateTime DateOfExtraction { get; set; }
        string Filename { get; set; }
        int RecordsExtracted { get; set; }
        int DistinctReleaseIdentifiersEncountered { get; set; }
        string FiltersUsed { get; set; }
        string Exception { get; set; }
        string SQLExecuted { get; set; }
        int CohortExtracted { get; set; }
        IExtractableDataSet ExtractableDataSet { get;}
        IReleaseLogEntry GetReleaseLogEntryIfAny();
    }
}