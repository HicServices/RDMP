using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// Records each separate execution of a given Task for example one Data Extraction for Project 2301 (of datasets Biochemistry, Prescribing and Haematology) including when it started 
    /// and ended and who ran the extraction.
    /// </summary>
    public class LoggingRunsTab : LoggingTab
    {
        protected override IEnumerable<ExecuteCommandViewLoggedData> GetCommands(int rowIdnex)
        {

            var taskId = (int)dataGridView1.Rows[rowIdnex].Cells["ID"].Value;
            yield return new ExecuteCommandViewLoggedData(_activator, LoggingTables.ProgressLog, new LogViewerFilter { Run = taskId });
            yield return new ExecuteCommandViewLoggedData(_activator, LoggingTables.FatalError, new LogViewerFilter { Run = taskId });
            yield return new ExecuteCommandViewLoggedData(_activator, LoggingTables.TableLoadRun, new LogViewerFilter { Run = taskId });
        }

        protected override LoggingTables GetTableEnum()
        {
            return LoggingTables.DataLoadRun;
        }

        protected override void FetchDataTable()
        {
            LoadDataTable(LogManager.GetTable(LoggingTables.DataLoadRun,IDFilter.Task,TopX));
        }
    }
}
