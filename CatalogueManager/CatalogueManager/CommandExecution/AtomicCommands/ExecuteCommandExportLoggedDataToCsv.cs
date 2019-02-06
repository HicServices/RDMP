// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using CatalogueManager.ItemActivation;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExportLoggedDataToCsv : ExecuteCommandViewLoggedData
    {
        public ExecuteCommandExportLoggedDataToCsv(IActivateItems activator, LogViewerFilter filter) : base(activator, LoggingTables.None, filter)
        {
        }

        public override string GetCommandName()
        {
            return "Export to CSV";
        }

        public override void Execute()
        {
            DataTable dt = new DataTable();

            var db = SelectOne(_loggingServers);
            var server = db.Discover(DataAccessContext.DataLoad).Server;

            if (db != null)
            {
                using (var con = server.GetConnection())
                {
                    con.Open();

                    string sql = String.Format(@"SELECT * FROM (
SELECT [dataLoadRunID]
	  ,eventType
      ,[description]
      ,[source]
      ,[time]
      ,[ID]
  FROM {0}
  {2}
UNION
SELECT [dataLoadRunID]
	  ,'OnError'
      ,[description]
      ,[source]
      ,[time]
      ,[ID]
  FROM {1}
  {2}
 ) as x
order by time ASC", LoggingTables.ProgressLog, LoggingTables.FatalError, _filter.GetWhereSql(LoggingTables.ProgressLog));

                    DbCommand cmd = server.GetCommand(sql, con);
                    DbDataAdapter da = server.GetDataAdapter(cmd);
                    da.Fill(dt);
                    StringBuilder sb = new StringBuilder(); 
                    IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                        Select(column => column.ColumnName);
                    sb.AppendLine(string.Join(",", columnNames));

                    foreach (DataRow row in dt.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field =>
                            string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        sb.AppendLine(string.Join(",", fields));
                    }

                    var outputfile = Path.GetTempFileName() + ".csv";

                    File.WriteAllText(outputfile, sb.ToString());
                    UsefulStuff.GetInstance().ShowFileInWindowsExplorer(new FileInfo(outputfile));
                }
            }
        }
    }
}