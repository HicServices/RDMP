namespace HIC.Logging
{
    /// <summary>
    /// Enum indicating one of the logging tables in the logging relational database
    /// </summary>
    public enum LoggingTables
    {
        None = 0,
        DataLoadTask,
        DataLoadRun,
        ProgressLog,
        FatalError,
        TableLoadRun,
        DataSource,
        
    }
}