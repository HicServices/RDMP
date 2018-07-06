using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueManager.CommandExecution;
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
            yield return new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataLoadRuns, new LogViewerFilter { Task = taskId });
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
