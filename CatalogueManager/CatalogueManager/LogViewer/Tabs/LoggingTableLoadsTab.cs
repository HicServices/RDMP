using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.CommandExecution;
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
            yield return new ExecuteCommandViewLoggedData(_activator, LogViewerNavigationTarget.DataSources, new LogViewerFilter { Table = tableId });
        }

        protected override DataTable FetchDataTable(LogManager lm)
        {
            return lm.ListTableLoadsAsTable(null);
        }

        public override void SetFilter(LogViewerFilter filter)
        {
            base.SetFilter(filter);

            string f = null;

            if (filter.Run != null)
                f = "dataLoadRunID=" + filter.Run;

            if (filter.Table != null)
                if (string.IsNullOrEmpty(f))
                    f = " ID=" + filter.Table;
                else
                    f += " AND ID=" + filter.Table;

             SetFilter(f);
        }

    }
}
