// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Data.Common;
using System.Windows.Forms;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.UI.SimpleDialogs
{
    /// <summary>
    /// Allows you to view the results of a query sent by the RDMP.  This is a reusable component.
    /// </summary>
    public partial class DataTableViewerUI : UserControl
    {
        public DataTableViewerUI(DataTable source, string caption)
        {
            InitializeComponent();
            
            this.Text = caption;
            dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;
            dataGridView1.DataSource = source;
        }

        public DataTableViewerUI(IDataAccessPoint source, string sql, string caption)
        {
            InitializeComponent();

            try
            {
                using (DbConnection con = DataAccessPortal.GetInstance().ExpectServer(source, DataAccessContext.DataExport).GetConnection())
                {
                    con.Open();

                    using(var cmd = DatabaseCommandHelper.GetCommand(sql, con))
                        using (var da = DatabaseCommandHelper.GetDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dataGridView1.DataSource = dt;
                        }
                }
            }
            catch (Exception e)
            {
                ExceptionViewer.Show("Failed to connect to source " + source + " and execute SQL: "+Environment.NewLine + sql,e);
            }

            this.Text = caption;
        }

    }
}
