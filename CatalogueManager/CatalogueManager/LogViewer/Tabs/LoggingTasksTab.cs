using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// High level categories of activities (like folders) e.g. Data Extraction, Loading Biochemistry etc.
    /// </summary>
    public class LoggingTasksTab : LoggingTab
    {
        public event NavigatePaneToEntityHandler NavigationPaneGoto;

        public LoggingTasksTab()
        {
            base.InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            NavigationPaneGoto(this, new NavigatePaneToEntityArgs(LogViewerNavigationTarget.DataLoadTasks, (int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value));
        }



        /// <summary>
        /// Should only ever be called once! sets the initial tasks that are known about
        /// </summary>
        /// <param name="lm"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public Dictionary<string, int> FetchDataTasks(LogManager lm, LogViewerFilterCollection filters)
        {
            _filters = filters;
            var dt = lm.ListDataTasksAsTable();

            Dictionary<string, int> toReturn = new Dictionary<string, int>();

            foreach (DataRow row in dt.Rows)
                toReturn.Add((string) row["name"], (int) row["ID"]);

            base.LoadDataTable(dt);

            return toReturn;
        }


        public void SetStateTo(LogManager lm, LogViewerFilterCollection filters)
        {
            if (filters.Task == null)
                SetFilter(null);
            else
                SetFilter("ID=" + filters.Task);
        }

    }
}
