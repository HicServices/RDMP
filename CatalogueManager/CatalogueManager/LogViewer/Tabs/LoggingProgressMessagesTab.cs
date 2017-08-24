using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// All messages generated during a run appear in this control.  This is the least structured table in logging and is most comparable with other simple logging methods
    /// </summary>
    public class LoggingProgressMessagesTab : LoggingTab
    {
        public event NavigatePaneToEntityHandler NavigationPaneGoto;

        public LoggingProgressMessagesTab()
        {
            base.InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            NavigationPaneGoto(this, new NavigatePaneToEntityArgs(LogViewerNavigationTarget.ProgressMessages, (int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value));
        }

        public void SetStateTo(LogManager logManager, LogViewerFilterCollection filter)
        {
            _filters = filter;

            if (!_bLoaded)
            {
                var dt = logManager.ListProgressMessagesAsTable(null);
                base.LoadDataTable(dt);    
            }

            if (filter.Run == null)
                SetFilter(null);
            else
                SetFilter("dataLoadRunID=" + filter.Run);
        }
    }
}