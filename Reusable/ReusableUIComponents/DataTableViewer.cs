using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents.Dialogs;


namespace ReusableUIComponents
{
    /// <summary>
    /// Allows you to view the results of a query sent by the RDMP.  This is a reusable component.
    /// </summary>
    public partial class DataTableViewer : UserControl
    {
        public DataTableViewer(DataTable source, string caption)
        {
            InitializeComponent();
            
            this.Text = caption;
            dataGridView1.DataSource = source;
        }

        public DataTableViewer(IDataAccessPoint source, string sql, string caption)
        {
            InitializeComponent();

            try
            {
                using (DbConnection con = DataAccessPortal.GetInstance().ExpectServer(source, DataAccessContext.DataExport).GetConnection())
                {
                    con.Open();

                    var cmd = DatabaseCommandHelper.GetCommand(sql, con);
                    var da = DatabaseCommandHelper.GetDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
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
