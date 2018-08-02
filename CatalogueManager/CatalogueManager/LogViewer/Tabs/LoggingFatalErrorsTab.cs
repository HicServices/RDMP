using System.Data;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// A view of all the exceptions and failure messages captured during a run.  If there are any of these then the run can be assumed to have failed.
    /// </summary>
    public class LoggingFatalErrorsTab : LoggingTab
    {
        protected override LoggingTables GetTableEnum()
        {
            return LoggingTables.FatalError;
        }

        protected override void FetchDataTable()
        {
            LoadDataTable(LogManager.GetTable(LoggingTables.FatalError, IDFilter.Run,TopX));
        }
    }
}