// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Rdmp.UI.CatalogueSummary.LoadEvents;

/// <summary>
/// Part of ViewInsertsAndUpdatesDialog (in the Updates tab), this control shows a sample of the updates that occurred as part of a data load.  The data load engine operates a 'newer
/// data is better' policy when loading data such that if a record with the same primary key comes in the old values for the record are moved into the archive table and the new
/// values are used to update the dataset (See 'Archive Tables' in UserManual.md.  This control lets you see the before and after values side by side.
/// </summary>
public partial class DiffDataTables : UserControl
{
    private const string DifferenceSymbol = "-|-";
    private const string WhitespaceDifference = "(WHITESPACE DIFFERENCE!)";

    public DiffDataTables()
    {
        InitializeComponent();

        dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;
    }

    public void PopulateWith(DataTable dt1, DataTable dt2)
    {
        var dtResult = new DataTable();

        foreach (DataColumn col in dt1.Columns)
            dtResult.Columns.Add(col.ColumnName);
            
        if(dt1.Columns.Count != dt2.Columns.Count)
            throw new NotSupportedException("Expected DataTables to have the same number of columns");
         
        if(dt1.Rows.Count != dt2.Rows.Count)
            throw new NotSupportedException("Expected DataTables to have the same number of rows");

        for (var r = 0; r < dt1.Rows.Count; r++)
        {
            var copyToRow = dtResult.Rows.Add();

            for (var c = 0; c < dt1.Columns.Count; c++)
            {
                var val1 = dt1.Rows[r][c] != null ? dt1.Rows[r][c].ToString() : "";
                var val2 = dt2.Rows[r][c] != null ? dt2.Rows[r][c].ToString() : "";

                if(val1.Equals(val2))
                    copyToRow[c] = val1;//regular difference
                else
                if (val1.Trim().Equals(val2.Trim()))//whitespace difference
                    copyToRow[c] = $"{val1}{WhitespaceDifference}";
                else
                    copyToRow[c] = $"{val1}{DifferenceSymbol}{val2}";
            }
        }

        dataGridView1.DataSource = dtResult;
    }

    public void HighlightDiffCells()
    {
        foreach (DataGridViewRow r in dataGridView1.Rows)
        foreach (DataGridViewCell c in r.Cells)
            if (c.Value != null)
                if (c.Value.ToString().Contains(DifferenceSymbol))
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
        if (e.KeyCode == Keys.Escape)
            dataGridView1.ClearSelection();
    }
}