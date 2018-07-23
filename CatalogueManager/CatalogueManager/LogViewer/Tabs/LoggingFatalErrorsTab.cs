using System.Data;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// A view of all the exceptions and failure messages captured during a run.  If there are any of these then the run can be assumed to have failed.
    /// </summary>
    public class LoggingFatalErrorsTab : LoggingTab
    {
        protected override DataTable FetchDataTable(LogManager lm)
        {
            return lm.ListFatalErrorsAsDataTable(null);
        }

        public override void SetFilter(LogViewerFilter filter)
        {
            base.SetFilter(filter);

            if (filter.Run == null)
                SetFilter("");
            else
                SetFilter("dataLoadRunID=" + filter.Run);
        }
    }
}