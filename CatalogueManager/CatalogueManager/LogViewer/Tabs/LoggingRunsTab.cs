using System;
using System.Windows.Forms;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// Records each separate execution of a given Task for example one Data Extraction for Project 2301 (of datasets Biochemistry, Prescribing and Haematology) including when it started 
    /// and ended and who ran the extraction.
    /// </summary>
    public class LoggingRunsTab : LoggingTab
    {
        public event NavigatePaneToEntityHandler NavigationPaneGoto;

        public LoggingRunsTab()
        {
            base.InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        void dataGridView1_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            NavigationPaneGoto(this, new NavigatePaneToEntityArgs(LogViewerNavigationTarget.DataLoadRuns, (int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value));
        }

        public void SetStateTo(LogManager lm, LogViewerFilterCollection filters)
        {
            _filters = filters;
            if (!_bLoaded)
            {
                var dt = lm.ListDataLoadRunsAsTable(null);
                LoadDataTable(dt);    
            }

            string f = null;
            
            if (_filters.Task != null)
                f = "dataLoadTaskID=" + filters.Task;

            if(_filters.Run != null)
                if(string.IsNullOrEmpty(f))
                    f = " ID=" + _filters.Run;
                else
                    f += " AND ID="+_filters.Run;

            SetFilter(f);
        }

    }
}
