using System.Data;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// Shows all the sources for data moved during a data run.  This includes the location of files that were loaded (they are likely not still there though what with archiving).  In the
    /// case of Data Extraction the DataSource is the SQL query that was used to perform the data extraction.
    /// </summary>
    public class LoggingDataSourcesTab : LoggingTab
    {
        protected override DataTable FetchDataTable(LogManager lm)
        {
            return lm.ListDataSourcesAsTable(null);
        }

        public override void SetFilter(LogViewerFilter filter)
        {
            base.SetFilter(filter);

            if (filter.Table == null)
                SetFilter("");
            else
                SetFilter("tableLoadRunID=" + filter.Table);
        }
    }
}