using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// A view of all the exceptions and failure messages captured during a run.  If there are any of these then the run can be assumed to have failed.
    /// </summary>
    public class LoggingFatalErrorsTab : LoggingTab
    {
        public event NavigatePaneToEntityHandler NavigationPaneGoto;

        public LoggingFatalErrorsTab()
        {
            base.InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            NavigationPaneGoto(this, new NavigatePaneToEntityArgs(LogViewerNavigationTarget.FatalErrors, (int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value));
        }

        public void SetStateTo(LogManager logManager, LogViewerFilterCollection filter)
        {
            _filters = filter;

            if (!_bLoaded)
            {
                var dt = logManager.ListFatalErrorsAsDataTable(null);
                base.LoadDataTable(dt);
            }

            if (filter.Run == null)
                SetFilter(null);
            else
                SetFilter("dataLoadRunID=" + filter.Run);
        }
    }
}