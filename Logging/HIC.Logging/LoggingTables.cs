namespace HIC.Logging
{
    /// <summary>
    /// Enum indicating which <see cref="CatalogueManager.LogViewer.Tabs.LoggingTab"/> is being talked about (being navigated to, shown etc).
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