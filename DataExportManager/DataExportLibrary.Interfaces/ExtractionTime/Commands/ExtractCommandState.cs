namespace DataExportLibrary.Interfaces.ExtractionTime.Commands
{
    /// <summary>
    /// The current state an IExtractCommand has reached in an Extraction Pipeline.  Datasets in an ExtractionConfiguration are typically extracted in parallel.
    /// </summary>
    public enum ExtractCommandState
    {
        NotLaunched,
        WaitingForSQLServer,
        WritingToFile,
        Crashed,
        UserAborted,
        Completed,
        Warning,
        WritingMetadata,
        WaitingToExecute
    }
}