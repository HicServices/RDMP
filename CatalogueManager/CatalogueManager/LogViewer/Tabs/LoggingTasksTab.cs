using System.Collections.Generic;
using System.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// High level categories of activities (like folders) e.g. Data Extraction, Loading Biochemistry etc.
    /// </summary>
    public class LoggingTasksTab : LoggingTab
    {
        protected override IEnumerable<ExecuteCommandViewLoggedData> GetCommands(int rowIndex)
        {
            var taskId = (int)dataGridView1.Rows[rowIndex].Cells["ID"].Value;
            yield return new ExecuteCommandViewLoggedData(_activator, LoggingTables.DataLoadRun, new LogViewerFilter { Task = taskId });
        }

        protected override LoggingTables GetTableEnum()
        {
            return LoggingTables.DataLoadTask;
        }
    }
}
