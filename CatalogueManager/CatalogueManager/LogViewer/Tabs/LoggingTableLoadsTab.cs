using System.Collections.Generic;
using System.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// During a run all destinations for data will appear in this control, for data loading this includes STAGING load and LIVE load (with 1 record per table being loaded).  It includes 
    /// counts for the number of INSERTS / UPDATES and DELETES carried out.  In the case of Data Extraction the record will refer to a 'table' as the .csv file given to the researcher
    /// (unless you are extracting into a researchers database of course).
    /// </summary>
    public class LoggingTableLoadsTab:LoggingTab
    {
        protected override IEnumerable<ExecuteCommandViewLoggedData> GetCommands(int rowIdnex)
        {
            var tableId = (int)dataGridView1.Rows[rowIdnex].Cells["ID"].Value;
            yield return new ExecuteCommandViewLoggedData(_activator, LoggingTables.DataSource, new LogViewerFilter { Table = tableId });
        }

        protected override LoggingTables GetTableEnum()
        {
            return LoggingTables.TableLoadRun;
        }
    }
}
