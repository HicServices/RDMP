namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface ISupplementalExtractionResults : IExtractionResults
    {
        int? CumulativeExtractionResults_ID { get; }
        int? ExtractionConfiguration_ID { get; }

        bool IsGlobal { get; }
        void CompleteAudit(string destinationDescription, int uniqueIdentifiers);
    }
}