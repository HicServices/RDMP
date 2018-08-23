using System.Data;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// All messages generated during a run appear in this control.  This is the least structured table in logging and is most comparable with other simple logging methods
    /// </summary>
    public class LoggingProgressMessagesTab : LoggingTab
    {
        protected override LoggingTables GetTableEnum()
        {
            return LoggingTables.ProgressLog;
        }

        protected override void FetchDataTable()
        {
            LoadDataTable(LogManager.GetTable(LoggingTables.ProgressLog,IDFilter.Run,TopX));
        }
    }
}