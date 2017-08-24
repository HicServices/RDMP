using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Dashboard.CatalogueSummary.LoadEvents
{
    /// <summary>
    /// Part of ViewInsertsAndUpdatesDialog (in the Updates tab), this control shows a sample of the updates that occurred as part of a data load.  The data load engine operates a 'newer
    /// data is better' policy when loading data such that if a record with the same primary key comes in the old values for the record are moved into the shadow archive table and the new
    /// values are used to update the dataset (See 'Shadow Archive Tables' in UserManual.docx.  This control lets you see the before and after values side by side.
    /// </summary>
    public partial class DiffDataTables : UserControl
    {
        private string DifferenceSymbol = "-|-";
        private string WhitespaceDifference = "(WHITESPACE DIFFERENCE!)";

        public DiffDataTables()
        {
            InitializeComponent();
        }

        public void PopulateWith(DataTable dt1, DataTable dt2)
        {
            DataTable dtResult = new DataTable();

            foreach (DataColumn col in dt1.Columns)
                dtResult.Columns.Add(col.ColumnName);
            
            if(dt1.Columns.Count != dt2.Columns.Count)
                throw new NotSupportedException("Exected DataTables to have the same number of columns");
         
            if(dt1.Rows.Count != dt2.Rows.Count)
                throw new NotSupportedException("Exected DataTables to have the same number of rows");

            for (int r = 0; r < dt1.Rows.Count; r++)
            {
                DataRow copyToRow = dtResult.Rows.Add();

                for (int c = 0; c < dt1.Columns.Count; c++)
                {
                    var val1 = dt1.Rows[r][c] != null ?dt1.Rows[r][c].ToString():"";
                    var val2 = dt2.Rows[r][c] != null ? dt2.Rows[r][c].ToString() : "";

                    if(val1.Equals(val2))
                        copyToRow[c] = val1;//regular difference
                    else
                    if (val1.Trim().Equals(val2.Trim()))//whitespace difference
                        copyToRow[c] = val1 + WhitespaceDifference;
                    else
                        copyToRow[c] = val1 + DifferenceSymbol + val2;
                }
            }
            
            dataGridView1.DataSource = dtResult;
        }

        public void HighlightDiffCells()
        {
            foreach (DataGridViewRow r in dataGridView1.Rows)
                foreach (DataGridViewCell c in r.Cells)
                    if (c.Value != null )
                        if(c.Value.ToString().Contains(DifferenceSymbol))
                            c.Style.BackColor = Color.PaleGreen;
                        else if (c.Value.ToString().Contains(WhitespaceDifference))
                            c.Style.BackColor = Color.Red;

        }

        public void Clear()
        {
            dataGridView1.DataSource = null;
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
                dataGridView1.ClearSelection();
        }
    }
}
