namespace DataExportLibrary.Interfaces.ExtractionTime.Commands
{
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