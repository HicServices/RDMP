using System;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See SupplementalExtractionResults
    /// </summary>
    public interface ISupplementalExtractionResults : IExtractionResults
    {
        int? CumulativeExtractionResults_ID { get; }
        int? ExtractionConfiguration_ID { get; }

        bool IsGlobal { get; }
        
        string ExtractedType { get; set; }
        int ExtractedId { get; set; }
        string ExtractedName { get; }
        string RepositoryType { get; }

        void CompleteAudit(string destinationDescription, int uniqueIdentifiers);
        Type GetExtractedType();
    }
}