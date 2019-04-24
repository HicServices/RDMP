// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CatalogueManager.CommandExecution.AtomicCommands;
using Rdmp.Core.Logging;

namespace CatalogueManager.LogViewer.Tabs
{
    /// <summary>
    /// Records each separate execution of a given Task for example one Data Extraction for Project 2301 (of datasets Biochemistry, Prescribing and Haematology) including when it started 
    /// and ended and who ran the extraction.
    /// </summary>
    public class LoggingRunsTabUI : LoggingTabUI
    {
        protected override IEnumerable<ExecuteCommandViewLoggedData> GetCommands(int rowIdnex)
        {

            var taskId = (int)dataGridView1.Rows[rowIdnex].Cells["ID"].Value;
            yield return new ExecuteCommandViewLoggedData(Activator, LoggingTables.ProgressLog, new LogViewerFilter { Run = taskId });
            yield return new ExecuteCommandViewLoggedData(Activator, LoggingTables.FatalError, new LogViewerFilter { Run = taskId });
            yield return new ExecuteCommandViewLoggedData(Activator, LoggingTables.TableLoadRun, new LogViewerFilter { Run = taskId });

            yield return new ExecuteCommandExportLoggedDataToCsv(Activator, new LogViewerFilter { Run = taskId });
        }

        protected override LoggingTables GetTableEnum()
        {
            return LoggingTables.DataLoadRun;
        }
    }
}
