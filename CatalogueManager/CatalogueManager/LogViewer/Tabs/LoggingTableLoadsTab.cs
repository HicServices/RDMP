using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public event NavigatePaneToEntityHandler NavigationPaneGoto;

        public LoggingTableLoadsTab()
        {
            base.InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }
        void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            NavigationPaneGoto(this,new NavigatePaneToEntityArgs(LogViewerNavigationTarget.TableLoadRuns,(int) dataGridView1.Rows[e.RowIndex].Cells["ID"].Value));
        }
        public void SetStateTo(LogManager lm,LogViewerFilterCollection filters)
        {
            _filters = filters;
            if (!_bLoaded)
            {
                var dt = lm.ListTableLoadsAsTable(null);
                base.LoadDataTable(dt);    
            }

            string f = null;
            
            if (filters.Run != null)
                f = "dataLoadRunID=" + filters.Run;

            if (_filters.Table != null)
                if (string.IsNullOrEmpty(f))
                    f = " ID=" + _filters.Table;
                else
                    f += " AND ID=" + _filters.Table;

             SetFilter(f);
        }

    }
}
