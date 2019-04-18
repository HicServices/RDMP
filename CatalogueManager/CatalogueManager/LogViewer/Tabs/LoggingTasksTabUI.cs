// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using HIC.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// High level categories of activities (like folders) e.g. Data Extraction, Loading Biochemistry etc.
    /// </summary>
    public class LoggingTasksTabUI : LoggingTabUI
    {
        protected override IEnumerable<ExecuteCommandViewLoggedData> GetCommands(int rowIndex)
        {
            var taskId = (int)dataGridView1.Rows[rowIndex].Cells["ID"].Value;
            yield return new ExecuteCommandViewLoggedData(Activator, LoggingTables.DataLoadRun, new LogViewerFilter { Task = taskId });
        }

        protected override LoggingTables GetTableEnum()
        {
            return LoggingTables.DataLoadTask;
        }
    }
}
