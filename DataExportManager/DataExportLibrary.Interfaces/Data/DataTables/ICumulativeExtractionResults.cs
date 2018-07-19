using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See CumulativeExtractionResults
    /// </summary>
    public interface ICumulativeExtractionResults : IExtractionResults, IRevertable
    {
        int ExtractionConfiguration_ID { get; set; }
        int ExtractableDataSet_ID { get; }

        DateTime DateOfExtraction { get; }
        int DistinctReleaseIdentifiersEncountered { get; set; }
        string FiltersUsed { get; set; }
        int CohortExtracted { get; }
        IExtractableDataSet ExtractableDataSet { get; }

        IReleaseLogEntry GetReleaseLogEntryIfAny();
        void CompleteAudit(Type destinationType, string destinationDescription, int recordsExtracted);

        List<ISupplementalExtractionResults> SupplementalExtractionResults { get; }
        ISupplementalExtractionResults AddSupplementalExtractionResult(string sqlExecuted, IMapsDirectlyToDatabaseTable extractedObject);
        bool IsFor(ISelectedDataSets selectedDataSet);
    }
}