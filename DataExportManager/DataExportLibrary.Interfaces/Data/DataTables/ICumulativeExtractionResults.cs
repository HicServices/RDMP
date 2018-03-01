using System;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See CumulativeExtractionResults
    /// </summary>
    public interface ICumulativeExtractionResults : IDeleteable, IRevertable
    {
        int ExtractionConfiguration_ID { get; set; }
        int ExtractableDataSet_ID { get; set; }

        DateTime DateOfExtraction { get; set; }
        string DestinationDescription { get; set; }
        DestinationType DestinationType { get; }
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