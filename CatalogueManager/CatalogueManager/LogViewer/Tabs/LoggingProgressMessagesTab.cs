using System.Data;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// All messages generated during a run appear in this control.  This is the least structured table in logging and is most comparable with other simple logging methods
    /// </summary>
    public class LoggingProgressMessagesTab : LoggingTab
    {
        protected override DataTable FetchDataTable(LogManager lm)
        {
            return lm.ListProgressMessagesAsTable(null);
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