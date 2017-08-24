using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode.Checks;

namespace ReusableLibraryCode
{
    public class SMOIndexRebuilder
    {
        public SMOIndexRebuilder()
        {
            
        }

        public void RebuildIndexes(SqlConnection con,ICheckNotifier notifier)
        {
            try
            {
                Server server = new Server(new ServerConnection(con));

                string databaseName = server.ConnectionContext.DatabaseName;

                notifier.OnCheckPerformed(new CheckEventArgs("Found default database " + databaseName, CheckResult.Success));

                foreach (Table tbl in server.Databases[databaseName].Tables)
                    foreach (Index idx in tbl.Indexes)
                    {
                        var dt = idx.EnumFragmentation(FragmentationOption.Fast);

                        var str = DescribeFragmentation(dt);

                        notifier.OnCheckPerformed(new CheckEventArgs(string.Format("Found Index {0} ",str), CheckResult.Success));

                        idx.Rebuild();
                        notifier.OnCheckPerformed(new CheckEventArgs("Index Rebuilt successfully", CheckResult.Success));
                    }

                notifier.OnCheckPerformed(new CheckEventArgs("All indexes rebuild ", CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to rebuild indexes ", CheckResult.Fail, e));
            }


        }

        private string DescribeFragmentation(DataTable dt)
        {
            HashSet<DataColumn> colsWithDataIn = new HashSet<DataColumn>();


            foreach (DataColumn col in dt.Columns)
                foreach (DataRow dr in dt.Rows)
                {
                    object value = dr[col];
                    if (value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        colsWithDataIn.Add(col);
                        break;
                    }
                }

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Join(",", colsWithDataIn.Select(c => c.ColumnName)));
            sb.Append(":");
            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn dataColumn in colsWithDataIn)
                    sb.Append(dr[dataColumn] + ",");

                sb.Append("|");
            }
            
            return sb.ToString();
        }
    }
}
