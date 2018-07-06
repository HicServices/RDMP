using System.Data;
using System.Runtime.InteropServices;
using CatalogueManager.CommandExecution;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// High level categories of activities (like folders) e.g. Data Extraction, Loading Biochemistry etc.
    /// </summary>
    public class LoggingTasksTab : LoggingTab
    {
        public LoggingTasksTab()
        {
            base.InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            var taskId = (int) dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
            var cmd = new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataLoadRuns, new LogViewerFilter {Task = taskId});
            cmd.Execute();
        }

        protected override DataTable FetchDataTable(LogManager lm)
        {
            return lm.ListDataTasksAsTable();
        }

        public override void SetFilter(LogViewerFilter filter)
        {
            base.SetFilter(filter);
            
            if (filter.Task == null)
                SetFilter("");
            else
                SetFilter("ID=" + filter.Task);
        }

    }
}
