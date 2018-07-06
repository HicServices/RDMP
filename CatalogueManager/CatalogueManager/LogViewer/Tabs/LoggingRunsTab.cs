using System;
using System.Data;
using System.Windows.Forms;
using CatalogueManager.CommandExecution;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// Records each separate execution of a given Task for example one Data Extraction for Project 2301 (of datasets Biochemistry, Prescribing and Haematology) including when it started 
    /// and ended and who ran the extraction.
    /// </summary>
    public class LoggingRunsTab : LoggingTab
    {
        public LoggingRunsTab()
        {
            base.InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            
            var taskId = (int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
            var cmd = new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.ProgressMessages, new LogViewerFilter { Run = taskId });
            cmd.Execute();
        }

        protected override DataTable FetchDataTable(LogManager lm)
        {
            return lm.ListDataLoadRunsAsTable(null);
        }

        public override void SetFilter(LogViewerFilter filter)
        {
            base.SetFilter(filter);

            string f = null;

            if (filter.Task != null)
                f = "dataLoadTaskID=" + filter.Task;

            if (filter.Run != null)
                if(string.IsNullOrEmpty(f))
                    f = " ID=" + filter.Run;
                else
                    f += " AND ID=" + filter.Run;

            SetFilter(f);
        }

    }
}
