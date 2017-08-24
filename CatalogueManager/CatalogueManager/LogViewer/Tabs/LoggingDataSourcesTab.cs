using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// Shows all the sources for data moved during a data run.  This includes the location of files that were loaded (they are likely not still there though what with archiving).  In the
    /// case of Data Extraction the DataSource is the SQL query that was used to perform the data extraction.
    /// </summary>
    public class LoggingDataSourcesTab : LoggingTab
    {

        public event NavigatePaneToEntityHandler NavigationPaneGoto;

        public LoggingDataSourcesTab()
        {
            base.InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            NavigationPaneGoto(this, new NavigatePaneToEntityArgs(LogViewerNavigationTarget.DataSources, (int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value));
        }

        public void SetStateTo(LogManager logManager, LogViewerFilterCollection filter)
        {
            _filters = filter;
            
            if (!_bLoaded)
            {
                var dt = logManager.ListDataSourcesAsTable(null);
                base.LoadDataTable(dt);
            }

            if (filter.Table == null)
                SetFilter(null);
            else
                SetFilter("tableLoadRunID=" + filter.Table);
        }
    }
}