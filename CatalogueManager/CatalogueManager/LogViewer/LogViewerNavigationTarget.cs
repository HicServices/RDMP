namespace CatalogueManager.LogViewer
{
    /// <summary>
    /// Enum indicating which <see cref="CatalogueManager.LogViewer.Tabs.LoggingTab"/> is being talked about (being navigated to, shown etc).
    /// </summary>
    public enum LogViewerNavigationTarget
    {
        DataLoadTasks,
        DataLoadRuns,
        ProgressMessages,
        FatalErrors,
        TableLoadRuns,
        DataSources
    }
}