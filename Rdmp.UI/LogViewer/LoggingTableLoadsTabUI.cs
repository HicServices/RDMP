// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Logging;
using Rdmp.UI.CommandExecution.AtomicCommands;

namespace Rdmp.UI.LogViewer
{
    /// <summary>
    /// During a run all destinations for data will appear in this control, for data loading this includes STAGING load and LIVE load (with 1 record per table being loaded).  It includes 
    /// counts for the number of INSERTS / UPDATES and DELETES carried out.  In the case of Data Extraction the record will refer to a 'table' as the .csv file given to the researcher
    /// (unless you are extracting into a researchers database of course).
    /// </summary>
    public class LoggingTableLoadsTabUI : LoggingTabUI
    {
        protected override IEnumerable<ExecuteCommandViewLoggedData> GetCommands(int rowIdnex)
        {
            var tableId = (int)dataGridView1.Rows[rowIdnex].Cells["ID"].Value;
            yield return new ExecuteCommandViewLoggedData(Activator, LoggingTables.DataSource, new LogViewerFilter { Table = tableId });
        }

        protected override LoggingTables GetTableEnum()
        {
            return LoggingTables.TableLoadRun;
        }
    }
}
